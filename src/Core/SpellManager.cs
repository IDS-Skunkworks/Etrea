using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class SpellManager
    {
        private static SpellManager instance = null;
        private ConcurrentDictionary<int, Spell> Spells;
        public int Count => Spells.Count;

        private SpellManager()
        {
            Spells = new ConcurrentDictionary<int, Spell>();
        }

        public static SpellManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpellManager();
                }
                return instance;
            }
        }

        public void SetSpellLockState(int id, bool locked, Session session)
        {
            if (Instance.Spells.ContainsKey(id))
            {
                Instance.Spells[id].OLCLocked = locked;
                Instance.Spells[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetSpellLockState(int id, out Guid lockHolder)
        {
            if (Instance.Spells.ContainsKey(id))
            {
                lockHolder = Instance.Spells[id].LockHolder;
                return Instance.Spells[id].OLCLocked;
            }
            lockHolder = Guid.Empty;
            return false;
        }

        public void LoadAllSpells(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllSpells(out hasErr);
            if (!hasErr && result != null)
            {
                Instance.Spells.Clear();
                foreach(var spell in result)
                {
                    Instance.Spells.TryAdd(spell.Key, spell.Value);
                }
            }
        }

        public Spell GetSpell(int id)
        {
            return Instance.Spells.ContainsKey(id) ? Instance.Spells[id] : null;
        }

        public List<Spell> GetSpell(int start, int end)
        {
            return end < start ? null : Instance.Spells.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public Spell GetSpell(string spellName)
        {
            return Instance.Spells.Values.FirstOrDefault(x => x.Name.IndexOf(spellName, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public List<Spell> GetSpell(string spellName, bool all)
        {
            return Instance.Spells.Values.Where(x => x.Name.IndexOf(spellName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<Spell> GetSpell()
        {
            return Instance.Spells.Values.ToList();
        }

        public List<Spell> GetSpell(ActorClass actorClass)
        {
            return Instance.Spells.Values.Where(x => x.AvailableToClass.HasFlag(actorClass)).ToList();
        }

        public List<Spell> GetSpell(SpellType spellType)
        {
            return Instance.Spells.Values.Where(x => x.SpellType == spellType).ToList();
        }

        public bool SpellExists(int spellID)
        {
            return Instance.Spells.ContainsKey(spellID);
        }

        public bool AddOrUpdateSpell(Spell spell, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveSpellToWorldDatabase(spell, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Spell {spell.Name} ({spell.ID}) to World Database", LogLevel.Error, true);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Spells.TryAdd(spell.ID, spell))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Spell {spell.Name} ({spell.ID}) to Spell Manager", LogLevel.Error, true);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Spells.TryGetValue(spell.ID, out Spell existingSpell))
                    {
                        Game.LogMessage($"ERROR: Spell {spell.ID} not found in Spell Manager for update", LogLevel.Error, true);
                        return false;
                    }
                    if (!Instance.Spells.TryUpdate(spell.ID, spell, existingSpell))
                    {
                        Game.LogMessage($"ERROR: Failed to update Spell {spell.ID} in Spell Manager due to a value mismatch", LogLevel.Error, true);
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SpellManager.AddOrUpdateSpell(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public bool RemoveSpell(int id)
        {
            if (Instance.Spells.ContainsKey(id))
            {
                return Instance.Spells.TryRemove(id, out _) && DatabaseManager.RemoveSpell(id);
            }
            Game.LogMessage($"ERROR: Error removing Spell with ID {id}, no such Spell in SpellManager", LogLevel.Error, true);
            return false;
        }
    }
}
