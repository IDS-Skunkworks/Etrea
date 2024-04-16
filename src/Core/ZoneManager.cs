using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class ZoneManager
    {
        private static ZoneManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Zone> Zones { get; set; }

        private ZoneManager()
        {
            Zones = new Dictionary<uint, Zone>();
        }

        public static ZoneManager Instance
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ZoneManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool AddNewZone(ref Descriptor desc, Zone z)
        {
            try
            {
                lock(_lock)
                {
                    Instance.Zones.Add(z.ZoneID, z);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} has added Zone {z.ZoneID} ({z.ZoneName}) to ZoneManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding Zone {z.ZoneID} ({z.ZoneName}) to ZoneManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveZone(ref Descriptor desc, uint zoneID, string zoneName)
        {
            try
            {
                lock(_lock)
                {
                    Instance.Zones.Remove(zoneID);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} has removed Zone {zoneID} ({zoneName}) from ZoneManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Zone {zoneID} ({zoneName}) from ZoneManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateZone(ref Descriptor desc, Zone zone)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance.Zones.ContainsKey(zone.ZoneID))
                    {
                        Instance.Zones.Remove(zone.ZoneID);
                        Instance.Zones.Add(zone.ZoneID, zone);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} has updated Zone {zone.ZoneID} ({zone.ZoneName}) in ZoneManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: Player {desc.Player.Name} attempted to update Zone {zone.ZoneID} ({zone.ZoneName}) in ZoneManager but the ID could not be found, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddNewZone(ref desc, zone);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error attempting to update Zone {zone.ZoneID} ({zone.ZoneName}) in ZoneManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal uint GetMaxAllocatedRID()
        {
            lock (_lock)
            {
                return Instance.Zones.Values.Max(x => x.MaxRoom);
            }
        }

        internal Zone GetZoneForRID(uint rid)
        {
            lock (_lock)
            {
                return Instance.Zones.Values.Where(x => rid >= x.MinRoom && rid <= x.MaxRoom).FirstOrDefault();
            }
        }

        internal bool IsRIDInZone(uint rid, uint zone)
        {
            lock (_lock)
            {
                if (Instance.Zones.ContainsKey(zone))
                {
                    return Instance.Zones[zone].MinRoom <= rid && Instance.Zones[zone].MaxRoom >= rid;
                }
                return false;
            }
        }

        internal int GetZoneCount()
        {
            lock (_lock)
            {
                return Instance.Zones.Count;
            }
        }

        internal Dictionary<uint, Zone> GetAllZones()
        {
            lock (_lock)
            {
                return Instance.Zones;
            }
        }

        internal List<Zone> GetZoneByIDRange(uint start, uint end)
        {
            lock (_lock)
            {
                return Instance.Zones.Values.Where(x => x.ZoneID >= start && x.ZoneID <= end).ToList();
            }
        }

        internal List<Zone> GetZoneByName(string name)
        {
            lock (_lock)
            {
                return Instance.Zones.Values.Where(x => Regex.IsMatch(x.ZoneName, name, RegexOptions.IgnoreCase)).ToList();
            }
        }

        internal Zone GetZone(uint id)
        {
            lock (_lock)
            {
                return Instance.Zones.ContainsKey(id) ? Instance.Zones[id] : null;
            }
        }

        internal bool ZoneExists(uint zone)
        {
            lock (_lock)
            {
                return Instance.Zones.ContainsKey(zone);
            }
        }

        internal void LoadAllZones(out bool hasError)
        {
            var result = DatabaseManager.LoadAllZones(out hasError);
            if (!hasError && result != null && result.Count > 0)
            {
                Instance.Zones.Clear();
                Instance.Zones = result;
            }
        }
    }
}
