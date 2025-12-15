using HarmonyLib;
using NineSolsAPI;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class SkillTree {
    // 0 = vanilla, 1 = medium, 2 = ledge_storage
    private static long LogicDifficulty = 0;

    public static void ApplySlotData(long? logicDifficulty) {
        LogicDifficulty = logicDifficulty ?? 0;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        if (LogicDifficulty > 0) {
            var swiftRunnerSkillNode = (SkillNodeData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["ae3f7be7afb294d2eba0f6f4d129c6d0SkillNodeData"];
            if (!swiftRunnerSkillNode.IsAcquired) {
                Log.Info($"SkillTree::GameLevel_Awake auto-unlocking Swift Runner since LogicDifficulty is {LogicDifficulty}");
                swiftRunnerSkillNode.PlayerPicked();

                InGameConsole.Add($"<color=orange>The Swift Runner skill has been automatically unlocked</color>\nbecause this slot was generated with a logic_difficulty of medium or higher"); // and skill tree rando does not exist yet
            }
        }
    }
}
