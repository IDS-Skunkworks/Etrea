using Etrea3.Objects;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Etrea3.Core
{
    public static class CharacterCreator
    {
        private static readonly string pattern = @"^[A-Za-z]+$";
        private static readonly Regex regex = new Regex(pattern);

        public static Player CreateNewCharacter(Session playerSession)
        {
            Game.LogMessage($"INFO: Client at {playerSession.Client.Client.RemoteEndPoint} has started the Character Creator", LogLevel.Info, true);
            Player player = new Player
            {
                ID = playerSession.ID,
            };
            player.KnownLanguages.TryAdd("Common", true);
            bool charComplete = false;
            bool statsAssigned = false;
            string charPassword = string.Empty;
            StringBuilder sb = new StringBuilder();
            while (!charComplete)
            {
                sb.Clear();
                ShowCharData(playerSession, player);
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Race{Constants.TabStop}{Constants.TabStop}2. Set Class{Constants.TabStop}{Constants.TabStop}3. Assign Stat Points");
                sb.AppendLine($"4. Set Title{Constants.TabStop}{Constants.TabStop}5. Set Name{Constants.TabStop}{Constants.TabStop}6. Set Short Description");
                sb.AppendLine($"7. Set Long Description{Constants.TabStop}{Constants.TabStop}8. Set Gender{Constants.TabStop}{Constants.TabStop}9. Set Password");
                sb.AppendLine($"10. Start Playing{Constants.TabStop}{Constants.TabStop}11. Return to Main Menu");
                sb.AppendLine("Selection:");
                playerSession.Send(sb.ToString());
                var input = playerSession.Read();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }
                if (!int.TryParse(input, out int option))
                {
                    playerSession.Send($"That does not look like a valid option...{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        if (!statsAssigned)
                        {
                            player.Race = GetPlayerRace(playerSession);
                        }
                        else
                        {
                            playerSession.Send($"You have already committed your stat points, your Race can no longer be changed.{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        player.Class = GetPlayerClass(playerSession);
                        break;

                    case 3:
                        if (player.Race == Race.None)
                        {
                            playerSession.Send($"You must choose a Race before you can assign Stat Points.{Constants.NewLine}");
                            break;
                        }
                        if (statsAssigned)
                        {
                            playerSession.Send($"You have already assigned your Stat Points!{Constants.NewLine}");
                            break;
                        }
                        AssignStatPoints(playerSession, ref player);
                        statsAssigned = true;
                        break;

                    case 4:
                        player.Title = GetPlayerTitle(playerSession);
                        break;

                    case 5:
                        player.Name = GetPlayerName(playerSession);
                        break;

                    case 6:
                        player.ShortDescription = GetPlayerShortDescription(playerSession);
                        break;

                    case 7:
                        player.LongDescription = GetPlayerLongDescription(playerSession);
                        break;

                    case 8:
                        player.Gender = GetPlayerGender(playerSession);
                        break;

                    case 9:
                        charPassword = GetPlayerPassword(playerSession);
                        break;

                    case 10:
                        if (!statsAssigned)
                        {
                            playerSession.Send($"You need to assign your stat points first...{Constants.NewLine}");
                            break;
                        }
                        if (string.IsNullOrEmpty(charPassword))
                        {
                            playerSession.Send($"You need to provide a password for your character...{Constants.NewLine}");
                            break;
                        }
                        if (!ValidateCharacter(playerSession, ref player))
                        {
                            break;
                        }
                        charComplete = true;
                        break;

                    case 11:
                        player = null;
                        return null;

                    default:
                        playerSession.Send($"That does not look like a valid option...{Constants.NewLine}");
                        break;
                }
            }
            FinalisePlayer(ref player);
            playerSession.Player = player;
            DatabaseManager.SavePlayer(playerSession, true, charPassword);
            return player;
        }

        private static void FinalisePlayer(ref Player player)
        {
            switch(player.Race)
            {
                case Race.Elf:
                    player.Intelligence += 2;
                    player.Charisma += 1;
                    player.Constitution -= 1;
                    player.Strength -= 1;
                    player.KnownLanguages.TryAdd("Elvish", true);
                    break;

                case Race.HalfElf:
                    player.Intelligence += 1;
                    player.Dexterity += 1;
                    player.KnownLanguages.TryAdd("Elvish", true);
                    break;

                case Race.Orc:
                    player.Strength += 2;
                    player.Constitution += 2;
                    player.Intelligence -= 2;
                    player.Charisma -= 2;
                    player.KnownLanguages.TryAdd("Orcish", true);
                    break;

                case Race.Dwarf:
                    player.Strength += 1;
                    player.Constitution += 2;
                    player.Dexterity -= 1;
                    player.Charisma -= 1;
                    player.KnownLanguages.TryAdd("Dwarvish", true);
                    break;

                case Race.Hobbit:
                    player.Dexterity += 2;
                    player.Charisma += 1;
                    player.Strength -= 2;
                    player.Constitution -= 1;
                    break;
            }
            switch(player.Class)
            {
                case ActorClass.Wizard:
                    player.Intelligence += 2;
                    player.Strength -= 1;
                    player.Constitution -= 1;
                    player.KnownLanguages.TryAdd("Draconic", true);
                    player.MaxHP = Math.Max(4, 4 + Helpers.CalculateAbilityModifier(player.Constitution));
                    player.MaxMP = Math.Max(10, 10 + Helpers.CalculateAbilityModifier(player.Intelligence));
                    player.AddSpell("Magic Missile");
                    player.AddSpell("Mage Armour");
                    player.AddSkill("Simple Weapons");
                    break;

                case ActorClass.Thief:
                    player.Dexterity += 2;
                    player.Strength -= 1;
                    player.Constitution -= 1;
                    player.MaxHP = Math.Max(6, 6 + Helpers.CalculateAbilityModifier(player.Constitution));
                    player.MaxMP = Math.Max(6, 6 + Helpers.CalculateAbilityModifier(player.Intelligence));
                    player.AddSkill("Light Armour");
                    player.AddSkill("Simple Weapons");
                    player.AddSkill("Martial Weapons");
                    break;

                case ActorClass.Cleric:
                    player.Wisdom += 2;
                    player.Strength -= 1;
                    player.Constitution -= 1;
                    player.MaxHP = Math.Max(8, 8 + Helpers.CalculateAbilityModifier(player.Constitution));
                    player.MaxMP = Math.Max(8, 8 + Helpers.CalculateAbilityModifier(player.Wisdom));
                    player.AddSpell("Cure Light Wounds");
                    player.AddSpell("Bless");
                    player.AddSkill("Simple Weapons");
                    player.AddSkill("Martial Weapons");
                    player.AddSkill("Light Armour");
                    player.AddSkill("Medium Armour");
                    break;

                case ActorClass.Fighter:
                    player.Strength += 2;
                    player.Constitution += 2;
                    player.Dexterity -= 1;
                    player.Intelligence -= 1;
                    player.Charisma -= 1;
                    player.MaxHP = Math.Max(10, 10 + Helpers.CalculateAbilityModifier(player.Constitution));
                    player.MaxMP = Math.Max(4, 4 + Helpers.CalculateAbilityModifier(player.Intelligence));
                    player.AddSkill("Light Armour");
                    player.AddSkill("Medium Armour");
                    player.AddSkill("Heavy Armour");
                    player.AddSkill("Simple Weapons");
                    player.AddSkill("Martial Weapons");
                    break;
            }
            player.Gold = 10 + Helpers.RollDice<ulong>(10, 10);
            player.MaxSP = Math.Max(20, 20 + Helpers.CalculateAbilityModifier(player.Constitution));
            player.CurrentHP = player.MaxHP;
            player.CurrentMP = player.MaxMP;
            player.CurrentSP = player.MaxSP;
            player.CurrentRoom = Game.PlayerStartRoom;
        }

        private static bool ValidateCharacter(Session session, ref Player p)
        {
            if (p.Race == Race.None)
            {
                session.Send($"You need to select a Race to continue...{Constants.NewLine}");
                return false;
            }
            if (p.Class == ActorClass.Undefined)
            {
                session.Send($"You need to choose a Class to continue...{Constants.NewLine}");
                return false;
            }
            if (p.Gender == Gender.Undefined)
            {
                session.Send($"You need to select a Gender to continue...{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(p.Name))
            {
                session.Send($"You need to choose a Name to continue...{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(p.ShortDescription))
            {
                session.Send($"You need to give yourself a short description to continue...{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(p.LongDescription))
            {
                session.Send($"You need to give yourself a full description to continue...{Constants.NewLine}");
                return false;
            }
            return true;
        }

        private static void ShowCharData(Session session, Player p)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Title: {p.Title}{Constants.TabStop}Name: {p.Name}");
            sb.AppendLine($"Gender: {p.Gender}{Constants.TabStop}Race: {p.Race}{Constants.TabStop}Class: {p.Class}");
            sb.AppendLine($"Short Description: {p.ShortDescription}");
            sb.AppendLine($"Long Description: {p.LongDescription}");
            sb.AppendLine($"STR: {p.Strength}{Constants.TabStop}{Constants.TabStop}DEX: {p.Dexterity}{Constants.TabStop}{Constants.TabStop}CON: {p.Constitution}");
            sb.AppendLine($"INT: {p.Intelligence}{Constants.TabStop}{Constants.TabStop}WIS: {p.Wisdom}{Constants.TabStop}{Constants.TabStop}CHA: {p.Charisma}");
            sb.AppendLine("Your final stats will be calculated when you begin playing");
            session.Send(sb.ToString());
        }

        private static Race GetPlayerRace(Session session)
        {
            bool validInput = false;
            Race retval = default;
            while (!validInput)
            {
                session.Send($"Valid Races: Human, HalfElf, Elf, Dwarf, Orc, Hobbit{Constants.NewLine}");
                session.Send($"Human: no stat modifiers, 25% Exp gain, +2 stat points{Constants.NewLine}");
                session.Send($"HalfElf: +1 INT, +1 DEX{Constants.NewLine}");
                session.Send($"Elf: +2 INT, +1 CHA, -1 CON, -1 STR{Constants.NewLine}");
                session.Send($"Dwarf: +1 STR, +2 CON, -1 DEX, -1 CHA{Constants.NewLine}");
                session.Send($"Orc: +2 STR, +2 CON, -2 INT, -2 CHA, 1+ AC{Constants.NewLine}");
                session.Send($"Hobbit: +2 DEX, +1 CHA, -2 STR, -1 CON{Constants.NewLine}");
                session.Send($"Selection (or END to return):");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        validInput = true;
                    }
                    else
                    {
                        if (Enum.TryParse<Race>(input.Trim(), true, out retval))
                        {
                            validInput = true;
                        }
                    }
                }
            }
            return retval;
        }

        private static ActorClass GetPlayerClass(Session session)
        {
            bool validInput = false;
            ActorClass retval = default;
            while (!validInput)
            {
                session.Send($"Valid Classes: Wizard, Thief, Cleric, Fighter{Constants.NewLine}");
                session.Send($"Wizard: +2 INT, -1 STR, -1 CON{Constants.NewLine}");
                session.Send($"Thief: +2 DEX, -1 STR, -1 CON{Constants.NewLine}");
                session.Send($"Cleric: +2 WIS, -1 STR, -1 CON{Constants.NewLine}");
                session.Send($"Fighter: +2 STR, +2 CON, -1 DEX, -1 INT, -1 CHA{Constants.NewLine}");
                session.Send($"Selection (or END to return):");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        validInput = true;
                    }
                    else
                    {
                        if (Enum.TryParse<ActorClass>(input.Trim(), true, out retval))
                        {
                            validInput = true;
                        }
                    }
                }
            }
            return retval;
        }

        private static void AssignStatPoints(Session session, ref Player p)
        {
            int pointsToAllocate = p.Race == Race.Human ? 12 : 10;
            StringBuilder sb = new StringBuilder();
            while(pointsToAllocate > 0)
            {
                sb.Clear();
                sb.AppendLine($"Points available: {pointsToAllocate}");
                sb.AppendLine($"STR: {p.Strength}{Constants.TabStop}{Constants.TabStop}DEX: {p.Dexterity}{Constants.TabStop}{Constants.TabStop}CON: {p.Constitution}");
                sb.AppendLine($"INT: {p.Intelligence}{Constants.TabStop}{Constants.TabStop}WIS: {p.Wisdom}{Constants.TabStop}{Constants.TabStop}CHA: {p.Charisma}");
                sb.AppendLine("Enter Stat to increase:");
                session.Send(sb.ToString());
                var input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    switch(input.Trim().ToUpper())
                    {
                        case "STR":
                            p.Strength++;
                            pointsToAllocate--;
                            break;

                        case "DEX":
                            p.Dexterity++;
                            pointsToAllocate--;
                            break;

                        case "CON":
                            p.Constitution++;
                            pointsToAllocate--;
                            break;

                        case "INT":
                            p.Intelligence++;
                            pointsToAllocate--;
                            break;

                        case "WIS":
                            p.Wisdom++;
                            pointsToAllocate--;
                            break;

                        case "CHA":
                            p.Charisma++;
                            pointsToAllocate--;
                            break;

                        default:
                            session.Send($"That doesn't seem like a valid option...{Constants.NewLine}");
                            break;
                    }
                }
            }
        }

        private static Gender GetPlayerGender(Session session)
        {
            bool validInput = false;
            Gender retval = default;
            while (!validInput)
            {
                session.Send($"Valid Genders: Male, Female, NonBinary{Constants.NewLine}");
                session.Send($"Selection (or END to return):");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        validInput = true;
                    }
                    else
                    {
                        if (Enum.TryParse<Gender>(input.Trim(), true, out retval))
                        {
                            validInput = true;
                        }
                    }
                }
            }
            return retval;
        }

        private static string GetPlayerName(Session session)
        {
            bool validInput = false;
            string pName = string.Empty;
            while (!validInput)
            {
                session.Send($"Enter your name:");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && regex.IsMatch(input.Trim()))
                {
                    pName = input.Trim();
                    validInput = true;
                }
            }
            return pName;
        }

        private static string GetPlayerShortDescription(Session session)
        {
            bool validInput = false;
            string pShortDescription = string.Empty;
            while (!validInput)
            {
                session.Send($"Enter short description, max. 50 characters:");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && input.Trim().Length <= 50)
                {
                    pShortDescription = input.Trim();
                    validInput = true;
                }
            }
            return pShortDescription;
        }

        private static string GetPlayerLongDescription(Session session)
        {
            int row = 1;
            StringBuilder sb = new StringBuilder();
            bool validInput = false;
            session.Send($"Enter your full description. This should be 30 lines or less");
            session.Send($"and each line should be 80 characters or less.");
            session.Send($"Descriptions can be changed later if you want.");
            session.Send($"Enter END on a new line to finish.{Constants.NewLine}");
            while (!validInput)
            {
                session.Send($"[{row}] ");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && input.Trim().Length <= 80)
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        validInput = true;
                    }
                    else
                    {
                        sb.AppendLine(input.Trim());
                        row++;
                        if (row > 30)
                        {
                            validInput = true;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private static string GetPlayerTitle(Session session)
        {
            bool validInput = false;
            string pTitle = string.Empty;
            while (!validInput)
            {
                session.Send($"Enter your title, max. 15 characters:");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && regex.IsMatch(input.Trim()) && input.Trim().Length <= 15)
                {
                    pTitle = input.Trim();
                    validInput = true;
                }
            }
            return pTitle;
        }

        private static string GetPlayerPassword(Session session)
        {
            bool validInput = false;
            string pPassword = string.Empty;
            while (!validInput)
            {
                session.Send($"Enter your password:");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    pPassword = input.Trim();
                    validInput = true;
                }
            }
            return pPassword;
        }
    }
}
