﻿using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using EasyEditor;
using Newtonsoft.Json;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArchipelagoRandomizer;

internal class ConnectionAndPopups {
    private static GUIStyle windowStyle;
    private static GUIStyle labelStyle;
    private static GUIStyle textFieldStyle;
    private static GUIStyle buttonStyle;

    private static void UpdateStyles() {
        if (labelStyle == null) {
            windowStyle = new GUIStyle(GUI.skin.window);

            // apparently this is what it takes to make a window *not* be transparent in IMGUI
            var bgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            bgColorTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            bgColorTex.Apply();
            windowStyle.normal.background = bgColorTex;
            windowStyle.onActive.background = bgColorTex;
            windowStyle.onFocused.background = bgColorTex;
            windowStyle.onHover.background = bgColorTex;
            windowStyle.onNormal.background = bgColorTex;

            labelStyle = new GUIStyle(GUI.skin.label);
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            buttonStyle = new GUIStyle(GUI.skin.button);
        }

        float scaleFactor = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
        int scaledFont = Mathf.RoundToInt(24 * scaleFactor);
        windowStyle.fontSize = scaledFont;
        labelStyle.fontSize = scaledFont;
        textFieldStyle.fontSize = scaledFont;
        buttonStyle.fontSize = scaledFont;
    }

    // State and Transition Methods

    public static ArchipelagoSession? APSession = null;

    public static Dictionary<string, object> SlotData = null;

    private enum ConnectionPopup {
        None,
        InputConnectionInfo,
        Connecting,
        ConnectionError,
        RoomIdMismatchWarning,
    };
    private static ConnectionPopup currentPopup = ConnectionPopup.None;

    private static string InputPopup_Server = "archipelago.gg:12345";
    private static string InputPopup_Slot = "Solarian1";
    private static string InputPopup_Password = "";

    private static APRandomizerSaveData? ConnectionPopups_ApSaveDataRef = null;

    private static string ConnectionPopups_DisplayWarningOrError = "";

    public static TaskCompletionSource<APRandomizerSaveData>? connected = null;
    public static TaskCompletionSource<bool>? roomIdMismatchTCS = null;

    public static async Task<APRandomizerSaveData> GetConnectionInfoFromUser(APRandomizerSaveData? apSaveData) {
        currentPopup = ConnectionPopup.InputConnectionInfo;
        ConnectionPopups_ApSaveDataRef = apSaveData;

        if (apSaveData?.apConnectionData != null) {
            var acd = apSaveData.apConnectionData;
            InputPopup_Server = $"{acd.hostname}:{acd.port}";
            InputPopup_Slot = acd.slotName;
            InputPopup_Password = acd.password;
        }

        connected = new TaskCompletionSource<APRandomizerSaveData>();
        return await connected.Task;
    }

    public static async Task ResumePreviousConnection(APRandomizerSaveData apSaveData) {
        currentPopup = ConnectionPopup.Connecting;
        ConnectionPopups_ApSaveDataRef = apSaveData;

        var acd = apSaveData.apConnectionData;
        InputPopup_Server = $"{acd.hostname}:{acd.port}";
        InputPopup_Slot = acd.slotName;
        InputPopup_Password = acd.password;

        connected = new TaskCompletionSource<APRandomizerSaveData>();
        AttemptToConnect();
        await connected.Task;
    }

    private static void ConnectButtonClicked() {
        ToastManager.Toast($"Connect button clicked: Server = {InputPopup_Server}, Slot = {InputPopup_Slot}, Password = {InputPopup_Password}");

        APConnectionData apConnData = new();
        var split = InputPopup_Server.Split(':');
        apConnData.hostname = split[0];
        // if the player left out a port number, use the default localhost port of 38281
        apConnData.port = (split.Length > 1) ? int.Parse(split[1]) : 38281;
        apConnData.slotName = InputPopup_Slot;
        apConnData.password = InputPopup_Password;

        if (ConnectionPopups_ApSaveDataRef == null) {
            var newApSaveData = new APRandomizerSaveData();
            newApSaveData.apConnectionData = new();
            newApSaveData.itemsAcquired = new();
            newApSaveData.locationsChecked = new();
            ConnectionPopups_ApSaveDataRef = newApSaveData;
        }
        ConnectionPopups_ApSaveDataRef.apConnectionData = apConnData;

        AttemptToConnect();
    }

    private static void CancelClickedAfterError() {
        ToastManager.Toast($"Cancel button clicked after error = {ConnectionPopups_DisplayWarningOrError}");
        currentPopup = ConnectionPopup.None;
        connected.SetException(new Exception(ConnectionPopups_DisplayWarningOrError));
    }

