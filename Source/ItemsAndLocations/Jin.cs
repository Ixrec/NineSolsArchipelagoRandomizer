namespace ArchipelagoRandomizer;

internal class Jin {
    public static GameFlagDescriptable? ApplyJinToPlayer(Item item, int count, int oldCount) {
        int moneyItemSize = 0;
        switch (item) {
            case Item.Jin800: moneyItemSize = 800; break;
            case Item.Jin320: moneyItemSize = 320; break;
            case Item.Jin50: moneyItemSize = 50; break;
            default: break;
        }
        if (moneyItemSize != 0) {
            // Since this is a consumable, you can't (always/reliably) take it away after it's been given,
            // so we only worry about adding jin when new jin items have arrived.
            var newMoneyItems = count - oldCount;
            if (newMoneyItems > 0) {
                var jinToAdd = newMoneyItems * moneyItemSize;
                SingletonBehaviour<GameCore>.Instance.playerGameData.AddGold(jinToAdd, GoldSourceTag.Chest);
            }

            var jinGFD = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[1];
            return jinGFD;
        }
        return null;
    }
}
