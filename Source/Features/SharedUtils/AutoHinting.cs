using ArchipelagoRandomizer.Locations;

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
        Log.Info($"Creating AP hint for shop location {location} / {locationId} because it contains a progression and/or useful item");
        ConnectionAndPopups.APSession.Hints.CreateHints(locationIds: [locationId]);

        modFlags[flagForLocation] = true;
    }
}
