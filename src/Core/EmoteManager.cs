using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class EmoteManager
    {
        private static EmoteManager _instance = null;
        private static readonly object _lockObject = new object();
        private Dictionary<uint, Emote> emotes { get; set; }

        private EmoteManager()
        {
            emotes = new Dictionary<uint, Emote>();
        }

        internal static EmoteManager Instance
        {
            get
            {
                lock( _lockObject)
                {
                    if( _instance  == null )
                    {
                        _instance = new EmoteManager();
                    }
                    return _instance;
                }
            }
        }

        internal List<Emote> GetAllEmotes(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var result = from e in emotes.Values where Regex.Match(e.EmoteName, name, RegexOptions.IgnoreCase).Success select e;
                return result.ToList();
            }
            return emotes.Values.ToList();
        }

        internal bool AddEmote(uint id, Emote emote)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.emotes.Add(id, emote);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding new Emote to EmoteManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveEmote(uint id)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.emotes.Remove(id);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing Emote from EmoteManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal Emote GetEmoteByID(uint id)
        {
            if(Instance.emotes.ContainsKey(id))
            {
                return Instance.emotes[id];
            }
            return null;
        }

        internal Emote GetEmoteByName(string name)
        {
            return (from Emote e in Instance.emotes.Values where Regex.Match(e.EmoteName, name, RegexOptions.IgnoreCase).Success select e).FirstOrDefault();
        }

        internal bool UpdateEmoteByID(uint id, ref Descriptor desc, Emote e)
        {
            try
            {
                if(Instance.emotes.ContainsKey(id))
                {
                    lock (_lockObject)
                    {
                        Instance.emotes.Remove(id);
                        Instance.emotes.Add(id, e);
                        Game.LogMessage($"INFO: Player {desc.Player.Name} updated Emote {e.EmoteName} in Emote Manager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: Emote Manager does not contain an Emote with ID {id} so an attempt will be made to add it", LogLevel.Warning, true);
                    return AddEmote(id, e);
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Emote {e.EmoteName}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool EmoteExists(uint id)
        {
            return Instance.emotes.ContainsKey(id);
        }

        internal bool EmoteExists(string emoteName)
        {
            return (from Emote e in Instance.emotes.Values where Regex.Match(e.EmoteName, emoteName, RegexOptions.IgnoreCase).Success select e).Count() > 0;
        }

        internal int GetEmoteCount()
        {
            return Instance.emotes.Count();
        }

        internal void LoadAllEmotes(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllEmotes(out hasErr);
            if(!hasErr && result != null && result.Count > 0)
            {
                Instance.emotes.Clear();
                Instance.emotes = result;
            }
        }
    }
}
