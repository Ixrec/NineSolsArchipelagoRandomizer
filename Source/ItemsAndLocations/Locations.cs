﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer;

public enum Location {
    AFM_BREAK_CORPSE,
    AFM_CHEST_UPPER_RIGHT,
    AFM_CHEST_LOWER_VENT,
    AFM_DB_SURVEILLANCE,

    AFE_CHEST_UPPER_PAGODA_LEFT,
    AFE_CHEST_UPPER_PAGODA_RIGHT,
    AFE_CHEST_MOVING_BOXES,
    AFE_CHEST_ELEVATOR,
    AFE_DROP_BAICHANG,
    AFE_CHEST_STATUE,
    AFE_CHEST_OVER_HAZARD,

    AFD_CHEST_CRYSTAL_CAVES,
    AFD_DROP_1_SHANGUI,
    AFD_DROP_2_SHANGUI,
    AFD_CHEST_BELOW_NODE,
    AFD_CHEST_STATUES,
    AFD_CHEST_LOWER_LEVEL,
    AFD_CHEST_UNDER_LOWER_LEFT_EXIT,
    AFD_FLOWER_UNDER_ELEVATOR,
    AFD_CHEST_UPPER_RIGHT_1,
    AFD_CHEST_UPPER_RIGHT_2,

    CH_COUNCIL_SIGN,
    CH_LAUNCH_MEMORIAL,
    CH_COUNCIL_TENETS,
    CH_CHEST_AXEBOT_AND_TURRETS,
    CH_CHEST_VENTS,

    FSP_KUAFU_GIFT_1,
    FSP_KUAFU_GIFT_2,
    FSP_SHUANSHUAN_MUSIC,
    FSP_SHUANSHUAN_PORTRAIT,
    FSP_CHIYOU_PORTRAIT,
    FSP_SHENNONG_PBV_QUEST,
    FSP_SHUANSHUAN_BOOK,
    FSP_CHIYOU_BOOK,
    //FSP_SHENNONG_SNAKE_QUEST, // post-all-poisons
    FSP_SHUANSHUAN_HIDDEN_POEM,
    FSP_CHEST_HALF_TREE,
    FSP_CHEST_FULL_TREE_1,
    FSP_CHEST_FULL_TREE_2,
    FSP_MUTANT_QUEST,
    FSP_KUAFU_HOMING_DARTS,
    FSP_KUAFU_THUNDERBURST_BOMB,
    // Damaged Fusang Amulet and Jin Medallion may be "missable" items, as well as // post-all-poisons

    CC_LADY_ETHEREAL,
    CC_FLOWER_LADY_ETHEREAL,
    CC_SHANHAI_CHIP,
    CC_CHEST_CAVES_UPPER_RIGHT,
    CC_CHEST_CAVES_CENTER,
    CC_CHEST_LEFT_EXIT,

    CTH_CHEST_LARGE_ELEVATOR,
    CTH_DROP_YANREN,
    CTH_CHEST_SIDE_ROOM,
    CTH_CHEST_VENTS_LEFT,
    CTH_ANOMALOUS_ROOT_NODE,
    CTH_CHEST_STATUES,
    
    PRE_CHEST_AFTER_LASERS,
    PRE_CHEST_UNDER_BOX,
    PRE_CHEST_UPPER_RIGHT,
    PRE_CHEST_STATUE,
    PRE_CHEST_UPPER_LEFT,
    PRE_DROP_JIAODUAN,
    
    PRC_CHEST_BREAKABLE_WALL_RIGHT,
    PRC_CHEST_NEAR_MOVING_BOX,
    PRC_CHEST_STATUE,
    PRC_CHEST_GUARDED_BY_BEETLE,
    PRC_CHEST_RIGHT_OF_PAGODA,
    PRC_SHANHAI_CHIP,
    PRC_RHIZOMATIC_ENERGY_METER,
    PRC_CHEST_LEFT_EXIT,
    PRC_CHEST_LEFT_OF_BRIDGE,
    
    RP_CONTROL_PANEL,
    RP_DROP_YINGZHAO,
    RP_KUAFU_SANCTUM,
    
    PRW_CHEST_BELOW_NODE,
    PRW_CHEST_GUARDED_BY_TURRET,
    PRW_CHEST_VENTS,
    PRW_CHEST_STATUE,
    PRW_DGRD,
    PRW_FLOWER,
    PRW_CHEST_RIGHT_EXIT,

    LYR_CHEST_LEFT_POOL_MIDDLE_1,
    LYR_CHEST_LEFT_POOL_MIDDLE_2,
    LYR_CHEST_LEFT_POOL_RIGHT,
    LYR_CHEST_LEFT_POOL_ABOVE,
    LYR_JI_MUSIC,
    LYR_CHEST_TOWER,
    LYR_CHEST_TOWER_ROOM,
    LYR_CHEST_ABOVE_NODE,
    LYR_CHEST_STATUES_ROOM,
    LYR_LAKE_STELE,
    LYR_CHEST_NYMPH_ROOM,
    LYR_CHEST_RIGHT_EXIT,
    
    GH_CHEST_NYMPH_ROPE,
    GH_WATER_REPORT,
    GH_SHANHAI_CHIP,
    GH_CHEST_RIGHT_HANGING_POOL,
    GH_UPPER_LEVEL_FOLIAGE,
    GH_UNDERWATER_VASE,
    GH_CHEST_LEFT_HANGING_POOL,
    GH_MUTATED_CROPS,
    GH_DROP_SHUIGUI,
    
    WOS_CHEST_HIGH_PLATFORMS_RIGHT,
    WOS_CHEST_HIGH_PLATFORMS_LEFT,
    WOS_FLOWER,
    WOS_DGRD,
    WOS_PIPELINE_PANEL,
    WOS_SHAFT_NEAR_NODE,
    
    AH_CHEST_GOUMANG_1,
    AH_CHEST_GOUMANG_2,
    AH_GOUMANG_SANCTUM,
    
    YC_CHEST_UPPER_CAVES_TOP_LEFT,
    YC_CHEST_UPPER_CAVES_BOTTOM_LEFT,
    YC_FARMLAND_MARKINGS,
    YC_ABOVE_MARKINGS,
    YC_CHEST_MIDDLE_CAVE,
    YC_CAVE_EGG,
    YC_NEAR_NODE,

    FGH_CHEST_RIGHT_SHIELD_ORB,
    FGH_CHEST_NEAR_NODE,
    FGH_CHEST_MIDDLE_SHIELD_ORB,
    FGH_HAMMER_FLOWER,
    FGH_HAMMER_BROS,
    FGH_MONITOR,
    FGH_CHIYOU_BRIDGE,
    FGH_CHEST_ABOVE_NODE,
    FGH_CHEST_RIGHT_ELEVATOR,
    FGH_PLATFORM_ROOM_BALL,
    FGH_PLATFORM_ROOM_DGRD,
    FGH_PLATFORM_ROOM_FLOWER,
    
