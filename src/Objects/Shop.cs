using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Etrea3.Objects
{
    [Serializable]
    public class Shop
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string ShopName { get; set; } = "A magic disappearing shop";
        [JsonProperty]
        public ConcurrentDictionary<int, int> BaseInventory { get; set; } = new ConcurrentDictionary<int, int>();
        [JsonProperty]
        public ConcurrentDictionary<int, int> CurrentInventory { get; set; } = new ConcurrentDictionary<int, int>();
        [JsonProperty]
        public ulong BaseGold { get; set; }
        [JsonProperty]
        public ulong CurrentGold { get; set; }
        [JsonIgnore]
        public bool HasCustomers
        {
            get
            {
                return SessionManager.Instance.GetShopCustomers(this);
            }
        }
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;

        public Shop()
        {

        }

        public bool HasItemInInventory(int id)
        {
            if (CurrentInventory.ContainsKey(id))
            {
                return CurrentInventory[id] == -1 || CurrentInventory[id] > 0;
            }
            return false;
        }

        public void ShowInventory(Session session, string criteria)
        {
            if (CurrentInventory != null)
            {
                var availableItems = CurrentInventory.Where(x => ItemManager.Instance.ItemExists(x.Key) && (x.Value == -1 || x.Value > 0)).ToList();
                if (availableItems !=  null && availableItems.Count > 0)
                {
                    bool matchingItems = false;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"{ShopName} says \"Certainly! This is what I have in stock!\"");
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                    sb.AppendLine($"%BYT%|| Price{Constants.TabStop}|| Item%PT%");
                    sb.AppendLine($"%BYT%||==============||{new string('=', 61)}%PT%");
                    foreach (var item in availableItems)
                    {
                        var i = ItemManager.Instance.GetItem(item.Key);
                        if (Regex.IsMatch(i.Name, criteria, RegexOptions.IgnoreCase) || Regex.IsMatch(i.ShortDescription, criteria, RegexOptions.IgnoreCase) || Regex.IsMatch(i.LongDescription, criteria, RegexOptions.IgnoreCase))
                        {
                            matchingItems = true;
                            var purchasePrice = Helpers.GetPurchasePrice(session, i.BaseValue);
                            if (purchasePrice.ToString("N0").Length >= 5)
                            {
                                sb.AppendLine($"%BYT%||%PT% {purchasePrice:N0}{Constants.TabStop}%BYT%||%PT% {(item.Value == -1 ? "Unlimited" : item.Value.ToString())} x {i.Name}, {i.ShortDescription}");
                            }
                            else
                            {
                                sb.AppendLine($"%BYT%||%PT% {purchasePrice:N0}{Constants.TabStop}{Constants.TabStop}%BYT%||%PT% {(item.Value == -1 ? "Unlimited" : item.Value.ToString())} x {i.Name}, {i.ShortDescription}");
                            }
                        }
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                    availableItems = null;
                    if (matchingItems)
                    {
                        session.Send(sb.ToString());
                        return;
                    }
                }
            }
            session.Send($"{ShopName} says \"Sorry, I don't have anything in stock right now.\"{Constants.NewLine}");
        }

        public void AppraiseItem(Session session, InventoryItem item)
        {
            var price = Helpers.GetSalePrice(session, item.BaseValue);
            session.Send($"%BYT%{ShopName} gives {item.ShortDescription} a quick look. \"I'd say that's worth about {price:N0} gold.\"{Constants.NewLine}%PT%");
        }

        public void PlayerBuyItem(Session session, string itemName)
        {
            var shopItems = CurrentInventory.Where(x => ItemManager.Instance.ItemExists(x.Key) && (x.Value == -1 || x.Value > 0)).ToList();
            if (shopItems.Count == 0)
            {
                session.Send($"%BYT%\"I'm sorry,\" {ShopName} says, \"but I don't have anything like that in stock right now.\"%PT%{Constants.NewLine}");
                return;
            }
            dynamic foundItem = null;
            foreach (var shopItem in shopItems)
            {
                var item = ItemManager.Instance.GetItem(shopItem.Key);
                if (item.Name.IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foundItem = item;
                    break;
                }
            }
            if (foundItem == null)
            {
                session.Send($"\"I'm sorry,\" {ShopName} says, \"but I don't have anything like that in stock right now.\"{Constants.NewLine}");
                return;
            }
            var purchasePrice = Helpers.GetPurchasePrice(session, foundItem.BaseValue);
            if (session.Player.Gold >= (ulong)Math.Abs(purchasePrice))
            {
                session.Send($"You hand over {purchasePrice:N0} gold to {ShopName}.{Constants.NewLine}");
                session.Player.AdjustGold(purchasePrice * -1, true);
                CurrentGold += (ulong)purchasePrice;
                session.Player.AddItemToInventory(foundItem.ID);
                if (CurrentInventory.TryGetValue(foundItem.ID, out int stock) && stock != -1)
                {
                    CurrentInventory[foundItem.ID]--;
                }
                session.Send($"%BYT%{ShopName} smiles broadly. \"I hope you enjoy your purchase!\"%PT%{Constants.NewLine}");
                Game.LogMessage($"SHOP: Player {session.Player.Name} purchased Item {foundItem.ID} from Shop {ID} for {purchasePrice:N0} gold", LogLevel.Shop, true);
                var npc = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).NPCsInRoom.Where(x => x.ShopID == ID).FirstOrDefault();
                if (npc != null && npc.MobProgs.Count > 0)
                {
                    foreach (var mp in npc.MobProgs.Keys)
                    {
                        var mobProg = MobProgManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerPurchaseItem, new { mob = npc.ID.ToString(), player = session.ID.ToString(), itemID = foundItem.ID, price = purchasePrice });
                        }
                    }
                }
            }
            else
            {
                session.Send($"%BRT%{ShopName} tuts. \"It looks like you're short of coin!\"%PT%{Constants.NewLine}");
            }
        }

        public void PlayerSellItem(Session session, InventoryItem item)
        {
            var salePrice = Helpers.GetSalePrice(session, item.BaseValue);
            if (CurrentGold >= (ulong)salePrice)
            {
                if (CurrentInventory.TryGetValue(item.ID, out var stock) && stock != -1)
                {
                    CurrentInventory[item.ID]++;
                }
                else
                {
                    CurrentInventory.TryAdd(item.ID, 1);
                }
                CurrentGold -= (ulong)salePrice;
                session.Player.AdjustGold(salePrice, true);
                session.Player.RemoveItemFromInventory(item);
                session.Send($"You hand {item.ShortDescription} to {ShopName} and pocket the {salePrice:N0} gold!{Constants.NewLine}");
                Game.LogMessage($"SHOP: Player {session.Player.Name} sold {item.ID} to Shop {ID} for {salePrice:N0} gold", LogLevel.Shop, true);
                var npc = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).NPCsInRoom.Where(x => x.ShopID == ID).FirstOrDefault();
                if (npc != null && npc.MobProgs.Count > 0)
                {
                    foreach(var mp in npc.MobProgs.Keys)
                    {
                        var mobProg = MobProgManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerSellItem, new { mob = npc.ID.ToString(), player = session.ID.ToString(), itemID = item.ID, price = salePrice });
                        }
                    }
                }
            }
            else
            {
                session.Send($"\"I'm sorry,\" {ShopName} says, \"but I can't afford that right now.\"{Constants.NewLine}");
            }
        }

        public void RestockShop()
        {
            Game.LogMessage($"INFO: Restocking Shop {ShopName}", LogLevel.Info, true);
            foreach(var item in BaseInventory)
            {
                if (item.Value != -1)
                {
                    CurrentInventory.AddOrUpdate(item.Key, item.Value, (key, currentVal) => currentVal < item.Value ? item.Value : currentVal);
                }
                else
                {
                    CurrentInventory.TryAdd(item.Key, item.Value);
                }
            }
            foreach(var item in CurrentInventory)
            {
                if ((item.Value == 0 || item.Value == -1) && !BaseInventory.ContainsKey(item.Key))
                {
                    CurrentInventory.TryRemove(item.Key, out _);
                }
            }
            CurrentGold = Math.Max(CurrentGold, BaseGold);
        }
    }
}
