using System.Collections.Generic;

namespace ArchipelagoRandomizer;

internal class Jades {
    public static bool ApplyJadeToPlayer(Item item, int count, int oldCount) {
        List<JadeData> jades = Player.i.mainAbilities.jadeDataColleciton.gameFlagDataList;
        JadeData? jadeEntry = null;
        switch (item) {
            case Item.ImmovableJade: jadeEntry = jades[0]; break;
            case Item.HarnessForceJade: jadeEntry = jades[1]; break;
            case Item.FocusJade: jadeEntry = jades[2]; break;
            case Item.SwiftDescentJade: jadeEntry = jades[3]; break;
            //case Item.MedicalJade: jadeEntry = jades[4]; break; // shop item
            //case Item.QuickDoseJade: jadeEntry = jades[5]; break; // shop item
            case Item.SteelyJade: jadeEntry = jades[6]; break;
            case Item.StasisJade: jadeEntry = jades[7]; break;
            case Item.MobQuellJadeYin: jadeEntry = jades[8]; break;
            //case Item.MobQuellJadeYang: jadeEntry = jades[9]; break; // shop item
            case Item.BearingJade: jadeEntry = jades[10]; break;
            case Item.DivineHandJade: jadeEntry = jades[11]; break;
            case Item.IronSkinJade: jadeEntry = jades[12]; break;
            case Item.PauperJade: jadeEntry = jades[13]; break;
            case Item.SwiftBladeJade: jadeEntry = jades[14]; break;
            //case Item.LastStandJade: jadeEntry = jades[15]; break; // shop item
            //case Item.RecoveryJade: jadeEntry = jades[16]; break; // shop item
            case Item.BreatherJade: jadeEntry = jades[17]; break;
            case Item.HedgehogJade: jadeEntry = jades[18]; break;
            //case Item.RicochetJade: jadeEntry = jades[19]; break; // shop item
            case Item.RevivalJade: jadeEntry = jades[20]; break;
            case Item.SoulReaperJade: jadeEntry = jades[21]; break;
            //case Item.HealthThiefJade: jadeEntry = jades[22]; break; // shop item
            case Item.QiBladeJade: jadeEntry = jades[23]; break;
            case Item.QiSwipeJade: jadeEntry = jades[24]; break;
            //case Item.ReciprocationJade: jadeEntry = jades[25]; break; // shop item
            case Item.CultivationJade: jadeEntry = jades[26]; break;
            case Item.AvariceJade: jadeEntry = jades[27]; break;
            default: break;
        }
        if (jadeEntry != null) {
            jadeEntry.acquired.SetCurrentValue(count > 0);
            jadeEntry.unlocked.SetCurrentValue(count > 0);

            var title = jadeEntry.Title;
            if (JadeCosts.JadeTitleToSlotDataCost.TryGetValue(title, out long cost)) {
                NotifyAndSave.WithCustomTextAndPanelType(jadeEntry, $"Collected {title} (Cost {cost}).", PlayerInfoPanelType.Jade, count, oldCount);
            } else {
                NotifyAndSave.Default(jadeEntry, count, oldCount);
            }
            return true;
        }
        return false;
    }
}
