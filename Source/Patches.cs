using HarmonyLib;
using RCGFSM.Items;
using RCGFSM.Map;
using RCGFSM.UIs;
using RCGFSM.Variable;
using RCGMaker.Runtime.FSM._4_Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SaveManager;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
public class Patches {
    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    [HarmonyPrefix]
    private static void GameLevel_Awake(GameLevel __instance) {
        Log.Info($"GameLevel_Awake {__instance.name}");

        //var x = new List<SolvableTagVariable>(__instance.gameObject.GetComponentsInChildren<SolvableTagVariable>(true));
        //Log.Info($"GameLevel_Awake {x.Count} / {string.Join("|", x.Select(c => c.name))}");
        var vbs = new List<VariableBool>(__instance.gameObject.GetComponentsInChildren<VariableBool>(true));
        vbs?.ForEach(vb => {
            if (importantIds.Contains(vb?.boolFlag?.FinalSaveID)) {
                Log.Info($"GameLevel_Awake {__instance.name} contains vb: {vb.name} - {vb?.boolFlag?.name} - {vb?.boolFlag?.FinalSaveID}");
            }
        });
        var svbas = new List<SetVariableBoolAction>(__instance.gameObject.GetComponentsInChildren<SetVariableBoolAction>(true));
        svbas?.ForEach(svba => {
            if (importantIds.Contains(svba?.targetFlag?.boolFlag?.FinalSaveID)) {
                Log.Info($"GameLevel_Awake {__instance.name} contains svba: {LocationTriggers.GetFullDisambiguatedPath(svba?.gameObject)} - {svba?.name} - {svba?.targetFlag?.name} - {svba?.targetFlag?.boolFlag?.name} - {svba?.targetFlag?.boolFlag?.FinalSaveID}");
            }
        });
        var fbcs = new List<FlagBoolCondition>(__instance.gameObject.GetComponentsInChildren<FlagBoolCondition>(true));
        fbcs?.ForEach(fbc => {
            if (importantIds.Contains(fbc?.flagBool?.boolFlag?.FinalSaveID)) {
                Log.Info($"GameLevel_Awake {__instance.name} contains fbc: {fbc.name} - {fbc?.flagBool?.name} - {fbc?.flagBool?.boolFlag?.name} - {fbc?.flagBool?.boolFlag?.FinalSaveID}");
            }
        });

        var vis = new List<VariableInt>(__instance.gameObject.GetComponentsInChildren<VariableInt>(true));
        vis?.ForEach(vi => {
            if (importantIds.Contains(vi?.FinalData?.FinalSaveID)) {
                Log.Info($"GameLevel_Awake {__instance.name} contains vi: {vi.name} - {vi?.FinalData?.name} - {vi?.FinalData?.FinalSaveID}");
            }
        });
        var svias = new List<SetVariableIntAction>(__instance.gameObject.GetComponentsInChildren<SetVariableIntAction>(true));
        svias?.ForEach(svia => {
            if (importantIds.Contains(svia?.targetFlag?.FinalData?.FinalSaveID)) {
                Log.Info($"GameLevel_Awake {__instance.name} contains svia: {LocationTriggers.GetFullDisambiguatedPath(svia?.gameObject)} - {svia?.name} - {svia?.targetFlag?.name} - {svia?.targetFlag?.FinalData?.name} - {svia?.targetFlag?.FinalData?.FinalSaveID}");
            }
        });
        var fics = new List<FlagIntCondition>(__instance.gameObject.GetComponentsInChildren<FlagIntCondition>(true));
        fics?.ForEach(fic => {
            if (importantIds.Contains(fic?.condition?.flagInt?.FinalData?.FinalSaveID)) {
                Log.Info($"{__instance.name} contains fbc: {fic.name} - {fic?.condition?.flagInt?.name} - {fic?.condition?.flagInt?.FinalData?.name} - {fic?.condition?.flagInt?.FinalData?.FinalSaveID}");
            }
        });

        Log.Info($"GameLevel_Awake {__instance.name} done");
    }
    [HarmonyPatch(typeof(CutsceneGetItem), nameof(CutsceneGetItem.GetItem))]
    [HarmonyPrefix]
    private static void CutsceneGetItem_GetItem(CutsceneGetItem __instance) {
        Log.Info($"\n!!!\nCutsceneGetItem_GetItem {__instance.bindCutscene.name} - {__instance.item.Title} / {__instance.item.Description} / {__instance.item.Summary}\n!!!\n");
    }

