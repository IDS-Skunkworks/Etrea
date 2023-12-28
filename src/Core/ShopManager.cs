using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class ShopManager
    {
        private static ShopManager _instance = null;
        private static readonly object _lockObject = new object();
        //private ILoggingProvider _loggingProvider;
        private Dictionary<uint, Shop> shops { get; set; }

        private ShopManager()
        {
            //_loggingProvider = loggingProvider;
            shops = new Dictionary<uint, Shop>();
        }

        internal static ShopManager Instance
        {
            get
            {
                lock(_lockObject)
                {
                    if(_instance == null)
                    {
                        _instance = new ShopManager();
                    }
                    return _instance;
                }
            }
        }

        internal Dictionary<uint, Shop> GetAllShops()
        {
            return Instance.shops;
        }

        internal bool AddNewShop(Shop s)
        {
            try
            {
                lock(_lockObject)
                {
                    _instance.shops.Add(s.ShopID, s);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error adding Shop ID {s.ShopName} ({s.ShopID}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateShop(ref Descriptor desc, Shop s)
        {
            try
            {
                if(Instance.shops.ContainsKey(s.ShopID))
                {
                    lock(_lockObject)
                    {
                        Instance.shops.Remove(s.ShopID);
                        Instance.shops.Add(s.ShopID, s);
                        Game.LogMessage($"Player {desc.Player.Name} updated Shop {s.ShopID} in Shop Manager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"Shop Manager does not contain a Shop with ID {s.ShopID}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance.shops.Add(s.ShopID, s);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error updating Shop {s.ShopID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal List<Shop> GetShopByIDRange(uint start, uint end)
        {
            var retval = shops.Values.Where(x => x.ShopID >= start && x.ShopID <= end).ToList();
            return retval;
        }

        internal List<Shop> GetShopsByName(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var result = from s in Instance.shops.Values where Regex.Match(s.ShopName, name, RegexOptions.IgnoreCase).Success select s;
                return result.ToList();
            }
            return Instance.shops.Values.ToList();
        }

        internal bool RemoveShop(uint id, string sname)
        {
            try
            {
                lock(_lockObject)
                {
                    _instance.shops.Remove(id);
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"Error removing Shop {sname} ({id}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal int GetShopCount()
        {
            return Instance.shops.Count;
        }

        internal Shop GetShop(uint id)
        {
            return Instance.shops.ContainsKey(id) ? Instance.shops[id] : null;
        }

        internal bool ShopExists(uint id)
        {
            return Instance.shops.ContainsKey(id);
        }

        internal void LoadAllShops(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllShops(out hasErr);
            if(!hasErr && result != null && result.Count > 0)
            {
                Instance.shops.Clear();
                Instance.shops = result;
            }
        }
    }
}
