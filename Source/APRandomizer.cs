using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;

namespace ArchipelagoRandomizer;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class APRandomizer : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
    private ConfigEntry<bool> enableSomethingConfig = null!;
    private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut = null!;

    private Harmony harmony = null!;

    private bool showAPConnInfoPopup = false;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(APRandomizer).Assembly);

        enableSomethingConfig = Config.Bind("General.Something", "Enable", true, "Enable the thing");
        somethingKeyboardShortcut = Config.Bind("General.Something", "Shortcut",
            new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl), "Shortcut to execute");

        // Usage of the modding API is entirely optional.
        // It provides utilities like the KeybindManager, utilities for Instantiating objects including the 
        // NineSols lifecycle hooks, displaying toast messages and preloading objects from other scenes.
        // If you do use the API make sure do have it installed when running your mod, and keep the dependency in the
        // thunderstore.toml.

        KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);

        // Item application testing
        // TODO: make a proper debug interface, maybe a popup with a really weird shortcut?

        Item[] items = [
            Item.TaiChiKick,
            Item.CloudLeap,
            Item.ChargedStrike,
            Item.AirDash,
            Item.MysticNymphScoutMode,
        ];
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[0]} count to 1"); ItemApplications.ApplyItemToPlayer(items[0], 1, 0); },
            new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[1]} count to 1"); ItemApplications.ApplyItemToPlayer(items[1], 1, 0); },
            new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[2]} count to 1"); ItemApplications.ApplyItemToPlayer(items[2], 1, 0); },
            new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[3]} count to 1"); ItemApplications.ApplyItemToPlayer(items[3], 1, 0); },
            new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[4]} count to 1"); ItemApplications.ApplyItemToPlayer(items[4], 1, 0); },
            new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));

        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[0]} count to 0"); ItemApplications.ApplyItemToPlayer(items[0], 0, 1); },
            new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[1]} count to 0"); ItemApplications.ApplyItemToPlayer(items[1], 0, 1); },
            new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[2]} count to 0"); ItemApplications.ApplyItemToPlayer(items[2], 0, 1); },
            new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[3]} count to 0"); ItemApplications.ApplyItemToPlayer(items[3], 0, 1); },
            new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[4]} count to 0"); ItemApplications.ApplyItemToPlayer(items[4], 0, 1); },
            new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftShift));

        KeybindManager.Add(this, () => {
            ToastManager.Toast($"toggling AP connection info popup");
            showAPConnInfoPopup = !showAPConnInfoPopup;
        }, new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    // Some fields are private and need to be accessed via reflection.
    // You can do this with `typeof(Player).GetField("_hasHat", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Player.i)`
    // or using harmony access tools:
    private static readonly AccessTools.FieldRef<Player, bool>
        PlayerHasHat = AccessTools.FieldRefAccess<Player, bool>("_hasHat");

    private void TestMethod() {
        if (!enableSomethingConfig.Value) return;
        ToastManager.Toast("Shortcut activated");
        Log.Info("Log messages will only show up in the logging console and LogOutput.txt");

        // Sometimes variables aren't set in the title screen. Make sure to check for null to prevent crashes.
        if (Player.i == null) return;

        var hasHat = PlayerHasHat.Invoke(Player.i);
        Player.i.SetHasHat(!hasHat);
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading

        harmony.UnpatchSelf();
    }

    private void Update() {
        if (showAPConnInfoPopup) {
            Cursor.visible = true;
        }
    }

    private GUIStyle windowStyle;
    private GUIStyle labelStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle buttonStyle;

    private string server = "archipelago.gg:12345";
    private string slot = "Solarian1";
    private string password = "";

    private void OnGUI() {
        if (labelStyle == null) {
            windowStyle = new GUIStyle(GUI.skin.window);
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
                }
            }, "Archipelago Connection Info", windowStyle);
        }
    }
}
