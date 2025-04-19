using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace Etrea3.Objects
{
    [Serializable]
    public class ResourceNode
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public int Depth { get; set; }
        [JsonProperty]
        public int ApperanceChance { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int, bool> CanFind { get; set; }
        [JsonIgnore]
        public int RoomID { get; set; }
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
        [JsonProperty]
        public ResourceVeinType VeinType { get; set; } = ResourceVeinType.Common;
        [JsonIgnore]
        private static readonly object lockObject = new object();

        public ResourceNode()
        {
            Name = "New Resource Vein";
            CanFind = new ConcurrentDictionary<int, bool>();
            LockHolder = Guid.Empty;
        }

        public int Mine(out bool depleted)
        {
            if (CanFind != null && CanFind.Count > 0 && Depth >= 1)
            {
                var itemId = CanFind.GetRandomElement();
                if (ItemManager.Instance.ItemExists(itemId))
                {
                    ReduceDepth(out depleted);
                    return itemId;
                }
                Game.LogMessage($"ERROR: Node ID {ID} returned Item {itemId} from a call to Mine() but no such Item was found in Item Manager", LogLevel.Error);
            }
            depleted = Depth <= 0;
            return -1;
        }

        private void ReduceDepth(out bool depleted)
        {
            lock (lockObject)
            {
                Depth--;
                depleted = Depth <= 0;
            }
        }
    }
}
