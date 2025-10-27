using System.Collections.Generic;
using static ArchipelagoRandomizer.NewGameCreation;

namespace ArchipelagoRandomizer;

internal class TeleportPoints {
    public enum FirstRootNodeChoice {
        ApemanFacilityMonitoring,
        GalacticDock,
        PowerReservoirEast,
        PowerReservoirCentral,
        LakeYaochiRuins,
        YinglongCanal,
        FactoryGreatHall,
        OuterWarehouse,
        GrottoOfScripturesEntry,
        GrottoOfScripturesEast,
        GrottoOfScripturesWest,
    }

    private static FirstRootNodeChoice firstNode = FirstRootNodeChoice.ApemanFacilityMonitoring;

    public static void ApplySlotData(long firstRootNodeSlotData) {
        switch (firstRootNodeSlotData) {
            case /*"apeman_facility_monitoring"*/  0: firstNode = FirstRootNodeChoice.ApemanFacilityMonitoring; break;
            case /*"galactic_dock"*/               1: firstNode = FirstRootNodeChoice.GalacticDock; break;
            case /*"power_reservoir_east"*/        2: firstNode = FirstRootNodeChoice.PowerReservoirEast; break;
            case /*"power_reservoir_central"*/     3: firstNode = FirstRootNodeChoice.PowerReservoirCentral; break;
            case /*"lake_yaochi_ruins"*/           4: firstNode = FirstRootNodeChoice.LakeYaochiRuins; break;
            case /*"yinglong_canal"*/              5: firstNode = FirstRootNodeChoice.YinglongCanal; break;
            case /*"factory_great_hall"*/          6: firstNode = FirstRootNodeChoice.FactoryGreatHall; break;
            case /*"outer_warehouse"*/             7: firstNode = FirstRootNodeChoice.OuterWarehouse; break;
            case /*"grotto_of_scriptures_entry"*/  8: firstNode = FirstRootNodeChoice.GrottoOfScripturesEntry; break;
            case /*"grotto_of_scriptures_east"*/   9: firstNode = FirstRootNodeChoice.GrottoOfScripturesEast; break;
            case /*"grotto_of_scriptures_west"*/  10: firstNode = FirstRootNodeChoice.GrottoOfScripturesWest; break;
        }
    }

    public static Dictionary<FirstRootNodeChoice, TeleportPoint> firstNodeToTPPoint = new Dictionary<FirstRootNodeChoice, TeleportPoint> {
        { FirstRootNodeChoice.ApemanFacilityMonitoring, TeleportPoint.ApemanFacilityMonitoring },
        { FirstRootNodeChoice.GalacticDock, TeleportPoint.GalacticDock },
        { FirstRootNodeChoice.PowerReservoirEast, TeleportPoint.PowerReservoirEast },
        { FirstRootNodeChoice.PowerReservoirCentral, TeleportPoint.PowerReservoirCentral },
        { FirstRootNodeChoice.LakeYaochiRuins, TeleportPoint.LakeYaochiRuins },
        { FirstRootNodeChoice.YinglongCanal, TeleportPoint.YinglongCanal },
        { FirstRootNodeChoice.FactoryGreatHall, TeleportPoint.FactoryGreatHall },
        { FirstRootNodeChoice.OuterWarehouse, TeleportPoint.OuterWarehouse },
        { FirstRootNodeChoice.GrottoOfScripturesEntry, TeleportPoint.GrottoOfScripturesEntry },
        { FirstRootNodeChoice.GrottoOfScripturesEast, TeleportPoint.GrottoOfScripturesEast },
        { FirstRootNodeChoice.GrottoOfScripturesWest, TeleportPoint.GrottoOfScripturesWest },
    };

    public static void SetFirstRootNodeAfterNewGameCreation() {
        var firstTP = firstNodeToTPPoint[firstNode];
        Log.Info($"SetFirstRootNodeAfterNewGameCreation() unlocking {firstTP}");

        if (firstTP != TeleportPoint.ApemanFacilityMonitoring) {
            var afmNode = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(teleportPointToGameFlagPath[TeleportPoint.ApemanFacilityMonitoring]);
            afmNode.unlocked.SetCurrentValue(false);
            Log.Info($"SetFirstRootNodeAfterNewGameCreation() also re-locking AFM");
        }

        var firstTPData = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(teleportPointToGameFlagPath[firstTP]);
        firstTPData.unlocked.SetCurrentValue(true);
    }
}
