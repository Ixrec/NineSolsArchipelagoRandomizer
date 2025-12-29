using HarmonyLib;
using NineSolsAPI;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class PreventWeakenedPrisonState {
    private static bool ShouldPrevent = false;

    public static void ApplySlotData(long? rawPreventValue) {
        if (rawPreventValue == null) {
            ShouldPrevent = false;
        } else {
            ShouldPrevent = (rawPreventValue == 1);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RCGFSM.PlayerAbility.PlayerAbilityModifyPackApplyAction), "OnStateEnterImplement")]
    static bool PlayerAbilityModifyPackApplyAction_OnStateEnterImplement(RCGFSM.PlayerAbility.PlayerAbilityModifyPackApplyAction __instance) {
        if (!ShouldPrevent)
            return true;

        if (__instance.Pack.name == "A5 Jail Debuff Pack 虛弱監獄") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A5_S2/[能力包] Player Ability Buff Debuff Override Pack FSM Variant/General FSM Object/--[States]/FSM/[State] Apply/[Action] PlayerAbilityModifyPackApplyAction") {
                Log.Info($"PreventWeakenedPrisonState patch preventing Prison's PlayerAbilityModifyPackApplyAction from running");
                ToastManager.Toast($"Prevented weakened Prison state from being applied to Yi, since this slot was generated with prevent_weakened_prison_state: true");
                return false;
            }
        }
        return true;
    }
}
