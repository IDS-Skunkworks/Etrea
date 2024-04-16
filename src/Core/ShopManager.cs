using System;
using System.Collections.Generic;
using System.Linq;
using Etrea2.Entities;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class ShopManager
    {
        private static ShopManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Shop> _shops { get; set; }

        private ShopManager()
        {
            _shops = new Dictionary<uint, Shop>();
        }

        internal static ShopManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ShopManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool AddNewShop(ref Descriptor desc, Shop s)
        {
            try
            {
                lock(_lock)
                {
                    Instance._shops.Add(s.ID, s);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} has added Shop {s.ID} ({s.ShopName}) to ShopManager", LogLevel.OLC, true);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding Shop ID {s.ID} ({s.ShopName}) to ShopManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateShop(ref Descriptor desc, Shop s)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._shops.ContainsKey(s.ID))
                    {
                        Instance._shops.Remove(s.ID);
                        Instance._shops.Add(s.ID, s);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} updated Shop {s.ID} ({s.ShopName}) in ShopManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: ShopManager does not contain a Shop with ID {s.ID} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddNewShop(ref desc, s);
                        return OK;
                    }
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Shop {s.ID} ({s.ShopName}) in ShopManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveShop(ref Descriptor desc, uint id, string name)
        {
            try
            {
                lock (_lock)
                {
                    Instance._shops.Remove(id);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} removed Shop {id} ({name}) from ShopManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Shop {id} ({name}) from ShopManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal List<Shop> GetShopByIDRange(uint start, uint end)
        {
            var retval = Instance._shops.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
            return retval;
        }

        internal List<Shop> GetAllShops()
        {
            lock (_lock)
            {
                return Instance._shops.Values.ToList();
            }
        }

        internal List<Shop> GetShopsByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var result = Instance._shops.Values.Where(x => Regex.IsMatch(x.ShopName, name, RegexOptions.IgnoreCase)).ToList();
                return result;
            }
            return Instance._shops.Values.ToList();
        }

        internal int GetShopCount()
        {
            return Instance._shops.Count;
        }

        internal Shop GetShop(uint id)
        {
            lock (_lock)
            {
                if (Instance._shops.ContainsKey(id))
                {
                    return Instance._shops[id];
                }
            }
            return null;
        }

        internal bool ShopExists(uint id)
        {
            return Instance._shops.ContainsKey(id);
        }

        internal void LoadAllShops(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllShops(out hasErr);
            if (!hasErr && result != null)
            {
                Instance._shops.Clear();
                Instance._shops = result;
            }
        }
    }
}
