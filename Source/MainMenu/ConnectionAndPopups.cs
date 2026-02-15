using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Newtonsoft.Json;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchipelagoRandomizer;

internal class ConnectionAndPopups {
    public static GUIStyle? windowStyle = null;
    public static GUIStyle? labelStyle = null;
    public static GUIStyle? textFieldStyle = null;
    public static GUIStyle? buttonStyle = null;
    public static Texture2D? bgColorTex = null;

    public static void UpdateStyles() {
        if (
            windowStyle == null ||
            labelStyle == null ||
            textFieldStyle == null ||
            buttonStyle == null
        ) {
            windowStyle = new GUIStyle(GUI.skin.window);

            // apparently this is what it takes to make a window *not* be transparent in IMGUI
            bgColorTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
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
    public static event Action<ArchipelagoSession>? OnSessionOpened;

    public static Dictionary<string, object>? SlotData = null;

    private enum ConnectionPopup {
        None,
        InputConnectionInfo,
        ChangeConnectionInfo,
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
    public static TaskCompletionSource<APConnectionData>? savedChanges = null;

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

    public static async Task<APConnectionData> ChangeConnectionInfo(APConnectionData acd) {
        currentPopup = ConnectionPopup.ChangeConnectionInfo;

        InputPopup_Server = $"{acd.hostname}:{acd.port}";
        InputPopup_Slot = acd.slotName;
        InputPopup_Password = acd.password;

        savedChanges = new TaskCompletionSource<APConnectionData>();
        return await savedChanges.Task;
    }

    public static async Task ResumePreviousConnection(APRandomizerSaveData apSaveData) {
        currentPopup = ConnectionPopup.Connecting;
        ConnectionPopups_ApSaveDataRef = apSaveData;

        var acd = apSaveData.apConnectionData;
        InputPopup_Server = $"{acd.hostname}:{acd.port}";
        InputPopup_Slot = acd.slotName;
        InputPopup_Password = acd.password;

        connected = new TaskCompletionSource<APRandomizerSaveData>();
        _ = AttemptToConnect();
        await connected.Task;
    }
    private static APConnectionData ParseConnDataFromPopupInputs() {
        APConnectionData apConnData = new();
        var split = InputPopup_Server.Split(':');
        apConnData.hostname = split[0];
        // if the player left out a port number, use the default localhost port of 38281
        apConnData.port = (split.Length > 1) ? int.Parse(split[1]) : 38281;
        apConnData.slotName = InputPopup_Slot;
        apConnData.password = InputPopup_Password;
        return apConnData;
    }

    private static void ConnectButtonClicked() {
        ToastManager.Toast($"Connect button clicked: Server = {InputPopup_Server}, Slot = {InputPopup_Slot}, Password = {InputPopup_Password}");

        APConnectionData apConnData = ParseConnDataFromPopupInputs();

        if (ConnectionPopups_ApSaveDataRef == null) {
            var newApSaveData = new APRandomizerSaveData();
            ConnectionPopups_ApSaveDataRef = newApSaveData;
        }
        ConnectionPopups_ApSaveDataRef.apConnectionData = apConnData;

        _ = AttemptToConnect();
    }

    private static void SaveInfoButtonClicked() {
        ToastManager.Toast($"Save info button clicked: Server = {InputPopup_Server}, Slot = {InputPopup_Slot}, Password = {InputPopup_Password}");

        APConnectionData apConnData = ParseConnDataFromPopupInputs();

        currentPopup = ConnectionPopup.None;
        savedChanges!.SetResult(apConnData); // savedChanges was initialized in ChangeConnectionInfo()
    }

    private static void CancelClickedAfterError() {
        ToastManager.Toast($"Cancel button clicked after error = {ConnectionPopups_DisplayWarningOrError}");
        currentPopup = ConnectionPopup.None;
        APSession = null;
        connected!.SetException(new Exception(ConnectionPopups_DisplayWarningOrError)); // connected was initialized in GetConnectionInfoFromUser()
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

        if (loginResult == null) {
            currentPopup = ConnectionPopup.None;
            APSession = null;
            connected!.SetException(new Exception("connection attempt aborted")); // connected was initialized in GetConnectionInfoFromUser()
        } else if (!loginResult.Successful) {
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

    public static void CleanupExistingAPServerConnection() {
        if (APSession != null) {
            Log.Info($"CleanupExistingAPServerConnection() called with a non-null AP session. Disconnecting socket, unsubscribing event handlers, and nulling out the session object.");
            APSession.Socket.DisconnectAsync();
            APSession.Items.ItemReceived -= ItemApplications.ItemReceived;
            APSession.MessageLog.OnMessageReceived -= OnAPMessage;
            APSession.Socket.ErrorReceived -= APSession_ErrorReceived;
            //OnSessionClosed(APSession, true);
            APSession = null;
        }
    }

    private static async Task<LoginResult?> ConnectToAPServer() {
        var acd = ConnectionPopups_ApSaveDataRef!.apConnectionData;
        Log.Info($"ConnectToAPServer() called with {acd.hostname} / {acd.port} / {acd.slotName} / {acd.password}");

        CleanupExistingAPServerConnection();

        var session = ArchipelagoSessionFactory.CreateSession(acd.hostname, acd.port);
        LoginResult result = session.TryConnectAndLogin("Nine Sols", acd.slotName, ItemsHandlingFlags.AllItems, password: acd.password, requestSlotData: true);
        if (!result.Successful)
            return result;

        APSession = session;
        SlotData = ((LoginSuccessful)result).SlotData;

        APSession.Socket.ErrorReceived += APSession_ErrorReceived;

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
        var warning = $"This AP server has a different room id from the one you previously connected to on this save file. ";
        if (countDifference > 0)
            warning += $"Continuing with this connection will likely tell the server to immediately mark {countDifference} locations as checked. ";
        warning += $"\n\nThis usually means you forgot to start a New Game or change connection info. Connect anyway?";
        Log.Warning(warning);

        ConnectionPopups_DisplayWarningOrError = warning;
        currentPopup = ConnectionPopup.RoomIdMismatchWarning;

        roomIdMismatchTCS = new TaskCompletionSource<bool>();
        Log.Info($"waiting for user response to room id warning");
        var roomIdMismatchUserResponse = await roomIdMismatchTCS.Task;
        Log.Info($"roomIdMismatchUserResponse={roomIdMismatchUserResponse}");
        if (roomIdMismatchUserResponse) {
            FinishConnectingToAPServer();
            return result;
        } else {
            CleanupExistingAPServerConnection();
            return null;
        }
    }

    private static void FinishConnectingToAPServer() {
        Log.Info($"Received SlotData: {JsonConvert.SerializeObject(SlotData)}");

        // all "eager" slot_data processing should go here
        if (SlotData != null) {
            if (SlotData.ContainsKey("jade_costs")) {
                JadeCosts.ApplySlotData(SlotData["jade_costs"]);
            }
            if (SlotData.ContainsKey("first_root_node_name")) {
                TeleportPoints.ApplySlotData((string)SlotData["first_root_node_name"]);
            }
            if (SlotData.ContainsKey("apworld_version")) {
                var worldVersion = Version.Parse((string)SlotData["apworld_version"]);
                var modVersion = new Version(0, 4, 3); // MUST MATCH .csproj VERSION
                var isVeryDifferent = (modVersion.Major != worldVersion.Major) || (modVersion.Minor != worldVersion.Minor);
                var onlyPatchDiffers = !isVeryDifferent && (modVersion.Build != worldVersion.Build);
                if (isVeryDifferent) {
                    InGameConsole.Add($"<color=red>Warning</color>: This Archipelago multiworld was generated with .apworld version <color=red>{worldVersion}</color>,\n" +
                        $"but you're playing version <color=red>{modVersion}</color> of the Archipelago Randomizer mod.\n<color=red>This is likely to cause game-breaking bugs.</color>");
                } else if (onlyPatchDiffers) {
                    InGameConsole.Add($"This Archipelago multiworld was generated with .apworld version <color=orange>{worldVersion}</color>,\n" +
                        $"but you're playing version <color=orange>{modVersion}</color> of the Archipelago Randomizer mod.\nThis is probably fine, but may cause issues.");
                } else {
                    Log.Info($"Not posting any version warning because world and mod version are both {worldVersion}");
                }
            }
            long? logicDifficulty = SlotData.ContainsKey("logic_difficulty") ? (long)SlotData["logic_difficulty"] : null;
            SkillTree.ApplySlotData(logicDifficulty);
            Shops.ApplySlotData(logicDifficulty);
            ShopUnlocks.ApplySlotData(SlotData);
            PreventWeakenedPrisonState.ApplySlotData(SlotData.ContainsKey("prevent_weakened_prison_state") ? (long)SlotData["prevent_weakened_prison_state"] : null);
        }

        Log.Info($"FinishConnectingToAPServer ConnectionPopups_ApSaveDataRef={ConnectionPopups_ApSaveDataRef} APSession={APSession}");

        if (connected == null) {
            Log.Error($"FinishConnectingToAPServer found connected was null somehow ???");
        } else if (ConnectionPopups_ApSaveDataRef == null) {
            Log.Error($"FinishConnectingToAPServer found ConnectionPopups_ApSaveDataRef was null somehow ???");
        } else {
            connected.SetResult(ConnectionPopups_ApSaveDataRef!);
        }

        // Most of ItemApplications assumes CurrentAPSaveData is non-null, so we must save these steps for *after* the `connected` TCS has been resolved
        ItemApplications.LoadSavedInventory(ConnectionPopups_ApSaveDataRef!);
        ItemApplications.SyncInventoryWithServer();

        if (APSession == null) {
            Log.Error($"Somehow APSession is null during a FinishConnectingToAPServer() call. How did this get called without an AP connection?");
            return;
        }
        APSession.Items.ItemReceived += ItemApplications.ItemReceived;
        APSession.MessageLog.OnMessageReceived += OnAPMessage;

        // ensure that our local locations state matches the AP server by simply re-reporting any "missed" locations
        // it's important to do this after setting up the event handlers above, since a missed location will lead to AP sending us an item and a message
        var locallyCheckedLocationIds = ConnectionPopups_ApSaveDataRef!.locationsChecked
            .Where(kv => kv.Value && Enum.TryParse<Location>(kv.Key, out var loc) && LocationNames.locationToArchipelagoId.ContainsKey(loc))
            .Select(kv => LocationNames.locationToArchipelagoId[Enum.Parse<Location>(kv.Key)]);
        var locationIdsMissedByServer = locallyCheckedLocationIds.Where(id =>
            APSession.Locations.AllLocations.Contains(id) &&
            !APSession.Locations.AllLocationsChecked.Contains(id));
        if (locationIdsMissedByServer.Any()) {
            InGameConsole.Add($"{locationIdsMissedByServer.Count()} locations you've previously checked were not marked as checked on the AP server:\n" +
                string.Join('\n', locationIdsMissedByServer.Select(id => "- " + LocationNames.locationNames[LocationNames.archipelagoIdToLocation[id]])) +
                $"\nSending them to the AP server now.");
            APSession.Locations.CompleteLocationChecks(locationIdsMissedByServer.ToArray());
        }

        OnSessionOpened?.Invoke(APSession);
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
            DrawInputThenConnectPopup();
        } else if (currentPopup == ConnectionPopup.ChangeConnectionInfo) {
            DrawInputThenSavePopup();
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

    private static void DrawInputThenConnectPopup() {
        DrawInputPopup("Connect to AP Server", ConnectButtonClicked);
    }

    private static void DrawInputThenSavePopup() {
        DrawInputPopup("Save Connection Information", SaveInfoButtonClicked);
    }

    private static void DrawInputPopup(string commitText, Action onCommitClicked) {
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

            if (GUILayout.Button(commitText, buttonStyle)) {
                onCommitClicked();
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
                _ = AttemptToConnect();
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
                roomIdMismatchTCS!.SetResult(true); // roomIdMismatchTCS was initialized in ConnectToAPServer()
            }
            if (GUILayout.Button("Cancel", buttonStyle)) {
                roomIdMismatchTCS!.SetResult(false);
            }
            GUILayout.EndHorizontal();
        }, "", windowStyle);
    }

    private static void OnAPMessage(LogMessage message) {
        try {
            var colorizedParts = message.Parts.Select(messagePart => {
                if (messagePart.IsBackgroundColor) return messagePart.Text;

                var c = messagePart.Color;
                var hexColor = $"{c.R:X2}{c.G:X2}{c.B:X2}";
                return $"<color=#{hexColor}>{messagePart.Text}</color>";
            });
            var inGameConsoleMessage = string.Join("", colorizedParts);

            bool showMessageOnScreen = true;
            if (!APRandomizer.Instance.ShowAPMessagesSetting.Value) {
                showMessageOnScreen = false;
            } else {
                if (APRandomizer.Instance.FilterAPMessagesByPlayerSetting.Value && APSession != null) {
                    if (message != null && message is ItemSendLogMessage) {
                        var islm = (message as ItemSendLogMessage);
                        var slot = APSession.ConnectionInfo.Slot;
                        showMessageOnScreen = (slot == islm?.Receiver.Slot || slot == islm?.Sender.Slot);
                    }
                }
            }

            if (showMessageOnScreen) {
                InGameConsole.Add(inGameConsoleMessage);
            } else {
                Log.Info($"Message from Archipelago server:\n{inGameConsoleMessage}");
            }
        } catch (Exception ex) {
            Log.Error($"Caught error in OnAPMessage: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    private static HashSet<string> SocketWarningsAlreadyShown = new();

    private static void APSession_ErrorReceived(Exception e, string message) {
        if (!SocketWarningsAlreadyShown.Contains(message)) {
            SocketWarningsAlreadyShown.Add(message);

            Log.Error(
                $"Received error from APSession.Socket: '{message}'\n" +
                $"(duplicates of this error will be silently ignored)\n" +
                $"\n" +
                $"{e.StackTrace}");

            InGameConsole.Add($"<color=orange>Received an error from APSession.Socket. This means you may have lost connection to the AP server. " +
                $"In order to safely reconnect to the AP server, we recommend quitting and resuming at your earliest convenience.</color>");
        }
    }
}
