using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class ScriptObjectManager
    {
        private static ScriptObjectManager instance = null;
        private ConcurrentDictionary<int, MobProg> MobProgs { get; set; }
        private ConcurrentDictionary<int, RoomProg> RoomProgs { get; set; }
        public int Count => MobProgs.Count + RoomProgs.Count;

        private ScriptObjectManager()
        {
            MobProgs = new ConcurrentDictionary<int, MobProg>();
            RoomProgs = new ConcurrentDictionary<int, RoomProg>();
        }

        public static ScriptObjectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScriptObjectManager();
                }
                return instance;
            }
        }

        public void SetScriptLockState<T>(int id, bool locked, Session session)
        {
            if (typeof(T) == typeof(MobProg))
            {
                if (Instance.MobProgs.ContainsKey(id))
                {
                    Instance.MobProgs[id].OLCLocked = locked;
                    Instance.MobProgs[id].LockHolder = locked ? session.ID : Guid.Empty;
                }
                return;
            }
            if (typeof(T) == typeof(RoomProg))
            {
                if (Instance.RoomProgs.ContainsKey(id))
                {
                    Instance.RoomProgs[id].OLCLocked = locked;
                    Instance.RoomProgs[id].LockHolder = locked ? session.ID: Guid.Empty;
                }
                return;
            }
        }

        public bool ScriptObjectExists<T>(int id)
        {
            if (typeof(T) == typeof(MobProg))
            {
                return Instance.MobProgs.ContainsKey(id);
            }
            if (typeof(T) == typeof(RoomProg))
            {
                return Instance.RoomProgs.ContainsKey(id);
            }
            return false;
        }

        public dynamic GetScriptObject<T>(int id)
        {
            if (typeof(T) == typeof(MobProg) && Instance.MobProgs.ContainsKey(id))
            {
                return Instance.MobProgs[id];
            }
            if (typeof(T) == typeof(RoomProg) && Instance.RoomProgs.ContainsKey(id))
            {
                return Instance.RoomProgs[id];
            }
            return null;
        }

        public MobProg GetMobProg(int id)
        {
            return Instance.GetScriptObject<MobProg>(id);
        }

        public List<T> GetScriptObject<T>()
        {
            if (typeof(T) == typeof(MobProg))
            {
                return Instance.MobProgs.Values.Cast<T>().ToList();
            }
            if (typeof(T) == typeof(RoomProg))
            {
                return Instance.RoomProgs.Values.Cast<T>().ToList();
            }
            return null;
        }

        public List<T> GetScriptObject<T>(int start, int end)
        {
            if (end <= start)
            {
                return null;
            }
            if (typeof(T) == typeof(MobProg))
            {
                return Instance.MobProgs.Values.Where(x => x.ID >= start && x.ID <= end).Cast<T>().ToList();
            }
            if (typeof(T) == typeof(RoomProg))
            {
                return Instance.RoomProgs.Values.Where(x => x.ID >= start && x.ID <= end).Cast<T>().ToList();
            }
            return null;
        }

        public bool AddOrUpdateScriptObject<T>(T script, bool isNew)
        {
            try
            {
                if (typeof(T) == typeof(MobProg))
                {
                    MobProg mp = script as MobProg;
                    if (!DatabaseManager.SaveScriptToWorldDatabase<MobProg>(mp, isNew))
                    {
                        Game.LogMessage($"ERROR: Failed to save MobProg {mp.ID} to World Database", LogLevel.Error);
                        return false;
                    }
                    if (isNew)
                    {
                        if (!Instance.MobProgs.TryAdd(mp.ID, mp))
                        {
                            Game.LogMessage($"ERROR: Failed to add MobProg {mp.ID} to MobProg Manager", LogLevel.Error);
                            return false;
                        }
                    }
                    else
                    {
                        if (!Instance.MobProgs.TryGetValue(mp.ID, out var existingMobProg))
                        {
                            Game.LogMessage($"ERROR: MobProg {mp.ID} not found in MobProg Manager for update", LogLevel.Error);
                            return false;
                        }
                        if (!Instance.MobProgs.TryUpdate(mp.ID, mp, existingMobProg))
                        {
                            Game.LogMessage($"ERROR: Failed to update MobProg {mp.ID} in MobProg Manager due to a value mismatch", LogLevel.Error);
                            return false;
                        }
                    }
                    return true;
                }
                if (typeof(T) == typeof(RoomProg))
                {
                    RoomProg rp = script as RoomProg;
                    if (!DatabaseManager.SaveScriptToWorldDatabase<RoomProg>(rp, isNew))
                    {
                        Game.LogMessage($"ERROR: Failed to save RoomProg {rp.ID} to World Database", LogLevel.Error);
                        return false;
                    }
                    if (isNew)
                    {
                        if (!Instance.RoomProgs.TryAdd(rp.ID, rp))
                        {
                            Game.LogMessage($"ERROR: Failed to add RoomProg {rp.ID} to RoomProg Manager", LogLevel.Error);
                            return false;
                        }
                    }
                    else
                    {
                        if (!Instance.RoomProgs.TryGetValue(rp.ID, out var existingRoomProg))
                        {
                            Game.LogMessage($"ERROR: RoomProg {rp.ID} not found in RoomProg Manager for update", LogLevel.Error);
                            return false;
                        }
                        if (!Instance.RoomProgs.TryUpdate(rp.ID, rp, existingRoomProg))
                        {
                            Game.LogMessage($"ERROR: Failed to update RoomProg {rp.ID} in RoomProg Manager due to a value mismatch", LogLevel.Error);
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgManager.AddOrUpdateMobProg(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveScriptObject<T>(int id)
        {
            if (typeof(T) == typeof(MobProg))
            {
                if (Instance.MobProgs.ContainsKey(id))
                {
                    if (Instance.MobProgs.TryRemove(id, out var mp) && DatabaseManager.RemoveScriptObject<MobProg>(id))
                    {
                        mp.Dispose();
                        return true;
                    }
                    Game.LogMessage($"ERROR: Failed to remove MobProg {id} from MobProg Manager and/or World Database", LogLevel.Error);
                    return false;
                }
                Game.LogMessage($"ERROR: Error removing MobProg {id}: No such MobProg in MobProg Manager", LogLevel.Error);
                return false;
            }
            if (typeof(T) == typeof(RoomProg))
            {
                if (Instance.RoomProgs.ContainsKey(id))
                {
                    if (Instance.RoomProgs.TryRemove(id, out var rp) && DatabaseManager.RemoveScriptObject<RoomProg>(id))
                    {
                        rp.Dispose();
                        return true;
                    }
                    Game.LogMessage($"ERROR: Failed to remove RoomProg {id} from RoomProg Manager and/or World Database", LogLevel.Error);
                    return false;
                }
                Game.LogMessage($"ERROR: Error removing RoomProg {id}: No such RoomProg in RoomProg Manager", LogLevel.Error);
                return false;
            }
            Game.LogMessage($"ERROR: RemoveScriptObject() called with unsupported object type: {typeof(T)}", LogLevel.Error);
            return false;
        }

        public bool LoadAllScripts()
        {
            if (!DatabaseManager.LoadAllScripts<MobProg>(out var allMobProgs) || allMobProgs == null)
            {
                return false;
            }
            if (!DatabaseManager.LoadAllScripts<RoomProg>(out var allRoomProgs) || allRoomProgs == null)
            {
                return false;
            }
            foreach(var mp in allMobProgs)
            {
                Instance.MobProgs.AddOrUpdate(mp.Key, mp.Value, (k, v) => mp.Value);
            }
            foreach(var rp in allRoomProgs)
            {
                Instance.RoomProgs.AddOrUpdate(rp.Key, rp.Value, (k, v) => rp.Value);
            }
            return true;
        }
    }
}
