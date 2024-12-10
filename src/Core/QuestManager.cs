using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class QuestManager
    {
        private static QuestManager instance = null;
        public ConcurrentDictionary<int, Quest> Quests;
        public int Count => Quests.Count;

        private QuestManager()
        {
            Quests = new ConcurrentDictionary<int, Quest>();
        }

        public static QuestManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new QuestManager();
                }
                return instance;
            }
        }

        public void SetQuestLockState(int id, bool locked, Session session)
        {
            if (Instance.Quests.ContainsKey(id))
            {
                Instance.Quests[id].OLCLocked = locked;
                Instance.Quests[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetQuestLockState(int id, out Guid lockHolder)
        {
            if (Instance.Quests.ContainsKey(id))
            {
                lockHolder = Instance.Quests[id].LockHolder;
                return Instance.Quests[id].OLCLocked;
            }
            lockHolder = Guid.Empty;
            return false;
        }

        public Quest GetQuest(int id)
        {
            return Instance.Quests.ContainsKey(id) ? Instance.Quests[id] : null;
        }

        public Quest GetQuest(Guid id)
        {
            return Instance.Quests.Values.FirstOrDefault(x => x.QuestGUID == id);
        }

        public List<Quest> GetQuest(int start, int end)
        {
            return end < start ? null : Instance.Quests.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public List<Quest> GetQuest()
        {
            return Instance.Quests.Values.ToList();
        }

        public List<Quest> GetQuest(string criteria)
        {
            return Instance.Quests.Values.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public bool QuestExists(int id)
        {
            return Instance.Quests.ContainsKey(id);
        }

        public List<Quest> GetQuestsForZone(int zoneID)
        {
            return Instance.Quests.Values.Where(x => x.Zone == zoneID).ToList();
        }

        public bool AddOrUpdateQuest(Quest newQuest, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveQuestToWorldDatabase(newQuest, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Quest {newQuest.Name} ({newQuest.ID}) to the World Database", LogLevel.Error, true);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Quests.TryAdd(newQuest.ID, newQuest))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Quest {newQuest.Name} ({newQuest.ID}) to Quest Manager", LogLevel.Error, true);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Quests.TryGetValue(newQuest.ID, out Quest existingQuest))
                    {
                        Game.LogMessage($"ERROR: Quest {newQuest.ID} not found in Quest Manager for update", LogLevel.Error, true);
                        return false;
                    }
                    if (!Instance.Quests.TryUpdate(newQuest.ID, newQuest, existingQuest))
                    {
                        Game.LogMessage($"ERROR: Failed to update Quest {newQuest.ID} in Quest Manager due to a value mismatch", LogLevel.Error, true);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in QuestManager.AddOrUpdateQuest(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public bool RemoveQuest(int id)
        {
            if (Instance.Quests.ContainsKey(id))
            {
                return Instance.Quests.TryRemove(id, out _) && DatabaseManager.RemoveQuest(id);
            }
            Game.LogMessage($"ERROR: Error removing Quest with ID {id}, no such Quest in QuestManager", LogLevel.Error, true);
            return false;
        }

        public void LoadAllQuests(out bool hasErr)
        {
            hasErr = false;
            var results = DatabaseManager.LoadAllQuests(out hasErr);
            if (!hasErr && results != null)
            {
                foreach(var r in results)
                {
                    Instance.Quests.AddOrUpdate(r.Key, r.Value, (k, v) => r.Value);
                }
            }
        }
    }
}
