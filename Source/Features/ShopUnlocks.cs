using Newtonsoft.Json.Linq;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArchipelagoRandomizer;

internal class ShopUnlocks {
    private enum ShopUnlockMethod {
        VanillaLikeLocations,
        SolSeals,
        UnlockItems,
    }
    private static ShopUnlockMethod unlockMethod = ShopUnlockMethod.VanillaLikeLocations;

    private static long kuafuSeals = 0;
    private static long chiyouSeals = 0;
    private static long kuafuExtraSeals = 0;

    public static void ApplySlotData(Dictionary<string, object> slotData) {
        if (!slotData.ContainsKey("shop_unlocks") || !(slotData["shop_unlocks"] is string)) {
            // assume this slot_data predates the shop unlock options, switch to defaults
            unlockMethod = ShopUnlockMethod.VanillaLikeLocations;
            return;
        }

        string rawUnlockMethod = (string)slotData["shop_unlocks"];
        if (rawUnlockMethod == "vanilla_like_locations") {
            unlockMethod = ShopUnlockMethod.VanillaLikeLocations;
        } else if (rawUnlockMethod == "sol_seals") {
            unlockMethod = ShopUnlockMethod.SolSeals;

            kuafuSeals = (long)slotData["kuafu_shop_unlock_sol_seals"];
            chiyouSeals = (long)slotData["chiyou_shop_unlock_sol_seals"];
            kuafuExtraSeals = (long)slotData["kuafu_extra_inventory_unlock_sol_seals"];
        } else if (rawUnlockMethod == "unlock_items") {
            unlockMethod = ShopUnlockMethod.UnlockItems;
        } else {
            Log.Error($"ShopUnlocks::ApplySlotData aborting because shop_unlocks was {rawUnlockMethod}");
            unlockMethod = ShopUnlockMethod.VanillaLikeLocations;
        }
    }

    public static void OnItemUpdate(Item item) {
        if (unlockMethod == ShopUnlockMethod.VanillaLikeLocations) {
            return; // items don't matter in this mode
        }

        if (unlockMethod == ShopUnlockMethod.SolSeals) {
            var sealCount = ItemApplications.GetSolSealsCount();
            if (sealCount >= kuafuSeals)
                ActuallyMoveKuafuToFSP();
            if (sealCount >= chiyouSeals)
                ActuallyMoveChiyouToFSP();
            if (sealCount >= kuafuExtraSeals)
                ActuallyUnlockKuafuExtraInventory();
        }

        if (unlockMethod == ShopUnlockMethod.UnlockItems) {
            var psuCount = (ItemApplications.ApInventory.ContainsKey(Item.ProgressiveShopUnlock) ? ItemApplications.ApInventory[Item.ProgressiveShopUnlock] : 0);
            if (psuCount >= 1)
                ActuallyMoveKuafuToFSP();
            if (psuCount >= 2)
                ActuallyMoveChiyouToFSP();
            if (psuCount >= 3)
                ActuallyUnlockKuafuExtraInventory();
        }
    }

    public static void OnLocationCheck(Location location) {
        if (unlockMethod != ShopUnlockMethod.VanillaLikeLocations) {
            return; // locations are only relevant in VanillaLikeLocations mode
        }

        // For now, we want Chiyou's behavior in randomizer to be: move into the FSP when his Bridge location is checked
        if (location == Location.FGH_CHIYOU_BRIDGE) {
            Log.Info("Moving Chiyou into FSP now that \"Factory (GH): Raise the Bridge for Chiyou\" has been checked");
            var chiyouRescuedYiAndMovedIntoFSPFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool"];
            chiyouRescuedYiAndMovedIntoFSPFlag.CurrentValue = true;
        }
    }

    // TODO: prevent vanilla Kuafu move condition from triggering

    private static string kuafuInFSPFlag = "e2ccc29dc8f187b45be6ce50e7f4174aScriptableDataBool";
    private static string chiyouInFSPFlag = "bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool";

    public static void ActuallyMoveKuafuToFSP() {
        InGameConsole.Add("moving Kuafu into FSP");
        var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[kuafuInFSPFlag];
        flag.CurrentValue = true;
    }
    public static void ActuallyMoveChiyouToFSP() {
        InGameConsole.Add("moving Chiyou into FSP");
        var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[chiyouInFSPFlag];
        flag.CurrentValue = true;
    }
    public static void ActuallyUnlockKuafuExtraInventory() {
        // TODO: figure out how to implement this
    }
}
