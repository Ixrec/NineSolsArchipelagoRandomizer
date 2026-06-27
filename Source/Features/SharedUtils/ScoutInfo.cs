using Archipelago.MultiClient.Net.Models;
using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.Items.ItemImpls;
using System.Collections.Generic;

namespace ArchipelagoRandomizer.Features.SharedUtils;

internal class ScoutInfo {
    private static string progressionHexColor = "AF99EF";
    private static string usefulHexColor = "6D8BE8";
    private static string trapHexColor = "FA8072";
    private static string fillerHexColor = "00EEEE";

    public static string scoutInfoToShopTitle(SerializableItemInfo scoutedItemInfo, bool useColors = true) {
        if (!useColors)
            return $"{scoutedItemInfo.Player.Name}'s {scoutedItemInfo.ItemDisplayName}";

        string itemColor;
        // hex values copied from user.kv
        if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
            itemColor = progressionHexColor;
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
            itemColor = usefulHexColor;
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
            itemColor = trapHexColor;
        } else {
            itemColor = fillerHexColor;
        }

        // EE00EE is the player color in AP clients, but here it's best to let the default red vs white coloring apply
        string shopTitle = $"{scoutedItemInfo.Player.Name}'s <color=#{itemColor}>{scoutedItemInfo.ItemDisplayName}</color>";

        // Finally, if the item happens to be this slot's Nine Sols jade with randomized costs, then we want to display the cost.
        var id = scoutedItemInfo.ItemId;
        if (scoutedItemInfo.ItemGame == "Nine Sols" && ItemNames.archipelagoIdToItem.TryGetValue(id, out var item)) {
            if (ConnectionAndPopups.APSession != null && scoutedItemInfo.PlayerSlot == ConnectionAndPopups.APSession.ConnectionInfo.Slot) {
                var jade = Jades.GetJadeDataFor(item);
                if (jade != null) {
                    if (JadeCosts.JadeEnglishTitleToSaveFlag.TryGetValue(jade.Title, out var saveFlag)) {
                        if (saveFlag != null && JadeCosts.JadeSaveFlagToSlotDataCost.TryGetValue(saveFlag, out long cost)) {
                            shopTitle += $" (Cost {cost})";
                        }
                    }
                }
            }
        }

        return shopTitle;
    }

    public static string itemFlagsSummary(SerializableItemInfo scoutedItemInfo) {
        var flagStrings = new List<string>();
        if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
            flagStrings.Add($"<color=#{progressionHexColor}>Progression</color>");
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
            flagStrings.Add($"<color=#{usefulHexColor}>Useful</color>");
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
            flagStrings.Add($"<color=#{trapHexColor}>Trap</color>");
        }

        if (flagStrings.Count > 0) {
            return string.Join(" & ", flagStrings);
        } else {
            return $"<color=#{fillerHexColor}>Filler</color>";
        }
    }

    public static string itemFlagsDescription(SerializableItemInfo scoutedItemInfo) {
        var receivingPlayer = scoutedItemInfo.Player.Name;
        if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement)) {
            return $"{receivingPlayer} needs this item to make progress.";
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) {
            return $"This item would help {receivingPlayer}, but it's not essential.";
        } else if (scoutedItemInfo.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Trap)) {
            return $"{receivingPlayer} would rather never receive this item.\n\nBut heroes are forged in agony.";
        } else {
            return $"{receivingPlayer} probably doesn't care about this item at all.";
        }
    }
}
