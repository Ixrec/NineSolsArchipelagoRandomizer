using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static ArchipelagoRandomizer.Features.EntranceRando.PortalData;
using static ArchipelagoRandomizer.Features.EntranceRando.EntranceMapping;

namespace ArchipelagoRandomizer.Features;

[HarmonyPatch]
internal class ERMain {
    static public bool entranceMappingActive = false;
    static public void ToggleMapping() {
        entranceMappingActive = !entranceMappingActive;
        ToastManager.Toast($"Set entranceMappingActive to {entranceMappingActive}. EntranceMap currently contains {EntranceMap.Count} mappings.");
    }

    static public void CreateCircularMapping() {
        EntranceMap.Clear();
        var portals = Enum.GetValues(typeof(Portal)).Cast<Portal>().ToArray();
        var previousPortal = portals.Last();
        foreach (var portal in portals) {
            if (DepartureOnlyPortals.Contains(portal) || ArrivalOnlyPortals.Contains(portal))
                continue;
            Log.Warning($"mapping {previousPortal} to {portal}");
            EntranceMap.Add(previousPortal, portal);
            previousPortal = portal;
        }
        ToastManager.Toast($"rewrote EntranceMap: all two-way portals are now linked in a circle");
    }

    static public void CreateRandomMapping() {
        EntranceMap.Clear();
        var portals = Enum.GetValues(typeof(Portal)).Cast<Portal>().ToArray();

        var unmappedDepartures = new List<Portal>();
        var unmappedArrivals = new List<Portal>();

        foreach (var portal in portals) {
            if (DepartureOnlyPortals.Contains(portal)) {
                unmappedDepartures.Add(portal);
            } else if (ArrivalOnlyPortals.Contains(portal)) {
                unmappedArrivals.Add(portal);
            } else {
                unmappedDepartures.Add(portal);
                unmappedArrivals.Add(portal);
            }
        }

        var rng = new Random();
        foreach (var departure in unmappedDepartures) {
            var i = rng.Next(0, unmappedArrivals.Count);

            var arrival = unmappedArrivals[i];
            unmappedArrivals.RemoveAt(i);

            Log.Warning($"mapping {departure} to {arrival}");
            EntranceMap.Add(departure, arrival);
        }

        ToastManager.Toast($"rewrote EntranceMap: all portals are now randomly linked");
    }
}
