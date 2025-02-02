namespace ArchipelagoRandomizer; 

internal class TriggerJiequan1 {
    private static string Jiequan1WaitingFlag = "b8c4a4988c7d1489881c95a5b43f6943ScriptableDataBool"; // A5_S1_約戰
    private static string Jiequan1MapMarkerFlag = "1c5d3bd95edce4801b8e779d43cd220aInterestPointData"; // A5_S1_InterestPointTagCore_約戰

    // TODO: actually call this once we have item receiving
    // TODO: check if 3 you have sol seals
    public static void ActuallyTriggerJiequan1Fight() {

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        var jiequanMapMarker = (flagDict[Jiequan1MapMarkerFlag] as InterestPointData)!;
        if (jiequanMapMarker.IsSolved) {
            // TODO: log
            return;
        }

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
}
