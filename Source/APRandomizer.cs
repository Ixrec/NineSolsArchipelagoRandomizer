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
    private static int x = -1;
    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(APRandomizer).Assembly);

        enableSomethingConfig = Config.Bind("General.Something", "Enable", true, "Enable the thing");
        somethingKeyboardShortcut = Config.Bind("General.Something", "Shortcut",
            new KeyboardShortcut(KeyCode.L), "Shortcut to execute");

        // Usage of the modding API is entirely optional.
        // It provides utilities like the KeybindManager, utilities for Instantiating objects including the 
        // NineSols lifecycle hooks, displaying toast messages and preloading objects from other scenes.
        // If you do use the API make sure do have it installed when running your mod, and keep the dependency in the
        // thunderstore.toml.

        KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);

        KeybindManager.Add(this, () => { ToastManager.Toast("I"); }, new KeyboardShortcut(KeyCode.I));
        KeybindManager.Add(this, () => { ToastManager.Toast("O"); }, new KeyboardShortcut(KeyCode.O));
        KeybindManager.Add(this, () => { ToastManager.Toast("P"); }, new KeyboardShortcut(KeyCode.P));

        KeybindManager.Add(this, () => {
            var ability = Player.i.mainAbilities.ParryCounterAbility;
            ToastManager.Toast($"KeyCode.Y ParryCounterAbility IsActivated={ability.IsActivated} IsAcquired={ability.IsAcquired}");
            //ToastManager.Toast($"KeyCode.Y PlayerMaxJadePowerStat Value={ability.Stat.Value} BaseValue={ability.Stat.BaseValue}");
            //ToastManager.Toast($"KeyCode.Y EndureDamageJade IsActivated={ability.IsActivated} IsAcquired={ability.AbilityData.IsAcquired}");
            ability.acquired.SetCurrentValue(true);
            //ability.Stat.BaseValue = 10;
        }, new KeyboardShortcut(KeyCode.Y));
        KeybindManager.Add(this, () => {
            var ability = Player.i.mainAbilities.ParryCounterAbility;
            ToastManager.Toast($"KeyCode.N ParryCounterAbility IsActivated={ability.IsActivated} IsAcquired={ability.IsAcquired}");
            //ToastManager.Toast($"KeyCode.N EndureDamageJade IsActivated={ability.IsActivated} IsAcquired={ability.AbilityData.IsAcquired}");
            ability.acquired.SetCurrentValue(false);
            //ability.Stat.BaseValue = 2;
        }, new KeyboardShortcut(KeyCode.N));

        KeybindManager.Add(this, () => {
            //string.Join("|", SingletonBehaviour<SaveManager>.Instance.allFlags.Select(f => f.Key))
            //string.Join("\n", SingletonBehaviour<UIManager>.Instance.allItemCollections.First().rawCollection.Select(f => f.Key))
            //GameObject.Find("AG_S2").transform.GetComponentsInChildren<MerchandiseTradeAction>();
            /*var c = SingletonBehaviour<UIManager>.Instance.allItemCollections.First().rawCollection;
            if (x < c.Count)
                x++;
            else
                x = 0;
            var randomInventoryItem = c[x];
            ToastManager.Toast($"T x={x} item={randomInventoryItem} / {randomInventoryItem?.Summary} / {randomInventoryItem?.Title} / {randomInventoryItem?.IsAcquired}");
            //SingletonBehaviour<UIManager>.Instance.ShowGetDescriptablePrompt(randomInventoryItem);
            SingletonBehaviour<UIManager>.Instance.ShowDescriptableNitification(randomInventoryItem);*/
            ToastManager.Toast($"T");
            //SingletonBehaviour<UIManager>.Instance.ShowMenu(PlayerInfoPanelType.TeleportPanel);
            showAPConnInfoPopup = !showAPConnInfoPopup;
        }, new KeyboardShortcut(KeyCode.T));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    // Some fields are private and need to be accessed via reflection.
    // You can do this with `typeof(Player).GetField("_hasHat", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Player.i)`
    // or using harmony access tools:
    private static readonly AccessTools.FieldRef<Player, bool>
        PlayerHasHat = AccessTools.FieldRefAccess<Player, bool>("_hasHat");

    private void TestMethod() {
        ToastManager.Toast("Hello Yi Nov 18");
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
