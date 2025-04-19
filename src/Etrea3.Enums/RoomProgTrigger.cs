using System;

namespace Etrea3
{
    [Flags]
    public enum RoomProgTrigger
    {
        // TODO: Add more flags...
        None = 0,                                                           // Should never be used
        PlayerEnter = 1 << 0,                                               // Player enters a room
        PlayerLeave = 1 << 1,                                               // Player leaves a room
        MobEnter = 1 << 2,                                                  // NPC enters a room
        MobLeave = 1 << 3,                                                  // NPC leaves a room
        MudTick = 1 << 4,                                                   // Every tick of the MUD clock
        MobDeath = 1 << 5,                                                  // When a MOB dies in combat in the room
        PlayerDeath = 1 << 6,                                               // When a Player dies in combat in the room
        TimeOfDayChange = 1 << 7,                                           // When TOD is updated to a new value
    }
}