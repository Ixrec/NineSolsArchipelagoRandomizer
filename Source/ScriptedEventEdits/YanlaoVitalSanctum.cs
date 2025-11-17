using HarmonyLib;
using RCGFSM.Variable;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

// Many objects have a save data flag to record that Yi has already interacted with the object, so if
// the player ever comes back the object knows to be in its disabled/empty/post-interaction state.

// For some reason, the vital sancta for Ji, Fuxi and Nuwa set not just their own flags, but also the
// flag for Yanlao's VS. This appears to be a simple copy-paste error from RCG that has no impact in
// vanilla because it's supposed to be impossible to reach any of those VSs before Yanlao's.
// Hence, this normally only matters in speedruns that exploit this to skip Yanlao entirely.

// In randomizer, this can lead to the Yanlao's Vital Sanctum location being impossible to check
// because his VS simply turned off when you did one of the other VSs. So we have to prevent that.

[HarmonyPatch]
class YanlaoVitalSanctum {
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static bool SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        if (__instance.targetFlag?.boolFlag?.FinalSaveID == "4ebde92ca5a98fe4baf012267ad6b45bScriptableDataBool") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            var isYanlaoSanctum = (goPath == "A0_S6/Room/Prefab/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] Get_BossKey = true");
            if (isYanlaoSanctum) {
                return true;
            } else {
                Log.Info($"Blocking an attempt to disable Yanlao's Vital Sanctum, because it's coming from someplace other than Yanlao's Vital Sanctum: goPath={goPath}");
                return false;
            }
        }
        return true;
    }

}
