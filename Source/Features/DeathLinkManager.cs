using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Dialogue;
using HarmonyLib;
using NineSolsAPI;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
public class DeathLinkManager {
    public static bool DeathLinkSettingValue = false;

    private static DeathLinkService? service = null;

    private static bool manualDeathInProgress = false;

    public static void ApplyModSetting(bool enabled) {
        DeathLinkSettingValue = enabled;

        if (DeathLinkSettingValue && service == null && ConnectionAndPopups.APSession != null) {
            CreateDeathLinkService();
        } else {
            ConnectionAndPopups.OnSessionOpened += (_) => {
                if (DeathLinkSettingValue && service == null) {
                    CreateDeathLinkService();
                }
            };
        }
    }

    // Here we move received death link processing off the websocket thread because this is believed to help prevent crashes
    private static DeathLink? lastDeathLinkObjectReceived = null;

    private static void CreateDeathLinkService() {
        if (service != null)
            return;

        if (ConnectionAndPopups.APSession == null) {
            Log.Error($"EnableDeathLinkImplHelper unable to create death link service because APSession is null");
            return;
        }

        service = ConnectionAndPopups.APSession.CreateDeathLinkService();
        service.OnDeathLinkReceived += deathLinkObject => lastDeathLinkObjectReceived = deathLinkObject;
        service.EnableDeathLink();
    }

    public static void Update() {
        if (lastDeathLinkObjectReceived != null) {
            var deathLinkObject = lastDeathLinkObjectReceived;
            lastDeathLinkObjectReceived = null;
            OnDeathLinkReceived(deathLinkObject);
        }
    }

    public static void OnDeathLinkReceived(DeathLink deathLinkObject) {
        Log.Info($"OnDeathLinkReceived() Timestamp={deathLinkObject.Timestamp}, Source={deathLinkObject.Source}, Cause={deathLinkObject.Cause}");

        var dpgo = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/Always Canvas/DialoguePlayer(KeepThisEnable)");
        var dp = dpgo?.GetComponent<DialoguePlayer>();
        if (dp != null) {
            var playingDialogueGraph = AccessTools.FieldRefAccess<DialoguePlayer, DialogueGraph>("playingDialogueGraph").Invoke(dp);
            if (playingDialogueGraph != null) {
                InGameConsole.Add($"<color=orange>Ignoring death link</orange> ({deathLinkObject.Cause}) because dying mid-dialogue can softlock.");
                return;
            }
        }

        InGameConsole.Add(deathLinkObject.Cause);

        // we don't need to worry about the game being paused, because the game conveniently buffers the death for us
        ActuallyKillThePlayer();
    }

    private static void ActuallyKillThePlayer() {
        DeathLinkManager.manualDeathInProgress = true;
        // This is the only health-reducing method I could find that doesn't require a DamageDealer or other hard-to-fabricate object
        Player.i.health.ReceiveDOT_Damage(9999);
        DeathLinkManager.manualDeathInProgress = false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Player), "NeedDeathPenalty", MethodType.Getter)]
    static void Player_get_NeedDeathPenalty(Player __instance, ref bool __result) {
        if (!DeathLinkManager.manualDeathInProgress)
            return;

        if (__result == true && APRandomizer.Instance.FlowerlessDeathLinkSetting.Value) {
            Log.Info($"Player_get_NeedDeathPenalty preventing the usual death penalty because the Flowerless setting is on");
            __result = false;
        }
    }

    private static List<string> deathMessages = new List<string> {
        " became one with the Tao.",
        " wasn't smoking enough.",
        " could not parry death.",
    };

    private static System.Random prng = new System.Random();

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.HandlePlayerKilled))]
    public static void GameLevel_HandlePlayerKilled(GameLevel __instance) {
        Log.Info($"GameLevel.HandlePlayerKilled called in {__instance.name}");

        // if this death was sent to us by another player's death link, do nothing, since that would start an infinite death loop
        if (manualDeathInProgress) {
            Log.Info($"GameLevel.HandlePlayerKilled patch ignoring death because this is a death we received from another player");
            return;
        }

        if (DeathLinkSettingValue == false) {
            Log.Info($"GameLevel.HandlePlayerKilled patch ignoring death since death_link is off");
            return;
        }

        if (service == null) {
            Log.Error($"Unable to send death to AP server because death link service is null");
            return;
        }
        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"Unable to send death to AP server because we don't have an established AP connection");
            return;
        }

        Log.Info($"GameLevel.HandlePlayerKilled detected a death, sending to AP server");
        var slotName = APSaveManager.CurrentAPSaveData.apConnectionData.slotName;
        var deathLinkMessage = slotName + deathMessages[prng.Next(0, deathMessages.Count)];
        InGameConsole.Add($"Because death link is enabled, sending this death to other players with the message: \"{deathLinkMessage}\"");
        service.SendDeathLink(new DeathLink(slotName, deathLinkMessage));
    }
}
