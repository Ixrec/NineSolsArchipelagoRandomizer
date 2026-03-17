using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace ArchipelagoRandomizer;

internal class Arrows {
    private static string[] maxAmmoIncreases = [
        "eb5ef12f4ef9e46eeb09809070d21db4PlayerAbilityData", // (KuaFoo) MaxAmmo Lv1
        "4f3107713e9dd43fc9968aa6579207c9PlayerAbilityData", // (KuaFoo) MaxAmmo Lv2
        "072576f6cb93e4921b287b4c50140e22PlayerAbilityData", // (KuaFoo) MaxAmmo Lv3
    ];

    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    public static bool ApplyArrowToPlayer(Item item, int count, int oldCount) {
        if (item == Item.AzureSandMagazine) {
            var inventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[8]; // ff9acceeaed756043976f6a3edc9d40fItemData
            inventoryItem.acquired.SetCurrentValue(count > 0);
            inventoryItem.unlocked.SetCurrentValue(count > 0);
            ((ItemData)inventoryItem).ownNum.SetCurrentValue(count);

            var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;
            for (var i = 0; i < maxAmmoIncreases.Length; i++) {
                var flagId = maxAmmoIncreases[i];
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

            return true;
        }

        string? arrowPWDFlag = null;
        switch (item) {
            // Note these are the "level 1" flags, there are others for levels 2 and 3
            case Item.ArrowCloudPiercer: arrowPWDFlag = "7837bd6bb550641d8a9f30492603c5eePlayerWeaponData"; break;
            case Item.ArrowShadowHunter: arrowPWDFlag = "3949bc0edba197d459f5d2d7f15c72e0PlayerWeaponData"; break;
            case Item.ArrowThunderBuster: arrowPWDFlag = "ef8f7eb3bcd7b444f80d5da539f3b133PlayerWeaponData"; break;
            default: break;
        }
        if (arrowPWDFlag != null) {
            var arrowPWD = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[arrowPWDFlag];
            arrowPWD.acquired?.SetCurrentValue(count > 0); // .unlocked and .equipped appear to be unnecessary

            // not worth trying to figure out if the bow "should" be disabled, since this can't happen in practice anyway
            if (count > 0) {
                EnableAzureBow(true);
            }

            NotifyAndSave.Default(arrowPWD, count, oldCount);
            return true;
        }
        return false;
    }

    private static void EnableAzureBow(bool enable) {
        Log.Info($"EnableAzureBow(enable={enable})");

        // This is the actual in-game bow-firing ability
        Player.i.mainAbilities.ArrowAbility.acquired?.SetCurrentValue(enable);
        // note that "2efd376b4493d40fca29f9e3d49669e9PlayerWeaponData" is the same PWD object as ArrowAbility, just two different ways of getting to it

        // These are additional flags that only matter in the pause menus, but are meant to go along with the bow
        var azureBowInventoryEntry = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["49f2fd2c691313f47970b15b58279418ItemData"];
        var azureSandInventoryEntry = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["25b45e1c416880d41a1f1444e45c24d2ItemData"];
        var azureBowOnStatusScreen = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["a68fe303d0077264aa66218d3900f0edItemData"];
        foreach (var itemData in new ItemData[] { azureBowInventoryEntry, azureSandInventoryEntry, azureBowOnStatusScreen }) {
            itemData.acquired?.SetCurrentValue(enable);
            itemData.unlocked?.SetCurrentValue(enable);
            itemData.ownNum.SetCurrentValue(enable ? 1 : 0);
        }
    }
}
