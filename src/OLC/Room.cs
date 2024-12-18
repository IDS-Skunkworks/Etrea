using Etrea3.Core;
using System;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateRoom(Session session)
        {
            Room newRoom = new Room();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Room ID: {newRoom.ID}{Constants.TabStop}Zone: {newRoom.ZoneID}{Constants.TabStop}Name: {newRoom.RoomName}");
                sb.AppendLine($"Short Desc: {newRoom.ShortDescription}");
                sb.AppendLine($"Long Desc: {newRoom.LongDescription}");
                sb.AppendLine($"Sign Text: {!string.IsNullOrEmpty(newRoom.SignText)}");
                sb.AppendLine($"Flags: {newRoom.Flags}");
                sb.AppendLine($"Exits: {newRoom.RoomExits.Count}");
                sb.AppendLine($"Starting NPCs: {newRoom.StartingNPCs.Count}{Constants.TabStop}Starting Items: {newRoom.StartingItems.Count}");
                sb.AppendLine($"Tick NPCs: {newRoom.SpawnNPCsOnTick.Count}{Constants.TabStop}Tick Items: {newRoom.SpawnItemsOnTick.Count}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Zone{Constants.TabStop}3. Set Name");
                sb.AppendLine($"4. Set Short Description{Constants.TabStop}5. Set Long Description");
                sb.AppendLine($"6. Set Flags{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}7. Manage Exits");
                sb.AppendLine($"8. Manage Starting NPCs{Constants.TabStop}{Constants.TabStop}9. Manage Starting Items");
                sb.AppendLine($"10. Manage Tick NPCs{Constants.TabStop}{Constants.TabStop}11. Manage Tick Items");
                sb.AppendLine($"12. Set Sign Text{Constants.TabStop}{Constants.TabStop}13. Clear Sign Text");
                sb.AppendLine($"14. Save Room{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}15. Return");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newRoom.ID = GetValue<int>(session, "Enter Room ID: ");
                        break;

                    case 2:
                        newRoom.ZoneID = GetValue<int>(session, "Enter Zone ID: ");
                        break;

                    case 3:
                        newRoom.RoomName = GetValue<string>(session, "Enter Room Name: ");
                        break;

                    case 4:
                        newRoom.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 5:
                        newRoom.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 6:
                        newRoom.Flags = GetEnumValue<RoomFlags>(session, "Enter Room Flags: ");
                        break;

                    case 7:
                        ManageExits(session, ref newRoom);
                        break;

                    case 8:
                        ManageStartingNPCs(session, ref newRoom);
                        break;

                    case 9:
                        ManageStartingItems(session, ref newRoom);
                        break;

                    case 10:
                        ManageTickNPCs(session, ref newRoom);
                        break;

                    case 11:
                        ManageTickItems(session, ref newRoom);
                        break;

                    case 12:
                        newRoom.SignText = Helpers.GetLongDescription(session);
                        break;

                    case 13:
                        newRoom.SignText = string.Empty;
                        break;

                    case 14:
                        if (ValidateAsset(session, newRoom, true, out _))
                        {
                            if (RoomManager.Instance.AddOrUpdateRoom(newRoom, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added a new Room: {newRoom.RoomName} ({newRoom.ID})", LogLevel.OLC, true);
                                session.Send($"%BGT%New Room has been successfully created.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add a new Room: {newRoom.RoomName} ({newRoom.ID}) however the attempt failed", LogLevel.OLC, true);
                                session.Send($"%BRT%Failed to save the new Room.%PT%{Constants.NewLine}");
                                continue;
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The new Room could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 15:
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteRoom(Session session)
        {
            while (true)
            {
                session.Send($"%BRT%This is a permanent change to the Realms!%PT%{Constants.NewLine}");
                session.Send($"Enter Room ID or END to return: ");
                string input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        return;
                    }
                    if (!int.TryParse(input.Trim(), out int rid))
                    {
                        session.Send($"Sorry, that isn't a valid Room ID!{Constants.NewLine}");
                        continue;
                    }
                    if (rid <= 0)
                    {
                        session.Send($"%BRT%Sorry, you can't delete that Room!%PT%{Constants.NewLine}");
                        continue;
                    }
                    var r = RoomManager.Instance.GetRoom(rid);
                    if (r == null)
                    {
                        session.Send($"%BRT%No Room with that ID could be found!%PT%{Constants.NewLine}");
                        continue;
                    }
                    if (r.OLCLocked)
                    {
                        var lockHolder = SessionManager.Instance.GetSession(r.LockHolder);
                        var msg = lockHolder != null ? $"%BRT%The specified Room is currently locked in OLC by {lockHolder.Player.Name}.%PT%{Constants.NewLine}" :
                            $"%BRT%The specified Room is currently locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                        session.Send(msg);
                        continue;
                    }
                    if (r.PlayersInRoom.Count > 0)
                    {
                        session.Send($"%BRT%There are Players in that Room and it cannot be deleted!%PT%{Constants.NewLine}");
                        continue;
                    }
                    if (r.NPCsInRoom.Count > 0)
                    {
                        bool npcDeleteErr = false;
                        while (r.NPCsInRoom.Count > 0)
                        {
                            if (!NPCManager.Instance.RemoveNPCInstance(r.NPCsInRoom[0].ID))
                            {
                                Game.LogMessage($"ERROR: Error removing NPC {r.NPCsInRoom[0].ID} from Room {r.ID}, aborting deletion of Room", LogLevel.Error, true);
                                session.Send($"%BRT%Failed to delete an NPC from the Room, aborting deletion of Room%PT%{Constants.NewLine}");
                                npcDeleteErr = true;
                                break;
                            }
                        }
                        if (npcDeleteErr)
                        {
                            continue;
                        }
                    }
                    if (r.ItemsInRoom.Count > 0)
                    {
                        r.ItemsInRoom.Clear();
                    }
                    if (RoomManager.Instance.RemoveRoom(r.ID))
                    {
                        Game.LogMessage($"OLC: Player {session.Player.Name} has removed Room {r.ID} ({r.RoomName})", LogLevel.OLC, true);
                        session.Send($"%BGT%The specified Room has been deleted.%PT%{Constants.NewLine}");
                        return;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Room {r.ID} ({r.RoomName}) but the attempt failed", LogLevel.OLC, true);
                        session.Send($"%BRT%Failed to remove the specified Room.%PT%{Constants.NewLine}");
                        continue;
                    }
                }
            }
        }

        private static void ChangeRoom(Session session)
        {
            session.Send("Enter Room ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int rid))
            {
                session.Send($"%BRT%That is not a valid Room ID.%PT%{Constants.NewLine}");
                return;
            }
            var r = RoomManager.Instance.GetRoom(rid);
            if (r == null)
            {
                session.Send($"%BRT%No Room with that ID could be found in Room Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (r.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(r.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Room is Locked by {lockingSession.Player.Name} and cannot be changed.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Room is Locked but the lock holder could not be found.%PT%{Constants.NewLine}";
                session.Send(msg);
                return;
            }
            RoomManager.Instance.SetRoomLockState(rid, true, session);
            var room = Helpers.Clone(RoomManager.Instance.GetRoom(rid));
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Room ID: {room.ID}{Constants.TabStop}Zone: {room.ZoneID}{Constants.TabStop}Name: {room.RoomName}");
                sb.AppendLine($"Short Desc: {room.ShortDescription}");
                sb.AppendLine($"Long Desc: {room.LongDescription}");
                sb.AppendLine($"Sign Text: {!string.IsNullOrEmpty(room.SignText)}");
                sb.AppendLine($"Flags: {room.Flags}");
                sb.AppendLine($"Exits: {room.RoomExits.Count}");
                sb.AppendLine($"Starting NPCs: {room.StartingNPCs.Count}{Constants.TabStop}Starting Items: {room.StartingItems.Count}");
                sb.AppendLine($"Tick NPCs: {room.SpawnNPCsOnTick.Count}{Constants.TabStop}Tick Items: {room.SpawnItemsOnTick.Count}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Zone{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Set Flags{Constants.TabStop}6. Manage Exits");
                sb.AppendLine($"7. Manage Starting NPCs{Constants.TabStop}{Constants.TabStop}8. Manage Starting Items");
                sb.AppendLine($"9. Manage Tick NPCs{Constants.TabStop}{Constants.TabStop}10. Manage Tick Items");
                sb.AppendLine($"11. Set Sign Text{Constants.TabStop}{Constants.TabStop}12. Clear Sign Text");
                sb.AppendLine($"13. Save Room{Constants.TabStop}14. Return");
                session.Send(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        room.ZoneID = GetValue<int>(session, "Enter Zone ID: ");
                        break;

                    case 2:
                        room.RoomName = GetValue<string>(session, "Enter Room Name: ");
                        break;

                    case 3:
                        room.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        room.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        room.Flags = GetEnumValue<RoomFlags>(session, "Enter Room Flags: ");
                        break;

                    case 6:
                        ManageExits(session, ref room);
                        break;

                    case 7:
                        ManageStartingNPCs(session, ref room);
                        break;

                    case 8:
                        ManageStartingItems(session, ref room);
                        break;

                    case 9:
                        ManageTickNPCs(session, ref room);
                        break;

                    case 10:
                        ManageTickItems(session, ref room);
                        break;

                    case 11:
                        room.SignText = Helpers.GetLongDescription(session);
                        break;

                    case 12:
                        room.SignText = string.Empty;
                        break;

                    case 13:
                        if (ValidateAsset(session, room, false, out _))
                        {
                            if (RoomManager.Instance.AddOrUpdateRoom(room, false))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Room: {room.RoomName} ({room.ID})", LogLevel.OLC, true);
                                session.Send($"%BGT%Room has been successfully updated.%PT%{Constants.NewLine}");
                                RoomManager.Instance.SetRoomLockState(rid, false, session);
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Room: {room.RoomName} ({room.ID}) however the attempt failed", LogLevel.OLC, true);
                                session.Send($"%BRT%Failed to save the updated Room.%PT%{Constants.NewLine}");
                                continue;
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The updated Room could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 14:
                        RoomManager.Instance.SetRoomLockState(rid, false, session);
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageStartingNPCs(Session session, ref Room room)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (room.StartingNPCs.Count > 0)
                {
                    sb.AppendLine("Current NPCs:");
                    foreach(var n in room.StartingNPCs)
                    {
                        var npc = NPCManager.Instance.GetNPC(n.Key);
                        if (npc != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{n.Value} x {npc.Name} ({npc.TemplateID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{n.Value} x {n.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Current NPCs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add NPC{Constants.TabStop}{Constants.TabStop}2. Remove NPC");
                sb.AppendLine($"3. Clear NPCs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        int npcID = GetValue<int>(session, "Enter NPC ID: ");
                        if (npcID > 0)
                        {
                            var npc = NPCManager.Instance.GetNPC(npcID);
                            if (npc != null && npc.ZoneID == room.ZoneID)
                            {
                                room.StartingNPCs.AddOrUpdate(npc.TemplateID, 1, (k, v) => v + 1);
                            }
                            else
                            {
                                session.Send($"%BRT%Either the NPC does not exist or is not valid for the Zone this Room is in.%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 2:
                        npcID = GetValue<int>(session, "Enter NPC ID: ");
                        if (room.StartingNPCs.ContainsKey(npcID))
                        {
                            var n = room.StartingNPCs[npcID];
                            if (n - 1 == 0)
                            {
                                room.StartingNPCs.TryRemove(npcID, out _);
                            }
                            else
                            {
                                room.StartingNPCs[npcID] = n--;
                            }
                        }
                        break;

                    case 3:
                        room.StartingNPCs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageStartingItems(Session session, ref Room room)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (room.StartingItems.Count > 0)
                {
                    sb.AppendLine("Starting Items:");
                    foreach(var i in room.StartingItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {i.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Starting Items: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (itemID > 0)
                        {
                            InventoryItem item = ItemManager.Instance.GetItem(itemID);
                            if (item != null)
                            {
                                room.StartingItems.AddOrUpdate(item.ID, 1, (k, v) => v + 1);
                            }
                            else
                            {
                                session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (room.StartingItems.ContainsKey(itemID))
                        {
                            var n = room.StartingItems[itemID];
                            if (n - 1 == 0)
                            {
                                room.StartingItems.TryRemove(itemID, out _);
                            }
                            else
                            {
                                room.StartingItems[itemID] = n--;
                            }
                        }
                        break;

                    case 3:
                        room.StartingItems.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageTickNPCs(Session session, ref Room room)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (room.SpawnNPCsOnTick.Count > 0)
                {
                    foreach(var n in room.SpawnNPCsOnTick)
                    {
                        var npc = NPCManager.Instance.GetNPC(n.Key);
                        if (npc != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{n.Value} x {npc.Name} ({npc.TemplateID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{n.Value} x {n.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Tick NPCs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add NPC{Constants.TabStop}{Constants.TabStop}2. Remove NPC");
                sb.AppendLine($"3. Clear NPCs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        int npcID = GetValue<int>(session, "Enter NPC ID: ");
                        if (npcID > 0)
                        {
                            var npc = NPCManager.Instance.GetNPC(npcID);
                            if (npc != null && npc.ZoneID == room.ZoneID)
                            {
                                room.SpawnNPCsOnTick.AddOrUpdate(npc.TemplateID, 1, (k, v) => v + 1);
                            }
                            else
                            {
                                session.Send($"%BRT%Either the NPC does not exist or is not valid for the Zone this Room is in.%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 2:
                        npcID = GetValue<int>(session, "Enter NPC ID: ");
                        if (room.SpawnNPCsOnTick.ContainsKey(npcID))
                        {
                            var n = room.SpawnNPCsOnTick[npcID];
                            if (n - 1 == 0)
                            {
                                room.SpawnNPCsOnTick.TryRemove(npcID, out _);
                            }
                            else
                            {
                                room.SpawnNPCsOnTick[npcID] = n--;
                            }
                        }
                        break;

                    case 3:
                        room.SpawnNPCsOnTick.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageTickItems(Session session, ref Room room)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (room.SpawnItemsOnTick.Count > 0)
                {
                    sb.AppendLine("Tick Items:");
                    foreach(var i in room.SpawnItemsOnTick)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {i.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Tick Items: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.TabStop}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        int itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (itemID > 0)
                        {
                            InventoryItem item = ItemManager.Instance.GetItem(itemID);
                            if (item != null)
                            {
                                room.SpawnItemsOnTick.AddOrUpdate(item.ID, 1, (k, v) => v + 1);
                            }
                            else
                            {
                                session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (room.SpawnNPCsOnTick.ContainsKey(itemID))
                        {
                            var n = room.SpawnNPCsOnTick[itemID];
                            if (n - 1 == 0)
                            {
                                room.SpawnNPCsOnTick.TryRemove(itemID, out _);
                            }
                            else
                            {
                                room.SpawnNPCsOnTick[itemID] = n--;
                            }
                        }
                        break;

                    case 3:
                        room.SpawnItemsOnTick.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.TabStop}");
                        continue;
                }
            }
        }

        private static void ManageExits(Session session, ref Room room)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (room.RoomExits.Count > 0)
                {
                    sb.AppendLine("Current Exits:");
                    foreach(var item in room.RoomExits)
                    {
                        sb.AppendLine($"{Constants.TabStop}{item.Value}");
                    }
                }
                else
                {
                    sb.AppendLine("Current Exits: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Exit{Constants.TabStop}{Constants.TabStop}2. Remove Exit");
                sb.AppendLine("3. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        string direction = GetValue<string>(session, "Enter Direction: ");
                        int destRID = GetValue<int>(session, "Enter Destination Room ID: ");
                        string skill = GetValue<string>(session, "Enter Required Skill: ");
                        if (!string.IsNullOrEmpty(direction) && Enum.TryParse<ExitDirection>(direction.Trim(), true, out _) && destRID >= 0)
                        {
                            RoomExit roomExit = new RoomExit
                            {
                                ExitDirection = direction.Trim().ToLower(),
                                DestinationRoomID = destRID
                            };
                            if (string.IsNullOrEmpty(skill))
                            {
                                roomExit.RequiredSkill = string.Empty;
                            }
                            else
                            {
                                var requiredSkill = SkillManager.Instance.GetSkill(skill.Trim());
                                roomExit.RequiredSkill = requiredSkill != null ? requiredSkill.Name : string.Empty;
                            }
                            if (room.RoomExits.TryAdd(roomExit.ExitDirection.ToLower(), roomExit))
                            {
                                session.Send($"%BGT%Room Exit added successfully.%PT%{Constants.NewLine}");
                            }
                            else
                            {
                                session.Send($"%BRT%Failed to add new Room Exit.%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 2:
                        string exitDir = GetValue<string>(session, "Enter Direction to remove: ");
                        if (!string.IsNullOrEmpty(exitDir) && room.RoomExits.ContainsKey(exitDir.Trim().ToLower()))
                        {
                            room.RoomExits.TryRemove(exitDir.Trim().ToLower(), out _);
                        }
                        break;

                    case 3:
                        return;

                    default:
                        session.Send($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}