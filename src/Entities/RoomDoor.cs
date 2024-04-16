using Newtonsoft.Json;

namespace Etrea2.Entities
{
    internal class RoomDoor
    {
        [JsonProperty]
        internal uint RequiredItemID { get; set; }
        [JsonProperty]
        internal bool IsLocked { get; set; }
        [JsonProperty]
        internal bool IsOpen { get; set; }
    }
}
