namespace ArchipelagoRandomizer; 

// A "removed" ability is one that you always have in the vanilla game, and might not even think of as a distinct ability,
// but in the randomizer we would like to take it away and item-ify it.

internal class RemovedAbilities {
    public static bool ApplyRemovedAbilityToPlayer(Item item, int count, int oldCount) {
        switch (item) {
            case Item.WallClimb:
                Player.i.statData.WallSlideEnabled = (count > 0);

                var cloudLeap = Player.i.mainAbilities.AirJumpAbility;
                NotifyAndSave.WithCustomText(cloudLeap, "Wall Climb", count, oldCount);
                return true;
            case Item.Grapple:
                //Grapple.enableGrappling = (count > 0);
                //patch Player::GrabHookCheck() to always return false for "regular hooks"
                //would be nice to also disable the grapple hook glowing at you
                //patch Player::HookToForceMoverCheck() to always return false for rails / ziplines

                var airDash = Player.i.mainAbilities.RollDodgeInAirUpgrade;
                NotifyAndSave.WithCustomText(airDash, "Grapple", count, oldCount);
                return true;
            case Item.LedgeGrab:
                //LedgeGrab.enableLedgeGrab = (count > 0);
                //patch Player::LedgeGrabCheck() to always return false

                var skullKickSkillCore = SingletonBehaviour<UIManager>.Instance.skillTreeUI.allSkillNodes[17].pluginCore;
                NotifyAndSave.WithCustomTextAndSkillTreeSprite(skullKickSkillCore, "Ledge Grab", count, oldCount);
                return true;
            default:
                return false;
        }
    }
}

// Since this is currently the only file to care about skill tree nodes, let's record the ordered list of nodes here for future reference.
// These are the skillCore.data.Title values for each element of SingletonBehaviour<UIManager>.Instance.skillTreeUI.allSkillNodes
/*
ImmortaDash
QBlast
TriplSlash
Parry
Cloud Leap
Air Dash
Swift Runner
QBoost
Charged Strike
Shadow Strike
Tai-ChKick
Bullet Deflect
Unbounded Counter
Swift Rise
LifRecovery
QBoost
Backlash
SkulKick
Breathing Exercise
Leverage
Water Flow
AzurRecovery
FulControl
IncisivDrain
Enhanced Bullet Deflect
Enhanced Blade
Unbounded Drain
Enhanced Talisman
QBoost
QBoost
Enhanced Talisman
Enhanced Blade
Enhanced Water Flow
Enhanced FulControl
Enhanced QBlast
Unbounded Charge
 */
