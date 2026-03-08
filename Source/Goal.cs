using HarmonyLib;
using NineSolsAPI;
using RCGFSM.Variable;
using System;
using System.Threading.Tasks;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Goal {
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static void SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        var id = __instance.targetFlag?.boolFlag?.FinalSaveID;
        if (id == null) {
            return; // not every SetVariableBoolAction is associated with a flag in the save file
        }

        var eigongKilled = false;
        if (id == "85d361b9-7c43-4a1d-91c2-fd19e4bbb0b1_6b037c39982d34953a066792ab66783aScriptableDataBool") {
            // Normal ending / 2-phase Eigong kill ??? 
            eigongKilled = true;
        } else if (id == "e965aab1c014b4273b928b17fbcff379ScriptableDataBool") {
            // True ending / 3-phase Eigong kill ???
            eigongKilled = true;
        }

        if (eigongKilled) {
            if (PlayerGamePlayData.Instance.memoryMode.CurrentValue) {
                Log.Info($"Goal code ignoring Eigong kill because this is Battle Memories mode");
                return;
            }

            // apparently this happens ~3 times on a normal Eigong death, some for both vars,
            // so we don't know what the individual vars really mean yet
            Log.Info($"Eigong flag: id={id}, TargetValue={__instance.TargetValue}");

            InGameConsole.Add($"Eigong defeat detected by SetVariableBoolAction_OnStateEnterImplement. Congratulations!", forceImmediateToast: true);

            InGameConsole.Add($"Telling the AP server that you've achieved your goal.", forceImmediateToast: true);

            // we want to time out relatively quickly if the server happens to be down, but don't
            // block whatever we (and the vanilla game) were doing on waiting for the AP server response
            var _ = Task.Run(() => {
                try {
                    var checkLocationTask = Task.Run(() => ConnectionAndPopups.APSession!.SetGoalAchieved());
                    if (!checkLocationTask.Wait(TimeSpan.FromSeconds(2))) {
                        var msg = $"AP server timed out when we tried to tell it that you've achieved your goal. Did the connection go down?";
                        Log.Warning(msg);
                        InGameConsole.Add($"<color=orange>{msg}</color>");
                    }
                } catch (Exception ex) {
                    Log.Error($"Caught error in SetGoalAchieved's timeout callback: '{ex.Message}'\n{ex.StackTrace}");
                }
            });
        }
    }
}
