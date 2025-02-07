using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Etrea3.Objects
{
    [Serializable]
    public class InventoryItem
    {
        [JsonProperty]
        public Guid ItemID { get; set; } = Guid.NewGuid();
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string ShortDescription { get; set; }
        [JsonProperty]
        public string LongDescription { get; set; }
        [JsonProperty]
        public int BaseValue { get; set; }
        [JsonProperty]
        public ItemType ItemType { get; set; }
        [JsonProperty]
        public bool IsMagical { get; set; } = false;
        [JsonProperty]
        public bool IsCursed { get; set; } = false;
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
    }

    [Serializable]
    public class Weapon : InventoryItem
    {
        [JsonProperty]
        public int NumberOfDamageDice { get; set; } = 1;
        [JsonProperty]
        public int SizeOfDamageDice { get; set; } = 6;
        [JsonProperty]
        public bool MonsterOnly { get; set; } = false;
        [JsonProperty]
        public bool IsTwoHanded { get; set; } = false;
        [JsonProperty]
        public int DamageModifier { get; set; }
        [JsonProperty]
        public int HitModifier { get; set; }
        [JsonProperty]
        public WeaponType WeaponType { get; set; }
        [JsonProperty]
        public HashSet<string> RequiredSkills { get; set; } = new HashSet<string>();
        [JsonProperty]
        public HashSet<string> AppliedBuffs { get; set; } = new HashSet<string>();
        [JsonIgnore]
        public bool AppliesBuffs => AppliedBuffs != null && AppliedBuffs.Count > 0;

        public Weapon()
        {
            ItemType = ItemType.Weapon;
            WeaponType = WeaponType.Undefined;
        }
    }

    [Serializable]
    public class Armour : InventoryItem
    {
        [JsonProperty]
        public HashSet<string> RequiredSkills { get; set; } = new HashSet<string>();
        [JsonProperty]
        public HashSet<string> AppliedBuffs { get; set; } = new HashSet<string>();
        [JsonIgnore]
        public bool AppliesBuffs => AppliedBuffs != null && AppliedBuffs.Count > 0;
        [JsonProperty]
        public int ACModifier { get; set; }
        [JsonProperty]
        public int DamageReduction { get; set; } = 0;
        [JsonProperty]
        public WearSlot Slot { get; set; } = WearSlot.None;
        [JsonProperty]
        public ArmourType ArmourType { get; set; } = ArmourType.Undefined;

        public Armour()
        {
            ItemType = ItemType.Armour;
        }
    }

    [Serializable]
    public class Consumable : InventoryItem
    {
        [JsonProperty]
        public ConsumableEffect Effects { get; set; }
        [JsonProperty]
        public HashSet<string> AppliedBuffs { get; set; } = new HashSet<string>();
        [JsonProperty]
        public int NumberOfDamageDice { get; set; }
        [JsonProperty]
        public int SizeofDamageDice { get; set; }
        [JsonIgnore]
        public bool AppliesBuffs => AppliedBuffs != null && AppliedBuffs.Count > 0;

        public Consumable()
        {
            ItemType = ItemType.Consumable;
        }

        public void Consume(Actor consumer)
        {
            foreach (ConsumableEffect effect in Enum.GetValues(typeof(ConsumableEffect)))
            {
                if (effect != ConsumableEffect.None && Effects.HasFlag(effect))
                {
                    switch (effect)
                    {
                        case ConsumableEffect.Antidote:
                            if (consumer.HasBuff("Poison"))
                            {
                                consumer.RemoveBuff("Poison");
                                if (consumer.ActorType == ActorType.Player)
                                {
                                    ((Player)consumer).Send($"%BGT%The poison in your system fades away...%PT%{Constants.NewLine}");
                                }
                            }
                            break;

                        case ConsumableEffect.Healing:
                            var hpEffect = Helpers.RollDice<int>(NumberOfDamageDice, SizeofDamageDice);
                            consumer.AdjustHP(hpEffect, out _);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BGT%A warmth rushes through you and your wounds start to heal!%PT%{Constants.NewLine}");
                            }
                            break;

                        case ConsumableEffect.DrainSP:
                            var buff = BuffManager.Instance.GetBuff("Energy Drain");
                            consumer.ApplyBuff(buff.Name, buff.Duration);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BYT%A sudden weakness flows through you, sapping your stamina!%PT%{Constants.NewLine}");
                            }
                            break;

                        case ConsumableEffect.DrainMP:
                            buff = BuffManager.Instance.GetBuff("Spirit Drain");
                            consumer.ApplyBuff(buff.Name, buff.Duration);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BRT%A rush of cold courses through you, freezing your very spirit!%PT%{Constants.NewLine}");
                            }
                            break;

                        case ConsumableEffect.MPRecovery:
                            buff = BuffManager.Instance.GetBuff("Spirit Fire");
                            consumer.ApplyBuff(buff.Name, buff.Duration);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BYT%You feel as though your very spirit is a rush with fire!%PT%{Constants.NewLine}");
                            }
                            break;

                        case ConsumableEffect.Death:
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BRT%Death comes for all!%PT%{Constants.NewLine}");
                            }
                            consumer.Kill(consumer, false);
                            break;

                        case ConsumableEffect.Poison:
                            buff = BuffManager.Instance.GetBuff("Poison");
                            consumer.ApplyBuff(buff.Name, buff.Duration);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BRT%The foul taste sickens you to your core!%PT%{Constants.NewLine}");
                            }
                            break;

                        case ConsumableEffect.Restoration:
                            buff = BuffManager.Instance.GetBuff("Restoration");
                            consumer.ApplyBuff(buff.Name, buff.Duration);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BYT%A holy radiance fills you!%PT%{Constants.NewLine}");
                            }
                            break;

                        case ConsumableEffect.SPRecovery:
                            buff = BuffManager.Instance.GetBuff("Energy Fire");
                            consumer.ApplyBuff(buff.Name, buff.Duration);
                            if (consumer.ActorType == ActorType.Player)
                            {
                                ((Player)consumer).Send($"%BGT%A sudden surge of strength flows through you as your stamina starts to return!%PT%{Constants.NewLine}");
                            }
                            break;
                    }
                }
            }
            if (AppliedBuffs != null && AppliedBuffs.Count > 0)
            {
                foreach (var b in AppliedBuffs)
                {
                    var buff = BuffManager.Instance.GetBuff(b);
                    if (buff != null)
                    {
                        consumer.ApplyBuff(buff.Name, buff.Duration);
                        if (consumer.ActorType == ActorType.Player)
                        {
                            ((Player)consumer).Send($"%BGT%The item's magic rushes through you!%PT%{Constants.NewLine}");
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class Ring : InventoryItem
    {
        [JsonProperty]
        public WearSlot Slot { get; set; } = WearSlot.Finger;
        [JsonProperty]
        public HashSet<string> AppliedBuffs { get; set; } = new HashSet<string>();
        [JsonIgnore]
        public bool AppliesBuffs => AppliedBuffs != null && AppliedBuffs.Count > 0;
        [JsonProperty]
        public int DamageModifier { get; set; } = 0;
        [JsonProperty]
        public int HitModifier { get; set; } = 0;
        [JsonProperty]
        public int ACModifier { get; set; } = 0;
        [JsonProperty]
        public int DamageReduction { get; set; } = 0;

        public Ring()
        {
            ItemType = ItemType.Ring;
        }
    }

    [Serializable]
    public class Scroll : InventoryItem
    {
        [JsonProperty]
        public string CastsSpell { get; set; }

        public Scroll()
        {
            ItemType = ItemType.Scroll;
        }
    }
}
