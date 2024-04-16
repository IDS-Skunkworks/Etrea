using Etrea2.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Etrea2.Core
{
    internal class SkillManager
    {
        private List<Skill> Skills { get; set; }
        private static SkillManager _instance = null;
        private static readonly object _lock = new object();

        private SkillManager()
        {
            Skills = new List<Skill>
            {
                new Skill
                {
                    Name = "Darkvision",
                    Description = "Allows you to see in places where there is no light",
                    MPCost = 0,
                    GoldToLearn = 6000
                },
                new Skill
                {
                    Name = "Light Armour",
                    Description = "Allows you to effectively use light armour",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Medium Armour",
                    Description = "Allows you to effectively use medium armour",
                    MPCost = 0,
                    GoldToLearn = 3000
                },
                new Skill
                {
                    Name = "Heavy Armour",
                    Description = "Allows you effectively use heavy armour",
                    MPCost = 0,
                    GoldToLearn = 5000
                },
                new Skill
                {
                    Name = "Light Armour Mastery",
                    Description = "Provides bonuses when using light armour",
                    MPCost = 0,
                    GoldToLearn = 4000
                },
                new Skill
                {
                    Name = "Medium Armour Mastery",
                    Description = "Provides bonuses when using medium armour",
                    MPCost = 0,
                    GoldToLearn = 6000
                },
                new Skill
                {
                    Name = "Heavy Armour Mastery",
                    Description = "Provides bonuses when using heavy armour",
                    MPCost = 0,
                    GoldToLearn = 10000
                },
                new Skill
                {
                    Name = "Simple Weapons",
                    Description = "Allows you to use simple weapons",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Martial Weapons",
                    Description = "Allows you to use martial weapons",
                    MPCost = 0,
                    GoldToLearn = 3000
                },
                new Skill
                {
                    Name = "Exotic Weapons",
                    Description = "Allows you to use exotic weapons",
                    MPCost = 0,
                    GoldToLearn = 4000
                },
                new Skill
                {
                    Name = "Simple Weapon Mastery",
                    Description = "Provides bonuses when using simple weapons",
                    MPCost = 0,
                    GoldToLearn = 4000
                },
                new Skill
                {
                    Name = "Martial Weapon Mastery",
                    Description = "Provides bonuses when using martial weapons",
                    MPCost = 0,
                    GoldToLearn = 6000
                },
                new Skill
                {
                    Name = "Exotic Weapon Mastery",
                    Description = "Provides bonuses when using exotic weapons",
                    MPCost = 0,
                    GoldToLearn = 8000
                },
                new Skill
                {
                    Name = "Dodge",
                    Description = "Provides armour class bonuses in combat when not wearing armour",
                    MPCost = 0,
                    GoldToLearn = 3000
                },
                new Skill
                {
                    Name = "Parry",
                    Description = "Provides armour class bonuses in combat when using Finesse weapons",
                    MPCost = 0,
                    GoldToLearn = 3000
                },
                new Skill
                {
                    Name = "Awareness",
                    Description = "Provides various bonuses in combat",
                    MPCost = 0,
                    GoldToLearn = 6000
                },
                new Skill
                {
                    Name = "Monkey Grip",
                    Description = "Allows use of a held item when using a two-handed weapons",
                    MPCost = 0,
                    GoldToLearn = 10000
                },
                new Skill
                {
                    Name = "Mercenary",
                    Description = "Grants bonuses when buying and selling items at shops",
                    MPCost = 0,
                    GoldToLearn = 5000
                },
                new Skill
                {
                    Name = "Gold Digger",
                    Description = "Allows you to get bonus gold from defeated enemies",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Gambling",
                    Description = "Grants bonuses in dice games",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Pickpocket",
                    Description = "Allows you steal items from NPCs",
                    MPCost = 5,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Backstab",
                    Description = "Launch a sneak attack from the shaodws",
                    MPCost = 15,
                    GoldToLearn = 8000
                },
                new Skill
                {
                    Name = "Desperate Attack",
                    Description = "Provides bonus damage at the cost of finesse",
                    MPCost = 10,
                    GoldToLearn = 4000
                },
                new Skill
                {
                    Name = "Quick Learner",
                    Description = "Allows you get bonus Exp from defeated enemies",
                    MPCost = 0,
                    GoldToLearn = 4000
                },
                new Skill
                {
                    Name = "Jump",
                    Description = "Allows you to jump certain obstacles",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Climb",
                    Description = "Allows you to climb certain obstacles",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Swim",
                    Description = "Allows you to swim over some bodies of water",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Read",
                    Description = "Allows you read scrolls",
                    MPCost = 0,
                    GoldToLearn = 2000
                },
                new Skill
                {
                    Name = "Extra Attack",
                    Description = "Grants an extra attack in combat",
                    MPCost = 0,
                    GoldToLearn = 3000
                },
                new Skill
                {
                    Name = "Mining",
                    Description = "Allows collection of resources from resource nodes",
                    MPCost = 0,
                    GoldToLearn = 1500
                },
                new Skill
                {
                    Name = "Blacksmithing",
                    Description = "Allows learning and crafting Blacksmithing recipes",
                    MPCost = 0,
                    GoldToLearn = 1500
                },
                new Skill
                {
                    Name = "Jewelcrafting",
                    Description = "Allows learning and crafting Jewelcrafting recipes",
                    MPCost = 0,
                    GoldToLearn = 1500
                },
                new Skill
                {
                    Name = "Alchemy",
                    Description = "Allows learning and crafting Alchemy recipes",
                    MPCost = 0,
                    GoldToLearn = 1500
                },
                new Skill
                {
                    Name = "Scribing",
                    Description = "Allows learning and crafting Scribe recipes",
                    MPCost = 0,
                    GoldToLearn = 1500
                },
                new Skill
                {
                    Name = "Cooking",
                    Description = "Allows learning and crafting of Cooking recipes",
                    MPCost = 0,
                    GoldToLearn = 1500
                }
            };
        }

        internal static SkillManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SkillManager();
                }
                return _instance;
            }
        }

        internal bool SkillExists(string name)
        {
            lock(_lock)
            {
                return Instance.Skills.Any(x => x.Name.ToLower() == name.ToLower());
            }
        }

        internal Skill GetSkill(string name)
        {
            lock(_lock)
            {
                return Instance.Skills.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            }
        }

        internal List<Skill> GetSkillByNameOrDescription(string name)
        {
            lock(_lock)
            {
                return Instance.Skills.Where(x => Regex.IsMatch(x.Name, name, RegexOptions.IgnoreCase)
                || Regex.IsMatch(x.Description, name, RegexOptions.IgnoreCase)).ToList();
            }
        }

        internal List<Skill> GetAllSkills()
        {
            lock (_lock)
            {
                return Instance.Skills;
            }
        }
    }
}
