using HarmonyLib;
using System.Reflection;

namespace ArchipelagoRandomizer.Items.ItemImpls;

internal class PipeUpgrades {
    public static GameFlagDescriptable GetPipeUpgradeGFD() => (PlayerAbilityData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[PUs[0]];

    public static GameFlagDescriptable? GetDisplayGFDFor(Item item) {
        if (item == Item.PipeUpgrade)
            return GetPipeUpgradeGFD();
        return null;
    }

    /*
     * Pipe Upgrades follow the same pattern as Computing Units and Pipe Vials, but significantly simpler because:
     * - they don't have an inventory item
     * - either all the PUs are AP items or none of them are
     */

    private static string[] PUs = [
        "c3e9631e6805c704f8c3fb1d1d60d78fPlayerAbilityData", // (KuaFoo) Potion value LV1
        "960072fcea97cb8438297365d3db963cPlayerAbilityData", // (KuaFoo) Potion value LV2
        "cb11b23d6a0659f418937331d46de6fcPlayerAbilityData", // (KuaFoo) Potion value LV3
        "0c1ddf20ca0b26447895b50183aebae9PlayerAbilityData", // (KuaFoo) Potion value LV4
        "89f506003825f404eac747bbb19560ccPlayerAbilityData", // (KuaFoo) Potion value LV5
        "075eabd7421b58e43af25cc1c57e79e3PlayerAbilityData", // (KuaFoo) Potion value LV6
        "cf5080950d381d843b000c91175434bfPlayerAbilityData", // (KuaFoo) Potion value LV7
        "05b87ad6d7c226245b6e917ec21d3416PlayerAbilityData", // (KuaFoo) Potion value LV8
    ];

    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    public static bool ApplyPipeUpgradeToPlayer(Item item, int count, int oldCount) {
        if (item != Item.PipeUpgrade)
            return false;

        var apCount = count;
        Log.Info($"ApplyPipeUpgrade(apCount={apCount})");

        var maxAPPUs = PUs.Length;
        if (apCount < 0 || apCount > maxAPPUs) {
            Log.Error($"ApplyPipeUpgrade passed {apCount}, but apCount must be between 0 and {maxAPPUs}");
            return false;
        }

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        for (var i = 0; i < PUs.Length; i++) {
            var flagId = PUs[i];
            var pad = flagDict[flagId] as PlayerAbilityData;
            if (pad == null)
                continue;

            //Log.Info($"ApplyPipeUpgrade setting {i}-th (shuffled by AP) PAD to {(i < apCount)}");
            pad.unlocked.CurrentValue = (i < apCount);
            pad.acquired.CurrentValue = (i < apCount);
            if (i < apCount) {
                padActivate.Invoke(pad, []);
            } else {
                padDeactivate.Invoke(pad, []);
            }
        }

        NotifyAndSave.WithCustomText(GetPipeUpgradeGFD(), "Collected Pipe Upgrade" /* without the "Lv.1" part*/, count, oldCount);
        return true;
    }
}
