using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class InGameConsole {
    public static List<string> consoleMessages = new();

    public static void Add(string message) {
        consoleMessages.Add(message);
        ToastManager.Toast(message);
    }

    public static bool ShowInGameConsole = false;

    public static GUIStyle? backlogStyle = null;

    public static void OnGUI() {
        ConnectionAndPopups.UpdateStyles();
        if (backlogStyle == null) {
            backlogStyle = new GUIStyle(GUI.skin.scrollView);

            // apparently this is what it takes to change a color in IMGUI
            var bgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            bgColorTex.SetPixel(0, 0, new Color(0, 0, 0, 1f));
            bgColorTex.Apply();
            backlogStyle.normal.background = bgColorTex;
            backlogStyle.onActive.background = bgColorTex;
            backlogStyle.onFocused.background = bgColorTex;
            backlogStyle.onHover.background = bgColorTex;
            backlogStyle.onNormal.background = bgColorTex;
        }

        if (!SingletonBehaviour<UIManager>.IsAvailable())
            return;

        var pausePanelUI = SingletonBehaviour<UIManager>.Instance.PausePanelUI;
        var mainPauseMenuGO = pausePanelUI.gameObject.transform.Find("Pause Menu").gameObject;
        var pauseMenuState = pausePanelUI.state;
        if (
            mainPauseMenuGO.activeSelf &&
            (pauseMenuState == UIPanelState.Show || pauseMenuState == UIPanelState.IsShowing)
        ) {
            DrawInGameConsole();
        }
    }

    private static string ConsoleInput = "";
    private static Vector2 scrollPos;

    private static void DrawInGameConsole() {
        float windowWidth = Screen.width * 0.5f;
        float windowHeight = Screen.height * 0.63f;
        var windowRect = new Rect((Screen.width - windowWidth) * 0.95f, (Screen.height - windowHeight) * 0.6f, windowWidth, windowHeight);

        var textFieldWidth = GUILayout.Width(windowRect.width * 0.6f);

        var windowStyle = ConnectionAndPopups.windowStyle;
        var labelStyle = ConnectionAndPopups.labelStyle;
        var textFieldStyle = ConnectionAndPopups.textFieldStyle;
        var buttonStyle = ConnectionAndPopups.buttonStyle;

        GUI.Window(11261729, windowRect, (int windowID) => {

            GUILayout.BeginVertical(GUILayout.Height(windowRect.height * 0.87f));
            scrollPos = GUILayout.BeginScrollView(scrollPos, backlogStyle);
            foreach (var m in consoleMessages)
                GUILayout.Label(m);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUI.GetNameOfFocusedControl() == "APCommandEntry") {
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
                    Log.Info("Enter pressed in entry");
            }
            GUI.SetNextControlName("APCommandEntry");
            ConsoleInput = GUILayout.TextField(ConsoleInput, textFieldStyle, GUILayout.Width(windowRect.width * 0.97f));
            GUILayout.EndHorizontal();

        }, "Archipelago Console", windowStyle);
    }
}
