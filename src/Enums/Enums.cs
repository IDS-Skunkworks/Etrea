using System;

namespace Kingdoms_of_Etrea
{
    internal enum LogLevel
    {
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Connection = 5
    }

    internal enum QuestType
    {
        Fetch,
        Kill
    }

    [Flags]
    internal enum NPCFlags
    {
        None = 0,                    // No modifiers
        MageArmour = 1 << 0,         // +20 AC
        TrueStrike = 1 << 1,         // +20 to hit
        Regen = 1 << 2,              // 1d6 extra hp regenerated on tick
        NoAttack = 1 << 3,           // NPC cannot be attacked
        Vendor = 1 << 4,            // NPC is a vendor and runs a shop
        Hostile = 1 << 5,           // NPC is hostile and will attack PCs
        Sentinel = 1 << 6,          // NPC does not move from the room they are in
        Coward = 1 << 7,           // NPC will flee combat if HP drops <10% of max
        Scavenger = 1 << 8,        // NPC will pick up items dropped on the floor
        Awareness = 1 << 9,        // NPC gets a bonus to Initiative rolls
        BreathWeapon = 1 << 10,    // NPC can use a breath weapon dealing 6d6+6 damage to the target
        Mercenary = 1 << 11       // NPC is a mercenary and can be hired by a player in the world
    }

    [Flags]
    internal enum RoomFlags
    {
        None = 0,               // Default
        Inside = 1 << 0,             // Room is inside, no weather effects
        Cave = 1 << 1,               // Room is a cave, no weather effects, resource nodes appear
        Water = 1 << 2,              // Room is under water
        Safe = 1 << 3,               // Room is safe, no hostile actions
        Shop = 1 << 4,              // Room is a shop
        Healing = 1 << 5,           // Room provides additional regen each tick
        NoTeleport = 1 << 6,        // Room cannot be teleported into or out of
        NoMobs = 1 << 7,           // NPCs cannot enter the room
        SkillTrainer = 1 << 8,     // Room has a trainer that allows players to learn new skills
        MagicTrainer = 1 << 9,     // Room has a trainer that allows players to learn new spells
        StatTrainer = 1 << 10,     // Room has a trainer that allows players to increase core stats
        Gambler = 1 << 11,         // Room has a gambler that allows players to gamble on dice rolls
        Dark = 1 << 12,            // Room is dark, players without a light source cannot see anything in the room
        NoMagic = 1 << 13,         // Magic cannot be cast in the room
        Blacksmith = 1 << 14,     // Room has a forge allowing smelting of ores and forging of items
        Alchemist = 1 << 15,      // Room has an alchemist workbench allowing crafting of potions
        Jeweler = 1 << 16,        // Room has a jeweler's workbench allowing crafting of rings
        Scribe = 1 << 17,        // Room has a scribes workbench allowing crafting of magical scrolls
        Mercenary = 1 << 18,     // Room has a mercenary allowing players to hire a follower
        QuestMaster = 1 << 19,   // Room has a quest master that allows players to take and turn in quests
        PostBox = 1 << 20,       // Room has a mailbox allowing players to send and receive mail
        Banker = 1 << 21,        // Room has a banker allowing players to deposit gold to keep it safe
        NoHealing = 1 << 22,      // Players in this room do not regenerate HP or MP even if resting
    }

    [Flags]
    internal enum SpellType
    {
        Buff = 1,
        Debuff = 2,
        Damage = 4,
        Healing = 8
    }

    internal enum ConnectionState
    {
        Playing,
        Disconnected,
        GetUsername,
        GetPassword,
        NewUsername,
        NewPassword,
        VerifyPassword,
        CreatingCharacter,
        MainMenu
    }

    internal enum Gender
    {
        Unknown,
        Male,
        Female,
        Genderless
    }

    internal enum ActorType
    {
        Unknown,
        Player,
        NonPlayer
    }

    internal enum ActorClass
    {
        Undefined,
        Wizard,
        Thief,
        Cleric,
        Fighter
    }

    internal enum ActorRace
    {
        Undefined,
        Human,
        Elf,
        HalfElf,
        Orc,
        Dwarf,
        Hobbit
    }

    internal enum ActorAlignment
    {
        Evil,
        Neutral,
        Good
    }

    internal enum ActorPosition
    {
        Dead,
        Dying,
        Incapacitated,
        Stunned,
        Sleeping,
        Resting,
        Sitting,
        Standing,
        Fighting,
        InOLC
    }

    [Flags]
    internal enum PotionEffect
    {
        None = 0,
        Healing = 1,
        MPHealing = 2,
        Poison = 4,
        Buff = 8
    }

    internal enum ArmourType
    {
        Light,
        Medium,
        Heavy
    }

    internal enum WeaponType
    {
        Dagger,
        Sword,
        GreatSword,
        Axe,
        Staff,
        Club,
        Bow,
        Crossbow,
        PoleArm
    }

    internal enum RecipeType
    {
        Blacksmithing,
        Alchemy,
        Scribing,
        Jewelcrafting
    }

    internal enum ItemType
    {
        Armour = 1,
        Weapon = 2,
        Potion = 4,
        Scroll = 8,
        Ring = 16,
        Junk = 32
    }
    
    [Flags]
    internal enum WearSlot
    {
        None = 0,
        Head = 1,
        Neck = 2,
        Armour = 4,
        FingerLeft = 8,
        FingerRight = 16,
        Weapon = 32,
        Held = 64
    }
}