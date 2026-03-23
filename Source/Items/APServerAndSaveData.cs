using Archipelago.MultiClient.Net.Helpers;
using ArchipelagoRandomizer.Items.ItemImpls;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ArchipelagoRandomizer.Items;

internal class APServerAndSaveData {
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

    public static void OnConnect(APRandomizerSaveData apSaveData) {
        // these methods must be called in this order
        LoadSavedInventory(apSaveData);         // updates APInventory to match local rando save file
        SyncInventoryWithServer();              // compares APInventory to server state, then updates APInventory if different
        InMemoryInventory.ScheduleInventoryReapplicationChecks(); // after waiting for the game to load, compares APInventory to Yi's vanilla game state
    }

    private static void LoadSavedInventory(APRandomizerSaveData apSaveData) {
        try {
            // The save data doesn't have explicit 0s for missing items, so
            // it's critical that we completely clear() the old inventory.
            InMemoryInventory.ApInventory.Clear();

            foreach (var (i, c) in apSaveData.itemsAcquired) {
                InMemoryInventory.ApInventory[Enum.Parse<Item>(i)] = c;
            }
            RemovedAbilities.LoadSavedInventory(in InMemoryInventory.ApInventory);
        } catch (Exception ex) {
            Log.Error($"Caught error in LoadSavedInventory: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    // Ensure that our local items state matches APSession.Items.AllItemsReceived. It's possible for AllItemsReceived to be out of date,
    // but in that case the ItemReceived event handler will be invoked as many times as it takes to get up to date.
    private static void SyncInventoryWithServer() {
        try {
            var totalItemsAcquired = InMemoryInventory.ApInventory.Sum(kv => kv.Value);
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

    private static void SyncItemCountWithAPServer(long itemId) {
        if (!ItemNames.archipelagoIdToItem.ContainsKey(itemId)) {
            InGameConsole.Add(
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
            InMemoryInventory.UpdateItemCount(item, itemCountSoFar);
        }
    }
}
