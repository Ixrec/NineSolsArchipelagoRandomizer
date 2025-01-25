using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class StateTransitions {
    // This is the best approach I've found for forcing certain FSM states to be chosen or skipped:
    // intercept the *transitions* into each undesirable state, and then do a manual transition

    // I don't know if this matters in practice for performance, but we use a single patch and
    // always check __instance.name first to reduce wasted computations of the full path
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {

        /*
         * Make Ji always appear at Daybreak Tower to give you the music sheet
         */

        // despite the "On" name, this state represents Ji *not* being at the Daybreak Tower
        if (__instance.name == "[State] On") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] On") {
                Log.Info($"GeneralState_OnStateEnter forcing Ji to exist at Daybreak Tower despite the base game hiding him");
                var jiDoesExistAtDaybreakTowerState = GameObject.Find("A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Off");
                jiDoesExistAtDaybreakTowerState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }

        /*
         * Make the FSP root node always appear in its default state, so it won't send you into any of
         * the usual Lear cutscenes (which are redundant because we start rando with full teleport).
         */

        // The 1st and 2nd states probably would never be hit anyway since we give ourselves horn + teleport
        // on new game creation in rando, but might as well play it safe.
        if (__instance.name == "[State] 第一次爆走") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 第一次爆走") {
                Log.Info($"GeneralState_OnStateEnter forcing FSP root node to its default state despite the base game wanting to do the 1st Lear talk");
                var fspRootNodeNormalState = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 普通存檔點");
                fspRootNodeNormalState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
        if (__instance.name == "[State] 第二次爆走") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 第二次爆走") {
                Log.Info($"GeneralState_OnStateEnter forcing FSP root node to its default state despite the base game wanting to do the 2nd Lear talk");
                var fspRootNodeNormalState = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 普通存檔點");
                fspRootNodeNormalState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
        if (__instance.name == "[State] 第三次爆走") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 第三次爆走") {
                Log.Info($"GeneralState_OnStateEnter forcing FSP root node to its default state despite the base game wanting to do the 3rd Lear talk");
                var fspRootNodeNormalState = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 普通存檔點");
                fspRootNodeNormalState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
    }
}
