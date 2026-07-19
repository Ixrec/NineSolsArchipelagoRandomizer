using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ArchipelagoRandomizer.SkillTree;
using static SceneConnectionPoint;

namespace ArchipelagoRandomizer.Features;

[HarmonyPatch]
internal class EntranceRando {
    // Direction is extremely important, so to be clear: All of the ABC_* entrances are ways to transition *into* the ABC area.
    // For example, GOSE_UPPER_ENTRANCE means entering GoS (East) from the upper (left) entrance,
    // and GOSE_LEAVING_LEFT_ROOM means "entering" GoS (East) by exiting the left side room (in vanilla Guiguzi's tomb).
    public enum Entrance {
        GOSE_UPPER_ENTRANCE,
        GOSE_MIDDLE_ENTRANCE,
        GOSE_LOWER_ENTRANCE,
        GOSE_LEAVING_LEFT_ROOM,
        GOSE_LEAVING_RIGHT_ROOM,

        YIN_JIFU_TOMB_ENTRANCE,
        GUIGUZI_TOMB_ENTRANCE,

        GOSW_UPPER_RIGHT_ENTRANCE,
    }

    public static readonly Dictionary<Entrance, (string, string)> SceneAndConnectionNames = new Dictionary<Entrance, (string, string)> {
        { Entrance.GOSE_UPPER_ENTRANCE, ("A10_S3_HistoryTomb_Right", "A10_S3_To_A10_SG6") },
        { Entrance.YIN_JIFU_TOMB_ENTRANCE, ("A10_SG1_Cave1", "A10_S3_To_A10_SG1") },
        { Entrance.GUIGUZI_TOMB_ENTRANCE, ("A10_SG1_Cave1", "A10_S3_To_A10_SG1") },

        { Entrance.GOSW_UPPER_RIGHT_ENTRANCE, ("A10_SG6_SisterMemory", "A10_S3_To_A10_SG6") },
    };

