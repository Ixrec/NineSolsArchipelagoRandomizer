using HarmonyLib;

namespace ExampleMod;

[HarmonyPatch]
public class Patches {

    // Patches are powerful. They can hook into other methods, prevent them from runnning,
    // change parameters and inject custom code.
    // Make sure to use them only when necessary and keep compatibility with other mods in mind.
    // Documentation on how to patch can be found in the harmony docs: https://harmony.pardeike.net/articles/patching.html
    [HarmonyPatch(typeof(Player), nameof(Player.SetStoryWalk))]
    [HarmonyPrefix]
    private static bool PatchStoryWalk(ref float walkModifier) {
        walkModifier = 1.0f;

        return true; // the original method should be executed
    }
}