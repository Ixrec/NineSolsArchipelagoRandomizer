using ArchipelagoRandomizer.Locations;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer;

// The game's "post-PonR" (Point of no Return) state adds many barriers to force you to the ending-related scripted events,
// but in randomizer these basically make post-PonR state unplayable.
// We make the state playable by actively preventing the game from adding the barriers that cause trouble for rando.

[HarmonyPatch]
internal class PostPONRBarriers {
    [HarmonyPrefix, HarmonyPatch(typeof(FlagBoolCondition), "isValid", MethodType.Getter)]
    static bool FlagBoolCondition_get_isValid(FlagBoolCondition __instance, ref bool __result) {
        if (__instance!.flagBool == null)
            return true;
        var flag = __instance.flagBool.boolFlag;

        // This is the post-PonR save data flag. Most of the changes we need to make are simply overriding checks of this flag.
        if (flag.FinalSaveID != "640eb10597916684cad00ab131593eb4ScriptableDataBool") // "A11_S2_Laboratory_remake_[Variable] 逃出易公魂境_進入最後階段"
            return true;

        var path = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);

        var shouldPreventPostPonrEffects = false;
        var levelName = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        if (levelName == "AG_S2") { // FSP
            // Leave FSP exactly as it was, the ruined version is totally unsalvageable for rando
            shouldPreventPostPonrEffects = true;
        } else if (levelName == "GameLevel") { // GD and some other places we don't care about
            // the only change in GD is a goop barrier to the left, so let's clear that out
            shouldPreventPostPonrEffects = true;
        } else if (levelName == "A2_S6") { // CTH
            // Keep the nymph puzzle room open, but let the BGM be changed
            shouldPreventPostPonrEffects = (__instance.gameObject.name == "[Condition] PhaseTwo == true");
        } else if (levelName == "AG_S1") { // CH
            // Remove the 3 barriers controlled by "General FSM Object_On And Off Switch 最終階段切換 Variant", but let the BGM be changed
            shouldPreventPostPonrEffects = path.Contains("/General FSM Object_On And Off Switch 最終階段切換 Variant/");
        }
        // TRC is handled separately in the patch below
        // all other levels have nothing we want to stop

        if (shouldPreventPostPonrEffects) {
            __result = false; // pretend Eigong has not escaped
            return false;
        } else {
            return true;
        }
    }

    // TRC is special because all of its post-PonR changes are controlled by a single FSM (with a single flag check), and we only want to disable some of them.
    // That means our usual FlagBoolCondition patch won't work. Instead, we detect TRC loads and manually disable the relevant GameObjects.

    private static string trcPostPONRBarriers = "A11_S1/Room/Phase相關切換Gameplay----------------/General FSM Object_On And Off Switch 最終階段切換 Variant/FSM Animator/LogicRoot/Phase3_On";

    [HarmonyPostfix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        try {
            if (__instance.name != "A11_S1") {
                return;
            }

            var go = GameObject.Find(trcPostPONRBarriers);

            // Unfortunately, two of the children have identical names, so we have to rely on child indices instead of GO paths.
            var barrierToYinglongCanal = go.transform.GetChild(1).gameObject;
            if (barrierToYinglongCanal.activeSelf) {
                Log.Info($"PostPONRBarriers deactivating TRC post-PonR barrier to Yinglong Canal");
                barrierToYinglongCanal.SetActive(false);
            }
            var barrierLeftOfNode = go.transform.GetChild(2).gameObject;
            if (barrierLeftOfNode.activeSelf) {
                Log.Info($"PostPONRBarriers deactivating TRC post-PonR barrier on the left side of the root node");
                barrierLeftOfNode.SetActive(false);
            }
            var barrierRightOfNode = go.transform.GetChild(3).gameObject;
            if (barrierRightOfNode.activeSelf) {
                Log.Info($"PostPONRBarriers deactivating TRC post-PonR barrier on the right side of the root node");
                barrierRightOfNode.SetActive(false);
            }

        } catch (Exception ex) {
            Log.Error($"PostPONRBarriers::GameLevel_Awake threw: {ex.Message}\nwith stack:\n{ex.StackTrace}\nand InnerException: {ex.InnerException?.Message}\nwith stack:\n{ex.InnerException?.StackTrace}");
        }
    }
}
