using System;
using System.Collections.Generic;
using Etrea2.Entities;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class ItemManager
    {
        private static ItemManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, InventoryItem> _items { get; set; }

        private ItemManager()
        {
            _items = new Dictionary<uint, InventoryItem>();
        }

        internal static ItemManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ItemManager();
                    }
                    return _instance;
                }
            }
        }

        internal InventoryItem GetItemByID(uint id)
        {
            lock (_lock)
            {
                if (Instance._items.ContainsKey(id))
                {
                    return Instance._items[id];
                }
            }
            return null;
        }

        internal List<InventoryItem> GetItemByIDRange(uint start, uint end)
        {
            lock (_lock)
            {
                return Instance._items.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
            }
        }

        internal InventoryItem GetItemByName(string name)
        {
            try
            {
                lock ( _lock)
                {
                    var retval = (from InventoryItem i in Instance._items.Values where Regex.IsMatch(i.Name, name, RegexOptions.IgnoreCase) select i).FirstOrDefault();
                    return retval;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemManager.GetItemByName(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal List<InventoryItem> GetItemByNameOrDescription(string input)
        {
            try
            {
                lock (_lock)
                {
                    var retval = (from InventoryItem i in Instance._items.Values
                                  where Regex.IsMatch(i.Name, input, RegexOptions.IgnoreCase) ||
                                  Regex.IsMatch(i.ShortDescription, input, RegexOptions.IgnoreCase) ||
                                  Regex.IsMatch(i.LongDescription, input, RegexOptions.IgnoreCase) select i).ToList();
                    return retval;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemManager.GetItemByNameOrDescription(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal bool AddItem(ref Descriptor desc, InventoryItem i)
        {
            try
            {
                lock ( _lock)
                {
                    Instance._items.Add(i.ID, i);
                    Game.LogMessage($"OLC: {desc.Player.Name} has added Item {i.Name} ({i.ID}) to ItemManager", LogLevel.OLC, true);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: {desc.Player.Name} encountered an error adding Item {i.Name} ({i.ID}) to ItemManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateItem(uint id, ref Descriptor desc, InventoryItem i)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._items.ContainsKey(id))
                    {
                        Instance._items.Remove(id);
                        Instance._items.Add(id, i);
                        Game.LogMessage($"OLC: {desc.Player.Name} updated Item {i.Name} ({i.ID}) in ItemManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: ItemManager does not contain an Item with ID {id} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddItem(ref desc, i);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Item {i.ID} ({i.Name}) in ItemManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveItem(ref Descriptor desc, uint id)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._items.ContainsKey(id))
                    {
                        Instance._items.Remove(id);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} removed Item {id} from ItemManager", LogLevel.OLC, true);
                        return true;
                    }
                    Game.LogMessage($"OLC: Player {desc.Player.Name} was unable to remove Item {id} from ItemManager, the ID does not exist", LogLevel.OLC, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Item {id} from ItemManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool ItemExists(uint id)
        {
            return Instance._items.ContainsKey(id);
        }

        internal void LoadAllItems(out bool hasError)
        {
            var result = DatabaseManager.LoadAllItems(out hasError);
            if (!hasError && result != null)
            {
                Instance._items.Clear();
                Instance._items = result;
            }
        }

        internal int GetItemCount()
        {
            lock (_lock)
            {
                return Instance._items.Count;
            }
        }
    }
}
