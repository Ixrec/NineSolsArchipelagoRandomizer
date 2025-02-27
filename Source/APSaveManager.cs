using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        // don't edit any empty "New Game" slots
        if (!__instance.SaveExist)
            return;

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

            var baseSaveExists = await checkSaveTask;
            if (!baseSaveExists) {
                // don't edit any empty "New Game" slots
            } else if (apSaveFileExists) {
                Log.Info($"{saveSlotVanillaFolderName} has AP save data");

                var apSaveData = JsonConvert.DeserializeObject<APRandomizerSaveData>(File.ReadAllText(saveSlotAPModFilePath));
                // TODO: validate items and locations?
                apSaveSlots[i] = apSaveData;
            } else {
                Log.Info($"{saveSlotVanillaFolderName} is a vanilla save");
                // This is a vanilla save, so we want to stop the user from trying to load it.

                // First, make the button non-interactive.
                var buttonGO = saveSlotButtonsGO.transform
                    .GetChild(i * 2) // the *2 is due to "Padding" GOs
                    .GetChild(0); // 0 is the big button for the save itself; 2 is the corresponding [Delete] button
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
            }
        }

        Log.Info($"Finished loading Archipelago save data");
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

    // since "async bool" isn't a thing, we need two patches to properly
    // insert our UI and networking code before the vanilla code
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "CreateOrLoadSaveSlotAndPlay")]
    public static bool StartMenuLogic_CreateOrLoadSaveSlotAndPlay_AllowOrSkipVanillaImpl(StartMenuLogic __instance, int slotIndex, bool SaveExists, bool LoadFromBackup = false, bool memoryChallengeMode = false) {
        if (memoryChallengeMode)
            return true;

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

        // Since Unity IMGUI popups can't be modal over RCG UI, we need to manually disable and re-enable the UI behind the popup
        var saveSlotMenuGO = GameObject.Find("MenuLogic/MainMenuLogic/Providers/StartGame SaveSlotPanel");
        // It's slightly visually nicer if we avoid disabling the "BG" part of the menu by targeting only these child GOs
        saveSlotMenuGO.transform.GetChild(1).gameObject.SetActive(false); // Title Text
        saveSlotMenuGO.transform.GetChild(3).gameObject.SetActive(false); // SlotGroup (contains the 4 big buttons)
        saveSlotMenuGO.transform.GetChild(4).gameObject.SetActive(false); // SavePanel_BackButton

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
            saveSlotMenuGO.transform.GetChild(1).gameObject.SetActive(true);
            saveSlotMenuGO.transform.GetChild(3).gameObject.SetActive(true);
            saveSlotMenuGO.transform.GetChild(4).gameObject.SetActive(true);
        }
    }

    public static void WriteCurrentSaveFile() {
        var saveSlotAPModFilePath = APSaveDataPathForSlot(selectedSlotIndex);
        File.WriteAllText(saveSlotAPModFilePath, JsonConvert.SerializeObject(CurrentAPSaveData));
        Log.Info($"WriteCurrentSaveFile() wrote AP save file at {saveSlotAPModFilePath}");
    }
}
