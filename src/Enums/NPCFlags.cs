using System;

namespace Etrea3
{
    [Flags]
    public enum NPCFlags
    {
        None = 0,                                       // No modifiers
        Regeneration = 1 << 0,                          // NPC regenerates constantly, even in combat and no-regen rooms
        NoAttack = 1 << 1,                              // NPC cannot be attacked or the target of hostil actions
        Hostile = 1 << 2,                               // NPC is hostile and may attack players
        Sentinel = 1 << 3,                              // NPC will not move from the room they are in
        Coward = 1 << 4,                                // NPC will flee combat is HP is <= 40% of max
        Scavenger = 1 << 5,                             // NPC may pick up items and gold left on the floor
        NoPush = 1 << 6,                                // NPC cannot be pushed by players regardless of their STR stat
        Flying = 1 << 7,                                // NPC can fly and can only be hit by ranged weapons
        NoBackstab = 1 << 8,                            // NPC cannot be hit with backstab attacks
        NoPickpocket = 1 << 9,                          // NPC cannot have items/gold stolen by the Pickpocket skill
        LitterBug = 1 << 10,                            // NPC may randomly drop items/gold it is carrying
    }
}