using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Etrea2.Entities
{
    [Serializable]
    internal class Quest
    {
        [JsonProperty]
        internal Guid QuestGUID { get; set; }
        [JsonProperty]
        internal uint QuestID { get; set; }
        [JsonProperty]
        internal string QuestName { get; set; }
        [JsonProperty]
        internal string QuestText { get; set; }
        [JsonProperty]
        internal uint QuestZone { get; set; }
        [JsonProperty]
        internal QuestType QuestType { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> FetchItems { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> Monsters { get; set; }
        [JsonProperty]
        internal bool IsCompleted { get; set; }
        [JsonProperty]
        internal uint RewardGold { get; set; }
        [JsonProperty]
        internal uint RewardExp { get; set; }
        [JsonProperty]
        internal List<InventoryItem> RewardItems { get; set; }

        internal Quest()
        {
            QuestGUID = Guid.NewGuid();
            FetchItems = new Dictionary<uint, uint>();
            Monsters = new Dictionary<uint, uint>();
            RewardItems = new List<InventoryItem>();
            IsCompleted = false;
        }

        internal Quest ShallowCopy()
        {
            var q = (Quest)this.MemberwiseClone();
            return q;
        }
    }
}
