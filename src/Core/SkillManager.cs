using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class SkillManager
    {
        private static SkillManager instance = null;
        private ConcurrentBag<Skill> Skills;
        public int Count => Skills.Count;

        private SkillManager()
        {
            Skills = new ConcurrentBag<Skill>
            {
                new Skill
                {
                    Name = "Light Armour",
                    Description = "Allows you to wear light armour",
                    AvailableToClass = ActorClass.Fighter | ActorClass.Thief | ActorClass.Wizard | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Medium Armour",
                    Description = "Allows you to wear medium armour",
                    AvailableToClass = ActorClass.Fighter | ActorClass.Thief | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Heavy Armour",
                    Description = "Allows you to wear heavy armour",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Simple Weapons",
                    Description = "Allows you to use simple weapons",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Martial Weapons",
                    Description = "Allows you use martial weapons",
                    AvailableToClass = ActorClass.Fighter | ActorClass.Thief | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Exotic Weapons",
                    Description = "Allows you to use exotic weapons",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Salesman",
                    Description = "Gives you more favourable prices when buying and selling",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Thief | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Gambling",
                    Description = "Gives you more favourable results when betting with dicers",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Cleric | ActorClass.Fighter | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Gold Digger",
                    Description = "Allows you to find bonus gold from defeated enemies",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Thief | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Monkey Grip",
                    Description = "Allows you to hold an off-hand item while using a two-handed weapon",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Read Scroll",
                    Description = "Allows you to read the magic inscribed in magical scrolls",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Scribe",
                    Description = "Allows you to imbue scrolls with the power of magic",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Cleric | ActorClass.Thief | ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Jeweler",
                    Description = "Allows you to learn and craft Jeweler recipes",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Cleric | ActorClass.Thief | ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Blacksmith",
                    Description = "Allows the use of a Forge to create blacksmithing recipes",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Thief | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Alchemist",
                    Description = "Allows the learning and crafting of alchemical recipes",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Thief | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Cooking",
                    Description = "Allows the learning and crafting of recipes to make consumables",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Mining",
                    Description = "Allows mining materials from resource nodes",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Thief | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Dodge",
                    Description = "Provides combat bonuses when lightly armoured",
                    AvailableToClass = ActorClass.Thief | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Parry",
                    Description = "Provides combat bonuses when using certain weapons",
                    AvailableToClass = ActorClass.Thief | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Extra Attack",
                    Description = "Grants extra attacks per combat round",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Climb",
                    Description = "Required to traverse some routes around the world",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Swim",
                    Description = "Required to traverse some routes around the world",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Jump",
                    Description = "Required to traverse some routes around the world",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Blademaster",
                    Description = "Provides combat bonuses when using edged and bladed weapons",
                    AvailableToClass = ActorClass.Fighter | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Sniper",
                    Description = "Provides combat bonuses when using ranged weapons",
                    AvailableToClass = ActorClass.Fighter | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Pugilism",
                    Description = "Provides combat bonuses when not using a weapon",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Desperate Attack",
                    Description = "Adopt a stance which sacrifices defence for pure damage",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Defensive Stance",
                    Description = "Adopt a stance that sacrifices damage output for better defence",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Elite Striker",
                    Description = "You have a knack for exploting enemy weaknesses, scoring critical hits more often",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Disarm",
                    Description = "A quick flurry of feints may cause an enemy to lose their weapon",
                    AvailableToClass = ActorClass.Fighter | ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Stun",
                    Description = "A well-placed strike doesn't need to cause damage if it leaves an enemy vulnerable",
                    AvailableToClass = ActorClass.Thief | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Whirlwind",
                    Description = "A spinning attack, striking everything in the area",
                    AvailableToClass = ActorClass.Fighter,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Fan of Knives",
                    Description = "A blur of knives in every direction, striking everything in the area",
                    AvailableToClass = ActorClass.Thief,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Quick Learner",
                    Description = "Provides bonus Exp from fallen enemies",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Thief | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Land Walker",
                    Description = "You are one with the land and the terrain is no barrier to your movement",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Fighter | ActorClass.Thief | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Pickpocket",
                    Description = "Allows you to steal gold and items from NPCs",
                    AvailableToClass = ActorClass.Thief | ActorClass.Wizard | ActorClass.Fighter | ActorClass.Cleric,
                    LearnCost = 5000
                },
                new Skill
                {
                    Name = "Summon",
                    Description = "Allows you to summon players to your location",
                    AvailableToClass = ActorClass.Wizard | ActorClass.Cleric,
                    LearnCost = 8000
                }
            };
        }

        public static SkillManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SkillManager();
                }
                return instance;
            }
        }

        public Skill GetSkill(string name)
        {
            return Instance.Skills.FirstOrDefault(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public List<Skill> GetSkill()
        {
            return Instance.Skills.ToList();
        }

        public List<Skill> GetSkill(ActorClass actorClass)
        {
            return Instance.Skills.Where(x => x.AvailableToClass.HasFlag(actorClass)).ToList();
        }
    }
}