    FU_CHEST_UPPER_RIGHT_EXIT,
    FU_DROP_KUIYAN,
    FU_CHEST_LOWER_ELEVATOR,
    FU_CHEST_STATUES,
    FU_DROP_SHANHAI,
    FU_EVACUATION_NOTICE,
    FU_CHEST_ABOVE_NODE,
    FU_CHEST_BELOW_LEFT_EXIT,
    FU_CHEST_BEHIND_BOXES,
    
    P_SCROLL_LOWER_RIGHT,
    P_CHEST_LOWER_LEFT,
    P_CHEST_ABOVE_STAIRS,
    P_CHEST_NEAR_NODE,
    P_SCROLL_UPPER_LEFT,
    P_CHESTS_BEFORE_KANGHUI_1,
    P_CHESTS_BEFORE_KANGHUI_2,
    P_CHESTS_BEFORE_KANGHUI_3,
    P_DROP_KANGHUI,
    
    FMR_CHEST_RIGHT_ELEVATOR,
    FMR_CHEST_ELEVATOR_TURRET,
    FMR_CHEST_LEFT_ELEVATOR,
    FMR_CHEST_WALKING_GREEN_PILLAR,
    FMR_CHEST_TURRET_GREEN_PILLAR,
    FMR_CHEST_NEAR_MOVING_PLATFORMS,
    FMR_FLOWER,

    FPA_CHEST_VENTS_LOWER_LEFT,
    FPA_CHEST_NEAR_JIEQUAN_STATUE,
    FPA_CHEST_TRIPLE_GUARD_SPAWNER,
    FPA_CHEST_VENTS_UPPER_LEFT,
    FPA_PRODUCTION_STATION,
    FPA_VENTS_BELOW_PRODUCTION,
    FPA_CHEST_BELOW_DOUBLE_SNIPER,
    FPA_PHARMACY_PANEL,
    FPA_PHARMACY_BALL,
    FPA_CHEST_RIGHT_FIRE_ZONE_BOTTOM,
    FPA_DEFEAT_SHANHAI,
    FPA_RIGHT_FIRE_ZONE_TOP,
    FPA_CHEST_PAST_WUQIANG,
    FPA_DROP_WUQIANG,
    
    SH_HAOTIAN_SPHERE,
    SH_CHEST_RIGHT,
    SH_JIEQUAN_FLOWER,
    SH_CHEST_LEFT,
    SH_JIEQUAN_SANCTUM,
    
    AM_CHEST_ABOVE_LEFT_EXIT,
    AM_DROP_YINYUE,
    AM_CHEST_NEAR_NODE,
    AM_CHEST_WALKING,
    AM_FLOWER,
    AM_CHEST_AFTER_FLOWER_1,
    AM_CHEST_AFTER_FLOWER_2,
    
    UC_INSCRIPTION,
    //UC_DROP_YELLOW_SNAKE, // post-all-poisons
    UC_NOTE,
    UC_SCROLL,
    
    LHP_DROP_LIEGUAN,
    
    GD_SIGN,
    GD_FLOWER,
    GD_SHAMAN,
    
    OW_SHANHAI_CHIP,
    OW_CHEST_ABOVE_SOL_STATUE,
    OW_CHEST_CRUSHER_GAUNTLET,
    OW_CHEST_GAUNTLET_ROOM,
    OW_CHEST_VENTS_ROBOT,
    OW_DROP_VENT_CRATE,
    OW_DATABASE,

    IW_CHEST_STATUES,
    IW_CHEST_WALKING,
    IW_DROP_TIEYAN,
    IW_NYMPH_PUZZLE_ROOM,
    IW_DGRD,
    IW_FLOWER,

    BR_CONSOLE,
    BR_CHEST_NEAR_CONSOLE,
    BR_GAUNTLET_1_CHEST,
    BR_GAUNTLET_2_CHEST,
    BR_GAUNTLET_2_CHEST_LASERS,
    BR_GAUNTLET_2_CHEST_BEETLE,
    BR_VAULT_CHEST_1,
    BR_VAULT_CHEST_2,
    BR_VAULT_SCROLL,
    BR_VAULT_CHEST_3,
    BR_VAULT_CHEST_4,

    YH_FLOWER,
    YH_VITAL_SANCTUM,

    GOSY_PAINTING,
    GOSY_COFFIN,
    GOSY_CHEST_TREASURE_1,
    GOSY_CHEST_TREASURE_2,
    GOSY_CHEST_TREASURE_3,
    GOSY_CHEST_TREASURE_4,
    GOSY_CHEST_TREASURE_5,
    GOSY_CHEST_TREASURE_6,
    GOSY_CHEST_LOWER_PORTAL,
    GOSY_CHEST_CAVES_LOWER_LEFT,
    GOSY_CHEST_MIDDLE_PORTAL_ALCOVE,
    GOSY_CHEST_MIDDLE_PORTAL_POOL,
    GOSY_CHEST_UPPER_RIGHT_PORTAL_1,
    GOSY_CHEST_UPPER_RIGHT_PORTAL_2,
    GOSY_CHEST_UPPER_RIGHT_PORTAL_3,
    GOSY_CHEST_NEAR_GREENHOUSE_ROOF,
    GOSY_CHEST_GREENHOUSE,
    GOSY_CHEST_NEAR_UPPER_RIGHT_EXIT,

    GOSE_CHEST_LURKER_GUARDED,
    GOSE_CHEST_PHANTOM_GUARDED,
    GOSE_CHEST_SPIKE_HALL_UPPER_RIGHT,
    GOSE_CHEST_SPIKE_HALL_UPPER_LEFT,
    GOSE_CHEST_LURKERS_UNDER_WALKWAY,
    GOSE_CHEST_PORTAL_BELOW_NODE,
    GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_1,
    GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_2,
    GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_3,
    GOSE_CHEST_ABOVE_YINJIFU_TOMB,
    GOSE_YINJIFU_MURAL,
    GOSE_CARVINGS,
    GOSE_CHEST_OUTSIDE_GUIGUZI_TOMB,
    GOSE_GUIGUZI_MURAL,
    
    GOSW_CHEST_ABOVE_ELEVATOR,
    GOSW_CHEST_TOP_MIDDLE_ROOM,
    GOSW_CHEST_BELOW_WESTERN_CLIFFS,
    GOSW_CHEST_BELOW_LUYAN_TOMB,
    GOSW_LUYAN_MURAL,
    GOSW_YINJIFU_FLOWER,
    GOSW_GUIGUZI_FLOWER,
    GOSW_LUYAN_FLOWER,
    GOSW_CHEST_LEAR_GRAVE,
    
    ASP_JI,
    ASP_VITAL_SANCTUM,
    ASP_SHANHAI_CHIP,

    ST_CHEST_LOWER_ELEVATOR,
    ST_CHEST_WALL_CLIMB,
    ST_CHEST_HAZARDS,
    ST_CHEST_ROPES_BELOW_NODE,
    ST_FLOWER_STOWAWAY,
    ST_STOWAWAY,
    ST_CHEST_NODE,
    ST_CHEST_PINK_WATER,
    
