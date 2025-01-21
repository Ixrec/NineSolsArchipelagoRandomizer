using HarmonyLib;

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
    }
}
