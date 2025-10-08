using HarmonyLib;
using System;

namespace ArchipelagoRandomizer;

// A "removed" ability is one that you always have in the vanilla game, and might not even think of as a distinct ability,
// but in the randomizer we would like to take it away and item-ify it.

[HarmonyPatch]
internal class RemovedAbilities {
    // These 3 abilities were not AP items in the earliest versions of this randomizer. If we're connected to a slot generated
    // on those early versions, then we need to let those abilities remain enabled despite the AP items not existing.
    private static bool olderWorldCompat = false;

    public static void ApplyWorldVersion(Version worldVersion) {
        if (worldVersion <= new Version(0, 1, 7)) {
            olderWorldCompat = true;
        }
    }

    private static bool hasWallClimbItem = false;
    private static bool hasGrappleItem = false;
    private static bool hasLedgeGrabItem = false;

    public static bool ApplyRemovedAbilityToPlayer(Item item, int count, int oldCount) {
        switch (item) {
            case Item.WallClimb:
                hasWallClimbItem = (count > 0);

                Player.i.statData.WallSlideEnabled = olderWorldCompat || hasWallClimbItem;

                var cloudLeap = Player.i.mainAbilities.AirJumpAbility;
                NotifyAndSave.WithCustomText(cloudLeap, "Wall Climb", count, oldCount);
                return true;
            case Item.Grapple:
                hasGrappleItem = (count > 0);

                var airDash = Player.i.mainAbilities.RollDodgeInAirUpgrade;
                NotifyAndSave.WithCustomText(airDash, "Grapple", count, oldCount);
                return true;
            case Item.LedgeGrab:
                hasLedgeGrabItem = (count > 0);

                var skullKickSkillCore = SingletonBehaviour<UIManager>.Instance.skillTreeUI.allSkillNodes[17].pluginCore;
                NotifyAndSave.WithCustomTextAndSkillTreeSprite(skullKickSkillCore, "Ledge Grab", count, oldCount);
                return true;
            default:
                return false;
        }
    }

    // Grapple needs two patches: one for regular hooks, and one for ziplines
    // TODO: would be nice to also disable the grapple hook glowing at you
    [HarmonyPrefix, HarmonyPatch(typeof(Player), "GrabHookCheck")]
    private static bool Player_GrabHookCheck(Player __instance, ref bool __result) {
        if (olderWorldCompat)
            return true;
        if (hasGrappleItem)
            return true;

        __result = false;
        return false;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(Player), "HookToForceMoverCheck")] // this is for ziplines
    private static bool Player_HookToForceMoverCheck(Player __instance, ref bool __result) {
        if (olderWorldCompat)
            return true;
        if (hasGrappleItem)
            return true;

        __result = false;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Player), "LedgeGrabCheck")]
    private static bool Player_LedgeGrabCheck(Player __instance, ref bool __result) {
        if (olderWorldCompat)
            return true;
        if (hasLedgeGrabItem)
            return true;

        __result = false;
        return false;
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
