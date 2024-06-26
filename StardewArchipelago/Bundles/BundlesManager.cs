﻿using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using Newtonsoft.Json;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace StardewArchipelago.Bundles
{
    public class BundlesManager
    {
        private IModHelper _modHelper;
        private static Dictionary<string, string> _vanillaBundleData;
        private Dictionary<string, string> _currentBundlesData;
        private BundleRooms BundleRooms { get; }

        public BundlesManager(IModHelper modHelper, StardewItemManager itemManager, string bundlesJson)
        {
            _modHelper = modHelper;
            var bundlesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(bundlesJson);
            BundleRooms = new BundleRooms(itemManager, bundlesDictionary);
            _vanillaBundleData = Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles");
            _currentBundlesData = BundleRooms.ToStardewStrings();
        }

        public void ReplaceAllBundles()
        {
            if (Game1.netWorldState.Value is not NetWorldState worldState)
            {
                throw new InvalidCastException($"World State was unexpected type: {Game1.netWorldState.GetType()}");
            }

            // private readonly NetStringDictionary<string, NetString> netBundleData = new NetStringDictionary<string, NetString>();
            var netBundleDataField = _modHelper.Reflection.GetField<NetStringDictionary<string, NetString>>(worldState, "netBundleData");
            var netBundleData = netBundleDataField.GetValue();
            netBundleData.Clear();
            var bundlesState = BackupBundleState(worldState);
            worldState.Bundles.Clear();
            worldState.BundleRewards.Clear();
            worldState.BundleData.Clear();
            worldState.SetBundleData(_currentBundlesData);
            RestoreBundleState(worldState, bundlesState);
        }

        private static Dictionary<int, bool[]> BackupBundleState(NetWorldState worldState)
        {
            var bundlesState = new Dictionary<int, bool[]>();
            foreach (var (key, values) in worldState.Bundles.Pairs)
            {
                bundlesState.Add(key, values);
            }

            return bundlesState;
        }

        private static void RestoreBundleState(NetWorldState worldState, Dictionary<int, bool[]> bundlesState)
        {
            foreach (var (key, values) in bundlesState)
            {
                if (worldState.Bundles.ContainsKey(key))
                {
                    worldState.Bundles.Remove(key);
                    worldState.Bundles.Add(key, values);
                }
            }
        }
    }
}
