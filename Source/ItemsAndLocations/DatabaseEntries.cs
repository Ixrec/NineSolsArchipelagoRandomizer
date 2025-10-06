using System.Collections.Generic;

namespace ArchipelagoRandomizer;

internal class DatabaseEntries {
    public static bool ApplyDatabaseEntryToPlayer(Item item, int count, int oldCount) {
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
            NotifyAndSave.Default(databaseEntry, count, oldCount);
            return true;
        }
        return false;
    }
}
