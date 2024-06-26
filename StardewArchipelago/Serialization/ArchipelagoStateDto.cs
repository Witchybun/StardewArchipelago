﻿using StardewArchipelago.Archipelago;
using System.Collections.Generic;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoStateDto
    {
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        public List<ReceivedItem> ItemsReceived { get; set; }
        public List<string> LocationsChecked { get; set; }
        public Dictionary<string, ScoutedLocation> LocationsScouted { get; set; }
        public Dictionary<string, string> LettersGenerated { get; set; }
        public List<string> SeasonsOrder { get; set; }
        public AppearanceRandomization? AppearanceRandomizerOverride { get; set; }
        public bool HideEmptyArchipelagoLetters { get; set; }
        public TrapItemsDifficulty? TrapDifficultyOverride { get; set; }
        public int TravelingMerchantPurchases { get; set; }
        public int StoredStarTokens { get; set; }

        public ArchipelagoStateDto()
        {
            ItemsReceived = new List<ReceivedItem>();
            LocationsChecked = new List<string>();
            LocationsScouted = new Dictionary<string, ScoutedLocation>();
            LettersGenerated = new Dictionary<string, string>();
            SeasonsOrder = new List<string>();
            AppearanceRandomizerOverride = null;
            HideEmptyArchipelagoLetters = false;
            TrapDifficultyOverride = null;
            TravelingMerchantPurchases = 0;
            StoredStarTokens = 0;
        }
    }
}
