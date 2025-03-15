using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class BlockedIPAddress
    {
        [JsonProperty]
        public string IPAddress { get; set; }
        [JsonProperty]
        public DateTime BlockedDateTime { get; set; }
        [JsonProperty]
        public string BlockedBy { get; set; }
    }
}
