using Etrea3.Core;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateMobProg(Session session)
        {
            var mobProg = new MobProg();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"ID: {mobProg.ID}");
                sb.AppendLine($"Name: {mobProg.Name}");
                sb.AppendLine($"Description: {mobProg.Description}");
                sb.AppendLine($"Script set?: {!string.IsNullOrEmpty(mobProg.Script)}");
                sb.AppendLine($"Triggers: {mobProg.Triggers}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Description{Constants.TabStop}4. Set Script Text");
                sb.AppendLine($"5. Set Triggers");
                sb.AppendLine($"6. Save{Constants.TabStop}{Constants.TabStop}7. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That doesn't look like a valid choice.%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        mobProg.ID = GetValue<int>(session, "Enter MobProg ID: ");
                        break;

                    case 2:
                        mobProg.Name = GetValue<string>(session, "Enter MobProg Name: ");
                        break;

                    case 3:
                        mobProg.Description = GetValue<string>(session, "Enter MobProg Description: ");
                        break;

                    case 4:
                        mobProg.Script = Helpers.GetMobProgScript(session);
                        break;

                    case 5:
                        mobProg.Triggers = GetEnumValue<MobProgTrigger>(session, "Enter MobProg Triggers: ");
                        break;

                    case 6:
                        if (ValidateAsset(session, mobProg, true, out _))
                        {
                            if (MobProgManager.Instance.AddOrUpdateMobProg(mobProg, true))
                            {
                                session.SendSystem($"%BGT%The new MobProg has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new MobProg: {mobProg.Name} ({mobProg.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The new MobProg was not successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add MobProg {mobProg.Name} ({mobProg.ID}) but the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new MobProg could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 7:
                        return;

                    default:
                        session.SendSystem($"%BRT%That doesn't look like a valid option.%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteMobProg(Session session)
        {
            while (true)
            {
                session.SendSystem($"Enter MobProg ID or END to return: ");
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int progID))
                {
                    session.SendSystem($"%BRT%That is not a valid MobProg ID.%PT%{Constants.NewLine}");
                    continue;
                }
                var mobProg = MobProgManager.Instance.GetMobProg(progID);
                if (mobProg == null)
                {
                    session.SendSystem($"%BRT%No MobProg with that ID could be found in MobProg Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (mobProg.OLCLocked)
                {
                    var lockHolder = SessionManager.Instance.GetSession(mobProg.LockHolder);
                    var msg = lockHolder != null ? $"%BRT%The specified MobProg is locked in OLC by {lockHolder.Player.Name}.%PT%{Constants.NewLine}" :
                        $"The specified MobProg is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.SendSystem(msg);
                    continue;
                }
                if (MobProgManager.Instance.RemoveMobProg(mobProg.ID))
                {
                    session.SendSystem($"%BGT%The specified MobProg has been successfully removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed MobProg {mobProg.ID} ({mobProg.Name})", LogLevel.OLC, true);
                    return;
                }
                else
                {
                    session.SendSystem($"%BRT%The specified MobProg could not be removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove MobProg {mobProg.ID} ({mobProg.Name}) but the attempt failed", LogLevel.OLC, true);
                    continue;
                }
            }
        }

        private static void ChangeMobProg(Session session)
        {
            session.SendSystem("Enter MobProg ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int progID))
            {
                session.SendSystem($"%BRT%That is not a valid MobProg ID.%PT%{Constants.NewLine}");
                return;
            }
            if (!MobProgManager.Instance.MobProgExists(progID))
            {
                session.SendSystem($"%BRT%That is not a valid MobProg ID.%PT%{Constants.NewLine}");
                return;
            }
            var mobProg = MobProgManager.Instance.GetMobProg(progID);
            if (mobProg.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(mobProg.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified MobProg is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"The specified MobProg is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            var updatedMobProg = Helpers.Clone<MobProg>(mobProg);
            MobProgManager.Instance.SetMobProgLockState(progID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"ID: {updatedMobProg.ID}");
                sb.AppendLine($"Name: {updatedMobProg.Name}");
                sb.AppendLine($"Description: {updatedMobProg.Description}");
                sb.AppendLine($"Script set?: {!string.IsNullOrEmpty(updatedMobProg.Script)}");
                sb.AppendLine($"Triggers: {updatedMobProg.Triggers}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}{Constants.TabStop}2. Set Description");
                sb.AppendLine($"3. Set Script Text{Constants.TabStop}{Constants.TabStop}4. Set Triggers");
                sb.AppendLine($"5. Save{Constants.TabStop}{Constants.TabStop}6. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That is not a valid option.%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        updatedMobProg.Name = GetValue<string>(session, "Enter MobProg Name: ");
                        break;

                    case 2:
                        updatedMobProg.Description = GetValue<string>(session, "Enter MobProg Description: ");
                        break;

                    case 3:
                        updatedMobProg.Script = Helpers.GetMobProgScript(session);
                        break;

                    case 4:
                        updatedMobProg.Triggers = GetEnumValue<MobProgTrigger>(session, "Enter MobProg Triggers: ");
                        break;

                    case 5:
                        if (ValidateAsset(session, updatedMobProg, false, out _))
                        {
                            if (MobProgManager.Instance.AddOrUpdateMobProg(updatedMobProg, false))
                            {
                                session.SendSystem($"%BGT%The updated MobProg has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated MobProg: {updatedMobProg.Name} ({updatedMobProg.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The updated MobProg was not successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update MobProg {updatedMobProg.Name} ({updatedMobProg.ID}) but the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated MobProg could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 6:
                        return;

                    default:
                        session.SendSystem($"%BRT%That is not a valid option.%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}