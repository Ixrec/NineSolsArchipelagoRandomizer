using HarmonyLib;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Jiequan1Fight {
    private static string Jiequan1WaitingFlag = "b8c4a4988c7d1489881c95a5b43f6943ScriptableDataBool"; // A5_S1_約戰
    private static string Jiequan1MapMarkerFlag = "1c5d3bd95edce4801b8e779d43cd220aInterestPointData"; // A5_S1_InterestPointTagCore_約戰

    private static int SealsToUnlockJiequan1 = 3;

    public static void OnItemUpdate(Item item) {
        if (item == Item.MysticNymphScoutMode || ItemApplications.IsSolSeal(item)) {
            bool hasNymph = ItemApplications.ApInventory.ContainsKey(Item.MysticNymphScoutMode) && ItemApplications.ApInventory[Item.MysticNymphScoutMode] > 0;
            var sealCount = ItemApplications.GetSolSealsCount();

            if (hasNymph && sealCount >= SealsToUnlockJiequan1)
                ActuallyTriggerJiequan1Fight();
        }
    }

    public static void ActuallyTriggerJiequan1Fight() {

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        var jiequanMapMarker = (flagDict[Jiequan1MapMarkerFlag] as InterestPointData)!;
        if (jiequanMapMarker.IsSolved) {
            Log.Info("Skipping Jiequan 1 trigger because that event is already 'solved' according to the base game flags");
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
}
