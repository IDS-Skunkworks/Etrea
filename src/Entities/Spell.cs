using Etrea2.Core;
using Newtonsoft.Json;
using System;

namespace Etrea2.Entities
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
        internal SpellElement SpellElement { get; set; }
        [JsonProperty]
        internal uint MPCost { get; set; }
        [JsonProperty]
        internal uint NumOfDamageDice { get; set; }
        [JsonProperty]
        internal uint SizeOfDamageDice { get; set; }
        [JsonProperty]
        internal uint AdditionalDamage { get; set; }
        [JsonProperty]
        internal bool AutoHitTarget { get; set; }
        [JsonProperty]
        internal uint GoldToLearn { get; set; }
        [JsonProperty]
        internal bool AOESpell { get; set; }
        [JsonProperty]
        internal bool BypassResistCheck { get; set; }
        [JsonProperty]
        internal bool ApplyAbilityModifier { get; set; }

        internal Spell ShallowCopy()
        {
            var s = (Spell)this.MemberwiseClone();
            return s;
        }

        internal void ApplyBuffSpell(Actor caster, Actor target, out bool hitsTarget, out int hpModifier)
        {
            hitsTarget = AutoHitTarget;
            hpModifier = 0;
            int abilityModifier = 0;
            if (SpellType == SpellType.Buff || SpellType == SpellType.Debuff)
            {
                if (caster.Class == ActorClass.Undefined || caster.Class == ActorClass.Cleric)
                {
                    // use Wisdom
                    abilityModifier = Helpers.CalculateAbilityModifier(caster.Wisdom);
                }
                else
                {
                    // use intelligence
                    abilityModifier = Helpers.CalculateAbilityModifier(caster.Intelligence);
                }
                if (NumOfDamageDice > 0 && SizeOfDamageDice > 0)
                {
                    // spell heals or causes damage in addition applying a buff/debuff so calculate and return
                    int spellResult = (int)(Helpers.RollDice(NumOfDamageDice, SizeOfDamageDice) + AdditionalDamage);
                    if (ApplyAbilityModifier)
                    {
                        spellResult += abilityModifier;
                    }
                    hpModifier = spellResult;
                }
                if (!AutoHitTarget)
                {
                    int hitRoll = (int)(Helpers.RollDice(1,20) + Helpers.CalculateAbilityModifier(caster.Dexterity));
                    target.CalculateArmourClass();
                    hitsTarget = hitRoll >= target.ArmourClass;
                }
                if (hitsTarget)
                {
                    var buffEffect = BuffManager.Instance.GetBuff(SpellName);
                    int bonusDuration = abilityModifier < 0 ? 0 : abilityModifier;
                    target.AddBuff(buffEffect.BuffName, bonusDuration, false);
                }
            }
            else
            {
                hitsTarget = false;
            }
        }

        internal int CalculateSpellHPEffect(Actor caster, Actor target, out bool hitsTarget)
        {
            hitsTarget = AutoHitTarget;
            var baseEffect = (int)(Helpers.RollDice(NumOfDamageDice, SizeOfDamageDice) + AdditionalDamage);
            int finalEffect = baseEffect;
            if (ApplyAbilityModifier)
            {
                if (SpellType == SpellType.Healing)
                {
                    var abilityMod = Helpers.CalculateAbilityModifier(caster.Wisdom);
                    finalEffect += abilityMod;
                }
                else
                {
                    var abilitMod = Helpers.CalculateAbilityModifier(caster.Intelligence);
                    finalEffect += abilitMod;
                }
            }
            if (BypassResistCheck || SpellElement == SpellElement.None)
            {
                finalEffect = finalEffect < 0 ? 0 : finalEffect;
                if (!AutoHitTarget)
                {
                    int hitRoll = (int)Helpers.RollDice(1, 20);
                    int abilityMod = Helpers.CalculateAbilityModifier(caster.Dexterity);
                    int hitRollFinal = hitRoll + abilityMod;
                    hitsTarget = hitRollFinal >= target.ArmourClass;
                }
                return finalEffect;
            }
            int resistance = 0;
            switch (SpellElement)
            {
                case SpellElement.Fire:
                    resistance = target.ResistFire;
                    break;

                case SpellElement.Ice:
                    resistance = target.ResistIce;
                    break;

                case SpellElement.Lightning:
                    resistance = target.ResistLightning;
                    break;

                case SpellElement.Earth:
                    resistance = target.ResistEarth;
                    break;

                case SpellElement.Dark:
                    resistance = target.ResistDark;
                    break;

                case SpellElement.Holy:
                    resistance = target.ResistHoly;
                    break;
            }
            if (resistance == 0)
            {
                finalEffect = finalEffect < 0 ? 0 : finalEffect;
                return finalEffect;
            }
            var resPerc = (double)resistance / 100;
            var effectMod = (int)Math.Round((finalEffect * resPerc) * -1, 0);
            finalEffect += effectMod;
            if (!AutoHitTarget)
            {
                int hitRoll = (int)Helpers.RollDice(1, 20);
                int abilityMod = Helpers.CalculateAbilityModifier(caster.Dexterity);
                int hitRollFinal = hitRoll + abilityMod;
                hitsTarget = hitRollFinal >= target.ArmourClass;
            }
            return finalEffect;
        }

        public override string ToString()
        {
            return SpellName;
        }
    }
}
