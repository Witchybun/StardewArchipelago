﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftSender
    {
        private readonly IMonitor _monitor;
        private readonly ArchipelagoClient _archipelago;
        private readonly IGiftingService _giftService;
        internal GiftGenerator GiftGenerator { get; }

        public GiftSender(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager, IGiftingService giftService)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _giftService = giftService;
            GiftGenerator = new GiftGenerator(itemManager);
        }

        public void SendGift(string slotName, bool isTrap)
        {
            try
            {
                if (!_archipelago.PlayerExists(slotName))
                {
                    Game1.chatBox?.addMessage($"Could not find player named {slotName}", Color.Gold);
                    return;
                }

                var giftObject = Game1.player.ActiveObject;
                if (!GiftGenerator.TryCreateGiftItem(Game1.player.ActiveObject, isTrap, out var giftItem,
                        out var giftTraits))
                {
                    // TryCreateGiftItem will log the reason if it fails
                    return;
                }

                var isValidRecipient = _giftService.CanGiftToPlayer(slotName, giftTraits.Select(x => x.Trait));
                var giftOrTrap = isTrap ? "trap" : "gift";
                if (!isValidRecipient)
                {
                    Game1.chatBox?.addMessage($"{slotName} cannot receive this {giftOrTrap}", Color.Gold);
                    return;
                }

                var tax = GetTaxForItem(giftObject, out var itemValue, out var taxRate);
                if (Game1.player.Money < tax)
                {
                    GiveCantAffordTaxFeedbackToPlayer(itemValue, tax, taxRate);
                    return;
                }


                var success = _giftService.SendGift(giftItem, giftTraits, slotName, out var giftId);
                _monitor.Log($"Sending {giftOrTrap} of {giftItem.Amount} {giftItem.Name} to {slotName} with {giftTraits.Length} traits. [ID: {giftId}]",
                    LogLevel.Info);
                if (!success)
                {
                    _monitor.Log($"Gift Failed to send properly", LogLevel.Error);
                    Game1.chatBox?.addMessage($"Unknown Error occurred while sending {giftOrTrap}.", Color.Red);
                    return;
                }

                Game1.player.ActiveObject = null;
                Game1.player.Money -= tax;
                GiveJojaPrimeInformationToPlayer(slotName, giftOrTrap, giftItem);
                GiveTaxFeedbackToPlayer(slotName, tax);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Unknown error occurred while attempting to process gift command.{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", LogLevel.Error);
                Game1.chatBox?.addMessage($"Could not complete gifting operation. Check SMAPI for error details.", Color.Red);
                return;
            }
        }

        private int GetTaxForItem(Object giftObject)
        {
            return GetTaxForItem(giftObject, out _, out _);
        }

        private int GetTaxForItem(Object giftObject, out int itemValue, out double taxRate)
        {
            itemValue = giftObject.Price * giftObject.Stack;
            taxRate = GetTaxRate();
            var tax = (int)Math.Round(taxRate * itemValue);
            return tax;
        }

        public List<Object> SendShuffleGifts(Dictionary<string, List<Object>> targets)
        {
            var objectsFailedToSend = new List<Object>();
            var totalTax = 0;
            foreach (var (player, gifts) in targets)
            {
                foreach (var giftObject in gifts)
                {
                    TrySendShuffleGift(giftObject, player, objectsFailedToSend, out var tax);
                    totalTax += tax;
                }
            }

            Game1.player.Money = Math.Max(0, Game1.player.Money - totalTax);
            GiveTaxFeedbackToPlayer(null, totalTax);

            return objectsFailedToSend;
        }

        private void TrySendShuffleGift(Object giftObject, string player, List<Object> objectsFailedToSend, out int tax)
        {
            tax = 0;
            try
            {
                if (!GiftGenerator.TryCreateGiftItem(giftObject, false, out var giftItem, out var giftTraits))
                {
                    objectsFailedToSend.Add(giftObject);
                    return;
                }

                giftTraits = giftTraits.Append(new GiftTrait("Shuffle", 1, 1)).ToArray();
                var success = _giftService.SendGift(giftItem, giftTraits, player, out var giftId);
                if (!success)
                {
                    objectsFailedToSend.Add(giftObject);
                    return;
                }

                tax = GetTaxForItem(giftObject);
            }
            catch (Exception ex)
            {
                objectsFailedToSend.Add(giftObject);
                _monitor.Log($"Unknown error occurred while attempting to gift.{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", LogLevel.Error);
                return;
            }
        }

        public List<string> GetAllPlayersThatCanReceiveGift(Object giftObject)
        {
            var validTargets = new List<string>();
            if (!GiftGenerator.TryCreateGiftItem(giftObject, false, out var giftItem, out var giftTraits))
            {
                return validTargets;
            }

            foreach (var player in _archipelago.Session.Players.AllPlayers)
            {
                if (player.Slot == _archipelago.CurrentPlayer.Slot)
                {
                    continue;
                }

                var isValidRecipient = _giftService.CanGiftToPlayer(player.Name, giftTraits.Select(x => x.Trait));
                if (isValidRecipient)
                {
                    validTargets.Add(player.Name);
                }
            }

            return validTargets;
        }

        private double GetTaxRate()
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.AYEISHA) && IsAyeishaHere(out var ayeisha))
            {
                return 0.25 - (Game1.player.getFriendshipHeartLevelForNPC(ayeisha.Name) * 0.025);
            }

            return 0.25;
        }

        private bool IsAyeishaHere(out NPC ayeisha)
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.AYEISHA))
            {
                ayeisha = null;
                return false;
            }

            foreach (var character in Game1.currentLocation.characters)
            {
                if (character.Name.Contains("Ayeisha", StringComparison.InvariantCultureIgnoreCase))
                {
                    ayeisha = character;
                    return true;
                }
            }

            ayeisha = null;
            return false;
        }

        private void GiveJojaPrimeInformationToPlayer(string recipient, string giftOrTrap, GiftItem giftItem)
        {
            if (IsAyeishaHere(out _))
            {
                return;
            }

            Game1.chatBox?.addMessage($"{recipient} will receive your {giftOrTrap} of {giftItem.Amount} {giftItem.Name} within 1 business day", Color.Gold);
        }

        private void GiveTaxFeedbackToPlayer(string recipient, int tax)
        {
            if (IsAyeishaHere(out var ayeisha))
            {
                var tomorrowSentence = recipient == null ? $"It'll get there tomorrow!" : $"It'll reach {recipient} tomorrow!";
                ayeisha.setNewDialogue($"Sure, I'll deliver that package! {tomorrowSentence} That'll cost you {tax}g.");
                Game1.drawDialogue(ayeisha);
                return;
            }

            Game1.chatBox?.addMessage($"You have been charged a tax of {tax}g", Color.Gold);
            Game1.chatBox?.addMessage($"Thank you for using Joja Prime", Color.Gold);
        }

        private void GiveCantAffordTaxFeedbackToPlayer(int itemValue, int tax, double taxRate)
        {
            if (IsAyeishaHere(out var ayeisha))
            {
                ayeisha.setNewDialogue($"Sorry {Game1.player.Name}, but it costs {tax}g to deliver this...");
                Game1.drawDialogue(ayeisha);
                return;
            }

            Game1.chatBox?.addMessage($"You cannot afford tax for this item", Color.Gold);
            Game1.chatBox?.addMessage($"The tax is {taxRate * 100}% of the item's value of {itemValue}g, so you must pay {tax}g to send it",
                Color.Gold);
        }
    }
}
