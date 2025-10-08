using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer;

public enum Item {
    // Sol Seals
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

    // Equipment
    //AzureBow, // better to let the arrow items immediately work in any order, since bow on its own does nothing
    ArrowCloudPiercer,
    ArrowThunderBuster,
    ArrowShadowHunter,

    // Upgrades
    HerbCatalyst,
    PipeVial,
    TaoFruit,
    GreaterTaoFruit,
    TwinTaoFruit,
    ComputingUnit,
    DarkSteel,

    // Key/Progression Items
    AbandonedMinesAccessToken,
    LegendOfThePorkyHeroes,
    JisHair,
    TianhuoSerum,
    ElevatorAccessToken,
    RhizomaticBomb,
    // assuming we count unlocking the other arrow types as "progression"
    HomingDarts,
    ThunderburstBomb,
    //YellowDragonsnakeVenomSac, // post-all-poisons
    //YellowDragonsnakeMedicinalBrew, // post-all-poisons
    //DamagedFusangAmulet, // post-all-poisons

    // Gifts For Shuanshuan
    FusangAmulet,
    MultiToolKit,
    AncientSheetMusic,
    UnknownSeed,
    GMFertilizer,
    SwordOfJie,
    AntiqueVinylRecord,
    QiankunBoard,
    RedGuifangClay,
    PenglaiRecipeCollection,
    TiandaoAcademyPeriodical,
    KunlunImmortalPortrait,
    VirtualRealityDevice,
    ReadyToEatRations,
    TheFourTreasuresOfTheStudy,

    // Miscellaneous Unique Items
    BloodyCrimsonHibiscus,
    AncientPenglaiBallad,
    PortraitOfYi,
    PoemHiddenInTheImmortalsPortrait,
    SoulSeveringBlade,
    FirestormRing,

    // Unique Recyclables/Treasure Items
    NobleRing,
    PassengerTokenZouyan,
    PassengerTokenAShou,
    PassengerTokenXipu,
    PassengerTokenYangfan,
    PassengerTokenJihai,
    PassengerTokenAimu,
    PassengerTokenShiyangyue,
    GeneEradicator,
    //JinMedallion, // post-all-poisons

    // Map Chips for Shanhai 9000
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
    CultivationJade,

    // Poisons
    MedicinalCitrine,
    GoldenYinglongEgg,
    MoltedTianmaHide,
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
    //RootCoreMonitoringDevice, // post-PonR
    FarmlandMarkings,
    EvacuationNoticeForMiners,
    PrisonersBambooScroll1,
    PrisonersBambooScroll2,
    PharmacyPanel,
    HaotianSphereModel,
    GalacticDockSign,
    UndergroundWaterTower,

    // Jin/Money filler
    Jin800,
    Jin320,
    Jin50,
    BasicComponent,
    StandardComponent,
    AdvancedComponent,

    // Removed abilities
    WallClimb,
    Grapple,
    LedgeGrab,
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

        //{ Item.AzureBow, "Azure Bow" },
        { Item.ArrowCloudPiercer, "Arrow: Cloud Piercer" },
        { Item.ArrowThunderBuster, "Arrow: Thunder Buster" },
        { Item.ArrowShadowHunter, "Arrow: Shadow Hunter" },

        { Item.HerbCatalyst, "Herb Catalyst" },
        { Item.PipeVial, "Pipe Vial" },
        { Item.TaoFruit, "Tao Fruit" },
        { Item.GreaterTaoFruit, "Greater Tao Fruit" },
        { Item.TwinTaoFruit, "Twin Tao Fruit" },
        { Item.ComputingUnit, "Computing Unit" },
        { Item.DarkSteel, "Dark Steel" },

        { Item.AbandonedMinesAccessToken, "Abandoned Mines Access Token" },
        { Item.LegendOfThePorkyHeroes, "(Artifact) Legend of the Porky Heroes" },
        { Item.JisHair, "Ji's Hair" },
        { Item.TianhuoSerum, "Tianhuo Serum" },
        { Item.ElevatorAccessToken, "Elevator Access Token" },
        { Item.RhizomaticBomb, "Rhizomatic Bomb" },
        { Item.HomingDarts, "Homing Darts" },
        { Item.ThunderburstBomb, "Thunderburst Bomb" },
        //{ Item.YellowDragonsnakeVenomSac, "Yellow Dragonsnake Venom Sac" }, // post-all-poisons
        //{ Item.YellowDragonsnakeMedicinalBrew, "Yellow Dragonsnake Medicinal Brew" }, // post-all-poisons
        //{ Item.DamagedFusangAmulet, "Damaged Fusang Amulet" }, // post-all-poisons, may also be missable

