using System.Linq;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class CompoundNameMapper : INameMapper
    {
        private List<INameMapper> _mappers;

        public CompoundNameMapper(SlotData slotData)
        {
            _mappers = new List<INameMapper>();
            _mappers.Add(new CraftingRecipeNameMapper());

            if (slotData.Mods.HasMod(ModNames.ARCHAEOLOGY))
            {
                _mappers.Add(new ArchaeologyNameMapper());
            }
        }

        public string GetEnglishName(string internalName)
        {
            return _mappers.Aggregate(internalName, (current, nameMapper) => nameMapper.GetEnglishName(current));
        }

        public string GetInternalName(string englishName)
        {
            return _mappers.Aggregate(englishName, (current, nameMapper) => nameMapper.GetInternalName(current));
        }

        public bool RecipeNeedsMapping(string itemOfRecipe)
        {
            return _mappers.Any(x => x.RecipeNeedsMapping(itemOfRecipe));
        }
    }
}