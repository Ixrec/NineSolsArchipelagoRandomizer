using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace ArchipelagoRandomizer;

internal class PipeVials {
    /*
     * Pipe Vials follow the same pattern as Computing Units.
     */

    private static string[] shopPVs = [
        "5c864e36f2cef9d4f8c24c0aa84010bePlayerAbilityData", // (升級) 煙斗使用次數_Lv1_議會販賣機
        "3f6040257515a41499d36e910a4a6e79PlayerAbilityData", // (升級) 煙斗使用次數_Lv2_議會販賣機
        "ec1fe3b64944cd643b2020f35e82f023PlayerAbilityData", // (升級) 煙斗使用次數_Lv3_議會販賣機
        "dd5adfbe0ac50fe4795b796153ee646dPlayerAbilityData", // (升級) 煙斗使用次數_Lv4_議會販賣機
    ];
    private static string[] otherPVs = [
        "ce7166af4ef39d7468c4ccc464fd90b9PlayerAbilityData", // (升級) 煙斗使用次數_AG_SG1
        "41c960dfcaaf7a14f813342db16f0481PlayerAbilityData", // (升級) 煙斗使用次數_A3SG4日昇樓內 (= Inside Daybreak Tower)
    ];

    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    public static bool ApplyPipeVialToPlayer(Item item, int count, int oldCount) {
        if (item != Item.PipeVial)
            return false;

        var apCount = count;
        Log.Info($"ApplyPipeVial(apCount={apCount})");
        var unshuffledPVs = shopPVs;
        var shuffledPVs = otherPVs;

        var maxAPPVs = shuffledPVs.Length;
        if (apCount < 0 || apCount > maxAPPVs) {
            Log.Error($"ApplyPipeVial passed {apCount}, but apCount must be between 0 and (on this slot) {maxAPPVs}");
            return false;
        }

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        for (var i = 0; i < shuffledPVs.Length; i++) {
            var flagId = shuffledPVs[i];
            var pad = flagDict[flagId] as PlayerAbilityData;
            if (pad == null)
                continue;

            //Log.Info($"ApplyPipeVial setting {i}-th (shuffled by AP) PAD to {(i < apCount)}");
            pad.unlocked.CurrentValue = (i < apCount);
            pad.acquired.CurrentValue = (i < apCount);
            if (i < apCount) {
                padActivate.Invoke(pad, []);
            } else {
                padDeactivate.Invoke(pad, []);
            }
        }

        /*foreach (var id in unshuffledPVs) {
            var pad = flagDict[id] as PlayerAbilityData;
            Log.Info($"ApplyPipeVial unshuffledPV {id} / {pad.name} is {pad.acquired.CurrentValue}");
        }*/

        var unshuffledPVCount = unshuffledPVs.Sum(flagId =>
            (flagDict[flagId] as PlayerAbilityData)!.acquired.CurrentValue ? 1 : 0);
        var inGameInventoryCount = unshuffledPVCount + apCount;
        //Log.Info($"ApplyPipeVial unshuffledPVCount={unshuffledPVCount}, inGameInventoryCount={inGameInventoryCount}");

        var cuInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[10];
        cuInventoryItem.unlocked.CurrentValue = (inGameInventoryCount > 0);
        cuInventoryItem.acquired.CurrentValue = (inGameInventoryCount > 0);
        (cuInventoryItem as ItemData)!.ownNum.CurrentValue = inGameInventoryCount;

        NotifyAndSave.Default(cuInventoryItem, count, oldCount);
        return true;
    }
}
