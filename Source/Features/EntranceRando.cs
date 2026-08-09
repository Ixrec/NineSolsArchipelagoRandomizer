using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ArchipelagoRandomizer.SkillTree;

namespace ArchipelagoRandomizer.Features;

/*
 * Identifying "entrances" is not nearly as simple as we'd like it to be, but here's what we need to know:
 * 
 * SceneConnectionPoint is the main type that represents loading transitions between areas,
 * including the ones we want to randomize. Note that many SceneConnectionPoints are for things we
 * don't want to randomize (e.g. cutscene transitions), and many entrances we do want to randomize
 * have multiple SceneConnectionPoint for various reasons.
 * 
 * SingletonBehaviour<GameCore>.Instance.gameLevel.name is the "level" name
 * SceneConnectionPoint.scene.SceneName is the "scene" name
 * SceneConnectionPoint.connectionID is the "connection id"
 *      I'll often call it a "connection name" since it's not a unique, and it's usually human-readable
 *
 * Although I could easily be missing something, it *seems* like we simply have no access to
 * "level" names for scenes other than the currently loaded one.
 * We also do not appear to have direct access to the current scene name.
 * In practice it feels like "level" is the abstraction an active, loaded area,
 * while "scene" is the abstraction for an unloaded area.
 * So in all the relevant patch methods below, I only know how to access current level, "target" scene, and connection name.
 * 
 * Finally, and most importantly, we *do* need ALL THREE of (current level name, target scene name, connection name)
 * to uniquely identify a single transition, because:
 * 1) Many A->B transitions have a corresponding B->A transition that uses *the same connection name*.
 * Obviously we have to be able to tell the A->B and B->A apart to change their targets correctly, so we need more than connection name.
 * 2) There are many, many connections with name "AG_Tutorial_Lear_S2_識破JumpKick" *and* target scene "A2_S6_LogisticCenter_Final".
 * Almost all of these appear to be dead, unused connections, except the Heng flashback that plays the first time you go from OW to IW.
 * 
 * Technically, even (level, scene, connection name) is not enough, but the only duplicates I've found with all three
 * are literally redundant duplicates where only one is used in practice, so we don't need to distinguish them.
 * Example: FU has two SCPs with name Connection_BoxChangeScene, level A6_S1, scene A1_S3_InnerHumanDisposal_Final, and connection A6_S1_To_A1_S3.
 * 
 * 
 */

[HarmonyPatch]
internal class EntranceRando {
    // we want to use these as dict keys/values, so we need value equality, hence structs instead of classes
    public struct ExitIds {
        public string levelName;
        public string sceneName;
        public string connectionName;
        public ExitIds(string l, string s, string c) {
            levelName = l;
            sceneName = s;
            connectionName = c;
        }
    }
    public struct EntranceIds {
        public string sceneName;
        public string connectionName;
        public EntranceIds(string s, string c) {
            sceneName = s;
            connectionName = c;
        }
    }

    /*
     * Terminology:
     * - A "portal" is a single in-game place in one area that, when Yi walks into it, triggers a transition to another portal.
     * Portal names are exactly the same in vanilla and all entrance rando seeds.
     * - A "(two-way) connection" is a pair of linked portals. The vanilla game has a hardcoded set of connections,
     * and "entrance rando" is all about randomly choosing a different set of connections.
     * - An "entrance" (especially in Archipelago) is a *directed* connection from one portal to another portal.
     * Confusingly, these two portals are often called the "entrance" and "exit" of that entrance.
     * If not for Archipelago's precedent, I would call this a "(directed) connection".
     * 
     * For now, we assume every A->B transition has a corresponding B->A transition;
     * this notion of "connection" doesn't make sense without that assumption. 
     * This is sometimes known as "coupled" ER. If we decide we want "uncoupled" ER too, we'll rethink this.
     * 
     */
    public enum Portal {
        GOSE_UPPER_PORTAL,
        GOSE_MIDDLE_PORTAL,
        GOSE_LOWER_PORTAL,

        GOSW_UPPER_RIGHT_PORTAL,
        GOSW_MIDDLE_RIGHT_PORTAL,
        GOSW_LOWER_RIGHT_ELEVATOR,
        GOSW_UPPER_LEFT_PORTAL,
        GOSW_LOWER_LEFT_PORTAL,
        GOSW_BOSS_PORTAL,
        ASP_PORTAL,

        GOSY_UPPER_RIGHT_PORTAL,
        GOSY_LOWER_RIGHT_PORTAL,
        GOSY_UPPER_ELEVATOR,
        GOSY_LOWER_ELEVATOR_SHAFT,
        GOSY_LEFT_PORTAL,
    }

    private static Dictionary<Portal, Portal> EntranceMap = new Dictionary<Portal, Portal> {
        { Portal.GOSE_UPPER_PORTAL, Portal.GOSE_LOWER_PORTAL },
        { Portal.GOSE_MIDDLE_PORTAL, Portal.GOSE_LOWER_PORTAL },
        { Portal.GOSE_LOWER_PORTAL, Portal.GOSE_UPPER_PORTAL },
    };

