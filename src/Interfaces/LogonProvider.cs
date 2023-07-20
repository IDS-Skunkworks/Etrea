using Kingdoms_of_Etrea.Core;
using System.Text;

namespace Kingdoms_of_Etrea.Interfaces
{
    internal class LogonProvider : ILogonProvider, IInputValidator
    {
        public void LogonPlayer(ref Descriptor _desc)
        {
            bool loggedIn = false;
            _desc.State = ConnectionState.GetUsername;
            uint authErrCount = 0;
            while (!loggedIn)
            {
                _desc.Send($"{Constants.NewLine}{Constants.NewLine}Enter your name: ");
                var pName = _desc.Read().Trim();
                if (ValidateInput(pName))
                {
                    if(DatabaseManager.CharacterExistsInDatabase(pName))
                    {
                        // player is in the database so ask for password
                        var pPwd = string.Empty;
                        _desc.State = ConnectionState.GetPassword;
                        while (!ValidateInput(pPwd))
                        {
                            _desc.Send($"{Constants.NewLine}Password: ");
                            pPwd = _desc.Read().Trim();
                            if (DatabaseManager.ValidatePlayerPassword(pName, pPwd))
                            {
                                if(SessionManager.Instance.GetPlayer(pName) == null)
                                {
                                    _desc.Player = DatabaseManager.LoadPlayerNew(pName);
                                    if (_desc.Player != null)
                                    {
                                        Game.LogMessage($"INFO: Player '{_desc.Player.Name}' has logged in successfully from {_desc.Client.Client.RemoteEndPoint}.", LogLevel.Info, true);
                                        loggedIn = true;
                                        RoomManager.Instance.LoadPlayerInRoom(_desc.Player.CurrentRoom, ref _desc);
                                    }
                                    else
                                    {
                                        _desc.Send($"Error loading from database, cannot enter game world.{Constants.NewLine}");
                                        SessionManager.Instance.Close(_desc);
                                        return;
                                    }
                                }
                                else
                                {
                                    _desc.Send($"This character is already logged in, multiple connections are not allowed.{Constants.NewLine}");
                                    Game.LogMessage($"ERROR: {_desc.Client.Client.RemoteEndPoint} attempted to log in player {pName} who is already logged in from {SessionManager.Instance.GetPlayer(pName).Client.Client.RemoteEndPoint}", LogLevel.Error, true);
                                    return;
                                }
                            }
                            else
                            {
                                authErrCount++;
                                Game.LogMessage($"WARN: Authentication error for player {pName} from {_desc.Client.Client.RemoteEndPoint} (Failure count: {authErrCount})", LogLevel.Warning, true);
                                _desc.Send($"Authentication error ({authErrCount}/{Constants.MaxAuthErrors}){Constants.NewLine}");
                                if(authErrCount >= Constants.MaxAuthErrors)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        // character does not exist so tell player they need to create a new character
                        Game.LogMessage($"WARN: Attempt to load character '{pName}' which does not exist.", LogLevel.Warning, true);
                        _desc.Send($"{Constants.NewLine}The specified character name cannot be found.{Constants.NewLine}");
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
