using NineSolsAPI;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchipelagoRandomizer;

internal class ConnectionAndPopups {
    private static bool showAPConnInfoPopup = false;

    public static void Update() {
        if (showAPConnInfoPopup) {
            Cursor.visible = true;
        }
    }

    public static TaskCompletionSource<bool> connected = null;

    public static async Task GetConnectionInfoFromUser() {
        showAPConnInfoPopup = true;
        connected = new TaskCompletionSource<bool>();
        await connected.Task;
    }

    private static GUIStyle windowStyle;
    private static GUIStyle labelStyle;
    private static GUIStyle textFieldStyle;
    private static GUIStyle buttonStyle;

    private static string server = "archipelago.gg:12345";
    private static string slot = "Solarian1";
    private static string password = "";

    public static void OnGUI() {
        if (labelStyle == null) {
            windowStyle = new GUIStyle(GUI.skin.window);

            // apparently this is what it takes to make a window *not* be transparent in IMGUI
            var bgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            bgColorTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            bgColorTex.Apply();
            windowStyle.normal.background = bgColorTex;
            windowStyle.onActive.background = bgColorTex;
            windowStyle.onFocused.background = bgColorTex;
            windowStyle.onHover.background = bgColorTex;
            windowStyle.onNormal.background = bgColorTex;

            labelStyle = new GUIStyle(GUI.skin.label);
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            buttonStyle = new GUIStyle(GUI.skin.button);
        }

        if (showAPConnInfoPopup) {
            float scaleFactor = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            int scaledFont = Mathf.RoundToInt(24 * scaleFactor);
            windowStyle.fontSize = scaledFont;
            labelStyle.fontSize = scaledFont;
            textFieldStyle.fontSize = scaledFont;
            buttonStyle.fontSize = scaledFont;

            float windowWidth = Screen.width * 0.4f;
            float windowHeight = Screen.height * 0.35f;
            var windowRect = new Rect((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight);
            var textFieldWidth = GUILayout.Width(windowRect.width * 0.6f);

            // "GUI.ModalWindow" exists but is useless here; it doesn't prevent RCG's UI widgets from receiving input
            GUI.Window(11261727, windowRect, (int windowID) => {
                GUILayout.Label("", labelStyle);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Server Address & Port", labelStyle);
                server = GUILayout.TextField(server, textFieldStyle, textFieldWidth);
                GUILayout.EndHorizontal();

                GUILayout.Label("   e.g. \"archipelago.gg:12345\", \"localhost:38281\"", labelStyle);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Player/Slot Name", labelStyle);
                slot = GUILayout.TextField(slot, textFieldStyle, textFieldWidth);
                GUILayout.EndHorizontal();

                GUILayout.Label("   e.g. \"Solarian1\"", labelStyle);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Password (if any)", labelStyle);
                password = GUILayout.TextField(password, textFieldStyle, textFieldWidth);
                GUILayout.EndHorizontal();

                GUILayout.Label("", labelStyle);

                if (GUILayout.Button("Connect to AP Server", buttonStyle)) {
                    ToastManager.Toast($"Connect button clicked: Server = {server}, Slot = {slot}, Password = {password}");
                    // TODO actually connect, handle errors, etc
                    /*
                     * var APSession = ArchipelagoSessionFactory.CreateSession("archipelago.gg", 62219);
                     * LoginResult result = APSession.TryConnectAndLogin("Outer Wilds", "IxrecOW", ItemsHandlingFlags.AllItems, version: new Version(0, 4, 4), password: "", requestSlotData: true);
                     * ToastManager.Toast($"login successful = {result.Successful}");
                     */
                    showAPConnInfoPopup = false;
                    connected.SetResult(true);
                }
            }, "Archipelago Connection Info", windowStyle);
        }
    }
}
