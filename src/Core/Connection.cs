﻿using Etrea3.Objects;
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
                        prompt = $"{Constants.NewLine}{Constants.BrightRedText}{playerSession.Player.CurrentHP:N0}/{playerSession.Player.MaxHP:N0} HP{Constants.PlainText}; {Constants.BrightGreenText}{playerSession.Player.CurrentMP:N0}/{playerSession.Player.MaxMP:N0} MP{Constants.PlainText}; {Constants.BrightYellowText}{playerSession.Player.CurrentSP:N0}/{playerSession.Player.MaxSP:N0} SP{Constants.PlainText} >>";
                        break;

                    case PlayerPrompt.Percentage:
                        prompt = $"{Constants.NewLine}{Constants.BrightRedText}{Math.Round((double)playerSession.Player.CurrentHP / playerSession.Player.MaxHP * 100, 0)}% HP{Constants.PlainText}; {Constants.BrightGreenText}{Math.Round((double)playerSession.Player.CurrentMP / playerSession.Player.MaxMP * 100, 0)}% MP{Constants.PlainText}; {Constants.BrightYellowText}{Math.Round((double)playerSession.Player.CurrentSP / playerSession.Player.MaxSP * 100, 0)}% SP{Constants.PlainText} >>";
                        break;
                }                    
                playerSession.Send(prompt);
                var input = playerSession.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    SessionManager.Instance.SetLastInputTime(playerSession.ID, DateTime.UtcNow);
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
                string welcomePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
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
