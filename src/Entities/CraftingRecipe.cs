using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Kingdoms_of_Etrea.Entities
{
    internal class Crafting
    {
        [Serializable]
        internal class Recipe
        {
            [JsonProperty]
            internal uint RecipieID { get; set; }
            [JsonProperty]
            internal string RecipieName { get; set; }
            [JsonProperty]
            internal RecipeType RecipeType { get; set; }
            [JsonProperty]
            internal string RecipieDescription { get; set; }
            [JsonProperty]
            internal uint RecipeResult { get; set; }
            [JsonProperty]
            //internal Dictionary<InventoryItem, uint> RequiredMaterials { get; set; }
            internal Dictionary<uint, uint> RequiredMaterials { get; set; }
        }
    }
}
