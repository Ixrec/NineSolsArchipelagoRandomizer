using HarmonyLib;
using RCGFSM.Items;
using RCGFSM.Map;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LadyESoulscapeEntrance {
    private static string LadyESoulscapeMapMarkerFlag = "a693b457c9f5a4bc3b7fa7f2a96e5b37InterestPointData"; // A7_S1_BrainRoom_花入口 FSM Object_InterestPoint
    private static string LadyESoulscapeEntranceOpenFlag = "bc24bdac2e273294b9b52f4c82fe0bd3ScriptableDataBool"; // A7_S1_BrainRoom_(Variable) VariableBool_異常訊號標記
    private static string LadyEBossFightEntranceOpenFlag = "f54ffa939efda244f9193ffd5379ee99ScriptableDataBool"; // A7_S1_BrainRoom_(Variable) VariableBool_被蝴蝶趕出來

    /* Related game flags include:
     * - "[Variable] VariableBool_蝴蝶BossKilled" / ScriptableDataBool "A7_S5_Scenario_Boss_(Variable)BossKilled_Butterfly" / FinalSaveID "6944565dad46a40c2abc1e23f2a43b9eScriptableDataBool"
     *      tracks defeating her
     *
     * these two are set after a hardcoded delay when you enter the brain room before/after boss states, I think they're just impl details for a background transition?
     * - [Variable] 階段三播過背景轉換動畫 / A7_S1_BrainRoom_Remake_[Variable] 階段三播過背景轉換動畫c5fbfe25-44fa-461a-8a74-35156c9758d3 / c5fbfe25-44fa-461a-8a74-35156c9758d3_d5d14d9cbe2ff4247b9d7d1b58ae339bScriptableDataBool
     * - [Variable] 階段四播過背景轉換動畫 / A7_S1_BrainRoom_Remake_[Variable] 階段四播過背景轉換動畫754a0ec0-0e42-441e-b24f-374dc1c51570 / 754a0ec0-0e42-441e-b24f-374dc1c51570_d5d14d9cbe2ff4247b9d7d1b58ae339bScriptableDataBool
     */

    private static int SealsToUnlockLadyE = 4;

    public static void OnItemUpdate(Item item) {
        if (ItemApplications.IsSolSeal(item)) {
            var sealCount = ItemApplications.GetSolSealsCount();

            if (sealCount >= SealsToUnlockLadyE)
                ActuallyTriggerLadyESoulscape();
        }
    }

    public static void ActuallyTriggerLadyESoulscape() {
        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        var ladyEMapMarker = (flagDict[LadyESoulscapeMapMarkerFlag] as InterestPointData)!;
        if (ladyEMapMarker.IsSolved) {
            Log.Info("Skipping Lady Ethereal Soulscape trigger because that event is already 'solved' according to the base game flags");
            return;
        }

        bool skipSoulscape = (
            ConnectionAndPopups.SlotData != null &&
            ConnectionAndPopups.SlotData.ContainsKey("skip_soulscape_platforming") &&
            (long)ConnectionAndPopups.SlotData["skip_soulscape_platforming"] > 0
        );
        if (skipSoulscape) {
            Log.Info("skip_soulscape_platforming is true, so unlocking the Lady Ethereal boss fight");
            (flagDict[LadyEBossFightEntranceOpenFlag] as ScriptableDataBool)!.CurrentValue = true;
        } else {
            Log.Info("skip_soulscape_platforming is false or N/A, so unlocking the Lady Ethereal soulscape entrance");
            (flagDict[LadyESoulscapeEntranceOpenFlag] as ScriptableDataBool)!.CurrentValue = true;
        }

        Log.Info("Triggering the Lady Ethereal Soulscape entrance map marker and notification");

        ladyEMapMarker.NPCPinned.CurrentValue = true;

        SingletonBehaviour<GameCore>.Instance.notificationUI.ShowNotification(
            new I2.Loc.LocalizedString("MinimapTitle/Minimap_UpdateMessage"),
            null,
            PlayerInfoPanelType.WorldMap,
            () => {
                ladyEMapMarker.NPCPinnedAnimationPlayed.CurrentValue = true;
            }
        );
    }

    // If this is the part where Lady E gives you back your nymph, but we still
    // don't have the AP nymph item, then don't let Yi have it yet.
    // This won't be an issue until alternate first node, since nymph is required for left Central Hall.
    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        if (__instance.name == "[Action] EnableButterfly") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A7_S1/Room/Prefab/A7_S1_三階段FSM/--[States]/FSM/[State] Phase4_腦室_蝴蝶死後/[Action] EnableButterfly") {
                if (ItemApplications.ApInventory[Item.MysticNymphScoutMode] == 0) {
                    Log.Info($"TriggerLadyESoulscape::PickItemAction_OnStateEnterImplement preventing the end of fight nymph return from giving Yi a nymph before the AP item is found");
                    return false;
                }
            }
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Minimap_PinPointAction), "OnStateEnterImplement")]
    static bool Minimap_PinPointAction_OnStateEnterImplement(Minimap_PinPointAction __instance) {
        if (__instance.name == "[Action] Minimap_PinPointAction") {
            var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
            if (goPath == "A7_S1/Room/Prefab/PhoneCallFSM_家裡古樹暴走/--[States]/FSM/[State] End/[Action] Minimap_PinPointAction") {
                Log.Info($"TriggerLadyESoulscape::Minimap_PinPointAction_OnStateEnterImplement preventing the game from showing an 'event point' marker on the map for" +
                    $" the post-Lady E anomalous FSP node and Lear talk. Since rando removes all of the Limitless Realm segments, you'd never be able to get rid of this marker.");
                return false;
            }
        }
        return true;
    }

}