    // here we need duplicate values because there are often multiple vanilla connections for the same transition,
    // depending on e.g. whether a certain cutscene has happened already
    private static readonly Dictionary<ExitIds, Portal> VanillaExits = new Dictionary<ExitIds, Portal> {
        { new ExitIds("A10_S3", "A10_SG6_SisterMemory", "A10_S3_To_A10_SG6"), Portal.GOSE_UPPER_PORTAL }, // first time Heng flashback
        { new ExitIds("A10_S3", "A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryB"), Portal.GOSE_UPPER_PORTAL }, // after the Heng flashback
        { new ExitIds("A10_S3", "A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryA"), Portal.GOSE_MIDDLE_PORTAL },
        { new ExitIds("A10_S3", "A10_S1_TombEntrance_remake", "A10_S1->A10_S3"), Portal.GOSE_LOWER_PORTAL },

        { new ExitIds("A10_S4", "A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryB"), Portal.GOSW_UPPER_RIGHT_PORTAL },
        { new ExitIds("A10_S4", "A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryA"), Portal.GOSW_MIDDLE_RIGHT_PORTAL },
        { new ExitIds("A10_S4", "A10_S1_TombEntrance_remake", "A10_S4_To_A10_S1_Elevator"), Portal.GOSW_LOWER_RIGHT_ELEVATOR },
        { new ExitIds("A10_S4", "A9_S1_Remake_4wei", "A10_S4_To_A9_S1"), Portal.GOSW_UPPER_LEFT_PORTAL },
        { new ExitIds("A10_S4", "A9_S1_Remake_4wei", "A9_S1_To_A10_S4_Elevator"), Portal.GOSW_LOWER_LEFT_PORTAL },
        { new ExitIds("A10_S4", "A10_S5_Boss_Jee", "A10_S4_To_BossFight_Jee"), Portal.GOSW_BOSS_PORTAL },
        { new ExitIds("A10_S4", "A10_S4_HistoryTomb_Left", "A10_S4_To_BossFight_Jee"), Portal.ASP_PORTAL },

        { new ExitIds("A10_S1", "A10_S3_HistoryTomb_Right", "A10_S1->A10_S3"), Portal.GOSY_UPPER_RIGHT_PORTAL },
        { new ExitIds("A10_S1", "A3_S5_BossGouMang_Final", "A3_S5_To_A10_S1"), Portal.GOSY_LOWER_RIGHT_PORTAL },
        { new ExitIds("A10_S1", "A10_S4_HistoryTomb_Left", "A10_S4_To_A10_S1_Elevator"), Portal.GOSY_UPPER_ELEVATOR },
        { new ExitIds("A10_S1", "A3_S2_GreenHouse_Final", "A10_S1_To_A3_S2"), Portal.GOSY_LOWER_ELEVATOR_SHAFT },
        { new ExitIds("A10_S1", "A3_S1_GardenRuins_Final", "A3_S1_to_A10_S1"), Portal.GOSY_LEFT_PORTAL },

        { new ExitIds("", "", ""), Portal.GOSE_LOWER_PORTAL },
    };
    // but this mapping needs to be unique per entrance, so let's store it in the other direction to enforce that
    private static readonly Dictionary<Portal, EntranceIds> VanillaEntrances = new Dictionary<Portal, EntranceIds> {
        { Portal.GOSE_UPPER_PORTAL, new EntranceIds("A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryB") },
        { Portal.GOSE_MIDDLE_PORTAL, new EntranceIds("A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryA") },
        { Portal.GOSE_LOWER_PORTAL, new EntranceIds("A10_S3_HistoryTomb_Right", "A10_S1->A10_S3") },

        { Portal.GOSW_UPPER_RIGHT_PORTAL, new EntranceIds("A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryB") },
        { Portal.GOSW_MIDDLE_RIGHT_PORTAL, new EntranceIds("A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryA") },
        { Portal.GOSW_LOWER_RIGHT_ELEVATOR, new EntranceIds("A10_S4_HistoryTomb_Left", "") },
        { Portal.GOSW_UPPER_LEFT_PORTAL, new EntranceIds("A10_S4_HistoryTomb_Left", "") },
        { Portal.GOSW_LOWER_LEFT_PORTAL, new EntranceIds("A10_S4_HistoryTomb_Left", "") },
        { Portal.GOSW_BOSS_PORTAL, new EntranceIds("A10_S4_HistoryTomb_Left", "A10_S4_To_BossFight_Jee") },
        { Portal.ASP_PORTAL, new EntranceIds("A10_S5_Boss_Jee", "A10_S4_To_BossFight_Jee") },

        { Portal.GOSY_UPPER_RIGHT_PORTAL, new EntranceIds("A10_S1_TombEntrance_remake", "A10_S1->A10_S3") },
        { Portal.GOSY_LOWER_RIGHT_PORTAL, new EntranceIds("A10_S1_TombEntrance_remake", "") },
        { Portal.GOSY_UPPER_ELEVATOR, new EntranceIds("A10_S1_TombEntrance_remake", "A10_S4_To_A10_S1_Elevator") },
        { Portal.GOSY_LOWER_ELEVATOR_SHAFT, new EntranceIds("A10_S1_TombEntrance_remake", "") },
        { Portal.GOSY_LEFT_PORTAL, new EntranceIds("A10_S1_TombEntrance_remake", "") },

        { Portal.GOSE_LOWER_PORTAL, new EntranceIds("", "") },
    };

    /*
     */

    // populated dynamically by the SCP Awake() patch
    private static Dictionary<ExitIds, Portal> HalfEditedExits = new Dictionary<ExitIds, Portal> {};

