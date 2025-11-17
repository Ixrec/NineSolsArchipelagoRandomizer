using HarmonyLib;
using System;
using System.Linq;

namespace ArchipelagoRandomizer.ScriptedEventEdits;

// In vanilla, the 3 statues will light up and Cixing will spawn if you have all 3 secret mural database entries,
// or if you've seen the little coffin hologram cutscenes in all 3 tombs.
// We want these to be triggered *only* by visiting the locations, not by having the items.

[HarmonyPatch]
class LearGraveStatuesAndMiniboss {
    private static string[] secretMuralEncyclopediaItemNames = [
        "Pedia_35_A10_SG2_經濟天尊",
        "Pedia_36_A10_SG4_軍事天尊",
        "Pedia_34_A10_SG1_科技天尊"
    ];

    // As far as I know, nothing else in the game checks for possession of the secret mural "items",
    // so we can simply tell HasItemCondition to always produce isValid: false on secret murals.
    [HarmonyPostfix, HarmonyPatch(typeof(HasItemCondition), "isValid", MethodType.Getter)]
    static void HasItemCondition_get_isValid(HasItemCondition __instance, ref bool __result) {
        //Log.Info($"HasItemCondition_get_isValid {__instance.item.name} / {__instance.item.Title} / __result={__result}");
        if (secretMuralEncyclopediaItemNames.Contains(__instance.item.name) && __result == true) {
            Log.Info($"forcing HasItemCondition to fail for {__instance.item.Title} so that the Lear grave statues and Cixing spawn will be activated by the Secret Mural *locations*, not the items");
            __result = false;
        }
    }
}
