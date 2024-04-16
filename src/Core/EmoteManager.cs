using System;
using System.Collections.Generic;
using Etrea2.Entities;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class EmoteManager
    {
        private static EmoteManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Emote> _emotes { get; set; }

        private EmoteManager()
        {
            _emotes = new Dictionary<uint, Emote>();
        }

        internal static EmoteManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EmoteManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool EmoteExists(uint id)
        {
            lock(_lock)
            {
                return Instance._emotes.ContainsKey(id);
            }
        }

        internal bool EmoteExists(string emote)
        {
            lock(_lock)
            {
                return Instance._emotes.Values.Where(x => x.EmoteName.ToLower() == emote.ToLower()).Any();
            }
        }

        internal List<Emote> GetAllEmotes(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var result = from e in Instance._emotes.Values where Regex.Match(e.EmoteName, name, RegexOptions.IgnoreCase).Success select e;
                return result.ToList();
            }
            return Instance._emotes.Values.ToList();
        }

        internal bool AddEmote(Emote emote, ref Descriptor desc)
        {
            try
            {
                lock (_lock)
                {
                    Instance._emotes.Add(emote.ID, emote);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} added Emote {emote.ID} ({emote.EmoteName}) to EmoteManager", LogLevel.OLC, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding new Emote to EmoteManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveEmote(uint id, ref Descriptor desc)
        {
            try
            {
                lock (_lock)
                {
                    Instance._emotes.Remove(id);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} removed Emote {id} from EmoteManager", LogLevel.OLC, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Emote {id} from EmoteManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal Emote GetEmoteByID(uint id)
        {
            if (Instance._emotes.ContainsKey(id))
            {
                return Instance._emotes[id];
            }
            return null;
        }

        internal Emote GetEmoteByName(string name)
        {
            return (from Emote e in Instance._emotes.Values where Regex.Match(e.EmoteName, name, RegexOptions.IgnoreCase).Success select e).FirstOrDefault();
        }

        internal bool UpdateEmoteByID(uint id, ref Descriptor desc, Emote e)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._emotes.ContainsKey(id))
                    {
                        Instance._emotes.Remove(id);
                        Instance._emotes.Add(id, e);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} updated Emote {id} ({e.EmoteName}) in EmoteManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: EmoteManager does not contain an Emote with ID {id} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddEmote(e, ref desc);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Emote {id} ({e.EmoteName}) in EmoteManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal int GetEmoteCount()
        {
            return Instance._emotes.Count();
        }

        internal void LoadAllEmotes(out bool hasError)
        {
            var result = DatabaseManager.LoadAllEmotes(out hasError);
            if (!hasError && result != null)
            {
                Instance._emotes.Clear();
                Instance._emotes = result;
            }
        }
    }
}
