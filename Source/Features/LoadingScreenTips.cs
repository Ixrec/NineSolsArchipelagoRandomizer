using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LoadingScreenTips {
    private static List<string> randomizerTips = [
        "Press F1 to access settings for all your Nine Sols mods, including this randomizer.",
        "Chiyou moves into Four Seasons Pavilion after you raise the factory bridge and talk to him.\nKuafu moves in after you use his vital sanctum, just like vanilla.",
        "Reaching Eigong requires only Sol Seal items. There's no need to visit Tianhuo Research Institute.",
        "Shennong will become sick only after you acquire your first poison item.",
        "The randomizer's \"logic\" assumes:\n- Jiequan requires Charged Strike\n- Lady Ethereal requires Air Dash\n- Ji requires Tai-Chi Kick\n- Eigong requires Air Dash or Cloud Leap",
    ];

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingScreenTipDataCollection), "FetchAcquiredTips", MethodType.Getter)]
    static void LoadingScreenTipDataCollection_get_FetchAcquiredTips(LoadingScreenTipDataCollection __instance, List<LoadingScreenTipData> __result) {
        if (__result == null) {
            Log.Error($"LoadingScreenTipDataCollection_get_FetchAcquiredTips aborting because this tip collection is null");
            return;
        }
        if (__result.Count <= 0) {
            Log.Error($"LoadingScreenTipDataCollection_get_FetchAcquiredTips aborting because this tip collection is empty");
            return;
        }

        var firstTip = __result[0];
        __result.Clear();
        foreach(var _ in randomizerTips)
            __result.Add(firstTip);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingScreenPanel), "UpdateView")]
    private static void LoadingScreenPanel_UpdateView(LoadingScreenPanel __instance) {
        var titleText = GameObject.Find("ApplicationCore(Clone) (RCGLifeCycle)/4 UIGroupManager/ApplicationUICam/[Canvas]LoadingScreenPanel/LoadingScreenPanel/TipPanel/PanelMask/DialoguePanel/Background/Outline/TitleText")
            .GetComponent<TextMeshProUGUI>();
        if (titleText.text == "TIPS") {
            titleText.text = "<color=#c97682>ARC</color><color=#75c275>HIP</color><color=#ca94c2>ELA</color><color=#d9a07d>GO R</color><color=#767ebd>AND</color><color=#eee391>OMI</color><color=#c97682>ZER</color>";
            titleText.enableWordWrapping = false; // for some reason Unity will randomly decide to word wrap this, so we have to force wrapping off to get a consistent display
        }

        var i = FieldRefAccess<LoadingScreenPanel, int>("currentIndex").Invoke(__instance);

        // let the vanilla code handle step.text

        var tipText = FieldRefAccess<LoadingScreenPanel, TMP_Text>("tipText").Invoke(__instance);
        tipText.text = randomizerTips[i];
    }
}
