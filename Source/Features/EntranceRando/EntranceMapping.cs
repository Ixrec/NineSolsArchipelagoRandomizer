using HarmonyLib;
using NineSolsAPI;
using System.Collections.Generic;
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

/* Known level/scene ids
 * 
 * level A10_S3 / scene A10_S3_HistoryTomb_Right / GoSE
 *      level A10_SG2 / scene A10_SG2_Cave2 / Guiguzi's Tomb
 *      level A10_SG1 / scene A10_SG1_Cave1 / Yin Jifu's tomb
 * level A10_S4 / scene A10_S4_HistoryTomb_Left / GoSW
 *      scene A10_SG6_SisterMemory is a variation of GoSW for the Heng flashback
 *      level A10_SG4 / scene A10_SG4_Cave4 / Luyan's tomb
 * level A10_SG4 / scene A10_S5_Boss_Jee / Ancient Stone Pillar aka Ji's arena
 *      level ??? / scene VR_Memory_Jee
 *      level ??? / scene A10_SG5_LearZone / Lear's Tomb
 * level A10_S1 / scene A10_S1_TombEntrance_remake / GoSY
 * level A3_S1 / scene A3_S1_GardenRuins_Final / LYR
 *      level A3_SG1 / scene A3_SG1 / shield statues room
 *      level A3_SG1 / scene A3_SG2 / nymph puzzle room
 * level A3_S2 / scene A3_S2_GreenHouse_Final / Greenhouse
 * level A3_S5_BossGouMang_GameLevel / scene A3_S5_BossGouMang_Final / Agrarian Hall
 *      level ??? / scene VR_Memory_Goumang
 * level A3_S3 / scene A3_S3_OxygenChamber_Final / W&OS
 * level A3_S7 / scene A3_S7_DragonWay_Final / YC
 * level A9_S4 / scene A9_S4 / ST
 * level A9_S1 / scene A9_S1_Remake_4wei / EDP
 * level A9_S2 / scene A9_S2_Remake_4wei / EDLA
 *      level ??? / scene VR_Memory_伏羲
 * level A9_S3 / scene A9_S3 / EDS
 *      level ??? / scene VR_Memory_伏羲&女媧
 * level A11_S1 / scene A11_S1_Hospital_remake / TRC
 * level A2_S6 / scene A2_S6_LogisticCenter_Final / CTH
 *      level A4_SG2 / scene A2_SG5_LaserRoom / laser puzzle room
 * level AG_S1 / scene AG_S1_SenateHall / CH
 * level AG_S2 / scene AG_S2_YiBase / FSP
 * level A2_S2 / scene A2_S2_ReactorRight_Final / PRE
 *      level A2_SG4 / scene A2_SG4_MemoryGondola_Final
 * level A2_S1 / scene A2_S1_ReactorMiddle_Final / PRC
 *      level A2_SG1 / scene A2_SG1_ReactorControlRoom
 * level A2_S5_ BossHorseman_GameLevel / scene A2_S5_BossHorseman_Final / RP
 * level A2_S3 / scene A2_S3_ReactorLeft_Final / PRW
 * level A1_S2_GameLevel / scene A1_S2_ConnectionToElevator_Final / AFE
 * level A1_S3_GameLevel / scene A1_S3_InnerHumanDisposal_Final / AFD
 * level A1_S1_GameLevel / scene A1_S1_HumanDisposal_Final / AFM
 * level GameLevel / scene A0_S10_SpaceshipYard / GD
 * level A7_S1 / scene A7_S1_BrainRoom_Remake / CC
 * level A5_S1 / scene A5_S1_CastleHub_remake / FGH
 *      level A5_S4b / scene A5_S4b_HerbRoom_Remake / FGH's nymph puzzle room
 * level A5_S4 / scene A5_S4_CastleMid_Remake_5wei / FPA
 *      level A5_S4b / scene A5_S4d_PoisonRoom / FPA's pharmacy
 *          !!! notice the FGH and FPA side rooms have identical level names
 * level A5_S5 / scene A5_S5_JieChuanHall / Shengwu Hall
 * level A6_S1 / scene A6_S1_AbandonMine_Remake_4wei / FU
 * level A6_S3 / scene A6_S3_Tutorial_And_SecretBoss_Remake / AM
 * level A0_S7 / scene A0_S7_CaveReturned / UC
 * level GameLevel / scene A0_S8_VillageReturned / PBV West
 * level GameLevel / scene A0_S9_AltarReturned / PBV East
 * level A5_S3 / scene A5_S3_UnderCastle_Remake_4wei / FMR
 * level A5_S2 / scene A5_S2_Jail_Remake_Final / Prison
 * level A4_S1 / scene A4_S1_NewBridgeToWarehouse_Final / OW
 * level A4_S2 / scene A4_S2_RouteToControlRoom_Final / IW
 *      level A4_SG1 / scene A4_SG1 / IW nymph puzzle room
 * level A4_S3 / scene A4_S3_ControlRoom_Final / BR
 * level A0_S6 / scene A4_S6_DaoBase_Final / Yangu Hall
 */

