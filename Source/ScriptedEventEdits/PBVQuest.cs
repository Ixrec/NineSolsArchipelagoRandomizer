using ArchipelagoRandomizer.Locations;
using HarmonyLib;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

[HarmonyPatch]
internal class PBVQuest {
    // If you already have the Abandoned Mines Access Token, then receiving the PBV rescue quest from Shennong will not give you another Token.
    // Since the randomizer location is triggered by the token giving, this was causing the location to never get checked if you received the Token early.
    // We unbreak that location by forcing the "do you already have the token?" condition to always be false.

    static string minesTokenItemDataName = "(重要道具)16_礦坑鑰匙";

    [HarmonyPostfix, HarmonyPatch(typeof(HasItemCondition), "isValid", MethodType.Getter)]
    static void HasItemCondition_get_isValid(HasItemCondition __instance, ref bool __result) {
        //Log.Warning($"HasItemCondition_get_isValid {__instance.item.name} / {__instance.item.Title} / __result={__result}");
        if (__instance.item.name == minesTokenItemDataName && __result == true) {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/FSM Animator/CutScene/[CtuScene] 軒軒神農擔心桃花村/--[States]/FSM/[State] 給桃花村鑰匙/[Action] 給桃花村鑰匙/[Condition] 沒有桃花村鑰匙") {
                Log.Info($"forcing HasItemCondition to pretend you don't have the {__instance.item.Title} so that the 'FSP: Receive Peach Blossom Village Quest' location trigger works");
                __result = false;
            }
        }
    }
}
