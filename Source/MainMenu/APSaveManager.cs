using HarmonyLib;
using Newtonsoft.Json;
using RCGMaker.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArchipelagoRandomizer;

/*
 * The flow of base game methods is:
 * - GO "MenuLogic/MainMenuLogic" in scene "TitleScreenMenu" has the StartMenuLogic component
 * - StartMenuLogic has or references most of what we care about here
 * - StartMenuLogic::Start() naturally does the initial setup, including UpdateSaveSlots()
 * - Most CRUD ops involving save files also call UpdateSaveSlots() -> loop over SaveSlotUIButton::UpdateUI()
 * - Simply clicking on a save calls StartMenuLogic::CreateOrLoadSaveSlotAndPlay()
 * - When you click Standard or Story Mode, that calls CreateNewGame() -> NewGame() for sounds and animations,
 *   - then NewGameChangeScene(slotIndex) to actually start a new game
 *   - then ChangeSceneClean("A0_S6_Intro_Video", ...)
 */

public class APConnectionData {
    public string hostname;
    public int port;
    public string slotName;
    public string password;
    public string? roomId;
}
public class APRandomizerSaveData {
    public APConnectionData apConnectionData;
    public Dictionary<string, bool> locationsChecked;
    public Dictionary<string, int> itemsAcquired;
    public Dictionary<string, bool> otherPersistentModFlags;
    // TODO: scouts and hints
}

[HarmonyPatch]
internal class APSaveManager {
    // slot 4 is Battle Memories, so we're only interested in slots 0-3
    public static APRandomizerSaveData?[] apSaveSlots = [null, null, null, null];

    // we can't use SaveManager.currentSlotIndex because that only gets set when the base game loads,
    // while we need to know our own slot as soon as the user selects one and we connect to AP
    public static int selectedSlotIndex = -1;

    public static APRandomizerSaveData? CurrentAPSaveData =>
        (selectedSlotIndex >= 0) ? apSaveSlots[selectedSlotIndex] : null;

    public static bool apSavesLoaded = false;

