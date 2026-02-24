using HarmonyLib;
using RCGFSM.Items;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class TeleportPoints {

    private static TeleportPoint firstNode = TeleportPoint.ApemanFacilityMonitoring;

    public static void ApplySlotData(string? firstRootNodeName) {
        switch (firstRootNodeName) {
            case "apeman_facility_monitoring": firstNode = TeleportPoint.ApemanFacilityMonitoring; break;
            case "galactic_dock": firstNode = TeleportPoint.GalacticDock; break;
            case "power_reservoir_east": firstNode = TeleportPoint.PowerReservoirEast; break;
            case "lake_yaochi_ruins": firstNode = TeleportPoint.LakeYaochiRuins; break;
            case "yinglong_canal": firstNode = TeleportPoint.YinglongCanal; break;
            case "factory_great_hall": firstNode = TeleportPoint.FactoryGreatHall; break;
            case "outer_warehouse": firstNode = TeleportPoint.OuterWarehouse; break;
            case "grotto_of_scriptures_entry": firstNode = TeleportPoint.GrottoOfScripturesEntry; break;
            case "grotto_of_scriptures_east": firstNode = TeleportPoint.GrottoOfScripturesEast; break;
            case "grotto_of_scriptures_west": firstNode = TeleportPoint.GrottoOfScripturesWest; break;
            case "agrarian_hall": firstNode = TeleportPoint.AgrarianHall; break;
            case "radiant_pagoda": firstNode = TeleportPoint.RadiantPagoda; break;
            case "apeman_facility_depths": firstNode = TeleportPoint.ApemanFacilityDepths; break;
            case "central_transport_hub": firstNode = TeleportPoint.CentralTransportHub; break;
            case "factory_underground": firstNode = TeleportPoint.FactoryUnderground; break;
            case "inner_warehouse": firstNode = TeleportPoint.InnerWarehouse; break;
            case "power_reservoir_west": firstNode = TeleportPoint.PowerReservoirWest; break;
            default:
                Log.Error($"Unrecognized first root node name: {firstRootNodeName}");
                firstNode = TeleportPoint.ApemanFacilityMonitoring;
                break;
        }
    }

    public enum TeleportPoint {
        FourSeasonsPavilion,
        ApemanFacilityMonitoring,
        ApemanFacilityElevatorUpper,
        ApemanFacilityElevatorLower,
        ApemanFacilityDepths,
        CentralTransportHub,
        GalacticDock,
        PowerReservoirEast,
        PowerReservoirCentral,
        RadiantPagoda,
        PowerReservoirWest,
        LakeYaochiRuins,
        Greenhouse,
        WaterAndOxygenSynthesis,
        AgrarianHall,
        YinglongCanal,
        CortexCenter,
        OuterWarehouse,
        InnerWarehouse,
        BoundlessRepository,
        FactoryGreatHall,
        FactoryPrison,
        FactoryMachineRoom,
        FactoryUnderground,
        FactoryProductionArea,
        ShengwuHall,
        AbandonedMines,
        GrottoOfScripturesEntry,
        GrottoOfScripturesWest,
        GrottoOfScripturesEast,
        SkyTower,
        EmpyreanDistrictPassages,
        EmpyreanDistrictLivingArea,
        EmpyreanDistrictSanctum,
        TiandaoResearchCenter,
        TiandaoResearchInstitute,
        NewKunlunControlHub,
    }

    public static Dictionary<TeleportPoint, string> teleportPointToGameFlagPath = new Dictionary<TeleportPoint, string> {
        { TeleportPoint.FourSeasonsPavilion, "9115d3446fcc24abab2c0030d55abd1eTeleportPointData" },
        { TeleportPoint.ApemanFacilityMonitoring, "b3cf5264bd5d54b06975638acac58b2aTeleportPointData" },
        { TeleportPoint.ApemanFacilityElevatorUpper, "113385c61a2544446925e7516fea6016TeleportPointData" },
        { TeleportPoint.ApemanFacilityElevatorLower, "2ee85cc763a3d413f9b6e18e7e1fee67TeleportPointData" },
        { TeleportPoint.ApemanFacilityDepths, "b9e8a5c6d9f6e4812bf9cc30810faadbTeleportPointData" },
        { TeleportPoint.CentralTransportHub, "e5246ef15ff2a41ae884071b014351f4TeleportPointData" },
        { TeleportPoint.GalacticDock, "3f4604f810c9e9a469f01d352d8035b9TeleportPointData" },
        { TeleportPoint.PowerReservoirEast, "610a3a3232701af47b876a660910fccaTeleportPointData" },
        { TeleportPoint.PowerReservoirCentral, "9bf82941e7038a44980ca867c751e5cbTeleportPointData" },
        { TeleportPoint.RadiantPagoda, "b3310817230814fa09f5fa3b12cb6293TeleportPointData" },
        { TeleportPoint.PowerReservoirWest, "a71b5769629d443ef9af5e3a05385c73TeleportPointData" },
        { TeleportPoint.LakeYaochiRuins, "07215bbc4dc4247aeb7a980bf4910bf9TeleportPointData" },
        { TeleportPoint.Greenhouse, "126a7caa6701e4f0a9059b603a309c3fTeleportPointData" },
        { TeleportPoint.WaterAndOxygenSynthesis, "b242c961367ad6a49889999af22a46d9TeleportPointData" },
        { TeleportPoint.AgrarianHall, "1025e9e6b5d9440979328bcc29d89468TeleportPointData" },
        { TeleportPoint.YinglongCanal, "685eb4dc303abbd43a67a07cf0459c64TeleportPointData" },
        { TeleportPoint.CortexCenter, "c971454704bfa480880660b06e4af2c7TeleportPointData" },
        { TeleportPoint.OuterWarehouse, "afbe61da78699487faccd6f0ae1d9667TeleportPointData" },
        { TeleportPoint.InnerWarehouse, "2c8de8a9dfacf4f65a0fa1e60a16f852TeleportPointData" },
        { TeleportPoint.BoundlessRepository, "242132789f3c9e94bbabb096bad651a5TeleportPointData" },
        { TeleportPoint.FactoryGreatHall, "c748d8c36b73c464fa38d9b156c0b0bcTeleportPointData" },
        { TeleportPoint.FactoryPrison, "28a1908d9e21d4136b8c903e2b92b0afTeleportPointData" }, // breaks weakened/prison state if enabled early
        { TeleportPoint.FactoryMachineRoom, "4970d157835c54adbb55bb4f8e245fdfTeleportPointData" },
        { TeleportPoint.FactoryUnderground, "f1e11be280022400caf9c8593ead7d43TeleportPointData" },
        { TeleportPoint.FactoryProductionArea, "7a581656cd08345b793d7ad2b14e9943TeleportPointData" },
        { TeleportPoint.ShengwuHall, "7b8d46b0c0c1845fe893ce18aa67bca3TeleportPointData" },
        { TeleportPoint.AbandonedMines, "3b7b8da692cb64d298142612c02daa4cTeleportPointData" },
        { TeleportPoint.GrottoOfScripturesEntry, "4bb93fbefaedb8e47949d1d384220c21TeleportPointData" },
        { TeleportPoint.GrottoOfScripturesWest, "fbaf57e3f6bea904ea8c150e5915bcf4TeleportPointData" },
        { TeleportPoint.GrottoOfScripturesEast, "1b81567d30abe194d845d3f08beae8fdTeleportPointData" },
        { TeleportPoint.SkyTower, "32bafafe1cacf5c49af2f7c9e45fe511TeleportPointData" },
        { TeleportPoint.EmpyreanDistrictPassages, "8e915ab1790ef69468d4d49d4f338db2TeleportPointData" },
        { TeleportPoint.EmpyreanDistrictLivingArea, "1a4e7bc2545139145ba229dac285581bTeleportPointData" },
        { TeleportPoint.EmpyreanDistrictSanctum, "ba5ec4c4a702ba84baec1326a803b2b6TeleportPointData" },
        { TeleportPoint.TiandaoResearchCenter, "5c30ad493bbdebc40a2370664d46b830TeleportPointData" },
        { TeleportPoint.TiandaoResearchInstitute, "473d9c581cd574f62a36519ae3d451ebTeleportPointData" },
        { TeleportPoint.NewKunlunControlHub, "da6613d2c8f7e4eb6ae1b4d0fe7fee93TeleportPointData" },
    };

    public static void SetFirstRootNodeAfterNewGameCreation() {
        Log.Info($"SetFirstRootNodeAfterNewGameCreation() unlocking {firstNode}");

        var firstTPData = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(teleportPointToGameFlagPath[firstNode]);
        firstTPData.unlocked.SetCurrentValue(true);

        SingletonBehaviour<SaveManager>.Instance.AutoSave(SaveManager.SaveSceneScheme.FlagOnly, forceShowIcon: true);
    }

    public static bool IsNodeUnlocked(TeleportPoint tp) {
        var flagPath = TeleportPoints.teleportPointToGameFlagPath[tp];
        return SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(flagPath).unlocked.CurrentValue;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        if (__instance.name == "[Action] Unlock A1_S1 SavePoint") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] Init/[Action] Unlock A1_S1 SavePoint") {
                Log.Info($"TeleportPoints::PickItemAction_OnStateEnterImplement preventing one of the FSP state machines from inexplicably auto-unlocking the AFM node");
                return false;
            }
        }
        return true;
    }

    // for debug tools
    public static void UnlockAllNonPrisonTeleportPoints() {
        foreach (var (tp, path) in teleportPointToGameFlagPath) {
            if (tp == TeleportPoint.FactoryPrison)
                continue;
            SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(path).unlocked.SetCurrentValue(true);
        }
    }
}
