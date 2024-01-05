using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
{
    internal class Skills
    {
        [Serializable]
        internal class Skill
        {
            [JsonProperty]
            internal string Name { get; set; }
            [JsonProperty]
            internal string Description { get; set; }
            [JsonProperty]
            internal uint MPCost { get; set; }
            [JsonProperty]
            internal uint GoldToLearn { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        private static List<Skill> _allSkills = new List<Skill>
        {
            new Skill {
                Name = "Darkvision",
                Description = "Allows you to see in places where there is no light",
                MPCost = 0,
                GoldToLearn = 50000 },

            new Skill {
                Name = "Light Armour",
                Description = "Teaches you how to effectively use light armour",
                MPCost = 0,
                GoldToLearn = 10000 },

            new Skill {
                Name = "Medium Armour",
                Description = "Teaches you how to effectively use medium armour",
                MPCost = 0,
                GoldToLearn = 20000 },

            new Skill {
                Name = "Heavy Armour",
                Description = "Teaches you how to effectively use heavy armour",
                MPCost = 0,
                GoldToLearn = 30000 },

            new Skill
            {
                Name = "Heavy Armour Mastery",
                Description = "Provides bonuses when wearing heavy armour",
                MPCost = 0,
                GoldToLearn = 25000 },

            new Skill
            {
                Name = "Medium Armour Mastery",
                Description = "Provides bonuses when wearing medium armour",
                MPCost = 0,
                GoldToLearn = 25000 },

            new Skill
            {
                Name = "Light Armour Mastery",
                Description = "Provides bonuses when wearing light armour",
                MPCost = 0,
                GoldToLearn = 25000 },

            new Skill
            {
                Name = "Simple Weapon Mastery",
                Description = "Provides bonuses when using simple weapons",
                MPCost = 0,
                GoldToLearn = 15000 },

            new Skill
            {
                Name = "Martial Weapon Mastery",
                Description = "Provides bonuses when using martial weapons",
                MPCost = 0,
                GoldToLearn = 36000
            },

            new Skill
            {
                Name = "Exotic Weapon Mastery",
                Description = "Provides bonuses when using exotic weapons",
                MPCost = 0,
                GoldToLearn = 60000
            },

            new Skill {
                Name = "Simple Weapons",
                Description = "Teaches you how to effectively use simple weapons",
                MPCost = 0,
                GoldToLearn = 5000 },

            new Skill {
                Name = "Martial Weapons",
                Description = "Teaches you how to effectively use martial weapons",
                MPCost = 0,
                GoldToLearn = 12000 },

            new Skill {
                Name = "Exotic Weapons",
                Description = "Teaches you how to effectively use exotic weapons",
                MPCost = 0,
                GoldToLearn = 20000 },

            new Skill {
                Name = "Hide",
                Description = "Teaches you how to become nearly invisible",
                MPCost = 8,
                GoldToLearn = 15000 },

            new Skill {
                Name = "Backstab",
                Description = "Teaches you how to effectively attack while hidden",
                MPCost = 10,
                GoldToLearn = 20000 },

            new Skill {
                Name = "Dodge",
                Description = "Provides an additional +2 AC bonus",
                MPCost = 0,
                GoldToLearn = 15000 },

            new Skill {
                Name = "Parry",
                Description = "Provides an additional +2 AC bonus",
                MPCost = 0,
                GoldToLearn = 15000 },

            new Skill {
                Name = "Awareness",
                Description = "Provides a bonus to initiative in comat",
                MPCost = 0,
                GoldToLearn = 15000 },

            new Skill {
                Name = "Monkey Grip",
                Description = "Allows using a shield whilst wielding a two-handed weapon",
                MPCost = 0,
                GoldToLearn = 20000 },

            new Skill {
                Name = "Gambling",
                Description = "Grants a bonus with dicers",
                MPCost = 0,
                GoldToLearn = 40000 },
            
            new Skill {
                Name = "Pickpocket",
                Description = "Lets you steal items from NPCs, if they have any",
                MPCost = 10,
                GoldToLearn = 50000 },

            new Skill {
                Name = "Mercenary",
                Description = "Grants a bonus when dealing with shopkeepers",
                MPCost = 0,
                GoldToLearn = 50000 },

            new Skill
            {
                Name = "Desperate Attack",
                Description = "Provides a damage bonus at the cost of finesse",
                MPCost = 15,
                GoldToLearn = 40000 },
            new Skill
            {
                Name = "Gold Digger",
                Description = "You gain bonus gold from defeated enemies",
                MPCost = 0,
                GoldToLearn = 50000 },
            new Skill
            {
                Name = "Quick Learner",
                Description = "Provides bonus Exp from defeated enemies",
                MPCost = 0,
                GoldToLearn = 50000 },
            new Skill {
                Name = "Jump",
                Description = "Allows you to jump certain obstacles",
                MPCost = 0,
                GoldToLearn = 10000
            },
            new Skill
            {
                Name = "Climb",
                Description = "Allows you to climb ropes and cliffs",
                MPCost = 0,
                GoldToLearn = 10000
            },
            new Skill
            {
                Name = "Read",
                Description = "Allows you to read scrolls",
                MPCost = 0,
                GoldToLearn = 10000
            },
            new Skill
            {
                Name = "Extra Attack",
                Description = "Grants an extra attack per turn",
                MPCost = 0,
                GoldToLearn = 10000
            },
            new Skill
            {
                Name = "Mining",
                Description = "Allows collection of items from resource nodes",
                MPCost = 0,
                GoldToLearn = 15000
            },
            new Skill
            {
                Name = "Blacksmithing",
                Description = "Allows crafting of Blacksmithing recipes",
                MPCost = 0,
                GoldToLearn = 20000
            },
            new Skill
            {
                Name = "Jewelcrafting",
                Description = "Allows crafting of Jewelcrafting recipes",
                MPCost = 0,
                GoldToLearn = 20000
            },
            new Skill
            {
                Name = "Alchemy",
                Description = "Allows crafting of Alchemy recipes",
                MPCost = 0,
                GoldToLearn = 20000
            },
            new Skill
            {
                Name = "Scribing",
                Description = "Allows crafting of Scribing recipes",
                MPCost = 0,
                GoldToLearn = 20000
            },
            new Skill
            {
                Name = "Cooking",
                Description = "Allows the prepartion of food and drinks",
                MPCost = 0,
                GoldToLearn = 10000
            }
        };

        internal static List<Skill> GetAllSkills(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var result = from s in _allSkills where Regex.Match(s.Name, name, RegexOptions.IgnoreCase).Success select s;
                return result.ToList();
            }
            return _allSkills;
        }

        internal static bool SkillExists(string skillName)
        {
            return _allSkills.Any(x => Regex.Match(x.Name, skillName, RegexOptions.IgnoreCase).Success);
        }

        internal static Skill GetSkill(string skillName)
        {
            return _allSkills.Where(x => Regex.Match(x.Name, skillName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
        }
    }
}
