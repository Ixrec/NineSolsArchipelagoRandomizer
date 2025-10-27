using HarmonyLib;
using NineSolsAPI;

namespace ArchipelagoRandomizer;

/*
 * The AFM (Apeman Facility (Monitoring)) area is not designed to be completed backwards. It turns out one of the falls you make on the way down is impossible
 * to jump back up, even with Cloud Leap and all the other abilities. It is possible with Cloud Leap *and* a ledge storage slash vault/getup.
 * 
 * So in this file we want to detect when the player has "entered AFM from below" on non-LS logic, and unlock the root node.
 */

[HarmonyPatch]
internal class AFMUnlock {
    [HarmonyPrefix, HarmonyPatch(typeof(GameCore), "ChangeScene", [typeof(SceneConnectionPoint.ChangeSceneData), typeof(bool), typeof(bool), typeof(float)])]
    static void GameCore_ChangeScene(GameCore __instance, SceneConnectionPoint.ChangeSceneData changeSceneData) {
        if (changeSceneData.sceneName != "A1_S1_HumanDisposal_Final") // the AFM scene
            return;

        string logicLevel = "vanilla";
        if (ConnectionAndPopups.SlotData != null && ConnectionAndPopups.SlotData.ContainsKey("logic_level")) {
            logicLevel = (string)ConnectionAndPopups.SlotData["logic_level"];
        }
        // TODO: check if this is still correct once trick logic is real
        if (logicLevel == "ledge_storage") {
            return; // upper AFM is reachable on this logic, so no need for us to unlock the node
        }

        if (changeSceneData.changeSceneMode == SceneConnectionPoint.ChangeSceneMode.Walk &&
            changeSceneData.connectionID == "A1_S1_To_A1_S2") { // AFM<->AFE connection

            var path = TeleportPoints.teleportPointToGameFlagPath[TeleportPoints.TeleportPoint.ApemanFacilityMonitoring];
            var afmUnlocked = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath(path).unlocked;
            if (afmUnlocked.CurrentValue == false) {
                ToastManager.Toast($"Now that you've found AF (Monitoring), <color=orange>the AFM teleport point has been unlocked</color> so you can reach the upper half.");
                afmUnlocked.SetCurrentValue(true);
            }
        }
    }
}
