using HarmonyLib;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

[HarmonyPatch]
class ChiyouAtFactoryBridge {
    // Make Chiyou stay at Factory (Great Hall) bridge to give you the bridge-lowering reward
    // even if you do Boundless Repository first.
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        if (__instance.name == "[State] 蚩尤不在") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A5_S1/Room/Prefab/BridgeLogic/NPC_ChiYou_A5狀態FSM Variant/--[States]/FSM/[State] 蚩尤不在") {
                var locationsChecked = APSaveManager.CurrentAPSaveData?.locationsChecked;
                var locName = Location.FGH_CHIYOU_BRIDGE.ToString();
                if (locationsChecked != null && locationsChecked.ContainsKey(locName) && locationsChecked[locName])
                    // if we've already checked the Chiyou Bridge location, no need to keep forcing Chiyou to show up here
                    return;

                Log.Info($"ChiyouAtFactoryBridge::GeneralState_OnStateEnter forcing Chiyou to exist at the Factory (Great Hall) bridge despite the base game hiding him");
                var chiyouExistsState = __instance.transform.parent.Find("[State] 蚩尤在");
                chiyouExistsState?.GetComponent<GeneralState>()?.OnStateEnter();
            }
        }
    }
}
