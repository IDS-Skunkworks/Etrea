using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Delete
        private static void DeleteRoom(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a backup of the database is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send($"Any players in the deleted Room will be teleported to Limbo. Items and NPCs in the Room will be deleted from the game.{Constants.NewLine}");
            desc.Send("Enter RID to delete: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
            {
                if(result > 0)
                {
                    var r = RoomManager.Instance.GetRoom(result);
                    if(r != null)
                    {
                        bool okToDelete = true;
                        var playersInRoom = RoomManager.Instance.GetPlayersInRoom(r.RoomID);
                        if(playersInRoom != null && playersInRoom.Count > 0)
                        {
                            while (RoomManager.Instance.GetPlayersInRoom(r.RoomID).Count > 0)
                            {
                                var p = RoomManager.Instance.GetPlayersInRoom(r.RoomID).FirstOrDefault();
                                if (p != null)
                                {
                                    if (!p.Player.Move(r.RoomID, Constants.LimboRID(), true, ref p))
                                    {
                                        okToDelete = false;
                                    }
                                }
                            }
                        }
                        if (RoomManager.Instance.GetRoom(result).ItemsInRoom != null && RoomManager.Instance.GetRoom(result).ItemsInRoom.Count > 0)
                        {
                            RoomManager.Instance.GetRoom(result).ItemsInRoom.Clear();
                        }
                        if (NPCManager.Instance.GetNPCsInRoom(result) != null && NPCManager.Instance.GetNPCsInRoom(result).Count > 0)
                        {
                            while (NPCManager.Instance.GetNPCsInRoom(result).Count > 0)
                            {
                                var n = NPCManager.Instance.GetNPCsInRoom(result).FirstOrDefault();
                                if (n != null)
                                {
                                    if (!NPCManager.Instance.RemoveNPCFromWorld(n.NPCGuid, n, result))
                                    {
                                        okToDelete = false;
                                    }
                                }
                            }
                        }
                        if (okToDelete)
                        {
                            if(DatabaseManager.DeleteRoomByID(ref desc, r.RoomID))
                            {
                                if(RoomManager.Instance.RemoveRoom(r.RoomID, ref desc))
                                {
                                    desc.Send($"Room successfully removed{Constants.NewLine}");
                                }
                                else
                                {
                                    desc.Send($"Unable to remove Room from Room Manager{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"Unable to remove Room from World database{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"Could not safely move all players to Limbo, Room cannot be removed{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"No Room with RID {result} could be found in Room Manager{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"RID 0 is integral to the World and cannot be deleted{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Edit
        private static void EditExistingRoom(ref Descriptor desc)
        {
            desc.Send("Enter RID of Room to edit: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint rid))
            {
                var r = RoomManager.Instance.GetRoom(rid);
                if(r != null)
                {
                    bool okToReturn = false;
                    StringBuilder sb = new StringBuilder();
                    while(!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Room ID: {r.RoomID}");
                        sb.AppendLine($"Zone ID: {r.ZoneID}");
                        sb.AppendLine($"Room Name: {r.RoomName}");
                        sb.AppendLine($"Short Description: {r.ShortDescription}");
                        sb.AppendLine($"Long Description: {r.LongDescription}");
                        if(r.RoomExits != null && r.RoomExits.Count > 0)
                        {
                            sb.AppendLine("Room Exits:");
                            for (int i = 0; i < r.RoomExits.Count; i++)
                            {
                                Room.Exit re = r.RoomExits[i];
                                sb.AppendLine($"Direction: {re.ExitDirection} to Room {re.DestinationRoomID}");
                            }
                        }
                        else
                        {
                            sb.AppendLine("Room Exits: None");
                        }
                        sb.AppendLine($"Room Flags: {r.Flags}");
                        sb.AppendLine($"Shop ID: {r.ShopID}");
                        if(r.SpawnNPCsAtStart != null && r.SpawnNPCsAtStart.Count > 0)
                        {
                            sb.AppendLine("Loaded NPCs:");
                            foreach(var n in r.SpawnNPCsAtStart)
                            {
                                var npc = NPCManager.Instance.GetNPCByID(n.Key);
                                if(npc != null)
                                {
                                    sb.AppendLine($"{n.Value} x {npc.Name}");
                                }
                                else
                                {
                                    sb.AppendLine($"{n.Value} x {n.Key}");
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine("Loaded NPCs: None");
                        }
                        sb.AppendLine();
                        if(r.SpawnItemsAtTick != null && r.SpawnNPCsAtStart.Count > 0)
                        {
                            sb.AppendLine("Spawn Items:");
                            foreach(var item in r.SpawnItemsAtTick)
                            {
                                var i = ItemManager.Instance.GetItemByID(item.Key);
                                if(i != null)
                                {
                                    sb.AppendLine($"{item.Value} x {i.Name}");
                                }
                                else
                                {
                                    sb.AppendLine($"{item.Value} x {item.Key}");
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine("Spawn Items: None");
                        }
                        sb.AppendLine("Options:");
                        sb.AppendLine($"1. Set Zone ID{Constants.TabStop}2. Set Room Name");
                        sb.AppendLine("3. Set Short Description");
                        sb.AppendLine("4. Set Long Description");
                        sb.AppendLine($"5. Add Room Exit{Constants.TabStop}6. Remove Room Exit");
                        sb.AppendLine($"7. Add Spawn Item{Constants.TabStop}8. Remove Spawn Item");
                        sb.AppendLine("9. Set Shop ID");
                        sb.AppendLine($"10. Add Room Flag{Constants.TabStop}11. Remove Room Flag");
                        sb.AppendLine($"12. Add NPC{Constants.TabStop}13. Remove NPC");
                        sb.AppendLine("14. Save Room");
                        sb.AppendLine("15. Exit without saving");
                        sb.Append("Selection: ");
                        desc.Send(sb.ToString());
                        var choice = desc.Read().Trim();
                        if(Helpers.ValidateInput(choice) && uint.TryParse(choice, out uint option))
                        {
                            if(option >= 1 && option <= 15)
                            {
                                switch(option)
                                {
                                    case 1:
                                        r.ZoneID = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                        break;

                                    case 2:
                                        r.RoomName = GetAssetStringValue(ref desc, "Enter Room Name: ");
                                        break;

                                    case 3:
                                        r.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                        break;

                                    case 4:
                                        r.LongDescription = Helpers.GetLongDescription(ref desc);
                                        break;

                                    case 5:
                                        var nx = CreateNewRoomExit(ref desc, r.RoomExits);
                                        r.RoomExits.Add(nx);
                                        break;

                                    case 6:
                                        if(r.RoomExits != null && r.RoomExits.Count > 0)
                                        {
                                            var x = GetRoomExit(ref desc, r.RoomExits);
                                            r.RoomExits.Remove(x);
                                        }
                                        break;

                                    case 7:
                                        var i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        if(r.SpawnItemsAtTick.ContainsKey(i))
                                        {
                                            r.SpawnItemsAtTick[i]++;
                                        }
                                        else
                                        {
                                            r.SpawnItemsAtTick.Add(i, 1);
                                        }
                                        break;

                                    case 8:
                                        i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        if(r.SpawnItemsAtTick.ContainsKey(i))
                                        {
                                            if (r.SpawnItemsAtTick[i] == 1)
                                            {
                                                r.SpawnItemsAtTick.Remove(i);
                                            }
                                            else
                                            {
                                                r.SpawnItemsAtTick[i]--;
                                            }
                                        }
                                        break;

                                    case 9:
                                        var sid = GetAssetUintValue(ref desc, "Enter Shop ID (0 to clear): ");
                                        if(sid > 0)
                                        {
                                            r.ShopID = sid;
                                        }
                                        else
                                        {
                                            r.ShopID = null;
                                        }
                                        break;

                                    case 10:
                                        var nf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                        if (nf != RoomFlags.None && !r.Flags.HasFlag(nf))
                                        {
                                            r.Flags |= nf;
                                        }
                                        break;

                                    case 11:
                                        var rf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                        if(rf != RoomFlags.None && r.Flags.HasFlag(rf))
                                        {
                                            r.Flags &= ~rf;
                                        }
                                        break;

                                    case 12:
                                        var nnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                        if(r.SpawnNPCsAtStart == null)
                                        {
                                            r.SpawnNPCsAtStart = new Dictionary<uint, uint>();
                                        }
                                        if(r.SpawnNPCsAtStart.ContainsKey(nnpc))
                                        {
                                            r.SpawnNPCsAtStart[nnpc]++;
                                        }
                                        else
                                        {
                                            r.SpawnNPCsAtStart.Add(nnpc, 1);
                                        }
                                        break;

                                    case 13:
                                        var rnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                        if(r.SpawnNPCsAtStart.ContainsKey(rnpc))
                                        {
                                            if (r.SpawnNPCsAtStart[rnpc] == 1)
                                            {
                                                r.SpawnNPCsAtStart.Remove(rnpc);
                                            }
                                            else
                                            {
                                                r.SpawnNPCsAtStart[rnpc]--;
                                            }
                                        }
                                        break;

                                    case 14:
                                        if(ValidateRoomAsset(ref desc, ref r, false))
                                        {
                                            if(DatabaseManager.UpdateRoom(ref desc, ref r))
                                            {
                                                if(RoomManager.Instance.UpdateRoom(ref desc, r))
                                                {
                                                    desc.Send($"Room updated successfully{Constants.NewLine}");
                                                    Game.LogMessage($"INFO: Player {desc.Player.Name} successfully updated Room: {r.RoomName} ({r.RoomID})", LogLevel.Info, true);
                                                    okToReturn = true;
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update Room in World database{Constants.NewLine}");
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
                    desc.Send($"No Room with RID {rid} could be found in Room Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Create
        private static void CreateNewRoom(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("A Room is an area of the World where items, players and NPCs interact. Rooms are grouped together to form Zones.");
            sb.AppendLine("All properties of a Room, except for its RID can be changed later in other areas of OLC.");
            desc.Send(sb.ToString());
            Room newRoom = new Room();
            newRoom.SpawnNPCsAtStart = new Dictionary<uint, uint>();
            newRoom.RoomExits = new List<Room.Exit>();
            newRoom.ShopID = null;
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Room ID: {newRoom.RoomID}");
                sb.AppendLine($"Zone ID: {newRoom.ZoneID}");
                sb.AppendLine($"Room Name: {newRoom.RoomName}");
                sb.AppendLine($"Short Description: {newRoom.ShortDescription}");
                sb.AppendLine($"Long Description: {newRoom.LongDescription}");
                if(newRoom.RoomExits != null && newRoom.RoomExits.Count > 0)
                {
                    sb.AppendLine("Room Exits:");
                    for(int i = 0; i < newRoom.RoomExits.Count; i++)
                    {
                        Room.Exit x = newRoom.RoomExits[i];
                        sb.AppendLine($"Direction: {x.ExitDirection} to Room {x.DestinationRoomID}");
                    }
                }
                else
                {
                    sb.AppendLine("Room Exits: None");
                }
                sb.AppendLine($"Room Flags: {newRoom.Flags}");
                sb.AppendLine($"Shop ID: {newRoom.ShopID}");
                if(newRoom.SpawnNPCsAtStart != null && newRoom.SpawnNPCsAtStart.Count > 0)
                {
                    sb.AppendLine("Loaded NPCs:");
                    foreach(var n in newRoom.SpawnNPCsAtStart)
                    {
                        var npc = NPCManager.Instance.GetNPCByID(n.Key);
                        if(npc != null)
                        {
                            sb.AppendLine($"{n.Value} x {npc.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{n.Value} x {n.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Loaded NPCs: None");
                }
                sb.AppendLine();
                if(newRoom.SpawnItemsAtTick != null && newRoom.SpawnNPCsAtStart.Count > 0)
                {
                    sb.AppendLine("Spawn Item:");
                    foreach(var item in newRoom.SpawnItemsAtTick)
                    {
                        var i = ItemManager.Instance.GetItemByID(item.Key);
                        if(i != null)
                        {
                            sb.AppendLine($"{item.Value} x {i.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{item.Value} x {item.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Spawn Item: None");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Room ID{Constants.TabStop}2. Set Zone ID");
                sb.AppendLine("3. Set Room Name");
                sb.AppendLine("4. Set Short Description");
                sb.AppendLine("5. Set Long Description");
                sb.AppendLine($"6. Add Room Exit{Constants.TabStop}7. Remove Room Exit");
                sb.AppendLine($"8. Add Spawn Item{Constants.TabStop}9. Remove Spawn Item");
                sb.AppendLine("10. Set Shop ID");
                sb.AppendLine($"11. Add Room Flag{Constants.TabStop}12. Remove Room Flag");
                sb.AppendLine($"13. Add NPC{Constants.TabStop}14. Remove NPC");
                sb.AppendLine("15. Save Room");
                sb.AppendLine("16. Exit without saving");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 16)
                    {
                        switch(result)
                        {
                            case 1:
                                newRoom.RoomID = GetAssetUintValue(ref desc, "Enter Room ID: ");
                                break;

                            case 2:
                                newRoom.ZoneID = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                break;

                            case 3:
                                newRoom.RoomName = GetAssetStringValue(ref desc, "Enter Room Name: ");
                                break;

                            case 4:
                                newRoom.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 5:
                                newRoom.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 6:
                                var nx = CreateNewRoomExit(ref desc, newRoom.RoomExits);
                                newRoom.RoomExits.Add(nx);
                                break;

                            case 7:
                                if(newRoom.RoomExits != null && newRoom.RoomExits.Count > 0)
                                {
                                    var x = GetRoomExit(ref desc, newRoom.RoomExits);
                                    newRoom.RoomExits.Remove(x);
                                }
                                break;

                            case 8:
                                var i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if(newRoom.SpawnItemsAtTick.ContainsKey(i))
                                {
                                    newRoom.SpawnItemsAtTick[i]++;
                                }
                                else
                                {
                                    newRoom.SpawnItemsAtTick.Add(i, 1);
                                }
                                break;

                            case 9:
                                i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if(newRoom.SpawnItemsAtTick.ContainsKey(i))
                                {
                                    if(newRoom.SpawnItemsAtTick[i] == 1)
                                    {
                                        newRoom.SpawnItemsAtTick.Remove(i);
                                    }
                                    else
                                    {
                                        newRoom.SpawnItemsAtTick[i]--;
                                    }
                                }
                                break;

                            case 10:
                                var sid = GetAssetUintValue(ref desc, "Enter Shop ID (0 to clear): ");
                                if (sid > 0)
                                {
                                    newRoom.ShopID = sid;
                                }
                                else
                                {
                                    newRoom.ShopID = null;
                                }
                                break;

                            case 11:
                                var nf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                if(nf != RoomFlags.None && !newRoom.Flags.HasFlag(nf))
                                {
                                    newRoom.Flags |= nf;
                                }
                                else
                                {
                                    newRoom.Flags = RoomFlags.None;
                                }
                                break;

                            case 12:
                                var rf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                if(rf != RoomFlags.None && newRoom.Flags.HasFlag(rf))
                                {
                                    newRoom.Flags &= ~rf;
                                }
                                break;

                            case 13:
                                var nnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                if(newRoom.SpawnNPCsAtStart.ContainsKey(nnpc))
                                {
                                    newRoom.SpawnNPCsAtStart[nnpc]++;
                                }
                                else
                                {
                                    newRoom.SpawnNPCsAtStart.Add(nnpc, 1);
                                }
                                break;

                            case 14:
                                var rnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                if(newRoom.SpawnNPCsAtStart.ContainsKey(rnpc))
                                {
                                    if(newRoom.SpawnNPCsAtStart[rnpc] == 1)
                                    {
                                        newRoom.SpawnNPCsAtStart.Remove(rnpc);
                                    }
                                    else
                                    {
                                        newRoom.SpawnNPCsAtStart[rnpc]--;
                                    }
                                }
                                break;

                            case 15:
                                if(ValidateRoomAsset(ref desc, ref newRoom, true))
                                {
                                    if(DatabaseManager.AddNewRoom(ref desc, newRoom))
                                    {
                                        if(RoomManager.Instance.AddNewRoom(ref desc, newRoom))
                                        {
                                            desc.Send($"Room saved successfully{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add Room to Room Manager, it may not be available until restart{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to save the Room to the World database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 16:
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
        private static bool ValidateRoomAsset(ref Descriptor desc, ref Room r, bool isNewRoom)
        {
            if((isNewRoom && !RoomManager.Instance.RoomExists(r.RoomID) && r.RoomID != 0) || !isNewRoom)
            {
                if(!ZoneManager.Instance.ZoneExists(r.ZoneID))
                {
                    desc.Send($"Zone ID does not exist.{Constants.NewLine}");
                    return false;
                }
                var z = ZoneManager.Instance.GetZone(r.ZoneID);
                if(r.RoomID < z.MinRoom || r.RoomID > z.MaxRoom)
                {
                    desc.Send($"Room ID is outside the bounds for the specified Zone.{Constants.NewLine}");
                    return false;
                }
                if(r.Flags.HasFlag(RoomFlags.Shop) && (!r.ShopID.HasValue || r.ShopID.Value == 0))
                {
                    desc.Send($"Room is flagged as a Shop but no Shop ID has been provided{Constants.NewLine}");
                    return false;
                }
                if(!r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue && r.ShopID.Value > 0)
                {
                    desc.Send($"Room has a Shop ID but is not flagged as being a Shop{Constants.NewLine}");
                    return false;
                }
                return true;
            }
            else
            {
                desc.Send($"Room ID in use or invalid.{Constants.NewLine}");
                return false;
            }
        }

        private static Room.Exit GetRoomExit(ref Descriptor desc, List<Room.Exit> currentExits)
        {
            while (true)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Current Exits:");
                foreach (var x in currentExits)
                {
                    sb.AppendLine($"Direction: {x.ExitDirection} to Room: {x.DestinationRoomID}");
                }
                sb.Append("Enter direction to remove: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input))
                {
                    if (currentExits.Where(x => x.ExitDirection.ToLower() == input.ToLower()).Count() > 0)
                    {
                        var selectedExit = currentExits.Where(x => x.ExitDirection.ToLower() == input.ToLower()).FirstOrDefault();
                        return selectedExit;
                    }
                    else
                    {
                        desc.Send($"There is no exit in that direction!{Constants.NewLine}");
                    }
                }
            }
        }

        private static Room.Exit CreateNewRoomExit(ref Descriptor desc, List<Room.Exit> currentExits)
        {
            bool OK = false;
            Room.Exit newRoomExit = new Room.Exit();
            while (!OK)
            {
                if (currentExits.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Current Exits:");
                    foreach (var x in currentExits)
                    {
                        sb.AppendLine($"Direction: {x.ExitDirection} to Room: {x.DestinationRoomID}");
                    }
                    desc.Send(sb.ToString());
                }
                uint newExitRID = 0;
                StringBuilder dirInfo = new StringBuilder();
                dirInfo.AppendLine("Enter a direction for the new Exit. Valid directions are:");
                dirInfo.AppendLine("Up, Down, North, Northeast, East, Southeast, South, Southwest");
                dirInfo.Append("West, Northwest: ");
                desc.Send(dirInfo.ToString());
                var dirInput = desc.Read().Trim();
                if (Helpers.ValidateInput(dirInput))
                {
                    bool directionOK = false;
                    switch(dirInput.ToLower())
                    {
                        case "up":
                        case "down":
                        case "east":
                        case "west":
                        case "north":
                        case "south":
                        case "northeast":
                        case "northwest":
                        case "southeast":
                        case "southwest":
                            directionOK = true;
                            break;

                        default:
                            desc.Send($"That is not a valid direction!{Constants.NewLine}");
                            break;
                    }
                    if(directionOK)
                    {
                        if (currentExits.Where(x => x.ExitDirection.ToLower() == dirInput.ToLower()).Count() == 0)
                        {
                            desc.Send("Enter target Room ID (this RID does not have to exist): ");
                            var ridInput = desc.Read().Trim();
                            if (Helpers.ValidateInput(ridInput) && uint.TryParse(ridInput, out newExitRID))
                            {
                                newRoomExit.ExitDirection = Helpers.CapitaliseFirstLetter(dirInput);
                                newRoomExit.DestinationRoomID = newExitRID;
                                OK = true;
                            }
                            else
                            {
                                desc.Send($"Input was not a valid RID{Constants.NewLine}");
                            }
                            newRoomExit.RequiredSkill = null;
                            desc.Send("Enter required Skill (Enter for no skill): ");
                            var reqSkill = desc.Read().Trim();
                            if(!string.IsNullOrEmpty(reqSkill))
                            {
                                var s = Skills.GetSkill(reqSkill);
                                newRoomExit.RequiredSkill = s ?? null;
                            }
                            newRoomExit.RoomDoor = null;
                            desc.Send("Add Door to Exit (Y/N)?");
                            var addDoor = desc.Read().Trim();
                            if(!string.IsNullOrEmpty(addDoor) && addDoor.ToLower() == "y")
                            {
                                var door = new Room.RoomDoor();
                                door.IsOpen = false;
                                door.IsLocked = false;
                                door.RequiredItemID = 0;
                                desc.Send("Enter ID of Key (0 for no key): ");
                                var keyID = desc.Read().Trim();
                                if(uint.TryParse(keyID, out uint uKeyID))
                                {
                                    door.RequiredItemID = uKeyID;
                                }
                                desc.Send("Is Locked (Y/N)?");
                                var isLocked = desc.Read().Trim();
                                if(!string.IsNullOrEmpty(isLocked) && isLocked.ToLower() == "y")
                                {
                                    door.IsLocked = true;
                                }
                                newRoomExit.RoomDoor = door;
                            }
                        }
                        else
                        {
                            desc.Send($"There is already an exit in that direction{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                }
            }
            return newRoomExit;
        }
        #endregion
    }
}
