﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Goals;
using StardewArchipelago.Locations;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Items.Mail
{
    public class MailPatcher
    {
        private static IMonitor _monitor;
        private readonly Harmony _harmony;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ArchipelagoStateDto _state;
        private static LetterActions _letterActions;

        public MailPatcher(IMonitor monitor, Harmony harmony, ArchipelagoClient archipelago, LocationChecker locationChecker, ArchipelagoStateDto state, LetterActions letterActions)
        {
            _monitor = monitor;
            _harmony = harmony;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _state = state;
            _letterActions = letterActions;
        }

        public void PatchMailBoxForApItems()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.exitThisMenu)),
                postfix: new HarmonyMethod(typeof(MailPatcher), nameof(MailPatcher.ExitThisMenu_ApplyLetterAction_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
                prefix: new HarmonyMethod(typeof(MailPatcher), nameof(MailPatcher.Mailbox_HideEmptyApLetters_Prefix))
            );

            if (_archipelago.SlotData.Fishsanity != Fishsanity.None)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
                    prefix: new HarmonyMethod(typeof(MailPatcher), nameof(MailPatcher.Mailbox_RemoveMasterAnglerStardropOnFishsanity_Prefix))
                );
            }

            if (_archipelago.SlotData.FestivalLocations != FestivalLocations.Vanilla)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
                    prefix: new HarmonyMethod(typeof(MailPatcher), nameof(MailPatcher.Mailbox_RemoveRarecrowSocietyRecipeOnFestivals_Prefix))
                );
            }
        }

        public static void ExitThisMenu_ApplyLetterAction_Postfix(IClickableMenu __instance, bool playSound)
        {
            try
            {
                if (__instance is not LetterViewerMenu letterMenuInstance || letterMenuInstance.mailTitle == null || letterMenuInstance.isFromCollection)
                {
                    return;
                }

                var title = letterMenuInstance.mailTitle;
                if (!MailKey.TryParse(title, out var apMailKey))
                {
                    return;
                }

                var apActionName = apMailKey.LetterOpenedAction;
                var apActionParameter = apMailKey.ActionParameter;

                if (string.IsNullOrWhiteSpace(apActionName))
                {
                    return;
                }

                _letterActions.ExecuteLetterAction(apActionName, apActionParameter);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ExitThisMenu_ApplyLetterAction_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public void mailbox()
        public static bool Mailbox_HideEmptyApLetters_Prefix(GameLocation __instance)
        {
            try
            {
                CleanMailboxUntilNonEmptyLetter();
                var mailbox = Game1.mailbox;
                if (mailbox == null || !mailbox.Any())
                {
                    return true; // run original logic
                }

                var nextLetter = mailbox.First();

                if (!MailKey.TryParse(nextLetter, out _))
                {
                    return true; // run original logic
                }

                var mailData = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
                if (!mailData.ContainsKey(nextLetter))
                {
                    mailData.Add(nextLetter, _state.LettersGenerated[nextLetter]);
                }

                // We force add the letter because it can contain custom content that can be then considered "to not be remembered" by the base game.
                // So if it's an ap letter, always remember it
                Game1.player.mailReceived.Add(nextLetter);

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Mailbox_HideEmptyApLetters_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void CleanMailboxUntilNonEmptyLetter()
        {
            if (!ModEntry.Instance.State.HideEmptyArchipelagoLetters)
            {
                return;
            }

            var mailbox = Game1.mailbox;
            while (mailbox.Count > 1)
            {
                var nextLetterInMailbox = Game1.mailbox[1];

                if (!MailKey.TryParse(nextLetterInMailbox, out var apMailKey))
                {
                    return;
                }

                if (!apMailKey.IsEmpty)
                {
                    return;
                }

                Game1.player.mailReceived.Add(nextLetterInMailbox);
                mailbox.RemoveAt(1);
            }
        }

        // public void mailbox()
        public static bool Mailbox_RemoveMasterAnglerStardropOnFishsanity_Prefix(GameLocation __instance)
        {
            try
            {
                var mailbox = Game1.mailbox;
                if (mailbox == null || !mailbox.Any())
                {
                    return true; // run original logic
                }
                
                var nextLetter = mailbox.First();
                if (!nextLetter.Equals(GoalCodeInjection.MASTER_ANGLER_LETTER))
                {
                    return true; // run original logic
                }

                ReplaceStardropWithSeafoamPudding();
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Mailbox_RemoveMasterAnglerStardropOnFishsanity_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void ReplaceStardropWithSeafoamPudding()
        {
            const string stardropText = "stardrop";
            const string stardropItemText = "%item object 434 1 %%";
            const string puddingText = "pudding";
            const string seafoamPuddingItemText = "%item object 265 10 %%";

            var mailContent = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            var masterAnglerLetterContent = mailContent[GoalCodeInjection.MASTER_ANGLER_LETTER];
            mailContent[GoalCodeInjection.MASTER_ANGLER_LETTER] = masterAnglerLetterContent
                .Replace(stardropItemText, seafoamPuddingItemText)
                .Replace(stardropText, puddingText);
        }

        private const string RARECROW_SOCIETY_LETTER = "RarecrowSociety";
        private const string RARECROW_SOCIETY_AP_LOCATION = "Collect All Rarecrows";

        // public void mailbox()
        public static bool Mailbox_RemoveRarecrowSocietyRecipeOnFestivals_Prefix(GameLocation __instance)
        {
            try
            {
                var mailbox = Game1.mailbox;
                if (mailbox == null || !mailbox.Any())
                {
                    return true; // run original logic
                }

                var nextLetter = mailbox.First();
                if (!nextLetter.Equals(RARECROW_SOCIETY_LETTER))
                {
                    return true; // run original logic
                }

                RemoveDeluxeScarecrowRecipe();
                _locationChecker.AddCheckedLocation(RARECROW_SOCIETY_AP_LOCATION);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Mailbox_RemoveRarecrowSocietyRecipeOnFestivals_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void RemoveDeluxeScarecrowRecipe()
        {
            const string deluxeScarecrowRecipeText = "Please accept this blueprint to commemorate your achievement.";
            const string deluxeScarecrowRecipeItemText = "%item craftingRecipe Deluxe_Scarecrow %%";

            var scoutedLocation = _archipelago.ScoutSingleLocation(RARECROW_SOCIETY_AP_LOCATION);
            var scoutedItemName = scoutedLocation.ItemName;
            var scoutedPlayer = scoutedLocation.PlayerName;
            var replacementText = $"We will send {scoutedItemName} to {scoutedPlayer} to commemorate your achievement.";
            const string recipeReplacementText = "";

            var mailContent = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            var masterAnglerLetterContent = mailContent[RARECROW_SOCIETY_LETTER];
            mailContent[RARECROW_SOCIETY_LETTER] = masterAnglerLetterContent
                .Replace(deluxeScarecrowRecipeItemText, recipeReplacementText)
                .Replace(deluxeScarecrowRecipeText, replacementText);
        }
    }
}
