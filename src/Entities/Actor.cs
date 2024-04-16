using Etrea2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etrea2.Entities
{
    [Serializable]
    internal abstract class Actor
    {
        [JsonProperty]
        internal string Name { get; set; }
        [JsonProperty]
        internal string ShortDescription { get; set; }
        [JsonProperty]
        internal string LongDescription { get; set; }
        [JsonProperty]
        internal string ArrivalMessage { get; set; }
        [JsonProperty]
        internal string DepartureMessage { get; set; }
        [JsonProperty]
        internal string Title { get; set; }
        [JsonProperty]
        internal uint Level { get; set; }
        [JsonProperty]
        internal Gender Gender { get; set; }
        [JsonProperty]
        internal ActorType ActorType { get; set; }
        [JsonProperty]
        internal ActorClass Class { get; set; }
        [JsonProperty]
        internal ActorRace Race { get; set; }
        [JsonProperty]
        internal ActorPosition Position { get; set; }
        [JsonProperty]
        internal Alignment Alignment { get; set; }
        [JsonProperty]
        internal int AlignmentScale { get; set; }
        [JsonProperty]
        internal uint CurrentRoom { get; set; }
        [JsonProperty]
        internal bool Visible { get; set; }
        [JsonProperty]
        internal Dictionary<string, int> Buffs { get; set; }
        [JsonProperty]
        internal List<InventoryItem> Inventory { get; set; }
        [JsonProperty]
        internal uint NumberOfAttacks { get; set; }
        [JsonProperty]
        internal int ResistFire { get; set; }
        [JsonProperty]
        internal int ResistIce { get; set; }
        [JsonProperty]
        internal int ResistLightning { get; set; }
        [JsonProperty]
        internal int ResistEarth { get; set; }
        [JsonProperty]
        internal int ResistHoly { get; set; }
        [JsonProperty]
        internal int ResistDark { get; set; }
        [JsonProperty]
        internal ulong Gold { get; set; }
        [JsonProperty]
        internal uint Strength { get; set; }
        [JsonProperty]
        internal uint Dexterity { get; set; }
        [JsonProperty]
        internal uint Constitution { get; set; }
        [JsonProperty]
        internal uint Intelligence { get; set; }
        [JsonProperty]
        internal uint Wisdom { get; set; }
        [JsonProperty]
        internal uint Charisma { get; set; }
        [JsonProperty]
        internal uint BaseArmourClass { get; set; }
        [JsonProperty]
        internal uint ArmourClass { get; set; }
        [JsonProperty]
        internal List<Spell> Spells { get; set; }
        [JsonProperty]
        internal List <Skill> Skills { get; set; }
        [JsonProperty]
        internal InventoryItem EquipHead { get; set; }
        [JsonProperty]
        internal InventoryItem EquipNeck { get; set; }
        [JsonProperty]
        internal InventoryItem EquipArmour { get; set; }
        [JsonProperty]
        internal InventoryItem EquipLeftFinger { get; set; }
        [JsonProperty]
        internal InventoryItem EquipRightFinger { get; set; }
        [JsonProperty]
        internal InventoryItem EquipWeapon { get; set; }
        [JsonProperty]
        internal InventoryItem EquipHeld { get; set; }
        [JsonProperty]
        internal int CurrentSP { get; set; }
        [JsonProperty]
        internal int MaxSP { get; set; }
        [JsonProperty]
        internal int MaxHP { get; set; }
        [JsonProperty]
        internal int MaxMP { get; set; }
        [JsonProperty]
        internal int CurrentHP { get; set; }
        [JsonProperty]
        internal int CurrentMP { get; set; }

        internal uint DoDamageRoll(Actor defender)
        {
            int damage = EquipWeapon != null ? (int)Helpers.RollDice(EquipWeapon.NumberOfDamageDice, EquipWeapon.SizeOfDamageDice) : (int)Helpers.RollDice(1, 2);
            if (EquipWeapon == null || !EquipWeapon.IsFinesse)
            {
                var abilityMod = Helpers.CalculateAbilityModifier(Strength);
                damage += abilityMod;
            }
            if (EquipWeapon != null && EquipWeapon.IsFinesse)
            {
                var abilityMod = Helpers.CalculateAbilityModifier(Dexterity);
                damage += abilityMod;
            }
            if (HasBuff("Desperate Attack"))
            {
                damage += 4;
            }
            if (HasBuff("Bless"))
            {
                damage += 1;
            }
            if (EquipWeapon != null && EquipWeapon.RequiredSkill != null)
            {
                if (EquipWeapon.RequiredSkill.Name.ToLower() == "Simple Weapons" && HasSkill("Simple Weapon Mastery"))
                {
                    damage += 2;
                }
                if (EquipWeapon.RequiredSkill.Name.ToLower() == "Martial Weapons" && HasSkill("Martial Weapon Mastery"))
                {
                    damage += 2;
                }
                if (EquipWeapon.RequiredSkill.Name.ToLower() == "Exotic Weapons" && HasSkill("Exotic Weapon Mastery"))
                {
                    damage += 2;
                }
            }
            if (defender.EquipArmour != null && defender.EquipArmour.DamageReductionModifier > 0)
            {
                damage -= defender.EquipArmour.DamageModifier;
            }
            if (defender.HasBuff("Barkskin"))
            {
                damage -= 2;
            }
            if (defender.HasBuff("Stoneskin"))
            {
                damage -= 3;
            }
            if (defender.HasBuff("Ironskin"))
            {
                damage -= 4;
            }
            damage = damage < 0 ? 0 : damage;
            return (uint)damage;
        }

        internal bool DoHitRoll(Actor target, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical)
        {
            baseHitRoll = Helpers.RollDice(1, 20);
            isCritical = baseHitRoll == 20;
            if (baseHitRoll > 1 && !isCritical)
            {
                int modHitRoll = (int)baseHitRoll;
                if (EquipWeapon != null && EquipWeapon.IsFinesse)
                {
                    modHitRoll += Helpers.CalculateAbilityModifier(Dexterity);
                }
                else
                {
                    modHitRoll += Helpers.CalculateAbilityModifier(Strength);
                }
                // TODO: Anything which directly affects an attacker's hit roll should be dealt with here
                //       Defender's AC is dealt with under the call to CalculateArmourClass() before we compare results
                if (HasBuff("Truestrike"))
                {
                    modHitRoll += 10;
                }
                if (HasBuff("Desperate Attack"))
                {
                    modHitRoll -= 4;
                }
                if (HasBuff("Bless"))
                {
                    modHitRoll += 1;
                }
                if (HasSkill("Awareness"))
                {
                    modHitRoll += 1;
                }
                if (EquipWeapon != null && EquipWeapon.RequiredSkill != null)
                {
                    if (EquipWeapon.RequiredSkill.Name == "Simple Weapons" && HasSkill("Simple Weapon Mastery"))
                    {
                        modHitRoll += 2;
                    }
                    if (EquipWeapon.RequiredSkill.Name == "Martial Weapons" && HasSkill("Martial Weapon Mastery"))
                    {
                        modHitRoll += 2;
                    }
                    if (EquipWeapon.RequiredSkill.Name == "Exotic Weapons" && HasSkill("Exotic Weapon Mastery"))
                    {
                        modHitRoll += 2;
                    }
                }
                target.CalculateArmourClass();
                finalHitRoll = modHitRoll < 1 ? 1 : (uint)modHitRoll;
                return finalHitRoll >= target.ArmourClass;
            }
            else
            {
                finalHitRoll = baseHitRoll;
                if (baseHitRoll == 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal void CalculateArmourClass()
        {
            int acBase = Convert.ToInt32(BaseArmourClass);
            int dexModifier = Helpers.CalculateAbilityModifier(Dexterity);
            int eqModifiers = 0, skillModifiers = 0;
            eqModifiers += EquipHead != null ? EquipHead.ArmourClassModifier : 0;
            eqModifiers += EquipNeck != null ? EquipNeck.ArmourClassModifier : 0;
            eqModifiers += EquipArmour != null ? EquipArmour.ArmourClassModifier : 0;
            eqModifiers += EquipHeld != null ? EquipHeld.ArmourClassModifier : 0;
            eqModifiers += EquipLeftFinger != null ? EquipLeftFinger.ArmourClassModifier : 0;
            eqModifiers += EquipRightFinger != null ? EquipRightFinger.ArmourClassModifier : 0;
            eqModifiers += EquipWeapon != null ? EquipWeapon.ArmourClassModifier : 0;
            if (EquipWeapon != null && EquipWeapon.IsFinesse && HasSkill("Parry"))
            {
                skillModifiers += 2;
            }
            if (HasSkill("Dodge") && (EquipArmour == null || (EquipArmour != null && EquipArmour.BaseArmourType == ArmourType.Light)))
            {
                skillModifiers += 2;
            }
            var finalAC = acBase + dexModifier + eqModifiers + skillModifiers;
            finalAC = finalAC < 0 ? 0 : finalAC;
            ArmourClass = Convert.ToUInt32(finalAC);
        }

        internal bool HasItemInInventory(uint itemID)
        {
            if (Inventory != null && Inventory.Count > 0)
            {
                return Inventory.Any(x => x.ID == itemID);
            }
            return false;
        }

        internal void AddSpell(string spellName)
        {
            var s = SpellManager.Instance.GetSpell(spellName);
            if (s != null && !Spells.Contains(s))
            {
                Spells.Add(s);
            }
        }

        internal void RemoveSpell(string spellName)
        {
            var s = SpellManager.Instance.GetSpell(spellName);
            if (s != null && Spells.Contains(s))
            {
                Spells.Remove(s);
            }
        }

        internal void AddSkill(string skillName)
        {
            var s = SkillManager.Instance.GetSkill(skillName);
            if (s != null && !Skills.Contains(s))
            {
                Skills.Add(s);
            }
        }

        internal void RemoveSkill(string skillName)
        {
            var s = SkillManager.Instance.GetSkill(skillName);
            if (s != null && Skills.Contains(s))
            {
                Skills.Remove(s);
            }
        }

        internal bool HasSpell(string spellName)
        {
            return Spells.Any(x => x != null && x.SpellName.ToLower() == spellName.ToLower());
        }

        internal bool HasSkill(string skillName)
        {
            return Skills.Any(x => x != null && x.Name.ToLower() == skillName.ToLower());
        }

        internal void AddBuff(string buffName, int bonusDuration, bool isPermanent)
        {
            // TODO: Check calls to this function and see if we can use Max to pass through the highest value for bonus duration
            var b = BuffManager.Instance.GetBuff(buffName);
            if (b != null)
            {
                CalculateArmourClass();
                if (Buffs.ContainsKey(buffName))
                {
                    if (isPermanent)
                    {
                        Buffs[buffName] = -1;
                    }
                    else
                    {
                        if (Buffs[buffName] > 0)
                        {
                            Buffs[buffName] += b.BuffDuration + bonusDuration;
                        }
                    }
                }
                else
                {
                    if (isPermanent)
                    {
                        Buffs.Add(buffName, -1);
                    }
                    else
                    {
                        Buffs.Add(buffName, b.BuffDuration + bonusDuration);
                    }
                }
                switch (buffName)
                {
                    // add cases here to add the effects of buffs which have been applied
                    case "Bulls Strength":
                        Strength += 4;
                        break;

                    case "Cats Grace":
                        Dexterity += 4;
                        break;

                    case "Bears Endurance":
                        Constitution += 4;
                        break;

                    case "Owls Wisdom":
                        Wisdom += 4;
                        break;

                    case "Eagles Splendour":
                        Charisma += 4;
                        break;

                    case "Foxs Cunning":
                        Intelligence += 4;
                        break;

                    case "Mage Armour":
                        ArmourClass += 5;
                        break;

                    case "Fae Fire":
                        ArmourClass -= 4;
                        break;

                    case "Minor Fire Resistance":
                        ResistFire += 15;
                        break;

                    case "Moderate Fire Resistance":
                        ResistFire += 30;
                        break;

                    case "Greater Fire Resistance":
                        ResistFire += 70;
                        break;

                    case "Minor Ice Resistance":
                        ResistIce += 15;
                        break;

                    case "Moderate Ice Resistance":
                        ResistIce += 30;
                        break;

                    case "Greater Ice Resistance":
                        ResistIce += 70;
                        break;

                    case "Minor Lightning Resistance":
                        ResistLightning += 15;
                        break;

                    case "Moderate Lightning Resistance":
                        ResistLightning += 30;
                        break;

                    case "Greater Lightning Resistance":
                        ResistLightning += 70;
                        break;

                    case "Minor Earth Resistance":
                        ResistEarth += 15;
                        break;

                    case "Moderate Earth Resistance":
                        ResistEarth += 30;
                        break;

                    case "Greater Earth Resistance":
                        ResistEarth += 70;
                        break;

                    case "Minor Dark Resistance":
                        ResistDark += 15;
                        break;

                    case "Moderate Dark Resistance":
                        ResistDark += 30;
                        break;

                    case "Greater Dark Resistance":
                        ResistDark += 70;
                        break;

                    case "Minor Holy Resistance":
                        ResistHoly += 15;
                        break;

                    case "Moderate Holy Resistance":
                        ResistHoly += 30;
                        break;

                    case "Greater Holy Resistance":
                        ResistHoly += 70;
                        break;

                    case "Minor Fire Weakness":
                        ResistFire -= 15;
                        break;

                    case "Minor Ice Weakness":
                        ResistIce -= 15;
                        break;

                    case "Minor Lightning Weakness":
                        ResistLightning -= 15;
                        break;

                    case "Minor Earth Weakness":
                        ResistEarth -= 15;
                        break;

                    case "Minor Holy Weakness":
                        ResistHoly -= 15;
                        break;

                    case "Minor Dark Weakness":
                        ResistDark -= 15;
                        break;

                    case "Moderate Fire Weakness":
                        ResistFire -= 30;
                        break;

                    case "Moderate Ice Weakness":
                        ResistIce -= 30;
                        break;

                    case "Moderate Lightning Weakness":
                        ResistLightning -= 30;
                        break;

                    case "Moderate Earth Weakness":
                        ResistEarth -= 30;
                        break;

                    case "Moderate Holy Weakness":
                        ResistHoly -= 30;
                        break;

                    case "Moderate Dark Weakness":
                        ResistDark -= 30;
                        break;

                    case "Greater Fire Weakness":
                        ResistFire -= 70;
                        break;

                    case "Greater Ice Weakness":
                        ResistIce -= 70;
                        break;

                    case "Greater Lightning Weakness":
                        ResistLightning -= 70;
                        break;

                    case "Greater Earth Weakness":
                        ResistEarth -= 70;
                        break;

                    case "Greater Holy Weakness":
                        ResistHoly -= 70;
                        break;

                    case "Greater Dark Weakness":
                        ResistDark -= 70;
                        break;

                    default:
                        break;
                }
                CalculateArmourClass();
            }
        }

        internal void RemoveBuff(string buffName)
        {
            if (Buffs.ContainsKey(buffName))
            {
                Buffs.Remove(buffName);
                switch(buffName)
                {
                    // add cases here to remove the effects of buffs which have expired
                    case "Bulls Strength":
                        Strength -= 4;
                        break;

                    case "Cats Grace":
                        Dexterity -= 4;
                        break;

                    case "Bears Endurance":
                        Constitution -= 4;
                        break;

                    case "Owls Wisdom":
                        Wisdom -= 4;
                        break;

                    case "Eagles Splendour":
                        Charisma -= 4;
                        break;

                    case "Foxs Cunning":
                        Intelligence -= 4;
                        break;

                    case "Mage Armour":
                        ArmourClass -= 5;
                        break;

                    case "Fae Fire":
                        ArmourClass += 4;
                        break;

                    case "Minor Fire Resistance":
                        ResistFire -= 15;
                        break;

                    case "Intermediate Fire Resistance":
                        ResistFire -= 30;
                        break;

                    case "Greater Fire Resistance":
                        ResistFire -= 70;
                        break;

                    case "Minor Ice Resistance":
                        ResistIce -= 15;
                        break;

                    case "Intermediate Ice Resistance":
                        ResistIce -= 30;
                        break;

                    case "Greater Ice Resistance":
                        ResistIce -= 70;
                        break;

                    case "Minor Lightning Resistance":
                        ResistLightning -= 15;
                        break;

                    case "Intermediate Lightning Resistance":
                        ResistLightning -= 30;
                        break;

                    case "Greater Lightning Resistance":
                        ResistLightning -= 70;
                        break;

                    case "Minor Earth Resistance":
                        ResistEarth -= 15;
                        break;

                    case "Intermediate Earth Resistance":
                        ResistEarth -= 30;
                        break;

                    case "Greater Earth Resistance":
                        ResistEarth -= 70;
                        break;

                    case "Minor Dark Resistance":
                        ResistDark -= 15;
                        break;

                    case "Intermediate Dark Resistance":
                        ResistDark -= 30;
                        break;

                    case "Greater Dark Resistance":
                        ResistDark -= 70;
                        break;

                    case "Minor Holy Resistance":
                        ResistHoly -= 15;
                        break;

                    case "Intermediate Holy Resistance":
                        ResistHoly -= 30;
                        break;

                    case "Greater Holy Resistance":
                        ResistHoly -= 70;
                        break;

                    default:
                        break;
                }
            }
        }

        internal void AdjustHP(int amount, out bool isKilled)
        {
            isKilled = false;
            CurrentHP += amount;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                isKilled = true;
            }
            if (CurrentHP > MaxHP)
            {
                CurrentHP = MaxHP;
            }
        }

        internal void AdjustSP(int amount)
        {
            CurrentSP += amount;
            if (CurrentSP < 0)
            {
                CurrentSP = 0;
            }
            if (CurrentSP > MaxSP)
            {
                CurrentSP = MaxSP;
            }
        }

        internal void AdjustMP(int amount)
        {
            CurrentMP += amount;
            if (CurrentMP < 0)
            {
                CurrentMP = 0;
            }
            if (CurrentMP > MaxMP)
            {
                CurrentMP = MaxMP;
            }
        }

        internal void SetStat(string stat, string value, ref Descriptor setter, out bool changeSuccess, out string statChanged, out string setValue)
        {
            // TODO: Use this function instead of the ones currently used in ActImmortal - we can also use these to set NPCs
            // needs to include options for setting resists
            changeSuccess = true;
            statChanged = stat;
            setValue = string.Empty;
            try
            {
                switch (stat.ToLower())
                {
                    case "strength":
                    case "str":
                        if (uint.TryParse(value, out uint uintValue))
                        {
                            Strength = uintValue;
                            statChanged = "Strength";
                            setValue = uintValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Strength to {uintValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Strength!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "dexterity":
                    case "dex":
                        if (uint.TryParse(value, out uintValue))
                        {
                            Dexterity = uintValue;
                            statChanged = "Dexterity";
                            setValue = uintValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Dexterity to {uintValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Dexterity!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "constitution":
                    case "con":
                        if (uint.TryParse(value, out uintValue))
                        {
                            Constitution = uintValue;
                            statChanged = "Constitution";
                            setValue = uintValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Constitution to {uintValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Constitution!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "intelligence":
                    case "int":
                        if (uint.TryParse(value, out uintValue))
                        {
                            Intelligence = uintValue;
                            statChanged = "Intelligence";
                            setValue = uintValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Intelligence to {uintValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Intelligence!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "wisdom":
                    case "wis":
                        if (uint.TryParse(value, out uintValue))
                        {
                            Wisdom = uintValue;
                            statChanged = "Wisdom";
                            setValue = uintValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Wisdom to {uintValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Wisdom!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "charisma":
                    case "cha":
                        if (uint.TryParse(value, out uintValue))
                        {
                            Charisma = uintValue;
                            statChanged = "Charisma";
                            setValue = uintValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Charisma to {uintValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Charisma!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "maxhp":
                        if (int.TryParse(value, out int intValue))
                        {
                            intValue = intValue < 1 ? 1 : intValue;
                            MaxHP = intValue;
                            if (CurrentHP > MaxHP)
                            {
                                CurrentHP = MaxHP;
                            }
                            statChanged = "Max HP";
                            setValue = intValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Max HP to {intValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Max HP!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "currenthp":
                        if (int.TryParse(value, out intValue))
                        {
                            intValue = intValue < 1 ? 1 : intValue;
                            if (intValue < MaxHP)
                            {
                                CurrentHP = intValue;
                                statChanged = "Current HP";
                                setValue = intValue.ToString();
                                setter.Send($"You have successfully changed {Name}'s Current HP to {intValue}!{Constants.NewLine}");
                            }
                            else
                            {
                                setter.Send($"You cannot set Current HP to be higher than Max HP!{Constants.NewLine}");
                                changeSuccess = false;
                            }
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Current HP!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "maxmp":
                        if (int.TryParse(value, out intValue))
                        {
                            intValue = intValue < 1 ? 1 : intValue;
                            MaxMP = intValue;
                            if (CurrentMP > MaxMP)
                            {
                                CurrentMP = MaxMP;
                            }
                            statChanged = "Max MP";
                            setValue = intValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Max MP to {intValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Max MP!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "currentmp":
                        if (int.TryParse(value, out intValue))
                        {
                            intValue = intValue < 1 ? 1 : intValue;
                            if (intValue < MaxMP)
                            {
                                CurrentMP = intValue;
                                statChanged = "Current MP";
                                setValue = intValue.ToString();
                                setter.Send($"You have successfully changed {Name}'s Current MP to {intValue}!{Constants.NewLine}");
                            }
                            else
                            {
                                setter.Send($"You cannot set Current MP to be higher than Max MP!{Constants.NewLine}");
                                changeSuccess = false;
                            }
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Current MP!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "maxsp":
                        if (int.TryParse(value, out intValue))
                        {
                            intValue = intValue < 1 ? 1 : intValue;
                            MaxSP = intValue;
                            if (CurrentSP > MaxSP)
                            {
                                CurrentSP = MaxSP;
                            }
                            statChanged = "Max SP";
                            setValue = intValue.ToString();
                            setter.Send($"You have successfully changed {Name}'s Max SP to {intValue}!{Constants.NewLine}");
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Max SP!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "currentsp":
                        if (int.TryParse(value, out intValue))
                        {
                            intValue = intValue < 1 ? 1 : intValue;
                            if (intValue < MaxSP)
                            {
                                CurrentSP = intValue;
                                statChanged = "Current SP";
                                setValue = intValue.ToString();
                                setter.Send($"You have successfully changed {Name}'s Current SP to {intValue}!{Constants.NewLine}");
                            }
                            else
                            {
                                setter.Send($"You cannot set Current SP to be higher than Max SP!{Constants.NewLine}");
                                changeSuccess = false;
                            }
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Current SP!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "baseac":
                    case "armourclass":
                    case "ac":
                        if (uint.TryParse(value, out uintValue))
                        {
                            uintValue = uintValue < 1 ? 1 : uintValue;
                            BaseArmourClass = uintValue;
                            CalculateArmourClass();
                            setter.Send($"You have successfully changed {Name}'s Base Armour Class to {uintValue}!{Constants.NewLine}");
                            statChanged = "Base Armour Class";
                            setValue = uintValue.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Armour Class!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "gold":
                    case "gp":
                        if (ulong.TryParse(value, out ulong ulongValue))
                        {
                            Gold = ulongValue;
                            setter.Send($"You have successfully changed {Name}'s Gold to {ulongValue}!{Constants.NewLine}");
                            statChanged = "Gold";
                            setValue = ulongValue.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Gold!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "alignment":
                    case "align":
                        if (Enum.TryParse<Alignment>(value, true, out Alignment newAlignment))
                        {
                            Alignment = newAlignment;
                            switch(Alignment)
                            {
                                case Alignment.Good:
                                    AlignmentScale = 50;
                                    break;

                                case Alignment.Neutral:
                                    AlignmentScale = 0;
                                    break;

                                case Alignment.Evil:
                                    AlignmentScale = -50;
                                    break;
                            }
                            statChanged = "Alignment";
                            setValue = newAlignment.ToString();
                        }
                        else
                        {
                            changeSuccess = false;
                            setter.Send($"That isn't a valid value for Alignment!{Constants.NewLine}");
                        }
                        break;

                    case "resistfire":
                    case "fireresist":
                        if (int.TryParse(value, out int result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistFire = result;
                            statChanged = "Fire Resistance";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Fire Resistance!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "resistice":
                    case "iceresist":
                        if (int.TryParse(value, out result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistIce = result;
                            statChanged = "Ice Resistance";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Ice Resistance!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "resistearth":
                    case "earthresist":
                        if (int.TryParse(value, out result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistEarth = result;
                            statChanged = "Earth Resistance";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Earth Resistance!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "lightningresist":
                    case "resistlightning":
                        if (int.TryParse(value, out result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistLightning = result;
                            statChanged = "Lightning Resistance";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Lightning Resistance!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "holyresist":
                    case "resistholy":
                        if (int.TryParse(value, out result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistHoly = result;
                            statChanged = "Holy Resistance";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Holy Resistance!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "darkresist":
                    case "resistdark":
                        if (int.TryParse(value, out result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistDark = result;
                            statChanged = "Dark Resistance";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Dark Resistance!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "resistall":
                        if (int.TryParse(value, out result))
                        {
                            if (result < -100)
                            {
                                result = -100;
                            }
                            if (result > 100)
                            {
                                result = 100;
                            }
                            ResistDark = result;
                            ResistEarth = result;
                            ResistFire = result;
                            ResistHoly = result;
                            ResistIce = result;
                            ResistLightning = result;
                            statChanged = "All Resistances";
                            setValue = result.ToString();
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for resistances!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    case "level":
                        if (uint.TryParse(value, out uintValue))
                        {
                            if (uintValue <= setter.Player.Level)
                            {
                                Level = uintValue;
                                statChanged = "Level";
                                setValue = uintValue.ToString();
                            }
                            else
                            {
                                setter.Send($"You cannot set someone to be a higher level than yourself!{Constants.NewLine}");
                                changeSuccess = false;
                            }
                        }
                        else
                        {
                            setter.Send($"That isn't a valid value for Level!{Constants.NewLine}");
                            changeSuccess = false;
                        }
                        break;

                    default:
                        setter.Send($"That doesn't look like something you can change...{Constants.NewLine}");
                        changeSuccess = false;
                        break;
                }
            }
            catch(Exception ex)
            {
                changeSuccess = false;
                Game.LogMessage($"ERROR: Player {setter.Player} encountered an error setting {stat} to {value} for {Name} (ActorType: {ActorType}: {ex.Message}", LogLevel.Error, true);
                setter.Send($"Your powers have failed you and the change was not made!{Constants.NewLine}");
            }
        }

        internal bool HasBuff(string buffName)
        {
            return Buffs.ContainsKey(buffName);
        }
    }

    [Serializable]
    internal class NPC : Actor
    {
        [JsonProperty]
        internal NPCFlags BehaviourFlags { get; set; }
        [JsonProperty]
        internal uint BaseExpAward { get; set; }
        [JsonProperty]
        internal uint BonusHitDice { get; set; }
        [JsonProperty]
        internal uint NumberOfHitDice => Level + BonusHitDice;
        [JsonProperty]
        internal uint HitDieSize { get; set; }
        [JsonProperty]
        internal uint NPCID { get; set; }
        [JsonProperty]
        internal uint MaxNumber { get; set; }
        [JsonProperty]
        internal uint AppearChance { get; set; }
        [JsonProperty]
        internal uint AppearsInZone { get; set; }
        internal Guid FollowingPlayer { get; set; }
        internal Guid NPCGuid { get; set; }
        internal bool IsFollower => FollowingPlayer != Guid.Empty;
        internal bool IsInCombat => CombatManager.Instance.IsNPCInCombat(this.NPCGuid);

        internal NPC()
        {
            Inventory = new List<InventoryItem>();
            Skills = new List<Skill>();
            Spells = new List<Spell>();
            Buffs = new Dictionary<string, int>();
            Name = "New NPC";
            DepartureMessage = $"{Name} wanders away{Constants.NewLine}";
            ArrivalMessage = $"{Name} arrives, looking mean!{Constants.NewLine}";
        }

        internal NPC ShallowCopy()
        {
            var npc = (NPC)this.MemberwiseClone();
            npc.NPCGuid = Guid.NewGuid();
            return npc;
        }

        internal bool FleeCombat(out uint destRID)
        {
            destRID = 0;
            try
            {
                var roomExits = RoomManager.Instance.GetRoom(CurrentRoom).RoomExits;
                if (roomExits.Count > 0)
                {
                    var rnd = new Random(DateTime.UtcNow.GetHashCode());
                    var exit = roomExits[rnd.Next(roomExits.Count)];
                    if (ZoneManager.Instance.IsRIDInZone(exit.DestinationRoomID, AppearsInZone))
                    {
                        destRID = exit.DestinationRoomID;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: NPC {Name} ({NPCID}) caused an error trying to flee combat: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool Move(ref NPC n, uint fromRID, uint toRID, bool wasTeleported)
        {
            try
            {
                var targetRoom = RoomManager.Instance.GetRoom(toRID);
                if (targetRoom == null)
                {
                    return false;
                }
                RoomManager.Instance.UpdateNPCsInRoom(fromRID, true, false, ref n);
                RoomManager.Instance.UpdateNPCsInRoom(toRID, false, false, ref n);
                NPCManager.Instance.MoveNPCToNewRID(n.NPCGuid, toRID);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Failed to move NPC {Name} ({NPCID}) from RID {fromRID} to {toRID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal void Kill()
        {
            if (Inventory != null && Inventory.Count > 0)
            {
                foreach (var i in Inventory)
                {
                    var item = i;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
            }
            if (EquipHead != null && !EquipHead.IsMonsterItem)
            {
                var item = EquipHead;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipHeld != null && !EquipHeld.IsMonsterItem)
            {
                var item = EquipHeld;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipWeapon != null && !EquipWeapon.IsMonsterItem)
            {
                var item = EquipWeapon;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipLeftFinger != null && !EquipLeftFinger.IsMonsterItem)
            {
                var item = EquipLeftFinger;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipRightFinger != null && !EquipRightFinger.IsMonsterItem)
            {
                var item = EquipRightFinger;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipArmour != null && !EquipArmour.IsMonsterItem)
            {
                var item = EquipArmour;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipNeck != null && !EquipNeck.IsMonsterItem)
            {
                var item = EquipNeck;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (IsFollower)
            {
                SessionManager.Instance.GetPlayerByGUID(FollowingPlayer).Player.FollowerID = Guid.Empty;
                SessionManager.Instance.GetPlayerByGUID(FollowingPlayer).Send($"Alas, your follower {Name} has been slain!{Constants.NewLine}");
            }
            NPCManager.Instance.RemoveNPCFromWorld(NPCGuid);
        }

        internal void Kill(bool killedInCombat, ref Descriptor desc)
        {
            bool dropsItems = false;
            if (Inventory != null && Inventory.Count > 0)
            {
                dropsItems = true;
                foreach(var i in Inventory)
                {
                    var item = i;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
            }
            if (EquipHead != null && !EquipHead.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipHead;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipHeld != null && !EquipHeld.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipHeld;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipWeapon != null && !EquipWeapon.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipWeapon;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipLeftFinger != null && !EquipLeftFinger.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipLeftFinger;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipRightFinger != null && !EquipRightFinger.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipRightFinger;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipArmour != null && !EquipArmour.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipArmour;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (EquipNeck != null && !EquipNeck.IsMonsterItem)
            {
                dropsItems = true;
                var item = EquipNeck;
                RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
            }
            if (IsFollower)
            {
                SessionManager.Instance.GetPlayerByGUID(FollowingPlayer).Player.FollowerID = Guid.Empty;
                SessionManager.Instance.GetPlayerByGUID(FollowingPlayer).Send($"Alas, your follower {Name} has been slain!{Constants.NewLine}");
            }
            if (killedInCombat && dropsItems)
            {
                if (dropsItems)
                {
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(CurrentRoom);
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        var article = Helpers.IsCharAVowel(Name[0]) ? "An" : "A";
                        foreach (var lp in localPlayers)
                        {
                            lp.Send($"{article} {Name} drops some items to the floor as their corpse is swallowed by the Winds of Magic!{Constants.NewLine}");
                        }
                    }
                }
                if (desc.Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(NPCID)))
                {
                    for (int n = 0; n < desc.Player.ActiveQuests.Count; n++)
                    {
                        if (desc.Player.ActiveQuests[n].Monsters.Keys.Contains(NPCID))
                        {
                            if (desc.Player.ActiveQuests[n].Monsters[NPCID] <= 1)
                            {
                                desc.Player.ActiveQuests[n].Monsters[NPCID] = 0;
                            }
                            else
                            {
                                desc.Player.ActiveQuests[n].Monsters[NPCID]--;
                            }
                        }
                    }
                }
            }
            desc.Player.UpdateAlignment(Alignment);
            NPCManager.Instance.RemoveNPCFromWorld(NPCGuid);
        }
    }

    [Serializable]
    internal class Player : Actor
    {
        [JsonProperty]
        internal bool ShowDetailedRollInfo { get; set; }
        [JsonProperty]
        internal List<Recipe> Recipes { get; set; }
        [JsonProperty]
        internal HashSet<Guid> CompletedQuests { get; set; }
        [JsonProperty]
        internal List<Quest> ActiveQuests { get; set; }
        internal Guid FollowerID { get; set; }
        [JsonProperty]
        internal ulong BankBalance { get; set; }
        [JsonProperty]
        internal Dictionary<string,string> CommandAliases { get; set; }
        [JsonProperty]
        internal Languages KnownLanguages { get; set; }
        [JsonProperty]
        internal Languages SpokenLanguage { get; set; }
        [JsonProperty]
        internal uint Exp { get; set; }
        [JsonProperty]
        internal List<InventoryItem> VaultStore { get; set; }
        internal bool PVP { get; set; }
        internal bool IsInCombat => CombatManager.Instance.IsPlayerInCombat(SessionManager.Instance.GetPlayer(Name).ID);

        internal Player()
        {
            ActiveQuests = new List<Quest>();
            Inventory = new List<InventoryItem>();
            Spells = new List<Spell>();
            Recipes = new List<Recipe>();
            Skills = new List<Skill>();
            CompletedQuests = new HashSet<Guid>();
            VaultStore = new List<InventoryItem>();
        }

        internal bool Move(uint fromRID, uint toRID, bool wasTeleported, bool bypassStaminaCheck = false)
        {
            try
            {
                var targetRoom = RoomManager.Instance.GetRoom(toRID);
                if (targetRoom == null)
                {
                    SessionManager.Instance.GetPlayer(Name).Send($"Some mysterious force pushes you back... You cannot go that way!{Constants.NewLine}");
                    return true;
                }
                if (!bypassStaminaCheck)
                {
                    int stamCost = 1;
                    if (RoomManager.Instance.GetRoom(fromRID).Flags.HasFlag(RoomFlags.HardTerrain))
                    {
                        stamCost += (int)Helpers.RollDice(1, 4);
                    }
                    if (RoomManager.Instance.GetRoom(toRID).Flags.HasFlag(RoomFlags.HardTerrain))
                    {
                        stamCost += (int)Helpers.RollDice(1, 4);
                    }
                    if (CurrentSP < stamCost)
                    {
                        SessionManager.Instance.GetPlayer(Name).Send($"You don't have the energy to move that fart just now...{Constants.NewLine}");
                        return true;
                    }
                    CurrentSP -= stamCost;
                }
                var desc = SessionManager.Instance.GetPlayer(Name);
                RoomManager.Instance.UpdatePlayersInRoom(fromRID, ref desc, true, wasTeleported, false, false);
                RoomManager.Instance.UpdatePlayersInRoom(toRID, ref desc, false, wasTeleported, false, false);
                desc.Player.CurrentRoom = toRID;
                if(this.FollowerID != Guid.Empty)
                {
                    var n = NPCManager.Instance.GetNPCByGUID(this.FollowerID);
                    if (n != null)
                    {
                        n.Move(ref n, fromRID, toRID, false);
                    }
                    else
                    {
                        this.FollowerID = Guid.Empty;
                    }
                }
                CurrentRoom = toRID;
                RoomManager.Instance.ProcessEnvironmentBuffs(fromRID);
                RoomManager.Instance.ProcessEnvironmentBuffs(toRID);
                RoomManager.Instance.GetRoom(CurrentRoom).DescribeRoom(ref desc, true);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error moving player {Name} from Room {fromRID} to Room {toRID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal void UpdateAlignment(Alignment npcAlignment)
        {
            switch(npcAlignment)
            {
                case Alignment.Evil:
                    AlignmentScale++;
                    break;

                case Alignment.Good:
                    AlignmentScale--;
                    break;
            }
            if (AlignmentScale <= -50)
            {
                Alignment = Alignment.Evil;
                return;
            }
            if (AlignmentScale > -5 && AlignmentScale < 50)
            {
                Alignment = Alignment.Neutral;
                return;
            }
            if (AlignmentScale >= 50)
            {
                Alignment = Alignment.Good;
            }
        }

        internal bool KnowsRecipe(string recipeName)
        {
            return Recipes.Where(x => x.RecipeName.ToLower() == recipeName.ToLower()).Any();
        }

        internal void Kill()
        {
            CurrentHP = 0;
            if (Buffs != null && Buffs.Count > 0)
            {
                var buffNames = Buffs.Where(x => x.Value != -1).Select(y => y.Key).ToList();
                foreach(var buffName in buffNames)
                {
                    RemoveBuff(buffName);
                }
            }
            Position = ActorPosition.Dead;
            uint xpLost = Exp > 3 ? Convert.ToUInt32(Exp * 0.1) : 0;
            Exp -= xpLost;
            ulong gp = Gold;
            RoomManager.Instance.AddGoldToRoom(CurrentRoom, gp);
            Gold = 0;
            RoomManager.Instance.GetRoom(CurrentRoom).ItemsInRoom.AddRange(Inventory);
            Inventory.Clear();
            Move(CurrentRoom, Constants.LimboRID(), true, true);
        }

        internal void AddGold(ulong gp, bool bypassSkillCheck)
        {
            ulong totalGP = gp;
            if (!bypassSkillCheck)
            {
                if(HasSkill("Gold Digger"))
                {
                    uint bonusGP = Convert.ToUInt32(gp * 0.5);
                    SessionManager.Instance.GetPlayer(Name).Send($"Your skills allow you to find an extra {bonusGP:N0} gold!{Constants.NewLine}");
                    totalGP += bonusGP;
                }
            }
            Gold += totalGP;
        }

        internal void AddExp(uint xp, bool bypassSkillCheck, bool bypassRaceCheck)
        {
            uint totalXP = xp;
            if (!bypassSkillCheck)
            {
                if (HasSkill("Quick Learner"))
                {
                    uint skillBonusXP = Convert.ToUInt32(xp * 0.25);
                    SessionManager.Instance.GetPlayer(Name).Send($"Your skills grant you a bonus of {skillBonusXP} Exp!{Constants.NewLine}");
                    totalXP += skillBonusXP;
                }
            }
            if (!bypassRaceCheck)
            {
                if (Race == ActorRace.Human)
                {
                    uint raceBonusXP = Convert.ToUInt32(xp * 0.25);
                    SessionManager.Instance.GetPlayer(Name).Send($"Your Human nature grants you a bonus of {raceBonusXP} Exp!{Constants.NewLine}");
                    totalXP += raceBonusXP;
                }
            }
            Exp += totalXP;
            if (LevelTable.HasCharacterAchievedNewLevel(Exp, Level, out uint newLevel))
            {
                LevelUp(newLevel - Level);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void LevelUp(uint levelsToAdvance)
        {
            for (uint i = 0; i < levelsToAdvance; i++)
            {
                Level++;
                uint hpIncrease = 0;
                uint mpIncrease = 0;
                int hpModifier = 0;
                int mpModifier = 0;
                switch(Class)
                {
                    case ActorClass.Cleric:
                        hpIncrease = Helpers.RollDice(1, 8);
                        mpIncrease = Helpers.RollDice(1, 8);
                        mpModifier = (int)mpIncrease + Helpers.CalculateAbilityModifier(Wisdom);
                        if (Level % 4 == 0)
                        {
                            Wisdom++;
                            Constitution++;
                            SessionManager.Instance.GetPlayer(Name).Send($"Your Wisdom and Constitution have improved!{Constants.NewLine}");
                        }
                        break;

                    case ActorClass.Fighter:
                        hpIncrease = Helpers.RollDice(1, 10);
                        mpIncrease = Helpers.RollDice(1, 4);
                        mpModifier = (int)mpIncrease + Helpers.CalculateAbilityModifier(Intelligence);
                        if (Level % 4 == 0)
                        {
                            Strength++;
                            Constitution++;
                            SessionManager.Instance.GetPlayer(Name).Send($"Your Strength and Constitution have improved!{Constants.NewLine}");
                        }
                        break;

                    case ActorClass.Thief:
                        hpIncrease = Helpers.RollDice(1, 6);
                        mpIncrease = Helpers.RollDice(1, 6);
                        mpModifier = (int)mpIncrease + Helpers.CalculateAbilityModifier(Intelligence);
                        if (Level % 4 == 0)
                        {
                            Dexterity++;
                            Intelligence++;
                            SessionManager.Instance.GetPlayer(Name).Send($"Your Dexterity and Intelligence have increased!{Constants.NewLine}");
                        }
                        break;

                    case ActorClass.Wizard:
                        hpIncrease = Helpers.RollDice(1, 4);
                        mpIncrease = Helpers.RollDice(1, 10);
                        mpModifier = Helpers.CalculateAbilityModifier(Intelligence);
                        if (Level % 4 == 0)
                        {
                            Intelligence++;
                            Wisdom++;
                            SessionManager.Instance.GetPlayer(Name).Send($"Your Intelligence and Wisdom have increased!{Constants.NewLine}");
                        }
                        break;
                }
                hpModifier = (int)hpIncrease + Helpers.CalculateAbilityModifier(Constitution);
                hpModifier = hpModifier < 1 ? 1 : hpModifier;
                mpModifier = mpModifier < 1 ? 1 : mpModifier;
                MaxHP += hpModifier;
                MaxMP += mpModifier;
                var stamIncrease = Helpers.RollDice(1, 10) + Helpers.CalculateAbilityModifier(Constitution);
                stamIncrease = stamIncrease < 1 ? 1 : stamIncrease;
                MaxSP += (int)stamIncrease;
                SessionManager.Instance.GetPlayer(Name).Send($"You gain {hpIncrease} HP, {mpIncrease} MP and {stamIncrease} SP");
            }
        }
    }
}