    EDP_CHEST_ABOVE_TRANSPORTER,
    EDP_CHEST_STATUES,
    EDP_CHEST_NODE_ALCOVE_1,
    EDP_CHEST_NODE_ALCOVE_2,
    EDP_CHEST_SLIDER_RIGHT_OF_NODE,
    EDP_CHEST_LEFT_PINK_POOL,
    EDP_CHEST_FAR_LEFT,
    EDP_DROP_TIANSHOU,
    EDP_WATER_TOWER,
    EDP_CHEST_LASERS_1,
    EDP_CHEST_LASERS_2,
    EDP_CHEST_HALLWAY_TURRET,

    EDLA_CHEST_RIGHT_ELEVATOR,
    EDLA_SHANHAI_CHIP,
    EDLA_BULLETIN_BOARD,
    EDLA_CHEST_WALKING_EAST_BUILDING,
    EDLA_CHEST_ABOVE_NODE,
    EDLA_CHEST_MIDDLE_ELEVATOR,
    EDLA_DROP_MUTANT_MIDDLE_ELEVATOR,
    EDLA_VITAL_SANCTUM,
    EDLA_CHEST_FIVE_BELLS_UPPER_RIGHT,
    EDLA_DROP_MUTANT_FIVE_BELLS,
    EDLA_CHEST_THEATER_RIGHT,
    EDLA_CHEST_THEATER_LEFT,
    EDLA_CHEST_EAST_BUILDING_ROOF,
    EDLA_DGRD,
    EDLA_FLOWER,
    EDLA_CHEST_BACKER_1,
    EDLA_CHEST_BACKER_2,
    EDLA_CHEST_BACKER_3,
    
    EDS_MONITORING_PANEL,
    EDS_CHEST_EAST_ROOF_WALKING,
    EDS_CHEST_EAST_ROOF_RIGHT,
    EDS_CHEST_EAST_ROOF_ABOVE,
    EDS_DROP_MUTANT_BELOW_HALL,
    EDS_DROP_MUTANT_BELOW_NODE,
    EDS_ITEM_BOTTOM_LEFT,
    EDS_ITEM_ABOVE_BOTTOM_LEFT,
    
    NH_VITAL_SANCTUM,
    NH_CHEST_AFTER_FENGS,
    NH_NUWA_FLOWER,
    
    TRC_DROP_SHANHAI,
    TRC_CHEST_SPIKES,
    TRC_DROP_BOOKSHELF,
    TRC_CHEST_CHIEN_ARENA,
    TRC_CHEST_ABOVE_SOL_STATUE,
    TRC_ITEM_SICKBAY_VENTS,
    TRC_DGRD,
    TRC_DROP_MUTANT_HIGHEST,
    TRC_DROP_MUTANT_XINGTIAN,
    TRC_CHEST_MUTANT_BARRIER,
    TRC_DROP_MUTANT_SOL_STATUE,
    TRC_CHEST_DG_HQ_CHEST_NEAR_SCREEN,
    TRC_CHEST_DG_HQ_MUTANT_NEAR_SCREEN,
    TRC_DG_HQ_SCREEN,
    // post-PonR
    //TRC_CHEST_DG_HQ_LOWER_1,
    //TRC_CHEST_DG_HQ_LOWER_2,
    //TRC_CHEST_DG_HQ_LOWER_3,
    //TRC_CHEST_DG_HQ_LOWER_4,
    //TRC_CHEST_DG_HQ_LOWER_5,
    //TRC_DROP_CHIEN,
    //NKCH_MONITORING_DEVICE,
}

