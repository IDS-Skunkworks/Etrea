using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
{
    internal class Buffs
    {
        [Serializable]
        internal class Buff
        {
            [JsonProperty]
            internal string BuffName { get; set; }
            [JsonProperty]
            internal string Description { get; set; }
            [JsonProperty]
            internal int BuffDuration { get; set; }
        }

        private static List<Buff> _buffs = new List<Buff>
        {
            new Buff
            {
                BuffName = "Bless",
                Description = "Provides mystical bonuses",
                BuffDuration = 10
            },
            new Buff
            {
                BuffName = "Mage Armour",
                Description = "Provides a bonus to AC in combat",
                BuffDuration = 2
            },
            new Buff
            {
                BuffName = "Light",
                Description = "Allows you, and those with you, to see in dark places",
                BuffDuration = 10
            },
            new Buff
            {
                BuffName = "Truestrike",
                Description = "Provides a bonus when striking enemies",
                BuffDuration = 2
            },
            new Buff
            {
                BuffName = "Bulls Strength",
                Description = "Provides a bonus to Strength",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Cats Grace",
                Description = "Provides a bonus to Dexterity",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Bears Endurance",
                Description = "Provides a bonus to Constitution",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Owls Wisdom",
                Description = "Provides a bonus to Wisdom",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Foxs Cunning",
                Description = "Provides a bonus to Intelligence",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Eagles Splendour",
                Description = "Provides a bonus to Charisma",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Acid Arrow",
                Description = "Magical acid deals damage over time",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Regen",
                Description = "Heals a small amount of damage over time",
                BuffDuration = 6
            },
            new Buff
            {
                BuffName = "Desperate Attack",
                Description = "Provides a damage bonus at the cost of finesse",
                BuffDuration = 1
            },
            new Buff
            {
                BuffName = "Fae Fire",
                Description = "Magical energies make the target easier to strike",
                BuffDuration = 4
            },
            new Buff
            {
                BuffName = "Poison",
                Description = "Toxins course through your body causing damage over time",
                BuffDuration = 6
            }
        };

        internal static List<Buff> GetAllBuffs()
        {
            return _buffs;
        }

        internal static bool BuffExists(string buffName)
        {
            return _buffs.Any(x => Regex.Match(x.BuffName, buffName, RegexOptions.IgnoreCase).Success);
        }

        internal static Buff GetBuff(string buffName)
        {
            return _buffs.Where(x => Regex.Match(x.BuffName, buffName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
        }
    }
}
