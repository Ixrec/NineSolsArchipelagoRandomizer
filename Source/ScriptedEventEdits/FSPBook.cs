using HarmonyLib;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

// Shuanshuan's book will not spawn on the FSP bookshelf if you:
// - already have the item in your inventory
// - already gave the book to Chiyou
// but we want these items/locations to be doable in any order, so these conditions need bypassing.

[HarmonyPatch]
class FSPBook {
    [HarmonyPostfix, HarmonyPatch(typeof(AbstractConditionComp), "FinalResult", MethodType.Getter)]
    static void AbstractConditionComp_get_FinalResult(AbstractConditionComp __instance, ref bool __result) {
        if (
            __instance.name == "[Condition] 還沒拿過小說" ||
            __instance.name == "[Condition] 蚩尤還沒要小說"
        ) {
            var locationsChecked = APSaveManager.CurrentAPSaveData?.locationsChecked;
            var locName = Location.FSP_SHUANSHUAN_BOOK.ToString();
            if (locationsChecked != null && locationsChecked.ContainsKey(locName) && locationsChecked[locName]) {
                // if we've already checked the Book location, then prevent it from appearing again
                if (__result == true) {
                    Log.Info($"AbstractConditionComp_get_FinalResult forcing the 'you don't already have Shuanshuan's book' check to fail since you already checked the FSP_SHUANSHUAN_BOOK location, and spawning it repeatedly feels like a bug");
                    __result = false;
                }
                return;
            }

            if (__result == true)
                // if this condition already evaluated to true, then we don't need to change its result to make the book appear
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/軒軒小說 FSM/--[States]/FSM/[State] Init/[Action] 小說在 Transition/[Condition] 還沒拿過小說") {
                Log.Info($"AbstractConditionComp_get_FinalResult forcing the 'you don't already have Shuanshuan's book' check to pass so that the FSP_SHUANSHUAN_BOOK location is checkable");
                __result = true;
            }
            if (goPath == "AG_S2/Room/NPCs/議會演出相關Binding/軒軒小說 FSM/--[States]/FSM/[State] Init/[Action] 小說在 Transition/[Condition] 蚩尤還沒要小說") {
                Log.Info($"AbstractConditionComp_get_FinalResult forcing the 'you haven't already given Shuanshuan's book to Chiyou' check to pass so that the FSP_SHUANSHUAN_BOOK location is checkable");
                __result = true;
            }
            return;
        }
    }
}
