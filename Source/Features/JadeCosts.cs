using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class JadeCosts {
    // The in-game JadeData object uses ints, and Archipelago.MultiClient.Net slot data only offers longs, so integer casting is unavoidable here.

    private static Dictionary<string, int> JadeTitleToVanillaCost = new(); // filled during game loading by the patch method

    public static Dictionary<string, long> JadeTitleToSlotDataCost = new(); // filled from slot_data on connect, if this slot randomized jade costs
    // test values
    /*new Dictionary<string, long> {
        { "Steely Jade", 7 },
        { "Divine Hand Jade", 0 },
        { "Focus Jade", 1 },
        { "Qi Swipe Jade", 99 },
    };*/

    public static void ApplySlotData(object jadeCosts) {
        if (jadeCosts is not JObject jadeCostsObject) {
            Log.Debug($"JadeCosts::ApplySlotData aborting because jadeCosts was not a JObject");
            return;
        }

        JadeTitleToSlotDataCost = new();
        foreach (var (jade, cost) in jadeCostsObject)
            JadeTitleToSlotDataCost[jade] = (long)(cost ?? 0);
    }

    // None of the usual ways of detecting BM mode work because jades "wake up" before BM mode is fully initialized,
    // so it's more practical to reset jade costs every time the main menu is opened.
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "Start")]
    public static void StartMenuLogic_Start_Prefix(StartMenuLogic __instance) {
        if (JadeTitleToSlotDataCost.Count > 0) {
            Log.Info($"JadeCosts::StartMenuLogic_Start_Prefix clearing slot data jade cost map, so these costs can't 'bleed' into another slot or BM mode");
            JadeTitleToSlotDataCost = new();
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(JadeData), "FlagAwake")]
    private static void JadeData_FlagAwake(JadeData __instance) {
        var title = __instance.Title;
        if (title == "") {
            return; // there's a few placeholder? unfinished? JadeDatas like this we have to ignore
        }

        if (!JadeTitleToVanillaCost.ContainsKey(title)) {
            JadeTitleToVanillaCost[title] = __instance.Cost;
        }

        if (JadeTitleToSlotDataCost.ContainsKey(title)) {
            var newCost = JadeTitleToSlotDataCost[title];
            Log.Info($"JadeCosts::JadeData_FlagAwake changing {title}'s cost from {__instance.Cost} to {newCost}");
            __instance.Cost = (int)newCost;
        } else {
            // check if we need to restore vanilla cost, to prevent "jade cost bleed" between save files
            if (__instance.Cost != JadeTitleToVanillaCost[title]) {
                var vanillaCost = JadeTitleToVanillaCost[title];
                Log.Info($"JadeCosts::JadeData_FlagAwake resetting {title}'s cost from {__instance.Cost} to its vanilla cost of {vanillaCost}");
                __instance.Cost = vanillaCost;
            }
        }
    }
}
