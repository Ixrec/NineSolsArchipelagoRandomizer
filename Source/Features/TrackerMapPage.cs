using HarmonyLib;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
class TrackerMapPage {
    private static Dictionary<string, string> GameLevelToTrackerPage = new Dictionary<string, string> {
        { "A6_S3", "abandoned_mines" },
        { "A3_S5_BossGouMang_GameLevel", "agrarian_hall" },
        { "A10S5", "ancient_stone_pillar" },
        { "A1_S3_GameLevel", "apeman_facility_depths" },
        { "A1_S2_GameLevel", "apeman_facility_elevator" },
        { "A1_S1_GameLevel", "apeman_facility_monitoring" },
        { "A4_S3", "boundless_repository" },
        { "AG_S1", "central_hall" },
        { "A2_S6", "central_transport_hub" },
        { "A7_S1", "cortex_center" },
        { "A9_S2", "empyrean_district_living_area" },
        { "A9_S1", "empyrean_district_passages" },
        { "A9_S3", "empyrean_district_sanctum" },
        { "A5_S1", "factory_great_hall" },
        { "A5_S3", "factory_machine_room" },
        { "A5_S4", "factory_production_area" },
        { "A6_S1", "factory_underground" },
        { "AG_S2", "four_seasons_pavilion" },
        { "GameLevel", "galactic_dock_and_village" },
        { "A3_S2", "greenhouse" },
        { "A10_S3", "grotto_of_scriptures_east" },
        { "A10_S1", "grotto_of_scriptures_entry" },
        { "A10_S4", "grotto_of_scriptures_west" },
        { "A4_S2", "inner_warehouse" },
        { "A3_S1", "lake_yaochi_ruins" },
        { "P2_R22_Savepoint_GameLevel", "nobility_hall" },
        { "A4_S1", "outer_warehouse" },
        { "A2_S1", "power_reservoir_central" },
        { "A2_S2", "power_reservoir_east" },
        { "A2_S3", "power_reservoir_west" },
        { "A5_S2", "prison" },
        { "A2_S5_ BossHorseman_GameLevel", "radiant_pagoda" },
        { "A5_S5", "shengwu_hall" },
        { "A9_S4", "sky_tower" },
        { "A11_S1", "tiandao_research_center" },
        { "A11_S2", "tianhuo_research_institute" },
        { "A0_S7", "underground_cave" },
        { "A3_S3", "water_and_oxygen_synthesis" },
        { "A0_S6", "yangu_hall" },
        { "A3_S7", "yinglong_canal" },
    };

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        var levelName = __instance.name;
        if (ConnectionAndPopups.APSession == null) {
            Log.Error($"TrackerMapPage::GameLevel_Awake aborting because APSession is null");
            return;
        }

        if (GameLevelToTrackerPage.TryGetValue(levelName, out var trackerPageName)) {
            var session = ConnectionAndPopups.APSession;
            var dsKey = $"{session.ConnectionInfo.Slot}_{session.ConnectionInfo.Team}_nine_sols_area";

            Log.Info($"TrackerMapPage::GameLevel_Awake setting DataStorage key \"{dsKey}\" to value \"{trackerPageName}\"");
            session.DataStorage[dsKey] = trackerPageName;
        } else {
            Log.Info($"TrackerMapPage::GameLevel_Awake called with unknown levelName = {levelName}");
        }
    }

}
