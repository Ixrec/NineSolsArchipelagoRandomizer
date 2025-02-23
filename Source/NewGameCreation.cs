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
        var fspTeleportPointPath = teleportPointToGameFlagPath[TeleportPoint.FourSeasonsPavilion];
        var teleportPointData = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(fspTeleportPointPath);
        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl calling StartGameGoTo");
        await SingletonBehaviour<ApplicationCore>.Instance.StartGameGoTo(teleportPointData);
        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl called StartGameGoTo");

        // and edit whatever state flags the randomizer needs to be different from vanilla.

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
        // also get Shennong into the FSP
        var shennongSavedFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["739a14dc66e1c674e820f75543e5a662ScriptableDataBool"];
        shennongSavedFlag.CurrentValue = true;
        // give player the teleporting horn immediately
        var hasFusangHornFlag = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["f6ddb914baaea4c11a4b995145dbbaadItemData"];
        hasFusangHornFlag.PlayerPicked();
        var hasTeleportFlag = (PlayerAbilityData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["950f8f3273611424d9b42ab209e8cac8PlayerAbilityData"];
        hasTeleportFlag.PlayerPicked();

        /* unused flags:
         * - mystic nymph pickup in far right of FSP:
         * e432044f5b9632d42a4455a41be4ac20ScriptableDataBool appears to control tutorial messages for nymph usage
         * bdd72488-8572-41c3-b6f5-5a145a3e1a7c_8aeaa90a7d08d4fc3a5c59630fe9716cScriptableDataBool is the "is hacked" flag for the hack point in the little vent that opens the door
         */

        Log.Info($"StartMenuLogic_NewGameChangeScene_APImpl edited flags");

        SingletonBehaviour<GameCore>.Instance.gameLevel.OnLevelStartPlaying.AddListener(FSPLevelStartPlayingHandler);
    }

    // This TP data lives here because: TODO: unlock a different first root node from the vanilla AFM depending on slot_data
    private enum TeleportPoint {
        FourSeasonsPavilion,
        ApemanFacilityMonitoring,
        ApemanFacilityElevatorUpper,
        ApemanFacilityElevatorLower,
        ApemanFacilityDepths,
        CentralTransportHub,
        GalacticDock,
        PowerReservoirEast,
        PowerReservoirCentral,
        RadiantPagoda,
        PowerReservoirWest,
        LakeYaochiRuins,
        Greenhouse,
        WaterAndOxygenSynthesis,
        AgrarianHall,
        YinglongCanal,
        CortexCenter,
        OuterWarehouse,
        InnerWarehouse,
        BoundlessRepository,
        FactoryGreatHall,
        FactoryPrison,
        FactoryMachineRoom,
        FactoryUnderground,
        FactoryProductionArea,
        ShengwuHall,
        AbandonedMines,
        GrottoOfScripturesEntry,
        GrottoOfScripturesWest,
        GrottoOfScripturesEast,
        SkyTower,
        EmpyreanDistrictPassages,
        EmpyreanDistrictLivingArea,
        EmpyreanDistrictSanctum,
        TiandaoResearchCenter,
        TiandaoResearchInstitute,
        NewKunlunControlHub,
    }
    private static Dictionary<TeleportPoint, string> teleportPointToGameFlagPath = new Dictionary<TeleportPoint, string> {
        { TeleportPoint.FourSeasonsPavilion, "9115d3446fcc24abab2c0030d55abd1eTeleportPointData" },
        { TeleportPoint.ApemanFacilityMonitoring, "b3cf5264bd5d54b06975638acac58b2aTeleportPointData" },
        { TeleportPoint.ApemanFacilityElevatorUpper, "113385c61a2544446925e7516fea6016TeleportPointData" },
        { TeleportPoint.ApemanFacilityElevatorLower, "2ee85cc763a3d413f9b6e18e7e1fee67TeleportPointData" },
        { TeleportPoint.ApemanFacilityDepths, "b9e8a5c6d9f6e4812bf9cc30810faadbTeleportPointData" },
        { TeleportPoint.CentralTransportHub, "e5246ef15ff2a41ae884071b014351f4TeleportPointData" },
        { TeleportPoint.GalacticDock, "3f4604f810c9e9a469f01d352d8035b9TeleportPointData" },
        { TeleportPoint.PowerReservoirEast, "610a3a3232701af47b876a660910fccaTeleportPointData" },
        { TeleportPoint.PowerReservoirCentral, "9bf82941e7038a44980ca867c751e5cbTeleportPointData" },
        { TeleportPoint.RadiantPagoda, "b3310817230814fa09f5fa3b12cb6293TeleportPointData" },
        { TeleportPoint.PowerReservoirWest, "a71b5769629d443ef9af5e3a05385c73TeleportPointData" },
        { TeleportPoint.LakeYaochiRuins, "07215bbc4dc4247aeb7a980bf4910bf9TeleportPointData" },
        { TeleportPoint.Greenhouse, "126a7caa6701e4f0a9059b603a309c3fTeleportPointData" },
        { TeleportPoint.WaterAndOxygenSynthesis, "b242c961367ad6a49889999af22a46d9TeleportPointData" },
        { TeleportPoint.AgrarianHall, "1025e9e6b5d9440979328bcc29d89468TeleportPointData" },
        { TeleportPoint.YinglongCanal, "685eb4dc303abbd43a67a07cf0459c64TeleportPointData" },
        { TeleportPoint.CortexCenter, "c971454704bfa480880660b06e4af2c7TeleportPointData" },
        { TeleportPoint.OuterWarehouse, "afbe61da78699487faccd6f0ae1d9667TeleportPointData" },
        { TeleportPoint.InnerWarehouse, "2c8de8a9dfacf4f65a0fa1e60a16f852TeleportPointData" },
        { TeleportPoint.BoundlessRepository, "242132789f3c9e94bbabb096bad651a5TeleportPointData" },
        { TeleportPoint.FactoryGreatHall, "c748d8c36b73c464fa38d9b156c0b0bcTeleportPointData" },
        //{ TeleportPoint.FactoryPrison, "28a1908d9e21d4136b8c903e2b92b0afTeleportPointData" }, // breaks weakened/prison state if enabled early
        { TeleportPoint.FactoryMachineRoom, "4970d157835c54adbb55bb4f8e245fdfTeleportPointData" },
        { TeleportPoint.FactoryUnderground, "f1e11be280022400caf9c8593ead7d43TeleportPointData" },
        { TeleportPoint.FactoryProductionArea, "7a581656cd08345b793d7ad2b14e9943TeleportPointData" },
        { TeleportPoint.ShengwuHall, "7b8d46b0c0c1845fe893ce18aa67bca3TeleportPointData" },
        { TeleportPoint.AbandonedMines, "3b7b8da692cb64d298142612c02daa4cTeleportPointData" },
        { TeleportPoint.GrottoOfScripturesEntry, "4bb93fbefaedb8e47949d1d384220c21TeleportPointData" },
        { TeleportPoint.GrottoOfScripturesWest, "fbaf57e3f6bea904ea8c150e5915bcf4TeleportPointData" },
        { TeleportPoint.GrottoOfScripturesEast, "1b81567d30abe194d845d3f08beae8fdTeleportPointData" },
        { TeleportPoint.SkyTower, "32bafafe1cacf5c49af2f7c9e45fe511TeleportPointData" },
        { TeleportPoint.EmpyreanDistrictPassages, "8e915ab1790ef69468d4d49d4f338db2TeleportPointData" },
        { TeleportPoint.EmpyreanDistrictLivingArea, "1a4e7bc2545139145ba229dac285581bTeleportPointData" },
        { TeleportPoint.EmpyreanDistrictSanctum, "ba5ec4c4a702ba84baec1326a803b2b6TeleportPointData" },
        { TeleportPoint.TiandaoResearchCenter, "5c30ad493bbdebc40a2370664d46b830TeleportPointData" },
        { TeleportPoint.TiandaoResearchInstitute, "473d9c581cd574f62a36519ae3d451ebTeleportPointData" },
        { TeleportPoint.NewKunlunControlHub, "da6613d2c8f7e4eb6ae1b4d0fe7fee93TeleportPointData" },
    };

    public static void UnlockAllTeleportPoints() {
        foreach (var (_, path) in teleportPointToGameFlagPath) {
            SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(path).unlocked.SetCurrentValue(true);
        }
    }

    // All of this is for making the FSP save point do just its opening animation the first time we load the FSP level without breaking anything
    private static bool automatedFirstFSPSavePointOpening = false;
    static void FSPLevelStartPlayingHandler() {
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
