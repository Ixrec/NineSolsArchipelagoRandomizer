using Archipelago.MultiClient.Net.Models;
using ArchipelagoRandomizer.Features;
using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.Locations;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class DarkSteelForcedPurchase {
    // 0 = vanilla, 1 = medium, 2 = ledge_storage
    private static long LogicDifficulty = 1;

    public static void ApplySlotData(long? logicDifficulty) {
        LogicDifficulty = logicDifficulty ?? 0;
    }

    // we only want to force the player to spend Dark Steel on CPS if they're at risk of "missing" a logically required CPS barrier skip
    private static bool ShouldBlock_ShopRandoOff(MerchandiseData __instance) {
        // vanilla logic doesn't expect bow tricks
        if (LogicDifficulty == 0)
            return false;

        // if this shop slot doesn't require a Dark Steel, then it's not relevant
        var entries = __instance.requireMaterialEntriesToBuy;
        if (entries.Count != 1)
            return false;
        var entry = entries[0];
        if (entry?.item.Title != "Dark Steel")
            return false;

        // CPS is only logically relevant as a substitute for Charged Strike, so if the player already has CS then we don't care about blocking shop items any more
        if (Player.i.mainAbilities.ChargedAttackAbility.IsAcquired)
            return false;

        // don't block CPS itself from being purchased, since the whole point is to prevent the player from "missing" CPS
        if (__instance.name == "Merchandise_2_1_貫穿箭LV2")
            return false;

        // if CPS already has been purchased, then we don't care about blocking shop items any more
        var cloudPiercerS = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["2f7009a00edd57c4fa4332ffcd15396aPlayerWeaponData"];
        if (cloudPiercerS.IsAcquired)
            return false;

        // if the player has multiple unspent Dark Steels, then it's safe to let them buy a non-CPS upgrade before CPS
        var darkSteelCount = GetRemainingDarkSteelCount();
        if (darkSteelCount > 1)
            return false;

        return true;
    }

    // TODO: non-English? do we need to use save ids here?
    private static bool IsDarkSteelPurchase(MerchandiseData __instance) {
        var entries = __instance.requireMaterialEntriesToBuy;
        if (entries.Count != 1)
            return false;

        var entry = entries[0];
        return (entry?.item.Title == "Dark Steel");
    }

    private static bool IsHerbCatalystPurchase(MerchandiseData __instance) {
        var entries = __instance.requireMaterialEntriesToBuy;
        if (entries.Count != 1)
            return false;
        var entry = entries[0];
        return (entry?.item.Title == "Herb Catalyst");
    }

    private static int GetRemainingDarkSteelCount() {
        var darkSteelInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[2].rawCollection[13];
        return ((ItemData)darkSteelInventoryItem).ownNum.CurrentValue;
    }
    private static int GetRemainingHerbCatalystCount() {
        var herbCatalystInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[2].rawCollection[14];
        return ((ItemData)herbCatalystInventoryItem).ownNum.CurrentValue;
    }

    private static List<Location> DarkSteelPurchases = [
        Location.SHOP_KUAFU_DARK_STEEL_1,
        Location.SHOP_KUAFU_DARK_STEEL_2,
        Location.SHOP_KUAFU_DARK_STEEL_3,
        Location.SHOP_KUAFU_EXTRA_DARK_STEEL_1,
        Location.SHOP_KUAFU_EXTRA_DARK_STEEL_2,
        Location.SHOP_KUAFU_EXTRA_DARK_STEEL_3,
    ];
    private static SerializableItemInfo[]? GetScoutsForDarkSteelPurchases() {
        var scouts = APSaveManager.CurrentAPSaveData?.scoutedLocations;
        if (scouts == null)
            return null;
        return DarkSteelPurchases.Select(location => scouts[location]).ToArray();
    }

    private static List<Location> HerbCatalystPurchases = [
        Location.SHOP_KUAFU_HERB_CATALYST_1,
        Location.SHOP_KUAFU_HERB_CATALYST_2,
        Location.SHOP_KUAFU_HERB_CATALYST_3,
        Location.SHOP_KUAFU_HERB_CATALYST_4,
        Location.SHOP_KUAFU_HERB_CATALYST_5,
        Location.SHOP_KUAFU_HERB_CATALYST_6,
        Location.SHOP_KUAFU_HERB_CATALYST_7,
        Location.SHOP_KUAFU_HERB_CATALYST_8,
    ];
    private static SerializableItemInfo[]? GetScoutsForHerbCatalystPurchases() {
        var scouts = APSaveManager.CurrentAPSaveData?.scoutedLocations;
        if (scouts == null)
            return null;
        return HerbCatalystPurchases.Select(location => scouts[location]).ToArray();
    }

    // we only force the player to spend Dark Steel / Herb Catalysts on early progression if they're at risk of "missing" a logically required DS/HC purchase
    private static bool ShouldBlock_ShopRandoOn(MerchandiseData __instance) {
        // The exact criteria we want to implement:
        // - if the player has M DSs/HCs left to spend,
        // - has received N DSs/HCs so far,
        // - and the first N shop slots for DS/HC contain M unbought *progression* items,
        // then
        // - all non-progression DS/HC slots become unpurchaseable
        // - all progression DS/HC slots after N become unpurchaseable

        // this isn't even a randomized shop slot
        if (!ShopRando.merchDataNameToLocation.TryGetValue(__instance.name, out var thisLocation))
            return false;

        bool isDS = IsDarkSteelPurchase(__instance);
        bool isHC = IsHerbCatalystPurchase(__instance);
        // this isn't one of the DS/HC shop slots we need to worry about blocking
        if (!isDS && !isHC)
            return false;

        int apReceivedCount = InMemoryInventory.ApInventory.GetValueOrDefault((isDS ? Item.DarkSteel : Item.HerbCatalyst), 0);

        var relevantScouts = (isDS ? GetScoutsForDarkSteelPurchases() : GetScoutsForHerbCatalystPurchases());
        var scoutsInLogic = relevantScouts.Take(apReceivedCount); // since our DS/HC generation logic says the Nth slot takes N DSs/HCs
        var unboughtProgressionInLogic = scoutsInLogic
            .Where(scout => {
                var location = LocationNames.archipelagoIdToLocation[scout.LocationId];
                var isChecked = APSaveManager.CurrentAPSaveData?.locationsChecked?.GetValueOrDefault(location.ToString(), false) ?? false;
                return !isChecked;
            })
            .Where(scout => scout.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement));
        var unboughtProgInLogicCount = unboughtProgressionInLogic.Count();

        int remainingMaterialCount = (isDS ? GetRemainingDarkSteelCount() : GetRemainingHerbCatalystCount());
        var blockingRequired = (unboughtProgInLogicCount >= remainingMaterialCount);
        if (!blockingRequired)
            return false;

        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(thisLocation, out var thisScout) ?? false) {
            var thisIsProgression = thisScout.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement);
            if (!thisIsProgression)
                return true; // the remaining DSs/HCs must go to progression items first

            var thisIndex = (isDS ? DarkSteelPurchases : HerbCatalystPurchases).FindIndex(loc => loc == thisLocation);
            if (thisIndex >= apReceivedCount)
                return true; // the remaining DSs/HCs must go to earlier items first
        }
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), nameof(MerchandiseData.HasEnoughMaterial))]
    public static void MerchandiseData_HasEnoughMaterial(MerchandiseData __instance, ref bool __result) {
        if (ShopRando.RandomizeShops) {
            if (ShouldBlock_ShopRandoOn(__instance)) {
                Log.Info($"MerchandiseData_HasEnoughMaterial patch blocking purchase of '{__instance.Title}' until more DS/HC items have been received");
                __result = false;
            }
        } else {
            if (ShouldBlock_ShopRandoOff(__instance)) {
                Log.Info($"MerchandiseData_HasEnoughMaterial patch blocking purchase of '{__instance.Title}' until Cloud Piercer S has been purchased");
                __result = false;
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "Description", MethodType.Getter)]
    static void MerchandiseData_get_Description(MerchandiseData __instance, ref string __result) {
        if (ShopRando.RandomizeShops) {
            if (ShouldBlock_ShopRandoOn(__instance)) {
                var itemName = IsDarkSteelPurchase(__instance) ? "Dark Steel" : "Herb Catalyst";

                __result = $"{LoadingScreenTips.apRainbow}: " +
                    $"\n" +
                    $"Because you're playing with shop randomization, " +
                    $"and you only have enough {itemName}s left to buy the in-logic shop locations with progression items, " +
                    $"<color=orange>you must spend your remaining {itemName}s on the in-logic shop locations with progression items</color>. " +
                    $"\n\n" +
                    __result;
            }
        } else {
            if (ShouldBlock_ShopRandoOff(__instance)) {
                __result = $"{LoadingScreenTips.apRainbow}: " +
                    "\n" +
                    "Because you're playing on medium or higher logic difficulty, " +
                    "you don't have Charged Strike yet, " +
                    "and you only have one Dark Steel available right now, " +
                    "<color=orange>you must spend your last Dark Steel on Cloud Piercer S</color>. " +
                    "\n\n" +
                    __result;
            }
        }
    }
}
