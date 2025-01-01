using System;

namespace Etrea3
{
    [Flags]
    public enum MobProgTrigger
    {
        None = 0,                                   // should never be used
        MudTick = 1 << 0,                           // when the MUD ticks MOBs
        PlayerEnter = 1 << 1,                       // when a player enters the same room as the mob
        PlayerLeave = 1 << 2,                       // when a player leaves the room the mob is in
        ReceiveItem = 1 << 3,                       // when a player gives an item to the mob
        MobAttacked = 1 << 4,                       // when the mob is attacked
        MobFlees = 1 << 5,                          // when the mob flees combat
        MobDeath = 1 << 6,                          // when the mob is killed
        EmoteTarget = 1 << 7,                       // when the mob is the target of an emote
        PlayerSay = 1 << 8,                         // when a player says something the mob can hear
        PlayerWhisper = 1 << 9,                     // when a player whispers something to the mob
        MobEnter = 1 << 10,                         // when a mob enters a room
        MobLeave = 1 << 11,                         // when a mob leaves a room
        PlayerLook = 1 << 12,                       // when a player looks at a mob
        ReceiveGold = 1 << 13,                      // when a player gives gold to the mob
        PlayerDropItem = 1 << 14,                   // when a player drops an item
        PlayerTakeItem = 1 << 15,                   // when a player takes an item
        PlayerPurchaseItem = 1 << 16,               // when a player purchases an item from a shop
        PlayerSellItem = 1 << 17,                   // when a player sells an item to a shop
        PlayerTakeGold = 1 << 18,                   // when a player takes gold from a room
        PlayerDropGold = 1 << 19,                   // when a player drops gold on the floor
        PlayerEnterShop = 1 << 20,                  // when a player enters a shop context to do business
        PlayerLeaveShop = 1 << 21,                  // when a player leaves a shop context
    }
}