    [HarmonyPrefix, HarmonyPatch(typeof(SceneConnectionPoint), "Awake")]
    static void SceneConnectionPoint_Awake(SceneConnectionPoint __instance) {
        var level = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        //Log.Warning($"SceneConnectionPoint_Awake {level} / {__instance} -> {__instance.scene.SceneName} / {__instance.connectionID}");

        var ids = new ExitIds(level, __instance.scene.SceneName, __instance.connectionID);
        if (!VanillaExits.TryGetValue(ids, out var sourceEntrance))
            return;
        if (!EntranceMap.TryGetValue(sourceEntrance, out var targetEntrance))
            return;
        if (!VanillaEntrances.TryGetValue(targetEntrance, out var targetEntranceIds))
            return;

        Log.Warning($"editing {sourceEntrance} to connect to {targetEntrance} part 1: changing connectionId from {__instance.connectionID} to {targetEntranceIds.connectionName}");
        __instance.connectionID = targetEntranceIds.connectionName;

        var halfEditedIds = new ExitIds(ids.levelName, ids.sceneName, targetEntranceIds.connectionName);
        HalfEditedExits[halfEditedIds] = sourceEntrance;
        Log.Warning($"editing {sourceEntrance} to connect to {targetEntrance} part 1.5: mapped {halfEditedIds} to {sourceEntrance}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameCore), "ChangeScene", [typeof(SceneConnectionPoint.ChangeSceneData), typeof(bool), typeof(bool), typeof(float)])]
    static void GameCore_ChangeScene(GameCore __instance, ref SceneConnectionPoint.ChangeSceneData changeSceneData) {
        var level = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        Log.Warning($"GameCore_ChangeScene {level} / {__instance} -> {changeSceneData.sceneName} / {changeSceneData.connectionID}");

        var ids = new ExitIds(level, changeSceneData.sceneName, changeSceneData.connectionID);
        // Use HalfEditedExits instead of VanillaExits, because the Awake() patch should have already edited the connectionId
        if (!HalfEditedExits.TryGetValue(ids, out var sourceEntrance))
            return;
        if (!EntranceMap.TryGetValue(sourceEntrance, out var targetEntrance))
            return;
        if (!VanillaEntrances.TryGetValue(targetEntrance, out var targetEntranceIds))
            return;

        Log.Warning($"editing {sourceEntrance} to connect to {targetEntrance} part 2: changing sceneName from {changeSceneData.sceneName} to {targetEntranceIds.sceneName}");
        changeSceneData.sceneName = targetEntranceIds.sceneName;
    }
}

/* original ER notes, in case we ever want to add more entrances and these can save time:

level A10_S3 / scene A10_S3_HistoryTomb_Right / GoSE:
[Warning:ArchipelagoRandomizer] A10_S3 / ToA10_S4_EntryB (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A10_S3_To_A10_S4_EntryB
    to GoSW up right normally
[Warning:ArchipelagoRandomizer] A10_S3 / A10_S3->A10_S1 (SceneConnectionPoint) -> A10_S1_TombEntrance_remake / A10_S1->A10_S3
    lower exit to GoSY up right
[Warning:ArchipelagoRandomizer] A10_S3 / ToA10_SG6 (SceneConnectionPoint) -> A10_SG6_SisterMemory / A10_S3_To_A10_SG6
    to special subset of GoSW up right for the Heng flashback scene
        how tf does this affect ER???
[Warning:ArchipelagoRandomizer] A10_S3 / ToA10_S4 (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A10_S3_To_A10_S4_EntryA
    middle exit to GoSW lower right
[Warning:ArchipelagoRandomizer] A10_S3 / Connection_Door (SceneConnectionPoint) -> A10_SG2_Cave2 / A10_S3_To_A10_SG2
    left room to Guiguzi's tomb
[Warning:ArchipelagoRandomizer] A10_S3 / Connection_Door (SceneConnectionPoint) -> A10_SG1_Cave1 / A10_S3_To_A10_SG1
    right room to Yin Jifu's tomb

level A10_SG2 / scene A10_SG2_Cave2 / Guiguzi's Tomb:
[Warning:ArchipelagoRandomizer] A10_SG2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A10_SG2 / ToA10_S4 (SceneConnectionPoint) -> A10_S3_HistoryTomb_Right / A10_S3_To_A10_SG2
    exit to GoSE

level A10_SG1 / scene A10_SG1_Cave1 / Yin Jifu's tomb:
A10_SG1_Cave1 / A10_S3_To_A10_SG1
[Warning:ArchipelagoRandomizer] A10_SG1_Cave1 / ToA10_S4 (SceneConnectionPoint) -> A10_S3_HistoryTomb_Right / A10_S3_To_A10_SG1
    exit to GoSE
[Warning:ArchipelagoRandomizer] A10_SG1_Cave1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

---

scene A10_SG6_SisterMemory is a variation of GoSW for the Heng flashback

level A10_S4 / scene A10_S4_HistoryTomb_Left / GoSW:
[Warning:ArchipelagoRandomizer] A10_S4 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A10_S4 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A10_S4 / ToA10_S3 (SceneConnectionPoint) -> A10_S3_HistoryTomb_Right / A10_S3_To_A10_S4_EntryA
    to GoSE mid
[Warning:ArchipelagoRandomizer] A10_S4 / ToA10_S3_EntryB (SceneConnectionPoint) -> A10_S3_HistoryTomb_Right / A10_S3_To_A10_S4_EntryB
    to GoSE upper
[Warning:ArchipelagoRandomizer] A10_S4 / A10_S4_To_A9_S1 (SceneConnectionPoint) -> A9_S1_Remake_4wei / A10_S4_To_A9_S1
    up left exit to EDP up right
[Warning:ArchipelagoRandomizer] A10_S4 / FromA10_SG6 (SceneConnectionPoint) -> A10_SG6_SisterMemory / A10_SG6_To_A10_S4
    Heng flashback?
[Warning:ArchipelagoRandomizer] A10_S4 / Connection_Prefab (SceneConnectionPoint) -> A9_S1_Remake_4wei / A9_S1_To_A10_S4_Elevator
    bot left exit to EDP bot right
[Warning:ArchipelagoRandomizer] A10_S4 / Connection_Door (SceneConnectionPoint) -> A10_SG5_LearZone / A10_S4_To_A10_SG5
    into Lear's tomb?
[Warning:ArchipelagoRandomizer] A10_S4 / Connection_Door (SceneConnectionPoint) -> A10_SG4_Cave4 / A10_S4_To_A10_SG4
    into Luyan's tomb
[Warning:ArchipelagoRandomizer] A10_S4 / Connection_Door (SceneConnectionPoint) -> A10_S5_Boss_Jee / A10_S4_To_BossFight_Jee
    into Ancient Stone Pillar
[Warning:ArchipelagoRandomizer] A10_S4 / Connection_Prefab (SceneConnectionPoint) -> A10_S1_TombEntrance_remake / A10_S4_To_A10_S1_Elevator
    elevator down to GoSY

level A10_SG4 / scene A10_SG4_Cave4 / Luyan's tomb
[Warning:ArchipelagoRandomizer] A10_SG4 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A10_SG4 / ToA10_S4 (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A10_S4_To_A10_SG4
    exit to GoSW

level A10_SG4 / scene A10_S5_Boss_Jee / Ancient Stone Pillar aka Ji's arena
[Warning:ArchipelagoRandomizer] A10S5 / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_Jee / A10_S5->VR_Memory_Jee
[Warning:ArchipelagoRandomizer] A10S5 / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_Jee / VR_Memory_Jee->A10_S5
[Warning:ArchipelagoRandomizer] A10S5 / A10_S4_To_A9_S1 (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A10_S4_To_BossFight_Jee
    exit to GoSW
[Warning:ArchipelagoRandomizer] A10S5 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

level ??? / scene VR_Memory_Jee

level ??? / scene A10_SG5_LearZone / Lear's Tomb

---

level A10_S1 / scene A10_S1_TombEntrance_remake / GoSY:
[Warning:ArchipelagoRandomizer] A10_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A10_S1 / A3_S1_to_A10_S1 (SceneConnectionPoint) -> A3_S1_GardenRuins_Final / A3_S1_to_A10_S1
    to LYR
[Warning:ArchipelagoRandomizer] A10_S1 / A3_S5_to_A10_S1 (SceneConnectionPoint) -> A3_S5_BossGouMang_Final / A3_S5_To_A10_S1
    to Agrarian Hall
[Warning:ArchipelagoRandomizer] A10_S1 / A10_S1_to_A3_S2_Jump (SceneConnectionPoint) -> A3_S2_GreenHouse_Final / A10_S1_To_A3_S2
    to Greenhouse
[Warning:ArchipelagoRandomizer] A10_S1 / A10_S1_to_A3_S2_Animation (SceneConnectionPoint) -> A3_S2_GreenHouse_Final / A10_S1_To_A3_S2
    probably the first trip to Greenhouse
[Warning:ArchipelagoRandomizer] A10_S1 / A10_S1->A10_S3 (SceneConnectionPoint) -> A10_S3_HistoryTomb_Right / A10_S1->A10_S3
    up right exit to GoSE lower
[Warning:ArchipelagoRandomizer] A10_S1 / Connection_Prefab (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A10_S4_To_A10_S1_Elevator
    elevator up to GoSW

---

level A3_S1 / scene A3_S1_GardenRuins_Final / LYR
[Warning:ArchipelagoRandomizer] A3_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab_To_AG_S1 (SceneConnectionPoint) -> AG_S1_SenateHall / AG_S1_To_A3_S1
    to CH
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab (1) (SceneConnectionPoint) -> A3_S5_BossGouMang / A3_S5_BackToGround
    maybe another unused connection???
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab_To_A10_S1 (SceneConnectionPoint) -> A10_S1_TombEntrance_remake / A3_S1_to_A10_S1
    to GoSY
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab (SceneConnectionPoint) -> A3_S7_DragonWay_Final / A3_S1_To_A3_S7
    to YC
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab (SceneConnectionPoint) -> A3_SG1 / A3_S1_To_A3_SG1
    shield statues room
--- I guess this is loaded in two halves?
[Warning:ArchipelagoRandomizer] A3_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab (SceneConnectionPoint) -> A3_SG4 / A3_S1_To_A3_SG4
    probably tower treasure room
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab (SceneConnectionPoint) -> A3_SG2 / A3_S1_To_A3_SG2
    nymph puzzle room
[Warning:ArchipelagoRandomizer] A3_S1 / Connection_Prefab (SceneConnectionPoint) -> A9_S4 / A3_S1->A9_S4
    to ST

level A3_SG1 / scene A3_SG1 / shield statues room

level A3_SG1 / scene A3_SG2 / nymph puzzle room
    same level id???

---

level A3_S2 / scene A3_S2_GreenHouse_Final / Greenhouse
[Warning:ArchipelagoRandomizer] A3_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A3_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A3_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A3_S2 / Connection_From_A10_S1 (SceneConnectionPoint) -> A10_S1_TombEntrance_remake / A10_S1_To_A3_S2
    to GoSY
[Warning:ArchipelagoRandomizer] A3_S2 / Connection_To_A3_S3 (SceneConnectionPoint) -> A3_S3_OxygenChamber_Final / A3_S2_To_A3_S3
    to W&OS
[Warning:ArchipelagoRandomizer] A3_S2 / Connection_To_Lear (SceneConnectionPoint) -> AG_Tutorial_Lear_S3_重擊ChargeAttack / AG_Tutorial_Lear_S3_重擊ChargeAttack

---

level A3_S5_BossGouMang_GameLevel / scene A3_S5_BossGouMang_Final / Agrarian Hall
[Warning:ArchipelagoRandomizer] A3_S5_BossGouMang_GameLevel / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_Goumang / A3_S5_To_VR_GouMang
[Warning:ArchipelagoRandomizer] A3_S5_BossGouMang_GameLevel / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_Goumang / VR_GouMang_To_A3_S5
[Warning:ArchipelagoRandomizer] A3_S5_BossGouMang_GameLevel / Connection_Prefab＿A3_S5_To_A10_S1 (SceneConnectionPoint) -> A10_S1_TombEntrance_remake / A3_S5_To_A10_S1
    to GoSY?
[Warning:ArchipelagoRandomizer] A3_S5_BossGouMang_GameLevel / Connection_Prefab (SceneConnectionPoint) -> A3_S3_OxygenChamber_Final / A3_S3_To_A3_S5
    to W&OS?
[Warning:ArchipelagoRandomizer] A3_S5_BossGouMang_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

level ??? / scene VR_Memory_Goumang

---

level A3_S3 / scene A3_S3_OxygenChamber_Final / W&OS
[Warning:ArchipelagoRandomizer] A3_S3 / A3_S2 (SceneConnectionPoint) -> A3_S2_GreenHouse_Final / A3_S2_To_A3_S3
[Warning:ArchipelagoRandomizer] A3_S3 / Connection_Prefab (SceneConnectionPoint) -> A3_S5_BossGouMang_Final / A3_S3_To_A3_S5
[Warning:ArchipelagoRandomizer] A3_S3 / A3_S7 (SceneConnectionPoint) -> A3_S7_DragonWay_Final / A3_S3_To_A3_S7
no mysteries here

---

level A3_S7 / scene A3_S7_DragonWay_Final / YC
[Warning:ArchipelagoRandomizer] A3_S7 / ToA3_S3 (SceneConnectionPoint) -> A3_S3_OxygenChamber_Final / A3_S3_To_A3_S7
    to W&OS
[Warning:ArchipelagoRandomizer] A3_S7 / ToA3_S1 (SceneConnectionPoint) -> A3_S1_GardenRuins_Final / A3_S1_To_A3_S7
    to LYR
[Warning:ArchipelagoRandomizer] A3_S7 / ToA11_S1 (SceneConnectionPoint) -> A11_S1_Hospital_remake / A3_S7_To_A11_S1
    to TRC

---

level A9_S4 / scene A9_S4 / ST
[Warning:ArchipelagoRandomizer] A9_S4 / A9_S3_to_A9_S2 (SceneConnectionPoint) -> A9_S1_Remake_4wei / A9_S1_to_A9_S4
    to EDP
[Warning:ArchipelagoRandomizer] A9_S4 / Connection_Prefab (SceneConnectionPoint) -> A3_S1_GardenRuins_Final / A3_S1->A9_S4
    to LYR

---

level A9_S1 / scene A9_S1_Remake_4wei / EDP
[Warning:ArchipelagoRandomizer] A9_S1 / Connection_Prefab (SceneConnectionPoint) -> A9_S2_Remake_4wei / A9_S1_To_A9_S2
    to EDLA
[Warning:ArchipelagoRandomizer] A9_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A9_S1 / A10_S4_To_A9_S1 (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A10_S4_To_A9_S1
    up right to GoSW up left
[Warning:ArchipelagoRandomizer] A9_S1 / A9_S3_to_A9_S2 (SceneConnectionPoint) -> A9_S4 / A9_S1_to_A9_S4
    to ST
[Warning:ArchipelagoRandomizer] A9_S1 / Connection_Prefab (SceneConnectionPoint) -> A10_S4_HistoryTomb_Left / A9_S1_To_A10_S4_Elevator
    lower right to GoSW lower left
[Warning:ArchipelagoRandomizer] A10_S3 / A10_S3->A10_S1 (SceneConnectionPoint) -> A10_S1_TombEntrance_remake / A10_S1->A10_S3
    lower exit to GoSY up right

---

level A9_S2 / scene A9_S2_Remake_4wei / EDLA
[Warning:ArchipelagoRandomizer] A9_S2 / Connection_Prefab (SceneConnectionPoint) -> A9_S1_Remake_4wei / A9_S1_To_A9_S2
    to EDP
[Warning:ArchipelagoRandomizer] A9_S2 / Connection_Prefab (SceneConnectionPoint) -> A9_SG1 / A9_S2_To_A9_SG1
[Warning:ArchipelagoRandomizer] A9_S2 / A9_S2_to_A9_S3_Memory (SceneConnectionPoint) -> A9_S3 / A9_S2_to_A9_S3_Memory
    to EDS with flashback
[Warning:ArchipelagoRandomizer] A9_S2 / A9_S2_to_A9_S3 (SceneConnectionPoint) -> A9_S3 / A9_S2_to_A9_S3
    to EDS
[Warning:ArchipelagoRandomizer] A9_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A9_S2 / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_伏羲 / A9_S2->VR
[Warning:ArchipelagoRandomizer] A9_S2 / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_伏羲 / VR->A9_S2

level ??? / scene VR_Memory_伏羲

---

level A9_S3 / scene A9_S3 / EDS
[Warning:ArchipelagoRandomizer] A9_S3 / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_Kuafu / A2_S5_SleepPod_To_AG_S4
[Warning:ArchipelagoRandomizer] A9_S3 / Connection_BackFromSleeppod (SceneConnectionPoint) -> AG_S4_KuaFuMemory / AG_S4_SleepPod_To_A2_S5
[Warning:ArchipelagoRandomizer] A9_S3 / A9_S2_to_A9_S3_Memory (SceneConnectionPoint) -> A9_S2_Remake_4wei / A9_S2_to_A9_S3_Memory
    to EDLA, unused(???) since the memory is on this side in EDS
[Warning:ArchipelagoRandomizer] A9_S3 / A9_S2_to_A9_S3 (SceneConnectionPoint) -> A9_S2_Remake_4wei / A9_S2_to_A9_S3
    to EDLA
[Warning:ArchipelagoRandomizer] A9_S3 / Connection_Door (SceneConnectionPoint) -> A9_S5_風氏 / A9_S3->A9_S5_風氏
    to Nobility Hall
[Warning:ArchipelagoRandomizer] A9_S3 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

level P2_R22_Savepoint_GameLevel / scene A9_S5_風氏 / Nobility Hall
[Warning:ArchipelagoRandomizer] P2_R22_Savepoint_GameLevel / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_伏羲&女媧 / A9_S5->VR_Memory_伏羲&女媧
[Warning:ArchipelagoRandomizer] P2_R22_Savepoint_GameLevel / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_伏羲&女媧 / VR_Memory_伏羲&女媧->A9_S5
[Warning:ArchipelagoRandomizer] P2_R22_Savepoint_GameLevel / Connection_Door (SceneConnectionPoint) -> A9_S3 / A9_S3->A9_S5_風氏
    back out to EDS
[Warning:ArchipelagoRandomizer] P2_R22_Savepoint_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

level ??? / scene VR_Memory_伏羲&女媧

---

level A11_S1 / scene A11_S1_Hospital_remake / TRC
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_To_A3S7 (SceneConnectionPoint) -> A3_S7_DragonWay_Final / A3_S7_To_A11_S1
    to YC
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Prefab Enter (SceneConnectionPoint) -> A11_S2_Laboratory_remake / A11_S2->A11_S1
    to TRI probably, likely won't shuffle
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Door (SceneConnectionPoint) -> A11_SG1_ShinTenRoom / A11_S1_To_A11_SG1
[Warning:ArchipelagoRandomizer] A11_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / A11_S1_To_A2_S6
    to CTH
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_Prefab Leave (SceneConnectionPoint) -> A11_S2_Laboratory_remake / A11_S1->A11_S2
    to TRI probably, likely won't shuffle
[Warning:ArchipelagoRandomizer] A11_S1 / Connection_CrateChange_Enter (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / A2_S6_To_A11_S1

---

level A2_S6 / scene A2_S6_LogisticCenter_Final / CTH
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Prefab_To_AG_S1_Drop (SceneConnectionPoint) -> AG_S1_SenateHall / AG_S1_To_A2_S6_2nd
    to CH, vents
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Prefab_To_A0_S10 (SceneConnectionPoint) -> A0_S10_SpaceshipYard / A0_S10_To_A2_S6
    to GD
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Prefab_TutorialBack (SceneConnectionPoint) -> AG_Tutorial_Lear_S2_識 破JumpKick / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Prefab_To_AG_S1 (SceneConnectionPoint) -> AG_S1_SenateHall / AG_S1_To_A2_S6
    to CH, top of big elevator
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Prefab_To_A1_S2 (SceneConnectionPoint) -> A1_S2_ConnectionToElevator_Final / A1_S2_RightLockCorridar
    to AFE
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_BoxChangeScene (SceneConnectionPoint) -> A11_S1_Hospital_remake / A2_S6_To_A11_S1
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Door (SceneConnectionPoint) -> A2_SG5_LaserRoom / A2_S6_To_A2_SG5
    to laser puzzle room
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_Prefab (SceneConnectionPoint) -> A2_S2_ReactorRight_Final / A2_S6_A2_S2
    to PRE
[Warning:ArchipelagoRandomizer] A2_S6 / Connection_CrateChange_Enter (SceneConnectionPoint) -> A11_S1_Hospital_remake / A11_S1_To_A2_S6
    to TRC

level A4_SG2 / scene A2_SG5_LaserRoom / laser puzzle room

---

level AG_S1 / scene AG_S1_SenateHall / CH
[Warning:ArchipelagoRandomizer] AG_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_S2_YiBase / AG_S1_To_AG_S2
    into FSP
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab_To_A7 (SceneConnectionPoint) -> A7_S1_BrainRoom_Remake / A7_To_AG_S1
    to CC
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab_To_A3_S1_PlaceHolder (SceneConnectionPoint) -> A3_S1_GardenRuins_Final / AG_S1_To_A3_S1
    to LYR
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab_To_A2_S6_PlaceHolder (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_S1_To_A2_S6
    to CTH, via nymph door, to big elevator
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab_To_A2_S6_2nd (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_S1_To_A2_S6_2nd
    to CTH, via vents
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Door (SceneConnectionPoint) -> AG_SG1 / AG_S1_To_AG_SG1
    into double axe room
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_ST_Hub / AG_S1_To_AG_STHub
[Warning:ArchipelagoRandomizer] AG_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_ST_Hub / AG_S1_To_AG_STHub
    control hub???

---

level AG_S2 / scene AG_S2_YiBase / FSP
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / Connection_From_A6_S1 (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A6_S1_To_AG_S2
[Warning:ArchipelagoRandomizer] AG_S2 / Connection__To_AG_S1 (SceneConnectionPoint) -> AG_S1_SenateHall / AG_S1_To_AG_S2
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] AG_S2 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] AG_S2 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] AG_S2 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] AG_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
ignore since we probably aren't shuffling this entrance

---

level A2_S2 / scene A2_S2_ReactorRight_Final / PRE
[Warning:ArchipelagoRandomizer] A2_S2 / Connection_Prefab (SceneConnectionPoint) -> A2_SG4_MemoryGondola_Final / A2_S1_To_A2_SG4
    to Heng flashback before PRC
[Warning:ArchipelagoRandomizer] A2_S2 / Connection_Prefab (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / A2_S6_A2_S2
    to CTH
[Warning:ArchipelagoRandomizer] A2_S2 / Connection_Prefab (SceneConnectionPoint) -> A2_S1_ReactorMiddle_Final / A2_S1_To_A2_S2
    to PRC

level A2_SG4 / scene A2_SG4_MemoryGondola_Final

---

level A2_S1 / scene A2_S1_ReactorMiddle_Final / PRC
[Warning:ArchipelagoRandomizer] A2_S1 / Connection_EnterPyramid (SceneConnectionPoint) -> A2_S5_BossHorseman_Final / A2_S1_To_A2_S5
    to RP
[Warning:ArchipelagoRandomizer] A2_S1 / Connection_Door (SceneConnectionPoint) -> A2_SG1_ReactorControlRoom / A2_S1_To_SG
    to reactor control room
[Warning:ArchipelagoRandomizer] A2_S1 / Connection_Prefab (SceneConnectionPoint) -> A2_S2_ReactorRight_Final / A2_S1_To_A2_S2
    to PRE
[Warning:ArchipelagoRandomizer] A2_S1 / Connection_Prefab (SceneConnectionPoint) -> A2_S3_ReactorLeft_Final / A2_S1_To_A2_S3
    to PRW

level A2_SG1 / scene A2_SG1_ReactorControlRoom

---

level A2_S5_ BossHorseman_GameLevel / scene A2_S5_BossHorseman_Final / RP
[Warning:ArchipelagoRandomizer] A2_S5_ BossHorseman_GameLevel / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_Kuafu / A2_S5_SleepPod_To_AG_S4
[Warning:ArchipelagoRandomizer] A2_S5_ BossHorseman_GameLevel / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_Kuafu / AG_S4_SleepPod_To_A2_S5
[Warning:ArchipelagoRandomizer] A2_S5_ BossHorseman_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A2_S5_ BossHorseman_GameLevel / Connection_Prefab (1) (SceneConnectionPoint) -> A2_S1_ReactorMiddle_Final / A2_S1_To_A2_S5
    to PRC
[Warning:ArchipelagoRandomizer] A2_S5_ BossHorseman_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

---

level A2_S3 / scene A2_S3_ReactorLeft_Final / PRW
[Warning:ArchipelagoRandomizer] A2_S3 / Connection_Prefab (SceneConnectionPoint) -> A2_S1_ReactorMiddle_Final / A2_S1_To_A2_S3
    to PRC
[Warning:ArchipelagoRandomizer] A2_S3 / Connection_Prefab (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A1_S3_A2_S3
    to AFD
[Warning:ArchipelagoRandomizer] A2_S3 / Connection_Prefab (SceneConnectionPoint) -> A2_SG4_MemoryGondola_Final / A2_S1_To_A2_SG4
    to Heng flashback before PRC

---

level A1_S2_GameLevel / scene A1_S2_ConnectionToElevator_Final / AFE
[Warning:ArchipelagoRandomizer] A1_S2_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A1_S2_GameLevel / Connection_Prefab (SceneConnectionPoint) -> A1_S1_HumanDisposal_Final / A1_S1_To_A1_S2
    to AFM
[Warning:ArchipelagoRandomizer] A1_S2_GameLevel / Connection_Prefab_To_A2_S6 (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / A1_S2_RightLockCorridar
    to CTH
[Warning:ArchipelagoRandomizer] A1_S2_GameLevel / Connection_Prefab_To_A1_S3 (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A1_S3_A1_S2
    to AFD

---

level A1_S3_GameLevel / scene A1_S3_InnerHumanDisposal_Final / AFD
[Warning:ArchipelagoRandomizer] A1_S3_GameLevel / Connection_Prefab_To_A1_S2 (SceneConnectionPoint) -> A1_S2_ConnectionToElevator_Final / A1_S3_A1_S2
    to AFE
[Warning:ArchipelagoRandomizer] A1_S3_GameLevel / Connection_BoxChangeScene (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A1_S3_To_A6_S1
    to FU
[Warning:ArchipelagoRandomizer] A1_S3_GameLevel / Connection_Prefab (SceneConnectionPoint) -> A2_S3_ReactorLeft_Final / A1_S3_A2_S3
    to PRW
[Warning:ArchipelagoRandomizer] A1_S3_GameLevel / Connection_CrateChange_Enter (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A6_S1_To_A1_S3
    unused? AM connection doesn't use this, no cutscene I know of

[Warning:ArchipelagoRandomizer] A1_S3_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    despite not existing??? this is the hide in boxes to FU connection

---

level A1_S1_GameLevel / scene A1_S1_HumanDisposal_Final / AFM
[Warning:ArchipelagoRandomizer] A1_S1_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A1_S1_GameLevel / Connection_Prefab (SceneConnectionPoint) -> A1_S2_ConnectionToElevator_Final / A1_S1_To_A1_S2
    to AFE
[Warning:ArchipelagoRandomizer] A1_S1_GameLevel / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S1_Parry / A1_S1_To_AG_Lear_S1
[Warning:ArchipelagoRandomizer] A1_S1_GameLevel / Connection_Prefab (1) (SceneConnectionPoint) -> A0_S3_altar / A0_S3_To_A1_S1

---

level GameLevel / scene A0_S10_SpaceshipYard / GD
[Warning:ArchipelagoRandomizer] GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] GameLevel / Connection_A0_S9 (SceneConnectionPoint) -> A0_S9_AltarReturned / A0_S9_To_A0_S10
    to PBV East
[Warning:ArchipelagoRandomizer] GameLevel / Connection From_AG_S2 (SceneConnectionPoint) -> AG_S2_YiBase / AG_S2_To_A0_S10
[Warning:ArchipelagoRandomizer] GameLevel / Connection_A2_S6 (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / A0_S10_To_A2_S6
    to CTH, behind Yanren arena
[Warning:ArchipelagoRandomizer] GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] GameLevel / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S2_HugeTaciChiParry / A1_S1_To_AG_LeeEar_S1
[Warning:ArchipelagoRandomizer] GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

---

level A7_S1 / scene A7_S1_BrainRoom_Remake / CC
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_A5S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A7_To_A5_S1
    to FGH
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_被蝴蝶推出來 (SceneConnectionPoint) -> VR_Memory_FuDie / VR_FuDie_To_A7_S1
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_To_BossButterflyFight (SceneConnectionPoint) -> A7_S5_Boss_ButterFly / A7_S1_BrainRoom->Butterfly_Boss_Fight
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_看完回憶回來 (SceneConnectionPoint) -> A7_S6_Memory_Butterfly_CutScene / A7_S6_To_A7_S1
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_AGS1 (SceneConnectionPoint) -> AG_S1_SenateHall / A7_To_AG_S1
    to CTH
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_To_A7_S2 (SceneConnectionPoint) -> A7_S2_SectionA / A7_S1_BrainRoom->A7_S2_SectionA
[Warning:ArchipelagoRandomizer] A7_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_BackFromSleeppod (SceneConnectionPoint) -> AG_S4_KuaFuMemory / AG_S4_SleepPod_To_A2_S5
[Warning:ArchipelagoRandomizer] A7_S1 / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> AG_S4_KuaFuMemory / A2_S5_SleepPod_To_AG_S4

---

level A5_S1 / scene A5_S1_CastleHub_remake / FGH
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Teleport (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A5_S1_To_A6_S1
    bottom left elevator to FU
[Warning:ArchipelagoRandomizer] A5_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Prefab_To_A6_S1 (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A5_S1_To_A6_S1_Hole
    fall through hole below catwalk to upper FU
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Prefab_From_A7_S1 (SceneConnectionPoint) -> A7_S1_BrainRoom_Remake / A7_To_A5_S1
    to CC
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Prefab_To_A4_S1 (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A5_S1_To_A4_S1
    to OW
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Teleport_姬And截全演出 (SceneConnectionPoint) -> A5_AC2_Jie&Jee / A5_S1_To_A5_AC2
    left elevator to FPA first time, plays the Jiequan & Ji cutscene after loading Shengwu Hall
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Prefab_To_A4_S1 (1) (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A6_S1_To_A5_S1_SideCave
    the one-way room from FU's upper right exit
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Teleport (SceneConnectionPoint) -> A5_S4_CastleMid_Remake_5wei / A5_S1_To_A5_S4_Right
    right elevator to FPA
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Teleport (SceneConnectionPoint) -> A5_S4_CastleMid_Remake_5wei / A5_S1_To_A5_S4_Left
    left elevator to FPA (all times, including after the first-time-only cutscene)
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Teleport (SceneConnectionPoint) -> T0_S1_ConnectionTest /
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Door (SceneConnectionPoint) -> A5_S4b_HerbRoom_Remake / A5_S1_to_A5_S4b
    to nymph puzzle room

level A5_S4b / scene A5_S4b_HerbRoom_Remake / FGH's nymph puzzle room

---

level A5_S4 / scene A5_S4_CastleMid_Remake_5wei / FPA
[Warning:ArchipelagoRandomizer] A5_S4 / Connection_Teleport (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A5_S4_Right
    lower right elevator to FGH
[Warning:ArchipelagoRandomizer] A5_S4 / Connection_Teleport (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A5_S4_Left
    lower left elevator to FGH
[Warning:ArchipelagoRandomizer] A5_S4 / Connection_Teleport_國師演出 (SceneConnectionPoint) -> A5_AC2_Jie&Jee / A5_AC2_To_A5_S4_Left
    unused? because the FGH transition is used instead for this cutscene?
[Warning:ArchipelagoRandomizer] A5_S4 / Connection_Door (SceneConnectionPoint) -> A5_S4d_PoisonRoom / A5_S4_to_A5_S4d
    into pharmacy
[Warning:ArchipelagoRandomizer] A5_S4 / Connection_Teleport (SceneConnectionPoint) -> A5_S5_JieChuanHall / A5_S4_To_A5_S5
    to Shengwu Hall

level A5_S4b / scene A5_S4d_PoisonRoom / FPA's pharmacy
[Warning:ArchipelagoRandomizer] A5_S4b / Connection_A5S4 (SceneConnectionPoint) -> A5_S4_CastleMid_Remake_5wei / A5_S4_to_A5_S4d

CONFIRMED level name is NOT UNIQUE

---

level A5_S5 / scene A5_S5_JieChuanHall / Shengwu Hall
[Warning:ArchipelagoRandomizer] A5_S5 / Connection_Teleport (SceneConnectionPoint) -> A5_S4_CastleMid_Remake_5wei / A5_S4_To_A5_S5
    to FPA
[Warning:ArchipelagoRandomizer] A5_S5 / Connection_EnterSleepPodMemory (SceneConnectionPoint) -> VR_Memory_JieChuan / A5_S5_To_VR_JieChuan
[Warning:ArchipelagoRandomizer] A5_S5 / Connection_BackFromSleeppod (SceneConnectionPoint) -> VR_Memory_JieChuan / VR_JieChuan_To_A5_S5
[Warning:ArchipelagoRandomizer] A5_S5 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A5_S5 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

Shengwu Hall preview for Jiequan & Ji cutscene
[Warning:ArchipelagoRandomizer] A5_S5 / Connection_From_A5S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A5_AC2
[Warning:ArchipelagoRandomizer] A5_S5 / Connection_To_A5S4 (SceneConnectionPoint) -> A5_S4_CastleMid_Remake_5wei / A5_AC2_To_A5_S4_Left
[Warning:ArchipelagoRandomizer] A5_S5 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

---

level A6_S1 / scene A6_S1_AbandonMine_Remake_4wei / FU
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Teleport (SceneConnectionPoint) -> A5_S3_UnderCastle_Remake_4wei / A5_S3_To_A6_S1
    elevator to FMR
[Warning:ArchipelagoRandomizer] A6_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    hide in boxes to AFD crystal caves (connection name is totally wrong?)
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S4_AirDash_空中閃避 / AG_Tutorial_Lear_AirDash
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S5_大反ChargedParry / AG_Tutorial_Lear_S4_大反ChargedParry
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Point (SceneConnectionPoint) -> AG_Tutorial_Lear_S2_識破JumpKick / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_LearTutorial (SceneConnectionPoint) -> AG_Tutorial_Lear_S4_AirDash_空中閃避 / AG_Tutorial_Lear_AirDash
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A6_S3 (SceneConnectionPoint) -> A6_S3_Tutorial_And_SecretBoss_Remake / A6_S1->A6_S3
    to AM
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A5_S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A6_S1_To_A5_S1_SideCave
    upper right exit back to FGH
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A5_S1_Hole (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A6_S1_Hole
    fly(?) back up to FGH
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_To_AG_S2 (SceneConnectionPoint) -> AG_S2_YiBase / A6_S1_To_AG_S2
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A4_S1 (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A6_S1_To_A4_S1
    left exit to OW
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S3_重擊ChargeAttack / AG_Tutorial_Lear_S3_重擊ChargeAttack
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S0_格擋複習 / A1_S1_To_AG_Lear_S0
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A6_S1_To_A1_S3
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A6_S1_To_A1_S3
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_Teleport (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A6_S1
    elevator from top left FU to bottom left FGH
[Warning:ArchipelagoRandomizer] A6_S1 / Connection_CrateChange_Enter (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A1_S3_To_A6_S1
    to AFD

FU<->AFD connection being AG_Tutorial_Lear_S2_識破JumpKick means that even scene + connection id is NOT UNIQUE, we NEED to use the current LEVEL name as well to disambiguate

---

level A6_S3 / scene A6_S3_Tutorial_And_SecretBoss_Remake / AM
[Warning:ArchipelagoRandomizer] A6_S3 / Connection_Prefab_A0_S7 (SceneConnectionPoint) -> A0_S7_CaveReturned / A6_S3_To_A0_S7
    to UC
[Warning:ArchipelagoRandomizer] A6_S3 / Connection_Prefab_A6_S1 (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A6_S1->A6_S3
    to FU

---

level A0_S7 / scene A0_S7_CaveReturned / UC
[Warning:ArchipelagoRandomizer] A0_S7 / Connection_To_A6_S3 (SceneConnectionPoint) -> A6_S3_Tutorial_And_SecretBoss_Remake / A6_S3_To_A0_S7
    to AM
[Warning:ArchipelagoRandomizer] A0_S7 / Connection_To_A0_S8 (SceneConnectionPoint) -> A0_S8_VillageReturned / A0_S7_To_A0_S8    
    to PBV

---

level GameLevel / scene A0_S8_VillageReturned / PBV West
[Warning:ArchipelagoRandomizer] GameLevel / Connection_To_A0_S9 (SceneConnectionPoint) -> A0_S9_AltarReturned / A0_S8_To_A0_S9
    to PBV East
[Warning:ArchipelagoRandomizer] GameLevel / Connection_To_A0_S7 (SceneConnectionPoint) -> A0_S7_CaveReturned / A0_S7_To_A0_S8
    to UC

---

level GameLevel / scene A0_S9_AltarReturned / PBV East
[Warning:ArchipelagoRandomizer] GameLevel / Connection_A0_S8 (SceneConnectionPoint) -> A0_S8_VillageReturned / A0_S8_To_A0_S9
    to PBV West
[Warning:ArchipelagoRandomizer] GameLevel / Connection_A0_S10 (SceneConnectionPoint) -> A0_S10_SpaceshipYard / A0_S9_To_A0_S10
    to GD

---

level A5_S3 / scene A5_S3_UnderCastle_Remake_4wei / FMR
[Warning:ArchipelagoRandomizer] A5_S3 / Connection_Teleport (SceneConnectionPoint) -> A5_S2_Jail_Remake_Final / A5_S2_To_A5_S3
    to Prison
[Warning:ArchipelagoRandomizer] A5_S3 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S5_ 大反ChargedParry / AG_Tutorial_Lear_S4_大反ChargedParry
[Warning:ArchipelagoRandomizer] A5_S3 / Connection_Teleport (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A5_S3_To_A6_S1
    elevator to FU

---
prison entry sequence:
[Warning:ArchipelagoRandomizer] A5_S1 / Connection_Prefab_A5_S1->A5_S2 (SceneConnectionPoint) -> A5_S2_Jail_Remake_Final / A5_S1_約戰
    apparently doesn't exist before the Jiequan 1 fight happens???

level A5_S2 / scene A5_S2_Jail_Remake_Final / Prison
[Warning:ArchipelagoRandomizer] A5_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A5_S2 / Connection_Prefab_約戰過來 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_約戰
[Warning:ArchipelagoRandomizer] A5_S2 / Connection_Teleport (SceneConnectionPoint) -> A5_S3_UnderCastle_Remake_4wei / A5_S2_To_A5_S3
    elevator to FMR
[Warning:ArchipelagoRandomizer] A5_S2 / Connection_Prefab_From_A5_S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A5_AC1
[Warning:ArchipelagoRandomizer] A5_S2 / Connection_Prefab_To_A5_S2 (SceneConnectionPoint) -> A5_S2_Jail_Remake / A5_AC1_To_A5_S2

---

level A4_S1 / scene A4_S1_NewBridgeToWarehouse_Final / OW
[Warning:ArchipelagoRandomizer] A4_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_Prefab_ToRight_A5_S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A4_S1
    to FGH
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_Prefab_To_A6_S1 (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A6_S1_To_A4_S1
    to FU
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_Prefab_ToLeft_A4_S6 (SceneConnectionPoint) -> A4_S6_DaoBase_Final / A4_S6_To_A4_S1
    to Yangu Hall
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A4_SG3_MemoryCrate New / A4_S1_To_A4_SG3
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A4_S2_RouteToControlRoom_Final / A4_S1_To_A4_S2
    to IW
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_Door (SceneConnectionPoint) -> A4_SG7_ZRoom_Arena / A4_S1_To_A4_SG7
[Warning:ArchipelagoRandomizer] A4_S1 / Connection_CrateChange_Enter (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A4_S2_To_A4_S1

    to IW first time Heng flashback:
[Warning:ArchipelagoRandomizer] A4_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick

[Warning:ArchipelagoRandomizer] A2_SG4 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A2_SG4 / EnterConnection_From_A4S1 (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A4_S1_To_A4_SG3
[Warning:ArchipelagoRandomizer] A2_SG4 / LeaveConnection_To_A4S2 (SceneConnectionPoint) -> A4_S2_RouteToControlRoom_Final / A4_S1_To_A4_S2

[Warning:ArchipelagoRandomizer] GameCore_ChangeScene A2_SG4 -> A4_S2_RouteToControlRoom_Final / A4_S1_To_A4_S2

---

level A4_S2 / scene A4_S2_RouteToControlRoom_Final / IW
[Warning:ArchipelagoRandomizer] A4_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S2 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S2 / Connection_Prefab Enter (SceneConnectionPoint) -> A4_S2_RouteToControlRoom_Final / A4_S3_To_A4_S2
[Warning:ArchipelagoRandomizer] A4_S2 / Connection_BoxChangeScene (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A4_S2_To_A4_S1
[Warning:ArchipelagoRandomizer] A4_S2 / Connection_Door (SceneConnectionPoint) -> A4_SG1 / A4_S2_To_A4_SG1
    to nymph puzzle room
[Warning:ArchipelagoRandomizer] A4_S2 / Connection_Prefab Leave (SceneConnectionPoint) -> A4_S3_ControlRoom_Final / A4_S2_To_A4_S3
    to BR
[Warning:ArchipelagoRandomizer] A4_S2 / Connection_CrateChange_Enter (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A4_S1_To_A4_S2
    to OW

level A4_SG1 / scene A4_SG1 / IW nymph puzzle room
[Warning:ArchipelagoRandomizer] A4_SG1 / Connection_Entry (SceneConnectionPoint) -> A4_S2_RouteToControlRoom_Final / A4_S2_To_A4_SG1

---

level A4_S3 / scene A4_S3_ControlRoom_Final / BR
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Door (SceneConnectionPoint) -> A1_S7_HumanDisposal_Elevator /
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Door_直接去A4 (SceneConnectionPoint) -> A4_S4_Container_Final / A4_S3_To_A4_S4_Entry
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Door_看道長狙擊演出 (SceneConnectionPoint) -> A4_SG5 / A4_S3_To_A4_SG5
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Door (SceneConnectionPoint) -> A1_S7_HumanDisposal_Elevator /
[Warning:ArchipelagoRandomizer] A4_S3 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S3 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Prefab_To_A4_S6 (SceneConnectionPoint) -> A4_S6_DaoBase_Final / A4_S6_To_A4_S3
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Prefab_Exit_From_A4_S4 (SceneConnectionPoint) -> A4_S4_Container_Final / A4_S3_To_A4_S4_Exit
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Prefab_To_BossFight (SceneConnectionPoint) -> A4_S5_DaoTrapHouse_Final / A4_S3_To_A4_S5_BossRoom
    to Yangu Hall
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Prefab_Exit_From_A4_SG6 (SceneConnectionPoint) -> A4_SG6_Fifth_Container / A4_S3_To_A4_SG6_Exit
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Prefab Enter (SceneConnectionPoint) -> A4_S3_ControlRoom_Final / A4_S2_To_A4_S3
    to IW
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Prefab Leave (SceneConnectionPoint) -> A4_S2_RouteToControlRoom_Final / A4_S3_To_A4_S2
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Back (SceneConnectionPoint) -> A4_SG4 / A4_SG4_To_A4_S3_ElementDoor
[Warning:ArchipelagoRandomizer] A4_S3 / Connection_Enter (SceneConnectionPoint) -> A4_SG4 / A4_S3_To_A4_SG4_ElementDoor

---

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

---

level is definitely NOT UNIQUE because we know "GameLevel" is the level string for PBV West, PBV East, and Galactic Dock
scene might be unique? I don't think I found any counterexamples in scenes we care about
connection id/name obviously isn't unique because nearly all two-way connections have the same id/name both ways
    it also isn't even "two-way unique" because there's a ton of unrelated connections with same names, admittedly most of them appear to be dead/unused
    especially the A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick duplicates, it's real once(?) but 99% of them are dead
unfortunately even scene + connection name is not unique on its own, most clearly because of the A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick duplicates
 */