    private static async Task AttemptToConnect() {
        LoginResult? loginResult = null;
        string? exceptionMessage = null;

        try {
            loginResult = await ConnectToAPServer();
        } catch (Exception ex) {
            Log.Info($"ConnectToAPServer() threw an exception:\n\n{ex.Message}\n{ex.StackTrace}");
            exceptionMessage = ex.Message;
        }

        if (loginResult == null || !loginResult.Successful) {
            var err = (exceptionMessage != null) ?
                    $"Failed to connect to AP server:\n{exceptionMessage}" :
                    $"Failed to connect to AP server:\n{string.Join("\n", ((LoginFailure)loginResult).Errors)}";
            Log.Info(err);

            currentPopup = ConnectionPopup.ConnectionError;
            ConnectionPopups_DisplayWarningOrError = err;
        } else {
            currentPopup = ConnectionPopup.None;
            connected!.SetResult(ConnectionPopups_ApSaveDataRef!);
        }
    }

    private static async Task<LoginResult?> ConnectToAPServer() {
        var acd = ConnectionPopups_ApSaveDataRef!.apConnectionData;
        Log.Info($"ConnectToAPServer() called with {acd.hostname} / {acd.port} / {acd.slotName} / {acd.password}");
        if (APSession != null) {
            //APSession.Items.ItemReceived -= APSession_ItemReceived;
            //APSession.MessageLog.OnMessageReceived -= APSession_OnMessageReceived;
            //OnSessionClosed(APSession, true);
            APSession = null;
        }
        var session = ArchipelagoSessionFactory.CreateSession(acd.hostname, acd.port);
        LoginResult result = session.TryConnectAndLogin("Nine Sols", acd.slotName, ItemsHandlingFlags.AllItems, version: new Version(0, 4, 4), password: acd.password, requestSlotData: true);
        if (!result.Successful)
            return result;

        APSession = session;
        SlotData = ((LoginSuccessful)result).SlotData;

        //APSession.Socket.ErrorReceived += APSession_ErrorReceived;

        var oldRoomId = acd.roomId;
        var newRoomId = APSession.RoomState.Seed;
        Log.Info($"old room id from save file is {oldRoomId}, new room id from AP server is {newRoomId}");

        // I don't know if RoomState.Seed is guaranteed to always exist, so if it somehow doesn't and newRoomId is null just act like nothing happened
        // oldRoomId being null usually means this is our first connection on a New Expedition.
        if (oldRoomId == null || newRoomId == null || oldRoomId == newRoomId) {
            if (oldRoomId == null || newRoomId == null) {
                ConnectionPopups_ApSaveDataRef.apConnectionData.roomId = newRoomId;
            }

            FinishConnectingToAPServer();
            return result;
        }

        // Room id doesn't match, show the user a warning popup letting them choose whether to "finish" the connection we've started.
        var modSaveCheckedLocationsCount = ConnectionPopups_ApSaveDataRef.locationsChecked.Where(kv => kv.Value).Count();
        var apServerCheckedLocationCount = APSession.Locations.AllLocationsChecked.Count;
        var countDifference = modSaveCheckedLocationsCount - apServerCheckedLocationCount;
        var warning = $"This AP server has a different room id from the one you previously connected to on this profile. ";
        if (countDifference > 0)
            warning += $"Continuing with this connection will likely tell the server to immediately mark {countDifference} locations as checked. ";
        warning += $"This usually means you forgot to start a New Game or change connection info. Connect anyway?";
        Log.Warning(warning);

        ConnectionPopups_DisplayWarningOrError = warning;
        currentPopup = ConnectionPopup.RoomIdMismatchWarning;

        roomIdMismatchTCS = new TaskCompletionSource<bool>();
        var roomIdMismatchUserResponse = await roomIdMismatchTCS.Task;
        if (roomIdMismatchUserResponse) {
            FinishConnectingToAPServer();
            return result;
        } else {
            return null;
        }
    }

    private static void FinishConnectingToAPServer() {
        Log.Info($"Received SlotData: {JsonConvert.SerializeObject(SlotData)}");

        // TODO: slot_data processing goes here

        Log.Info($"FinishConnectingToAPServer ConnectionPopups_ApSaveDataRef={ConnectionPopups_ApSaveDataRef} APSession={APSession}");
        // Ensure that our local items state matches APSession.Items.AllItemsReceived. It's possible for AllItemsReceived to be out of date,
        // but in that case the ItemReceived event handler will be invoked as many times as it takes to get up to date.
        var totalItemsAcquired = ConnectionPopups_ApSaveDataRef!.itemsAcquired.Sum(kv => kv.Value);
        var totalItemsReceived = APSession!.Items.AllItemsReceived.Count;
        // TODO
        /*if (totalItemsReceived > totalItemsAcquired) {
            Log.Info($"AP server state has more items ({totalItemsReceived}) than local save data ({totalItemsAcquired}). Attempting to update local save data to match.");
            foreach (var itemInfo in APSession.Items.AllItemsReceived)
                saveDataChanged = SyncItemCountWithAPServer(itemInfo.ItemId);
        }*/

        //APSession.Items.ItemReceived += APSession_ItemReceived;
        //APSession.MessageLog.OnMessageReceived += APSession_OnMessageReceived;

        // ensure that our local locations state matches the AP server by simply re-reporting any "missed" locations
        // it's important to do this after setting up the event handlers above, since a missed location will lead to AP sending us an item and a message
        // TODO
        /*var locallyCheckedLocationIds = ConnectionPopups_ApSaveDataRef.locationsChecked
            .Where(kv => kv.Value && LocationNames.locationToArchipelagoId.ContainsKey(kv.Key))
            .Select(kv => (long)LocationNames.locationToArchipelagoId[kv.Key]);
        var locationIdsMissedByServer = locallyCheckedLocationIds.Where(id =>
            APSession.Locations.AllLocations.Contains(id) &&
            !APSession.Locations.AllLocationsChecked.Contains(id));
        if (locationIdsMissedByServer.Any()) {
            ArchConsoleManager.WakeupConsoleMessages.Add($"{locationIdsMissedByServer.Count()} locations you've previously checked were not marked as checked on the AP server:\n" +
                locationIdsMissedByServer.Join(id => "- " + LocationNames.locationNames[LocationNames.archipelagoIdToLocation[id]], "\n") +
                $"\nSending them to the AP server now.");
            APSession.Locations.CompleteLocationChecks(locationIdsMissedByServer.ToArray());
        }*/

        //OnSessionOpened(APSession);

        Log.Info($"FinishConnectingToAPServer B ");
        if (connected == null) {
            Log.Info($"FinishConnectingToAPServer found connected was null somehow ???");
        } else {
            if (ConnectionPopups_ApSaveDataRef == null) {
                Log.Info($"FinishConnectingToAPServer found ConnectionPopups_ApSaveDataRef was null somehow ???");
            } else {
                connected.SetResult(ConnectionPopups_ApSaveDataRef!);
            }
        }
    }