internal class LocationNames {
    public static Dictionary<Location, string> locationNames = new Dictionary<Location, string> {
        { Location.AFM_BREAK_CORPSE, "AF (Monitoring): Break Corpse" },
        { Location.AFM_CHEST_UPPER_RIGHT, "AF (Monitoring): Upper Right" },
        { Location.AFM_CHEST_LOWER_VENT, "AF (Monitoring): Lower Vent" },
        { Location.AFM_DB_SURVEILLANCE, "AF (Monitoring): Examine Apeman Surveillance" },

        { Location.AFE_CHEST_UPPER_PAGODA_LEFT, "AF (Elevator): Hidden Atop Upper Level Pagoda (Left Chest)" },
        { Location.AFE_CHEST_UPPER_PAGODA_RIGHT, "AF (Elevator): Hidden Atop Upper Level Pagoda (Right Chest)" },
        { Location.AFE_CHEST_MOVING_BOXES, "AF (Elevator): Moving Boxes" },
        { Location.AFE_CHEST_ELEVATOR, "AF (Elevator): Elevator Shaft" },
        { Location.AFE_DROP_BAICHANG, "AF (Elevator): Defeat Red Tiger Elite: Baichang" },
        { Location.AFE_CHEST_STATUE, "AF (Elevator): Hack Statue" },
        { Location.AFE_CHEST_OVER_HAZARD, "AF (Elevator): Over Electrified Floor" },

        { Location.AFD_CHEST_CRYSTAL_CAVES, "AF (Depths): Crystal Caves" },
        { Location.AFD_DROP_1_SHANGUI, "AF (Depths): Defeat Celestial Spectre: Shangui (1st Reward)" },
        { Location.AFD_DROP_2_SHANGUI, "AF (Depths): Defeat Celestial Spectre: Shangui (2nd Reward)" },
        { Location.AFD_CHEST_BELOW_NODE, "AF (Depths): Below Root Node" },
        { Location.AFD_CHEST_STATUES, "AF (Depths): Hack 3 Statues" },
        { Location.AFD_CHEST_LOWER_LEVEL, "AF (Depths): Lower Level" },
        { Location.AFD_CHEST_UNDER_LOWER_LEFT_EXIT, "AF (Depths): Under Lower Left Exit" },
        { Location.AFD_FLOWER_UNDER_ELEVATOR, "AF (Depths): Tianhou Flower Under Elevator" },
        { Location.AFD_CHEST_UPPER_RIGHT_1, "AF (Depths): Upper Right Chest (1st Reward)" },
        { Location.AFD_CHEST_UPPER_RIGHT_2, "AF (Depths): Upper Right Chest (2nd Reward)" },

        { Location.CH_COUNCIL_SIGN, "Central Hall: Examine Council Sign" },
        { Location.CH_LAUNCH_MEMORIAL, "Central Hall: Examine Launch Memoral" },
        { Location.CH_COUNCIL_TENETS, "Central Hall: Examine Council Tenets" },
        { Location.CH_CHEST_AXEBOT_AND_TURRETS, "Central Hall: Turrets and Double Axe Robot Room" },
        { Location.CH_CHEST_VENTS, "Central Hall: Vents" },

        { Location.FSP_KUAFU_GIFT_1, "FSP: Kuafu's 1st Gift" },
        { Location.FSP_KUAFU_GIFT_2, "FSP: Kuafu's 2nd Gift" },
        { Location.FSP_SHUANSHUAN_MUSIC, "FSP: Decode the Ancient Sheet Music" },
        { Location.FSP_SHUANSHUAN_PORTRAIT, "FSP: Have Yi's Portrait Painted" },
        { Location.FSP_CHIYOU_PORTRAIT, "FSP: Give Yi's Portrait to Chiyou" },
        { Location.FSP_SHENNONG_PBV_QUEST, "FSP: Receive Peach Blossom Village Quest" },
        { Location.FSP_SHUANSHUAN_BOOK, "FSP: Take Shuanshuan's Book" },
        { Location.FSP_CHIYOU_BOOK, "FSP: Give Shuanshuan's Book to Chiyou" },
        //{ Location.FSP_SHENNONG_SNAKE_QUEST, "FSP: Give Venom Sac to Shennong" }, // post-all-poisons
        { Location.FSP_SHUANSHUAN_HIDDEN_POEM, "FSP: Reveal the Kunlun Immortal Portrait's Secret" },
        { Location.FSP_CHEST_HALF_TREE, "FSP: Half-Grown Tree Chest" },
        { Location.FSP_CHEST_FULL_TREE_1, "FSP: Fully Grown Tree 1st Chest" },
        { Location.FSP_CHEST_FULL_TREE_2, "FSP: Fully Grown Tree 2nd Chest" },
        { Location.FSP_MUTANT_QUEST, "FSP: Isolate the Mutant Gene Sequence" },
        { Location.FSP_KUAFU_HOMING_DARTS, "FSP: Give Kuafu the Homing Darts" },
        { Location.FSP_KUAFU_THUNDERBURST_BOMB, "FSP: Give Kuafu the Thunderburst Bomb" },

        { Location.CC_LADY_ETHEREAL, "Cortex Center: Defeat Lady Ethereal" },
        { Location.CC_FLOWER_LADY_ETHEREAL, "Cortex Center: Lady Ethereal's Tianhou Flower" },
        { Location.CC_SHANHAI_CHIP, "Cortex Center: Retrieve Chip From Shanhai 9000" },
        { Location.CC_CHEST_CAVES_UPPER_RIGHT, "Cortex Center: Crystal Caves Upper Right" },
        { Location.CC_CHEST_CAVES_CENTER, "Cortex Center: Crystal Caves Central Chamber" },
        { Location.CC_CHEST_LEFT_EXIT, "Cortex Center: Near Left Exit" },

        { Location.CTH_CHEST_LARGE_ELEVATOR, "CTH: Large Elevator Shaft" },
        { Location.CTH_DROP_YANREN, "CTH: Defeat Red Tiger Elite: Yanren" },
        { Location.CTH_CHEST_SIDE_ROOM, "CTH: Side Room Near Right Exit" },
        { Location.CTH_CHEST_VENTS_LEFT, "CTH: Lower Left Vents" },
        { Location.CTH_ANOMALOUS_ROOT_NODE, "CTH: Examine Panel by Root Node" },
        { Location.CTH_CHEST_STATUES, "CTH: Hack 2 Statues" },

        { Location.PRE_CHEST_AFTER_LASERS, "PR (East): After Lasers" },
        { Location.PRE_CHEST_UNDER_BOX, "PR (East): Under Moving Box" },
        { Location.PRE_CHEST_UPPER_RIGHT, "PR (East): Top Platform in Upper Right Shaft" },
        { Location.PRE_CHEST_STATUE, "PR (East): Hack Statue" },
        { Location.PRE_CHEST_UPPER_LEFT, "PR (East): Upper Left Room" },
        { Location.PRE_DROP_JIAODUAN, "PR (East): Defeat Celestial Guardian: Jiaoduan" },

        { Location.PRC_CHEST_BREAKABLE_WALL_RIGHT, "PR (Central): Breakable Wall Near Right Transporter" },
        { Location.PRC_CHEST_NEAR_MOVING_BOX, "PR (Central): Near Moving Box" },
        { Location.PRC_CHEST_STATUE, "PR (Central): Hack Statue" },
        { Location.PRC_CHEST_GUARDED_BY_BEETLE, "PR (Central): Guarded By Beetle" },
        { Location.PRC_CHEST_RIGHT_OF_PAGODA, "PR (Central): Near One-Way Door Right of Pagoda" },
        { Location.PRC_SHANHAI_CHIP, "PR (Central): Retrieve Chip From Shanhai 9000" },
        { Location.PRC_RHIZOMATIC_ENERGY_METER, "PR (Central): Examine Energy Meter" },
        { Location.PRC_CHEST_LEFT_EXIT, "PR (Central): Near Left Transporter" },
        { Location.PRC_CHEST_LEFT_OF_BRIDGE, "PR (Central): Left of Light Bridge" },

        { Location.RP_CONTROL_PANEL, "Examine Radiant Pagoda Control Panel" },
        { Location.RP_DROP_YINGZHAO, "Defeat General Yingzhao" },
        { Location.RP_KUAFU_SANCTUM, "Kuafu's Vital Sanctum" },

        { Location.PRW_CHEST_BELOW_NODE, "PR (West): Below Root Node" },
        { Location.PRW_CHEST_GUARDED_BY_TURRET, "PR (West): Guarded By Turret" },
        { Location.PRW_CHEST_VENTS, "PR (West): Vents" },
        { Location.PRW_CHEST_STATUE, "PR (West): Hack Statue" },
        { Location.PRW_DGRD, "PR (West): Dusk Guardian Recording Device" },
        { Location.PRW_FLOWER, "PR (West): Tianhou Flower" },
        { Location.PRW_CHEST_RIGHT_EXIT, "PR (West): Near Right Transporter" },

        { Location.LYR_CHEST_LEFT_POOL_MIDDLE_1, "LYR: First Chest in Leftmost Pool" },
        { Location.LYR_CHEST_LEFT_POOL_MIDDLE_2, "LYR: Second Chest in Leftmost Pool" },
        { Location.LYR_CHEST_LEFT_POOL_RIGHT, "LYR: Right of Leftmost Pool" },
        { Location.LYR_CHEST_LEFT_POOL_ABOVE, "LYR: Above Leftmost Pool" },
        { Location.LYR_JI_MUSIC, "LYR: Hear Ji Reminisce About Daybreak Tower" },
        { Location.LYR_CHEST_TOWER, "LYR: Daybreak Tower Chest" },
        { Location.LYR_CHEST_TOWER_ROOM, "LYR: Daybreak Tower Bell Puzzle" },
        { Location.LYR_CHEST_ABOVE_NODE, "LYR: Above Root Node" },
        { Location.LYR_CHEST_STATUES_ROOM, "LYR: Statue Hack Room Near Root Node" },
        { Location.LYR_LAKE_STELE, "LYR: Examine Stele" },
        { Location.LYR_CHEST_NYMPH_ROOM, "LYR: Nymph Puzzle Room Near Stele" },
        { Location.LYR_CHEST_RIGHT_EXIT, "LYR: Ropes Near Right Exit" },

        { Location.GH_CHEST_NYMPH_ROPE, "Greenhouse: Hackable Rope Near Wreckage" },
        { Location.GH_WATER_REPORT, "Greenhouse: Examine Water Report" },
        { Location.GH_SHANHAI_CHIP, "Greenhouse: Retrieve Chip From Shanhai 9000" },
        { Location.GH_CHEST_RIGHT_HANGING_POOL, "Greenhouse: Near Rightmost Hanging Pool" },
        { Location.GH_UPPER_LEVEL_FOLIAGE, "Greenhouse: Hidden in Upper Level Foliage" },
        { Location.GH_UNDERWATER_VASE, "Greenhouse: Vase In Water Above Root Node" },
        { Location.GH_CHEST_LEFT_HANGING_POOL, "Greenhouse: In Leftmost Hanging Pool" },
        { Location.GH_MUTATED_CROPS, "Greenhouse: Examine Mutated Crops" },
        { Location.GH_DROP_SHUIGUI, "Greenhouse: Defeat Celestial Spectre: Shuigui" },

        { Location.WOS_CHEST_HIGH_PLATFORMS_RIGHT, "W&OS: High Platforms Right of Center" },
        { Location.WOS_CHEST_HIGH_PLATFORMS_LEFT, "W&OS: High Platforms Left of Center" },
        { Location.WOS_FLOWER, "W&OS: Tianhou Flower" },
        { Location.WOS_DGRD, "W&OS: Dusk Guardian Recording Device" },
        { Location.WOS_PIPELINE_PANEL, "W&OS: Examine Pipeline Panel" },
        { Location.WOS_SHAFT_NEAR_NODE, "W&OS: Climb Elevator Shaft By Root Node" },

        { Location.AH_CHEST_GOUMANG_1, "Chest After Goumang (1st Reward)" },
        { Location.AH_CHEST_GOUMANG_2, "Chest After Goumang (2nd Reward)" },
        { Location.AH_GOUMANG_SANCTUM, "Goumang's Vital Sanctum" },

        { Location.YC_CHEST_UPPER_CAVES_TOP_LEFT, "Yinglong Canal: Top Left of Upper Caves" },
        { Location.YC_CHEST_UPPER_CAVES_BOTTOM_LEFT, "Yinglong Canal: Bottom Left of Upper Caves" },
        { Location.YC_FARMLAND_MARKINGS, "Yinglong Canal: Examine Farmland Markings" },
        { Location.YC_ABOVE_MARKINGS, "Yinglong Canal: Climbing Puzzle Above Farmland Markings" },
        { Location.YC_CHEST_MIDDLE_CAVE, "Yinglong Canal: Between Egg Cave and Farmland Markings" },
        { Location.YC_CAVE_EGG, "Yinglong Canal: Break Center Yinglong Egg" },
        { Location.YC_NEAR_NODE, "Yinglong Canal: Near Root Node" },

        { Location.FGH_CHEST_RIGHT_SHIELD_ORB, "Factory (GH): Near Rightmost Shield Orb" },
        { Location.FGH_CHEST_NEAR_NODE, "Factory (GH): Near Root Node" },
        { Location.FGH_CHEST_MIDDLE_SHIELD_ORB, "Factory (GH): Near Middle Shield Orb" },
        { Location.FGH_HAMMER_FLOWER, "Factory (GH): Tianhou Flower Below Hammers" },
        { Location.FGH_HAMMER_BROS, "Factory (GH): Break the Hammers" },
        { Location.FGH_MONITOR, "Factory (GH): Examine Furnance Monitor" },
        { Location.FGH_CHIYOU_BRIDGE, "Factory (GH): Raise the Bridge for Chiyou" },
        { Location.FGH_CHEST_ABOVE_NODE, "Factory (GH): Roof Above Root Node" },
        { Location.FGH_CHEST_RIGHT_ELEVATOR, "Factory (GH): Near Right Elevator" },
        { Location.FGH_PLATFORM_ROOM_BALL, "Factory (GH): Ball Drop in Platform Puzzle Room" },
        { Location.FGH_PLATFORM_ROOM_DGRD, "Factory (GH): Recording Device in Platform Puzzle Room" },
        { Location.FGH_PLATFORM_ROOM_FLOWER, "Factory (GH): Tianhou Flower in Platform Puzzle Room" },

        { Location.FU_CHEST_UPPER_RIGHT_EXIT, "Factory (U): Near Upper Right Exit" },
        { Location.FU_DROP_KUIYAN, "Factory (U): Defeat Red Tiger Elite: Kuiyan" },
        { Location.FU_CHEST_LOWER_ELEVATOR, "Factory (U): Near Lower Elevator" },
        { Location.FU_CHEST_STATUES, "Factory (U): Hack 2 Statues" },
        { Location.FU_DROP_SHANHAI, "Factory (U): Find Broken Shanhai 9000" },
        { Location.FU_EVACUATION_NOTICE, "Factory (U): Examine Evacuation Notice" },
        { Location.FU_CHEST_ABOVE_NODE, "Factory (U): Above Root Node" },
        { Location.FU_CHEST_BELOW_LEFT_EXIT, "Factory (U): Below Walkway to Left Exit" },
        { Location.FU_CHEST_BEHIND_BOXES, "Factory (U): Behind Moving Boxes" },

        { Location.P_SCROLL_LOWER_RIGHT, "Prison: Examine Scroll in Lower Right Cell" },
        { Location.P_CHEST_LOWER_LEFT, "Prison: Lower Left Cell" },
        { Location.P_CHEST_ABOVE_STAIRS, "Prison: Above Stairs on Second Level" },
        { Location.P_CHEST_NEAR_NODE, "Prison: Near Root Node" },
        { Location.P_SCROLL_UPPER_LEFT, "Prison: Examine Scroll in Upper Left Cell" },
        { Location.P_CHESTS_BEFORE_KANGHUI_1, "Prison: 1st Chest in Room Before Kanghui" },
        { Location.P_CHESTS_BEFORE_KANGHUI_2, "Prison: 2nd Chest in Room Before Kanghui" },
        { Location.P_CHESTS_BEFORE_KANGHUI_3, "Prison: 3rd Chest in Room Before Kanghui" },
        { Location.P_DROP_KANGHUI, "Prison: Defeat Kanghui" },

        { Location.FMR_CHEST_RIGHT_ELEVATOR, "Factory (MR): Below Right Elevator" },
        { Location.FMR_CHEST_ELEVATOR_TURRET, "Factory (MR): Break Turret Below Elevator" },
        { Location.FMR_CHEST_LEFT_ELEVATOR, "Factory (MR): Above Left Elevator" },
        { Location.FMR_CHEST_WALKING_GREEN_PILLAR, "Factory (MR): Walking Chest Above Green Pillar" },
        { Location.FMR_CHEST_TURRET_GREEN_PILLAR, "Factory (MR): Behind Turret Near Green Pillar" },
        { Location.FMR_CHEST_NEAR_MOVING_PLATFORMS, "Factory (MR): Near Moving Platforms" },
        { Location.FMR_FLOWER, "Factory (MR): Tianhou Flower Above Right Elevator" },

        { Location.FPA_CHEST_VENTS_LOWER_LEFT, "Factory (PA): Behind Fire in Lower Left Vents" },
        { Location.FPA_CHEST_NEAR_JIEQUAN_STATUE, "Factory (PA): Hallway Near Jiequan Statue" },
        { Location.FPA_CHEST_TRIPLE_GUARD_SPAWNER, "Factory (PA): Three Guard Spawners" },
        { Location.FPA_CHEST_VENTS_UPPER_LEFT, "Factory (PA): Behind Hack in Upper Left Vents" },
        { Location.FPA_PRODUCTION_STATION, "Factory (PA): Examine Production Station" },
        { Location.FPA_VENTS_BELOW_PRODUCTION, "Factory (PA): Vents Below Production Station" },
        { Location.FPA_CHEST_BELOW_DOUBLE_SNIPER, "Factory (PA): Below Double Sniper Zone" },
        { Location.FPA_PHARMACY_PANEL, "Factory (PA): Examine Pharmacy Panel" },
        { Location.FPA_PHARMACY_BALL, "Factory (PA): Ball Drop in Pharmacy" },
        { Location.FPA_CHEST_RIGHT_FIRE_ZONE_BOTTOM, "Factory (PA): Bottom of Right Fire Zone" },
        { Location.FPA_DEFEAT_SHANHAI, "Factory (PA): Defeat Shanhai 9000" },
        { Location.FPA_RIGHT_FIRE_ZONE_TOP, "Factory (PA): Top of Right Fire Zone" },
        { Location.FPA_CHEST_PAST_WUQIANG, "Factory (PA): Far Side of Wuqiang Arena" },
        { Location.FPA_DROP_WUQIANG, "Factory (PA): Defeat Celestial Sentinel: Wuqiang" },

        { Location.SH_HAOTIAN_SPHERE, "Examine Sphere Before Jiequan" },
        { Location.SH_CHEST_RIGHT, "Chest Before Jiequan" },
        { Location.SH_JIEQUAN_FLOWER, "Jiequan's Tianhou Flower" },
        { Location.SH_CHEST_LEFT, "Chest After Jiequan" },
        { Location.SH_JIEQUAN_SANCTUM, "Jiequan's Vital Sanctum" },

        { Location.AM_CHEST_ABOVE_LEFT_EXIT, "AM: Above Left Exit" },
        { Location.AM_DROP_YINYUE, "AM: Defeat Celestial Warden: Yinyue" },
        { Location.AM_CHEST_NEAR_NODE, "AM: Near Root Node" },
        { Location.AM_CHEST_WALKING, "AM: Walking Chest" },
        { Location.AM_FLOWER, "AM: Tianhou Flower" },
        { Location.AM_CHEST_AFTER_FLOWER_1, "AM: 1st Chest After Flower" },
        { Location.AM_CHEST_AFTER_FLOWER_2, "AM: 2nd Chest After Flower" },

        { Location.UC_INSCRIPTION, "UC: Examine Stone Inscription" },
        //{ Location.UC_DROP_YELLOW_SNAKE, "UC: Defeat the Yellow Dragonsnake" }, // post-all-poisons
        { Location.UC_NOTE, "UC: Examine Note" },
        { Location.UC_SCROLL, "UC: Examine Scroll" },

        { Location.LHP_DROP_LIEGUAN, "Village: Defeat Red Tiger Elite: Lieguan" },

        { Location.GD_SIGN, "Galactic Dock: Examine Sign" },
        { Location.GD_FLOWER, "Galactic Dock: Tianhou Flower" },
        { Location.GD_SHAMAN, "Galactic Dock: Shaman's Gift" },

        { Location.OW_SHANHAI_CHIP, "OW: Retrieve Chip From Shanhai 9000" },
        { Location.OW_CHEST_ABOVE_SOL_STATUE, "OW: Above Sol Statue" },
        { Location.OW_CHEST_CRUSHER_GAUNTLET, "OW: Crusher Gauntlet" },
        { Location.OW_CHEST_GAUNTLET_ROOM, "OW: Enemy Gauntlet Room" },
        { Location.OW_CHEST_VENTS_ROBOT, "OW: Vents Above Robot" },
        { Location.OW_DROP_VENT_CRATE, "OW: Inside Crate Dropped From Vent Hack" },
        { Location.OW_DATABASE, "OW: Examine Warehouse Database" },

        { Location.IW_CHEST_STATUES, "IW: Hack 3 Statues" },
        { Location.IW_CHEST_WALKING, "IW: Shielded Walking Chest" },
        { Location.IW_DROP_TIEYAN, "IW: Defeat Celestial Enforcer: Tieyan" },
        { Location.IW_NYMPH_PUZZLE_ROOM, "IW: Nymph Puzzle Room" },
        { Location.IW_DGRD, "IW: Dusk Guardian Recording Device" },
        { Location.IW_FLOWER, "IW: Tianhou Flower" },

        { Location.BR_CONSOLE, "BR: Examine Console" },
        { Location.BR_CHEST_NEAR_CONSOLE, "BR: Near Xingtian Console" },
        { Location.BR_GAUNTLET_1_CHEST, "BR: Gauntlet Part 1 Chest" },
        { Location.BR_GAUNTLET_2_CHEST, "BR: Gauntlet Part 2 First Chest" },
        { Location.BR_GAUNTLET_2_CHEST_LASERS, "BR: Gauntlet Part 2 Chest Past Lasers" },
        { Location.BR_GAUNTLET_2_CHEST_BEETLE, "BR: Gauntlet Part 2 Chest Past Beetle" },
        { Location.BR_VAULT_CHEST_1, "BR: Vault 1st Chest" },
        { Location.BR_VAULT_CHEST_2, "BR: Vault 2nd Chest" },
        { Location.BR_VAULT_SCROLL, "BR: Examine Vault Scroll" },
        { Location.BR_VAULT_CHEST_3, "BR: Vault 3rd Chest" },
        { Location.BR_VAULT_CHEST_4, "BR: Vault 4th Chest" },

        { Location.YH_FLOWER, "Yanlao's Tianhou Flower" },
        { Location.YH_VITAL_SANCTUM, "Yanlao's Vital Sanctum" },

        { Location.GOSY_PAINTING, "GoS (Entry): Examine Painting" },
        { Location.GOSY_COFFIN, "GoS (Entry): Examine Coffin" },
        { Location.GOSY_CHEST_TREASURE_1, "GoS (Entry): Poem Treasure 1st Chest" },
        { Location.GOSY_CHEST_TREASURE_2, "GoS (Entry): Poem Treasure 2nd Chest" },
        { Location.GOSY_CHEST_TREASURE_3, "GoS (Entry): Poem Treasure 3rd Chest" },
        { Location.GOSY_CHEST_TREASURE_4, "GoS (Entry): Poem Treasure 4th Chest" },
        { Location.GOSY_CHEST_TREASURE_5, "GoS (Entry): Poem Treasure 5th Chest" },
        { Location.GOSY_CHEST_TREASURE_6, "GoS (Entry): Poem Treasure 6th Chest" },
        { Location.GOSY_CHEST_LOWER_PORTAL, "GoS (Entry): Lower Caves Portal" },
        { Location.GOSY_CHEST_CAVES_LOWER_LEFT, "GoS (Entry): Lower Left Caves" },
        { Location.GOSY_CHEST_MIDDLE_PORTAL_ALCOVE, "GoS (Entry): Alcove Above Middle Caves Portal" },
        { Location.GOSY_CHEST_MIDDLE_PORTAL_POOL, "GoS (Entry): Yellow Pool Above Middle Caves Portal" },
        { Location.GOSY_CHEST_UPPER_RIGHT_PORTAL_1, "GoS (Entry): Upper Right Caves Portal 1st Chest" },
        { Location.GOSY_CHEST_UPPER_RIGHT_PORTAL_2, "GoS (Entry): Upper Right Caves Portal 2nd Chest" },
        { Location.GOSY_CHEST_UPPER_RIGHT_PORTAL_3, "GoS (Entry): Upper Right Caves Portal 3rd Chest" },
        { Location.GOSY_CHEST_NEAR_GREENHOUSE_ROOF, "GoS (Entry): Near Greenhouse Roof" },
        { Location.GOSY_CHEST_GREENHOUSE, "GoS (Entry): Greenhouse Between Elevators" },
        { Location.GOSY_CHEST_NEAR_UPPER_RIGHT_EXIT, "GoS (Entry): Near Upper Right Exit" },

        { Location.GOSE_CHEST_LURKER_GUARDED, "GoS (East): Lurker Near Lower Exit" },
        { Location.GOSE_CHEST_PHANTOM_GUARDED, "GoS (East): Guarded By Phantom Ninja" },
        { Location.GOSE_CHEST_SPIKE_HALL_UPPER_RIGHT, "GoS (East): Spike Ball Hall Upper Right" },
        { Location.GOSE_CHEST_SPIKE_HALL_UPPER_LEFT, "GoS (East): Spike Ball Hall Upper Left" },
        { Location.GOSE_CHEST_LURKERS_UNDER_WALKWAY, "GoS (East): Lurkers Under Tunnel Walkway" },
        { Location.GOSE_CHEST_PORTAL_BELOW_NODE, "GoS (East): Portal Below Root Node" },
        { Location.GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_1, "GoS (East): Alcove Between Tombs 1st Chest" },
        { Location.GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_2, "GoS (East): Alcove Between Tombs 2nd Chest" },
        { Location.GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_3, "GoS (East): Alcove Between Tombs 3rd Chest" },
        { Location.GOSE_CHEST_ABOVE_YINJIFU_TOMB, "GoS (East): Upper Right of Room Above Yin Jifu's Tomb" },
        { Location.GOSE_YINJIFU_MURAL, "GoS (East): Examine Mural in Yin Jifu's Tomb" },
        { Location.GOSE_CARVINGS, "GoS (East): Examine Stone Carvings" },
        { Location.GOSE_CHEST_OUTSIDE_GUIGUZI_TOMB, "GoS (East): Outside Guiguzi's Tomb" },
        { Location.GOSE_GUIGUZI_MURAL, "GoS (East): Examine Mural in Guiguzi's Tomb" },

        { Location.GOSW_CHEST_ABOVE_ELEVATOR, "GoS (West): Above Elevator" },
        { Location.GOSW_CHEST_TOP_MIDDLE_ROOM, "GoS (West): Platforms In Top Middle Room" },
        { Location.GOSW_CHEST_BELOW_WESTERN_CLIFFS, "GoS (West): Below Western Cliffs" },
        { Location.GOSW_CHEST_BELOW_LUYAN_TOMB, "GoS (West): Below Luyan's Tomb" },
        { Location.GOSW_LUYAN_MURAL, "GoS (West): Examine Mural in Luyan's Tomb" },
        { Location.GOSW_YINJIFU_FLOWER, "GoS (West): Yin Jifu's Tianhou Flower" },
        { Location.GOSW_GUIGUZI_FLOWER, "GoS (West): Guiguzi's Tianhou Flower" },
        { Location.GOSW_LUYAN_FLOWER, "GoS (West): Luyan's Tianhou Flower" },
        { Location.GOSW_CHEST_LEAR_GRAVE, "GoS (West): Chest in Lear's Grave" },

        { Location.ASP_JI, "Examine Ji" },
        { Location.ASP_VITAL_SANCTUM, "Ji's Vital Sanctum" },
        { Location.ASP_SHANHAI_CHIP, "Retrieve Chip From Shanhai 1000" },

        { Location.ST_CHEST_LOWER_ELEVATOR, "Sky Tower: Above Lower Elevator" },
        { Location.ST_CHEST_WALL_CLIMB, "Sky Tower: Wall Climb Platform" },
        { Location.ST_CHEST_HAZARDS, "Sky Tower: Surrounded By Hazards" },
        { Location.ST_CHEST_ROPES_BELOW_NODE, "Sky Tower: Ropes Below Root Node" },
        { Location.ST_FLOWER_STOWAWAY, "Sky Tower: Stowaway's Tianhou Flower" },
        { Location.ST_STOWAWAY, "Sky Tower: Examine Stowaway's Belongings" },
        { Location.ST_CHEST_NODE, "Sky Tower: By Root Node" },
        { Location.ST_CHEST_PINK_WATER, "Sky Tower: Pink Water Near Root Node" },

        { Location.EDP_CHEST_ABOVE_TRANSPORTER, "ED (Passages): Above Transporter" },
        { Location.EDP_CHEST_STATUES, "ED (Passages): Hack 2 Statues" },
        { Location.EDP_CHEST_NODE_ALCOVE_1, "ED (Passages): Alcove Right of Root Node 1st Chest" },
        { Location.EDP_CHEST_NODE_ALCOVE_2, "ED (Passages): Alcove Right of Root Node 2nd Chest" },
        { Location.EDP_CHEST_SLIDER_RIGHT_OF_NODE, "ED (Passages): Slider Right Of Root Node" },
        { Location.EDP_CHEST_LEFT_PINK_POOL, "ED (Passages): Above Pink Pool Left Of Root Node" },
        { Location.EDP_CHEST_FAR_LEFT, "ED (Passages): Far Left Rooms" },
        { Location.EDP_DROP_TIANSHOU, "ED (Passages): Defeat The Great Miner: Tianshou" },
        { Location.EDP_WATER_TOWER, "ED (Passages): Examine Water Tower" },
        { Location.EDP_CHEST_LASERS_1, "ED (Passages): Alcove Between Lasers" },
        { Location.EDP_CHEST_LASERS_2, "ED (Passages): Covered By Lasers" },
        { Location.EDP_CHEST_HALLWAY_TURRET, "ED (Passages): Above Upper Hallway Turret" },

        { Location.EDLA_CHEST_RIGHT_ELEVATOR, "ED (Living Area): Above Right Elevator" },
        { Location.EDLA_SHANHAI_CHIP, "ED (Living Area): Retrieve Chip From Shanhai 9000" },
        { Location.EDLA_BULLETIN_BOARD, "ED (Living Area): Examine Bulletin Board" },
        { Location.EDLA_CHEST_WALKING_EAST_BUILDING, "ED (Living Area): Walking Chest In East Building" },
        { Location.EDLA_CHEST_ABOVE_NODE, "ED (Living Area): Above Root Node" },
        { Location.EDLA_CHEST_MIDDLE_ELEVATOR, "ED (Living Area): Middle Elevator Chest Room" },
        { Location.EDLA_DROP_MUTANT_MIDDLE_ELEVATOR, "ED (Living Area): Defeat Mutated Zouyan in Middle Elevator Room" },
        { Location.EDLA_VITAL_SANCTUM, "ED (Living Area): Fuxi's Vital Sanctum" },
        { Location.EDLA_CHEST_FIVE_BELLS_UPPER_RIGHT, "ED (Living Area): Upper Right of Five Bells Room" },
        { Location.EDLA_DROP_MUTANT_FIVE_BELLS, "ED (Living Area): Defeat Mutated A-Shou in Five Bells Room" },
        { Location.EDLA_CHEST_THEATER_RIGHT, "ED (Living Area): Roof Right of Opera Theater" },
        { Location.EDLA_CHEST_THEATER_LEFT, "ED (Living Area): Roof Left of Opera Theater" },
        { Location.EDLA_CHEST_EAST_BUILDING_ROOF, "ED (Living Area): East Building Rooftops" },
        { Location.EDLA_DGRD, "ED (Living Area): Dusk Guardian Recording Device" },
        { Location.EDLA_FLOWER, "ED (Living Area): Tianhou Flower" },
        { Location.EDLA_CHEST_BACKER_1, "ED (Living Area): Backer Room 1st Chest" },
        { Location.EDLA_CHEST_BACKER_2, "ED (Living Area): Backer Room 2nd Chest" },
        { Location.EDLA_CHEST_BACKER_3, "ED (Living Area): Backer Room 3rd Chest" },

        { Location.EDS_MONITORING_PANEL, "ED (Sanctum): Examine Monitoring Panel" },
        { Location.EDS_CHEST_EAST_ROOF_WALKING, "ED (Sanctum): East Rooftop Walking Chest" },
        { Location.EDS_CHEST_EAST_ROOF_RIGHT, "ED (Sanctum): Right Of East Rooftop" },
        { Location.EDS_CHEST_EAST_ROOF_ABOVE, "ED (Sanctum): Above East Rooftop" },
        { Location.EDS_DROP_MUTANT_BELOW_HALL, "ED (Sanctum): Defeat Mutated Xipu Below Nobility Hall" },
        { Location.EDS_DROP_MUTANT_BELOW_NODE, "ED (Sanctum): Defeat Mutated Yangfan Below Root Node" },
        { Location.EDS_ITEM_BOTTOM_LEFT, "ED (Sanctum): Lower Left Garden" },
        { Location.EDS_ITEM_ABOVE_BOTTOM_LEFT, "ED (Sanctum): Above Lower Left Garden" },

        { Location.NH_VITAL_SANCTUM, "Nuwa's Vital Sanctum" },
        { Location.NH_CHEST_AFTER_FENGS, "Chest After Fengs" },
        { Location.NH_NUWA_FLOWER, "Nuwa's Tianhou Flower" },

        { Location.TRC_DROP_SHANHAI, "TRC: Find Broken Shanhai 9000" },
        { Location.TRC_CHEST_SPIKES, "TRC: Covered In Spikes" },
        { Location.TRC_DROP_BOOKSHELF, "TRC: Ground Floor Bookshelf" },
        { Location.TRC_CHEST_CHIEN_ARENA, "TRC: Before Chien Arena" },
        { Location.TRC_CHEST_ABOVE_SOL_STATUE, "TRC: Above Sol Statue" },
        { Location.TRC_ITEM_SICKBAY_VENTS, "TRC: Past Spikes In Sickbay Vents" },
        { Location.TRC_DGRD, "TRC: Dusk Guardian Recording Device" },
        { Location.TRC_DROP_MUTANT_HIGHEST, "TRC: Destroy Tendril In The Highest Room" },
        { Location.TRC_DROP_MUTANT_XINGTIAN, "TRC: Defeat Mutated Shiyangyue Near Xingtian Sickbay" },
        { Location.TRC_CHEST_MUTANT_BARRIER, "TRC: Behind Mutant Barrier" },
        { Location.TRC_DROP_MUTANT_SOL_STATUE, "TRC: Destroy Tendril Near Sol Statue" },
        { Location.TRC_CHEST_DG_HQ_CHEST_NEAR_SCREEN, "TRC: Chest Near Dusk Guardian HQ Screen" },
        { Location.TRC_CHEST_DG_HQ_MUTANT_NEAR_SCREEN, "TRC: Destroy Tendril Near Guardian HQ Screen" },
        { Location.TRC_DG_HQ_SCREEN, "TRC: Examine Dusk Guardian HQ Screen" },
        // post-PonR
        //{ Location.TRC_CHEST_DG_HQ_LOWER_1, "TRC: Dusk Guardian HQ Lower Level 1st Chest" },
        //{ Location.TRC_CHEST_DG_HQ_LOWER_2, "TRC: Dusk Guardian HQ Lower Level 2nd Chest" },
        //{ Location.TRC_CHEST_DG_HQ_LOWER_3, "TRC: Dusk Guardian HQ Lower Level 3rd Chest" },
        //{ Location.TRC_CHEST_DG_HQ_LOWER_4, "TRC: Dusk Guardian HQ Lower Level 4th Chest" },
        //{ Location.TRC_CHEST_DG_HQ_LOWER_5, "TRC: Dusk Guardian HQ Lower Level 5th Chest" },
        //{ Location.TRC_DROP_CHIEN, "TRC: Defeat Chien" },
        //{ Location.NKCH_MONITORING_DEVICE, "Control Hub: Examine Monitoring Device" },
    };

    public static Dictionary<string, Location> locationNamesReversed = locationNames.ToDictionary(ln => ln.Value, ln => ln.Key);

    // leave these as null until we load the ids, so any attempt to work with ids before that will fail loudly
    public static Dictionary<long, Location> archipelagoIdToLocation = null;
    public static Dictionary<Location, long> locationToArchipelagoId = null;

    public static void LoadArchipelagoIds(string locationsFileContent) {
        var locationsData = JArray.Parse(locationsFileContent);
        archipelagoIdToLocation = new();
        locationToArchipelagoId = new();
        foreach (var locationData in locationsData) {
            // Skip event locations, since they intentionally don't have ids
            if (locationData["address"].Type == JTokenType.Null) continue;

            var archipelagoId = (long)locationData["address"];
            var name = (string)locationData["name"];

            if (!locationNamesReversed.ContainsKey(name))
                throw new System.Exception($"LoadArchipelagoIds failed: unknown location name {name}");

            var location = locationNamesReversed[name];
            archipelagoIdToLocation.Add(archipelagoId, location);
            locationToArchipelagoId.Add(location, archipelagoId);
        }
    }
}
