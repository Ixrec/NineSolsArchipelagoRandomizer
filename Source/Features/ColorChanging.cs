using HarmonyLib;
using UnityEngine;

namespace ArchipelagoRandomizer.Features;

[HarmonyPatch]
internal class ColorChanging {
    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        var YiSpriteHolder = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite");
        if (YiSpriteHolder.GetComponent<_2dxFX_ColorChange>() == null) {
            // avoid making any edits if the setting is never enabled
            if (!APRandomizer.Instance.RandomizeYiColorSetting.Value)
                return;

            YiSpriteHolder.AddComponent<_2dxFX_ColorChange>();
        }
        var YiHueComponent = YiSpriteHolder.GetComponent<_2dxFX_ColorChange>();

        if (APRandomizer.Instance.RandomizeYiColorSetting.Value) {
            var prng = new System.Random();
            var newHue = prng.Next(360);
            Log.Info($"Changing Yi's robe's hue to {newHue}");
            YiHueComponent?._HueShift = newHue;
        } else {
            Log.Warning($"GameLevel_Awake D2");
            YiHueComponent?._HueShift = 0;
        }
    }
}
