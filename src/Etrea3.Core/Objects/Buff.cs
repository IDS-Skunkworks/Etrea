using Etrea3.Core;
using Newtonsoft.Json;
using System;

namespace Etrea3.Objects
{
    [Serializable]
    public class Buff
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public int Duration { get; set; }

        public int CalculateDuration(Actor caster, Spell spell)
        {
            if (spell == null || caster == null)
            {
                return Duration;
            }
            if (!spell.ApplyAbilityModifier)
            {
                return Duration;
            }
            int bonusDuration = 0;
            if (caster.ActorType == ActorType.Player)
            {
                if (((Player)caster).Class == ActorClass.Wizard)
                {
                    bonusDuration = Helpers.CalculateAbilityModifier(caster.Intelligence);
                }
                else
                {
                    bonusDuration = Helpers.CalculateAbilityModifier(caster.Wisdom);
                }
            }
            else
            {
                bonusDuration = Math.Max(Helpers.CalculateAbilityModifier(caster.Intelligence), Helpers.CalculateAbilityModifier(caster.Wisdom));
            }
            return Math.Max(1, Duration + bonusDuration);
        }
    }
}
