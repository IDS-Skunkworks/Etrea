using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Objects
{
    [Serializable]
    public abstract class Actor
    {
        [JsonProperty]
        public Guid ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public ActorPosition Position { get; set; } = ActorPosition.Standing;
        [JsonProperty]
        public int Level { get; set; } = 1;
        [JsonProperty]
        public string ShortDescription { get; set; }
        [JsonProperty]
        public string LongDescription { get; set; }
        [JsonProperty]
        public int CurrentRoom { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<string, int> Buffs { get; set; } = new ConcurrentDictionary<string, int>();
        [JsonProperty]
        public ConcurrentDictionary<Guid, InventoryItem> Inventory { get; set; } = new ConcurrentDictionary<Guid, InventoryItem>();
        [JsonProperty]
        public ConcurrentDictionary<string, bool> Spells { get; set; } = new ConcurrentDictionary<string, bool>();
        [JsonProperty]
        public ConcurrentDictionary<string, bool> Skills { get; set; } = new ConcurrentDictionary<string, bool>();
        [JsonIgnore]
        public ConcurrentDictionary<Guid, bool> TargetQueue { get; set; } = new ConcurrentDictionary<Guid, bool>();
        [JsonProperty]
        public ulong Gold { get; set; }
        [JsonProperty]
        public int Strength { get; set; } = 10;
        [JsonProperty]
        public int Dexterity { get; set; } = 10;
        [JsonProperty]
        public int Constitution { get; set; } = 10;
        [JsonProperty]
        public int Intelligence { get; set; } = 10;
        [JsonProperty]
        public int Wisdom { get; set; } = 10;
        [JsonProperty]
        public int Charisma { get; set; } = 10;
        [JsonProperty]
        public int BaseArmourClass { get; set; } = 10;
        [JsonProperty]
        public int ArmourClass => CalculateArmourClass();
        [JsonProperty]
        public int DamageReduction => CalculateDamageReduction();
        [JsonProperty]
        public bool Visible { get; set; } = true;
        [JsonProperty]
        public int NumberOfAttacks { get; set; } = 1;
        [JsonProperty]
        public Armour HeadEquip { get; set; } = null;
        [JsonProperty]
        public Armour NeckEquip { get; set; } = null;
        [JsonProperty]
        public Weapon WeaponEquip { get; set; } = null;
        [JsonProperty]
        public Armour HeldEquip { get; set; } = null;
        [JsonProperty]
        public Armour ArmourEquip { get; set; } = null;
        [JsonProperty]
        public Ring LeftFingerEquip { get; set; } = null;
        [JsonProperty]
        public Ring RightFingerEquip { get; set; } = null;
        [JsonProperty]
        public Armour FeetEquip { get; set; } = null;
        [JsonProperty]
        public Gender Gender { get; set; } = Gender.Undefined;
        [JsonProperty]
        public Race Race { get; set; } = Race.None;
        [JsonProperty]
        public int CurrentHP { get; set; }
        [JsonProperty]
        public int CurrentMP { get; set; }
        [JsonProperty]
        public int MaxHP { get; set; }
        [JsonProperty]
        public int MaxMP { get; set; }
        [JsonProperty]
        public ActorAlignment Alignment { get; set; } = ActorAlignment.Neutral;
        [JsonProperty]
        public bool NaturalArmour { get; set; }
        [JsonProperty]
        public ActorType ActorType { get; set; }
        [JsonIgnore]
        public bool InCombat => TargetQueue.Count > 0;

        public virtual void Kill(Actor killer, bool killedInCombat)
        {
            // Remove the actor from any combat lists held by other actors and clear its own target queue
            NPCManager.Instance.RemoveActorFromNPCCombatQueue(ID);
            SessionManager.Instance.RemoveActorFromPlayerCombatQueue(ID);
            TargetQueue.Clear();
            KillActor(killer, killedInCombat);
        }

        protected virtual void KillActor(Actor killer, bool killedInCombat)
        {
            // handled in the override methods on NPC and Player
        }

        public virtual void Move(int newRID, bool wasTeleported)
        {
            MoveActor(newRID, wasTeleported);
        }

        protected virtual void MoveActor(int newRID, bool wasTeleported)
        {
            // handled in the override methods on NPC and Player
        }

        public virtual void Restore()
        {
            RestoreActor();
        }

        protected virtual void RestoreActor()
        {
            // override in derived classes
        }

        public void Esuna()
        {
            List<string> debuffList = new List<string>();
            foreach(var b in Buffs)
            {
                var s = SpellManager.Instance.GetSpell(b.Key);
                if (s != null && s.SpellType == SpellType.Debuff)
                {
                    debuffList.Add(b.Key);
                }
            }
            if (debuffList.Count > 0)
            {
                foreach(var b in debuffList)
                {
                    Buffs.TryRemove(b, out _);
                }
            }
        }

        public bool CanMove()
        {
            if (InCombat)
            {
                return false;
            }
            if (ActorType == ActorType.Player && ((Player)this).ShopContext != null)
            {
                return false;
            }
            if (ActorType == ActorType.NonPlayer && ((NPC)this).ShopID != 0)
            {
                int sid = ((NPC)this).ShopID;
                if (ShopManager.Instance.GetShop(sid).HasCustomers)
                {
                    return false;
                }
                if (((NPC)this).Flags.HasFlag(NPCFlags.Sentinel))
                {
                    return false;
                }
            }
            return Position == ActorPosition.Standing;
        }

        public string GetDiagnosis()
        {
            int percHealth = CurrentHP / MaxHP * 100;
            if (percHealth == 100)
            {
                return $"{Name} is in excellent condition.";
            }
            if (percHealth >= 90)
            {
                return $"{Name} has a few scratches";
            }
            if (percHealth >= 75)
            {
                return $"{Name} has some small wounds and bruises";
            }
            if (percHealth >= 50)
            {
                return $"{Name} has some quite a few wounds";
            }
            if (percHealth >= 30)
            {
                return $"{Name} has some big nasty wounds and scratches";
            }
            if (percHealth >= 15)
            {
                return $"{Name} has taken severe damage";
            }
            return $"{Name} could bleed out at any moment";
        }

        public bool HasBuff(string name)
        {
            return Buffs != null && Buffs.ContainsKey(name) && (Buffs[name] > 0 || Buffs[name] == -1);
        }

        public void ApplyBuff(string buffName, int duration)
        {
            if (Buffs.TryGetValue(buffName, out var buff))
            {
                // Increase duration if the actor already has the buff and the buff is not permanent (-1)
                if (buff >= 0)
                {
                    Buffs.AddOrUpdate(buffName, duration, (k, v) => v + duration);
                }
            }
            else
            {
                // Doesn't have the buff so add it and make sure we apply stat changes if needed
                if (Buffs.TryAdd(buffName, duration))
                {
                    switch (buffName)
                    {
                        case "Foxs Cunning":
                            Intelligence += 4;
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

                        case "Cats Grace":
                            Dexterity += 4;
                            break;

                        case "Bulls Strength":
                            Strength += 4;
                            break;
                    }
                }
            }
        }

        public void RemoveBuff(string buffName, bool removePermBuff)
        {
            if (Buffs.TryGetValue(buffName, out var duration) && duration > 0)
            {
                // decrement an existing buff
                var newDuration = duration - 1;
                Game.LogMessage($"TICK: Decrementing duration of Buff '{buffName}' on {Name}, new value = {newDuration}", LogLevel.Info);
                Buffs.TryUpdate(buffName, newDuration, duration);
            }
            if (Buffs.TryGetValue(buffName, out duration) && duration == 0)
            {
                // remove an expired buff
                Game.LogMessage($"TICK: Removing expired Buff '{buffName}' on {Name}", LogLevel.Info);
                if (ActorType == ActorType.Player)
                {
                    ((Player)this).Send($"%BYT%You feel the magic of {buffName} fade away...%PT%{Constants.NewLine}");
                }
                if (Buffs.TryRemove(buffName, out _))
                {
                    switch (buffName)
                    {
                        case "Foxs Cunning":
                            Intelligence -= 4;
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

                        case "Cats Grace":
                            Dexterity -= 4;
                            break;

                        case "Bulls Strength":
                            Strength -= 4;
                            break;
                    }
                }
            }
            if (removePermBuff)
            {
                // remove a permabuff that is no longer applicable
                if (Buffs.TryRemove(buffName, out _))
                {
                    switch (buffName)
                    {
                        case "Foxs Cunning":
                            Intelligence -= 4;
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

                        case "Cats Grace":
                            Dexterity -= 4;
                            break;

                        case "Bulls Strength":
                            Strength -= 4;
                            break;
                    }
                }
            }
        }

        public void RemoveBuff(string buffName)
        {
            Buffs.TryRemove(buffName, out _);
        }

        private int CalculateDamageReduction()
        {
            int damageReduction = 0;
            if (ArmourEquip != null)
            {
                damageReduction += ArmourEquip.DamageReduction;
            }
            if (HeldEquip != null)
            {
                damageReduction += HeldEquip.DamageReduction;
            }
            if (FeetEquip != null)
            {
                damageReduction += FeetEquip.DamageReduction;
            }
            if (LeftFingerEquip != null)
            {
                damageReduction += LeftFingerEquip.DamageReduction;
            }
            if (RightFingerEquip != null)
            {
                damageReduction += RightFingerEquip.DamageReduction;
            }
            if (HeadEquip != null)
            {
                damageReduction += HeadEquip.DamageReduction;
            }
            if (NeckEquip != null)
            {
                damageReduction += NeckEquip.DamageReduction;
            }
            if (HasBuff("Bark Skin"))
            {
                damageReduction += 2;
            }
            if (HasBuff("Iron Skin"))
            {
                damageReduction += 4;
            }
            if (HasBuff("Shield"))
            {
                damageReduction += 2;
            }
            return damageReduction;
        }

        public void AddToTargetQueue(Actor target)
        {
            TargetQueue.TryAdd(target.ID, true);
        }

        public bool HitsTarget(Actor target, out bool isCritical, out int baseRoll, out int modRoll)
        {
            baseRoll = Helpers.RollDice<int>(1, 20);
            modRoll = baseRoll;
            if (target.ActorType == ActorType.NonPlayer)
            {
                if (((NPC)target).Flags.HasFlag(NPCFlags.Flying))
                {
                    if (WeaponEquip == null)
                    {
                        if (ActorType == ActorType.Player)
                        {
                            ((Player)this).Send($"%BRT%Your target is flying and out of reach!%PT%{Constants.NewLine}");
                        }
                        isCritical = false;
                        return false;
                    }
                    var wpnIsBow = WeaponEquip.WeaponType == WeaponType.Longbow && WeaponEquip.WeaponType == WeaponType.Shortbow && WeaponEquip.WeaponType == WeaponType.Crossbow;
                    if (!wpnIsBow)
                    {
                        if (ActorType == ActorType.Player)
                        {
                            ((Player)this).Send($"%BRT%Your target is flying and out of reach!%PT%{Constants.NewLine}");
                        }
                        isCritical = false;
                        return false;
                    }
                }
            }
            if (baseRoll == 1)
            {
                isCritical = false;
                return false;
            }
            if (baseRoll == 20)
            {
                isCritical = true;
                return true;
            }
            if (HasSkill("Elite Striker") && baseRoll >= 18)
            {
                isCritical = true;
                return true;
            }
            isCritical = false;
            if (HasBuff("True Strike"))
            {
                modRoll += 20;
            }
            if (HasBuff("Desperate Attack"))
            {
                modRoll -= 4;
            }
            if (HasBuff("Bless"))
            {
                modRoll += 1;
            }
            if (WeaponEquip != null)
            {
                Weapon w = (Weapon)WeaponEquip;
                if (w.WeaponType == WeaponType.Longbow || w.WeaponType == WeaponType.Shortbow || w.WeaponType == WeaponType.Crossbow)
                {
                    modRoll = Math.Max(1, modRoll + Helpers.CalculateAbilityModifier(Dexterity));
                }
                else
                {
                    modRoll = Math.Max(1, modRoll + Helpers.CalculateAbilityModifier(Strength));
                }
                if (HasSkill("Blademaster"))
                {
                    if (w.WeaponType == WeaponType.Sword || w.WeaponType == WeaponType.Dagger || w.WeaponType == WeaponType.Axe)
                    {
                        modRoll += 2;
                    }
                }
                if (HasSkill("Sniper"))
                {
                    if (w.WeaponType == WeaponType.Longbow || w.WeaponType == WeaponType.Shortbow || w.WeaponType == WeaponType.Crossbow)
                    {
                        modRoll += 2;
                    }
                }
            }
            else
            {
                modRoll = Math.Max(1, modRoll + Helpers.CalculateAbilityModifier(Strength));
                if (HasSkill("Pugilism"))
                {
                    modRoll += 2;
                }
            }
            return modRoll >= target.ArmourClass;
        }

        public int CalculateHitDamage(Actor target, bool isCritical)
        {
            int numDamageDice = WeaponEquip != null ? ((Weapon)WeaponEquip).NumberOfDamageDice : 1;
            int szDamageDice = WeaponEquip != null ? ((Weapon)WeaponEquip).SizeOfDamageDice : 2;
            int baseDmg = Helpers.RollDice<int>(numDamageDice, szDamageDice);
            int abilityMod = 0;
            if (WeaponEquip != null)
            {
                Weapon w = (Weapon)WeaponEquip;
                if (w.WeaponType == WeaponType.Longbow || w.WeaponType == WeaponType.Shortbow || w.WeaponType == WeaponType.Crossbow)
                {
                    abilityMod = Helpers.CalculateAbilityModifier(Dexterity);
                }
                else
                {
                    abilityMod = Helpers.CalculateAbilityModifier(Strength);
                }
            }
            else
            {
                abilityMod = Helpers.CalculateAbilityModifier(Strength);
            }
            int modDmg = Math.Max(1, baseDmg + abilityMod);
            if (WeaponEquip != null)
            {
                Weapon w = (Weapon)WeaponEquip;
                if (w.WeaponType == WeaponType.Sword || w.WeaponType == WeaponType.Axe || w.WeaponType == WeaponType.Dagger)
                {
                    if (HasSkill("Blademaster"))
                    {
                        modDmg += 1;
                    }
                }
                if (w.WeaponType == WeaponType.Shortbow || w.WeaponType == WeaponType.Longbow || w.WeaponType == WeaponType.Crossbow)
                {
                    if (HasSkill("Sniper"))
                    {
                        modDmg += 1;
                    }
                }
            }
            else
            {
                if (HasSkill("Pugilism"))
                {
                    modDmg += 1;
                }
            }
            if (HasBuff("Desperate Attack"))
            {
                modDmg += 4;
            }
            if (HasBuff("Bless"))
            {
                modDmg += 2;
            }
            if (isCritical)
            {
                modDmg *= 2;
            }
            modDmg = Math.Max(0, modDmg - target.DamageReduction);
            return modDmg;
        }

        public bool RemoveEquipment(WearSlot slot, out dynamic item)
        {
            switch(slot)
            {
                case WearSlot.Weapon:
                    if (WeaponEquip != null)
                    {
                        item = WeaponEquip;
                        if (((Weapon)item).IsCursed)
                        {
                            return false;
                        }
                        WeaponEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;

                case WearSlot.Finger:
                    if (RightFingerEquip != null)
                    {
                        item = RightFingerEquip;
                        if (((Ring)item).IsCursed)
                        {
                            return false;
                        }
                        RightFingerEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    if (LeftFingerEquip != null)
                    {
                        item = LeftFingerEquip;
                        item = RightFingerEquip;
                        if (((Ring)item).IsCursed)
                        {
                            return false;
                        }
                        LeftFingerEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;

                case WearSlot.Head:
                    if (HeadEquip != null)
                    {
                        item = HeadEquip;
                        if (((Armour)item).IsCursed)
                        {
                            return false;
                        }
                        HeadEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;

                case WearSlot.Held:
                    if (HeldEquip != null)
                    {
                        item = HeldEquip;
                        if (((Armour)item).IsCursed)
                        {
                            return false;
                        }
                        HeldEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;

                case WearSlot.Body:
                    if (ArmourEquip != null)
                    {
                        item = ArmourEquip;
                        if (((Armour)item).IsCursed)
                        {
                            return false;
                        }
                        ArmourEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;

                case WearSlot.Neck:
                    if (NeckEquip != null)
                    {
                        item = NeckEquip;
                        if (((Armour)item).IsCursed)
                        {
                            return false;
                        }
                        NeckEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;

                case WearSlot.Feet:
                    if (FeetEquip != null)
                    {
                        item = FeetEquip;
                        if (((Armour)item).IsCursed)
                        {
                            return false;
                        }
                        FeetEquip = null;
                        AddItemToInventory(item);
                        return true;
                    }
                    break;
            }
            item = null;
            return false;
        }

        public void EquipItem(InventoryItem item, WearSlot slot)
        {
            switch(slot)
            {
                case WearSlot.Weapon:
                    WeaponEquip = (Weapon)item;
                    RemoveItemFromInventory(item);
                    break;

                case WearSlot.Finger:
                    if (RightFingerEquip == null)
                    {
                        RightFingerEquip = (Ring)item;
                        RemoveItemFromInventory(item);
                    }
                    else
                    {
                        if (LeftFingerEquip == null)
                        {
                            LeftFingerEquip = (Ring)item;
                            RemoveItemFromInventory(item);
                        }
                    }
                    break;

                case WearSlot.Head:
                    if (HeadEquip == null)
                    {
                        HeadEquip = (Armour)item;
                        RemoveItemFromInventory(item);
                    }
                    break;

                case WearSlot.Held:
                    if (HeldEquip == null)
                    {
                        HeldEquip = (Armour)item;
                        RemoveItemFromInventory(item);
                    }    
                    break;

                case WearSlot.Body:
                    if (ArmourEquip == null)
                    {
                        ArmourEquip = (Armour)item;
                        RemoveItemFromInventory(item);
                    }
                    break;

                case WearSlot.Neck:
                    if (NeckEquip == null)
                    {
                        NeckEquip = (Armour)item;
                        RemoveItemFromInventory(item);
                    }
                    break;

                case WearSlot.Feet:
                    if (FeetEquip == null)
                    {
                        FeetEquip = (Armour)item;
                        RemoveItemFromInventory(item);
                    }
                    break;
            }
        }

        private int CalculateArmourClass()
        {
            // Only recalculate AC if the Actor doesn't have natural armour
            if (NaturalArmour)
            {
                return BaseArmourClass;
            }
            int baseAC = BaseArmourClass;
            if (Race == Race.Orc)
            {
                baseAC += 1;
            }
            baseAC += Helpers.CalculateAbilityModifier(Dexterity);
            if (ArmourEquip != null)
            {
                baseAC += ArmourEquip.ACModifier;
            }
            if (HeldEquip != null)
            {
                baseAC += HeldEquip.ACModifier;
            }
            if (FeetEquip != null)
            {
                baseAC += FeetEquip.ACModifier;
            }
            if (LeftFingerEquip != null)
            {
                baseAC += LeftFingerEquip.ACModifier;
            }
            if (RightFingerEquip != null)
            {
                baseAC += RightFingerEquip.ACModifier;
            }
            if (HeadEquip != null)
            {
                baseAC += HeadEquip.ACModifier;
            }
            if (NeckEquip != null)
            {
                baseAC += NeckEquip.ACModifier;
            }
            if (HasBuff("Mage Armour"))
            {
                baseAC += 4;
            }
            if (HasBuff("Shield"))
            {
                baseAC += 2;
            }
            return baseAC;
        }

        public bool AddItemToInventory(InventoryItem item)
        {
            return Inventory.TryAdd(item.ItemID, item);
        }

        public bool AddItemToInventory(int id)
        {
            try
            {
                var i = ItemManager.Instance.GetItem(id);
                dynamic itemToAdd = null;
                switch (i.ItemType)
                {
                    case ItemType.Misc:
                        itemToAdd = Helpers.Clone<InventoryItem>(i);
                        break;

                    case ItemType.Weapon:
                        itemToAdd = Helpers.Clone<Weapon>(i);
                        break;

                    case ItemType.Consumable:
                        itemToAdd = Helpers.Clone<Consumable>(i);
                        break;

                    case ItemType.Armour:
                        itemToAdd = Helpers.Clone<Armour>(i);
                        break;

                    case ItemType.Ring:
                        itemToAdd = Helpers.Clone<Ring>(i);
                        break;

                    case ItemType.Scroll:
                        itemToAdd = Helpers.Clone<Scroll>(i);
                        break;
                }
                itemToAdd.ItemID = Guid.NewGuid();
                if (!Inventory.TryAdd(itemToAdd.ItemID, itemToAdd))
                {
                    Game.LogMessage($"WARN: Failed to add item {itemToAdd.ID} to the inventory of player {Name}", LogLevel.Warning);
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in Actor.AddItemToInventory(): {ex.Message}", LogLevel.Error);
                Game.LogMessage($"DEBUG: ID passed to Actor.AddItemToInventory(): {id}", LogLevel.Debug);
                return false;
            }
        }

        public bool RemoveItemFromInventory(InventoryItem item)
        {
            return Inventory.TryRemove(item.ItemID, out _);
        }

        public bool RemoveItemFromInventory(int id)
        {
            try
            {
                var i = Inventory.Values.FirstOrDefault(x => x.ID == id);
                if (i != null)
                {
                    Inventory.TryRemove(i.ItemID, out _);
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in Actor.RemoveItemFromInventory(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool HasItemInInventory(int id)
        {
            return Inventory.Values.Any(x => x.ID == id);
        }

        public bool HasItemInInventory(string criteria)
        {
            return Inventory.Values.Any(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public InventoryItem GetInventoryItem(int id)
        {
            return Inventory.Values.FirstOrDefault(x => x.ID == id);
        }

        public InventoryItem GetInventoryItem(string criteria)
        {
            return Inventory.Values.FirstOrDefault(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public int GetInventoryItemCount(int id)
        {
            return Inventory.Values.Count(x => x.ID == id);
        }

        public bool CanEquip(InventoryItem item, WearSlot slot, out string reason)
        {
            if (item == null)
            {
                reason = "item was null";
                return false;
            }
            bool isPlayer = ActorType == ActorType.Player;
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    Weapon w = (Weapon)item;
                    if (w.MonsterOnly)
                    {
                        reason = "This item can only be used by monsters.";
                        return false;
                    }
                    if (slot != WearSlot.Weapon)
                    {
                        reason = "You cannot equip this in that slot!";
                        return false;
                    }
                    if (isPlayer && w.RequiredSkills != null && w.RequiredSkills.Count > 0)
                    {
                        foreach (var skill in w.RequiredSkills)
                        {
                            if (!HasSkill(skill))
                            {
                                reason = "You lack the skill required to use this weapon!";
                                return false;
                            }
                        }
                    }
                    if (isPlayer && w.IsTwoHanded && HeldEquip != null && !HasSkill("Monkey Grip"))
                    {
                        reason = "You are holding something in your off-hand and cannot use a two-handed weapon!";
                        return false;
                    }
                    if (WeaponEquip != null)
                    {
                        reason = "You are already using a weapon!";
                        return false;
                    }
                    reason = string.Empty;
                    return true;

                case ItemType.Armour:
                    Armour a = (Armour)item;
                    if (slot != WearSlot.Body && slot != WearSlot.Head && slot != WearSlot.Feet && slot != WearSlot.Held && slot != WearSlot.Neck)
                    {
                        reason = "You cannot equip this in that slot!";
                        return false;
                    }
                    if (isPlayer && a.RequiredSkills != null && a.RequiredSkills.Count > 0)
                    {
                        foreach (var skill in a.RequiredSkills)
                        {
                            if (!HasSkill(skill))
                            {
                                reason = "You lack the skill require to use this item!";
                                return false;
                            }
                        }
                    }
                    if (a.Slot == WearSlot.Body && ArmourEquip != null)
                    {
                        reason = "You are already wearing some armour!";
                        return false;
                    }
                    if (a.Slot == WearSlot.Head && HeadEquip != null)
                    {
                        reason = "You already have something on your head!";
                        return false;
                    }
                    if (a.Slot == WearSlot.Feet && FeetEquip != null)
                    {
                        reason = "You are already wearing something on your feet!";
                        return false;
                    }
                    if (a.Slot == WearSlot.Held && HeldEquip != null)
                    {
                        reason = "You are already holding something in your off-hand!";
                        return false;
                    }
                    if (a.Slot == WearSlot.Neck && NeckEquip != null)
                    {
                        reason = "You are already wearing something around your neck!";
                        return false;
                    }
                    reason = string.Empty;
                    return true;

                case ItemType.Ring:
                    Ring r = (Ring)item;
                    if (slot != WearSlot.Finger)
                    {
                        reason = "You cannot equip this in that slot!";
                        return false;
                    }
                    if (LeftFingerEquip != null && RightFingerEquip != null)
                    {
                        reason = "You are already wearing rings on both hands!";
                        return false;
                    }
                    reason = string.Empty;
                    return true;

                default:
                    reason = "That type of item cannot be worn!";
                    return false;
            }
        }

        public bool HasSkill(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return Skills.ContainsKey(name);
        }

        public bool KnowsSpell(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return Spells.ContainsKey(name);
        }

        public void AdjustHP(int amount, out bool isKilled)
        {
            CurrentHP = amount < 0 ? CurrentHP = Math.Max(0, CurrentHP - Math.Abs(amount)) : CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
            isKilled = CurrentHP == 0;
        }

        public void AdjustMP(int amount)
        {
            CurrentMP = amount < 0 ? CurrentMP = Math.Max(0, CurrentMP - Math.Abs(amount)) : CurrentMP = Math.Min(MaxMP, CurrentMP + amount);
        }

        public void AdjustMaxHP(int amount)
        {
            if (amount < 0)
            {
                MaxHP = Math.Max(0, MaxHP - amount);
                CurrentHP = Math.Min(CurrentHP, MaxHP);
            }
            else
            {
                MaxHP += amount;
            }
        }

        public void AdjustMaxMP(int amount)
        {
            if (amount < 0)
            {
                MaxMP = Math.Max(0, MaxMP - amount);
                CurrentMP = Math.Min(CurrentMP, MaxMP);
            }
            else
            {
                MaxMP += amount;
            }
        }

        public bool CanBeSeenBy(Actor actor)
        {
            if (Visible || actor.Level >= Constants.ImmLevel || actor.HasBuff("Truesight") || actor.ID == ID)
            {
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class NPC : Actor
    {
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
        [JsonProperty]
        public int TemplateID { get; set; }
        [JsonProperty]
        public int ZoneID { get; set; }
        [JsonProperty]
        public int BonusHitDice { get; set; } = 0;
        [JsonProperty]
        public int NumberOfHitDice => Level + BonusHitDice;
        [JsonProperty]
        public int HitDieSize { get; set; } = 10;
        [JsonProperty]
        public int BonusHP { get; set; }
        [JsonProperty]
        public int MaxNumberInWorld { get; set; }
        [JsonProperty]
        public int AppearanceChance { get; set; }
        [JsonProperty]
        public string DepatureMessage { get; set; }
        [JsonProperty]
        public string ArrivalMessage { get; set; }
        [JsonProperty]
        public NPCFlags Flags { get; set; } = NPCFlags.None;
        [JsonProperty]
        public int ExpAward { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int, bool> MobProgs { get; set; } = new ConcurrentDictionary<int, bool>();
        [JsonProperty]
        public int ShopID { get; set; }
        [JsonIgnore]
        public ConcurrentDictionary<string, ulong> PlayersRemembered { get; set; } = new ConcurrentDictionary<string, ulong>();        

        public NPC()
        {
            LockHolder = Guid.Empty;
            Name = "Template NPC";
            NaturalArmour = true;
            ActorType = ActorType.NonPlayer;
            DepatureMessage = $"{Name} strides away";
            ArrivalMessage = $"{Name} wanders in";
        }

        protected override void RestoreActor()
        {
            CurrentHP = MaxHP;
            CurrentMP = MaxMP;
            Esuna();
        }

        protected override void MoveActor(int newRID, bool wasTeleported)
        {
            string msgToLeavingRoom = wasTeleported ? $"{Name} vanishes in a flash of light!" : DepatureMessage;
            string msgToArrivalRoom = wasTeleported ? $"{Name} appears from a swirling cloud of magic!" : ArrivalMessage;
            var playersInLeavingRoom = RoomManager.Instance.GetRoom(CurrentRoom).PlayersInRoom;
            if (playersInLeavingRoom != null && playersInLeavingRoom.Count > 0)
            {
                foreach (var p in playersInLeavingRoom)
                {
                    p.Send($"{msgToLeavingRoom}{Constants.NewLine}");
                }
            }
            if (MobProgs.Count > 0)
            {
                foreach (var mp in MobProgs.Keys)
                {
                    var mobProg = MobProgManager.Instance.GetMobProg(mp);
                    if (mobProg != null)
                    {
                        mobProg.Init();
                        mobProg.TriggerEvent(MobProgTrigger.MobLeave, new { mob = ID.ToString() });
                    }
                }
            }
            CurrentRoom = newRID;
            var playersInArrivalRoom = RoomManager.Instance.GetRoom(CurrentRoom).PlayersInRoom;
            if (playersInArrivalRoom != null && playersInArrivalRoom.Count > 0)
            {
                foreach(var p in playersInArrivalRoom)
                {
                    p.Send($"{msgToArrivalRoom}{Constants.NewLine}");
                }
            }
            if (MobProgs.Count > 0)
            {
                foreach (var mp in MobProgs.Keys)
                {
                    var mobProg = MobProgManager.Instance.GetMobProg(mp);
                    if (mobProg != null)
                    {
                        mobProg.Init();
                        mobProg.TriggerEvent(MobProgTrigger.MobEnter, new { mob = ID.ToString() });
                    }
                }
            }
        }

        protected override void KillActor(Actor killer, bool killedInCombat)
        {
            if (killer != null && killer.ActorType == ActorType.Player && killedInCombat)
            {
                // award Exp and gold
                SessionManager.Instance.GetSession(killer.ID).Player.AdjustExp(ExpAward, false, true);
                SessionManager.Instance.GetSession(killer.ID).Player.AdjustGold(Convert.ToInt32(Gold), false, true);
                // update the slain counter for the player based off NPC template ID
                SessionManager.Instance.GetSession(killer.ID).Player.SlainMonsters.AddOrUpdate(TemplateID, 1, (k, v) => v + 1);
            }
            if (killedInCombat)
            {
                bool dropsItems = false;
                if (Inventory.Count > 0)
                {
                    dropsItems = true;
                    foreach (var i in Inventory)
                    {
                        Inventory.TryRemove(i.Key, out var item);
                        dynamic dropItem = null;
                        switch(item.ItemType)
                        {
                            case ItemType.Ring:
                                dropItem = (Ring)item;
                                break;

                            case ItemType.Weapon:
                                dropItem = (Weapon)item;
                                break;

                            case ItemType.Armour:
                                dropItem = (Armour)item;
                                break;

                            case ItemType.Consumable:
                                dropItem = (Consumable)item;
                                break;

                            case ItemType.Scroll:
                                dropItem = (Scroll)item;
                                break;

                            default:
                                dropItem = item;
                                break;
                        }
                        RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, dropItem);
                    }
                }
                // check equipment slots and drop those items too
                if (HeadEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, HeadEquip);
                }
                if (NeckEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, NeckEquip);
                }
                if (ArmourEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ArmourEquip);
                }
                if (WeaponEquip != null)
                {
                    if (!((Weapon)WeaponEquip).MonsterOnly)
                    {
                        dropsItems = true;
                        RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, WeaponEquip);
                    }
                }
                if (HeldEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, HeldEquip);
                }
                if (FeetEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, FeetEquip);
                }
                if (LeftFingerEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, LeftFingerEquip);
                }
                if (RightFingerEquip != null)
                {
                    dropsItems = true;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, RightFingerEquip);
                }
                if (dropsItems)
                {
                    var localPlayers = RoomManager.Instance.GetRoom(CurrentRoom).PlayersInRoom;
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        var pn = Gender == Gender.Male ? 0 : Gender == Gender.Female ? 2 : 3;
                        foreach (var lp in localPlayers)
                        {
                            lp.Send($"{Name} drops some items, {Constants.PosessivePronouns[pn]} corpse swallowed by the Winds of Magic!{Constants.NewLine}");
                        }
                    }
                }
            }
            if (MobProgs.Count > 0)
            {
                foreach(var mp in MobProgs.Keys)
                {
                    var mobProg = MobProgManager.Instance.GetMobProg(mp);
                    if (mobProg != null)
                    {
                        mobProg.Init();
                        var killerID = killer != null ? killer.ID.ToString() : string.Empty;
                        mobProg.TriggerEvent(MobProgTrigger.MobDeath, new { mob = ID.ToString(), killer = killerID });
                    }
                }
            }
            NPCManager.Instance.RemoveNPCInstance(ID);
        }

        public void RememberPlayer(Actor player, ulong mudTick)
        {
            PlayersRemembered.AddOrUpdate(player.Name, mudTick, (k, v) => v = mudTick);
        }

        public void ForgetPlayer(Actor player)
        {
            if (PlayersRemembered.ContainsKey(player.Name))
            {
                PlayersRemembered.TryRemove(player.Name, out _);
            }
        }

        public bool RemembersPlayer(Actor player, out ulong mudTick)
        {
            if (PlayersRemembered.ContainsKey(player.Name))
            {
                mudTick = PlayersRemembered[player.Name];
                return true;
            }
            mudTick = 0;
            return false;
        }
    }

    [Serializable]
    public class Player : Actor
    {
        [JsonProperty]
        public string Title { get; set; }
        [JsonIgnore]
        public Shop ShopContext { get; set; } = null;
        [JsonProperty]
        public ConcurrentDictionary<int, bool> Recipes { get; set; } = new ConcurrentDictionary<int, bool>();
        [JsonProperty]
        public ConcurrentDictionary<string, string> CommandAliases { get; set; } = new ConcurrentDictionary<string, string>();
        [JsonProperty]
        public ConcurrentDictionary<Guid, bool> ActiveQuests { get; set; } = new ConcurrentDictionary<Guid, bool>();
        [JsonProperty]
        public ConcurrentDictionary<Guid, bool> CompletedQuests { get; set; } = new ConcurrentDictionary<Guid, bool>();
        [JsonProperty]
        public ConcurrentDictionary<string, bool> KnownLanguages { get; set; } = new ConcurrentDictionary<string, bool>();
        [JsonProperty]
        public ConcurrentDictionary<int, int> SlainMonsters { get; set; } = new ConcurrentDictionary<int, int>();
        [JsonIgnore]
        public List<PlayerMail> CurrentMail { get; set; } = new List<PlayerMail>();
        [JsonProperty]
        public string SpokenLanguage { get; set; } = "Common";
        [JsonProperty]
        public bool ShowDetailedRollInfo { get; set; } = false;
        [JsonProperty]
        public ActorClass Class { get; set; } = ActorClass.Undefined;
        [JsonProperty]
        public int MaxSP { get; set; } = 20;
        [JsonProperty]
        public int CurrentSP { get; set; } = 20;
        [JsonProperty]
        public ConcurrentDictionary<Guid, InventoryItem> VaultItems { get; set; } = new ConcurrentDictionary<Guid, InventoryItem>();
        [JsonProperty]
        public ulong VaultGold { get; set; }
        [JsonProperty]
        public uint Exp { get; set; }
        [JsonProperty]
        public int AlignmentScale { get; set; }
        [JsonProperty]
        public PlayerPrompt PromptStyle { get; set; } = PlayerPrompt.Normal;
        [JsonIgnore]
        public bool IsImmortal => Level >= Constants.ImmLevel;
        [JsonProperty]
        public PlayerFlags Flags { get; set; } = PlayerFlags.None;
        [JsonProperty]
        public DateTime ThawTime { get; set; }
        [JsonIgnore]
        private System.Timers.Timer FreezeTimer;
        [JsonIgnore]
        public Guid Snooping { get; set; } = Guid.Empty;

        public Player()
        {
            NaturalArmour = false;
            ActorType = ActorType.Player;
        }

        public void FreezePlayer(DateTime thawTime)
        {
            ThawTime = thawTime;
            Flags |= PlayerFlags.Frozen;
            if (FreezeTimer == null)
            {
                FreezeTimer = new System.Timers.Timer();
                FreezeTimer.Elapsed += FreezeTimer_Elapsed;
            }
            FreezeTimer.Interval = 60000;
            FreezeTimer.Start();
        }

        public double GetRemainingFreezeDuration()
        {
            var thawTime = ThawTime - DateTime.UtcNow;
            return thawTime.TotalMinutes;
        }

        private void FreezeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.UtcNow >= ThawTime)
            {
                ThawPlayer();
                return;
            }
            int thawTime = (int)Math.Round((ThawTime - DateTime.UtcNow).TotalMinutes, 0);
            Send($"%BMT%You are frozen! You will be able to move in {thawTime} minutes%PT%{Constants.NewLine}");
        }

        public void ThawPlayer()
        {
            FreezeTimer?.Stop();
            Flags &= ~PlayerFlags.Frozen;
            Send($"%BMT%The magic holding you fades and you can move again!%PT%{Constants.NewLine}");
        }

        protected override void MoveActor(int newRID, bool wasTeleported)
        {
            // TODO: MobProgs for player leaving and entering room
            string msgToLeavingRoom = wasTeleported ? "%N% vanishes, swallowed by the Winds of Magic!" : "%N% strides away";
            string msgToArrivalRoom = wasTeleported ? "%N% appears from a swirling cloud of magic!" : "%N% strides in";
            var playersInLeavingRoom = RoomManager.Instance.GetRoom(CurrentRoom).PlayersInRoom.Where(x => x.Player.ID != ID).ToList();
            if (playersInLeavingRoom != null && playersInLeavingRoom.Count > 0)
            {
                foreach (var p in playersInLeavingRoom)
                {
                    switch(CanBeSeenBy(p.Player))
                    {
                        case true:
                            msgToLeavingRoom = msgToLeavingRoom.Replace("%N%", Name);
                            break;

                        case false:
                            msgToLeavingRoom = msgToLeavingRoom.Replace("%N%", "Something");
                            break;
                    }
                    p.Send($"{msgToLeavingRoom}{Constants.NewLine}");
                }
            }
            foreach(var n in RoomManager.Instance.GetRoom(CurrentRoom).NPCsInRoom)
            {
                if (n.MobProgs.Count > 0)
                {
                    foreach(var mp in n.MobProgs.Keys)
                    {
                        var mobProg = MobProgManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerLeave, new { player = ID.ToString(), mob = n.ID.ToString() });
                        }
                    }
                }
            }
            CurrentRoom = newRID;
            var playersInArrivalRoom = RoomManager.Instance.GetRoom(CurrentRoom).PlayersInRoom.Where(x => x.Player.ID != ID).ToList();
            if (playersInArrivalRoom != null && playersInArrivalRoom.Count > 0)
            {
                foreach(var p in playersInArrivalRoom)
                {
                    switch(CanBeSeenBy(p.Player))
                    {
                        case true:
                            msgToArrivalRoom = msgToArrivalRoom.Replace("%N%", Name);
                            break;

                        case false:
                            msgToArrivalRoom = msgToArrivalRoom.Replace("%N%", "Something");
                            break;
                    }
                    p.Send($"{msgToArrivalRoom}{Constants.NewLine}");
                }
            }
            RoomManager.Instance.GetRoom(CurrentRoom).DescribeRoom(SessionManager.Instance.GetSession(ID));
            foreach(var n in RoomManager.Instance.GetRoom(CurrentRoom).NPCsInRoom)
            {
                if (n.MobProgs.Count > 0)
                {
                    foreach(var m in n.MobProgs)
                    {
                        var mp = MobProgManager.Instance.GetMobProg(m.Key);
                        if (mp != null)
                        {
                            mp.Init();
                            mp.TriggerEvent(MobProgTrigger.PlayerEnter, new { player = ID.ToString(), mob = n.ID.ToString() });
                        }
                    }
                }
            }
        }

        protected override void KillActor(Actor killer, bool killedInCombat)
        {
            // remove exp and gold, clear target queue, drop inventory and teleport to limbo
            TargetQueue.Clear();
            var msg = killer == null ? $"Alas, you have been slain!{Constants.NewLine}" : $"You have been slain by {killer.Name}...{Constants.NewLine}";
            Send(msg);
            if (Level >= 3)
            {
                // lose some XP if we are level 3 or higher
                var xpLoss = Convert.ToUInt32(Exp / 100 * Level);
                Exp = Math.Max(0, Exp - xpLoss);
                Send($"You have lost {xpLoss:N0} Exp!{Constants.NewLine}");
            }
            if (Gold > 0)
            {
                RoomManager.Instance.AddGoldToRoom(CurrentRoom, Gold);
                Gold = 0;
                Send($"You have lost all the Gold you were carrying!{Constants.NewLine}");
            }
            if (Inventory.Count > 0)
            {
                Send($"You have dropped all the items you were carrying!{Constants.NewLine}");
                foreach(var i in Inventory)
                {
                    Inventory.TryRemove(i.Key, out InventoryItem item);
                    dynamic dropItem = null;
                    switch (item.ItemType)
                    {
                        case ItemType.Ring:
                            dropItem = (Ring)item;
                            break;

                        case ItemType.Weapon:
                            dropItem = (Weapon)item;
                            break;

                        case ItemType.Armour:
                            dropItem = (Armour)item;
                            break;

                        case ItemType.Consumable:
                            dropItem = (Consumable)item;
                            break;

                        case ItemType.Scroll:
                            dropItem = (Scroll)item;
                            break;

                        default:
                            dropItem = item;
                            break;
                    }
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, dropItem);
                }
            }
            CurrentHP = 0;
            CurrentMP = 0;
            CurrentSP = 0;
            Position = ActorPosition.Dead;
            List<string> nonPermBuffs = Buffs.Where(x => x.Value > -1).Select(x => x.Key).ToList();
            if (nonPermBuffs.Count > 0)
            {
                foreach (var b in nonPermBuffs)
                {
                    Buffs.TryRemove(b, out _);
                }
            }
            Move(Game.Limbo, true);
        }

        protected override void RestoreActor()
        {
            CurrentMP = MaxMP;
            CurrentHP = MaxHP;
            CurrentSP = MaxSP;
            Esuna();
        }

        public void AddLanguage(string language)
        {
            KnownLanguages.AddOrUpdate(language, true, (k, v) => true);
        }

        public void RemoveLanguage(string language)
        {
            KnownLanguages.TryRemove(language, out _);
        }

        public bool KnowsLanguage(string language)
        {
            return KnownLanguages.ContainsKey(language);
        }

        public bool KnowsRecipe(int recipeID)
        {
            return Recipes.ContainsKey(recipeID);
        }

        public void UpdateAlignment(int amount)
        {
            if (amount < 0)
            {
                // negative alignment moves towards Evil
                AlignmentScale = Math.Max(-100, AlignmentScale -= amount);
            }
            else
            {
                // positive alignment moves towards Good
                AlignmentScale = Math.Min(100, AlignmentScale += amount);
            }
            if (AlignmentScale <= -50)
            {
                Alignment = ActorAlignment.Evil;
            }
            if (AlignmentScale > -50 && AlignmentScale < 50)
            {
                Alignment = ActorAlignment.Neutral;
            }
            if (AlignmentScale >= 50)
            {
                Alignment = ActorAlignment.Good;
            }
        }

        public void Send(string message)
        {
            var s = SessionManager.Instance.GetSession(ID);
            if (s != null)
            {
                s.Send(message);
            }
        }

        public InventoryItem GetVaultItem(string criteria)
        {
            return VaultItems.Values.FirstOrDefault(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public void AdjustExp(int amount, bool bypassBonusChecks = false, bool notifyPlayer = false)
        {
            if (amount < 0)
            {
                Exp = Convert.ToUInt32(Math.Max(0, Exp - Math.Abs(amount)));
                if (notifyPlayer)
                {
                    Send($"You have lost {amount:N0} Exp!{Constants.NewLine}");
                }
            }
            else
            {
                int bonusExp = 0;
                if (!bypassBonusChecks)
                {
                    if (HasSkill("Quick Learner"))
                    {
                        bonusExp += Convert.ToInt32(amount * 0.1);
                    }
                    if (Race == Race.Human)
                    {
                        bonusExp += Convert.ToInt32(amount * 0.25);
                    }
                }
                Exp += Convert.ToUInt32(amount + bonusExp);
                if (notifyPlayer)
                {
                    Send($"You have gained {amount:N0} Exp!{Constants.NewLine}");
                    if (bonusExp > 0)
                    {
                        Send($"You have gained {bonusExp:N0} bonus Exp!{Constants.NewLine}");
                    }
                }
                if (LevelTable.HasAchievedNewLevel(Exp, Level, out int nLevel))
                {
                    var levelsToAdvance = nLevel - Level;
                    Game.LogMessage($"DEBUG: Advancing Player {Name} by {levelsToAdvance} character levels", LogLevel.Debug);
                    LevelUp(nLevel - Level);
                }
            }
        }

        private void LevelUp(int levelsToAdvance)
        {
            // TODO: Give Skills / Spells at certain levels?
            for (int i = 0; i < levelsToAdvance; i++)
            {
                Level++;
                int hpIncrease = 0;
                int mpIncrease = 0;
                int spIncrease = 0;
                switch(Class)
                {
                    case ActorClass.Thief:
                        hpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 6) + Helpers.CalculateAbilityModifier(Constitution));
                        mpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 6) + Helpers.CalculateAbilityModifier(Intelligence));
                        spIncrease = Math.Max(1, Helpers.RollDice<int>(1, 10) + Helpers.CalculateAbilityModifier(Constitution));
                        if (Level % 4 == 0)
                        {
                            Charisma++;
                            Dexterity++;
                            Send($"Your Charisma and Dexterity have improved!{Constants.NewLine}");
                        }
                        break;

                    case ActorClass.Fighter:
                        hpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 10) + Helpers.CalculateAbilityModifier(Constitution));
                        mpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 4) + Helpers.CalculateAbilityModifier(Intelligence));
                        spIncrease = Math.Max(1, Helpers.RollDice<int>(1, 10) + Helpers.CalculateAbilityModifier(Constitution));
                        if (Level % 4 == 0)
                        {
                            Strength++;
                            Constitution++;
                            Send($"Your Strength and Constitution have improved!{Constants.NewLine}");
                        }
                        break;

                    case ActorClass.Cleric:
                        hpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 8) + Helpers.CalculateAbilityModifier(Constitution));
                        mpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 8) + Helpers.CalculateAbilityModifier(Wisdom));
                        spIncrease = Math.Max(1, Helpers.RollDice<int>(1, 10) + Helpers.CalculateAbilityModifier(Constitution));
                        if (Level % 4 == 0)
                        {
                            Wisdom++;
                            Charisma++;
                            Send($"Your Wisdom and Charisma have improved!{Constants.NewLine}");
                        }
                        break;

                    case ActorClass.Wizard:
                        hpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 4) + Helpers.CalculateAbilityModifier(Constitution));
                        mpIncrease = Math.Max(1, Helpers.RollDice<int>(1, 10) + Helpers.CalculateAbilityModifier(Intelligence));
                        spIncrease = Math.Max(1, Helpers.RollDice<int>(1, 10) + Helpers.CalculateAbilityModifier(Constitution));
                        if (Level % 4 == 0)
                        {
                            Intelligence++;
                            Charisma++;
                            Send($"Your Intelligence and Charisma have improved!{Constants.NewLine}");
                        }
                        break;
                }
                CurrentHP += hpIncrease;
                CurrentMP += mpIncrease;
                CurrentSP += spIncrease;
                MaxHP += hpIncrease;
                MaxMP += mpIncrease;
                MaxSP += spIncrease;
                Send($"You have gained %BRT%{hpIncrease}%PT% HP, %BGT%{mpIncrease}%PT% MP and %BYT%{spIncrease}%PT% SP!{Constants.NewLine}");
            }
        }

        public void AdjustGold(long amount, bool bypassBonusChecks = false, bool notifyPlayer = false)
        {
            if (amount < 0)
            {
                ulong absAmount = (ulong)Math.Abs(amount);
                Gold = Math.Max(0, Gold - absAmount);
            }
            else
            {
                Gold += (ulong)amount;
                if (notifyPlayer)
                {
                    Send($"You gain {amount:N0} Gold!{Constants.NewLine}");
                }
                if (!bypassBonusChecks && HasSkill("Gold Digger"))
                {
                    var bonusAmount =  amount * 0.1;
                    if (notifyPlayer)
                    {
                        Send($"Your skills grant you a bonus of {bonusAmount:N0} Gold!{Constants.NewLine}");
                    }
                    Gold += (ulong)bonusAmount;
                }
                if (!bypassBonusChecks && Race == Race.Human)
                {
                    var bonusAmount = amount * 0.1;
                    if (notifyPlayer)
                    {
                        Send($"Your Human nature grants you a bonus of {bonusAmount:N0} Gold!{Constants.NewLine}");
                    }
                    Gold += (ulong)bonusAmount;
                }
            }
        }

        public void AdjustSP(int amount)
        {
            CurrentSP = amount < 0 ? CurrentSP = Math.Max(0, CurrentSP - Math.Abs(amount)) : CurrentSP = Math.Min(MaxSP, CurrentSP + amount);
        }

        public void AdjustMaxSP(int amount)
        {
            if (amount < 0)
            {
                MaxSP = Math.Max(0, MaxSP - amount);
                CurrentSP = Math.Min(MaxSP, CurrentSP);
            }
            else
            {
                MaxSP += amount;
            }
        }

        public bool AddSkill(string name)
        {
            var skill = SkillManager.Instance.GetSkill(name);
            if (skill != null)
            {
                return Skills.TryAdd(skill.Name, true);
            }
            return false;
        }

        public bool RemoveSkill(string name)
        {
            var skill = SkillManager.Instance.GetSkill(name);
            if (skill != null)
            {
                return Skills.TryRemove(skill.Name, out _);
            }
            return false;
        }

        public bool AddSpell(int id)
        {
            var s = SpellManager.Instance.GetSpell(id);
            if (s != null)
            {
                return Spells.TryAdd(s.Name, true);
            }
            return false;
        }

        public bool AddSpell(string name)
        {
            var s = SpellManager.Instance.GetSpell(name);
            if (s != null)
            {
                return Spells.TryAdd(s.Name, true);
            }
            return false;
        }

        public bool AddRecipe(string name)
        {
            var r = RecipeManager.Instance.GetRecipe(name).FirstOrDefault();
            if (r != null)
            {
                return Recipes.TryAdd(r.ID, true);
            }
            return false;
        }

        public bool AddRecipe(int id)
        {
            var r = RecipeManager.Instance.GetRecipe(id);
            if (r != null)
            {
                return Recipes.TryAdd(r.ID, true);
            }
            return false;
        }

        public bool RemoveRecipe(string name)
        {
            var r = RecipeManager.Instance.GetRecipe(name).FirstOrDefault();
            if (r != null)
            {
                return Recipes.TryRemove(r.ID, out _);
            }
            return false;
        }

        public bool RemoveRecipe(int id)
        {
            var r = RecipeManager.Instance.GetRecipe(id);
            if (r != null)
            {
                return Recipes.TryRemove(r.ID, out _);
            }
            return false;
        }

        public bool RemoveSpell(int id)
        {
            var s = SpellManager.Instance.GetSpell(id);
            if (s != null)
            {
                return Spells.TryRemove(s.Name, out _);
            }
            return false;
        }

        public bool RemoveSpell(string name)
        {
            var s = SpellManager.Instance.GetSpell(name);
            if (s != null)
            {
                return Spells.TryRemove(s.Name, out _);
            }
            return false;
        }

        public bool AddItemToVault(int id)
        {
            var i = Inventory.Values.FirstOrDefault(x => x.ID == id);
            if (i != null)
            {
                Inventory.TryRemove(i.ItemID, out _);
                VaultItems.TryAdd(i.ItemID, i);
                return true;
            }
            return false;
        }

        public bool AddItemToVault(InventoryItem item)
        {
            if (item != null)
            {
                Inventory.TryRemove(item.ItemID, out _);
                VaultItems.TryAdd(item.ItemID, item);
                return true;
            }
            return false;
        }

        public bool RemoveItemFromVault(InventoryItem item)
        {
            if (item != null)
            {
                VaultItems.TryRemove(item.ItemID, out _);
                Inventory.TryAdd(item.ItemID, item);
                return true;
            }
            return false;
        }

        public bool RemoveItemFromVault(int id)
        {
            var i = VaultItems.Values.FirstOrDefault(x => x.ID == id);
            if (i != null)
            {
                VaultItems.TryRemove(i.ItemID, out _);
                Inventory.TryAdd(i.ItemID, i);
                return true;
            }
            return false;
        }

        public bool AddItemToVault(string name)
        {
            var i = Inventory.Values.FirstOrDefault(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            if (i != null)
            {
                Inventory.TryRemove(i.ItemID, out _);
                VaultItems.TryAdd(i.ItemID, i);
                return true;
            }
            return false;
        }

        public bool AddGoldToVault(ulong amount)
        {
            if (Gold < amount)
            {
                return false;
            }
            Gold = Math.Max(0, Gold - amount);
            VaultGold += amount;
            return true;
        }

        public bool RemoveGoldFromVault(ulong amount)
        {
            if (VaultGold < amount)
            {
                return false;
            }
            VaultGold = Math.Max(0, VaultGold - amount);
            Gold += amount;
            return true;
        }

        public void UpdatePromptStyle()
        {
            PromptStyle = PromptStyle == PlayerPrompt.Normal ? PlayerPrompt.Percentage : PlayerPrompt.Normal;
        }
    }
}
