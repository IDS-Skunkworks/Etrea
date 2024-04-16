using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etrea2.Core
{
    internal class CombatManager
    {
        private static CombatManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<Guid, CombatSession> CombatSessions;

        private CombatManager()
        {
            CombatSessions = new Dictionary<Guid, CombatSession>();
        }

        internal static CombatManager Instance
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CombatManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool IsPlayerInCombat(Guid playerId)
        {
            lock(_lock)
            {
                if (Instance.CombatSessions.Count > 0)
                {
                    return Instance.CombatSessions.Values.Where(x => x.Attacker is Descriptor && (x.Attacker as Descriptor).ID == playerId).Any();
                }
                return false;
            }
        }

        internal bool IsNPCInCombat(Guid npcId)
        {
            lock(_lock)
            {
                if (Instance.CombatSessions.Count > 0)
                {
                    return Instance.CombatSessions.Values.Where(x => x.Attacker is NPC && (x.Attacker as NPC).NPCGuid == npcId).Any();
                }
                return false;
            }
        }

        internal bool IsPlayerInCombatWithNPC(Guid playerId, Guid npcId)
        {
            lock (_lock)
            {
                if (Instance.CombatSessions.Count > 0)
                {
                    return Instance.CombatSessions.Values.Where(x => x.Attacker is Descriptor && x.Defender is NPC && (x.Defender as NPC).NPCGuid == npcId && (x.Attacker as Descriptor).ID == playerId).Any();
                }
                return false;
            }
        }

        internal List<Guid> GetCombatSessionsForCombatant(Guid id)
        {
            lock(_lock)
            {
                if (Instance.CombatSessions.Count > 0)
                {
                    return Instance.CombatSessions.Values.Where(x => x.AttackerID == id || x.DefenderID == id).Select(x => x.SessionID).ToList();
                }
            }
            return new List<Guid>();
        }

        internal List<Guid> GetCombatSessionsForCombatantPairing(Guid attacker, Guid defender)
        {
            lock (_lock)
            {
                if (Instance.CombatSessions.Count > 0)
                {
                    List<Guid> result = new List<Guid>();
                    result.AddRange(from CombatSession s in Instance.CombatSessions.Values where s.AttackerID == attacker && s.DefenderID == defender select s.SessionID);
                    result.AddRange(from CombatSession s in Instance.CombatSessions.Values where s.AttackerID == defender && s.DefenderID == attacker select s.SessionID);
                    return result;
                }
            }
            return new List<Guid>();
        }

        internal Dictionary<Guid, CombatSession> GetCombatQueue()
        {
            lock (_lock)
            {
                return Instance.CombatSessions;
            }
        }

        internal void AddCombatSession(CombatSession session)
        {
            lock (_lock)
            {
                Instance.CombatSessions.Add(session.SessionID, session);
            }
        }

        internal void RemoveCombatSession(Guid sessionID)
        {
            lock (_lock)
            {
                if (Instance.CombatSessions.ContainsKey(sessionID))
                {
                    if (Instance.CombatSessions[sessionID].Attacker != null && Instance.CombatSessions[sessionID].Attacker is Descriptor)
                    {
                        SessionManager.Instance.GetPlayerByGUID((Instance.CombatSessions[sessionID].Attacker as Descriptor).ID).Player.Position = ActorPosition.Standing;
                    }
                    if (Instance.CombatSessions[sessionID].Defender != null && Instance.CombatSessions[sessionID].Defender is Descriptor)
                    {
                        SessionManager.Instance.GetPlayerByGUID((Instance.CombatSessions[sessionID].Defender as Descriptor).ID).Player.Position = ActorPosition.Standing;
                    }
                    Instance.CombatSessions.Remove(sessionID);
                }
            }
        }

        internal void ProcessCombatQueue(out List<Guid> completedSessions)
        {
            completedSessions = new List<Guid>();
            lock( _lock)
            {
                var toProcess = new Dictionary<Guid, CombatSession>(Instance.CombatSessions);
                foreach(var session in toProcess)
                {
                    //bool sessionComplete = false;
                    if (session.Value.Attacker == null || session.Value.Defender == null)
                    {
                        Game.LogMessage($"WARN: Aborting Combat session {session.Key}, attacker and/or defender was null", LogLevel.Warning, true);
                        completedSessions.Add(session.Key);
                        continue;
                    }

                    // PVE combat, so one participant is a Player the other is an NPC
                    if ((session.Value.Attacker is Descriptor && session.Value.Defender is NPC) || (session.Value.Attacker is NPC && session.Value.Defender is Descriptor))
                    {
                        if (session.Value.Attacker is Descriptor)
                        {
                            var attacker = SessionManager.Instance.GetPlayerByGUID((session.Value.Attacker as Descriptor).ID);
                            var defender = NPCManager.Instance.GetNPCByGUID((session.Value.Defender as NPC).NPCGuid);
                            if (attacker != null && defender != null)
                            {
                                var article = Helpers.IsCharAVowel(defender.Name[0]) ? "an" : "a";
                                for (int a = 0; a < attacker.Player.NumberOfAttacks; a++)
                                {
                                    bool hits = attacker.Player.DoHitRoll(defender, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical);
                                    var wpn = attacker.Player.EquipWeapon?.Name.ToLower() ?? "strike";
                                    if (hits)
                                    {
                                        var damage = attacker.Player.DoDamageRoll(defender);
                                        if (isCritical)
                                        {
                                            damage *= 2;
                                            attacker.Send($"You have struck a critical blow against your enemy!{Constants.NewLine}");
                                        }
                                        defender.AdjustHP((int)damage * -1, out bool isKilled);
                                        if (isKilled)
                                        {
                                            attacker.Send($"Your {wpn} hits for lethal damage, killing {article} {defender.Name}!{Constants.NewLine}");
                                            defender.Kill(true, ref attacker);
                                            attacker.Send($"You gain {defender.BaseExpAward} Exp and {defender.Gold} gold!{Constants.NewLine}");
                                            attacker.Player.AddExp(defender.BaseExpAward, false, false);
                                            attacker.Player.AddGold(defender.Gold, false);
                                            completedSessions.AddRange(GetCombatSessionsForCombatant((session.Value.Defender as NPC).NPCGuid));
                                            break;
                                        }
                                        else
                                        {
                                            var pDmg = (uint)Math.Round((double)damage / defender.CurrentHP * 100, 0);
                                            if (attacker.Player.ShowDetailedRollInfo || attacker.Player.Level >= Constants.ImmLevel)
                                            {
                                                attacker.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): Your {wpn} {Helpers.GetDamageString(pDmg)} {article} {defender.Name} for {damage} damage!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                attacker.Send($"Your {wpn} {Helpers.GetDamageString(pDmg)} {article} {defender.Name}!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (attacker.Player.ShowDetailedRollInfo || attacker.Player.Level >= Constants.ImmLevel)
                                        {
                                            attacker.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): Your {wpn} misses {article} {defender.Name}!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            attacker.Send($"Your {wpn} misses {defender.Name}!{Constants.NewLine}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // one or both participants in the combat session was null so flag for removal and move on to the next one
                                completedSessions.Add(session.Key);
                                continue;
                            }
                        }

                        if (session.Value.Attacker is NPC)
                        {
                            var attacker = NPCManager.Instance.GetNPCByGUID((session.Value.Attacker as NPC).NPCGuid);
                            var defender = SessionManager.Instance.GetPlayerByGUID((session.Value.Defender as Descriptor).ID);
                            if (attacker != null && defender != null)
                            {
                                var hpPerc = (int)Math.Round((double)attacker.CurrentHP / attacker.MaxHP * 100, 0);
                                var article = Helpers.IsCharAVowel(attacker.Name[0]) ? "An" : "A";
                                if (attacker.BehaviourFlags.HasFlag(NPCFlags.Coward))
                                {
                                    if (hpPerc <= 90)
                                    {
                                        if (attacker.FleeCombat(out uint destRid))
                                        {
                                            defender.Send($"{article} {attacker.Name} breaks combat and flees from you!{Constants.NewLine}");
                                            completedSessions.AddRange(GetCombatSessionsForCombatant(attacker.NPCGuid));
                                            attacker.Move(ref attacker, attacker.CurrentRoom, destRid, false);
                                            continue;
                                        }
                                        else
                                        {
                                            defender.Send($"{article} {attacker.Name} looks like fleeing but can't break away from you!{Constants.NewLine}");
                                            int willCheck = (int)Helpers.RollDice(1, 20);
                                            var willMod = Helpers.CalculateAbilityModifier(attacker.Wisdom);
                                            willCheck += willMod;
                                            if (willCheck < 10)
                                            {
                                                // abort the current round for the NPC if they fail a will check
                                                continue;
                                            }
                                        }
                                    }
                                }
                                if (attacker.Spells != null && attacker.Spells.Count > 0)
                                {
                                    var healSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Healing && !x.AOESpell).ToList();
                                    var damageSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Damage && !x.AOESpell).ToList();
                                    var buffSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Buff && !x.AOESpell).ToList();
                                    var debuffSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Debuff && !x.AOESpell).ToList();
                                    Spell spell = null;
                                    if (hpPerc <= 50)
                                    {
                                        spell = healSpells.Where(x => x.MPCost <= attacker.CurrentMP).FirstOrDefault();
                                        if (spell != null)
                                        {
                                            attacker.AdjustMP((int)spell.MPCost * -1);
                                            var effect = spell.CalculateSpellHPEffect(attacker, attacker, out _);
                                            attacker.AdjustHP(effect, out _);
                                            defender.Send($"{article} {attacker.Name} heals their wounds with a spell!{Constants.NewLine}");
                                            continue;
                                        }
                                        else
                                        {
                                            defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                        }
                                    }
                                    var availDamageSpells = damageSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                    if (availDamageSpells.Count > 0)
                                    {
                                        spell = availDamageSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availDamageSpells.Count)];
                                        if (spell != null)
                                        {
                                            attacker.AdjustMP((int)spell.MPCost * -1);
                                            var effect = spell.CalculateSpellHPEffect(attacker, defender.Player, out bool spellHitTarget);
                                            if (spellHitTarget)
                                            {
                                                if (effect < 0)
                                                {
                                                    // player absorbed the attack so increment health by effect * -1
                                                    defender.Player.AdjustHP(effect * -1, out _);
                                                    defender.Send($"{article} {attacker.Name} hurls {spell} at you, but you absorb the effect!{Constants.NewLine}");
                                                    continue;
                                                }
                                                else
                                                {
                                                    // spell damaged the player
                                                    defender.Send($"{article} {attacker.Name} hurls a {spell} spell at you, blasting you for {effect} damage!{Constants.NewLine}");
                                                    defender.Player.AdjustHP(effect * -1, out bool isKilled);
                                                    if (isKilled)
                                                    {
                                                        defender.Send($"Your wounds are serious and many and death overcomes you...{Constants.NewLine}");
                                                        completedSessions.AddRange(GetCombatSessionsForCombatant(defender.ID));
                                                        defender.Player.Kill();
                                                        continue;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                defender.Send($"{article} {attacker.Name} hurls {spell} at you, but their magic fizzles and the spell misses!{Constants.NewLine}");
                                            }
                                        }
                                        else
                                        {
                                            defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                    }
                                    var availBuffSpells = buffSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                    if (availBuffSpells.Count > 0)
                                    {
                                        spell = availBuffSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availBuffSpells.Count)];
                                        if (spell != null)
                                        {
                                            spell.ApplyBuffSpell(attacker, attacker, out bool hitsTarget, out int hpModifier);
                                            attacker.AdjustMP((int)spell.MPCost * -1);
                                            if (hitsTarget)
                                            {
                                                defender.Send($"{article} {attacker.Name} buffs their abilities with the power of {spell}!{Constants.NewLine}");
                                                if (hpModifier != 0)
                                                {
                                                    attacker.AdjustHP(hpModifier, out _);
                                                }
                                                continue;
                                            }
                                            else
                                            {
                                                defender.Send($"{article} {attacker.Name} tries to cast {spell} on themselves, but their magic fizzles and vanishes!{Constants.NewLine}");
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                    }
                                    var availDebuffSpells = debuffSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                    if (availDebuffSpells.Count > 0)
                                    {
                                        spell = availDebuffSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availDebuffSpells.Count)];
                                        if (spell != null)
                                        {
                                            spell.ApplyBuffSpell(attacker, defender.Player, out bool hitsTarget, out int hpModifier);
                                            attacker.AdjustMP((int)spell.MPCost * -1);
                                            if (hitsTarget)
                                            {
                                                defender.Send($"{article} {attacker.Name} curses you with the power of {spell}!{Constants.NewLine}");
                                                if (hpModifier != 0)
                                                {
                                                    defender.Player.AdjustHP(hpModifier * -1, out bool isKilled);
                                                    if (isKilled)
                                                    {
                                                        defender.Send($"Your wounds are many and serious and death overcomes you...{Constants.NewLine}");
                                                        defender.Player.Kill();
                                                        completedSessions.AddRange(GetCombatSessionsForCombatant(defender.ID));
                                                        continue;
                                                    }
                                                }
                                                continue;
                                            }
                                            else
                                            {
                                                defender.Send($"{article} {attacker.Name} tries to curse you with {spell} but their magic fizzles and vanishes!{Constants.NewLine}");
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        defender.Send($"{article} {attacker.Name} wants to cast a spell, but doesn't have the resources!{Constants.NewLine}");
                                    }
                                }
                                for (int a = 0; a < attacker.NumberOfAttacks; a++)
                                {
                                    bool hits = attacker.DoHitRoll(defender.Player, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical);
                                    var wpn = attacker.EquipWeapon?.Name.ToLower() ?? "strike";
                                    if (hits)
                                    {
                                        var damage = attacker.DoDamageRoll(defender.Player);
                                        if (isCritical)
                                        {
                                            damage *= 2;
                                            defender.Send($"{article} {attacker.Name} strikes a critical blow against you!{Constants.NewLine}");
                                        }
                                        defender.Player.AdjustHP((int)damage * -1, out bool isKilled);
                                        if (isKilled)
                                        {
                                            completedSessions.AddRange(GetCombatSessionsForCombatant(defender.ID));
                                            defender.Send($"{article} {attacker.Name}'s {wpn} hits you for lethal damage, killing you!{Constants.NewLine}");
                                            defender.Player.Kill();
                                            break;
                                        }
                                        else
                                        {
                                            var pDmg = (uint)Math.Round((double)damage / defender.Player.CurrentHP * 100, 0);
                                            if (defender.Player.ShowDetailedRollInfo || defender.Player.Level >= Constants.ImmLevel)
                                            {
                                                defender.Send($"{article} {attacker.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} you for {damage} damage!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                defender.Send($"{article} {attacker.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} you!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        defender.Send($"{article} {attacker.Name}'s {wpn} misses you!{Constants.NewLine}");
                                    }
                                }
                            }
                            else
                            {
                                // one or both participants in the round was null so flag for removal and continue on
                                completedSessions.Add(session.Key);
                                continue;
                            }
                        }
                        continue;
                    }

                    // EVE combat, both participants are NPCs
                    if (session.Value.Attacker is NPC && session.Value.Defender is NPC)
                    {
                        NPC attacker = NPCManager.Instance.GetNPCByGUID((session.Value.Attacker as NPC).NPCGuid);
                        NPC defender = NPCManager.Instance.GetNPCByGUID((session.Value.Defender as NPC).NPCGuid);
                        Descriptor attackerOwner = attacker.IsFollower ? SessionManager.Instance.GetPlayerByGUID(attacker.FollowingPlayer) : null;
                        Descriptor defenderOwner = defender.IsFollower ? SessionManager.Instance.GetPlayerByGUID(defender.FollowingPlayer) : null;
                        Spell spell = null;
                        if (attacker != null && defender != null)
                        {
                            var healSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Healing && !x.AOESpell).ToList();
                            var buffSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Buff && !x.AOESpell).ToList();
                            var debuffSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Debuff && !x.AOESpell).ToList();
                            var damageSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Damage && !x.AOESpell).ToList();
                            if (attacker.IsFollower)
                            {
                                // see if we want to heal or buff the owning player
                                var cutOff = attackerOwner.Player.MaxHP / 2;
                                if (attackerOwner.Player.CurrentHP < cutOff)
                                {
                                    if (healSpells != null && healSpells.Count > 0)
                                    {
                                        var availHealSpells = healSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                        if (availHealSpells != null && availHealSpells.Count > 0)
                                        {
                                            spell = availHealSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availHealSpells.Count)];
                                            if (spell != null)
                                            {
                                                int hpMod = spell.CalculateSpellHPEffect(attacker, attackerOwner.Player, out _);
                                                attacker.AdjustMP((int)spell.MPCost * -1);
                                                attackerOwner.Player.AdjustHP(hpMod, out _);
                                                attackerOwner.Send($"{attacker.Name} heals your wounds with the power of {spell}!{Constants.NewLine}");
                                                continue;
                                            }
                                            else
                                            {
                                                attackerOwner.Send($"{attacker.Name} wants to prepare a spell, but doesn't have the resources!{Constants.NewLine}");
                                            }
                                        }
                                        else
                                        {
                                            attackerOwner.Send($"{attacker.Name} wants to prepare a spell, but doesn't have the resources!{Constants.NewLine}");
                                        }
                                    }
                                }
                                var availBuffSpells = buffSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                if (availBuffSpells != null && availBuffSpells.Count > 0)
                                {
                                    spell = availBuffSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availBuffSpells.Count)];
                                    if (spell != null)
                                    {
                                        spell.ApplyBuffSpell(attacker, attackerOwner.Player, out bool hitsTarget, out int hpMod);
                                        attacker.AdjustMP((int)spell.MPCost * -1);
                                        if (hitsTarget)
                                        {
                                            attackerOwner.Send($"{attacker.Name} blesses you with the power of {spell}!{Constants.NewLine}");
                                            if (hpMod != 0)
                                            {
                                                attackerOwner.Player.AdjustHP(hpMod, out _);
                                            }
                                            continue;
                                        }
                                        else
                                        {
                                            attackerOwner.Send($"{attacker.Name} prepares a spell to assist you, but their magic fizzles and vanishes!{Constants.NewLine}");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        attackerOwner.Send($"{attacker.Name} wants to prepare a spell, but doesn't have the resources!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    attackerOwner.Send($"{attacker.Name} wants to prepare a spell, but doesn't have the resources!{Constants.NewLine}");
                                }
                                continue;
                            }
                            var hpCutOff = attacker.MaxHP / 2;
                            if (attacker.CurrentHP <= hpCutOff)
                            {
                                // NPC is less than half health so see if it can heal itself
                                var availHealSpells = healSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                if (availHealSpells != null && availHealSpells.Count > 0)
                                {
                                    spell = availHealSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availHealSpells.Count)];
                                    if (spell != null)
                                    {
                                        int hpMod = spell.CalculateSpellHPEffect(attacker, attacker, out _);
                                        attacker.AdjustMP((int)spell.MPCost * -1);
                                        attacker.AdjustHP(hpMod, out _);
                                        if (attacker.IsFollower)
                                        {
                                            attackerOwner.Send($"{attacker.Name} heals their wounds with a spell!{Constants.TabStop}");
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        if (attacker.IsFollower)
                                        {
                                            attackerOwner.Send($"{attacker.Name} wants to prepare a spell, but doesn't have the resources!{Constants.NewLine}");
                                        }
                                    }
                                }
                            }
                            if (damageSpells != null && damageSpells.Count > 0)
                            {
                                var availDamageSpells = damageSpells.Where(x => x.MPCost <= attacker.CurrentMP).ToList();
                                if (availDamageSpells != null && availDamageSpells.Count > 0)
                                {
                                    spell = availDamageSpells[new Random(DateTime.UtcNow.GetHashCode()).Next(0, availDamageSpells.Count)];
                                    if (spell != null)
                                    {
                                        attacker.AdjustMP((int)spell.MPCost * -1);
                                        var effect = spell.CalculateSpellHPEffect(attacker, defender, out bool hitsTarget);
                                        if (hitsTarget)
                                        {
                                            defender.AdjustHP(effect * -1, out bool isKilled);
                                            if (effect < 0)
                                            {
                                                // spell was absorbed by target
                                                if (attacker.IsFollower)
                                                {
                                                    attackerOwner.Send($"{attacker.Name} hurls {spell} at {defender.Name}, but the effect was absorbed!{Constants.NewLine}");
                                                }
                                                if (defender.IsFollower)
                                                {
                                                    defenderOwner.Send($"{attacker.Name} hurls {spell} at {defender.Name}, but they absorb the spell's effect!{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                // spell damaged target
                                                if (isKilled)
                                                {
                                                    if (attacker.IsFollower)
                                                    {
                                                        attackerOwner.Send($"{attacker.Name} has slain {defender.Name}!{Constants.NewLine}");
                                                    }
                                                    if (defender.IsFollower)
                                                    {
                                                        defenderOwner.Send($"{attacker.Name} has slain {defender.Name}!{Constants.NewLine}");
                                                    }
                                                    completedSessions.AddRange(GetCombatSessionsForCombatant(defender.NPCGuid));
                                                    defender.Kill(true, ref attackerOwner);
                                                    continue;
                                                }
                                                else
                                                {
                                                    if (attacker.IsFollower)
                                                    {
                                                        attackerOwner.Send($"{attacker.Name} hurls {spell} at {defender.Name}, blasting them for {effect} damage!{Constants.NewLine}");
                                                    }
                                                    if (defender.IsFollower)
                                                    {
                                                        defenderOwner.Send($"{attacker.Name} hurls {spell} at {defender.Name}, blasting them for {effect} damage!{Constants.NewLine}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (attacker.IsFollower)
                                            {
                                                attackerOwner.Send($"{attacker.Name} tries to cast {spell} at {defender.Name}, but their magic fizzles and the spell fails!{Constants.NewLine}");
                                            }
                                            if (defender.IsFollower)
                                            {
                                                defenderOwner.Send($"{attacker.Name} tries to cast {spell} at {defender.Name}, but their magic fizzles and the spell fails!{Constants.NewLine}");
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }
                            for (int a = 0; a < attacker.NumberOfAttacks; a++)
                            {
                                bool hits = attacker.DoHitRoll(defender, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical);
                                var wpn = attacker.EquipWeapon?.Name.ToLower() ?? "strike";
                                if (hits)
                                {
                                    var damage = attacker.DoDamageRoll(defender);
                                    if (isCritical)
                                    {
                                        damage *= 2;
                                        if (attacker.IsFollower)
                                        {
                                            attackerOwner.Send($"{attacker.Name} strikes a critical blow against {defender.Name}!{Constants.NewLine}");
                                        }
                                        if (defender.IsFollower)
                                        {
                                            defenderOwner.Send($"{attacker.Name} strikes a critical blow against {defender.Name}!{Constants.NewLine}");
                                        }
                                    }
                                    defender.AdjustHP((int)damage * -1, out bool isKilled);
                                    if (isKilled)
                                    {
                                        completedSessions.AddRange(GetCombatSessionsForCombatant(defender.NPCGuid));
                                        if (attacker.IsFollower)
                                        {
                                            attackerOwner.Send($"{attacker.Name} has slain {defender.Name}!{Constants.NewLine}");
                                        }
                                        if (defender.IsFollower)
                                        {
                                            defenderOwner.Send($"{defender.Name} has been slain by {attacker.Name}!{Constants.NewLine}");
                                        }
                                        defender.Kill(true, ref attackerOwner);
                                    }
                                    else
                                    {
                                        var pDmg = (uint)Math.Round((double)damage / defender.CurrentHP * 100, 0);
                                        if (attacker.IsFollower)
                                        {
                                            if (attackerOwner.Player.ShowDetailedRollInfo || attackerOwner.Player.Level >= Constants.ImmLevel)
                                            {
                                                attackerOwner.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): {attacker.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} {defender.Name} for {damage} damage!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                attackerOwner.Send($"{attacker.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} {defender.Name}!{Constants.NewLine}");
                                            }
                                        }
                                        if (defender.IsFollower)
                                        {
                                            if (defenderOwner.Player.ShowDetailedRollInfo || defenderOwner.Player.Level >= Constants.ImmLevel)
                                            {
                                                defenderOwner.Send($"{attacker.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} {defender.Name} for {damage}!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                defenderOwner.Send($"{attacker.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} {defender.Name}!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // one or both participants in the round was null so flag for removal and continue on
                            completedSessions.Add(session.Key);
                            continue;
                        }
                    }

                    // PVP combat, both participants are Players
                    if (session.Value.Attacker is Descriptor && session.Value.Defender is Descriptor)
                    {
                        Descriptor attacker = session.Value.Attacker as Descriptor;
                        Descriptor defender = session.Value.Defender as Descriptor;
                        if (attacker != null && defender != null)
                        {
                            for (int a = 0; a < attacker.Player.NumberOfAttacks; a++)
                            {
                                bool hits = attacker.Player.DoHitRoll(defender.Player, out uint baseHitRoll, out uint finalHitRoll, out bool isCritical);
                                var wpn = attacker.Player.EquipWeapon?.Name.ToLower() ?? "strike";
                                if (hits)
                                {
                                    var damage = attacker.Player.DoDamageRoll(defender.Player);
                                    if (isCritical)
                                    {
                                        damage *= 2;
                                        attacker.Send($"You have struck a critical blow against {defender.Player.Name}!{Constants.NewLine}");
                                        defender.Send($"{attacker.Player.Name} strikes a critical blow against you!{Constants.NewLine}");
                                    }
                                    defender.Player.AdjustHP((int)damage * -1, out bool isKilled);
                                    if (isKilled)
                                    {
                                        completedSessions.AddRange(GetCombatSessionsForCombatant(defender.ID));
                                        completedSessions.Add(session.Key);
                                        attacker.Send($"Your {wpn} hits {defender.Player.Name} for lethal damage, killing them!{Constants.NewLine}");
                                        defender.Send($"{attacker.Player.Name}'s {wpn} hits you for lethal damage, killing you!{Constants.NewLine}");
                                        defender.Player.Kill();
                                        break;
                                    }
                                    else
                                    {
                                        var pDmg = (uint)Math.Round((double)damage / defender.Player.CurrentHP * 100, 0);
                                        if (attacker.Player.ShowDetailedRollInfo || attacker.Player.Level >= Constants.ImmLevel)
                                        {
                                            attacker.Send($"ROLL: {baseHitRoll} ({finalHitRoll}): Your {wpn} {Helpers.GetDamageString(pDmg)} {defender.Player.Name} for {damage} damage!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            attacker.Send($"Your {wpn} {Helpers.GetDamageString(pDmg)} {defender.Player.Name}!{Constants.NewLine}");
                                        }
                                        if (defender.Player.ShowDetailedRollInfo || defender.Player.Level >= Constants.ImmLevel)
                                        {
                                            defender.Send($"{attacker.Player.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} you for {damage} damage!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            defender.Send($"{attacker.Player.Name}'s {wpn} {Helpers.GetDamageString(pDmg)} you!{Constants.NewLine}");
                                        }
                                    }
                                }
                                else
                                {
                                    defender.Send($"{attacker.Player.Name}'s {wpn} misses you!{Constants.NewLine}");
                                }
                            }
                        }
                        else
                        {
                            // one or both participants in the round was null so flag for removal and continue on
                            completedSessions.Add(session.Key);
                            continue;
                        }
                    }
                }
            }
        }
    }
}
