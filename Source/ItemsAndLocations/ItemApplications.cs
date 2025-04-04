﻿using Archipelago.MultiClient.Net.Helpers;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class ItemApplications {
    // If multiple ItemReceived() events interleave, we and the base agme can end up very confused about the state of our inventory.
    private readonly static object itemReceivedLock = new object();
    public static void ItemReceived(IReceivedItemsHelper receivedItemsHelper) {
        try {
            var receivedItems = new HashSet<long>();
            while (receivedItemsHelper.PeekItem() != null) {
                var itemId = receivedItemsHelper.PeekItem().ItemId;
                receivedItems.Add(itemId);
                receivedItemsHelper.DequeueItem();
            }

            lock (itemReceivedLock) {
                Log.Info($"ItemReceived event with item ids {string.Join(", ", receivedItems)}. Updating these item counts.");
                foreach (var itemId in receivedItems)
                    SyncItemCountWithAPServer(itemId);
            }

            APSaveManager.ScheduleWriteToCurrentSaveFile();
        } catch (Exception ex) {
            Log.Error($"Caught error in APSession_ItemReceived: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    public readonly static Dictionary<Item, int> ApInventory = new Dictionary<Item, int>();
    public static void LoadSavedInventory(APRandomizerSaveData apSaveData) {
        try {
            foreach (var (i, c) in apSaveData.itemsAcquired) {
                ApInventory[Enum.Parse<Item>(i)] = c;
            }
        } catch (Exception ex) {
            Log.Error($"Caught error in LoadSavedInventory: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    // Ensure that our local items state matches APSession.Items.AllItemsReceived. It's possible for AllItemsReceived to be out of date,
    // but in that case the ItemReceived event handler will be invoked as many times as it takes to get up to date.
    public static void SyncInventoryWithServer() {
        try {
            var totalItemsAcquired = ApInventory.Sum(kv => kv.Value);
            var totalItemsReceived = ConnectionAndPopups.APSession!.Items.AllItemsReceived.Count;

            if (totalItemsReceived > totalItemsAcquired) {
                Log.Info($"AP server state has more items ({totalItemsReceived}) than local save data ({totalItemsAcquired}). Attempting to apply new items.");
                foreach (var itemInfo in ConnectionAndPopups.APSession.Items.AllItemsReceived)
                    SyncItemCountWithAPServer(itemInfo.ItemId);
            }
        } catch (Exception ex) {
            Log.Error($"Caught error in SyncInventoryWithServer: '{ex.Message}'\n{ex.StackTrace}");
        }
    }

    public static void SyncItemCountWithAPServer(long itemId) {
        if (!ItemNames.archipelagoIdToItem.ContainsKey(itemId)) {
            ToastManager.Toast(
                $"<color=red>Warning</color>: This mod does not recognize the item id {itemId}, which the Archipelago server just sent us. " +
                $"Check if your mod version matches the .apworld version used to generate this multiworld.");
            return;
        }

        var item = ItemNames.archipelagoIdToItem[itemId];
        var itemEnumName = item.ToString();
        Log.Info($"SyncItemCountWithAPServer mapped id {itemId} to item {item} / {itemEnumName}");

        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"Somehow CurrentAPSaveData is null during a SyncItemCountWithAPServer() call. We should've created or loaded save data when connecting to the AP server.");
            return;
        }
        if (APSaveManager.CurrentAPSaveData.itemsAcquired == null) {
            Log.Error($"Somehow CurrentAPSaveData.itemsAcquired is null during a SyncItemCountWithAPServer() call.");
            return;
        }
        var itemsAcquired = APSaveManager.CurrentAPSaveData.itemsAcquired;
        if (!itemsAcquired.ContainsKey(itemEnumName))
            itemsAcquired[itemEnumName] = 0;

        if (ConnectionAndPopups.APSession == null) {
            Log.Error($"Somehow APSession is null during a SyncItemCountWithAPServer() call. How did this get called without an AP connection?");
            return;
        }
        var itemCountSoFar = ConnectionAndPopups.APSession.Items.AllItemsReceived.Where(i => i.ItemId == itemId).Count();
        var savedCount = itemsAcquired[itemEnumName];
        if (savedCount >= itemCountSoFar) {
            // APSession does client-side caching, so AllItemsReceived having fewer of an item than our save data usually just means the
            // client-side cache is out of date and will be brought up to date shortly with ItemReceived events. Thus, we ignore this case.
            if (savedCount == itemCountSoFar)
                Log.Info($"SyncItemCountWithAPServer ignoring item {item} since savedCount ({savedCount}) == itemCountSoFar ({itemCountSoFar}).");
            else
                Log.Info($"SyncItemCountWithAPServer ignoring item {item} since savedCount ({savedCount}) > itemCountSoFar ({itemCountSoFar}), which usually means the client-side cache hasn't finished updating yet.");
            return;
        } else {
            UpdateItemCount(item, itemCountSoFar);
        }
    }

    // When we receive items on the main menu, especially during connection,
    // hang onto them here until we can apply them for real in the game.
    public static HashSet<(Item, int)> deferredUpdates = new();

    [HarmonyPostfix, HarmonyPatch(typeof(GameCore), "InitializeGameLevel")]
    private static async void GameLevel_InitializeGameCore_Postfix(GameCore __instance, GameLevel newLevel) {
        if (deferredUpdates.Count <= 0)
            return;

        Log.Info($"GameLevel_InitializeGameCore_Postfix has {deferredUpdates.Count} deferredUpdates to execute. Waiting for base game fade-in to finish.");
        await UniTask.Delay(1000);

        foreach (var (i, c) in deferredUpdates)
            UpdateItemCount(i, c);
        deferredUpdates.Clear();
        APSaveManager.WriteCurrentSaveFile();
    }

    public static void UpdateItemCount(Item item, int count) {
        Log.Info($"UpdateItemCount(item={item}, count={count})");

        if (!SingletonBehaviour<GameCore>.IsAvailable()) {
            Log.Info($"UpdateItemCount deferring this item update since we aren't in the game yet");
            deferredUpdates.Add((item, count));
            return;
        }

        var oldCount = ApInventory.GetValueOrDefault(item);

        var (gfd, showPopup) = ApplyItemToPlayer(item, count, oldCount);

        if (gfd != null && count > oldCount) {
            // These are the important parts of ItemGetUIShowAction.Implement(). That method is more complex, but we want more consistency than the base game.
            // Note that ShowGetDescriptablePrompt() will display "No Data" unless the gfd has already been applied

            // Turns out ShowGetDescriptablePrompt() is prone to native Unity crashes. It's most consistent when called
            // immediately after an AP location check + item receipt for an important item like Mystic Nymph.
            // Unfortunately a delayed call is even more crash-y, often crashing just on toggling nymph without any AP networking.
            // So for the time being we have to settle for "notifications" on all items.
            SingletonBehaviour<UIManager>.Instance.ShowDescriptableNitification(gfd);

            SingletonBehaviour<SaveManager>.Instance.AutoSave(SaveManager.SaveSceneScheme.FlagOnly, forceShowIcon: true);
        }

        ApInventory[item] = count;

        Jiequan1Fight.OnItemUpdate(item);
        LadyESoulscapeEntrance.OnItemUpdate(item);

        if (APSaveManager.CurrentAPSaveData == null) {
            Log.Error($"UpdateItemCount(item={item}, count={count}) unable to write to save file because there is no save file. If you're the developer doing hot reloading, this is normal.");
        } else {
            APSaveManager.CurrentAPSaveData.itemsAcquired[item.ToString()] = count;
        }
    }

    public static bool IsSolSeal(Item item) {
        return (item >= Item.SealOfKuafu && item <= Item.SealOfNuwa);
    }

    public static int GetSolSealsCount() {
        var sealCount = 0;
        for (var seal = Item.SealOfKuafu; seal <= Item.SealOfNuwa; seal++) {
            if (ApInventory.ContainsKey(seal) && ApInventory[seal] > 0)
                sealCount++;
        }
        return sealCount;
    }

    private static (GameFlagDescriptable?, bool) ApplyItemToPlayer(Item item, int count, int oldCount) {
        Log.Info($"ApplyItemToPlayer(item={item}, count={count}, oldCount={oldCount})");

        // we're unlikely to use these, but: RollDodgeAbility is regular ground dash
        PlayerAbilityData? ability = null;
        switch (item) {
            case Item.TaiChiKick: ability = Player.i.mainAbilities.ParryJumpKickAbility; break;
            case Item.AirDash: ability = Player.i.mainAbilities.RollDodgeInAirUpgrade; break;
            case Item.ChargedStrike: ability = Player.i.mainAbilities.ChargedAttackAbility; break;
            case Item.CloudLeap: ability = Player.i.mainAbilities.AirJumpAbility; break;
            case Item.SuperMutantBuster: ability = Player.i.mainAbilities.KillZombieFooAbility; break;
            case Item.UnboundedCounter: ability = Player.i.mainAbilities.ParryCounterAbility; break;
            case Item.MysticNymphScoutMode: ability = Player.i.mainAbilities.HackDroneAbility; break;
            default: break;
        }
        if (ability != null) {
            ability.acquired.SetCurrentValue(count > 0);
            ability.equipped.SetCurrentValue(count > 0);
            if (ability.BindingItemPicked != null) {
                Log.Info($"!!! {ability} has BindingItemPicked={ability.BindingItemPicked} !!!");
            }
            return (ability, true);
        }

        // TODO: is there a better way than UIManager to get at the GameFlagDescriptables for every inventory item? SaveManager.allFlags.flagDict???
        List<ItemDataCollection> inventory = SingletonBehaviour<UIManager>.Instance.allItemCollections;
        GameFlagDescriptable? solSealInventoryItem = null;
        switch (item) {
            case Item.SealOfKuafu: solSealInventoryItem = inventory[0].rawCollection[0]; break;
            case Item.SealOfGoumang: solSealInventoryItem = inventory[0].rawCollection[1]; break;
            case Item.SealOfYanlao: solSealInventoryItem = inventory[0].rawCollection[2]; break;
            case Item.SealOfJiequan: solSealInventoryItem = inventory[0].rawCollection[3]; break;
            case Item.SealOfLadyEthereal: solSealInventoryItem = inventory[0].rawCollection[4]; break;
            case Item.SealOfJi: solSealInventoryItem = inventory[0].rawCollection[5]; break;
            case Item.SealOfFuxi: solSealInventoryItem = inventory[0].rawCollection[6]; break;
            case Item.SealOfNuwa: solSealInventoryItem = inventory[0].rawCollection[7]; break;
            default: break;
        }
        if (solSealInventoryItem != null) {
            solSealInventoryItem.acquired.SetCurrentValue(count > 0);
            solSealInventoryItem.unlocked.SetCurrentValue(count > 0);
            ((ItemData)solSealInventoryItem).ownNum.SetCurrentValue(count);
            return (solSealInventoryItem, true);
        }

        GameFlagDescriptable? inventoryItem = null;
        switch (item) {
            case Item.BloodyCrimsonHibiscus: inventoryItem = inventory[0].rawCollection[8]; break;
            case Item.AncientPenglaiBallad: inventoryItem = inventory[0].rawCollection[9]; break;
            case Item.PoemHiddenInTheImmortalsPortrait: inventoryItem = inventory[0].rawCollection[10]; break;
            case Item.ThunderburstBomb: inventoryItem = inventory[0].rawCollection[11]; break;
            case Item.GeneEradicator: inventoryItem = inventory[0].rawCollection[12]; break;
            case Item.HomingDarts: inventoryItem = inventory[0].rawCollection[13]; break;
            case Item.SoulSeveringBlade: inventoryItem = inventory[0].rawCollection[14]; break;
            case Item.FirestormRing: inventoryItem = inventory[0].rawCollection[15]; break;
            //case Item.YellowDragonsnakeVenomSac: inventoryItem = inventory[0].rawCollection[16]; break; // post-all-poisons
            //case Item.YellowDragonsnakeMedicinalBrew: inventoryItem = inventory[0].rawCollection[17]; break; // post-all-poisons
            case Item.JisHair: inventoryItem = inventory[0].rawCollection[18]; break;
            case Item.TianhuoSerum: inventoryItem = inventory[0].rawCollection[19]; break;
            case Item.ElevatorAccessToken: inventoryItem = inventory[0].rawCollection[20]; break;
            case Item.RhizomaticBomb: inventoryItem = inventory[0].rawCollection[21]; break;
            //case Item.RhizomaticArrow: inventoryItem = inventory[0].rawCollection[22]; break; // post-PonR
            case Item.AbandonedMinesAccessToken: inventoryItem = inventory[0].rawCollection[23]; break;
            //case Item.FriendPhoto: inventoryItem = inventory[0].rawCollection[24]; break; // post-PonR
            // Tao Fruits are handled separately because of the associated skill point increases

            // inventory[1]
            case Item.AbandonedMinesChip: inventoryItem = inventory[1].rawCollection[0]; break;
            case Item.PowerReservoirChip: inventoryItem = inventory[1].rawCollection[1]; break;
            case Item.AgriculturalZoneChip: inventoryItem = inventory[1].rawCollection[2]; break;
            case Item.WarehouseZoneChip: inventoryItem = inventory[1].rawCollection[3]; break;
            case Item.TransmutationZoneChip: inventoryItem = inventory[1].rawCollection[4]; break;
            case Item.CentralCoreChip: inventoryItem = inventory[1].rawCollection[5]; break;
            case Item.EmpyreanDistrictChip: inventoryItem = inventory[1].rawCollection[6]; break;
            case Item.GrottoOfScripturesChip: inventoryItem = inventory[1].rawCollection[7]; break;
            case Item.ResearchCenterChip: inventoryItem = inventory[1].rawCollection[8]; break;
            case Item.FusangAmulet: inventoryItem = inventory[1].rawCollection[9]; break;
            case Item.MultiToolKit: inventoryItem = inventory[1].rawCollection[10]; break;
            case Item.TiandaoAcademyPeriodical: inventoryItem = inventory[1].rawCollection[11]; break;
            case Item.KunlunImmortalPortrait: inventoryItem = inventory[1].rawCollection[12]; break;
            case Item.QiankunBoard: inventoryItem = inventory[1].rawCollection[13]; break;
            case Item.AncientSheetMusic: inventoryItem = inventory[1].rawCollection[14]; break;
            case Item.UnknownSeed: inventoryItem = inventory[1].rawCollection[15]; break;
            case Item.VirtualRealityDevice: inventoryItem = inventory[1].rawCollection[16]; break;
            case Item.AntiqueVinylRecord: inventoryItem = inventory[1].rawCollection[17]; break;
            case Item.ReadyToEatRations: inventoryItem = inventory[1].rawCollection[18]; break;
            case Item.SwordOfJie: inventoryItem = inventory[1].rawCollection[19]; break;
            case Item.RedGuifangClay: inventoryItem = inventory[1].rawCollection[20]; break;
            case Item.TheFourTreasuresOfTheStudy: inventoryItem = inventory[1].rawCollection[21]; break;
            //case Item.CrimsonHibiscus: inventoryItem = inventory[1].rawCollection[22]; break; // not a thing since we skip the intro
            case Item.GMFertilizer: inventoryItem = inventory[1].rawCollection[23]; break;
            case Item.PenglaiRecipeCollection: inventoryItem = inventory[1].rawCollection[24]; break;

            case Item.MedicinalCitrine: inventoryItem = inventory[1].rawCollection[25]; break;
            case Item.GoldenYinglongEgg: inventoryItem = inventory[1].rawCollection[26]; break;
            //case Item.MoltedTianmaHide: inventoryItem = inventory[1].rawCollection[27]; break; // shop item
            case Item.ResidualHair: inventoryItem = inventory[1].rawCollection[28]; break;
            case Item.Oriander: inventoryItem = inventory[1].rawCollection[29]; break;
            case Item.TurtleScorpion: inventoryItem = inventory[1].rawCollection[30]; break;
            case Item.PlantagoFrog: inventoryItem = inventory[1].rawCollection[31]; break;
            case Item.PorcineGem: inventoryItem = inventory[1].rawCollection[32]; break;
            /*case Item.BallOfFlavor: inventoryItem = inventory[1].rawCollection[33]; break; // shop item
            case Item.DragonsWhip: inventoryItem = inventory[1].rawCollection[34]; break; // shop item
            case Item.Necroceps: inventoryItem = inventory[1].rawCollection[35]; break; // shop item
            case Item.Guiseng: inventoryItem = inventory[1].rawCollection[36]; break; // shop item
            case Item.ThunderCentipede: inventoryItem = inventory[1].rawCollection[37]; break; // shop item
            case Item.WallClimbingGecko: inventoryItem = inventory[1].rawCollection[38]; break; // shop item
            case Item.GutwrenchFruit: inventoryItem = inventory[1].rawCollection[39]; break;*/ // shop item
            //case Item.DamagedFusangAmulet: inventoryItem = inventory[1].rawCollection[40]; break; // post-all-poisons
            case Item.LegendOfThePorkyHeroes: inventoryItem = inventory[1].rawCollection[41]; break;
            case Item.PortraitOfYi: inventoryItem = inventory[1].rawCollection[42]; break;

            // inventory[2]
            case Item.BasicComponent: inventoryItem = inventory[2].rawCollection[0]; break;
            case Item.StandardComponent: inventoryItem = inventory[2].rawCollection[1]; break;
            case Item.AdvancedComponent: inventoryItem = inventory[2].rawCollection[2]; break;
            case Item.NobleRing: inventoryItem = inventory[2].rawCollection[3]; break;
            //case Item.ShuanshuanCoin: inventoryItem = inventory[2].rawCollection[4]; break; // technically a shop item
            //case Item.JinMedallion: inventoryItem = inventory[2].rawCollection[5]; break; // post-all-poisons
            case Item.PassengerTokenAShou: inventoryItem = inventory[2].rawCollection[6]; break;
            case Item.PassengerTokenZouyan: inventoryItem = inventory[2].rawCollection[7]; break;
            case Item.PassengerTokenXipu: inventoryItem = inventory[2].rawCollection[8]; break;
            case Item.PassengerTokenJihai: inventoryItem = inventory[2].rawCollection[9]; break;
            case Item.PassengerTokenYangfan: inventoryItem = inventory[2].rawCollection[10]; break;
            case Item.PassengerTokenAimu: inventoryItem = inventory[2].rawCollection[11]; break;
            case Item.PassengerTokenShiyangyue: inventoryItem = inventory[2].rawCollection[12]; break;
            case Item.DarkSteel: inventoryItem = inventory[2].rawCollection[13]; break;
            case Item.HerbCatalyst: inventoryItem = inventory[2].rawCollection[14]; break;

            // inventory[3] - all of these either aren't randomized items, or are handled elsewhere
            //Azure Sand
            //Jin
            //Root Stabilizer ???
            //Fusang Horn
            //Mystic Nymph
            //Super Mutant Buster
            //Computing Unit
            //Azure Bow
            //Azure Sand Magazine
            //Medicine Pipe
            //Pipe Vial
            default: break;
        }
        if (inventoryItem != null) {
            inventoryItem.acquired.SetCurrentValue(count > 0);
            inventoryItem.unlocked.SetCurrentValue(count > 0);
            if (inventoryItem is ItemData) {
                var apCountDiff = count - oldCount;
                var oldInGameCount = ((ItemData)inventoryItem).ownNum.CurrentValue;
                var newInGameCount = oldInGameCount + apCountDiff;
                Log.Info($"{item} apCountDiff={apCountDiff}, oldInGameCount={oldInGameCount}, newInGameCount={newInGameCount}");
                ((ItemData)inventoryItem).ownNum.SetCurrentValue(newInGameCount);
            }
            // TODO: do we also need pickItemEventRaiser.Raise(); ? so far doesn't look like it
            return (inventoryItem, true);
        }

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

            return (taoFruitInventoryItem, true);
        }

        // TODO: is there a better way than UIManager to get at the GameFlagDescriptables for every database entry?
        // We only care about allPediaCollections[2]. [0] is character entries, which we don't randomize. [1] seems unused.
        List<GameFlagDescriptable> database = SingletonBehaviour<UIManager>.Instance.allPediaCollections[2].rawCollection;
        GameFlagDescriptable? databaseEntry = null;
        switch (item) {
            case Item.DeadPersonsNote: databaseEntry = database[0]; break;
            case Item.CampScroll: databaseEntry = database[1]; break;
            case Item.ApemanSurveillanceFootage: databaseEntry = database[2]; break;
            case Item.CouncilDigitalSignage: databaseEntry = database[3]; break;
            case Item.NewKunlunLaunchMemorial: databaseEntry = database[4]; break;
            case Item.AnomalousRootNode: databaseEntry = database[5]; break;
            case Item.RhizomaticEnergyMeter: databaseEntry = database[6]; break;
            case Item.DuskGuardianRecordingDevice1: databaseEntry = database[7]; break;
            case Item.RadiantPagodaControlPanel: databaseEntry = database[8]; break;
            case Item.LakeYaochiStele: databaseEntry = database[9]; break;
            case Item.AncientCavePainting: databaseEntry = database[10]; break;
            case Item.CoffinInscription: databaseEntry = database[11]; break;
            case Item.YellowWaterReport: databaseEntry = database[12]; break;
            case Item.MutatedCrops: databaseEntry = database[13]; break;
            case Item.WaterSynthesisPipelinePanel: databaseEntry = database[14]; break;
            case Item.DuskGuardianRecordingDevice2: databaseEntry = database[15]; break;
            case Item.FarmlandMarkings: databaseEntry = database[16]; break;
            case Item.CouncilTenets: databaseEntry = database[17]; break;
            case Item.TransmutationFurnaceMonitor: databaseEntry = database[18]; break;
            case Item.EvacuationNoticeForMiners: databaseEntry = database[19]; break;
            case Item.WarehouseDatabase: databaseEntry = database[20]; break;
            case Item.DuskGuardianRecordingDevice3: databaseEntry = database[21]; break;
            case Item.AncientWeaponConsole: databaseEntry = database[22]; break;
            case Item.HexachremVaultScroll: databaseEntry = database[23]; break;
            case Item.PrisonersBambooScroll1: databaseEntry = database[24]; break;
            case Item.PrisonersBambooScroll2: databaseEntry = database[25]; break;
            case Item.JieClanFamilyPrecept: databaseEntry = database[26]; break;
            case Item.GuardProductionStation: databaseEntry = database[27]; break;
            case Item.DuskGuardianRecordingDevice4: databaseEntry = database[28]; break;
            case Item.PharmacyPanel: databaseEntry = database[29]; break;
            case Item.HaotianSphereModel: databaseEntry = database[30]; break;
            case Item.CaveStoneInscription: databaseEntry = database[31]; break;
            case Item.GalacticDockSign: databaseEntry = database[32]; break;
            case Item.StoneCarvings: databaseEntry = database[33]; break;
            case Item.SecretMural1: databaseEntry = database[34]; break;
            case Item.SecretMural2: databaseEntry = database[35]; break;
            case Item.SecretMural3: databaseEntry = database[36]; break;
            case Item.StowawaysCorpse: databaseEntry = database[37]; break;
            case Item.UndergroundWaterTower: databaseEntry = database[38]; break;
            case Item.EmpyreanBulletinBoard: databaseEntry = database[39]; break;
            case Item.DuskGuardianRecordingDevice5: databaseEntry = database[40]; break;
            case Item.VitalSanctumTowerMonitoringPanel: databaseEntry = database[41]; break;
            case Item.DuskGuardianRecordingDevice6: databaseEntry = database[42]; break;
            case Item.DuskGuardianHeadquartersScreen: databaseEntry = database[43]; break;
            //case Item.RootCoreMonitoringDevice: databaseEntry = database[44]; break; // post-PonR
            default: break;
        }
        if (databaseEntry != null) {
            databaseEntry.acquired.SetCurrentValue(count > 0);
            databaseEntry.unlocked.SetCurrentValue(count > 0);
            return (databaseEntry, true);
        }

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
            return (jadeEntry, true);
        }

        string? arrowPWDFlag = null;
        switch (item) {
            // Note these are the "level 1" flags, there are others for levels 2 and 3
            case Item.ArrowCloudPiercer: arrowPWDFlag = "7837bd6bb550641d8a9f30492603c5eePlayerWeaponData"; break;
            case Item.ArrowShadowHunter: arrowPWDFlag = "3949bc0edba197d459f5d2d7f15c72e0PlayerWeaponData"; break;
            case Item.ArrowThunderBuster: arrowPWDFlag = "ef8f7eb3bcd7b444f80d5da539f3b133PlayerWeaponData"; break;
            default: break;
        }
        if (arrowPWDFlag != null) {
            var arrowPWD = (PlayerWeaponData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict[arrowPWDFlag];
            arrowPWD.acquired?.SetCurrentValue(count > 0); // .unlocked and .equipped appear to be unnecessary

            // not worth trying to figure out if the bow "should" be disabled, since this can't happen in practice anyway
            if (count > 0) {
                EnableAzureBow(true);
            }

            return (arrowPWD, true);
        }

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
            return (jinGFD, false);
        }

        if (item == Item.ComputingUnit) {
            return ApplyComputingUnit(count);
        }
        if (item == Item.PipeVial) {
            return ApplyPipeVial(count);
        }

        ToastManager.Toast($"unable to apply item {item} (count = {count})");
        return (null, false);
    }

    private static void EnableAzureBow(bool enable) {
        Log.Info($"EnableAzureBow(enable={enable})");

        // This is the actual in-game bow-firing ability
        Player.i.mainAbilities.ArrowAbility.acquired?.SetCurrentValue(enable);
        // note that "2efd376b4493d40fca29f9e3d49669e9PlayerWeaponData" is the same PWD object as ArrowAbility, just two different ways of getting to it

        // These are additional flags that only matter in the pause menus, but are meant to go along with the bow
        var azureBowInventoryEntry = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["49f2fd2c691313f47970b15b58279418ItemData"];
        var azureSandInventoryEntry = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["25b45e1c416880d41a1f1444e45c24d2ItemData"];
        var azureBowOnStatusScreen = (ItemData)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["a68fe303d0077264aa66218d3900f0edItemData"];
        foreach (var itemData in new ItemData[] { azureBowInventoryEntry, azureSandInventoryEntry, azureBowOnStatusScreen }) {
            itemData.acquired?.SetCurrentValue(enable);
            itemData.unlocked?.SetCurrentValue(enable);
        }
    }

    /*
     * Computing Units need a bunch of specialized code unfortunately. While the CU count displayed in the Inventory menu
     * is just an integer in FlagDict as you'd expect, the max jade cost is not. Instead it's a set of StatModifiers applied
     * at runtime by 8 different PlayerAbilityData objects that each represent only one of the possible CU upgrades.
     * And in order to get the count right, we also need to monitor the CUs that aren't shuffled by AP.
     */

    private static string[] shopCUs = [
        "7e52ba8ef1da6e946806ba1791d92791PlayerAbilityData",
        "5df7e883274aabc489885525a490b5c3PlayerAbilityData",
        "d5b834eb32c40e64f9ce89f5033162c8PlayerAbilityData",
        "d6dc79cb7147ff14aa68f5658a285f22PlayerAbilityData",
    ];
    private static string[] otherCUs = [
        "07577e5ef6cbe394db65f58dfc8f7908PlayerAbilityData",
        "df57aee56d7eec1459ffe946cac8523ePlayerAbilityData",
        "cd52a0bd3cef5634bb0083c19c77e8f3PlayerAbilityData",
        "c191c1bc2afb8d84c870e1858b7ee156PlayerAbilityData",
    ];

    private static MethodInfo padActivate = AccessTools.Method(typeof(PlayerAbilityData), "Activate", []);
    private static MethodInfo padDeactivate = AccessTools.Method(typeof(PlayerAbilityData), "DeActivate", []);

    private static (GameFlagDescriptable?, bool) ApplyComputingUnit(int apCount) {
        Log.Info($"ApplyComputingUnit(apCount={apCount})");
        var unshuffledCUs = shopCUs;
        var shuffledCUs = otherCUs;

        var maxAPCUs = shuffledCUs.Length;
        if (apCount < 0 || apCount > maxAPCUs) {
            Log.Error($"ApplyComputingUnit passed {apCount}, but apCount must be between 0 and (on this slot) {maxAPCUs}");
            return (null, false);
        }

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        for (var i = 0; i < shuffledCUs.Length; i++) {
            var flagId = shuffledCUs[i];
            var pad = flagDict[flagId] as PlayerAbilityData;

            //Log.Info($"ApplyComputingUnit setting {i}-th (shuffled by AP) PAD to {(i < apCount)}");
            pad.unlocked.CurrentValue = (i < apCount);
            pad.acquired.CurrentValue = (i < apCount);
            if (i < apCount) {
                padActivate.Invoke(pad, []);
            } else {
                padDeactivate.Invoke(pad, []);
            }
        }

        /*foreach (var id in unshuffledCUs) {
            var pad = flagDict[id] as PlayerAbilityData;
            Log.Info($"ApplyComputingUnit unshuffledCU {id} / {pad.name} is {pad.acquired.CurrentValue}");
        }*/

        var unshuffledCUCount = unshuffledCUs.Sum(flagId =>
            (flagDict[flagId] as PlayerAbilityData)!.acquired.CurrentValue ? 1 : 0);
        var inGameInventoryCount = unshuffledCUCount + apCount;
        //Log.Info($"ApplyComputingUnit unshuffledCUCount={unshuffledCUCount}, inGameInventoryCount={inGameInventoryCount}");

        var cuInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[6];
        cuInventoryItem.unlocked.CurrentValue = (inGameInventoryCount > 0);
        cuInventoryItem.acquired.CurrentValue = (inGameInventoryCount > 0);
        (cuInventoryItem as ItemData)!.ownNum.CurrentValue = inGameInventoryCount;

        return (cuInventoryItem, true);
    }

    /*
     * Pipe Vials follow the same pattern as Computing Units.
     */

    private static string[] shopPVs = [
        "5c864e36f2cef9d4f8c24c0aa84010bePlayerAbilityData", // (升級) 煙斗使用次數_Lv1_議會販賣機
        "3f6040257515a41499d36e910a4a6e79PlayerAbilityData", // (升級) 煙斗使用次數_Lv2_議會販賣機
        "ec1fe3b64944cd643b2020f35e82f023PlayerAbilityData", // (升級) 煙斗使用次數_Lv3_議會販賣機
        "dd5adfbe0ac50fe4795b796153ee646dPlayerAbilityData", // (升級) 煙斗使用次數_Lv4_議會販賣機
    ];
    private static string[] otherPVs = [
        "ce7166af4ef39d7468c4ccc464fd90b9PlayerAbilityData", // (升級) 煙斗使用次數_AG_SG1
        "41c960dfcaaf7a14f813342db16f0481PlayerAbilityData", // (升級) 煙斗使用次數_A3SG4日昇樓內 (= Inside Daybreak Tower)
    ];

    // TODO: move the similar parts of CU and PV methods into a shared helper? maybe after we're sure there aren't more weird items
    private static (GameFlagDescriptable?, bool) ApplyPipeVial(int apCount) {
        Log.Info($"ApplyPipeVial(apCount={apCount})");
        var unshuffledPVs = shopPVs;
        var shuffledPVs = otherPVs;

        var maxAPPVs = shuffledPVs.Length;
        if (apCount < 0 || apCount > maxAPPVs) {
            Log.Error($"ApplyPipeVial passed {apCount}, but apCount must be between 0 and (on this slot) {maxAPPVs}");
            return (null, false);
        }

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;

        for (var i = 0; i < shuffledPVs.Length; i++) {
            var flagId = shuffledPVs[i];
            var pad = flagDict[flagId] as PlayerAbilityData;

            //Log.Info($"ApplyPipeVial setting {i}-th (shuffled by AP) PAD to {(i < apCount)}");
            pad.unlocked.CurrentValue = (i < apCount);
            pad.acquired.CurrentValue = (i < apCount);
            if (i < apCount) {
                padActivate.Invoke(pad, []);
            } else {
                padDeactivate.Invoke(pad, []);
            }
        }

        /*foreach (var id in unshuffledPVs) {
            var pad = flagDict[id] as PlayerAbilityData;
            Log.Info($"ApplyPipeVial unshuffledPV {id} / {pad.name} is {pad.acquired.CurrentValue}");
        }*/

        var unshuffledPVCount = unshuffledPVs.Sum(flagId =>
            (flagDict[flagId] as PlayerAbilityData)!.acquired.CurrentValue ? 1 : 0);
        var inGameInventoryCount = unshuffledPVCount + apCount;
        //Log.Info($"ApplyPipeVial unshuffledPVCount={unshuffledPVCount}, inGameInventoryCount={inGameInventoryCount}");

        var cuInventoryItem = SingletonBehaviour<UIManager>.Instance.allItemCollections[3].rawCollection[10];
        cuInventoryItem.unlocked.CurrentValue = (inGameInventoryCount > 0);
        cuInventoryItem.acquired.CurrentValue = (inGameInventoryCount > 0);
        (cuInventoryItem as ItemData)!.ownNum.CurrentValue = inGameInventoryCount;

        return (cuInventoryItem, true);
    }
}
