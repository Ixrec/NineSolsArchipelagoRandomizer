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

    private Harmony harmony = null!;

    public static string SaveSlotsPath => Application.persistentDataPath;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(APRandomizer).Assembly);

        DeathLinkSetting = Config.Bind("", "Death Link", false,
            "When you die, everyone who enabled death link dies. Of course, the reverse is true too.");

        Log.Info($"applying initial DeathLink setting of {DeathLinkSetting.Value}");
        DeathLinkManager.ApplyModSetting(DeathLinkSetting.Value);

        DeathLinkSetting.SettingChanged += (_, _) => {
            Log.Info($"DeathLink setting changed to {DeathLinkSetting.Value}");
            DeathLinkManager.ApplyModSetting(DeathLinkSetting.Value);
        };

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
        ConnectionAndPopups.Update();
        DebugTools.Update();
    }
    private void OnGUI() {
        ConnectionAndPopups.OnGUI();
        DebugTools.OnGUI();
    }
}
