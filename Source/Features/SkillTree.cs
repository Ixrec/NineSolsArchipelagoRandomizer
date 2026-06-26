using ArchipelagoRandomizer.Features;
using ArchipelagoRandomizer.Features.SharedUtils;
using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.Locations;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class SkillTree {
    public enum Skill {
        // Starting skills
        ImmortalDash,
        QiBlast,
        TripleSlash,
        Parry,

        // Major vanilla abilities (which are skill nodes only to act as prerequisites)
        CloudLeap,
        AirDash,
        TaiChiKick,
        ChargedStrike,
        UnboundedCounter,

        // Logic-relevant skills
        SwiftRunner,
        BulletDeflect,
        EnhancedBulletDeflect,

        // Talisman upgrades
        WaterFlow,
        EnhancedWaterFlow,
        FullControl,
        EnhancedFullControl,
        EnhancedQiBlast,

        // Other unique skills
        ShadowStrike,
        SwiftRise,
        LifeRecovery,
        Backlash,
        SkullKick,
        BreathingExercise,
        Leverage,
        AzureRecovery,
        IncisiveDrain,
        UnboundedDrain,
        UnboundedCharge,

        // Other progressive skills
        QiBoost1,
        QiBoost2,
        QiBoost3,
        QiBoost4,
        EnhancedTalismanLeft,
        EnhancedTalismanRight,
        EnhancedBlade1,
        EnhancedBlade2,
    };

    // Each skill has a SkillNodeUIControlButton, a SkillNodeData, and a SkillCore. All 3 of these objects have exactly
    // the same .name value in Unity, for every skill (I checked), so we only need to remember one name per skill.
    public static readonly Dictionary<Skill, string> UnityObjectNames = new Dictionary<Skill, string> {
        { Skill.ImmortalDash, "(Skill) 0_閃避" },
        { Skill.QiBlast, "(Skill) 流派 Foo 1_一氣貫通" },
        { Skill.TripleSlash, "(Skill) 0_攻擊" },
        { Skill.Parry, "(Skill) 0_parry 格擋" },

        { Skill.CloudLeap, "(Skill) 5_二段跳" },
        { Skill.AirDash, "(Skill) 3_Air Dodge空中閃避" },
        { Skill.TaiChiKick, "(Skill) 1_識破" },
        { Skill.ChargedStrike, "(Skill) 2_Charge Atk蓄力攻擊" },
        { Skill.UnboundedCounter, "(Skill) 4_ParryCounter 大反" },

        { Skill.SwiftRunner, "(Skill) Run 奔跑" },
        { Skill.BulletDeflect, "(Skill) Accurate Parry To Reflect Projectile 精準反彈" },
        { Skill.EnhancedBulletDeflect, "(Skill) Reflect Projectile Damage 增加反彈傷害" },

        { Skill.WaterFlow, "(Skill) 流派 Foo 3_行雲流水" },
        { Skill.EnhancedWaterFlow, "(Skill) 流派升級 Foo 3_行雲流水 升級" },
        { Skill.FullControl, "(Skill) 流派 Foo 2_收放自如" },
        { Skill.EnhancedFullControl, "(Skill) 流派升級 Foo 2_收放自如 升級" },
        { Skill.EnhancedQiBlast, "(Skill) 流派升級 Foo 1_一氣貫通 升級" },

        { Skill.ShadowStrike, "(Skill) 0_背襲" },
        { Skill.SwiftRise, "(Skill) 受身閃避" },
        { Skill.LifeRecovery, "(Skill) 收魂回命" },
        { Skill.Backlash, "(Skill) 1_識破造成額外內傷" },
        { Skill.SkullKick, "(Skill) 0_下踢" },
        { Skill.BreathingExercise, "(Skill) Accurate Parry RestoreInjury 精準回內傷" },
        { Skill.Leverage, "(Skill) ParryCounterReflect Dmg Lv1 借力打力" },
        { Skill.AzureRecovery, "(Skill) 收魂回砂" },
        { Skill.IncisiveDrain, "(Skill) 1_識破二顆氣" },
        { Skill.UnboundedDrain, "(Skill) 大反更多氣" },
        { Skill.UnboundedCharge, "(Skill) Charge Chi 無量蓄氣" },

        { Skill.QiBoost1, "(Skill) Foo Power +1 內力提升 LV1" },
        { Skill.QiBoost2, "(Skill) Foo Power +1 內力提升 LV2" },
        { Skill.QiBoost3, "(Skill) Foo Power +1 內力提升 LV3" },
        { Skill.QiBoost4, "(Skill) Foo Power +1 內力提升 LV4" },
        { Skill.EnhancedTalismanLeft, "(Skill) 0_符咒傷害提升 2" },
        { Skill.EnhancedTalismanRight, "(Skill) 0_符咒傷害提升 1" },
        { Skill.EnhancedBlade1, "(Skill) 0 氣刃攻擊力提升 1" },
        { Skill.EnhancedBlade2, "(Skill) 0_氣刃攻擊力提升 2" },
    };

    public static Dictionary<string, Skill> UnityObjectNameToSkill = UnityObjectNames.ToDictionary(x => x.Value, x => x.Key);

    public static readonly Dictionary<Skill, string> SaveFlagIds = new Dictionary<Skill, string> {
        { Skill.ImmortalDash, "af9cb112a715e4955afaa3e740f4fe5aSkillNodeData" },
        { Skill.QiBlast, "261c03bb170884f0084f3d4a8c17f708SkillNodeData" },
        { Skill.TripleSlash, "d8cbeba2a689a422abdb956743a07891SkillNodeData" },
        { Skill.Parry, "19b09ad0c66d84337826a5c0184625edSkillNodeData" },

        { Skill.CloudLeap, "827cb8277cd144d83861460103607ed7SkillNodeData" },
        { Skill.AirDash, "b6279cb10939e9d4ebda64aea801f75cSkillNodeData" },
        { Skill.TaiChiKick, "15371e774c66f4ce9a58dc63b1464910SkillNodeData" },
        { Skill.ChargedStrike, "e4c62cea0f9fb4759b69624d571a3c8dSkillNodeData" },
        { Skill.UnboundedCounter, "82ea1161b33ea423caa77f67fe049046SkillNodeData" },

        { Skill.SwiftRunner, "ae3f7be7afb294d2eba0f6f4d129c6d0SkillNodeData" },
        { Skill.BulletDeflect, "f168f2477ae5a481ca147ee9ce61833bSkillNodeData" },
        { Skill.EnhancedBulletDeflect, "3c5bc531c4961479189e171da6b1ca5dSkillNodeData" },

        { Skill.WaterFlow, "b24699dc273034e34867754a3c97c4c4SkillNodeData" },
        { Skill.EnhancedWaterFlow, "9811579f8600a48ecbb002eba20f5bcbSkillNodeData" },
        { Skill.FullControl, "92e91b67a2b794671a74c275d4c1d2b6SkillNodeData" },
        { Skill.EnhancedFullControl, "f0db8641341dc4fd39368da2b3a8a821SkillNodeData" },
        { Skill.EnhancedQiBlast, "fcb5a8efa2ca14f818f53071b10aab11SkillNodeData" },

        { Skill.ShadowStrike, "0c8ce74dd338f4ecaa525eee0b37cc1cSkillNodeData" },
        { Skill.SwiftRise, "c57cd9a29dca44f9d94fe76f4b6c248dSkillNodeData" },
        { Skill.LifeRecovery, "f44bf0d0544f84e548eb76a00f12cccdSkillNodeData" },
        { Skill.Backlash, "6ce6b20889a30401d89edbea192fdb70SkillNodeData" },
        { Skill.SkullKick, "930431c2cc50341778d2f9736d27ee6eSkillNodeData" },
        { Skill.BreathingExercise, "08fecc31de6974aaca652b6beb1cbbfeSkillNodeData" },
        { Skill.Leverage, "41ea48c62b044a041bdf3f7640dbeb4cSkillNodeData" },
        { Skill.AzureRecovery, "cee53a9ab05cb4f25bc3d5f900fa523cSkillNodeData" },
        { Skill.IncisiveDrain, "b969aebcc39544a1eb88a6dc4f538052SkillNodeData" },
        { Skill.UnboundedDrain, "304e6970e1f624f0d9254b428b81a73eSkillNodeData" },
        { Skill.UnboundedCharge, "42fd1d09af02e41229825f330c193658SkillNodeData" },

        { Skill.QiBoost1, "b3e48a60ad0b84648952dc21712b27c0SkillNodeData" },
        { Skill.QiBoost2, "411e38c06854a484cb7eb7e2d5cd9b9eSkillNodeData" },
        { Skill.QiBoost3, "66ae60d46a1bf4e46aafe55fa7a0a34bSkillNodeData" },
        { Skill.QiBoost4, "c2c80a7aa73a24226b410bd2064b2a5cSkillNodeData" },
        { Skill.EnhancedTalismanLeft, "459bd9b1979414acdbcba2a3644c056cSkillNodeData" },
        { Skill.EnhancedTalismanRight, "9efa79aa5093f4681b650e0dbc0d02feSkillNodeData" },
        { Skill.EnhancedBlade1, "f9c12b4ba239e49ff8992d316de77179SkillNodeData" },
        { Skill.EnhancedBlade2, "fadab0801872448a088b7cc05d63aac0SkillNodeData" },
    };

    public static readonly Dictionary<Item, List<Skill>> ItemToSkills = new Dictionary<Item, List<Skill>> {
        { Item.SwiftRunner, new List<Skill>{ Skill.SwiftRunner } },
        { Item.ProgressiveBulletDeflect, new List<Skill>{ Skill.BulletDeflect, Skill.EnhancedBulletDeflect } },

        { Item.ProgressiveWaterFlow, new List<Skill>{ Skill.WaterFlow, Skill.EnhancedWaterFlow } },
        { Item.ProgressiveFullControl, new List<Skill>{ Skill.FullControl, Skill.EnhancedFullControl } },
        { Item.EnhancedQiBlast, new List<Skill>{ Skill.EnhancedQiBlast } },

        { Item.ShadowStrike, new List<Skill>{ Skill.ShadowStrike } },
        { Item.SwiftRise, new List<Skill>{ Skill.SwiftRise } },
        { Item.LifeRecovery, new List<Skill>{ Skill.LifeRecovery } },
        { Item.Backlash, new List<Skill>{ Skill.Backlash } },
        { Item.SkullKick, new List<Skill>{ Skill.SkullKick } },
        { Item.BreathingExercise, new List<Skill>{ Skill.BreathingExercise } },
        { Item.Leverage, new List<Skill>{ Skill.Leverage } },
        { Item.AzureRecovery, new List<Skill>{ Skill.AzureRecovery } },
        { Item.IncisiveDrain, new List<Skill>{ Skill.IncisiveDrain } },
        { Item.UnboundedDrain, new List<Skill>{ Skill.UnboundedDrain } },
        { Item.UnboundedCharge, new List<Skill>{ Skill.UnboundedCharge } },

        { Item.QiBoost, new List<Skill>{ Skill.QiBoost1, Skill.QiBoost2, Skill.QiBoost3, Skill.QiBoost4 } },
        { Item.EnhancedTalisman, new List<Skill>{ Skill.EnhancedTalismanLeft, Skill.EnhancedTalismanRight } },
        { Item.EnhancedBlade, new List<Skill>{ Skill.EnhancedBlade1, Skill.EnhancedBlade2 } },
    };

    public static readonly Dictionary<Location, Skill> LocationToSkill = new Dictionary<Location, Skill> {
        { Location.SKILL_SWIFT_RUNNER, Skill.SwiftRunner },
        { Location.SKILL_QB_TL, Skill.QiBoost1 },
        { Location.SKILL_SS, Skill.ShadowStrike },
        { Location.SKILL_BD, Skill.BulletDeflect },
        { Location.SKILL_SWIFT_RISE, Skill.SwiftRise },
        { Location.SKILL_LR, Skill.LifeRecovery },
        { Location.SKILL_QB_TR, Skill.QiBoost2 },
        { Location.SKILL_BL, Skill.Backlash },
        { Location.SKILL_SK, Skill.SkullKick },
        { Location.SKILL_BE, Skill.BreathingExercise },
        { Location.SKILL_LV, Skill.Leverage },
        { Location.SKILL_WF, Skill.WaterFlow },
        { Location.SKILL_FC, Skill.FullControl },
        { Location.SKILL_ID, Skill.IncisiveDrain },
        { Location.SKILL_EBD, Skill.EnhancedBulletDeflect },
        { Location.SKILL_EB_L, Skill.EnhancedBlade1 },
        { Location.SKILL_UD, Skill.UnboundedDrain },
        { Location.SKILL_ET_L, Skill.EnhancedTalismanLeft },
        { Location.SKILL_QB_BL, Skill.QiBoost3 },
        { Location.SKILL_AR, Skill.AzureRecovery },
        { Location.SKILL_QB_BR, Skill.QiBoost4 },
        { Location.SKILL_ET_R, Skill.EnhancedTalismanRight },
        { Location.SKILL_EB_R, Skill.EnhancedBlade2 },
        { Location.SKILL_UC, Skill.UnboundedCharge },
        { Location.SKILL_EWF, Skill.EnhancedWaterFlow },
        { Location.SKILL_EFC, Skill.EnhancedFullControl },
        { Location.SKILL_EQB, Skill.EnhancedQiBlast },
    };

    public static Dictionary<Skill, Location> SkillToLocation = LocationToSkill.ToDictionary(x => x.Value, x => x.Key);

    // 0 = vanilla, 1 = medium, 2 = ledge_storage
    private static long LogicDifficulty = 0;
    private static bool RandomizeSkillTree = false;

    public static void ApplySlotData(long? logicDifficulty, long? randomizeSkillTree) {
        LogicDifficulty = logicDifficulty ?? 0;
        RandomizeSkillTree = randomizeSkillTree > 0;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        try {
            if (LogicDifficulty > 0) {
                var swiftRunnerSkillNode = (SkillNodeData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["ae3f7be7afb294d2eba0f6f4d129c6d0SkillNodeData"];
                if (!swiftRunnerSkillNode.IsAcquired) {
                    Log.Info($"SkillTree::GameLevel_Awake auto-unlocking Swift Runner since LogicDifficulty is {LogicDifficulty}");
                    swiftRunnerSkillNode.PlayerPicked();

                    InGameConsole.Add($"<color=orange>The Swift Runner skill has been automatically unlocked</color>\nbecause this slot was generated with a logic_difficulty of medium or higher"); // and skill tree rando does not exist yet
                }
            }
        } catch (Exception ex) {
            Log.Error($"SkillTree::GameLevel_Awake threw: {ex.Message}\nwith stack:\n{ex.StackTrace}\nand InnerException: {ex.InnerException?.Message}\nwith stack:\n{ex.InnerException?.StackTrace}");
        }
    }

    // Although the skill tree is not fully revealed right away, there's no reason to delay *scouting* all of it on a skill rando slot
    public static void EnsureSkillTreeScouted() {
        if (!RandomizeSkillTree)
            return;
        //Log.Info($"SkillTree::EnsureSkillTreeScouted()");
        List<long> skillLocationIds = LocationToSkill.Keys.Select(loc => LocationNames.locationToArchipelagoId[loc]).ToList();
        LocationScouter.ScoutLocations(skillLocationIds);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UITabsItem), nameof(UITabsItem.TabFocus))]
    private static void UITabsItem_TabFocus(UITabsItem __instance) {
        if (__instance.PanelType != PlayerInfoPanelType.SkillTree)
            return;
        EnsureSkillTreeScouted();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SkillNodeUIControlButton), "OnEnable")]
    private static void SkillNodeUIControlButton_Start(SkillNodeUIControlButton __instance) {
        if (!UnityObjectNameToSkill.TryGetValue(__instance.name, out var skill))
            return;
        if (!SkillToLocation.TryGetValue(skill, out var location))
            return;

        GameObject apLogo = new GameObject("APRandomizer_APLogo");
        apLogo.transform.SetParent(__instance.transform.Find("viewUI/old"), false);
        apLogo.transform.SetAsFirstSibling(); // because sibling order determines z-order

        var apLogoImage = apLogo.AddComponent<Image>();
        apLogoImage.sprite = APLogo.getApLogoSprite(alpha: 0.4f);

        if (skill == Skill.WaterFlow || skill == Skill.FullControl) { // small diamonds
            apLogoImage.transform.localScale *= 0.6f;
        } else if (skill == Skill.EnhancedWaterFlow || skill == Skill.EnhancedFullControl || skill == Skill.EnhancedQiBlast) { // large diamonds
            apLogoImage.transform.localScale *= 0.8f;
        }

        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            var hexColor = ScoutInfo.scoutInfoToHexColor(scoutedItemInfo);
            var borderGO = __instance.transform.Find("viewUI/Color"); // for some reason "Color" is confusingly the name of the normally monochrome border
            ColorUtility.TryParseHtmlString($"#{hexColor}", out var color);
            borderGO.GetComponent<UnityEngine.UI.Image>().color = color;
        }
    }

    private static bool IsLocationChecked(Location location) {
        return APSaveManager.CurrentAPSaveData?.locationsChecked?.GetValueOrDefault(location.ToString(), false) ?? false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SkillCore), nameof(SkillCore.IsAcquired), MethodType.Getter)]
    private static bool SkillCore_IsAcquired(SkillCore __instance, ref bool __result) {
        if (!UnityObjectNameToSkill.TryGetValue(__instance.name, out var skill))
            return true;
        if (!SkillToLocation.TryGetValue(skill, out var location))
            return true;

        var isChecked = IsLocationChecked(location);

        //Log.Warning($"SkillCore_IsAcquired called for {__instance.name} / {skill}, setting to {isAcquired}");
        __result = isChecked;
        return false;
    }

    // it might be possible to avoid these additional patches by instead patching SND/PAD/GFD::IsAcquired, but those are so generic I'd rather not risk it

    [HarmonyPrefix, HarmonyPatch(typeof(SkillNodeData), nameof(SkillNodeData.IsRequiredAbilitiesAcquired), MethodType.Getter)]
    private static bool SkillNodeData_IsRequiredAbilitiesAcquired(SkillNodeData __instance, ref bool __result) {
        if (!UnityObjectNameToSkill.TryGetValue(__instance.name, out var skill))
            return true;
        if (!SkillToLocation.TryGetValue(skill, out var _))
            return true;

        // conceptually, this is the vanilla IsRequiredAbilitiesAcquired impl but with PAD::IsAcquired replaced by IsLocationChecked()
        foreach (PlayerAbilityData requiredAbility in __instance.requiredAbilities) {
            var prereqName = requiredAbility.name;

            // if any of the prereqs are not skills we know, bail out and let the vanilla code handle it
            if (!UnityObjectNameToSkill.TryGetValue(prereqName, out var prereqSkill)) {
                Log.Warning($"SkillNodeData_IsRequiredAbilitiesAcquired failed: {__instance.name} had requiredAbility {prereqName} with no known skill");
                return true;
            }
            if (!SkillToLocation.TryGetValue(prereqSkill, out var prereqLocation)) {
                // this failure is expected on the unrandomized skills, so don't bother warning
                //Log.Warning($"SkillNodeData_IsRequiredAbilitiesAcquired failed: {__instance.name} had requiredAbility {prereqName} / {prereqSkill} with no known location");
                return true;
            }

            var isChecked = IsLocationChecked(prereqLocation);
            if (!isChecked) {
                //Log.Warning($"SkillNodeData_IsRequiredAbilitiesAcquired forcing result to FALSE for {__instance.name} because {prereqLocation} is not checked");
                __result = false;
                return false;
            }
        }
        //Log.Warning($"SkillNodeData_IsRequiredAbilitiesAcquired forcing result to TRUE for {__instance.name}");
        __result = true;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BoolFieldValueBinder), nameof(BoolFieldValueBinder.IsValid), MethodType.Getter)]
    private static bool BoolFieldValueBinder_IsValid(BoolFieldValueBinder __instance, ref bool __result) {
        if (!__instance.name.StartsWith("[Condition] Not IsAcquired 尚未取得"))
            return true;
        if (__instance.transform.parent?.parent?.name != "LockPanel")
            return true;
        if (__instance.fieldName != "IsAcquired")
            return true;

        //Log.Warning($"BoolFieldValueBinder_IsValid for {__instance.name}");
        var dip = AccessTools.FieldRefAccess<BoolFieldValueBinder, DescriptableInstanceProvider>("descriptableInstanceProvider").Invoke(__instance);
        if (!UnityObjectNameToSkill.TryGetValue(dip.CurrentInstance.name, out var skill))
            return true;
        if (!SkillToLocation.TryGetValue(skill, out var location))
            return true;

        var isChecked = IsLocationChecked(location);

        //Log.Warning($"BoolFieldValueBinder_IsValid called for {__instance.name} / {skill}, setting to {isChecked}");
        __result = isChecked;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SkillCore), nameof(SkillCore.SkillAcquired))]
    private static bool SkillCore_SkillAcquired(SkillCore __instance) {
        if (!UnityObjectNameToSkill.TryGetValue(__instance.name, out var skill))
            return true;
        if (!SkillToLocation.TryGetValue(skill, out var location))
            return true;

        Log.Info($"SkillCore_SkillAcquired called for {__instance.name} / {skill} / {location}");
        LocationTriggers.CheckLocation(location);

        return false;
    }

    // The description panel to the right of the actual skill tree gets updated from two places: a single ItemDescriptionProvider.UpdateView() call
    // for the whole panel, and then one TextValueBinder.UpdateView() call for each text object.
    // As far as I can tell this double-setting is strictly redundant and serves no practical purpose, but it's important to understand that
    // a) the TVB::UV()s rely on IDP::UV() updating IDP::_currentDescriptable, and b) the TVB::UV()s happen *after* the IDP::UV(),
    // and c) the bottommost part of the panel has other components which also rely on IDP::_currentDescriptable being up to date.

    // That's why we want to leave IDP::UV() alone and only patch the TVB::UV() calls.

    private static string SkillTreeRightPanelGOPath = "GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/UI-Canvas/[Tab] MenuTab/CursorProvider/Menu Vertical Layout/Panels/[經絡]SkillTreeUI Manager/Description Provider/RightPart/Background/Outline";
    private static string SkillTreeTypeGOPath = $"{SkillTreeRightPanelGOPath}/Type";
    private static string SkillTreeTitleGOPath = $"{SkillTreeRightPanelGOPath}/Title";
    private static string SkillTreeDescriptionGOPath = $"{SkillTreeRightPanelGOPath}/Scroll View/Viewport/description";

    [HarmonyPrefix, HarmonyPatch(typeof(TextValueBinder), nameof(TextValueBinder.UpdateView))]
    private static bool TextValueBinder_UpdateView(TextValueBinder __instance, ref IDescriptable data) {
        string[] tvbNames = ["Type", "Title", "description"];
        if (!tvbNames.Contains(__instance.name))
            return true;

        var goPath = LocationTriggers.GetFullDisambiguatedPath(__instance.gameObject);
        string[] tvbPaths = [SkillTreeTypeGOPath, SkillTreeTitleGOPath, SkillTreeDescriptionGOPath];
        if (!tvbPaths.Contains(goPath))
            return true;

        SkillNodeData skillNodeData = (SkillNodeData)data;
        if (skillNodeData == null)
            return true;
        if (!UnityObjectNameToSkill.TryGetValue(skillNodeData.name, out var skill))
            return true;
        if (!SkillToLocation.TryGetValue(skill, out var location))
            return true;

        // If this node is still visually a padlock / not available for purchase, don't spoil what item it has
        if (!skillNodeData.IsRevealed)
            return true;

        //Log.Info($"skill tree TVB::UpdateView called for {skillNodeData.name} / {location} / {skillNodeData.IsRevealed} / {skillNodeData.IsRootAcquired}");

        if (APSaveManager.CurrentAPSaveData?.scoutedLocations?.TryGetValue(location, out var scoutedItemInfo) ?? false) {
            if (goPath == SkillTreeTypeGOPath) {
                __instance.textUI.text = LocationNames.locationNames[location];
            } else if (goPath == SkillTreeTitleGOPath) {
                __instance.textUI.text = ScoutInfo.scoutInfoToShopTitle(scoutedItemInfo);
            } else if (goPath == SkillTreeDescriptionGOPath) {
                __instance.textUI.text = ScoutInfo.itemFlagsSummary(scoutedItemInfo) +
                $"\n\n" +
                ScoutInfo.itemFlagsDescription(scoutedItemInfo);
            }
        } else {
            __instance.textUI.text = $"<color=red>ERROR: Location Not Scouted</color>";
        }
        return false;
    }
}
