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
                    Game.LogMessage($"ERROR: Failed to save Zone {z.ZoneName} ({z.ZoneID}) to the World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Zones.TryAdd(z.ZoneID, z))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Zone {z.ZoneName} ({z.ZoneID}) to Zone Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Zones.TryGetValue(z.ZoneID, out Zone existingZone))
                    {
                        Game.LogMessage($"ERROR: Zone {z.ZoneID} not found in Zone Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Zones.TryUpdate(z.ZoneID, z, existingZone))
                    {
                        Game.LogMessage($"ERROR: Failed to update Zone {z.ZoneID} in Zone Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ZoneManager.AddOrUpdateZone(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveZone(int id)
        {
            if (Instance.Zones.ContainsKey(id))
            {
                return Instance.Zones.TryRemove(id, out _) && DatabaseManager.RemoveZone(id);
            }
            Game.LogMessage($"ERROR: Error removing Zone {id}: No such Zone in Zone Manager", LogLevel.Error);
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
            foreach (var zone in Instance.Zones.Values)
            {
                zone.PulseZone();
            }
            Game.LogMessage($"INFO: Restocking Shops", LogLevel.Info);
            foreach (var s in ShopManager.Instance.GetShop())
            {
                s.RestockShop();
            }
        }
    }
}
