using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class ServerIPRecord
    {
        [JsonProperty]
        public string ip { get; set; }
        [JsonProperty]
        public string hostname { get; set; }
        [JsonProperty]
        public string city { get; set; }
        [JsonProperty]
        public string region { get; set; }
        [JsonProperty]
        public string country { get; set; }
        [JsonProperty]
        public string loc { get; set; }
        [JsonProperty]
        public string org { get; set; }
        [JsonProperty]
        public string postal { get; set; }
        [JsonProperty]
        public string timezone { get; set; }
        [JsonProperty]
        public string readme { get; set; }
    }
}
