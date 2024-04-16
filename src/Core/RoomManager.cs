using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Etrea2.Entities;

namespace Etrea2.Core
{
    internal class RoomManager
    {
        private static RoomManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Room> Rooms { get; set; }

        private RoomManager()
        {
            Rooms = new Dictionary<uint, Room>();
        }

        internal static RoomManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new RoomManager();
                    }
                    return _instance;
                }
            }
        }

        internal Dictionary<uint, Room> GetAllRooms()
        {
            lock(_lock)
            {
                return Instance.Rooms;
            }
        }

        internal bool RoomExists(uint roomId)
        {
            lock (_lock)
            {
                return Instance.Rooms.ContainsKey(roomId);
            }
        }

        internal void LoadAllRooms(out bool hasError)
        {
            var result = DatabaseManager.LoadAllRooms(out hasError);
            if (!hasError && result != null)
            {
                lock (_lock)
                {
                    Instance.Rooms.Clear();
                    Instance.Rooms = result;
                }
            }
        }

        internal void UpdateNPCsInRoom(uint rid, bool isLeaving, bool wasTeleported, ref NPC n)
        {
            lock (_lock)
            {
                if (isLeaving)
                {
                    var playersToNotify = Instance.GetPlayersInRoom(rid);
                    if (playersToNotify != null && playersToNotify.Count > 0)
                    {
                        foreach (var p in playersToNotify.Where(x => !x.Player.IsInCombat))
                        {
                            p.Send($"{Constants.NewLine}{n.DepartureMessage}{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    var playersToNotify = Instance.GetPlayersInRoom(rid);
                    if (playersToNotify != null && playersToNotify.Count > 0)
                    {
                        foreach (var p in playersToNotify.Where(x => !x.Player.IsInCombat))
                        {
                            p.Send($"{Constants.NewLine}{n.ArrivalMessage}{Constants.NewLine}");
                        }
                    }
                }
            }
        }

        internal void UpdatePlayersInRoom(uint rid, ref Descriptor desc, bool playerLeaving, bool wasTeleported, bool isQuittingGame, bool isJoiningGame)
        {
            var pn = desc.Player.Name;
            if (playerLeaving)
            {
                lock (_lock)
                {
                    string msg = string.Empty;
                    if (!isQuittingGame)
                    {
                        if (desc.Player.Visible)
                        {
                            msg = wasTeleported ? $"{desc.Player.Name} is spirited away by the Winds of Magic!{Constants.NewLine}" : $"{desc.Player.Name} walks away{Constants.NewLine}";
                        }
                        else
                        {
                            msg = wasTeleported ? $"The Winds of Magic swirl and spirit something away...{Constants.NewLine}" : $"There is a slight breeze as something walks away{Constants.NewLine}";
                        }
                    }
                    else
                    {
                        msg = desc.Player.Visible ? $"{desc.Player.Name} fades out of existence...{Constants.NewLine}" : $"Something fades out of existence...{Constants.NewLine}";
                    }
                    foreach (var d in Instance.GetPlayersInRoom(rid).Where(x => x.Player.Name != pn))
                    {
                        d.Send(msg);
                    }
                }
            }
            else
            {
                lock (_lock)
                {
                    string msg = string.Empty;
                    if (!isJoiningGame)
                    {
                        if (desc.Player.Visible)
                        {
                            msg = wasTeleported ? $"{desc.Player.Name} arrives from a swirling cloud of magic! {Constants.NewLine}" : $"{desc.Player.Name} arrives {Constants.NewLine}";
                        }
                        else
                        {
                            msg = wasTeleported ? $"A swirling cloud of magic signals the arrival of something{Constants.NewLine}" : $"There is a slight breeze as something arrives{Constants.NewLine}";
                        }
                    }
                    else
                    {
                        msg = desc.Player.Visible ? $"{desc.Player.Name} fades into existence...{Constants.NewLine}" : $"Something fades into existence...{Constants.NewLine}";
                    }
                    foreach (var d in Instance.GetPlayersInRoom(rid).Where(x => x.Player.Name != pn))
                    {
                        d.Send(msg);
                    }
                }
            }
        }

        internal List<Room> GetRoomIDsForZone(uint zoneId)
        {
            lock(_lock)
            {
                return (from r in Instance.Rooms.Values where r.ZoneID == zoneId select r).ToList();
            }
        }

        internal void LoadPlayerInRoom(uint roomId, ref Descriptor desc)
        {
            lock (_lock)
            {
                var playersInRoom = Instance.GetPlayersInRoom(roomId);
                var pName = desc.Player?.Name;
                foreach (var p in playersInRoom.Where(x => x.Player.Name != pName))
                {
                    if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                    {
                        p.Send($"With a burst of light {desc.Player.Name} appears in the world!{Constants.NewLine}");
                    }
                }
            }
        }

        internal void ProcessEnvironmentBuffs(uint rid)
        {
            var players = Instance.GetPlayersInRoom(rid);
            var npcs = NPCManager.Instance.GetNPCsInRoom(rid);
            lock (_lock)
            {
                Instance.GetRoom(rid).HasLightSource = false;
            }
            if (players != null && players.Count > 0)
            {
                if (players.Any(x => x.Player.HasBuff("Light")))
                {
                    lock (_lock)
                    {
                        Instance.GetRoom(rid).HasLightSource = true;
                    }
                }
            }
            if (npcs != null && npcs.Count > 0)
            {
                if (npcs.Any(x => x.HasBuff("Light")))
                {
                    lock (_lock)
                    {
                        Instance.GetRoom(rid).HasLightSource = true;
                    }
                }
            }
        }

        internal bool AddItemToRoomInventory(uint rid, ref InventoryItem i)
        {
            try
            {
                if (i != null)
                {
                    lock (_lock)
                    {
                        Instance.Rooms[rid].ItemsInRoom.Add(i);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: Attempt to add null to the Inventory of RID {rid}", LogLevel.Warning, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding Item {i.Name} to ItemsInRoom for Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveItemFromRoomInventory(uint rid, ref InventoryItem i)
        {
            try
            {
                lock (_lock)
                {
                    Instance.Rooms[rid].ItemsInRoom.Remove(i);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing Item {i.Name} from ItemsInRoom for Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool ClearRoomInventory(uint rid)
        {
            try
            {
                lock (_lock)
                {
                    Instance.Rooms[rid].ItemsInRoom.Clear();
                    Instance.Rooms[rid].GoldInRoom = 0;
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error clearing Inventory for Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNewRoom(ref Descriptor desc, Room r)
        {
            try
            {
                lock (_lock)
                {
                    Instance.Rooms.Add(r.RoomID, r);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} has added Room {r.RoomID} ({r.RoomName}) to RoomManager", LogLevel.OLC, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding Room {r.RoomID} ({r.RoomName}) to RoomManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveRoom(uint rid, string roomName, ref Descriptor desc)
        {
            try
            {
                lock (_lock)
                {
                    Instance.Rooms.Remove(rid);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} removed Room {rid} ({roomName}) from RoomManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Room {rid} ({roomName}) from RoomManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateRoom(ref Descriptor desc, Room r)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance.Rooms.ContainsKey(r.RoomID))
                    {
                        Instance.Rooms.Remove(r.RoomID);
                        Instance.Rooms.Add(r.RoomID, r);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} updated Room {r.RoomID} ({r.RoomName}) in RoomManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: RoomManager does not contain a Room with ID {r.RoomID} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddNewRoom(ref desc, r);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Room {r.RoomID} ({r.RoomName}) in RoomManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddGoldToRoom(uint rid, ulong gold)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance.Rooms.ContainsKey(rid))
                    {
                        Instance.Rooms[rid].GoldInRoom += gold;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding {gold:N0} gold to Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveGoldFromRoom(uint rid, ulong gold)
        {
            try
            {
                lock(_lock)
                {
                    if (Instance.Rooms.ContainsKey(rid))
                    {
                        Instance.Rooms[rid].GoldInRoom -= gold;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing {gold:N0} gold from Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal List<NPC> GetNPCsInRoom(uint rid)
        {
            try
            {
                lock(_lock)
                {
                    if (Instance.Rooms.ContainsKey(rid))
                    {
                        return Instance.Rooms[rid].NPCsInRoom;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error getting list of NPCs in Room {rid}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal List<Descriptor> GetPlayersInRoom(uint rid)
        {
            try
            {
                lock(_lock)
                {
                    if (Instance.Rooms.ContainsKey(rid))
                    {
                        return Instance.Rooms[rid].PlayersInRoom;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error getting list of Players in Room {rid}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal List<Room> GetRoomsWithSpecifiedShop(uint shopID)
        {
            try
            {
                lock(_lock)
                {
                    return Instance.Rooms.Values.Where(x => x.ShopID.HasValue && x.ShopID.Value == shopID).ToList();
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error getting list of Rooms with Shop ID {shopID}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal Room GetRoom(uint roomID)
        {
            lock(_lock)
            {
                return Instance.Rooms.ContainsKey(roomID) ? Instance.Rooms[roomID] : null;
            }
        }

        internal List<Room> GetRoomsByIDRange(uint start, uint end)
        {
            try
            {
                lock (_lock)
                {
                    return Instance.Rooms.Values.Where(x => x.RoomID >= start && x.RoomID <= end).ToList();
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error getting Rooms in range {start}-{end}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal List<Room> GetRoomByNameOrDescription(string n)
        {
            try
            {
                lock(_lock)
                {
                    return (from r in Instance.Rooms.Values
                            where Regex.IsMatch(r.RoomName, n, RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(r.ShortDescription, n, RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(r.LongDescription, n, RegexOptions.IgnoreCase)
                            select r).ToList();
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in GetRoomByNameOrDescription (Criteria: {n}): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal List<Room> GetRoomsWithPlayers()
        {
            try
            {
                lock (_lock)
                {
                    return Instance.Rooms.Values.Where(x => x.PlayersInRoom.Count > 0).ToList();
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in GetRoomsWithPlayers(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal int GetRoomCount()
        {
            lock(_lock)
            {
                return Instance.Rooms.Count;
            }
        }
    }
}
