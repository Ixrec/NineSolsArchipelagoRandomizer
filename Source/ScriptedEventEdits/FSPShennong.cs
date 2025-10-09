using HarmonyLib;
using System;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class FSPShennong {
    // we check the in-game inventory instead of the AP inventory because, without shop rando, not all poisons are AP items
    private static bool HasPoison() {
        var inventoryTab2 = SingletonBehaviour<UIManager>.Instance.allItemCollections[1].rawCollection;
        for (int i = 25; i <= 39; i++) { // MedicinalCitrine through GutwrenchFruit
            if ((((ItemData)inventoryTab2[i])?.ownNum.CurrentValue ?? 0) > 0) {
                return true;
            }
        }
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(AbstractConditionComp), "FinalResult", MethodType.Getter)]
    static void AbstractConditionComp_get_FinalResult(AbstractConditionComp __instance, ref bool __result) {
        // if Shennong has been given a poison already, then we don't need to edit anything
        var shennongSavedFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["739a14dc66e1c674e820f75543e5a662ScriptableDataBool"];
        if (shennongSavedFlag.CurrentValue)
            return;

        // if Shennong has not yet been given a poison, and the player has a poison to give, then
        // we can safely put FSP Shennong in his sick state without blocking other FSP scenes
        if (__instance.name == "[Condition] 去過腦室") {
            if (__result == true)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/NPC_ShinNon_Base/NPC_ShinNon_Base_FSM/--[States]/FSM/[State] Init/[Transition] 沒救過神農->中毒神農在家/[Condition] 去過腦室") {
                if (!HasPoison())
                    return;

                Log.Info($"FSPShennong removing the 'has seen Cortex Center jumpscare' condition for sick Shennong to move to FSP");
                __result = true;
                return;
            }
        }
        if (__instance.name == "[Condition] 拿完芶芒權限") {
            if (__result == true)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/NPC_ShinNon_Base/NPC_ShinNon_Base_FSM/--[States]/FSM/[State] Init/[Transition] 沒救過神農->中毒神農在家/[Condition] 拿完芶芒權限") {
                if (!HasPoison())
                    return;

                Log.Info($"FSPShennong removing the 'has Goumang seal' condition for sick Shennong to move to FSP");
                __result = true;
                return;
            }
        }

        // if Shennong has not yet been given a poison, but the player has no poison items yet, then
        // we need to allow all of Shennong's other scenes to play out despite him not being "saved"
        if (__instance.name == "[Condition] 救過神農") {
            if (__result == true)
                return;
            if (HasPoison())
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/--[States]/FSM/[State] Init/[Transition] 軒軒神農擔心桃花村 Transition/[Condition] 救過神農") {
                Log.Info($"FSPShennong removing the 'has saved Shennong' condition for Shennong to offer the PBV quest");
                __result = true;
                return;
            }

            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/--[States]/FSM/[State] Init/[Action] 選第一個合法的文物演出Transition/[Transition]種子_第一次成長演出/[Condition] 救過神農") {
                Log.Info($"FSPShennong removing the 'has saved Shennong' condition for the Unknown Seed's first growth scene");
                __result = true;
                return;
            }
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/--[States]/FSM/[State] Init/[Action] 選第一個合法的文物演出Transition/[Transition]種子_第二次成長演出/[Condition] 救過神農") {
                Log.Info($"FSPShennong removing the 'has saved Shennong' condition for the Unknown Seed's second growth scene");
                __result = true;
                return;
            }
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/--[States]/FSM/[State] Init/[Action] 選第一個合法的文物演出Transition/[Transition]種子_最終成長演出/[Condition] 救過神農") {
                Log.Info($"FSPShennong removing the 'has saved Shennong' condition for the Unknown Seed's third growth scene");
                __result = true;
                return;
            }

            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/NPC_ShinNon_Base/NPC_ShinNon_Base_FSM/--[States]/FSM/[State] Init/[Transition] 救過神農->神農在家/[Condition] 救過神農") {
                Log.Info($"FSPShennong removing the 'has saved Shennong' condition for Shennong to exist normally in the FSP");
                __result = true;
                return;
            }
        }
        if (__instance.name == "[Condition] 解救過神農") {
            if (__result == true)
                return;
            if (HasPoison())
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/NPC_ShinNon_Base/NPC_ShinNon_Base_FSM/--[States]/FSM/[State] 神農演出中/[Action] 演出完畢->正常神農/[Condition] 解救過神農") {
                Log.Info($"FSPShennong removing the 'has saved Shennong' condition for Shennong to return to his default state after giving the PBV quest");
                __result = true;
                return;
            }
        }
    }
}
