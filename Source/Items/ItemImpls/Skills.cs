using System.Linq;

namespace ArchipelagoRandomizer.Items.ItemImpls;

internal class Skills {
    public static GameFlagDescriptable? GetDisplayGFDFor(Item item) {
        if (!SkillTree.ItemToSkills.ContainsKey(item))
            return null;

        var firstSkill = SkillTree.ItemToSkills[item][0];
        var saveFlagId = SkillTree.SaveFlagIds[firstSkill];
        return (SkillNodeData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[saveFlagId];
    }

    public static bool ApplySkillToPlayer(Item item, int count, int oldCount) {
        if (!SkillTree.ItemToSkills.ContainsKey(item))
            return false;

        // This will often be a list of length 1, but we might as well be generic since every skill item maps to one or more SkillNodeData.
        var padList = SkillTree.ItemToSkills[item].Select(skill => SkillTree.SaveFlagIds[skill]).ToArray();

        Log.Info($"ApplySkillToPlayer(item={item}, count={count})");

        PlayerAbilityDataList.ApplyPADListItemToPlayer(count, padList);

        NotifyAndSave.WithCustomText(GetDisplayGFDFor(item)!, $"Collected {ItemNames.itemNames[item]}.", count, oldCount);
        return true;
    }
}
