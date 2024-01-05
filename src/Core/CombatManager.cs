using Kingdoms_of_Etrea.Interfaces;
using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Kingdoms_of_Etrea.Core
{
    internal class CombatManager
    {
        private static CombatManager _instnace = null;
        private static readonly object _lockObject = new object();
        private static ILoggingProvider _loggingProvider = new LoggingProvider();
        private Dictionary<Guid, CombatSessionNew> CombatQueue;

        private CombatManager()
        {
            CombatQueue = new Dictionary<Guid, CombatSessionNew>();
        }

        internal static CombatManager Instance
        {
            get
            {
                lock(_lockObject)
                {
                    if (_instnace == null)
                    {
                        _instnace = new CombatManager();
                    }
                    return _instnace;
                }
            }
        }

        internal bool IsPlayerInCombat(Guid playerID)
        {
            if(CombatQueue.Count > 0)
            {
                lock(_lockObject)
                {
                    foreach(var _ in from s in CombatQueue.Values
                                     where s.Attacker is Descriptor
                                     where (s.Attacker as Descriptor).Id == playerID
                                     select new { })
                    {
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal bool IsNPCInCombatNew(Guid npcID)
        {
            if(CombatQueue.Count > 0)
            {
                lock(_lockObject)
                {
                    foreach (var _ in from s in CombatQueue.Values
                                      where s.Attacker is NPC
                                      where (s.Attacker as NPC).NPCGuid == npcID
                                      select new { })
                    {
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal bool IsPlayerInCombatWithNPC(Guid playerID, Guid npcID)
        {
            if (CombatQueue.Count > 0)
            {
                lock(_lockObject)
                {
                    foreach(var _ in from s in CombatQueue.Values
                                     where s.Attacker is Descriptor
                                     where s.Defender is NPC
                                     where (s.Attacker as Descriptor).Id == playerID && (s.Defender as NPC).NPCGuid == npcID
                                     select new { })
                    {
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal List<Guid> GetCombatSessionsForCombattant(Guid combatantID)
        {
            var retval = new List<Guid>();
            if (CombatQueue.Count > 0)
            {
                lock(_lockObject)
                {
                    var matchingSessions = CombatQueue.Values.Where(x => x.AttackerID == combatantID || x.DefenderID == combatantID).Select(x => x.SessionID).ToList();
                    retval.AddRange(matchingSessions);
                }
            }
            return retval;
        }

        internal List<Guid> GetCombatSessionsForCombatantPairing(Guid attackerID, Guid defenderID)
        {
            var retval = new List<Guid>();
            if (CombatQueue.Count > 0)
            {
                lock(_lockObject)
                {
                    var matchingSessions = CombatQueue.Values.Where(x => x.AttackerID == attackerID && x.DefenderID == defenderID).Select(x => x.SessionID).ToList();
                    retval.AddRange(matchingSessions);
                    matchingSessions = CombatQueue.Values.Where(x => x.AttackerID == defenderID && x.DefenderID == attackerID).Select(x => x.SessionID).ToList();
                    retval.AddRange(matchingSessions);
                }
            }
            return retval;
        }

        //internal Guid GetCombatSessionForNPC(Guid npcID)
        //{
        //    Guid retval = Guid.Empty;
        //    if(CombatQueue.Count > 0)
        //    {
        //        lock(_lockObject)
        //        {
        //            var matchingSessions = CombatQueue.Where(kv => kv.Value.Participants.Any(s => s.Attacker is NPC && (((NPC)s.Attacker).NPCGuid == npcID) || ((NPC)s.Target).NPCGuid == npcID)).ToList();
        //            if(matchingSessions.Count == 1)
        //            {
        //                retval = matchingSessions.First().Key;
        //            }
        //            else
        //            {
        //                Game.LogMessage($"DEBUG: Request to get Combat Session for {npcID} returned {matchingSessions.Count} sessions when 1 was expected", LogLevel.Debug, true);
        //            }
        //        }
        //    }
        //    return retval;
        //}

        //internal Guid GetNPCGuidFromCombatSession(Guid sessionID)
        //{
        //    Guid retval = Guid.Empty;
        //    if(CombatQueue.Count > 0 )
        //    {
        //        lock(_lockObject)
        //        {
        //            if(CombatQueue.ContainsKey(sessionID))
        //            {
        //                var participant = CombatQueue[sessionID].Participants.Where(p => p.Attacker is NPC).FirstOrDefault();
        //                return (participant.Attacker as NPC).NPCGuid;
        //            }
        //        }
        //    }
        //    return retval;
        //}

        internal void ProcessCombatQueue(out List<Guid> completedSessions)
        {
            completedSessions = new List<Guid>();
            foreach(var session in CombatQueue.Values)
            {
                bool sessionComplete = false;
                var attacker = session.Attacker;
                var defender = session.Defender;
                if (attacker == null || defender == null)
                {
                    Game.LogMessage($"WARN: Combat Manager is aborting session {session.SessionID}: attacker and/or defender was null", LogLevel.Warning, true);
                    completedSessions.Add(session.SessionID);
                    sessionComplete = true;
                    break;
                }
                bool attackerIsPlayer = session.Attacker is Descriptor;
                bool defenderIsPlayer = session.Defender is Descriptor;
                // Check the HP of both attacker and defender in case the values were modified outside of CombatManager
                if(attackerIsPlayer)
                {
                    var attackerHP = SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.Stats.CurrentHP;
                    if (attackerHP <= 0)
                    {
                        completedSessions.Add(session.SessionID);
                        sessionComplete = true;
                        break;
                    }
                }
                else
                {
                    var attackerHP = NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid) != null ? NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid).Stats.CurrentHP : 0;
                    if (attackerHP <= 0)
                    {
                        completedSessions.Add(session.SessionID);
                        sessionComplete = true;
                        break;
                    }
                }
                if (defenderIsPlayer)
                {
                    var defenderHP = SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP;
                    if (defenderHP <= 0)
                    {
                        completedSessions.Add(session.SessionID);
                        sessionComplete = true;
                        break;
                    }
                }
                else
                {
                    var defenderHP = NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP;
                    if (defenderHP <= 0)
                    {
                        completedSessions.Add(session.SessionID);
                        sessionComplete = true;
                        break;
                    }
                }
                if (attackerIsPlayer)
                {
                    // Attacker is a player
                    bool showRollInfo = ((Descriptor)attacker).Player.ShowDetailedRollInfo || ((Descriptor)attacker).Player.Level >= Constants.ImmLevel;
                    for (int a = 0; a < ((Descriptor)attacker).Player.NumberOfAttacks; a++)
                    {
                        var targetAC = defenderIsPlayer ? ((Descriptor)defender).Player.Stats.ArmourClass : ((NPC)defender).Stats.ArmourClass;
                        var targetName = defenderIsPlayer ? ((Descriptor)defender).Player.Name : ((NPC)defender).Name;
                        var hitRoll = Helpers.RollDice(1, 20);
                        bool fumble = hitRoll == 1;
                        bool critical = hitRoll == 20;
                        InventoryItem weapon = null;
                        if (((Descriptor)attacker).Player.EquippedItems != null && ((Descriptor)attacker).Player.EquippedItems.Weapon != null)
                        {
                            weapon = ((Descriptor)attacker).Player.EquippedItems.Weapon;
                        }
                        if (!fumble && !critical)
                        {
                            // Normal attack, not a natural 1 or 20
                            if (weapon != null)
                            {
                                var damage = Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice);
                                var ability = weapon.IsFinesse ? ((Descriptor)attacker).Player.Stats.Dexterity : ((Descriptor)attacker).Player.Stats.Strength;
                                var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                var hitRollFinal = Convert.ToInt32(hitRoll + abilityModifier);
                                var finalDamage = damage + abilityModifier > 0 ? Convert.ToUInt32(damage + abilityModifier) : 1;
                                if (((Descriptor)attacker).Player.HasBuff("Truestrike"))
                                {
                                    hitRollFinal += 10;
                                }
                                if (((Descriptor)attacker).Player.HasBuff("Desperate Attack"))
                                {
                                    hitRollFinal -= 4;
                                    finalDamage += 4;
                                }
                                if (hitRollFinal >= targetAC)
                                {
                                    // attack landed
                                    if (showRollInfo)
                                    {
                                        ((Descriptor)attacker).Send($"You rolled {hitRoll} ({hitRollFinal}) and hit {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        ((Descriptor)attacker).Send($"Your {weapon.Name} hits {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                    }
                                    if (defenderIsPlayer)
                                    {
                                        // apply damage to player
                                        if (((Descriptor)defender).Player.Stats.CurrentHP <= finalDamage)
                                        {
                                            // kill the player
                                            sessionComplete = true;
                                            ((Descriptor)attacker).Send($"You have killed {targetName}!{Constants.NewLine}");
                                            ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} has killed you!{Constants.NewLine}");
                                            ((Descriptor)defender).Player.Kill();
                                        }
                                        else
                                        {
                                            // haven't done enough damage to kill the player, so just reduce their HP
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                            ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name}'s {weapon.Name} hits you for {finalDamage} damage!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        // apply damage to the NPC
                                        if (((NPC)defender).Stats.CurrentHP <= finalDamage)
                                        {
                                            // Kill the NPC
                                            sessionComplete = true;
                                            ((Descriptor)attacker).Send($"You have killed {targetName} and gained {((NPC)defender).BaseExpAward} Exp and {((NPC)defender).Stats.Gold} gold!{Constants.NewLine}");
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddExp(((NPC)defender).BaseExpAward);
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddGold(((NPC)defender).Stats.Gold);
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.UpdateAlignment(((NPC)defender).Alignment);
                                            // Check and update any quests that might depend on killing this NPC
                                            var npcID = ((NPC)defender).NPCID;
                                            if (((Descriptor)attacker).Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(npcID)))
                                            {
                                                for (int q = 0; q < ((Descriptor)attacker).Player.ActiveQuests.Count; q++)
                                                {
                                                    if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters.Keys.Contains(npcID))
                                                    {
                                                        if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] <= 1)
                                                        {
                                                            ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] = 0;
                                                        }
                                                        else
                                                        {
                                                            ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID]--;
                                                        }
                                                    }
                                                }
                                            }
                                            ((NPC)defender).Kill(true);
                                        }
                                        else
                                        {
                                            // reduce NPC HP
                                            NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                        }
                                    }
                                }
                                else
                                {
                                    // attack missed
                                    if (showRollInfo)
                                    {
                                        ((Descriptor)attacker).Send($"You rolled {hitRoll} ({hitRollFinal}) and missed {targetName}!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        ((Descriptor)attacker).Send($"Your {weapon.Name} misses {targetName}!{Constants.NewLine}");
                                    }
                                    if (defenderIsPlayer)
                                    {
                                        ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} misses you with their {weapon.Name}!{Constants.NewLine}");
                                    }
                                }
                            }
                            else
                            {
                                // No weapon equipped by the attacker
                                var damage = Helpers.RollDice(1, 2); // base damage for a punch
                                var abilityModifier = ActorStats.CalculateAbilityModifier(((Descriptor)attacker).Player.Stats.Strength);
                                var hitRollFinal = Convert.ToInt32(hitRoll + abilityModifier);
                                var finalDamage = damage + abilityModifier > 0 ? Convert.ToUInt32(damage + abilityModifier) : 1;
                                if (hitRollFinal >= targetAC)
                                {
                                    // attack landed
                                    if (showRollInfo)
                                    {
                                        ((Descriptor)attacker).Send($"You rolled {hitRoll} ({hitRollFinal}) and hit {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        ((Descriptor)attacker).Send($"Your strike hits {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                    }
                                    if (defenderIsPlayer)
                                    {
                                        // apply damage to player
                                        if (((Descriptor)defender).Player.Stats.CurrentHP <= finalDamage)
                                        {
                                            // kill the player
                                            sessionComplete = true;
                                            ((Descriptor)attacker).Send($"You have killed {targetName}!{Constants.NewLine}");
                                            ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} has killed you!{Constants.NewLine}");
                                            ((Descriptor)defender).Player.Kill();
                                        }
                                        else
                                        {
                                            // haven't done enough damage to kill the player, so just reduce their HP
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                            ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name}'s strike hits you for {finalDamage} damage!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        // apply damage to the NPC
                                        if (((NPC)defender).Stats.CurrentHP <= finalDamage)
                                        {
                                            // Kill the NPC
                                            sessionComplete = true;
                                            ((Descriptor)attacker).Send($"You have killed {targetName} and gained {((NPC)defender).BaseExpAward} Exp and {((NPC)defender).Stats.Gold} gold!{Constants.NewLine}");
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddExp(((NPC)defender).BaseExpAward);
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddGold(((NPC)defender).Stats.Gold);
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.UpdateAlignment(((NPC)defender).Alignment);
                                            // Check and update any quests that might depend on killing this NPC
                                            var npcID = ((NPC)defender).NPCID;
                                            if (((Descriptor)attacker).Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(npcID)))
                                            {
                                                for (int q = 0; q < ((Descriptor)attacker).Player.ActiveQuests.Count; q++)
                                                {
                                                    if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters.Keys.Contains(npcID))
                                                    {
                                                        if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] <= 1)
                                                        {
                                                            ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] = 0;
                                                        }
                                                        else
                                                        {
                                                            ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID]--;
                                                        }
                                                    }
                                                }
                                            }
                                            ((NPC)defender).Kill(true);
                                        }
                                        else
                                        {
                                            // reduce NPC HP
                                            NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                        }
                                    }
                                }
                                else
                                {
                                    // attack missed
                                    if (showRollInfo)
                                    {
                                        ((Descriptor)attacker).Send($"You rolled {hitRoll} ({hitRollFinal}) and missed {targetName}!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        ((Descriptor)attacker).Send($"Your strike misses {targetName}!{Constants.NewLine}");
                                    }
                                    if (defenderIsPlayer)
                                    {
                                        ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name}'s strike misses you!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        if (critical)
                        {
                            // Critical attack, natural 20
                            if (weapon != null)
                            {
                                var damage = Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice) * 2;
                                var ability = weapon.IsFinesse ? ((Descriptor)attacker).Player.Stats.Dexterity : ((Descriptor)attacker).Player.Stats.Strength;
                                var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                if (abilityModifier >= 1)
                                {
                                    damage += Convert.ToUInt32(abilityModifier);
                                }
                                if (((Descriptor)attacker).Player.HasBuff("Desperate Attack"))
                                {
                                    damage += 4;
                                }
                                ((Descriptor)attacker).Send($"You struck a critical blow and hit {targetName} for {damage} damage!{Constants.NewLine}");
                                if (defenderIsPlayer)
                                {
                                    // defender is a player
                                    if (((Descriptor)defender).Player.Stats.CurrentHP <= damage)
                                    {
                                        // Player has been killed
                                        sessionComplete = true;
                                        ((Descriptor)attacker).Send($"You have killed {targetName}!{Constants.NewLine}");
                                        ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} has killed you!{Constants.NewLine}");
                                        ((Descriptor)defender).Player.Kill();
                                    }
                                    else
                                    {
                                        // Player has been damaged
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(damage);
                                        ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} struck a critical blow, hitting you for {damage} damage!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    // defender is an NPC
                                    if (((NPC)defender).Stats.CurrentHP <= damage)
                                    {
                                        // killed the NPC
                                        sessionComplete = true;
                                        ((Descriptor)attacker).Send($"You have killed {targetName} and gained {((NPC)defender).BaseExpAward} Exp and {((NPC)defender).Stats.Gold} gold!{Constants.NewLine}");
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddExp(((NPC)defender).BaseExpAward);
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddGold(((NPC)defender).Stats.Gold);
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.UpdateAlignment(((NPC)defender).Alignment);
                                        // Check and update any quests that might depend on killing this NPC
                                        var npcID = ((NPC)defender).NPCID;
                                        if (((Descriptor)attacker).Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(npcID)))
                                        {
                                            for (int q = 0; q < ((Descriptor)attacker).Player.ActiveQuests.Count; q++)
                                            {
                                                if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters.Keys.Contains(npcID))
                                                {
                                                    if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] <= 1)
                                                    {
                                                        ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] = 0;
                                                    }
                                                    else
                                                    {
                                                        ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID]--;
                                                    }
                                                }
                                            }
                                        }
                                        ((NPC)defender).Kill(true);
                                    }
                                    else
                                    {
                                        // damaged the NPC
                                        NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(damage);
                                    }
                                }
                            }
                            else
                            {
                                // no weapon, punching damage only
                                var damage = Helpers.RollDice(1, 2) * 2;
                                var ability = ((Descriptor)attacker).Player.Stats.Strength;
                                var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                if (abilityModifier >= 1)
                                {
                                    damage += Convert.ToUInt32(abilityModifier);
                                }
                                if (((Descriptor)attacker).Player.HasBuff("Desperate Attack"))
                                {
                                    damage += 4;
                                }
                                ((Descriptor)attacker).Send($"You struck a critical blow and hit {targetName} for {damage} damage!{Constants.NewLine}");
                                if (defenderIsPlayer)
                                {
                                    // defender is a player
                                    if (((Descriptor)defender).Player.Stats.CurrentHP <= damage)
                                    {
                                        // Player has been killed
                                        sessionComplete = true;
                                        ((Descriptor)attacker).Send($"You have killed {targetName}!{Constants.NewLine}");
                                        ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} has killed you!{Constants.NewLine}");
                                        ((Descriptor)defender).Player.Kill();
                                    }
                                    else
                                    {
                                        // Player has been damaged
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(damage);
                                        ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} struck a critical blow, hitting you for {damage} damage!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    // defender is an NPC
                                    if (((NPC)defender).Stats.CurrentHP <= damage)
                                    {
                                        // killed the NPC
                                        sessionComplete = true;
                                        ((Descriptor)attacker).Send($"You have killed {targetName} and gained {((NPC)defender).BaseExpAward} Exp and {((NPC)defender).Stats.Gold} gold!{Constants.NewLine}");
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddExp(((NPC)defender).BaseExpAward);
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.AddGold(((NPC)defender).Stats.Gold);
                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)attacker).Id).Player.UpdateAlignment(((NPC)defender).Alignment);
                                        // Check and update any quests that might depend on killing this NPC
                                        var npcID = ((NPC)defender).NPCID;
                                        if (((Descriptor)attacker).Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(npcID)))
                                        {
                                            for (int q = 0; q < ((Descriptor)attacker).Player.ActiveQuests.Count; q++)
                                            {
                                                if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters.Keys.Contains(npcID))
                                                {
                                                    if (((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] <= 1)
                                                    {
                                                        ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID] = 0;
                                                    }
                                                    else
                                                    {
                                                        ((Descriptor)attacker).Player.ActiveQuests[q].Monsters[npcID]--;
                                                    }
                                                }
                                            }
                                        }
                                        ((NPC)defender).Kill(true);
                                    }
                                    else
                                    {
                                        // damaged the NPC
                                        NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(damage);
                                    }
                                }
                            }
                        }
                        if (fumble)
                        {
                            // Fumbled attack, natural miss
                            ((Descriptor)attacker).Send($"You fumbled your attack and missed {targetName} wildly!{Constants.NewLine}");
                            if (defenderIsPlayer)
                            {
                                ((Descriptor)defender).Send($"{((Descriptor)attacker).Player.Name} fumbled their attack and misses you wildly!{Constants.NewLine}");
                            }
                        }
                        if (sessionComplete)
                        {
                            // break out so we don't process more attacks if someone is dead
                            break;
                        }
                    }
                    if (sessionComplete)
                    {
                        // add the session ID to the list of completed sessions
                        completedSessions.Add(session.SessionID);
                        break;
                    }
                }
                else
                {
                    // Attacker is an NPC
                    if (((NPC)attacker).BehaviourFlags.HasFlag(NPCFlags.Coward))
                    {
                        var fleePoint = Convert.ToUInt32(Math.Round(((NPC)attacker).Stats.MaxHP * 0.18));
                        if (((NPC)attacker).Stats.CurrentHP < fleePoint)
                        {
                            if (((NPC)attacker).FleeCombat(ref _loggingProvider, out uint fleeTo))
                            {
                                sessionComplete = true;
                                completedSessions.Add(session.SessionID);
                                if (defenderIsPlayer)
                                {
                                    ((Descriptor)defender).Send($"{((NPC)attacker).Name} breaks combat and flees!{Constants.NewLine}");
                                    var n = (NPC)attacker;
                                    ((NPC)attacker).Move(ref n, n.CurrentRoom, fleeTo, false);
                                }
                                else
                                {
                                    if (((NPC)defender).IsFollower)
                                    {
                                        SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name} breaks combat with your follower and flees!{Constants.NewLine}");
                                        var n = (NPC)attacker;
                                        ((NPC)attacker).Move(ref n, n.CurrentRoom, fleeTo, false);
                                    }
                                }
                            }
                            else
                            {
                                if (defenderIsPlayer)
                                {
                                    ((Descriptor)defender).Send($"{((NPC)attacker).Name} looks like fleeing, but can't break combat!{Constants.NewLine}");
                                }
                                else
                                {
                                    if (((NPC)defender).IsFollower)
                                    {
                                        SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name} looks like fleeing but can't break away from your follower!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                    }
                    bool npcUsesBasicAttack = true;
                    if (((NPC)attacker).Skills != null && ((NPC)attacker).Skills.Count > 0)
                    {
                        var mobPercHealth = (double)((NPC)attacker).Stats.CurrentHP / ((NPC)attacker).Stats.MaxHP * 100;
                        if (mobPercHealth < 30 && ((NPC)attacker).HasSkill("Desperate Attack"))
                        {
                            if (!((NPC)attacker).HasBuff("Desperate Attack") && ((NPC)attacker).Stats.CurrentMP >= Skills.GetSkill("Desperate Attack").MPCost)
                            {
                                NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid).Stats.CurrentMP -= (int)Skills.GetSkill("Desperate Attack").MPCost;
                                NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid).AddBuff("Desperate Attack");
                                if (defenderIsPlayer)
                                {
                                    ((Descriptor)defender).Send($"{((NPC)attacker).Name} gets ready for a desperate attack!{Constants.NewLine}");
                                }
                            }
                        }
                    }
                    if (((NPC)attacker).Spells != null && ((NPC)attacker).Spells.Count > 0 && !((NPC)attacker).HasBuff("Silence"))
                    {
                        bool healPlayer = false, healSelf = false;
                        if (((NPC)attacker).IsFollower)
                        {
                            var ownerHP = SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.Stats.CurrentHP;
                            var ownerMaxHP = SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.Stats.MaxHP;
                            var ownerHPPerc = (double)SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.Stats.CurrentHP / SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.Stats.MaxHP * 100;
                            if (ownerHPPerc <= 45)
                            {
                                var mobHealingSpells = ((NPC)attacker).Spells.Where(x => x.SpellType == SpellType.Healing).ToList();
                                if (mobHealingSpells.Count > 0)
                                {
                                    var rnd = new Random(DateTime.Now.GetHashCode());
                                    var healSpell = mobHealingSpells[rnd.Next(mobHealingSpells.Count)];
                                    if (((NPC)attacker).Stats.CurrentMP >= healSpell.MPCost)
                                    {
                                        healPlayer = true;
                                        var hpRecovered = Helpers.RollDice(healSpell.NumOfDamageDice, healSpell.SizeOfDamageDice);
                                        NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)healSpell.MPCost * -1);
                                        SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"{((NPC)attacker).Name} prepares a spell to heal your wounds!{Constants.NewLine}");
                                        var abilityModifier = ActorStats.CalculateAbilityModifier(((NPC)attacker).Stats.Wisdom);
                                        if (abilityModifier >= 1)
                                        {
                                            hpRecovered += Convert.ToUInt32(abilityModifier * healSpell.NumOfDamageDice);
                                        }
                                        if (ownerHP + hpRecovered > ownerMaxHP)
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.Stats.CurrentHP = (int)ownerMaxHP;
                                        }
                                        else
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.Stats.CurrentHP += (int)hpRecovered;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var mobCurrentHP = NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid).Stats.CurrentHP;
                            var mobMaxHP = NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid).Stats.MaxHP;
                            var mobPercHP = (double)mobCurrentHP / mobMaxHP * 100;
                            if (mobPercHP < 40)
                            {
                                var mobHealingSpells = ((NPC)attacker).Spells.Where(x => x.SpellType == SpellType.Healing).ToList();
                                if (mobHealingSpells.Count > 0)
                                {
                                    var rnd = new Random(DateTime.Now.GetHashCode());
                                    var healSpell = mobHealingSpells[rnd.Next(mobHealingSpells.Count)];
                                    if (((NPC)attacker).Stats.CurrentMP >= healSpell.MPCost)
                                    {
                                        healSelf = true;
                                        var hpRecovered = Helpers.RollDice(healSpell.NumOfDamageDice, healSpell.SizeOfDamageDice);
                                        NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)healSpell.MPCost * -1);
                                        var abilityModifier = ActorStats.CalculateAbilityModifier(((NPC)attacker).Stats.Wisdom);
                                        if (abilityModifier >= 1)
                                        {
                                            hpRecovered += Convert.ToUInt32(abilityModifier * healSpell.NumOfDamageDice);
                                        }
                                        if (mobCurrentHP + hpRecovered > mobMaxHP)
                                        {
                                            NPCManager.Instance.SetNPCHealthToMax(((NPC)attacker).NPCGuid);
                                        }
                                        else
                                        {
                                            NPCManager.Instance.AdjustNPCHealth(((NPC)attacker).NPCGuid, (int)(hpRecovered));
                                        }
                                    }
                                }
                            }
                        }
                        npcUsesBasicAttack = !healPlayer || !healSelf; // NPC has healed itself or its owner this turn so doesn't get an attack
                    }
                    if (npcUsesBasicAttack)
                    {
                        // No healing used this turn, so see if we have an attack spell to use
                        var attackSpells = ((NPC)attacker).Spells.Where(x => x.SpellType == SpellType.Damage).ToList();
                        if (attackSpells != null && attackSpells.Count > 0 && ((NPC)attacker).HasBuff("Silence"))
                        {
                            var rnd = new Random(DateTime.Now.GetHashCode());
                            var attackSpell = attackSpells[rnd.Next(attackSpells.Count)];
                            if (((NPC)attacker).Stats.CurrentMP >= attackSpell.MPCost)
                            {
                                npcUsesBasicAttack = false;
                                NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)attackSpell.MPCost * -1);
                                var damage = Helpers.RollDice(attackSpell.NumOfDamageDice, attackSpell.SizeOfDamageDice);
                                var abilityModifier = ActorStats.CalculateAbilityModifier(((NPC)attacker).Stats.Intelligence);
                                if (abilityModifier >= 1)
                                {
                                    damage += Convert.ToUInt32(abilityModifier * attackSpell.NumOfDamageDice);
                                }
                                bool spellHitsTarget = true;
                                if (!attackSpell.AutoHitTarget)
                                {
                                    var toHit = Helpers.RollDice(1, 20);
                                    var dexMod = ActorStats.CalculateAbilityModifier(((NPC)attacker).Stats.Dexterity);
                                    if (dexMod > 0)
                                    {
                                        toHit += Convert.ToUInt32(dexMod);
                                    }
                                    var targetAC = defenderIsPlayer ? ((Descriptor)defender).Player.Stats.ArmourClass : ((NPC)defender).Stats.ArmourClass;
                                    spellHitsTarget = toHit >= targetAC;
                                    if (spellHitsTarget)
                                    {
                                        var targetHP = defenderIsPlayer ? SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP : NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP;
                                        if (targetHP <= damage)
                                        {
                                            // killed the target
                                            sessionComplete = true;
                                            if (defenderIsPlayer)
                                            {
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).NPCGuid).Send($"Your follower blasts {((Descriptor)defender).Player.Name} with {attackSpell.SpellName}, killing them!{Constants.NewLine}");
                                                }
                                                ((Descriptor)defender).Send($"{((NPC)attacker).Name} blasts you with {attackSpell.SpellName}, killing you!{Constants.NewLine}");
                                                ((Descriptor)defender).Player.Kill();
                                            }
                                            else
                                            {
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).NPCGuid).Send($"Your follower blasts {((NPC)defender).Name} with {attackSpell.SpellName}, killing them!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).NPCGuid).Send($"{((NPC)attacker).Name} blasts your follower with {attackSpell.SpellName}, killing them!{Constants.NewLine}");
                                                }
                                                ((NPC)defender).Kill(true);
                                            }
                                        }
                                        else
                                        {
                                            // damaged target
                                            if (defenderIsPlayer)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(damage);
                                                ((Descriptor)defender).Send($"{((NPC)attacker).Name}'s {attackSpell.SpellName} blasts you for {damage} damage!{Constants.NewLine}");
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).NPCGuid).Send($"Your follower blasts {((Descriptor)defender).Player.Name} with {attackSpell.SpellName} for {damage} damage!{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(damage);
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower blasts {((NPC)defender).Name} with {attackSpell.SpellName} for {damage} damage!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name} blasts your follower with {attackSpell.SpellName} for {damage} damage!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // spell missed the target
                                        if (((NPC)attacker).IsFollower)
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower's spell fizzles and fails!{Constants.NewLine}");
                                        }
                                        if (defenderIsPlayer)
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Send($"{((NPC)attacker).Name}'s spell fizzles and fails!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            if (((NPC)defender).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name}'s spell aimed at your follower fizzles and fails!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (npcUsesBasicAttack)
                    {
                        // try a buff spell on the NPC or its owner (if there is one)
                        var buffSpells = ((NPC)attacker).Spells.Where(x => x.SpellType == SpellType.Buff).ToList();
                        if (buffSpells != null && buffSpells.Count > 0 && !((NPC)attacker).HasBuff("Silence"))
                        {
                            bool buffPlayer = false, buffSelf = false;
                            if (((NPC)attacker).IsFollower)
                            {
                                var rnd = new Random(DateTime.Now.GetHashCode());
                                var buffSpell = buffSpells[rnd.Next(buffSpells.Count)];
                                var mobMP = ((NPC)attacker).Stats.CurrentMP;
                                if (mobMP >= buffSpell.MPCost)
                                {
                                    if (!SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.HasBuff(buffSpell.SpellName))
                                    {
                                        buffPlayer = true;
                                        NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)buffSpell.MPCost * -1);
                                        SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Player.AddBuff(buffSpell.SpellName);
                                        SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"{((NPC)attacker).Name} blesses you with the power of {buffSpell.SpellName}!{Constants.NewLine}");
                                    }
                                }
                            }
                            if (!buffPlayer)
                            {
                                var rnd = new Random(DateTime.Now.GetHashCode());
                                var buffSpell = buffSpells[rnd.Next(buffSpells.Count)];
                                var mobMP = ((NPC)attacker).Stats.CurrentMP;
                                if (mobMP >= buffSpell.MPCost)
                                {
                                    if (!NPCManager.Instance.GetNPCByGUID(((NPC)attacker).NPCGuid).HasBuff(buffSpell.SpellName))
                                    {
                                        buffSelf = true;
                                        ((NPC)attacker).AddBuff(buffSpell.SpellName);
                                        NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)buffSpell.MPCost * -1);
                                        if (((NPC)attacker).IsFollower)
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"{((NPC)attacker).Name} blesses themselves with the power of {buffSpell.SpellName}!");
                                        }
                                    }
                                }
                            }
                            npcUsesBasicAttack = !buffPlayer || !buffSelf;
                        }
                    }
                    if (npcUsesBasicAttack)
                    {
                        // try a debuff spell on the target
                        var debuffSpells = ((NPC)attacker).Spells.Where(x => x.SpellType == SpellType.Debuff).ToList();
                        if (debuffSpells != null && debuffSpells.Count > 0 && !((NPC)attacker).HasBuff("Silence"))
                        {
                            var rnd = new Random(DateTime.Now.GetHashCode());
                            var debuffSpell = debuffSpells[rnd.Next(debuffSpells.Count)];
                            var mobMP = ((NPC)attacker).Stats.CurrentMP;
                            if (mobMP >= debuffSpell.MPCost)
                            {
                                if (defenderIsPlayer)
                                {
                                    if (!SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.HasBuff(debuffSpell.SpellName))
                                    {
                                        npcUsesBasicAttack = false;
                                        NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)debuffSpell.MPCost * -1);
                                        bool spellHits = true;
                                        if (!debuffSpell.AutoHitTarget)
                                        {
                                            var toHit = Helpers.RollDice(1, 20);
                                            var ability = ((NPC)attacker).Stats.Dexterity;
                                            var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                            var targetAC = ((Descriptor)defender).Player.Stats.ArmourClass;
                                            if (abilityModifier > 0)
                                            {
                                                toHit += Convert.ToUInt32(abilityModifier);
                                            }
                                            spellHits = toHit >= targetAC;
                                            if (spellHits)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.AddBuff(debuffSpell.SpellName);
                                                if (debuffSpell.NumOfDamageDice > 0)
                                                {
                                                    var damage = Helpers.RollDice(debuffSpell.NumOfDamageDice, debuffSpell.SizeOfDamageDice);
                                                    ability = ((NPC)attacker).Stats.Intelligence;
                                                    abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                                    if (abilityModifier > 0)
                                                    {
                                                        damage += Convert.ToUInt32(abilityModifier * debuffSpell.NumOfDamageDice);
                                                    }
                                                    var targetHP = SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP;
                                                    if (targetHP <= damage)
                                                    {
                                                        // kill the player
                                                        sessionComplete = true;
                                                        if (((NPC)attacker).IsFollower)
                                                        {
                                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has slain {((Descriptor)defender).Player.Name}!{Constants.NewLine}");
                                                        }
                                                        ((Descriptor)defender).Send($"{((NPC)attacker).Name} has killed you!{Constants.NewLine}");
                                                        ((Descriptor)defender).Player.Kill();
                                                    }
                                                    else
                                                    {
                                                        // damage the player
                                                        SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(damage);
                                                        ((Descriptor)defender).Send($"{((NPC)attacker).Name} calls on the power of {debuffSpell.SpellName} to hinder you, causing {damage} damage!{Constants.NewLine}");
                                                        if (((NPC)attacker).IsFollower)
                                                        {
                                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower calls on the power of {debuffSpell.SpellName} to hinder {((Descriptor)defender).Player.Name} and cause {damage} damage!{Constants.NewLine}");
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // spell missed the target
                                                SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Send($"{((NPC)attacker).Name}'s spell fizzles and fails!{Constants.NewLine}");
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower's spell fizzles and fails!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).HasBuff(debuffSpell.SpellName))
                                    {
                                        npcUsesBasicAttack = false;
                                        NPCManager.Instance.AdjustNPCMana(((NPC)attacker).NPCGuid, (int)debuffSpell.MPCost * -1);
                                        bool spellHits = true;
                                        if (!debuffSpell.AutoHitTarget)
                                        {
                                            var toHit = Helpers.RollDice(1, 20);
                                            var ability = ((NPC)attacker).Stats.Dexterity;
                                            var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                            var targetAC = ((NPC)defender).Stats.ArmourClass;
                                            if (abilityModifier > 0)
                                            {
                                                toHit += Convert.ToUInt32(abilityModifier);
                                            }
                                            spellHits = toHit >= targetAC;
                                            if (spellHits)
                                            {
                                                NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).AddBuff(debuffSpell.SpellName);
                                                if (debuffSpell.NumOfDamageDice > 0)
                                                {
                                                    var damage = Helpers.RollDice(debuffSpell.NumOfDamageDice, debuffSpell.SizeOfDamageDice);
                                                    ability = ((NPC)attacker).Stats.Dexterity;
                                                    abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                                    if (abilityModifier > 0)
                                                    {
                                                        damage += Convert.ToUInt32(abilityModifier * debuffSpell.NumOfDamageDice);
                                                    }
                                                    var targetHP = NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP;
                                                    if (targetHP <= damage)
                                                    {
                                                        // kill the npc
                                                        sessionComplete = true;
                                                        if (((NPC)attacker).IsFollower)
                                                        {
                                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has slain {((NPC)defender).Name}!{Constants.NewLine}");
                                                        }
                                                        if (((NPC)defender).IsFollower)
                                                        {
                                                            SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"Your follower has been brutally slain by {((NPC)attacker).Name}!{Constants.NewLine}");
                                                        }
                                                        NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Kill(true);
                                                    }
                                                    else
                                                    {
                                                        // damage the npc
                                                        NPCManager.Instance.AdjustNPCHealth(((NPC)defender).NPCGuid, (int)damage * -1);
                                                        if (((NPC)attacker).IsFollower)
                                                        {
                                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower's {debuffSpell.SpellName} has hindered {((NPC)defender).Name} and caused {damage} damage!{Constants.NewLine}");
                                                        }
                                                        if (((NPC)defender).IsFollower)
                                                        {
                                                            SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name}'s {debuffSpell.SpellName} has hindered your follower and caused {damage} damage!{Constants.NewLine}");
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // spell missed
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower's {debuffSpell.SpellName} spell fizzles and fails!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name}'s {debuffSpell.SpellName} fizzles and failes to hit your follower!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (npcUsesBasicAttack)
                    {
                        for (int a = 0; a < ((NPC)attacker).NumberOfAttacks; a++)
                        {
                            var targetAC = defenderIsPlayer ? ((Descriptor)defender).Player.Stats.ArmourClass : ((NPC)defender).Stats.ArmourClass;
                            var targetName = defenderIsPlayer ? ((Descriptor)defender).Player.Name : ((NPC)defender).Name;
                            var hitRoll = Helpers.RollDice(1, 20);
                            bool fumble = hitRoll == 1;
                            bool critical = hitRoll == 20;
                            InventoryItem weapon = null;
                            if (((NPC)attacker).EquippedItems != null && ((NPC)attacker).EquippedItems.Weapon != null)
                            {
                                weapon = ((NPC)attacker).EquippedItems.Weapon;
                            }
                            if (!fumble && !critical)
                            {
                                // Normal attack, not a natural 1 or 20
                                if (weapon != null)
                                {
                                    var damage = Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice);
                                    var ability = weapon.IsFinesse ? ((NPC)attacker).Stats.Dexterity : ((NPC)attacker).Stats.Strength;
                                    var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                    var hitRollFinal = Convert.ToUInt32(hitRoll + abilityModifier);
                                    var finalDamage = damage + abilityModifier > 0 ? Convert.ToUInt32(damage + abilityModifier) : 1;
                                    if (((NPC)attacker).HasBuff("Truestrike"))
                                    {
                                        hitRollFinal += 10;
                                    }
                                    if (((NPC)attacker).HasBuff("Desperate Attack"))
                                    {
                                        hitRollFinal -= 4;
                                        finalDamage += 4;
                                    }
                                    if (hitRollFinal >= targetAC)
                                    {
                                        // attack landed
                                        if (defenderIsPlayer)
                                        {
                                            if (((Descriptor)defender).Player.Stats.CurrentHP <= finalDamage)
                                            {
                                                // kill the player
                                                sessionComplete = true;
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                                }
                                                ((Descriptor)defender).Send($"{((NPC)attacker).Name} has killed you!{Constants.NewLine}");
                                                ((Descriptor)defender).Player.Kill();
                                            }
                                            else
                                            {
                                                // damage the player
                                                SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                                ((Descriptor)defender).Send($"{((NPC)attacker).Name}'s {weapon.Name} hits you for {finalDamage} damage!{Constants.NewLine}");
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has hit {targetName} with their {weapon.Name} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // NPC hit another NPC
                                            if (((NPC)defender).Stats.CurrentHP <= finalDamage)
                                            {
                                                // kill the target NPC
                                                sessionComplete = true;
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"Your follower has been brutally slain by {((NPC)attacker).Name}!{Constants.NewLine}");
                                                }
                                                ((NPC)defender).Kill(true);
                                            }
                                            else
                                            {
                                                // damage the target NPC
                                                NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has hit {targetName} with their {weapon.Name} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name} has hit your follower with their {weapon.Name} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // attack missed
                                        if (defenderIsPlayer)
                                        {
                                            ((Descriptor)defender).Send($"{((NPC)attacker).Name} misses you with their {weapon.Name}!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            if (((NPC)defender).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name} misses your follower with their {weapon.Name}!");
                                            }
                                        }
                                        if (((NPC)attacker).IsFollower)
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has missed {targetName} with their {weapon.Name}!{Constants.NewLine}");
                                        }
                                    }
                                }
                                else
                                {
                                    // no weapon, basic 1d2 + STR damage
                                    var damage = Helpers.RollDice(1, 2);
                                    var ability = ((NPC)attacker).Stats.Strength;
                                    var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                    var hitRollFinal = Convert.ToUInt32(hitRoll + abilityModifier);
                                    var finalDamage = damage + abilityModifier > 0 ? Convert.ToUInt32(damage + abilityModifier) : 1;
                                    if (((NPC)attacker).HasBuff("Truestrike"))
                                    {
                                        hitRollFinal += 10;
                                    }
                                    if (((NPC)attacker).HasBuff("Desperate Attack"))
                                    {
                                        hitRollFinal -= 4;
                                        finalDamage += 4;
                                    }
                                    if (hitRollFinal >= targetAC)
                                    {
                                        // attack landed
                                        if (defenderIsPlayer)
                                        {
                                            if (((Descriptor)defender).Player.Stats.CurrentHP <= finalDamage)
                                            {
                                                // kill the player
                                                sessionComplete = true;
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                                }
                                                ((Descriptor)defender).Send($"{((NPC)attacker).Name} has killed you!{Constants.NewLine}");
                                                ((Descriptor)defender).Player.Kill();
                                            }
                                            else
                                            {
                                                // damage the player
                                                SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                                ((Descriptor)defender).Send($"{((NPC)attacker).Name}'s strike hits you for {finalDamage} damage!{Constants.NewLine}");
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has hit {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // NPC hit another NPC
                                            if (((NPC)defender).Stats.CurrentHP <= finalDamage)
                                            {
                                                // kill the target NPC
                                                sessionComplete = true;
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"Your follower has been brutally slain by {((NPC)attacker).Name}!{Constants.NewLine}");
                                                }
                                                ((NPC)defender).Kill(true);
                                            }
                                            else
                                            {
                                                // damage the target NPC
                                                NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                                if (((NPC)attacker).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has hit {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                                if (((NPC)defender).IsFollower)
                                                {
                                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name}'s strike has hit your follower for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (critical)
                            {
                                if (weapon != null)
                                {
                                    var damage = Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice) * 2;
                                    var ability = weapon.IsFinesse ? ((NPC)attacker).Stats.Dexterity : ((NPC)attacker).Stats.Strength;
                                    var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                    var finalDamage = damage + abilityModifier > 0 ? Convert.ToUInt32(damage + abilityModifier) : 1;
                                    if (((NPC)attacker).HasBuff("Desperate Attack"))
                                    {
                                        finalDamage += 4;
                                    }
                                    if (((NPC)attacker).IsFollower)
                                    {
                                        SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower struck a critical blow against {targetName} hitting for {finalDamage} damage!{Constants.NewLine}");
                                    }
                                    if (defenderIsPlayer)
                                    {
                                        // defender is a player
                                        if (((Descriptor)defender).Player.Stats.CurrentHP <= finalDamage)
                                        {
                                            // kill the player
                                            sessionComplete = true;
                                            ((Descriptor)defender).Send($"{((NPC)attacker).Name} has killed you!{Constants.NewLine}");
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                            }
                                            ((Descriptor)defender).Player.Kill();
                                        }
                                        else
                                        {
                                            // wound the player
                                            ((Descriptor)defender).Send($"{((NPC)attacker).Name} struck a critical blow, hitting you for {finalDamage} damage!{Constants.NewLine}");
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has {targetName} with their {weapon.Name} for {finalDamage} damage!{Constants.NewLine}");
                                            }
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                        }
                                    }
                                    else
                                    {
                                        // defender is an NPC
                                        if (((NPC)defender).Stats.CurrentHP <= finalDamage)
                                        {
                                            // kill the NPC
                                            sessionComplete = true;
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has slain {targetName}!{Constants.NewLine}");
                                            }
                                            if (((NPC)defender).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"Your follower has been brutally slain by {((NPC)attacker).Name}!{Constants.NewLine}");
                                            }
                                            ((NPC)defender).Kill(true);
                                        }
                                        else
                                        {
                                            // wound the NPC
                                            NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                            if (((NPC)defender).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name} struck a critical blow against your follower for {finalDamage} damage!{Constants.NewLine}");
                                            }
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower hits {targetName} with their {weapon.Name} for {finalDamage} damage!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // No weapon equipped
                                    var damage = Helpers.RollDice(1, 2) * 2;
                                    var ability = ((NPC)attacker).Stats.Strength;
                                    var abilityModifier = ActorStats.CalculateAbilityModifier(ability);
                                    var hitRollFinal = Convert.ToUInt32(hitRoll + abilityModifier);
                                    var finalDamage = damage + abilityModifier > 0 ? Convert.ToUInt32(damage + abilityModifier) : 1;
                                    if (((NPC)attacker).HasBuff("Desperate Attack"))
                                    {
                                        hitRollFinal -= 4;
                                        finalDamage += 4;
                                    }
                                    if (defenderIsPlayer)
                                    {
                                        if (((Descriptor)defender).Player.Stats.CurrentHP <= finalDamage)
                                        {
                                            // kill the player
                                            sessionComplete = true;
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                            }
                                            ((Descriptor)defender).Send($"{((NPC)attacker).Name} has killed you!{Constants.NewLine}");
                                            ((Descriptor)defender).Player.Kill();
                                        }
                                        else
                                        {
                                            // damage the player
                                            SessionManager.Instance.GetPlayerByGUID(((Descriptor)defender).Id).Player.Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                            ((Descriptor)defender).Send($"{((NPC)attacker).Name}'s strike hits you for {finalDamage} damage!{Constants.NewLine}");
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has hit {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // NPC hit another NPC
                                        if (((NPC)defender).Stats.CurrentHP <= finalDamage)
                                        {
                                            // kill the target NPC
                                            sessionComplete = true;
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has brutally slain {targetName}!{Constants.NewLine}");
                                            }
                                            if (((NPC)defender).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"Your follower has been brutally slain by {((NPC)attacker).Name}!{Constants.NewLine}");
                                            }
                                            ((NPC)defender).Kill(true);
                                        }
                                        else
                                        {
                                            // damage the target NPC
                                            NPCManager.Instance.GetNPCByGUID(((NPC)defender).NPCGuid).Stats.CurrentHP -= Convert.ToInt32(finalDamage);
                                            if (((NPC)attacker).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower has hit {targetName} for {finalDamage} damage!{Constants.NewLine}");
                                            }
                                            if (((NPC)defender).IsFollower)
                                            {
                                                SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)attacker).Name}'s strike has hit your follower for {finalDamage} damage!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                            if (fumble)
                            {
                                // fumbled and missed
                                if (((NPC)attacker).IsFollower)
                                {
                                    SessionManager.Instance.GetPlayerByGUID(((NPC)attacker).FollowingPlayer).Send($"Your follower fumbles their attack and misses {targetName} wildly!{Constants.NewLine}");
                                }
                                if (defenderIsPlayer)
                                {
                                    ((Descriptor)defender).Send($"{((NPC)attacker).Name} fumbled their attack and misses you wildly!{Constants.NewLine}");
                                }
                                if (!defenderIsPlayer && ((NPC)defender).IsFollower)
                                {
                                    SessionManager.Instance.GetPlayerByGUID(((NPC)defender).FollowingPlayer).Send($"{((NPC)defender).Name} fumbled their attack and missed your follower wildly!{Constants.NewLine}");
                                }
                            }
                            if (sessionComplete)
                            {
                                // break out so we don't process more attacks if someone is dead
                                break;
                            }
                        }
                        if (sessionComplete)
                        {
                            // add the session ID to the list of completed sessions
                            completedSessions.Add(session.SessionID);
                            break;
                        }
                    }
                }
            }
        }

        internal Dictionary<Guid, CombatSessionNew> GetCombatQueue()
        {
            return CombatQueue;
        }

        internal void AddCombatSession(CombatSessionNew session)
        {
            lock(_lockObject)
            {
                CombatQueue.Add(session.SessionID, session);
            }
        }

        internal void RemoveCombatSession(Guid g)
        {
            lock(_lockObject)
            {
                if (CombatQueue[g].Attacker != null && CombatQueue[g].Attacker is Descriptor)
                {
                    SessionManager.Instance.GetPlayerByGUID((CombatQueue[g].Attacker as Descriptor).Id).Player.Position = ActorPosition.Standing;
                }
                if (CombatQueue[g].Defender != null && CombatQueue[g].Defender is Descriptor)
                {
                    SessionManager.Instance.GetPlayerByGUID((CombatQueue[g].Defender as Descriptor).Id).Player.Position = ActorPosition.Standing;
                }
                CombatQueue.Remove(g);
            }
        }
    }
}