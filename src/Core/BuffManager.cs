using Etrea2.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Etrea2.Core
{
    internal class BuffManager
    {
        private List<Buff> Buffs { get; set; }
        private static BuffManager _instance = null;
        private static readonly object _lock = new object();

        private BuffManager()
        {
            Buffs = new List<Buff>
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
                    BuffName = "Barkskin",
                    Description = "Provides a small level of damage reduction",
                    BuffDuration = 4
                },
                new Buff
                {
                    BuffName = "Stoneskin",
                    Description = "Provides a moderate level of damage reduction",
                    BuffDuration = 4
                },
                new Buff
                {
                    BuffName = "Ironskin",
                    Description = "Provides a good level of damage reduction",
                    BuffDuration = 4
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
                    Description = "Magic acid deals damage over time",
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
                    Description = "Magical energies make the target easier to hit in combat",
                    BuffDuration = 4
                },
                new Buff
                {
                    BuffName = "Poison",
                    Description = "Toxins course through your body causing damage over time",
                    BuffDuration = 6
                },
                new Buff
                {
                    BuffName = "Silence",
                    Description = "Your ability to cast spells is impared",
                    BuffDuration = 4
                },
                new Buff
                {
                    BuffName = "Minor Fire Resistance",
                    Description = "Provides a small resistance to elemental fire",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Fire Resistance",
                    Description = "Provides a moderate resistance to elemental fire",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Fire Resistance",
                    Description = "Provides excellent resistance to elemental fire",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Minor Ice Resistance",
                    Description = "Provides a small resistance to elemental ice",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Ice Resistance",
                    Description = "Provides a moderate resistance to elemental ice",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Ice Resistance",
                    Description = "Provides excellent resistance to elemental ice",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Minor Lightning Resistance",
                    Description = "Provides a small resistance to elemental lightning",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Lightning Resistance",
                    Description = "Provides a moderate resistance to elemental lightning",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Lightning Resistance",
                    Description = "Provides excellent resistance to elemental lightning",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Minor Earth Resistance",
                    Description = "Provides a small resistance to elemental earth",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Earth Resistance",
                    Description = "Provides a moderate resistance to elemental earth",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Earth Resistance",
                    Description = "Provides excellent resistance to elemental earth",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Minor Holy Resistance",
                    Description = "Provides a small resistance to elemental holy",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Holy Resistance",
                    Description = "Provides a moderate resistance to elemental holy",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Holy Resistance",
                    Description = "Provides excellent resistance to elemental holy",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Minor Dark Resistance",
                    Description = "Provides a small resistance to elemental dark",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Dark Resistance",
                    Description = "Provides a moderate resistance to elemental dark",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Dark Resistance",
                    Description = "Provides excellent resistance to elemental dark",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Lesser Fire Weakness",
                    Description = "Reduces resistance to elemental fire",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Lesser Ice Weakness",
                    Description = "Reduces resistance to elemental ice",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Lesser Lightning Weakness",
                    Description = "Reduces resistance to elemental lightning",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Lesser Earth Weakness",
                    Description = "Reduces resistance to elemental earth",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Lesser Holy Weakness",
                    Description = "Reducses resistance to elemental holy",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Lesser Dark Weakness",
                    Description = "Reduces resistance to elemental dark",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Fire Weakness",
                    Description = "Reduces resistance to elemental fire",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Ice Weakness",
                    Description = "Reduces resistance to elemental ice",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Lightning Weakness",
                    Description = "Reduces resistance to elemental lightning",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Earth Weakness",
                    Description = "Reduces resistance to elemental earth",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Holy Weakness",
                    Description = "Reducses resistance to elemental holy",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Moderate Dark Weakness",
                    Description = "Reduces resistance to elemental dark",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Fire Weakness",
                    Description = "Reduces resistance to elemental fire",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Ice Weakness",
                    Description = "Reduces resistance to elemental ice",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Lightning Weakness",
                    Description = "Reduces resistance to elemental lightning",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Earth Weakness",
                    Description = "Reduces resistance to elemental earth",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Holy Weakness",
                    Description = "Reducses resistance to elemental holy",
                    BuffDuration = 5
                },
                new Buff
                {
                    BuffName = "Greater Dark Weakness",
                    Description = "Reduces resistance to elemental dark",
                    BuffDuration = 5
                },
            };
        }

        internal static BuffManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuffManager();
                }
                return _instance;
            }
        }

        internal bool BuffExists(string buffName)
        {
            lock (_lock)
            {
                return Instance.Buffs.Where(x => x.BuffName.ToLower() == buffName.ToLower()).Any();
            }
        }

        internal Buff GetBuff(string buffName)
        {
            lock (_lock)
            {
                return Instance.Buffs.Where(x => x.BuffName == buffName).FirstOrDefault();
            }
        }

        internal List<Buff> GetAllBuffs()
        {
            lock (_lock)
            {
                return Instance.Buffs;
            }
        }
    }
}
