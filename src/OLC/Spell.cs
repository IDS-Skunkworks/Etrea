using Etrea3.Core;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateSpell(Session session)
        {
            Spell newSpell = new Spell();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Spell ID: {newSpell.ID}{Constants.TabStop}Spell Name: {newSpell.Name}");
                sb.AppendLine($"Description: {newSpell.Description}");
                sb.AppendLine($"Type: {newSpell.SpellType}{Constants.TabStop}Available To: {newSpell.AvailableToClass}");
                sb.AppendLine($"MP Cost: {newSpell.MPCostExpression}");
                sb.AppendLine($"Damage: {newSpell.DamageExpression}");
                sb.AppendLine($"Auto-Hit: {newSpell.AutoHitTarget}{Constants.TabStop}{Constants.TabStop}Learn Cost: {newSpell.LearnCost}");
                sb.AppendLine($"AOE: {newSpell.IsAOE}{Constants.TabStop}Ability Modifier: {newSpell.ApplyAbilityModifier}");
                sb.AppendLine($"Applied Buffs: {newSpell.AppliedBuffs.Count}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}3. Set Description");
                sb.AppendLine($"4. Set Type{Constants.TabStop}{Constants.TabStop}5. Set Classes{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}6. Set MP Cost");
                sb.AppendLine($"7. Set Damage{Constants.TabStop}{Constants.TabStop}8. Set Auto-Hit{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}9. Set AOE");
                sb.AppendLine($"10. Set Learn Cost{Constants.TabStop}11. Set Ability Modifier{Constants.TabStop}12. Manage Buffs");
                sb.AppendLine($"13. Save{Constants.TabStop}{Constants.TabStop}14. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.ToLower(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newSpell.ID = GetValue<int>(session, "Enter Spell ID: ");
                        break;

                    case 2:
                        newSpell.Name = GetValue<string>(session, "Enter Spell Name: ");
                        break;

                    case 3:
                        newSpell.Description = GetValue<string>(session, "Enter Spell Description: ");
                        break;

                    case 4:
                        newSpell.SpellType = GetEnumValue<SpellType>(session, "Enter Spell Type: ");
                        break;

                    case 5:
                        newSpell.AvailableToClass = GetEnumValue<ActorClass>(session, "Enter Classes: ");
                        break;

                    case 6:
                        newSpell.MPCostExpression = GetValue<string>(session, "Enter MP Cost: ");
                        break;

                    case 7:
                        newSpell.DamageExpression = GetValue<string>(session, "Enter Damage: ");
                        break;

                    case 8:
                        newSpell.AutoHitTarget = GetValue<bool>(session, "Enter Auto-Hit: ");
                        break;

                    case 9:
                        newSpell.IsAOE = GetValue<bool>(session, "Enter AOE: ");
                        break;

                    case 10:
                        newSpell.LearnCost = GetValue<int>(session, "Enter Gold to Learn: ");
                        break;

                    case 11:
                        newSpell.ApplyAbilityModifier = GetValue<bool>(session, "Enter Apply Ability Modifier: ");
                        break;

                    case 12:
                        ManageSpellBuffs(session, ref newSpell);
                        break;

                    case 13:
                        if (ValidateAsset(session, newSpell, true, out _))
                        {
                            if (SpellManager.Instance.AddOrUpdateSpell(newSpell, true))
                            {
                                session.Send($"%BGT%The new Spell has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Spell: {newSpell.Name} ({newSpell.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%The new Spell could not be saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Spell {newSpell.Name} ({newSpell.ID}) but the attempt failed.", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The Spell could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 14:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteSpell(Session session)
        {
            session.Send($"Enter Spell ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int spellID))
            {
                session.Send($"%RT%That is not a valid Spell ID.%PT%{Constants.NewLine}");
                return;
            }
            var spell = SpellManager.Instance.GetSpell(spellID);
            if (spell == null)
            {
                session.Send($"%BRT%No Spell with that ID could be found in Spell Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (spell.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(spell.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Spell is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified spell is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.Send(msg);
                return;
            }
            if (SpellManager.Instance.RemoveSpell(spell.ID))
            {
                session.Send($"%BGT%The specified Spell was removed successfully.%PT%{Constants.NewLine}");
                Game.LogMessage($"OLC: Player {session.Player.Name} has removed Spell {spell.Name} ({spell.ID})", LogLevel.OLC, true);
            }
            else
            {
                session.Send($"%BRT%The specified Spell could not be removed.%PT%{Constants.NewLine}");
                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Spell {spell.Name} ({spell.ID}) but the attempt failed", LogLevel.OLC, true);
            }
        }

        private static void ChangeSpell(Session session)
        {
            session.Send($"Enter Spell ID or END to return:");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int spellID))
            {
                session.Send($"%BRT%That is not a valid Spell ID.%PT%{Constants.NewLine}");
                return;
            }
            var spell = SpellManager.Instance.GetSpell(spellID);
            if (spell == null)
            {
                session.Send($"BRT%No Spell with that ID could be found in Spell Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (spell.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(spell.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Spell is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified spell is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.Send(msg);
                return;
            }
            SpellManager.Instance.SetSpellLockState(spellID, true, session);
            var modSpell = Helpers.Clone(SpellManager.Instance.GetSpell(spell.ID));
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Spell ID: {modSpell.ID}{Constants.TabStop}Spell Name: {modSpell.Name}");
                sb.AppendLine($"Description: {modSpell.Description}");
                sb.AppendLine($"Type: {modSpell.SpellType}{Constants.TabStop}Available To: {modSpell.AvailableToClass}");
                sb.AppendLine($"MP Cost: {modSpell.MPCostExpression}");
                sb.AppendLine($"Damage: {modSpell.DamageExpression}");
                sb.AppendLine($"Auto-Hit: {modSpell.AutoHitTarget}{Constants.TabStop}{Constants.TabStop}Learn Cost: {modSpell.LearnCost}");
                sb.AppendLine($"AOE: {modSpell.IsAOE}{Constants.TabStop}Ability Modifier: {modSpell.ApplyAbilityModifier}");
                sb.AppendLine($"Applied Buffs: {modSpell.AppliedBuffs.Count}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}{Constants.TabStop}2. Set Description{Constants.TabStop}{Constants.TabStop}3. Set Type");
                sb.AppendLine($"4. Set Classes{Constants.TabStop}{Constants.TabStop}5. Set MP Cost");
                sb.AppendLine($"6. Set Damage{Constants.TabStop}{Constants.TabStop}7. Set Auto-Hit{Constants.TabStop}{Constants.TabStop}8. Set AOE");
                sb.AppendLine($"9. Set Learn Cost{Constants.TabStop}{Constants.TabStop}10. Set Ability Modifier{Constants.TabStop}11. Manage Buffs");
                sb.AppendLine($"12. Save{Constants.TabStop}{Constants.TabStop}13. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.ToLower(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modSpell.Name = GetValue<string>(session, "Enter Spell Name: ");
                        break;

                    case 2:
                        modSpell.Description = GetValue<string>(session, "Enter Spell Description: ");
                        break;

                    case 3:
                        modSpell.SpellType = GetEnumValue<SpellType>(session, "Enter Spell Type: ");
                        break;

                    case 4:
                        modSpell.AvailableToClass = GetEnumValue<ActorClass>(session, "Enter Classes: ");
                        break;

                    case 5:
                        modSpell.MPCostExpression = GetValue<string>(session, "Enter MP Cost: ");
                        break;

                    case 6:
                        modSpell.DamageExpression = GetValue<string>(session, "Enter Damage: ");
                        break;

                    case 7:
                        modSpell.AutoHitTarget = GetValue<bool>(session, "Enter Auto-Hit: ");
                        break;

                    case 8:
                        modSpell.IsAOE = GetValue<bool>(session, "Enter AOE: ");
                        break;

                    case 9:
                        modSpell.LearnCost = GetValue<int>(session, "Enter Gold to Learn: ");
                        break;

                    case 10:
                        modSpell.ApplyAbilityModifier = GetValue<bool>(session, "Enter Apply Ability Modifier: ");
                        break;

                    case 11:
                        ManageSpellBuffs(session, ref modSpell);
                        break;

                    case 12:
                        if (ValidateAsset(session, modSpell, false, out _))
                        {
                            if (SpellManager.Instance.AddOrUpdateSpell(modSpell, false))
                            {
                                SpellManager.Instance.SetSpellLockState(spellID, false, session);
                                session.Send($"%BGT%The Spell has been updated successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Spell: {modSpell.Name} ({modSpell.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%The updated Spell could not be saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Spell {modSpell.Name} ({modSpell.ID}) but the attempt failed.", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The Spell could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 13:
                        SpellManager.Instance.SetSpellLockState(spellID, false, session);
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageSpellBuffs(Session session, ref Spell spell)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (spell.AppliedBuffs.Count > 0)
                {
                    sb.AppendLine("Applied Buffs:");
                    foreach(var b in spell.AppliedBuffs)
                    {
                        var buff = BuffManager.Instance.GetBuff(b.Key);
                        if (buff != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{buff.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{b.Key} (Unknown Buff)");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Applied Buffs: None");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Buff{Constants.TabStop}{Constants.TabStop}2. Remove Buff");
                sb.AppendLine($"3. Clear Buffs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice:");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var buffName = GetValue<string>(session, "Enter Buff Name: ");
                        var buff = BuffManager.Instance.GetBuff(buffName);
                        if (buff != null)
                        {
                            spell.AppliedBuffs.TryAdd(buff.Name, true);
                        }
                        else
                        {
                            session.Send($"%BRT%No Buff with that name could be found in Buff Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        buffName = GetValue<string>(session, "Enter Buff Name: ");
                        if (spell.AppliedBuffs.ContainsKey(buffName))
                        {
                            spell.AppliedBuffs.TryRemove(buffName, out _);
                        }
                        break;

                    case 3:
                        spell.AppliedBuffs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}