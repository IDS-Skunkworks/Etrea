using Newtonsoft.Json;
using System;

namespace Etrea2.Entities
{
    [Serializable]
    internal class Skill
    {
        [JsonProperty]
        internal string Name { get; set; }
        [JsonProperty]
        internal string Description { get; set; }
        [JsonProperty]
        internal uint MPCost { get; set; }
        [JsonProperty]
        internal uint GoldToLearn { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
