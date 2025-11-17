using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using HarmonyLib;
using NineSolsAPI;
using System;
using UnityEngine;

namespace ArchipelagoRandomizer;

class DebugTools {
    public static bool ShowDebugToolsPopup = false;

    public static void Update() {
        if (ShowDebugToolsPopup) {
            Cursor.visible = true;
        }
    }

    public static void OnGUI() {
        ConnectionAndPopups.UpdateStyles();

        if (ShowDebugToolsPopup) {
            DrawDebugToolsPopup();
        }
    }

    private static string DebugPopup_Item = "";
    private static string DebugPopup_Count = "";

    private static void DrawDebugToolsPopup() {
        float windowWidth = Screen.width * 0.6f;
        float windowHeight = Screen.height * 0.7f;
        var windowRect = new Rect((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight);

        var textFieldWidth = GUILayout.Width(windowRect.width * 0.6f);

        var windowStyle = ConnectionAndPopups.windowStyle;
        var labelStyle = ConnectionAndPopups.labelStyle;
        var textFieldStyle = ConnectionAndPopups.textFieldStyle;
        var buttonStyle = ConnectionAndPopups.buttonStyle;

        var centeredLabelStyle = new GUIStyle(labelStyle);
        centeredLabelStyle.alignment = TextAnchor.UpperCenter;

        // "GUI.ModalWindow" exists but is useless here; it doesn't prevent RCG's UI widgets from receiving input
        GUI.Window(11261728, windowRect, (int windowID) => {
            GUILayout.Label("", centeredLabelStyle);
            GUILayout.Label("NPCs & Events", centeredLabelStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock Jiequan 1 Fight", buttonStyle)) {
                ToastManager.Toast("unlocking Jiequan 1 Fight");
                Jiequan1Fight.ActuallyTriggerJiequan1Fight();
            }
            if (GUILayout.Button("Unlock Lady E Soulscape", buttonStyle)) {
                ToastManager.Toast("unlocking Lady E Soulscape");
                LadyESoulscapeEntrance.ActuallyTriggerLadyESoulscape();
            }
            if (GUILayout.Button("Move Chiyou into FSP", buttonStyle)) {
                ToastManager.Toast("moving Chiyou into FSP");
                var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool"];
                flag.CurrentValue = true;
            }
            if (GUILayout.Button("Move Kuafu into FSP", buttonStyle)) {
                ToastManager.Toast("moving Kuafu into FSP");
                var flag = (ScriptableDataBool)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["e2ccc29dc8f187b45be6ce50e7f4174aScriptableDataBool"];
                flag.CurrentValue = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("", centeredLabelStyle);
            GUILayout.Label("Miscellaneous", centeredLabelStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock Most Teleports", buttonStyle)) {
                ToastManager.Toast("unlocking most teleport points");
                TeleportPoints.UnlockAllNonPrisonTeleportPoints();
            }
            if (GUILayout.Button("Test Death Link", buttonStyle)) {
                ToastManager.Toast("triggering test death link");
                DeathLinkManager.OnDeathLinkReceived(new DeathLink("death link test player", "death link test cause"));
            }
            if (GUILayout.Button("Give 9999 Jin", buttonStyle)) {
                ToastManager.Toast("giving 99999 jin");
                SingletonBehaviour<GameCore>.Instance.playerGameData.AddGold(99999, GoldSourceTag.DevCheat);
            }
            if (GUILayout.Button("Check 1 Unchecked Location", buttonStyle)) {
                ToastManager.Toast("triggering random unchecked location check");
                var locId = ConnectionAndPopups.APSession!.Locations.AllMissingLocations[0];
                LocationTriggers.CheckLocation(LocationNames.archipelagoIdToLocation[locId]);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable Weakened Prison State", buttonStyle)) {
                ToastManager.Toast("enabling weakened prison state");
                var weakenedPrisonStateFlag = (PlayerAbilityScenarioModifyPack)SaveManager.Instance.allFlags.FlagDict["df6a9a9f7748f4baba6207afdf10ea31PlayerAbilityScenarioModifyPack"];
                weakenedPrisonStateFlag.ApplyOverriding(weakenedPrisonStateFlag);
            }
            if (GUILayout.Button("Disable Weakened Prison State", buttonStyle)) {
                ToastManager.Toast("disabling weakened prison state");
                var weakenedPrisonStateFlag = (PlayerAbilityScenarioModifyPack)SaveManager.Instance.allFlags.FlagDict["df6a9a9f7748f4baba6207afdf10ea31PlayerAbilityScenarioModifyPack"];
                weakenedPrisonStateFlag.RevertApply(weakenedPrisonStateFlag);
            }
            if (GUILayout.Button("Toggle Hat", buttonStyle)) {
                ToastManager.Toast("toggling hat");
                var p = Player.i;
                if (p) {
                    var hasHat = AccessTools.FieldRefAccess<Player, bool>("_hasHat").Invoke(p);
                    p.SetHasHat(!hasHat); // this method does more than just set the value of _hasHat
                }
            }
            if (GUILayout.Button("Toggle Story Walk", buttonStyle)) {
                ToastManager.Toast("toggling story walk");
                var p = Player.i;
                p?.SetStoryWalk(!p.IsStoryWalk, 1f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("", centeredLabelStyle);
            GUILayout.Label("Core Progression Items", centeredLabelStyle);

            var fixedWidthLabelStyle = new GUIStyle(labelStyle);
            fixedWidthLabelStyle.fixedWidth = 200;

            var onOffButtonStyle = new GUIStyle(buttonStyle);
            onOffButtonStyle.fixedWidth = 50;

            GUILayout.BeginHorizontal();
            GUILayout.Label("MysticNymphScoutMode", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.MysticNymphScoutMode, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.MysticNymphScoutMode, 0);
            }

            GUILayout.Label("TaiChiKick", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.TaiChiKick, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.TaiChiKick, 0);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("ChargedStrike", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.ChargedStrike, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.ChargedStrike, 0);
            }

            GUILayout.Label("AirDash", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.AirDash, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.AirDash, 0);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("UnboundedCounter", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.UnboundedCounter, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.UnboundedCounter, 0);
            }

            GUILayout.Label("CloudLeap", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.CloudLeap, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.CloudLeap, 0);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("SuperMutantBuster", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.SuperMutantBuster, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.SuperMutantBuster, 0);
            }

            GUILayout.Label("Wall Climb", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.WallClimb, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.WallClimb, 0);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Grapple", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.Grapple, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.Grapple, 0);
            }

            GUILayout.Label("Ledge Grab", fixedWidthLabelStyle);
            if (GUILayout.Button("On", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.LedgeGrab, 1);
            }
            if (GUILayout.Button("Off", onOffButtonStyle)) {
                ItemApplications.UpdateItemCount(Item.LedgeGrab, 0);
            }
            GUILayout.EndHorizontal();

            var updateButtonStyle = new GUIStyle(buttonStyle);
            updateButtonStyle.fixedWidth = 80;

            GUILayout.Label("", centeredLabelStyle);
            GUILayout.Label("Arbitrary Item Update (using the mod's Item enum, not the AP names)", centeredLabelStyle);

            GUILayout.BeginHorizontal();
            DebugPopup_Item = GUILayout.TextField(DebugPopup_Item, textFieldStyle, GUILayout.Width(windowRect.width * 0.3f));
            DebugPopup_Count = GUILayout.TextField(DebugPopup_Count, textFieldStyle, GUILayout.Width(windowRect.width * 0.1f));
            if (GUILayout.Button("Update", updateButtonStyle)) {
                ItemApplications.UpdateItemCount(Enum.Parse<Item>(DebugPopup_Item), int.Parse(DebugPopup_Count));
            }
            GUILayout.EndHorizontal();

        }, "Archipelago Randomizer Debug Tools (Ctrl+Alt+Shift+D to show/hide)", windowStyle);
    }
}
