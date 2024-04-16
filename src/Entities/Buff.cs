using Newtonsoft.Json;
using System;

namespace Etrea2.Entities
{
    [Serializable]
    internal class Buff
    {
        [JsonProperty]
        internal string BuffName { get; set; }
        [JsonProperty]
        internal string Description { get; set; }
        [JsonProperty]
        internal int BuffDuration { get; set; }
    }
}
