using Etrea2.Core;
using System.Text;

namespace Etrea2.Interfaces
{
    internal class LogonProvider : ILogonProvider, IInputValidator
    {
        public void LogonPlayer(ref Descriptor desc)
        {
            bool loggedIn = false;
            desc.State = ConnectionState.GetUsername;
            uint authErrCount = 0;
            while (!loggedIn)
            {
                desc.Send($"{Constants.NewLine}{Constants.NewLine}Enter your name: ");
                var pName = desc.Read().Trim();
                if (ValidateInput(pName))
                {
                    if (DatabaseManager.CharacterExistsInDatabase(pName))
                    {
                        var pPwd = string.Empty;
                        desc.State = ConnectionState.GetPassword;
                        while (!ValidateInput(pPwd))
                        {
                            desc.Send($"{Constants.NewLine}Password: ");
                            pPwd = desc.Read().Trim();
                            if (DatabaseManager.ValidatePlayerPassword(pName, pPwd))
                            {
                                if (SessionManager.Instance.GetPlayer(pName) == null)
                                {
                                    desc.Player = DatabaseManager.LoadPlayer(pName);
                                    if (desc.Player != null)
                                    {
                                        Game.LogMessage($"CONNECTION: Player '{desc.Player.Name}' has logged in successfully from {desc.Client.Client.RemoteEndPoint}.", LogLevel.Connection, true);
                                        loggedIn = true;
                                        RoomManager.Instance.LoadPlayerInRoom(desc.Player.CurrentRoom, ref desc);
                                    }
                                    else
                                    {
                                        desc.Send($"Error loading from database, cannot enter game world.{Constants.NewLine}");
                                        SessionManager.Instance.Close(desc);
                                        return;
                                    }
                                }
                                else
                                {
                                    var oldConnection = SessionManager.Instance.GetPlayer(pName);
                                    var oldEndPoint = oldConnection.Client.Client.RemoteEndPoint;
                                    Game.LogMessage($"CONNECTION: Disconnecting existing session for player {pName} on {oldEndPoint} to allow new connection from {desc.Client.Client.RemoteEndPoint}", LogLevel.Connection, true);
                                    SessionManager.Instance.Close(oldConnection);
                                    desc.Player = DatabaseManager.LoadPlayer(pName);
                                    if (desc.Player != null)
                                    {
                                        Game.LogMessage($"CONNECTION: Player '{desc.Player.Name}' has logged in successfully from {desc.Client.Client.RemoteEndPoint}.", LogLevel.Connection, true);
                                        loggedIn = true;
                                        RoomManager.Instance.LoadPlayerInRoom(desc.Player.CurrentRoom, ref desc);
                                    }
                                    else
                                    {
                                        desc.Send($"Error loading from database, cannot enter game world.{Constants.NewLine}");
                                        SessionManager.Instance.Close(desc);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                authErrCount++;
                                Game.LogMessage($"WARN: Authentication error for player {pName} from {desc.Client.Client.RemoteEndPoint} (Failure count: {authErrCount})", LogLevel.Warning, true);
                                desc.Send($"Authentication error ({authErrCount}/{Constants.MaxAuthErrors}){Constants.NewLine}");
                                if (authErrCount >= Constants.MaxAuthErrors)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Attempt from {desc.Client.Client.RemoteEndPoint} to load character '{pName}' which does not exist.", LogLevel.Warning, true);
                        desc.Send($"{Constants.NewLine}The specified character name cannot be found.{Constants.NewLine}");
                        break;
                    }
                }
            }
        }

        public bool ValidateInput(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && Encoding.UTF8.GetByteCount(input) == input.Length;
        }
    }
}
