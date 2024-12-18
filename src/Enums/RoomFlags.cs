using System;

namespace Etrea3
{
    [Flags]
    public enum RoomFlags : ulong
    {
        None = 0,
        Inside = 1 << 0,
        Cave = 1 << 1,
        Water = 1 << 2,
        Safe = 1 << 3,
        Healing = 1 << 4,
        NoTeleport = 1 << 5,
        NoMobs = 1 << 6,
        SkillTrainer = 1 << 7,
        MagicTrainer = 1 << 8,
        StatTrainer = 1 << 9,
        Gambler = 1 << 10,
        Dark = 1 << 11,
        NoMagic = 1 << 12,
        Blacksmith = 1 << 13,
        Alchemist = 1 << 14,
        Jeweler = 1 << 15,
        QuestMaster = 1 << 16,
        PostBox = 1 << 17,
        Banker = 1 << 18,
        NoHealing = 1 << 19,
        Vault = 1 << 20,
        LanguageTrainer = 1 << 21,
        HardTerrain = 1 << 22,
        Chef = 1 << 23,
        Exorcist = 1 << 24,
        Scribe = 1 << 25,
        Sign = 1 << 26,
    }
}