using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class JadeCosts {
    private static Dictionary<string, string?> JadeEnglishTitleToSaveFlag = new Dictionary<string, string?> {
        { "Immovable Jade", "b8fd8e42229824b788bc222b837382f2JadeData" },
        { "Harness Force Jade", "a0a2cb6d037ee4d80a74fd447a21682eJadeData" },
        { "Focus Jade", "36eb7e7b95e91467191b8f24dbbb5a3eJadeData" },
        { "Swift Descent Jade", "1e635338961c24feb93798c36c07f128JadeData" },
        { "Medical Jade", "8417398823dca444b924aa9e49e82385JadeData" },
        { "Quick Dose Jade", "316728bf6fa814c8085a4ce094c6cabbJadeData" },
        { "Steely Jade", "28837290da6d24917ad6c99213d99d3dJadeData" },
        { "Stasis Jade", "1e983ace0eb874a3a883c5f1f50e2926JadeData" },
        { "Mob Quell Jade - Yin", "45a17198c6bff4c42989f3e2d9cb583bJadeData" },
        { "Mob Quell Jade - Yang", "8ff52186b5d2849f6930bd5bf5d86b8aJadeData" },
        { "Bearing Jade", "fce2186e0ae684bde9548905d5ed5533JadeData" },
        { "Divine Hand Jade", "ef792d1867d1a4a9c8ec6cd721ee5cb3JadeData" },
        { "Iron Skin Jade", "ff5f58b8404514c11b7ec4166b294349JadeData" },
        { "Pauper Jade", "562375e7a68ec42b28f3bdd5f45d7b72JadeData" },
        { "Swift Blade Jade", "b4c7da472cfba425ba5d0b0309dc4f17JadeData" },
        { "Last Stand Jade", "3411e0d523aec41f9be4e24ff81b6293JadeData" },
        { "Recovery Jade", "e6f162e19282346db96145ee80b5ccc1JadeData" },
        { "Breather Jade", "3ddbef7a7a579497b82fe3712177c089JadeData" },
        { "Hedgehog Jade", "3c8fd0425b80a405a8fb9623094fcafcJadeData" },
        { "Ricochet Jade", "dbda764ac569f4d6b871fb6c82f11adeJadeData" },
        { "Revival Jade", "987349e8a21844d28a86853bb0e5de09JadeData" },
        { "Soul Reaper Jade", "468a3373787c2443794e57f101b5f794JadeData" },
        { "Health Thief Jade", "1796f5882076b4c7c859bc4b0747d8bbJadeData" },
        { "Qi Blade Jade", "dfa6bbf26dfef4032a5287a7d9b27881JadeData" },
        { "Qi Swipe Jade", "111a1eb49b6d0476488eba696f991e19JadeData" },
        { "Reciprocation Jade", "589b90f2463944b95aeb6821385b3be6JadeData" },
        { "Cultivation Jade", "cfcd9f0d330344e628e7d8742955c172JadeData" },
        { "Avarice Jade", "88263fdff21bc8b4da3977c47ab02f03JadeData" },
        // the two unused jades, which unfortunately I have been putting in slot data, so mod code needs to know to ignore them
        { "Qi Thief Jade", null },
        { "Killing Blow Jade", null },
    };

    // Filled from the base game before we apply any custom costs
    private static Dictionary<string, int> JadeSaveFlagToVanillaCost = new();

    // Filled from slot_data on connect, if this slot randomized jade costs, and then reset to new() whenever we go back to the start menu.
    // We rely on this being empty if and only if the jade costs should be vanilla.
    public static Dictionary<string, long> JadeSaveFlagToSlotDataCost = new();

    // test values
    /*new Dictionary<string, long> {
        { "Steely Jade", 7 },
        { "Divine Hand Jade", 0 },
        { "Focus Jade", 1 },
        { "Qi Swipe Jade", 99 },
    };*/

    public static void ApplySlotData(object? jadeCosts) {
        JadeSaveFlagToSlotDataCost = new();

        if (jadeCosts is string && (string)jadeCosts == "vanilla") {
            return;
        }
        if (jadeCosts is not JObject jadeCostsObject) {
            Log.Error($"JadeCosts::ApplySlotData aborting because jadeCosts was neither 'vanilla' nor a JObject");
            return;
        }

        foreach (var (englishTitle, cost) in jadeCostsObject) {
            if (JadeEnglishTitleToSaveFlag.ContainsKey(englishTitle)) {
                var saveFlag = JadeEnglishTitleToSaveFlag[englishTitle];
                if (saveFlag == null) {
                    // This is one of the unused jades, so we simply do nothing
                } else {
                    // The in-game JadeData object uses ints, and Archipelago.MultiClient.Net slot data only offers longs, so integer casting is unavoidable here.
                    JadeSaveFlagToSlotDataCost[saveFlag] = (long)(cost ?? 0);
                }
            } else {
                Log.Error($"Unrecognized jade in slot data: {englishTitle}");
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        try {
            var useVanillaCosts = (JadeSaveFlagToSlotDataCost.Count == 0);
            if (PlayerGamePlayData.Instance.memoryMode.CurrentValue) {
                useVanillaCosts = true;
            }

            var jadeCollection = Player.i.mainAbilities.jadeDataColleciton; // [sic]
            List<JadeData> jades = jadeCollection.gameFlagDataList;

            // lazy init the vanilla cost map
            if (JadeSaveFlagToVanillaCost.Count == 0) {
                foreach (var jade in jades)
                    JadeSaveFlagToVanillaCost[jade.FinalSaveID] = jade.Cost;
            }

            int costsChanged = 0;
            foreach (var jade in jades) {
                var saveFlag = jade.FinalSaveID;
                if (!JadeSaveFlagToVanillaCost.ContainsKey(saveFlag)) {
                    Log.Error($"jade cost application failed for {jade.Title} / {saveFlag}, somehow it was missing from JadeSaveFlagToVanillaCost");
                    continue;
                }
                if (!JadeSaveFlagToSlotDataCost.ContainsKey(saveFlag)) {
                    Log.Error($"jade cost application failed for {jade.Title} / {saveFlag}, somehow it was missing from JadeSaveFlagToSlotDataCost");
                    continue;
                }

                var cost = (useVanillaCosts ? JadeSaveFlagToVanillaCost[saveFlag] : (int)JadeSaveFlagToSlotDataCost[saveFlag]);
                if (jade.Cost != cost) {
                    jade.Cost = cost;
                    AccessTools.FieldRefAccess<JadeData, List<StatModifierEntry>>("EquipEffectModifierEntries").Invoke(jade)[0].value = cost;
                    costsChanged++;
                }
            }

            var totalCostBefore = jadeCollection.PlayerCurrentJadePowerStat.Value;
            AccessTools.Method(typeof(JadeDataCollection), "InitCalculateCurrentJadePowerUsage", []).Invoke(jadeCollection, []);
            var totalCostAfter = jadeCollection.PlayerCurrentJadePowerStat.Value;

            if (costsChanged == 0) {
                if (useVanillaCosts) {
                    Log.Info($"JadeCosts::GameLevel_Awake did nothing because all jades were already set to their vanilla costs");
                } else {
                    Log.Info($"JadeCosts::GameLevel_Awake did nothing because all jades were already set to this slot's custom jade costs");
                }
            } else if (useVanillaCosts) {
                Log.Info($"JadeCosts::GameLevel_Awake reset {costsChanged} jades to their vanilla costs; cost in use changed from {totalCostBefore} to {totalCostAfter}");
            } else {
                Log.Info($"JadeCosts::GameLevel_Awake applied {costsChanged} custom jade costs; cost in use changed from {totalCostBefore} to {totalCostAfter}");
            }
        } catch (Exception ex) {
            Log.Error($"JadeCosts::GameLevel_Awake threw: {ex.Message}\nwith stack:\n{ex.StackTrace}\nand InnerException: {ex.InnerException?.Message}\nwith stack:\n{ex.InnerException?.StackTrace}");
        }
    }
}
