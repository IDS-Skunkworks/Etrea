using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class ItemManager
    {
        private static ItemManager instance = null;
        private ConcurrentDictionary<int, InventoryItem> Items { get; set; }
        public int Count => Instance.Items.Count;

        public ItemManager()
        {
            Items = new ConcurrentDictionary<int, InventoryItem>();
        }

        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemManager();
                }
                return instance;
            }
        }

        public void SetItemLockState(int id, bool locked, Session session)
        {
            if (Instance.Items.ContainsKey(id))
            {
                Instance.Items[id].OLCLocked = locked;
                Instance.Items[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetItemLockState(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.Items.ContainsKey(id))
            {
                lockHolder = Instance.Items[id].LockHolder;
                return Instance.Items[id].OLCLocked;
            }
            return false;
        }

        public dynamic GetItem(int id)
        {
            if (Instance.Items.ContainsKey(id))
            {
                var i = Instance.Items[id];
                switch (i.ItemType)
                {
                    case ItemType.Misc:
                        return i;

                    case ItemType.Weapon:
                        return (Weapon)i;

                    case ItemType.Ring:
                        return (Ring)i;

                    case ItemType.Armour:
                        return (Armour)i;

                    case ItemType.Scroll:
                        return (Scroll)i;

                    case ItemType.Consumable:
                        return (Consumable)i;
                }
            }
            return null;
        }

        public dynamic GetItem(string criteria)
        {
            var matches = GetItems(criteria);
            if (matches == null || matches.Count == 0)
            {
                return null;
            }
            var i = matches.First();
            switch (i.ItemType)
            {
                case ItemType.Misc:
                    return i;

                case ItemType.Weapon:
                    return (Weapon)i;

                case ItemType.Ring:
                    return (Ring)i;

                case ItemType.Armour:
                    return (Armour)i;

                case ItemType.Scroll:
                    return (Scroll)i;

                case ItemType.Consumable:
                    return (Consumable)i;
            }
            return null;
        }

        public List<InventoryItem> GetItems(string criteria)
        {
            return string.IsNullOrEmpty(criteria) ? null : Instance.Items.Values.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0
            || x.ShortDescription.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0 
            || x.LongDescription.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<InventoryItem> GetItem(int start, int end)
        {
            return end <= start ? null : Instance.Items.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public List<InventoryItem> GetItem()
        {
            return Instance.Items.Values.ToList();
        }

        public bool ItemExists(int id)
        {
            return Instance.Items.ContainsKey(id);
        }

        public bool AddOrUpdateItem(InventoryItem item, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveItemToWorldDatabase(item, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Item {item.Name} ({item.ID}) to World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Items.TryAdd(item.ID, item))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Item {item.Name} ({item.ID}) to Item Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Items.TryGetValue(item.ID, out InventoryItem existingItem))
                    {
                        Game.LogMessage($"ERROR: Item {item.ID} not found in Item Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Items.TryUpdate(item.ID, item, existingItem))
                    {
                        Game.LogMessage($"ERROR: Failed to update Item {item.ID} in Item Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in InventoryManager.AddOrUpdateItem(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveItem(int id)
        {
            if (Instance.Items.ContainsKey(id))
            {
                return Instance.Items.TryRemove(id, out _) && DatabaseManager.RemoveItem(id);
            }
            Game.LogMessage($"ERROR: Error removing Item with ID {id}, no such Item in ItemManager", LogLevel.Error);
            return false;
        }

        public void LoadAllItems(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllItems(out hasErr);
            if (!hasErr && result != null)
            {
                foreach(var item in result)
                {
                    Instance.Items.AddOrUpdate(item.Key, item.Value, (k, v) => item.Value);
                }
            }
        }
    }
}
