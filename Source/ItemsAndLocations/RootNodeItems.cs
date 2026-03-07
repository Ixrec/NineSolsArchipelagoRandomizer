using System;
using System.Collections.Generic;
using System.Text;

namespace ArchipelagoRandomizer.ItemsAndLocations;

internal class RootNodeItems {
    private static Dictionary<Item, TeleportPoints.TeleportPoint> nodeItemToTPoint = new() {
        { Item.OWRootNode, TeleportPoints.TeleportPoint.OuterWarehouse },
        { Item.FGHRootNode, TeleportPoints.TeleportPoint.FactoryGreatHall },
        { Item.AFDRootNode, TeleportPoints.TeleportPoint.ApemanFacilityDepths },
        { Item.PRERootNode, TeleportPoints.TeleportPoint.PowerReservoirEast },
        { Item.RPRootNode, TeleportPoints.TeleportPoint.RadiantPagoda },
        { Item.LYRRootNode, TeleportPoints.TeleportPoint.LakeYaochiRuins },
        { Item.GoSERootNode, TeleportPoints.TeleportPoint.GrottoOfScripturesEast },
    };

    public static bool ApplyNodeToPlayer(Item item, int count, int oldCount) {
        if (!nodeItemToTPoint.ContainsKey(item))
            return false;

        var tpoint = nodeItemToTPoint[item];

        var shouldBeUnlocked = (count > 0);

        var path = TeleportPoints.teleportPointToGameFlagPath[tpoint];
        var unlockedFlag = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(path).unlocked;

        var isUnlocked = unlockedFlag.CurrentValue;
        if (isUnlocked != shouldBeUnlocked) {
            Log.Info($"RootNodeItems::ApplyNodeToPlayer unlocking node for {tpoint}");
            unlockedFlag.SetCurrentValue(shouldBeUnlocked);

            var anomalousRootNodeDbEntry = SingletonBehaviour<UIManager>.Instance.allPediaCollections[2].rawCollection[5];
            NotifyAndSave.WithCustomText(anomalousRootNodeDbEntry, $"Collected {ItemNames.itemNames[item]}", count, oldCount);
        }

        return true;
    }
}
