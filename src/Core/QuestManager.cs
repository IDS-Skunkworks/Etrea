using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kingdoms_of_Etrea.Entities;

namespace Kingdoms_of_Etrea.Core
{
    internal class QuestManager
    {
        private static QuestManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Quest> quests { get; set; }

        private QuestManager()
        {
            quests = new Dictionary<uint, Quest>();
        }

        internal static QuestManager Instance
        {
            get
            {
                lock (_lock)
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
            return Instance.quests.Count;
        }

        internal Dictionary<uint, Quest> GetAllQuests()
        {
            return Instance.quests;
        }

        internal bool QuestExists(uint id)
        {
            return Instance.quests.ContainsKey(id);
        }

        internal bool QuestExists(Guid guid)
        {
            return Instance.quests.Values.Any(x => x.QuestGUID == guid);
        }

        internal List<Quest> GetQuestsByIDRange(uint start, uint end)
        {
            var retval = quests.Values.Where(x => x.QuestID >= start && x.QuestID <= end).ToList();
            return retval;
        }

        internal List<Quest> GetQuestByNameOrDescription(string criteria)
        {
            var retval = quests.Values.Where(x => Regex.Match(x.QuestName, criteria, RegexOptions.IgnoreCase).Success || Regex.Match(x.QuestText, criteria, RegexOptions.IgnoreCase).Success).ToList();
            return retval;
        }

        internal Quest GetQuest(uint id)
        {
            if(quests.ContainsKey(id))
            {
                return quests[id];
            }
            return null;
        }

        internal Quest GetQuest(Guid id)
        {
            var q = quests.Values.Where(x => x.QuestGUID == id).FirstOrDefault();
            return q;
        }

        internal Quest GetQuest(string name)
        {
            var q = quests.Values.Where(x => Regex.Match(x.QuestName, name, RegexOptions.IgnoreCase).Success).FirstOrDefault();
            return q;
        }

        internal void LoadAllQuests(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllQuests(out hasErr);
            if(!hasErr)
            {
                Instance.quests.Clear();
                Instance.quests = result;
            }
        }

        internal List<Quest> GetQuestsForZone(uint zoneId)
        {
            if(Instance.quests.Count > 0)
            {
                lock(_lock)
                {
                    var result = Instance.quests.Values.Where(x => x.QuestZone == zoneId).ToList();
                    return result;
                }
            }
            return null;
        }

        internal bool RemoveQuest(ref Descriptor desc, uint id)
        {
            try
            {
                if(Instance.quests.ContainsKey(id))
                {
                    lock(_lock)
                    {
                        Instance.quests.Remove(id);
                    }
                    Game.LogMessage($"INFO: Player {desc.Player} removed Quest {id} from QuestManager", LogLevel.Info, true);
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: Player {desc.Player} attempted to remove Quest {id} from QuestManager but this ID could not be found", LogLevel.Warning, true);
                    return false;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error removing Quest {id} from QuestManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveQuest(ref Descriptor desc, Guid id)
        {
            try
            {
                var q = Instance.quests.Values.Where(x => x.QuestGUID == id).FirstOrDefault();
                if(q != null)
                {
                    lock(_lock)
                    {
                        Instance.quests.Remove(q.QuestID);
                    }
                    Game.LogMessage($"INFO: Player {desc.Player} removed Quest {id} from QuestManager", LogLevel.Info, true);
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: Player {desc.Player} attempted to remove Quest {id} from QuestManager but this ID could not be found", LogLevel.Warning, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error removing Quest {id} from QuestManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateQuest(ref Descriptor desc, Quest q)
        {
            try
            {
                if(Instance.quests.ContainsKey(q.QuestID))
                {
                    lock(_lock)
                    {
                        Instance.quests.Remove(q.QuestID);
                        Instance.quests.Add(q.QuestID, q);
                        Game.LogMessage($"INFO: Player {desc.Player} updated Quest {q.QuestID} in QuestManager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: QuestManager does not contain a Quest with ID {q.QuestID}", LogLevel.Warning, true);
                    lock(_lock)
                    {
                        Instance.quests.Add(q.QuestID, q);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Quest {q.QuestID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool AddNewQuest(ref Descriptor desc, Quest q)
        {
            try
            {
                lock(_lock)
                {
                    Instance.quests.Add(q.QuestID, q);
                    Game.LogMessage($"INFO: Player {desc.Player} added Quest '{q.QuestName}' ({q.QuestID}) to QuestManager", LogLevel.Info, true);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error adding a new Quest to QuestManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
    }
}
