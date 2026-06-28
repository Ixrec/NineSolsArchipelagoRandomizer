using ArchipelagoRandomizer.Features;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace ArchipelagoRandomizer.Items.ItemImpls;

internal class ComputingUnits {
    public static GameFlagDescriptable GetComputingUnitGFD() => SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[6];

    public static GameFlagDescriptable? GetDisplayGFDFor(Item item) {
        if (item == Item.ComputingUnit)
            return GetComputingUnitGFD();
        return null;
    }

    /*
     * Computing Units need a bunch of specialized code unfortunately. While the CU count displayed in the Inventory menu
     * is just an integer in FlagDict as you'd expect, the max jade cost is not. Instead it's a set of StatModifiers applied
     * at runtime by 8 different PlayerAbilityData objects that each represent only one of the possible CU upgrades.
     * And in order to get the count right, we also need to monitor the CUs that aren't shuffled by AP.
     */

    private static string[] shopCUs = [
        "7e52ba8ef1da6e946806ba1791d92791PlayerAbilityData",
        "5df7e883274aabc489885525a490b5c3PlayerAbilityData",
        "d5b834eb32c40e64f9ce89f5033162c8PlayerAbilityData",
        "d6dc79cb7147ff14aa68f5658a285f22PlayerAbilityData",
    ];
    private static string[] otherCUs = [
        "07577e5ef6cbe394db65f58dfc8f7908PlayerAbilityData",
        "df57aee56d7eec1459ffe946cac8523ePlayerAbilityData",
        "cd52a0bd3cef5634bb0083c19c77e8f3PlayerAbilityData",
        "c191c1bc2afb8d84c870e1858b7ee156PlayerAbilityData",
    ];

    public static bool ApplyComputingUnitToPlayer(Item item, int count, int oldCount) {
        if (item != Item.ComputingUnit)
            return false;

        Log.Info($"ApplyComputingUnit(count={count})");
        var unshuffledCUs = shopCUs;
        var shuffledCUs = otherCUs;
        if (ShopRando.RandomizeShops) {
            unshuffledCUs = [];
            shuffledCUs = shopCUs.Concat(otherCUs).ToArray();
        }

        PlayerAbilityDataList.ApplyPADListItemToPlayer(count, shuffledCUs);

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;
        var unshuffledCUCount = unshuffledCUs.Sum(flagId =>
            (flagDict[flagId] as PlayerAbilityData)!.acquired.CurrentValue ? 1 : 0);
        var inGameInventoryCount = unshuffledCUCount + count;
        //Log.Info($"ApplyComputingUnit unshuffledCUCount={unshuffledCUCount}, inGameInventoryCount={inGameInventoryCount}");

        var cuInventoryItem = GetComputingUnitGFD();
        cuInventoryItem.unlocked.CurrentValue = (inGameInventoryCount > 0);
        cuInventoryItem.acquired.CurrentValue = (inGameInventoryCount > 0);
        (cuInventoryItem as ItemData)!.ownNum.CurrentValue = inGameInventoryCount;

        NotifyAndSave.Default(cuInventoryItem, count, oldCount);
        return true;
    }
}
