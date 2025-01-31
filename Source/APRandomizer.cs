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
            Item.MysticNymphScoutMode,
            Item.TaiChiKick,
            Item.CloudLeap,
            Item.ChargedStrike,
            Item.AirDash,
            Item.UnboundedCounter,
            Item.SuperMutantBuster,
        ];
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[0]} count to 1"); ItemApplications.UpdateItemCount(items[0], 1); },
            new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[1]} count to 1"); ItemApplications.UpdateItemCount(items[1], 1); },
            new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[2]} count to 1"); ItemApplications.UpdateItemCount(items[2], 1); },
            new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[3]} count to 1"); ItemApplications.UpdateItemCount(items[3], 1); },
            new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[4]} count to 1"); ItemApplications.UpdateItemCount(items[4], 1); },
            new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[5]} count to 1"); ItemApplications.UpdateItemCount(items[5], 1); },
            new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[6]} count to 1"); ItemApplications.UpdateItemCount(items[6], 1); },
            new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));

        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[0]} count to 0"); ItemApplications.UpdateItemCount(items[0], 0); },
            new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[1]} count to 0"); ItemApplications.UpdateItemCount(items[1], 0); },
            new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[2]} count to 0"); ItemApplications.UpdateItemCount(items[2], 0); },
            new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[3]} count to 0"); ItemApplications.UpdateItemCount(items[3], 0); },
            new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[4]} count to 0"); ItemApplications.UpdateItemCount(items[4], 0); },
            new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[5]} count to 0"); ItemApplications.UpdateItemCount(items[5], 0); },
            new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftShift));
        KeybindManager.Add(this, () => { ToastManager.Toast($"setting {items[6]} count to 0"); ItemApplications.UpdateItemCount(items[6], 0); },
            new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftShift));

        KeybindManager.Add(this, () => { ToastManager.Toast("T"); }, new KeyboardShortcut(KeyCode.T));

        /*var item = Item.PipeVial;
        KeybindManager.Add(this, () => {
            ItemApplications.UpdateItemCount(item, 0);
        }, new KeyboardShortcut(KeyCode.Alpha0));
        KeybindManager.Add(this, () => {
            ItemApplications.UpdateItemCount(item, 1);
        }, new KeyboardShortcut(KeyCode.Alpha1));
        KeybindManager.Add(this, () => {
            ItemApplications.UpdateItemCount(item, 2);
        }, new KeyboardShortcut(KeyCode.Alpha2));
        KeybindManager.Add(this, () => {
            ItemApplications.UpdateItemCount(item, 3);
        }, new KeyboardShortcut(KeyCode.Alpha3));
        KeybindManager.Add(this, () => {
            ItemApplications.UpdateItemCount(item, 4);
        }, new KeyboardShortcut(KeyCode.Alpha4));*/

        KeybindManager.Add(this, () => {
            ToastManager.Toast("CST unlocking all TPs");
            NewGameCreation.UnlockAllTeleportPoints();
        }, new KeyboardShortcut(KeyCode.T, KeyCode.LeftShift, KeyCode.LeftControl));
        KeybindManager.Add(this, () => {
            ToastManager.Toast("CSG giving 99999 jin");
            SingletonBehaviour<GameCore>.Instance.playerGameData.AddGold(99999, GoldSourceTag.DevCheat);
        }, new KeyboardShortcut(KeyCode.G, KeyCode.LeftShift, KeyCode.LeftControl));
        KeybindManager.Add(this, () => {
            ToastManager.Toast("CSJ triggering Jiequan 1");
            Jiequan1Trigger.ActuallyTriggerJiequan1Fight();
        }, new KeyboardShortcut(KeyCode.J, KeyCode.LeftShift, KeyCode.LeftControl));

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
}