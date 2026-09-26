using System.Collections.Generic;

namespace ArchipelagoRandomizer.Features.EntranceRando;

internal class PortalData {
    public enum Portal {
        GOSE_UPPER_PORTAL,
        GOSE_MIDDLE_PORTAL,
        GOSE_LOWER_PORTAL,

        ASP_PORTAL,
        GOSW_UPPER_RIGHT_PORTAL,
        GOSW_MIDDLE_RIGHT_PORTAL,
        GOSW_LOWER_RIGHT_ELEVATOR,
        GOSW_UPPER_LEFT_PORTAL,
        GOSW_LOWER_LEFT_TRANSPORTER,
        GOSW_BOSS_PORTAL,

        GOSY_UPPER_RIGHT_PORTAL,
        GOSY_LOWER_RIGHT_PORTAL,
        GOSY_UPPER_ELEVATOR,
        GOSY_LOWER_ELEVATOR_SHAFT,
        GOSY_LEFT_PORTAL,

        LYR_LEFT_PORTAL,
        LYR_TOP_ELEVATOR,
        LYR_BOTTOM_PORTAL,
        LYR_RIGHT_PORTAL,

        GREENHOUSE_TOP_ELEVATOR_SHAFT,
        GREENHOUSE_BOTTOM_PORTAL,

        AH_LEFT_PORTAL,
        AH_RIGHT_ELEVATOR,

        WOS_LEFT_PORTAL,
        WOS_TOP_PORTAL,
        WOS_RIGHT_PORTAL,

        YC_LEFT_PORTAL,
        YC_TOP_PORTAL,
        YC_RIGHT_PORTAL,

        ST_BOTTOM_ELEVATOR,
        ST_RIGHT_PORTAL,

        EDP_LEFT_PORTAL,
        EDP_TOP_ELEVATOR,
        EDP_LOWER_RIGHT_TRANSPORTER,
        EDP_UPPER_RIGHT_PORTAL,

        EDLA_BOTTOM_ELEVATOR,
        EDLA_LEFT_PORTAL,

        NH_PORTAL,
        EDS_RIGHT_PORTAL,
        EDS_BOSS_PORTAL,

        TRC_LEFT_CRATES,
        TRC_RIGHT_PORTAL,

        CTH_LOWER_LEFT_PORTAL,
        CTH_MIDDLE_LEFT_PORTAL,
        CTH_UPPER_LEFT_VENT_SHAFT,
        CTH_UPPER_LEFT_PORTAL,
        CTH_LOWER_RIGHT_TRANSPORTER,
        CTH_RIGHT_CRATES,

        CH_UPPER_LEFT_PORTAL,
        CH_BOTTOM_VENT_SHAFT,
        CH_LOWER_RIGHT_PORTAL,
        CH_UPPER_RIGHT_PORTAL,

        PRE_LEFT_TRANSPORTER,
        PRE_RIGHT_TRANSPORTER,

        RP_PORTAL,
        PRC_LEFT_TRANSPORTER,
        PRC_RIGHT_TRANSPORTER,
        PRC_BOSS_PORTAL,

        PRW_LEFT_TRANSPORTER,
        PRW_RIGHT_TRANSPORTER,

        AFE_LOWER_LEFT_PORTAL,
        AFE_UPPER_LEFT_PORTAL,
        AFE_RIGHT_PORTAL,

        AFD_UPPER_LEFT_CRATES,
        AFD_LOWER_LEFT_TRANSPORTER,
        AFD_RIGHT_PORTAL,

        AFM_RIGHT_PORTAL,

        GD_LEFT_PORTAL,
        GD_RIGHT_PORTAL,

        CC_LEFT_PORTAL,
        CC_RIGHT_PORTAL,

        FGH_LEFT_PORTAL,
        FGH_BOTTOM_LEFT_ELEVATOR,
        FGH_BOTTOM_RIGHT_HOLE_PORTAL,
        FGH_BOTTOM_RIGHT_SIDE_CAVE_PORTAL,
        FGH_TOP_LEFT_ELEVATOR,
        FGH_TOP_RIGHT_ELEVATOR,
        FGH_RIGHT_PORTAL,

        SH_ELEVATOR,
        FPA_BOTTOM_LEFT_ELEVATOR,
        FPA_BOTTOM_RIGHT_ELEVATOR,
        FPA_TOP_ELEVATOR,

        FU_LEFT_PORTAL,
        FU_TOP_LEFT_ELEVATOR,
        FU_BOTTOM_ELEVATOR,
        FU_LOWER_RIGHT_CRATES,
        FU_MIDDLE_RIGHT_PORTAL,
        FU_UPPER_RIGHT_HOLE_PORTAL,
        FU_UPPER_RIGHT_SIDE_CAVE_PORTAL,

        AM_LEFT_PORTAL,
        AM_RIGHT_PORTAL,

        UC_LEFT_PORTAL,
        // for now we don't randomize the entrances between UC and PBV east/west
        PBV_EAST_RIGHT_PORTAL,

        FMR_LOWER_LEFT_ELEVATOR,
        FMR_RIGHT_ELEVATOR,

        PRISON_ELEVATOR,

        OW_MIDDLE_LEFT_PORTAL,
        OW_UPPER_LEFT_CRATES,
        OW_LOWER_RIGHT_PORTAL,
        OW_MIDDLE_RIGHT_PORTAL,

        IW_RIGHT_CRATES,
        IW_BOTTOM_ELEVATOR,

        BR_TOP_ELEVATOR,
        BR_RIGHT_PORTAL,

        YH_LEFT_PORTAL,
        YH_RIGHT_PORTAL,
    }

