using HarmonyLib;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

// In Empyrean District (Living Area), the opera theater with Fuxi's sanctum can end up in a bunch of
// weird and softlock-y states because a lot of its state machine conditions are checking for
// the Seal of Fuxi *item*, not the flag that you just finished Fuxi's vital sanctum.

// In other words, if you have checked the sanctum AP location already, then we need to mess with
// each of the conditions that's "incorrectly" looking for the seal item.

[HarmonyPatch]
class OperaTheater {
    [HarmonyPostfix, HarmonyPatch(typeof(AbstractConditionComp), "FinalResult", MethodType.Getter)]
    static void AbstractConditionComp_get_FinalResult(AbstractConditionComp __instance, ref bool __result) {
        var locationsChecked = APSaveManager.CurrentAPSaveData?.locationsChecked;
        var locName = Location.EDLA_VITAL_SANCTUM.ToString();
        var sanctumChecked = (locationsChecked != null && locationsChecked.ContainsKey(locName) && locationsChecked[locName]);
        if (!sanctumChecked)
            return;

        if (__instance.name == "[Condition] 拿到伏羲玉璽") {
            if (__result == true)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/--[States]/FSM/[State] Init/[Action] 拿到了伏羲玉璽後/[Condition] 拿到伏羲玉璽") {
                Log.Info($"AbstractConditionComp_get_FinalResult forcing the opera theater into its post-Vital Sanctum state, since you checked the sanctum location but don't have the seal item");
                __result = true;
            }
        }

        if (__instance.name == "[Condition] 沒有王璽") {
            if (__result == false)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/[Mech]BlastDoorx6_FSM Variant /--[States]/FSM/[State] Init/[Transition] Closed/[Condition] 沒有王璽") {
                Log.Info($"AbstractConditionComp_get_FinalResult preventing the left opera theater door from waking up closed, since you checked the sanctum location but don't have the seal item");
                __result = false;
            }
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/[Mech]BlastDoorx6_FSM Variant  (2)/--[States]/FSM/[State] Init/[Transition] Closed/[Condition] 沒有王璽") {
                Log.Info($"AbstractConditionComp_get_FinalResult preventing the right opera theater door from waking up closed, since you checked the sanctum location but don't have the seal item");
                __result = false;
            }

            // These transitions keep getting re-checked forever, so we can't afford to log anything in these cases
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/[Mech]BlastDoorx6_FSM Variant /--[States]/FSM/[State] Opened/[Transition] ToClosing###1/[Condition] 沒有王璽") {
                //Log.Info($"AbstractConditionComp_get_FinalResult preventing the left opera theater door from closing, since you checked the sanctum location but don't have the seal item");
                __result = false;
            }
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/[Mech]BlastDoorx6_FSM Variant  (2)/--[States]/FSM/[State] Opened/[Transition] ToClosing###1/[Condition] 沒有王璽") {
                //Log.Info($"AbstractConditionComp_get_FinalResult preventing the right opera theater door from closing, since you checked the sanctum location but don't have the seal item");
                __result = false;
            }
        }

        if (__instance.name == "[Condition] 有王璽") {
            if (__result == true)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/[Mech]BlastDoorx6_FSM Variant /--[States]/FSM/[State] Closed/[Transition] ToClosing/[Condition] 有王璽") {
                Log.Info($"AbstractConditionComp_get_FinalResult forcing the left opera theater door to open, since you checked the sanctum location but don't have the seal item");
                __result = true;
            }
            if (goPath == "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/[Mech]BlastDoorx6_FSM Variant  (2)/--[States]/FSM/[State] Closed/[Transition] ToClosing/[Condition] 有王璽") {
                Log.Info($"AbstractConditionComp_get_FinalResult forcing the right opera theater door to open, since you checked the sanctum location but don't have the seal item");
                __result = true;
            }
        }
    }
}
