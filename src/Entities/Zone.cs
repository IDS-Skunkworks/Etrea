using Newtonsoft.Json;

namespace Etrea2.Entities
{
    internal class Zone
    {
        [JsonProperty]
        internal uint ZoneID { get; set; }
        [JsonProperty]
        internal string ZoneName { get; set; }
        [JsonProperty]
        internal uint MinRoom { get; set; }
        [JsonProperty]
        internal uint MaxRoom { get; set; }

        internal Zone ShallowCopy()
        {
            var z = (Zone)this.MemberwiseClone();
            return z;
        }
    }
}