    public static readonly List<Portal> DepartureOnlyPortals = new List<Portal> {
        Portal.GOSY_LOWER_ELEVATOR_SHAFT,
        Portal.GREENHOUSE_BOTTOM_PORTAL,
        Portal.CH_BOTTOM_VENT_SHAFT,
        Portal.FGH_BOTTOM_RIGHT_HOLE_PORTAL,
    };

    public static readonly List<Portal> ArrivalOnlyPortals = new List<Portal> {
        Portal.GREENHOUSE_TOP_ELEVATOR_SHAFT,
        Portal.WOS_TOP_PORTAL,
        Portal.CTH_UPPER_LEFT_VENT_SHAFT,
        Portal.FU_UPPER_RIGHT_HOLE_PORTAL,
    };

    // we want to use these as dict keys/values, so we need value equality, hence structs instead of classes
    public struct DepartureIds {
        public string levelName;
        public string sceneName;
        public string connectionName;
        public DepartureIds(string l, string s, string c) {
            levelName = l;
            sceneName = s;
            connectionName = c;
        }
    }

    public struct ArrivalIds {
        public string sceneName;
        public string connectionName;
        public ArrivalIds(string s, string c) {
            sceneName = s;
            connectionName = c;
        }
    }


    // here we need duplicate values because there are often multiple vanilla connections for the same transition,
    // depending on e.g. whether a certain cutscene has happened already
    public static readonly Dictionary<DepartureIds, Portal> VanillaDepartures = new Dictionary<DepartureIds, Portal> {
        { new DepartureIds("A10_S3", "A10_SG6_SisterMemory", "A10_S3_To_A10_SG6"), Portal.GOSE_UPPER_PORTAL }, // first time Heng flashback
        { new DepartureIds("A10_S3", "A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryB"), Portal.GOSE_UPPER_PORTAL }, // after the Heng flashback
        { new DepartureIds("A10_S3", "A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryA"), Portal.GOSE_MIDDLE_PORTAL },
        { new DepartureIds("A10_S3", "A10_S1_TombEntrance_remake", "A10_S1->A10_S3"), Portal.GOSE_LOWER_PORTAL },

        { new DepartureIds("A10S5", "A10_S4_HistoryTomb_Left", "A10_S4_To_BossFight_Jee"), Portal.ASP_PORTAL },
        { new DepartureIds("A10_S4", "A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryB"), Portal.GOSW_UPPER_RIGHT_PORTAL },
        { new DepartureIds("A10_S4", "A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryA"), Portal.GOSW_MIDDLE_RIGHT_PORTAL },
        { new DepartureIds("A10_S4", "A10_S1_TombEntrance_remake", "A10_S4_To_A10_S1_Elevator"), Portal.GOSW_LOWER_RIGHT_ELEVATOR },
        { new DepartureIds("A10_S4", "A9_S1_Remake_4wei", "A10_S4_To_A9_S1"), Portal.GOSW_UPPER_LEFT_PORTAL },
        { new DepartureIds("A10_S4", "A9_S1_Remake_4wei", "A9_S1_To_A10_S4_Elevator"), Portal.GOSW_LOWER_LEFT_TRANSPORTER },
        { new DepartureIds("A10_S4", "A10_S5_Boss_Jee", "A10_S4_To_BossFight_Jee"), Portal.GOSW_BOSS_PORTAL },

        { new DepartureIds("A10_S1", "A10_S3_HistoryTomb_Right", "A10_S1->A10_S3"), Portal.GOSY_UPPER_RIGHT_PORTAL },
        { new DepartureIds("A10_S1", "A3_S5_BossGouMang_Final", "A3_S5_To_A10_S1"), Portal.GOSY_LOWER_RIGHT_PORTAL },
        { new DepartureIds("A10_S1", "A10_S4_HistoryTomb_Left", "A10_S4_To_A10_S1_Elevator"), Portal.GOSY_UPPER_ELEVATOR },
        { new DepartureIds("A10_S1", "A3_S2_GreenHouse_Final", "A10_S1_To_A3_S2"), Portal.GOSY_LOWER_ELEVATOR_SHAFT }, // departure-only portal
        { new DepartureIds("A10_S1", "A3_S1_GardenRuins_Final", "A3_S1_to_A10_S1"), Portal.GOSY_LEFT_PORTAL },

        { new DepartureIds("A3_S1", "AG_S1_SenateHall", "AG_S1_To_A3_S1"), Portal.LYR_LEFT_PORTAL },
        { new DepartureIds("A3_S1", "A9_S4", "A3_S1->A9_S4"), Portal.LYR_TOP_ELEVATOR },
        { new DepartureIds("A3_S1", "A3_S7_DragonWay_Final", "A3_S1_To_A3_S7"), Portal.LYR_BOTTOM_PORTAL },
        { new DepartureIds("A3_S1", "A10_S1_TombEntrance_remake", "A3_S1_to_A10_S1"), Portal.LYR_RIGHT_PORTAL },

        { new DepartureIds("A3_S2", "A10_S1_TombEntrance_remake", "A10_S1_To_A3_S2"), Portal.GREENHOUSE_TOP_ELEVATOR_SHAFT }, // arrival-only portal
        { new DepartureIds("A3_S2", "A3_S3_OxygenChamber_Final", "A3_S2_To_A3_S3"), Portal.GREENHOUSE_BOTTOM_PORTAL }, // departure-only portal

        { new DepartureIds("A3_S5_BossGouMang_GameLevel", "A10_S1_TombEntrance_remake", "A3_S5_To_A10_S1"), Portal.AH_LEFT_PORTAL },
        { new DepartureIds("A3_S5_BossGouMang_GameLevel", "A3_S3_OxygenChamber_Final", "A3_S3_To_A3_S5"), Portal.AH_RIGHT_ELEVATOR },

        { new DepartureIds("A3_S3", "A3_S7_DragonWay_Final", "A3_S3_To_A3_S7"), Portal.WOS_LEFT_PORTAL },
        { new DepartureIds("A3_S3", "A3_S2_GreenHouse_Final", "A3_S2_To_A3_S3"), Portal.WOS_TOP_PORTAL }, // arrival-only portal
        { new DepartureIds("A3_S3", "A3_S5_BossGouMang_Final", "A3_S3_To_A3_S5"), Portal.WOS_RIGHT_PORTAL },

        { new DepartureIds("A3_S7", "A11_S1_Hospital_remake", "A3_S7_To_A11_S1"), Portal.YC_LEFT_PORTAL },
        { new DepartureIds("A3_S7", "A3_S1_GardenRuins_Final", "A3_S1_To_A3_S7"), Portal.YC_TOP_PORTAL },
        { new DepartureIds("A3_S7", "A3_S3_OxygenChamber_Final", "A3_S3_To_A3_S7"), Portal.YC_RIGHT_PORTAL },

        { new DepartureIds("A9_S4", "A3_S1_GardenRuins_Final", "A3_S1->A9_S4"), Portal.ST_BOTTOM_ELEVATOR },
        { new DepartureIds("A9_S4", "A9_S1_Remake_4wei", "A9_S1_to_A9_S4"), Portal.ST_RIGHT_PORTAL },

        { new DepartureIds("A9_S1", "A9_S4", "A9_S1_to_A9_S4"), Portal.EDP_LEFT_PORTAL },
        { new DepartureIds("A9_S1", "A9_S2_Remake_4wei", "A9_S1_To_A9_S2"), Portal.EDP_TOP_ELEVATOR },
        { new DepartureIds("A9_S1", "A10_S4_HistoryTomb_Left", "A10_S4_To_A9_S1"), Portal.EDP_UPPER_RIGHT_PORTAL },
        { new DepartureIds("A9_S1", "A10_S4_HistoryTomb_Left", "A9_S1_To_A10_S4_Elevator"), Portal.EDP_LOWER_RIGHT_TRANSPORTER },

        { new DepartureIds("A9_S2", "A9_S1_Remake_4wei", "A9_S1_To_A9_S2"), Portal.EDLA_BOTTOM_ELEVATOR },
        { new DepartureIds("A9_S2", "A9_S3", "A9_S2_to_A9_S3_Memory"), Portal.EDLA_LEFT_PORTAL },
        { new DepartureIds("A9_S2", "A9_S3", "A9_S2_to_A9_S3"), Portal.EDLA_LEFT_PORTAL },

        { new DepartureIds("P2_R22_Savepoint_GameLevel", "A9_S3", "A9_S3->A9_S5_風氏"), Portal.NH_PORTAL },
        { new DepartureIds("A9_S3", "A9_S2_Remake_4wei", "A9_S2_to_A9_S3"), Portal.EDS_RIGHT_PORTAL },
        { new DepartureIds("A9_S3", "A9_S5_風氏", "A9_S3->A9_S5_風氏"), Portal.EDS_BOSS_PORTAL },

        { new DepartureIds("A11_S1", "A2_S6_LogisticCenter_Final", "A11_S1_To_A2_S6"), Portal.TRC_LEFT_CRATES },
        { new DepartureIds("A11_S1", "A3_S7_DragonWay_Final", "A3_S7_To_A11_S1"), Portal.TRC_RIGHT_PORTAL },

        { new DepartureIds("A2_S6", "A1_S2_ConnectionToElevator_Final", "A1_S2_RightLockCorridar"), Portal.CTH_LOWER_LEFT_PORTAL },
        { new DepartureIds("A2_S6", "A0_S10_SpaceshipYard", "A0_S10_To_A2_S6"), Portal.CTH_MIDDLE_LEFT_PORTAL },
        { new DepartureIds("A2_S6", "AG_S1_SenateHall", "AG_S1_To_A2_S6_2nd"), Portal.CTH_UPPER_LEFT_VENT_SHAFT }, // arrival-only portal
        { new DepartureIds("A2_S6", "AG_S1_SenateHall", "AG_S1_To_A2_S6"), Portal.CTH_UPPER_LEFT_PORTAL },
        { new DepartureIds("A2_S6", "A11_S1_Hospital_remake", "A2_S6_To_A11_S1"), Portal.CTH_RIGHT_CRATES },
        { new DepartureIds("A2_S6", "A2_S2_ReactorRight_Final", "A2_S6_A2_S2"), Portal.CTH_LOWER_RIGHT_TRANSPORTER },

        { new DepartureIds("AG_S1", "A7_S1_BrainRoom_Remake", "A7_To_AG_S1"), Portal.CH_UPPER_LEFT_PORTAL },
        { new DepartureIds("AG_S1", "A2_S6_LogisticCenter_Final", "AG_S1_To_A2_S6_2nd"), Portal.CH_BOTTOM_VENT_SHAFT }, // departure-only portal
        { new DepartureIds("AG_S1", "A2_S6_LogisticCenter_Final", "AG_S1_To_A2_S6"), Portal.CH_LOWER_RIGHT_PORTAL },
        { new DepartureIds("AG_S1", "A3_S1_GardenRuins_Final", "AG_S1_To_A3_S1"), Portal.CH_UPPER_RIGHT_PORTAL },

        { new DepartureIds("A2_S2", "A2_SG4_MemoryGondola_Final", "A2_S1_To_A2_SG4"), Portal.PRE_LEFT_TRANSPORTER }, // first time Heng flashback
        { new DepartureIds("A2_S2", "A2_S1_ReactorMiddle_Final", "A2_S1_To_A2_S2"), Portal.PRE_LEFT_TRANSPORTER }, // after the Heng flashback
        { new DepartureIds("A2_S2", "A2_S6_LogisticCenter_Final", "A2_S6_A2_S2"), Portal.PRE_RIGHT_TRANSPORTER },

        { new DepartureIds("A2_S5_ BossHorseman_GameLevel", "A2_S1_ReactorMiddle_Final", "A2_S1_To_A2_S5"), Portal.RP_PORTAL },
        { new DepartureIds("A2_S1", "A2_S3_ReactorLeft_Final", "A2_S1_To_A2_S3"), Portal.PRC_LEFT_TRANSPORTER },
        { new DepartureIds("A2_S1", "A2_S2_ReactorRight_Final", "A2_S1_To_A2_S2"), Portal.PRC_RIGHT_TRANSPORTER },
        { new DepartureIds("A2_S1", "A2_S5_BossHorseman_Final", "A2_S1_To_A2_S5"), Portal.PRC_BOSS_PORTAL },

        { new DepartureIds("A2_S3", "A1_S3_InnerHumanDisposal_Final", "A1_S3_A2_S3"), Portal.PRW_LEFT_TRANSPORTER },
        { new DepartureIds("A2_S3", "A2_SG4_MemoryGondola_Final", "A2_S1_To_A2_SG4"), Portal.PRW_RIGHT_TRANSPORTER }, // first time Heng flashback
        { new DepartureIds("A2_S3", "A2_S1_ReactorMiddle_Final", "A2_S1_To_A2_S3"), Portal.PRW_RIGHT_TRANSPORTER }, // after the Heng flashback

        { new DepartureIds("A1_S2_GameLevel", "A1_S3_InnerHumanDisposal_Final", "A1_S3_A1_S2"), Portal.AFE_LOWER_LEFT_PORTAL },
        { new DepartureIds("A1_S2_GameLevel", "A1_S1_HumanDisposal_Final", "A1_S1_To_A1_S2"), Portal.AFE_UPPER_LEFT_PORTAL },
        { new DepartureIds("A1_S2_GameLevel", "A2_S6_LogisticCenter_Final", "A1_S2_RightLockCorridar"), Portal.AFE_RIGHT_PORTAL },

        { new DepartureIds("A1_S3_GameLevel", "A6_S1_AbandonMine_Remake_4wei", "A1_S3_To_A6_S1"), Portal.AFD_UPPER_LEFT_CRATES },
        { new DepartureIds("A1_S3_GameLevel", "A2_S3_ReactorLeft_Final", "A1_S3_A2_S3"), Portal.AFD_LOWER_LEFT_TRANSPORTER },
        { new DepartureIds("A1_S3_GameLevel", "A1_S2_ConnectionToElevator_Final", "A1_S3_A1_S2"), Portal.AFD_RIGHT_PORTAL },

        { new DepartureIds("A1_S1_GameLevel", "A1_S2_ConnectionToElevator_Final", "A1_S1_To_A1_S2"), Portal.AFM_RIGHT_PORTAL },

        { new DepartureIds("GameLevel", "A0_S9_AltarReturned", "A0_S9_To_A0_S10"), Portal.GD_LEFT_PORTAL },
        { new DepartureIds("GameLevel", "A2_S6_LogisticCenter_Final", "A0_S10_To_A2_S6"), Portal.GD_RIGHT_PORTAL },

        { new DepartureIds("A7_S1", "A5_S1_CastleHub_remake", "A7_To_A5_S1"), Portal.CC_LEFT_PORTAL },
        { new DepartureIds("A7_S1", "AG_S1_SenateHall", "A7_To_AG_S1"), Portal.CC_RIGHT_PORTAL },

        { new DepartureIds("A5_S1", "A4_S1_NewBridgeToWarehouse_Final", "A5_S1_To_A4_S1"), Portal.FGH_LEFT_PORTAL },
        { new DepartureIds("A5_S1", "A6_S1_AbandonMine_Remake_4wei", "A5_S1_To_A6_S1"), Portal.FGH_BOTTOM_LEFT_ELEVATOR },
        { new DepartureIds("A5_S1", "A6_S1_AbandonMine_Remake_4wei", "A5_S1_To_A6_S1_Hole"), Portal.FGH_BOTTOM_RIGHT_HOLE_PORTAL }, // departure-only portal
        { new DepartureIds("A5_S1", "A6_S1_AbandonMine_Remake_4wei", "A6_S1_To_A5_S1_SideCave"), Portal.FGH_BOTTOM_RIGHT_SIDE_CAVE_PORTAL },

        { new DepartureIds("A5_S1", "A5_AC2_Jie&Jee", "A5_S1_To_A5_AC2"), Portal.FGH_TOP_LEFT_ELEVATOR }, // first time Jiequan & Ji cutscene
        { new DepartureIds("A5_S1", "A5_S4_CastleMid_Remake_5wei", "A5_S1_To_A5_S4_Left"), Portal.FGH_TOP_LEFT_ELEVATOR }, // after the Jiequan & Ji cutscene
        { new DepartureIds("A5_S1", "A5_S4_CastleMid_Remake_5wei", "A5_S1_To_A5_S4_Right"), Portal.FGH_TOP_RIGHT_ELEVATOR },
            // needs logic for being unlocked from FPA
        { new DepartureIds("A5_S1", "A7_S1_BrainRoom_Remake", "A7_To_A5_S1"), Portal.FGH_RIGHT_PORTAL },

        { new DepartureIds("A5_S5", "A5_S4_CastleMid_Remake_5wei", "A5_S4_To_A5_S5"), Portal.SH_ELEVATOR },
        { new DepartureIds("A5_S4", "A5_S1_CastleHub_remake", "A5_S1_To_A5_S4_Left"), Portal.FPA_BOTTOM_LEFT_ELEVATOR },
        { new DepartureIds("A5_S4", "A5_S1_CastleHub_remake", "A5_S1_To_A5_S4_Right"), Portal.FPA_BOTTOM_RIGHT_ELEVATOR },
        { new DepartureIds("A5_S4", "A5_S5_JieChuanHall", "A5_S4_To_A5_S5"), Portal.FPA_TOP_ELEVATOR },

        { new DepartureIds("A6_S1", "A4_S1_NewBridgeToWarehouse_Final", "A6_S1_To_A4_S1"), Portal.FU_LEFT_PORTAL },
        { new DepartureIds("A6_S1", "A5_S1_CastleHub_remake", "A5_S1_To_A6_S1"), Portal.FU_TOP_LEFT_ELEVATOR },
        { new DepartureIds("A6_S1", "A5_S3_UnderCastle_Remake_4wei", "A5_S3_To_A6_S1"), Portal.FU_BOTTOM_ELEVATOR },
        { new DepartureIds("A6_S1", "A1_S3_InnerHumanDisposal_Final", "A6_S1_To_A1_S3"), Portal.FU_LOWER_RIGHT_CRATES },
        { new DepartureIds("A6_S1", "A6_S3_Tutorial_And_SecretBoss_Remake", "A6_S1->A6_S3"), Portal.FU_MIDDLE_RIGHT_PORTAL },
        { new DepartureIds("A6_S1", "A5_S1_CastleHub_remake", "A5_S1_To_A6_S1_Hole"), Portal.FU_UPPER_RIGHT_HOLE_PORTAL }, // arrival-only portal
        { new DepartureIds("A6_S1", "A5_S1_CastleHub_remake", "A6_S1_To_A5_S1_SideCave"), Portal.FU_UPPER_RIGHT_SIDE_CAVE_PORTAL },

        { new DepartureIds("A6_S3", "A6_S1_AbandonMine_Remake_4wei", "A6_S1->A6_S3"), Portal.AM_LEFT_PORTAL },
        { new DepartureIds("A6_S3", "A0_S7_CaveReturned", "A6_S3_To_A0_S7"), Portal.AM_RIGHT_PORTAL },

        { new DepartureIds("A0_S7", "A6_S3_Tutorial_And_SecretBoss_Remake", "A6_S3_To_A0_S7"), Portal.UC_LEFT_PORTAL },
        { new DepartureIds("GameLevel", "A0_S10_SpaceshipYard", "A0_S9_To_A0_S10"), Portal.PBV_EAST_RIGHT_PORTAL },

        { new DepartureIds("A5_S3", "A5_S2_Jail_Remake_Final", "A5_S2_To_A5_S3"), Portal.FMR_LOWER_LEFT_ELEVATOR },
            // needs logic for being unlocked from Prison
        { new DepartureIds("A5_S3", "A6_S1_AbandonMine_Remake_4wei", "A5_S3_To_A6_S1"), Portal.FMR_RIGHT_ELEVATOR },

        { new DepartureIds("A5_S2", "A5_S3_UnderCastle_Remake_4wei", "A5_S2_To_A5_S3"), Portal.PRISON_ELEVATOR },

        { new DepartureIds("A4_S1", "A4_S6_DaoBase_Final", "A4_S6_To_A4_S1"), Portal.OW_MIDDLE_LEFT_PORTAL },
        { new DepartureIds("A4_S1", "A4_SG3_MemoryCrate New", "A4_S1_To_A4_SG3"), Portal.OW_UPPER_LEFT_CRATES }, // first time Heng flashback
        { new DepartureIds("A4_S1", "A4_S2_RouteToControlRoom_Final", "A4_S1_To_A4_S2"), Portal.OW_UPPER_LEFT_CRATES }, // after the Heng flashback
        { new DepartureIds("A4_S1", "A6_S1_AbandonMine_Remake_4wei", "A6_S1_To_A4_S1"), Portal.OW_LOWER_RIGHT_PORTAL },
        { new DepartureIds("A4_S1", "A5_S1_CastleHub_remake", "A5_S1_To_A4_S1"), Portal.OW_MIDDLE_RIGHT_PORTAL },

        { new DepartureIds("A4_S2", "A4_S1_NewBridgeToWarehouse_Final", "A4_S2_To_A4_S1"), Portal.IW_RIGHT_CRATES },
        { new DepartureIds("A4_S2", "A4_S3_ControlRoom_Final", "A4_S2_To_A4_S3"), Portal.IW_BOTTOM_ELEVATOR },

        { new DepartureIds("A4_S3", "A4_S2_RouteToControlRoom_Final", "A4_S3_To_A4_S2"), Portal.BR_TOP_ELEVATOR },
        { new DepartureIds("A4_S3", "A4_S5_DaoTrapHouse_Final", "A4_S3_To_A4_S5_BossRoom"), Portal.BR_RIGHT_PORTAL },

        { new DepartureIds("A0_S6", "A4_S3_ControlRoom_Final", "A4_S6_To_A4_S3"), Portal.YH_LEFT_PORTAL },
        { new DepartureIds("A0_S6", "A4_S1_NewBridgeToWarehouse_Final", "A4_S6_To_A4_S1"), Portal.YH_RIGHT_PORTAL },
    };

