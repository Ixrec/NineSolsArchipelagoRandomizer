namespace ArchipelagoRandomizer; 

internal class TriggerLadyESoulscape {
    private static string LadyESoulscapeEntranceOpenFlag = "bc24bdac2e273294b9b52f4c82fe0bd3ScriptableDataBool"; // A7_S1_BrainRoom_(Variable) VariableBool_異常訊號標記
    private static string LadyESoulscapeMapMarkerFlag = "a693b457c9f5a4bc3b7fa7f2a96e5b37InterestPointData"; // A7_S1_BrainRoom_花入口 FSM Object_InterestPoint

    // TODO: actually call this once we have item receiving
    // TODO: check if you have 4 sol seals
    public static void ActuallyTriggerLadyESoulscape() {

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        var ladyEMapMarker = (flagDict[LadyESoulscapeMapMarkerFlag] as InterestPointData)!;
        if (ladyEMapMarker.IsSolved) {
            // TODO: log
            return;
        }

        (flagDict[LadyESoulscapeEntranceOpenFlag] as ScriptableDataBool)!.CurrentValue = true;

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
}
