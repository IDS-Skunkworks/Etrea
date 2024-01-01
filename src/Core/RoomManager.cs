using System;
using Kingdoms_of_Etrea.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class RoomManager
    {
        private static RoomManager _instance = null;
        private static readonly object _lockObject = new object();
        private Dictionary<uint, Room> rooms { get; set; }

        private RoomManager()
        {
            rooms = new Dictionary<uint, Room>();
        }

        internal static RoomManager Instance
        {
            get
            {
                lock(_lockObject)
                {
                    if(_instance == null)
                    {
                        _instance = new RoomManager();
                    }
                    return _instance;
                }
            }
        }

        internal Dictionary<uint, Room> GetAllRooms()
        {
            return Instance.rooms;
        }

        internal bool RoomExists(uint targetRID)
        {
            return Instance.rooms.Keys.Contains(targetRID);
        }

        internal void LoadAllRooms(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllRoomsNew(out hasErr);
            if(!hasErr)
            {
                Instance.rooms.Clear();
                Instance.rooms = result;
            }
        }

        internal void UpdateNPCsInRoom(uint rid, bool isLeaving, bool wasTeleported, ref NPC n)
        {
            if(isLeaving)
            {
                var playersToNotify = Instance.GetPlayersInRoom(rid);
                if(playersToNotify != null && playersToNotify.Count > 0)
                {
                    foreach(var p in playersToNotify.Where(x => !x.Player.IsInCombat))
                    {
                        p.Send($"{Constants.NewLine}{n.DepartMessage}{Constants.NewLine}");
                    }
                }
            }
            else
            {
                var playersToNotify = Instance.GetPlayersInRoom(rid);
                if(playersToNotify != null && playersToNotify.Count > 0)
                {
                    foreach(var p in playersToNotify.Where(x => !x.Player.IsInCombat))
                    {
                        p.Send($"{Constants.NewLine}{n.ArrivalMessage}{Constants.NewLine}");
                    }
                }
            }
        }

        internal void UpdatePlayersInRoom(uint rid, ref Descriptor desc, bool playerLeaving, bool wasTeleported, bool isQuittingGame, bool isJoiningGame)
        {
            var pn = desc.Player.Name;
            if (playerLeaving)
            {
                lock(_lockObject)
                {
                    string msg = string.Empty;
                    if(!isQuittingGame)
                    {
                        if(desc.Player.Visible)
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
                lock(_lockObject)
                {
                    string msg = string.Empty;
                    if(!isJoiningGame)
                    {
                        if(desc.Player.Visible)
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
                    foreach(var d in Instance.GetPlayersInRoom(rid).Where(x => x.Player.Name != pn))
                    {
                        d.Send(msg);
                    }
                }
            }
        }

        internal List<uint> GetRoomIDSForZone(uint zoneID)
        {
            var result = (from Room r in Instance.rooms.Values where r.ZoneID == zoneID select r.RoomID).ToList();
            return result;
        }

        internal void LoadPlayerInRoom(uint rid, ref Descriptor desc)
        {
            lock(_lockObject)
            {
                var playersInRoom = GetRoom(rid).PlayersInRoom(rid);
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
            var players = Instance.GetRoom(rid).PlayersInRoom(rid);
            var npcs = NPCManager.Instance.GetNPCsInRoom(rid);
            lock(_lockObject)
            {
                Instance.GetRoom(rid).HasLightSource = false;
            }
            if(players != null &&  players.Count > 0)
            {
                if(players.Any(x => x.Player.HasBuff("Light")))
                {
                    lock (_lockObject)
                    {
                        Instance.GetRoom(rid).HasLightSource = true;
                    }
                }
            }
            if (npcs != null && npcs.Count > 0)
            {
                if(npcs.Any(x => x.HasBuff("Light")))
                {
                    lock (_lockObject)
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
                lock (_lockObject)
                {
                    Instance.rooms[rid].ItemsInRoom.Add(i);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding Item {i.Name} to ItemsInRoom for Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveItemFromRoomInventory(uint rid, ref InventoryItem i)
        {
            try
            {
                lock (_lockObject)
                {
                    Instance.rooms[rid].ItemsInRoom.Remove(i);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing Item {i.Name} from ItemsInRoom for Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool ClearRoomInventory(uint rid)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.rooms[rid].ItemsInRoom.Clear();
                    Instance.rooms[rid].GoldInRoom = 0;
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error clearing ItemsInRoom for Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal string GetRoomShortDescription(uint rid)
        {
            return GetRoom(rid).ShortDescription;
        }

        internal string GetRoomLongDescription(uint rid)
        {
            var targetRoom = GetRoom(rid);
            if (targetRoom == null)
            {
                return "That way lies only the void...";
            }
            return targetRoom.LongDescription;
        }

        internal bool UpdateRoom(ref Descriptor desc, Room r)
        {
            try
            {
                if(Instance.rooms.ContainsKey(r.RoomID))
                {
                    lock(_lockObject)
                    {
                        Instance.rooms.Remove(r.RoomID);
                        Instance.rooms.Add(r.RoomID, r);
                        Game.LogMessage($"INFO: Player {desc.Player.Name} updated Room {r.RoomID} in the Room Manager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: Room Manager does not contain a Room with ID {r.RoomID}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance.rooms.Add(r.RoomID, r);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Room {r.RoomID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNewRoom(ref Descriptor desc, Room r)
        {
            try
            {
                lock (_lockObject)
                {
                    Instance.rooms.Add(r.RoomID, r);
                    Game.LogMessage($"INFO: Player {desc.Player} added Room '{r.RoomName}' ({r.RoomID}) to RoomManager", LogLevel.Info, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error adding a new Room to RoomManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveRoom(uint rid, ref Descriptor desc)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.rooms.Remove(rid);
                }
                Game.LogMessage($"Player {desc.Player.Name} removed Room {rid} from Room Manager", LogLevel.Warning, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error removing Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddGoldToRoom(uint rid, uint gp)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.rooms[rid].GoldInRoom += gp;
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error adding {gp} gold to Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool GetGoldFromRoom(uint rid, uint gp)
        {
            try
            {
                lock (_lockObject)
                {
                    Instance.rooms[rid].GoldInRoom -= gp;
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"Error removing {gp} gold from Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal List<NPC> GetNPCsInRoom(uint rid)
        {
            return NPCManager.Instance.GetNPCsInRoom(rid);
        }

        internal List<Descriptor> GetPlayersInRoom(uint rid)
        {
            return GetRoom(rid).PlayersInRoom(rid);
        }

        internal List<Room> GetRoomsWithSpecifiedShop(uint shopID)
        {
            return Instance.rooms.Values.Where(x => x.ShopID.HasValue && x.ShopID == shopID).ToList();
        }

        internal Room GetRoom(uint rid)
        {
            return Instance.rooms.ContainsKey(rid) ? Instance.rooms[rid] : null;
        }

        internal List<Room> GetRoomsByIDRange(uint start, uint end)
        {
            var retval = rooms.Values.Where(x => x.RoomID >= start && x.RoomID <= end).ToList();
            return retval;
        }

        internal List<Room> GetRoomByNameOrDescription(string n)
        {
            return (from Room r in Instance.rooms.Values where Regex.Match(r.RoomName, n, RegexOptions.IgnoreCase).Success ||
                    Regex.Match(r.ShortDescription, n, RegexOptions.IgnoreCase).Success || Regex.Match(r.LongDescription, n, RegexOptions.IgnoreCase).Success select r).ToList();
        }

        internal int GetRoomCount()
        {
            return Instance.rooms.Count;
        }
    }
}
