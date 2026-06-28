using HarmonyLib;
using System;
using System.Reflection;

namespace ArchipelagoRandomizer.Items;

/*
 * PlayerAbilityData is a reasonably intuitive class when it's used for one singular ability, independent of other "abilities".
 * But several non-unique items in this game are modeled as a sequence of PADs, one for each level/upgrade.
 * 
 * For example, in order to give the player "3 computing units" (regardless of their current CU count), 
 * there is unfortuantely no single flag we can simply set to the number 3.
 * Instead we must gather the full list of PADs representing CUs, call Activate() on the first 3, and DeActivate() on all the others.
 * 
 * This class exists to help model that common pattern.
 */

internal class PlayerAbilityDataList {
    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    public static void ApplyPADListItemToPlayer(int count, string[] padList) {
        Log.Info($"ApplyPADListItemToPlayer newCount={count}, padList={padList})");

        var padCount = padList.Length;
        if (count < 0 || count > padCount) {
            Log.Warning($"ApplyPADListItemToPlayer was passed {count}, which must be between 0 and {padCount}; clamping it");
            count = Math.Clamp(count, 0, padCount);
        }

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        for (var i = 0; i < padList.Length; i++) {
            var flagId = padList[i];
            var pad = flagDict[flagId] as PlayerAbilityData;
            if (pad == null)
                continue;

            pad.unlocked.CurrentValue = (i < count);
            pad.acquired.CurrentValue = (i < count);
            if (i < count) {
                padActivate.Invoke(pad, []);
            } else {
                padDeactivate.Invoke(pad, []);
            }
        }
    }
}
