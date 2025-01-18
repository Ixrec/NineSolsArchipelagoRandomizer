using HarmonyLib;
using RCGFSM.Animation;
using RCGFSM.Items;
using RCGFSM.Seamless;
using RCGFSM.UIs;
using RCGFSM.Variable;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
public class Patches {

    // Patches are powerful. They can hook into other methods, prevent them from runnning,
    // change parameters and inject custom code.
    // Make sure to use them only when necessary and keep compatibility with other mods in mind.
    // Documentation on how to patch can be found in the harmony docs: https://harmony.pardeike.net/articles/patching.html
    [HarmonyPatch(typeof(Player), nameof(Player.SetStoryWalk))]
    [HarmonyPrefix]
    private static bool PatchStoryWalk(ref float walkModifier) {
        walkModifier = 1.0f;

        return true; // the original method should be executed
    }

    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    [HarmonyPrefix]
    private static void GameLevel_Awake(GameLevel __instance) {
        Log.Info($"GameLevel_Awake {__instance.name}");

        var x = new List<SolvableTagVariable>(__instance.gameObject.GetComponentsInChildren<SolvableTagVariable>(true));
        Log.Info($"GameLevel_Awake {x.Count} / {string.Join("|", x.Select(c => c.name))}");
    }
    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.HandlePlayerKilled))]
    [HarmonyPrefix]
    private static void GameLevel_HandlePlayerKilled(GameLevel __instance) {
        Log.Info($"GameLevel_HandlePlayerKilled {__instance.name}");
    }
    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.JumpToNextTreasureBox))]
    [HarmonyPrefix]
    private static void GameLevel_JumpToNextTreasureBox(GameLevel __instance) {
        Log.Info($"GameLevel_JumpToNextTreasureBox {__instance.name}");
    }
    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.FindAllTreasureBox))]
    [HarmonyPrefix]
    private static void GameLevel_FindAllTreasureBox(GameLevel __instance) {
        Log.Info($"GameLevel_FindAllTreasureBox {__instance.name}");
    }

    [HarmonyPatch(typeof(CutsceneGetItem), nameof(CutsceneGetItem.GetItem))]
    [HarmonyPrefix]
    private static void CutsceneGetItem_GetItem(CutsceneGetItem __instance) {
        Log.Info($"\n!!!\nCutsceneGetItem_GetItem {__instance.bindCutscene.name} - {__instance.item.Title} / {__instance.item.Description} / {__instance.item.Summary}\n!!!\n");
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
    }*/
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
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SavePanel), nameof(SavePanel.ClearCurrentSavePoint))]
    public static void SavePanel_ClearCurrentSavePoint() {
        Log.Info($"SavePanel_ClearCurrentSavePoint");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SavePoint), nameof(SavePoint.UnbindSavePointEvent))]
    public static void SavePoint_UnbindSavePointEvent() {
        Log.Info($"SavePoint_UnbindSavePointEvent");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameCore), nameof(GameCore.TeleportToSavePoint))]
    public static void GameCore_TeleportToSavePoint(GameCore __instance, TeleportPointData point) {
        Log.Info($"GameCore_TeleportToSavePoint {point}");
    }
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
    }*/
    [HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.CreateOrLoadSaveSlotAndPlay))]
    public static void StartMenuLogic_CreateOrLoadSaveSlotAndPlay(StartMenuLogic __instance, int slotIndex, bool SaveExists) {
        Log.Info($"StartMenuLogic_CreateOrLoadSaveSlotAndPlay {slotIndex} {SaveExists}");
    }
    /*[HarmonyPrefix, HarmonyPatch(typeof(StartMenuLogic), nameof(StartMenuLogic.DeleteSlot))]
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

    [HarmonyPrefix, HarmonyPatch(typeof(SetVariableBoolAction), "OnStateEnterImplement")]
    static void SetVariableBoolAction_OnStateEnterImplement(SetVariableBoolAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"SetVariableBoolAction_OnStateEnterImplement called on {goPath}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BossDieFlower), "Start")]
    public static void BossDieFlower_Start(BossDieFlower __instance) {
        Log.Info($"BossDieFlower_Start {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(BossDieFlower), "ShowFlower")]
    public static void BossDieFlower_ShowFlower(BossDieFlower __instance) {
        Log.Info($"BossDieFlower_ShowFlower {LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject)}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(VideoPlayAction), "OnStateEnterImplement")]
    static void VideoPlayAction_OnStateEnterImplement(VideoPlayAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"VideoPlayAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(CutScenePlayAction), "OnStateEnterImplement")]
    static void CutScenePlayAction_OnStateEnterImplement(CutScenePlayAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"CutScenePlayAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseTradeAction), "OnStateEnterImplement")]
    static void MerchandiseTradeAction_OnStateEnterImplement(MerchandiseTradeAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"MerchandiseTradeAction_OnStateEnterImplement called on {goPath}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PreloadGameLevelAction), "OnStateEnterImplement")]
    static void PreloadGameLevelAction_OnStateEnterImplement(PreloadGameLevelAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"PreloadGameLevelAction_OnStateEnterImplement called on {goPath}");
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
    /*[HarmonyPrefix, HarmonyPatch(typeof(AnimatorPlayAction), "OnStateEnterImplement")]
    static void AnimatorPlayAction_OnStateEnterImplement(AnimatorPlayAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"AnimatorPlayAction_OnStateEnterImplement called on {goPath}");
    }*/
    /*[HarmonyPrefix, HarmonyPatch(typeof(ItemViewedAction), "OnStateEnterImplement")]
    static void ItemViewedAction_OnStateEnterImplement(ItemViewedAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ItemViewedAction_OnStateEnterImplement called on {goPath}");
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(AbstractStateAction), nameof(AbstractStateAction.OnActionEnter))]
    static void AbstractStateAction_OnActionEnter(AbstractStateAction __instance) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        var delayActionModifier = AccessTools.FieldRefAccess<AbstractStateAction, DelayActionModifier>("delayActionModifier").Invoke(__instance);
        Log.Info($"AbstractStateAction_OnActionEnter called on {__instance.GetType()} at {goPath} with delayTime of {delayActionModifier?.delayTime} before calling OnStateEnterImplement");
    }*/

    /*[HarmonyPrefix, HarmonyPatch(typeof(AnimationEvents), "InvokeAnimationEvent")]
    static void AnimationEvents_InvokeAnimationEvent(AnimationEvents __instance, AnimationEvents.AnimationEvent e) {
        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"AnimationEvents_InvokeAnimationEvent called on {goPath} with {e}");
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
}
