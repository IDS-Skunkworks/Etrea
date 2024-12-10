using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class Zone
    {
        [JsonProperty]
        public int ZoneID { get; set; }
        [JsonProperty]
        public string ZoneName { get; set; }
        [JsonProperty]
        public int MinRoom { get; set; }
        [JsonProperty]
        public int MaxRoom { get; set; }
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;
    }
}
