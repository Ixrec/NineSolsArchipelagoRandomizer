using System.Collections.Generic;

namespace ArchipelagoRandomizer;

internal class TaoFruit {
    public static GameFlagDescriptable? ApplyTaoFruitToPlayer(Item item, int count, int oldCount) {
        List<ItemDataCollection> inventory = SingletonBehaviour<UIManager>.Instance.allItemCollections;
        GameFlagDescriptable? taoFruitInventoryItem = null;
        int skillPointsPerFruit = 0;
        switch (item) {
            case Item.GreaterTaoFruit:
                taoFruitInventoryItem = inventory[0].rawCollection[25];
                skillPointsPerFruit = 2;
                break;
            case Item.TaoFruit:
                taoFruitInventoryItem = inventory[0].rawCollection[26];
                skillPointsPerFruit = 1;
                break;
            case Item.TwinTaoFruit:
                taoFruitInventoryItem = inventory[0].rawCollection[27];
                skillPointsPerFruit = 4;
                break;
            default: break;
        }
        if (taoFruitInventoryItem != null) {
            taoFruitInventoryItem.acquired.SetCurrentValue(count > 0);
            taoFruitInventoryItem.unlocked.SetCurrentValue(count > 0);
            ((ItemData)taoFruitInventoryItem).ownNum.SetCurrentValue(count);

            // Tao Fruits also award skill points on absorption. Since these skill points are consumables,
            // you can't (always/reliably) take them away after they've been given, so we only worry about
            // adding a skill point when new fruit items have arrived.
            var newFruitItems = count - oldCount;
            if (newFruitItems > 0) {
                var totalSkillPointsToAward = newFruitItems * skillPointsPerFruit;
                Log.Info($"Giving the player {totalSkillPointsToAward} skill points for the {newFruitItems} new '{taoFruitInventoryItem.Title}'s they received");
                SingletonBehaviour<GameCore>.Instance.playerGameData.SkillPointLeft += totalSkillPointsToAward;
            }

            return taoFruitInventoryItem;
        }
        return null;
    }
}
