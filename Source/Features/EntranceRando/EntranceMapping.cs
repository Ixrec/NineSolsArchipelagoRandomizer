using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ArchipelagoRandomizer.Features.EntranceRando.PortalData;
using static SceneConnectionPoint;

namespace ArchipelagoRandomizer.Features.EntranceRando;

// This file focuses on the technical details of actually changing which entrances/portals
// send Yi to which other entrances/portals in-game.
// Thus, this is where we keep all the walls of text documenting how I arrived at this implementation.

/* Distinguishing "entrances" / portals
 * 
 * SceneConnectionPoint is the main type that represents loading transitions between areas,
 * including the ones we want to randomize. Note that many SceneConnectionPoints are for things we
 * don't want to randomize (e.g. cutscene transitions), and many entrances we do want to randomize
 * have multiple SceneConnectionPoint for various reasons.
 * 
 * SingletonBehaviour<GameCore>.Instance.gameLevel.name is the "level" name
 * SceneConnectionPoint.scene.SceneName is the "scene" name
 * SceneConnectionPoint.connectionID is the "connection id"
 *      I'll often call it a "connection name" since it's not a unique id, and it's usually human-readable
 *
 * Scene names and connection ids/names are not too hard to access, but I never found a way to access "level" names
 * for scenes other than the currently loaded one. Fortunately our patches only need the current level name.
 * FTR GameCore holds previous/current scene names, but in practice we only ever needed target scene.
 * 
 * Ideally, connection name alone would be enough to identify a transition. Complications include:
 * - Many A->B transitions have a corresponding B->A transition that uses the same connection name,
 * so something from the current or target scene/level is also necessary.
 * - (current level/scene, target level/scene) is of course not sufficient whenever there are multiple connections
 * between the same two areas, e.g. FGH and FU, so connection name is necessary
 * - For some reason, most areas have unused connections with name AG_Tutorial_Lear_S2_識破JumpKick and target scene
 * A2_S6_LogisticCenter_Final. While we don't need to edit any of these, these are cases where theoretically
 * both current and target areas matter.
 * - Even more technically, (level, scene, connection name) is not quite enough because FU has two "Connection_BoxChangeScene" SCPs
 * with level A6_S1, scene A1_S3_InnerHumanDisposal_Final, and connection A6_S1_To_A1_S3. Fortunately, both are unused.
 * 
 * In practice, we currently use triples of (current level name, target scene name, connection name) to identify transitions.
 * This seems to work well for all the transitions we want to remap, and all 3 strings are easy enough to get that
 * it's not worth trying to figure out if we can remove one of them.
 */

/* Terminology
 * 
 * - A "portal" is a single in-game place in one area that, when Yi walks into it, triggers a transition to another portal.
 * Portal names are exactly the same in vanilla and all entrance rando seeds.
 * 
 * - A "(two-way) connection" is a pair of linked portals. The vanilla game has a hardcoded set of connections,
 * and "entrance rando" is all about randomly choosing a different set of connections.
 * Note this definition assumes every A->B transition has a corresponding B->A transition.
 * This is called "coupled" ER. If we decide we want "uncoupled" ER too, we'll rethink this.
 * 
 * - An "AP entrance" is a *directed* connection from one portal to another portal.
 * Confusingly, these two portals are often called the "entrance" and "exit" of that entrance.
 * I will avoid this term whenever possible, and prepend "AP" when I have to use it.
 * 
 * - A "departure" is the act of entering a portal.
 * - An "arrival" is the act of exiting a portal.
 * Making a Nine Sols portal work for departures is very different from making it work for arrivals,
 * so it's extremely important that we avoid mixing up these directions in the implementation.
 */

