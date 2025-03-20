using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using System;
using UnityEngine;
using System.IO;
using System.Reflection;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

namespace ArchipelagoRandomizer;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class APRandomizer : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
    private ConfigEntry<bool> DeathLinkSetting = null!;
    public ConfigEntry<bool> ShowAPMessagesSetting = null!;

    private Harmony harmony = null!;

    public static APRandomizer Instance = null;

    public static string SaveSlotsPath => Application.persistentDataPath;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);
        Instance = this;

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(APRandomizer).Assembly);

        // Client settings

        DeathLinkSetting = Config.Bind("", "Death Link", false,
            "When you die, everyone who enabled death link dies. Of course, the reverse is true too.");

        Log.Info($"applying initial DeathLink setting of {DeathLinkSetting.Value}");
        DeathLinkManager.ApplyModSetting(DeathLinkSetting.Value);

        DeathLinkSetting.SettingChanged += (_, _) => {
            Log.Info($"DeathLink setting changed to {DeathLinkSetting.Value}");
            DeathLinkManager.ApplyModSetting(DeathLinkSetting.Value);
        };

        ShowAPMessagesSetting = Config.Bind("", "Show Archipelago Messages In-Game", true,
            "Display all messages the Archipelago server sends to clients in the main game window. Turn this off if you find the AP messages too spammy." +
            "\n\nThe messages can still be read in the BepInEx console window, or in any AP text client you have connected to the same slot.");

        // Loading AP ids

        Log.Info($"trying to load Archipelago item and location IDs");
        try {
            var assembly = typeof(APRandomizer).GetTypeInfo().Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream("ArchipelagoRandomizer.items.jsonc"))) {
                ItemNames.LoadArchipelagoIds(reader.ReadToEnd());
            }
            using (var reader = new StreamReader(assembly.GetManifestResourceStream("ArchipelagoRandomizer.locations.jsonc"))) {
                LocationNames.LoadArchipelagoIds(reader.ReadToEnd());
            }
            Log.Info($"loaded Archipelago item and location IDs");
        } catch (Exception ex) {
            Log.Warning($"id loading threw: {ex.Message} with stack:\n{ex.StackTrace}");
            if (ex.InnerException != null) {
                Log.Warning($"id loading threw inner: {ex.InnerException.Message} with stack:\n{ex.InnerException.StackTrace}");
            }
        }

        // Debug keybinds

        KeybindManager.Add(this, () => {
            DebugTools.ShowDebugToolsPopup = !DebugTools.ShowDebugToolsPopup;
        }, new KeyboardShortcut(KeyCode.D, KeyCode.LeftShift, KeyCode.LeftControl, KeyCode.LeftAlt));

        KeybindManager.Add(this, () => {
            for(var i = 0; i < 30; i++)
                Log.Info("");
        }, new KeyboardShortcut(KeyCode.X, KeyCode.LeftShift, KeyCode.LeftControl, KeyCode.LeftAlt));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading

        harmony.UnpatchSelf();
    }

    private void Update() {
        Jiequan1Fight.Update();
        LadyESoulscapeEntrance.Update();

        ConnectionAndPopups.Update();
        DebugTools.Update();
    }
    private void OnGUI() {
        ConnectionAndPopups.OnGUI();
        DebugTools.OnGUI();
    }
}
