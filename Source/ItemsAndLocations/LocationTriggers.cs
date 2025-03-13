using HarmonyLib;
using NineSolsAPI;
using RCGFSM.Items;
using RCGFSM.PlayerAction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RCGFSM.Items.PickItemAction;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LocationTriggers {
    public static void CheckLocation(Location location) {
        Log.Info($"CheckLocation() called with Archipelago location: {location}");

        var locationsChecked = APSaveManager.CurrentAPSaveData.locationsChecked;
        var locationEnumName = location.ToString();
        if (!locationsChecked.ContainsKey(locationEnumName))
            locationsChecked[locationEnumName] = false;

        if (locationsChecked[locationEnumName]) return;

        locationsChecked[locationEnumName] = true;
        APSaveManager.WriteCurrentSaveFile();

        if (!LocationNames.locationToArchipelagoId.ContainsKey(location)) {
            Log.Error($"Location {location} is missing an AP id, so not sending anything to the AP server");
            return;
        }

        ToastManager.Toast($"CheckLocation() telling the AP server that Location.{location} / \"{LocationNames.locationNames[location]}\" was just checked.");

        var locationId = LocationNames.locationToArchipelagoId[location];
        // we want to time out relatively quickly if the server happens to be down, but don't
        // block whatever we (and the vanilla game) were doing on waiting for the AP server response
        var _ = Task.Run(() => {
            var checkLocationTask = Task.Run(() => ConnectionAndPopups.APSession!.Locations.CompleteLocationChecks(locationId));
            if (!checkLocationTask.Wait(TimeSpan.FromSeconds(2))) {
                var msg = $"AP server timed out when we tried to tell it that you checked location '{LocationNames.locationNames[location]}'. Did the connection go down?";
                Log.Warning(msg);
                ToastManager.Toast($"<color='orange'>{msg}</color>");
            }
        });

        // For now, we want Chiyou's behavior in randomizer to be: move into the FSP when his Bridge location is checked
        if (location == Location.FGH_CHIYOU_BRIDGE) {
            Log.Info("Moving Chiyou into FSP now that \"Factory (GH): Raise the Bridge for Chiyou\" has been checked");
            var chiyouRescuedYiAndMovedIntoFSPFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool"];
            chiyouRescuedYiAndMovedIntoFSPFlag.CurrentValue = true;
        }
    }

    // A "full path" is a slash-delimited sequence of GameObject names, e.g. RootGameObject/NextObject/AnotherObject/LeafObject.
    // But a GameObject can have multiple child GOs with identical names, so full path is not enough to uniquely identify an object.
    // Nine Sols does this *a lot* unfortunately. It mostly affects us with GOs representing Jin chests, especially same-sized chests.
    // So for us a "disambiguated" path adds child indices with a ### where necessary: RootGameObject/NextObject###1/AnotherObject/LeafObject.
    // I chose multiple #s because I found a single # in a long path was too easy to overlook.
    public static string GetFullDisambiguatedPath(GameObject go) {
        var transform = go.transform;
        List<string> pathParts = new List<string>();

        while (transform != null) {
            var currentGOName = transform.name;

            var parent = transform.parent;
            if (parent != null && parent.childCount > 1) {
                bool hasSiblingWithIdenticalName = false;
                for (var i = 0; i < parent.childCount; ++i) {
                    var sibling = parent.GetChild(i);
                    if (sibling.name == currentGOName && sibling != transform) {
                        hasSiblingWithIdenticalName = true;
                        break;
                    }
                }
                if (hasSiblingWithIdenticalName)
                    currentGOName += "###" + transform.GetSiblingIndex();
            }

            pathParts.Add(currentGOName);
            transform = transform.parent;
        }

        pathParts.Reverse();
        return string.Join("/", pathParts);
    }

    private static Dictionary<string, Location> goPathToLocation = new Dictionary<string, Location> {
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
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_S 小寶箱_小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_UPPER_PAGODA_RIGHT
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_S 小寶箱_小量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_UPPER_PAGODA_LEFT
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
        { // for the skill point increase
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.AFD_FLOWER_UNDER_ELEVATOR
        },
        { // for the Tao Fruit inventory item
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
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
            "AG_S1/Room/Prefab/寶箱/LootProvider 中錢袋/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_CHEST_VENTS
        },
        {
            "AG_SG1/Room/LootProvider 藥斗擴充瓶/0_DropPickable 藥斗擴充瓶  FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_CHEST_AXEBOT_AND_TURRETS
        },

        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S  小量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_BELOW_NODE
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S  小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_RIGHT_EXIT
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_CHEST_GUARDED_BY_TURRET
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_VENTS
        },
        { // for the skill point increase
            "A2_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.PRW_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A2_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
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
            Location.PRC_CHEST_LEFT_OF_BRIDGE
        },
        {
            "A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.PRC_SHANHAI_CHIP // removal by force
        },
        {
            "A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M61_A2_S1_索取A2記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.PRC_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/LootProvider 無懼玉/0_DropPickable 無懼玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_RIGHT_OF_PAGODA
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_GUARDED_BY_BEETLE
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_S 小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_NEAR_MOVING_BOX
        },
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
        // TODO: figure out why this location doesn't trigger / picking up the item here doesn't give Yi an item???
        {
            "A2_S2/Room/Prefab/寶箱 Chests/LootProvider 收金玉/0_DropPickable 收金玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_UNDER_BOX
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/EventBinder 小錢袋/LootProvider/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_STATUE
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_UPPER_RIGHT
        },

        {
            "A2_S5_ BossHorseman_GameLevel/Room/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.RP_CONTROL_PANEL
        },
        {
            "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/LootProvider/0_DropPickable 藥斗功率 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.RP_DROP_YINGZHAO
        },
        {
            "A2_S5_ BossHorseman_GameLevel/Room/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.RP_KUAFU_SANCTUM
        },

        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###6/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_MIDDLE_1
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_MIDDLE_2
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_RIGHT
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋###1/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_CHEST_LEFT_POOL_ABOVE
        },
        {
            "A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/FSM Animator/LogicRoot/[Off]Node/SimpleCutSceneFSM_初次遇見姬/--[States]/FSM/[State] PlayCutScene/[Action] 取得樂譜",
            Location.LYR_JI_MUSIC
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_TOWER
        },
        {
            "A3_SG4/Room/Prefab/LootProvider 菸斗擴充/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_CHEST_TOWER_ROOM
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/LootProvider 中錢袋 + 中量金/2_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_CHEST_ABOVE_NODE
        },
        {
            "A3_SG1/Room/Prefab/EventBinder/LootProvider/0_DropPickable 不動玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_CHEST_STATUES_ROOM
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM 瑤池碑文/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_LAKE_STELE
        },
        {
            "A3_SG1/Room/Prefab/LootProvider/0_DropPickable 奪魂玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_CHEST_NYMPH_ROOM
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋###0/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_CHEST_RIGHT_EXIT
        },

        {
            "A3_S2/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_CHEST_NYMPH_ROPE
        },
        {
            "A3_S2/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_WATER_REPORT
        },
        {
            "A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.GH_SHANHAI_CHIP // removal by force
        },
        {
            "A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M87_A3_S2_A3地圖魚索取記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.GH_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###0/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GH_CHEST_RIGHT_HANGING_POOL
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GH_UPPER_LEVEL_FOLIAGE
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/LootProvider 文物種子/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_UNDERWATER_VASE
        },
        {
            //"A3_S2/Room/Prefab/寶箱 Chests/LootProvider 小錢袋 & 中量金/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner", // for breaking the chest
            "A3_S2/Room/Prefab/寶箱 Chests/LootProvider 小錢袋 & 中量金/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_CHEST_LEFT_HANGING_POOL
        },
        {
            "A3_S2/Room/Prefab/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_MUTATED_CROPS
        },
        {
            "A3_S2/Room/Prefab/Gameplay_8/RCGEventSharingGroup/LootProvider 貪財玉/0_DropPickable 貪財玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_DROP_SHUIGUI
        },

        {
            "A3_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.WOS_CHEST_HIGH_PLATFORMS_RIGHT
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/[Mech]GiantTreasureChest FSM_InteractVer Variant 大量金/FSM Animator/View/TreasureBox_L/LogicRoot/Loot Spawner_1",
            Location.WOS_CHEST_HIGH_PLATFORMS_LEFT
        },
        { // for the skill point increase
            "A3_S3/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.WOS_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A3_S3/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.WOS_FLOWER
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.WOS_DGRD
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM 造水造氧管線/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.WOS_PIPELINE_PANEL
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/LootProvider 丹藥催化器/0_DropPickable 丹藥催化器 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.WOS_SHAFT_NEAR_NODE
        },

        {
            "A3_S5_BossGouMang_GameLevel/Room/Treasure Chests 寶箱/LootProvider_文物即食口糧&中錢袋/0_DropPickable_即食口糧 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AH_CHEST_GOUMANG_1
        },
        {
            "A3_S5_BossGouMang_GameLevel/Room/Treasure Chests 寶箱/LootProvider_文物即食口糧&中錢袋/0_DropPickable_中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AH_CHEST_GOUMANG_2
        },
        {
            "A3_S5_BossGouMang_GameLevel/Room/Sleeppod/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.AH_GOUMANG_SANCTUM
        },

        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 修練玉/0_DropPickable 修練玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_CHEST_UPPER_CAVES_TOP_LEFT
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.YC_CHEST_UPPER_CAVES_BOTTOM_LEFT
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/1_DropPickable 耕地標示/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_FARMLAND_MARKINGS
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 大錢袋/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_ABOVE_MARKINGS
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_CHEST_MIDDLE_CAVE
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 應龍金卵/0_DropPickable 應龍金卵 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_CAVE_EGG
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###0/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.YC_NEAR_NODE
        },

        // Receiving the Azure Bow from Kuafu is implemented as several "items", and we'd like to prevent any of them from applying until you actually get the AP items for bows/arrows
        { // the Azure Bow Inventory entry
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得弓箭",
            Location.FSP_KUAFU_GIFT_1
        },
        { // the Azure Sand Inventory entry
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得蒼砂",
            Location.FSP_KUAFU_GIFT_1
        },
        { // the arrow type selector on the Status
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得[狀態欄]蒼弓",
            Location.FSP_KUAFU_GIFT_1
        },
        { // this one is the actual bow-firing ability
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得弓箭能力",
            Location.FSP_KUAFU_GIFT_1
        },

        {
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得貫穿弓箭",
            Location.FSP_KUAFU_GIFT_2
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/FSM Animator/CutScene/[CutScene] 古樂譜_解出曲子/--[States]/FSM/[State] PlayCutScene/[Action] 取得簡譜",
            Location.FSP_SHUANSHUAN_MUSIC
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/FSM Animator/CutScene/[CutScene] 文房四寶_畫羿/--[States]/FSM/[State] PlayCutScene/[Action] 拿到畫作",
            Location.FSP_SHUANSHUAN_PORTRAIT
        },
        {
            "AG_S2/Room/NPCs/NPC_ChiYou_BaseFSM/FSM Animator/LogicRoot/NPC_ChiYou Variant/General FSM Object/Animator(FSM)/LogicRoot/SimpleCutSceneFSM_蚩尤買畫/--[States]/FSM/[State] 買畫結果/[Action] 拿到借力玉",
            Location.FSP_CHIYOU_PORTRAIT
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/FSM Animator/CutScene/[CtuScene] 軒軒神農擔心桃花村/--[States]/FSM/[State] 給桃花村鑰匙/[Action] 給桃花村鑰匙",
            Location.FSP_SHENNONG_PBV_QUEST
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/軒軒小說 FSM/--[States]/FSM/[State] 拿到小說/[Action] 拿到小說",
            Location.FSP_SHUANSHUAN_BOOK
        },
        {
            "AG_S2/Room/NPCs/NPC_ChiYou_BaseFSM/FSM Animator/LogicRoot/NPC_ChiYou Variant/General FSM Object/Animator(FSM)/LogicRoot/SimpleCutSceneFSM_蚩尤買小說/--[States]/FSM/[State] 買小說結果/[Action] 拿到化傷玉",
            Location.FSP_CHIYOU_BOOK
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/ShanShan 軒軒分身 FSM/FSM Animator/CutScene/[CutScene] 名畫作_畫作解謎/--[States]/FSM/[State] 取得畫作謎題/[Action] 取得畫作謎題",
            Location.FSP_SHUANSHUAN_HIDDEN_POEM
        },
        {
            "AG_S2/Room/Prefab/Treasure Chests 寶箱/LootProvider 刺蝟玉/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FSP_CHEST_HALF_TREE
        },
        {
            "AG_S2/Room/Prefab/Treasure Chests 寶箱/LootProvider 算力元件/0_DropPickable 算力元件 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FSP_CHEST_FULL_TREE_1
        },
        {
            "AG_S2/Room/Prefab/Treasure Chests 寶箱/LootProvider 大錢袋 & 大量金/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FSP_CHEST_FULL_TREE_2
        },
        // [CutScene]血清&原始細胞 trigger for both Serum and Hair at the same time
        { // 道具 / tool, has the ItemGetUIShowAction component
            "AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/NPC_AICore_Base/NPC_AICore_Base_FSM/FSM Animator/LogicRoot/NPC_AICore_FSM/General FSM Object/Animator(FSM)/LogicRoot/[CutScene]血清&原始細胞/[EnableInvoker] 取得符爆能力/取得符爆天禍道具",
            Location.FSP_MUTANT_QUEST
        },
        { // 能力 / ability, has the CutsceneGetItem component
            "AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/NPC_AICore_Base/NPC_AICore_Base_FSM/FSM Animator/LogicRoot/NPC_AICore_FSM/General FSM Object/Animator(FSM)/LogicRoot/[CutScene]血清&原始細胞/[EnableInvoker] 取得符爆能力/取得符爆天禍能力",
            Location.FSP_MUTANT_QUEST
        },
        // [CutScene]原始細胞 trigger for bringing Hair after having the Serum conversation
        { // 道具 / tool, has the ItemGetUIShowAction component
            "AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/NPC_AICore_Base/NPC_AICore_Base_FSM/FSM Animator/LogicRoot/NPC_AICore_FSM/General FSM Object/Animator(FSM)/LogicRoot/[CutScene]原始細胞/[EnableInvoker] 取得符爆能力/取得符爆天禍道具",
            Location.FSP_MUTANT_QUEST
        },
        { // 能力 / ability, has the CutsceneGetItem component
            "AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/NPC_AICore_Base/NPC_AICore_Base_FSM/FSM Animator/LogicRoot/NPC_AICore_FSM/General FSM Object/Animator(FSM)/LogicRoot/[CutScene]原始細胞/[EnableInvoker] 取得符爆能力/取得符爆天禍能力",
            Location.FSP_MUTANT_QUEST
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/--[States]/FSM/[State] 製作追蹤箭/[Action] 取得追蹤箭",
            Location.FSP_KUAFU_HOMING_DARTS
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/--[States]/FSM/[State] 製作爆破箭/[Action] 取得爆破箭",
            Location.FSP_KUAFU_THUNDERBURST_BOMB
        },

        {
            "A7_S1/Room/Prefab/A7_S1_三階段FSM/FSM Animator/Phase4_AfterBossFight/靈樞入口 FSM Object/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.CC_LADY_ETHEREAL
        },
        { // for the skill point increase
            "A7_S1/Room/Prefab/A7_S1_ 三階段FSM/FSM Animator/Phase4_AfterBossFight/Pickable_DIESunDeadbody 伏蝶屍體 大道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.CC_FLOWER_LADY_ETHEREAL
        },
        { // for the Greater Tao Fruit inventory item
            "A7_S1/Room/Prefab/A7_S1_三階段FSM/FSM Animator/Phase4_AfterBossFight/Pickable_DIESunDeadbody 伏蝶屍體 大道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.CC_FLOWER_LADY_ETHEREAL
        },
        {
            "A7_S1/Room/NPC_GuideFish_A7/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.CC_SHANHAI_CHIP // removal by force
        },
        {
            "A7_S1/Room/NPC_GuideFish_A7/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M190_A7_S1_A7地圖魚索取記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.CC_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A7_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.CC_CHEST_CAVES_UPPER_RIGHT
        },
        {
            "A7_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.CC_CHEST_CAVES_CENTER
        },
        {
            "A7_S1/Room/Prefab/寶箱 Chests/LootProvider 名畫作/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CC_CHEST_LEFT_EXIT
        },

        {
            "A5_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_CHEST_RIGHT_SHIELD_ORB
        },
        {
            "A5_S1/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_CHEST_NEAR_NODE
        },
        {
            "A5_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FGH_CHEST_MIDDLE_SHIELD_ORB
        },
        { // for the skill point increase
            "A5_S1/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.FGH_HAMMER_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A5_S1/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.FGH_HAMMER_FLOWER
        },
        {
            "A5_S1/Room/FlashKill Binding/截家家訓鐵鎚 FSM Object Variant/FSM Animator/LogicRoot/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_HAMMER_BROS
        },
        {
            "A5_S1/Room/Prefab/寶箱 Chests/1_DropPickable 煉丹爐系統報告 FSM 煉丹爐心狀態/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_MONITOR
        },
        { // "V3" I assume if you spoke to Chiyou in other locations first? this is what I got on a full-game playthrough
            "A5_S1/Room/Prefab/BridgeLogic/NPC_ChiYou_A5狀態FSM Variant/FSM Animator/LogicRoot/NPC_ChiYou_野外/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  開橋後感謝_V3/[EndAction]/[Action]開橋謝禮",
            Location.FGH_CHIYOU_BRIDGE
        },
        { // "V4" if you spoke to Chiyou before lowering the bridge
            "A5_S1/Room/Prefab/BridgeLogic/NPC_ChiYou_A5狀態FSM Variant/FSM Animator/LogicRoot/NPC_ChiYou_野外/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  開橋後感謝_V4/[EndAction]/[Action]開橋謝禮",
            Location.FGH_CHIYOU_BRIDGE
        },
        { // "V5" if this is your first time speaking to Chiyou
            "A5_S1/Room/Prefab/BridgeLogic/NPC_ChiYou_A5狀態FSM Variant/FSM Animator/LogicRoot/NPC_ChiYou_野外/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  開橋後感謝_V5/[EndAction]/[Action]開橋謝禮",
            Location.FGH_CHIYOU_BRIDGE
        },
        // I don't know if the V1-3 GOs are ever used or relevant to this location
        {
            "A5_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FGH_CHEST_ABOVE_NODE
        },
        {
            "A5_S1/Room/Prefab/寶箱 Chests/LootProvider 大錢袋/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_CHEST_RIGHT_ELEVATOR
        },
        {
            "A5_S4b/Room/Simple Binding Tool/ZombiePodFSM/FSM Animator/LogicRoot/LootProvider/0_DropPickable 肥料 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_PLATFORM_ROOM_BALL
        },
        {
            "A5_S4b/Room/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FGH_PLATFORM_ROOM_DGRD
        },
        { // for the skill point increase
            "A5_S4b/Room/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.FGH_PLATFORM_ROOM_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A5_S4b/Room/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.FGH_PLATFORM_ROOM_FLOWER
        },

        {
            "A6_S1/Room/Prefab/寶箱 Chests/LootProvider 速落玉/0_DropPickable 速落玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_CHEST_UPPER_RIGHT_EXIT
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/LootProvider 中錢袋###3/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_DROP_KUIYAN
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FU_CHEST_LOWER_ELEVATOR
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/StatueTreasureEventBinder 調息玉 /LootProvider 大錢袋/0_DropPickable 大錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_CHEST_STATUES
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/0_DropPickable A1&A6地圖魚晶片/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_DROP_SHANHAI
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM 對掘金者的撤離通知/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_EVACUATION_NOTICE
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_CHEST_ABOVE_NODE
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FU_CHEST_BELOW_LEFT_EXIT
        },
        {
            "A6_S1/Room/Prefab/寶箱 Chests/LootProvider 中錢袋###2/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FU_CHEST_BEHIND_BOXES
        },

        {
            "A4_S1/Room/NPC_GuideFish_A4/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.OW_SHANHAI_CHIP // removal by force
        },
        {
            "A4_S1/Room/NPC_GuideFish_A4/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M114_A4_S1_A4地圖魚索取記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.OW_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A4_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.OW_CHEST_ABOVE_SOL_STATUE
        },
        {
            "A4_S1/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.OW_CHEST_CRUSHER_GAUNTLET
        },
        {
            "A4_SG7/Room/Prefab/Level_1/LootProvider 調息玉/4_DropPickable 調息玉 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.OW_CHEST_GAUNTLET_ROOM
        },
        {
            "A4_S1/Room/Prefab/寶箱 Chests/LootProvider 虛擬穿戴裝置 VR/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.OW_CHEST_VENTS_ROBOT
        },
        {
            "A4_S1/Room/Prefab/寶箱 Chests/CullingGroup(特規)(含筆記) 大錢袋/MovingPlatform_Group_A4 Variant (1)/PlatformRoot/LootProvider 大錢袋/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.OW_DROP_VENT_CRATE
        },
        {
            "A4_S1/Room/Prefab/寶箱 Chests/1_DropPickable 運輸系統回報 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.OW_DATABASE
        },

        {
            "A4_S2/Room/Prefab/寶箱 Chests/EventBinder 藥草催化器/LootProvider 藥草催化器/0_DropPickable 藥斗功率材料 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.IW_CHEST_STATUES
        },
        {
            "A4_S2/Room/Prefab/寶箱 Chests/LootProvider 大錢袋/3_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.IW_CHEST_WALKING
        },
        {
            "A4_S2/Room/Prefab/寶箱 Chests/LootProvider 風火環/4_DropPickable 風火環 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.IW_DROP_TIEYAN
        },
        {
            "A4_SG1/Room/Prefab/LootProvider 龍蟲蛻/0_DropPickable 藥材 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.IW_NYMPH_PUZZLE_ROOM
        },
        {
            "A4_S2/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.IW_DGRD
        },
        { // for the skill point increase
            "A4_S2/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.IW_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A4_S2/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.IW_FLOWER
        },

        {
            "A4_S3/Room/EventBinder 玄鐵 戰神原型機圖示/1_DropPickable SceneObserve FSM 戰神原型機圖示/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_CONSOLE
        },
        {
            "A4_S3/Room/Prefab/LootProvider 爆破箭藍圖/0_DropPickable 爆破箭藍圖 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_CHEST_NEAR_CONSOLE
        },
        {
            "A4_S4/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.BR_GAUNTLET_1_CHEST
        },
        {
            "A4_S4/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.BR_GAUNTLET_2_CHEST
        },
        {
            "A4_S4/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.BR_GAUNTLET_2_CHEST_LASERS
        },
        {
            "A4_S4/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###0/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.BR_GAUNTLET_2_CHEST_BEETLE
        },
        {
            "A4_SG4/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_VAULT_CHEST_1
        },
        {
            "A4_SG4/Room/Prefab/寶箱 Chests/LootProvider 七傷玉/3_DropPickable 七傷玉 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_VAULT_CHEST_2
        },
        {
            "A4_SG4/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM 寶庫承辦筆記/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_VAULT_SCROLL
        },
        {
            "A4_SG4/Room/Prefab/寶箱 Chests/LootProvider 玄鐵/0_DropPickable 弓箭升級 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_VAULT_CHEST_3
        },
        {
            "A4_SG4/Room/Prefab/寶箱 Chests/LootProvider 文物 道長寶物/0_DropPickable 道長寶物 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.BR_VAULT_CHEST_4
        },

        { // for the skill point increase
            "A0_S6/Room/Prefab/SimpleCutSceneFSM_道長死亡/FSM Animator/LogicRoot/Cutscene_TaoChangPart2/TaoChair/Ball/TaoChang/DIESunFlower_FSM/--[States]/FSM/[State] Flower_Picking/[Action] PlayerIncreaseSkillPointAction",
            Location.YH_FLOWER
        },
        { // for the Greater Tao Fruit inventory item
            "A0_S6/Room/Prefab/SimpleCutSceneFSM_道長死亡/FSM Animator/LogicRoot/Cutscene_TaoChangPart2/TaoChair/Ball/TaoChang/DIESunFlower_FSM/--[States]/FSM/[State] Flower_Picking/[Action] 拿到道果",
            Location.YH_FLOWER
        },
        {
            // also "A0_S6/Room/Prefab/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] AirBridgeConnect/[Action] 取得奄老百科二階" for Yanlao expanded db entry
            "A0_S6/Room/Prefab/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] AirBridgeConnect/[Action] ItemGetUIShowAction",
            Location.YH_VITAL_SANCTUM
        },

        {
            "A5_S2/Room/Prefab/寶箱  Chests/1_DropPickable 逃獄一/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.P_SCROLL_LOWER_RIGHT
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/LootProvider 群咒玉/0_DropPickable 群咒玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.P_CHEST_LOWER_LEFT
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###8/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.P_CHEST_ABOVE_STAIRS
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.P_CHEST_NEAR_NODE
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/1_DropPickable 逃獄二/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.P_SCROLL_UPPER_LEFT
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###7/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.P_CHESTS_BEFORE_KANGHUI_1
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.P_CHESTS_BEFORE_KANGHUI_2
        },
        {
            "A5_S2/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###6/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.P_CHESTS_BEFORE_KANGHUI_3
        },
        {
            "A5_S2/Room/JailBossRoom/LootProvider/0_DropPickable 戒指 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.P_DROP_KANGHUI
        },

        {
            "A5_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FMR_CHEST_RIGHT_ELEVATOR
        },
        {
            "A5_S3/Room/Prefab/寶箱 Chests/LootProvider 藥草催化器/0_DropPickable 藥斗功率升級 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FMR_CHEST_ELEVATOR_TURRET
        },
        {
            "A5_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FMR_CHEST_LEFT_ELEVATOR
        },
        {
            "A5_S3/Room/Prefab/寶箱 Chests/LootProvider 大錢袋/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FMR_CHEST_WALKING_GREEN_PILLAR
        },
        {
            "A5_S3/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FMR_CHEST_TURRET_GREEN_PILLAR
        },
        {
            "A5_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FMR_CHEST_NEAR_MOVING_PLATFORMS
        },
        { // for the skill point increase
            "A5_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 大道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.FMR_FLOWER
        },
        { // for the Greater Tao Fruit inventory item
            "A5_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 大道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.FMR_FLOWER
        },

        {
            "A5_S4/Room/Prefab/寶箱 Chest/LootProvider 中錢袋###2/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_CHEST_VENTS_LOWER_LEFT
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/BR_TreasureDing_S 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FPA_CHEST_NEAR_JIEQUAN_STATUE
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FPA_CHEST_TRIPLE_GUARD_SPAWNER
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/LootProvider 中錢袋###8/2_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_CHEST_VENTS_UPPER_LEFT
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/1_DropPickable 煉丹爐原理/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_PRODUCTION_STATION
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/LootProvider 小錢袋###1/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_VENTS_BELOW_PRODUCTION
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/LootProvider 小錢袋###6/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_CHEST_BELOW_DOUBLE_SNIPER
        },
        {
            "A5_S4b/Room/Prefab/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_PHARMACY_PANEL
        },
        {
            "A5_S4b/Room/Simple Binding Tool/ZombiePodFSM/FSM Animator/LogicRoot/LootProvider/0_DropPickable 毒藥 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_PHARMACY_BALL
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/LootProvider 小錢袋###5/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_CHEST_RIGHT_FIRE_ZONE_BOTTOM
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/A5地圖魚暴走情境 FSM Object Variant A5山海RAM/FSM Animator/LogicRoot/LootProvider/0_DropPickable A5山海RAM FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_DEFEAT_SHANHAI
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/LootProvider 藥材 餘毛/0_DropPickable 藥材 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_RIGHT_FIRE_ZONE_TOP
        },
        {
            "A5_S4/Room/Prefab/寶箱 Chest/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.FPA_CHEST_PAST_WUQIANG
        },
        {
            "A5_S4/Room/LootProvider/0_DropPickable 爆劍玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.FPA_DROP_WUQIANG
        },

        {
            "A5_S5/Room/Prefab/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.SH_HAOTIAN_SPHERE
        },
        {
            "A5_S5/Room/寶箱 Chests/LootProvider 文物寶劍/0_DropPickable 截氏寶劍 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.SH_CHEST_RIGHT
        },
        { // for the skill point increase
            "A5_S5/Room/EventBinder/DIESunFlower_FSM/--[States]/FSM/[State] Flower_Picking/[Action] PlayerIncreaseSkillPointAction",
            Location.SH_JIEQUAN_FLOWER
        },
        { // for the Greater Tao Fruit inventory item
            "A5_S5/Room/EventBinder/DIESunFlower_FSM/--[States]/FSM/[State] Flower_Picking/[Action] 拿到道果",
            Location.SH_JIEQUAN_FLOWER
        },
        {
            "A5_S5/Room/寶箱 Chests/LootProvider 尋敵鏢藍圖/0_DropPickable 追蹤弓材料 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.SH_CHEST_LEFT
        },
        {
            "A5_S5/Room/Simple Binding Tool 暈眩情境/Sleeppod/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.SH_JIEQUAN_SANCTUM
        },

        {
            "A6_S3/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AM_CHEST_ABOVE_LEFT_EXIT
        },
        {
            "A6_S3/Room/LootProvider/0_DropPickable Bag FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AM_DROP_YINYUE
        },
        {
            "A6_S3/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###0/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AM_CHEST_NEAR_NODE
        },
        {
            "A6_S3/Room/Prefab/寶箱  Chests/LootProvider 快攻玉/2_DropPickable 快攻玉 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AM_CHEST_WALKING
        },
        { // for the skill point increase
            "A6_S3/Room/Prefab/寶箱  Chests/Pickable_DIESunDeadbody 太陽人屍體 (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.AM_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A6_S3/Room/Prefab/寶箱  Chests/Pickable_DIESunDeadbody 太陽人屍體 (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.AM_FLOWER
        },
        {
            "A6_S3/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AM_CHEST_AFTER_FLOWER_1
        },
        {
            "A6_S3/Room/Prefab/寶箱  Chests/BR_TreasureDing_S 小量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AM_CHEST_AFTER_FLOWER_2
        },

        {
            "A0_S7/Room/Prefab/1_DropPickable 石洞內的碑文 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.UC_INSCRIPTION
        },
        {
            //"A0_S4 gameLevel/Room/Prefab/1_DropPickable 遺書 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem", // intro-only?
            "A0_S7/Room/Prefab/1_DropPickable 遺書 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem", // revisit during "real" game
            Location.UC_NOTE
        },
        {
            //"A0_S4 gameLevel/Room/Prefab/1_DropPickable 竹簡 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem", // intro-only?
            "A0_S7/Room/Prefab/1_DropPickable 竹簡 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem", // revisit during "real" game
            Location.UC_SCROLL
        },

        {
            "GameLevel/Room/Prefab/LootProvider 玄鐵/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            // this extra space was probably a typo???
            //"GameLevel/Room/Prefab/LootProvider 玄 鐵/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LHP_DROP_LIEGUAN
        },

        {
            "GameLevel/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GD_SIGN
        },
        { // for the skill point increase
            "GameLevel/Room/Prefab/Treasure Chests 寶箱/Pickable_DIESunDeadbody 太陽人屍體 (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.GD_FLOWER
        },
        { // for the Tao Fruit inventory item
            "GameLevel/Room/Prefab/Treasure Chests 寶箱/Pickable_DIESunDeadbody 太陽人屍體 (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.GD_FLOWER
        },
        {
            "GameLevel/Room/Prefab/村民避難所_階段 FSM Object/FSM Animator/View/村民避難所ControlRoom/Phase1(截全打電話後)/EventBinding/釋放村民 FSM/--[States]/FSM/[State] PlayCutScene/[Action] 取得毒物_豬黃",
            Location.GD_SHAMAN
        },

        {
            "A2_S6/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.CTH_CHEST_LARGE_ELEVATOR
        },
        {
            "A2_S6/Room/Prefab/寶箱 Chests/MiniBossFight 大錢袋/LootProvider 大錢袋/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CTH_DROP_YANREN
        },
        {
            "A4_SG2/Room/Prefab/LootProvider 玄鐵/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CTH_CHEST_SIDE_ROOM
        },
        {
            "A2_S6/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.CTH_CHEST_VENTS_LEFT
        },
        {
            "A2_S6/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CTH_ANOMALOUS_ROOT_NODE
        },
        {
            "A2_S6/Room/Prefab/寶箱 Chests/EventBinder 文房四寶/LootProvider 文房四寶/2_DropPickable 文房四寶 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CTH_CHEST_STATUES
        },

        {
            "A10_S1/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM (1) 創世經文/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSY_PAINTING
        },
        {
            "A10_S1/Room/[CutScene]關鳥籠/FSM Animator/LogicRoot/After/1_DropPickable SceneObserve FSM 殭屍棺材銘文/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSY_COFFIN
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/SecretTreasureRoom/BR_TreasureDing_M 中寶箱_中量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_TREASURE_1
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/SecretTreasureRoom/BR_TreasureDing_S 小量金 (2)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_TREASURE_2
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/SecretTreasureRoom/BR_TreasureDing_M 中寶箱_中量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_TREASURE_3
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/SecretTreasureRoom/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_TREASURE_4
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/SecretTreasureRoom/BR_TreasureDing_M 中寶箱_中量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_TREASURE_5
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/SecretTreasureRoom/[Mech]GiantTreasureChest FSM_InteractVer Variant_大量金/FSM Animator/View/TreasureBox_L/LogicRoot/Loot Spawner_1",
            Location.GOSY_CHEST_TREASURE_6
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/LootProvider 玄鐵/0_DropPickable 弓箭升級 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSY_CHEST_LOWER_PORTAL
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中寶箱 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_CAVES_LOWER_LEFT
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###6/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_MIDDLE_PORTAL_ALCOVE
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###7/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_MIDDLE_PORTAL_POOL
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_UPPER_RIGHT_PORTAL_1
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSY_CHEST_UPPER_RIGHT_PORTAL_2
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSY_CHEST_UPPER_RIGHT_PORTAL_3
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/LootProvider 文物棋盤/0_DropPickable 棋盤 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSY_CHEST_NEAR_GREENHOUSE_ROOF
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/LootProvider 劍氣玉/0_DropPickable 玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSY_CHEST_GREENHOUSE
        },
        {
            "A10_S1/Room/Prefab/寶箱 Chests/[Mech]GiantTreasureChest FSM_InteractVer Variant (1)/FSM Animator/View/TreasureBox_L/LogicRoot/Loot Spawner_1",
            Location.GOSY_CHEST_NEAR_UPPER_RIGHT_EXIT
        },

        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 小量金 (2)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_LURKER_GUARDED
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 小量金###7/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_PHANTOM_GUARDED
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/LootProvider 文物赤土/0_DropPickable 赤土 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_CHEST_SPIKE_HALL_UPPER_RIGHT
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/LootProvider 藥斗功率/0_DropPickable 藥斗強度材料 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_CHEST_SPIKE_HALL_UPPER_LEFT
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 小量金 (3)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_LURKERS_UNDER_WALKWAY
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 小量金 (4)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_PORTAL_BELOW_NODE
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_1
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_2
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 小量金 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_ALCOVE_BETWEEN_TOMBS_3
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSE_CHEST_ABOVE_YINJIFU_TOMB
        },
        {
            "A10_SG1_Cave1/Room/Prefab/A10_TombSet FSM/FSM Animator/LogicRoot/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_YINJIFU_MURAL
        },
        {
            "A10_S3/Room/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_CARVINGS
        },
        {
            "A10_S3/Room/AllTreasure 寶箱 Chests/LootProvider 蛤蟆衣/0_DropPickable 藥材 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_CHEST_OUTSIDE_GUIGUZI_TOMB
        },
        {
            "A10_SG2/Room/A10_TombSet FSM Variant/FSM Animator/LogicRoot/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSE_GUIGUZI_MURAL
        },

        {
            "A10_S4/Room/Treasure寶箱 Chests/LootProvider 中錢袋&中量金/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_CHEST_ABOVE_ELEVATOR
        },
        {
            "A10_S4/Room/Treasure寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GOSW_CHEST_TOP_MIDDLE_ROOM
        },
        {
            "A10_S4/Room/Treasure寶箱 Chests/LootProvider 大量金&神手玉/0_DropPickable 神手玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_CHEST_BELOW_WESTERN_CLIFFS
        },
        {
            "A10_S4/Room/Treasure寶箱 Chests/LootProvider 算力元件/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_CHEST_BELOW_LUYAN_TOMB
        },
        {
            "A10_SG4/Room/A10_TombSet FSM Variant/FSM Animator/LogicRoot/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_LUYAN_MURAL
        },
        // Yes, I compared the images of the 3 sages with the 3 bodies to make sure these locations were correctly labeled.
        { // for the skill point increase
            "A10_SG5/Room/Prefab/0_DropPickable 小道果 FSM_B/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.GOSW_YINJIFU_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A10_SG5/Room/Prefab/0_DropPickable 小道果 FSM_B/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_YINJIFU_FLOWER
        },
        { // for the skill point increase
            "A10_SG5/Room/Prefab/0_DropPickable 小道果 FSM_C/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.GOSW_GUIGUZI_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A10_SG5/Room/Prefab/0_DropPickable 小道果 FSM_C/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_GUIGUZI_FLOWER
        },
        { // for the skill point increase
            "A10_SG5/Room/Prefab/0_DropPickable 小道果 FSM_A/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.GOSW_LUYAN_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A10_SG5/Room/Prefab/0_DropPickable 小道果 FSM_A/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_LUYAN_FLOWER
        },
        {
            "A10_SG5/Room/Prefab/LootProvider 元能箭/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GOSW_CHEST_LEAR_GRAVE
        },

        {
            "A10S5/Room/Boss And Environment Binder/1_DropPickable 亮亮 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.ASP_JI
        },
        {
            "A10S5/Room/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.ASP_VITAL_SANCTUM
        },
        {
            "A10S5/Room/NPC_GuideFish_A10/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.ASP_SHANHAI_CHIP // removal by force
        },
        {
            "A10S5/Room/NPC_GuideFish_A10/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M241_A10_S5_A10地圖魚取得記憶體_Chat01_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.ASP_SHANHAI_CHIP // peaceful purchase
        },

        {
            "A9_S4/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小寶箱/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.ST_CHEST_LOWER_ELEVATOR
        },
        {
            "A9_S4/Room/Prefab/寶箱 Chests/LootProvider 中錢袋&中量金###2/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.ST_CHEST_WALL_CLIMB
        },
        {
            "A9_S4/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小寶箱 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.ST_CHEST_HAZARDS
        },
        {
            "A9_S4/Room/Prefab/寶箱 Chests/LootProvider 藥材尾帶/0_DropPickable 尾帶 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.ST_CHEST_ROPES_BELOW_NODE
        },
        { // for the skill point increase
            "A9_S4/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.ST_FLOWER_STOWAWAY
        },
        { // for the Tao Fruit inventory item
            "A9_S4/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.ST_FLOWER_STOWAWAY
        },
        {
            "A9_S4/A9_S4_Skin/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.ST_STOWAWAY
        },
        {
            "A9_S4/Room/Prefab/寶箱 Chests/LootProvider 文物肥料/0_DropPickable 肥料 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.ST_CHEST_NODE
        },
        {
            "A9_S4/Room/Prefab/寶箱 Chests/LootProvider 中錢袋&中量金###3/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.ST_CHEST_PINK_WATER
        },

        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/LootProvider 中錢袋###1/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDP_CHEST_ABOVE_TRANSPORTER
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/StatueEventBinder 大錢袋 & 中量金/Step Unlock FSM Two Step Floor Secret/FSM Animator/View/Sealed Treasure/View/Platform/LootProvider 大錢袋/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDP_CHEST_STATUES
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小量金###8/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDP_CHEST_NODE_ALCOVE_1
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小量金###9/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDP_CHEST_NODE_ALCOVE_2
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 (3)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDP_CHEST_SLIDER_RIGHT_OF_NODE
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDP_CHEST_LEFT_PINK_POOL
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/LootProvider 藥斗升級/0_DropPickable 藥斗升級 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDP_CHEST_FAR_LEFT
        },
        {
            "A9_S1/Room/Prefab/WaterFallEventBinder/LootProvider 弓箭強度 玄鐵/0_DropPickable 弓箭強度 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDP_DROP_TIANSHOU
        },
        {
            "A9_S1/Room/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDP_WATER_TOWER
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小寶箱 (2)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDP_CHEST_LASERS_1
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/LootProvider 中錢袋###7/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDP_CHEST_LASERS_2
        },
        {
            "A9_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M 中寶箱 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDP_CHEST_HALLWAY_TURRET
        },

        {
            "A9_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小寶箱 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDLA_CHEST_RIGHT_ELEVATOR
        },
        {
            "A9_S2/Room/NPC_GuideFish_A9/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.EDLA_SHANHAI_CHIP // removal by force
        },
        {
            "A9_S2/Room/NPC_GuideFish_A9/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M249_A9_S2_A9地圖魚索取記憶體_Chat03_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.EDLA_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A9_S2/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_BULLETIN_BOARD
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/LootProvider 大錢袋/0_DropPickable 大錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_WALKING_EAST_BUILDING
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小寶箱/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDLA_CHEST_ABOVE_NODE
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中寶箱 中量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDLA_CHEST_MIDDLE_ELEVATOR
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/LootProvider 乘客令牌：鄒巖/0_DropPickable 乘客令牌：鄒巖 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_DROP_MUTANT_MIDDLE_ELEVATOR
        },
        {
            "A9_S2/Room/伏羲嚇人 FSMGeneral FSM Object --開/FSM Animator/LogicRoot/Sleeppod  FSM (1)/[CutScene]BackFromSleeppod/--[States]/FSM/[State] 女媧來電/[Action] ItemGetUIShowAction",
            Location.EDLA_VITAL_SANCTUM
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中寶箱 中量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDLA_CHEST_FIVE_BELLS_UPPER_RIGHT
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/LootProvider 乘客令牌：阿守/0_DropPickable 乘客令牌：阿守 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_DROP_MUTANT_FIVE_BELLS
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/LootProvider 中錢袋 & 中量金/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_THEATER_RIGHT
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/LootProvider 鱉蠍/3_DropPickable 鱉蠍 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_THEATER_LEFT
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/LootProvider 文物-猴桃木/0_DropPickable 文物-猴桃木 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_EAST_BUILDING_ROOF
        },
        {
            "A9_S2/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_DGRD
        },
        { // for the skill point increase
            "A9_S2/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.EDLA_FLOWER
        },
        { // for the Tao Fruit inventory item
            "A9_S2/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action]ItemPick",
            Location.EDLA_FLOWER
        },
        {
            "A9_SG1/Room/Prefab/LootProvider 大錢袋/0_DropPickable 大錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_BACKER_1
        },
        {
            "A9_SG1/Room/Prefab/LootProvider 大錢袋 /0_DropPickable 大錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_BACKER_2
        },
        {
            "A9_SG1/Room/Prefab/LootProvider 大錢袋 大量金/0_DropPickable 大錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDLA_CHEST_BACKER_3
        },

        {
            "A9_S3/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_MONITORING_PANEL
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/LootProvider 算力元件/0_DropPickable 算力元件 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_CHEST_EAST_ROOF_WALKING
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/BR_TreasureDing_S 小寶箱/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.EDS_CHEST_EAST_ROOF_RIGHT
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/LootProvider 中錢袋 & 中量金/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_CHEST_EAST_ROOF_ABOVE
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/LootProvider 乘客令牌：夕溥/0_DropPickable 乘客令牌：夕溥 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_DROP_MUTANT_BELOW_HALL
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/LootProvider 乘客令牌：央凡/0_DropPickable 乘客令牌：央凡 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_DROP_MUTANT_BELOW_NODE
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/0_DropPickable Bag FSM_通行證/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_ITEM_BOTTOM_LEFT
        },
        {
            "A9_S3/Room/Prefab/Treasure寶箱 Chests/3_DropPickable 乘客令牌 4 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.EDS_ITEM_ABOVE_BOTTOM_LEFT
        },

        {
            "P2_R22_Savepoint_GameLevel/Room/Sleeppod  FSM (1)/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.NH_VITAL_SANCTUM
        },
        {
            "P2_R22_Savepoint_GameLevel/Room/Prefab/LootProvider/0_DropPickable Bag FSM_風氏血清/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.NH_CHEST_AFTER_FENGS
        },
        { // for the skill point increase
            "P2_R22_Savepoint_GameLevel/Room/Prefab/EventBinder (Boss Fight 相關)/General Boss Fight FSM Object_風氏兄妹/FSM Animator/AfterFightEndNode/風氏死亡肉肉花/DIESunFlower_FSM_風氏兄妹 LootProvider 雙生道果/--[States]/FSM/[State] Flower_Picking/[Action] PlayerIncreaseSkillPointAction",
            Location.NH_NUWA_FLOWER
        },
        { // for the Twin Tao Fruit inventory item
            "P2_R22_Savepoint_GameLevel/Room/Prefab/EventBinder (Boss Fight 相關)/General Boss Fight FSM Object_風氏兄妹/FSM Animator/AfterFightEndNode/風氏死亡肉肉花/DIESunFlower_FSM_風氏兄妹 LootProvider 雙生道果/--[States]/FSM/[State] Flower_Picking/[Action] 拿到道果",
            Location.NH_NUWA_FLOWER
        },

        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/4_DropPickable A11地圖魚 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DROP_SHANHAI
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小量金 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.TRC_CHEST_SPIKES
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/LootProvider 天道卷軸/0_DropPickable 天道卷軸 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DROP_BOOKSHELF
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M 中量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.TRC_CHEST_CHIEN_ARENA
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M 中寶箱 中量金 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.TRC_CHEST_ABOVE_SOL_STATUE
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/3_DropPickable 乘客令牌：愛姆 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_ITEM_SICKBAY_VENTS
        },
        {
            "A11_S1/Room/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DGRD
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/AlienChainDropItem 玄鐵 FSM/FSM Animator/LogicRoot/LootProvider/4_DropPickable 玄鐵 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DROP_MUTANT_HIGHEST
        },
        {
            "A11_S1/Room/Phase相關切換Gameplay----------------/General FSM Object_On And Off Switch 最終階段切換 Variant/FSM Animator/LogicRoot/Phase3_Off/Phase3_Off/LootProvider 乘客令牌：史陽月/0_DropPickable 乘客令牌：史陽月 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DROP_MUTANT_XINGTIAN
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/LootProvider 中錢袋&大量金/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_CHEST_MUTANT_BARRIER
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/AlienChainDropItem 中錢袋 FSM/FSM Animator/LogicRoot/LootProvider/4_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DROP_MUTANT_SOL_STATUE
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/AlienChainDropItem FSM 中錢袋 /FSM Animator/LogicRoot/LootProvider/4_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_CHEST_DG_HQ_MUTANT_NEAR_SCREEN
        },
        {
            "A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小量金###0/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.TRC_CHEST_DG_HQ_CHEST_NEAR_SCREEN
        },
        {
            "A11_S1/Room/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.TRC_DG_HQ_SCREEN
        },
    };
    /* unused gopaths:
     * 
     * [Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/RitualFlowerItem/LootProvider/After/LogicRoot/FSM Animator/SimpleCutSceneFSM_EncounterBoar (開頭介紹野豬的演出)/CullingGroup/[On]Node/LogicRoot/FSM Animator/軒軒野豬情境OnOff FSM/Room/A0_S4 gameLevel
     * for the Crimson Hibiscus dropped by the boar in the intro sequence
     *
     * AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/Butterfly_CutSceneFSM/--[States]/FSM/[State] GetButterfly/[Action]GetButterfly 玄蝶 狀態列
     * for the mystic nymph next to Yi's vital sanctum, won't be reachable once we make the FSP into a proper starting hub area
     * 
     * AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/NPC_AICore_Base/NPC_AICore_Base_FSM/FSM Animator/LogicRoot/NPC_AICore_FSM/General FSM Object/--[States]/FSM/[State]  初次對話演出/[Action] 取得玉石系統
     * for the Jade System received from Abacus/Ruyi
     * 
     * AG_LeeEar_S1/Room/NPC_Lear Variant (1)/General FSM Object/--[States]/FSM/[State] 立繪結束後演出/[Action] 取得笛子
     * for Fusang Horn from 1st Lear chat
     * 
     * AG_LeeEar_S1/Room/NPC_Lear Variant (1)/General FSM Object/--[States]/FSM/[State] 立繪結束後演出/[Action] 取得神遊功能
     * for Teleport from 2nd Lear chat
     * 
     * AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/--[States]/FSM/[State] 製作追蹤箭/[Action] 取得追蹤箭
     * for the Shadow Hunter that Kuafu makes from Homing Darts
     * 
     * a whole bunch for character database entries I won't bother recording
     * 
     * [Info   :ArchipelagoRandomizer] CutScenePlayAction_OnStateEnterImplement called on A10_SG5/Room/Prefab/Hologram_ReportMachine FSM/--[States]/FSM/[State] ReadingNote1/[Action] 閱讀動畫
     * [Info   :CutsceneSkip] SimpleCutsceneManager_PlayAnimation A10_SG5/Room/Prefab/Hologram_ReportMachine FSM/FSM Animator/LogicRoot/[CutScene] 無為宣言
     * for watching the Inaction Declaration
     * 
     * TRC_CHEST_DG_HQ_LOWER_1 - 5:
     * A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M 中量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner
     * A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M 中量金 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner
     * A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M 中寶箱 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner
     * A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小量金 (2)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner
     * A11_S1/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S 小量金###9/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner
     * 
     * A11_S1/Room/山海9000結局/Rebind/A11Pod_Z衝出天禍怪FSM_Prototype_Chaser 小倩Variant/FSM Animator/LogicRoot/LootProvider_小倩照片/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem
     * for Friend Photo
     * 
     * AG_ST_Hub/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem
     * for Root Core Monitoring Device
     * 
     * AG_S2/Room/NPCs/SimpleCutSceneFSM_結尾/--[States]/FSM/[State] 大爆炸結局演出PlayCutScene/[Action] 拿到元能箭
     * for Rhizomatic Arrow (the GFD is "(重要道具)15_元能箭 (ItemData)")
     * 
     * AG_S2/Room/NPCs/議會演出相關Binding/NPC_ShinNon_Base/NPC_ShinNon_Base_FSM/FSM Animator/LogicRoot/NPC_ShinNong_BaseVer/General FSM Object/--[States]/FSM/[State] 製作毒蛇藥酒/[Action] 取得毒蛇藥酒
     * for Shennong giving Yellow Dragonsnake Medicinal Brew
     */

    // Receiving items from cutscenes, including:
    // - removing map chips from Shanhai 9000s by force
    // - some FSP locations involving talking to NPCs
    [HarmonyPrefix, HarmonyPatch(typeof(ItemGetUIShowAction), "Implement")]
    static bool ItemGetUIShowAction_Implement(ItemGetUIShowAction __instance) {
        Log.Info($"ItemGetUIShowAction_Implement called on {__instance.item.Title}");

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ItemGetUIShowAction_Implement called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        return true; // not a randomized location, let vanilla impl handle this
    }

    // So far, I've only seen this class come up in the FSP mutant gene / SMB acquisition scene.
    [HarmonyPatch(typeof(CutsceneGetItem), nameof(CutsceneGetItem.GetItem))]
    [HarmonyPrefix]
    private static bool CutsceneGetItem_GetItem(CutsceneGetItem __instance) {
        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"CutsceneGetItem_GetItem called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        return true; // not a randomized location, let vanilla impl handle this
    }

    // Buying items from NPCs, including:
    // - peacefully buying map chips from Shanhai 9000s
    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseTradeAction), "OnStateEnterImplement")]
    static bool MerchandiseTradeAction_OnStateEnterImplement(MerchandiseTradeAction __instance) {
        Log.Info($"MerchandiseTradeAction_OnStateEnterImplement called on {__instance.merchandiseData.item.Title}");

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"MerchandiseTradeAction_OnStateEnterImplement called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        return true; // not a randomized location, let vanilla impl handle this
    }

    // Picking up chest items, enemy item drops, examining database entries
    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        // Apparently pickItemData can be null, and is null on the PickItemAction for PRE_CHEST_UNDER_BOX. No idea why that's allowed.
        //Log.Info($"PickItemAction_OnStateEnterImplement called on {__instance?.GetInstanceID()} containing: {__instance?.pickItemData?.name}\n{__instance?.pickItemData?.Title}\n{__instance?.pickItemData?.Summary}\n{__instance?.pickItemData?.Description}");
        if (__instance.scheme != PickableScheme.GetItem) {
            //Log.Info($"PickItemAction_OnStateEnterImplement: this is not a GetItem action, letting vanilla code handle it");
            return true;
        }

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"PickItemAction_OnStateEnterImplement called on GO with an associated AP location: {goPath}");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        //Log.Info($"PickItemAction_OnStateEnterImplement ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }

    // Absorbing tianhuo flowers/tao fruits
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerIncreaseSkillPointAction), "OnStateEnterImplement")]
    static bool PlayerIncreaseSkillPointAction_OnStateEnterImplement(PlayerIncreaseSkillPointAction __instance) {
        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        //Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement called on {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement called on GO with an associated AP location: {goPath}");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        //Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }

    // This gets called for anything that drops items, including killing enemies, but we only use it
    // for "money-only" chests since those locations don't involve an "item" the above patch would get.
    // We patch CheckGenerateItems instead of GenerateItems because CGI only gets invoked at actual drop time, while
    // GI also gets invoked preemptively by EnterLevelReset every time the chest/enemy/etc gets loaded into a scene.
    [HarmonyPostfix, HarmonyPatch(typeof(LootSpawner), "CheckGenerateItems")]
    static void LootSpawner_CheckGenerateItems(LootSpawner __instance) {
        // Most chests and scenery objects drop a tiny amount of jin just for hitting them, separate from their content.
        // This "hit loot" almost always comes from a GO named "HitLootSpawner", so ignoring this name makes the logs a lot nicer.
        if (__instance.name == "HitLootSpawner")
            return;

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        //Log.Info($"LootSpawner_CheckGenerateItems called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"LootSpawner_CheckGenerateItems called on GO with an associated AP location: {goPath}");

            var dropItemPrefabs = AccessTools.FieldRefAccess<LootSpawner, List<DropItem>>("dropItemPrefabs").Invoke(__instance);
            if (dropItemPrefabs.Count == 0) {
                Log.Info($"ignoring LootSpawner_CheckGenerateItems call because dropItemPrefabs is empty (this usually means a duplicate call on an already checked location)");
                return;
            }

            List<string> dropDescs = new();
            int totalDropJin = 0;
            foreach (var dropItem in dropItemPrefabs) {
                if (dropItem.TryGetComponent<DropMoney>(out var dm)) {
                    dropDescs.Add($"{dm.moneyValue} jin");
                    totalDropJin += dm.moneyValue;
                } else
                    dropDescs.Add($"{dropItem.name} (not money)");
            }

            Log.Info($"LootSpawner_CheckGenerateItems would've generated {totalDropJin} jin for GO: {goPath}\nClearing dropItemPrefabs which were: {string.Join(", ", dropDescs)}.");
            dropItemPrefabs.Clear();

            CheckLocation(goPathToLocation[goPath]);
        }

        //Log.Info($"LootSpawner_CheckGenerateItems ContainsKey() false");
    }

    // violently and fatally extracting map chips from Shanhai 9000s
    [HarmonyPrefix, HarmonyPatch(typeof(GuideFishLogic), "ConfirmKillFish")]
    static bool GuideFishLogic_ConfirmKillFish(GuideFishLogic __instance) {
        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"GuideFishLogic_ConfirmKillFish called on {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            //Log.Info($"GuideFishLogic_ConfirmKillFish ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        //Log.Info($"GuideFishLogic_ConfirmKillFish ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }
}
