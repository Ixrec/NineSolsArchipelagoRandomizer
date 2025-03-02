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

        /*
         * Move the Apemen into Galactic Dock if they haven't already moved, since the vanilla trigger will be very easy to bypass in rando
         */

        // In vanilla, the village is in its invaded state as soon as you finish the intro, but the trigger for the apemen to move into Galactic Dock
        // is not any of the Jiequan scenes; it's Chiyou rescuing Yi from Factory (Underground). Since this is very easy to bypass in rando, and looks
        // like a pretty serious bug if you fight Lieguan early, it's worth "fixing" even if there are no locations behind it atm.
        if (__instance.name == "[State] Phase 0") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "GameLevel/Room/Prefab/村民避難所_階段 FSM Object/--[States]/FSM/[State] Phase 0") {
                Log.Info($"GeneralState_OnStateEnter forcing the Apemen to evacuate into Galactic Dock even though the base game doesn't think they've done that yet");
                var initialEvacState = GameObject.Find("GameLevel/Room/Prefab/村民避難所_階段 FSM Object/--[States]/FSM/[State] Phase 1(被截權綁架後被截權綁架後)");
                initialEvacState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
        /*
         * In case we do make locations for Shennong's quest in the future, more notes:
         * Galactic Dock apemen status: "GameLevel/Room/Prefab/村民避難所_階段 FSM Object"
	        [State] Phase 0
	        [State] Phase 1(被截權綁架後被截權綁架後)
	        [State] Phase 2(拯救NPC後)
	        [State] Phase 3(神農支線 二次入清)
	        [State] Phase 4二次入清(撤離)
        Phase 1 iff "[Variable] 被蚩尤救回後" / "A6_S1_蚩尤救回羿" / "bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool" (Chiyou rescues Yi)
        Phase 2 iff "[Variable] 成功解救" / "A0_S10_SpaceshipYard_[Variable] 成功解救" / "69abeddd7aad4462a87c1ba7102feb51ScriptableDataBool" (when you release the apemen?)
            or "[Variable] 桃花村人中毒事件" / "A0_S10_SpaceshipYard_[Variable] 觸發桃花村人中毒" / "bacf1ad9920b7c041a5dea893a5bf763ScriptableDataBool" when the villager gets sick
        Phase 3 iff "A0_S10_SpaceshipYard_[Variable] 解決桃花村被二次入侵" - "97bd7c73da538a54bbdc7c27e2514dfbScriptableDataBool" defended PBV 2nd time
            or "[Variable] 桃花村被二次入侵" / "A0_S10_SpaceshipYard_[Variable] 觸發桃花村被二次入侵" / "d0dbd0c8828ceec4088788d2b547007eScriptableDataBool" 2nd PBV invasion
        Phase 4 iff "A0_S10_SpaceshipYard_[Variable] 送別演出完畢" - "1f754074ea8482c45ade3ebca8cdc782ScriptableDataBool" (true ending evac scene finished playing?)
            or from connection AG_S2 / FSP, I guess that means if you triggered evac scene directly from FSP instead of walking to GD? (how does this even happen)
         */
    }
}
