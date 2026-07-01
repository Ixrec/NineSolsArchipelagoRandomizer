using HarmonyLib;

namespace ArchipelagoRandomizer.Features;

[HarmonyPatch]
internal class ExpMultiplier {
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerGamePlayData), nameof(PlayerGamePlayData.GainExp))]
    private static void PlayerGamePlayData_GainExp(PlayerGamePlayData __instance, ref int exp) {
        var multiplier = APRandomizer.Instance.ExperienceMultiplierSetting.Value;
        //Log.Warning($"multiplying {exp} experience by {multiplier}");
        exp *= multiplier;
    }

    // For lack of a better place to put this, we also edit the max level here
    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        if (SkillTree.RandomizeSkillTree) {
            //Log.Warning($"setting max level to 99");
            PlayerGamePlayData.Instance.MaxLevel.Stat.BaseValue = 99;
        }
    }
}
