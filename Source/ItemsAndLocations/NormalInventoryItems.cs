using System.Collections.Generic;

namespace ArchipelagoRandomizer;

internal class NormalInventoryItems {
    public static bool ApplyNormalInventoryItemToPlayer(Item item, int count, int oldCount) {
        // TODO: is there a better way than UIManager to get at the GameFlagDescriptables for every inventory item? SaveManager.allFlags.flagDict???
        // TODO: why are we handlign sol seals in a separate block?
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
            NotifyAndSave.Default(solSealInventoryItem, count, oldCount);
            return true;
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
            NotifyAndSave.Default(inventoryItem, count, oldCount);
            return true;
        }

        return false;
    }
}
