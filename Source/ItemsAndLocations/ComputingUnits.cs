using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace ArchipelagoRandomizer;

internal class ComputingUnits {
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

    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    public static GameFlagDescriptable? ApplyComputingUnitToPlayer(Item item, int count, int oldCount) {
        if (item != Item.ComputingUnit)
            return null;

        var apCount = count;
        Log.Info($"ApplyComputingUnit(apCount={apCount})");
        var unshuffledCUs = shopCUs;
        var shuffledCUs = otherCUs;

        var maxAPCUs = shuffledCUs.Length;
        if (apCount < 0 || apCount > maxAPCUs) {
            Log.Error($"ApplyComputingUnit passed {apCount}, but apCount must be between 0 and (on this slot) {maxAPCUs}");
            return null;
        }

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        for (var i = 0; i < shuffledCUs.Length; i++) {
            var flagId = shuffledCUs[i];
            var pad = flagDict[flagId] as PlayerAbilityData;

            //Log.Info($"ApplyComputingUnit setting {i}-th (shuffled by AP) PAD to {(i < apCount)}");
            pad.unlocked.CurrentValue = (i < apCount);
            pad.acquired.CurrentValue = (i < apCount);
            if (i < apCount) {
                padActivate.Invoke(pad, []);
            } else {
                padDeactivate.Invoke(pad, []);
            }
        }

        /*foreach (var id in unshuffledCUs) {
            var pad = flagDict[id] as PlayerAbilityData;
            Log.Info($"ApplyComputingUnit unshuffledCU {id} / {pad.name} is {pad.acquired.CurrentValue}");
        }*/

        var unshuffledCUCount = unshuffledCUs.Sum(flagId =>
            (flagDict[flagId] as PlayerAbilityData)!.acquired.CurrentValue ? 1 : 0);
        var inGameInventoryCount = unshuffledCUCount + apCount;
        //Log.Info($"ApplyComputingUnit unshuffledCUCount={unshuffledCUCount}, inGameInventoryCount={inGameInventoryCount}");

        var cuInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[6];
        cuInventoryItem.unlocked.CurrentValue = (inGameInventoryCount > 0);
        cuInventoryItem.acquired.CurrentValue = (inGameInventoryCount > 0);
        (cuInventoryItem as ItemData)!.ownNum.CurrentValue = inGameInventoryCount;

        return cuInventoryItem;
    }
}
