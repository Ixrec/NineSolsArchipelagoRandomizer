using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LoadingScreenTips {
    public static string apRainbow = "<color=#c97682>ARC</color><color=#75c275>HIP</color><color=#ca94c2>ELA</color><color=#d9a07d>GO R</color><color=#767ebd>AND</color><color=#eee391>OMI</color><color=#c97682>ZER</color>";

    // Keep this list in sync with the README. When copying to the README: replace the end-of-line ,s with blank lines, and replace the \ns with <br>s.
    private static List<string> randomizerTips = [
        "Press F1 to access settings for all your Nine Sols mods, including this randomizer.",
        "Reaching Eigong requires only Sol Seal items. There's no need to visit Tianhuo Research Institute.",
        "This randomizer depends on the TeleportFromAnywhere mod because an important item may end up randomly placed in a \"dead end\" you can only escape by teleporting.",
        "Shennong will become sick only after you acquire your first poison item.",
        "The randomizer's \"logic\" assumes:\n- Jiequan requires Charged Strike\n- Lady Ethereal requires Air Dash\n- Ji requires Tai-Chi Kick\n- Eigong requires Air Dash or Cloud Leap",
        "There are 5 mutants who drop an item when permanently killed with Super Mutant Buster.\n2 in ED (Living Area), 2 in ED (Sanctum), and 1 in TRC.",
        "The Peach Blossom Village rescue can be done as soon as you find the Abandoned Mines Access Token. It's no longer tied to escaping Prison and being rescued by Chiyou.",
        "Since talking to Ji at Daybreak Tower is a location, this randomizer makes Ji one of the few NPCs who can talk to you after his own death.",
        "Fighting Jiequan for real at the top of the Factory Zone can be done either before or after the Prison sequence. This makes Jiequan the only NPC who can kill you after he dies.",
        "All \"Limitless Realm\" segments are disabled and skipped in this randomizer.",
        "If Apeman Facility (Monitoring) was not your first root node, then that node will be automatically unlocked when you enter AF(M), because the upper part of AF(M) is unreachable without it.",
        "The large spike ball in Grotto (East) will never land in Grotto (Entry) in this randomizer, since it would block critical paths if we let it.",
        "This randomizer doesn't touch the items that are only reachable after the \"Point of no Return\", or after giving Shennong all poisons. You're free to replay that content or ignore it.",
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
        foreach (var _ in randomizerTips)
            __result.Add(firstTip);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(LoadingScreenPanel), "UpdateView")]
    private static void LoadingScreenPanel_UpdateView(LoadingScreenPanel __instance) {
        var titleText = GameObject.Find("ApplicationCore(Clone) (RCGLifeCycle)/4 UIGroupManager/ApplicationUICam/[Canvas]LoadingScreenPanel/LoadingScreenPanel/TipPanel/PanelMask/DialoguePanel/Background/Outline/TitleText")
            .GetComponent<TextMeshProUGUI>();
        if (titleText.text == "TIPS") {
            titleText.text = apRainbow;
            titleText.enableWordWrapping = false; // for some reason Unity will randomly decide to word wrap this, so we have to force wrapping off to get a consistent display
        }

        var causedByUserInput = (__instance.nextActionData.Action.WasPressed || __instance.previousActionData.Action.WasPressed);
        if (!causedByUserInput) {
            // This means UpdateView() was called by ShowLoading() after selecting which tip to display.
            // The selection is usually done with Random.Range(0, 3), so later tips are hardly ever seen. We can do better.
            // The easiest way to make tips properly random is to re-roll here, because this is right after ShowLoading()'s Random.Range() call.
            FieldRefAccess<LoadingScreenPanel, int>("currentIndex").Invoke(__instance) = Random.Range(0, randomizerTips.Count);
        }

        var i = FieldRefAccess<LoadingScreenPanel, int>("currentIndex").Invoke(__instance);

        var step = FieldRefAccess<LoadingScreenPanel, TMP_Text>("step").Invoke(__instance);
        step.text = $"{i + 1}/{randomizerTips.Count}"; // mostly copied from vanilla code

        var tipText = FieldRefAccess<LoadingScreenPanel, TMP_Text>("tipText").Invoke(__instance);
        tipText.text = randomizerTips[i];
    }
}
