using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class BuffManager
    {
        private static BuffManager instance = null;
        private ConcurrentBag<Buff> Buffs;
        public int Count => Buffs.Count;

        private BuffManager()
        {
            Buffs = new ConcurrentBag<Buff>
            {
                new Buff
                {
                    Name = "Truesight",
                    Description = "Allows you to see things as they really are.",
                    Duration = 10
                },
                new Buff
                {
                    Name = "Darkvision",
                    Description = "Allows you to see normally, even in the blackest of night.",
                    Duration = 10
                },
                new Buff
                {
                    Name = "Bark Skin",
                    Description = "Your skill becomes as the toughest bark, only the hardest hits can harm you!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Mage Armour",
                    Description = "The Winds of Magic protect you from all but the most determined strikes!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Bulls Strength",
                    Description = "The Winds of Magic grant you the strength of a rampaging bull!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Cats Grace",
                    Description = "The Winds of Magic grant you the dexterity of the stealthiest feline!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Eagles Splendour",
                    Description = "The Winds of Magic grant you the splendour of the most majestic eagle!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Owls Wisdom",
                    Description = "The Winds of Magic grant you the wisdom of the wisest owl!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Bears Endurance",
                    Description = "The Winds of Magic grant you the constitution of the most steadfast ursa!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Foxs Cunning",
                    Description = "The Winds of Magic grant you the intelligence of the most wiley fox!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Desperate Attack",
                    Description = "You sacrifice finesse for pure striking power!",
                    Duration = 4
                },
                new Buff
                {
                    Name = "Defensive Stance",
                    Description = "Adopt a defensive posture, relying on stamina to hold an attacker at bay!",
                    Duration = 4
                },
                new Buff
                {
                    Name = "Shield",
                    Description = "The Winds of Magic protect you from all but the most determined strikes!",
                    Duration = 4
                },
                new Buff
                {
                    Name = "Stunned",
                    Description = "Staggered, you are unable to take any action!",
                    Duration = 2
                },
                new Buff
                {
                    Name = "Esuna",
                    Description = "The Winds of Magic purge you of all harmful effects!",
                    Duration = 1
                },
                new Buff
                {
                    Name = "Antidote",
                    Description = "The Winds of Magic cure you of all poisons and toxins!",
                    Duration = 1
                },
                new Buff
                {
                    Name = "Restoration",
                    Description = "The Winds of Magic cure you of all harmful effects and fully restore your health and spirit!",
                    Duration = 1
                },
                new Buff
                {
                    Name = "Land Walker",
                    Description = "The Winds of Magic make traversing the world less difficult.",
                    Duration = 10
                },
                new Buff
                {
                    Name = "Energy Drain",
                    Description = "The Winds of Magic sap your stamina.",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Spirit Drain",
                    Description = "The Winds of Magic sap your spiritual energy.",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Spirit Fire",
                    Description = "The Winds of Magic restore your spiritual energy.",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Energy Fire",
                    Description = "The Winds of Magic restore your stamina.",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Poison",
                    Description = "Toxins course through your blood!",
                    Duration = 5
                },
                new Buff
                {
                    Name = "Iron Skin",
                    Description = "The Winds of Magic make your skin as iron, turning aside all but the strongest attack!",
                    Duration = 5
                }
            };
        }

        public static BuffManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BuffManager();
                }
                return instance;
            }
        }

        public bool BuffExists(string name)
        {
            return Instance.Buffs.Any(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public Buff GetBuff(string name)
        {
            return Instance.Buffs.FirstOrDefault(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public List<Buff> GetBuff()
        {
            return Instance.Buffs.ToList();
        }
    }
}
