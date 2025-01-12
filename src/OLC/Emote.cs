using Etrea3.Core;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        // %pg% / %tg% = performer/target gender: male, female, nonbinary
        // %pg1% / %tg1% = obj pronoun: him, her, them
        // %pg2% / %tg2% = pos pronoun: his, hers, their
        // %pg3% / %tg3% = per pronoun: he, she, they
        // %pn% / %tn% = performer/target name
        private static void CreateEmote(Session session)
        {
            Emote newEmote = new Emote();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Emote ID: {newEmote.ID}{Constants.TabStop}Emote Name: {newEmote.Name}");
                sb.AppendLine($"Messages To Performer:");
                sb.AppendLine($"{Constants.TabStop}No Target: {newEmote.MessageToPerformer[0]}");
                sb.AppendLine($"{Constants.TabStop}With Target: {newEmote.MessageToPerformer[1]}");
                sb.AppendLine($"{Constants.TabStop}Target Not Found: {newEmote.MessageToPerformer[2]}");
                sb.AppendLine($"{Constants.TabStop}Performer == Target: {newEmote.MessageToPerformer[3]}");
                sb.AppendLine();
                sb.AppendLine($"Message to Target: {newEmote.MessageToTarget}");
                sb.AppendLine();
                sb.AppendLine($"Messages To Others:");
                sb.AppendLine($"{Constants.TabStop}No Target: {newEmote.MessageToOthers[0]}");
                sb.AppendLine($"{Constants.TabStop}With Target: {newEmote.MessageToOthers[1]}");
                sb.AppendLine($"{Constants.TabStop}Target Not Found: {newEmote.MessageToOthers[2]}");
                sb.AppendLine($"{Constants.TabStop}Performer == Target: {newEmote.MessageToOthers[3]}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Messages to Performer{Constants.TabStop}4. Set Message to Target");
                sb.AppendLine("5. Set Messages to Others");
                sb.AppendLine($"6. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}7. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newEmote.ID = GetValue<int>(session, "Enter Emote ID: ");
                        break;

                    case 2:
                        newEmote.Name = GetValue<string>(session, "Enter Emote Name: ");
                        break;

                    case 3:
                        session.SendSystem($"Set Messages to Performer:{Constants.NewLine}");
                        newEmote.MessageToPerformer[0] = GetValue<string>(session, "No Target: ");
                        newEmote.MessageToPerformer[1] = GetValue<string>(session, "With Target: ");
                        newEmote.MessageToPerformer[2] = GetValue<string>(session, "Target Not Found: ");
                        newEmote.MessageToPerformer[3] = GetValue<string>(session, "Performer is Target: ");
                        break;

                    case 4:
                        newEmote.MessageToTarget = GetValue<string>(session, "Message to Target: ");
                        break;

                    case 5:
                        session.SendSystem($"Set Messages to Others:{Constants.NewLine}");
                        newEmote.MessageToOthers[0] = GetValue<string>(session, "No Target: ");
                        newEmote.MessageToOthers[1] = GetValue<string>(session, "With Target: ");
                        newEmote.MessageToOthers[2] = GetValue<string>(session, "Target Not Found: ");
                        newEmote.MessageToOthers[3] = GetValue<string>(session, "Performer is Target: ");
                        break;

                    case 6:
                        if (ValidateAsset(session, newEmote, true, out _))
                        {
                            if (EmoteManager.Instance.AddOrUpdateEmote(newEmote, true))
                            {
                                session.SendSystem($"%BGT%The new Emote has been successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} added new Emote: {newEmote.Name}", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%Failed to save the new Emote.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Emote ({newEmote.Name}) however the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Emote could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 7:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteEmote(Session session)
        {
            while(true)
            {
                session.SendSystem($"%BRT%This is a permanent change to the Realms!%PT%{Constants.NewLine}");
                session.SendSystem($"Enter Emote ID or Name or END to return: ");
                string input = session.Read();
                if (string.IsNullOrEmpty(input))
                {
                    session.SendSystem($"%BRT%Sorry, that is not a valid Emote Name or ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (int.TryParse(input.Trim(), out int value))
                {
                    if (!EmoteManager.Instance.EmoteExists(value))
                    {
                        session.SendSystem($"%BRT%No Emote with that ID could be found in Emote Manager.%PT%{Constants.NewLine}");
                        continue;
                    }
                    var emote = EmoteManager.Instance.GetEmote(value);
                    if (emote.OLCLocked)
                    {
                        var lockingSession = SessionManager.Instance.GetSession(emote.LockHolder);
                        var msg = lockingSession != null ? $"%BRT%The specified Emote is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                            $"%BRT%The specified Emote is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                        session.SendSystem(msg);
                        continue;
                    }
                    if (EmoteManager.Instance.RemoveEmote(emote.ID))
                    {
                        session.SendSystem($"%BGT%The specified Emote has been removed successfully.%PT%{Constants.NewLine}");
                        Game.LogMessage($"OLC: Player {session.Player.Name} has removed Emote {emote.Name} ({emote.ID})", LogLevel.OLC, true);
                        return;
                    }
                    else
                    {
                        session.SendSystem($"%BRT%The specified Emote could not be removed.%PT%{Constants.NewLine}");
                        Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Emote {emote.Name} ({emote.ID}) but the attempt failed", LogLevel.OLC, true);
                        continue;
                    }
                }
                else
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        return;
                    }
                    if (!EmoteManager.Instance.EmoteExists(input.Trim()))
                    {
                        session.SendSystem($"%BRT%No Emote with that name could be found in Emote Manager.%PT%{Constants.NewLine}");
                        continue;
                    }
                    var emote = EmoteManager.Instance.GetEmote(input.Trim());
                    if (emote.OLCLocked)
                    {
                        var lockingSession = SessionManager.Instance.GetSession(emote.LockHolder);
                        var msg = lockingSession != null ? $"%BRT%The specified Emote is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                            $"%BRT%The specified Emote is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                        session.SendSystem(msg);
                        continue;
                    }
                    if (EmoteManager.Instance.RemoveEmote(emote.ID))
                    {
                        session.SendSystem($"%BGT%The specified Emote has been removed successfully.%PT%{Constants.NewLine}");
                        Game.LogMessage($"OLC: Player {session.Player.Name} has removed Emote {emote.Name} ({emote.ID})", LogLevel.OLC, true);
                        return;
                    }
                    else
                    {
                        session.SendSystem($"%BRT%The specified Emote could not be removed.%PT%{Constants.NewLine}");
                        Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Emote {emote.Name} ({emote.ID}) but the attempt failed", LogLevel.OLC, true);
                        continue;
                    }
                }
            }
        }

        private static void ChangeEmote(Session session)
        {
            session.SendSystem($"Enter Emote ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int emoteID))
            {
                session.SendSystem($"%BRT%Sorry, that is not a valid Emote ID.%PT%{Constants.NewLine}");
                return;
            }
            if (!EmoteManager.Instance.EmoteExists(emoteID))
            {
                session.SendSystem($"%BRT%No Emote with that ID could be found in Emote Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (EmoteManager.Instance.GetEmote(emoteID).OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(EmoteManager.Instance.GetEmote(emoteID).LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Emote is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Emote is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            EmoteManager.Instance.SetEmoteLockState(emoteID, true, session);
            var emote = Helpers.Clone(EmoteManager.Instance.GetEmote(emoteID));
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Emote ID: {emote.ID}{Constants.TabStop}Emote Name: {emote.Name}");
                sb.AppendLine($"Messages To Performer:");
                sb.AppendLine($"{Constants.TabStop}No Target: {emote.MessageToPerformer[0]}");
                sb.AppendLine($"{Constants.TabStop}With Target: {emote.MessageToPerformer[1]}");
                sb.AppendLine($"{Constants.TabStop}Target Not Found: {emote.MessageToPerformer[2]}");
                sb.AppendLine($"{Constants.TabStop}Performer == Target: {emote.MessageToPerformer[3]}");
                sb.AppendLine();
                sb.AppendLine($"Message to Target: {emote.MessageToTarget}");
                sb.AppendLine();
                sb.AppendLine($"Messages To Others:");
                sb.AppendLine($"{Constants.TabStop}No Target: {emote.MessageToOthers[0]}");
                sb.AppendLine($"{Constants.TabStop}With Target: {emote.MessageToOthers[1]}");
                sb.AppendLine($"{Constants.TabStop}Target Not Found: {emote.MessageToOthers[2]}");
                sb.AppendLine($"{Constants.TabStop}Performer == Target: {emote.MessageToOthers[3]}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name");
                sb.AppendLine($"2. Set Messages to Performer{Constants.TabStop}3. Set Message to Target");
                sb.AppendLine("4. Set Messages to Others");
                sb.AppendLine($"5. Save{Constants.TabStop}{Constants.TabStop}6. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        emote.Name = GetValue<string>(session, "Enter Emote Name: ");
                        break;

                    case 2:
                        session.SendSystem($"Set Messages to Performer:{Constants.NewLine}");
                        emote.MessageToPerformer[0] = GetValue<string>(session, "No Target: ");
                        emote.MessageToPerformer[1] = GetValue<string>(session, "With Target: ");
                        emote.MessageToPerformer[2] = GetValue<string>(session, "Target Not Found: ");
                        emote.MessageToPerformer[3] = GetValue<string>(session, "Performer is Target: ");
                        break;

                    case 3:
                        emote.MessageToTarget = GetValue<string>(session, "Message to Target: ");
                        break;

                    case 4:
                        session.SendSystem($"Set Messages to Others:{Constants.NewLine}");
                        emote.MessageToOthers[0] = GetValue<string>(session, "No Target: ");
                        emote.MessageToOthers[1] = GetValue<string>(session, "With Target: ");
                        emote.MessageToOthers[2] = GetValue<string>(session, "Target Not Found: ");
                        emote.MessageToOthers[3] = GetValue<string>(session, "Performer is Target: ");
                        break;

                    case 5:
                        if (ValidateAsset(session, emote, false, out _))
                        {
                            if (EmoteManager.Instance.AddOrUpdateEmote(emote, false))
                            {
                                session.SendSystem($"%BGT%The updated Emote has been successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} updated Emote: {emote.Name}", LogLevel.OLC, true);
                                EmoteManager.Instance.SetEmoteLockState(emote.ID, false, session);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%Failed to save the new Emote.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Emote ({emote.Name}) however the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Emote could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 6:
                        EmoteManager.Instance.SetEmoteLockState(emote.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}