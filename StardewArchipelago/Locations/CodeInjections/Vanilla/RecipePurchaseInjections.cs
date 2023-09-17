﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Util;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class RecipePurchaseInjections
    {
        private const string CHEFSANITY_LOCATION_PREFIX = "Learn Recipe ";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
        }

        // public static Dictionary<ISalable, int[]> getSaloonStock()
        public static bool GetSaloonStock_ReplaceRecipesWithChefsanityChecks_Prefix(ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                var saloonStock = new Dictionary<ISalable, int[]>();
                Utility.AddStock(saloonStock, new Object(Vector2.Zero, 346, int.MaxValue));
                Utility.AddStock(saloonStock, new Object(Vector2.Zero, 196, int.MaxValue));
                Utility.AddStock(saloonStock, new Object(Vector2.Zero, 216, int.MaxValue));
                Utility.AddStock(saloonStock, new Object(Vector2.Zero, 224, int.MaxValue));
                Utility.AddStock(saloonStock, new Object(Vector2.Zero, 206, int.MaxValue));
                Utility.AddStock(saloonStock, new Object(Vector2.Zero, 395, int.MaxValue));

                AddArchipelagoRecipesToSaloonStock(saloonStock);

                if (Game1.dishOfTheDay.Stack > 0 && !Utility.getForbiddenDishesOfTheDay().Contains<int>(Game1.dishOfTheDay.ParentSheetIndex))
                {
                    Utility.AddStock(saloonStock, Game1.dishOfTheDay.getOne() as Object, Game1.dishOfTheDay.Price, Game1.dishOfTheDay.Stack);
                }

                Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.Saloon, saloonStock);
                if (Game1.player.activeDialogueEvents.ContainsKey("willyCrabs"))
                {
                    Utility.AddStock(saloonStock, new Object(Vector2.Zero, 732, int.MaxValue));
                }

                __result = saloonStock;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetSaloonStock_ReplaceRecipesWithChefsanityChecks_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_ReplaceTropicalCurryWithChefsanityCheck_Prefix(IslandSouth __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (tileLocation.X != 14 || tileLocation.Y != 22 || __instance.getCharacterFromName("Gus") == null ||
                    !__instance.getCharacterFromName("Gus").getTileLocation().Equals(new Vector2(14f, 21f)))
                {
                    return true; // run original logic
                }

                var islandResortStock = new Dictionary<ISalable, int[]>();
                Utility.AddStock(islandResortStock, new Object(Vector2.Zero, 873, int.MaxValue), 300);
                Utility.AddStock(islandResortStock, new Object(Vector2.Zero, 346, int.MaxValue), 250);
                Utility.AddStock(islandResortStock, new Object(Vector2.Zero, 303, int.MaxValue), 500);
                Utility.AddStock(islandResortStock, new Object(Vector2.Zero, 459, int.MaxValue), 400);
                Utility.AddStock(islandResortStock, new Object(Vector2.Zero, 612, int.MaxValue), 200);
                var mangoWine = new Object(Vector2.Zero, 348, int.MaxValue);
                var mango = new Object(834, 1);
                mangoWine.Price = mango.Price * 3;
                mangoWine.Name = mango.Name + " Wine";
                mangoWine.preserve.Value = Object.PreserveType.Wine;
                mangoWine.preservedParentSheetIndex.Value = mango.ParentSheetIndex;
                mangoWine.Quality = 2;
                Utility.AddStock(islandResortStock, mangoWine, 2500);
                AddArchipelagoRecipeItem(islandResortStock, "Tropical Curry", 2000);

                Game1.activeClickableMenu = new ShopMenu(islandResortStock, who: "Gus", context: "ResortBar");
                
                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_ReplaceTropicalCurryWithChefsanityCheck_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public static Dictionary<ISalable, int[]> getIslandMerchantTradeStock(Farmer who)
        public static bool GetIslandMerchantTradeStock_ReplaceBananaPuddingWithChefsanityCheck_Prefix(Farmer who, ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                var islandMerchantStock = new Dictionary<ISalable, int[]>();
                var warpTotemFarm = new Object(688, 1);
                islandMerchantStock.Add(warpTotemFarm, new[] { 0, int.MaxValue, 830, 5, });
                var taroTuber = new Object(831, 1);
                islandMerchantStock.Add(taroTuber, new[] { 0, int.MaxValue, 881, 2, });
                var pineappleSeeds = new Object(833, 1);
                islandMerchantStock.Add(pineappleSeeds, new[] { 0, int.MaxValue, 851, 1, });
                if (Game1.netWorldState.Value.GoldenCoconutCracked.Value)
                {
                    var goldenCoconut = new Object(791, 1);
                    islandMerchantStock.Add(goldenCoconut, new[] { 0, int.MaxValue, 88, 10, });
                }

                var tropicalTV = new TV(2326, Vector2.Zero);
                islandMerchantStock.Add(tropicalTV, new[] { 0, int.MaxValue, 830, 30, });
                var jungleTorch = new Furniture(2331, Vector2.Zero);
                islandMerchantStock.Add(jungleTorch, new[] { 0, int.MaxValue, 848, 5, });
                if (Game1.dayOfMonth % 2 == 0)
                {
                    var tropicalChair = new Furniture(134, Vector2.Zero);
                    islandMerchantStock.Add(tropicalChair, new[] { 0, int.MaxValue, 837, 1, });
                }

                var bananaSapling = new Object(69, 1);
                islandMerchantStock.Add(bananaSapling, new[] { 0, int.MaxValue, 852, 5, });
                var mangoSapling = new Object(835, 1);
                islandMerchantStock.Add(mangoSapling, new[] { 0, int.MaxValue, 719, 75, });
                if (Game1.dayOfMonth % 7 == 1)
                {
                    var smallCap = new Hat(79);
                    islandMerchantStock.Add(smallCap, new[] { 0, int.MaxValue, 830, 30, });
                }

                if (Game1.dayOfMonth % 7 == 3)
                {
                    var bluebirdMask = new Hat(80);
                    islandMerchantStock.Add(bluebirdMask, new[] { 0, int.MaxValue, 830, 30, });
                }

                if (Game1.dayOfMonth % 7 == 5)
                {
                    var deluxeCowboyHat = new Hat(81);
                    islandMerchantStock.Add(deluxeCowboyHat, new[] { 0, int.MaxValue, 830, 30, });
                }

                var wildDoubleBed = new BedFurniture(2496, Vector2.Zero);
                islandMerchantStock.Add(wildDoubleBed, new[] { 0, int.MaxValue, 848, 100, });
                var tropicalBed = new BedFurniture(2176, Vector2.Zero);
                islandMerchantStock.Add(tropicalBed, new[] { 0, int.MaxValue, 829, 20, });
                if (Game1.dayOfMonth % 7 == 0)
                {
                    var tropicalDoubleBed = new BedFurniture(2180, Vector2.Zero);
                    islandMerchantStock.Add(tropicalDoubleBed, new[] { 0, int.MaxValue, 91, 5, });
                }

                if (Game1.dayOfMonth % 7 == 2)
                {
                    var palmWallOrnament = new Furniture(2393, Vector2.Zero);
                    islandMerchantStock.Add(palmWallOrnament, new[] { 0, int.MaxValue, 832, 1, });
                }

                if (Game1.dayOfMonth % 7 == 4)
                {
                    var volcanoPhoto = new Furniture(2329, Vector2.Zero);
                    islandMerchantStock.Add(volcanoPhoto, new[] { 0, int.MaxValue, 834, 5, });
                }

                if (Game1.dayOfMonth % 7 == 6)
                {
                    var oceanicRug = new Furniture(1228, Vector2.Zero);
                    islandMerchantStock.Add(oceanicRug, new[] { 0, int.MaxValue, 838, 3, });
                }

                var mahoganySeed = new Object(292, 1);
                islandMerchantStock.Add(mahoganySeed, new[] { 0, int.MaxValue, 836, 1, });
                var luauSkirt = new Clothing(7);
                islandMerchantStock.Add(luauSkirt, new[] { 0, int.MaxValue, 830, 50, });



                AddBananaPuddingToStock(islandMerchantStock);

                if (!Game1.player.cookingRecipes.ContainsKey("Deluxe Retaining Soil"))
                {
                    var deluxeRetainingSoilRecipe = new Object(920, 1, true);
                    islandMerchantStock.Add(deluxeRetainingSoilRecipe, new[] { 0, 1, 848, 50, });
                }

                if (Game1.dayOfMonth == 28 && Game1.stats.getStat("hardModeMonstersKilled") > 50U)
                {
                    var galaxySoul = new Object(896, 1);
                    islandMerchantStock.Add(galaxySoul, new[] { 0, int.MaxValue, 910, 10, });
                }

                __result = islandMerchantStock;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log(
                    $"Failed in {nameof(GetIslandMerchantTradeStock_ReplaceBananaPuddingWithChefsanityCheck_Prefix)}:\n{ex}",
                    LogLevel.Error);
                return true; // run original logic
            }
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_ReplaceGingerAleWithChefsanityCheck_Prefix(VolcanoDungeon __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tileIndexAtLocation = __instance.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
                if (tileIndexAtLocation == 367 || tileIndexAtLocation != 77 || !Game1.player.canUnderstandDwarves)
                {
                    return true; // run original logic
                }

                var random = new Random((int)(Game1.stats.DaysPlayed + 898U + (long)Game1.uniqueIDForThisGame));
                var dwarfStock = new Dictionary<ISalable, int[]>();
                dwarfStock.Add(new Boots(853), new[] { 0, int.MaxValue, 848, 100 });
                Utility.AddStock(dwarfStock, new Object(Vector2.Zero, 286, int.MaxValue), 150);
                Utility.AddStock(dwarfStock, new Object(Vector2.Zero, 287, int.MaxValue), 300);
                Utility.AddStock(dwarfStock, new Object(Vector2.Zero, 288, int.MaxValue), 500);
                Utility.AddStock(dwarfStock, new Object(Vector2.Zero, random.NextDouble() < 0.5 ? 244 : 237, int.MaxValue), 600);

                if (random.NextDouble() < 0.25)
                {
                    Utility.AddStock(dwarfStock, new Hat(77), 5000);
                }

                if (!Game1.player.craftingRecipes.ContainsKey("Warp Totem: Island"))
                {
                    Utility.AddStock(dwarfStock, new Object(886, 1, true), 5000);
                }

                AddGingerAleToStock(dwarfStock);

                Game1.activeClickableMenu = new ShopMenu(dwarfStock, who: "VolcanoShop", context: "VolcanoShop");

                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_ReplaceGingerAleWithChefsanityCheck_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void AddBananaPuddingToStock(Dictionary<ISalable, int[]> stock)
        {
            const string bananaPudding = "Banana Pudding";
            if (_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
            {
                AddArchipelagoRecipeItem(stock, bananaPudding, 0, 881, 30);
                return;
            }

            if (!Game1.player.cookingRecipes.ContainsKey(bananaPudding))
            {
                var bananaPuddingRecipe = new Object(904, 1, true);
                stock.Add(bananaPuddingRecipe, new[] { 0, 1, 881, 30, });
            }
        }

        private static void AddGingerAleToStock(Dictionary<ISalable, int[]> stock)
        {
            const string gingerAle = "Ginger Ale";
            if (_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
            {
                AddArchipelagoRecipeItem(stock, gingerAle, 1000);
                return;
            }

            if (!Game1.player.cookingRecipes.ContainsKey(gingerAle))
            {
                Utility.AddStock(stock, new Object(903, 1, true), 500);
            }
        }

        private static void AddArchipelagoRecipesToSaloonStock(Dictionary<ISalable, int[]> saloonStock)
        {
            AddArchipelagoRecipeItem(saloonStock, "Hashbrowns", 50);
            AddArchipelagoRecipeItem(saloonStock, "Omelet", 100);
            AddArchipelagoRecipeItem(saloonStock, "Pancakes", 100);
            AddArchipelagoRecipeItem(saloonStock, "Bread", 100);
            AddArchipelagoRecipeItem(saloonStock, "Tortilla", 100);
            AddArchipelagoRecipeItem(saloonStock, "Pizza", 150);
            AddArchipelagoRecipeItem(saloonStock, "Maki Roll", 300);

            if (_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Friendship))
            {
                AddArchipelagoRecipeItem(saloonStock, "Cookies", 300);
            }

            AddArchipelagoRecipeItem(saloonStock, "Triple Shot Espresso", 5000);
        }

        private static void AddArchipelagoRecipeItem(Dictionary<ISalable, int[]> stock, string name, int moneyPrice, int itemPriceId = -1, int itemPriceAmount = 0)
        {
            var location = $"{CHEFSANITY_LOCATION_PREFIX}{name}";
            if (!_locationChecker.IsLocationMissingAndExists(location))
            {
                return;
            }

            var recipeName = $"{name} Recipe";
            var recipeApItem = new PurchaseableArchipelagoLocation(recipeName, location, _helper, _locationChecker, _archipelago);
            var prices = (itemPriceId > -1 && itemPriceAmount > 0) ? new[] { moneyPrice, 1, itemPriceId, itemPriceAmount, } : new[] { moneyPrice, 1 };
            stock.Add(recipeApItem, prices);
        }
    }
}