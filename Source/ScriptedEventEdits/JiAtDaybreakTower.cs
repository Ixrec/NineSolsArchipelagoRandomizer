using HarmonyLib;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class JiAtDaybreakTower {
    /*
     * Make Ji always appear at Daybreak Tower to give you the music sheet
     */
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        // despite the "On" name, this state represents Ji *not* being at the Daybreak Tower
        if (__instance.name == "[State] On") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] On") {
                Log.Info($"GeneralState_OnStateEnter forcing Ji to exist at Daybreak Tower despite the base game hiding him");
                var jiDoesExistAtDaybreakTowerState = GameObject.Find("A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Off");
                jiDoesExistAtDaybreakTowerState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
    }
}
