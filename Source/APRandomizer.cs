using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArchipelagoRandomizer;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class APRandomizer : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
    public ConfigEntry<bool> BossScalingSetting = null!;
    public ConfigEntry<bool> ForceTrueEigongSetting = null!;
    private ConfigEntry<bool> DeathLinkSetting = null!;
    public ConfigEntry<bool> FlowerlessDeathLinkSetting = null!;
    public ConfigEntry<bool> ShowAPMessagesSetting = null!;
    public ConfigEntry<bool> FilterAPMessagesByPlayerSetting = null!;

    private Harmony harmony = null!;

    public static APRandomizer Instance = null!;

    public static string SaveSlotsPath => Application.persistentDataPath;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);
        Instance = this;

        // Current patch hacks. TODO: if these seem stable, apply them to NineSolsAPI, then remove them here
        var bepinexManagerObject = this.gameObject;
        bepinexManagerObject.hideFlags = HideFlags.HideAndDontSave;

        var temp = new GameObject();
        UnityEngine.Object.DontDestroyOnLoad(temp);
        var nineSolsAPIFullscreenCanvasObject = temp.scene.GetRootGameObjects().FirstOrDefault(go => go.name == "NineSolsAPI-FullscreenCanvas (RCGLifeCycle)");
        nineSolsAPIFullscreenCanvasObject?.hideFlags = HideFlags.HideAndDontSave;

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(APRandomizer).Assembly);

        // Client settings

        BossScalingSetting = Config.Bind("", "Boss Scaling", false,
            "Edit the health and damage values of (non-Eigong) Battle Memories bosses so they scale with the actual order you end up fighting them in the randomizer, instead of the vanilla game's expected order." +
            "\n\nFor example: If you fight Ji first, he'll have lower stats than vanilla. If you fight Yanlao last, he'll have higher stats than vanilla. Exact adjustments will be shown when you encounter the boss." +
            "\n\nSince Battle Memories stats are used as a guide, this setting ignores all bosses not included in the Battle Memories mode.");

        ForceTrueEigongSetting = Config.Bind("", "Force True Eigong", false,
            "Set the true ending flag when you first enter New Kunlun Control Hub, so you can fight all 3 phases of True Eigong without having to do all the usual sidequests first." +
            "\n\nIf you toggle this setting after unlocking the New Kunlun Control Hub node, then the true ending flag will be toggled immediately.");

        ForceTrueEigongSetting.SettingChanged += (_, _) => {
            Log.Info($"ForceTrueEigongSetting changed to {ForceTrueEigongSetting.Value}");
            var hasNKCHNode = TeleportPoints.IsNodeUnlocked(TeleportPoints.TeleportPoint.NewKunlunControlHub);
            if (hasNKCHNode) {
                ToastManager.Toast($"<color=orange>Changing the true ending flag to {ForceTrueEigongSetting.Value}</color> because the 'Force True Eigong' setting was changed.");
                var trueEndingFlag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["e78958a13315eb9418325caf25da9d4dScriptableDataBool"];
                trueEndingFlag.CurrentValue = ForceTrueEigongSetting.Value;
            }
        };

        DeathLinkSetting = Config.Bind("Death Link", "Death Link", false,
            "When you die, everyone who enabled death link dies. Of course, the reverse is true too.");

        Log.Info($"applying initial DeathLink setting of {DeathLinkSetting.Value}");
        DeathLinkManager.ApplyModSetting(DeathLinkSetting.Value);

        DeathLinkSetting.SettingChanged += (_, _) => {
            Log.Info($"DeathLink setting changed to {DeathLinkSetting.Value}");
            DeathLinkManager.ApplyModSetting(DeathLinkSetting.Value);
        };

        FlowerlessDeathLinkSetting = Config.Bind("Death Link", "Flowerless Death Link", true,
            "When you die from receiving a death link, no Tianhuo flower will be produced, and no jin or experience will be taken away." +
            "\n\nHas no effect on regular deaths.");

        ShowAPMessagesSetting = Config.Bind("Archipelago Server Messages", "Show AP Messages", true,
            "Display all messages the Archipelago server sends to clients in the main game window. Turn this off if you find the AP messages too spammy." +
            "\n\nHidden messages can still be read in the BepInEx console window, or in any AP text client you have connected to the same slot.");

        FilterAPMessagesByPlayerSetting = Config.Bind("Archipelago Server Messages", "Filter By Player", false,
            "Only display 'Player1 found Player2's Item' messsages if you are one of those players. " +
            "In larger multiworlds, this should hide the vast majority of AP messages not relevant to you, without disabling them completely." +
            "\n\nHidden messages can still be read in the BepInEx console window, or in any AP text client you have connected to the same slot.");

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
            for (var i = 0; i < 30; i++)
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
        ItemApplications.Update();
        DeathLinkManager.Update();
    }
    private void OnGUI() {
        ConnectionAndPopups.OnGUI();
        DebugTools.OnGUI();
    }
}
