using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class RoomExit
    {
        [JsonProperty]
        public int DestinationRoomID { get; set; }
        [JsonProperty]
        public string ExitDirection { get; set; }
        [JsonProperty]
        public string RequiredSkill { get; set; }
        public override string ToString()
        {
            return $"{ExitDirection} to Room {DestinationRoomID}";
        }
    }
}
