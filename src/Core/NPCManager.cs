using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class NPCManager
    {
        private static NPCManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, NPC> _NPCTemplates { get; set; }
        private Dictionary<Guid, NPC> _NPCInstances { get; set; }

        private NPCManager()
        {
            _NPCTemplates = new Dictionary<uint, NPC>();
            _NPCInstances = new Dictionary<Guid, NPC>();
        }

        internal static NPCManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new NPCManager();
                    }
                    return _instance;
                }
            }
        }

        internal List<NPC> GetNPCsForZone(uint zoneId)
        {
            try
            {
                lock (_lock)
                {
                    return (from x in Instance._NPCTemplates.Values where x.AppearsInZone == zoneId select x).ToList();
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error looking up NPCs for Zone {zoneId}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal List<NPC> GetNPCByNameOrDescription(string name)
        {
            lock(_lock)
            {
                return Instance._NPCTemplates.Values.Where(x => Regex.IsMatch(x.Name, name, RegexOptions.IgnoreCase)
                || Regex.IsMatch(x.ShortDescription, name, RegexOptions.IgnoreCase) || Regex.IsMatch(x.LongDescription, name, RegexOptions.IgnoreCase)).ToList();
            }
        }

        internal NPC GetNPCByGUID(Guid guid)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._NPCInstances.ContainsKey(guid))
                    {
                        return Instance._NPCInstances[guid];
                    }
                }
                return null;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error finding NPC Instance {guid}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal NPC GetNPCByID(uint id)
        {
            lock (_lock)
            {
                if (Instance._NPCTemplates.ContainsKey(id))
                {
                    return Instance._NPCTemplates[id];
                }
            }
            return null;
        }

        internal List<NPC> GetNPCByIDRange(uint start, uint end)
        {
            lock(_lock)
            {
                return Instance._NPCTemplates.Values.Where(x => x.NPCID >= start && x.NPCID <= end).ToList();
            }
        }

        internal List<NPC> GetNPCsInRoom(uint rid)
        {
            lock ( _lock)
            {
                return Instance._NPCInstances.Values.Where(x => x.CurrentRoom == rid).ToList();
            }
        }

        internal void SetNPCFollowing(ref Descriptor desc, bool isFollowing)
        {
            var fid = desc.Player.FollowerID;
            lock (_lock)
            {
                if (Instance._NPCInstances.ContainsKey(fid))
                {
                    Instance._NPCInstances[fid].FollowingPlayer = isFollowing ? desc.ID : Guid.Empty;
                }
            }
        }

        internal Dictionary<Guid, NPC> GetAllNPCInstances()
        {
            lock(_lock)
            {
                return Instance._NPCInstances;
            }
        }

        internal void LoadAllNPCs(out bool hasError)
        {
            var result = DatabaseManager.LoadAllNPCS(out hasError);
            if (!hasError && result != null)
            {
                Instance._NPCTemplates.Clear();
                Instance._NPCTemplates = result;
            }
        }

        internal int GetCountOfNPCsInWorld(uint npcID)
        {
            lock( _lock)
            {
                return Instance._NPCInstances.Where(x => x.Value.NPCID == npcID).Count();
            }
        }

        internal int GetNPCTemplateCount()
        {
            lock (_lock)
            {
                return Instance._NPCTemplates.Count;
            }
        }

        internal bool NPCExists(uint npcID)
        {
            lock (_lock)
            {
                return Instance._NPCTemplates.ContainsKey(npcID);
            }
        }

        internal bool AddNPC(ref Descriptor desc, NPC n)
        {
            try
            {
                lock (_lock)
                {
                    Instance._NPCTemplates.Add(n.NPCID, n);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} has added NPC {n.NPCID} ({n.Name}) to NPCManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encounterd an error adding NPC {n.NPCID} ({n.Name}) to NPCManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateNPC(ref Descriptor desc, NPC n)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._NPCTemplates.ContainsKey(n.NPCID))
                    {
                        Instance._NPCTemplates.Remove(n.NPCID);
                        Instance._NPCTemplates.Add(n.NPCID, n);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} has updated NPC {n.NPCID} ({n.Name}) in NPCManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: NPCManager does not contain an NPC with ID {n.NPCID} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddNPC(ref desc, n);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating NPC {n.NPCID} ({n.Name}) in NPCManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveNPC(ref Descriptor desc, uint npcID, string npcName)
        {
            try
            {
                lock (_lock)
                {
                    Instance._NPCTemplates.Remove(npcID);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} remove NPC {npcID} ({npcName}) from NPCManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing NPC {npcID} ({npcName}) from NCPManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveNPCFromWorld(Guid npcGUID)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._NPCInstances.ContainsKey(npcGUID))
                    {
                        Instance._NPCInstances.Remove(npcGUID);
                        Game.LogMessage($"INFO: NPC Instance {npcGUID} was removed from the world", LogLevel.Info, false);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing NPC Instance {npcGUID} from the world: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool MoveNPCToNewRID(Guid npcID, uint targetRID)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._NPCInstances.ContainsKey(npcID))
                    {
                        Instance._NPCInstances[npcID].CurrentRoom = targetRID;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating RID of NPC Instance {npcID} to {targetRID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNPCToWorld(NPC n, uint rid)
        {
            try
            {
                n.NPCGuid = Guid.NewGuid();
                var hp = Helpers.RollDice(n.NumberOfHitDice, n.HitDieSize);
                var mp = Helpers.RollDice(n.NumberOfHitDice, 8);
                var hpModifier = Helpers.CalculateAbilityModifier(n.Constitution) * n.NumberOfHitDice;
                var mpModifier = Helpers.CalculateAbilityModifier(n.Intelligence) * n.NumberOfHitDice;
                var hpFinal = (hpModifier + hp) <= 0 ? 1 : hpModifier += hp;
                var mpFinal = (mpModifier + mp) <= 0 ? 1 : mpModifier += mp;
                n.MaxHP = (int)hpFinal;
                n.CurrentHP = (int)hpFinal;
                n.MaxMP = (int)mpFinal;
                n.CurrentMP = (int)mpFinal;
                n.CurrentRoom = rid;
                n.CalculateArmourClass();
                lock (_lock)
                {
                    Instance._NPCInstances.Add(n.NPCGuid, n);
                    Game.LogMessage($"INFO: Added NPC {n.Name} to Room {rid}", LogLevel.Info, true);
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
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding NPC {n.Name} to Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNPCToWorld(uint npcID, uint rid)
        {
            try
            {
                var newNPC = Instance._NPCTemplates[npcID].ShallowCopy();
                NPC toAdd = null;
                using (var ms = new MemoryStream())
                {
                    IFormatter f = new BinaryFormatter();
                    f.Serialize(ms, newNPC);
                    ms.Seek(0, SeekOrigin.Begin);
                    toAdd = (NPC)f.Deserialize(ms);
                }
                toAdd.NPCGuid = Guid.NewGuid();

                var hp = Helpers.RollDice(toAdd.NumberOfHitDice, toAdd.HitDieSize);
                var mp = Helpers.RollDice(toAdd.NumberOfHitDice, 10);
                var hpModifier = Helpers.CalculateAbilityModifier(toAdd.Constitution) * toAdd.NumberOfHitDice;
                var mpModifier = Helpers.CalculateAbilityModifier(toAdd.Intelligence) * toAdd.NumberOfHitDice;
                var hpFinal = (hpModifier + hp) <= 0 ? 1 : hpModifier += hp;
                var mpFinal = (mpModifier + mp) <= 0 ? 1 : mpModifier += mp;
                toAdd.MaxHP = (int)hpFinal;
                toAdd.CurrentHP = (int)hpFinal;
                toAdd.MaxMP = (int)mpFinal;
                toAdd.CurrentMP = (int)mpFinal;
                toAdd.CurrentRoom = rid;
                toAdd.CalculateArmourClass();
                lock (_lock)
                {
                    Instance._NPCInstances.Add(toAdd.NPCGuid, toAdd);
                    Game.LogMessage($"INFO: Added NPC {toAdd.Name} to Room {rid}", LogLevel.Info, true);
                }
                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(rid);
                if (playersToNotify != null && playersToNotify.Count > 0)
                {
                    var article = Helpers.IsCharAVowel(toAdd.Name[0]) ? "an" : "a";
                    foreach (var p in playersToNotify)
                    {
                        p.Send($"{Constants.NewLine}The Winds of Magic swirl and give life to {article} {toAdd.Name}!{Constants.NewLine}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding NPC {npcID} to Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal NPC GetHirelingNPC(ref Descriptor desc, string npcType)
        {
            NPC hireling = new NPC();
            hireling.NumberOfAttacks = (uint)Math.Round((double)desc.Player.Level / 2, 0) <= 5 ? (uint)Math.Round((double)desc.Player.Level / 2, 0) : 5;
            if (hireling.NumberOfAttacks < 1)
            {
                // Ensure hireling NPCs always have at least one attack per round
                hireling.NumberOfAttacks = 1;
            }
            hireling.Skills = new List<Skill>();
            hireling.Spells = new List<Spell>();
            hireling.Inventory = new List<InventoryItem>();
            hireling.FollowingPlayer = desc.ID;
            hireling.Gold = 100;
            hireling.ActorType = ActorType.NonPlayer;
            hireling.Level = desc.Player.Level;
            hireling.Position = ActorPosition.Standing;
            hireling.Race = ActorRace.Human;
            var rnd = new Random(DateTime.Now.GetHashCode());
            hireling.Name = HirelingNames[rnd.Next(HirelingNames.Count)];
            hireling.ShortDescription = $"{desc.Player}'s follower";
            hireling.ArrivalMessage = $"{hireling.Name} arrives, following {desc.Player}";
            hireling.DepartureMessage = $"{hireling.Name} leaves, following {desc.Player}";
            if (Regex.IsMatch(npcType, "fighter", RegexOptions.IgnoreCase))
            {
                hireling.Title = "Fighter";
                hireling.HitDieSize = 10;
                hireling.Strength = Helpers.RollDice(3, 6) + 2;
                hireling.Dexterity = Helpers.RollDice(3, 6);
                hireling.Intelligence = Helpers.RollDice(3, 6);
                hireling.Constitution = Helpers.RollDice(3, 6) + 2;
                hireling.Wisdom = Helpers.RollDice(3, 6);
                hireling.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Fighter;
                hireling.AddSkill("Desperate Attack");
                hireling.AddSkill("Awareness");
                hireling.AddSkill("Parry");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            if (Regex.IsMatch(npcType, "mage", RegexOptions.IgnoreCase))
            {
                hireling.Title = "Sorceror";
                hireling.HitDieSize = 4;
                hireling.Strength = Helpers.RollDice(3, 6);
                hireling.Dexterity = Helpers.RollDice(3, 6);
                hireling.Intelligence = Helpers.RollDice(3, 6) + 4;
                hireling.Constitution = Helpers.RollDice(3, 6);
                hireling.Wisdom = Helpers.RollDice(3, 6);
                hireling.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Wizard;
                hireling.AddSpell("Magic Missile");
                hireling.AddSpell("Truestrike");
                hireling.AddSpell("Acid Arrow");
                hireling.AddSpell("Firebolt");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            if (Regex.IsMatch(npcType, "thief", RegexOptions.IgnoreCase))
            {
                hireling.Title = "Thief";
                hireling.HitDieSize = 6;
                hireling.Strength = Helpers.RollDice(3, 6);
                hireling.Dexterity = Helpers.RollDice(3, 6) + 4;
                hireling.Intelligence = Helpers.RollDice(3, 6);
                hireling.Constitution = Helpers.RollDice(3, 6);
                hireling.Wisdom = Helpers.RollDice(3, 6);
                hireling.Charisma = Helpers.RollDice(3, 6);
                hireling.Class = ActorClass.Thief;
                hireling.AddSkill("Awareness");
                hireling.AddSkill("Parry");
                hireling.AddSkill("Dodge");
                hireling.AddSkill("Backstab");
                hireling.LongDescription = $"{hireling.Title} {hireling.Name} is {desc.Player}'s follower!";
                return hireling;
            }
            if (Regex.IsMatch(npcType, "priest", RegexOptions.IgnoreCase))
            {
                hireling.Title = "Cleric";
                hireling.HitDieSize = 8;
                hireling.Strength = Helpers.RollDice(3, 6) + 1;
                hireling.Dexterity = Helpers.RollDice(3, 6) + 1;
                hireling.Intelligence = Helpers.RollDice(3, 6) + 1;
                hireling.Constitution = Helpers.RollDice(3, 6);
                hireling.Wisdom = Helpers.RollDice(3, 6) + 1;
                hireling.Charisma = Helpers.RollDice(3, 6);
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
            "Felicity",
            "Bartram"
        };
    }
}
