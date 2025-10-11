using HarmonyLib;
using NineSolsAPI;
using System.Collections.Generic;
using static HarmonyLib.AccessTools;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
class BossScaling
{
    private static Dictionary<string, int> BossToVanillaOrder = new Dictionary<string, int> {
        { "StealthGameMonster_SpearHorseMan", 1 },
        { "StealthGameMonster_GouMang Variant", 2 },
        { "StealthGameMonster_BossZombieSpear", 2 },
        { "StealthGameMonster_BossZombieHammer", 2 },
        { "Monster_GiantMechClaw", 3 },
        { "StealthGameMonster_Boss_JieChuan", 4 },
        { "StealthGameMonster_Boss_ButterFly Variant", 5 },
        { "StealthGameMonster_伏羲_新", 6 },
        { "StealthGameMonster_新女媧 Variant", 6 },
        { "StealthGameMonster_Boss_Jee", 7 },
        //{ "Boss_Yi Gung", 8 },
    };

    private static Dictionary<string, string> BossToDisplayName = new Dictionary<string, string> {
        { "StealthGameMonster_SpearHorseMan", "Yingzhao" },
        { "StealthGameMonster_GouMang Variant", "Goumang" },
        { "StealthGameMonster_BossZombieSpear", "Goumang's Small Jiangshi" },
        { "StealthGameMonster_BossZombieHammer", "Goumang's Big Jiangshi" },
        { "Monster_GiantMechClaw", "Sky Rending Claw" },
        { "StealthGameMonster_Boss_JieChuan", "Jiequan" },
        { "StealthGameMonster_Boss_ButterFly Variant", "Lady Ethereal" },
        { "StealthGameMonster_伏羲_新", "Fuxi" },
        { "StealthGameMonster_新女媧 Variant", "Nuwa" },
        { "StealthGameMonster_Boss_Jee", "Ji" },
        //{ "Boss_Yi Gung", "Eigong" },
    };

    private static string BossScaling_EncounteredBossesListName = "BossesEncounteredInGame";

    private static Dictionary<string, string> BossToSaveDataName = new Dictionary<string, string> {
        { "StealthGameMonster_SpearHorseMan", "Yingzhao" },
        { "StealthGameMonster_GouMang Variant", "Goumang" },
        { "StealthGameMonster_BossZombieSpear", "Goumang" },
        { "StealthGameMonster_BossZombieHammer", "Goumang" },
        { "Monster_GiantMechClaw", "Yanlao" },
        { "StealthGameMonster_Boss_JieChuan", "Jiequan" },
        { "StealthGameMonster_Boss_ButterFly Variant", "Lady Ethereal" },
        { "StealthGameMonster_伏羲_新", "Fengs" },
        { "StealthGameMonster_新女媧 Variant", "Fengs" },
        { "StealthGameMonster_Boss_Jee", "Ji" },
        //{ "Boss_Yi Gung", "Eigong" },
    };

    private static List<string> AlreadyScaledBosses = new();

    static FieldRef<MonsterStat, float> BaseAttackValueRef => FieldRefAccess<MonsterStat, float>("BaseAttackValue");
    static FieldRef<MonsterStat, float> BaseHealthValueRef => FieldRefAccess<MonsterStat, float>("BaseHealthValue");