    // Update/Draw Methods

    public static void Update() {
        if (currentPopup != ConnectionPopup.None) {
            Cursor.visible = true;
        }
    }

    public static void OnGUI() {
        UpdateStyles();

        if (currentPopup == ConnectionPopup.InputConnectionInfo) {
            DrawInputPopup();
        } else if (currentPopup == ConnectionPopup.Connecting) {
            DrawConnectingPopup();
        } else if (currentPopup == ConnectionPopup.ConnectionError) {
            DrawErrorPopup();
        } else if (currentPopup == ConnectionPopup.RoomIdMismatchWarning) {
            DrawRoomIdMismatchPopup();
        }
    }
    private static Rect GetPopupWindowRectangle() {
        float windowWidth = Screen.width * 0.4f;
        float windowHeight = Screen.height * 0.35f;
        return new Rect((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight);
    }

    private static void DrawInputPopup() {
        var windowRect = GetPopupWindowRectangle();
        var textFieldWidth = GUILayout.Width(windowRect.width * 0.6f);

        // "GUI.ModalWindow" exists but is useless here; it doesn't prevent RCG's UI widgets from receiving input
        GUI.Window(11261727, windowRect, (int windowID) => {
            GUILayout.Label("", labelStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Server Address & Port", labelStyle);
            InputPopup_Server = GUILayout.TextField(InputPopup_Server, textFieldStyle, textFieldWidth);
            GUILayout.EndHorizontal();

            GUILayout.Label("   e.g. \"archipelago.gg:12345\", \"localhost:38281\"", labelStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Player/Slot Name", labelStyle);
            InputPopup_Slot = GUILayout.TextField(InputPopup_Slot, textFieldStyle, textFieldWidth);
            GUILayout.EndHorizontal();

            GUILayout.Label("   e.g. \"Solarian1\"", labelStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Password (if any)", labelStyle);
            InputPopup_Password = GUILayout.TextField(InputPopup_Password, textFieldStyle, textFieldWidth);
            GUILayout.EndHorizontal();

            GUILayout.Label("", labelStyle);

            if (GUILayout.Button("Connect to AP Server", buttonStyle)) {
                ConnectButtonClicked();
            }
        }, "Archipelago Connection Info", windowStyle);
    }

    private static void DrawConnectingPopup() {
        var windowRect = GetPopupWindowRectangle();
        GUI.Window(11261727, windowRect, (int windowID) => {
            GUILayout.Label("\n\n\nConnecting...", labelStyle);
        }, "", windowStyle);
    }

    private static void DrawErrorPopup() {
        var windowRect = GetPopupWindowRectangle();
        GUI.Window(11261727, windowRect, (int windowID) => {
            GUILayout.Label(ConnectionPopups_DisplayWarningOrError, labelStyle, GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retry", buttonStyle)) {
                AttemptToConnect();
            }
            if (GUILayout.Button("Cancel", buttonStyle)) {
                CancelClickedAfterError();
            }
            GUILayout.EndHorizontal();
        }, "", windowStyle);
    }

    private static void DrawRoomIdMismatchPopup() {
        var windowRect = GetPopupWindowRectangle();
        GUI.Window(11261727, windowRect, (int windowID) => {
            GUILayout.Label("", labelStyle);

            GUILayout.Label(ConnectionPopups_DisplayWarningOrError, labelStyle);

            GUILayout.Label("", labelStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Connect Anyway", buttonStyle)) {
                roomIdMismatchTCS.SetResult(true);
            }
            if (GUILayout.Button("Cancel", buttonStyle)) {
                roomIdMismatchTCS.SetResult(false);
            }
            GUILayout.EndHorizontal();
        }, "", windowStyle);
    }
}
