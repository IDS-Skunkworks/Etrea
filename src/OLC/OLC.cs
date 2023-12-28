using Kingdoms_of_Etrea.Core;
using System;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Menus
        private static void CreateNewObject(ref Descriptor desc)
        {
            bool validInput = false;
            while (!validInput)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("This will allow you to create a new game asset for use in the world.");
                sb.AppendLine("Which type of asset do you want to create:");
                sb.AppendLine("1. Item");
                sb.AppendLine("2. Zone");
                sb.AppendLine("3. Room");
                sb.AppendLine("4. Shop");
                sb.AppendLine("5. NPC");
                sb.AppendLine("6. Emote");
                sb.AppendLine("7. Resource Node");
                sb.AppendLine("8. Crafting Recipe");
                sb.AppendLine("9. Quest");
                sb.AppendLine("10. Return to main menu");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input))
                {
                    if (uint.TryParse(input, out uint option))
                    {
                        if (option > 0 && option <= 10)
                        {
                            switch (option)
                            {
                                case 1:
                                    CreateNewItem(ref desc);
                                    break;

                                case 2:
                                    CreateNewZone(ref desc);
                                    break;

                                case 3:
                                    CreateNewRoom(ref desc);
                                    break;

                                case 4:
                                    CreateNewShop(ref desc);
                                    break;

                                case 5:
                                    CreateNewNPC(ref desc);
                                    break;

                                case 6:
                                    CreateNewEmote(ref desc);
                                    break;

                                case 7:
                                    CreateNewResourceNode(ref desc);
                                    break;

                                case 8:
                                    CreateNewCraftingRecipe(ref desc);
                                    break;

                                case 9:
                                    CreateNewQuest(ref desc);
                                    break;

                                case 10:
                                    validInput = true;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static void EditExistingObject(ref Descriptor desc)
        {
            bool isValid = false;
            StringBuilder sb = new StringBuilder();
            while (!isValid)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine("This will allow you to modify an existing world asset.");
                sb.AppendLine("Which type of asset do you wish to modify?");
                sb.AppendLine("1. Item");
                sb.AppendLine("2. Zone");
                sb.AppendLine("3. Room");
                sb.AppendLine("4. Shop");
                sb.AppendLine("5. NPC");
                sb.AppendLine("6. Emote");
                sb.AppendLine("7. Resource Node");
                sb.AppendLine("8. Crafting Recipe");
                sb.AppendLine("9. Quest");
                sb.AppendLine("10. Return to main menu");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 10)
                    {
                        switch(result)
                        {
                            case 1:
                                EditExistingItem(ref desc);
                                break;

                            case 2:
                                EditExistingZone(ref desc);
                                break;

                            case 3:
                                EditExistingRoom(ref desc);
                                break;

                            case 4:
                                EditExistingShop(ref desc);
                                break;

                            case 5:
                                EditExistingNPC(ref desc);
                                break;

                            case 6:
                                EditExistingEmote(ref desc);
                                break;

                            case 7:
                                EditExistingNode(ref desc);
                                break;

                            case 8:
                                EditExistingRecipe(ref desc);
                                break;

                            case 9:
                                EditExistingQuest(ref desc);
                                break;

                            case 10:
                                isValid = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void DeleteExistingObject(ref Descriptor desc)
        {
            bool isValid = false;
            StringBuilder sb = new StringBuilder();
            while(!isValid)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine("This will allow you to delete existing game assets.");
                sb.AppendLine("Deleted assets cannot be recovered unless a backup of the World database is restored.");
                sb.AppendLine("Which type of asset do you wish to delete?");
                sb.AppendLine("1. Item");
                sb.AppendLine("2. Zone");
                sb.AppendLine("3. Room");
                sb.AppendLine("4. Shop");
                sb.AppendLine("5. NPC");
                sb.AppendLine("6. Emote");
                sb.AppendLine("7. Resource Node");
                sb.AppendLine("8. Crafting Recipe");
                sb.AppendLine("9. Quest");
                sb.AppendLine("10. Return to main menu");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 10)
                    {
                        switch(result)
                        {
                            case 1:
                                DeleteItem(ref desc);
                                break;

                            case 2:
                                DeleteZone(ref desc);
                                break;

                            case 3:
                                DeleteRoom(ref desc);
                                break;

                            case 4:
                                DeleteShop(ref desc);
                                break;

                            case 5:
                                DeleteNPC(ref desc);
                                break;

                            case 6:
                                DeleteEmote(ref desc);
                                break;

                            case 7:
                                DeleteResourceNode(ref desc);
                                break;

                            case 8:
                                DeleteCraftingRecipe(ref desc);
                                break;

                            case 9:
                                DeleteQuest(ref desc);
                                break;

                            case 10:
                                isValid = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        internal static void StartOLC(ref Descriptor desc)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                Game.LogMessage($"INFO: Player {desc.Player.Name} has started OLC", LogLevel.Info, true);
                bool validInput = false;
                while (!validInput)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Welcome to the Online Constructor (OLC), {desc.Player.Name}!");
                    sb.AppendLine("Using this tool you can create, edit and delete game objects such as Zones,");
                    sb.AppendLine("Rooms, Items and NPCs.");
                    sb.AppendLine("To begin, choose from the following options:");
                    sb.AppendLine("1. Create game assets");
                    sb.AppendLine("2. Edit game assets");
                    sb.AppendLine("3. Delete game assets");
                    sb.AppendLine("4. Exit OLC and return to the game");
                    sb.Append("Selection: ");
                    desc.Send(sb.ToString());
                    var input = desc.Read().Trim();
                    if (Helpers.ValidateInput(input))
                    {
                        if (uint.TryParse(input, out uint option))
                        {
                            if (option > 0 && option <= 4)
                            {
                                switch (option)
                                {
                                    case 1:
                                        CreateNewObject(ref desc);
                                        break;

                                    case 2:
                                        EditExistingObject(ref desc);
                                        break;

                                    case 3:
                                        DeleteExistingObject(ref desc);
                                        break;

                                    case 4:
                                        desc.Send($"Goodbye!{Constants.NewLine}");
                                        Game.LogMessage($"INFO: Player {desc.Player.Name} has exited OLC", LogLevel.Info, true);
                                        validInput = true;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                desc.Send($"Only the Gods may reshape reality!{Constants.NewLine}");
                Game.LogMessage($"WARN: Player {desc.Player.Name} attempted to start OLC", LogLevel.Warning, true);
            }
            
        }
        #endregion

        #region MiscFunctions
        private static bool GetAssetBooleanValue(ref Descriptor desc, string prompt)
        {
            desc.Send(prompt);
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && bool.TryParse(input, out bool retval))
            {
                return retval;
            }
            desc.Send($"Input could not be validated{Constants.NewLine}");
            return false;
        }

        private static string GetAssetStringValue(ref Descriptor desc, string prompt)
        {
            desc.Send(prompt);
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input))
            {
                return input;
            }
            desc.Send($"Input could not be validated{Constants.NewLine}");
            return string.Empty;
        }

        private static uint GetAssetUintValue(ref Descriptor desc, string prompt)
        {
            desc.Send(prompt);
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint retval))
            {
                return retval;
            }
            desc.Send($"Input must be an integer >= 0{Constants.NewLine}");
            return 0;
        }

        private static T GetAssetEnumValue<T>(ref Descriptor desc, string prompt) where T : struct, Enum
        {
            desc.Send(prompt);
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && Enum.TryParse<T>(input, true, out T retval))
            {
                return retval;
            }
            desc.Send($"Unable to parse input to a valid value{Constants.NewLine}");
            return default;
        }

        private static int GetAssetIntegerValue(ref Descriptor desc, string prompt)
        {
            desc.Send(prompt);
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && int.TryParse(input, out int retval))
            {
                return retval;
            }
            desc.Send($"Input must be a valid integer{Constants.NewLine}");
            return 0;
        }
        #endregion
    }
}