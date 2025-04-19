using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class RoomManager
    {
        private static RoomManager instance = null;
        private ConcurrentDictionary<int, Room> Rooms { get; set; }
        public int Count => Instance.Rooms.Count;

        private RoomManager()
        {
            Rooms = new ConcurrentDictionary<int, Room>();
        }

        public static RoomManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RoomManager();
                }
                return instance;
            }
        }

        public void RunRoomProgs(ulong tickCount, RoomProgTrigger trigger)
        {
            _ = Task.Run(() =>
            {
                ConcurrentBag<Room> progRooms = (ConcurrentBag<Room>)Instance.Rooms.Values.Where(x => x.RoomProgs.Count > 0);
                Parallel.ForEach(progRooms, r =>
                {
                    foreach (var p in r.RoomProgs)
                    {
                        RoomProg rp = ScriptObjectManager.Instance.GetScriptObject<RoomProg>(p.Key);
                        if (rp == null)
                        {
                            Game.LogMessage($"WARN: Cannot execute RoomProg {p.Key} in Room {r.ID} for Trigger {trigger}: No such RoomProg could be found", LogLevel.Warning);
                            continue;
                        }
                        if (rp.Triggers.HasFlag(trigger))
                        {
                            rp.Init();
                            rp.TriggerEvent(trigger, new { r.ID, tickCount });
                        }
                    }
                });
                progRooms = null;
            });
        }

        public void SetRoomLockState(int id, bool locked, Session session)
        {
            if (Instance.Rooms.ContainsKey(id))
            {
                Instance.Rooms[id].OLCLocked = locked;
                Instance.Rooms[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool ToggleRoomFlag(int id, RoomFlags flag)
        {
            if (Instance.Rooms.ContainsKey(id))
            {
                Instance.Rooms[id].Flags ^= flag;
                return true;
            }
            return false;
        }

        public bool SetRoomFlag(int id, RoomFlags flag, bool enable)
        {
            if (Instance.Rooms.ContainsKey(id))
            {
                switch(enable)
                {
                    case true:
                        Instance.Rooms[id].Flags |= flag;
                        break;

                    case false:
                        Instance.Rooms[id].Flags &= ~flag;
                        break;
                }
                return true;
            }
            return false;
        }

        public bool GetRoomLockState(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.Rooms.ContainsKey(id))
            {
                lockHolder = Instance.Rooms[id].LockHolder;
                return Instance.Rooms[id].OLCLocked;
            }
            return false;
        }

        public List<Room> GetRoomsWithItems()
        {
            return Instance.Rooms.Values.Where(x => x.ItemsInRoom.Count > 0).ToList();
        }

        public List<Room> GetRoom()
        {
            return Instance.Rooms.Values.OrderBy(x => x.ID).ToList();
        }

        public bool RoomExists(int id)
        {
            return Instance.Rooms.ContainsKey(id);
        }

        public bool LoadAllRooms()
        {
            if (!DatabaseManager.LoadAllRooms(out var allRooms) || allRooms == null)
            {
                return false;
            }
            foreach (var room in allRooms)
            {
                Instance.Rooms.AddOrUpdate(room.Key, room.Value, (k, v) => room.Value);
            }
            return true;
        }

        public List<Room> GetRoomsForZone(int zoneId)
        {
            return (from r in Instance.Rooms.Values where r.ZoneID == zoneId select r).OrderBy(x => x.ID).ToList();
        }

        public Room GetRoom(int roomId)
        {
            if (Instance.Rooms.ContainsKey(roomId))
            {
                return Instance.Rooms[roomId];
            }
            return null;
        }

        public List<Room> GetRoom(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                return null;
            }
            return (from r in Instance.Rooms.Values
                    where Regex.IsMatch(r.RoomName, criteria, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(r.ShortDescription, criteria, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(r.LongDescription, criteria, RegexOptions.IgnoreCase)
                    select r).OrderBy(x => x.ID).ToList();
        }

        public List<Room> GetRoom(int start, int end)
        {
            if (end < start || end == start)
            {
                return null;
            }
            return (from r in Instance.Rooms.Values where r.ID >= start && r.ID <= end select r).OrderBy(x => x.ID).ToList();
        }

        public void LoadPlayerIntoRoom(int roomID, Session session)
        {
            if (Instance.Rooms.ContainsKey(roomID))
            {
                var playersInRoom = Instance.Rooms[roomID].PlayersInRoom.Where(x => x.ID != session.ID && session.Player.CanBeSeenBy(x.Player)).ToList();
                if (playersInRoom != null && playersInRoom.Count > 0)
                {
                    foreach(var player in playersInRoom)
                    {
                        player.Send($"With a burst of light {session.Player.Name} appears in the world!{Constants.NewLine}");
                    }
                }
            }
        }

        public bool AddItemToRoomInventory(int roomID, int itemID)
        {
            if (!ItemManager.Instance.ItemExists(itemID))
            {
                Game.LogMessage($"ERROR: Cannot spawn Item {itemID} in Room {roomID}, no such Item in Item Manager", LogLevel.Error);
                return false;
            }
            if (!Instance.Rooms.ContainsKey(roomID))
            {
                Game.LogMessage($"ERROR: Cannot spawn Item {itemID} in Room {roomID}, no such Room in Room Manager", LogLevel.Error);
                return false;
            }
            dynamic spawmItem = null;
            var baseItem = ItemManager.Instance.GetItem(itemID);
            switch (baseItem.ItemType)
            {
                case ItemType.Misc:
                    spawmItem = Helpers.Clone<InventoryItem>(baseItem);
                    break;

                case ItemType.Weapon:
                    spawmItem = Helpers.Clone<Weapon>(baseItem);
                    break;

                case ItemType.Consumable:
                    spawmItem = Helpers.Clone<Consumable>(baseItem);
                    break;

                case ItemType.Armour:
                    spawmItem = Helpers.Clone<Armour>(baseItem);
                    break;

                case ItemType.Ring:
                    spawmItem = Helpers.Clone<Ring>(baseItem);
                    break;

                case ItemType.Scroll:
                    spawmItem = Helpers.Clone<Scroll>(baseItem);
                    break;
            }
            if (spawmItem.ItemID == Guid.Empty)
            {
                spawmItem.ItemID = Guid.NewGuid();
            }
            return Instance.AddItemToRoomInventory(roomID, spawmItem);
        }

        public bool AddItemToRoomInventory(int roomID, InventoryItem item)
        {
            if (!Instance.Rooms.ContainsKey(roomID))
            {
                Game.LogMessage($"ERROR: Attempt to add an item to Room {roomID} but no such Room in RoomManager", LogLevel.Error);
                return false;
            }
            if (item == null)
            {
                Game.LogMessage($"ERROR: Attempt to add a null to the inventory of Room {roomID}", LogLevel.Error);
                return false;
            }
            try
            {
                Instance.Rooms[roomID].ItemsInRoom.TryAdd(item.ItemID, item);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomManager.AddItemToRoomInventory(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveItemFromRoomInventory(int roomID, InventoryItem item)
        {
            if (!Instance.Rooms.ContainsKey(roomID))
            {
                Game.LogMessage($"ERROR: Attempt to remove an item from the inventory of Room {roomID} but no such Room in RoomManager", LogLevel .Error);
                return false;
            }
            if (item == null)
            {
                Game.LogMessage($"ERROR: Attempt to remove null from the inventory of Room {roomID}", LogLevel.Error);
                return false;
            }
            try
            {
                return Instance.Rooms[roomID].ItemsInRoom.TryRemove(item.ItemID, out _);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomManager.RemoveItemFromRoomInventory(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool ClearRoomInventory(int roomID)
        {
            if (Instance.Rooms.ContainsKey(roomID))
            {
                Instance.Rooms[roomID].ItemsInRoom.Clear();
                return true;
            }
            return false;
        }

        public bool UpdateRoomNode(int roomID, ResourceNode node)
        {
            if (!Instance.Rooms.ContainsKey(roomID))
            {
                return false;
            }
            Instance.Rooms[roomID].RSSNode = node;
            return true;
        }

        public bool RoomIDInUse(int roomID)
        {
            if (Instance.RoomExists(roomID))
            {
                return true;
            }
            if (DatabaseManager.RoomIDInUse(roomID))
            {
                return true;
            }
            return false;
        }

        public bool AddOrUpdateRoom(Room r, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveRoomToWorldDatabase(r, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Room {r.RoomName} ({r.ID}) to the World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Rooms.TryAdd(r.ID, r))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Room {r.RoomName} ({r.ID}) to Room Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Rooms.TryGetValue(r.ID, out Room existingRoom))
                    {
                        Game.LogMessage($"ERROR: Room {r.ID} not found in Room Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Rooms.TryUpdate(r.ID, r, existingRoom))
                    {
                        Game.LogMessage($"ERROR: Failed to update Room {r.ID} in Room Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomManager.AddOrUpdateRoom(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveRoom(int roomID)
        {
            if (Instance.Rooms.ContainsKey(roomID))
            {
                return Instance.Rooms.TryRemove(roomID, out _) && DatabaseManager.RemoveRoom(roomID);
            }
            Game.LogMessage($"ERROR: Error removing Room with ID {roomID}, no such Room in RoomManager", LogLevel.Error);
            return false;
        }

        public bool AddGoldToRoom(int roomID, ulong gold)
        {
            if (Instance.Rooms.ContainsKey(roomID))
            {
                Instance.Rooms[roomID].GoldInRoom += gold;
                return true;
            }
            Game.LogMessage($"ERROR: Attempt to add {gold:N0} gold to Room {roomID}, no such Room in RoomManager", LogLevel.Error);
            return false;
        }

        public bool RemoveGoldFromRoom(int roomID, ulong gold)
        {
            if (Instance.Rooms.ContainsKey(roomID))
            {
                Instance.Rooms[roomID].GoldInRoom -= gold;
                return true;
            }
            Game.LogMessage($"ERROR: Attempt to remove {gold:N0} gold from Room {roomID}, no such Room in RoomManager", LogLevel.Error);
            return false;
        }
    }
}