    /*
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
        to AM
    [Warning:ArchipelagoRandomizer] A1_S3_GameLevel / Connection_Prefab (SceneConnectionPoint) -> A2_S3_ReactorLeft_Final / A1_S3_A2_S3
        to PRW
    [Warning:ArchipelagoRandomizer] A1_S3_GameLevel / Connection_CrateChange_Enter (SceneConnectionPoint) -> A6_S1_AbandonMine_Remake_4wei / A6_S1_To_A1_S3
        unused? AM connection doesn't use this, no cutscene I know of

    ---

    level A1_S1_GameLevel / scene A1_S1_HumanDisposal_Final / AFM
    [Warning:ArchipelagoRandomizer] A1_S1_GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    [Warning:ArchipelagoRandomizer] A1_S1_GameLevel / Connection_Prefab (SceneConnectionPoint) -> A1_S2_ConnectionToElevator_Final / A1_S1_To_A1_S2
    [Warning:ArchipelagoRandomizer] A1_S1_GameLevel / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S1_Parry / A1_S1_To_AG_Lear_S1
    [Warning:ArchipelagoRandomizer] A1_S1_GameLevel / Connection_Prefab (1) (SceneConnectionPoint) -> A0_S3_altar / A0_S3_To_A1_S1

    ---

    level GameLevel / scene A0_S10_SpaceshipYard / GD
    [Warning:ArchipelagoRandomizer] GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    [Warning:ArchipelagoRandomizer] GameLevel / Connection_A0_S9 (SceneConnectionPoint) -> A0_S9_AltarReturned / A0_S9_To_A0_S10
    [Warning:ArchipelagoRandomizer] GameLevel / Connection From_AG_S2 (SceneConnectionPoint) -> AG_S2_YiBase / AG_S2_To_A0_S10
    [Warning:ArchipelagoRandomizer] GameLevel / Connection_A2_S6 (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / A0_S10_To_A2_S6
        to CTH, behind Yanren arena
    [Warning:ArchipelagoRandomizer] GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    [Warning:ArchipelagoRandomizer] GameLevel / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S2_HugeTaciChiParry / A1_S1_To_AG_LeeEar_S1
    [Warning:ArchipelagoRandomizer] GameLevel / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    
    ---

    level A7_S1 / scene A7_S1_BrainRoom_Remake / CC
    [Warning:ArchipelagoRandomizer] A7_S1 / Connection_Prefab_A5S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A7_To_A5_S1
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

    level  / scene A6_S1_AbandonMine_Remake_4wei / AM
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Teleport (SceneConnectionPoint) -> A5_S3_UnderCastle_Remake_4wei / A5_S3_To_A6_S1
    [Warning:ArchipelagoRandomizer] A6_S1 / 演出結束換景 (要自己拉) (SceneConnectionPoint) -> A2_S6_LogisticCenter_Final / AG_Tutorial_Lear_S2_識破JumpKick
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S4_AirDash_空中閃避 / AG_Tutorial_Lear_AirDash
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S5_大反ChargedParry / AG_Tutorial_Lear_S4_大反ChargedParry
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Point (SceneConnectionPoint) -> AG_Tutorial_Lear_S2_識破JumpKick / AG_Tutorial_Lear_S2_識破JumpKick
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_LearTutorial (SceneConnectionPoint) -> AG_Tutorial_Lear_S4_AirDash_空中閃避 / AG_Tutorial_Lear_AirDash
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A6_S3 (SceneConnectionPoint) -> A6_S3_Tutorial_And_SecretBoss_Remake / A6_S1->A6_S3
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A5_S1 (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A6_S1_To_A5_S1_SideCave
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A5_S1_Hole (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A6_S1_Hole
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_To_AG_S2 (SceneConnectionPoint) -> AG_S2_YiBase / A6_S1_To_AG_S2
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab_A4_S1 (SceneConnectionPoint) -> A4_S1_NewBridgeToWarehouse_Final / A6_S1_To_A4_S1
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S3_重擊ChargeAttack / AG_Tutorial_Lear_S3_重擊ChargeAttack
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Prefab (SceneConnectionPoint) -> AG_Tutorial_Lear_S0_格擋複習 / A1_S1_To_AG_Lear_S0
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A6_S1_To_A1_S3
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_BoxChangeScene (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A6_S1_To_A1_S3
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_Teleport (SceneConnectionPoint) -> A5_S1_CastleHub_remake / A5_S1_To_A6_S1
    [Warning:ArchipelagoRandomizer] A6_S1 / Connection_CrateChange_Enter (SceneConnectionPoint) -> A1_S3_InnerHumanDisposal_Final / A1_S3_To_A6_S1
        to AFD
     */

    [HarmonyPrefix, HarmonyPatch(typeof(SceneConnectionPoint), "Awake")]
    static void SceneConnectionPoint_Awake(SceneConnectionPoint __instance) {
        var level = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        Log.Warning($"{level} / {__instance} -> {__instance.scene.SceneName} / {__instance.connectionID}");
 
        if (
            level == "A10_S3" &&
            __instance.scene.SceneName == "A10_S1_TombEntrance_remake" &&
            __instance.connectionID == "A10_S1->A10_S3"
        ) {
            Log.Warning($"editing connection 1!");
            //changeSceneData.sceneName = "A10_S3_HistoryTomb_Right"; // must wait until scene is loaded from a string in ChangeScene
            __instance.connectionID = "A10_S3_To_A10_S4_EntryB";
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameCore), "ChangeScene", [typeof(SceneConnectionPoint.ChangeSceneData), typeof(bool), typeof(bool), typeof(float)])]
    static void GameCore_ChangeScene(GameCore __instance, ref SceneConnectionPoint.ChangeSceneData changeSceneData) {
        var level = SingletonBehaviour<GameCore>.Instance.gameLevel.name;
        Log.Warning($"GameCore_ChangeScene {level} -> {changeSceneData.sceneName} / {changeSceneData.connectionID}");

        if (
            level == "A10_S3" &&
            changeSceneData.sceneName == "A10_S1_TombEntrance_remake" &&
            changeSceneData.connectionID == "A10_S3_To_A10_S4_EntryB" // already changed by other patch
        ) {
            Log.Warning($"editing connection 2!");
            changeSceneData.sceneName = "A10_S3_HistoryTomb_Right";
            //changeSceneData.connectionID = "A10_S3_To_A10_S4_EntryB"; // no-op, nothing reads CSD.connectionID
        }
    }
}
