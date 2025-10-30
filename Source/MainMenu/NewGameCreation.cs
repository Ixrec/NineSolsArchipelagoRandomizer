using HarmonyLib;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class NewGameCreation {
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
        var fspTeleportPointPath = TeleportPoints.teleportPointToGameFlagPath[TeleportPoints.TeleportPoint.FourSeasonsPavilion];
        var teleportPointData = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(fspTeleportPointPath);
        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl calling StartGameGoTo");
        await SingletonBehaviour<ApplicationCore>.Instance.StartGameGoTo(teleportPointData);
        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl called StartGameGoTo");

        // and edit whatever state flags the randomizer needs to be different from vanilla.

        // if the player walks left after teleporting to AFM, we don't want them to get softlocked by the tutorial
        var afmCombatTutorialFinishedFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["ec69828570eb146668c414415a3b739bScriptableDataBool"];
        afmCombatTutorialFinishedFlag.CurrentValue = true;
        // turn the power on so we have immediate access to the root node without grapple + nymph
        var interactedWithNymphPickupFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["9ed84b86-9844-4950-b963-e2df6d0d8adc_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool"];
        interactedWithNymphPickupFlag.CurrentValue = true;
        var nymphPickupExitDoorOpenedFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["f7365cb3-7f12-4f69-9592-c9bead9df09f_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool"];
        nymphPickupExitDoorOpenedFlag.CurrentValue = true;
        var fspPowerFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["c0e1d094c1c8da0449d7cc3ff0fe6061ScriptableDataBool"];
        fspPowerFlag.CurrentValue = true;
        // skip most of the cutscenes that normally happen on your first FSP visit
        var firstRuyiTalkFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["ee3aad8a-54ea-462e-999c-e59034d31552_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool"];
        firstRuyiTalkFlag.CurrentValue = true;
        Player.i.mainAbilities.JadeSystem.AbilityData.PlayerPicked(); // this is when Ruyi repairs your Jade System
        var shuanshuanFoundFSPFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["116aba82301a72f4496302c9d7b32602ScriptableDataBool"];
        shuanshuanFoundFSPFlag.CurrentValue = true;
        var FSPDayNightSystemFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["a7047d63b0d9f466fa1240d488abe3b8ScriptableDataBool"];
        FSPDayNightSystemFlag.CurrentValue = true; // this flag appears to also prevent Ruyi's door from perma-closing on Yi,
            // and it *might* (needs more testing) be required for some NPC sidequests to advance "over time"
        // pretend the "Yi waking up after post-prison Chiyou rescue" scene has already played, because that's a required condition for FSP_SHENNONG_PBV_QUEST
        var wakeupAfterChiyouRescue_cutscenePlayedFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["e0b7244f28229054d9ef63438841ad72ScriptableDataBool"];
        wakeupAfterChiyouRescue_cutscenePlayedFlag.CurrentValue = true;
        // give player the teleporting horn immediately
        var hasFusangHornFlag = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["f6ddb914baaea4c11a4b995145dbbaadItemData"];
        hasFusangHornFlag.PlayerPicked();
        var hasTeleportFlag = (PlayerAbilityData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["950f8f3273611424d9b42ab209e8cac8PlayerAbilityData"];
        hasTeleportFlag.PlayerPicked();

        FSPEntrance.NewGameFlagEdits();

        /* unused flags:
         * - mystic nymph pickup in far right of FSP:
         * e432044f5b9632d42a4455a41be4ac20ScriptableDataBool appears to control tutorial messages for nymph usage
         * bdd72488-8572-41c3-b6f5-5a145a3e1a7c_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool is the "is hacked" flag for the hack point in the little vent that opens the door
         */

        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl edited flags");

        SingletonBehaviour<GameCore>.Instance.gameLevel.OnLevelStartPlaying.AddListener(FSPLevelStartPlayingHandler);
    }

    // All of this is for:
    // - allowing us to set the first_root_node after the base game has unlocked AFM so we can relock it (this may no longer be necessary now that we've found the root cause)
    // - making the FSP save point do just its opening animation the first time we load the FSP level without breaking anything
    private static bool automatedFirstFSPSavePointOpening = false;
    static void FSPLevelStartPlayingHandler() {
        TeleportPoints.SetFirstRootNodeAfterNewGameCreation();

        Log.Info($"FSPLevelStartPlayingHandler now trying to open save point");
        var spgo = GameObject.Find("AG_S2/Room/議會古樹管理 FSM Variant/FSM Animator/LogicRoot/SavePoint_AG_S2");

        // This "isActived" flag is how the save point knows to do a smaller "SP_PlayerEnter" animation next time,
        // without visually reverting to its unopened state.
        var isActived = typeof(SavePoint).GetField("isActived", BindingFlags.Instance | BindingFlags.NonPublic);
        isActived.SetValue(spgo.GetComponent<SavePoint>(), true);

        automatedFirstFSPSavePointOpening = true;
        var spa = spgo.transform.Find("LotusSP_AG_S2").GetComponent<Animator>();
        spa.Play("SP_opening");

        SingletonBehaviour<GameCore>.Instance.gameLevel.OnLevelStartPlaying.RemoveListener(FSPLevelStartPlayingHandler);
    }
    [HarmonyPrefix, HarmonyPatch(typeof(AnimationEvents), "InvokeAnimationEvent")]
    static bool AnimationEvents_InvokeAnimationEvent(AnimationEvents __instance, AnimationEvents.AnimationEvent e) {
        if (!automatedFirstFSPSavePointOpening)
            return true;

        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/FSM Animator/LogicRoot/SavePoint_AG_S2/LotusSP_AG_S2") {
            Log.Info($"AnimationEvents_InvokeAnimationEvent skipping normal events for FSP node activation and doing playerexit anim instead");
            automatedFirstFSPSavePointOpening = false;
            var spa = __instance.gameObject.GetComponent<Animator>();
            spa.Play("SP_PlayerExit");
            return false;
        }
        return true;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(InteractInteraction), "InteractedImplementation")]
    static void InteractInteraction_InteractedImplementation(InteractInteraction __instance) {
        if (!automatedFirstFSPSavePointOpening)
            return;

        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        if (goPath == "AG_S2/Room/議會古樹管理 FSM Variant/FSM Animator/LogicRoot/SavePoint_AG_S2/Interactable_Interact/Interact Interaction") {
            Log.Info($"InteractInteraction_InteractedImplementation cancelling the upcoming animation event skip because the player actually interacted with the node");
            automatedFirstFSPSavePointOpening = false;
        }
    }
}