/* Code executed during transitions
 * 
 * SceneConnectionPoint.connectionId and SceneConnectionPoint.scene are how the vanilla game determines which
 * SCP in the current scene maps to which SCP in which other scene after Yi runs into an SCP/portal trigger.
 * In order to change the portal mapping, these are the two details we must change.
 * 
 * Every way of Yi entering a portal eventually ends up at a line like:
 *      SingletonBehaviour<GameCore>.Instance.ChangeScene(connection.GetData());
 * 
 * SceneConnectionPoint::TriggerChangeScene() is probably the most common caller of GameCore::ChangeScene(),
 * but AnimationChangeScene::ChangeScene() and DoorChangeScene::DoorInteractReaction() have also been observed.
 * 
 * GetData() copies SCP.connectionId and SCP.scene.sceneName onto a ChangeSceneData object.
 * That makes GetData() a particularly appealing method to patch, because it lets us
 * change the sceneName (a simple string) instead of scene (a much more complex type).
 * 
 * Thus, the core of our final implementation is:
 * - a SCP::GetData() prefix patch for editing connectionId before GetData() constructs an unpatchable delegate referencing it
 * - a GameCore::ChangeScene() prefix patch for editing sceneName after it's been copied from SCP::scene, but before it gets used
 *   - an SCP::GetData() *postfix* patch might also work, haven't tried that
 * 
 * The full implementation needs a few more patches for corner cases that only affect a handful of portals,
 * but conceptually these two patches are the important/"core" ones that affect every single portal.
 */

/* Hazards
 * 
 * My first attempt at ER edited most SCPs' connection names in SceneConnectionPoint::Awake().
 * This turned out to be a bad idea, because editing them this early screws up *arrivals* into the scene.
 * It was rarely a fatal error (which is why it took me so long to figure this out), but it often skipped
 * animations, and could lead to Yi arriving at completely the wrong portal when some of an area's portals
 * are mapped to each other.
 * 
 * Do not patch SCP::FindNextSceneConnection(), because that will completely break hot reloading.
 * If you do try to hot reload with a FindNextSceneConnection() patch, you'll immediately softlock
 * on a black screen. I have no idea why it's like this.
 */

internal class EntranceMapping {
    // for testing the one-way portals
    public static Dictionary<Portal, Portal> EntranceMap = new Dictionary<Portal, Portal> {
        { Portal.OW_MIDDLE_LEFT_PORTAL, Portal.GREENHOUSE_TOP_ELEVATOR_SHAFT },
        { Portal.OW_UPPER_LEFT_CRATES, Portal.WOS_TOP_PORTAL },
        { Portal.OW_LOWER_RIGHT_PORTAL, Portal.CTH_UPPER_LEFT_VENT_SHAFT },
        { Portal.OW_MIDDLE_RIGHT_PORTAL, Portal.FU_UPPER_RIGHT_HOLE_PORTAL },

        { Portal.GOSY_LOWER_ELEVATOR_SHAFT, Portal.CH_UPPER_RIGHT_PORTAL },
        { Portal.GREENHOUSE_BOTTOM_PORTAL, Portal.CH_UPPER_RIGHT_PORTAL },
        { Portal.CH_BOTTOM_VENT_SHAFT, Portal.CH_UPPER_RIGHT_PORTAL },
        { Portal.FGH_BOTTOM_RIGHT_HOLE_PORTAL, Portal.CH_UPPER_RIGHT_PORTAL },
    };

    [HarmonyPrefix, HarmonyPatch(typeof(SceneConnectionPoint), "Awake")]
    static void SceneConnectionPoint_Awake(SceneConnectionPoint __instance) {
        // Almost all SCPs in the game use FindConnectionMode.ID, and ER broke uniquely for FU_LEFT_PORTAL because it's one of the few .Distance users.
        // Specifically, without this patch, FU_LEFT_PORTAL mapped to any other FU_* portal would incorrectly spawn you at FU_LEFT_PORTAL again.
        // I assume this happens because FU_LEFT_PORTAL is closest to itself, and .Distance mode assumes you're changing scenes.
        // Fortunately, simply changing FU_LEFT_PORTAL back to .ID mode makes it work the same as every other portal.
        if (ERMain.entranceMappingActive && __instance.findMode != FindConnectionMode.ID) {
            Log.Info($"EntranceRando changing SceneConnectionPoint ({__instance} / {__instance.scene.SceneName} / {__instance.connectionID})'s .findMode from FindConnectionMode.Distance to .ID");
            __instance.findMode = FindConnectionMode.ID;
        }
        //Log.Warning($"SceneConnectionPoint_Awake {__instance.scene.SceneName} / {__instance.connectionID} / {__instance.changeSceneMode}");
    }

    private static Dictionary<DepartureIds, Portal> HalfEditedDepartures = new Dictionary<DepartureIds, Portal> { };

