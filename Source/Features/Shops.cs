using HarmonyLib;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Shops {
    // 0 = vanilla, 1 = medium, 2 = ledge_storage
    private static long LogicDifficulty = 1;

    public static void ApplySlotData(long? logicDifficulty) {
        LogicDifficulty = logicDifficulty ?? 0;
    }

    // we only want to force the player to spend Dark Steel on CPS if they're at risk of "missing" a logically required CPS barrier skip
    private static bool ShouldBlock(MerchandiseData __instance) {
        // vanilla logic doesn't expect bow tricks
        if (LogicDifficulty == 0)
            return false;

        // if this shop slot doesn't require a Dark Steel, then it's not relevant
        var entries = __instance.requireMaterialEntriesToBuy;
        if (entries.Count != 1)
            return false;
        var entry = entries[0];
        if (entry?.item.Title != "Dark Steel")
            return false;

        // CPS is only logically relevant as a substitute for Charged Strike, so if the player already has CS then we don't care about blocking shop items any more
        if (Player.i.mainAbilities.ChargedAttackAbility.IsAcquired)
            return false;

        // don't block CPS itself from being purchased, since the whole point is to prevent the player from "missing" CPS
        if (__instance.name == "Merchandise_2_1_貫穿箭LV2")
            return false;

        // if CPS already has been purchased, then we don't care about blocking shop items any more
        var cloudPiercerS = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["2f7009a00edd57c4fa4332ffcd15396aPlayerWeaponData"];
        if (cloudPiercerS.IsAcquired)
            return false;

        // if the player has multiple unspent Dark Steels, then it's safe to let them buy a non-CPS upgrade before CPS
        var darkSteelInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[2].rawCollection[13];
        var darkSteelCount = ((ItemData)darkSteelInventoryItem).ownNum.CurrentValue;
        if (darkSteelCount > 1)
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
                "Because you're playing on medium or higher logic difficulty, " +
                "you don't have Charged Strike yet, " +
                "and you only have one Dark Steel available right now, " +
                "<color=orange>you must spend your last Dark Steel on Cloud Piercer S</color>. " +
                "\n\n" +
                __result;
        }
    }
}
