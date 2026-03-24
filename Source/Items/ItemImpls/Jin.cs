namespace ArchipelagoRandomizer.Items.ItemImpls;

internal class Jin {
    public static GameFlagDescriptable GetJinGFD() => SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[1];

    private static int? GetMoneyItemSize(Item item) {
        switch (item) {
            case Item.Jin800: return 800;
            case Item.Jin320: return 320;
            case Item.Jin50: return 50;
            default: return null;
        }
    }

    public static GameFlagDescriptable? GetDisplayGFDFor(Item item) {
        if (GetMoneyItemSize(item) != null)
            return GetJinGFD();
        return null;
    }

    public static bool ApplyJinToPlayer(Item item, int count, int oldCount) {
        var moneyItemSize = GetMoneyItemSize(item);
        if (moneyItemSize != null) {
            // Since this is a consumable, you can't (always/reliably) take it away after it's been given,
            // so we only worry about adding jin when new jin items have arrived.
            var newMoneyItems = count - oldCount;
            if (newMoneyItems > 0) {
                var jinToAdd = newMoneyItems * (int)moneyItemSize;
                SingletonBehaviour<GameCore>.Instance.playerGameData.AddGold(jinToAdd, GoldSourceTag.Chest);
            }

            NotifyAndSave.Default(GetJinGFD(), count, oldCount);
            return true;
        }
        return false;
    }
}
