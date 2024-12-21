using System;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public static class LogonProvider
    {
        public static Player LogonPlayer(Session session)
        {
            if (session == null)
            {
                Game.LogMessage($"ERROR: Attempt to initiate logon with a null session", LogLevel.Error, true);
                return null;
            }
            SessionManager.Instance.UpdateSessionStatus(session.ID, ConnectionState.GetUserName);
            int authErrorCount = 0;
            while (true)
            {
                if (session == null)
                {
                    return null;
                }
                try
                {
                    session.Send($"{Constants.NewLine}{Constants.NewLine}Enter your name (EXIT to go back): ");
                    var playerName = session.Read();
                    if (!string.IsNullOrEmpty(playerName))
                    {
                        if (playerName.Trim().ToUpper() == "EXIT")
                        {
                            return null;
                        }
                        if (DatabaseManager.CharacterExists(playerName.Trim()))
                        {
                            session.Send($"{Constants.NewLine}Password: ");
                            var playerPassword = session.Read();
                            if (!string.IsNullOrEmpty(playerPassword))
                            {
                                if (DatabaseManager.ValidatePlayerPassword(playerName.Trim(), playerPassword.Trim()))
                                {
                                    if (SessionManager.Instance.GetSession(playerName.Trim()) == null)
                                    {
                                        var p = DatabaseManager.LoadPlayer(playerName.Trim());
                                        if (p != null)
                                        {
                                            p.ID = session.ID;
                                            SessionManager.Instance.UpdateSessionPlayer(session.ID, p);
                                            SessionManager.Instance.UpdateSessionStatus(session.ID, ConnectionState.Playing);
                                            Game.LogMessage($"CONNECTION: Player {playerName.Trim()} has logged in successfully from {session.Client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                                            return p;
                                        }
                                        else
                                        {
                                            session.Send($"Dark sorceries are preventing you from entering the world right now!{Constants.NewLine}");
                                            SessionManager.Instance.Close(session);
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        var oldSession = SessionManager.Instance.GetSession(playerName);
                                        var oldEndpoint = oldSession.Client.Client.RemoteEndPoint;
                                        Game.LogMessage($"CONNECTION: Disconnecting existing session for player '{playerName}' to allow connection from {session.Client.Client.RemoteEndPoint}'", LogLevel.Connection, true);
                                        SessionManager.Instance.Close(oldSession);
                                        var p = DatabaseManager.LoadPlayer(playerName.Trim());
                                        if (p != null)
                                        {
                                            p.ID = session.ID;
                                            SessionManager.Instance.UpdateSessionPlayer(session.ID, p);
                                            SessionManager.Instance.UpdateSessionStatus(session.ID, ConnectionState.Playing);
                                            Game.LogMessage($"CONNECTION: Player {playerName} has logged in successfully from {session.Client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                                            return p;
                                        }
                                        else
                                        {
                                            session.Send($"Dark sorceries are preventing you from entering the world right now!{Constants.NewLine}");
                                            SessionManager.Instance.Close(session);
                                            return null;
                                        }
                                    }
                                }
                                else
                                {
                                    authErrorCount++;
                                    Game.LogMessage($"WARN: Authentication error for player {playerName.Trim()} from {session.Client.Client.RemoteEndPoint} (Failure count: {authErrorCount})", LogLevel.Warning, true);
                                    session.Send($"Authentication error ({authErrorCount}/{Constants.MaxAuthErrors}){Constants.NewLine}");
                                    if (authErrorCount >= Constants.MaxAuthErrors)
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Game.LogMessage($"WARN: Attempt from {session.Client.Client.RemoteEndPoint} to load player '{playerName}' which does not exist", LogLevel.Warning, true);
                            session.Send($"{Constants.NewLine}The given player cannot be found!{Constants.NewLine}");
                            break;
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Game.LogMessage($"ERROR: Error in LogonProvider.LogonPlayer(): {ex.Message}", LogLevel.Error, true);
                    break;
                }
            }
            return null;
        }
    }
}