    /*[HarmonyPrefix, HarmonyPatch(typeof(CharacterStat), nameof(CharacterStat.AddModifier))]
    public static void CharacterStat_AddModifier(CharacterStat __instance, StatModifier mod) {
        Log.Info($"CharacterStat_AddModifier {__instance} / {mod}, had {__instance.statModifiers.Count} before, mod.Value={mod.Value}, mod.Type={mod.Type}, mod.Order={mod.Order}");
        //Log.Info($"CharacterStat_AddModifier {new System.Diagnostics.StackTrace()}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(ItemData), nameof(ItemData.FlagInitStart))]
    public static void ItemData_FlagInitStart(ItemData __instance) {
        if (__instance.effectModifiers.Count  > 0)
            Log.Info($"ItemData_FlagInitStart {__instance}, {__instance.Title}, with {__instance.effectModifiers.Count}");
    }*/
    [HarmonyPrefix, HarmonyPatch(typeof(SetStatDataAction), "OnStateEnterImplement")]
    public static void SetStatDataAction_OnStateEnterImplement(SetStatDataAction __instance) {
        Log.Info($"SetStatDataAction_OnStateEnterImplement {__instance}, source {__instance.SourceStatData.name} -> target {__instance.TargetStatData.name}, Value={__instance.SourceStatData.Stat.Value}, statModifiers.Count={__instance.SourceStatData.Stat.statModifiers.Count}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(GameFlagDescriptable), "PlayerPicked")]
    public static void GameFlagDescriptable_PlayerPicked(GameFlagDescriptable __instance) {
        Log.Info($"GameFlagDescriptable_PlayerPicked {__instance}, {__instance.name}, {__instance.Title}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(TeleportToSavePointAction), "OnStateEnterImplement")]
    public static void TeleportToSavePointAction_OnStateEnterImplement(TeleportToSavePointAction __instance) {
        Log.Info($"TeleportToSavePointAction_OnStateEnterImplement {__instance}, {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)}");
    }

    private static string[] importantIds = [
        //"e0b7244f28229054d9ef63438841ad72ScriptableDataBool", // another Chiyou rescue flag???, unsure how much it affects
        //"640eb10597916684cad00ab131593eb4ScriptableDataBool", // a post-PonR flag, unsure how much it affects
        //"bf49eb7e251013c4cb62eca6e586b465ScriptableDataBool", // post-prison Chiyou rescue
        // unclear Lady E flags
        //"fa23d2a4-55aa-4544-a58f-6c1ef92b5b95_6a7e9701c4ef0487683e312ec59d4d60ScriptableDataBool",
        //"803e8a8d-139a-4d22-bb92-5f72b78d3284_6a7e9701c4ef0487683e312ec59d4d60ScriptableDataBool",
        //"c434ef94bad3bfb42b29810f97bde967ScriptableDataBool", // "has been hacked"???, relevant to Shennong reaching FSP on his own, I think this is the CC jumpscare
        // tree?
        //"ed1ff3c012acb7f42854d7811e73374bGameFlagInt"
    ];
    [HarmonyPrefix, HarmonyPatch(typeof(AbstractScriptableData<FlagFieldBool, bool>), "CurrentValue", MethodType.Setter)]
    public static void ScriptableDataBool_set_CurrentValue(AbstractScriptableData<FlagFieldBool, bool> __instance, bool value) {
        //Log.Info($"ASD<FFB,b>_set_CurrentValue {__instance} / {__instance.FinalSaveID} / {__instance.CurrentValue} -> {value}");

        if (importantIds.Contains(__instance.FinalSaveID)) {
            Log.Warning($"!!!");
            Log.Warning($"!!!");
            Log.Warning($"ASD<FFB,b>_set_CurrentValue {__instance} / {__instance.FinalSaveID} / {__instance.CurrentValue} -> {value}");
            Log.Warning($"!!!");
            Log.Warning($"!!!");
        }
    }

    /*[HarmonyPrefix, HarmonyPatch(typeof(PlayerSensor), "OnTriggerEnter2D")]
    public static void PlayerSensor_OnTriggerEnter2D(PlayerSensor __instance, Collider2D other) {
        Log.Info($"PlayerSensor_OnTriggerEnter2D {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)} - Player={other.CompareTag("Player")}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerSensor), "PlayerEnterCheck")]
    public static void PlayerSensor_PlayerEnterCheck(PlayerSensor __instance, Collider2D other) {
        Log.Info($"PlayerSensor_PlayerEnterCheck {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)} - Player={other.CompareTag("Player")}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerSensor), "TriggerPlayerEnterEvent")]
    public static void PlayerSensor_TriggerPlayerEnterEvent(PlayerSensor __instance) {
        var _runTimeIsValidDeprecated = AccessTools.FieldRefAccess<PlayerSensor, bool>("_runTimeIsValidDeprecated").Invoke(__instance);
        Log.Info($"PlayerSensor_TriggerPlayerEnterEvent {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)} - hasEvent={__instance.PlayerEnterEvent != null} - _runTimeIsValidDeprecated={_runTimeIsValidDeprecated}");
    }*/

    [HarmonyPrefix, HarmonyPatch(typeof(Minimap_PinPointAction), "OnStateEnterImplement")]
    public static void Minimap_PinPointAction_OnStateEnterImplement(Minimap_PinPointAction __instance) {
        Log.Info($"Minimap_PinPointAction_OnStateEnterImplement {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)} - {__instance.interestPoints.Count}");
    }

    /*[HarmonyPrefix, HarmonyPatch(typeof(MenuUIPanel), nameof(MenuUIPanel.ShowTab))]
    public static void MenuUIPanel_ShowTab(MenuUIPanel __instance, PlayerInfoPanelType panelType) {
        Log.Info($"MenuUIPanel_ShowTab {panelType}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(MenuUIPanel), nameof(MenuUIPanel.GoToTab))]
    public static void MenuUIPanel_GoToTab(PlayerInfoPanelType tabType) {
        Log.Info($"MenuUIPanel_GoToTab {tabType}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(TabsUI), nameof(TabsUI.GoToTab), [typeof(PlayerInfoPanelType)])]
    public static void TabsUI_GoToTab(TabsUI __instance, PlayerInfoPanelType panelType) {
        Log.Info($"TabsUI_GoToTab {panelType}");

        var items = AccessTools.FieldRefAccess<TabsUI, List<UITabsItem>>("items").Invoke(__instance);
        Log.Info($"TabsUI_GoToTab {items} {string.Join("|", items.Select(i => i.gameObject.name))}");

        Log.Info($"TabsUI_GoToTab {string.Join("|", items.Select(i => i.IsAllValid))}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SelectableNavigationProvider), nameof(SelectableNavigationProvider.Reset))]
    public static void SelectableNavigationProvider_Reset(SelectableNavigationProvider __instance) {
        Log.Info($"SelectableNavigationProvider_Reset");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SelectableNavigationProvider), nameof(SelectableNavigationProvider.AutoNavigateBindForAll))]
    public static void SelectableNavigationProvider_AutoNavigateBindForAll(SelectableNavigationProvider __instance) {
        Log.Info($"SelectableNavigationProvider_AutoNavigateBindForAll");
    }*/
    /*[HarmonyPrefix, HarmonyPatch(typeof(SelectableNavigationProvider), nameof(SelectableNavigationProvider.OnBecomeInteractable))]
    public static void SelectableNavigationProvider_OnBecomeInteractable(SelectableNavigationProvider __instance) {
        Log.Info($"SelectableNavigationProvider_OnBecomeInteractable \"{__instance?.transform?.parent?.name}/{__instance?.name}\"");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SelectableNavigationProvider), nameof(SelectableNavigationProvider.OnBecomeNotInteractable))]
    public static void SelectableNavigationProvider_OnBecomeNotInteractable(SelectableNavigationProvider __instance) {
        Log.Info($"SelectableNavigationProvider_OnBecomeNotInteractable \"{__instance?.transform?.parent?.name}/{__instance?.name}\"");
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(TeleportPointButton), nameof(TeleportPointButton.OnSelect))]
    public static void TeleportPointButton_OnSelect(TeleportPointButton __instance) {
        Log.Info($"TeleportPointButton_OnSelect {__instance?.transform?.parent?.name}/{__instance?.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(UIControlButton), nameof(UIControlButton.OnSubmit))]
    public static void UIControlButton_OnSubmit(UIControlButton __instance) {
        Log.Info($"UIControlButton_OnSubmit {__instance?.transform?.parent?.name}/{__instance?.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(UIControlButton), nameof(UIControlButton.Submit))]
    public static void UIControlButton_Submit(UIControlButton __instance) {
        Log.Info($"UIControlButton_Submit {__instance?.transform?.parent?.name}/{__instance?.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(UIControlButton), nameof(UIControlButton.OnSubmitBlocked))]
    public static void UIControlButton_OnSubmitBlocked(UIControlButton __instance) {
        Log.Info($"UIControlButton_OnSubmitBlocked {__instance?.transform?.parent?.name}/{__instance?.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(UIControlButton), "SubmitImplementation")]
    public static void UIControlButton_SubmitImplementation(UIControlButton __instance) {
        Log.Info($"UIControlButton_SubmitImplementation {__instance?.transform?.parent?.name}/{__instance?.name}");

        if (SingletonBehaviour<GameCore>.IsAvailable()) {
            var gc = SingletonBehaviour<GameCore>.Instance; // implicitly creates the singleton, can break everything if we don't check IsAvailable()
            var spuc = gc?.savePanelUiController;
            var sp = spuc?.CurrentSavePointGameObjectOnScene;
            Log.Info($"UIControlButton_SubmitImplementation {gc} - {spuc} - {sp}");
        }
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(GameCore), nameof(GameCore.TeleportToSavePoint))]
    public static void GameCore_TeleportToSavePoint(GameCore __instance, TeleportPointData point) {
        Log.Info($"GameCore_TeleportToSavePoint {point}");
    }*/
    /*[HarmonyPrefix, HarmonyPatch(typeof(GameCore), nameof(GameCore.ChangeScene))]
    public static void GameCore_ChangeScene(GameCore __instance, SceneConnectionPoint.ChangeSceneData changeSceneData, bool showTip, bool captureLastImage, float delayTime) {
        Log.Info($"GameCore_ChangeScene {changeSceneData} {showTip} {captureLastImage} {delayTime}");
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(ConfirmPanelProvider), nameof(ConfirmPanelProvider.ShowConfirm), [typeof(string), typeof(UnityAction), typeof(string)])]
    public static void ConfirmPanelProvider_ShowConfirm(ConfirmPanelProvider __instance, string message, UnityAction confirmCallBack, string title) {
        Log.Info($"ConfirmPanelProvider_ShowConfirm {message} - {confirmCallBack} - {title}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.CreateNewGame))]
    public static void StartMenuLogic_CreateNewGame(StartMenuLogic __instance, int mode) {
        Log.Info($"StartMenuLogic_CreateNewGame {mode}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.NewGame))]
    public static void StartMenuLogic_NewGame(StartMenuLogic __instance, int slotIndex) {
        Log.Info($"StartMenuLogic_NewGame {slotIndex}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.CreateOrLoadSaveSlotAndPlay))]
    public static void StartMenuLogic_CreateOrLoadSaveSlotAndPlay(StartMenuLogic __instance, int slotIndex, bool SaveExists) {
        Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay {slotIndex} {SaveExists}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.DeleteSlot))]
    public static void StartMenuLogic_DeleteSlot(StartMenuLogic __instance, int i) {
        Log.Info($"StartMenuLogic_DeleteSlot {i}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), "Start")]
    public static void StartMenuLogic_Start(StartMenuLogic __instance) {
        Log.Info($"StartMenuLogic_Start");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.UpdateSaveSlots))]
    public static void StartMenuLogic_UpdateSaveSlots(StartMenuLogic __instance) {
        Log.Info($"StartMenuLogic_UpdateSaveSlots");
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(LootSpawner), "SpawnWithPercentage")]
    static void LootSpawner_SpawnWithPercentage(LootSpawner __instance, float percentage, Vector3 dropPosition) {
        if (percentage < 1) {
            Log.Info($"LootSpawner_SpawnWithPercentage called on {__instance.name} with percentage={percentage} and dropPosition={dropPosition}");
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            Log.Info($"LootSpawner_SpawnWithPercentage called on GO: {goPath}");
        }
    }*/

    /*static public Dictionary<string, VariableBool> trackedFlags = new Dictionary<string, VariableBool>();

    [HarmonyPostfix, HarmonyPatch(typeof(VariableBool), "ScriptableData", MethodType.Getter)]
    static void VariableBool_get_ScriptableData(VariableBool __instance, ScriptableDataBool __result) {
        if (__result == null)
            return;

        var finalSaveID = AccessTools.FieldRefAccess<GameFlagBase, string>("_finalSaveID").Invoke(__result);

        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);

        if (trackedFlags.ContainsKey(finalSaveID) && trackedFlags[finalSaveID] == __instance)
            return;

        if (trackedFlags.ContainsKey(finalSaveID)) {
            // this happens a lot, probably no actual value in trying to record go paths long term? most of them are of no interest anyway
            //Log.Info($"VariableBool_get_ScriptableData found a second??? VBool for\n{finalSaveID}\nwhich was:\n{goPath}");
            return;
        }

        //Log.Info($"VariableBool_get_ScriptableData adding entry for\n{finalSaveID}\nto\n{goPath}");
        trackedFlags[finalSaveID] = __instance;
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    static void GeneralState_OnStateEnter(GeneralState __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance?.gameObject);
        if (goPath.Contains("伏羲嚇人"))
            Log.Info($"GeneralState_OnStateEnter: {__instance.name} from {__instance.Context.LastState} using {__instance.Context.LastTransition} \ngoPath={goPath}");
    }*/

    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static void SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        var id = __instance?.targetFlag?.boolFlag?.FinalSaveID;
        if (id != null && id != "") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance?.gameObject);
            Log.Info($"SetVariableBoolAction_OnStateEnterImplement: flag={__instance?.targetFlag?.boolFlag?.FinalSaveID}, old value={__instance?.targetFlag?.FlagValue}, new value={__instance?.TargetValue}, goPath={goPath}");
        }
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableIntAction), "OnStateEnterImplement")]
    static void SetVariableIntAction_OnStateEnterImplement(SetVariableIntAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        //Log.Info($"SetVariableIntAction_OnStateEnterImplement: flag={__instance.targetFlag.intFlag.FinalSaveID}, old value={__instance.targetFlag.Value}, new value={__instance.TargetValue}, goPath={goPath}");
        Log.Info($"SetVariableIntAction_OnStateEnterImplement: goPath={goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableFloatAction), "OnStateEnterImplement")]
    static void SetVariableFloatAction_OnStateEnterImplement(SetVariableFloatAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        //Log.Info($"SetVariableFloatAction_OnStateEnterImplement: flag={__instance.targetFlag}, old value={__instance.targetFlag.Value}, new value={__instance.TargetValue}, goPath={goPath}");
        Log.Info($"SetVariableFloatAction_OnStateEnterImplement: goPath={goPath}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(VideoPlayAction), "OnStateEnterImplement")]
    static void VideoPlayAction_OnStateEnterImplement(VideoPlayAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"VideoPlayAction_OnStateEnterImplement called on {goPath}");
    }
    /*[HarmonyPrefix, HarmonyPatch(typeof(CutScenePlayAction), "OnStateEnterImplement")]
    static void CutScenePlayAction_OnStateEnterImplement(CutScenePlayAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"CutScenePlayAction_OnStateEnterImplement called on {goPath}");
    }*/
    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseTradeAction), "OnStateEnterImplement")]
    static void MerchandiseTradeAction_OnStateEnterImplement(MerchandiseTradeAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"MerchandiseTradeAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseData), "Trade")]
    static void MerchandiseData_Trade(MerchandiseData __instance) {
        Log.Info($"MerchandiseData_Trade called on {__instance.item}, {__instance.item?.Title}, out of {__instance.numLeftToBuy}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(GiveItemAction), "OnStateEnterImplement")]
    static void GiveItemAction_OnStateEnterImplement(GiveItemAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"GiveItemAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(ShowPanelAction), "OnStateEnterImplement")]
    static void ShowPanelAction_OnStateEnterImplement(ShowPanelAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ShowPanelAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(HidePanelAction), "OnStateEnterImplement")]
    static void HidePanelAction_OnStateEnterImplement(HidePanelAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"HidePanelAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(ItemViewedAction), "OnStateEnterImplement")]
    static void ItemViewedAction_OnStateEnterImplement(ItemViewedAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ItemViewedAction_OnStateEnterImplement called on {goPath}");
    }

    /*[HarmonyPrefix, HarmonyPatch(typeof(AbstractStateAction), nameof(AbstractStateAction.OnActionEnter))]
    static void AbstractStateAction_OnActionEnter(AbstractStateAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        var delayActionModifier = AccessTools.FieldRefAccess<AbstractStateAction, DelayActionModifier>("delayActionModifier").Invoke(__instance);
        Log.Info($"AbstractStateAction_OnActionEnter called on {__instance.GetType()} at {goPath} with delayTime of {delayActionModifier?.delayTime} before calling OnStateEnterImplement");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AnimatorPlayAction), "OnStateEnterImplement")]
    static void AnimatorPlayAction_OnStateEnterImplement(AnimatorPlayAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"AnimatorPlayAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(AnimationEvents), "InvokeAnimationEvent")]
    static void AnimationEvents_InvokeAnimationEvent(AnimationEvents __instance, AnimationEvents.AnimationEvent e) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"AnimationEvents_InvokeAnimationEvent called on {goPath} with {e}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(Animator), "Play", [typeof(string)])]
    static void Animator_PlayString(Animator __instance, string stateName) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"Animator_PlayString called with {stateName} on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(Animator), "Play", [typeof(int)])]
    static void Animator_PlayInt(Animator __instance, int stateNameHash) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"Animator_PlayInt called with {stateNameHash} on {goPath}");
    }*/

    // !!! somehow these two patches completely break item popup display (images are white rectangles, it's usually scroll or hibiscus, often in German, etc)
    /*[HarmonyPrefix, HarmonyPatch(typeof(ItemDescriptionProvider), "UpdateView")]
    static void ItemDescriptionProvider_UpdateView(ItemDescriptionProvider __instance, IDescriptable data) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ItemDescriptionProvider_UpdateView called on {goPath} with {data.Title}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(ItemDescriptionProvider), "UpdateViewWith")]
    static void ItemDescriptionProvider_UpdateViewWith(ItemDescriptionProvider __instance, GameObject gameObject, IDescriptable data) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ItemDescriptionProvider_UpdateViewWith called on {goPath} with {gameObject.name} and {data.Title}");
    }*/
    [HarmonyPrefix, HarmonyPatch(typeof(SaveManager), "AutoSave")]
    static void SaveManager_AutoSave_Prefix(SaveManager __instance, SaveSceneScheme saveSceneType, bool forceShowIcon, Transform overridePos) {
        Log.Info($"SaveManager_AutoSave_Prefix called with saveSceneType={saveSceneType}, forceShowIcon={forceShowIcon}, overridePos={overridePos}");
        //Log.Info($"SaveManager_AutoSave_Prefix call stack: {new System.Diagnostics.StackTrace()}");
    }
    [HarmonyPostfix, HarmonyPatch(typeof(SaveManager), "AutoSave")]
    static void SaveManager_AutoSave_Postfix(SaveManager __instance, SaveSceneScheme saveSceneType, bool forceShowIcon, Transform overridePos) {
        Log.Info($"SaveManager_AutoSave_Postfix called with saveSceneType={saveSceneType}, forceShowIcon={forceShowIcon}, overridePos={overridePos}");
    }
}
