using ArchipelagoRandomizer.Locations;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer.Features;

[HarmonyPatch]
internal class ShopRando {
    public static bool RandomizeShops = false; // a lot of files need to know this, and it's easier to make it public in one place than keep reimplementing this

    public static void ApplySlotData(long? randomizeShops) {
        RandomizeShops = ((randomizeShops ?? 0) == 1);
    }

    private static Dictionary<string, Location> merchDataNameToLocation = new Dictionary<string, Location> {
        // Kuafu's shop / `UpgradeEntries` (including extra/"二階" inventory)
        { "Merchandise_0_MaxAmmo value LV1", Location.SHOP_KUAFU_NORMAL_1 }, // Azure Sand Magazine (1/3) - 1000
        { "Merchandise_0_MaxAmmo value LV2", Location.SHOP_KUAFU_NORMAL_2 }, // Azure Sand Magazine (2/3) - 2500
        { "Merchandise_0_MaxAmmo value LV3_二階", Location.SHOP_KUAFU_EXTRA_1 }, // Azure Sand Magazine (3/3) - 4000
        { "Merchandise_2_1_貫穿箭LV2", Location.SHOP_KUAFU_DARK_STEEL_1 }, // Cloud Piercer S - 1000
        { "Merchandise_2_2_貫穿箭LV3_二階", Location.SHOP_KUAFU_EXTRA_DARK_STEEL_1 }, // Cloud Piercer X - 2500
        //{ "Merchandise_3_1_爆破箭LV1", null }, // Thunder Buster // not a "real" shop slot, this exchange is done in dialogue, see FSP_KUAFU_THUNDERBURST_BOMB location
        { "Merchandise_3_2_爆破箭LV2", Location.SHOP_KUAFU_DARK_STEEL_2 }, // Thunder Buster S - 1000
        { "Merchandise_3_3_爆破箭LV3_二階", Location.SHOP_KUAFU_EXTRA_DARK_STEEL_2 }, // Thunder Buster X - 2500
        //{ "Merchandise_4_1_追蹤箭LV1", null }, // Shadow Hunter // not a "real" shop slot, this exchange is done in dialogue, see FSP_KUAFU_HOMING_DARTS location
        { "Merchandise_4_2_追蹤箭LV2", Location.SHOP_KUAFU_DARK_STEEL_3 }, // Shadow Hunter S - 1000
        { "Merchandise_4_3_追蹤箭LV3_二階", Location.SHOP_KUAFU_EXTRA_DARK_STEEL_3 }, // Shadow Hunter X - 2500
        { "Merchandise_5_Potion value LV1", Location.SHOP_KUAFU_HERB_CATALYST_1 }, // Pipe Upgrade (1/8) - 800
        { "Merchandise_5_Potion value LV2", Location.SHOP_KUAFU_HERB_CATALYST_2 }, // Pipe Upgrade (2/8) - 800
        { "Merchandise_5_Potion value LV3", Location.SHOP_KUAFU_HERB_CATALYST_3 }, // Pipe Upgrade (3/8) - 800
        { "Merchandise_5_Potion value LV4", Location.SHOP_KUAFU_HERB_CATALYST_4 }, // Pipe Upgrade (4/8) - 800
        { "Merchandise_5_Potion value LV5", Location.SHOP_KUAFU_HERB_CATALYST_5 }, // Pipe Upgrade (5/8) - 800
        { "Merchandise_5_Potion value LV6", Location.SHOP_KUAFU_HERB_CATALYST_6 }, // Pipe Upgrade (6/8) - 800
        { "Merchandise_5_Potion value LV7", Location.SHOP_KUAFU_HERB_CATALYST_7 }, // Pipe Upgrade (7/8) - 800
        { "Merchandise_5_Potion value LV8", Location.SHOP_KUAFU_HERB_CATALYST_8 }, // Pipe Upgrade (8/8) - 800
        { "Merchandise_6_0_咒滅化緣", Location.SHOP_KUAFU_NORMAL_3 }, // Transmute Unto Wealth - 1000
        { "Merchandise_6_1_咒滅化生", Location.SHOP_KUAFU_NORMAL_4 }, // Transmute Unto Life - 1000
        { "Merchandise_6_2_咒滅化息_二階", Location.SHOP_KUAFU_EXTRA_2 }, // Transmute Unto Qi - 1250

        // Chiyou's shop / `ShopItemDataCollection_蚩尤商品清單` (here "二階" means after he moves into FSP; "一階" is out of logic)
        { "(商品)0_蚩尤一階_恢復玉", Location.SHOP_CHIYOU_LOW_1 }, // Recovery Jade - 1000
        { "(商品)0_蚩尤一階_竊命玉", Location.SHOP_CHIYOU_LOW_2 }, // Health Thief Jade - 600
        { "(商品)0_蚩尤一階_背水玉", Location.SHOP_CHIYOU_HIGH_1 }, // Last Stand Jade - 2000
        { "(商品)0_蚩尤一階_速藥玉", Location.SHOP_CHIYOU_MEDIUM_1 }, // Quick Dose Jade - 1250
        { "(商品)0_蚩尤二階_奉還玉", Location.SHOP_CHIYOU_HIGH_2 }, // Reciprocation Jade - 4000
        //{ "(商品)0_蚩尤二階_替死玉", null }, // Revival Jade - 2000 // only purchaseable if you missed the bridge encounter where he gifts it, i.e. FGH_CHIYOU_BRIDGE
        { "(商品)1_蚩尤一階_算力元件_1", Location.SHOP_CHIYOU_LOW_3 }, // Computing Unit (1/4) - 1000
        { "(商品)1_蚩尤一階_算力元件_2", Location.SHOP_CHIYOU_MEDIUM_2 }, // Computing Unit (2/4) - 1800
        { "(商品)1_蚩尤二階_算力元件_3", Location.SHOP_CHIYOU_HIGH_3 }, // Computing Unit (3/4) - 3500
        { "(商品)1_蚩尤二階_算力元件_4", Location.SHOP_CHIYOU_HIGH_4 }, // Computing Unit (4/4) - 5000
        { "(商品)2_蚩尤一階_藥材_1", Location.SHOP_CHIYOU_LOW_4 }, // Ball of Flavor - 600
        { "(商品)2_蚩尤一階_藥材_2", Location.SHOP_CHIYOU_LOW_5 }, // Dragon's Whip - 600
        { "(商品)2_蚩尤一階_藥材_3", Location.SHOP_CHIYOU_MEDIUM_3 }, // Necroceps - 1200
        { "(商品)2_蚩尤一階_藥材_5", Location.SHOP_CHIYOU_MEDIUM_4 }, // Guiseng - 1200 // looks like _4 is skipped?
        { "(商品)2_蚩尤二階_藥材_6", Location.SHOP_CHIYOU_MEDIUM_5 }, // Thunder Centipede - 1800
        { "(商品)2_蚩尤二階_藥材_7", Location.SHOP_CHIYOU_MEDIUM_6 }, // Wall-climbing Gecko - 1800
        { "(商品)2_蚩尤二階_藥材_8", Location.SHOP_CHIYOU_HIGH_5 }, // Gutwrench Fruit - 3600

        // 3D Printer 'shop' / `AGS2_議會_販賣機_商品清單`
        { "(商品)議會販賣機_0_1_反彈玉", Location.SHOP_PRINTER_LOW_1 }, // Ricochet Jade - 500
        { "(商品)議會販賣機_0_2_藥學玉", Location.SHOP_PRINTER_LOW_2 }, // Medical Jade - 500
        { "(商品)議會販賣機_0_3_群咒玉 陽", Location.SHOP_PRINTER_LOW_3 }, // Mob Quell Jade - Yang - 750
        { "(商品)議會販賣機_1_藥斗數量 1", Location.SHOP_PRINTER_LOW_4 }, // Pipe Vial - 1000
        { "(商品)議會販賣機_1_藥斗數量 2", Location.SHOP_PRINTER_MEDIUM_1 }, // Pipe Vial - 2000
        { "(商品)議會販賣機_1_藥斗數量 3", Location.SHOP_PRINTER_HIGH_1 }, // Pipe Vial - 4000
        { "(商品)議會販賣機_1_藥斗數量 4", Location.SHOP_PRINTER_HIGH_2 }, // Pipe Vial - 6000
        // "(商品)議會販賣機_2_軒軒紀念幣" is probably Shuanshuan's Coin, which we aren't randomizing
    };