    [HarmonyPrefix, HarmonyPatch(typeof(SceneConnectionPoint), "GetData")]
    static void SceneConnectionPoint_GetData(SceneConnectionPoint __instance) {
        var level = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        //Log.Warning($"SceneConnectionPoint_GetData {level} / {__instance.scene.SceneName} / {__instance.connectionID}");
        if (!ERMain.entranceMappingActive) return;

        var ids = new DepartureIds(level, __instance.scene.SceneName, __instance.connectionID);
        if (!VanillaDepartures.TryGetValue(ids, out var departurePortal))
            return;
        if (!EntranceMap.TryGetValue(departurePortal, out var arrivalPortal))
            return;
        if (!VanillaArrivals.TryGetValue(arrivalPortal, out var arrivalIds))
            return;

        Log.Info($"mapping {departurePortal} to {arrivalPortal} part 1/2: changing connectionId from {__instance.connectionID} to {arrivalIds.connectionName}");
        __instance.connectionID = arrivalIds.connectionName;

        var halfEditedIds = new DepartureIds(ids.levelName, ids.sceneName, arrivalIds.connectionName);
        HalfEditedDepartures[halfEditedIds] = departurePortal;
        //Log.Warning($"editing {departurePortal} to connect to {arrivalPortal} part 1.5: mapped halfEditedIds to {departurePortal}");
    }

    private static string? lastArrivalConnectionId = null;

    [HarmonyPrefix, HarmonyPatch(typeof(GameCore), "ChangeScene", [typeof(SceneConnectionPoint.ChangeSceneData), typeof(bool), typeof(bool), typeof(float)])]
    static void GameCore_ChangeScene(GameCore __instance, ref SceneConnectionPoint.ChangeSceneData changeSceneData) {
        var level = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        //Log.Warning($"GameCore_ChangeScene {level} / {changeSceneData.sceneName} / {changeSceneData.connectionID}");
        if (!ERMain.entranceMappingActive) return;

        var ids = new DepartureIds(level, changeSceneData.sceneName, changeSceneData.connectionID);
        // Use HalfEditedDepartures instead of VanillaDepartures, because the previous patch has already edited the connectionId
        if (!HalfEditedDepartures.TryGetValue(ids, out var departurePortal))
            return;
        if (!EntranceMap.TryGetValue(departurePortal, out var arrivalPortal))
            return;
        if (!VanillaArrivals.TryGetValue(arrivalPortal, out var arrivalIds))
            return;

        Log.Info($"mapping {departurePortal} to {arrivalPortal} part 2/2: changing sceneName from {changeSceneData.sceneName} to {arrivalIds.sceneName}");
        changeSceneData.sceneName = arrivalIds.sceneName;

        lastArrivalConnectionId = changeSceneData.connectionID;
    }

    // Unfortunately the vanilla game impl of IsFromThisConnectionCondition.isValid checks the "previous scene" as well as the connection id,
    // so arrivals from unexpected scenes can get randomly broken by not running some of the necessary animations.
    // In practice this was breaking GOSW_LOWER_RIGHT_ELEVATOR, ST_BOTTOM_ELEVATOR and AH_RIGHT_ELEVATOR by leaving Yi trapped below the elevators.
    [HarmonyPrefix, HarmonyPatch(typeof(IsFromThisConnectionCondition), "isValid", MethodType.Getter)]
    static bool IsFromThisConnectionCondition_isValid(IsFromThisConnectionCondition __instance, ref bool __result) {
        //Log.Warning($" === IsFromThisConnectionCondition_isValid lastArrivalConnectionId = {lastArrivalConnectionId}, target_conn_id = {__instance.targetConnection.connectionID}, savePoint = {__instance.savePoint}, from flag = {__instance.targetConnection.fromConnection}, target scene = {__instance.targetConnection.scene.SceneName}");
        if (
            ERMain.entranceMappingActive &&
            __instance.savePoint == null &&
            __instance.targetConnection.fromConnection == true &&
            __instance.targetConnection.connectionID == lastArrivalConnectionId
        ) {
            Log.Info($"forcing an IsFromThisConnectionCondition for connection id {__instance.targetConnection.connectionID} to evaluate to true");
            // now that the "previous scene" check is the only one left, skip it by forcing the result to true
            __result = true;
            return false;
        }
        return true; // leave the vanilla behavior alone
    }
}
