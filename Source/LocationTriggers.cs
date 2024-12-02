using HarmonyLib;
using NineSolsAPI;
using RCGFSM.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RCGFSM.Items.ItemGetUIShowAction;
using static RCGFSM.Items.PickItemAction;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LocationTriggers {
    public static string GetFullPath(GameObject go) {
        var transform = go.transform;
        List<string> pathParts = new List<string>();
        while (transform != null) {
            pathParts.Add(transform.name);
            transform = transform.parent;
        }
        return string.Join("/", pathParts);
    }

    private static void CheckLocation(Location location) {
        // TODO: talk to AP of course
        ToastManager.Toast($"CheckLocation() called with Archipelago location: {location}");
    }

    private static Dictionary<string, Location> goPathToLocation = new Dictionary<string, Location> {
        {
            "[Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/1_DropPickable 遺書 FSM/Prefab/Room/A0_S4 gameLevel",
            Location.UC_NOTE
        },
        {
            "[Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/1_DropPickable 竹簡 FSM/Prefab/Room/A0_S4 gameLevel",
            Location.UC_SCROLL
        },
        {
            "[Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/A1_S1_DropPickable_Flower/LootProvider/QCmachine  染血的大紅槿/Treasure 寶箱 Chests/Prefab/Room/A1_S1_GameLevel",
            Location.AFM_BREAK_CORPSE
        },
        {
            "LootDropSpawner/MonsterCore/StealthGameMonster_Spearman/EngagingToken/GamePlayS3_2/Room/A1_S1_GameLevel",
            Location.AFM_CHEST_UPPER_RIGHT
        },
    };
    /* unused gopaths:
     * 
     * [Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/RitualFlowerItem/LootProvider/After/LogicRoot/FSM Animator/SimpleCutSceneFSM_EncounterBoar (開頭介紹野豬的演出)/CullingGroup/[On]Node/LogicRoot/FSM Animator/軒軒野豬情境OnOff FSM/Room/A0_S4 gameLevel
     * for the Crimson Hibiscus dropped by the boar in the intro sequence
     * 
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
        Log.Info($"PickItemAction_OnStateEnterImplement called on {__instance.GetInstanceID()} containing: {__instance.pickItemData.name}\n{__instance.pickItemData.Title}\n{__instance.pickItemData.Summary}\n{__instance.pickItemData.Description}");
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

    // nope, this is called before Yi breaks the chest, in anticipation of it
    [HarmonyPrefix, HarmonyPatch(typeof(LootSpawner), "GenerateItems")]
    static bool LootSpawner_GenerateItems(LootSpawner __instance) {
        Log.Info($"LootSpawner_GenerateItems called on {__instance.name}");

        var goPath = GetFullPath(__instance.gameObject);
        Log.Info($"LootSpawner_GenerateItems called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"LootSpawner_GenerateItems ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        Log.Info($"LootSpawner_GenerateItems ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }
}
