using ArchipelagoRandomizer.Items.ItemImpls;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using System.Collections.Generic;

namespace ArchipelagoRandomizer.Items;

[HarmonyPatch]
internal class InMemoryInventory {
    public readonly static Dictionary<Item, int> ApInventory = new Dictionary<Item, int>();

    // When we receive items on the main menu, especially during connection,
    // hang onto them here until we can apply them for real in the game.
    private static HashSet<(Item, int)> deferredUpdates = new();

    [HarmonyPostfix, HarmonyPatch(typeof(GameCore), "InitializeGameLevel")]
    private static async void GameCore_InitializeGameLevel_Postfix(GameCore __instance, GameLevel newLevel) {
        if (inventoryNeedsReapplicationChecks)
            RunInventoryReapplicationChecks();

        if (deferredUpdates.Count <= 0)
            return;

        Log.Info($"GameCore_InitializeGameLevel_Postfix has {deferredUpdates.Count} deferredUpdates to execute. Waiting for base game fade-in to finish.");
        await UniTask.Delay(1000);

        foreach (var (i, c) in deferredUpdates)
            UpdateItemCount(i, c);
        deferredUpdates.Clear();
        APSaveManager.WriteCurrentSaveFile();
    }

    public static void UpdateItemCount(Item item, int count) {
        Log.Info($"UpdateItemCount(item={item}, count={count})");

        if (!SingletonBehaviour<GameCore>.IsAvailable()) {
            Log.Info($"UpdateItemCount deferring this item update since we aren't in the game yet");
            deferredUpdates.Add((item, count));
            return;
        }

        var oldCount = ApInventory.GetValueOrDefault(item);

        ApplyItemToPlayer(item, count, oldCount);

        ApInventory[item] = count;

        Jiequan1Fight.OnItemUpdate(item);
        LadyESoulscapeEntrance.OnItemUpdate(item);
        ShopUnlocks.OnItemUpdate(item);

        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"UpdateItemCount(item={item}, count={count}) unable to write to save file because there is no save file. If you're the developer doing hot reloading, this is normal.");
        } else {
            APSaveManager.CurrentAPSaveData.itemsAcquired[item.ToString()] = count;
        }
    }

    public static bool IsSolSeal(Item item) {
        return (item >= Item.SealOfKuafu && item <= Item.SealOfNuwa);
    }

    public static int GetSolSealsCount() {
        var sealCount = 0;
        for (var seal = Item.SealOfKuafu; seal <= Item.SealOfNuwa; seal++) {
            if (ApInventory.ContainsKey(seal) && ApInventory[seal] > 0)
                sealCount++;
        }
        return sealCount;
    }

    // This is purely for visual display purposes. Many AP items do not have a "real" GFD, and this will return one with a "good enough" sprite.
    public static GameFlagDescriptable? GetDisplayGFDFor(Item item) {
        return
            VanillaAbilities.GetDisplayGFDFor(item) ??
            RemovedAbilities.GetDisplayGFDFor(item) ??
            NormalInventoryItems.GetDisplayGFDFor(item) ??
            TaoFruit.GetDisplayGFDFor(item) ??
            DatabaseEntries.GetDisplayGFDFor(item) ??
            Jades.GetJadeDataFor(item) ??
            Arrows.GetDisplayGFDFor(item) ??
            Jin.GetDisplayGFDFor(item) ??
            ComputingUnits.GetDisplayGFDFor(item) ??
            PipeVials.GetDisplayGFDFor(item) ??
            PipeUpgrades.GetDisplayGFDFor(item) ??
            RootNodeItems.GetDisplayGFDFor(item) ??
            (item == Item.ProgressiveShopUnlock ? Jin.GetJinGFD() : null);
    }

    private static void ApplyItemToPlayer(Item item, int count, int oldCount) {
        Log.Info($"ApplyItemToPlayer(item={item}, count={count}, oldCount={oldCount})");

        if (VanillaAbilities.ApplyVanillaAbilityToPlayer(item, count, oldCount)) return;
        if (RemovedAbilities.ApplyRemovedAbilityToPlayer(item, count, oldCount)) return;
        if (NormalInventoryItems.ApplyNormalInventoryItemToPlayer(item, count, oldCount)) return;
        if (TaoFruit.ApplyTaoFruitToPlayer(item, count, oldCount)) return;
        if (DatabaseEntries.ApplyDatabaseEntryToPlayer(item, count, oldCount)) return;
        if (Jades.ApplyJadeToPlayer(item, count, oldCount)) return;
        if (Arrows.ApplyArrowToPlayer(item, count, oldCount)) return;
        if (Jin.ApplyJinToPlayer(item, count, oldCount)) return;
        if (ComputingUnits.ApplyComputingUnitToPlayer(item, count, oldCount)) return;
        if (PipeVials.ApplyPipeVialToPlayer(item, count, oldCount)) return;
        if (PipeUpgrades.ApplyPipeUpgradeToPlayer(item, count, oldCount)) return;
        if (RootNodeItems.ApplyNodeToPlayer(item, count, oldCount)) return;

        if (item == Item.ProgressiveShopUnlock) { // the "real implementation" is in ShopUnlocks.OnItemUpdate()
            NotifyAndSave.WithCustomText(Jin.GetJinGFD(), "Collected Progressive Shop Unlock.", count, oldCount);
            return;
        }

        InGameConsole.Add($"unable to apply item {item} (count = {count})");
    }

    // Item "re-application": If an AP item has been received, the local rando save claims it has been applied before,
    // yet for some reason Yi doesn't have it, then we want to try re-applying it.
    // Of course this is only correct for unique non-consumable items, and any way of reliably causing this is still
    // a bug we should fix directly, but hopefully this mitigates those bugs and makes them easier to pin down.

    private static bool inventoryNeedsReapplicationChecks = false;
    public static void ScheduleInventoryReapplicationChecks() {
        inventoryNeedsReapplicationChecks = true;
    }

    private static void RunInventoryReapplicationChecks() {
        foreach (var i in VanillaAbilities.vanillaAbilityItems)
            if ((ApInventory.GetValueOrDefault(i) == 1) && !VanillaAbilities.PlayerHasVanillaAbility(i)) {
                InGameConsole.Add($"<color=orange>Re-applying item '{i}'. Somehow Yi was missing it, despite it having been received and applied already.</color>");
                VanillaAbilities.ApplyVanillaAbilityToPlayer(i, 1, 1);
            }

        // "removed abilities" are ignored here since, by definition, there is no "vanilla game state" for those items.
        // LoadSavedInventory()'s call to RemovedAbilities.LoadSavedInventory() is already as thorough a resync as it's possible to do.

        Item[] solSeals = [
            Item.SealOfKuafu,
            Item.SealOfGoumang,
            Item.SealOfYanlao,
            Item.SealOfJiequan,
            Item.SealOfLadyEthereal,
            Item.SealOfJi,
            Item.SealOfFuxi,
            Item.SealOfNuwa,
        ];
        foreach (var s in solSeals)
            if ((ApInventory.GetValueOrDefault(s) == 1) && !NormalInventoryItems.PlayerHasInventoryItem(s)) {
                InGameConsole.Add($"<color=orange>Re-applying item '{s}'. Somehow Yi was missing it, despite it having been received and applied already.</color>");
                NormalInventoryItems.ApplyNormalInventoryItemToPlayer(s, 1, 1);
            }

        // For now, reapply checks are only worth the effort for non-consumable progression items,
        // which leaves arrows as the only item types we haven't implemented repplay checks for yet.

        // If we ever get reports of mysteriously missing *consumable* prog items, like Shuanshuan artifacts or Access Tokens,
        // maybe we can do those too. But those definitely aren't worth it unless we get such reports.

        inventoryNeedsReapplicationChecks = false;
    }
}
