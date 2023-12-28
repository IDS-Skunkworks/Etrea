using Kingdoms_of_Etrea.Entities;
using System.Collections.Generic;
using System;
using System.Text;

namespace Kingdoms_of_Etrea.Core
{
    internal static class CharacterCreator
    {
        private static bool ValidateInput(string input)
        {
            return !string.IsNullOrEmpty(input) && Encoding.UTF8.GetByteCount(input) == input.Length;
        }

        internal static void CreateNewCharacter(ref Descriptor _desc)
        {
            // select race, select class, modify stats, select gender, enter name, title & description
            Game.LogMessage($"INFO: Client at {_desc.Client.Client.RemoteEndPoint} has started the character creator", LogLevel.Info, true);
            _desc.Player = null;
            bool validRace = false;
            bool validClass = false;
            bool validStats = false;
            bool validGender = false;
            bool validNameAndDesc = false;
            bool validPwd = false;
            uint statPointsToAllocate = 10;
            string playerName = string.Empty;
            string playerTitle = string.Empty;
            string playerLongDesc = string.Empty;
            string playerShortDesc = string.Empty;
            string playerPwd = string.Empty;
            Player p = new Player();
            p.ShowDetailedRollInfo = false;
            p.Visible = true;
            p.Stats = new ActorStats
            {
                Strength = 10,
                Dexterity = 10,
                Constitution= 10,
                Intelligence = 10,
                Wisdom = 10,
                Charisma = 10,
                ArmourClass = 10
            };
            p.EquippedItems = new EquippedItems();
            p.Inventory = new List<InventoryItem>();
            p.Buffs = new Dictionary<string, int>();
            p.Skills = new List<Skills.Skill>();
            p.Spells = new List<Spells.Spell>();
            p.KnownRecipes = new List<Crafting.Recipe>();
            p.CompletedQuests = new HashSet<Guid>();
            p.ActiveQuests = new List<Quest>();
            p.KnownLanguages |= Languages.Common;
            p.SpokenLanguage = Languages.Common;
            p.NumberOfAttacks = 1;
            p.PVP = false;
            while (!validRace)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Select a race for your new character:");
                sb.AppendLine("1. Human (no stat modifiers, +25% Exp gain, bonus stat points)");
                sb.AppendLine("2. Elf (+2 Int, +1 Cha, -1 Con, -1 Str)");
                sb.AppendLine("3. Half-Elf (+1 Int, +1 Dex)");
                sb.AppendLine("4. Orc (+2 Str, +2 Con, -2 Int, -2 Cha, +1 AC)");
                sb.AppendLine("5. Dwarf (+1 Str, +2 Con, -1 Dex, -1 Cha)");
                sb.AppendLine("6. Hobbit (+2 Dex, +1 Cha, -2 Str, -1 Con)");
                sb.AppendLine("7. Exit Character Creator");
                sb.Append("Selection: ");
                _desc.Send(sb.ToString());
                var input = _desc.Read().Trim();
                if (ValidateInput(input))
                {
                    if(uint.TryParse(input, out uint selection))
                    {
                        switch(selection)
                        {
                            case 1:
                                p.Race = ActorRace.Human;
                                statPointsToAllocate = 12;
                                validRace = true;
                                break;

                                case 2:
                                p.Race = ActorRace.Elf;
                                p.Stats.Intelligence += 2;
                                p.Stats.Charisma += 1;
                                p.Stats.Constitution -= 1;
                                p.Stats.Strength -= 1;
                                p.KnownLanguages |= Languages.Elvish;
                                validRace = true;
                                break;

                                case 3:
                                p.Race = ActorRace.HalfElf;
                                p.Stats.Intelligence += 1;
                                p.Stats.Dexterity += 1;
                                p.KnownLanguages |= Languages.Elvish;
                                validRace = true;
                                break;

                                case 4:
                                p.Race = ActorRace.Orc;
                                p.Stats.Strength += 2;
                                p.Stats.Constitution += 2;
                                p.Stats.Intelligence -= 2;
                                p.Stats.Charisma -= 2;
                                p.Stats.ArmourClass += 1;
                                p.KnownLanguages |= Languages.Orcish;
                                validRace = true;
                                break;

                                case 5:
                                p.Race = ActorRace.Dwarf;
                                p.Stats.Strength += 1;
                                p.Stats.Constitution += 2;
                                p.Stats.Charisma -= 1;
                                p.Stats.Dexterity -= 1;
                                p.KnownLanguages |= Languages.Dwarvish;
                                validRace = true;
                                break;

                                case 6:
                                p.Race = ActorRace.Hobbit;
                                p.Stats.Dexterity += 2;
                                p.Stats.Charisma += 1;
                                p.Stats.Strength -= 2;
                                p.Stats.Constitution -= 1;
                                validRace = true;
                                break;

                                case 7:
                                return;

                            default:
                                _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                                break;
                        }
                    }
                    else
                    {
                        _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"{Constants.NewLine}Sorry, I didn't understand that{Constants.NewLine}");
                }
            }
            while (!validClass)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Select a class for your new character:");
                sb.AppendLine("1. Wizard (+2 Int, -1 Str, -1 Con)");
                sb.AppendLine("2. Thief (+2 Dex, -1 Str, -1 Con)");
                sb.AppendLine("3. Cleric (+2 Wis, -1 Str, -1 Con");
                sb.AppendLine("4. Fighter (+2 Str, +2 Con, -1 Dex, -1 Int, -1 Cha)");
                sb.AppendLine("5. Exit Character Creator");
                sb.Append("Selection: ");
                _desc.Send(sb.ToString());
                var input = _desc.Read().Trim();
                if(ValidateInput(input))
                {
                    if(uint.TryParse(input, out uint selection))
                    {
                        switch(selection)
                        {
                            case 1:
                                p.Stats.Intelligence += 2;
                                p.Stats.Strength -= 1;
                                p.Stats.Constitution -= 1;
                                p.Class = ActorClass.Wizard;
                                p.AddSkill("Light Armour");
                                p.AddSkill("Simple Weapons");
                                p.AddSpell("Magic Missile");
                                p.AddSpell("Light");
                                p.KnownLanguages |= Languages.Draconic;
                                validClass = true;
                                break;
                                
                                case 2:
                                p.Stats.Dexterity += 2;
                                p.Stats.Strength -= 1;
                                p.Stats.Constitution -= 1;
                                p.Class = ActorClass.Thief;
                                p.AddSkill("Light Armour");
                                p.AddSkill("Simple Weapons");
                                p.AddSkill("Martial Weapons");
                                p.AddSkill("Hide");
                                validClass = true;
                                break;

                                case 3:
                                p.Stats.Wisdom += 2;
                                p.Stats.Strength -= 1;
                                p.Stats.Constitution -= 1;
                                p.Class = ActorClass.Cleric;
                                p.AddSkill("Light Armour");
                                p.AddSkill("Simple Weapons");
                                p.AddSkill("Martial Weapons");
                                p.AddSpell("Cure Light Wounds");
                                p.AddSpell("Regen");
                                validClass = true;
                                break;

                                case 4:
                                p.Stats.Strength += 2;
                                p.Stats.Constitution += 2;
                                p.Stats.Dexterity -= 1;
                                p.Stats.Intelligence -= 1;
                                p.Stats.Charisma -= 1;
                                p.Class = ActorClass.Fighter;
                                p.AddSkill("Light Armour");
                                p.AddSkill("Medium Armour");
                                p.AddSkill("Heavy Armour");
                                p.AddSkill("Simple Weapons");
                                p.AddSkill("Martial Weapons");
                                validClass = true;
                                break;

                                case 5:
                                return;

                            default:
                                _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                                break;
                        }
                    }
                    else
                    {
                        _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"{Constants.NewLine}Sorry, I didn't understand that{Constants.NewLine}");
                }
            }
            while (!validStats)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Your current stats from your race and class selection are:");
                sb.AppendLine($"1. Strength:{Constants.TabStop}{Constants.TabStop}{p.Stats.Strength}{Constants.TabStop}({ActorStats.CalculateAbilityModifier(p.Stats.Strength)})");
                sb.AppendLine($"2. Dexterity:{Constants.TabStop}{Constants.TabStop}{p.Stats.Dexterity}{Constants.TabStop}({ActorStats.CalculateAbilityModifier(p.Stats.Dexterity)})");
                sb.AppendLine($"3. Constitution:{Constants.TabStop}{p.Stats.Constitution}{Constants.TabStop}({ActorStats.CalculateAbilityModifier(p.Stats.Constitution)})");
                sb.AppendLine($"4. Intelligence:{Constants.TabStop}{p.Stats.Intelligence}{Constants.TabStop}({ActorStats.CalculateAbilityModifier(p.Stats.Intelligence)})");
                sb.AppendLine($"5. Wisdom:{Constants.TabStop}{Constants.TabStop}{p.Stats.Wisdom}{Constants.TabStop}({ActorStats.CalculateAbilityModifier(p.Stats.Wisdom)})");
                sb.AppendLine($"6. Charisma:{Constants.TabStop}{Constants.TabStop}{p.Stats.Charisma}{Constants.TabStop}({ActorStats.CalculateAbilityModifier(p.Stats.Charisma)})");
                sb.AppendLine("7. Exit Character Creator");
                sb.AppendLine($"You have {Constants.BoldText}{statPointsToAllocate} points{Constants.PlainText} to spend on increasing stats");
                sb.Append("Selection: ");
                _desc.Send(sb.ToString());
                var input = _desc.Read().Trim();
                if(ValidateInput(input))
                {
                    if(uint.TryParse(input, out uint selection))
                    {
                        switch(selection)
                        {
                            case 1:
                                p.Stats.Strength++;
                                statPointsToAllocate--;
                                break;

                            case 2:
                                p.Stats.Dexterity++;
                                statPointsToAllocate--;
                                break;

                            case 3:
                                p.Stats.Constitution++;
                                statPointsToAllocate--;
                                break;

                            case 4:
                                p.Stats.Intelligence++;
                                statPointsToAllocate--;
                                break;

                            case 5:
                                p.Stats.Wisdom++;
                                statPointsToAllocate--;
                                break;

                            case 6:
                                p.Stats.Charisma++;
                                statPointsToAllocate--;
                                break;

                            case 7:
                                return;

                            default:
                                _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                                break;
                        }
                        validStats = statPointsToAllocate == 0;
                    }
                    else
                    {
                        _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"{Constants.NewLine}Sorry, I didn't understand that{Constants.NewLine}");
                }
            }
            while (!validGender)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Select a gender for your new character:");
                sb.AppendLine("1. Male");
                sb.AppendLine("2. Female");
                sb.AppendLine("3. Genderless");
                sb.AppendLine("4. Exit Character Creator");
                sb.Append("Selection: ");
                _desc.Send(sb.ToString());
                var input = _desc.Read().Trim();
                if(ValidateInput(input))
                {
                    if(uint.TryParse(input, out uint selection))
                    {
                        switch(selection)
                        {
                            case 1:
                                p.Gender = Gender.Male;
                                validGender = true;
                                break;

                            case 2:
                                p.Gender = Gender.Female;
                                validGender = true;
                                break;

                            case 3:
                                p.Gender = Gender.Genderless;
                                validGender = true;
                                break;

                            case 4:
                                return;

                            default:
                                _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                                break;
                        }
                    }
                    else
                    {
                        _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"{Constants.NewLine}Sorry, I didn't understand that{Constants.NewLine}");
                }
            }
            while (!validNameAndDesc)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Name: {p.Name ?? string.Empty}");
                sb.AppendLine($"Title: {p.Title ?? string.Empty}");
                sb.AppendLine($"Short Description: {p.ShortDescription ?? string.Empty}");
                sb.AppendLine($"Long Description: {p.LongDescription ?? string.Empty}");
                sb.AppendLine($"Race: {p.Race}");
                sb.AppendLine($"Class: {p.Class}");
                sb.AppendLine($"Gender: {p.Gender}");
                sb.AppendLine($"Languages: {p.KnownLanguages}");
                sb.AppendLine();
                sb.AppendLine("1. Change Name");
                sb.AppendLine("2. Change Title");
                sb.AppendLine("3. Enter Short Description");
                sb.AppendLine("4. Enter Long Description");
                sb.AppendLine("5. Exit Character Creator");
                sb.Append("Selection: ");
                _desc.Send(sb.ToString());
                var input = _desc.Read().Trim();
                if(ValidateInput(input))
                {
                    if(uint.TryParse(input, out uint selection))
                    {
                        switch(selection)
                        {
                            case 1:
                                playerName = GetNewCharName(ref _desc);
                                p.Name = playerName;
                                break;

                            case 2:
                                playerTitle = GetNewCharTitle(ref _desc);
                                p.Title = playerTitle;
                                break;

                            case 3:
                                playerShortDesc = GetNewCharShortDesc(ref _desc);
                                p.ShortDescription = playerShortDesc;
                              break;

                            case 4:
                                playerLongDesc = GetNewCharLongDesc(ref _desc);
                                p.LongDescription = playerLongDesc;
                                break;

                            case 5:
                                return;

                            default:
                                _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                                break;
                        }
                    }
                    else
                    {
                        _desc.Send($"{Constants.NewLine}Sorry, that doesn't seem like a valid option{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"{Constants.NewLine}Sorry, I didn't understand that{Constants.NewLine}");
                }
                validNameAndDesc = !string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(playerTitle) && !string.IsNullOrEmpty(playerShortDesc) && !string.IsNullOrEmpty(playerLongDesc);
            }
            while (!validPwd)
            {
                StringBuilder sb = new StringBuilder();
                _desc.Send($"Please enter a password for the new character{Constants.NewLine}");
                _desc.Send("Password: ");
                var input = _desc.Read().Trim();
                if(ValidateInput(input))
                {
                    playerPwd = input;
                    validPwd = true;
                }
            }

            // Set starting HP/MP and items base off class
            switch (p.Class)
            {
                case ActorClass.Cleric:
                    p.Stats.MaxHP = Convert.ToUInt32(8 + ActorStats.CalculateAbilityModifier(p.Stats.Constitution));
                    p.Stats.MaxMP = Convert.ToUInt32(8 + ActorStats.CalculateAbilityModifier(p.Stats.Wisdom));
                    break;

                case ActorClass.Fighter:
                    p.Stats.MaxHP = Convert.ToUInt32(10 + ActorStats.CalculateAbilityModifier(p.Stats.Constitution));
                    p.Stats.MaxMP = Convert.ToUInt32(4 + ActorStats.CalculateAbilityModifier(p.Stats.Intelligence));
                    break;

                case ActorClass.Thief:
                    p.Stats.MaxHP = Convert.ToUInt32(6 + ActorStats.CalculateAbilityModifier(p.Stats.Constitution));
                    p.Stats.MaxMP = Convert.ToUInt32(6 + ActorStats.CalculateAbilityModifier(p.Stats.Intelligence));
                    break;

                case ActorClass.Wizard:
                    p.Stats.MaxHP = Convert.ToUInt32(4 + ActorStats.CalculateAbilityModifier(p.Stats.Constitution));
                    p.Stats.MaxMP = Convert.ToUInt32(10 + ActorStats.CalculateAbilityModifier(p.Stats.Intelligence));
                    break;

                default:
                    p.Stats.MaxHP = Convert.ToUInt32(6 + ActorStats.CalculateAbilityModifier(p.Stats.Constitution));
                    p.Stats.MaxMP = Convert.ToUInt32(8 + ActorStats.CalculateAbilityModifier(p.Stats.Intelligence));
                    break;
            }

            p.Position = ActorPosition.Standing;
            p.Level = 1;
            p.Stats.CurrentHP = (int)p.Stats.MaxHP;
            p.Stats.CurrentMP = (int)p.Stats.MaxMP;
            p.Stats.CurrentMaxHP = p.Stats.MaxHP;
            p.Stats.CurrentMaxMP = p.Stats.MaxMP;
            p.Stats.ArmourClass = Convert.ToUInt32(10 + ActorStats.CalculateAbilityModifier(p.Stats.Dexterity));
            p.Alignment = ActorAlignment.Neutral;
            p.Stats.Gold = 50 + Helpers.RollDice(25, 2);
            p.CurrentRoom = Constants.PlayerStartRoom();
            p.Type = ActorType.Player;

            _desc.Player = p;

            DatabaseManager.SavePlayerNew(ref _desc, true, playerPwd);

            Game.LogMessage($"INFO: {_desc.Client.Client.RemoteEndPoint} has entered the world as a new character: {p.Name}", LogLevel.Info, true);

            RoomManager.Instance.UpdatePlayersInRoom(Constants.PlayerStartRoom(), ref _desc, false, false, false, true);
        }

        private static string GetNewCharTitle(ref Descriptor _desc)
        {
            bool valid = false;
            string input = string.Empty;
            while (!valid)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Enter a title for your character. An example title might be Countess, Lord or");
                sb.AppendLine("Adventurer. Your title should be 15 characters or less and may be changed at any point in the game.");
                sb.Append("Character Title: ");
                _desc.Send(sb.ToString());
                input = _desc.Read().Trim();
                if (ValidateInput(input))
                {
                    if (input.Length <= 15)
                    {
                        valid = true;
                    }
                    else
                    {
                        _desc.Send($"Sorry, that is too long to be a title.{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                }
            }
            return input;
        }

        private static string GetNewCharShortDesc(ref Descriptor _desc)
        {
            bool valid = false;
            string input = string.Empty;
            while (!valid)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Enter a short description for your character. Short descriptions should be no longer");
                sb.AppendLine("than 40 characters and can be changed at any point in the game.");
                sb.AppendLine("An example short description might be: the cloaked and sinster mage");
                sb.Append("Short Description: ");
                _desc.Send(sb.ToString());
                input = _desc.Read().Trim();
                if (ValidateInput(input))
                {
                    if (input.Length <= 40)
                    {
                        valid = true;
                    }
                    else
                    {
                        _desc.Send($"Sorry, that is too long to be your short description{Constants.NewLine}");
                    }
                }
                else
                {
                    _desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                }
            }
            return input;
        }

        private static string GetNewCharLongDesc(ref Descriptor _desc)
        {
            int row = 1;
            StringBuilder charDesc = new StringBuilder();
            bool valid = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Enter a description for your character. Your description should be no more than");
            sb.AppendLine("30 lines long and each line should be no longer than 80 characters.");
            sb.AppendLine("Your description may be changed at any point in the game.");
            sb.AppendLine("Enter END on a new line to finish editing.");
            _desc.Send(sb.ToString());
            while (!valid)
            {
                _desc.Send($"[{row}] ");
                var input = _desc.Read().Trim();
                if(ValidateInput(input) && input.Length <= 80)
                {
                    if(input.ToUpper() == "END" && row >= 2)
                    {
                        valid = true;
                    }
                    else
                    {
                        charDesc.AppendLine(input);
                        row++;
                        if(row > 30)
                        {
                            valid = true;
                        }
                    }
                }
                else
                {
                    _desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                }
            }
            return charDesc.ToString();
        }

        private static string GetNewCharName(ref Descriptor _desc)
        {
            bool valid = false;
            string input = string.Empty;
            while (!valid)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Enter a name for your character. Names must be unique and no longer than");
                sb.AppendLine("20 characters. The name of your character cannot be changed in the game.");
                sb.Append("Character Name: ");
                _desc.Send(sb.ToString());
                input = _desc.Read().Trim();
                if (ValidateInput(input))
                {
                    if (input.Length <= 20 && !DatabaseManager.CharacterExistsInDatabase(input))
                    {
                        if(input.IndexOf(' ') >= 0)
                        {
                            _desc.Send($"Your name cannot contain whitespace{Constants.NewLine}");
                            valid = false;
                        }
                        else
                        {
                            valid = true;
                        }
                    }
                    else
                    {
                        _desc.Send($"Either that name is already taken or it is too long.{Constants.NewLine}");
                    }    
                }
                else
                {
                    _desc.Send($"Sorry, that doesn't seem to be valid.{Constants.NewLine}");
                }
            }
            return input;
        }
    }
}
