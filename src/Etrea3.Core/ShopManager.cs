using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class ShopManager
    {
        private static ShopManager instance = null;
        private ConcurrentDictionary<int, Shop> Shops { get; set; }
        public int Count => Shops.Count;

        private ShopManager()
        {
            Shops = new ConcurrentDictionary<int, Shop>();
        }

        public static ShopManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShopManager();
                }
                return instance;
            }
        }

        public void SetShopLockStatus(int id, bool locked, Session session)
        {
            if (Instance.Shops.ContainsKey(id))
            {
                Instance.Shops[id].OLCLocked = locked;
                Instance.Shops[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetShopLockedStatus(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.Shops.ContainsKey(id))
            {
                lockHolder = Instance.Shops[id].LockHolder;
                return Instance.Shops[id].OLCLocked;
            }
            return false;
        }

        public bool AddOrUpdateShop(Shop shop, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveShopToWorldDatabase(shop, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Shop {shop.ShopName} ({shop.ID}) to the World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Shops.TryAdd(shop.ID, shop))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Shop {shop.ShopName} ({shop.ID}) to Shop Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Shops.TryGetValue(shop.ID, out Shop existingShop))
                    {
                        Game.LogMessage($"ERROR: Shop {shop.ID} not found in Shop Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Shops.TryUpdate(shop.ID, shop, existingShop))
                    {
                        Game.LogMessage($"ERROR: Failed to update Shop {shop.ID} in Shop Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ShopManager.AddOrUpdateShop(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveShop(int id)
        {
            if (Instance.Shops.ContainsKey(id))
            {
                return Instance.Shops.TryRemove(id, out _) && DatabaseManager.RemoveShop(id);
            }
            Game.LogMessage($"ERROR: Error removing Shop with ID {id}, no such Shop in ShopManager", LogLevel.Error);
            return false;
        }

        public Shop GetShop(int id)
        {
            return Instance.Shops.ContainsKey(id) ? Instance.Shops[id] : null;
        }

        public List<Shop> GetShop(int start, int end)
        {
            return end <= start ? null : Instance.Shops.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public List<Shop> GetShop(string criteria)
        {
            return string.IsNullOrEmpty(criteria) ? null :
                Instance.Shops.Values.Where(x => x.ShopName.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<Shop> GetShop()
        {
            return Instance.Shops.Values.ToList();
        }

        public bool ShopExists(int id)
        {
            return Instance.Shops.ContainsKey(id);
        }

        public bool LoadAllShops()
        {
            if (!DatabaseManager.LoadAllShops(out var allShops) || allShops == null)
            {
                return false;
            }
            foreach (var shop in allShops)
            {
                Instance.Shops.AddOrUpdate(shop.Key, shop.Value, (k, v) => shop.Value);
            }
            return true;
        }

        public void RestockShops()
        {
            Task.Run(() =>
            {
                Parallel.ForEach(Instance.Shops, x =>
                {
                    x.Value.RestockShop();
                });
            });
        }
    }
}
