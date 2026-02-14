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
        if (!slotData.ContainsKey("shop_unlocks")) {
            // assume this slot_data predates the shop unlock options, switch to defaults
            unlockMethod = ShopUnlockMethod.VanillaLikeLocations;
            return;
        }

        var rawUnlockMethod = (long)slotData["shop_unlocks"];
        if (rawUnlockMethod == 0) { // "vanilla_like_locations"
            unlockMethod = ShopUnlockMethod.VanillaLikeLocations;
        } else if (rawUnlockMethod == 1) { // "sol_seals"
            unlockMethod = ShopUnlockMethod.SolSeals;

            kuafuSeals = (long)slotData["kuafu_shop_unlock_sol_seals"];
            chiyouSeals = (long)slotData["chiyou_shop_unlock_sol_seals"];
            kuafuExtraSeals = (long)slotData["kuafu_extra_inventory_unlock_sol_seals"];
        } else if (rawUnlockMethod == 2) { // "unlock_items"
            unlockMethod = ShopUnlockMethod.UnlockItems;
        } else {
            Log.Error($"ShopUnlocks::ApplySlotData aborting because shop_unlocks was {rawUnlockMethod}");
            unlockMethod = ShopUnlockMethod.VanillaLikeLocations;
        }
    }

    // The special case of 0 seal unlocks has to be checked right away without waiting for any item/location sends to happen
    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        // we only care about the 0 seals case
        if (unlockMethod != ShopUnlockMethod.SolSeals)
            return;
        if (kuafuSeals > 0 && chiyouSeals > 0 && kuafuExtraSeals > 0)
            return;

        // if we already have one or more seals, OnItemUpdate() would've handled this by now
        var sealCount = ItemApplications.GetSolSealsCount();
        if (sealCount > 0)
            return;

        Log.Info($"ShopUnlocks::GameLevel_Awake handling zero seal unlocks");
        if (kuafuSeals == 0)
            ActuallyMoveKuafuToFSP();
        if (chiyouSeals == 0)
            ActuallyMoveChiyouToFSP();
        if (kuafuExtraSeals == 0)
            ActuallyUnlockKuafuExtraInventory();
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
        var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[kuafuInFSPFlag];
        if (flag.CurrentValue == false) {
            InGameConsole.Add("Moving Kuafu into FSP and unlocking his shop");
            flag.CurrentValue = true;
        }
    }
    public static void ActuallyMoveChiyouToFSP() {
        var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[chiyouInFSPFlag];
        if (flag.CurrentValue == false) {
            InGameConsole.Add("Moving Chiyou into FSP and unlocking his shop");
            flag.CurrentValue = true;
        }
    }

    private static string KuafuExtraInventory_ModSaveFlag = "UnlockedKuafuFSPShopExtraInventory";
    public static void ActuallyUnlockKuafuExtraInventory() {
        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"ShopUnlocks::ActuallyUnlockKuafuExtraInventory() aborting because there's no AP connection/save file to write to");
            return;
        }

        APSaveManager.CurrentAPSaveData.otherPersistentModFlags.TryGetValue(KuafuExtraInventory_ModSaveFlag, out var kuafuExtraInventoryUnlocked);
        if (!kuafuExtraInventoryUnlocked) {
            InGameConsole.Add("Unlocking the extra inventory of Kuafu's FSP shop");
            APSaveManager.CurrentAPSaveData.otherPersistentModFlags[KuafuExtraInventory_ModSaveFlag] = true;
        }
    }

    private static GameObject? kuafuShopPanel = null;

    // block Kuafu's extra inventory by pretending the Chiyou rescue didn't happen if Kuafu's shop UI is open
    [HarmonyPrefix, HarmonyPatch(typeof(FlagFieldBoolEntry), "isValid", MethodType.Getter)]
    static bool FlagFieldBoolEntry_get_isValid(FlagFieldBoolEntry __instance, ref bool __result) {
        if (__instance.flagBase.name != "A6_S1_蚩尤救回羿") // Chiyou rescue flag
            return true;

        if (kuafuShopPanel == null)
            kuafuShopPanel = (GameObject?)GameObject.Find("AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/[Set] 中間層選項/Canvas/[中間層] UI Interact Options Root Panel/ConfirmProvider/UpgradeTable");

        //Log.Info($"FlagFieldBoolEntry_get_isValid {__instance.flagBase.name} {kuafuShopPanel.activeSelf} {kuafuExtraInventoryUnlocked}"); // logs every Update() in some shops
        if (kuafuShopPanel?.activeSelf ?? false) {
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
            if (
                goPath == "A2_S5_ BossHorseman_GameLevel/Room/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] Get_BossKey = true" ||
                goPath == "VR_Kuafu/VR_Kuafu_Skin/BG_DREAM/VRMemory FSM/PlayerSensor FSM Prototype/--[States]/FSM/[State] Play End 表演結束/[Action] GetBossKey"
            ) {
                Log.Info($"ShopUnlocks::SetVariableBoolAction_OnStateEnterImplement preventing Kuafu from being moved into FSP by {goPath}");
                return false;
            }
        }
        return true;
    }

    // override Radiant Pagoda's "is Kuafu in FSP yet" checks to (if shop_unlocks is not VLL) instead rely on whether the AP location has been checked

    [HarmonyPostfix, HarmonyPatch(typeof(AbstractConditionComp), "FinalResult", MethodType.Getter)]
    static void AbstractConditionComp_get_FinalResult(AbstractConditionComp __instance, ref bool __result) {
        if (unlockMethod == ShopUnlockMethod.VanillaLikeLocations)
            return;

        var locationsChecked = APSaveManager.CurrentAPSaveData?.locationsChecked;
        var locName = Location.RP_KUAFU_SANCTUM.ToString();
        var sanctumChecked = (locationsChecked != null && locationsChecked.ContainsKey(locName) && locationsChecked[locName]);

        // unfortunately these door conditions also get checked repeatedly so we can't afford to log these edits
        if (__instance.name == "[Condition] GetBossKeyAuthority") {
            if (__result == sanctumChecked)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (
                goPath == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant/--[States]/FSM/[State] Init/[Action] HasGotBossAuthority Transition/[Condition] GetBossKeyAuthority" ||
                goPath == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant/--[States]/FSM/[State] Closed/[Action] HasGotBossAuthority Transition/[Condition] GetBossKeyAuthority"
            ) {
                //Log.Info($"AbstractConditionComp_get_FinalResult forcing the door left of Yingzhao's arena to stay open, since the AP sanctum location ({sanctumChecked}) doesn't match the Kuafu-in-FSP flag ({__result})");
                __result = sanctumChecked;
            }
            if (
                goPath == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant (1)/--[States]/FSM/[State] Init/[Action] HasGotBossAuthority Transition/[Condition] GetBossKeyAuthority" ||
                goPath == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant (1)/--[States]/FSM/[State] Closed/[Action] HasGotBossAuthority Transition/[Condition] GetBossKeyAuthority"
            ) {
                //Log.Info($"AbstractConditionComp_get_FinalResult forcing the door right of Yingzhao's arena to stay open, since the AP sanctum location ({sanctumChecked}) doesn't match the Kuafu-in-FSP flag ({__result})");
                __result = sanctumChecked;
            }
        }
        if (__instance.name == "[Condition] GetBossKeyAuthority == false") {
            if (__result == !sanctumChecked)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant (1)/--[States]/FSM/[State] HoloOpened/[Action] Bosskilled But No Authority -> ClosedDoor/[Condition] GetBossKeyAuthority == false") {
                //Log.Info($"AbstractConditionComp_get_FinalResult forcing the door right of Yingzhao's arena to stay open, since the AP sanctum location ({sanctumChecked}) doesn't match the Kuafu-in-FSP flag ({!__result})");
                __result = !sanctumChecked;
            }
            if (goPath == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant/--[States]/FSM/[State] HoloOpened/[Action] Bosskilled But No Authority -> ClosedDoor/[Condition] GetBossKeyAuthority == false") {
                //Log.Info($"AbstractConditionComp_get_FinalResult forcing the door left of Yingzhao's arena to stay open, since the AP sanctum location ({sanctumChecked}) doesn't match the Kuafu-in-FSP flag ({!__result})");
                __result = !sanctumChecked;
            }
        }

        // this condition is only checked on room load so we can log it, and fortunately this is the more important one for rando to log
        if (__instance.name == "[Condition] FlagBoolCondition") {
            if (__result == sanctumChecked)
                return;

            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A2_S5_ BossHorseman_GameLevel/Room/Sleeppod  FSM/EnterSleeppodFSM/--[States]/FSM/[State] Init/[Action] Entered Transition/[Condition] FlagBoolCondition") {
                Log.Info($"AbstractConditionComp_get_FinalResult changing the 'should Kuafu's vital sanctum be open' check, since the AP sanctum location ({sanctumChecked}) doesn't match the Kuafu-in-FSP flag ({__result})");
                __result = sanctumChecked;
            }
        }
    }

    // we ignore the vanilla Chiyou move condition for now, since it's so impractical to trigger in rando
}
