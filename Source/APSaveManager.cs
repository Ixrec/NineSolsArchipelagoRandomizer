using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public uint port;
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

    public static APRandomizerSaveData? currentApSaveSlot => apSaveSlots[currentSlotIndex];

    public static bool apSavesLoaded = false;

    public static int currentSlotIndex =>
        AccessTools.FieldRefAccess<SaveManager, int>("currentSlotIndex")
            .Invoke(SingletonBehaviour<SaveManager>.Instance);

    // we don't do any real menu edits around Start() because it turned out the base game
    // hasn't even properly loaded saveslot3 (and only 3???) by the time this is called
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "Start")]
    public static void StartMenuLogic_Start_Prefix(StartMenuLogic __instance) {
        apSavesLoaded = false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(SaveSlotUIButton), "UpdateUI")]
    public static void SaveSlotUIButton_UpdateUI_Postfix(SaveSlotUIButton __instance) {
        if (!apSavesLoaded) {
            LoadAPSaves();
            apSavesLoaded = true;
        }

        var apData = apSaveSlots[__instance.index];
        if (apData != null) {
            __instance.enabled = true;
            var cd = apData.apConnectionData;
            __instance.lastSceneText.text += "\n" + cd.hostname + " - " + cd.slotName;
        } else {
            __instance.enabled = false;
            __instance.lastSceneText.text += "\n[Vanilla Save]";
        }
    }

    private static void LoadAPSaves() {
        Log.Info($"Loading Archipelago save data");

        var saveSlotsPath = APRandomizer.SaveSlotsPath;
        var saveSlotButtonsGO = GameObject.Find("MenuLogic/MainMenuLogic/Providers/StartGame SaveSlotPanel/SlotGroup/SlotGroup H");

        for (var i = 0; i <= 3; i++) {
            var saveSlotVanillaFolderName = SaveManager.GetSlotDirPath(i);
            var saveSlotAPModFileName = saveSlotVanillaFolderName + "_Ixrec_ArchipelagoRandomizer.json";
            var saveSlotAPModFilePath = saveSlotsPath + "/" + saveSlotAPModFileName;
            var apSaveFileExists = File.Exists(saveSlotAPModFilePath);

            if (!apSaveFileExists) {
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
            } else {
                Log.Info($"{saveSlotVanillaFolderName} has AP save data");

                var apSaveData = JsonConvert.DeserializeObject<APRandomizerSaveData>(File.ReadAllText(saveSlotAPModFilePath));
                // TODO: validate items and locations?
                apSaveSlots[i] = apSaveData;
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

        // TODO: if we have connected to the AP slot for this save file already...
        bool isAlreadyConnected = false;
        return isAlreadyConnected;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "CreateOrLoadSaveSlotAndPlay")]
    public static async void StartMenuLogic_CreateOrLoadSaveSlotAndPlay_EnsureAPConnection(StartMenuLogic __instance, int slotIndex, bool SaveExists, bool LoadFromBackup = false, bool memoryChallengeMode = false) {
        if (memoryChallengeMode)
            return;

        // TODO: if we have connected to the AP slot for this save file already...
        bool isAlreadyConnected = false;
        if (isAlreadyConnected)
            return;

        if (!SaveExists) {
            // show connection info popup
            /*
             * var tcs = new TaskCompletionSource<T>();
             * tcs.SetResult(result);
             * tcs.SetException(exc);
             */
        }

        // TODO: attempt connection
        // TODO: display error/retry popup
    }

}
