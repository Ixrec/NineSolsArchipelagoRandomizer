using HarmonyLib;
using UnityEngine;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

[HarmonyPatch]
class ApemenEvacuation {
    // In vanilla, the village is in its invaded state as soon as you finish the intro, but the trigger for the apemen to move into Galactic Dock
    // is not any of the Jiequan scenes; it's Chiyou rescuing Yi from Factory (Underground). Since this is very easy to bypass in rando, and looks
    // like a pretty serious bug if you fight Lieguan early, it's worth "fixing" even if there are no locations behind it atm.
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        if (__instance.name == "[State] Phase 0") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "GameLevel/Room/Prefab/村民避難所_階段 FSM Object/--[States]/FSM/[State] Phase 0") {
                Log.Info($"GeneralState_OnStateEnter forcing the Apemen to evacuate into Galactic Dock even though the base game doesn't think they've done that yet");
                var initialEvacState = GameObject.Find("GameLevel/Room/Prefab/村民避難所_階段 FSM Object/--[States]/FSM/[State] Phase 1(被截權綁架後被截權綁架後)");
                initialEvacState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
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