    [HarmonyPrefix, HarmonyPatch(typeof(MonsterBase), "Awake")]
    private static void MonsterBase_Awake(MonsterBase __instance) {
        if (!APRandomizer.Instance.BossScalingSetting.Value) {
            Log.Debug($"BossScaling doing nothing because the 'Boss Scaling' mod setting is off.");
            return;
        }

        var name = __instance.name;
        if (PlayerGamePlayData.Instance.memoryMode.CurrentValue) {
            Log.Debug($"BossScaling ignoring {name} because this is Battle Memories mode");
            return;
        }
        if (!BossToVanillaOrder.ContainsKey(name)) {
            Log.Debug($"BossScaling ignoring {name} because it's not in our boss list");
            return;
        }
        if (name == "StealthGameMonster_Boss_JieChuan" && SingletonBehaviour<GameCore>.Instance.gameLevel.name == "A5_S1") {
            Log.Info($"BossScaling ignoring {name} because this is the unwinnable Jiequan 1 fight, not the 'real' Jiequan");
            return;
        }

        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"BossScaling aborting because APSession is null. If you're the developer doing hot reloading, this is normal.");
            return;
        }
        APSaveManager.CurrentAPSaveData.persistentModStringLists.TryGetValue(BossScaling_EncounteredBossesListName, out List<string>? encounteredBossesList);
        var updateSaveData = false;
        if (encounteredBossesList == null) {
            encounteredBossesList = new();
            updateSaveData = true;
        }
        var saveDataName = BossToSaveDataName[name];
        if (!encounteredBossesList.Contains(saveDataName)) {
            encounteredBossesList.Add(saveDataName);
            updateSaveData = true;
        }
        var actualOrder = encounteredBossesList.IndexOf(saveDataName) + 1;
        if (updateSaveData) {
            APSaveManager.CurrentAPSaveData.persistentModStringLists[BossScaling_EncounteredBossesListName] = encounteredBossesList;
            APSaveManager.ScheduleWriteToCurrentSaveFile();
        }

        var vanillaOrder = BossToVanillaOrder[name];
        if (actualOrder == vanillaOrder) {
            ToastManager.Toast($"{BossToDisplayName.GetValueOrDefault(name)}'s health and damage have been left unchanged, since you encountered them as boss #{actualOrder} just like vanilla.");
            return;
        }

        float scaledAttack, scaledHealth;
        if (actualOrder > vanillaOrder) {
            (scaledAttack, scaledHealth) = ScaleBetweenVanillaAndBattleMemories(__instance, actualOrder); // "late" boss, scale it up toward Battle Memories
        } else {
            (scaledAttack, scaledHealth) = ScaleBetweenZeroAndVanilla(__instance, actualOrder); // "early" boss, scale it down toward zero
        }

        var stats = __instance.monsterStat;
        var baseAttack = BaseAttackValueRef.Invoke(stats);
        var baseHealth = BaseHealthValueRef.Invoke(stats);
        if (AlreadyScaledBosses.Contains(name)) {
            Log.Info($"BossScaling skipping {name} because we already scaled it this session");
        } else {
            Log.Info($"BossScaling actually applying scaled stats to {name}");
            BaseAttackValueRef.Invoke(stats) = scaledAttack;
            BaseHealthValueRef.Invoke(stats) = scaledHealth;
            AlreadyScaledBosses.Add(name);
        }

        ToastManager.Toast($"{BossToDisplayName.GetValueOrDefault(name)}'s " +
            $"health and damage have been set to <color=orange>{scaledHealth / baseHealth * 100}% and {scaledAttack / baseAttack * 100}%</color> of their vanilla values\n" +
            $"because you're encountering them as <color=orange>boss #{actualOrder} instead of #{vanillaOrder}</color>");
    }

    private static (float, float) ScaleBetweenVanillaAndBattleMemories(MonsterBase __instance, int actualOrder) {
        var name = __instance.name;
        var vanillaOrder = BossToVanillaOrder[name];
        var stats = __instance.monsterStat;
        var baseAttack = BaseAttackValueRef.Invoke(stats);
        var baseHealth = BaseHealthValueRef.Invoke(stats);

        var bmAttack = stats.BossMemoryAttackScale * baseAttack;
        var bmHealth = stats.BossMemoryHealthScale * baseHealth;

        // We pretend the Battle Memories values represent an expected endgame/Eigong-fighting Yi, i.e. fight number 8.
        // BM Eigong does have multipliers >1, so arguably 8 is less than BM, but when I tried 9 the scaling seemed milder than we want in practice.

        // remember y = mx + b or "slope-intercept form" from algebra class?
        var attackSlope = (bmAttack - baseAttack) / (8 - vanillaOrder);
        var attackIntercept = baseAttack - (attackSlope * vanillaOrder);
        var healthSlope = (bmHealth - baseHealth) / (8 - vanillaOrder);
        var healthIntercept = baseHealth - (healthSlope * vanillaOrder);

        var scaledAttack = (attackSlope * actualOrder) + attackIntercept;
        var scaledHealth = (healthSlope * actualOrder) + healthIntercept;
        return (scaledAttack, scaledHealth);
    }

    private static (float, float) ScaleBetweenZeroAndVanilla(MonsterBase __instance, int actualOrder) {
        var name = __instance.name;
        var vanillaOrder = BossToVanillaOrder[name];
        var stats = __instance.monsterStat;
        var baseAttack = BaseAttackValueRef.Invoke(stats);
        var baseHealth = BaseHealthValueRef.Invoke(stats);

        // We pretend zero health and zero damage represents a "boss #0", so that it's impossible for an actual boss at #1 or later to get non-positive health or damage.

        // remember y = mx + b or "slope-intercept form" from algebra class? except here b is zero by definition
        var attackSlope = (baseAttack) / (vanillaOrder);
        var healthSlope = (baseHealth) / (vanillaOrder);

        var scaledAttack = (attackSlope * actualOrder);
        var scaledHealth = (healthSlope * actualOrder);
        return (scaledAttack, scaledHealth);
    }
}
