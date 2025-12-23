using HarmonyLib;
using RCGFSM.Variable;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
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
        } else if (unlockMethod == ShopUnlockMethod.SolSeals) {
            var sealCount = ItemApplications.GetSolSealsCount();
            if (sealCount >= kuafuSeals)
                ActuallyMoveKuafuToFSP();
            if (sealCount >= chiyouSeals)
                ActuallyMoveChiyouToFSP();
            if (sealCount >= kuafuExtraSeals)
                ActuallyUnlockKuafuExtraInventory();
        } else if (unlockMethod == ShopUnlockMethod.UnlockItems) {
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

        if (location == Location.RP_KUAFU_SANCTUM) {
            ActuallyMoveKuafuToFSP();
        } else if (location == Location.FGH_CHIYOU_BRIDGE) {
            ActuallyMoveChiyouToFSP();
            ActuallyUnlockKuafuExtraInventory();
        }
    }

    private static string kuafuInFSPFlag = "e2ccc29dc8f187b45be6ce50e7f4174aScriptableDataBool"; // also the "has used Kuafu's VS" flag
    private static string chiyouInFSPFlag = "bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool";
    public static void ActuallyMoveKuafuToFSP() {
        InGameConsole.Add("Moving Kuafu into FSP and unlocking his shop");
        var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[kuafuInFSPFlag];
        flag.CurrentValue = true;
    }
    public static void ActuallyMoveChiyouToFSP() {
        InGameConsole.Add("Moving Chiyou into FSP and unlocking his shop");
        var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[chiyouInFSPFlag];
        flag.CurrentValue = true;
    }

    private static string KuafuExtraInventory_ModSaveFlag = "UnlockedKuafuFSPShopExtraInventory";
    public static void ActuallyUnlockKuafuExtraInventory() {
        InGameConsole.Add("Unlocking the extra inventory of Kuafu's FSP shop");
        APSaveManager.CurrentAPSaveData!.otherPersistentModFlags[KuafuExtraInventory_ModSaveFlag] = true;
    }

    private static GameObject? kuafuShopPanel = null;

    // block Kuafu's extra inventory by pretending the Chiyou rescue didn't happen if Kuafu's shop UI is open
    [HarmonyPrefix, HarmonyPatch(typeof(FlagFieldBoolEntry), "isValid", MethodType.Getter)]
    static bool FlagFieldBoolEntry_get_isValid(FlagFieldBoolEntry __instance, ref bool __result) {
        if (__instance.flagBase.name != "A6_S1_蚩尤救回羿") // Chiyou rescue flag
            return true;

        if (kuafuShopPanel == null)
            kuafuShopPanel = GameObject.Find("AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/ConfirmProvider/UpgradeTable");

        //Log.Info($"FlagFieldBoolEntry_get_isValid {__instance.flagBase.name} {kuafuShopPanel.activeSelf} {kuafuExtraInventoryUnlocked}"); // logs every Update() in some shops
        if (kuafuShopPanel.activeSelf) {
            if (
                APSaveManager.CurrentAPSaveData != null &&
                APSaveManager.CurrentAPSaveData.otherPersistentModFlags.TryGetValue(KuafuExtraInventory_ModSaveFlag, out var kuafuExtraInventoryUnlocked) &&
                kuafuExtraInventoryUnlocked
            ) {
                __result = true;
                return true;
            }
            __result = false;
            return false;
        }
        return true;
    }

    // prevent vanilla Kuafu move condition from triggering
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static bool SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        if (__instance.targetFlag?.boolFlag?.FinalSaveID == kuafuInFSPFlag) {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A2_S5_ BossHorseman_GameLevel/Room/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] Get_BossKey = true") {
                Log.Info($"ShopUnlocks::SetVariableBoolAction_OnStateEnterImplement preventing Kuafu from moving into FSP");
                return false;
            }
        }
        return true;
    }

    // we ignore the vanilla Chiyou move condition for now, since it's so impractical to trigger in rando
}
