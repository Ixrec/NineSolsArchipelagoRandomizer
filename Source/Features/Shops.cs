using HarmonyLib;
using NineSolsAPI;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Shops {
    // 0 = vanilla, 1 = medium, 2 = ledge_storage
    private static long LogicDifficulty = 1;

    public static void ApplySlotData(long? logicDifficulty) {
        LogicDifficulty = logicDifficulty ?? 0;
    }

    // we only want to force the player to spend their first Dark Steel on CPS if they're on medium or higher logic,
    // which means only block purchase of Dark Steel-requiring shop slots that aren't CPS itself, until CPS is acquired
    private static bool ShouldBlock(MerchandiseData __instance) {
        if (LogicDifficulty == 0)
            return false;

        var entries = __instance.requireMaterialEntriesToBuy;
        if (entries.Count != 1)
            return false;

        var entry = entries[0];
        if (entry?.item.Title != "Dark Steel")
            return false;

        if (__instance.name == "Merchandise_2_1_貫穿箭LV2")
            return false;

        var cloudPiercerS = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["2f7009a00edd57c4fa4332ffcd15396aPlayerWeaponData"];
        if (cloudPiercerS.IsAcquired)
            return false;

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), nameof(MerchandiseData.HasEnoughMaterial))]
    public static void MerchandiseData_HasEnoughMaterial(MerchandiseData __instance, ref bool __result) {
        if (ShouldBlock(__instance)) {
            Log.Info($"MerchandiseData_HasEnoughMaterial patch blocking purchase of '{__instance.Title}' until Cloud Piercer S has been purchased");
            __result = false;
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseData), "Description", MethodType.Getter)]
    static void MerchandiseData_get_Description(MerchandiseData __instance, ref string __result) {
        if (ShouldBlock(__instance)) {
            __result = $"{LoadingScreenTips.apRainbow}: " +
                "\n" +
                "<color=orange>You must purchase Cloud Piercer S first</color>, " +
                "because you're playing on medium or higher logic difficulty." +
                "\n\n" +
                __result;
        }
    }
}
