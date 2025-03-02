using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
class FSPEntrance
{
    private static string FSPDoorOpened_ModSaveFlag = "OpenedFSPDoorFromOutside";

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        if (__instance.name != "AG_S2") {
            return;
        }

        if (APSaveManager.CurrentAPSaveData == null) {
            return;
        }
        APSaveManager.CurrentAPSaveData.otherPersistentModFlags.TryGetValue(FSPDoorOpened_ModSaveFlag, out var fspDoorOpened);
        if (fspDoorOpened) {
            Log.Info($"FSPEntrance::GameLevel_Awake() doing nothing because the FSP door has already been opened from the outside.");
            return;
        }

        Log.Info($"The FSP door has not yet been opened from the outside." +
            " Disabling the FSP exit trigger so the player can't leave the normal way," +
            " even if they jump over the Ruyi conversation trigger.");
        GameObject.Find("AG_S2/Room/Connections/議會出口 FSM Object").SetActive(false);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        if (
            __instance.name == "[State] Disabled" &&
            __instance.transform.parent.parent.parent.name == "SimpleCutSceneFSM_還沒摸古樹打電話提醒（初次拿 回家笛 一面）"
        ) {
            if (APSaveManager.CurrentAPSaveData == null) {
                return;
            }
            APSaveManager.CurrentAPSaveData.otherPersistentModFlags.TryGetValue(FSPDoorOpened_ModSaveFlag, out var fspDoorOpened);
            if (fspDoorOpened) {
                Log.Info($"FSPEntrance::GeneralState_OnStateEnter() doing nothing because the FSP door has already been opened from the outside.");
                return;
            }

            Log.Info($"FSPEntrance::GeneralState_OnStateEnter setting up Ruyi conversation FSM so he'll tell the player about the 'jammed door' if they try to use it.");
            var triggerState = __instance.transform.parent.Find("[State] WaitForTrigger");

            // disable the automatic transition from "waiting for Yi to trigger the Ruyi chat" to "actually Ruyi has nothing to say"
            triggerState.transform.Find("[Action] Disable Checked").gameObject.SetActive(false);
            // Then we can safely enter the WaitForTrigger state.
            // For some reason OnStateEnter() doesn't work here, we need ForceEnterState()
            AccessTools.Method(typeof(GeneralState), "ForceEnterState", []).Invoke(triggerState?.GetComponent<GeneralState>(), []);
        }
    }

    // This flag ensures that when we trigger the Ruyi call, it'll go to the short one-panel reminder instead of the longer back-and-forth conversation
    public static void NewGameFlagEdits() {
        var anomalousFSPNodeRuyiCall_heardLongVersionFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["63130d58-d394-4d7e-a930-164c55792f62_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool"];
        anomalousFSPNodeRuyiCall_heardLongVersionFlag.CurrentValue = true;
    }

    // Now we need to change the text of that one panel
    [HarmonyPrefix, HarmonyPatch(typeof(LocalizationManager), "GetTranslation")]
    static bool LocalizationManager_GetTranslation(string Term, ref string __result, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters = true) {
        //Log.Info($"LocalizationManager_GetTranslation: {Term}");
        if (Term == "AG_S2/M45_AG_S2_提醒古樹初次暴走_Chat06") {
            Log.Info($"Editing Ruyi dialogue to explain the jammed FSP door.");
            __result = "Apologies, my lord. In this randomizer the Pavilion door is jammed, and can only be opened from the outside. You will have to find another path to Central Hall before you can open it.";
            return false;
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SceneConnectionPoint), "TriggerChangeScene")]
    static void SceneConnectionPoint_TriggerChangeScene(SceneConnectionPoint __instance) {
        if (__instance.transform.parent.parent.parent.name == "General FSM Object_ZDoor_YiBase") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S1/Room/Prefab/回家的大門/General FSM Object_ZDoor_YiBase/FSM Animator/LogicRoot/Connection_Prefab") {
                Log.Info($"Entered FSP from outside. The FSP door will no longer be considered stuck.");
                APSaveManager.CurrentAPSaveData!.otherPersistentModFlags[FSPDoorOpened_ModSaveFlag] = true;
                APSaveManager.ScheduleWriteToCurrentSaveFile();
            }
        }
    }
}
