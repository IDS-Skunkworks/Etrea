using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class Skill
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public int LearnCost { get; set; }
        [JsonProperty]
        public ActorClass AvailableToClass { get; set; }
    }
}
