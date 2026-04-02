using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace ArchipelagoRandomizer.Items.ItemImpls;

internal class Arrows {
    private static string[] maxAmmoIncreases = [
        "eb5ef12f4ef9e46eeb09809070d21db4PlayerAbilityData", // (KuaFoo) MaxAmmo Lv1
        "4f3107713e9dd43fc9968aa6579207c9PlayerAbilityData", // (KuaFoo) MaxAmmo Lv2
        "072576f6cb93e4921b287b4c50140e22PlayerAbilityData", // (KuaFoo) MaxAmmo Lv3
    ];

    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    private static string pwdCloudPiercer = "7837bd6bb550641d8a9f30492603c5eePlayerWeaponData";
    private static string pwdCloudPiercerS = "2f7009a00edd57c4fa4332ffcd15396aPlayerWeaponData"; // (Weapon)1 穿雲箭_LV2
    private static string pwdCloudPiercerX = "9dfa4667af28b6a4da8c443c9814e40dPlayerWeaponData"; // (Weapon)1 穿雲箭_LV3
    private static string pwdThunderBuster = "ef8f7eb3bcd7b444f80d5da539f3b133PlayerWeaponData";
    private static string pwdThunderBusterS = "b4b36f48e6a6ec849a613f2fdeda1a2dPlayerWeaponData"; // (Weapon)2 爆破箭_LV2
    private static string pwdThunderBusterX = "4b323612d5dc8bd49b3fd4508d7b485bPlayerWeaponData"; // (Weapon)2 爆破箭_LV3
    private static string pwdShadowHunter = "3949bc0edba197d459f5d2d7f15c72e0PlayerWeaponData";
    private static string pwdShadowHunterS = "11df21b39de54f9479514d7135be8d57PlayerWeaponData"; // (Weapon)3 追蹤箭_LV2
    private static string pwdShadowHunterX = "a9402e3a9e1e04f4488265f1c6d42641PlayerWeaponData"; // (Weapon)3 追蹤箭_LV3

    public static GameFlagDescriptable? GetDisplayGFDFor(Item item) {
        string? flag;
        switch (item) {
            case Item.ArrowCloudPiercer:
            case Item.ProgressiveCloudPiercer:
                flag = pwdCloudPiercer;
                break;
            case Item.ArrowThunderBuster:
            case Item.ProgressiveThunderBuster:
                flag = pwdThunderBuster;
                break;
            case Item.ArrowShadowHunter:
            case Item.ProgressiveShadowHunter:
                flag = pwdShadowHunter;
                break;
            case Item.AzureSandMagazine:
                return SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[8]; // ff9acceeaed756043976f6a3edc9d40fItemData
            default: return null;
        }
        return (flag == null) ? null : (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[flag];
    }

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

        // the arrow situation is complicated enough I'm not going to try supporting disabling/rolling back arrow upgrades
        string[] arrowPWDFlags = [];
        switch (item) {
            case Item.ArrowCloudPiercer: arrowPWDFlags = [pwdCloudPiercer]; break;
            case Item.ArrowThunderBuster: arrowPWDFlags = [pwdThunderBuster]; break;
            case Item.ArrowShadowHunter: arrowPWDFlags = [pwdShadowHunter]; break;

            case Item.ProgressiveCloudPiercer:
                if (count >= 3)
                    arrowPWDFlags = [pwdCloudPiercer, pwdCloudPiercerS, pwdCloudPiercerX];
                else if (count == 2)
                    arrowPWDFlags = [pwdCloudPiercer, pwdCloudPiercerS];
                else if (count == 1)
                     arrowPWDFlags = [pwdCloudPiercer];
                break;

            case Item.ProgressiveThunderBuster:
                if (count >= 3)
                    arrowPWDFlags = [pwdThunderBuster, pwdThunderBusterS, pwdThunderBusterX];
                else if (count == 2)
                    arrowPWDFlags = [pwdThunderBuster, pwdThunderBusterS];
                else if (count == 1)
                    arrowPWDFlags = [pwdThunderBuster];
                break;

            case Item.ProgressiveShadowHunter:
                if (count >= 3)
                    arrowPWDFlags = [pwdShadowHunter, pwdShadowHunterS, pwdShadowHunterX];
                else if (count == 2)
                    arrowPWDFlags = [pwdShadowHunter, pwdShadowHunterS];
                else if (count == 1)
                    arrowPWDFlags = [pwdShadowHunter];
                break;

            default: break;
        }

        if (arrowPWDFlags.Length > 0) {
            foreach (var flag in arrowPWDFlags) {
                var arrowPWD = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[flag];
                arrowPWD.acquired?.SetCurrentValue(count > 0); // .unlocked and .equipped appear to be unnecessary

                // not worth trying to figure out if the bow "should" be disabled, since this can't happen in practice anyway
                if (count > 0) {
                    EnableAzureBow(true);
                }
            }

            var lastFlag = arrowPWDFlags[arrowPWDFlags.Length - 1];
            var lastPWD = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[lastFlag];
            NotifyAndSave.Default(lastPWD, count, oldCount);
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
