using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LoadingScreenTips {
    private static List<string> randomizerTips = [
        "Chiyou moves into Four Seasons Pavilion after you raise the factory bridge and talk to him.\nKuafu moves in after you use his vital sanctum, just like vanilla.",
        "Fighting Eigong in the randomizer requires only Sol Seal items. There's no need to visit Tiandao Research Center.",
    ];
    // TODO: a Shennong tip after I figure out how I want him to work

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingScreenTipDataCollection), "FetchAcquiredTips", MethodType.Getter)]
    static void LoadingScreenTipDataCollection_get_FetchAcquiredTips(LoadingScreenTipDataCollection __instance, List<LoadingScreenTipData> __result) {
        if (__result == null || __result.Count <= 0 || __result[0].name != "AG_TipData_Bow") {
            Log.Error($"LoadingScreenTipDataCollection_get_FetchAcquiredTips aborting because these are not the expected tip results");
            return;
        }

        var bowTip = __result[0];
        __result.Clear();

        foreach(var _ in randomizerTips)
            __result.Add(bowTip);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingScreenPanel), "UpdateView")]
    private static void LoadingScreenPanel_UpdateView(LoadingScreenPanel __instance) {
        var titleText = GameObject.Find("ApplicationCore(Clone) (RCGLifeCycle)/4 UIGroupManager/ApplicationUICam/[Canvas]LoadingScreenPanel/LoadingScreenPanel/TipPanel/PanelMask/DialoguePanel/Background/Outline/TitleText")
            .GetComponent<TextMeshProUGUI>();
        if (titleText.text == "TIPS") {
            titleText.text = "<color=#c97682>ARC</color><color=#75c275>HIP</color><color=#ca94c2>ELA</color><color=#d9a07d>GO R</color><color=#767ebd>AND</color><color=#eee391>OMI</color><color=#c97682>ZER</color>";
        }

        var i = FieldRefAccess<LoadingScreenPanel, int>("currentIndex").Invoke(__instance);

        // let the vanilla code handle step.text

        var tipText = FieldRefAccess<LoadingScreenPanel, TMP_Text>("tipText").Invoke(__instance);
        tipText.text = randomizerTips[i];
    }
}
