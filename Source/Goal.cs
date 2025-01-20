using HarmonyLib;
using NineSolsAPI;
using RCGFSM.Variable;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Goal {
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static void SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        var id = __instance.targetFlag.boolFlag.FinalSaveID;

        var eigongKilled = false;
        if (id == "85d361b9-7c43-4a1d-91c2-fd19e4bbb0b1_6b037c39982d34953a066792ab66783aScriptableDataBool") {
            // Normal ending / 2-phase Eigong kill ??? 
            eigongKilled = true;
        } else if (id == "e965aab1c014b4273b928b17fbcff379ScriptableDataBool") {
            // True ending / 3-phase Eigong kill ???
            eigongKilled = true;
        }

        // apparently this happens ~3 times on a normal Eigong death, some for both vars,
        // so we don't know what the individual vars really mean yet
        // TODO: avoid duplicate messages here for now
        if (eigongKilled) {
            Log.Info($"Eigong flag: id={id}, TargetValue={__instance.TargetValue}");

            Log.Info($"Eigong defeat detected by SetVariableBoolAction_OnStateEnterImplement. Congratulations!");
            ToastManager.Toast($"Eigong defeat detected by SetVariableBoolAction_OnStateEnterImplement. Congratulations!");
            // TODO: actually tell an AP server we goaled
            // TODO: alternate goals???
        }
    }
}
