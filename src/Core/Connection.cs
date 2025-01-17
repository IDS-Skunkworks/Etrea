using Etrea3.Objects;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Etrea3.Core
{
    public class Connection
    {
        private Session playerSession;

        public Connection(Session session)
        {
            playerSession = session;
            Thread thread = new Thread(HandleConnection)
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            thread.Start();
        }

        private void HandleConnection()
        {
            WelcomeMessage();
            MainMenu();
            if (playerSession.IsConnected && playerSession.Player == null)
            {
                HandleConnection();
            }
            if (!playerSession.IsConnected)
            {
                return;
            }
            bool first = true;
            while (playerSession != null && playerSession.IsConnected)
            {
                if (first)
                {
                    var motd = DatabaseManager.GetMOTD();
                    if (!string.IsNullOrEmpty(motd))
                    {
                        // show the message of the day
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine();
                        sb.AppendLine($"  {new string('=', 77)}");
                        sb.AppendLine($"|| MESSAGE OF THE DAY");
                        foreach(var line in motd.Split(new[] {Constants.NewLine}, StringSplitOptions.None))
                        {
                            sb.AppendLine($"|| {line}");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        playerSession.Send(sb.ToString());
                    }
                    if (!RoomManager.Instance.RoomExists(playerSession.Player.CurrentRoom))
                    {
                        // Player is trying to load into a room that doesn't exist, so move them to Limbo
                        Game.LogMessage($"WARN: Player {playerSession.Player.Name} tried to load into Room {playerSession.Player.CurrentRoom} which does not exist, transferring to Limbo", LogLevel.Warning, true);
                        playerSession.Send($"The world has shifted and the Gods have transported you to Limbo!{Constants.NewLine}");
                        SessionManager.Instance.GetSession(playerSession.ID).Player.CurrentRoom = Game.Limbo;
                    }
                    RoomManager.Instance.LoadPlayerIntoRoom(playerSession.Player.CurrentRoom, playerSession);
                    RoomManager.Instance.GetRoom(playerSession.Player.CurrentRoom).DescribeRoom(playerSession);
                    SessionManager.Instance.UpdateSessionStatus(playerSession.ID, ConnectionState.Playing);
                    SessionManager.Instance.SetLastInputTime(playerSession.ID, DateTime.UtcNow);
                    first = false;
                }
                string prompt = string.Empty;
                switch(playerSession.Player.PromptStyle)
                {
                    case PlayerPrompt.Normal:
                        prompt = $"{Constants.NewLine}%BRT%{playerSession.Player.CurrentHP:N0}/{playerSession.Player.MaxHP:N0} HP%PT%; %BGT%{playerSession.Player.CurrentMP:N0}/{playerSession.Player.MaxMP:N0} MP%PT%; %BYT%{playerSession.Player.CurrentSP:N0}/{playerSession.Player.MaxSP:N0} SP%PT% >>";
                        break;

                    case PlayerPrompt.Percentage:
                        prompt = $"{Constants.NewLine}%BRT%{Math.Round((double)playerSession.Player.CurrentHP / playerSession.Player.MaxHP * 100, 0)}% HP%PT%; %BGT%{Math.Round((double)playerSession.Player.CurrentMP / playerSession.Player.MaxMP * 100, 0)}% MP%PT%; %BYT%{Math.Round((double)playerSession.Player.CurrentSP / playerSession.Player.MaxSP * 100, 0)}% SP%PT% >>";
                        break;
                }
                playerSession.Send(prompt);
                var input = playerSession.Read();
                if (playerSession.Player.Flags.HasFlag(PlayerFlags.Frozen))
                {
                    var freezeDuration = playerSession.Player.GetRemainingFreezeDuration();
                    if (freezeDuration <= 0)
                    {
                        playerSession.Player.ThawPlayer();
                        playerSession.Send($"%BMT%The power holding you fades. You can move again.%PT%{Constants.NewLine}");
                    }
                    else
                    {
                        playerSession.Send($"%BMT%You have been frozen by the Gods!%PT%{Constants.NewLine}");
                        playerSession.Send($"%BMT%You must wait {(int)freezeDuration} minutes before you can do anything!%PT%{Constants.NewLine}");
                        continue;
                    }
                }
                if (!string.IsNullOrEmpty(input))
                {
                    string tInput = input.Trim();
                    if (ValidateInput(tInput))
                    {
                        try
                        {
                            CommandParser.Parse(playerSession, ref tInput);
                        }
                        catch (Exception ex)
                        {
                            Game.LogMessage($"ERROR: Error parsing input from {playerSession.Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
                            Game.LogMessage($"DEBUG: Input from {playerSession.Client.Client.RemoteEndPoint}: {tInput}", LogLevel.Debug, true);
                            playerSession.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        Game.LogMessage($"ERROR: Input from {playerSession.Client.Client.RemoteEndPoint} could not be validated", LogLevel.Error, true);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                // string is empty
                return false;
            }
            if (Encoding.UTF8.GetByteCount(input) != input.Length)
            {
                // string length and byte count don't match
                return false;
            }
            // TODO: Possibly some additional RegEx checks to make sure we're not sending bizarro special characters
            return true;
        }

        private void WelcomeMessage()
        {
            try
            {
                string welcomePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world");
                string[] welcomeMessage = File.ReadAllLines($"{welcomePath}\\welcome.txt");
                StringBuilder sb = new StringBuilder();
                foreach(string line in welcomeMessage)
                {
                    sb.AppendLine(Helpers.ParseColourCodes(line));
                }
                playerSession.Send(Constants.BoldText);
                playerSession.Send(sb.ToString());
                playerSession.Send(Constants.PlainText);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("1. Log In");
                sb.AppendLine("2. Create Character");
                sb.AppendLine("3. Exit");
                sb.AppendLine("Choice: ");
                playerSession.Send(sb.ToString());
                Game.LogMessage($"ERROR: Error sending welcome message to {playerSession.Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
            }
        }

        private void MainMenu()
        {
            SessionManager.Instance.UpdateSessionStatus(playerSession.ID, ConnectionState.MainMenu);
            bool validSelection = false;
            while (playerSession != null && playerSession.IsConnected && !validSelection)
            {
                try
                {
                    var playerInput = playerSession.Read();
                    if (playerInput != null && int.TryParse(playerInput.Trim(), out int option))
                    {
                        switch(option)
                        {
                            case 1:
                                var player = LogonProvider.LogonPlayer(playerSession);
                                if (player != null)
                                {
                                    SessionManager.Instance.UpdateSessionPlayer(playerSession.ID, player);
                                    validSelection = true;
                                }
                                break;

                            case 2:
                                player = CreateNewCharacter();
                                if (player != null)
                                {
                                    SessionManager.Instance.UpdateSessionPlayer(playerSession.ID, player);
                                    validSelection = true;
                                }
                                break;

                            case 3:
                                validSelection = true;
                                SessionManager.Instance.Close(playerSession);
                                break;

                            default:
                                playerSession.Send($"Sorry, that is not a valid option.{Constants.NewLine}");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.LogMessage($"ERROR: Error reading from socket at Connecton.MainMenu(): {ex.Message}", LogLevel.Error, true);
                    SessionManager.Instance.Close(playerSession);
                }
            }
        }

        private Player CreateNewCharacter()
        {
            SessionManager.Instance.UpdateSessionStatus(playerSession.ID, ConnectionState.CreatingCharacter);
            Player newPlayer = CharacterCreator.CreateNewCharacter(playerSession);
            if (newPlayer == null)
            {
                playerSession.Send($"A valid character was not created, returning to the main menu...{Constants.NewLine}");
                MainMenu();
            }
            return newPlayer;
        }
    }
}
