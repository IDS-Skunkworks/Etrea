using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Kingdoms_of_Etrea.Core;

namespace Kingdoms_of_Etrea.Entities
{
    [Serializable]
    internal class InventoryItem
    {
        [JsonProperty]
        internal uint Id { get; set; }
        [JsonProperty]
        internal string Name { get; set; }
        [JsonProperty]
        internal string ShortDescription { get; set; }
        [JsonProperty]
        internal string LongDescription { get; set; }
        [JsonProperty]
        internal uint BaseValue { get; set; }
        [JsonProperty]
        internal WearSlot Slot { get; set; }
        [JsonProperty]
        internal uint NumberOfDamageDice { get; set; }
        [JsonProperty]
        internal uint SizeOfDamageDice { get; set; }
        [JsonProperty]
        internal bool IsMagical { get; set; }
        [JsonProperty]
        internal bool IsTwoHanded { get; set; }
        [JsonProperty]
        internal bool IsMonsterItem { get; set; }
        internal bool IsRanged
        {
            get
            {
                return BaseWeaponType == WeaponType.Bow || BaseWeaponType == WeaponType.Crossbow;
            }
        }
        [JsonProperty]
        internal WeaponType BaseWeaponType { get; set; }
        [JsonProperty]
        internal ArmourType BaseArmourType { get; set; }
        [JsonProperty]
        internal Skills.Skill RequiredSkill { get; set; }
        [JsonProperty]
        internal int HitModifier { get; set; }
        [JsonProperty]
        internal int DamageModifier { get; set; }
        [JsonProperty]
        internal int ArmourClassModifier { get; set; }
        [JsonProperty]
        internal PotionEffect PotionEffect { get; set; }
        [JsonProperty]
        internal bool IsToxic
        {
            get
            {
                return PotionEffect == PotionEffect.Poison;
            }
        }
        [JsonProperty]
        internal List<string> AppliedBuffs { get; set; }
        [JsonProperty]
        internal string CastsSpell { get; set; }
        [JsonProperty]
        internal ItemType ItemType { get; set; }
        internal bool AppliesBuff
        {
            get
            { 
                return AppliedBuffs != null && AppliedBuffs.Count > 0;
            }
        }

        internal bool CanPlayerEquip(ref Descriptor desc, WearSlot slot, out string msg)
        {
            msg = string.Empty;
            if(!this.Slot.HasFlag(slot))
            {
                msg = $"{Name} can't be equipped in {slot}!{Constants.NewLine}";
                return false;
            }
            if(IsTwoHanded)
            {
                if(desc.Player.EquippedItems.Held != null && !desc.Player.HasSkill("Monkey Grip"))
                {
                    msg = $"You can't use a two-handed item while holding something!{Constants.NewLine}";
                    return false;
                }
            }
            if(RequiredSkill != null)
            {
                if(!desc.Player.HasSkill(RequiredSkill.Name))
                {
                    msg = $"You lack the skill to use this item!{Constants.NewLine}";
                    return false;
                }
            }
            if(this.Slot.HasFlag(slot))
            {
                switch(Slot)
                {
                    case WearSlot.Head:
                        if(desc.Player.EquippedItems.Head != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Neck:
                        if (desc.Player.EquippedItems.Neck != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Weapon:
                        if (desc.Player.EquippedItems.Weapon != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Armour:
                        if (desc.Player.EquippedItems.Armour != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.FingerLeft:
                        if (desc.Player.EquippedItems.FingerLeft != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.FingerRight:
                        if (desc.Player.EquippedItems.FingerRight != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Held:
                        if (desc.Player.EquippedItems.Held != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $"{Name}, {ShortDescription}";
        }
    }
}
