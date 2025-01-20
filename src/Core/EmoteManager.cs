using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class EmoteManager
    {
        private static EmoteManager instance = null;
        private ConcurrentDictionary<int, Emote> Emotes { get; set; }
        public int Count => Emotes.Count;

        private EmoteManager()
        {
            Emotes = new ConcurrentDictionary<int, Emote>();
        }

        public static EmoteManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EmoteManager();
                }
                return instance;
            }
        }

        public void SetEmoteLockState(int id, bool locked, Session session)
        {
            if (Instance.Emotes.ContainsKey(id))
            {
                Instance.Emotes[id].OLCLocked = locked;
                Instance.Emotes[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetEmoteLockState(int id, out Guid lockHolder)
        {
            if (Instance.Emotes.ContainsKey(id))
            {
                lockHolder = Instance.Emotes[id].LockHolder;
                return Instance.Emotes[id].OLCLocked;
            }
            lockHolder = Guid.Empty;
            return false;
        }

        public bool EmoteExists(string name)
        {
            return Instance.Emotes.Values.Any(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public bool EmoteExists(int id)
        {
            return Instance.Emotes.ContainsKey(id);
        }

        public List<Emote> GetEmote(int start, int end)
        {
            return end <= start ? null : Instance.Emotes.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public Emote GetEmote(int id)
        {
            return Instance.Emotes.ContainsKey(id) ? Instance.Emotes[id] : null;
        }

        public Emote GetEmote(string name)
        {
            return Instance.Emotes.Values.FirstOrDefault(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public List<Emote> GetEmote()
        {
            return Instance.Emotes.Values.ToList();
        }

        public bool AddOrUpdateEmote(Emote emote, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveEmoteToWorldDatabase(emote, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Emote {emote.Name} ({emote.ID}) to the World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Emotes.TryAdd(emote.ID, emote))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Emote {emote.Name} ({emote.ID}) to Emote Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Emotes.TryGetValue(emote.ID, out Emote existingEmote))
                    {
                        Game.LogMessage($"ERROR: Emote {emote.ID} not found in Emote Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Emotes.TryUpdate(emote.ID, emote, existingEmote))
                    {
                        Game.LogMessage($"ERROR: Failed to update Emote {emote.ID} is Emote Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in EmoteManager.AddOrUpdateEmote(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveEmote(int id)
        {
            if (Instance.Emotes.ContainsKey(id))
            {
                return Instance.Emotes.TryRemove(id, out _) && DatabaseManager.RemoveEmote(id);
            }
            Game.LogMessage($"ERROR: Error removing Emote '{id}': No such Emote in EmoteManager", LogLevel.Error);
            return false;
        }

        public void LoadAllEmotes(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllEmotes(out hasErr);
            if (!hasErr && result != null)
            {
                foreach (var emote in result)
                {
                    Instance.Emotes.AddOrUpdate(emote.Key, emote.Value, (k, v) => emote.Value);
                }
            }
        }
    }
}
