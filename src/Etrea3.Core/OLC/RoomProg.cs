using Etrea3.Core;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateRoomProg(Session session)
        {
            var mobProg = new RoomProg();
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
                        mobProg.ID = GetValue<int>(session, "Enter RoomProg ID: ");
                        break;

                    case 2:
                        mobProg.Name = GetValue<string>(session, "Enter RoomProg Name: ");
                        break;

                    case 3:
                        mobProg.Description = GetValue<string>(session, "Enter RoomProg Description: ");
                        break;

                    case 4:
                        mobProg.Script = Helpers.GetMobProgScript(session);
                        break;

                    case 5:
                        mobProg.Triggers = GetEnumValue<RoomProgTrigger>(session, "Enter RoomProg Triggers: ");
                        break;

                    case 6:
                        if (ValidateAsset(session, mobProg, true, out _))
                        {
                            if (ScriptObjectManager.Instance.AddOrUpdateScriptObject<RoomProg>(mobProg, true))
                            {
                                session.SendSystem($"%BGT%The new RoomProg has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new RoomProg: {mobProg.Name} ({mobProg.ID})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The new RoomProg was not successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add RoomProg {mobProg.Name} ({mobProg.ID}) but the attempt failed", LogLevel.OLC);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new RoomProg could not be validated and will not be saved.%PT%{Constants.NewLine}");
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

        private static void DeleteRoomProg(Session session)
        {
            while (true)
            {
                session.SendSystem($"Enter RoomProg ID or END to return: ");
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int progID))
                {
                    session.SendSystem($"%BRT%That is not a valid RoomProg ID.%PT%{Constants.NewLine}");
                    continue;
                }
                var mobProg = ScriptObjectManager.Instance.GetScriptObject<RoomProg>(progID);
                if (mobProg == null)
                {
                    session.SendSystem($"%BRT%No RoomProg with that ID could be found in RoomProg Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (mobProg.OLCLocked)
                {
                    var lockHolder = SessionManager.Instance.GetSession(mobProg.LockHolder);
                    var msg = lockHolder != null ? $"%BRT%The specified RoomProg is locked in OLC by {lockHolder.Player.Name}.%PT%{Constants.NewLine}" :
                        $"The specified RoomProg is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.SendSystem(msg);
                    continue;
                }
                if (ScriptObjectManager.Instance.RemoveScriptObject<RoomProg>(mobProg.ID))
                {
                    session.SendSystem($"%BGT%The specified RoomProg has been successfully removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed RoomProg {mobProg.ID} ({mobProg.Name})", LogLevel.OLC);
                    return;
                }
                else
                {
                    session.SendSystem($"%BRT%The specified RoomProg could not be removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove RoomProg {mobProg.ID} ({mobProg.Name}) but the attempt failed", LogLevel.OLC);
                    continue;
                }
            }
        }

        private static void ChangeRoomProg(Session session)
        {
            session.SendSystem("Enter RoomProg ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int progID))
            {
                session.SendSystem($"%BRT%That is not a valid RoomProg ID.%PT%{Constants.NewLine}");
                return;
            }
            if (!ScriptObjectManager.Instance.ScriptObjectExists<RoomProg>(progID))
            {
                session.SendSystem($"%BRT%That is not a valid RoomProg ID.%PT%{Constants.NewLine}");
                return;
            }
            var mobProg = ScriptObjectManager.Instance.GetScriptObject<RoomProg>(progID);
            if (mobProg.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(mobProg.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified RoomProg is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"The specified RoomProg is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            RoomProg updatedMobProg = Helpers.Clone<RoomProg>(mobProg);
            ScriptObjectManager.Instance.SetScriptLockState<RoomProg>(progID, true, session);
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
                switch (option)
                {
                    case 1:
                        updatedMobProg.Name = GetValue<string>(session, "Enter RoomProg Name: ");
                        break;

                    case 2:
                        updatedMobProg.Description = GetValue<string>(session, "Enter RoomProg Description: ");
                        break;

                    case 3:
                        updatedMobProg.Script = Helpers.GetMobProgScript(session);
                        break;

                    case 4:
                        updatedMobProg.Triggers = GetEnumValue<RoomProgTrigger>(session, "Enter RoomProg Triggers: ");
                        break;

                    case 5:
                        if (ValidateAsset<RoomProg>(session, updatedMobProg, false, out _))
                        {
                            if (ScriptObjectManager.Instance.AddOrUpdateScriptObject<RoomProg>(updatedMobProg, false))
                            {
                                session.SendSystem($"%BGT%The updated RoomProg has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated RoomProg: {updatedMobProg.Name} ({updatedMobProg.ID})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The updated RoomProg was not successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update RoomProg {updatedMobProg.Name} ({updatedMobProg.ID}) but the attempt failed", LogLevel.OLC);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated RoomProg could not be validated and will not be saved.%PT%{Constants.NewLine}");
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