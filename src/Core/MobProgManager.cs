using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    internal class MobProgManager
    {
        private static MobProgManager instance = null;
        private ConcurrentDictionary<int, MobProg> MobProgs { get; set; }
        public int Count => MobProgs.Count;

        private MobProgManager()
        {
            MobProgs = new ConcurrentDictionary<int, MobProg>();
        }

        public static MobProgManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MobProgManager();
                }
                return instance;
            }
        }

        public void SetMobProgLockState(int id, bool locked, Session session)
        {
            if (Instance.MobProgs.ContainsKey(id))
            {
                Instance.MobProgs[id].OLCLocked = locked;
                Instance.MobProgs[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool MobProgExists(int id)
        {
            return Instance.MobProgs.ContainsKey(id);
        }

        public MobProg GetMobProg(int id)
        {
            return Instance.MobProgs.ContainsKey(id) ? Instance.MobProgs[id] : null;
        }

        public List<MobProg> GetMobProg()
        {
            return Instance.MobProgs.Values.ToList();
        }

        public List<MobProg> GetMobProg(int start, int end)
        {
            return end <= start ? null : Instance.MobProgs.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public bool AddOrUpdateMobProg(MobProg mobProg, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveMobProgToWorldDatabase(mobProg, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save MobProg {mobProg.ID} to World Dataase", LogLevel.Error, true);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.MobProgs.TryAdd(mobProg.ID, mobProg))
                    {
                        Game.LogMessage($"ERROR: Failed to add MobProg {mobProg.ID} to MobProg Manager", LogLevel.Error, true);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.MobProgs.TryGetValue(mobProg.ID, out var existingMobProg))
                    {
                        Game.LogMessage($"ERROR: MobProg {mobProg.ID} not found in MobProg Manager for update", LogLevel.Error, true);
                        return false;
                    }
                    if (!Instance.MobProgs.TryUpdate(mobProg.ID, mobProg, existingMobProg))
                    {
                        Game.LogMessage($"ERROR: Failed to update MobProg {mobProg.ID} in MobProg Manager due to a value mismatch", LogLevel.Error, true);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgManager.AddOrUpdateMobProg(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public bool RemoveMobProg(int id)
        {
            if (Instance.MobProgs.ContainsKey(id))
            {
                if (Instance.MobProgs.TryRemove(id, out var mp) && DatabaseManager.RemoveMobProg(id))
                {
                    mp.Dispose();
                    return true;
                }
                Game.LogMessage($"ERROR: Failed to remove MobProg {id} from MobProg Manager and/or World Database", LogLevel.Error, true);
                return false;
            }
            Game.LogMessage($"ERROR: Error removing MobProg {id}: No such MobProg in MobProg Manager", LogLevel.Error, true);
            return false;
        }

        public void LoadAllMobProgs(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllMobProgs(out hasErr);
            if (!hasErr && result != null)
            {
                foreach (var mobProg in result)
                {
                    Instance.MobProgs.AddOrUpdate(mobProg.Key, mobProg.Value, (k, v) => mobProg.Value);
                }
            }
        }
    }
}
