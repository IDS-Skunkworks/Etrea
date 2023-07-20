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
        None = 0,               // No modifiers
        MageArmour = 1,         // +20 AC
        TrueStrike = 2,         // +20 to hit
        Regen = 4,              // 1d6 extra hp regenerated on tick
        NoAttack = 8,           // NPC cannot be attacked
        Vendor = 16,            // NPC is a vendor and runs a shop
        Hostile = 32,           // NPC is hostile and will attack PCs
        Sentinel = 64,          // NPC does not move from the room they are in
        Coward = 128,           // NPC will flee combat if HP drops <10% of max
        Scavenger = 256,        // NPC will pick up items dropped on the floor
        Awareness = 512,        // NPC gets a bonus to Initiative rolls
        BreathWeapon = 1024,    // NPC can use a breath weapon dealing 6d6+6 damage to the target
        Mercenary = 2048        // NPC is a mercenary and can be hired by a player in the world
    }

    [Flags]
    internal enum RoomFlags
    {
        None = 0,               // Default
        Inside = 1,             // Room is inside, no weather effects
        Cave = 2,               // Room is a cave, no weather effects, resource nodes appear
        Water = 4,              // Room is under water
        Safe = 8,               // Room is safe, no hostile actions
        Shop = 16,              // Room is a shop
        Healing = 32,           // Room provides additional regen each tick
        NoTeleport = 64,        // Room cannot be teleported into or out of
        NoMobs = 128,           // NPCs cannot enter the room
        SkillTrainer = 256,     // Room has a trainer that allows players to learn new skills
        MagicTrainer = 512,     // Room has a trainer that allows players to learn new spells
        StatTrainer = 1024,     // Room has a trainer that allows players to increase core stats
        Gambler = 2048,         // Room has a gambler that allows players to gamble on dice rolls
        Dark = 4096,            // Room is dark, players without a light source cannot see anything in the room
        NoMagic = 8192,         // Magic cannot be cast in the room
        Blacksmith = 16384,     // Room has a forge allowing smelting of ores and forging of items
        Alchemist = 32768,      // Room has an alchemist workbench allowing crafting of potions
        Jeweler = 65536,        // Room has a jeweler's workbench allowing crafting of rings
        Scribe = 131072,        // Room has a scribes workbench allowing crafting of magical scrolls
        Mercenary = 262144,     // Room has a mercenary allowing players to hire a follower
        QuestMaster = 524288,   // Room has a quest master that allows players to take and turn in quests
        PostBox = 1048576       // Room has a mailbox allowing players to send and receive mail
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