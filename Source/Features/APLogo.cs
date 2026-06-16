using System;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer.Features; 

internal class APLogo {

    private static int size = 512;
    private static Sprite? apLogoSprite = null;
    private static Texture2D? apLogoTexture = null;

    private static void drawCircle(Texture2D tex, Vector2Int center, int radius, Color color) {
        for (var x = -radius; x <= radius; x++) {
            for (var y = -radius; y <= radius; y++) {
                var distanceFromCenter = Math.Sqrt((x * x) + (y * y));
                if (distanceFromCenter < radius) {
                    tex.SetPixel(center.x + x, center.y + y, color);
                }
            }
        }
    }

    public static Sprite getApLogoSprite(float alpha = 1.0f) {
        if (apLogoSprite != null)
            return apLogoSprite;

        Log.Info($"getApLogoSprite() drawing the AP logo");
        var center = new Vector2Int(size / 2, size / 2);
        var spinnerRadius = 100;
        var pointRadius = 60;
        var outlineRadius = 70;

        apLogoTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        apLogoTexture.name = "APRandomizer_Logo";
        foreach (var x in Enumerable.Range(0, size))
            foreach (var y in Enumerable.Range(0, size))
                apLogoTexture.SetPixel(x, y, Color.clear);

        // Used the eyedropper tool on https://github.com/ArchipelagoMW/Archipelago/blob/main/data/icon.png
        var apRed = new Color(201 / 256f, 118 / 256f, 130 / 256f, alpha);
        var apGreen = new Color(117 / 256f, 194 / 256f, 117 / 256f, alpha);
        var apPurple = new Color(202 / 256f, 148 / 256f, 194 / 256f, alpha);
        var apOrange = new Color(217 / 256f, 160 / 256f, 125 / 256f, alpha);
        var apBlue = new Color(118 / 256f, 126 / 256f, 189 / 256f, alpha);
        var apYellow = new Color(238 / 256f, 227 / 256f, 145 / 256f, alpha);

        var angleToIntOffsets = (int degrees) => new Vector2Int(
            (int)Math.Round(spinnerRadius * Math.Cos(Mathf.Deg2Rad * degrees)),
            (int)Math.Round(spinnerRadius * Math.Sin(Mathf.Deg2Rad * degrees))
        );
        drawCircle(apLogoTexture, center + angleToIntOffsets(90), outlineRadius, Color.black);
        drawCircle(apLogoTexture, center + angleToIntOffsets(90), pointRadius, apRed);
        drawCircle(apLogoTexture, center + angleToIntOffsets(30), outlineRadius, Color.black);
        drawCircle(apLogoTexture, center + angleToIntOffsets(30), pointRadius, apGreen);
        drawCircle(apLogoTexture, center + angleToIntOffsets(150), outlineRadius, Color.black);
        drawCircle(apLogoTexture, center + angleToIntOffsets(150), pointRadius, apYellow);
        drawCircle(apLogoTexture, center + angleToIntOffsets(-30), outlineRadius, Color.black);
        drawCircle(apLogoTexture, center + angleToIntOffsets(-30), pointRadius, apPurple);
        drawCircle(apLogoTexture, center + angleToIntOffsets(-150), outlineRadius, Color.black);
        drawCircle(apLogoTexture, center + angleToIntOffsets(-150), pointRadius, apBlue);
        drawCircle(apLogoTexture, center + angleToIntOffsets(-90), outlineRadius, Color.black);
        drawCircle(apLogoTexture, center + angleToIntOffsets(-90), pointRadius, apOrange);
        apLogoTexture.Apply();

        apLogoSprite = Sprite.Create(
            apLogoTexture,
            new Rect(0.0f, 0.0f, apLogoTexture.width, apLogoTexture.height),
            new Vector2(0.5f, 0.5f),
            100.0f
        );
        return apLogoSprite;
    }
}