[HarmonyPatch]
internal class EntranceMapping {
    public static Dictionary<Portal, Portal> EntranceMap = new Dictionary<Portal, Portal> {
        // hardcode test mappings here
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

        if (arrivalPortal == Portal.EDP_TOP_ELEVATOR) {
            var pinkWaterfallDisabled = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["a2dba9e5-61cf-453a-8981-efb081fb0b11_4256ef2ec22f942dc9f70607bb00391fScriptableDataBool"];
            pinkWaterfallDisabled.CurrentValue = true;
            ToastManager.Toast("disabling the pink waterfall at the top of ED (Passages)");
        } else if (arrivalPortal == Portal.AM_RIGHT_PORTAL) {
            var minesDoorOpened = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["95df6e5e-f2ae-413a-996c-9dae1420b836_104b8d0cf618434478e9e75ae3ee9d88ScriptableDataBool"];
            minesDoorOpened.CurrentValue = true;
            ToastManager.Toast("opening the Abandoned Mines door");
        }
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

    /*
level A0_S6 / scene A4_S6_DaoBase_Final / Yangu Hall

during cutscenes/Claw fight:
[Warning:ArchipelagoRandomizer] A4_S5 / Connection_Prefab_To_A4_S6 (SceneConnectionPoint) -> A4_S6_DaoBase_Final / A4_S5_BossRoom_To_A4_S6
[Warning:ArchipelagoRandomizer] A4_S5 / Connection_Prefab_From_A4_S3 (SceneConnectionPoint) -> A4_S3_ControlRoom_Final / A4_S3_To_A4_S5_BossRoom

on defeating Claw:
[Warning:ArchipelagoRandomizer] GameCore_ChangeScene A4_S5 -> A4_S6_DaoBase_Final / A4_S5_BossRoom_To_A4_S6

post-fight Yangu Hall:
[Warning:ArchipelagoRandomizer] A0_S6 / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_TaoChang / A4_S6_SleepPod_To_VR_TaoChang
[Warning:ArchipelagoRandomizer] A0_S6 / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_TaoChang / VR_TaoChang_To_A4_S6
[Warning:ArchipelagoRandomizer] A0_S6 / Connection_Prefab_FromBossFight (SceneConnectionPoint) -> A4_S5_DaoTrapHouse_Final / A4_S5_BossRoom_To_A4_S6
    to BR
[Warning:ArchipelagoRandomizer] A0_S6 / Connection_Prefab_Exit (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A4_S6_To_A4_S1
    to OW
[Warning:ArchipelagoRandomizer] A0_S6 / Connection_Prefab_BackTo_A4_S3 (SceneConnectionPoint) -> A4_S3_ControlRoom_Final / A4_S6_To_A4_S3
[Warning:ArchipelagoRandomizer] A0_S6 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
 */
}
