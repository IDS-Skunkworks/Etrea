using System;
using System.Linq;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public static class CombatManager
    {
        public static void ProcessCombat()
        {
            var playersInCombat = SessionManager.Instance.ActivePlayers.Where(x => x.Player.TargetQueue.Count > 0);
            if (playersInCombat.Count() > 0)
            {
                foreach(var p in playersInCombat)
                {
                    var wpnName = p.Player.WeaponEquip != null ? p.Player.WeaponEquip.Name.ToLower() : "strike";
                    var targetID = p.Player.TargetQueue.Keys.First();
                    var tNPC = NPCManager.Instance.GetNPC(targetID);
                    if (tNPC == null)
                    {
                        Game.LogMessage($"COMBAT: Removing {targetID} from target queue of {p.Player.Name}, no such NPC Instance in NPC Manager", LogLevel.Combat);
                        p.Player.TargetQueue.TryRemove(targetID, out _);
                        continue;
                    }
                    if (tNPC.CurrentRoom != p.Player.CurrentRoom)
                    {
                        Game.LogMessage($"COMBAT: Removing {targetID} from target queue of {p.Player.Name}, target is not in the same Room", LogLevel.Combat);
                        p.Player.TargetQueue.TryRemove(targetID, out _);
                        continue;
                    }
                    Game.LogMessage($"COMBAT: {p.Player.Name} is targeting {tNPC.Name} in Room {p.Player.CurrentRoom}", LogLevel.Combat);
                    for (int attacks = 0; attacks < p.Player.NumberOfAttacks; attacks++)
                    {
                        if (p.Player.HitsTarget(tNPC, out bool criticalHit, out int baseRoll, out int modRoll))
                        {
                            var damage = p.Player.CalculateHitDamage(tNPC, criticalHit);
                            Game.LogMessage($"COMBAT: Player {p.Player.Name} in Room {p.Player.CurrentRoom} hits {tNPC.Name} for {damage} points of damage", LogLevel.Combat);
                            var dmgString = Helpers.GetDamageString(damage, tNPC.MaxHP);
                            if (p.Player.ShowDetailedRollInfo)
                            {
                                p.Send($"%BGT%[Roll: {baseRoll}/{modRoll}]: Your {wpnName} %BRT%{dmgString}%BGT% {tNPC.Name} causing %BRT%{damage}%BGT% damage!%PT%{Constants.NewLine}");
                            }
                            else
                            {
                                p.Send($"%BGT%Your {wpnName} %BRT%{dmgString}%BGT% {tNPC.Name}!%PT%{Constants.NewLine}");
                            }
                            tNPC.AdjustHP((damage * -1), out bool killed);
                            if (killed)
                            {
                                Game.LogMessage($"COMBAT: Player {p.Player.Name} has killed {tNPC.Name} in Room {p.Player.CurrentRoom}", LogLevel.Combat);
                                tNPC.Kill(p.Player, true);
                                break;
                            }
                        }
                        else
                        {
                            if (p.Player.ShowDetailedRollInfo)
                            {
                                p.Send($"%BRT%[Roll: {baseRoll}/{modRoll}]: Your {wpnName} misses {tNPC.Name}!%PT%{Constants.NewLine}");
                            }
                            else
                            {
                                p.Send($"%BRT%Your {wpnName} misses {tNPC.Name}!%PT%{Constants.NewLine}");
                            }
                        }
                    }
                }
            }
            var npcsInCombat = NPCManager.Instance.AllNPCInstances.Where(x => x.TargetQueue.Count > 0);
            if (npcsInCombat.Count() > 0)
            {
                foreach(var n in npcsInCombat)
                {
                    if (!NPCManager.Instance.NPCInstanceExists(n.ID))
                    {
                        continue;
                    }
                    var targetID = n.TargetQueue.Keys.FirstOrDefault();
                    Actor target = SessionManager.Instance.GetSession(targetID) != null ? (Actor)SessionManager.Instance.GetSession(targetID).Player :
                        NPCManager.Instance.GetNPC(targetID);
                    if (target == null)
                    {
                        Game.LogMessage($"COMBAT: Removing {targetID} from target queue of {n.Name}, no Player or NPC with that ID could be found", LogLevel.Combat);
                        n.TargetQueue.TryRemove(targetID, out _);
                        continue;
                    }
                    if (target.CurrentHP <= 0)
                    {
                        Game.LogMessage($"COMBAT: Removing {targetID} from target queue of {n.Name}, target is dead", LogLevel.Combat);
                        n.TargetQueue.TryRemove(targetID, out _);
                        continue;
                    }
                    if (target.CurrentRoom != n.CurrentRoom)
                    {
                        Game.LogMessage($"COMBAT: Removing {targetID} from target queue of {n.Name}, target is not in the same Room", LogLevel.Combat);
                        n.TargetQueue.TryRemove(targetID, out _);
                        continue;
                    }
                    Game.LogMessage($"COMBAT: {n.Name} is targeting {target.ActorType} {target.Name} in Room {n.CurrentRoom}", LogLevel.Combat);
                    var npcSpells = (from string spellName in n.Spells.Keys where SpellManager.Instance.GetSpell(spellName) != null select SpellManager.Instance.GetSpell(spellName)).ToList();
                    var npcDamageSpells = npcSpells.Where(x => x.SpellType == SpellType.Damage && x.MPCost(n) <= n.CurrentMP).ToList();
                    var npcHealingSpells = npcSpells.Where(x => x.SpellType == SpellType.Healing && x.MPCost(n) <= n.CurrentMP).ToList();
                    var npcBuffSpells = npcSpells.Where(x => x.SpellType == SpellType.Buff && x.MPCost(n) <= n.CurrentMP).ToList();
                    var npcDebuffSpells = npcSpells.Where(x => x.SpellType == SpellType.Debuff && x.MPCost(n) <= n.CurrentMP).ToList();
                    var percHP = n.CurrentHP / n.MaxHP * 100;
                    if (n.Flags.HasFlag(NPCFlags.Coward))
                    {
                        if (percHP < 60)
                        {
                            if (Helpers.FleeCombat(n, out int rid))
                            {
                                if (target.ActorType == ActorType.Player)
                                {
                                    ((Player)target).Send($"%BYT%{n.Name} suddenly breaks combat and flees!%PT%{Constants.NewLine}");
                                }
                                if (n.MobProgs.Count > 0)
                                {
                                    foreach (var mp in n.MobProgs.Keys)
                                    {
                                        var mobProg = MobProgManager.Instance.GetMobProg(mp);
                                        if (mobProg != null)
                                        {
                                            string attackerID = target.ActorType == ActorType.Player ? target.ID.ToString() : string.Empty;
                                            mobProg.Init();
                                            mobProg.TriggerEvent(MobProgTrigger.MobFlees, new { mob = n.ID.ToString(), player = attackerID });
                                        }
                                    }
                                }
                                n.Move(rid, false);
                            }
                            else
                            {
                                if (target.ActorType == ActorType.Player)
                                {
                                    ((Player)target).Send($"%BYT%{n.Name} wants to flee but can't break away from you!%PT%{Constants.NewLine}");
                                }
                            }    
                            continue;
                        }
                    }
                    if (percHP <= 50 && npcHealingSpells != null && npcHealingSpells.Count > 0)
                    {
                        var healSpell = npcHealingSpells.GetRandomElement();
                        if (healSpell != null)
                        {
                            healSpell.Cast(n, n);
                            if (target.ActorType == ActorType.Player)
                            {
                                ((Player)target).Send($"%BYT%{n.Name} summons the Winds of Magic to aid them!%PT%{Constants.NewLine}");
                            }
                            continue;
                        }
                    }
                    if (percHP <= 50)
                    {
                        var conumables = n.Inventory.Values.Where(x => x.ItemType == ItemType.Consumable).ToList();
                        if (conumables.Count > 0)
                        {
                            var consumable = conumables.GetRandomElement() as Consumable;
                            consumable.Consume(n);
                            if (target.ActorType == ActorType.Player)
                            {
                                ((Player)target).Send($"%BYT%{n.Name} consumes {consumable.ShortDescription}!%PT%{Constants.NewLine}");
                            }
                            continue;
                        }
                    }
                    // should only really be doing this if the NPC actually has spells...
                    var magChance = Math.Max(1, Math.Min(9, 5 + Helpers.CalculateAbilityModifier(n.Intelligence))) * 10;
                    if (Helpers.RollDice<int>(1, 100) < magChance)
                    {
                        var spells = npcSpells.Except(npcSpells.Where(x => x.SpellType == SpellType.Healing)).ToList();
                        if (spells != null && spells.Count > 0)
                        {
                            var spell = spells.GetRandomElement();
                            if (spell != null)
                            {
                                if (spell.SpellType == SpellType.Buff)
                                {
                                    spell.Cast(n, n);
                                    if (target.ActorType == ActorType.Player)
                                    {
                                        ((Player)target).Send($"%BYT%{n.Name} calls on the Winds of Magic to aid them!%PT%{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    spell.Cast(n, target);
                                }
                                continue;
                            }
                        }
                    }
                    var wpnName = n.WeaponEquip != null ? n.WeaponEquip.Name.ToLower() : "strike";
                    for (int attacks = 0; attacks < n.NumberOfAttacks; attacks++)
                    {
                        if (n.HitsTarget(target, out bool isCritical, out int baseRoll, out int modRoll))
                        {
                            var damage = n.CalculateHitDamage(target, isCritical);
                            Game.LogMessage($"COMBAT: NPC {n.Name} in Room {n.CurrentRoom} hits {target.Name} for {damage} damage", LogLevel.Combat);
                            var dmgString = Helpers.GetDamageString(damage, target.MaxHP);
                            if (target.ActorType == ActorType.Player)
                            {
                                var tPlayer = (Player)target;
                                if (tPlayer.ShowDetailedRollInfo)
                                {
                                    tPlayer.Send($"%BRT%[Roll: {baseRoll}/{modRoll}]: {n.Name}'s {wpnName} %BYT%{dmgString}%BRT% you causing %BYT%{damage}%BRT% damage!%PT%{Constants.NewLine}");
                                }
                                else
                                {
                                    tPlayer.Send($"%BRT%{n.Name}'s {wpnName} %BBT%{dmgString}%BRT% you!%PT%{Constants.NewLine}");
                                }
                                tPlayer.AdjustHP((damage * -1), out bool isKilled);
                                if (isKilled)
                                {
                                    Game.LogMessage($"COMBAT: Player {tPlayer.Name} was killed in Room {tPlayer.CurrentRoom} by {n.Name}", LogLevel.Combat);
                                    tPlayer.Kill(n, true);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (target.ActorType == ActorType.Player)
                            {
                                var tPlayer = (Player)target;
                                if (tPlayer.ShowDetailedRollInfo)
                                {
                                    tPlayer.Send($"%BRT%[Roll: {baseRoll}/{modRoll}]: {n.Name}'s {wpnName} misses you!%PT%{Constants.NewLine}");
                                }
                                else
                                {
                                    tPlayer.Send($"%BRT%{n.Name}'s {wpnName} misses you!%PT%{Constants.NewLine}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
