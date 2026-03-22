using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchipelagoRandomizer.Locations {
    public class LocationScouter {
        public static void ScoutAllLocations() {
            ScoutLocationsImpl(null);
        }
        public static void ScoutLocations(List<long> locationIDs) {
            ScoutLocationsImpl(locationIDs);
        }

        private static void ScoutLocationsImpl(List<long>? locationIDs = null) {
            var saveData = APSaveManager.CurrentAPSaveData;
            if (saveData == null) {
                Log.Error("Scouting aborted! No current save data to write scouts to.");
                return;
            }
            var session = ConnectionAndPopups.APSession;
            if (session == null) {
                Log.Error("Scouting aborted! No AP session to scout with.");
                return;
            }

            // null locationIDs means scout *all* locations
            if (locationIDs == null) {
                locationIDs = new List<long>();
                foreach (Location loc in LocationNames.locationNames.Keys)
                    if (LocationNames.locationToArchipelagoId.ContainsKey(loc))
                        locationIDs.Add(LocationNames.locationToArchipelagoId[loc]);
            }

            var scoutedLocations = saveData.scoutedLocations;

            HashSet<long> finalLocationIDs = new();
            foreach (long id in locationIDs) {
                var isARealLocationId = LocationNames.archipelagoIdToLocation.ContainsKey(id);
                var alreadyScouted = scoutedLocations.ContainsKey(LocationNames.archipelagoIdToLocation[id]);

                if (isARealLocationId && !alreadyScouted)
                    finalLocationIDs.Add(id);
            }

            var scoutedCount = scoutedLocations.Count;
            var toScoutCount = finalLocationIDs.Count;
            if (toScoutCount == 0) {
                Log.Info($"ScoutLocations() doing nothing because all {locationIDs.Count} locationIds have already been scouted");
                return;
            }

            Log.Info($"calling AP.MC.Net's ScoutLocationsAsync() with {toScoutCount} locationIds");
            var scoutTask = Task.Run(() => session.Locations.ScoutLocationsAsync(finalLocationIDs.ToList().ToArray()).ContinueWith(locationInfoPacket => {
                var result = locationInfoPacket.Result;
                foreach (var (locationId, scoutedItemInfo) in result) {
                    Location modLocation = LocationNames.archipelagoIdToLocation[locationId];
                    scoutedLocations.Add(modLocation, scoutedItemInfo.ToSerializable());
                }

                APSaveManager.WriteCurrentSaveFile();
                Log.Info($"added {result.Count} location scouts to save data, for a new total of {scoutedLocations.Count}");
            }));
            if (!scoutTask.Wait(TimeSpan.FromSeconds(5))) {
                Log.Error("Scouting timed out!");
            }
        }
    }
}
