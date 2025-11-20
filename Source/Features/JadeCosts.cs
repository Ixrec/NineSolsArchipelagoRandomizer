using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class JadeCosts {
    // The in-game JadeData object uses ints, and Archipelago.MultiClient.Net slot data only offers longs, so integer casting is unavoidable here.

    // Filled from the base game before we apply any custom costs
    private static Dictionary<string, int> JadeTitleToVanillaCost = new();

    // Filled from slot_data on connect, if this slot randomized jade costs, and then reset to new() whenever we go back to the start menu.
    // We rely on this being empty if and only if the jade costs should be vanilla.
    public static Dictionary<string, long> JadeTitleToSlotDataCost = new();

    // test values
    /*new Dictionary<string, long> {
        { "Steely Jade", 7 },
        { "Divine Hand Jade", 0 },
        { "Focus Jade", 1 },
        { "Qi Swipe Jade", 99 },
    };*/

    public static void ApplySlotData(object jadeCosts) {
        JadeTitleToSlotDataCost = new();

        if (jadeCosts is string && (string)jadeCosts == "vanilla") {
            return;
        }
        if (jadeCosts is not JObject jadeCostsObject) {
            Log.Error($"JadeCosts::ApplySlotData aborting because jadeCosts was neither 'vanilla' nor a JObject");
            return;
        }

        foreach (var (jade, cost) in jadeCostsObject)
            JadeTitleToSlotDataCost[jade] = (long)(cost ?? 0);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        var useVanillaCosts = (JadeTitleToSlotDataCost.Count == 0);
        if (PlayerGamePlayData.Instance.memoryMode.CurrentValue) {
            useVanillaCosts = true;
        }

        List<JadeData> jades = Player.i.mainAbilities.jadeDataColleciton.gameFlagDataList;

        // lazy init the vanilla cost map
        if (JadeTitleToVanillaCost.Count == 0) {
            foreach (var jade in jades)
                JadeTitleToVanillaCost[jade.Title] = jade.Cost;
        }

        int costsChanged = 0;
        foreach (var jade in jades) {
            var title = jade.Title;
            var cost = (useVanillaCosts ? JadeTitleToVanillaCost[title] : (int)JadeTitleToSlotDataCost[title]);
            if (jade.Cost != cost) {
                jade.Cost = cost;
                costsChanged++;
            }
        }

        if (costsChanged == 0) {
            if (useVanillaCosts) {
                Log.Info($"JadeCosts::GameLevel_Awake did nothing because all jades were already set to their vanilla costs");
            } else {
                Log.Info($"JadeCosts::GameLevel_Awake did nothing because all jades were already set to this slot's custom jade costs");
            }
        } else if (useVanillaCosts) {
            Log.Info($"JadeCosts::GameLevel_Awake reset {costsChanged} jades to their vanilla costs");
        } else {
            Log.Info($"JadeCosts::GameLevel_Awake applied {costsChanged} custom jade costs");
        }
    }
}
