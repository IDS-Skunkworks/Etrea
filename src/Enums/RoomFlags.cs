using System;

namespace Etrea2
{
    [Flags]
    internal enum RoomFlags
    {
        None = 0,                                                           // Default
        Inside = 1 << 0,                                                    // Room is inside, no weather effects
        Cave = 1 << 1,                                                      // Room is inside, no weather effects; ResourceNodes can spawn
        Water = 1 << 2,                                                     // Room is in or under water, players must have the Swim skill or something similar to navigate
        Safe = 1 << 3,                                                      // Room is safe, no hostile actions here
        Shop = 1 << 4,                                                      // Room is a shop where players can trade with an NPC vendor
        Healing = 1 << 5,                                                   // Room provides addiitonal regen each tick
        NoTeleport = 1 << 6,                                                // Players cannot teleport into or out of this room
        NoMobs = 1 << 7,                                                    // NPCs cannot enter this room
        SkillTrainer = 1 << 8,                                              // Room has a trainer that allows players to learn new skills
        MagicTrainer = 1 << 9,                                              // Room has a trainer that allows players to learn new spells
        StatTrainer = 1 << 10,                                              // Room has a trainer that allows players to increase core stats
        Gambler = 1 << 11,                                                  // Room has a gambler that allows players to wager on dice rolls
        Dark = 1 << 12,                                                     // Room is dark, players without a lightsource or nightvision cannot see anything
        NoMagic = 1 << 13,                                                  // No spells can be cast in this room
        Blacksmith = 1 << 14,                                               // Room has a smithy allowing players to learn and craft Blacksmithing recipes
        Alchemist = 1 << 15,                                                // Room has an alchemy workshop allowing players to learn and craft Alchemy recipes
        Jeweler = 1 << 16,                                                  // Room has a jeweler workshop allowing players to learn and craft Jeweler recipes
        Scribe = 1 << 17,                                                   // Room has a scribe, allowing players to learn and craft Scribe recipes
        Mercenary = 1 << 18,                                                // Room has Mercenary Captain NPC allowing players to recruit followers
        QuestMaster = 1 << 19,                                              // Room has a Quest Master NPC allowing players to take and return quests
        PostBox = 1 << 20,                                                  // Room has a mailbox allowing players to send, receive and read mail
        Banker = 1 << 21,                                                   // Room has a Banker NPC allowing players to deposit gold for safe keeping
        NoHealing = 1 << 22,                                                // Players in this room do not regenerate
        ItemVault = 1 << 23,                                                // Room has a vault allowing players to store and retrieve items
        LanguageTrainer = 1 << 24,                                          // Room has an NPC allowing players to learn additional languages
        HardTerrain = 1 << 25,                                              // Moving to or from this room costs players an additional 1d4 stamina
        Chef = 1 << 26,                                                     // Room has a kitchen allowing players to learn and craft Cooking recipes
        Exorcist = 1 << 27,                                                 // Room has a priest that can remove cursed items from player's equipment
    }
}