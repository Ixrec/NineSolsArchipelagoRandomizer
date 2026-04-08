using Archipelago.MultiClient.Net.Models;
using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.Items.ItemImpls;
using ArchipelagoRandomizer.Locations;
using HarmonyLib;
using RCGMaker.AddressableAssets;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer.Features;

[HarmonyPatch]
internal class ShopRando {
    public static bool RandomizeShops = false; // a lot of files need to know this, and it's easier to make it public in one place than keep reimplementing this

    public static void ApplySlotData(long? randomizeShops) {
        RandomizeShops = ((randomizeShops ?? 0) == 1);
    }

    public static Dictionary<string, Location> merchDataNameToLocation = new Dictionary<string, Location> {
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
        if (!RandomizeShops)
            return;
        //Log.Info($"ShopRando::EnsureShopsScouted()");
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

            // This is also our best opportunity to update the shop slot icons
            ReplaceMDataGFDs(__instance);
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

    private static string progressionHexColor = "AF99EF";
    private static string usefulHexColor = "6D8BE8";
    private static string trapHexColor = "FA8072";
    private static string fillerHexColor = "00EEEE";

    public static string scoutInfoToShopTitle(SerializableItemInfo scoutedItemInfo, bool useColors = true) {
        if (!useColors)
            return $"{scoutedItemInfo.Player.Name}'s {scoutedItemInfo.ItemDisplayName}";

        string itemColor;
        // hex values copied from user.kv
        if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
            itemColor = progressionHexColor;
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
            itemColor = usefulHexColor;
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
            itemColor = trapHexColor;
        } else {
            itemColor = fillerHexColor;
        }

        // EE00EE is the player color in AP clients, but here it's best to let the default red vs white coloring apply
        string shopTitle = $"{scoutedItemInfo.Player.Name}'s <color=#{itemColor}>{scoutedItemInfo.ItemDisplayName}</color>";

        // Finally, if the item happens to be this slot's Nine Sols jade with randomized costs, then we want to display the cost.
        var id = scoutedItemInfo.ItemId;
        if (scoutedItemInfo.ItemGame == "Nine Sols" && ItemNames.archipelagoIdToItem.TryGetValue(id, out var item)) {
            if (ConnectionAndPopups.APSession != null && scoutedItemInfo.PlayerSlot == ConnectionAndPopups.APSession.ConnectionInfo.Slot) {
                var jade = Jades.GetJadeDataFor(item);
                if (jade != null) {
                    if (JadeCosts.JadeEnglishTitleToSaveFlag.TryGetValue(jade.Title, out var saveFlag)) {
                        if (saveFlag != null && JadeCosts.JadeSaveFlagToSlotDataCost.TryGetValue(saveFlag, out long cost)) {
                            shopTitle += $" (Cost {cost})";
                        }
                    }
                }
            }
        }

        return shopTitle;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "Title", MethodType.Getter)]
    static void MerchandiseData_Title(MerchandiseData __instance, ref string __result) {
        if (!RandomizeShops)
            return;
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        //Log.Warning($"MerchandiseData_Title patch for {name} / {location}");
        //__result = $"{location}";
        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            // If this shop slot is already purchased, skip the <color> tags so the base game's graying out behavior can work as intended
            __result = scoutInfoToShopTitle(scoutedItemInfo, useColors: (__instance.numLeftToBuy.CurrentValue > 0));
        } else {
            __result = $"<color=red>ERROR: Location Not Scouted</color>";
        }
    }

    // unfortunately ConfirmPanelProvider uses merchandiseData.item.Title instead of merchandiseData.Title, bypassing the previous patch and necessitating this one
    [HarmonyPostfix, HarmonyPatch(typeof(ConfirmPanelProvider), "messageTranslate")]
    static void ConfirmPanelProvider_messageTranslate(ConfirmPanelProvider __instance, string message, MerchandiseData merchandiseData, ref string __result) {
        if (!RandomizeShops)
            return;
        var name = merchandiseData.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        string apTitle = "";
        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            apTitle = $"{scoutedItemInfo.Player.Name}'s {scoutedItemInfo.ItemDisplayName}";
        } else {
            apTitle = $"<color=red>ERROR: Location Not Scouted</color>";
        }

        var before = __result;
        __result = $"Purchase {apTitle}?"; // __result.Replace(merchandiseData.item.Title, apTitle); not used beacuse messages like "modify" don't make sense in shop rando
        Log.Info($"ConfirmPanelProvider_messageTranslate working around CPP's bypass of the MD.Title patch by editing \"{before}\" to \"{__result}\"");
    }

    private static string itemFlagsSummary(SerializableItemInfo scoutedItemInfo) {
        var flagStrings = new List<string>();
        if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
            flagStrings.Add($"<color=#{progressionHexColor}>Progression</color>");
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
            flagStrings.Add($"<color=#{usefulHexColor}>Useful</color>");
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
            flagStrings.Add($"<color=#{trapHexColor}>Trap</color>");
        }

        if (flagStrings.Count > 0) {
            return string.Join(" & ", flagStrings);
        } else {
            return $"<color=#{fillerHexColor}>Filler</color>";
        }
    }

    private static string itemFlagsDescription(SerializableItemInfo scoutedItemInfo) {
        var receivingPlayer = scoutedItemInfo.Player.Name;
        if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
            return $"{receivingPlayer} needs this item to make progress.";
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
            return $"This item would help {receivingPlayer}, but it's not essential.";
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
            return $"{receivingPlayer} would rather never receive this item.\n\nBut heroes are forged in agony.";
        } else {
            return $"{receivingPlayer} probably doesn't care about this item at all.";
        }
    }

    private static bool merchDataHasBeenCollectedRemotely(MerchandiseData md, Location location) {
        var locationId = LocationNames.locationToArchipelagoId[location];
        var isRemotelyChecked = ConnectionAndPopups.APSession?.Locations.AllLocationsChecked.Contains(locationId) ?? false;
        if (isRemotelyChecked) {
            var isLocallyChecked = APSaveManager.CurrentAPSaveData?.locationsChecked?.GetValueOrDefault(location.ToString(), false) ?? false;
            if (!isLocallyChecked) {
                // If a location has been checked remotely, but not locally (i.e. our local save file doesn't recall it being checked),
                // then we assume it's been !collected by another player.
                return true;
            }
        }
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "Description", MethodType.Getter)]
    static void MerchandiseData_Description(MerchandiseData __instance, ref string __result) {
        if (!RandomizeShops)
            return;
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        //Log.Warning($"MerchandiseData_Description patch for {name} / {location}");
        //__result = $"This is a randomized shop slot for Archipelago location {location}";
        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            __result = itemFlagsSummary(scoutedItemInfo) +
                $"\n\n" +
                itemFlagsDescription(scoutedItemInfo);
        } else {
            __result = $"For some reason, Archipelago location {location} has not been scouted. " +
                $"You can still purchase/check this location if you want, but we don't know what item you'll get." +
                $"\n\nThis is probably Eigong's fault somehow.";
        }

        if (__instance.numLeftToBuy.CurrentValue == 0 && merchDataHasBeenCollectedRemotely(__instance, location)) {
            __result = $"This shop location is considered checked by the Archipelago server, despite not being checked in the current save file. " +
                $"This is probably working as designed, since Archipelago's !collect feature, changing machines, same-slot co-op, " +
                $"and creating a fresh save file can all lead to this state." +
                $"\n\n" +
                __result;
        }

        // we'll put the ForcedPurchase description change here, since otherwise we'd have two patches of the same base game method both editing the __result
        var blockingPurchaseDescriptions = ForcedPurchases.ShouldBlock_ShopRandoOn(__instance);
        if (blockingPurchaseDescriptions != null) {
            var itemName = ForcedPurchases.IsDarkSteelPurchase(__instance) ? "Dark Steel" : "Herb Catalyst";

            __result = $"{LoadingScreenTips.apRainbow}: " +
                $"\n" +
                $"Because you're playing with shop randomization, " +
                $"and you only have enough {itemName}s left to buy the in-logic shop location(s) with progression item(s), " +
                $"<color=orange>your remaining {itemName}(s) must be spent on</color>:" +
                $"\n" +
                string.Join("\n", blockingPurchaseDescriptions.Select(str => $"- {str}")) +
                $"\n\n" +
                __result;
        }
    }

    // The yellow text at the very top of the shop UI's right-side description.
    // In vanilla this is the item type (Jade, Equipment, etc) but in rando we prefer to put the AP shop location name here.
    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "ItemType", MethodType.Getter)]
    static void MerchandiseData_ItemType(MerchandiseData __instance, ref string __result) {
        if (!RandomizeShops)
            return;
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        __result = LocationNames.locationNames[location];
    }

    private static GameFlagDescriptable ChooseDisplayGFDForScoutedItem(SerializableItemInfo scoutedItemInfo) {
        var id = scoutedItemInfo.ItemId;

        if (scoutedItemInfo.ItemGame == "Nine Sols" && ItemNames.archipelagoIdToItem.ContainsKey(id)) {
            // This is a Nine Sols item, so use the "correct" GFD for it
            var item = ItemNames.archipelagoIdToItem[scoutedItemInfo.ItemId];
            return InMemoryInventory.GetDisplayGFDFor(item) ?? Jin.GetJinGFD();
        } else {
            // For other games, we use the 3 levels of component to represent filler/useful/progression
            if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
                return InMemoryInventory.GetDisplayGFDFor(Item.AdvancedComponent)!;
            } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
                return InMemoryInventory.GetDisplayGFDFor(Item.StandardComponent)!;
            } else {
                return InMemoryInventory.GetDisplayGFDFor(Item.BasicComponent)!;
            }
        }
    }

    // The right side of the shop UI uses this getter when the selected item changes
    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseData), "SpriteRef", MethodType.Getter)]
    static bool MerchandiseData_SpriteRef(MerchandiseData __instance, ref RCGAssetReference __result) {
        if (!RandomizeShops)
            return true;
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return true;
        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            var gfd = ChooseDisplayGFDForScoutedItem(scoutedItemInfo);
            __result = gfd.SpriteRef;
            return false;
        }
        return true;
    }
    // The left side of the shop UI is made up of 5 MerchandiseItemButtons.
    // On wakeup and scroll, all of the MIButtons' "bindData" properties get updated to 5 of the MerchandiseData objects in shop.dataList.
    // Each MIButton has an image on the left which is (usually) taken from its bindData.item.
    // Thus, the most robust way to edit all the images on the left of the shop UI is to re-assign all the MData.items to different GFDs.
    static void ReplaceMDataGFDs(ShopUIPanel shop) {
        foreach (var md in shop.dataList) {
            if (md == null)
                continue;
            if (!merchDataNameToLocation.TryGetValue(md.name, out var location))
                return;
            var scoutedLocations = APSaveManager.CurrentAPSaveData?.scoutedLocations;
            if (scoutedLocations == null)
                continue;
            if (!scoutedLocations.TryGetValue(location, out var scoutedItemInfo))
                continue;

            var gfd = ChooseDisplayGFDForScoutedItem(scoutedItemInfo);
            //Log.Warning($"ReplaceBindDataItems {shop} -> setting md: {md} image to {gfd}");
            md.item = gfd;

            // I have no idea why, but:
            // - only Kuafu's Dark Steel arrow upgrades seem to have a non-null mainMaterial
            // - the value of mainMaterial is not a material at all, but the PlayerWeaponData corresponding to the arrow upgrade itself
            // - MIButton.UpdateView() will favor mainMaterial over md.item for setting the display image
            // Together these strange facts explain why so many of my shop icon edit attempts would fail just on Kuafu's DS slots.
            // We have to also edit mainMaterial to get these slots' icons to behave like the others.
            if (md.mainMaterial != null) {
                //Log.Warning($"ReplaceBindDataItems {shop}::{md} found a non-null mainMaterial: {md.mainMaterial}, nulling it");
                md.mainMaterial = null;
            }

            var locationId = LocationNames.locationToArchipelagoId[location];
            if (md.numLeftToBuy.CurrentValue > 0) {
                if (merchDataHasBeenCollectedRemotely(md, location)) {
                    // If a location has been !collected by another player, we set it to "already purchased"/"none left" status so you can't accidentally waste money on it
                    md.numLeftToBuy.SetCurrentValue(0);
                }
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "IsRevealed", MethodType.Getter)]
    static void MerchandiseData_IsRevealed(MerchandiseData __instance, ref bool __result) {
        if (!RandomizeShops)
            return;
        var name = __instance.name;
        if (name == "(商品)0_蚩尤二階_替死玉") // Revival Jade only purchaseable if you missed the bridge encounter where he gifts it, i.e. FGH_CHIYOU_BRIDGE
            __result = false; // in rando we don't want a "duplicate Revival Jade" to appear depending on shop unlocks, so force this to stay hidden

        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return;

        //Log.Warning($"MerchandiseData_IsRevealed patch for {name} / {location}");

        if (name.EndsWith("_二階")) {
            // this is one of Kuafu's "extra inventory" slots, so whether we should show it depends on the extra inventory flag
            if (
                APSaveManager.CurrentAPSaveData != null &&
                APSaveManager.CurrentAPSaveData.otherPersistentModFlags.TryGetValue(ShopUnlocks.KuafuExtraInventory_ModSaveFlag, out var kuafuExtraInventoryUnlocked) &&
                kuafuExtraInventoryUnlocked
            ) {
                __result = true;
            } else {
                __result = false;
            }
        } else {
            // all other randomized shop slots should simply always be visible
            __result = true;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseData), "CanTrade", MethodType.Getter)]
    static bool MerchandiseData_CanTrade(MerchandiseData __instance, ref bool __result) {
        if (!RandomizeShops)
            return true;
        var name = __instance.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return true;

        // this line is basically the vanilla implementation, but without the unlockedCondition.IsValid check, since we want those conditions ignored in rando
        __result = __instance.HasEnoughMaterial() && __instance.HasLeft && __instance.HasEnoughMoney;

        return false;
    }

    // Because we force all shop slots visible, we expose a "vanilla bug" where after purchasing a DS/HC Kuafu upgrade,
    // the right side of the shop UI for that upgrade will show "Modified" right on top of the "Required: DS/HC" text.
    // Fortunately the "Required: ..." text comes from a special MaterialAdditionalDescription component, so the easiest fix is patching a HasLeft check into it.
    [HarmonyPrefix, HarmonyPatch(typeof(MaterialAdditionalDescription), "UpdateView")]
    static bool MaterialAdditionalDescription_UpdateView(MaterialAdditionalDescription __instance, IDescriptable data) {
        if (!RandomizeShops)
            return true;
        if (!(data is MerchandiseData))
            return true;
        var mdata = (MerchandiseData)data;
        var name = mdata.name;
        if (!merchDataNameToLocation.TryGetValue(name, out var location))
            return true;

        if (!mdata.HasLeft) {
            __instance.gameObject.SetActive(value: false);
            return false;
        } else {
            return true;
        }
    }
}