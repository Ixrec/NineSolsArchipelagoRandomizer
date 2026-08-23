using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.ScriptedEventEdits;
using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class InGameConsole {
    public static List<string> consoleMessages = new();

    private static readonly int backlogLimit = 1000;
    private static bool truncatingBacklog = false;

    public static void Add(string message, bool forceImmediateToast = false) {
        consoleMessages.Add(message);

        while (consoleMessages.Count > backlogLimit) {
            consoleMessages.RemoveAt(0);
            truncatingBacklog = true;
        }

        if (forceImmediateToast)
            ToastManager.Toast(message);
        else
            ScheduleToast(message);
    }

    private const int SHORT_DELAY_MS = 100;
    private const int LONG_DELAY_MS = 1000;
    private static int currentDelayTime = SHORT_DELAY_MS;
    private const int MAX_TOASTS_BEFORE_HIDING = 5;

    private static Task? displayToastsTask = null;
    public static ConcurrentStack<string> pendingToasts = new ConcurrentStack<string>();

    private static void ScheduleToast(string message) {
        pendingToasts.Push(message);

        if (displayToastsTask == null)
            StartNewToastTask();
    }

    private static void StartNewToastTask() {
        //Log.Warning($"toast scheduling - StartNewToastTask() called");
        displayToastsTask = Task
            .Delay(currentDelayTime)
            .ContinueWith(_ => DisplayToasts(), TaskScheduler.Default);
    }

    private static void DisplayToasts() {
        //Log.Warning($"toast scheduling - DisplayToasts() called with {pendingToasts.Count} pending");
        if (pendingToasts.Count > MAX_TOASTS_BEFORE_HIDING) {
            if (APRandomizer.Instance.BatchSimultaneousMessagesSetting.Value) {
                var toastCount = pendingToasts.Count;
                pendingToasts.Clear();
                ToastManager.Toast($"Received {toastCount} messages within {((currentDelayTime == SHORT_DELAY_MS) ? "100 ms" : "1 second")}. Pause to read them.");
            } else {
                while (pendingToasts.TryPop(out var message))
                    ToastManager.Toast(message);
            }

            currentDelayTime = LONG_DELAY_MS;
            StartNewToastTask();
        } else {
            while (pendingToasts.TryPop(out var message))
                ToastManager.Toast(message);

            currentDelayTime = SHORT_DELAY_MS;
            displayToastsTask = null;
        }
    }

    public static GUIStyle? grayWindowStyle = null;
    public static GUIStyle? clearWindowStyle = null;
    public static GUIStyle? labelStyle = null;
    public static GUIStyle? textFieldStyle = null;
    public static GUIStyle? buttonStyle = null;
    public static GUIStyle? backlogStyle = null;
    public static Texture2D? clearBgColorTex = null;
    public static Texture2D? grayBgColorTex = null;
    public static Texture2D? blackBgColorTex = null;

    public static void UpdateStyles() {
        if (
            grayWindowStyle == null ||
            clearWindowStyle == null ||
            labelStyle == null ||
            textFieldStyle == null ||
            buttonStyle == null ||
            backlogStyle == null
        ) {
            grayWindowStyle = new GUIStyle(GUI.skin.window);
            clearWindowStyle = new GUIStyle(GUI.skin.window);

            // apparently this is what it takes to set a window color in IMGUI
            clearBgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            clearBgColorTex.SetPixel(0, 0, Color.clear);
            clearBgColorTex.Apply();
            clearWindowStyle.normal.background = clearBgColorTex;
            clearWindowStyle.onActive.background = clearBgColorTex;
            clearWindowStyle.onFocused.background = clearBgColorTex;
            clearWindowStyle.onHover.background = clearBgColorTex;
            clearWindowStyle.onNormal.background = clearBgColorTex;

            grayBgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            grayBgColorTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            grayBgColorTex.Apply();
            grayWindowStyle.normal.background = grayBgColorTex;
            grayWindowStyle.onActive.background = grayBgColorTex;
            grayWindowStyle.onFocused.background = grayBgColorTex;
            grayWindowStyle.onHover.background = grayBgColorTex;
            grayWindowStyle.onNormal.background = grayBgColorTex;

            backlogStyle = new GUIStyle(GUI.skin.scrollView);

            blackBgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            blackBgColorTex.SetPixel(0, 0, new Color(0, 0, 0, 1f));
            blackBgColorTex.Apply();
            backlogStyle.normal.background = blackBgColorTex;
            backlogStyle.onActive.background = blackBgColorTex;
            backlogStyle.onFocused.background = blackBgColorTex;
            backlogStyle.onHover.background = blackBgColorTex;
            backlogStyle.onNormal.background = blackBgColorTex;

            labelStyle = new GUIStyle(GUI.skin.label);
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            buttonStyle = new GUIStyle(GUI.skin.button);
        }

        float scaleFactor = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
        int scaledFont = Mathf.RoundToInt(24 * scaleFactor);
        clearWindowStyle.fontSize = scaledFont;
        grayWindowStyle.fontSize = scaledFont;
        labelStyle.fontSize = scaledFont;
        textFieldStyle.fontSize = scaledFont;
        buttonStyle.fontSize = scaledFont;
    }

    public static bool drewConsoleInLastOnGUICall = false;

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
            DrawInGameConsole(!drewConsoleInLastOnGUICall);
            DrawSealCounts(!drewConsoleInLastOnGUICall);
            drewConsoleInLastOnGUICall = true;
        } else {
            drewConsoleInLastOnGUICall = false;
        }
    }

    private static string ConsoleInput = "";
    private static Vector2? scrollPos = null;

    private static void DrawInGameConsole(bool resetScrollPosition) {
        UpdateStyles();

        float windowWidth = Screen.width * 0.5f;
        float windowHeight = Screen.height * 0.63f;
        var windowRect = new Rect((Screen.width - windowWidth) * 0.95f, (Screen.height - windowHeight) * 0.6f, windowWidth, windowHeight);
        float scrollPanelHeight = windowRect.height * 0.87f;

        if (scrollPos == null || resetScrollPosition) {
            scrollPos = new Vector2(0, float.MaxValue);
        }

        string windowName = "Archipelago Console";
        if (truncatingBacklog) {
            windowName += $" (last {backlogLimit} messages)";
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

        }, windowName, grayWindowStyle);
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

    private static void DrawSealCounts(bool updateText) {
        UpdateStyles();

        if (updateText) {
            UpdateSealCountsText();
        }

        float windowWidth = Screen.width * 0.5f; // same as console
        float windowHeight = Screen.height * 0.12f; // much shorter than console
        var windowRect = new Rect(
            (Screen.width - windowWidth) * 0.95f, // same as console
            (Screen.height - windowHeight) * 0.09f, // much higher than console
            windowWidth, windowHeight);

        GUI.Window(11261730, windowRect, (int windowID) => {
            GUILayout.BeginVertical();
            GUILayout.Label(sealCountsText);
            GUILayout.EndVertical();
        }, "", clearWindowStyle);
    }

    private static string sealCountsText = "";

    private static void UpdateSealCountsText() {
        int sealCount = InMemoryInventory.GetSolSealsCount();

        sealCountsText = $"Eigong seals: {sealCount}/{NewKunlunControlHubEntrance.GetSealCountForEigong()}"
            + $"\nLady E seals: {sealCount}/{LadyESoulscapeEntrance.GetSealCountForLadyE()}"
            + $"\nPrison seals: {sealCount}/{Jiequan1Fight.GetSealCountForPrison()}";
    }
}
