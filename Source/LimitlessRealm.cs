using HarmonyLib;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
class LimitlessRealm
{
    // For now, all we want to do with Limitless Realm segments is skip them.
    [HarmonyPrefix, HarmonyPatch(typeof(SavePoint), nameof(SavePoint.CurrentTutorial))]
    public static bool SavePoint_CurrentTutorial(SavePoint __instance, ref TutorialEntry? __result) {
        // The vanilla behavior is basically to return .berserkBindingTutorial, whether or not it's null
        if (__instance.berserkBindingTutorial != null) {
            Log.Info($"SavePoint_CurrentTutorial changing __result to be null to avoid a Limitless Realm segment");
            __result = null;
            return false;
        }
        return true;
    }

    /* Notes:
     * 
     * SavePoint is the most important Component for root nodes / teleport points / save points
     * I believe the root nodes which can become "anomalous" and connect to a Limitless Realm tutorial sequence 
     * will always have a non-null .berserkBindingTutorial (TutorialEntry)
     * SavePoint.berserkConnectionScene (SceneConnectionPoint) is just SavePoint.berserkBindingTutorial.connectionpoint
     * There is also a .berserkBindingTutorials plural, but it appears to be consistently unused.
     * A TutorialEntry object is basically a SceneConnectionPoint with some validity checks (usually if the player has an ability yet),
     * since the actual "tutorial" is always its own scene.
     */
}
