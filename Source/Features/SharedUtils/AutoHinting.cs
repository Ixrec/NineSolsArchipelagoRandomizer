using ArchipelagoRandomizer.Locations;
using System;
using System.Threading.Tasks;

namespace ArchipelagoRandomizer.Features.SharedUtils;

internal class AutoHinting {
    public static void EnsureLocationAutoHinted(Location location) {
        if (APSaveManager.CurrentAPSaveData == null)
            return;
        if (ConnectionAndPopups.APSession == null)
            return;
        if (!LocationNames.locationToArchipelagoId.ContainsKey(location))
            return;
        if (APSaveManager.CurrentAPSaveData.scoutedLocations == null)
            return;
        if (!APSaveManager.CurrentAPSaveData.scoutedLocations.TryGetValue(location, out var scoutedItemInfo))
            return;

        // For now, we auto-hint prog and useful items
        if (!scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement) &&
            !scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude))
            return;

        // Use save data flags to avoid wasting network requests on a hint we've already created
        var flagForLocation = location.ToString() + "_HasBeenHinted";
        var modFlags = APSaveManager.CurrentAPSaveData.otherPersistentModFlags;
        if (modFlags.ContainsKey(flagForLocation) && modFlags[flagForLocation])
            return;

        var locationId = LocationNames.locationToArchipelagoId[location];
        Log.Info($"Creating AP hint for location {location} / {locationId} because it contains a progression and/or useful item");

        // we want to time out relatively quickly if the server happens to be down, but don't
        // block whatever we (and the vanilla game) were doing on waiting for the AP server response
        var _ = Task.Run(() => {
            var hintTask = Task.Run(() => ConnectionAndPopups.APSession.Hints.CreateHints(locationIds: [locationId]));
            if (!hintTask.Wait(TimeSpan.FromSeconds(2))) {
                InGameConsole.Add($"<color=orange>AP server timed out when we tried to generate a hint. Did the connection go down?</color>");
            }
            modFlags[flagForLocation] = true;
        });
    }
}
