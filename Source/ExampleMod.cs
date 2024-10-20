using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;

namespace ExampleMod;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class ExampleMod : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
    private ConfigEntry<bool> enableSomethingConfig = null!;
    private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut = null!;

    private Harmony harmony = null!;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(ExampleMod).Assembly);

        enableSomethingConfig = Config.Bind("General.Something", "Enable", true, "Enable the thing");
        somethingKeyboardShortcut = Config.Bind("General.Something", "Shortcut",
            new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl), "Shortcut to execute");

        // Usage of the modding API is entirely optional.
        // It provides utilities like the KeybindManager, utilities for Instantiating objects including the 
        // NineSols lifecycle hooks, displaying toast messages and preloading objects from other scenes.
        // If you do use the API make sure do have it installed when running your mod, and keep the dependency in the
        // thunderstore.toml.

        KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);

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
}