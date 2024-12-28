using HarmonyLib;
using NineSolsAPI;
using RCGFSM.Items;
using RCGFSM.PlayerAction;
using System.Collections.Generic;
using UnityEngine;
using static RCGFSM.Items.PickItemAction;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LocationTriggers {
    private static void CheckLocation(Location location) {
        // TODO: talk to AP of course
        ToastManager.Toast($"CheckLocation() called with Archipelago location: {location}");
    }

    public static string GetFullPath(GameObject go) {
        var transform = go.transform;
        List<string> pathParts = new List<string>();
        while (transform != null) {
            pathParts.Add(transform.name);
            transform = transform.parent;
        }
        pathParts.Reverse();
        return string.Join("/", pathParts);
    }

    private static Dictionary<string, Location> goPathToLocation = new Dictionary<string, Location> {
        {
            "A0_S4 gameLevel/Room/Prefab/1_DropPickable 遺書 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.UC_NOTE
        },
        {
            "A0_S4 gameLevel/Room/Prefab/1_DropPickable 竹簡 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.UC_SCROLL
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/QCmachine  染血的大紅槿/LootProvider/A1_S1_DropPickable_Flower/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFM_BREAK_CORPSE
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M_中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFM_CHEST_UPPER_RIGHT
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S_小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFM_CHEST_LOWER_VENT
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFM_DB_SURVEILLANCE
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_S 小寶箱_小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_UPPER_PAGODA
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_MOVING_BOXES
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFE_CHEST_ELEVATOR
        },
        {
            "A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/3_DropPickable 定身玉  FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFE_DROP_BAICHANG
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_OVER_HAZARD
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/EventBinder 小錢袋/LootProvider/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFE_CHEST_STATUE
        },
        {
            // save flag 37b820ea-4a5a-40a2-a581-767d5362ed5f_19ef97be8cb7b4fca9d79b754bb6c81cScriptableDataBool -> A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/MoneyCrateFlag
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFD_CHEST_LOWER_LEVEL
        },
        {
            // save flag 537b2e71-c903-4d2b-9525-1f501c333287_19ef97be8cb7b4fca9d79b754bb6c81cScriptableDataBool -> A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/BR_TreasureDing_M/MoneyCrateFlag
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_UNDER_LOWER_LEFT_EXIT
        },
        {
            "A1_S3_GameLevel/Room/Prefab/EventBinder/LootProvider 藥草催化器/0_DropPickable 斷魂刃 FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_DROP_1_SHANGUI
        },
        {
            "A1_S3_GameLevel/Room/Prefab/EventBinder/LootProvider 藥草催化器/0_DropPickable 藥草催化器 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_DROP_2_SHANGUI
        },
        {
            // save flag 7aae1bfb-7415-4e03-b8c8-0e593375aff5_19ef97be8cb7b4fca9d79b754bb6c81cScriptableDataBool -> A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金 (1)/MoneyCrateFlag
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFD_CHEST_BELOW_NODE
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 扶桑牌/0_DropPickable 文物 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_CRYSTAL_CAVES
        },
        {
            // save flag "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[Variables]/[Variable] Picked",
            //"A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] [Variable] Picked = true",
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.AFD_FLOWER_UNDER_ELEVATOR
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 中錢袋、藥材-雄橙/0_DropPickable 藥材-雄橙 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_UPPER_RIGHT_1
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 中錢袋、藥材-雄橙/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_UPPER_RIGHT_2
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/EventBinder 卸力玉/LootProvider/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_STATUES
        },
        {
            "AG_S1/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking Errors/[Action] GetItem",
            Location.CH_LAUNCH_MEMORIAL
        },
        {
            "AG_S1/Room/Prefab/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_COUNCIL_SIGN
        },
        {
            "AG_S1/Room/Prefab/1_DropPickable SceneObserve FSM (2)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_COUNCIL_TENETS
        },
        {
            "AG_S1/Room/Prefab/寶箱/LootProvider  中錢袋/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_CHEST_VENTS
        },
        {
            "AG_SG1/Room/LootProvider 藥斗擴充瓶/0_DropPickable 藥斗擴充瓶  FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_CHEST_AXEBOT_AND_TURRETS
        },
        /*{
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S  小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            both Location.PRW_CHEST_BELOW_NODE and Location.PRW_CHEST_RIGHT_EXIT! See "PRW SPECIAL CASE" code in the patch method.
        },*/
        {
            "A2_S3/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_CHEST_GUARDED_BY_TURRET
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_VENTS
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.PRW_FLOWER
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_DGRD
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/EventBinder 中錢袋/LootProvider/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_CHEST_STATUE
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 左/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_LEFT_EXIT
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 左/1_DropPickable SceneObserve FSM 元能柱調查報告/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_RHIZOMATIC_ENERGY_METER
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 左/LootProvider 多功能工具組/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_ABOVE_NODE
        },
        /*{
            "???", // TODO: detect chip extraction properly, or was this part of the wider bug where the OnStateEnterImplement patch stopped working at all???
            Location.PRC_SHANHAI_CHIP

        SetVariableBoolAction_OnStateEnterImplement called on A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M61_A2_S1_索取A2記憶體_Chat02_Option3_Ans00/[Action] 選擇拔晶片->true (1)
        when you select the forcibly remove option
        ItemGetUIShowAction_Implement called on GO: A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片
        after the little removal cutscene is done

        choosing to pay triggers *none* of our existing patches?
        },*/
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/LootProvider 無懼玉/0_DropPickable 無懼玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_RIGHT_OF_PAGODA
        },
        /*{
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_GUARDED_BY_BEETLE and Location.PRC_CHEST_NEAR_MOVING_BOX
        },*/
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_BREAKABLE_WALL_RIGHT
        },
        {
            // breaking chest itself triggers "A2_S1/Room/Prefab/寶箱 Chests 右/EventBinder 中錢袋/LootProvider /Step Unlock FSM One Step Floor Secret Variant Variant/FSM Animator/View/Sealed Treasure/View/Platform/BR_TreasureDing_M/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            "A2_S1/Room/Prefab/寶箱 Chests 右/EventBinder 中錢袋/LootProvider /3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_STATUE
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRE_CHEST_UPPER_LEFT
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/EventBinder_開啟橋後觸發Boss Fight 算力元件/LootProvider 算力元件/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_DROP_JIAODUAN
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRE_CHEST_AFTER_LASERS
        },
        /*{
            // BR_JumperPrinter(Clone)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner
            // in case we ever want to turn the 320 jin into another location
            "", // didn't call our patches ???
            Location.PRE_CHEST_UNDER_BOX
        },*/
        {
            "A2_S2/Room/Prefab/寶箱 Chests/EventBinder 小錢袋/LootProvider/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_STATUE
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_UPPER_RIGHT
        },
    };
    /* unused gopaths:
     * 
     * [Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/RitualFlowerItem/LootProvider/After/LogicRoot/FSM Animator/SimpleCutSceneFSM_EncounterBoar (開頭介紹野豬的演出)/CullingGroup/[On]Node/LogicRoot/FSM Animator/軒軒野豬情境OnOff FSM/Room/A0_S4 gameLevel
     * for the Crimson Hibiscus dropped by the boar in the intro sequence
     *
     * AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/Butterfly_CutSceneFSM/--[States]/FSM/[State] GetButterfly/[Action]GetButterfly 玄蝶 狀態列
     * for the mystic nymph next to Yi's vital sanctum, won't be reachable once we make the FSP into a proper starting hub area
     */

    // Receiving items from cutscenes
    [HarmonyPrefix, HarmonyPatch(typeof(ItemGetUIShowAction), "Implement")]
    static bool ItemGetUIShowAction_Implement(ItemGetUIShowAction __instance) {
        Log.Info($"ItemGetUIShowAction_Implement called on {__instance.item.Title}");

        var goPath = GetFullPath(__instance.gameObject);
        Log.Info($"ItemGetUIShowAction_Implement called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        return true; // not a randomized location, let vanilla impl handle this
    }

    // Picking up chest items, enemy item drops, examining database entries
    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        Log.Info($"PickItemAction_OnStateEnterImplement called on {__instance.GetInstanceID()} containing: {__instance.pickItemData.name}\n{__instance.pickItemData?.Title}\n{__instance.pickItemData?.Summary}\n{__instance.pickItemData?.Description}");
        if (__instance.scheme != PickableScheme.GetItem) {
            Log.Info($"PickItemAction_OnStateEnterImplement: this is not a GetItem action, letting vanilla code handle it");
            return true;
        }

        var goPath = GetFullPath(__instance.gameObject);
        Log.Info($"PickItemAction_OnStateEnterImplement called on GO: {goPath}");

        // Use Traverse to access private field
        ItemProvider itemProvider = Traverse.Create(__instance).Field("itemProvider").GetValue() as ItemProvider;

        GameFlagDescriptable gameFlagDescriptable = ((!(itemProvider != null) || !(itemProvider.item != null)) ? __instance.pickItemData : itemProvider.item);
        Log.Info($"PickItemAction_OnStateEnterImplement gfd: {gameFlagDescriptable.Title}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"PickItemAction_OnStateEnterImplement ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        Log.Info($"PickItemAction_OnStateEnterImplement ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }

    // Absorbing tianhou flowers/tao fruits
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerIncreaseSkillPointAction), "OnStateEnterImplement")]
    static bool PlayerIncreaseSkillPointAction_OnStateEnterImplement(PlayerIncreaseSkillPointAction __instance) {
        var goPath = GetFullPath(__instance.gameObject);
        Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement called on {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }

    // This gets called for anything that drops items, including killing enemies, but we only use it
    // for "money-only" chests since those locations don't involve an "item" the above patch would get.
    // We patch CheckGenerateItems instead of GenerateItems because CGI only gets invoked at actual drop time, while
    // GI also gets invoked preemptively by EnterLevelReset every time the chest/enemy/etc gets loaded into a scene.
    [HarmonyPostfix, HarmonyPatch(typeof(LootSpawner), "CheckGenerateItems")]
    static void LootSpawner_CheckGenerateItems(LootSpawner __instance) {
        var goPath = GetFullPath(__instance.gameObject);
        Log.Info($"LootSpawner_CheckGenerateItems called on GO: {goPath}");

        // TODO: how many duplicate names like this are there? may have to rethink chest identification if this keeps happening
        if (goPath == "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S  小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner") {
            Log.Info($"LootSpawner_CheckGenerateItems PRW SPECIAL CASE");
            var chestGo = __instance.gameObject.transform.parent.parent.parent.parent.parent.parent.gameObject;
            var allChestsGo = chestGo.transform.parent.gameObject;

            // PRW_CHEST_RIGHT_EXIT is "寶箱 Chests"'s 2nd child / index 1, while PRW_CHEST_BELOW_NODE is its 3rd child / index 2
            if (allChestsGo.transform.GetChild(1).gameObject == chestGo) {
                CheckLocation(Location.PRW_CHEST_RIGHT_EXIT);
            } else {
                CheckLocation(Location.PRW_CHEST_BELOW_NODE);
            }
            return;
        } else if (goPath == "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner") {
            Log.Info($"LootSpawner_CheckGenerateItems PRC SPECIAL CASE");
            var chestGo = __instance.gameObject.transform.parent.parent.parent.parent.parent.parent.gameObject;
            var allChestsGo = chestGo.transform.parent.gameObject;

            // PRC_CHEST_NEAR_MOVING_BOX is "寶箱 Chests 右"'s 2nd child / index 1, while PRC_CHEST_GUARDED_BY_BEETLE is its 4th child / index 3
            if (allChestsGo.transform.GetChild(1).gameObject == chestGo) {
                CheckLocation(Location.PRC_CHEST_GUARDED_BY_BEETLE);
            } else {
                CheckLocation(Location.PRC_CHEST_NEAR_MOVING_BOX);
            }
            return;
        }

        if (goPathToLocation.ContainsKey(goPath)) {
            var dropItemPrefabs = AccessTools.FieldRefAccess<LootSpawner, List<DropItem>>("dropItemPrefabs").Invoke(__instance);
            string dropDesc = "";
            foreach (var dropItem in dropItemPrefabs) {
                if (dropItem.TryGetComponent<DropMoney>(out var dm))
                    dropDesc += $", {dm.moneyValue} jin";
                else
                    dropDesc += $", {dropItem.name} (not money)";
            }

            Log.Info($"LootSpawner_CheckGenerateItems ContainsKey() true for GO\n{goPath}\nClearing dropItemPrefabs which were {dropDesc}.");
            dropItemPrefabs.Clear();

            CheckLocation(goPathToLocation[goPath]);
        }

        //Log.Info($"LootSpawner_CheckGenerateItems ContainsKey() false");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GuideFishLogic), "ConfirmKillFish")]
    static bool GuideFishLogic_ConfirmKillFish(GuideFishLogic __instance) {
        var goPath = GetFullPath(__instance.gameObject);
        Log.Info($"GuideFishLogic_ConfirmKillFish called on {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"GuideFishLogic_ConfirmKillFish ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        Log.Info($"GuideFishLogic_ConfirmKillFish ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }
}
