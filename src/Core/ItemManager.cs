using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class ItemManager
    {
        private static ItemManager _instance = null;
        private static readonly object _lockObject = new object();
        private Dictionary<uint, InventoryItem> items { get; set; }

        private ItemManager()
        {
            items = new Dictionary<uint, InventoryItem>();
        }

        internal static ItemManager Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new ItemManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool AddItem(uint id, InventoryItem i)
        {
            try
            {
                lock(_lockObject)
                {
                    Instance.items.Add(id, i);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error adding item '{i.Name}' with ID {id}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal InventoryItem GetItemByID(uint id)
        {
            if(Instance.items.ContainsKey(id))
            {
                return Instance.items[id];
            }
            return null;
        }

        internal InventoryItem GetItemByName(string name)
        {
            return (from InventoryItem item in Instance.items.Values where Regex.Match(item.Name, name, RegexOptions.IgnoreCase).Success select item).FirstOrDefault();
        }

        internal List<InventoryItem> GetItemByNameOrDescription(string n)
        {
            return (from InventoryItem item in Instance.items.Values
                    where Regex.Match(item.Name, n, RegexOptions.IgnoreCase).Success ||
                    Regex.Match(item.ShortDescription, n, RegexOptions.IgnoreCase).Success || Regex.Match(item.LongDescription, n, RegexOptions.IgnoreCase).Success
                    select item).ToList();
        }

        internal bool UpdateItemByID(uint id, ref Descriptor desc, InventoryItem item)
        {
            try
            {
                if(Instance.items.ContainsKey(id))
                {
                    lock(_lockObject)
                    {
                        Instance.items.Remove(id);
                        Instance.items.Add(id, item);
                        Game.LogMessage($"Player {desc.Player.Name} updated item with ID {id} in the Item Manager.", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"Item Manager does not contain an item with ID {id}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance.items.Add(id, item);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Player {desc.Player.Name} unable to update Item ID {id} due to an exception: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveItemByID(uint id, ref Descriptor desc)
        {
            try
            {
                if(Instance.items.ContainsKey(id))
                {
                    lock(_lockObject)
                    {
                        Instance.items.Remove(id);
                    }
                    Game.LogMessage($"Player {desc.Player.Name} removed item ID {id} from the Item Manager", LogLevel.Info, true);
                    return true;
                }
                Game.LogMessage($"Player {desc.Player.Name} was unable to remove item ID {id} from the Item Manager, the ID does not exist", LogLevel.Warning, true);
                return false;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Player {desc.Player.Name} was unable to remove item ID {id} from the Item Manager, Exception: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool ItemExists(uint id)
        {
            return Instance.items.ContainsKey(id);
        }

        internal int GetItemCount()
        {
            return Instance.items.Count;
        }

        internal void LoadAllItems(out bool hasError)
        {
            var result = DatabaseManager.LoadAllItems(out hasError);
            if(!hasError && result != null && result.Count > 0)
            {
                Instance.items.Clear();
                Instance.items = result;
            }
        }
    }
}
