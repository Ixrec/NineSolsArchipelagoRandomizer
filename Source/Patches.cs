using HarmonyLib;
using NineSolsAPI;
using RCGFSM.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        Log.Info($"CutsceneGetItem_GetItem {__instance.bindCutscene.name} - {__instance.item.Title} / {__instance.item.Description} / {__instance.item.Summary}");
    }

    [HarmonyPatch(typeof(SkippableManager), nameof(SkippableManager.RegisterSkippable))]
    [HarmonyPrefix]
    private static void SkippableManager_RegisterSkippable(SkippableManager __instance, ISkippable skippable) {
        Log.Info($"SkippableManager_RegisterSkippable is MonoBehaviour = {(skippable is MonoBehaviour)}");
        Log.Info($"SkippableManager_RegisterSkippable {(skippable as MonoBehaviour)?.name} - {skippable.GetType()} - {(skippable as MonoBehaviour)?.transform?.parent?.name}");
    }
    [HarmonyPatch(typeof(SkippableManager), nameof(SkippableManager.UnRegisterSkippable))]
    [HarmonyPrefix]
    private static void SkippableManager_UnRegisterSkippable(SkippableManager __instance, ISkippable skippable) {
        Log.Info($"SkippableManager_UnRegisterSkippable is MonoBehaviour = {(skippable is MonoBehaviour)}");
        Log.Info($"SkippableManager_UnRegisterSkippable {(skippable as MonoBehaviour)?.name} - {skippable.GetType()} - {(skippable as MonoBehaviour)?.transform?.parent?.name}");
    }
    [HarmonyPatch(typeof(SkippableManager), nameof(SkippableManager.ClearReference))]
    [HarmonyPrefix]
    private static void SkippableManager_ClearReference(SkippableManager __instance) {
        Log.Info($"SkippableManager_ClearReference");
    }
    [HarmonyPatch(typeof(SkippableManager), nameof(SkippableManager.TrySkip))]
    [HarmonyPrefix]
    private static void SkippableManager_TrySkip(SkippableManager __instance) {
        Log.Info($"SkippableManager_TrySkip");
    }

    [HarmonyPatch(typeof(SimpleCutsceneManager), nameof(SimpleCutsceneManager.Play), [])]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_Play(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_Play 0-ary {__instance.name}");
    }
    [HarmonyPatch(typeof(SimpleCutsceneManager), nameof(SimpleCutsceneManager.Play), [typeof(Action)])]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_Play(SimpleCutsceneManager __instance, Action callback) {
        Log.Info($"SimpleCutsceneManager_Play 1-ary {__instance.name}");
    }
    [HarmonyPatch(typeof(SimpleCutsceneManager), "End")]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_End(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_End {__instance.name}");
    }
    [HarmonyPatch(typeof(SimpleCutsceneManager), nameof(SimpleCutsceneManager.Pause))]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_Pause(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_Pause {__instance.name}");
    }
    [HarmonyPatch(typeof(SimpleCutsceneManager), nameof(SimpleCutsceneManager.Resume))]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_Resume(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_Resume {__instance.name}");
        ToastManager.Toast($"Press ??? to Skip This Cutscene ({__instance.name})");
        APRandomizer.cutScene = __instance;
    }
    [HarmonyPatch(typeof(SimpleCutsceneManager), "SetPauseLoop")]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_SetPauseLoop(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_SetPauseLoop {__instance.name}");
    }
    [HarmonyPatch(typeof(SimpleCutsceneManager), "EnterLevelAwake")]
    [HarmonyPrefix]
    private static void SimpleCutsceneManager_EnterLevelAwake(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_EnterLevelAwake {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SimpleCutsceneManager), "PlayWithoutLockControl")]
    private static void SimpleCutsceneManager_PlayWithoutLockControl(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_PlayWithoutLockControl {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SimpleCutsceneManager), "PlayAnimation")]
    private static void SimpleCutsceneManager_PlayAnimation(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_PlayAnimation {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SimpleCutsceneManager), "BeforePlay")]
    private static void SimpleCutsceneManager_BeforePlay(SimpleCutsceneManager __instance) {
        Log.Info($"SimpleCutsceneManager_BeforePlay {__instance.name}");
        ToastManager.Toast($"Press ??? to Skip This Cutscene ({__instance.name})");
        APRandomizer.cutScene = __instance;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ICutScene), "PlayCutscene")]
    private static void ICutScene_PlayCutscene(ICutScene __instance) {
        Log.Info($"ICutScene_PlayCutscene {__instance.name}");
    }

    [HarmonyPatch(typeof(BubbleDialogueController), "ShowNode")]
    [HarmonyPrefix]
    private static void BubbleDialogueController_ShowNode(BubbleDialogueController __instance) {
        Log.Info($"BubbleDialogueController_ShowNode {__instance.name}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DialoguePlayer), "StartDialogue")]
    private static void DialoguePlayer_StartDialogue(DialoguePlayer __instance) {
        Log.Info($"DialoguePlayer_StartDialogue {__instance.name}");
        ToastManager.Toast($"Press ??? to Skip This Dialogue");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(DialoguePlayer), "TextProgress", [typeof(bool)])]
    private static void DialoguePlayer_TextProgress(DialoguePlayer __instance, bool BubbleChanged) {
        Log.Info($"DialoguePlayer_TextProgress {__instance.name} {BubbleChanged}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(DialoguePlayer), "UpdateCharacter")]
    private static void DialoguePlayer_UpdateCharacter(DialoguePlayer __instance) {
        Log.Info($"DialoguePlayer_UpdateCharacter {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(DialoguePlayer), "EndDialogue")]
    private static void DialoguePlayer_EndDialogue(DialoguePlayer __instance) {
        Log.Info($"DialoguePlayer_EndDialogue {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(DialoguePlayer), "PlayVoice")]
    private static void DialoguePlayer_PlayVoice(DialoguePlayer __instance) {
        Log.Info($"DialoguePlayer_PlayVoice {__instance.name}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DialogueBubble), "ShowBubble")]
    private static void DialogueBubble_ShowBubble(DialogueBubble __instance) {
        Log.Info($"DialogueBubble_ShowBubble {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(DialogueBubble), "ProgressShowText")]
    private static void DialogueBubble_ProgressShowText(DialogueBubble __instance) {
        Log.Info($"DialogueBubble_ProgressShowText {__instance.name}");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(DialogueBubble), "EndProgressText")]
    private static void DialogueBubble_EndProgressText(DialogueBubble __instance) {
        Log.Info($"DialogueBubble_EndProgressText {__instance.name}");
    }

    [HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    [HarmonyPrefix]
    private static void PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        Log.Info($"PickItemAction_OnStateEnterImplement {__instance.pickItemData.name} - {__instance.pickItemData.Summary} / {__instance.pickItemData.Description} / {__instance.pickItemData.Title}");
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
    [HarmonyPrefix, HarmonyPatch(typeof(SelectableNavigationProvider), nameof(SelectableNavigationProvider.OnBecomeInteractable))]
    public static void SelectableNavigationProvider_OnBecomeInteractable(SelectableNavigationProvider __instance) {
        Log.Info($"SelectableNavigationProvider_OnBecomeInteractable \"{__instance?.transform?.parent?.name}/{__instance?.name}\"");
    }
    [HarmonyPrefix, HarmonyPatch(typeof(SelectableNavigationProvider), nameof(SelectableNavigationProvider.OnBecomeNotInteractable))]
    public static void SelectableNavigationProvider_OnBecomeNotInteractable(SelectableNavigationProvider __instance) {
        Log.Info($"SelectableNavigationProvider_OnBecomeNotInteractable \"{__instance?.transform?.parent?.name}/{__instance?.name}\"");
    }

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

}
