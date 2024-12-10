using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class ZoneManager
    {
        private static ZoneManager instance = null;
        private ConcurrentDictionary<int, Zone> Zones { get; set; }
        public int Count => Zones.Count;
        public int MaxAllocatedRoomID => Instance.Zones.Values.Max(x => x.MaxRoom);

        private ZoneManager()
        {
            Zones = new ConcurrentDictionary<int, Zone>();
        }

        public static ZoneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ZoneManager();
                }
                return instance;
            }
        }

        public void SetZoneLockState(int id, bool locked, Session session)
        {
            if (Instance.Zones.ContainsKey(id))
            {
                Instance.Zones[id].OLCLocked = locked;
                Instance.Zones[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetZoneLockState(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.Zones.ContainsKey(id))
            {
                lockHolder = Instance.Zones[id].LockHolder;
                return Instance.Zones[id].OLCLocked;
            }
            return false;
        }

        public Zone GetZone(int id)
        {
            return Instance.Zones.ContainsKey(id) ? Instance.Zones[id] : null;
        }

        public List<Zone> GetZone(int start, int end)
        {
            return end <= start ? null : Instance.Zones.Values.Where(x => x.ZoneID >= start && x.ZoneID <= end).ToList();
        }

        public Zone GetZone(string criteria)
        {
            return Instance.Zones.Values.FirstOrDefault(x => x.ZoneName.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public List<Zone> GetZone()
        {
            return Instance.Zones.Values.ToList();
        }

        public Zone GetZoneForRID(int rid)
        {
            return Instance.Zones.Values.Where(x => rid >= x.MinRoom && rid <= x.MaxRoom).FirstOrDefault();
        }

        public bool ZoneExists(int id)
        {
            return Instance.Zones.ContainsKey(id);
        }

        public bool AddOrUpdateZone(Zone z, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveZoneToWorldDatabase(z, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Zone {z.ZoneName} ({z.ZoneID}) to the World Database", LogLevel.Error, true);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Zones.TryAdd(z.ZoneID, z))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Zone {z.ZoneName} ({z.ZoneID}) to Zone Manager", LogLevel.Error, true);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Zones.TryGetValue(z.ZoneID, out Zone existingZone))
                    {
                        Game.LogMessage($"ERROR: Zone {z.ZoneID} not found in Zone Manager for update", LogLevel.Error, true);
                        return false;
                    }
                    if (!Instance.Zones.TryUpdate(z.ZoneID, z, existingZone))
                    {
                        Game.LogMessage($"ERROR: Failed to update Zone {z.ZoneID} in Zone Manager due to a value mismatch", LogLevel.Error, true);
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ZoneManager.AddOrUpdateZone(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public bool RemoveZone(int id)
        {
            if (Instance.Zones.ContainsKey(id))
            {
                return Instance.Zones.TryRemove(id, out _) && DatabaseManager.RemoveZone(id);
            }
            Game.LogMessage($"ERROR: Error removing Zone {id}: No such Zone in Zone Manager", LogLevel.Error, true);
            return false;
        }

        public void LoadAllZones(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllZones(out hasErr);
            if (!hasErr && result != null)
            {
                foreach(var zone in result)
                {
                    Instance.Zones.AddOrUpdate(zone.Key, zone.Value, (k, v) => zone.Value);
                }
            }
        }

        public void PulseAllZones()
        {
            foreach (var zone in Instance.GetZone())
            {
                Game.LogMessage($"INFO: Pulsing Zone {zone.ZoneName} (ID: {zone.ZoneID})", LogLevel.Info, true);
                var npcsForZone = NPCManager.Instance.GetNPCsForZone(zone.ZoneID);
                var roomsForZone = RoomManager.Instance.GetRoomsForZone(zone.ZoneID);
                // spawn new NPCs - spawn at tick first, then random npc generation
                foreach (var r in roomsForZone.Where(x => x.SpawnNPCsOnTick.Count > 0 && !x.Flags.HasFlag(RoomFlags.NoMobs)))
                {
                    foreach (var n in r.SpawnNPCsOnTick)
                    {
                        var npcTemplate = NPCManager.Instance.GetNPC(n.Key);
                        if (npcTemplate != null)
                        {
                            if (r.NPCsInRoom.Where(x => x.TemplateID == npcTemplate.TemplateID).Count() < n.Value)
                            {
                                if (NPCManager.Instance.GetNPCInstanceCount(npcTemplate.TemplateID) < npcTemplate.MaxNumberInWorld)
                                {
                                    NPCManager.Instance.AddNewNPCInstance(npcTemplate.TemplateID, r.ID);
                                }
                                else
                                {
                                    Game.LogMessage($"INFO: Not spawing NPC {npcTemplate.TemplateID} in Room {r.ID}, max number in world reached", LogLevel.Info, true);
                                }
                            }
                            else
                            {
                                Game.LogMessage($"INFO: Not spawing NPC {npcTemplate.TemplateID} in Room {r.ID}, max number in Room reached", LogLevel.Info, true);
                            }
                        }
                        else
                        {
                            Game.LogMessage($"ERROR: Cannot spawn instance of NPC {n.Key} in Room {r.ID}: No such NPC Template in NPC Manager", LogLevel.Error, true);
                        }
                    }
                }
                foreach (var r in roomsForZone.Where(x => !x.Flags.HasFlag(RoomFlags.NoMobs)))
                {
                    foreach (var npc in npcsForZone)
                    {
                        var roll = Helpers.RollDice<int>(1, 100);
                        if (roll < npc.AppearanceChance && NPCManager.Instance.GetNPCInstanceCount(npc.TemplateID) + 1 <= npc.MaxNumberInWorld)
                        {
                            NPCManager.Instance.AddNewNPCInstance(npc.TemplateID, r.ID);
                        }
                    }
                }
                // spawn tick items, if any
                foreach (var r in roomsForZone.Where(x => x.SpawnItemsOnTick.Count > 0))
                {
                    foreach (var i in r.SpawnItemsOnTick)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            while (r.ItemsInRoom.Values.Where(x => x.ID == item.ID).Count() < i.Value)
                            {
                                dynamic newItem = null;
                                switch (item.ItemType)
                                {
                                    case ItemType.Misc:
                                        newItem = Helpers.Clone<InventoryItem>(item);
                                        break;

                                    case ItemType.Weapon:
                                        newItem = Helpers.Clone<Weapon>(item);
                                        break;

                                    case ItemType.Consumable:
                                        newItem = Helpers.Clone<Consumable>(item);
                                        break;

                                    case ItemType.Armour:
                                        newItem = Helpers.Clone<Armour>(item);
                                        break;

                                    case ItemType.Ring:
                                        newItem = Helpers.Clone<Ring>(item);
                                        break;

                                    case ItemType.Scroll:
                                        newItem = Helpers.Clone<Scroll>(item);
                                        break;
                                }
                                Game.LogMessage($"INFO: Spawning Item {i.Key} in Room {r.ID}", LogLevel.Info, true);
                                newItem.ItemID = Guid.NewGuid();
                                RoomManager.Instance.AddItemToRoomInventory(r.ID, newItem);
                            }
                        }
                        else
                        {
                            Game.LogMessage($"ERROR: Not spawning Item {i.Key} in Room {r.ID}: No such Item in Item Manager", LogLevel.Error, true);
                        }
                    }
                }
                // spawn nodes in caves, if there are any
                var caves = roomsForZone.Where(x => x.Flags.HasFlag(RoomFlags.Cave)).ToList();
                foreach (var r in caves)
                {
                    if (r.RSSNode == null)
                    {
                        var roll = Helpers.RollDice<int>(1, 100);
                        var node = NodeManager.Instance.GetRandomNode(roll);
                        if (node != null)
                        {
                            var newNode = Helpers.Clone(node);
                            newNode.Depth = Helpers.RollDice<int>(1, 4);
                            r.RSSNode = newNode;
                            Game.LogMessage($"INFO: Spawning Resource Node {newNode.Name} in Room {r.ID}", LogLevel.Info, true);
                        }
                    }
                }
            }
            // restock shops
            Game.LogMessage($"INFO: Restocking Shops", LogLevel.Info, true);
            foreach (var s in ShopManager.Instance.GetShop())
            {
                s.RestockShop();
            }
        }
    }
}
