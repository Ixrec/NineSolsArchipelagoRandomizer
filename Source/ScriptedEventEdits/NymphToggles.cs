using HarmonyLib;
using RCGFSM.Items;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

// Some cutscenes will turn your Nymph on and off for story reasons, but in randomizer we don't want that.

[HarmonyPatch]
internal class NymphToggles {
    // If this is the part where Lady E gives you back your nymph, but we still
    // don't have the AP nymph item, then don't let Yi have it yet.
    private static List<string> postEtherealNymphReturns = [
        "A7_S6_Memory_Butterfly_CutScene_GameLevel/A7_S6_Cutscene FSM/--[States]/FSM/[State] PlayCutScene/[Action] EnableButterfly", // the actual PlayerAbilityData
        "A7_S6_Memory_Butterfly_CutScene_GameLevel/A7_S6_Cutscene FSM/--[States]/FSM/[State] PlayCutScene/[Action] 取回玄蝶###5", // 裝備_玄蝶
        "A7_S6_Memory_Butterfly_CutScene_GameLevel/A7_S6_Cutscene FSM/--[States]/FSM/[State] PlayCutScene/[Action] 取回玄蝶###6", // 狀態欄_玄蝶
        "A7_S1/Room/Prefab/A7_S1_三階段FSM/--[States]/FSM/[State] Phase4_腦室_蝴蝶死後/[Action] EnableButterfly", // the actual PlayerAbilityData, again
    ];

    /*
     * The true ending GD evacuation cutscene is controlled by GameLevel/Room/Prefab/SpaceShipYard 階段 FSM Object/--[States]/FSM/
     *  [State] Phase0(初始) - the normal state the hangar is in for most of the game
     *  [State] Phase1(解開真結局) - the cutscene will trigger
     *  [State] Phase2 - the hangar doors are open and the ship is gone, because the apemen have evacuated
     */
    private static string trueEndingNymphRemoval = "GameLevel/Room/Prefab/SpaceShipYard 階段 FSM Object/FSM Animator/View/Phase 1/SimpleCutSceneFSM/--[States]/FSM/[State] PlayCutSceneEnd/[Action] DisableButterfly";

    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        if (__instance.name == "[Action] EnableButterfly" || __instance.name == "[Action] 取回玄蝶") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (postEtherealNymphReturns.Contains(goPath)) {
                if (ItemApplications.ApInventory.GetValueOrDefault(Item.MysticNymphScoutMode, 0) == 0) {
                    Log.Info($"NymphToggles::PickItemAction_OnStateEnterImplement preventing a post-Ethereal cutscene action from giving Yi a nymph because you don't have the AP nymph item yet");
                    return false;
                }
            }
        }

        if (__instance.name == "[Action] DisableButterfly") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (trueEndingNymphRemoval == goPath) {
                if (ItemApplications.ApInventory.GetValueOrDefault(Item.MysticNymphScoutMode, 0) == 1) {
                    Log.Info($"NymphToggles::PickItemAction_OnStateEnterImplement preventing a true ending cutscene action from taking Yi's nymph away");
                    return false;
                }
            }
        }

        return true;
    }
}
