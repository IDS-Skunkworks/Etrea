using Kingdoms_of_Etrea.Interfaces;
using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kingdoms_of_Etrea.Core
{
    internal class NPCManager
    {
        private static NPCManager _instance = null;
        private static readonly object _lockObject = new object();
        private ILoggingProvider _loggingProvider;
        private Dictionary<uint, NPC> NPCs { get; set; }
        private Dictionary<Guid, NPC> NPCIDs { get; set; }

        private NPCManager(ILoggingProvider loggingProvider)
        {
            _loggingProvider = loggingProvider;
            NPCs = new Dictionary<uint, NPC>();
            NPCIDs = new Dictionary<Guid, NPC>();
        }

        internal static NPCManager Instance
        {
            get
            {
                lock(_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new NPCManager(new LoggingProvider());
                    }
                    return _instance;
                }
            }
        }

        internal List<NPC> GetNPCsInRoom(uint rid)
        {
            List<NPC> result = new List<NPC>();
            lock(_lockObject)
            {
                result = Instance.NPCIDs.Values.Where(x => x.CurrentRoom == rid).ToList();
            }
            return result;
        }

        internal void SetNPCHealthToMax(Guid npcID)
        {
            if(Instance.NPCIDs.ContainsKey(npcID))
            {
                lock(_lockObject)
                {
                    Instance.NPCIDs[npcID].Stats.CurrentHP = (int)Instance.NPCIDs[npcID].Stats.MaxHP;
                }
            }
        }

        internal void AdjustNPCHealth(Guid NPCID, int amount)
        {
            if(Instance.NPCIDs.ContainsKey(NPCID))
            {
                lock(_lockObject)
                {
                    Instance.NPCIDs[NPCID].Stats.CurrentHP += amount;
                }
            }
        }

        internal void AdjustNPCMana(Guid NPCID, int amount)
        {
            if (Instance.NPCIDs.ContainsKey(NPCID))
            {
                lock (_lockObject)
                {
                    Instance.NPCIDs[NPCID].Stats.CurrentMP += amount;
                }
            }
        }

        internal void SetNPCFollowing(ref Descriptor desc, bool isFollowing)
        {
            var fid = desc.Player.FollowerID;
            lock(_lockObject)
            {
                if (Instance.NPCIDs.ContainsKey(desc.Player.FollowerID))
                {
                    Instance.NPCIDs[desc.Player.FollowerID].FollowingPlayer = isFollowing ? desc.Id : Guid.Empty;
                }
            }
        }

        internal Dictionary<Guid, NPC> GetAllNPCIDS()
        {
            return Instance.NPCIDs;
        }

        internal void LoadAllNPCs(out bool hasError)
        {
            var result = DatabaseManager.LoadAllNPCS(out hasError);
            if(!hasError && result != null && result.Count > 0)
            {
                Instance.NPCs.Clear();
                Instance.NPCs = result;
            }
        }

        internal int GetCountOfNPCsInWorld(uint npcID)
        {
            return Instance.NPCIDs.Where(x => x.Value.NPCID == npcID).Count();
        }

        internal bool UpdateNPC(ref Descriptor desc, NPC n)
        {
            try
            {
                if(Instance.NPCs.ContainsKey(n.NPCID))
                {
                    lock(_lockObject)
                    {
                        Instance.NPCs.Remove(n.NPCID);
                        Instance.NPCs.Add(n.NPCID, n);
                        _loggingProvider.LogMessage($"INFO: Player {desc.Player.Name} updated NPC with ID {n.NPCID} in the NPC Manager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    _loggingProvider.LogMessage($"WARN: NPC Manager does not contain an NPC with ID {n.NPCID}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance.NPCs.Add(n.NPCID, n);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                _loggingProvider.LogMessage($"ERROR: Error updating NPC {n.NPCID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveNPCByID(ref Descriptor desc, uint id)
        {
            try
            {
                if (Instance.NPCs.ContainsKey(id))
                {
                    lock(_lockObject)
                    {
                        Instance.NPCs.Remove(id);
                    }
                    _loggingProvider.LogMessage($"INFO: Player {desc.Player.Name} removed NPC ID {id} from the NPC Manager", LogLevel.Info, true);
                    return true;
                }
                _loggingProvider.LogMessage($"WARN: Player {desc.Player.Name} was unable to remove NPC ID {id} from the NPC Manager, the ID could not be found", LogLevel.Warning, true);
                return false;
            }
            catch(Exception ex)
            {
                _loggingProvider.LogMessage($"ERROR: Error removing NPC {id}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNPCToWorld(NPC n, uint rid)
        {
            try
            {
                n.NPCGuid = Guid.NewGuid();
                var hp = Helpers.RollDice(n.NumberOfHitDice, n.SizeOfHitDice);
                var mp = Helpers.RollDice(n.NumberOfHitDice, 8);
                var hpModifier = ActorStats.CalculateAbilityModifier(n.Stats.Constitution) * n.NumberOfHitDice;
                var mpModifier = ActorStats.CalculateAbilityModifier(n.Stats.Intelligence) * n.NumberOfHitDice;
                var hpFinal = (hpModifier + hp) <= 0 ? 1 : hpModifier += hp;
                var mpFinal = (mpModifier + mp) <= 0 ? 1 : mpModifier += mp;
                if (n.EquippedItems == null)
                {
                    n.EquippedItems = new EquippedItems();
                }
                n.Stats.MaxHP = Convert.ToUInt32(hpFinal);
                n.Stats.CurrentHP = (int)hpFinal;
                n.Stats.MaxMP = Convert.ToUInt32(mpFinal);
                n.Stats.CurrentMP = (int)mpFinal;
                n.CurrentRoom = rid;
                NPCManager.Instance.GetNPCsInRoom(rid).Add(n);
                n.CalculateArmourClass();
                lock (_lockObject)
                {
                    NPCIDs.Add(n.NPCGuid, n);
                    _loggingProvider.LogMessage($"INFO: Added {n.Name} to room {rid}", LogLevel.Info, true);
                }
                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(rid);
                if (playersToNotify != null && playersToNotify.Count > 0)
                {
                    foreach (var p in playersToNotify)
                    {
                        var article = Helpers.IsCharAVowel(n.Name[0]) ? "an" : "a";
                        p.Send($"{Constants.NewLine}The Winds of Magic swirl and give life to {article} {n.Name}!{Constants.NewLine}");
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                _loggingProvider.LogMessage($"ERROR: Error adding NPC to world: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNPCToWorld(uint npcID, uint rid)
        {
            try
            {
                var newNPC = Instance.NPCs[npcID].ShallowCopy();
                NPC toAdd = null;
                using (var ms = new MemoryStream())
                {
                    IFormatter f = new BinaryFormatter();
                    f.Serialize(ms, newNPC);
                    ms.Seek(0, SeekOrigin.Begin);
                    toAdd = (NPC)f.Deserialize(ms);
                }
                toAdd.NPCGuid = Guid.NewGuid();

                var hp = Helpers.RollDice(toAdd.NumberOfHitDice, toAdd.SizeOfHitDice);
                var mp = Helpers.RollDice(toAdd.NumberOfHitDice, 8);
                var hpModifier = ActorStats.CalculateAbilityModifier(toAdd.Stats.Constitution) * toAdd.NumberOfHitDice;
                var mpModifier = ActorStats.CalculateAbilityModifier(toAdd.Stats.Intelligence) * toAdd.NumberOfHitDice;
                var hpFinal = (hpModifier + hp) <= 0 ? 1 : hpModifier += hp;
                var mpFinal = (mpModifier + mp) <= 0 ? 1 : mpModifier += mp;
                if(toAdd.EquippedItems == null)
                {
                    toAdd.EquippedItems = new EquippedItems();
                }
                toAdd.Stats.MaxHP = (uint)hpFinal;
                toAdd.Stats.CurrentHP = (int)hpFinal;
                toAdd.Stats.MaxMP = (uint)mpFinal;
                toAdd.Stats.CurrentMP = (int)mpFinal;
                toAdd.CurrentRoom = rid;
                toAdd.CalculateArmourClass();
                lock(_lockObject)
                {
                    NPCIDs.Add(toAdd.NPCGuid, toAdd);
                    _loggingProvider.LogMessage($"INFO: Added {toAdd.Name} to room {rid}", LogLevel.Info, true);
                }
                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(rid);
                if(playersToNotify != null &&  playersToNotify.Count > 0)
                {
                    var article = Helpers.IsCharAVowel(toAdd.Name[0]) ? "an" : "a";
                    foreach(var p in playersToNotify)
                    {
                        p.Send($"{Constants.NewLine}The Winds of Magic swirl and give life to {article} {toAdd.Name}!{Constants.NewLine}");
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                _loggingProvider.LogMessage($"ERROR: Error adding NPC: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal NPC GetHirelingNPC(ref Descriptor desc, string npcType)
        {
            NPC hireling = new NPC();
            hireling.NumberOfAttacks = desc.Player.Level <= 5 ? desc.Player.Level : 5;
            hireling.NumberOfHitDice = desc.Player.Level;
            hireling.Skills = new List<Skills.Skill>();
            hireling.Spells = new List<Spells.Spell>();
            hireling.Inventory = new List<InventoryItem>();
            hireling.EquippedItems = new EquippedItems();
            hireling.FollowingPlayer = desc.Id;
            hireling.Stats.Gold = 100;
            hireling.Type = ActorType.NonPlayer;
            hireling.Level = desc.Player.Level;
            hireling.Position = ActorPosition.Standing;
            hireling.Race = ActorRace.Human;
            var rnd = new Random(DateTime.Now.GetHashCode());
            hireling.Name = HirelingNames[rnd.Next(HirelingNames.Count)];
            hireling.ShortDescription = $"{desc.Player}'s follower";
            hireling.ArrivalMessage = $"{hireling.Name} arrives, following {desc.Player}";
            hireling.DepartMessage = $"{hireling.Name} leaves, following {desc.Player}";
            if(Regex.Match(npcType, "fighter", RegexOptions.IgnoreCase).Success)
            {
                hireling.Title = "Fighter";
                hireling.SizeOfHitDice = 10;
                hireling.Stats.Strength = Helpers.RollDice(3, 6) + 2;
                hireling.Stats.Dexterity = Helpers.RollDice(3, 6);
                hireling.Stats.Intelligence = Helpers.RollDice(3, 6);
                hireling.Stats.Constitution = Helpers.RollDice(3, 6) + 2;
                hireling.Stats.Wisdom = Helpers.RollDice(3, 6);
                hireling.Stats.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Fighter;
                hireling.AddSkill("Desperate Attack");
                hireling.AddSkill("Awareness");
                hireling.AddSkill("Parry");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            if (Regex.Match(npcType, "mage", RegexOptions.IgnoreCase).Success)
            {
                hireling.Title = "Sorceror";
                hireling.SizeOfHitDice = 4;
                hireling.Stats.Strength = Helpers.RollDice(3, 6);
                hireling.Stats.Dexterity = Helpers.RollDice(3, 6);
                hireling.Stats.Intelligence = Helpers.RollDice(3, 6) + 4;
                hireling.Stats.Constitution = Helpers.RollDice(3, 6);
                hireling.Stats.Wisdom = Helpers.RollDice(3, 6);
                hireling.Stats.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Wizard;
                hireling.AddSpell("Magic Missile");
                hireling.AddSpell("Truestrike");
                hireling.AddSpell("Acid Arrow");
                hireling.AddSpell("Firebolt");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            if (Regex.Match(npcType, "thief", RegexOptions.IgnoreCase).Success)
            {
                hireling.Title = "Thief";
                hireling.SizeOfHitDice = 6;
                hireling.Stats.Strength = Helpers.RollDice(3, 6);
                hireling.Stats.Dexterity = Helpers.RollDice(3, 6) + 4;
                hireling.Stats.Intelligence = Helpers.RollDice(3, 6);
                hireling.Stats.Constitution = Helpers.RollDice(3, 6);
                hireling.Stats.Wisdom = Helpers.RollDice(3, 6);
                hireling.Stats.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Thief;
                hireling.AddSkill("Awareness");
                hireling.AddSkill("Parry");
                hireling.AddSkill("Dodge");
                hireling.AddSkill("Backstab");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            if (Regex.Match(npcType, "priest", RegexOptions.IgnoreCase).Success)
            {
                hireling.Title = "Cleric";
                hireling.SizeOfHitDice = 8;
                hireling.Stats.Strength = Helpers.RollDice(3, 6) + 1;
                hireling.Stats.Dexterity = Helpers.RollDice(3, 6) + 1;
                hireling.Stats.Intelligence = Helpers.RollDice(3, 6) + 1;
                hireling.Stats.Constitution = Helpers.RollDice(3, 6);
                hireling.Stats.Wisdom = Helpers.RollDice(3, 6) + 1;
                hireling.Stats.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Cleric;
                hireling.AddSpell("Cure Light Wounds");
                hireling.AddSpell("Cure Moderate Wounds");
                hireling.AddSpell("Regen");
                hireling.AddSpell("Fae Fire");
                hireling.AddSpell("Firebolt");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            return null;
        }

        internal bool MoveNPCToNewRID(Guid npcid, uint rid)
        {
            try
            {
                lock(_lockObject)
                {
                    if(Instance.NPCIDs.ContainsKey(npcid))
                    {
                        Instance.NPCIDs[npcid].CurrentRoom = rid;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal NPC GetNPCByGUID(Guid npcGuid)
        {
            try
            {
                lock(_lockObject)
                {
                    if(Instance.NPCIDs.ContainsKey(npcGuid))
                    {
                        return Instance.NPCIDs[npcGuid];
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        internal List<NPC> GetNPCsForZone(uint zoneId)
        {
            var retval = new List<NPC>();
            retval.AddRange((from x in Instance.NPCs where x.Value.AppearsInZone == zoneId select x.Value).ToList());
            return retval;
        }

        internal bool RemoveNPCFromWorld(Guid g, NPC npcid, uint rid)
        {
            try
            {
                lock(_lockObject)
                {
                    if(Instance.NPCIDs.ContainsKey(g))
                    {
                        Instance.NPCIDs.Remove(g);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool AddNewNPC(NPC n)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.NPCs.Add(n.NPCID, n);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal List<NPC> GetNPCByIDRange(uint start, uint end)
        {
            var retval = NPCs.Values.Where(x => x.NPCID >= start && x.NPCID <= end).ToList();
            return retval;
        }

        internal List<NPC> GetNPCByNameOrDescription(string criteria)
        {
            var retval = new List<NPC>();
            retval.AddRange(from NPC n in Instance.NPCs.Values
                            where Regex.Match(n.Name, criteria, RegexOptions.IgnoreCase).Success
                            || Regex.Match(n.ShortDescription, criteria, RegexOptions.IgnoreCase).Success
                            || Regex.Match(n.LongDescription, criteria, RegexOptions.IgnoreCase).Success
                            select n);
            return retval;
        }

        internal int GetNPCCount()
        {
            return Instance.NPCs.Count;
        }

        internal NPC GetNPCByID(uint id)
        {
            if(Instance.NPCs.ContainsKey(id))
            {
                return Instance.NPCs[id];
            }
            return null;
        }

        internal Dictionary<uint, NPC> GetAllNPCs()
        {
            return Instance.NPCs;
        }

        internal bool NPCExists(uint id)
        {
            return Instance.NPCs.ContainsKey(id);
        }

        private static List<string> HirelingNames = new List<string>
        {
            "Denholm",
            "Aluria",
            "Meriel",
            "Erykah",
            "Bronson",
            "Caetlin",
            "Bishop",
            "Gael",
            "Vincent",
            "Tammy",
            "Viktoria",
            "Kirsty",
            "Clint",
            "Huxley",
            "Rayf",
            "Clifford",
            "Tabitha",
            "Eldred",
            "Petra",
            "Stephanie",
            "Thane",
            "Felicity"
        };
    }
}
