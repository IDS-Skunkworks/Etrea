using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class SpellManager
    {
        private static SpellManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<string, Spell> Spells { get; set; }

        private SpellManager()
        {
            Spells = new Dictionary<string, Spell>();
        }

        internal static SpellManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SpellManager();
                    }
                    return _instance;
                }
            }
        }

        internal List<Spell> GetAllSpells()
        {
            lock (_lock)
            {
                return Instance.Spells.Values.ToList();
            }
        }

        internal bool AddSpell(ref Descriptor desc, Spell spell)
        {
            try
            {
                lock (_lock)
                {
                    Instance.Spells.Add(spell.SpellName, spell);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} added Spell '{spell.SpellName}' to SpellManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding Spell '{spell.SpellName}' to SpellManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateSpell(ref Descriptor desc, Spell spell)
        {
            try
            {
                lock(_lock)
                {
                    if (Instance.Spells.ContainsKey(spell.SpellName))
                    {
                        Instance.Spells.Remove(spell.SpellName);
                        Instance.Spells.Add(spell.SpellName, spell);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} has updated Spell '{spell.SpellName}' in SpellManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: Player {desc.Player.Name} tried to update Spell '{spell.SpellName}' in SpellManager, but it could not be found and will be added instead", LogLevel.OLC, true);
                        bool OK = AddSpell(ref desc, spell);
                        return OK;
                    }
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Spell '{spell.SpellName}' in SpellManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveSpell(ref Descriptor desc, Spell spell)
        {
            try
            {
                lock(_lock)
                {
                    Instance.Spells.Remove(spell.SpellName);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} has removed Spell '{spell.SpellName}' from SpellManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Spell '{spell.SpellName}' from SpellManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal Spell GetSpell(string spellName)
        {
            lock (_lock)
            {
                if (Instance.Spells.Count > 0)
                {
                    return Instance.Spells.Values.Where(x => Regex.IsMatch(x.SpellName, spellName, RegexOptions.IgnoreCase)).FirstOrDefault();
                }
                return null;
            }
        }

        internal List<Spell> GetSpells(string spellName)
        {
            lock( _lock)
            {
                return Instance.Spells.Values.Where(x => Regex.IsMatch(x.SpellName, spellName, RegexOptions.IgnoreCase) || Regex.IsMatch(x.Description, spellName, RegexOptions.IgnoreCase)).ToList();
            }
        }

        internal void LoadAllSpells(out bool hasError)
        {
            var result = DatabaseManager.LoadAllSpells(out hasError);
            if (!hasError && result != null)
            {
                Instance.Spells.Clear();
                Instance.Spells = result;
            }
        }

        internal bool SpellExists(string spellName)
        {
            lock (_lock)
            {
                return Instance.Spells.Values.Where(x => Regex.IsMatch(x.SpellName, spellName, RegexOptions.IgnoreCase)).Any();
            }
        }

        internal int GetSpellCount()
        {
            lock (_lock)
            {
                return Instance.Spells.Count;
            }
        }
    }
}
