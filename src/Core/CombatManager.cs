using Kingdoms_of_Etrea.Interfaces;
using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Kingdoms_of_Etrea.Core
{
    internal class CombatManager
    {
        // https://github.com/DavidRieman/WheelMUD/blob/main/src/WarriorRogueMage/Combat/WRMCombat.cs
        // https://github.com/Arthmoor/AFKMud/blob/master/src/fight.cpp
        private static CombatManager _instnace = null;
        private static readonly object _lockObject = new object();
        private static ILoggingProvider _loggingProvider = new LoggingProvider();
        private Dictionary<Guid, CombatSession> CombatQueue;

        private CombatManager()
        {
            CombatQueue = new Dictionary<Guid, CombatSession>();
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

        internal bool IsNPCInCombat(Guid npcID)
        {
            if(CombatQueue.Count > 0)
            {
                lock(_lockObject)
                {
                    foreach (var _ in from s in CombatQueue
                                      from p in s.Value.Participants
                                          //where p.Attacker.Type == ActorType.NonPlayer
                                      where p.Attacker is NPC
                                      where (p.Attacker as NPC).NPCGuid == npcID
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

        internal Guid GetNPCGuidFromCombatSession(Guid sessionID)
        {
            Guid retval = Guid.Empty;
            if(CombatQueue.Count > 0 )
            {
                lock(_lockObject)
                {
                    if(CombatQueue.ContainsKey(sessionID))
                    {
                        var participant = CombatQueue[sessionID].Participants.Where(p => p.Attacker is NPC).FirstOrDefault();
                        return (participant.Attacker as NPC).NPCGuid;
                    }
                }
            }
            return retval;
        }

        internal void ProcessCombatRound(CombatSession session, Guid combatSessionID, out bool combatFinished)
        {
            combatFinished = false;
            var participants = session.SortCombatList();
            if(participants.Count == 0)
            {
                combatFinished = true;
                return;
            }
            List<(uint init, dynamic Attacker, dynamic Target)> finishedSessions = new List<(uint init, dynamic Attacker, dynamic Target)>();
            foreach(var p in participants)
            {
                // null check here in case the participant is dead...
                Descriptor pAttackingPlayer = p.Attacker is Descriptor ? SessionManager.Instance.GetPlayerByGUID((p.Attacker as Descriptor).Id) : null;
                NPC pAttackingNPC = p.Attacker is NPC ? NPCManager.Instance.GetNPCByGUID((p.Attacker as NPC).NPCGuid) : null;
                Descriptor pTargetPlayer = p.Target is Descriptor ? SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id) : null;
                NPC pTargetNPC = p.Target is NPC ? NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid) : null;
                bool attackerNull = pAttackingPlayer == null && pAttackingNPC == null;
                bool targetNull = pTargetPlayer == null && pTargetNPC == null;
                if(attackerNull && targetNull)
                {
                    // both combatants are null, so something strange has happened - break combat
                    combatFinished = true;
                    return;
                }
                if((!attackerNull && targetNull) || (attackerNull && !targetNull))
                {
                    // attacker but no target or no attacker but target
                    finishedSessions.Add(p);
                    break;
                }
                var attackerHP = pAttackingPlayer != null ? pAttackingPlayer.Player.Stats.CurrentHP : pAttackingNPC.Stats.CurrentHP;
                var targetHP = pTargetPlayer != null ? pTargetPlayer.Player.Stats.CurrentHP : pTargetNPC.Stats.CurrentHP;
                if (attackerHP <= 0 || targetHP <= 0)
                {
                    // one of the round participants is dead, so return from processing
                    combatFinished = true;
                    if(participants.Count == 1)
                    {
                        // if we only have one group of particpants, end the session
                        combatFinished = true;
                    }
                    else
                    {
                        // add the current participant group to a list to remove from the session
                        finishedSessions.Add(p);
                    }
                    if(attackerHP > 0 && targetHP <= 0)
                    {
                        // attacker won
                        if(p.Attacker is Descriptor)
                        {
                            if(p.Target is NPC)
                            {
                                // attacking player beat an NPC
                                var n = p.Target as NPC;
                                var player = p.Attacker as Descriptor;
                                player.Send($"You have beaten {n.Name} and obtained {n.BaseExpAward} Exp and {n.Stats.Gold} gold!{Constants.NewLine}");
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.AddExp(n.BaseExpAward, ref player);
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.AddGold(n.Stats.Gold, ref player);
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(n.CurrentRoom);
                                NPCManager.Instance.GetNPCByGUID(n.NPCGuid).Kill(true);
                                if(player.Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(n.NPCID)))
                                {
                                    for(int i = 0; i < player.Player.ActiveQuests.Count; i++)
                                    {
                                        if (player.Player.ActiveQuests[i].Monsters.Keys.Contains(n.NPCID))
                                        {
                                            if (player.Player.ActiveQuests[i].Monsters[n.NPCID] <= 1)
                                            {
                                                player.Player.ActiveQuests[i].Monsters[n.NPCID] = 0;
                                            }
                                            else
                                            {
                                                player.Player.ActiveQuests[i].Monsters[n.NPCID]--;
                                            }
                                        }
                                    }
                                }
                                combatFinished = true;
                            }
                            else
                            {
                                // attacking player beat another player
                                var target = p.Target as Descriptor;
                                var attacker = p.Attacker as Descriptor;
                                target.Player.Kill(ref target);
                                target.Send($"You have been killed by {attacker.Player.Name}!{Constants.NewLine}");
                                attacker.Send($"You have killed {target.Player.Name}!{Constants.NewLine}");
                                combatFinished = true;
                            }
                        }
                        else
                        {
                            if(p.Target is Descriptor)
                            {
                                // attacking NPC beat a player
                                var target = p.Target as Descriptor;
                                var attacker = p.Attacker as NPC;
                                target.Player.Kill(ref target);
                                target.Send($"You have been killed by {attacker.Name}!{Constants.NewLine}");
                            }
                            else
                            {
                                // attacking NPC beat another NPC
                                var attacker = p.Attacker as NPC;
                                var target = p.Target as NPC;
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(target.CurrentRoom);
                                NPCManager.Instance.GetNPCByGUID(target.NPCGuid).Kill(true);
                            }
                        }
                    }
                    else
                    {
                        // target won
                        if(p.Target is Descriptor)
                        {
                            if(p.Attacker is NPC)
                            {
                                // target player beat an attacking npc
                                var n = p.Attacker as NPC;
                                var player = p.Target as Descriptor;
                                player.Send($"You have beaten {n.Name} and obtained {n.BaseExpAward} Exp and {n.Stats.Gold} gold!{Constants.NewLine}");
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.AddGold(n.Stats.Gold, ref player);
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.AddExp(n.BaseExpAward, ref player);
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(n.CurrentRoom);
                                NPCManager.Instance.GetNPCByGUID(n.NPCGuid).Kill(true);
                                if (player.Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(n.NPCID)))
                                {
                                    for (int i = 0; i < player.Player.ActiveQuests.Count; i++)
                                    {
                                        if (player.Player.ActiveQuests[i].Monsters.Keys.Contains(n.NPCID))
                                        {
                                            if (player.Player.ActiveQuests[i].Monsters[n.NPCID] <= 1)
                                            {
                                                player.Player.ActiveQuests[i].Monsters[n.NPCID] = 0;
                                            }
                                            else
                                            {
                                                player.Player.ActiveQuests[i].Monsters[n.NPCID]--;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // target player beat an attacking player
                                var target = p.Target as Descriptor;
                                var attacker = p.Attacker as Descriptor;
                                target.Player.Kill(ref target);
                                target.Send($"You have been killed by {attacker.Player.Name}!{Constants.NewLine}");
                                attacker.Send($"You have killed {target.Player.Name}!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            if(p.Attacker is Descriptor)
                            {
                                // target npc beat attacking player
                                var attacker = p.Attacker as Descriptor;
                                var target = p.Target as NPC;
                                attacker.Player.Kill(ref attacker);
                                attacker.Send($"You have been killed by {target.Name}!{Constants.NewLine}");
                            }
                            else
                            {
                                // target npc beat attacking NPC
                                var attacker = p.Attacker as NPC;
                                var target = p.Target as NPC;
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(target.CurrentRoom);
                                attacker.Kill(true);
                            }
                        }
                    }
                    return;
                }
                // both attacker and target are still alive, so process the round
                if(p.Attacker is NPC)
                {
                    var attacker = (NPC)p.Attacker;
                    Descriptor attackerOwner = null;
                    Descriptor targetOwner = null;
                    if(p.Attacker is NPC)
                    {
                        attackerOwner = SessionManager.Instance.GetPlayerByGUID((p.Attacker as NPC).FollowingPlayer);
                    }
                    if(p.Target is NPC)
                    {
                        targetOwner = SessionManager.Instance.GetPlayerByGUID((p.Target as NPC).FollowingPlayer);
                    }
                    if((p.Attacker as NPC).BehaviourFlags.HasFlag(NPCFlags.Coward))
                    {
                        var fleePoint = Convert.ToUInt32(Math.Round(attacker.Stats.MaxHP * 0.18));
                        var attackerCurrentHP = NPCManager.Instance.GetNPCByGUID((p.Attacker as NPC).NPCGuid).Stats.CurrentHP;
                        if(attackerCurrentHP < fleePoint)
                        {
                            if(attacker.FleeCombat(ref _loggingProvider, out uint fleeTo))
                            {
                                combatFinished = true;
                                if(p.Target is Descriptor)
                                {
                                    (p.Target as Descriptor).Send($"{attacker.Name} breaks combat and flees!{Constants.NewLine}");
                                    (p.Target as Descriptor).Player.CombatSessionID = Guid.Empty;
                                    attacker.Move(ref attacker, attacker.CurrentRoom, fleeTo, false);
                                    return;
                                }
                            }
                            else
                            {
                                if(p.Target is Descriptor)
                                {
                                    (p.Target as Descriptor).Send($"{attacker.Name} looks like fleeing but can't break combat!");
                                }
                            }
                        }
                    }
                    bool npcUseBasicAttack = true;
                    if(attacker.Skills != null && attacker.Skills.Count > 0)
                    {
                        var mobCurHP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentHP;
                        var mobMaxHP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.MaxHP;
                        var mobPercHealth = (double)mobCurHP / mobMaxHP * 100.0;
                        if(mobPercHealth <= 30 && attacker.HasSkill("Desperate Attack"))
                        {
                            if(!attacker.HasBuff("Desperate Attack") && attacker.Stats.CurrentMP >= Skills.GetSkill("Desperate Attack").MPCost)
                            {
                                attacker.Stats.CurrentMP -= (int)Skills.GetSkill("Deserpate Attack").MPCost;
                                attacker.AddBuff("Desperate Attack");
                                if(p.Target is Descriptor)
                                {
                                    (p.Target as Descriptor).Send($"{attacker.Name} gets ready for a desperate attack!{Constants.NewLine}");
                                }
                            }
                        }
                    }
                    if(npcUseBasicAttack)
                    {
                        if(attacker.Spells != null && attacker.Spells.Count > 0)
                        {
                            bool healPlayer = false, healSelf = false;
                            if(attacker.FollowingPlayer != Guid.Empty)
                            {
                                // NPC is a follower so see if the owning player is injured and heal
                                var mobOwner = SessionManager.Instance.GetPlayerByGUID(attacker.FollowingPlayer);
                                if(mobOwner != null)
                                {
                                    var ownerPercHealth = (double)mobOwner.Player.Stats.CurrentHP / mobOwner.Player.Stats.MaxHP * 100.0;
                                    if(ownerPercHealth <= 45)
                                    {
                                        var mobHealingSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Healing).ToList();
                                        if(mobHealingSpells.Count > 0)
                                        {
                                            var rnd = new Random(DateTime.Now.GetHashCode());
                                            var healSpell = mobHealingSpells[rnd.Next(mobHealingSpells.Count)];
                                            var mobMP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentMP;
                                            if (mobMP >= healSpell.MPCost)
                                            {
                                                var hpRecovered = Helpers.RollDice(healSpell.NumOfDamageDice, healSpell.SizeOfDamageDice);
                                                NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)healSpell.MPCost * -1);
                                                mobOwner.Send($"{attacker.Name} prepares a spell to heal your wounds!{Constants.NewLine}");
                                                healPlayer = true;
                                                if (ActorStats.CalculateAbilityModifier(attacker.Stats.Wisdom) > 0)
                                                {
                                                    // add bonus recovery if the NPC has a high Wisdom stat
                                                    hpRecovered += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Wisdom) * healSpell.NumOfDamageDice);
                                                }
                                                if (mobOwner.Player.Stats.CurrentHP + hpRecovered > mobOwner.Player.Stats.MaxHP)
                                                {
                                                    mobOwner.Player.Stats.CurrentHP = (int)mobOwner.Player.Stats.MaxHP;
                                                }
                                                else
                                                {
                                                    mobOwner.Player.Stats.CurrentHP += (int)hpRecovered;
                                                }
                                            }
                                        }
                                    }
                                }
                                // see if the NPC wants to heal itself
                                var mobCurrentHP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentHP;
                                var mobMaxHP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.MaxHP;
                                var mobCurrentMP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentMP;
                                var mobHPPerc = (double)mobCurrentHP / mobMaxHP * 100.0;
                                if(mobHPPerc <= 40)
                                {
                                    var mobHealingSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Healing).ToList();
                                    if(mobHealingSpells.Count > 0)
                                    {
                                        var rnd = new Random(DateTime.Now.GetHashCode());
                                        var healSpell = mobHealingSpells[rnd.Next(mobHealingSpells.Count)];
                                        if (mobCurrentMP >= healSpell.MPCost)
                                        {
                                            var hpRecovered = Helpers.RollDice(healSpell.NumOfDamageDice, healSpell.SizeOfDamageDice);
                                            NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)healSpell.MPCost * -1);
                                            healSelf = true;
                                            if (ActorStats.CalculateAbilityModifier(attacker.Stats.Wisdom) > 0)
                                            {
                                                // bonus healing for high Wisdom stat
                                                hpRecovered += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Wisdom) * healSpell.NumOfDamageDice);
                                            }
                                            if (mobCurrentHP + hpRecovered > mobMaxHP)
                                            {
                                                NPCManager.Instance.SetNPCHealthToMax(attacker.NPCGuid);
                                            }
                                            else
                                            {
                                                NPCManager.Instance.AdjustNPCHealth(attacker.NPCGuid, (int)hpRecovered);
                                            }
                                        }
                                    }
                                }
                                npcUseBasicAttack = !healPlayer || !healSelf; // if the NPC has healed itself or its owner player this turn, no attacks for them
                            }
                            if(npcUseBasicAttack)
                            {
                                // we didn't heal this turn, so see if the NPC has any attacking spells to use on the target
                                var attackSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Damage).ToList();
                                if(attackSpells != null && attackSpells.Count > 0)
                                {
                                    var rnd = new Random(DateTime.Now.GetHashCode());
                                    var attackSpell = attackSpells[rnd.Next(attackSpells.Count)];
                                    var mobCurrentMP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentMP;
                                    if(mobCurrentMP >= attackSpell.MPCost)
                                    {
                                        NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)attackSpell.MPCost * -1);
                                        npcUseBasicAttack = false;
                                        var damage = Convert.ToInt64(Helpers.RollDice(attackSpell.NumOfDamageDice, attackSpell.SizeOfDamageDice));
                                        if(ActorStats.CalculateAbilityModifier(attacker.Stats.Intelligence) > 0)
                                        {
                                            damage += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Intelligence) * attackSpell.NumOfDamageDice);
                                        }
                                        bool spellHits = true;
                                        if(!attackSpell.AutoHitTarget)
                                        {
                                            var toHit = Helpers.RollDice(1, 20);
                                            if(ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity) > 0)
                                            {
                                                toHit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity));
                                            }
                                            spellHits = toHit > p.Target.Stats.ArmourClass;
                                        }
                                        if(spellHits)
                                        {
                                            if(p.Target is Descriptor)
                                            {
                                                if(damage >= (p.Target as Descriptor).Player.Stats.CurrentHP)
                                                {
                                                    // spell killed a player
                                                    var t = SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id);
                                                    t.Send($"{attacker.Name}'s {attackSpell.SpellName} kills you!{Constants.NewLine}");
                                                    t.Player.Kill(ref t);
                                                    combatFinished = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    // spell hit and damaged a player
                                                    var d = Convert.ToUInt32(damage);
                                                    SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id).Player.Stats.CurrentHP -= (int)d;
                                                    (p.Target as Descriptor).Send($"{attacker.Name} blasts you with {attackSpell.SpellName} for {d} damage!{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                // mob attack spell hit another mob
                                                // TODO: Check the code in the basic attack section - need to deal with when the dead mob is a follower or when a follower
                                                // uses magic to kill an enemy mob and correctly end combat, or leave it going
                                                if(damage >= (NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid).Stats.CurrentHP))
                                                {
                                                    // spell killed an NPC
                                                    var t = NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid);
                                                    //t.Kill(ref t, out bool dropsLoot);
                                                    t.Kill(true);
                                                    combatFinished = true;
                                                    break;
                                                }
                                                if(damage <= 0)
                                                {
                                                    // spell hit the mob but was absorbed
                                                    NPCManager.Instance.AdjustNPCHealth((p.Target as NPC).NPCGuid, (int)damage * -1);
                                                }
                                                else
                                                {
                                                    // spell damaged an NPC
                                                    NPCManager.Instance.AdjustNPCHealth((p.Target as NPC).NPCGuid, (int)damage);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if(p.Target is Descriptor)
                                            {
                                                (p.Target as Descriptor).Send($"{attacker.Name} tries to cast a spell but the Winds of Magic turn against them!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                                if(npcUseBasicAttack)
                                {
                                    // no attack spells, so try and buff ourselves or the Owner (if there is one)
                                    var buffSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Buff).ToList();
                                    if(buffSpells != null && buffSpells.Count > 0)
                                    {
                                        bool buffPlayer = false, buffSelf = false;
                                        if (attacker.FollowingPlayer != Guid.Empty)
                                        {
                                            var rnd = new Random(DateTime.Now.GetHashCode());
                                            var buffSpell = buffSpells[rnd.Next(buffSpells.Count)];
                                            var mobMP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentMP;
                                            if(mobMP >= buffSpell.MPCost)
                                            {
                                                //var owner = SessionManager.Instance.GetPlayerByGUID(attacker.FollowingPlayer);
                                                if(attackerOwner != null)
                                                {
                                                    NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)buffSpell.MPCost * -1);
                                                    if (!attackerOwner.Player.HasBuff(buffSpell.SpellName))
                                                    {
                                                        attackerOwner.Player.AddBuff(buffSpell.SpellName);
                                                        attackerOwner.Send($"{attacker.Name} blesses you with the power of {buffSpell.SpellName}!{Constants.NewLine}");
                                                        buffPlayer = true;
                                                        if(p.Target is Descriptor)
                                                        {
                                                            (p.Target as Descriptor).Send($"{attacker.Name} blesses {attackerOwner.Player.Name} with the power of {buffSpell.SpellName}!{Constants.NewLine}");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if(!buffPlayer)
                                        {
                                            var rnd = new Random(DateTime.Now.GetHashCode());
                                            var buffSpell = buffSpells[rnd.Next(buffSpells.Count)];
                                            var mobMP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentMP;
                                            if (mobMP >= buffSpell.MPCost)
                                            {
                                                if (!attacker.HasBuff(buffSpell.SpellName))
                                                {
                                                    NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)buffSpell.MPCost * -1);
                                                    attacker.AddBuff(buffSpell.SpellName);
                                                    buffSelf = true;
                                                    if(attacker.FollowingPlayer != Guid.Empty)
                                                    {
                                                        //var owner = SessionManager.Instance.GetPlayerByGUID(attacker.FollowingPlayer);
                                                        if(attackerOwner != null)
                                                        {
                                                            attackerOwner.Send($"{attacker.Name} blesses themselves with the power of {buffSpell.SpellName}!{Constants.NewLine}");
                                                        }
                                                    }
                                                    if(p.Target is Descriptor)
                                                    {
                                                        (p.Target as Descriptor).Send($"{attacker.Name} blesses themselves with the power of {buffSpell.SpellName}!{Constants.NewLine}");
                                                    }
                                                }
                                            }
                                        }
                                        npcUseBasicAttack = !buffSelf || !buffPlayer;
                                    }
                                }
                                if(npcUseBasicAttack)
                                {
                                    // no healing, attack or buff spells cast - lastly check debuff to see if there's anything to do here
                                    var debuffSpells = attacker.Spells.Where(x => x.SpellType == SpellType.Debuff).ToList();
                                    if(debuffSpells != null && debuffSpells.Count > 0)
                                    {
                                        var rnd = new Random(DateTime.Now.GetHashCode());
                                        var debuffSpell = debuffSpells[rnd.Next(debuffSpells.Count)];
                                        var mobMP = NPCManager.Instance.GetNPCByGUID(attacker.NPCGuid).Stats.CurrentMP;
                                        if(mobMP >= debuffSpell.MPCost)
                                        {
                                            if(p.Target is NPC)
                                            {
                                                var t = p.Target as NPC;
                                                if(!t.HasBuff(debuffSpell.SpellName))
                                                {
                                                    npcUseBasicAttack = false;
                                                    NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)debuffSpell.MPCost * -1);
                                                    bool spellHits = true;
                                                    if(!debuffSpell.AutoHitTarget)
                                                    {
                                                        var toHit = Helpers.RollDice(1, 20);
                                                        if(ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity) > 0)
                                                        {
                                                            toHit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity));
                                                        }
                                                        spellHits = toHit >= t.Stats.ArmourClass;
                                                    }
                                                    if(spellHits)
                                                    {
                                                        NPCManager.Instance.GetNPCByGUID(t.NPCGuid).AddBuff(debuffSpell.SpellName);
                                                        if(debuffSpell.NumOfDamageDice > 0)
                                                        {
                                                            var dmg = Helpers.RollDice(debuffSpell.NumOfDamageDice, debuffSpell.SizeOfDamageDice);
                                                            if(ActorStats.CalculateAbilityModifier(attacker.Stats.Intelligence) > 0)
                                                            {
                                                                dmg += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Intelligence));
                                                            }
                                                            var tHP = NPCManager.Instance.GetNPCByGUID(t.NPCGuid).Stats.CurrentHP;
                                                            if(dmg >= tHP)
                                                            {
                                                                // killed the npc
                                                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(t.CurrentRoom);
                                                                t.Kill(true);
                                                            }
                                                            else
                                                            {
                                                                NPCManager.Instance.AdjustNPCHealth(t.NPCGuid, (int)dmg);
                                                            }
                                                        }
                                                        if(attacker.FollowingPlayer != Guid.Empty)
                                                        {
                                                            attackerOwner.Send($"{attacker.Name} calls on the power of {debuffSpell.SpellName} to hinder {t.Name}!{Constants.NewLine}");
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var t = p.Target as Descriptor;
                                                if(!t.Player.HasBuff(debuffSpell.SpellName))
                                                {
                                                    npcUseBasicAttack = false;
                                                    NPCManager.Instance.AdjustNPCMana(attacker.NPCGuid, (int)debuffSpell.MPCost * -1);
                                                    bool spellHits = true;
                                                    if(!debuffSpell.AutoHitTarget)
                                                    {
                                                        var toHit = Helpers.RollDice(1, 20);
                                                        if(ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity) > 0)
                                                        {
                                                            toHit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity));
                                                        }
                                                        spellHits = toHit >= t.Player.Stats.ArmourClass;
                                                    }
                                                    if(spellHits)
                                                    {
                                                        SessionManager.Instance.GetPlayerByGUID(t.Id).Player.AddBuff(debuffSpell.SpellName);
                                                        if(debuffSpell.NumOfDamageDice > 0)
                                                        {
                                                            var dmg = Helpers.RollDice(debuffSpell.NumOfDamageDice, debuffSpell.SizeOfDamageDice);
                                                            if(ActorStats.CalculateAbilityModifier(attacker.Stats.Intelligence) > 0)
                                                            {
                                                                dmg += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(attacker.Stats.Intelligence));
                                                            }
                                                            if(dmg >= SessionManager.Instance.GetPlayerByGUID(t.Id).Player.Stats.CurrentHP)
                                                            {
                                                                // kill player
                                                                t.Send($"{attacker.Name}'s {debuffSpell.SpellName} has dealt you lethal damage!{Constants.NewLine}");
                                                                SessionManager.Instance.GetPlayerByGUID(t.Id).Player.Kill(ref t);
                                                            }
                                                            else
                                                            {
                                                                t.Send($"{attacker.Name}'s {debuffSpell.SpellName} deals {dmg} damage to you!{Constants.NewLine}");
                                                                SessionManager.Instance.GetPlayerByGUID(t.Id).Player.Stats.CurrentHP -= (int)dmg;
                                                            }
                                                        }
                                                        t.Send($"{attacker.Name} calls on the power of {debuffSpell.SpellName} to hinder you!{Constants.NewLine}");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if(npcUseBasicAttack)
                    {
                        // we've been through skills and spells, if we get here the NPC is just swinging their weapon
                        for (int i = 0; i < attacker.NumberOfAttacks; i++)
                        {
                            InventoryItem weapon = null;
                            if(attacker.EquippedItems != null && attacker.EquippedItems.Weapon != null)
                            {
                                weapon = attacker.EquippedItems.Weapon;
                            }
                            var baseHitRoll = Helpers.RollDice(1, 20);
                            int finalHitRoll = weapon != null && weapon.IsRanged ? Convert.ToInt32(baseHitRoll + ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity)) 
                                : Convert.ToInt32(baseHitRoll + ActorStats.CalculateAbilityModifier(attacker.Stats.Strength));
                            // ROLL MODIFIERS
                            finalHitRoll += attacker.HasBuff("Truestrike") ? 10 : 0;
                            finalHitRoll -= attacker.HasBuff("Desperate Attack") ? 4 : 0;
                            if(baseHitRoll != 1)
                            {
                                uint baseDamage = 0;
                                int finalDamage = 0;
                                if(weapon != null)
                                {
                                    var abilityMod = weapon.IsRanged ? ActorStats.CalculateAbilityModifier(attacker.Stats.Dexterity)
                                        : ActorStats.CalculateAbilityModifier(attacker.Stats.Strength);
                                    baseDamage = Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice);
                                    finalDamage = Convert.ToInt32(baseDamage + abilityMod);
                                }
                                else
                                {
                                    baseDamage = Helpers.RollDice(1, 2);
                                    finalDamage = Convert.ToInt32(baseDamage + ActorStats.CalculateAbilityModifier(attacker.Stats.Strength));
                                    finalDamage = finalDamage < 1 ? 1 : finalDamage; // ensure we can never do less than 1 damage when we hit
                                }
                                if(baseHitRoll == 20)
                                {
                                    // critical hit, double the final damage
                                    finalDamage *= 2;
                                }
                                // DAMAGE MODIFIERS FOR SKILLS / BUFFS
                                finalDamage += attacker.HasBuff("Desperate Attack") ? 4 : 0;
                                finalDamage = finalDamage < 1 ? 1 : finalDamage; // if we hit, we should always do at least 1 point of damage
                                var wStr = weapon != null ? weapon.Name.ToLower() : "strike";
                                if(p.Target is Descriptor)
                                {
                                    var tPlayer = SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id);
                                    if(baseHitRoll == 20 || finalHitRoll >= tPlayer.Player.Stats.ArmourClass)
                                    {
                                        if(finalDamage >= tPlayer.Player.Stats.CurrentHP)
                                        {
                                            tPlayer.Send($"{attacker.Name}'s {wStr} kills you!{Constants.NewLine}");
                                            tPlayer.Player.Kill(ref tPlayer);
                                            if(attackerOwner != null)
                                            {
                                                attackerOwner.Send($"{attacker.Name}'s {wStr} kills {tPlayer.Player}!{Constants.NewLine}");
                                            }
                                        }
                                        else
                                        {
                                            SessionManager.Instance.GetPlayerByGUID(tPlayer.Id).Player.Stats.CurrentHP -= finalDamage;
                                            var msg = tPlayer.Player.Level >= Constants.ImmLevel || tPlayer.Player.ShowDetailedRollInfo
                                                ? $"{attacker.Name}'s {wStr} hits you for {finalDamage} damage!{Constants.NewLine}"
                                                : $"{attacker.Name}'s {wStr} hits you!{Constants.NewLine}";
                                            tPlayer.Send(msg);
                                            if(attackerOwner != null)
                                            {
                                                var ownerMsg = attackerOwner.Player.Level >= Constants.ImmLevel || attackerOwner.Player.ShowDetailedRollInfo
                                                    ? $"{attacker.Name}'s {wStr} hits {tPlayer.Player} for {finalDamage} damage!{Constants.NewLine}"
                                                    : $"{attacker.Name}'s {wStr} hits {tPlayer.Player}!{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        tPlayer.Send($"{attacker.Name}'s {wStr} misses you!{Constants.NewLine}");
                                        if(attackerOwner != null)
                                        {
                                            attackerOwner.Send($"{attacker.Name}'s {wStr} misses {tPlayer.Player}!{Constants.NewLine}");
                                        }
                                    }
                                }
                                else
                                {
                                    // NPC vs NPC
                                    var tNPC = NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid);
                                    if(tNPC != null)
                                    {
                                        if (baseHitRoll == 20 || finalHitRoll >= tNPC.Stats.ArmourClass)
                                        {
                                            if (finalDamage >= tNPC.Stats.CurrentHP)
                                            {
                                                if (attackerOwner != null)
                                                {
                                                    attackerOwner.Send($"{attacker.Name}'s {wStr} hits {tNPC.Name} for lethal damage, killing it!{Constants.NewLine}");
                                                    attackerOwner.Send($"You gain {tNPC.BaseExpAward} Exp and {tNPC.Stats.Gold} gold!{Constants.NewLine}");
                                                    SessionManager.Instance.GetPlayerByGUID(attackerOwner.Id).Player.AddExp(tNPC.BaseExpAward, ref attackerOwner);
                                                    SessionManager.Instance.GetPlayerByGUID(attackerOwner.Id).Player.AddGold(tNPC.Stats.Gold, ref attackerOwner);
                                                    combatFinished = true;
                                                }
                                                if(targetOwner != null)
                                                {
                                                    targetOwner.Send($"{attacker.Name}'s {wStr} hits {tNPC.Name} for lethal damage, killing them instantly!{Constants.NewLine}");
                                                }
                                                tNPC.Kill(true);
                                            }
                                            else
                                            {
                                                if (attackerOwner != null)
                                                {
                                                    attackerOwner.Send($"{attacker.Name}'s {wStr} hits {tNPC.Name} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                                if(targetOwner != null)
                                                {
                                                    targetOwner.Send($"{attacker.Name}'s {wStr} hits {tNPC.Name} for {finalDamage} damage!{Constants.NewLine}");
                                                }
                                                tNPC.Stats.CurrentHP -= finalDamage;
                                            }
                                        }
                                        else
                                        {
                                            // attack missed
                                            if (attackerOwner != null)
                                            {
                                                attackerOwner.Send($"{attacker.Name}'s {wStr} misses {tNPC.Name}!{Constants.NewLine}");
                                            }
                                            if(targetOwner != null)
                                            {
                                                targetOwner.Send($"{attacker.Name}'s {wStr} misses {tNPC.Name}!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // attacker is a player so just process regular attacks, players can use skills and spells themselves
                    var attacker = p.Attacker as Descriptor;
                    for(int i = 0; i < attacker.Player.NumberOfAttacks; i++)
                    {
                        if(p.Target is NPC)
                        {
                            var n = NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid);
                            if(n != null)
                            {
                                InventoryItem weapon = null;
                                uint damage = 0;
                                int hitMod = 0;
                                if(attacker.Player.EquippedItems != null && attacker.Player.EquippedItems.Weapon != null)
                                {
                                    weapon = attacker.Player.EquippedItems.Weapon;
                                }
                                if(weapon != null)
                                {
                                    damage = Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice);
                                    var abilityMod = weapon.IsRanged ? ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Dexterity)
                                        : ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                                    int damageMod = Convert.ToInt32(damage + abilityMod);
                                    damage = damageMod < 1 ? 1 : Convert.ToUInt32(damageMod);
                                    hitMod = weapon.IsRanged ? ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Dexterity)
                                        : ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                                }
                                else
                                {
                                    damage = Helpers.RollDice(1, 2);
                                    var abilityMod = ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                                    int damageMod = Convert.ToInt32(damage + abilityMod);
                                    damage = damageMod < 1 ? 1 : Convert.ToUInt32(damageMod);
                                    hitMod = ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                                }
                                var toHit = Helpers.RollDice(1, 20);
                                hitMod += attacker.Player.HasBuff("Truestrike") ? 10 : 0;
                                hitMod -= attacker.Player.HasBuff("Desperate Attack") ? 4 : 0;
                                int finalHitRoll = Convert.ToInt32(toHit + hitMod);
                                if(toHit == 20)
                                {
                                    damage *= 2; // double damage on a critical hit
                                }
                                var targetAC = (p.Target is Descriptor) ? SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id).Player.Stats.ArmourClass
                                    : NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid).Stats.ArmourClass;
                                var wStr = weapon != null ? weapon.Name.ToLower() : "strike";
                                if(toHit > 1)
                                {
                                    if (finalHitRoll == 20 || finalHitRoll >= targetAC)
                                    {
                                        // player hit the target with their weapon
                                        if (attacker.Player.Level >= Constants.ImmLevel || attacker.Player.ShowDetailedRollInfo)
                                        {
                                            attacker.Send($"You roll {toHit} (Modified: {finalHitRoll}) and hit {n.Name} with your {wStr} for {damage} damage!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            attacker.Send($"Your {wStr} hits {n.Name} for {damage} damage!{Constants.NewLine}");
                                        }
                                        if (damage >= (NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid).Stats.CurrentHP))
                                        {
                                            // player killed the NPC
                                            var npc = NPCManager.Instance.GetNPCByGUID((p.Target as NPC).NPCGuid);
                                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(npc.CurrentRoom);
                                            attacker.Send($"You have killed {npc.Name} and gained {npc.BaseExpAward} Exp and {npc.Stats.Gold} gold!{Constants.NewLine}");
                                            SessionManager.Instance.GetPlayerByGUID(attacker.Id).Player.AddExp(n.BaseExpAward, ref attacker);
                                            SessionManager.Instance.GetPlayerByGUID(attacker.Id).Player.AddGold(n.Stats.Gold, ref attacker);
                                            npc.Kill(true);
                                            if (attacker.Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(n.NPCID)))
                                            {
                                                for (int q = 0; q < attacker.Player.ActiveQuests.Count; q++)
                                                {
                                                    if (attacker.Player.ActiveQuests[q].Monsters.Keys.Contains(n.NPCID))
                                                    {
                                                        if (attacker.Player.ActiveQuests[q].Monsters[n.NPCID] <= 1)
                                                        {
                                                            attacker.Player.ActiveQuests[q].Monsters[n.NPCID] = 0;
                                                        }
                                                        else
                                                        {
                                                            attacker.Player.ActiveQuests[q].Monsters[n.NPCID]--;
                                                        }
                                                    }
                                                }
                                            }
                                            combatFinished = true;
                                            break;
                                        }
                                        else
                                        {
                                            // player damaged the NPC
                                            NPCManager.Instance.AdjustNPCHealth((p.Target as NPC).NPCGuid, (int)damage * -1);
                                        }
                                    }
                                    else
                                    {
                                        // player missed the target
                                        if (attacker.Player.Level >= Constants.ImmLevel || attacker.Player.ShowDetailedRollInfo)
                                        {
                                            attacker.Send($"You roll {toHit} (Modified: {finalHitRoll}) and miss {n.Name} with your {wStr}!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            attacker.Send($"Your {wStr} misses {n.Name}!{Constants.NewLine}");
                                        }
                                    }
                                }
                                else
                                {
                                    attacker.Send($"You fumble your attack and miss {n.Name}!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                // couldn't get the NPC from NPCManager, so finish the combat session
                                // this can happen if the NPC is purged by an IMM or hit with a spell outside CombatManager
                                combatFinished = true;
                            }
                        }
                        else
                        {
                            // target is another player
                            InventoryItem weapon = null;
                            int damage = 0;
                            string wStr = "strike";
                            if(attacker.Player.EquippedItems != null && attacker.Player.EquippedItems.Weapon != null)
                            {
                                weapon = attacker.Player.EquippedItems.Weapon;
                                wStr = weapon.Name.ToLower();
                            }
                            int toHit = Convert.ToInt32(Helpers.RollDice(1, 20));
                            int hitMod = toHit;
                            if(weapon != null)
                            {
                                hitMod += weapon.IsRanged ? ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Dexterity)
                                    : ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                                damage = Convert.ToInt32(Helpers.RollDice(weapon.NumberOfDamageDice, weapon.SizeOfDamageDice));
                                damage += weapon.IsRanged ? ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Dexterity)
                                    : ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                            }
                            else
                            {
                                hitMod += ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                                damage = Convert.ToInt32(Helpers.RollDice(1, 2));
                                damage += ActorStats.CalculateAbilityModifier(attacker.Player.Stats.Strength);
                            }
                            damage = damage <= 0 ? 1 : damage;
                            if(toHit > 1)
                            {
                                if(toHit == 20 || hitMod >= pTargetPlayer.Player.Stats.ArmourClass)
                                {
                                    if(attacker.Player.Level >= Constants.ImmLevel || attacker.Player.ShowDetailedRollInfo)
                                    {
                                        attacker.Send($"You rolled {toHit} (Modified: {hitMod}) and hit {pTargetPlayer.Player}{Constants.NewLine}");
                                    }
                                    damage = toHit == 20 ? damage * 2 : damage; // double damage for a critical hit
                                    if(damage >= pTargetPlayer.Player.Stats.CurrentHP)
                                    {
                                        pTargetPlayer.Send($"{attacker.Player}'s {wStr} deals lethal damage, killing you!{Constants.NewLine}");
                                        attacker.Send($"Your {wStr} hits {pTargetPlayer.Player} for lethal damage, killing them!{Constants.NewLine}");
                                        pTargetPlayer.Player.Kill(ref pTargetPlayer);
                                        combatFinished = true;
                                    }
                                    else
                                    {
                                        pTargetPlayer.Send($"{attacker.Player}'s {wStr} hits you for {damage} damage!{Constants.NewLine}");
                                        attacker.Send($"Your {wStr} hits {pTargetPlayer.Player} for {damage} damage!{Constants.NewLine}");
                                        pTargetPlayer.Player.Stats.CurrentHP -= damage;
                                    }
                                }
                                else
                                {
                                    // miss
                                    if (attacker.Player.Level >= Constants.ImmLevel || attacker.Player.ShowDetailedRollInfo)
                                    {
                                        attacker.Send($"You rolled {toHit} (Modified: {hitMod}) and miss {pTargetPlayer.Player}{Constants.NewLine}");
                                    }
                                    pTargetPlayer.Send($"{attacker.Player}'s {wStr} misses you!{Constants.NewLine}");
                                    attacker.Send($"Your {wStr} misses {pTargetPlayer.Player}!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                // super miss
                                pTargetPlayer.Send($"{attacker.Player} fumbles their {wStr} and misses you!{Constants.NewLine}");
                                attacker.Send($"You fumble your {wStr} and miss {pTargetPlayer.Player}!{Constants.NewLine}");
                            }
                        }
                    }
                }
            }
            if(finishedSessions.Count > 0)
            {
                // clear out participant groups where that session has finished
                foreach(var s in finishedSessions)
                {
                    session.Participants.Remove(s);
                }
            }
        }

        internal Dictionary<Guid, CombatSession> GetCombatQueue()
        {
            return CombatQueue;
        }

        internal Guid AddCombatSession(CombatSession session)
        {
            var g = Guid.NewGuid();
            lock(_lockObject)
            {
                CombatQueue.Add(g, session);
            }
            return g;
        }

        internal void RemoveCombatSession(Guid g)
        {
            lock(_lockObject)
            {
                foreach(var p in Instance.CombatQueue[g].Participants)
                {
                    if(p.Attacker != null && p.Attacker is Descriptor)
                    {
                        SessionManager.Instance.GetPlayerByGUID((p.Attacker as Descriptor).Id).Player.Position = ActorPosition.Standing;
                        SessionManager.Instance.GetPlayerByGUID((p.Attacker as Descriptor).Id).Player.CombatSessionID = Guid.Empty;
                    }
                    if(p.Target != null && p.Target is Descriptor)
                    {
                        SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id).Player.Position = ActorPosition.Standing;
                        SessionManager.Instance.GetPlayerByGUID((p.Target as Descriptor).Id).Player.CombatSessionID = Guid.Empty;
                    }
                }
                CombatQueue.Remove(g);
            }
        }
    }
}