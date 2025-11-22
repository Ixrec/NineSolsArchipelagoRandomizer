using HarmonyLib;
using RCGFSM.Variable;

namespace ArchipelagoRandomizer;

// In Grotto of Scriptures (East), you can trigger a large spike ball to break open a path to Grotto (West).
// If you return to Grotto (Entry) you'll find this spike ball has crashed through the mini-greenhouse,
// opening a large gap in the floor and blocking the drop down to the coffin.
// From the randomizer's point of view, this adds and deletes multiple critical region connections in an
// irreversible way, which is just not compatible with any sane rando logic. So we have to prevent it.

[HarmonyPatch]
class GoSEntrySpikeBall {
    [HarmonyPostfix, HarmonyPatch(typeof(AbstractConditionComp), "FinalResult", MethodType.Getter)]
    static void AbstractConditionComp_get_FinalResult(AbstractConditionComp __instance, ref bool __result) {
        if (__instance.name.StartsWith("[Condition] var On = ")) {
            if (__instance.transform.parent.parent.parent.parent.parent.parent.name != "A10S1_GlassRoof")
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A10_S1/Room/Prefab/A10S1_GlassRoof/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Init/[Transition] On/[Condition] var On = true") {
                if (__result == true) {
                    Log.Info($"GoSEntrySpikeBall::AbstractConditionComp_get_FinalResult preventing spike ball damage in Grotto (Entry)");
                    __result = false;
                }
            }
            if (goPath == "A10_S1/Room/Prefab/A10S1_GlassRoof/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Init/[Transition] Off/[Condition] var On = false") {
                if (__result == false) {
                    Log.Info($"GoSEntrySpikeBall::AbstractConditionComp_get_FinalResult preventing spike ball damage in Grotto (Entry)");
                    __result = true;
                }
            }
        }
    }

    // Annoyingly, this particular FSM also sets the "spike ball triggered" flag right after checking it.
    // So we have to patch away that redundant set, as well as the earlier check.
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static bool SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        if (__instance.targetFlag?.boolFlag?.FinalSaveID == "c11fd0cd94ffb2f4bbf766378f4ebca2ScriptableDataBool") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A10_S1/Room/Prefab/A10S1_GlassRoof/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Off/[Action]  OnOffFlag = false") {
                Log.Info($"GoSEntrySpikeBall::SetVariableBoolAction_OnStateEnterImplement preventing Grotto (Entry) from resetting the spike ball");
                return false;
            }
        }
        return true;
    }
}
