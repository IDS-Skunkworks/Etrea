using Kingdoms_of_Etrea.Core;
using Newtonsoft.Json;

namespace Kingdoms_of_Etrea.Entities
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

        public Zone(uint zid)
        {
            ZoneManager.Instance.GetZone(zid);
        }

        internal Zone()
        {

        }
    }
}
