using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
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
        internal string Title { get; set; }
        [JsonProperty]
        internal uint Level { get; set; }
        [JsonProperty]
        internal Gender Gender { get; set; }
        [JsonProperty]
        internal ActorType Type { get; set; }
        [JsonProperty]
        internal ActorStats Stats { get; set; }
        [JsonProperty]
        internal ActorClass Class { get; set; }
        [JsonProperty]
        internal ActorRace Race { get; set; }
        [JsonProperty]
        internal ActorPosition Position { get; set; }
        [JsonProperty]
        internal ActorAlignment Alignment { get; set; }
        [JsonProperty]
        internal int AlignmentScale { get; set; }
        [JsonProperty]
        internal EquippedItems EquippedItems { get; set; }
        [JsonProperty]
        internal uint CurrentRoom { get; set; }
        [JsonProperty]
        internal bool Visible { get; set; }
        [JsonProperty]
        internal Dictionary<string, int> Buffs { get; set; }
        [JsonProperty]
        internal List<Skills.Skill> Skills { get; set; }
        [JsonProperty]
        internal List<Spells.Spell> Spells { get; set; }
        [JsonProperty]
        internal List<InventoryItem> Inventory { get; set; }
        [JsonProperty]
        internal List<InventoryItem> VaultStore { get; set; }
        [JsonProperty]
        internal uint NumberOfAttacks { get; set; }
        [JsonProperty]
        internal uint CombatInitiative { get; set; }
        [JsonProperty]
        internal int FireResistance { get; set; }
        [JsonProperty]
        internal int IceResistance { get; set; }
        [JsonProperty]
        internal int LightningResistance { get; set; }
        [JsonProperty]
        internal int EarthResistance { get; set; }
        [JsonProperty]
        internal int HolyResistance { get; set; }
        [JsonProperty]
        internal int DarkResistance { get; set; }

        internal void CalculateArmourClass()
        {
            int baseAC = 10;
            int dexModifier = ActorStats.CalculateAbilityModifier(Stats.Dexterity);
            int eqModifiers = 0, skillModifiers = 0;
            eqModifiers += EquippedItems.Head != null ? EquippedItems.Head.ArmourClassModifier : 0;
            eqModifiers += EquippedItems.Neck != null ? EquippedItems.Neck.ArmourClassModifier : 0;
            eqModifiers += EquippedItems.Armour != null ? EquippedItems.Armour.ArmourClassModifier : 0;
            eqModifiers += EquippedItems.Held != null ? EquippedItems.Held.ArmourClassModifier : 0;
            eqModifiers += EquippedItems.FingerLeft != null ? EquippedItems.FingerLeft.ArmourClassModifier : 0;
            eqModifiers += EquippedItems.FingerRight != null ? EquippedItems.FingerRight.ArmourClassModifier : 0;
            if(HasSkill("Parry"))
            {
                skillModifiers += 2;
            }
            if(HasSkill("Dodge"))
            {
                skillModifiers += 2;
            }
            var finalAC = baseAC + dexModifier + eqModifiers + skillModifiers;
            finalAC = finalAC < 0 ? 0 : finalAC;
            Stats.ArmourClass = Convert.ToUInt32(finalAC);
        }

        internal bool HasItemInInventory(uint itemID)
        {
            if(Inventory != null && Inventory.Count > 0)
            {
                return Inventory.Any(x => x.Id ==  itemID);
            }
            return false;
        }

        internal bool HasItemInVault(uint itemID)
        {
            if (VaultStore != null && VaultStore.Count > 0)
            {
                return VaultStore.Any(x => x.Id == itemID);
            }
            return false;
        }

        internal void AddSpell(string spellName)
        {
            if(Spells == null)
            {
                Spells = new List<Spells.Spell>();
            }
            if (!Spells.Any(x => Regex.Match(x.SpellName, spellName, RegexOptions.IgnoreCase).Success))
            {
                var s = Entities.Spells.GetSpell(spellName);
                Spells.Add(s);
            }
        }

        internal void RemoveSpell(string spellName)
        {
            if(Spells == null)
            {
                Spells = new List<Spells.Spell>();
            }
            if (Spells.Any(x => Regex.Match(x.SpellName, spellName, RegexOptions.IgnoreCase).Success))
            {
                var s = Entities.Spells.GetSpell(spellName);
                Spells.Remove(s);
            }
        }

        internal bool HasSpell(string spellName)
        {
            if(Spells == null)
            {
                Spells = new List<Spells.Spell>();
            }
            if (Spells.Count > 0)
            {
                return Spells.Any(x => Regex.Match(x.SpellName, spellName, RegexOptions.IgnoreCase).Success);
            }
            return false;
        }

        internal void AddSkill(string skillName)
        {
            if(Skills == null)
            {
                Skills = new List<Skills.Skill>();
            }
            if (!Skills.Any(x => Regex.Match(x.Name, skillName, RegexOptions.IgnoreCase).Success))
            {
                var s = Entities.Skills.GetSkill(skillName);
                Skills.Add(s);
            }
        }

        internal void RemoveSkill(string skillName)
        {
            if (Skills == null)
            {
                Skills = new List<Skills.Skill>();
            }
            if (Skills.Any(x => Regex.Match(x.Name, skillName, RegexOptions.IgnoreCase).Success))
            {
                var s = Entities.Skills.GetSkill(skillName);
                Skills.Remove(s);
            }
        }

        internal bool HasSkill(string skillName)
        {
            if (Skills == null)
            {
                Skills = new List<Skills.Skill>();
            }
            if (Skills.Count > 0)
            {
                return Skills.Any(x => Regex.Match(x.Name, skillName, RegexOptions.IgnoreCase).Success);
            }
            return false;
        }

        internal bool HasBuff(string buffName)
        {
            if(Buffs == null)
            {
                Buffs = new Dictionary<string, int>();
            }
            if (Buffs != null && Buffs.Count > 0)
            {
                return Buffs.Keys.Any(x => Regex.Match(x, buffName, RegexOptions.IgnoreCase).Success);
            }
            return false;
        }

        internal void RemoveBuff(string buffName)
        {
            if(Buffs == null)
            {
                Buffs = new Dictionary<string, int>();
            }    
            if (Buffs.ContainsKey(buffName))
            {
                Buffs.Remove(buffName);
                switch (buffName)
                {
                    // add cases here to remove the effects of buffs which have expired
                    case "Bulls Strength":
                        Stats.Strength -= 4;
                        break;

                    case "Cats Grace":
                        Stats.Dexterity -= 4;
                        break;

                    case "Bears Endurance":
                        Stats.Constitution -= 4;
                        break;

                    case "Owls Wisdom":
                        Stats.Wisdom -= 4;
                        break;

                    case "Eagles Splendour":
                        Stats.Charisma -= 4;
                        break;

                    case "Foxs Cunning":
                        Stats.Intelligence -= 4;
                        break;

                    case "Mage Armour":
                        Stats.ArmourClass -= 5;
                        break;

                    case "Fae Fire":
                        Stats.ArmourClass += 4;
                        break;
                }
            }
            CalculateArmourClass();
        }

        internal void AddBuff(string buffName, bool permBuff = false)
        {
            CalculateArmourClass();
            if (Buffs == null)
            {
                Buffs = new Dictionary<string, int>();
            }
            if(Buffs.ContainsKey(buffName))
            {
                if(permBuff)
                {
                    Buffs[buffName] = -1;
                }
                else
                {
                    if (Buffs[buffName] > 0)
                    {
                        Buffs[buffName] += Entities.Buffs.GetBuff(buffName).BuffDuration;
                    }
                }
            }
            else
            {
                if(permBuff)
                {
                    Buffs.Add(buffName, -1);
                }
                else
                {
                    Buffs.Add(buffName, Entities.Buffs.GetBuff(buffName).BuffDuration);
                }
            }
            switch (buffName)
            {
                // add cases here to add the effects of buffs which have been applied
                case "Bulls Strength":
                    Stats.Strength += 4;
                    break;

                case "Cats Grace":
                    Stats.Dexterity += 4;
                    break;

                case "Bears Endurance":
                    Stats.Constitution += 4;
                    break;

                case "Owls Wisdom":
                    Stats.Wisdom += 4;
                    break;

                case "Eagles Splendour":
                    Stats.Charisma += 4;
                    break;

                case "Foxs Cunning":
                    Stats.Intelligence += 4;
                    break;

                case "Mage Armour":
                    Stats.ArmourClass += 5;
                    break;

                case "Fae Fire":
                    Stats.ArmourClass -= 4;
                    break;
            }
        }
    }

    [Serializable]
    internal class NPC : Actor
    {
        [JsonProperty]
        internal NPCFlags BehaviourFlags { get; set; }
        [JsonProperty]
        internal string DepartMessage { get; set; }
        [JsonProperty]
        internal string ArrivalMessage { get; set; }
        [JsonProperty]
        internal uint BaseExpAward { get; set; }
        [JsonProperty]
        internal uint NumberOfHitDice { get; set; }
        [JsonProperty]
        internal uint SizeOfHitDice { get; set; }
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

        internal NPC()
        {
            Inventory = new List<InventoryItem>();
            Skills = new List<Skills.Skill>();
            Spells = new List<Spells.Spell>();
            Buffs = new Dictionary<string, int>();
            Name = "NPC";
            DepartMessage = $"{Name} wanders away{Constants.NewLine}";
            ArrivalMessage = $"{Name} arrives, looking mean!{Constants.NewLine}";
            Stats = new ActorStats();
        }

        internal NPC ShallowCopy()
        {
            var npc = (NPC)this.MemberwiseClone();
            npc.NPCGuid = Guid.NewGuid();
            return npc;
        }

        internal bool FleeCombat(ref ILoggingProvider loggingProvider, out uint destRid)
        {
            destRid = 0;
            try
            {
                var roomExits = RoomManager.Instance.GetRoom(CurrentRoom).RoomExits;
                if(roomExits.Count > 0)
                {
                    var rnd = new Random(DateTime.Now.GetHashCode());
                    var exit = roomExits[rnd.Next(roomExits.Count)];
                    if(ZoneManager.Instance.IsRIDInZone(exit.DestinationRoomID, AppearsInZone))
                    {
                        destRid = exit.DestinationRoomID;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                loggingProvider.LogMessage($"ERROR: Error while NPC fleeing combat: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal virtual bool Move(ref NPC n, uint fromRoomID, uint destRoomID, bool wasTeleported)
        {
            try
            {
                var targetRoom = RoomManager.Instance.GetRoom(destRoomID);
                if(targetRoom == null)
                {
                    return false;
                }
                else
                {
                    RoomManager.Instance.UpdateNPCsInRoom(fromRoomID, true, false, ref n);
                    RoomManager.Instance.UpdateNPCsInRoom(destRoomID, false, false, ref n);
                    NPCManager.Instance.MoveNPCToNewRID(n.NPCGuid, destRoomID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Failed to move NPC from room {fromRoomID} to room {destRoomID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal void Kill(bool killedInCombat)
        {
            bool dropsItems = false;
            if(Inventory != null && Inventory.Count > 0)
            {
                dropsItems = true;
                foreach(var i in Inventory)
                {
                    var item = i;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
            }
            if(EquippedItems != null)
            {
                if(EquippedItems.Head != null && !EquippedItems.Head.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.Head;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
                if(EquippedItems.Held != null && !EquippedItems.Held.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.Held;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
                if(EquippedItems.Weapon != null && !EquippedItems.Weapon.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.Weapon;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
                if(EquippedItems.FingerLeft != null && !EquippedItems.FingerLeft.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.FingerLeft;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
                if(EquippedItems.FingerRight != null && !EquippedItems.FingerRight.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.FingerRight;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
                if(EquippedItems.Armour != null && !EquippedItems.Armour.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.Armour;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
                if(EquippedItems.Neck != null && !EquippedItems.Neck.IsMonsterItem)
                {
                    dropsItems = true;
                    var item = EquippedItems.Neck;
                    RoomManager.Instance.AddItemToRoomInventory(CurrentRoom, ref item);
                }
            }
            if(FollowingPlayer != Guid.Empty)
            {
                SessionManager.Instance.GetPlayerByGUID(FollowingPlayer).Player.FollowerID = Guid.Empty;
                SessionManager.Instance.GetPlayerByGUID(FollowingPlayer).Send($"Alas, your follower has been slain!{Constants.NewLine}");
            }
            if(killedInCombat && dropsItems)
            {
                var localPlayers = RoomManager.Instance.GetPlayersInRoom(CurrentRoom);
                if(localPlayers != null && localPlayers.Count > 0)
                {
                    var article = Helpers.IsCharAVowel(Name[0]) ? "An" : "A";
                    foreach(var lp in localPlayers)
                    {
                        lp.Send($"{article} {Name} drops some items before their corpse is swallowed by the Winds of Magic!{Constants.NewLine}");
                    }
                }
            }
            NPCManager.Instance.RemoveNPCFromWorld(NPCGuid, this, CurrentRoom);
        }
    }

    internal class Player : Actor
    {
        [JsonProperty]
        internal Guid CombatSessionID { get; set; }
        [JsonProperty]
        internal bool ShowDetailedRollInfo { get; set; }
        [JsonProperty]
        internal List<Crafting.Recipe> KnownRecipes { get; set; }
        [JsonProperty]
        internal HashSet<Guid> CompletedQuests { get; set; }
        [JsonProperty]
        internal List<Quest> ActiveQuests { get; set; }
        internal Guid FollowerID { get; set; }
        [JsonProperty]
        internal uint BankBalance { get; set; }
        [JsonProperty]
        internal Dictionary<string,string> CommandAliases { get; set; }
        [JsonProperty]
        internal Languages KnownLanguages { get; set; }
        [JsonProperty]
        internal Languages SpokenLanguage { get; set; }
        internal bool IsInCombat => CombatSessionID != Guid.Empty;
        internal bool PVP;
        internal uint IdleTicks { get; set; }

        internal virtual bool Move(uint fromRoomId, uint destRoomId, bool wasTeleported, /*ref Descriptor desc,*/ bool bypassStamCheck = false)
        {
            try
            {
                var targetRoom = RoomManager.Instance.GetRoom(destRoomId);
                if (targetRoom == null)
                {
                    SessionManager.Instance.GetPlayer(Name).Send("Some mysterious force pushes you back... You cannot go that way!");
                    return true;
                }
                else
                {
                    if(!bypassStamCheck)
                    {
                        uint stamCost = 1;
                        if (RoomManager.Instance.GetRoom(fromRoomId).Flags.HasFlag(RoomFlags.HardTerrain))
                        {
                            stamCost += Helpers.RollDice(1, 4);
                        }
                        if (RoomManager.Instance.GetRoom(destRoomId).Flags.HasFlag(RoomFlags.HardTerrain))
                        {
                            stamCost += Helpers.RollDice(1, 4);
                        }
                        if (this.Stats.CurrentSP < stamCost)
                        {
                            SessionManager.Instance.GetPlayer(Name).Send($"You don't have the energy to move that far just now...{Constants.NewLine}");
                            return true;
                        }
                        this.Stats.CurrentSP -= stamCost;
                    }
                    var pDesc = SessionManager.Instance.GetPlayer(Name);
                    RoomManager.Instance.UpdatePlayersInRoom(fromRoomId, ref pDesc, true, wasTeleported, false, false);   // Player leaving a room
                    RoomManager.Instance.UpdatePlayersInRoom(destRoomId, ref pDesc, false, wasTeleported, false, false);  // Player arriving in a room
                    if(this.FollowerID != Guid.Empty)
                    {
                        var n = NPCManager.Instance.GetNPCByGUID(this.FollowerID);
                        if (n != null)
                        {
                            n.Move(ref n, fromRoomId, destRoomId, false);
                        }
                        else
                        {
                            this.FollowerID = Guid.Empty;
                        }
                    }
                    CurrentRoom = destRoomId;
                    RoomManager.Instance.ProcessEnvironmentBuffs(fromRoomId);
                    RoomManager.Instance.ProcessEnvironmentBuffs(destRoomId);
                    Room.DescribeRoom(ref pDesc, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error moving player {Name} from room {fromRoomId} to room {destRoomId}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool KnowsRecipe(string recipeName)
        {
            if(!string.IsNullOrEmpty(recipeName))
            {
                var r = (from kr in KnownRecipes where Regex.Match(kr.RecipeName, recipeName, RegexOptions.IgnoreCase).Success select kr).FirstOrDefault();
                return r != null;
            }
            return false;
        }

        internal void Kill(/*ref Descriptor descriptor*/)
        {
            Stats.CurrentHP = 0;
            if (Buffs != null && Buffs.Count > 0)
            {
                var buffNames = Buffs.Keys.ToList();
                foreach (var buffName in buffNames)
                {
                    RemoveBuff(buffName);
                }
            }
            Position = ActorPosition.Dead;
            CombatSessionID = Guid.Empty;
            uint xpLost = Stats.Exp > 3 ? Convert.ToUInt32(Stats.Exp * 0.1) : 0;
            Stats.Exp -= xpLost;
            //uint gp = descriptor.Player.Stats.Gold;
            uint gp = this.Stats.Gold;
            //RoomManager.Instance.AddGoldToRoom(descriptor.Player.CurrentRoom, gp);
            RoomManager.Instance.AddGoldToRoom(this.CurrentRoom, gp);
            //descriptor.Player.Stats.Gold = 0;
            this.Stats.Gold = 0;
            RoomManager.Instance.GetRoom(CurrentRoom).ItemsInRoom.AddRange(Inventory);
            Inventory.Clear();
            //Move(descriptor.Player.CurrentRoom, Constants.LimboRID(), true, ref descriptor);
            Move(this.CurrentRoom, Constants.LimboRID(), true, true);
        }

        internal void AddGold(uint gp/*, ref Descriptor desc*/)
        {
            uint totalGP = gp;
            if(HasSkill("Gold Digger"))
            {
                uint bonusGP = Convert.ToUInt32(gp * 0.5);
                SessionManager.Instance.GetPlayer(Name).Send($"Your skills allow you to find an extra {bonusGP} gold!{Constants.NewLine}");
                totalGP += bonusGP;
            }
            Stats.Gold += totalGP;
        }

        internal void AddExp(uint xp/*, ref Descriptor desc*/)
        {
            Stats.Exp += xp;
            if(Race == ActorRace.Human)
            {
                uint bonusXP = Convert.ToUInt32(xp * 0.25);
                SessionManager.Instance.GetPlayer(Name).Send($"Your Human nature grants you a bonus of {bonusXP} Exp!{Constants.NewLine}");
                Stats.Exp += bonusXP;
            }
            if(HasSkill("Quick Learner"))
            {
                uint bonusXP = Convert.ToUInt32(xp * 0.25);
                SessionManager.Instance.GetPlayer(Name).Send($"Your skills grant you a bonus of {bonusXP} Exp!{Constants.NewLine}");
                Stats.Exp += bonusXP;
            }
            if(LevelTable.HasCharAchievedNewLevel(Stats.Exp, Level, out uint newLevel))
            {
                // moving to level 1 causes newLevel to return 2?
                LevelUp(newLevel - Level);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void LevelUp(uint levelsToAdvance)
        {
            for(uint i = 0; i < levelsToAdvance; i++)
            {
                Level++;
                uint hpIncrease = 0;
                uint mpIncrease = 0;
                int hpMod;
                int mpMod = 0;
                switch(Class)
                {
                    case ActorClass.Cleric:
                        hpIncrease = Helpers.RollDice(1, 8);
                        mpIncrease = Helpers.RollDice(1, 8);
                        mpMod = (int)mpIncrease + ActorStats.CalculateAbilityModifier(Stats.Wisdom);
                        if (Level % 4 == 0)
                        {
                            Stats.Wisdom++;
                            Stats.Constitution++;
                        }
                        break;

                    case ActorClass.Fighter:
                        hpIncrease = Helpers.RollDice(1, 10);
                        mpIncrease = Helpers.RollDice(1, 4);
                        mpMod = (int)mpIncrease + ActorStats.CalculateAbilityModifier(Stats.Intelligence);
                        if (Level % 4 == 0)
                        {
                            Stats.Strength++;
                            Stats.Constitution++;
                        }
                        break;

                    case ActorClass.Thief:
                        hpIncrease = Helpers.RollDice(1, 6);
                        mpIncrease = Helpers.RollDice(1, 6);
                        mpMod = (int)mpIncrease + ActorStats.CalculateAbilityModifier(Stats.Intelligence);
                        if (Level % 4 == 0)
                        {
                            Stats.Dexterity++;
                            Stats.Intelligence++;
                        }
                        break;

                    case ActorClass.Wizard:
                        hpIncrease = Helpers.RollDice(1, 4);
                        mpIncrease = Helpers.RollDice(1, 10);
                        mpMod = (int)mpIncrease + ActorStats.CalculateAbilityModifier(Stats.Intelligence);
                        if (Level % 4 == 0)
                        {
                            Stats.Intelligence++;
                            Stats.Wisdom++;
                        }
                        break;
                }
                hpMod = (int)hpIncrease + ActorStats.CalculateAbilityModifier(Stats.Constitution);
                if (hpMod < 1)
                {
                    hpMod = 1;
                }
                if (mpMod < 1)
                {
                    mpMod = 1;
                }
                Stats.MaxHP += Convert.ToUInt32(hpMod);
                Stats.MaxMP += Convert.ToUInt32(mpMod);
                Stats.CurrentMaxHP += Convert.ToUInt32(hpMod);
                Stats.CurrentMaxMP += Convert.ToUInt32(mpMod);
                var stamIncrease = Convert.ToInt32(Helpers.RollDice(1,10) + ActorStats.CalculateAbilityModifier(Stats.Constitution));
                if(stamIncrease < 1)
                {
                    stamIncrease = 1;
                }
                Stats.MaxSP += Convert.ToUInt32(stamIncrease);
                Stats.CurrentSP += Convert.ToUInt32(stamIncrease);
            }
        }
    }

    internal class PlayerBuff
    {
        [JsonProperty]
        internal string BuffID { get; set; }
        [JsonProperty]
        internal string BuffName { get; set; }
        [JsonProperty]
        internal uint BuffDuration { get; set; }
        [JsonProperty]
        internal uint ElapsedTicks { get; set; }
        internal Buffs BuffEffect { get; set; }
    }

    [Serializable]
    internal class EquippedItems
    {
        [JsonProperty]
        internal InventoryItem Head { get; set; }
        [JsonProperty]
        internal InventoryItem Neck { get; set; }
        [JsonProperty]
        internal InventoryItem Armour { get; set; }
        [JsonProperty]
        internal InventoryItem FingerLeft { get; set; }
        [JsonProperty]
        internal InventoryItem FingerRight { get; set; }
        [JsonProperty]
        internal InventoryItem Weapon { get; set; }
        [JsonProperty]
        internal InventoryItem Held { get; set; }
    }
}
