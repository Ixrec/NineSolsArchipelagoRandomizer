using HarmonyLib;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
class LimitlessRealm {
    // For now, all we want to do with Limitless Realm tutorial segments is skip them.
    // Note that this patch does not affect the FSP's Limitless Realm segments; those use a state machine we handle separately below.
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

    /*
     * Make the FSP root node always appear in its default state, so it won't send you into any of
     * the usual Lear cutscenes (which are redundant because we start rando with full teleport).
     */
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
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
    }
}
