﻿using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship
{
    public class Friends
    {
        public const string PET_NAME = "Pet";

        private static Dictionary<string, string> _stardewNameToArchipelagoName = new Dictionary<string, string>{
            {"MisterGinger", "Mr. Ginger"}, {"MarlonFay", "Marlon"}, {"GuntherSilvian", "Gunther"}, {"MorrisTod", "Morris"},
        };

        private List<ArchipelagoFriend> _friends;
        private Dictionary<string, ArchipelagoFriend> _friendsByName;

        public Friends()
        {
            InitializeFriends();
            _friendsByName = new Dictionary<string, ArchipelagoFriend>();
        }

        private void InitializeFriends()
        {
            _friends = new List<ArchipelagoFriend>();
            var npcs = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
            foreach (var (name, npcInfo) in npcs)
            {
                var villagerInfoParts = npcInfo.Split('/');
                var gender = villagerInfoParts[4];
                var datable = villagerInfoParts[5] == "datable";
                var spawnLocation = villagerInfoParts[10].Split(" ")[0];
                var spawnsOnIsland = IsIslandLocation(spawnLocation);
                var apName = _stardewNameToArchipelagoName.ContainsKey(name) ? _stardewNameToArchipelagoName[name] : name;

                var friend = new ArchipelagoFriend(name, apName, datable, false, spawnsOnIsland, name.Contains("Dwarf"), false);
                _friends.Add(friend);
            }
        }

        private bool IsIslandLocation(string spawnLocation)
        {
            var location = Game1.getLocationFromName(spawnLocation);
            if (location == null)
            {
                return false;
            }

            return location is IslandLocation;
        }

        public ArchipelagoFriend GetFriend(string name)
        {
            if (name == null)
            {
                return null;
            }

            if (!_friendsByName.ContainsKey(name))
            {
                var friend = _friends.FirstOrDefault(x => x.StardewName == name || x.ArchipelagoName == name);
                if (friend == null)
                {
                    friend = TryFindKidWithThatName(name);
                    if (friend == null)
                    {
                        return null;
                    }
                }

                _friendsByName.Add(name, friend);
            }

            return _friendsByName[name];
        }

        private static ArchipelagoFriend TryFindKidWithThatName(string name)
        {
            var kids = Game1.player.getChildren();
            for (var kidIndex = 0; kidIndex < kids.Count; kidIndex++)
            {
                var kid = kids[kidIndex];
                if (kid.Name != name)
                {
                    continue;
                }

                var firstOrSecond = kidIndex % 2 == 0 ? "First" : "Second";
                return new ArchipelagoFriend(kid.Name, $"{firstOrSecond} Child", false, false, false,
                    false, true);
            }

            return null;
        }

        public ArchipelagoFriend GetFriend(Friendship friendship)
        {
            return GetFriend(GetNpcName(friendship));
        }

        private string GetNpcName(Friendship friendship)
        {
            var farmer = Game1.player;
            foreach (var name in farmer.friendshipData.Keys)
            {
                if (ReferenceEquals(farmer.friendshipData[name], friendship))
                {
                    return name;
                }
            }

            return null;
        }

        public void AddPet(string petName)
        {
            if (_friendsByName.ContainsKey(petName))
            {
                return;
            }

            var pet = new ArchipelagoFriend(petName, PET_NAME, false, true, false, false, false);
            _friends.Add(pet);
            _friendsByName.Add(petName, pet);
        }
    }
}
