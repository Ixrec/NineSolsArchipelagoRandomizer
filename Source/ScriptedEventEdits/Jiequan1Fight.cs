using Com.LuisPedroFonseca.ProCamera2D;
using HarmonyLib;
using I2.Loc;
using RCGFSM.Animation;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Jiequan1Fight {
    private static string Jiequan1WaitingFlag = "b8c4a4988c7d1489881c95a5b43f6943ScriptableDataBool"; // A5_S1_約戰
    private static string Jiequan1MapMarkerFlag = "1c5d3bd95edce4801b8e779d43cd220aInterestPointData"; // A5_S1_InterestPointTagCore_約戰

    private static int SealsToUnlockJiequan1 = 3;

    // See comments in LadyESoulscapeEntrance.cs. I haven't reproduced the crash with Jiequan 1, but let's apply the workaround here too.
    private static bool TriggerOnNextUpdate = false;

    public static void OnItemUpdate(Item item) {
        if (item == Item.MysticNymphScoutMode || ItemApplications.IsSolSeal(item)) {
            bool hasNymph = ItemApplications.ApInventory.ContainsKey(Item.MysticNymphScoutMode) && ItemApplications.ApInventory[Item.MysticNymphScoutMode] > 0;
            var sealCount = ItemApplications.GetSolSealsCount();

            if (hasNymph && sealCount >= SealsToUnlockJiequan1)
                TriggerOnNextUpdate = true;
        }
    }

    public static void Update() {
        if (TriggerOnNextUpdate) {
            TriggerOnNextUpdate = false;
            ActuallyTriggerJiequan1Fight();
        }
    }

    public static void ActuallyTriggerJiequan1Fight() {

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        var jiequanMapMarker = (flagDict[Jiequan1MapMarkerFlag] as InterestPointData)!;
        if (jiequanMapMarker.IsSolved) {
            Log.Info("Skipping Jiequan 1 trigger because that event is already 'solved' according to the base game flags");
            return;
        } else if (jiequanMapMarker.NPCPinned.CurrentValue) {
            Log.Info("Skipping Jiequan 1 trigger because that event is already 'pinned' on the map according to the base game flags");
            return;
        }

        Log.Info("Triggering the Jiequan 1 fight map marker and notification");

        (flagDict[Jiequan1WaitingFlag] as ScriptableDataBool)!.CurrentValue = true;

        jiequanMapMarker.NPCPinned.CurrentValue = true;

        SingletonBehaviour<GameCore>.Instance.notificationUI.ShowNotification(
            new I2.Loc.LocalizedString("MinimapTitle/Minimap_UpdateMessage"),
            null,
            PlayerInfoPanelType.WorldMap,
            () => {
                jiequanMapMarker.NPCPinnedAnimationPlayed.CurrentValue = true;
            }
        );
    }

    // We also need to prevent the vanilla triggers from activating Jiequan 1 before you meet whatever condition the randomizer wants
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        // In the vanilla game, the Jiequan call that enables the Jiequan 1 fight in Factory (Great Hall) can be triggered from 3 places:
        // Kuafu: "A2_S5_ BossHorseman_GameLevel/Room/SimpleCutSceneFSM_殺死三隻王後接到 截全約戰電話 Variant/--[States]/FSM/[State] WaitForTrigger"
        // Goumang: "A3_S5_BossGouMang_GameLevel/Room/SimpleCutSceneFSM_殺死三隻王後接到 截全約戰電話 Variant/--[States]/FSM/[State] WaitForTrigger"
        // Yanlao: "A0_S6/Room/SimpleCutSceneFSM_殺死三隻王後接到 截全約戰電話 Variant/--[States]/FSM/[State] WaitForTrigger"

        // Since "SimpleCutSceneFSM_殺死三隻王後接到 截全約戰電話 Variant" is such a unique name, for once I don't think we need full path comparisons here.
        if (
            __instance.name == "[State] WaitForTrigger" &&
            __instance.transform.parent?.parent?.parent?.name == "SimpleCutSceneFSM_殺死三隻王後接到 截全約戰電話 Variant"
        ) {
            Log.Info($"TriggerJiequan1::GeneralState_OnStateEnter disabling the vanilla Jiequan call trigger after the vital sanctum");
            // Fortunately, all 3 of these FSMs also have a consistent "[State] Disabled" we can route to instead
            var disabledState = __instance.transform.parent.Find("[State] Disabled");
            // For some reason OnStateEnter() doesn't work here, we need ForceEnterState()
            AccessTools.Method(typeof(GeneralState), "ForceEnterState", []).Invoke(disabledState?.GetComponent<GeneralState>(), []);
        }
    }

    /*
     * The patches above deal with getting the Jiequan 1 fight to be added to the game world.
     * 
     * The patches below deal with the fight itself, because we want it to be a choice in rando, rather
     * than forcing you to do half of Prison right away if you happen to already have the items
     */

    // Must be *post*fix, or the vanilla impl will undo a lot of this
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter_FactoryFightPatch(GeneralState __instance) {
        if (__instance.name == "[State] WaitFirstTimeContact") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A5_S1/Room/FlashKill Binding/werw/--[States]/FSM/[State] WaitFirstTimeContact") {
                Log.Info($"GeneralState_OnStateEnter_FactoryFightPatch reconfiguring the Jiequan 1 fight so the player has to opt-in to doing it");

                // By invoking only this animation component instead of the whole state, we can make Jiequan visually exist,
                // but just stand there menacingly without ever triggering his cutscenes or combat behavior.
                var apa = GameObject.Find("A5_S1/Room/FlashKill Binding/werw/--[States]/FSM/[State] PlayingFirstTimeCutScene L").GetComponent<AnimatorPlayAction>();
                AccessTools.Method(typeof(AnimatorPlayAction), "OnStateEnterImplement", []).Invoke(apa, []);

                // Move the unused copy of the 'Open the transmutation crucible?' button prompt in front of Jiequan.
                // In its default central position, the player may hit by the hammers (which leads to weird bugs) or not notice it at all.
                var t = GameObject.Find("A5_S1/Room/FlashKill Binding/werw/FSM Animator/LogicRoot/Interactable_Merchandise_AskRelease結權/General FSM Object/FSM Animator/LogicRoot/Interactable_MerchandiseVer").transform;
                var lp = t.localPosition;
                lp.x = -110;
                t.localPosition = lp;
            }
        }
    }
    // Disable the camera lock that "[State] PlayingFirstTimeCutScene L"/"R" normally implies, so you can still play all of F(GH) normally
    [HarmonyPrefix, HarmonyPatch(typeof(ProCamera2DTriggerBoundaries), "Init")]
    private static void ProCamera2DTriggerBoundaries_Init(ProCamera2DTriggerBoundaries __instance) {
        if (__instance.transform.parent?.parent?.parent?.parent?.name == "werw") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A5_S1/Room/FlashKill Binding/werw/FSM Animator/LogicRoot/ProCamera2DTriggerBoundaries/trigger boundaires") {
                Log.Info($"ProCamera2DTriggerBoundaries_Init {goPath} skipping initialization by setting .inited to true, preventing the Jiequan 1 camera lock from kicking in before you start the fight");
                AccessTools.FieldRefAccess<ProCamera2DTriggerBoundaries, bool>("inited").Invoke(__instance) = true;
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(LocalizationManager), "GetTranslation")]
    static bool LocalizationManager_GetTranslation(string Term, ref string __result, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters = true) {
        //Log.Info($"LocalizationManager_GetTranslation: {Term}");
        // This term is used for both the "real" prompt at Jiequan 2 *and* the unused copy at Jiequan 1,
        // so we have to check which scene we're in before editing it.
        if (Term == "A5_S5/ConfirmReleaseJieChuan" && GameCore.IsAvailable() && GameCore.Instance.CurrentSceneName == "A5_S1_CastleHub_remake") {
            Log.Info($"Editing the unused copy of 'Open the transmutation crucible?' to an explanation of the rando Prison sequence.");
            __result = """
                Start the Jiequan 1 fight and Prison sequence?

                Once this begins, you <color=#ff5959>cannot teleport</color> freely until you've activated the Prison root node.
                """;
            return false;
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ConfirmPanelProvider), "ConfirmButtonClicked")]
    static void ConfirmPanelProvider_ConfirmButtonClicked(ConfirmPanelProvider __instance) {
        if (__instance.transform.parent?.parent?.parent?.parent?.name != "Interactable_Merchandise_AskRelease結權")
            return;

        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        if (goPath == "A5_S1/Room/FlashKill Binding/werw/FSM Animator/LogicRoot/Interactable_Merchandise_AskRelease結權/General FSM Object/FSM Animator/LogicRoot/Interactable_MerchandiseVer") {
            Log.Info($"Forcing Jiequan 1 back into his usual 'Yi just walked in the room' cutscene trigger state");

            GameObject.Find("A5_S1/Room/FlashKill Binding/werw/--[States]/FSM/[State] WaitFirstTimeContact/[Transition] PlayerEnter R->TimeLine CutScene R")
                .GetComponent<RCGEventReceiveTransition>().TransitionCheck();
        }
    }
}
