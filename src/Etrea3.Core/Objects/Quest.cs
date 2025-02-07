using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Etrea3.Objects
{
    [Serializable]
    public class Quest
    {
        [JsonProperty]
        public Guid QuestGUID { get; set; } = Guid.NewGuid();
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string FlavourText { get; set; }
        [JsonProperty]
        public int Zone { get; set; }
        [JsonProperty]
        public QuestType QuestType { get; set; } = QuestType.Undefined;
        [JsonProperty]
        public ulong RewardGold { get; set; }
        [JsonProperty]
        public uint RewardExp { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int,int> RequiredItems { get; set; } = new ConcurrentDictionary<int,int>();
        [JsonProperty]
        public ConcurrentDictionary<int, int> RequiredMonsters { get; set; } = new ConcurrentDictionary<int, int>();
        [JsonProperty]
        public ConcurrentDictionary<int, int> RewardItems { get; set; } = new ConcurrentDictionary<int, int>();
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }

        public bool IsComplete(Session session)
        {
            if (RequiredItems.Count > 0)
            {
                foreach(var kvp in RequiredItems)
                {
                    if (session.Player.Inventory.Values.Where(x => x.ID == kvp.Key).Count() != kvp.Value)
                    {
                        return false;
                    }
                }
            }
            if (RequiredMonsters.Count > 0)
            {
                foreach(var kvp in RequiredMonsters)
                {
                    if (!session.Player.SlainMonsters.ContainsKey(kvp.Key))
                    {
                        return false;
                    }
                    if (session.Player.SlainMonsters.TryGetValue(kvp.Key, out int killCount))
                    {
                        if (killCount < kvp.Value)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
