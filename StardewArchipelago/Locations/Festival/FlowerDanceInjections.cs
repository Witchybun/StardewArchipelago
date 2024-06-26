﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.Festival
{
    public static class FlowerDanceInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _shopReplacer = shopReplacer;
        }

        // public void setUpFestivalMainEvent()
        public static void SetUpFestivalMainEvent_FlowerDance_Postfix(Event __instance)
        {
            try
            {
                if (!__instance.isSpecificFestival("spring24"))
                {
                    return;
                }

                if (Game1.player.dancePartner.Value == null)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.FLOWER_DANCE);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetUpFestivalMainEvent_FlowerDance_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static ShopMenu _lastShopMenuUpdated = null;
        // public override void update(GameTime time)
        public static void Update_HandleFlowerDanceShopFirstTimeOnly_Postfix(ShopMenu __instance, GameTime time)
        {
            try
            {
                // We only run this once for each menu
                if (_lastShopMenuUpdated == __instance || __instance.storeContext != "Temp" || !Game1.CurrentEvent.isSpecificFestival("spring24"))
                {
                    return;
                }

                _lastShopMenuUpdated = __instance;
                var myActiveHints = _archipelago.GetMyActiveHints();
                _shopReplacer.ReplaceShopItem(__instance.itemPriceAndStock, FestivalLocationNames.RARECROW_5, item => _shopReplacer.IsRarecrow(item, 5), myActiveHints);
                _shopReplacer.PlaceShopRecipeCheck(__instance.itemPriceAndStock, FestivalLocationNames.TUB_O_FLOWERS_RECIPE, "Tub o' Flowers", myActiveHints, new []{1000, 1});

                    __instance.forSale = __instance.itemPriceAndStock.Keys.ToList();
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Update_HandleFlowerDanceShopFirstTimeOnly_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
