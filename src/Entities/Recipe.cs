using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Etrea2.Entities
{
    [Serializable]
    internal class Recipe
    {
        [JsonProperty]
        internal uint RecipeID { get; set; }
        [JsonProperty]
        internal string RecipeName { get; set; }
        [JsonProperty]
        internal RecipeType RecipeType { get; set; }
        [JsonProperty]
        internal string RecipeDescription { get; set; }
        [JsonProperty]
        internal uint RecipeResult { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> RequiredMaterials { get; set; }

        internal Recipe()
        {
            RequiredMaterials = new Dictionary<uint, uint>();
        }

        internal Recipe ShallowCopy()
        {
            var r = (Recipe)this.MemberwiseClone();
            return r;
        }
    }
}
