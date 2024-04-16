using Etrea2.Core;
using Etrea2.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create Room
        private static void CreateNewRoom(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("A Room is an area of the World where items, players and NPCs interact. Rooms are grouped together to form Zones. All properties of a Room, except for its RID can be changed later in other areas of OLC.");
            desc.Send(sb.ToString());
            Room newRoom = new Room
            {
                SpawnNPCsAtStart = new Dictionary<uint, uint>(),
                RoomExits = new List<Exit>(),
                ShopID = null
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Room ID: {newRoom.RoomID}{Constants.TabStop}Zone ID: {newRoom.ZoneID}");
                sb.AppendLine($"Room Name: {newRoom.RoomName}");
                sb.AppendLine($"Short Description: {newRoom.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{newRoom.LongDescription}");
                if (newRoom.RoomExits != null && newRoom.RoomExits.Count > 0)
                {
                    sb.AppendLine("Room Exits:");
                    for (int i = 0; i < newRoom.RoomExits.Count; i++)
                    {
                        Exit x = newRoom.RoomExits[i];
                        sb.AppendLine($"Direction: {x.ExitDirection} to Room {x.DestinationRoomID}");
                    }
                }
                else
                {
                    sb.AppendLine("Room Exits: None");
                }
                sb.AppendLine($"Room Flags: {newRoom.Flags}");
                sb.AppendLine($"Shop ID: {newRoom.ShopID}");
                if (newRoom.SpawnNPCsAtStart != null && newRoom.SpawnNPCsAtStart.Count > 0)
                {
                    sb.AppendLine("Loaded NPCs:");
                    foreach (var n in newRoom.SpawnNPCsAtStart)
                    {
                        var npc = NPCManager.Instance.GetNPCByID(n.Key);
                        if (npc != null)
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
                if (newRoom.SpawnItemsAtTick != null && newRoom.SpawnItemsAtTick.Count > 0)
                {
                    sb.AppendLine("Spawn Item:");
                    foreach (var item in newRoom.SpawnItemsAtTick)
                    {
                        var i = ItemManager.Instance.GetItemByID(item.Key);
                        if (i != null)
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
                if (newRoom.SpawnNPCsAtTick != null && newRoom.SpawnNPCsAtTick.Count > 0)
                {
                    sb.AppendLine("Spawn NPCS:");
                    foreach (var npc in newRoom.SpawnNPCsAtTick)
                    {
                        var n = NPCManager.Instance.GetNPCByID(npc.Key);
                        if (n != null)
                        {
                            sb.AppendLine($"{npc.Value} x {n.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{npc.Value} x {npc.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Spawn NPCs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Room ID{Constants.TabStop}{Constants.TabStop}2. Set Zone ID");
                sb.AppendLine($"3. Set Room Name{Constants.TabStop}4. Set Short Description");
                sb.AppendLine($"5. Set Long Description{Constants.TabStop}6. Edit Long Description");
                sb.AppendLine($"7. Add Exit{Constants.TabStop}{Constants.TabStop}8. Remove Exit");
                sb.AppendLine($"9. Add Spawn Item{Constants.TabStop}10. Remove Spawn Item");
                sb.AppendLine($"11. Add Spawn NPC{Constants.TabStop}12. Remove Spawn NPC");
                sb.AppendLine($"13. Set Shop ID{Constants.TabStop}{Constants.TabStop}14. Add Flag{Constants.TabStop}{Constants.TabStop}15. Remove Flag");
                sb.AppendLine($"16. Add NPC{Constants.TabStop}{Constants.TabStop}17. Remove NPC");
                sb.AppendLine($"18. Save{Constants.TabStop}{Constants.TabStop}19. Exit");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 19)
                    {
                        switch (result)
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
                                newRoom.LongDescription = Helpers.EditLongDescription(ref desc, newRoom.LongDescription);
                                break;

                            case 7:
                                var nx = CreateRoomExit(ref desc, ref newRoom);
                                if (nx != null)
                                {
                                    newRoom.RoomExits.Add(nx);
                                }
                                break;

                            case 8:
                                if (newRoom.RoomExits != null && newRoom.RoomExits.Count > 0)
                                {
                                    var x = GetRoomExit(ref desc, newRoom.RoomExits);
                                    if (x != null)
                                    {
                                        newRoom.RoomExits.Remove(x);
                                    }
                                }
                                break;

                            case 9:
                                var i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if (newRoom.SpawnItemsAtTick.ContainsKey(i))
                                {
                                    newRoom.SpawnItemsAtTick[i]++;
                                }
                                else
                                {
                                    newRoom.SpawnItemsAtTick.Add(i, 1);
                                }
                                break;

                            case 10:
                                i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if (newRoom.SpawnItemsAtTick.ContainsKey(i))
                                {
                                    if (newRoom.SpawnItemsAtTick[i] == 1)
                                    {
                                        newRoom.SpawnItemsAtTick.Remove(i);
                                    }
                                    else
                                    {
                                        newRoom.SpawnItemsAtTick[i]--;
                                    }
                                }
                                break;

                            case 11:
                                i = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                if (newRoom.SpawnNPCsAtTick.ContainsKey(i))
                                {
                                    newRoom.SpawnNPCsAtTick[i]++;
                                }
                                else
                                {
                                    newRoom.SpawnNPCsAtTick.Add(i, 1);
                                }
                                break;

                            case 12:
                                i = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                if (newRoom.SpawnNPCsAtTick.ContainsKey(i))
                                {
                                    if (newRoom.SpawnNPCsAtTick[i] == 1)
                                    {
                                        newRoom.SpawnNPCsAtTick.Remove(i);
                                    }
                                    else
                                    {
                                        newRoom.SpawnNPCsAtTick[i]--;
                                    }
                                }
                                break;

                            case 13:
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

                            case 14:
                                var nf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                if (nf != RoomFlags.None && !newRoom.Flags.HasFlag(nf))
                                {
                                    newRoom.Flags |= nf;
                                }
                                else
                                {
                                    newRoom.Flags = RoomFlags.None;
                                }
                                break;

                            case 15:
                                var rf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                if (rf != RoomFlags.None && newRoom.Flags.HasFlag(rf))
                                {
                                    newRoom.Flags &= ~rf;
                                }
                                break;

                            case 16:
                                var nnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                if (newRoom.SpawnNPCsAtStart.ContainsKey(nnpc))
                                {
                                    newRoom.SpawnNPCsAtStart[nnpc]++;
                                }
                                else
                                {
                                    newRoom.SpawnNPCsAtStart.Add(nnpc, 1);
                                }
                                break;

                            case 17:
                                var rnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                if (newRoom.SpawnNPCsAtStart.ContainsKey(rnpc))
                                {
                                    if (newRoom.SpawnNPCsAtStart[rnpc] == 1)
                                    {
                                        newRoom.SpawnNPCsAtStart.Remove(rnpc);
                                    }
                                    else
                                    {
                                        newRoom.SpawnNPCsAtStart[rnpc]--;
                                    }
                                }
                                break;

                            case 18:
                                if (ValidateRoomAsset(ref desc, ref newRoom, true))
                                {
                                    if (DatabaseManager.AddNewRoom(ref desc, newRoom))
                                    {
                                        if (RoomManager.Instance.AddNewRoom(ref desc, newRoom))
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

                            case 19:
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

        #region Edit Room
        private static void EditExistingRoom(ref Descriptor desc)
        {
            desc.Send("Enter RID of Room to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint rid))
            {
                if (RoomManager.Instance.RoomExists(rid))
                {
                    var r = RoomManager.Instance.GetRoom(rid).ShallowCopy();
                    if (r != null)
                    {
                        bool okToReturn = false;
                        StringBuilder sb = new StringBuilder();
                        while (!okToReturn)
                        {
                            sb.Clear();
                            sb.AppendLine($"Room ID: {r.RoomID}{Constants.TabStop}{Constants.TabStop}Zone ID: {r.ZoneID}");
                            sb.AppendLine($"Room Name: {r.RoomName}");
                            sb.AppendLine($"Short Description: {r.ShortDescription}");
                            sb.AppendLine($"Long Description:{Constants.NewLine}{r.LongDescription}");
                            if (r.RoomExits != null && r.RoomExits.Count > 0)
                            {
                                sb.AppendLine("Room Exits:");
                                for (int i = 0; i < r.RoomExits.Count; i++)
                                {
                                    Exit re = r.RoomExits[i];
                                    sb.AppendLine($"Direction: {re.ExitDirection} to Room {re.DestinationRoomID}");
                                }
                            }
                            else
                            {
                                sb.AppendLine("Room Exits: None");
                            }
                            sb.AppendLine($"Room Flags: {r.Flags}");
                            sb.AppendLine($"Shop ID: {r.ShopID}");
                            if (r.SpawnNPCsAtStart != null && r.SpawnNPCsAtStart.Count > 0)
                            {
                                sb.AppendLine("Loaded NPCs:");
                                foreach (var n in r.SpawnNPCsAtStart)
                                {
                                    var npc = NPCManager.Instance.GetNPCByID(n.Key);
                                    if (npc != null)
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
                            if (r.SpawnItemsAtTick != null && r.SpawnItemsAtTick.Count > 0)
                            {
                                sb.AppendLine("Spawn Items:");
                                foreach (var item in r.SpawnItemsAtTick)
                                {
                                    var i = ItemManager.Instance.GetItemByID(item.Key);
                                    if (i != null)
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
                            sb.AppendLine();
                            if (r.SpawnNPCsAtTick != null && r.SpawnNPCsAtTick.Count > 0)
                            {
                                sb.AppendLine("Spawn NPCS:");
                                foreach (var npc in r.SpawnNPCsAtTick)
                                {
                                    var n = NPCManager.Instance.GetNPCByID(npc.Key);
                                    if (n != null)
                                    {
                                        sb.AppendLine($"{npc.Value} x {n.Name}");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"{npc.Value} x {npc.Key}");
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendLine("Spawn NPC: None");
                            }
                            sb.AppendLine("Options:");
                            sb.AppendLine($"1. Set Zone ID{Constants.TabStop}2. Set Room Name");
                            sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description{Constants.TabStop}5. Edit Long Description");
                            sb.AppendLine($"6. Add Room Exit{Constants.TabStop}7. Remove Room Exit");
                            sb.AppendLine($"8. Add Spawn Item{Constants.TabStop}9. Remove Spawn Item");
                            sb.AppendLine($"10. Add Spawn NPC{Constants.TabStop}11. Remove Spawn NPC");
                            sb.AppendLine($"12. Set Shop ID{Constants.TabStop}13. Add Flag{Constants.TabStop}14. Remove Flag");
                            sb.AppendLine($"15. Add NPC{Constants.TabStop}16. Remove NPC");
                            sb.AppendLine($"17. Save{Constants.TabStop}{Constants.TabStop}18. Exit");
                            sb.Append("Selection: ");
                            desc.Send(sb.ToString());
                            var choice = desc.Read().Trim();
                            if (Helpers.ValidateInput(choice) && uint.TryParse(choice, out uint option))
                            {
                                if (option >= 1 && option <= 18)
                                {
                                    switch (option)
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
                                            r.LongDescription = Helpers.EditLongDescription(ref desc, r.LongDescription);
                                            break;

                                        case 6:
                                            var nx = CreateRoomExit(ref desc, ref r);
                                            if (nx != null)
                                            {
                                                r.RoomExits.Add(nx);
                                            }
                                            break;

                                        case 7:
                                            if (r.RoomExits != null && r.RoomExits.Count > 0)
                                            {
                                                var x = GetRoomExit(ref desc, r.RoomExits);
                                                if (x != null)
                                                {
                                                    r.RoomExits.Remove(x);
                                                }
                                            }
                                            break;

                                        case 8:
                                            var i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                            if (r.SpawnItemsAtTick.ContainsKey(i))
                                            {
                                                r.SpawnItemsAtTick[i]++;
                                            }
                                            else
                                            {
                                                r.SpawnItemsAtTick.Add(i, 1);
                                            }
                                            break;

                                        case 9:
                                            i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                            if (r.SpawnItemsAtTick.ContainsKey(i))
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

                                        case 10:
                                            i = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                            if (r.SpawnNPCsAtTick.ContainsKey(i))
                                            {
                                                r.SpawnNPCsAtTick[i]++;
                                            }
                                            else
                                            {
                                                r.SpawnNPCsAtTick.Add(i, 1);
                                            }
                                            break;

                                        case 11:
                                            i = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                            if (r.SpawnNPCsAtTick.ContainsKey(i))
                                            {
                                                if (r.SpawnNPCsAtTick[i] == 1)
                                                {
                                                    r.SpawnNPCsAtTick.Remove(i);
                                                }
                                                else
                                                {
                                                    r.SpawnNPCsAtTick[i]--;
                                                }
                                            }
                                            break;

                                        case 12:
                                            var sid = GetAssetUintValue(ref desc, "Enter Shop ID (0 to clear): ");
                                            if (sid > 0)
                                            {
                                                r.ShopID = sid;
                                            }
                                            else
                                            {
                                                r.ShopID = null;
                                            }
                                            break;

                                        case 13:
                                            var nf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                            if (nf != RoomFlags.None && !r.Flags.HasFlag(nf))
                                            {
                                                r.Flags |= nf;
                                            }
                                            break;

                                        case 14:
                                            var rf = GetAssetEnumValue<RoomFlags>(ref desc, "Enter Room Flag: ");
                                            if (rf != RoomFlags.None && r.Flags.HasFlag(rf))
                                            {
                                                r.Flags &= ~rf;
                                            }
                                            break;

                                        case 15:
                                            var nnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                            if (r.SpawnNPCsAtStart == null)
                                            {
                                                r.SpawnNPCsAtStart = new Dictionary<uint, uint>();
                                            }
                                            if (r.SpawnNPCsAtStart.ContainsKey(nnpc))
                                            {
                                                r.SpawnNPCsAtStart[nnpc]++;
                                            }
                                            else
                                            {
                                                r.SpawnNPCsAtStart.Add(nnpc, 1);
                                            }
                                            break;

                                        case 16:
                                            var rnpc = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                            if (r.SpawnNPCsAtStart.ContainsKey(rnpc))
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

                                        case 17:
                                            if (ValidateRoomAsset(ref desc, ref r, false))
                                            {
                                                if (DatabaseManager.UpdateRoom(ref desc, ref r))
                                                {
                                                    if (RoomManager.Instance.UpdateRoom(ref desc, r))
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

                                        case 18:
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
                    desc.Send($"No Room with RID {rid} could be found in Room Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Delete Room
        private static void DeleteRoom(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a backup of the database is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send($"Any players in the deleted Room will be teleported to Limbo. Items and NPCs in the Room will be deleted from the game.{Constants.NewLine}");
            desc.Send("Enter the ID of the Room to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
            {
                var r = RoomManager.Instance.GetRoom(result);
                if (r != null || r.RoomID == 0)
                {
                    bool okToDelete = true;
                    var playersInRoom = RoomManager.Instance.GetPlayersInRoom(r.RoomID);
                    if (playersInRoom != null && playersInRoom.Count > 0)
                    {
                        while (RoomManager.Instance.GetPlayersInRoom(r.RoomID).Count > 0)
                        {
                            var p = RoomManager.Instance.GetPlayersInRoom(r.RoomID).FirstOrDefault();
                            if (p != null)
                            {
                                if (!p.Player.Move(r.RoomID, Constants.LimboRID(), true, true))
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
                                if (!NPCManager.Instance.RemoveNPCFromWorld(n.NPCGuid))
                                {
                                    okToDelete = false;
                                }
                            }
                        }
                    }
                    if (okToDelete)
                    {
                        if (DatabaseManager.DeleteRoomByID(ref desc, r.RoomID))
                        {
                            if (RoomManager.Instance.RemoveRoom(r.RoomID, r.RoomName, ref desc))
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
                    desc.Send($"The specified RID could not be found or was Room 0 which cannot be deleted.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Helper Functions
        private static Exit GetRoomExit(ref Descriptor desc, List<Exit> currentExits)
        {
            while (true)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Current Exits:");
                foreach (var x in currentExits)
                {
                    sb.AppendLine($"Direction: {x.ExitDirection} to Room: {x.DestinationRoomID}");
                }
                sb.Append("Enter direction to remove or END to return: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input))
                {
                    if (input == "END")
                    {
                        return null;
                    }
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

        private static Exit CreateRoomExit(ref Descriptor desc, ref Room room)
        {
            bool OK = false;
            Exit newExit = new Exit();
            StringBuilder sb = new StringBuilder();
            while (!OK)
            {
                sb.Clear();
                if (room.RoomExits.Count > 0)
                {
                    sb.AppendLine("Current Exits:");
                    foreach (var x in room.RoomExits)
                    {
                        sb.AppendLine($"Direction: {x.ExitDirection} to Room: {x.DestinationRoomID}");
                    }
                }
                uint newExitRID = 0;
                sb.AppendLine("Enter a direction for the new exit. Valid directions are: Up, Down,");
                sb.AppendLine("North, Northeast, East, Southeast, South, Southwest, West and Northwest.");
                sb.AppendLine("Enter END to abort.");
                sb.AppendLine("Direction: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input))
                {
                    bool directionOK = false;
                    switch (input.ToLower())
                    {
                        case "END":
                            return null;
                            
                        case "up":
                        case "u":
                            input = "up";
                            directionOK = true;
                            break;
                        case "down":
                        case "d":
                            input = "down";
                            directionOK = true;
                            break;
                        case "north":
                        case "n":
                            input = "north";
                            directionOK = true;
                            break;
                        case "east":
                        case "e":
                            input = "east";
                            directionOK = true;
                            break;
                        case "south":
                        case "s":
                            input = "south";
                            directionOK = true;
                            break;
                        case "west":
                        case "w":
                            input = "west";
                            directionOK = true;
                            break;
                        case "northeast":
                        case "ne":
                            input = "northeast";
                            directionOK = true;
                            break;
                        case "southeast":
                        case "se":
                            input = "southeast";
                            directionOK = true;
                            break;
                        case "southwest":
                        case "sw":
                            input = "southwest";
                            directionOK = true;
                            break;
                        case "northwest":
                        case "nw":
                            input = "northwest";
                            directionOK = true;
                            break;

                        default:
                            desc.Send($"That doesn't seem like a valid direction!{Constants.NewLine}");
                            break;
                    }
                    if (directionOK)
                    {
                        if (room.RoomExits.Where(x => x.ExitDirection.ToLower() == input.ToLower()).Count() == 0)
                        {
                            desc.Send("Enter the RID of the room the exit leads to (this room doesn't have to exist):");
                            var ridInput = desc.Read().Trim();
                            if (Helpers.ValidateInput(ridInput) && uint.TryParse(ridInput, out newExitRID))
                            {
                                newExit.ExitDirection = Helpers.CapitaliseFirstLetter(input);
                                newExit.DestinationRoomID = newExitRID;
                                OK = true;
                                newExit.RequiredSkill = null;
                                desc.Send("Enter required Skill (Enter for no skill): ");
                                var reqSkill = desc.Read().Trim();
                                if (!string.IsNullOrEmpty(reqSkill))
                                {
                                    var s = SkillManager.Instance.GetSkill(reqSkill);
                                    newExit.RequiredSkill = s ?? null;
                                }
                                newExit.RoomDoor = null;
                                desc.Send("Add Door to Exit (Y/N)?");
                                var addDoor = desc.Read().Trim();
                                if (!string.IsNullOrEmpty(addDoor) && addDoor.ToLower() == "y")
                                {
                                    var door = new RoomDoor();
                                    door.IsOpen = false;
                                    door.IsLocked = false;
                                    door.RequiredItemID = 0;
                                    desc.Send("Enter ID of Key (0 for no key): ");
                                    var keyID = desc.Read().Trim();
                                    if (uint.TryParse(keyID, out uint uKeyID))
                                    {
                                        door.RequiredItemID = uKeyID;
                                    }
                                    desc.Send("Is Locked (Y/N)?");
                                    var isLocked = desc.Read().Trim();
                                    if (!string.IsNullOrEmpty(isLocked) && isLocked.ToLower() == "y")
                                    {
                                        door.IsLocked = true;
                                    }
                                    newExit.RoomDoor = door;
                                }
                            }
                            else
                            {
                                desc.Send($"Input was not a valid RID!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"There is already an exit in that direction!{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
            return newExit;
        }
        #endregion

        #region Validation Functions
        private static bool ValidateRoomAsset(ref Descriptor desc, ref Room r, bool isNewRoom)
        {
            if ((isNewRoom && !RoomManager.Instance.RoomExists(r.RoomID) && r.RoomID != 0) || !isNewRoom)
            {
                if (!ZoneManager.Instance.ZoneExists(r.ZoneID))
                {
                    desc.Send($"Zone ID does not exist.{Constants.NewLine}");
                    return false;
                }
                var z = ZoneManager.Instance.GetZone(r.ZoneID);
                if (r.RoomID < z.MinRoom || r.RoomID > z.MaxRoom)
                {
                    desc.Send($"Room ID is outside the bounds for the specified Zone.{Constants.NewLine}");
                    return false;
                }
                if (r.Flags.HasFlag(RoomFlags.Shop) && (!r.ShopID.HasValue || r.ShopID.Value == 0))
                {
                    desc.Send($"Room is flagged as a Shop but no Shop ID has been provided{Constants.NewLine}");
                    return false;
                }
                if (!r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue && r.ShopID.Value > 0)
                {
                    desc.Send($"Room has a Shop ID but is not flagged as being a Shop{Constants.NewLine}");
                    return false;
                }
                if (string.IsNullOrEmpty(r.RoomName) || string.IsNullOrEmpty(r.ShortDescription) || string.IsNullOrEmpty(r.LongDescription))
                {
                    desc.Send($"Room Name, Short Description and Long Description are required{Constants.NewLine}");
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
        #endregion
    }
}