using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class QuestManager
    {
        private static QuestManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Quest> Quests { get; set; }

        private QuestManager()
        {
            Quests = new Dictionary<uint, Quest>();
        }

        internal static QuestManager Instance
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new QuestManager();
                    }
                    return _instance;
                }
            }
        }

        internal int GetQuestCount()
        {
            lock(_lock)
            {
                return Instance.Quests.Count;
            }
        }

        internal List<Quest> GetAllQuests()
        {
            lock(_lock)
            {
                return Instance.Quests.Values.ToList();
            }
        }

        internal bool QuestExists(uint id)
        {
            lock(_lock)
            {
                return Instance.Quests.ContainsKey(id);
            }
        }

        internal bool QuestExists(Guid guid)
        {
            lock(_lock)
            {
                return Instance.Quests.Values.Any(x => x.QuestGUID == guid);
            }
        }

        internal List<Quest> GetQuestsByIDRange(uint start, uint end)
        {
            lock(_lock)
            {
                return Instance.Quests.Values.Where(x => x.QuestID >= start && x.QuestID <= end).ToList();
            }
        }

        internal List<Quest> GetQuestsByNameOrDescription(string term)
        {
            lock (_lock)
            {
                return Instance.Quests.Values.Where(x => Regex.IsMatch(x.QuestName, term, RegexOptions.IgnoreCase) || Regex.IsMatch(x.QuestText, term, RegexOptions.IgnoreCase)).ToList();
            }
        }

        internal Quest GetQuest(uint id)
        {
            lock (_lock)
            {
                return Instance.Quests.ContainsKey(id) ? Instance.Quests[id] : null;
            }
        }

        internal Quest GetQuest(Guid id)
        {
            lock (_lock)
            {
                return Instance.Quests.Values.Where(x => x.QuestGUID == id).FirstOrDefault();
            }
        }

        internal void LoadAllQuests(out bool hasError)
        {
            var result = DatabaseManager.LoadAllQuests(out hasError);
            if (!hasError && result != null)
            {
                Instance.Quests.Clear();
                Instance.Quests = result;
            }
        }

        internal List<Quest> GetQuestsForZone(uint zone)
        {
            lock (_lock)
            {
                return Instance.Quests.Values.Where(x => x.QuestZone == zone).ToList();
            }
        }

        internal bool RemoveQuest(ref Descriptor desc, Guid id, string name)
        {
            var q = GetQuest(id);
            if (q != null)
            {
                return RemoveQuest(ref desc, q.QuestID, q.QuestName);
            }
            Game.LogMessage($"ERROR: Player {desc.Player.Name} tried to remove Quest {id} from QuestManager but the ID was not found", LogLevel.Error, true);
            return false;
        }

        internal bool RemoveQuest(ref Descriptor desc, uint id, string name)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance.Quests.ContainsKey(id))
                    {
                        Instance.Quests.Remove(id);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} removed Quest {id} ({name}) from QuestManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: Player {desc.Player.Name} tried to remove Quest {id} ({name}) from QuestManager but the ID was not found", LogLevel.OLC, true);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Quest {id} ({name}) from QuestManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateQuest(ref Descriptor desc, Quest q)
        {
            try
            {
                lock(_lock)
                {
                    if (Instance.Quests.ContainsKey(q.QuestID))
                    {
                        Instance.Quests.Remove(q.QuestID);
                        Instance.Quests.Add(q.QuestID, q);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} updated Quest {q.QuestID} ({q.QuestName}) in QuestManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: QuestManager does not contain Quest {q.QuestID} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddQuest(ref desc, q);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Quest {q.QuestID} ({q.QuestName}) in QuestManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddQuest(ref Descriptor desc, Quest q)
        {
            try
            {
                lock(_lock)
                {
                    Instance.Quests.Add(q.QuestID, q);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} added Quest {q.QuestID} ({q.QuestName}) to QuestManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding Quest {q.QuestID} ({q.QuestName}) to QuestManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
    }
}