        { Item.FusangAmulet, "(Artifact) Fusang Amulet" },
        { Item.MultiToolKit, "(Artifact) Multi-tool Kit" },
        { Item.AncientSheetMusic, "(Artifact) Ancient Sheet Music" },
        { Item.UnknownSeed, "(Artifact) Unknown Seed" },
        { Item.GMFertilizer, "(Artifact) GM Fertilizer" },
        { Item.SwordOfJie, "(Artifact) Sword of Jie" },
        { Item.AntiqueVinylRecord, "(Artifact) Antique Vinyl Record" },
        { Item.QiankunBoard, "(Artifact) Qiankun Board" },
        { Item.RedGuifangClay, "(Artifact) Red Guifang Clay" },
        { Item.PenglaiRecipeCollection, "(Artifact) Penglai Recipe Collection" },
        { Item.TiandaoAcademyPeriodical, "(Artifact) Tiandao Academy Periodical" },
        { Item.KunlunImmortalPortrait, "(Artifact) Kunlun Immortal Portrait" },
        { Item.VirtualRealityDevice, "(Artifact) Virtual Reality Device" },
        { Item.ReadyToEatRations, "(Artifact) Ready-to-Eat Rations" },
        { Item.TheFourTreasuresOfTheStudy, "(Artifact) The Four Treasures of the Study" },

        { Item.BloodyCrimsonHibiscus, "Bloody Crimson Hibiscus" },
        { Item.AncientPenglaiBallad, "Ancient Penglai Ballad" },
        { Item.PortraitOfYi, "(Artifact) Portrait of Yi" },
        { Item.PoemHiddenInTheImmortalsPortrait, "Poem Hidden in the Immortal's Portrait" },
        { Item.SoulSeveringBlade, "Soul-Severing Blade" },
        { Item.FirestormRing, "Firestorm Ring" },
        //{ Item.FriendPhoto, "Friend Photo" }, // post-PonR

        { Item.NobleRing, "(Recyclable) Noble Ring" },
        { Item.PassengerTokenZouyan, "(Recyclable) Passenger Token: Zouyan" },
        { Item.PassengerTokenAShou, "(Recyclable) Passenger Token: A-Shou" },
        { Item.PassengerTokenXipu, "(Recyclable) Passenger Token: Xipu" },
        { Item.PassengerTokenYangfan, "(Recyclable) Passenger Token: Yangfan" },
        { Item.PassengerTokenJihai, "(Recyclable) Passenger Token: Jihai" },
        { Item.PassengerTokenAimu, "(Recyclable) Passenger Token: Aimu" },
        { Item.PassengerTokenShiyangyue, "(Recyclable) Passenger Token: Shiyangyue" },
        { Item.GeneEradicator, "Gene Eradicator" },
        //{ Item.JinMedallion, "Jin Medallion" }, // post-all-poisons, may also be missable

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
        { Item.CultivationJade, "Cultivation Jade" },

        { Item.MedicinalCitrine, "(Poison) Medicinal Citrine" },
        { Item.GoldenYinglongEgg, "(Poison) Golden Yinglong Egg" },
        { Item.MoltedTianmaHide, "(Poison) Molted Tianma Hide" },
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
        //{ Item.RootCoreMonitoringDevice, "(Database) Root Core Monitoring Device" }, // post-PonR
        { Item.FarmlandMarkings, "(Database) Farmland Markings" },
        { Item.EvacuationNoticeForMiners, "(Database) Evacuation Notice For Miners" },
        { Item.PrisonersBambooScroll1, "(Database) Prisoner's Bamboo Scroll I" },
        { Item.PrisonersBambooScroll2, "(Database) Prisoner's Bamboo Scroll II" },
        { Item.PharmacyPanel, "(Database) Pharmacy Panel" },
        { Item.HaotianSphereModel, "(Database) Haotian Sphere Model" },
        { Item.GalacticDockSign, "(Database) Galactic Dock Sign" },
        { Item.UndergroundWaterTower, "(Database) Underground Water Tower" },

        { Item.Jin800, "Jin x800" },
        { Item.Jin320, "Jin x320" },
        { Item.Jin50, "Jin x50" },
        { Item.BasicComponent, "(Recyclable) Basic Component" },
        { Item.StandardComponent, "(Recyclable) Standard Component" },
        { Item.AdvancedComponent, "(Recyclable) Advanced Component" },

        { Item.WallClimb, "Wall Climb" },
        { Item.Grapple, "Grapple" },
        { Item.LedgeGrab, "Ledge Grab" },
    };

    public static Dictionary<string, Item> itemNamesReversed = itemNames.ToDictionary(itemName => itemName.Value, itemName => itemName.Key);

    // leave these as null until we load the ids, so any attempt to work with ids before that will fail loudly
    public static Dictionary<long, Item> archipelagoIdToItem = null;
    public static Dictionary<Item, long> itemToArchipelagoId = null;

    public static void LoadArchipelagoIds(string itemsFileContent) {
        var itemsData = JArray.Parse(itemsFileContent);
        archipelagoIdToItem = new();
        itemToArchipelagoId = new();
        foreach (var itemData in itemsData) {
            // Skip event items, since they intentionally don't have ids
            if (itemData["code"].Type == JTokenType.Null) continue;

            var archipelagoId = (long)itemData["code"];
            var name = (string)itemData["name"];

            if (!itemNamesReversed.ContainsKey(name))
                throw new System.Exception($"LoadArchipelagoIds failed: unknown item name {name}");

            var item = itemNamesReversed[name];
            archipelagoIdToItem.Add(archipelagoId, item);
            itemToArchipelagoId.Add(item, archipelagoId);
        }
    }
}
