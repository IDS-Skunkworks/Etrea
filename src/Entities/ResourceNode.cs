using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdoms_of_Etrea.Entities
{
    [Serializable]
    internal class ResourceNode
    {
        [JsonProperty]
        internal uint Id {  get; set; }
        [JsonProperty]
        internal string NodeName { get; set; }
        [JsonProperty]
        internal uint NodeDepth { get; set; }
        [JsonProperty]
        internal uint RID { get; set; }
        [JsonProperty]
        internal uint AppearanceChance { get; set; }
        [JsonProperty]
        internal List<InventoryItem> CanFind { get; set; }

        public override string ToString()
        {
            return NodeName.ToLower();
        }

        internal ResourceNode ShallowCopy()
        {
            var node = (ResourceNode)MemberwiseClone();
            return node;
        }

        internal InventoryItem Mine()
        {
            if(CanFind != null && CanFind.Count > 0)
            {
                var rnd = new Random(DateTime.Now.GetHashCode());
                return CanFind[rnd.Next(CanFind.Count)];
            }
            return null;
        }
    }
}
