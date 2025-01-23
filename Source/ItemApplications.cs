using NineSolsAPI;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

internal class ItemApplications {
    public static void UpdateItemCount(Item item, int count) {
        Log.Info($"UpdateItemCount(item={item}, count={count})");

        var oldCount = ApInventory.GetValueOrDefault(item);

        // TODO: apply all items when game is actually loaded
        if (SingletonBehaviour<GameCore>.IsAvailable()) {
            var gfd = ApplyItemToPlayer(item, count, oldCount);

            // TODO: updating multiple items doesn't stack these popups, they overwrite each other
            if (gfd != null && count > oldCount) {
                // These are the important parts of ItemGetUIShowAction.Implement(). That method is more complex, but we want more consistency than the base game.
                // Note that ShowGetDescriptablePrompt() will display "No Data" unless the gfd has already been applied
                SingletonBehaviour<UIManager>.Instance.ShowGetDescriptablePrompt(gfd);
                SingletonBehaviour<SaveManager>.Instance.AutoSave(SaveManager.SaveSceneScheme.CurrentSceneAndPos, forceShowIcon: true);
            }
        }

        ApInventory[item] = count;
    }

    // TODO: load from a save file
    private static Dictionary<Item, int> ApInventory = new Dictionary<Item, int>();

    private static GameFlagDescriptable? ApplyItemToPlayer(Item item, int count, int oldCount) {
        Log.Info($"ApplyItemToPlayer(item={item}, count={count}, oldCount={oldCount})");

        // we're unlikely to use these, but: RollDodgeAbility is regular ground dash
        PlayerAbilityData? ability = null;
        switch (item) {
            case Item.TaiChiKick: ability = Player.i.mainAbilities.ParryJumpKickAbility; break;
            case Item.AirDash: ability = Player.i.mainAbilities.RollDodgeInAirUpgrade; break;
            case Item.ChargedStrike: ability = Player.i.mainAbilities.ChargedAttackAbility; break;
            case Item.CloudLeap: ability = Player.i.mainAbilities.AirJumpAbility; break;
            case Item.AzureBow: ability = Player.i.mainAbilities.ArrowAbility; break;
            case Item.SuperMutantBuster: ability = Player.i.mainAbilities.KillZombieFooAbility; break;
            case Item.UnboundedCounter: ability = Player.i.mainAbilities.ParryCounterAbility; break;
            case Item.MysticNymphScoutMode: ability = Player.i.mainAbilities.HackDroneAbility; break;
            default: break;
        }
        if (ability != null) {
            ability.acquired.SetCurrentValue(count > 0);
            ability.equipped.SetCurrentValue(count > 0);
            if (ability.BindingItemPicked != null) {
                Log.Info($"!!! {ability} has BindingItemPicked={ability.BindingItemPicked} !!!");
            }
            return ability;
        }

        // TODO: is there a better way than UIManager to get at the GameFlagDescriptables for every inventory item? SaveManager.allFlags.flagDict???
        List<ItemDataCollection> inventory = SingletonBehaviour<UIManager>.Instance.allItemCollections;
        GameFlagDescriptable? inventoryItem = null;
        switch (item) {
            case Item.SealOfKuafu: inventoryItem = inventory[0].rawCollection[0]; break;
            case Item.SealOfGoumang: inventoryItem = inventory[0].rawCollection[1]; break;
            case Item.SealOfYanlao: inventoryItem = inventory[0].rawCollection[2]; break;
            case Item.SealOfJiequan: inventoryItem = inventory[0].rawCollection[3]; break;
            case Item.SealOfLadyEthereal: inventoryItem = inventory[0].rawCollection[4]; break;
            case Item.SealOfJi: inventoryItem = inventory[0].rawCollection[5]; break;
            case Item.SealOfFuxi: inventoryItem = inventory[0].rawCollection[6]; break;
            case Item.SealOfNuwa: inventoryItem = inventory[0].rawCollection[7]; break;

            case Item.BloodyCrimsonHibiscus: inventoryItem = inventory[0].rawCollection[8]; break;
            case Item.AncientPenglaiBallad: inventoryItem = inventory[0].rawCollection[9]; break;
            case Item.PoemHiddenInTheImmortalsPortrait: inventoryItem = inventory[0].rawCollection[10]; break;
            case Item.ThunderburstBomb: inventoryItem = inventory[0].rawCollection[11]; break;
            case Item.GeneEradicator: inventoryItem = inventory[0].rawCollection[12]; break;
            case Item.HomingDarts: inventoryItem = inventory[0].rawCollection[13]; break;
            case Item.SoulSeveringBlade: inventoryItem = inventory[0].rawCollection[14]; break;
            case Item.FirestormRing: inventoryItem = inventory[0].rawCollection[15]; break;
            //case Item.YellowDragonsnakeVenomSac: inventoryItem = inventory[0].rawCollection[16]; break; // post-all-poisons
            //case Item.YellowDragonsnakeMedicinalBrew: inventoryItem = inventory[0].rawCollection[17]; break; // post-all-poisons
            case Item.JisHair: inventoryItem = inventory[0].rawCollection[18]; break;
            case Item.TianhuoSerum: inventoryItem = inventory[0].rawCollection[19]; break;
            case Item.ElevatorAccessToken: inventoryItem = inventory[0].rawCollection[20]; break;
            case Item.RhizomaticBomb: inventoryItem = inventory[0].rawCollection[21]; break;
            //case Item.RhizomaticArrow: inventoryItem = inventory[0].rawCollection[22]; break; // post-PonR
            case Item.AbandonedMinesAccessToken: inventoryItem = inventory[0].rawCollection[22]; break;
            //case Item.FriendPhoto: inventoryItem = inventory[0].rawCollection[23]; break; // post-PonR
            case Item.GreaterTaoFruit: inventoryItem = inventory[0].rawCollection[24]; break;
            case Item.TaoFruit: inventoryItem = inventory[0].rawCollection[25]; break;
            case Item.TwinTaoFruit: inventoryItem = inventory[0].rawCollection[26]; break;

            // inventory[1]
            case Item.AbandonedMinesChip: inventoryItem = inventory[1].rawCollection[0]; break;
            case Item.PowerReservoirChip: inventoryItem = inventory[1].rawCollection[1]; break;
            case Item.AgriculturalZoneChip: inventoryItem = inventory[1].rawCollection[2]; break;
            case Item.WarehouseZoneChip: inventoryItem = inventory[1].rawCollection[3]; break;
            case Item.TransmutationZoneChip: inventoryItem = inventory[1].rawCollection[4]; break;
            case Item.CentralCoreChip: inventoryItem = inventory[1].rawCollection[5]; break;
            case Item.EmpyreanDistrictChip: inventoryItem = inventory[1].rawCollection[6]; break;
            case Item.GrottoOfScripturesChip: inventoryItem = inventory[1].rawCollection[7]; break;
            case Item.ResearchCenterChip: inventoryItem = inventory[1].rawCollection[8]; break;
            case Item.FusangAmulet: inventoryItem = inventory[1].rawCollection[9]; break;
            case Item.MultiToolKit: inventoryItem = inventory[1].rawCollection[10]; break;
            case Item.TiandaoAcademyPeriodical: inventoryItem = inventory[1].rawCollection[11]; break;
            case Item.KunlunImmortalPortrait: inventoryItem = inventory[1].rawCollection[12]; break;
            case Item.QiankunBoard: inventoryItem = inventory[1].rawCollection[13]; break;
            case Item.AncientMusicSheet: inventoryItem = inventory[1].rawCollection[14]; break;
            case Item.UnknownSeed: inventoryItem = inventory[1].rawCollection[15]; break;
            case Item.VirtualRealityDevice: inventoryItem = inventory[1].rawCollection[16]; break;
            case Item.AntiqueVinylRecord: inventoryItem = inventory[1].rawCollection[17]; break;
            case Item.ReadyToEatRations: inventoryItem = inventory[1].rawCollection[18]; break;
            case Item.SwordOfJie: inventoryItem = inventory[1].rawCollection[19]; break;
            case Item.RedGuifangClay: inventoryItem = inventory[1].rawCollection[20]; break;
            case Item.TheFourTreasuresOfTheStudy: inventoryItem = inventory[1].rawCollection[21]; break;
            //case Item.CrimsonHibiscus: inventoryItem = inventory[1].rawCollection[22]; break; // not a thing since we skip the intro
            case Item.GMFertilizer: inventoryItem = inventory[1].rawCollection[23]; break;
            case Item.PenglaiRecipeCollection: inventoryItem = inventory[1].rawCollection[24]; break;

            case Item.MedicinalCitrine: inventoryItem = inventory[1].rawCollection[25]; break;
            case Item.GoldenYinglongEgg: inventoryItem = inventory[1].rawCollection[26]; break;
            //case Item.MoltedTianmaHide: inventoryItem = inventory[1].rawCollection[27]; break; // shop item
            case Item.ResidualHair: inventoryItem = inventory[1].rawCollection[28]; break;
            case Item.Oriander: inventoryItem = inventory[1].rawCollection[29]; break;
            case Item.TurtleScorpion: inventoryItem = inventory[1].rawCollection[30]; break;
            case Item.PlantagoFrog: inventoryItem = inventory[1].rawCollection[31]; break;
            case Item.PorcineGem: inventoryItem = inventory[1].rawCollection[32]; break;
            /*case Item.BallOfFlavor: inventoryItem = inventory[1].rawCollection[33]; break; // shop item
            case Item.DragonsWhip: inventoryItem = inventory[1].rawCollection[34]; break; // shop item
            case Item.Necroceps: inventoryItem = inventory[1].rawCollection[35]; break; // shop item
            case Item.Guiseng: inventoryItem = inventory[1].rawCollection[36]; break; // shop item
            case Item.ThunderCentipede: inventoryItem = inventory[1].rawCollection[37]; break; // shop item
            case Item.WallClimbingGecko: inventoryItem = inventory[1].rawCollection[38]; break; // shop item
            case Item.GutwrenchFruit: inventoryItem = inventory[1].rawCollection[39]; break;*/ // shop item
            //case Item.DamagedFusangAmulet: inventoryItem = inventory[1].rawCollection[40]; break; // post-all-poisons
            case Item.LegendOfThePorkyHeroes: inventoryItem = inventory[1].rawCollection[41]; break;
            case Item.PortraitOfYi: inventoryItem = inventory[1].rawCollection[42]; break;

            // inventory[2]
            case Item.BasicComponent: inventoryItem = inventory[2].rawCollection[0]; break;
            case Item.StandardComponent: inventoryItem = inventory[2].rawCollection[1]; break;
            case Item.AdvancedComponent: inventoryItem = inventory[2].rawCollection[2]; break;
            case Item.NobleRing: inventoryItem = inventory[2].rawCollection[3]; break;
            //case Item.ShuanshuanCoin: inventoryItem = inventory[2].rawCollection[4]; break; // technically a shop item
            //case Item.JinMedallion: inventoryItem = inventory[2].rawCollection[5]; break; // post-all-poisons
            case Item.PassengerTokenAShou: inventoryItem = inventory[2].rawCollection[6]; break;
            case Item.PassengerTokenZouyan: inventoryItem = inventory[2].rawCollection[7]; break;
            case Item.PassengerTokenXipu: inventoryItem = inventory[2].rawCollection[8]; break;
            case Item.PassengerTokenJihai: inventoryItem = inventory[2].rawCollection[9]; break;
            case Item.PassengerTokenYangfan: inventoryItem = inventory[2].rawCollection[10]; break;
            case Item.PassengerTokenAimu: inventoryItem = inventory[2].rawCollection[11]; break;
            case Item.PassengerTokenShiyangyue: inventoryItem = inventory[2].rawCollection[12]; break;
            case Item.DarkSteel: inventoryItem = inventory[2].rawCollection[13]; break;
            case Item.HerbCatalyst: inventoryItem = inventory[2].rawCollection[14]; break;

            // inventory[3]
            //Azure Sand
            //Jin
            //Root Stabilizer ???
            //Fusang Horn
            //Mystic Nymph
            //Super Mutant Buster
            case Item.ComputingUnit: inventoryItem = inventory[3].rawCollection[6]; break;
            //Azure Bow
            //Azure Sand Magazine
            //Medicine Pipe
            case Item.PipeVial: inventoryItem = inventory[3].rawCollection[10]; break;
            default: break;
        }
        if (inventoryItem != null) {
            inventoryItem.acquired.SetCurrentValue(count > 0);
            inventoryItem.unlocked.SetCurrentValue(count > 0);
            if (inventoryItem is ItemData) {
                ((ItemData)inventoryItem).ownNum.SetCurrentValue(count);
            }
            // TODO: do we also need pickItemEventRaiser.Raise(); ?
            return inventoryItem;
        }

        // TODO: is there a better way than UIManager to get at the GameFlagDescriptables for every database entry?
        // We only care about allPediaCollections[2]. [0] is character entries, which we don't randomize. [1] seems unused.
        List<GameFlagDescriptable> database = SingletonBehaviour<UIManager>.Instance.allPediaCollections[2].rawCollection;
        GameFlagDescriptable? databaseEntry = null;
        switch (item) {
            case Item.DeadPersonsNote: databaseEntry = database[0]; break;
            case Item.CampScroll: databaseEntry = database[1]; break;
            case Item.ApemanSurveillanceFootage: databaseEntry = database[2]; break;
            case Item.CouncilDigitalSignage: databaseEntry = database[3]; break;
            case Item.NewKunlunLaunchMemorial: databaseEntry = database[4]; break;
            case Item.AnomalousRootNode: databaseEntry = database[5]; break;
            case Item.RhizomaticEnergyMeter: databaseEntry = database[6]; break;
            case Item.DuskGuardianRecordingDevice1: databaseEntry = database[7]; break;
            case Item.RadiantPagodaControlPanel: databaseEntry = database[8]; break;
            case Item.LakeYaochiStele: databaseEntry = database[9]; break;
            case Item.AncientCavePainting: databaseEntry = database[10]; break;
            case Item.CoffinInscription: databaseEntry = database[11]; break;
            case Item.YellowWaterReport: databaseEntry = database[12]; break;
            case Item.MutatedCrops: databaseEntry = database[13]; break;
            case Item.WaterSynthesisPipelinePanel: databaseEntry = database[14]; break;
            case Item.DuskGuardianRecordingDevice2: databaseEntry = database[15]; break;
            case Item.FarmlandMarkings: databaseEntry = database[16]; break;
            case Item.CouncilTenets: databaseEntry = database[17]; break;
            case Item.TransmutationFurnaceMonitor: databaseEntry = database[18]; break;
            case Item.EvacuationNoticeForMiners: databaseEntry = database[19]; break;
            case Item.WarehouseDatabase: databaseEntry = database[20]; break;
            case Item.DuskGuardianRecordingDevice3: databaseEntry = database[21]; break;
            case Item.AncientWeaponConsole: databaseEntry = database[22]; break;
            case Item.HexachremVaultScroll: databaseEntry = database[23]; break;
            case Item.PrisonersBambooScroll1: databaseEntry = database[24]; break;
            case Item.PrisonersBambooScroll2: databaseEntry = database[25]; break;
            case Item.JieClanFamilyPrecept: databaseEntry = database[26]; break;
            case Item.GuardProductionStation: databaseEntry = database[27]; break;
            case Item.DuskGuardianRecordingDevice4: databaseEntry = database[28]; break;
            case Item.PharmacyPanel: databaseEntry = database[29]; break;
            case Item.HaotianSphereModel: databaseEntry = database[30]; break;
            case Item.CaveStoneInscription: databaseEntry = database[31]; break;
            case Item.GalacticDockSign: databaseEntry = database[32]; break;
            case Item.StoneCarvings: databaseEntry = database[33]; break;
            case Item.SecretMural1: databaseEntry = database[34]; break;
            case Item.SecretMural2: databaseEntry = database[35]; break;
            case Item.SecretMural3: databaseEntry = database[36]; break;
            case Item.StowawaysCorpse: databaseEntry = database[37]; break;
            case Item.UndergroundWaterTower: databaseEntry = database[38]; break;
            case Item.EmpyreanBulletinBoard: databaseEntry = database[39]; break;
            case Item.DuskGuardianRecordingDevice5: databaseEntry = database[40]; break;
            case Item.VitalSanctumTowerMonitoringPanel: databaseEntry = database[41]; break;
            case Item.DuskGuardianRecordingDevice6: databaseEntry = database[42]; break;
            case Item.DuskGuardianHeadquartersScreen: databaseEntry = database[43]; break;
            //case Item.RootCoreMonitoringDevice: databaseEntry = database[44]; break; // post-PonR
            default: break;
        }
        if (databaseEntry != null) {
            databaseEntry.acquired.SetCurrentValue(count > 0);
            databaseEntry.unlocked.SetCurrentValue(count > 0);
            return databaseEntry;
        }

        List<JadeData> jades = Player.i.mainAbilities.jadeDataColleciton.gameFlagDataList;
        JadeData? jadeEntry = null;
        switch (item) {
            case Item.ImmovableJade: jadeEntry = jades[0]; break;
            case Item.HarnessForceJade: jadeEntry = jades[1]; break;
            case Item.FocusJade: jadeEntry = jades[2]; break;
            case Item.SwiftDescentJade: jadeEntry = jades[3]; break;
            //case Item.MedicalJade: jadeEntry = jades[4]; break; // shop item
            //case Item.QuickDoseJade: jadeEntry = jades[5]; break; // shop item
            case Item.SteelyJade: jadeEntry = jades[6]; break;
            case Item.StasisJade: jadeEntry = jades[7]; break;
            case Item.MobQuellJadeYin: jadeEntry = jades[8]; break;
            //case Item.MobQuellJadeYang: jadeEntry = jades[9]; break; // shop item
            case Item.BearingJade: jadeEntry = jades[10]; break;
            case Item.DivineHandJade: jadeEntry = jades[11]; break;
            case Item.IronSkinJade: jadeEntry = jades[12]; break;
            case Item.PauperJade: jadeEntry = jades[13]; break;
            case Item.SwiftBladeJade: jadeEntry = jades[14]; break;
            //case Item.LastStandJade: jadeEntry = jades[15]; break; // shop item
            //case Item.RecoveryJade: jadeEntry = jades[16]; break; // shop item
            case Item.BreatherJade: jadeEntry = jades[17]; break;
            case Item.HedgehogJade: jadeEntry = jades[18]; break;
            //case Item.RicochetJade: jadeEntry = jades[19]; break; // shop item
            case Item.RevivalJade: jadeEntry = jades[20]; break;
            case Item.SoulReaperJade: jadeEntry = jades[21]; break;
            //case Item.HealthThiefJade: jadeEntry = jades[22]; break; // shop item
            case Item.QiBladeJade: jadeEntry = jades[23]; break;
            case Item.QiSwipeJade: jadeEntry = jades[24]; break;
            //case Item.ReciprocationJade: jadeEntry = jades[25]; break; // shop item
            case Item.CultivationJade: jadeEntry = jades[26]; break;
            case Item.AvariceJade: jadeEntry = jades[27]; break;
            default: break;
        }
        if (jadeEntry != null) {
            jadeEntry.acquired.SetCurrentValue(count > 0);
            jadeEntry.unlocked.SetCurrentValue(count > 0);
            return jadeEntry;
        }

        int moneyItemSize = 0;
        switch (item) {
            case Item.Jin800: moneyItemSize = 800; break;
            case Item.Jin320: moneyItemSize = 320; break;
            case Item.Jin50: moneyItemSize = 50; break;
            default: break;
        }
        if (moneyItemSize != 0) {
            // Since this is a consumable, you can't (always/reliably) take it away after it's been given,
            // so we only worry about adding jin when new jin items have arrived.
            var newMoneyItems = count - oldCount;
            if (newMoneyItems > 0) {
                var jinToAdd = newMoneyItems * moneyItemSize;
                SingletonBehaviour<GameCore>.Instance.playerGameData.AddGold(jinToAdd, GoldSourceTag.Chest);
            }

            var jinGFD = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[1];
            return jinGFD;
        }

        ToastManager.Toast($"unable to apply item {item} (count = {count})");
        return null;
    }
}
