using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
{
    internal class Spells
    {
        [Serializable]
        internal class Spell
        {
            [JsonProperty]
            internal string SpellName { get; set; }
            [JsonProperty]
            internal string Description { get; set; }
            [JsonProperty]
            internal SpellType SpellType { get; set; }
            [JsonProperty]
            internal uint MPCost { get; set; }
            [JsonProperty]
            internal uint NumOfDamageDice { get; set; }
            [JsonProperty]
            internal uint SizeOfDamageDice { get; set; }
            [JsonProperty]
            internal bool AutoHitTarget { get; set; }
            [JsonProperty]
            internal bool RequiresTarget { get; set; }
            [JsonProperty]
            internal uint GoldToLearn { get; set; }
            public override string ToString()
            {
                return SpellName;
            }
        }

        private static List<Spell> _allSpells = new List<Spell>
        {
            new Spell
            {
                SpellName = "Bless",
                Description = "Provides mystical bonuses",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 10000
            },

            new Spell
            {
                SpellName = "Guiding Bolt",
                Description = "Blasts a target with holy power",
                SpellType = SpellType.Damage,
                MPCost = 14,
                NumOfDamageDice = 2,
                SizeOfDamageDice = 8,
                AutoHitTarget = false,
                RequiresTarget = true,
                GoldToLearn = 10000
            },

            new Spell
            {
                SpellName = "Magic Missile",
                Description = "Fires seeking missiles of pure force at your target",
                SpellType = SpellType.Damage,
                MPCost = 10,
                NumOfDamageDice = 1,
                SizeOfDamageDice = 4,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 10000
            },
            new Spell
            {
                SpellName = "Light",
                Description = "Creates an orb that follows you, providing a source of light",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 6000
            },
            new Spell
            {
                SpellName = "Truestrike",
                Description = "Mystical forces make it easier to hit foes",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 10000
            },
            new Spell
            {
                SpellName = "Mage Armour",
                Description = "Magical energies provide short-term protection in combat",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 10000
            },
            new Spell
            {
                SpellName = "Acid Arrow",
                Description = "Fires a bolt of magical acid, dealing damage over time",
                SpellType = SpellType.Debuff,
                MPCost = 10,
                NumOfDamageDice = 1,
                SizeOfDamageDice = 3,
                AutoHitTarget = false,
                RequiresTarget = true,
                GoldToLearn = 10000
            },
            new Spell
            {
                SpellName = "Bulls Strength",
                Description = "Provides a bonus to Strength",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Cats Grace",
                Description = "Provides a bonus to Dexterity",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Bears Endurance",
                Description = "Provides a bonus to Constitution",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Owls Wisdom",
                Description = "Provides a bonus to Wisdom",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Foxs Cunning",
                Description = "Provides a bonus to Intelligence",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Eagles Splendour",
                Description = "Provides a bonus to Charisma",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Cure Light Wounds",
                Description = "Heals a small amount of damage",
                SpellType = SpellType.Healing,
                MPCost = 10,
                NumOfDamageDice = 1,
                SizeOfDamageDice = 8,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 10000
            },
            new Spell
            {
                SpellName = "Cure Moderate Wounds",
                Description = "Heals a moderate amount of damage",
                SpellType = SpellType.Healing,
                MPCost = 15,
                NumOfDamageDice = 2,
                SizeOfDamageDice = 8,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 15000
            },
            new Spell
            {
                SpellName = "Cure Serious Wounds",
                Description = "Heals a large amount of damage",
                SpellType = SpellType.Healing,
                MPCost = 20,
                NumOfDamageDice = 3,
                SizeOfDamageDice = 8,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 20000
            },
            new Spell
            {
                SpellName = "Cure Critical Wounds",
                Description = "Heals a large amount of damage",
                SpellType = SpellType.Healing,
                MPCost = 30,
                NumOfDamageDice = 4,
                SizeOfDamageDice = 8,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 25000
            },
            new Spell
            {
                SpellName = "Regen",
                Description = "Heals a small amount of damage over time",
                SpellType = SpellType.Buff,
                MPCost = 10,
                NumOfDamageDice = 1,
                SizeOfDamageDice = 4,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 9000
            },
            new Spell
            {
                SpellName = "Fae Fire",
                Description = "Magic energies make the target easier to strike",
                SpellType = SpellType.Debuff,
                MPCost = 10,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 8000
            },
            new Spell
            {
                SpellName = "Firebolt",
                Description = "A blast of fire causes damage",
                SpellType = SpellType.Damage,
                MPCost = 20,
                NumOfDamageDice = 3,
                SizeOfDamageDice = 4,
                AutoHitTarget = false,
                RequiresTarget = true,
                GoldToLearn = 20000
            },
            new Spell
            {
                SpellName = "Fireball",
                Description = "An explosive ball of fire causes damage",
                SpellType = SpellType.Damage,
                MPCost = 35,
                NumOfDamageDice = 8,
                SizeOfDamageDice = 6,
                AutoHitTarget = false,
                RequiresTarget = true,
                GoldToLearn = 28000
            },
            new Spell
            {
                SpellName = "Lightning Bolt",
                Description = "A blast of lightning deals damage",
                SpellType = SpellType.Damage,
                MPCost = 40,
                NumOfDamageDice = 8,
                SizeOfDamageDice = 6,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 30000
            },
            new Spell
            {
                SpellName = "Lightning Blast",
                Description = "A massive blast of lightning deals damage",
                SpellType = SpellType.Damage,
                MPCost = 50,
                NumOfDamageDice = 10,
                SizeOfDamageDice = 10,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 35000
            },
            new Spell
            {
                SpellName = "Restoration",
                Description = "Heals a massive amount of damage",
                SpellType = SpellType.Healing,
                MPCost = 50,
                NumOfDamageDice = 10,
                SizeOfDamageDice = 8,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 30000
            },
            new Spell
            {
                SpellName = "Poison",
                Description = "Magical poison deals damage over time",
                SpellType = SpellType.Debuff,
                MPCost = 20,
                NumOfDamageDice = 1,
                SizeOfDamageDice = 6,
                AutoHitTarget = false,
                RequiresTarget = true,
                GoldToLearn = 12000
            },
            new Spell
            {
                SpellName = "Silence",
                Description = "Magically silence a target to stop them using magic",
                SpellType = SpellType.Debuff,
                MPCost = 20,
                NumOfDamageDice = 0,
                SizeOfDamageDice = 0,
                AutoHitTarget = true,
                RequiresTarget = true,
                GoldToLearn = 12000
            }
        };

        internal static List<Spell> GetAllSpells(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var result = from s in _allSpells where Regex.Match(s.SpellName, name, RegexOptions.IgnoreCase).Success
                             || Regex.Match(s.Description, name, RegexOptions.IgnoreCase).Success select s;
                return result.ToList();
            }
            return _allSpells;
        }

        internal static bool SpellExists(string spellName)
        {
            return _allSpells.Any(x => Regex.Match(x.SpellName, spellName, RegexOptions.IgnoreCase).Success);
        }

        internal static Spell GetSpell(string spellName)
        {
            return _allSpells.Where(x => Regex.Match(x.SpellName, spellName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
        }
    }
}
