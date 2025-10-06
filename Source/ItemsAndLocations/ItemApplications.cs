using Archipelago.MultiClient.Net.Helpers;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class ItemApplications {
    public static void ItemReceived(IReceivedItemsHelper receivedItemsHelper) {
        try {
            while (receivedItemsHelper.PeekItem() != null) {
                var itemId = receivedItemsHelper.PeekItem().ItemId;
                ReceivedItemIds.Add(itemId);
                receivedItemsHelper.DequeueItem();
            }
        } catch (Exception ex) {
            Log.Error($"Caught error in APSession_ItemReceived: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    // Here we move received item processing off the websocket thread because this is believed to help prevent crashes
    private static ConcurrentBag<long> ReceivedItemIds = new();
    // If multiple ItemReceived() events interleave, we and the base game can end up very confused about the state of our inventory.
    private readonly static object itemReceivedLock = new object();
    public static void Update() {
        if (ReceivedItemIds.IsEmpty)
            return;

        try {
            lock (itemReceivedLock) {
                Log.Info($"ItemReceived event with item ids {string.Join(", ", ReceivedItemIds)}. Updating these item counts.");
                while (ReceivedItemIds.TryTake(out var itemId))
                    SyncItemCountWithAPServer(itemId);
            }

            APSaveManager.ScheduleWriteToCurrentSaveFile();
        } catch (Exception ex) {
            Log.Error($"Caught error in APSession_ItemReceived: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    public readonly static Dictionary<Item, int> ApInventory = new Dictionary<Item, int>();
    public static void LoadSavedInventory(APRandomizerSaveData apSaveData) {
        try {
            foreach (var (i, c) in apSaveData.itemsAcquired) {
                ApInventory[Enum.Parse<Item>(i)] = c;
            }
        } catch (Exception ex) {
            Log.Error($"Caught error in LoadSavedInventory: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    // Ensure that our local items state matches APSession.Items.AllItemsReceived. It's possible for AllItemsReceived to be out of date,
    // but in that case the ItemReceived event handler will be invoked as many times as it takes to get up to date.
    public static void SyncInventoryWithServer() {
        try {
            var totalItemsAcquired = ApInventory.Sum(kv => kv.Value);
            var totalItemsReceived = ConnectionAndPopups.APSession!.Items.AllItemsReceived.Count;

            if (totalItemsReceived > totalItemsAcquired) {
                Log.Info($"AP server state has more items ({totalItemsReceived}) than local save data ({totalItemsAcquired}). Attempting to apply new items.");
                foreach (var itemInfo in ConnectionAndPopups.APSession.Items.AllItemsReceived)
                    SyncItemCountWithAPServer(itemInfo.ItemId);
            }
        } catch (Exception ex) {
            Log.Error($"Caught error in SyncInventoryWithServer: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    public static void SyncItemCountWithAPServer(long itemId) {
        if (!ItemNames.archipelagoIdToItem.ContainsKey(itemId)) {
            ToastManager.Toast(
                $"<color=red>Warning</color>: This mod does not recognize the item id {itemId}, which the Archipelago server just sent us. " +
                $"Check if your mod version matches the .apworld version used to generate this multiworld.");
            return;
        }

        var item = ItemNames.archipelagoIdToItem[itemId];
        var itemEnumName = item.ToString();
        Log.Info($"SyncItemCountWithAPServer mapped id {itemId} to item {item} / {itemEnumName}");

        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"Somehow CurrentAPSaveData is null during a SyncItemCountWithAPServer() call. We should've created or loaded save data when connecting to the AP server.");
            return;
        }
        if (APSaveManager.CurrentAPSaveData.itemsAcquired == null) {
            Log.Error($"Somehow CurrentAPSaveData.itemsAcquired is null during a SyncItemCountWithAPServer() call.");
            return;
        }
        var itemsAcquired = APSaveManager.CurrentAPSaveData.itemsAcquired;
        if (!itemsAcquired.ContainsKey(itemEnumName))
            itemsAcquired[itemEnumName] = 0;

        if (ConnectionAndPopups.APSession == null) {
            Log.Error($"Somehow APSession is null during a SyncItemCountWithAPServer() call. How did this get called without an AP connection?");
            return;
        }
        var itemCountSoFar = ConnectionAndPopups.APSession.Items.AllItemsReceived.Where(i => i.ItemId == itemId).Count();
        var savedCount = itemsAcquired[itemEnumName];
        if (savedCount >= itemCountSoFar) {
            // APSession does client-side caching, so AllItemsReceived having fewer of an item than our save data usually just means the
            // client-side cache is out of date and will be brought up to date shortly with ItemReceived events. Thus, we ignore this case.
            if (savedCount == itemCountSoFar)
                Log.Info($"SyncItemCountWithAPServer ignoring item {item} since savedCount ({savedCount}) == itemCountSoFar ({itemCountSoFar}).");
            else
                Log.Info($"SyncItemCountWithAPServer ignoring item {item} since savedCount ({savedCount}) > itemCountSoFar ({itemCountSoFar}), which usually means the client-side cache hasn't finished updating yet.");
            return;
        } else {
            UpdateItemCount(item, itemCountSoFar);
        }
    }

    // When we receive items on the main menu, especially during connection,
    // hang onto them here until we can apply them for real in the game.
    public static HashSet<(Item, int)> deferredUpdates = new();

    [HarmonyPostfix, HarmonyPatch(typeof(GameCore), "InitializeGameLevel")]
    private static async void GameLevel_InitializeGameCore_Postfix(GameCore __instance, GameLevel newLevel) {
        if (deferredUpdates.Count <= 0)
            return;

        Log.Info($"GameLevel_InitializeGameCore_Postfix has {deferredUpdates.Count} deferredUpdates to execute. Waiting for base game fade-in to finish.");
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

        var (gfd, customText) = ApplyItemToPlayer(item, count, oldCount);

        NotifyAndSave.Default(gfd, count, oldCount);

        ApInventory[item] = count;

        Jiequan1Fight.OnItemUpdate(item);
        LadyESoulscapeEntrance.OnItemUpdate(item);

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

    private static (GameFlagDescriptable?, string?) ApplyItemToPlayer(Item item, int count, int oldCount) {
        Log.Info($"ApplyItemToPlayer(item={item}, count={count}, oldCount={oldCount})");

        var ability = VanillaAbilities.ApplyVanillaAbilityToPlayer(item, count, oldCount);
        if (ability != null) {
            return (ability, null);
        }

        var inventoryItem = NormalInventoryItems.ApplyNormalInventoryItemToPlayer(item, count, oldCount);
        if (inventoryItem != null) {
            return (inventoryItem, null);
        }

        var fruit = TaoFruit.ApplyTaoFruitToPlayer(item, count, oldCount);
        if (fruit != null) {
            return (fruit, null);
        }

        var dbEntry = DatabaseEntries.ApplyDatabaseEntryToPlayer(item, count, oldCount);
        if (dbEntry != null) {
            return (dbEntry, null);
        }

        var jade = Jades.ApplyJadeToPlayer(item, count, oldCount);
        if (jade != null) {
            return (jade, null);
        }

        var arrow = Arrows.ApplyArrowToPlayer(item, count, oldCount);
        if (arrow != null) {
            return (arrow, null);
        }

        var jin = Jin.ApplyJinToPlayer(item, count, oldCount);
        if (jin != null) {
            return (jin, null);
        }

        var cu = ComputingUnits.ApplyComputingUnitToPlayer(item, count, oldCount);
        if (cu != null) {
            return (cu, null);
        }

        var pv = PipeVials.ApplyPipeVialToPlayer(item, count, oldCount);
        if (pv != null) {
            return (pv, null);
        }

        ToastManager.Toast($"unable to apply item {item} (count = {count})");
        return (null, null);
    }
}
