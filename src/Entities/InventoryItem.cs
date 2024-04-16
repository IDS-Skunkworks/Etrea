using Newtonsoft.Json;
using Etrea2.Core;
using System.Collections.Generic;
using System;

namespace Etrea2.Entities
{
    [Serializable]
    internal class InventoryItem
    {
        [JsonProperty]
        internal uint ID { get; set; }
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
        internal bool IsFinesseWeapon { get; set; }
        [JsonProperty]
        internal bool IsMonsterItem { get; set; }
        internal bool IsFinesse
        {
            get
            {
                return BaseWeaponType == WeaponType.Bow || BaseWeaponType == WeaponType.Crossbow || IsFinesseWeapon;
            }
        }
        [JsonProperty]
        internal WeaponType BaseWeaponType { get; set; }
        [JsonProperty]
        internal ArmourType BaseArmourType { get; set; }
        [JsonProperty]
        internal Skill RequiredSkill { get; set; }
        [JsonProperty]
        internal int HitModifier { get; set; }
        [JsonProperty]
        internal int DamageModifier { get; set; }
        [JsonProperty]
        internal int ArmourClassModifier { get; set; }
        [JsonProperty]
        internal uint DamageReductionModifier { get; set; }
        [JsonProperty]
        internal ConsumableEffect ConsumableEffect { get; set; }
        [JsonProperty]
        internal bool IsToxic
        {
            get
            {
                return ConsumableEffect.HasFlag(ConsumableEffect.Poison);
            }
        }
        [JsonProperty]
        internal List<string> AppliedBuffs { get; set; }
        [JsonProperty]
        internal string CastsSpell { get; set; }
        [JsonProperty]
        internal ItemType ItemType { get; set; }
        [JsonProperty]
        internal bool IsCursed { get; set; }
        internal bool AppliesBuff
        {
            get
            {
                return AppliedBuffs != null && AppliedBuffs.Count > 0;
            }
        }

        internal InventoryItem ShallowCopy()
        {
            var i = (InventoryItem)this.MemberwiseClone();
            return i;
        }

        internal bool CanPlayerEquip(ref Descriptor desc, WearSlot slot, out string msg)
        {
            msg = string.Empty;
            if (!this.Slot.HasFlag(slot))
            {
                msg = $"{Name} can't be equipped in {slot}!{Constants.NewLine}";
                return false;
            }
            if (IsTwoHanded)
            {
                if (desc.Player.EquipHeld != null && !desc.Player.HasSkill("Monkey Grip"))
                {
                    msg = $"You can't use a two-handed item while holding something!{Constants.NewLine}";
                    return false;
                }
            }
            if (RequiredSkill != null)
            {
                if (!desc.Player.HasSkill(RequiredSkill.Name))
                {
                    msg = $"You lack the skill to use this item!{Constants.NewLine}";
                    return false;
                }
            }
            if (this.Slot.HasFlag(slot))
            {
                switch (Slot)
                {
                    case WearSlot.Head:
                        if (desc.Player.EquipHead != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Neck:
                        if (desc.Player.EquipNeck != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Weapon:
                        if (desc.Player.EquipWeapon != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Armour:
                        if (desc.Player.EquipArmour != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.FingerLeft:
                        if (desc.Player.EquipLeftFinger != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.FingerRight:
                        if (desc.Player.EquipRightFinger != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;

                    case WearSlot.Held:
                        if (desc.Player.EquipHeld != null)
                        {
                            msg = $"You already have something equipped there!{Constants.NewLine}";
                            return false;
                        }
                        break;
                }
            }
            return true;
        }
    }
}
