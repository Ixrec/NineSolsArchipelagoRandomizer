using HarmonyLib;

namespace ArchipelagoRandomizer;

/*
 * The AFE (Apeman Facility (Elevator)) area is not designed to be completed backwards, so it turns out the two small elevators (not the big one to Central Hall)
 * become impassable barriers when you enter AFE from below. The elevator call buttons at the bottom of each shaft are not active until you do the hacks at the
 * top of each shaft, which is of course impossible if you start at the bottom.
 * 
 * So in this file we want to detect when the player has "entered AFE from below" and unlock these two elevators.
 */

[HarmonyPatch]
internal class AFEElevators {
    [HarmonyPrefix, HarmonyPatch(typeof(GameCore), "ChangeScene", [typeof(SceneConnectionPoint.ChangeSceneData), typeof(bool), typeof(bool), typeof(float)])]
    static void GameCore_ChangeScene(GameCore __instance, SceneConnectionPoint.ChangeSceneData changeSceneData) {
        if (changeSceneData.sceneName != "A1_S2_ConnectionToElevator_Final") // the AFE scene
            return;

        if (changeSceneData.changeSceneMode == SceneConnectionPoint.ChangeSceneMode.Walk &&
            changeSceneData.connectionID == "A1_S1_To_A1_S2") {
            // this is the AFM<->AFE connection the vanilla game expects you to enter AFE from; we can let them hack the elevators normally
            return;
        } else {
            Log.Info($"Detected the player entering AFE from an entrance besides the AFM<->AFE one. Unlocking the two small elevators to prevent softlock.");

            var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;
            var afeUpperElevatorUnlockedFlag = (ScriptableDataBool)flagDict["b80c71f6-5b58-42a0-96ba-780a82ce59e5_51c211e21fecd9e4c92f41d8d72aa395ScriptableDataBool"];
            afeUpperElevatorUnlockedFlag.CurrentValue = true;
            var afeLowerElevatorUnlockedFlag = (ScriptableDataBool)flagDict["5075bb10-3b30-420b-b1ce-3a86a5890e8b_51c211e21fecd9e4c92f41d8d72aa395ScriptableDataBool"];
            afeLowerElevatorUnlockedFlag.CurrentValue = true;
        }
    }
}
