using HarmonyLib;
using NineSolsAPI;
using UnityEngine;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

[HarmonyPatch]
class NewKunlunControlHubEntrance {
    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        if (__instance.name != "AG_S1") {
            return;
        }

        Log.Info($"Disabling a GO that would auto-set the PonR flag if we left it active (which probably shouldn't exist at all)");
        GameObject.Find("AG_S1/Room/Prefab/Phase相關切換Gameplay----------------/General FSM Object_On And Off Switch 最終階段切換_古樹/--[States]/FSM/[State] On/[Action]  OnOffFlag = true").SetActive(false);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        if (__instance.name == "[State] Off") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S1/Room/Prefab/Phase相關切換Gameplay----------------/General FSM Object_On And Off Switch 最終階段切換_古樹/--[States]/FSM/[State] Off") {
                Log.Info($"NewKunlunControlHubEntrance::GeneralState_OnStateEnter forcing NKCH entrance into its interactable state");
                var onState = __instance.transform.parent.Find("[State] On");
                onState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AbstractInteraction), "InteractEnter")]
    static bool AbstractInteraction_InteractEnter(AbstractInteraction __instance) {
        if (__instance.transform.parent?.parent?.parent?.parent?.parent?.name != "General FSM Object_ZDoor_STHubTeleportarium Variant (1)")
            return true;

        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        if (goPath == "AG_S1/Room/Prefab/Phase相關切換Gameplay----------------/General FSM Object_On And Off Switch 最終階段切換_古樹/FSM Animator/LogicRoot/[On]Node/Phase2_遊戲最終階段 /General FSM Object_ZDoor_STHubTeleportarium Variant (1)/FSM Animator/LogicRoot/CanInteract/Interactable_Interact/Interact Interaction") {
            var sealCount = ItemApplications.GetSolSealsCount();

            Log.Info($"AbstractInteraction_InteractEnter pressed E on the Central Hall -> New Kunlun Control Hub zbridge prompt with {sealCount} sol seals");

            long sealsToUnlock = 8;
            if (ConnectionAndPopups.SlotData != null && ConnectionAndPopups.SlotData.ContainsKey("seals_for_eigong")) {
                sealsToUnlock = (long)ConnectionAndPopups.SlotData["seals_for_eigong"];
            }

            if (sealCount >= sealsToUnlock) {
                Log.Info($"AbstractInteraction_InteractEnter letting the player enter; {sealCount} >= {sealsToUnlock}. " +
                    $"Also triggering a Point of no Return backup save, so they'll be able to keep playing after Eigong.");
                SaveManager.Instance.AutoSave(SaveManager.SaveSceneScheme.BackUpNoReturnPoint);

                if (APRandomizer.Instance.ForceTrueEigongSetting.Value) {
                    ToastManager.Toast($"<color=orange>Setting the true ending flag</color> because the 'Force True Eigong' setting is enabled.");
                    var trueEndingFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["e78958a13315eb9418325caf25da9d4dScriptableDataBool"];
                    trueEndingFlag.CurrentValue = true;
                }
                return true;
            }

            ToastManager.Toast($"You need {sealsToUnlock} Sol Seals to unlock New Kunlun Control Hub and the final Eigong fight.");
            ToastManager.Toast($"Currently, you only have {sealCount} Sol Seals.");
            return false;
        }
        return true;
    }
}
