using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.Locations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchipelagoRandomizer.Features.SharedUtils;

internal class AutoHinting {
    private static ConcurrentBag<Location> LocationsToHint = new();
    private readonly static object locationsToHintLock = new object();

    public static void Update() {
        if (LocationsToHint.IsEmpty)
            return;

        if (APSaveManager.CurrentAPSaveData == null)
            return;
        if (ConnectionAndPopups.APSession == null)
            return;
        var modFlags = APSaveManager.CurrentAPSaveData.otherPersistentModFlags;

        try {
            HashSet<Location> locations = new();
            HashSet<string> hintFlags = new();
            lock (locationsToHintLock) {
                while (LocationsToHint.TryTake(out var location)) {
                    // Use save data flags to avoid wasting network requests on a hint we've already created
                    var flagForLocation = location.ToString() + "_HasBeenHinted";
                    if (modFlags.ContainsKey(flagForLocation) && modFlags[flagForLocation])
                        continue;

                    locations.Add(location);
                    hintFlags.Add(flagForLocation);
                }
            }

            if (locations.Count == 0)
                return;

            var locationIds = locations.Select(location => LocationNames.locationToArchipelagoId[location]).ToArray();

            // we want to time out relatively quickly if the server happens to be down, but don't
            // block whatever we (and the vanilla game) were doing on waiting for the AP server response
            var _ = Task.Run(() => {
                Log.Info($"AutoHinting::Update() subtask asking the AP server to generate {locationIds.Length} hint(s).");
                var hintTask = Task.Run(() => ConnectionAndPopups.APSession.Hints.CreateHints(locationIds: locationIds));
                if (!hintTask.Wait(TimeSpan.FromSeconds(2))) {
                    InGameConsole.Add($"<color=orange>AP server timed out when we tried to generate {locationIds.Length} hint(s). Did the connection go down?</color>");
                }
                foreach (var flag in hintFlags)
                    modFlags[flag] = true;
                APSaveManager.ScheduleWriteToCurrentSaveFile();
            });
        } catch (Exception ex) {
            Log.Error($"Caught error in AutoHinting::Update(): '{ex.Message}'\n{ex.StackTrace}");
        }
    }

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

        //Log.Info($"Scheduling creation of AP hint for location {location} because it contains a progression and/or useful item");
        LocationsToHint.Add(location);
    }
}
