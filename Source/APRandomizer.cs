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
            Jiequan1Fight.ActuallyTriggerJiequan1Fight();
        }, new KeyboardShortcut(KeyCode.J, KeyCode.LeftShift, KeyCode.LeftControl));
        KeybindManager.Add(this, () => {
            ToastManager.Toast("CSL triggering Lady E");
            LadyESoulscapeEntrance.ActuallyTriggerLadyESoulscape();
        }, new KeyboardShortcut(KeyCode.L, KeyCode.LeftShift, KeyCode.LeftControl));
        KeybindManager.Add(this, () => {
            ToastManager.Toast("CSC triggering Chiyou in FSP");
            var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool"];
            flag.CurrentValue = true;
        }, new KeyboardShortcut(KeyCode.C, KeyCode.LeftShift, KeyCode.LeftControl));
        KeybindManager.Add(this, () => {
            ToastManager.Toast("CSK triggering Kuafu in FSP");
            var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["e2ccc29dc8f187b45be6ce50e7f4174aScriptableDataBool"];
            flag.CurrentValue = true;
        }, new KeyboardShortcut(KeyCode.K, KeyCode.LeftShift, KeyCode.LeftControl));
        KeybindManager.Add(this, () => {
            ToastManager.Toast("CSD triggering test death link");
            DeathLinkManager.OnDeathLinkReceived(new DeathLink("death link test player", "death link test cause"));
        }, new KeyboardShortcut(KeyCode.D, KeyCode.LeftShift, KeyCode.LeftControl));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading

        harmony.UnpatchSelf();
    }

    private void Update() {
        ConnectionAndPopups.Update();
    }
    private void OnGUI() {
        ConnectionAndPopups.OnGUI();
    }
}
