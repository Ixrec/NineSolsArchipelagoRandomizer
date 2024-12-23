﻿using ArchipelagoRandomizer;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

public enum Item {
    SealOfKuafu,
    SealOfGoumang,
    SealOfYanlao,
    SealOfJiequan,
    SealOfLadyEthereal,
    SealOfJi,
    SealOfFuxi,
    SealOfNuwa,

    // Vanilla's core progression upgrades
    MysticNymphScoutMode,
    TaiChiKick,
    ChargedStrike,
    AirDash,
    UnboundedCounter,
    CloudLeap,
    SuperMutantBuster,

    // Starting abilities we might take away
    //WallClimb,
    //Grapple,

    // Equipment???
    AzureBow,
    ArrowCloudPiercer,

    // Upgrades
    HerbCatalyst,
    PipeVial,
    TaoFruit,
    GreaterTaoFruit,
    TwinTaoFruit,
    ComputingUnit,
    DarkSteel,

    // Key Items???
    BloodyCrimsonHibiscus,
    FusangAmulet,
    SoulSeveringBlade,
    AncientPenglaiBallad,
    PortraitOfYi,
    AbandonedMinesAccessToken,
    LegendOfThePorkyHeroes,
    YellowDragonsnakeVenomSac,
    YellowDragonsnakeMedicinalBrew,
    PoemHiddenInTheImmortalsPortrait,
    DamagedFusangAmulet,
    JinMedallion,
    MultiToolKit,
    AncientMusicSheet,
    UnknownSeed,
    GMFertilizer,
    NobleRing,
    SwordOfJie,
    HomingDarts,
    FirestormRing,
    ThunderburstBomb,
    AntiqueVinylRecord,
    QiankunBoard,
    RedGuifangClay,
    JisHair,
    PassengerTokenZouyan,
    PassengerTokenAShou,
    PenglaiRecipeCollection,
    PassengerTokenXipu,
    PassengerTokenYangfan,
    PassengerTokenJihai,
    PassengerTokenAimu,
    PassengerTokenShiyangyue,

    CentralCoreChip,
    PowerReservoirChip,
    AgriculturalZoneChip,
    AbandonedMinesChip,
    WarehouseZoneChip,
    TransmutationZoneChip,
    GrottoOfScripturesChip,
    EmpyreanDistrictChip,
    ResearchCenterChip,

    // Jades
    StasisJade,
    BearingJade,
    HarnessForceJade,
    IronSkinJade,
    HedgehogJade,
    PauperJade,
    SteelyJade,
    ImmovableJade,
    SoulReaperJade,
    AvariceJade,
    RevivalJade,
    SwiftDescentJade,
    MobQuellJadeYin,
    MobQuellJadeYang,
    FocusJade,
    SwiftBladeJade,
    BreatherJade,
    QiSwipeJade,
    QiBladeJade,
    DivineHandJade,

    // Poisons
    MedicinalCitrine,
    GoldenYinglongEgg,
    ResidualHair,
    PorcineGem,
    PlantagoFrog,
    Oriander,
    TurtleScorpion,

    // Database entries
    ApemanSurveillanceFootage,
    CouncilDigitalSignage,
    NewKunlunLaunchMemorial,
    CouncilTenets,
    AnomalousRootNode,
    RhizomaticEnergyMeter,
    RadiantPagodaControlPanel,
    DuskGuardianRecordingDevice1,
    LakeYaochiStele,
    YellowWaterReport,
    MutatedCrops,
    DuskGuardianRecordingDevice2,
    WaterSynthesisPipelinePanel,
    JieClanFamilyPrecept,
    TransmutationFurnaceMonitor,
    DuskGuardianRecordingDevice4,
    GuardProductionStation,
    CaveStoneInscription,
    DeadPersonsNote,
    CampScroll,
    WarehouseDatabase,
    DuskGuardianRecordingDevice3,
    AncientWeaponConsole,
    HexachremVaultScroll,
    AncientCavePainting,
    CoffinInscription,
    StoneCarvings,
    SecretMural1,
    SecretMural2,
    SecretMural3,
    StowawaysCorpse,
    EmpyreanBulletinBoard,
    DuskGuardianRecordingDevice5,
    VitalSanctumTowerMonitoringPanel,
    DuskGuardianRecordingDevice6,
    DuskGuardianHeadquartersScreen,
    RootCoreMonitoringDevice,
    FarmlandMarkings,
    EvacuationNoticeForMiners,
    PrisonersBambooScroll1,
    PrisonersBambooScroll2,
    PharmacyPanel,
    HaotianSphereModel,
    GalacticDockSign,
    UndergroundWaterTower,

