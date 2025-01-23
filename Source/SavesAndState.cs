using HarmonyLib;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class SavesAndState {
    // since "async bool" isn't a thing, we need two patches to replace the vanilla code with our own async code
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "NewGameChangeScene")]
    static bool StartMenuLogic_NewGameChangeScene_SkipVanillaImpl(StartMenuLogic __instance, int slotIndex) {
        return false;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "NewGameChangeScene")]
    static async void StartMenuLogic_NewGameChangeScene_APImpl(StartMenuLogic __instance, int slotIndex) {
        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl: slotIndex={slotIndex}");

        // copy-pasted-ish from vanilla impl
        await SingletonBehaviour<SaveManager>.Instance.LoadSaveAtSlot(slotIndex);
        var _newGameMode = AccessTools.FieldRefAccess<StartMenuLogic, int>("_newGameMode").Invoke(__instance);
        if (_newGameMode == 1) {
            __instance.gameModeFlag.CurrentValue = 1;
            __instance.storyModeAbilityData.SetToActivated();
            __instance.storyModeAbilityData.ActivateCheck();
        }
        __instance.data.memoryMode.CurrentValue = false;

        // Now, instead of loading "A0_S6_Intro_Video", we load directly into FSP.

        // This magic string came from evaluating SingletonBehaviour<SaveManager>.Instance.currentPlayerData.lastTeleportPointPath in the UE console.
        var fspTeleportPointPath = "9115d3446fcc24abab2c0030d55abd1eTeleportPointData";
        var teleportPointData = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(fspTeleportPointPath);
        await SingletonBehaviour<ApplicationCore>.Instance.StartGameGoTo(teleportPointData);

        // and edit whatever state flags the randomizer needs to be different from vanilla.

        // turn the power on so we have immediate access to the root node without grapple + nymph
        var fspPowerFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["c0e1d094c1c8da0449d7cc3ff0fe6061ScriptableDataBool"];
        fspPowerFlag.CurrentValue = true;
        // skip most of the cutscenes that normally happen on your first FSP visit
        var firstRuyiTalkFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["ee3aad8a-54ea-462e-999c-e59034d31552_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool"];
        firstRuyiTalkFlag.CurrentValue = true;
        Player.i.mainAbilities.JadeSystem.AbilityData.PlayerPicked(); // this is when Ruyi repairs your Jade System
        var shuanshuanFoundFSPFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["116aba82301a72f4496302c9d7b32602ScriptableDataBool"];
        shuanshuanFoundFSPFlag.CurrentValue = true;
        // give player the teleporting horn immediately
        var hasFusangHornFlag = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["f6ddb914baaea4c11a4b995145dbbaadItemData"];
        hasFusangHornFlag.PlayerPicked();
        var hasTeleportFlag = (PlayerAbilityData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["950f8f3273611424d9b42ab209e8cac8PlayerAbilityData"];
        hasTeleportFlag.PlayerPicked();
    }

    // This is the best approach I've found so far for editing FSMs: intercept transitions into a state and then do a manual transition afterward.
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        // despite the "On" name, this state represents Ji *not* being at the Daybreak Tower
        if (__instance.name == "[State] On") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] On") {
                Log.Info($"GeneralState_OnStateEnter forcing Ji to exist at Daybreak Tower despite the base game hiding him");
                var jiDoesExistAtDaybreakTowerState = GameObject.Find("A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[States]/FSM/[State] Off");
                jiDoesExistAtDaybreakTowerState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }

        if (__instance.name == "[State] 第一次爆走") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 第一次爆走") {
                Log.Info($"GeneralState_OnStateEnter forcing FSP root node to its default state despite the base game wanting to do the 1st Lear talk");
                // TODO: why does this make the node look dead until our first time interacting with it???
                var fspRootNodeNormalState = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 普通存檔點");
                fspRootNodeNormalState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
        if (__instance.name == "[State] 第二次爆走") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 第二次爆走") {
                Log.Info($"GeneralState_OnStateEnter forcing FSP root node to its default state despite the base game wanting to do the 2nd Lear talk");
                var fspRootNodeNormalState = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 普通存檔點");
                fspRootNodeNormalState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
        if (__instance.name == "[State] 第三次爆走") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 第三次爆走") {
                Log.Info($"GeneralState_OnStateEnter forcing FSP root node to its default state despite the base game wanting to do the 3rd Lear talk");
                var fspRootNodeNormalState = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/--[States]/FSM/[State] 普通存檔點");
                fspRootNodeNormalState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
    }
}
