using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Entities;
using Kingdoms_of_Etrea.Interfaces;
using System.Linq;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Delete
        private static void DeleteZone(ref Descriptor desc)
        {
            desc.Send($"{Constants.BrightRedText}Deleting a Zone will also delete all Rooms in that Zone. Please take care when using this function{Constants.PlainText}{Constants.NewLine}");
            desc.Send("Enter ID of Zone to delete (0 to return): ");
            var zoneID = desc.Read().Trim();
            if(Helpers.ValidateInput(zoneID) && uint.TryParse(zoneID, out uint id))
            {
                if(id > 0)
                {
                    var z = ZoneManager.Instance.GetZone(id);
                    if (z != null)
                    {
                        desc.Send($"Do you really want to delete '{z.ZoneName}'? (Y/N): ");
                        var confirmation = desc.Read().Trim();
                        if (Helpers.ValidateInput(confirmation) && confirmation.ToUpper() == "Y")
                        {
                            bool allRoomsRemoved = true;
                            var roomsInZone = RoomManager.Instance.GetRoomIDSForZone(z.ZoneID);
                            if (roomsInZone != null && roomsInZone.Count > 0)
                            {
                                foreach (var r in roomsInZone)
                                {
                                    var playersInRoom = RoomManager.Instance.GetPlayersInRoom(r);
                                    bool OK = true;
                                    if (playersInRoom != null && playersInRoom.Count > 0)
                                    {
                                        while(RoomManager.Instance.GetPlayersInRoom(r).Count > 0)
                                        {
                                            var p = RoomManager.Instance.GetPlayersInRoom(r).FirstOrDefault();
                                            if(p != null)
                                            {
                                                if(!p.Player.Move(r, Constants.LimboRID(), true))
                                                {
                                                    OK = false;
                                                }
                                            }
                                        }
                                    }
                                    if (OK)
                                    {
                                        if (RoomManager.Instance.RemoveRoom(r, ref desc) && DatabaseManager.DeleteRoomByID(ref desc, r))
                                        {
                                            desc.Send($"Room ID {r} deleted from Room Manager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} deleted Room {r} from Room Manager and World Database", LogLevel.Warning, true);
                                        }
                                        else
                                        {
                                            desc.Send($"Could not remove Room {r} from Room Manager or World database, cannot continue with Zone removal{Constants.NewLine}");
                                            allRoomsRemoved = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (allRoomsRemoved)
                            {
                                if (ZoneManager.Instance.RemoveZone(id, z.ZoneName) && DatabaseManager.DeleteZoneByID(ref desc, id))
                                {
                                    desc.Send($"Zone {id} and all associated Rooms removed successfully.{Constants.NewLine}");
                                    Game.LogMessage($"Player {desc.Player.Name} removed Zone {id} and all associated Rooms from the world", LogLevel.Warning, true);
                                }
                                else
                                {
                                    desc.Send($"Could not remove Zone {id} from Zone Manager or World database{Constants.NewLine}");
                                    Game.LogMessage($"Player {desc.Player.Name} failed to remove Zone {id} and associated Rooms, please check logs for related messages", LogLevel.Warning, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"Zone 0 is integral to the World and cannot be deleted.{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Edit
        private static void EditExistingZone(ref Descriptor desc)
        {
            bool okToReturn = false;
            desc.Send("Enter ID of Zone to edit: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
            {
                var z = ZoneManager.Instance.GetZone(result);
                if(z != null)
                {
                    StringBuilder sb = new StringBuilder();
                    while (!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine();
                        sb.AppendLine($"Zone ID: {z.ZoneID}");
                        sb.AppendLine($"Zone Name: {z.ZoneName}");
                        sb.AppendLine($"Start RID: {z.MinRoom}");
                        sb.AppendLine($"End RID: {z.MaxRoom}");
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine("1. Set Zone Name");
                        sb.AppendLine("2. Set Start RID");
                        sb.AppendLine("3. Set End RID");
                        sb.AppendLine("4. Save Zone");
                        sb.AppendLine("5. Exit without saving");
                        sb.Append("Selection: ");
                        desc.Send(sb.ToString());
                        var opt = desc.Read().Trim();
                        if (Helpers.ValidateInput(opt) && uint.TryParse(opt, out uint option))
                        {
                            if(option >= 1 && option <= 5)
                            {
                                switch(option)
                                {
                                    case 1:
                                        z.ZoneName = GetAssetStringValue(ref desc, "Enter Zone Name: ");
                                        break;

                                    case 2:
                                        z.MinRoom = GetAssetUintValue(ref desc, "Enter Start RID: ");
                                        break;

                                    case 3:
                                        z.MaxRoom = GetAssetUintValue(ref desc, "Enter End RID: ");
                                        break;

                                    case 4:
                                        if(ValidateZone(ref desc, ref z, false))
                                        {
                                            if(DatabaseManager.UpdateZoneByID(ref desc, ref z))
                                            {
                                                if(ZoneManager.Instance.UpdateZone(ref desc, z))
                                                {
                                                    desc.Send($"Zone updated successfully{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update Zone in the Zone Manager{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update Zone in the World database{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 5:
                                        okToReturn = true;
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
                else
                {
                    desc.Send($"No Zone with that ID in Zone Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Create
        private static void CreateNewZone(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("A Zone is an area of the game comprising of a number of rooms. NPCs can move around within a Zone but cannot leave it. This allows the world to be segmented into specific areas by theme, geography or type.");
            sb.AppendLine("A Zone has an ID number, name and values for the Room IDs at the start and end of the Zone. A Zone cannot contain rooms that are outside this range of numbers.");
            Zone newZone = new Zone();
            desc.Send(sb.ToString());
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Zone ID: {newZone.ZoneID}");
                sb.AppendLine($"Zone Name: {newZone.ZoneName}");
                sb.AppendLine($"Start RID: {newZone.MinRoom}");
                sb.AppendLine($"End RID: {newZone.MaxRoom}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Zone ID");
                sb.AppendLine("2. Set Zone Name");
                sb.AppendLine("3. Set Start Room ID");
                sb.AppendLine("4. Set End Room ID");
                sb.AppendLine("5. Save New Zone");
                sb.AppendLine("6. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 6)
                    {
                        switch(result)
                        {
                            case 1:
                                newZone.ZoneID = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                break;

                            case 2:
                                newZone.ZoneName = GetAssetStringValue(ref desc, "Enter Zone Name: ");
                                break;

                            case 3:
                                newZone.MinRoom = GetAssetUintValue(ref desc, "Enter Start RID: ");
                                break;

                            case 4:
                                newZone.MaxRoom = GetAssetUintValue(ref desc, "Enter End RID: ");
                                break;

                            case 5:
                                if(ValidateZone(ref desc, ref newZone, true))
                                {
                                    if(DatabaseManager.AddNewZone(newZone))
                                    {
                                        if(ZoneManager.Instance.AddNewZone(newZone))
                                        {
                                            desc.Send($"New Zone successfully added to the Zone Manager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully added new Zone: {newZone.ZoneName} ({newZone.ZoneID})", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new Zone to the Zone Manager, it may not be available until the game is restarted.{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} failed to add new Zone {newZone.ZoneName} ({newZone.ZoneID}) to the Zone Manager, it may not be available until the game restarts", LogLevel.Warning, true);
                                        }    
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store Zone in World database{Constants.NewLine}");
                                        Game.LogMessage($"Player {desc.Player.Name} failed to add new zone {newZone.ZoneName} ({newZone.ZoneID}) in the World database", LogLevel.Error, true);
                                    }
                                }
                                break;

                            case 6:
                                okToReturn = true;
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
        #endregion

        #region Functions
        private static bool ValidateZone(ref Descriptor desc, ref Zone z, bool isNewZone)
        {
            if(string.IsNullOrEmpty(z.ZoneName))
            {
                desc.Send($"Zone does not have a name{Constants.NewLine}");
                return false;
            }
            if(isNewZone && ZoneManager.Instance.ZoneExists(z.ZoneID))
            {
                desc.Send($"The specified Zone ID is already in use{Constants.NewLine}");
                return false;
            }
            if(z.MinRoom >= z.MaxRoom)
            {
                desc.Send($"Zone start RID must be less than Zone end RID{Constants.NewLine}");
                return false;
            }
            if(isNewZone && z.MinRoom < ZoneManager.Instance.GetMaxAllocatedRID())
            {
                desc.Send($"Zone start RID overlaps with an existing Zone{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}
