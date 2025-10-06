namespace ArchipelagoRandomizer;

internal class VanillaAbilities {
    public static bool ApplyVanillaAbilityToPlayer(Item item, int count, int oldCount) {
        // we're unlikely to use these, but: RollDodgeAbility is regular ground dash
        PlayerAbilityData? ability = null;
        ItemData? abilityInventoryItem = null;
        switch (item) {
            case Item.TaiChiKick: ability = Player.i.mainAbilities.ParryJumpKickAbility; break;
            case Item.AirDash: ability = Player.i.mainAbilities.RollDodgeInAirUpgrade; break;
            case Item.ChargedStrike: ability = Player.i.mainAbilities.ChargedAttackAbility; break;
            case Item.CloudLeap: ability = Player.i.mainAbilities.AirJumpAbility; break;
            case Item.SuperMutantBuster:
                ability = Player.i.mainAbilities.KillZombieFooAbility;
                abilityInventoryItem = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["9d0a49e76ce762c47844dbd20e21c737ItemData"];
                break;
            case Item.UnboundedCounter: ability = Player.i.mainAbilities.ParryCounterAbility; break;
            case Item.MysticNymphScoutMode: ability = Player.i.mainAbilities.HackDroneAbility; break;
            default: break;
        }
        if (abilityInventoryItem != null) {
            abilityInventoryItem.acquired.SetCurrentValue(count > 0);
            abilityInventoryItem.unlocked.SetCurrentValue(count > 0);
            abilityInventoryItem.ownNum.SetCurrentValue(count);
        }
        if (ability != null) {
            ability.acquired.SetCurrentValue(count > 0);
            ability.equipped.SetCurrentValue(count > 0);
            if (ability.BindingItemPicked != null) {
                Log.Info($"!!! {ability} has BindingItemPicked={ability.BindingItemPicked} !!!");
            }
            NotifyAndSave.Default(ability, count, oldCount);
            return true;
        }
        return false;
    }
}