    // we don't do any real menu edits around Start() because it turned out the base game
    // hasn't even properly loaded saveslot3 (and only 3???) by the time this is called
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "Start")]
    public static void StartMenuLogic_Start_Prefix(StartMenuLogic __instance) {
        apSavesLoaded = false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(SaveSlotUIButton), "UpdateUI")]
    public static async void SaveSlotUIButton_UpdateUI_Postfix(SaveSlotUIButton __instance) {
        if (!apSavesLoaded) {
            await LoadAPSaves();
            apSavesLoaded = true;
        }

        if (!__instance.SaveExist) {
            __instance.enabled = true; // If this empty/"New Game" slot was a vanilla save that just got deleted, be sure to un-disable it
            return; // but otherwise don't edit any empty/"New Game" slots.
        }

        var apData = apSaveSlots[__instance.index];
        if (apData != null) {
            __instance.enabled = true;
            var cd = apData.apConnectionData;
            __instance.lastSceneText.text += "\n" + cd.hostname + ":" + cd.port + " - " + cd.slotName;
        } else {
            __instance.enabled = false;
            __instance.lastSceneText.text += "\n[Vanilla Save]";
        }
    }

    private static string APSaveDataPathForSlot(int i) {
        var saveSlotsPath = APRandomizer.SaveSlotsPath;
        var saveSlotVanillaFolderName = SaveManager.GetSlotDirPath(i);
        var saveSlotAPModFileName = saveSlotVanillaFolderName + "_Ixrec_ArchipelagoRandomizer.json";
        return saveSlotsPath + "/" + saveSlotAPModFileName;
    }

    private static async Task LoadAPSaves() {
        Log.Info($"Loading Archipelago save data");

        var saveSlotButtonsGO = GameObject.Find("MenuLogic/MainMenuLogic/Providers/StartGame SaveSlotPanel/SlotGroup/SlotGroup H");

        for (var i = 0; i <= 3; i++) {
            var checkSaveTask = SingletonBehaviour<SaveManager>.Instance.CheckSaveExist(i);

            var saveSlotVanillaFolderName = SaveManager.GetSlotDirPath(i);
            var saveSlotAPModFilePath = APSaveDataPathForSlot(i);
            var apSaveFileExists = File.Exists(saveSlotAPModFilePath);

            var buttonsForThisSlotT = saveSlotButtonsGO.transform.GetChild(i * 2); // the *2 is due to "Padding" GOs

            var baseSaveExists = await checkSaveTask;
            if (!baseSaveExists) {
                // don't edit any empty "New Game" slots; just make sure we aren't holding onto any stale data/UI for them
                apSaveSlots[i] = null;
                SetChangeButtonVisible(false, buttonsForThisSlotT, i);
            } else if (apSaveFileExists) {
                Log.Info($"{saveSlotVanillaFolderName} has AP save data");

                try {
                    var apSaveData = JsonConvert.DeserializeObject<APRandomizerSaveData>(File.ReadAllText(saveSlotAPModFilePath));
                    // TODO: validate items and locations?
                    apSaveSlots[i] = apSaveData;
                } catch (Exception ex) {
                    Log.Error($"failed to deserialize randomizer save data in {saveSlotAPModFilePath}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }

                SetChangeButtonVisible(true, buttonsForThisSlotT, i);
            } else {
                Log.Info($"{saveSlotVanillaFolderName} is a vanilla save");
                // This is a vanilla save, so we want to stop the user from trying to load it.

                // First, make the button non-interactive.
                var buttonGO = buttonsForThisSlotT.GetChild(0); // 0 is the big button for the save itself; 2 is the corresponding [Delete] button
                buttonGO.GetComponent<UnityEngine.UI.Button>().enabled = false;
                buttonGO.GetComponent<SelectableNavigationRemapping>().enabled = false;

                // Second, edit the colors of the text components so it "looks disabled" to the user.
                var textGridGO = buttonGO.transform.Find("AnimationRoot/Grid");
                foreach (var t in textGridGO.GetComponentsInChildrenOfDepthOne<TMPro.TextMeshProUGUI>()) {
                    // I don't know why .RGBMultiplied() fails/throws here, so we do it the marginally harder way
                    var color = t.color;
                    color.r = color.r / 2;
                    color.g = color.g / 2;
                    color.b = color.b / 2;
                    t.color = color;
                }

                SetChangeButtonVisible(false, buttonsForThisSlotT, i);
            }
        }

        Log.Info($"Finished loading Archipelago save data");
    }

    private static void SetChangeButtonVisible(bool visible, Transform saveSlotButtonsT, int slotIndex) {
        // child 0 is the big button for the save itself, 1 is the padding object, and 2 is the corresponding |Delete| button

        var deletePaddingT = saveSlotButtonsT.GetChild(1);
        deletePaddingT.GetComponent<LayoutElement>().preferredWidth = (visible ? 70 : 105);

        var deleteButtonT = saveSlotButtonsT.GetChild(2);
        deleteButtonT.GetComponent<LayoutElement>().preferredWidth = (visible ? 80 : 100);

        // Create the change button (and padding) if we haven't already for this slot
        if (visible && saveSlotButtonsT.childCount == 3) {
            var changePaddingT = UnityEngine.Object.Instantiate(deletePaddingT);
            changePaddingT.name = $"APRandomizer_Padding_Slot{slotIndex}";
            changePaddingT.transform.SetParent(saveSlotButtonsT, false);
            changePaddingT.GetComponent<LayoutElement>().preferredWidth = 50;

            var changeButtonT = UnityEngine.Object.Instantiate(deleteButtonT);
            changeButtonT.name = $"APRandomizer_ChangeConnectionInfo_Slot{slotIndex}";
            changeButtonT.transform.SetParent(saveSlotButtonsT, false);
            changeButtonT.GetComponent<LayoutElement>().preferredWidth = 80;

            var changeTextGO = changeButtonT.Find("Text (TMP)");
            changeTextGO.GetComponent<TMPro.TextMeshProUGUI>().text = "Change\nConnection\nInformation";

            // This removes the "Delete? [Confirm] [Cancel]" popup that the vanilla Delete button comes with
            UnityEngine.Object.Destroy(changeButtonT.GetComponent<UISubmitConfirmAddOn>());

            var changeUICB = changeButtonT.GetComponent<UIControlButton>();

            // This onSubmit event has a handler calling SaveSlotUIButton.DeleteSave(), which is how the Delete button actually deletes.
            // Removing an event handler you don't own is very hard, so it's easier to just set this to null and do our own
            // submit handler by patching UIControlButton::SubmitImplementation() separately.
            changeUICB.onSubmit = null;

            // unbreak some button implementation details that Object.Instantiate() couldn't magically handle for us
            AccessTools.FieldRefAccess<UIControlButton, UIControlGroup>("belongGroup").Invoke(changeUICB) =
                changeButtonT.GetComponentInParent<UIControlGroup>(includeInactive: true); // the GO we want is inactive when quitting to menu
            AccessTools.FieldRefAccess<UIControlButton, AbstractConditionComp[]>("_activateConditions").Invoke(changeUICB) = []; // this just needs to be non-null
            AccessTools.FieldRefAccess<UIControlButton, AutoDisableAnimator>("_autoDisableAnimator").Invoke(changeUICB) = changeButtonT.GetComponent<AutoDisableAnimator>();
            AccessTools.FieldRefAccess<UIControlButton, Selectable>("_button").Invoke(changeUICB) = changeButtonT.GetComponent<Button>();
            // The last things this button needs to work can't be done here; see the patches below
        } else if (saveSlotButtonsT.childCount == 5) {
            var changePaddingT = saveSlotButtonsT.GetChild(3);
            changePaddingT.gameObject.SetActive(visible);

            var changeButtonT = saveSlotButtonsT.GetChild(4);
            changeButtonT.gameObject.SetActive(visible);
        }
    }

    // UIControlGroup is supposed to iterate over its _uiInteractables to tell them all when to
    // start and stop responding to user input. Unfortunately, _uiInteractables is an array that
    // we can't edit at runtime from a mod, so our Change buttons can't go in there.
    // Instead we listen for when the corresponding Delete buttons are changed.
    [HarmonyPostfix, HarmonyPatch(typeof(UIControlButton), "OnBecomeInteractable")]
    public static void UIControlButton_OnBecomeInteractable(UIControlButton __instance) {
        if (__instance.name == "Delete Button" && __instance.transform.parent?.name.StartsWith("SaveSlotContainer_Slot") == true) {
            if (__instance.transform.parent.childCount == 5) {
                var changeButtonT = __instance.transform.parent.GetChild(4);
                //Log.Info($"UIControlButton_OnBecomeInteractable {__instance.transform.parent?.name}/{__instance.name} calling corresponding change button's OnBecomeInteractable()");
                changeButtonT.GetComponent<UIControlButton>().OnBecomeInteractable();
            }
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(UIControlButton), "OnBecomeNotInteractable")]
    public static void UIControlButton_OnBecomeNotInteractable(UIControlButton __instance) {
        if (__instance.name == "Delete Button" && __instance.transform.parent?.name.StartsWith("SaveSlotContainer_Slot") == true) {
            if (__instance.transform.parent.childCount == 5) {
                var changeButtonT = __instance.transform.parent.GetChild(4);
                //Log.Info($"UIControlButton_OnBecomeNotInteractable {__instance.transform.parent?.name}/{__instance.name} calling corresponding change button's OnBecomeNotInteractable()");
                changeButtonT.GetComponent<UIControlButton>().OnBecomeNotInteractable();
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UIControlButton), "SubmitImplementation")]
    public static async void UIControlButton_SubmitImplementation(UIControlButton __instance) {
        if (__instance.name.StartsWith("APRandomizer_ChangeConnectionInfo_Slot")) {
            int slotIndex = int.Parse(__instance.name.Substring("APRandomizer_ChangeConnectionInfo_Slot".Length));
            Log.Info($"UIControlButton_SubmitImplementation for {__instance.name} parsed slotIndex={slotIndex} showing change info popup");
            var oldConnData = apSaveSlots[slotIndex].apConnectionData;

            HideSaveMenu();
            var newConnData = await ConnectionAndPopups.ChangeConnectionInfo(oldConnData);
            UnHideSaveMenu();

            apSaveSlots[slotIndex].apConnectionData = newConnData;
            WriteSaveFileForSlot(slotIndex);

            // calls SaveSlotUIButton::UpdateUI() for every slot, including the one we just changed
            GameObject.Find("MenuLogic/MainMenuLogic").GetComponent<StartMenuLogic>().UpdateSaveSlots();
        }
    }

    /*
     * In the end I decided not to use SaveManager._fileSystem directly, but
     * since I did figure out how to "break in" and use it, here's how:
     * 
object fileSystem = typeof(SaveManager)
    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
    .Where(f => f.Name == "_fileSystem").First()
    .GetValue(SingletonBehaviour<SaveManager>.Instance);
var interfaceMethods = fileSystem.GetType().GetInterface("IFileSystem").GetMethods();

var fe = interfaceMethods.Where(m => m.Name == "FileExists").First();
var rab = interfaceMethods.Where(m => m.Name == "ReadAllBytes").First();
var df = interfaceMethods.Where(m => m.Name == "DeleteFile").First();
var dd = interfaceMethods.Where(m => m.Name == "DeleteDirectory").First();
var cd = interfaceMethods.Where(m => m.Name == "CreateDirectory").First();
var wab = interfaceMethods.Where(m => m.Name == "WriteAllBytes").First();

var de = interfaceMethods.Where(m => m.Name == "DirectoryExists").First();
de.Invoke(fileSystem, new object[] { "saveslot0" });

byte[] bytes = [];
wab.Invoke(fileSystem, new object[] { "saveslot0", bytes });
     */

    private static void HideSaveMenu() {
        // Since Unity IMGUI popups can't be modal over RCG UI, we need to manually disable and re-enable the UI behind the popup
        var saveSlotMenuGO = GameObject.Find("MenuLogic/MainMenuLogic/Providers/StartGame SaveSlotPanel");
        // It's slightly visually nicer if we avoid disabling the "BG" part of the menu by targeting only these child GOs
        saveSlotMenuGO.transform.GetChild(1).gameObject.SetActive(false); // Title Text
        saveSlotMenuGO.transform.GetChild(3).gameObject.SetActive(false); // SlotGroup (contains the 4 big buttons)
        saveSlotMenuGO.transform.GetChild(4).gameObject.SetActive(false); // SavePanel_BackButton

    }
    private static void UnHideSaveMenu() {
        var saveSlotMenuGO = GameObject.Find("MenuLogic/MainMenuLogic/Providers/StartGame SaveSlotPanel");
        saveSlotMenuGO.transform.GetChild(1).gameObject.SetActive(true);
        saveSlotMenuGO.transform.GetChild(3).gameObject.SetActive(true);
        saveSlotMenuGO.transform.GetChild(4).gameObject.SetActive(true);
    }

    // since "async bool" isn't a thing, we need two patches to properly
    // insert our UI and networking code before the vanilla code
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "CreateOrLoadSaveSlotAndPlay")]
    public static bool StartMenuLogic_CreateOrLoadSaveSlotAndPlay_AllowOrSkipVanillaImpl(StartMenuLogic __instance, int slotIndex, bool SaveExists, bool LoadFromBackup = false, bool memoryChallengeMode = false) {
        if (memoryChallengeMode) {
            if (ConnectionAndPopups.APSession != null) {
                Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay_AllowOrSkipVanillaImpl setting APSession to null since the player has switched to Battle Memories");
                ConnectionAndPopups.APSession = null;
            }
            return true;
        }

        bool isAlreadyConnected = (ConnectionAndPopups.APSession != null) && (slotIndex == selectedSlotIndex);
        Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay_AllowOrSkipVanillaImpl returning {isAlreadyConnected}");
        return isAlreadyConnected;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "CreateOrLoadSaveSlotAndPlay")]
    public static async void StartMenuLogic_CreateOrLoadSaveSlotAndPlay_EnsureAPConnection(StartMenuLogic __instance, int slotIndex, bool SaveExists, bool LoadFromBackup = false, bool memoryChallengeMode = false) {
        if (memoryChallengeMode)
            return;

        bool isAlreadyConnected = (ConnectionAndPopups.APSession != null) && (slotIndex == selectedSlotIndex);
        if (isAlreadyConnected) {
            Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay_EnsureAPConnection returning early because we're already connected to slot {slotIndex}");
            return;
        }

        HideSaveMenu();

        try {
            selectedSlotIndex = slotIndex;
            var apSaveData = apSaveSlots[slotIndex];
            if (SaveExists) {
                Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay_EnsureAPConnection calling ResumePreviousConnection");
                await ConnectionAndPopups.ResumePreviousConnection(apSaveData!);
            } else {
                Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay_EnsureAPConnection calling GetConnectionInfoFromUser");
                apSaveData = await ConnectionAndPopups.GetConnectionInfoFromUser(null);
                apSaveSlots[slotIndex] = apSaveData;
            }

            WriteCurrentSaveFile();

            Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay_EnsureAPConnection re-calling CreateOrLoadSaveSlotAndPlay now that we're connected");
            await __instance.CreateOrLoadSaveSlotAndPlay(slotIndex, SaveExists, LoadFromBackup, memoryChallengeMode);
        } catch (Exception ex) {
            selectedSlotIndex = -1;
            Log.Warning($"GetConnectionInfoFromUser threw: {ex.Message} with stack:\n{ex.StackTrace}");
        } finally {
            UnHideSaveMenu();
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SaveManager), "DeleteSave")]
    public static async void SaveManager_DeleteSave(SaveManager __instance, int i) {
        var wasApSave = (apSaveSlots[i] != null);
        var saveSlotAPModFilePath = APSaveDataPathForSlot(i);

        Log.Info($"SaveManager_DeleteSave() deleting AP save file at {saveSlotAPModFilePath}");
        File.Delete(saveSlotAPModFilePath);

        apSaveSlots[i] = null;
        selectedSlotIndex = -1;
        ConnectionAndPopups.APSession = null;

        var buttonsForThisSlotT = GameObject.Find("MenuLogic/MainMenuLogic/Providers/StartGame SaveSlotPanel/SlotGroup/SlotGroup H")
            .transform.GetChild(i * 2); // the *2 is due to "Padding" GOs

        // Hide the Change Connection Information button if there was one, since the base game only knows to hide Delete
        if (buttonsForThisSlotT.childCount == 5) {
            buttonsForThisSlotT.GetChild(4).gameObject.SetActive(false);
        }

        // If this was a vanilla save, then after deletion the big button needs to be re-enabled
        if (!wasApSave) {
            var buttonGO = buttonsForThisSlotT.GetChild(0); // 0 is the big button for the save itself; 2 is the corresponding [Delete] button
            buttonGO.GetComponent<UnityEngine.UI.Button>().enabled = true;
            buttonGO.GetComponent<SelectableNavigationRemapping>().enabled = true;

            // Re-enabling the button includes un-darkening the text
            var textGridGO = buttonGO.transform.Find("AnimationRoot/Grid");
            foreach (var t in textGridGO.GetComponentsInChildrenOfDepthOne<TMPro.TextMeshProUGUI>()) {
                var color = t.color;
                color.r = color.r * 2;
                color.g = color.g * 2;
                color.b = color.b * 2;
                t.color = color;
            }
        }
    }

    public static void WriteSaveFileForSlot(int i) {
        var saveSlotAPModFilePath = APSaveDataPathForSlot(i);
        File.WriteAllText(saveSlotAPModFilePath, JsonConvert.SerializeObject(apSaveSlots[i]));
        Log.Info($"WriteCurrentSaveFile() wrote AP save file at {saveSlotAPModFilePath}");

        if (scheduledSaveFileWriteTCS != null)
            scheduledSaveFileWriteTCS = null;
    }

    public static void WriteCurrentSaveFile() {
        WriteSaveFileForSlot(selectedSlotIndex);
    }

    public static Task? scheduledSaveFileWriteTCS = null;

    public static void ScheduleWriteToCurrentSaveFile() {
        if (scheduledSaveFileWriteTCS != null)
            return;

        Log.Info($"ScheduleWriteToCurrentSaveFile() called with no pending write, so scheduling one");
        scheduledSaveFileWriteTCS = Task.Run(async () => {
            await Task.Delay(1000);
            Log.Info($"ScheduleWriteToCurrentSaveFile() task callback now actually callling WriteCurrentSaveFile()");
            WriteCurrentSaveFile();
        });
    }
}