    public static void EnsureShopsScouted() {
        Log.Info($"ShopRando::EnsureShopsScouted()");
        List<long> shopLocationIds = merchDataNameToLocation.Values.Select(loc => LocationNames.locationToArchipelagoId[loc]).ToList();
        LocationScouter.ScoutLocations(shopLocationIds);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ShopUIPanel), "ShowInit")]
    static void ShopUIPanel_ShowInit(ShopUIPanel __instance) {
        if (
            __instance.name == "UpgradeTable" || /* Kuafu */
            __instance.name == "ShopTable" /* Chiyou & 3D Printer */
        ) {
            // Even though this happens a split second before the shop UI renders, it's still fast enough in testing
            // that the shop UI has all the scouts we need by the time the other patch methods get called.
            EnsureShopsScouted();
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseData), "Trade")]
    static bool MerchandiseData_Trade(MerchandiseData __instance) {
        if (!RandomizeShops)
            return true;

        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return true;

        Log.Info($"MerchandiseData_Trade patch intercepting trade of {__instance.name} because it's AP location {location}");

        // Check the location *before* deducting the price, so the worst case failure state is
        // the player having some extra money, rather than missing a now-uncheckable location.
        LocationTriggers.CheckLocation(location);

        // copy-pasted from vanilla impl of Trade()
        if (__instance.numLeftToBuy.CurrentValue > 0) {
            __instance.numLeftToBuy.CurrentValue--;
        }
        __instance.currencyRequirement.ConsumeCheck();
        __instance.ConsumeMaterials();
        // this part remains commented out because we don't want to give the player the vanilla item in this shop slot
        //    if (item != null) {
        //        item.PlayerPicked();
        //    }
        //    OutcomeResult.Receive();

        return false; // skip vanilla impl
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "Title", MethodType.Getter)]
    static void MerchandiseData_Title(MerchandiseData __instance, ref string __result) {
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        //Log.Warning($"MerchandiseData_Title patch for {name} / {location}");
        //__result = $"{location}";
        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            string itemColor;
            // hex values copied from user.kv
            if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
                itemColor = "AF99EF";
            } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
                itemColor = "6D8BE8";
            } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
                itemColor = "FA8072";
            } else {
                itemColor = "00EEEE";
            }
            __result = $"<color=#EE00EE>{scoutedItemInfo.Player.Name}</color>'s <color=#{itemColor}>{scoutedItemInfo.ItemDisplayName}</color>";
        } else {
            __result = $"<color=red>ERROR: Location Not Scouted</color>";
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "Description", MethodType.Getter)]
    static void MerchandiseData_Description(MerchandiseData __instance, ref string __result) {
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        //Log.Warning($"MerchandiseData_Description patch for {name} / {location}");
        //__result = $"This is a randomized shop slot for Archipelago location {location}";
        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            var receivingPlayer = scoutedItemInfo.Player.Name;
            if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
                __result = $"{receivingPlayer} needs this item to make progress.";
            } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
                __result = $"This item would help {receivingPlayer}, but it's not essential.";
            } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
                __result = $"{receivingPlayer} would rather never receive this item. But as we all know: heroes are forged in agony.";
            } else {
                __result = $"{receivingPlayer} probably doesn't care about this item at all.";
            }
        } else {
            __result = $"For some reason, Archipelago location {location} has not been scouted. " +
                $"You can still purchase/check this location if you want, but we don't know what item you'll get." +
                $"\n\nThis is probably Eigong's fault somehow.";
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "IsRevealed", MethodType.Getter)]
    static void MerchandiseData_IsRevealed(MerchandiseData __instance, ref bool __result) {
        var name = __instance.name;
        if (name == "(商品)0_蚩尤二階_替死玉") // Revival Jade only purchaseable if you missed the bridge encounter where he gifts it, i.e. FGH_CHIYOU_BRIDGE
            __result = false; // in rando we don't want a "duplicate Revival Jade" to appear depending on shop unlocks, so force this to stay hidden

        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        //Log.Warning($"MerchandiseData_IsRevealed patch for {name} / {location}");
        __result = true;
    }
}