    // but this mapping needs to be unique per portal, so let's store it in the other direction to enforce that
    public static readonly Dictionary<Portal, ArrivalIds> VanillaArrivals = new Dictionary<Portal, ArrivalIds> {
        { Portal.GOSE_UPPER_PORTAL, new ArrivalIds("A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryB") },
        { Portal.GOSE_MIDDLE_PORTAL, new ArrivalIds("A10_S3_HistoryTomb_Right", "A10_S3_To_A10_S4_EntryA") },
        { Portal.GOSE_LOWER_PORTAL, new ArrivalIds("A10_S3_HistoryTomb_Right", "A10_S1->A10_S3") },

        { Portal.ASP_PORTAL, new ArrivalIds("A10_S5_Boss_Jee", "A10_S4_To_BossFight_Jee") },
        { Portal.GOSW_UPPER_RIGHT_PORTAL, new ArrivalIds("A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryB") },
        { Portal.GOSW_MIDDLE_RIGHT_PORTAL, new ArrivalIds("A10_S4_HistoryTomb_Left", "A10_S3_To_A10_S4_EntryA") },
        { Portal.GOSW_LOWER_RIGHT_ELEVATOR, new ArrivalIds("A10_S4_HistoryTomb_Left", "A10_S4_To_A10_S1_Elevator") },
        { Portal.GOSW_UPPER_LEFT_PORTAL, new ArrivalIds("A10_S4_HistoryTomb_Left", "A10_S4_To_A9_S1") },
        { Portal.GOSW_LOWER_LEFT_TRANSPORTER, new ArrivalIds("A10_S4_HistoryTomb_Left", "A9_S1_To_A10_S4_Elevator") },
        { Portal.GOSW_BOSS_PORTAL, new ArrivalIds("A10_S4_HistoryTomb_Left", "A10_S4_To_BossFight_Jee") },

        { Portal.GOSY_UPPER_RIGHT_PORTAL, new ArrivalIds("A10_S1_TombEntrance_remake", "A10_S1->A10_S3") },
        { Portal.GOSY_LOWER_RIGHT_PORTAL, new ArrivalIds("A10_S1_TombEntrance_remake", "A3_S5_To_A10_S1") },
        { Portal.GOSY_UPPER_ELEVATOR, new ArrivalIds("A10_S1_TombEntrance_remake", "A10_S4_To_A10_S1_Elevator") },
        { Portal.GOSY_LOWER_ELEVATOR_SHAFT, new ArrivalIds("A10_S1_TombEntrance_remake", "A10_S1_To_A3_S2") }, // departure-only portal
        { Portal.GOSY_LEFT_PORTAL, new ArrivalIds("A10_S1_TombEntrance_remake", "A3_S1_to_A10_S1") },

        { Portal.LYR_LEFT_PORTAL, new ArrivalIds("A3_S1_GardenRuins_Final", "AG_S1_To_A3_S1") },
        { Portal.LYR_TOP_ELEVATOR, new ArrivalIds("A3_S1_GardenRuins_Final", "A3_S1->A9_S4") },
        { Portal.LYR_BOTTOM_PORTAL, new ArrivalIds("A3_S1_GardenRuins_Final", "A3_S1_To_A3_S7") },
        { Portal.LYR_RIGHT_PORTAL, new ArrivalIds("A3_S1_GardenRuins_Final", "A3_S1_to_A10_S1") },

        { Portal.GREENHOUSE_TOP_ELEVATOR_SHAFT, new ArrivalIds("A3_S2_GreenHouse_Final", "A10_S1_To_A3_S2") }, // arrival-only portal
        { Portal.GREENHOUSE_BOTTOM_PORTAL, new ArrivalIds("A3_S2_GreenHouse_Final", "A3_S2_To_A3_S3") },

        { Portal.AH_LEFT_PORTAL, new ArrivalIds("A3_S5_BossGouMang_Final", "A3_S5_To_A10_S1") },
        { Portal.AH_RIGHT_ELEVATOR, new ArrivalIds("A3_S5_BossGouMang_Final", "A3_S3_To_A3_S5") },

        { Portal.WOS_LEFT_PORTAL, new ArrivalIds("A3_S3_OxygenChamber_Final", "A3_S3_To_A3_S7") },
        { Portal.WOS_TOP_PORTAL, new ArrivalIds("A3_S3_OxygenChamber_Final", "A3_S2_To_A3_S3") }, // arrival-only portal
        { Portal.WOS_RIGHT_PORTAL, new ArrivalIds("A3_S3_OxygenChamber_Final", "A3_S3_To_A3_S5") },

        { Portal.YC_LEFT_PORTAL, new ArrivalIds("A3_S7_DragonWay_Final", "A3_S7_To_A11_S1") },
        { Portal.YC_TOP_PORTAL, new ArrivalIds("A3_S7_DragonWay_Final", "A3_S1_To_A3_S7") },
        { Portal.YC_RIGHT_PORTAL, new ArrivalIds("A3_S7_DragonWay_Final", "A3_S3_To_A3_S7") },

        { Portal.ST_BOTTOM_ELEVATOR, new ArrivalIds("A9_S4", "A3_S1->A9_S4") },
        { Portal.ST_RIGHT_PORTAL, new ArrivalIds("A9_S4", "A9_S1_to_A9_S4") },

        { Portal.EDP_LEFT_PORTAL, new ArrivalIds("A9_S1_Remake_4wei", "A9_S1_to_A9_S4") },
        { Portal.EDP_TOP_ELEVATOR, new ArrivalIds("A9_S1_Remake_4wei", "A9_S1_To_A9_S2") },
            // broken as target: stuck in pink waterfall
            // missing elevator animation as target
        { Portal.EDP_LOWER_RIGHT_TRANSPORTER, new ArrivalIds("A9_S1_Remake_4wei", "A9_S1_To_A10_S4_Elevator") },
        { Portal.EDP_UPPER_RIGHT_PORTAL, new ArrivalIds("A9_S1_Remake_4wei", "A10_S4_To_A9_S1") },

        { Portal.EDLA_BOTTOM_ELEVATOR, new ArrivalIds("A9_S2_Remake_4wei", "A9_S1_To_A9_S2") },
        { Portal.EDLA_LEFT_PORTAL, new ArrivalIds("A9_S2_Remake_4wei", "A9_S2_to_A9_S3") },

        { Portal.NH_PORTAL, new ArrivalIds("A9_S5_風氏", "A9_S3->A9_S5_風氏") },
        { Portal.EDS_RIGHT_PORTAL, new ArrivalIds("A9_S3", "A9_S2_to_A9_S3") },
        { Portal.EDS_BOSS_PORTAL, new ArrivalIds("A9_S3", "A9_S3->A9_S5_風氏") },

        { Portal.TRC_LEFT_CRATES, new ArrivalIds("A11_S1_Hospital_remake", "A2_S6_To_A11_S1") },
        { Portal.TRC_RIGHT_PORTAL, new ArrivalIds("A11_S1_Hospital_remake", "A3_S7_To_A11_S1") },

        { Portal.CTH_LOWER_LEFT_PORTAL, new ArrivalIds("A2_S6_LogisticCenter_Final", "A1_S2_RightLockCorridar") },
        { Portal.CTH_MIDDLE_LEFT_PORTAL, new ArrivalIds("A2_S6_LogisticCenter_Final", "A0_S10_To_A2_S6") },
        { Portal.CTH_UPPER_LEFT_VENT_SHAFT, new ArrivalIds("A2_S6_LogisticCenter_Final", "AG_S1_To_A2_S6_2nd") }, // arrival-only portal
        { Portal.CTH_UPPER_LEFT_PORTAL, new ArrivalIds("A2_S6_LogisticCenter_Final", "AG_S1_To_A2_S6") },
        { Portal.CTH_LOWER_RIGHT_TRANSPORTER, new ArrivalIds("A2_S6_LogisticCenter_Final", "A2_S6_A2_S2") },
        { Portal.CTH_RIGHT_CRATES, new ArrivalIds("A2_S6_LogisticCenter_Final", "A11_S1_To_A2_S6") },

        { Portal.CH_UPPER_LEFT_PORTAL, new ArrivalIds("AG_S1_SenateHall", "A7_To_AG_S1") },
        { Portal.CH_BOTTOM_VENT_SHAFT, new ArrivalIds("AG_S1_SenateHall", "AG_S1_To_A2_S6_2nd") }, // departure-only portal
        { Portal.CH_LOWER_RIGHT_PORTAL, new ArrivalIds("AG_S1_SenateHall", "AG_S1_To_A2_S6") },
        { Portal.CH_UPPER_RIGHT_PORTAL, new ArrivalIds("AG_S1_SenateHall", "AG_S1_To_A3_S1") },

        { Portal.PRE_LEFT_TRANSPORTER, new ArrivalIds("A2_S2_ReactorRight_Final", "A2_S1_To_A2_S2") },
        { Portal.PRE_RIGHT_TRANSPORTER, new ArrivalIds("A2_S2_ReactorRight_Final", "A2_S6_A2_S2") },

        { Portal.RP_PORTAL, new ArrivalIds("A2_S5_BossHorseman_Final", "A2_S1_To_A2_S5") },
        { Portal.PRC_LEFT_TRANSPORTER, new ArrivalIds("A2_S1_ReactorMiddle_Final", "A2_S1_To_A2_S3") },
        { Portal.PRC_RIGHT_TRANSPORTER, new ArrivalIds("A2_S1_ReactorMiddle_Final", "A2_S1_To_A2_S2") },
        { Portal.PRC_BOSS_PORTAL, new ArrivalIds("A2_S1_ReactorMiddle_Final", "A2_S1_To_A2_S5") },

        { Portal.PRW_LEFT_TRANSPORTER, new ArrivalIds("A2_S3_ReactorLeft_Final", "A1_S3_A2_S3") },
        { Portal.PRW_RIGHT_TRANSPORTER, new ArrivalIds("A2_S3_ReactorLeft_Final", "A2_S1_To_A2_S3") },

        { Portal.AFE_LOWER_LEFT_PORTAL, new ArrivalIds("A1_S2_ConnectionToElevator_Final", "A1_S3_A1_S2") },
        { Portal.AFE_UPPER_LEFT_PORTAL, new ArrivalIds("A1_S2_ConnectionToElevator_Final", "A1_S1_To_A1_S2") },
        { Portal.AFE_RIGHT_PORTAL, new ArrivalIds("A1_S2_ConnectionToElevator_Final", "A1_S2_RightLockCorridar") },

        { Portal.AFD_UPPER_LEFT_CRATES, new ArrivalIds("A1_S3_InnerHumanDisposal_Final", "A6_S1_To_A1_S3") },
        { Portal.AFD_LOWER_LEFT_TRANSPORTER, new ArrivalIds("A1_S3_InnerHumanDisposal_Final", "A1_S3_A2_S3") },
        { Portal.AFD_RIGHT_PORTAL, new ArrivalIds("A1_S3_InnerHumanDisposal_Final", "A1_S3_A1_S2") },

        { Portal.AFM_RIGHT_PORTAL, new ArrivalIds("A1_S1_HumanDisposal_Final", "A1_S1_To_A1_S2") },

        { Portal.GD_LEFT_PORTAL, new ArrivalIds("A0_S10_SpaceshipYard", "A0_S9_To_A0_S10") },
        { Portal.GD_RIGHT_PORTAL, new ArrivalIds("A0_S10_SpaceshipYard", "A0_S10_To_A2_S6") },

        { Portal.CC_LEFT_PORTAL, new ArrivalIds("A7_S1_BrainRoom_Remake", "A7_To_A5_S1") },
        { Portal.CC_RIGHT_PORTAL, new ArrivalIds("A7_S1_BrainRoom_Remake", "A7_To_AG_S1") },

        { Portal.FGH_LEFT_PORTAL, new ArrivalIds("A5_S1_CastleHub_remake", "A5_S1_To_A4_S1") },
        { Portal.FGH_BOTTOM_LEFT_ELEVATOR, new ArrivalIds("A5_S1_CastleHub_remake", "A5_S1_To_A6_S1") },
        { Portal.FGH_BOTTOM_RIGHT_HOLE_PORTAL, new ArrivalIds("A5_S1_CastleHub_remake", "A5_S1_To_A6_S1_Hole") }, // departure-only portal
        { Portal.FGH_BOTTOM_RIGHT_SIDE_CAVE_PORTAL, new ArrivalIds("A5_S1_CastleHub_remake", "A6_S1_To_A5_S1_SideCave") },
        { Portal.FGH_TOP_LEFT_ELEVATOR, new ArrivalIds("A5_S1_CastleHub_remake", "A5_S1_To_A5_S4_Left") },
        { Portal.FGH_TOP_RIGHT_ELEVATOR, new ArrivalIds("A5_S1_CastleHub_remake", "A5_S1_To_A5_S4_Right") },
        { Portal.FGH_RIGHT_PORTAL, new ArrivalIds("A5_S1_CastleHub_remake", "A7_To_A5_S1") },

        { Portal.SH_ELEVATOR, new ArrivalIds("A5_S5_JieChuanHall", "A5_S4_To_A5_S5") },
        { Portal.FPA_BOTTOM_LEFT_ELEVATOR, new ArrivalIds("A5_S4_CastleMid_Remake_5wei", "A5_S1_To_A5_S4_Left") },
        { Portal.FPA_BOTTOM_RIGHT_ELEVATOR, new ArrivalIds("A5_S4_CastleMid_Remake_5wei", "A5_S1_To_A5_S4_Right") },
        { Portal.FPA_TOP_ELEVATOR, new ArrivalIds("A5_S4_CastleMid_Remake_5wei", "A5_S4_To_A5_S5") },

        { Portal.FU_LEFT_PORTAL, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A6_S1_To_A4_S1") },
        { Portal.FU_TOP_LEFT_ELEVATOR, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A5_S1_To_A6_S1") },
        { Portal.FU_BOTTOM_ELEVATOR, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A5_S3_To_A6_S1") },
        { Portal.FU_LOWER_RIGHT_CRATES, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A1_S3_To_A6_S1") },
        { Portal.FU_MIDDLE_RIGHT_PORTAL, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A6_S1->A6_S3") },
        { Portal.FU_UPPER_RIGHT_HOLE_PORTAL, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A5_S1_To_A6_S1_Hole") }, // arrival-only portal
        { Portal.FU_UPPER_RIGHT_SIDE_CAVE_PORTAL, new ArrivalIds("A6_S1_AbandonMine_Remake_4wei", "A6_S1_To_A5_S1_SideCave") },

        { Portal.AM_LEFT_PORTAL, new ArrivalIds("A6_S3_Tutorial_And_SecretBoss_Remake", "A6_S1->A6_S3") },
        { Portal.AM_RIGHT_PORTAL, new ArrivalIds("A6_S3_Tutorial_And_SecretBoss_Remake", "A6_S3_To_A0_S7") },
            // broken as target: Yi death loops in the closed door

        { Portal.UC_LEFT_PORTAL, new ArrivalIds("A0_S7_CaveReturned", "A6_S3_To_A0_S7") },
        { Portal.PBV_EAST_RIGHT_PORTAL, new ArrivalIds("A0_S9_AltarReturned", "A0_S9_To_A0_S10") },
            // broken as target: Yi death loops in the unbroken rock formation

        { Portal.FMR_LOWER_LEFT_ELEVATOR, new ArrivalIds("A5_S3_UnderCastle_Remake_4wei", "A5_S2_To_A5_S3") },
        { Portal.FMR_RIGHT_ELEVATOR, new ArrivalIds("A5_S3_UnderCastle_Remake_4wei", "A5_S3_To_A6_S1") },

        { Portal.PRISON_ELEVATOR, new ArrivalIds("A5_S2_Jail_Remake_Final", "A5_S2_To_A5_S3") },

        { Portal.OW_MIDDLE_LEFT_PORTAL, new ArrivalIds("A4_S1_NewBridgeToWarehouse_Final", "A4_S6_To_A4_S1") },
        { Portal.OW_UPPER_LEFT_CRATES, new ArrivalIds("A4_S1_NewBridgeToWarehouse_Final", "A4_S2_To_A4_S1") },
        { Portal.OW_LOWER_RIGHT_PORTAL, new ArrivalIds("A4_S1_NewBridgeToWarehouse_Final", "A6_S1_To_A4_S1") },
        { Portal.OW_MIDDLE_RIGHT_PORTAL, new ArrivalIds("A4_S1_NewBridgeToWarehouse_Final", "A5_S1_To_A4_S1") },

        { Portal.IW_RIGHT_CRATES, new ArrivalIds("A4_S2_RouteToControlRoom_Final", "A4_S1_To_A4_S2") },
        { Portal.IW_BOTTOM_ELEVATOR, new ArrivalIds("A4_S2_RouteToControlRoom_Final", "A4_S2_To_A4_S3") },

        { Portal.BR_TOP_ELEVATOR, new ArrivalIds("A4_S3_ControlRoom_Final", "A4_S2_To_A4_S3") },
        { Portal.BR_RIGHT_PORTAL, new ArrivalIds("A4_S3_ControlRoom_Final", "A4_S3_To_A4_S5_BossRoom") },

        { Portal.YH_LEFT_PORTAL, new ArrivalIds("A4_S6_DaoBase_Final", "A4_S5_BossRoom_To_A4_S6") },
            // spawns Yi at the defeated Claw instead of at the door
            // possibly broken as target: should arriving here start the claw fight?
        { Portal.YH_RIGHT_PORTAL, new ArrivalIds("A4_S6_DaoBase_Final", "A4_S6_To_A4_S1") },
    };
}
