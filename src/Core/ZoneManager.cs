using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class ZoneManager
    {
        private static ZoneManager _instance = null;
        private static readonly object _lockObject = new object();
        //private ILoggingProvider _loggingProvider;
        private Dictionary<uint, Zone> zones { get; set; }

        private ZoneManager()
        {
            //_loggingProvider = loggingProvider;
            zones = new Dictionary<uint, Zone>();
        }

        internal static ZoneManager Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new ZoneManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool AddNewZone(Zone z)
        {
            try
            {
                lock (_lockObject)
                {
                    _instance.zones.Add(z.ZoneID, z);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error adding Zone {z.ZoneName} ({z.ZoneID}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal uint GetMaxAllocatedRID()
        {
            var maxAllocatedRID = Instance.zones.Max(x => x.Value.MaxRoom);
            return maxAllocatedRID;
        }

        internal Zone GetZoneForRID(uint rid)
        {
            var z = Instance.zones.Values.Where(x => rid >= x.MinRoom && rid <= x.MaxRoom).FirstOrDefault();
            return z;
        }

        internal bool IsRIDInZone(uint rid, uint zoneID)
        {
            return zones[zoneID].MinRoom <= rid && zones[zoneID].MaxRoom >= rid;
        }

        internal bool RemoveZone(uint zoneID, string zoneName)
        {
            try
            {
                lock (_lockObject)
                {
                    _instance.zones.Remove(zoneID);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error remvoing Zone {zoneName} ({zoneID}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateZone(ref Descriptor desc, Zone z)
        {
            try
            {
                if(Instance.zones.ContainsKey(z.ZoneID))
                {
                    lock(_lockObject)
                    {
                        Instance.zones.Remove(z.ZoneID);
                        Instance.zones.Add(z.ZoneID, z);
                        Game.LogMessage($"Player {desc.Player.Name} updated Zone with ID {z.ZoneID} in the Zone Manager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"Zone Manager does not contain a Zone with ID {z.ZoneID}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance.zones.Add(z.ZoneID, z);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error updating Zone {z.ZoneName} ({z.ZoneID}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal int GetZoneCount()
        {
            return Instance.zones.Count;
        }

        internal Zone GetZone(uint zid)
        {
            return Instance.zones[zid];
        }

        internal Dictionary<uint, Zone> GetAllZones()
        {
            return Instance.zones;
        }

        internal List<Zone> GetZoneByName(string n)
        {
            return (from Zone z in Instance.zones.Values where Regex.Match(z.ZoneName, n, RegexOptions.IgnoreCase).Success select z).ToList();
        }

        internal bool ZoneExists(uint zoneId)
        {
            return Instance.zones.ContainsKey(zoneId);
        }

        internal void LoadAllZones(out bool hasError)
        {
            var result = DatabaseManager.LoadAllZones(out hasError);
            if (!hasError && result != null && result.Count > 0)
            {
                Instance.zones.Clear();
                Instance.zones = result;
            }
        }
    }
}
