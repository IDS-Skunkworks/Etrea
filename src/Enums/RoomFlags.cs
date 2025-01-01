using System;

namespace Etrea3
{
    [Flags]
    public enum RoomFlags : ulong
    {
        None = 0,                                   // No flags
        Inside = 1 << 0,                            // Inside, no weather
        Cave = 1 << 1,                              // Cave, no weather, can spawn mining nodes
        Water = 1 << 2,                             // Water, needs Swimming skill to cross
        Safe = 1 << 3,                              // No attacking or hostile actions
        Healing = 1 << 4,                           // Bonus to tick regeneration for all in the room
        NoTeleport = 1 << 5,                        // Cannot teleport into/out of this room
        NoMobs = 1 << 6,                            // Mobs cannot enter this room
        SkillTrainer = 1 << 7,                      // Skill trainer for learning skills
        MagicTrainer = 1 << 8,                      // Magic trainer for learning new spells
        StatTrainer = 1 << 9,                       // Stat trainer to improve core stats
        Gambler = 1 << 10,                          // Gambler for playing the dice game
        Dark = 1 << 11,                             // No natural light source, cannot see into/out of without bringing ligth
        NoMagic = 1 << 12,                          // Magic cannot be used here
        Blacksmith = 1 << 13,                       // Blacksmith allows learning/crafting Blacksmith recipes
        Alchemist = 1 << 14,                        // Alchemist allows learning/crafting Alchemy recipes
        Jeweler = 1 << 15,                          // Jeweler allows learning/crafting Jeweler recipes
        QuestMaster = 1 << 16,                      // Quests can be taken and returned here
        PostBox = 1 << 17,                          // Players can read/write mail here
        Banker = 1 << 18,                           // Players can deosit/withdraw gold here - stored gold isn't lost on death
        NoHealing = 1 << 19,                        // No automatic regen in this room
        Vault = 1 << 20,                            // Players can deposit/withdraw items here - stored items aren't lost on death
        LanguageTrainer = 1 << 21,                  // Trainer allows players to learn additional languages
        HardTerrain = 1 << 22,                      // Drains 2* normal SP to move into/out of this room
        Chef = 1 << 23,                             // Chef allows learning/crafting Cooking recipes
        Exorcist = 1 << 24,                         // Exorcist allows players to pay to remove cursed equipment
        Scribe = 1 << 25,                           // Scribe allows learning/crafting Scribe recipes
        Sign = 1 << 26,                             // Sign can be looked at with the READ command
        GodRoom = 1 << 27,                          // Only Immortals can enter this room
    }
}