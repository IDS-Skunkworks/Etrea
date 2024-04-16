using System;

namespace Etrea2
{
    [Flags]
    internal enum NPCFlags
    {
        None = 0,                                   // No behaviour modifiers
        Regen = 1 << 0,                             // NPC will regenerate HP in combat
        NoAttack = 1 << 1,                          // NPC cannot be the target of hostile actions
        Hostile = 1 << 2,                           // NPC is hostile and will try to start combat
        Sentinel = 1 << 3,                          // NPC does not move from the room they are in
        Coward = 1 << 4,                            // NPC will attempt to flee combat if HP drops below 10% of maximum
        Scavenger = 1 << 5,                         // NPC will take items and gold dropped in rooms
        Awareness = 1 << 6,                         // NPC will receive bonuses to certain combat rolls
        Mercenary = 1 << 7,                         // NPC is a mercenary and can be hired by players
        NoPush = 1 << 8,                            // NPC cannot be pushed, regardless of player strength or other stats
    }
}