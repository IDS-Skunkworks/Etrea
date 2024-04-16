using Newtonsoft.Json;

namespace Etrea2.Entities
{
    internal class Exit
    {
        [JsonProperty]
        internal uint DestinationRoomID { get; set; }
        [JsonProperty]
        internal string ExitDirection { get; set; }
        [JsonProperty]
        internal RoomDoor RoomDoor { get; set; }
        [JsonProperty]
        internal Skill RequiredSkill { get; set; }

        public override string ToString()
        {
            return $"{ExitDirection} ({DestinationRoomID})";
        }
    }
}