using Kingdoms_of_Etrea.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading;
using Kingdoms_of_Etrea.Entities;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class Connection : IInputValidator
    {
        private Descriptor _desc;
        private readonly ILogonProvider _logonProvider;

        internal Connection(Descriptor desc)
        {
            _desc = desc;
            _logonProvider = new LogonProvider();
            var thread = new Thread(HandleConnection);
            thread.IsBackground = true;
            thread.Start();

            // Can't remeber why I added this as it was never finished... Commenting out for now...
            //var monitor = new Thread(MonitorConnection);
            //monitor.IsBackground = true;
            //monitor.Start();
        }

        //private void MonitorConnection()
        //{
        //    if(_desc != null)
        //    {

        //    }
        //    else
        //    {

        //    }
        //}

        private void HandleConnection()
        {
            WelcomeMessage();
            MainMenu();
            bool first = true;
            while (_desc != null && _desc.IsConnected)
            {
                if(first)
                {
                    // if the player is trying to load into a room that no longer exits, port them to Limbo to avoid a crash...
                    uint pStartRoom = _desc.Player.CurrentRoom;
                    if(!RoomManager.Instance.RoomExists(pStartRoom))
                    {
                        Game.LogMessage($"WARN: Player {_desc.Player.Name} tried to load in room {pStartRoom} which doesn't exist - moving to Limo", LogLevel.Warning, true);
                        _desc.Player.CurrentRoom = Constants.LimboRID();
                        _desc.Send($"The world has shifted and the Gods have transported you to Limbo for safe keeping!{Constants.NewLine}");
                    }
                    RoomManager.Instance.ProcessEnvironmentBuffs(pStartRoom);
                    Room.DescribeRoom(ref _desc);
                    first = false;
                }
                string prompt = $"{Constants.NewLine}{Constants.BrightRedText}{_desc.Player.Stats.CurrentHP}/{_desc.Player.Stats.MaxHP} HP{Constants.PlainText}; {Constants.BrightBluetext}{_desc.Player.Stats.CurrentMP}/{_desc.Player.Stats.MaxMP} MP{Constants.PlainText}; {Constants.BrightYellowText}{_desc.Player.Stats.CurrentSP}/{_desc.Player.Stats.MaxSP} SP{Constants.PlainText} >>";
                _desc.Send(prompt);

                var input = _desc.Read();
                _desc.Player.IdleTicks = 0;
                if (!string.IsNullOrEmpty(input))
                {
                    input = input.Trim();
                    if (ValidateInput(input))
                    {
                        try
                        {
                            CommandParser.ParseCommand(ref _desc, input);
                        }
                        catch (Exception ex)
                        {
                            Game.LogMessage($"ERROR: Error parsing input from {_desc.Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
                            _desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public bool ValidateInput(string input)
        {
            return !string.IsNullOrEmpty(input) && Encoding.UTF8.GetByteCount(input) == input.Length;
        }

        private void CreateNewChar()
        {
            _desc.State = ConnectionState.CreatingCharacter;
            CharacterCreator.CreateNewCharacter(ref _desc);
            if(_desc.Player == null)
            {
                _desc.Send($"A valid character was not created, returning to the main menu...{Constants.NewLine}");
                MainMenu();
            }
        }

        private void MainMenu()
        {
            _desc.State = ConnectionState.MainMenu;
            bool validSelection = false;
            while(_desc != null && _desc.IsConnected && !validSelection)
            {
                try
                {
                    var pInput = _desc.Read().Trim();
                    if (ValidateInput(pInput) && int.TryParse(pInput, out int opt))
                    {
                        if (opt >= 1 && opt <= 3)
                        {
                            validSelection = true;
                            if (opt == 1)
                            {
                                _logonProvider.LogonPlayer(ref _desc);
                                if (_desc.Player != null)
                                {
                                    _desc.State = ConnectionState.Playing;
                                    return;
                                }
                                MainMenu();
                            }
                            if (opt == 2)
                            {
                                CreateNewChar();
                            }
                            if (opt == 3)
                            {
                                SessionManager.Instance.Close(_desc);
                            }
                        }
                        else
                        {
                            _desc.Send($"Sorry, that doesn't look like a valid option.{Constants.NewLine}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.LogMessage($"ERROR: Error reading from socket at MainMenu(): {ex.Message}", LogLevel.Error, true);
                    SessionManager.Instance.Close(_desc);
                }
            }
        }

        private void WelcomeMessage()
        {
            try
            {
                string welcomePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
                string[] welcomeLines = File.ReadAllLines($"{welcomePath}\\welcome.txt");
                StringBuilder sb = new StringBuilder();
                foreach(string currentLine in welcomeLines)
                {
                    var line = Regex.Replace(currentLine, "{BR}", Constants.BrightRedText);
                    line = Regex.Replace(line, "{RE}", Constants.PlainText);
                    line = Regex.Replace(line, "{BY}", Constants.BrightYellowText);
                    line = Regex.Replace(line, "{G}", Constants.GreenText);
                    line = Regex.Replace(line, "{W}", Constants.WhiteText);
                    line = Regex.Replace(line, "{BW}", Constants.BrightWhiteText);
                    line = Regex.Replace(line, "{BB}", Constants.BrightBluetext);

                    sb.AppendLine(line);
                }
                _desc.Send(Constants.BoldText);
                _desc.Send(sb.ToString());
                _desc.Send(Constants.PlainText);
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error sending welcome message: {ex.Message}", LogLevel.Error, true);
            }
        }
    }
}