    // Jin/Money filler
    // TODO: check these are the real values attached to the chests, without the extra bits for intermediate breakage steps
    Jin320,
    Jin110,
    Jin50,
    BasicComponent,
    StandardComponent,
    AdvancedComponent,
}

internal class ItemNames {
    public static Dictionary<Item, string> itemNames = new Dictionary<Item, string> {
        { Item.SealOfKuafu, "Seal of Kuafu" },
        { Item.SealOfGoumang, "Seal of Goumang" },
        { Item.SealOfYanlao, "Seal of Yanlao" },
        { Item.SealOfJiequan, "Seal of Jiequan" },
        { Item.SealOfLadyEthereal, "Seal of Lady Ethereal" },
        { Item.SealOfJi, "Seal of Ji" },
        { Item.SealOfFuxi, "Seal of Fuxi" },
        { Item.SealOfNuwa, "Seal of Nuwa" },

        { Item.MysticNymphScoutMode, "Mystic Nymph: Scout Mode" },
        { Item.TaiChiKick, "Tai-Chi Kick" },
        { Item.ChargedStrike, "Charged Strike" },
        { Item.AirDash, "Air Dash" },
        { Item.UnboundedCounter, "Unbounded Counter" },
        { Item.CloudLeap, "Cloud Leap" },
        { Item.SuperMutantBuster, "Super Mutant Buster" },

        { Item.AzureBow, "Azure Bow" },
        { Item.ArrowCloudPiercer, "Arrow: Cloud Piercer" },

        { Item.HerbCatalyst, "Herb Catalyst" },
        { Item.PipeVial, "Pipe Vial" },
        { Item.TaoFruit, "Tao Fruit" },
        { Item.GreaterTaoFruit, "Greater Tao Fruit" },
        { Item.TwinTaoFruit, "Twin Tao Fruit" },
        { Item.ComputingUnit, "Computing Unit" },
        { Item.DarkSteel, "Dark Steel" },

        { Item.BloodyCrimsonHibiscus, "Bloody Crimson Hibiscus" },
        { Item.FusangAmulet, "Fusang Amulet" },
        { Item.SoulSeveringBlade, "Soul-Severing Blade" },
        { Item.AncientPenglaiBallad, "Ancient Penglai Ballad" },
        { Item.PortraitOfYi, "Portrait of Yi" },
        { Item.AbandonedMinesAccessToken, "Abandoned Mines Access Token" },
        { Item.LegendOfThePorkyHeroes, "Legend of the Porky Heroes" },
        { Item.YellowDragonsnakeVenomSac, "Yellow Dragonsnake Venom Sac" },
        { Item.YellowDragonsnakeMedicinalBrew, "Yellow Dragonsnake Medicinal Brew" },
        { Item.PoemHiddenInTheImmortalsPortrait, "Poem Hidden in the Immortal's Portrait" },
        { Item.DamagedFusangAmulet, "Damaged Fusang Amulet" },
        { Item.JinMedallion, "Jin Medallion" },
        { Item.MultiToolKit, "Multi-tool Kit" },
        { Item.AncientMusicSheet, "Ancient Music Sheet" },
        { Item.UnknownSeed, "Unknown Seed" },
        { Item.GMFertilizer, "GM Fertilizer" },
        { Item.NobleRing, "Noble Ring" },
        { Item.SwordOfJie, "Sword of Jie" },
        { Item.HomingDarts, "Homing Darts" },
        { Item.FirestormRing, "Firestorm Ring" },
        { Item.ThunderburstBomb, "Thunderburst Bomb" },
        { Item.AntiqueVinylRecord, "Antique Vinyl Record" },
        { Item.QiankunBoard, "Qiankun Board" },
        { Item.RedGuifangClay, "Red Guifang Clay" },
        { Item.JisHair, "Ji's Hair" },
        { Item.PassengerTokenZouyan, "Passenger Token: Zouyan" },
        { Item.PassengerTokenAShou, "Passenger Token: A-Shou" },
        { Item.PenglaiRecipeCollection, "Penglai Recipe Collection" },
        { Item.PassengerTokenXipu, "Passenger Token: Xipu" },
        { Item.PassengerTokenYangfan, "Passenger Token: Yangfan" },
        { Item.PassengerTokenJihai, "Passenger Token: Jihai" },
        // unsure how we want to handle post-PonR stuff
        //{ Item.FriendPhoto, "Friend Photo" },
        { Item.PassengerTokenAimu, "Passenger Token: Aimu" },
        { Item.PassengerTokenShiyangyue, "Passenger Token: Shiyangyue" },

        { Item.CentralCoreChip, "Central Core Chip" },
        { Item.PowerReservoirChip, "Power Reservoir Chip" },
        { Item.AgriculturalZoneChip, "Agricultural Zone Chip" },
        { Item.AbandonedMinesChip, "Abandoned Mines Chip" },
        { Item.WarehouseZoneChip, "Warehouse Zone Chip" },
        { Item.TransmutationZoneChip, "Transmutation Zone Chip" },
        { Item.GrottoOfScripturesChip, "Grotto of Scriptures Chip" },
        { Item.EmpyreanDistrictChip, "Empyrean District Chip" },
        { Item.ResearchCenterChip, "Research Center Chip" },

        { Item.StasisJade, "Stasis Jade" },
        { Item.BearingJade, "Bearing Jade" },
        { Item.HarnessForceJade, "Harness Force Jade" },
        { Item.IronSkinJade, "Iron Skin Jade" },
        { Item.HedgehogJade, "Hedgehog Jade" },
        { Item.PauperJade, "Pauper Jade" },
        { Item.SteelyJade, "Steely Jade" },
        { Item.ImmovableJade, "Immovable Jade" },
        { Item.SoulReaperJade, "Soul Reaper Jade" },
        { Item.AvariceJade, "Avarice Jade" },
        { Item.RevivalJade, "Revival Jade" },
        { Item.SwiftDescentJade, "Swift Descent Jade" },
        { Item.MobQuellJadeYin, "Mob Quell Jade - Yin" },
        { Item.MobQuellJadeYang, "Mob Quell Jade - Yang" },
        { Item.FocusJade, "Focus Jade" },
        { Item.SwiftBladeJade, "Swift Blade Jade" },
        { Item.BreatherJade, "Breather Jade" },
        { Item.QiSwipeJade, "Qi Swipe Jade" },
        { Item.QiBladeJade, "Qi Blade Jade" },
        { Item.DivineHandJade, "Divine Hand Jade" },

        { Item.MedicinalCitrine, "(Poison) Medicinal Citrine" },
        { Item.GoldenYinglongEgg, "(Poison) Golden Yinglong Egg" },
        { Item.ResidualHair, "(Poison) Residual Hair" },
        { Item.PorcineGem, "(Poison) Porcine Gem" },
        { Item.PlantagoFrog, "(Poison) Plantago Frog" },
        { Item.Oriander, "(Poison) Oriander" },
        { Item.TurtleScorpion, "(Poison) Turtle Scorpion" },

        { Item.ApemanSurveillanceFootage, "(Database) Apeman Surveillance Footage" },
        { Item.CouncilDigitalSignage, "(Database) Council Digital Signage" },
        { Item.NewKunlunLaunchMemorial, "(Database) New Kunlun Launch Memorial" },
        { Item.CouncilTenets, "(Database) Council Tenets" },
        { Item.AnomalousRootNode, "(Database) Anomalous Root Node" },
        { Item.RhizomaticEnergyMeter, "(Database) Rhizomatic Energy Meter" },
        { Item.RadiantPagodaControlPanel, "(Database) Radiant Pagoda Control Panel" },
        { Item.DuskGuardianRecordingDevice1, "(Database) Dusk Guardian Recording Device 1" },
        { Item.LakeYaochiStele, "(Database) Lake Yaochi Stele" },
        { Item.YellowWaterReport, "(Database) Yellow Water Report" },
        { Item.MutatedCrops, "(Database) Mutated Crops" },
        { Item.DuskGuardianRecordingDevice2, "(Database) Dusk Guardian Recording Device 2" },
        { Item.WaterSynthesisPipelinePanel, "(Database) Water Synthesis Pipeline Panel" },
        { Item.JieClanFamilyPrecept, "(Database) Jie Clan Family Precept" },
        { Item.TransmutationFurnaceMonitor, "(Database) Transmutation Furnace Monitor" },
        { Item.DuskGuardianRecordingDevice4, "(Database) Dusk Guardian Recording Device 4" },
        { Item.GuardProductionStation, "(Database) Guard Production Station" },
        { Item.CaveStoneInscription, "(Database) Cave Stone Inscription" },
        { Item.DeadPersonsNote, "(Database) Dead Person's Note" },
        { Item.CampScroll, "(Database) Camp Scroll" },
        { Item.WarehouseDatabase, "(Database) Warehouse Database" },
        { Item.DuskGuardianRecordingDevice3, "(Database) Dusk Guardian Recording Device 3" },
        { Item.AncientWeaponConsole, "(Database) Ancient Weapon Console" },
        { Item.HexachremVaultScroll, "(Database) Hexachrem Vault Scroll" },
        { Item.AncientCavePainting, "(Database) Ancient Cave Painting" },
        { Item.CoffinInscription, "(Database) Coffin Inscription" },
        { Item.StoneCarvings, "(Database) Stone Carvings" },
        { Item.SecretMural1, "(Database) Secret Mural I" },
        { Item.SecretMural2, "(Database) Secret Mural II" },
        { Item.SecretMural3, "(Database) Secret Mural III" },
        { Item.StowawaysCorpse, "(Database) Stowaway's Corpse" },
        { Item.EmpyreanBulletinBoard, "(Database) Empyrean Bulletin Board" },
        { Item.DuskGuardianRecordingDevice5, "(Database) Dusk Guardian Recording Device 5" },
        { Item.VitalSanctumTowerMonitoringPanel, "(Database) Vital Sanctum Tower Monitoring Panel" },
        { Item.DuskGuardianRecordingDevice6, "(Database) Dusk Guardian Recording Device 6" },
        { Item.DuskGuardianHeadquartersScreen, "(Database) Dusk Guardian Headquarters" },
        { Item.RootCoreMonitoringDevice, "(Database) Root Core Monitoring Device" },
        { Item.FarmlandMarkings, "(Database) Farmland Markings" },
        { Item.EvacuationNoticeForMiners, "(Database) Evacuation Notice For Miners" },
        { Item.PrisonersBambooScroll1, "(Database) Prisoner's Bamboo Scroll I" },
        { Item.PrisonersBambooScroll2, "(Database) Prisoner's Bamboo Scroll II" },
        { Item.PharmacyPanel, "(Database) Pharmacy Panel" },
        { Item.HaotianSphereModel, "(Database) Haotian Sphere Model" },
        { Item.GalacticDockSign, "(Database) Galactic Dock Sign" },
        { Item.UndergroundWaterTower, "(Database) Underground Water Tower" },

        { Item.Jin320, "Jin x320" },
        { Item.Jin110, "Jin x110" },
        { Item.Jin50, "Jin x50" },
        { Item.BasicComponent, "Basic Component" },
        { Item.StandardComponent, "Standard Component" },
        { Item.AdvancedComponent, "Advanced Component" },
    };
}
