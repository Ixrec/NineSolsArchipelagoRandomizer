using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class InGameConsole {
    public static List<string> consoleMessages = new();

    public static void Add(string message) {
        consoleMessages.Add(message);
        ToastManager.Toast(message);
    }

    public static GUIStyle? windowStyle = null;
    public static GUIStyle? labelStyle = null;
    public static GUIStyle? textFieldStyle = null;
    public static GUIStyle? buttonStyle = null;
    public static GUIStyle? backlogStyle = null;

    public static void UpdateStyles() {
        if (
            windowStyle == null ||
            labelStyle == null ||
            textFieldStyle == null ||
            buttonStyle == null ||
            backlogStyle == null
        ) {
            windowStyle = new GUIStyle(GUI.skin.window);

            // apparently this is what it takes to make a window *not* be transparent in IMGUI
            var grayBgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            grayBgColorTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            grayBgColorTex.Apply();
            windowStyle.normal.background = grayBgColorTex;
            windowStyle.onActive.background = grayBgColorTex;
            windowStyle.onFocused.background = grayBgColorTex;
            windowStyle.onHover.background = grayBgColorTex;
            windowStyle.onNormal.background = grayBgColorTex;

            labelStyle = new GUIStyle(GUI.skin.label);
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            buttonStyle = new GUIStyle(GUI.skin.button);

            backlogStyle = new GUIStyle(GUI.skin.scrollView);

            // apparently this is what it takes to change a color in IMGUI
            var blackBgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            blackBgColorTex.SetPixel(0, 0, new Color(0, 0, 0, 1f));
            blackBgColorTex.Apply();
            backlogStyle.normal.background = blackBgColorTex;
            backlogStyle.onActive.background = blackBgColorTex;
            backlogStyle.onFocused.background = blackBgColorTex;
            backlogStyle.onHover.background = blackBgColorTex;
            backlogStyle.onNormal.background = blackBgColorTex;
        }

        float scaleFactor = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
        int scaledFont = Mathf.RoundToInt(24 * scaleFactor);
        windowStyle.fontSize = scaledFont;
        labelStyle.fontSize = scaledFont;
        textFieldStyle.fontSize = scaledFont;
        buttonStyle.fontSize = scaledFont;
    }

    public static bool ShowInGameConsole = false;

    public static void OnGUI() {
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
    private static Vector2? scrollPos = null;

    private static void DrawInGameConsole() {
        UpdateStyles();

        float windowWidth = Screen.width * 0.5f;
        float windowHeight = Screen.height * 0.63f;
        var windowRect = new Rect((Screen.width - windowWidth) * 0.95f, (Screen.height - windowHeight) * 0.6f, windowWidth, windowHeight);
        float scrollPanelHeight = windowRect.height * 0.87f;

        if (scrollPos == null) {
            scrollPos = new Vector2(0, scrollPanelHeight);
        }

        GUI.Window(11261729, windowRect, (int windowID) => {

            GUILayout.BeginVertical(GUILayout.Height(scrollPanelHeight));
            scrollPos = GUILayout.BeginScrollView((Vector2)scrollPos, backlogStyle);
            foreach (var m in consoleMessages)
                GUILayout.Label(m);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUI.GetNameOfFocusedControl() == "APCommandEntry") {
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)) {
                    ExecuteAPCommand();
                }
            }
            GUI.SetNextControlName("APCommandEntry");
            ConsoleInput = GUILayout.TextField(ConsoleInput, textFieldStyle, GUILayout.Width(windowRect.width * 0.97f));
            GUILayout.EndHorizontal();

        }, "Archipelago Console", windowStyle);
    }

    private static void ExecuteAPCommand() {
        var text = ConsoleInput;

        if (text == "") return;

        if (ConnectionAndPopups.APSession == null) {
            Add($"<color=orange>Cannot send AP message '{text}' without a connection to the AP server</color>");
            return;
        }

        // we want to time out relatively quickly if the server happens to be down, but don't
        // block whatever we (and the vanilla game) were doing on waiting for the AP server response
        var _ = Task.Run(() => {
            var sayPacketTask = Task.Run(() => ConnectionAndPopups.APSession.Say(text));
            if (!sayPacketTask.Wait(TimeSpan.FromSeconds(2))) {
                Add($"<color=orange>AP server timed out when we tried to send the message '{text}'. Did the connection go down?</color>");
            }
            ConsoleInput = ""; // only clear the entry if we've executed the command successfully
        });
    }
}
