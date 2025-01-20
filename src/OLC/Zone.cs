using Etrea3.Core;
using System.Linq;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateZone(Session session)
        {
            Zone newZone = new Zone();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Zone ID: {newZone.ZoneID}");
                sb.AppendLine($"Zone Name: {newZone.ZoneName}");
                sb.AppendLine($"Start Room: {newZone.MinRoom}{Constants.TabStop}{Constants.TabStop}End Room: {newZone.MaxRoom}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Zone ID{Constants.TabStop}{Constants.TabStop}2. Set Zone Name");
                sb.AppendLine($"3. Set Start Room{Constants.TabStop}4. Set End Room");
                sb.AppendLine($"5. Save Zone{Constants.TabStop}{Constants.TabStop}6. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newZone.ZoneID = GetValue<int>(session, "Enter Zone ID: ");
                        break;

                    case 2:
                        newZone.ZoneName = GetValue<string>(session, "Enter Zone Name: ");
                        break;

                    case 3:
                        newZone.MinRoom = GetValue<int>(session, "Enter Start Room ID: ");
                        break;

                    case 4:
                        newZone.MaxRoom = GetValue<int>(session, "Enter End Room ID: ");
                        break;

                    case 5:
                        if (ValidateAsset(session, newZone, true, out _))
                        {
                            if (ZoneManager.Instance.AddOrUpdateZone(newZone, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added a new Zone: {newZone.ZoneName} ({newZone.ZoneID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%New Zone has been successfully created.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add a new Zone: {newZone.ZoneName} ({newZone.ZoneID}) however the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%Failed to save the new Zone.%PT%{Constants.NewLine}");
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Zone could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 6:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteZone(Session session)
        {
            while(true)
            {
                session.SendSystem($"%BRT%This is a permanent change to the Realms. All Rooms in the Zone will also be removed.%PT%{Constants.NewLine}");
                session.SendSystem("Enter Zone ID or END to return: ");
                string input = session.Read();
                if (string.IsNullOrEmpty(input))
                {
                    session.SendSystem($"%BRT%Sorry, that is not a valid Zone ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int value))
                {
                    session.SendSystem($"%BRT%Sorry, that is not a valid Zone ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (value <= 0)
                {
                    session.SendSystem($"%BRT%Sorry, that Zone cannot be removed.%PT%{Constants.NewLine}");
                    continue;
                }
                var zone = ZoneManager.Instance.GetZone(value);
                if (zone == null)
                {
                    session.SendSystem($"%BRT%No Zone with that ID could be found in Zone Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (zone.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(zone.LockHolder);
                    var msg = lockingSession != null ? $"%BRT%The specified Zone is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                        $"%BRT%The specified Zone is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.SendSystem(msg);
                    continue;
                }
                while (RoomManager.Instance.GetRoom(zone.MinRoom, zone.MaxRoom).Count > 0)
                {
                    Game.LogMessage($"OLC: Player {session.Player.Name} has started removal of Zone {zone.ZoneName} ({zone.ZoneID})", LogLevel.OLC);
                    var r = RoomManager.Instance.GetRoom(zone.MinRoom, zone.MaxRoom).FirstOrDefault();
                    if (r != null)
                    {
                        if (r.PlayersInRoom.Count > 0)
                        {
                            session.SendSystem($"%BRT%Cannot remove Room {r.ID}, there is at least one Player in the Room.%PT%{Constants.NewLine}");
                            Game.LogMessage($"OLC: Player {session.Player.Name} failed to remove Room {r.ID} as part of a Zone removal, at least one Player is in the Room", LogLevel.OLC);
                            return;
                        }
                        if (r.OLCLocked)
                        {
                            session.SendSystem($"%BRT%Cannot remove Room {r.ID}, the Room is Locked in OLC.%PT%{Constants.NewLine}");
                            Game.LogMessage($"OLC: Player {session.Player.Name} failed to remove Room {r.ID} as part of a Zone removal, the Room is Locked", LogLevel.OLC);
                            return;
                        }
                        if (!RoomManager.Instance.ClearRoomInventory(r.ID))
                        {
                            session.SendSystem($"%BRT%Failed to clear Inventory of Room {r.ID}.%PT%{Constants.NewLine}");
                            Game.LogMessage($"OLC: Player {session.Player.Name} failed to remove Room {r.ID} as part of a Zone removal, the Room inventory could not be cleared", LogLevel.OLC);
                            return;
                        }
                        while (r.NPCsInRoom.Count > 0)
                        {
                            var n = r.NPCsInRoom.FirstOrDefault();
                            if (n != null)
                            {
                                if (!NPCManager.Instance.RemoveNPCInstance(n.ID))
                                {
                                    session.SendSystem($"%BRT%Failed to remove NPCs from Room {r.ID}.%PT%{Constants.NewLine}");
                                    Game.LogMessage($"OLC: Player {session.Player.Name} failed to remove Room {r.ID} as part of a Zone removal, NPC instances could not be cleared", LogLevel.OLC);
                                    return;
                                }
                            }
                        }
                        if (!RoomManager.Instance.RemoveRoom(r.ID))
                        {
                            session.SendSystem($"%BRT%Failed to remove Room {r.ID} from Room Manager.{Constants.NewLine}");
                            Game.LogMessage($"OLC: Player {session.Player.Name} failed to remove Room {r.ID}, the request to Room Manager returned FALSE", LogLevel.OLC);
                            return;
                        }
                        Game.LogMessage($"OLC: Player {session.Player.Name} removed Room {r.ID} as part of a Zone removal", LogLevel.OLC);
                    }
                }
                session.SendSystem($"%BGT%All Rooms in the Zone have been removed.%PT%{Constants.NewLine}");
                if (ZoneManager.Instance.RemoveZone(zone.ZoneID))
                {
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed Zone {zone.ZoneID} ({zone.ZoneName})", LogLevel.OLC);
                    session.SendSystem($"%BGT%The specified Zone has been deleted.%PT%{Constants.NewLine}");
                    return;
                }
                else
                {
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Zone {zone.ZoneID} ({zone.ZoneName}) however the attempt failed", LogLevel.OLC);
                    session.SendSystem($"%BRT%Failed to remove the specified Zone.%PT%{Constants.NewLine}");
                }
            }
        }

        private static void ChangeZone(Session session)
        {
            session.SendSystem("Enter Zone ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int zoneID))
            {
                session.SendSystem($"%BRT%That is not a valid Zone ID.%PT%{Constants.NewLine}");
                return;
            }
            if (!ZoneManager.Instance.ZoneExists(zoneID))
            {
                session.SendSystem($"%BRT%No Zone with that ID could be found in Zone Manager!%PT%{Constants.NewLine}");
                return;
            }
            if (ZoneManager.Instance.GetZone(zoneID).OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(ZoneManager.Instance.GetZone(zoneID).LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Zone is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Zone is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            var zone = Helpers.Clone(ZoneManager.Instance.GetZone(zoneID));
            ZoneManager.Instance.SetZoneLockState(zone.ZoneID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.Clear();
                sb.AppendLine($"Zone ID: {zone.ZoneID}");
                sb.AppendLine($"Zone Name: {zone.ZoneName}");
                sb.AppendLine($"Start Room: {zone.MinRoom}{Constants.TabStop}{Constants.TabStop}End Room: {zone.MaxRoom}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Zone Name");
                sb.AppendLine($"2. Set Start Room{Constants.TabStop}3. Set End Room");
                sb.AppendLine($"4. Save Zone{Constants.TabStop}{Constants.TabStop}5. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        zone.ZoneName = GetValue<string>(session, "Enter Zone Name: ");
                        break;

                    case 2:
                        zone.MinRoom = GetValue<int>(session, "Enter Zone Start Room: ");
                        break;

                    case 3:
                        zone.MaxRoom = GetValue<int>(session, "Enter Zone End Room: ");
                        break;

                    case 4:
                        if (ValidateAsset(session, zone, false, out _))
                        {
                            if (ZoneManager.Instance.AddOrUpdateZone(zone, false))
                            {
                                ZoneManager.Instance.SetZoneLockState(zone.ZoneID, false, session);
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Zone: {zone.ZoneName} ({zone.ZoneID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The Zone has been successfully updated.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Zone: {zone.ZoneName} ({zone.ZoneID}) however the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%Failed to save the updated Zone.%PT%{Constants.NewLine}");
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Zone could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 5:
                        ZoneManager.Instance.SetZoneLockState(zone.ZoneID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}