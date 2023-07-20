﻿using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Entities;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Delete
        private static void DeleteEmote(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a backup of the database is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send($"Enter the ID or name of the Emote to delete: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input))
            {
                Emote e = null;
                if(uint.TryParse(input, out uint emoteID))
                {
                    e = EmoteManager.Instance.GetEmoteByID(emoteID);
                }
                else
                {
                    e = EmoteManager.Instance.GetEmoteByName(input);
                }
                if(e != null)
                {
                    if(DatabaseManager.DeleteEmoteByID(ref desc, e.EmoteID))
                    {
                        if(EmoteManager.Instance.RemoveEmote(emoteID))
                        {
                            desc.Send($"Emote successfully removed from EmoteManager and World database.{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"Unable to remove Emote from the EmoteManager{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Unable to remove Emote from the World database{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"No Emote with that ID or name could be found.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Create
        private static void CreateNewEmote(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"An Emote is a way for players to interact with eachother and with NPCs.");
            sb.AppendLine("All properties of an Emote except for its ID can be changed later in other areas of OLC.");
            sb.AppendLine("When setting messages the placeholders {Player} and {Target} can be used");
            sb.AppendLine("and will be replaced with relevant values when the Emote is used.");
            desc.Send(sb.ToString());
            Emote e = new Emote();
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Emote ID: {e.EmoteID}{Constants.TabStop}Emote Name: {e.EmoteName}");
                sb.AppendLine($"Message to Player with Target:{Constants.NewLine}{e.MsgToPlayerWithTarget}");
                sb.AppendLine($"Message to Player with no Target:{Constants.NewLine}{e.MsgToPlayerWithNoTarget}");
                sb.AppendLine($"Message to Target:{Constants.NewLine}{e.MsgToTarget}");
                sb.AppendLine($"Message to Others with Target:{Constants.NewLine}{e.MsgToOthersWithTarget}");
                sb.AppendLine($"Message to Others with no Target:{Constants.NewLine}{e.MsgToOthersWithNoTarget}");
                sb.AppendLine($"Message to Others with Vis Player and Invis Target:{Constants.NewLine}{e.MsgToOthersWithVisPlayerAndInvisTarget}");
                sb.AppendLine($"Message to Others with Invis Player and Invis Target:{Constants.NewLine}{e.MsgToOthersWithInvisPlayerAndTarget}");
                sb.AppendLine($"Message to Player Target not found:{Constants.NewLine}{e.MsgToPlayerTargetNotFound}");
                sb.AppendLine($"Message to Others Target not found:{Constants.NewLine}{e.MsgToOthersTargetNotFound}");
                sb.AppendLine($"Message to Others when Target is Player:{Constants.NewLine}{e.MsgToOthersWhenTargetIsPlayer}");
                sb.AppendLine($"Message to Player when Target is Player:{Constants.NewLine}{e.MsgToPlayerWhenTargetIsPlayer}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Emote ID");
                sb.AppendLine("2. Set Emote Name");
                sb.AppendLine("3. Set Message to Player with Target");
                sb.AppendLine("4. Set Message to Player with no Target");
                sb.AppendLine("5. Set Message to Target");
                sb.AppendLine("6. Set Message to Others with Target");
                sb.AppendLine("7. Set Message to Others with no Target");
                sb.AppendLine("8. Set Message to Others with Vis Player and Invis Target");
                sb.AppendLine("9. Set Message to Others with Invis Player and Invis Target");
                sb.AppendLine("10. Set Message to Player Target not found");
                sb.AppendLine("11. Set Message to Others Target not found");
                sb.AppendLine("12. Set Message to Others when Target is Player");
                sb.AppendLine("13. Set Message to Player when Target is Player");
                sb.AppendLine("14. Save Emote");
                sb.AppendLine("15. Exit without saving");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if(option >=1 && option <= 15)
                    {
                        switch(option)
                        {
                            case 1:
                                e.EmoteID = GetAssetUintValue(ref desc, "Enter Emote ID: ");
                                break;

                            case 2:
                                e.EmoteName = GetAssetStringValue(ref desc, "Enter Emote Name: ");
                                break;

                            case 3:
                                e.MsgToPlayerWithTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 4:
                                e.MsgToPlayerWithNoTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 5:
                                e.MsgToTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 6:
                                e.MsgToOthersWithTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 7:
                                e.MsgToOthersWithNoTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 8:
                                e.MsgToOthersWithVisPlayerAndInvisTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 9:
                                e.MsgToOthersWithInvisPlayerAndTarget = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 10:
                                e.MsgToPlayerTargetNotFound = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 11:
                                e.MsgToOthersTargetNotFound = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 12:
                                e.MsgToOthersWhenTargetIsPlayer = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 13:
                                e.MsgToPlayerWhenTargetIsPlayer = GetAssetStringValue(ref desc, "Enter Message: ");
                                break;

                            case 14:
                                if(ValidateEmoteAsset(ref desc, e, true))
                                {
                                    if(DatabaseManager.AddNewEmote(ref desc, e))
                                    {
                                        if(EmoteManager.Instance.AddEmote(e.EmoteID, e))
                                        {
                                            desc.Send($"Emote saved successfully{Constants.NewLine}");
                                            Game.LogMessage($"INFO: Player {desc.Player.Name} has added Emote {e.EmoteName} ({e.EmoteID}) to the World Database and EmoteManager", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add Emote to EmoteManager, it may not be available until restart{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to save the Emote to the World database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 15:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Edit
        private static void EditExistingEmote(ref Descriptor descriptor)
        {
            descriptor.Send($"Enter the ID or Emote Name to edit: ");
            var input = descriptor.Read().Trim();
            if(Helpers.ValidateInput(input))
            {
                Emote e = null;
                if(uint.TryParse(input, out uint emoteID))
                {
                    e = EmoteManager.Instance.GetEmoteByID(emoteID);
                }
                else
                {
                    e = EmoteManager.Instance.GetEmoteByName(input);
                }
                if(e != null)
                {
                    bool okToReturn = false;
                    StringBuilder sb = new StringBuilder();
                    while(!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Emote ID: {e.EmoteID}{Constants.TabStop}Emote Name: {e.EmoteName}");
                        sb.AppendLine($"Message to Player with Target:{Constants.NewLine}{e.MsgToPlayerWithTarget}");
                        sb.AppendLine($"Message to Player with no Target:{Constants.NewLine}{e.MsgToPlayerWithNoTarget}");
                        sb.AppendLine($"Message to Target:{Constants.NewLine}{e.MsgToTarget}");
                        sb.AppendLine($"Message to Others with Target:{Constants.NewLine}{e.MsgToOthersWithTarget}");
                        sb.AppendLine($"Message to Others with no Target:{Constants.NewLine}{e.MsgToOthersWithNoTarget}");
                        sb.AppendLine($"Message to Others with Vis Player and Invis Target:{Constants.NewLine}{e.MsgToOthersWithVisPlayerAndInvisTarget}");
                        sb.AppendLine($"Message to Others with Invis Player and Invis Target:{Constants.NewLine}{e.MsgToOthersWithInvisPlayerAndTarget}");
                        sb.AppendLine($"Message to Player Target not found:{Constants.NewLine}{e.MsgToPlayerTargetNotFound}");
                        sb.AppendLine($"Message to Others Target not found:{Constants.NewLine}{e.MsgToOthersTargetNotFound}");
                        sb.AppendLine($"Message to Others when Target is Player:{Constants.NewLine}{e.MsgToOthersWhenTargetIsPlayer}");
                        sb.AppendLine($"Message to Player when Target is Player:{Constants.NewLine}{e.MsgToPlayerWhenTargetIsPlayer}");
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine("1. Change Emote Name");
                        sb.AppendLine("2. Change Message to Player with Target");
                        sb.AppendLine("3. Change Message to Player with no Target");
                        sb.AppendLine("4. Change Message to Target");
                        sb.AppendLine("5. Change Message to Others with Target");
                        sb.AppendLine("6. Change Message to Others with no Target");
                        sb.AppendLine("7. Change Message to Others with Vis Player and Invis Target");
                        sb.AppendLine("8. Change Message to Others with Invis Player and Invis Target");
                        sb.AppendLine("9. Change Message to Player Target not found");
                        sb.AppendLine("10. Change Message to Others Target not found");
                        sb.AppendLine("11. Change Message to Others when Target is Player");
                        sb.AppendLine("12. Change Message to Player when Target is Player");
                        sb.AppendLine("13. Save Emote");
                        sb.AppendLine("14. Exit without saving");
                        sb.Append("Selection: ");
                        descriptor.Send(sb.ToString());
                        var choice = descriptor.Read().Trim();
                        if(Helpers.ValidateInput(choice) && uint.TryParse(choice, out uint option))
                        {
                            if(option >= 1 && option <= 14)
                            {
                                switch(option)
                                {
                                    case 1:
                                        e.EmoteName = GetAssetStringValue(ref descriptor, "Enter Emote Name: ");
                                        break;

                                    case 2:
                                        e.MsgToPlayerWithTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 3:
                                        e.MsgToPlayerWithNoTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 4:
                                        e.MsgToTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 5:
                                        e.MsgToOthersWithTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 6:
                                        e.MsgToOthersWithNoTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 7:
                                        e.MsgToOthersWithVisPlayerAndInvisTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 8:
                                        e.MsgToOthersWithInvisPlayerAndTarget = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 9:
                                        e.MsgToPlayerTargetNotFound = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 10:
                                        e.MsgToOthersTargetNotFound = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 11:
                                        e.MsgToOthersWhenTargetIsPlayer = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 12:
                                        e.MsgToPlayerWhenTargetIsPlayer = GetAssetStringValue(ref descriptor, "Enter Message: ");
                                        break;

                                    case 13:
                                        if(ValidateEmoteAsset(ref descriptor, e))
                                        {
                                            if(DatabaseManager.UpdateEmoteByID(ref descriptor, ref e))
                                            {
                                                if(EmoteManager.Instance.UpdateEmoteByID(e.EmoteID, ref descriptor, e))
                                                {
                                                    descriptor.Send($"Emote updated successfully.{Constants.NewLine}");
                                                    Game.LogMessage($"INFO: Player {descriptor.Player.Name} successfully updated Emote {e.EmoteName} ({e.EmoteID})", LogLevel.Info, true);
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    descriptor.Send($"Failed to update Emote in the Emote Manager{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                descriptor.Send($"Failed to update Emote in the World Database{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 14:
                                        okToReturn = true;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            descriptor.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    descriptor.Send($"No matching emote could be found.{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Functions
        private static bool ValidateEmoteAsset(ref Descriptor descriptor, Emote e, bool newEmote = false)
        {
            if (e == null)
            {
                return false;
            }
            if(e.EmoteID == 0)
            {
                descriptor.Send($"EmoteID cannot be 0{Constants.NewLine}");
                return false;
            }
            if(newEmote && EmoteManager.Instance.GetEmoteByID(e.EmoteID) != null)
            {
                descriptor.Send($"EmoteID {e.EmoteID} is already in use{Constants.NewLine}");
                return false;
            }
            if(string.IsNullOrEmpty(e.EmoteName))
            {
                descriptor.Send($"EmoteName requires a value{Constants.NewLine}");
                return false;
            }
            if(newEmote && EmoteManager.Instance.GetEmoteByName(e.EmoteName) != null)
            {
                descriptor.Send($"EmoteName '{e.EmoteName}' is already in use{Constants.NewLine}");
                return false;
            }
            if(string.IsNullOrEmpty(e.MsgToPlayerWithTarget) || string.IsNullOrEmpty(e.MsgToPlayerWithNoTarget) || string.IsNullOrEmpty(e.MsgToTarget)
                || string.IsNullOrEmpty(e.MsgToOthersWithTarget) || string.IsNullOrEmpty(e.MsgToOthersWithNoTarget) || string.IsNullOrEmpty(e.MsgToOthersWithVisPlayerAndInvisTarget)
                || string.IsNullOrEmpty(e.MsgToOthersWithInvisPlayerAndTarget) || string.IsNullOrEmpty(e.MsgToPlayerTargetNotFound) || string.IsNullOrEmpty(e.MsgToOthersTargetNotFound)
                || string.IsNullOrEmpty(e.MsgToOthersWhenTargetIsPlayer))
            {
                descriptor.Send($"One or more requires messages are empty{Constants.NewLine}");
                return false;
            }

            return true;
        }
        #endregion
    }
}