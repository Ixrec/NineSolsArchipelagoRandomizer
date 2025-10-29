using HarmonyLib;

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
                    Log.Info($"GoSEntrySpikeBall undoing spike ball damage in Grotto (Entry)");
                    __result = false;
                }
            }
            if (goPath == "A10_S1/Room/Prefab/A10S1_GlassRoof/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Init/[Transition] Off/[Condition] var On = false") {
                if (__result == false) {
                    Log.Info($"GoSEntrySpikeBall undoing spike ball damage in Grotto (Entry)");
                    __result = true;
                }
            }
        }
    }
}
