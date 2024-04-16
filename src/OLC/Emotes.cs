using Etrea2.Core;
using Etrea2.Entities;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
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
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Emote ID: {e.ID}{Constants.TabStop}Emote Name: {e.EmoteName}");
                sb.AppendLine($"Message to Player with Target:{Constants.NewLine}{e.MsgToPlayerWithTarget.Trim()}");
                sb.AppendLine($"Message to Player with no Target:{Constants.NewLine}{e.MsgToPlayerWithNoTarget.Trim()}");
                sb.AppendLine($"Message to Target:{Constants.NewLine}{e.MsgToTarget.Trim()}");
                sb.AppendLine($"Message to Others with Target:{Constants.NewLine}{e.MsgToOthersWithTarget.Trim()}");
                sb.AppendLine($"Message to Others with no Target:{Constants.NewLine}{e.MsgToOthersWithNoTarget.Trim()}");
                sb.AppendLine($"Message to Others with Vis Player and Invis Target:{Constants.NewLine}{e.MsgToOthersWithVisPlayerAndInvisTarget.Trim()}");
                sb.AppendLine($"Message to Others with Invis Player and Invis Target:{Constants.NewLine}{e.MsgToOthersWithInvisPlayerAndTarget.Trim()}");
                sb.AppendLine($"Message to Player Target not found:{Constants.NewLine}{e.MsgToPlayerTargetNotFound.Trim()}");
                sb.AppendLine($"Message to Others Target not found:{Constants.NewLine}{e.MsgToOthersTargetNotFound.Trim()}");
                sb.AppendLine($"Message to Others when Target is Player:{Constants.NewLine}{e.MsgToOthersWhenTargetIsPlayer.Trim()}");
                sb.AppendLine($"Message to Player when Target is Player:{Constants.NewLine}{e.MsgToPlayerWhenTargetIsPlayer.Trim()}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Emote ID{Constants.TabStop}{Constants.TabStop}2. Set Emote Name");
                sb.AppendLine($"3. Set Message to Player with Target{Constants.TabStop}4. Set Message to Player with no Target");
                sb.AppendLine($"5. Set Message to Target{Constants.TabStop}6. Set Message to Others with Target");
                sb.AppendLine($"7. Set Message to Others with no Target");
                sb.AppendLine("8. Set Message to others with Vis Player and Inivs Target");
                sb.AppendLine($"9. Set Message to Others with Invis Player and Invis Target");
                sb.AppendLine($"10. Set Message to Player when Target not Found{Constants.TabStop}11. Set Message to Others when Target not found");
                sb.AppendLine($"11. Set Message to Others Target not found{Constants.TabStop}12. Set Message to Others when Target is Player");
                sb.AppendLine("13. Set Message to Player when Target is Player");
                sb.AppendLine($"14. Save{Constants.TabStop}15. Exit");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 15)
                    {
                        switch (option)
                        {
                            case 1:
                                e.ID = GetAssetUintValue(ref desc, "Enter Emote ID: ");
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
                                if (ValidateEmoteAsset(ref desc, e, true))
                                {
                                    if (DatabaseManager.AddNewEmote(ref desc, e))
                                    {
                                        if (EmoteManager.Instance.AddEmote(e, ref desc))
                                        {
                                            desc.Send($"Emote saved successfully{Constants.NewLine}");
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
            descriptor.Send($"Enter the ID or Emote Name to edit or END to return: ");
            var input = descriptor.Read().Trim();
            if (Helpers.ValidateInput(input))
            {
                if (input == "END")
                {
                    return;
                }
                Emote e = null;
                if (uint.TryParse(input, out uint emoteID))
                {
                    if (EmoteManager.Instance.EmoteExists(emoteID))
                    {
                        e = EmoteManager.Instance.GetEmoteByID(emoteID).ShallowCopy();
                    }
                }
                else
                {
                    if (EmoteManager.Instance.EmoteExists(input))
                    {
                        e = EmoteManager.Instance.GetEmoteByName(input).ShallowCopy();
                    }
                }
                if (e != null)
                {
                    bool okToReturn = false;
                    StringBuilder sb = new StringBuilder();
                    while (!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Emote ID: {e.ID}{Constants.TabStop}Emote Name: {e.EmoteName}");
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
                        sb.AppendLine($"1. Set Emote Name{Constants.TabStop}2. Set Message to Player with Target");
                        sb.AppendLine($"3. Set Message to Player with no Target{Constants.TabStop}4. Set Message to Target");
                        sb.AppendLine($"5. Set Message to Others with Target{Constants.TabStop}6. Set Message to Others with no Target");
                        sb.AppendLine($"7. Set Message to others with Vis Player and Inivs Target{Constants.TabStop}8. Set Message to Others with Invis Player and Invis Target");
                        sb.AppendLine($"9. Set Message to Player when Target not found{Constants.TabStop}10. Set Message to Others Target not found");
                        sb.AppendLine($"11. Set Message to Others when Target is Player{Constants.TabStop}12. Set Message to Player when Target is Player");
                        sb.AppendLine($"13. Save{Constants.TabStop}14. Exit");
                        sb.Append("Selection: ");
                        descriptor.Send(sb.ToString());
                        var choice = descriptor.Read().Trim();
                        if (Helpers.ValidateInput(choice) && uint.TryParse(choice, out uint option))
                        {
                            if (option >= 1 && option <= 14)
                            {
                                switch (option)
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
                                        if (ValidateEmoteAsset(ref descriptor, e))
                                        {
                                            if (DatabaseManager.UpdateEmoteByID(ref descriptor, ref e))
                                            {
                                                if (EmoteManager.Instance.UpdateEmoteByID(e.ID, ref descriptor, e))
                                                {
                                                    descriptor.Send($"Emote updated successfully.{Constants.NewLine}");
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

        #region Delete
        private static void DeleteEmote(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a backup of the database is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send($"Enter the ID of the Emote to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint emoteID))
            {
                Emote e = EmoteManager.Instance.GetEmoteByID(emoteID);
                if (e != null)
                {
                    if (DatabaseManager.DeleteEmoteByID(ref desc, e.ID))
                    {
                        if (EmoteManager.Instance.RemoveEmote(emoteID, ref desc))
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

        #region Validation Functions
        private static bool ValidateEmoteAsset(ref Descriptor descriptor, Emote e, bool newEmote = false)
        {
            if (e == null)
            {
                return false;
            }
            if (e.ID == 0)
            {
                descriptor.Send($"EmoteID cannot be 0{Constants.NewLine}");
                return false;
            }
            if (newEmote && EmoteManager.Instance.GetEmoteByID(e.ID) != null)
            {
                descriptor.Send($"EmoteID {e.ID} is already in use{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(e.EmoteName))
            {
                descriptor.Send($"EmoteName requires a value{Constants.NewLine}");
                return false;
            }
            if (newEmote && EmoteManager.Instance.GetEmoteByName(e.EmoteName) != null)
            {
                descriptor.Send($"EmoteName '{e.EmoteName}' is already in use{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(e.MsgToPlayerWithTarget) || string.IsNullOrEmpty(e.MsgToPlayerWithNoTarget) || string.IsNullOrEmpty(e.MsgToTarget)
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