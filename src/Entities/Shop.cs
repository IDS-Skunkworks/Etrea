using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Etrea2.Core;
using System.Text;

namespace Etrea2.Entities
{
    internal class Shop
    {
        [JsonProperty]
        internal uint ID { get; set; }
        [JsonProperty]
        internal string ShopName { get; set; }
        [JsonProperty]
        internal Alignment ShopAlignment { get; set; }
        [JsonProperty]
        internal Dictionary<uint, int> BaseInventoryItems { get; set; }
        internal Dictionary<uint, int> InventoryItems { get; set; }
        [JsonProperty]
        internal uint BaseGold { get; set; }
        [JsonProperty]
        internal uint CurrentGold { get; set; }

        internal Shop()
        {
            BaseInventoryItems = new Dictionary<uint, int>();
            InventoryItems = new Dictionary<uint, int>();
            ID = 0;
            ShopName = "New Shop";
            ShopAlignment = Alignment.Neutral;
        }

        internal Shop ShallowCopy()
        {
            var s = (Shop)this.MemberwiseClone();
            return s;
        }

        internal bool HasItemInInventory(uint itemID)
        {
            return InventoryItems.ContainsKey(itemID) && (InventoryItems[itemID] > 0 || InventoryItems[itemID] == -1);
        }

        internal void RestockShop()
        {
            foreach(var item in BaseInventoryItems)
            {
                if (item.Value != -1)
                {
                    if (InventoryItems.ContainsKey(item.Key) && InventoryItems[item.Key] < BaseInventoryItems[item.Key])
                    {
                        InventoryItems[item.Key] = BaseInventoryItems[item.Key];
                    }
                    if (!InventoryItems.ContainsKey(item.Key))
                    {
                        InventoryItems.Add(item.Key, item.Value);
                    }
                }
                if (!InventoryItems.ContainsKey(item.Key) && item.Value == -1)
                {
                    InventoryItems.Add(item.Key, item.Value);
                }
            }
            List<uint> itemsToRemove = new List<uint>();
            foreach(var item in InventoryItems)
            {
                if (item.Value == 0)
                {
                    if (!BaseInventoryItems.ContainsKey(item.Key))
                    {
                        itemsToRemove.Add(item.Key);
                    }
                }
                if (item.Value == -1)
                {
                    if (!BaseInventoryItems.ContainsKey(item.Key))
                    {
                        itemsToRemove.Add(item.Key);
                    }
                }
            }
            if (itemsToRemove.Count > 0)
            {
                foreach(var item in itemsToRemove)
                {
                    InventoryItems.Remove(item);
                }
            }
            if (CurrentGold < BaseGold)
            {
                CurrentGold = BaseGold;
            }
        }

        internal void BuyItem(ref Descriptor desc, string input)
        {
            // player is buying an item from the shop
            if (InventoryItems != null && InventoryItems.Count > 0)
            {
                var availableItemIDs = InventoryItems.Where(x => x.Value == -1 || x.Value > 0).Select(x => x.Key).ToList();
                List<InventoryItem> availableItems = new List<InventoryItem>();
                if (availableItemIDs.Count > 0)
                {
                    foreach(var i in availableItemIDs)
                    {
                        var item = ItemManager.Instance.GetItemByID(i);
                        if (item != null)
                        {
                            availableItems.Add(item);
                        }
                    }
                    var buyItem = availableItems.Where(x => Regex.IsMatch(x.Name, input, RegexOptions.IgnoreCase)).FirstOrDefault();
                    if (buyItem != null)
                    {
                        var purchasePrice = Helpers.GetNewPurchasePrice(ref desc, buyItem.BaseValue);
                        if (ShopAlignment != Alignment.Neutral)
                        {
                            if (ShopAlignment != desc.Player.Alignment)
                            {
                                var priceMod = Convert.ToUInt32(buyItem.BaseValue * 0.1);
                                purchasePrice += priceMod;
                            }
                            if (ShopAlignment == desc.Player.Alignment)
                            {
                                var priceMod = Convert.ToUInt32(buyItem.BaseValue * 0.1);
                                purchasePrice = purchasePrice - priceMod <= 0 ? 1 : purchasePrice - priceMod;
                            }
                        }
                        if (desc.Player.HasSkill("Mercenary"))
                        {
                            var mercMod = Convert.ToUInt32(buyItem.BaseValue * 0.05);
                            purchasePrice = purchasePrice - mercMod <= 0 ? 1 : purchasePrice - mercMod;
                        }
                        if (desc.Player.Gold >= purchasePrice)
                        {
                            desc.Send($"The shopkeeper pockets your {purchasePrice:N0} gold and hands over {buyItem.ShortDescription}{Constants.NewLine}");
                            desc.Player.Inventory.Add(buyItem);
                            desc.Player.Gold -= purchasePrice;
                            CurrentGold += purchasePrice;
                            Game.LogMessage($"SHOP: {desc.Player.Name} has purchased {buyItem.Name} ({buyItem.ID}) from Shop {ShopName} ({ID}) in Room {desc.Player.CurrentRoom} for {purchasePrice} gold", LogLevel.Shop, true);
                            if (InventoryItems[buyItem.ID] > 0 && InventoryItems[buyItem.ID] - 1 <= 0)
                            {
                                InventoryItems[buyItem.ID] = 0;
                            }
                            else
                            {
                                if (InventoryItems[buyItem.ID] != -1)
                                {
                                    InventoryItems[buyItem.ID]--;
                                }
                            }
                        }
                    }
                    else
                    {
                        Game.LogMessage($"DEBUG: Player {desc.Player.Name} tried to buy '{input}' at Shop {ID} ({ShopName}) but no matching item was found in ItemManager", LogLevel.Debug, true);
                        desc.Send($"No such item could be found...{Constants.NewLine}");
                    }
                }
            }
        }

        internal void SellItem(ref Descriptor desc, InventoryItem item)
        {
            // player is selling an item to the shop
            var price = Helpers.GetNewSalePrice(ref desc, item.BaseValue);
            if (ShopAlignment != Alignment.Neutral)
            {
                if (ShopAlignment != desc.Player.Alignment)
                {
                    var priceMod = Convert.ToUInt32(item.BaseValue * 0.1);
                    price = price - priceMod <= 0 ? 1 : price - priceMod;
                }
                if (ShopAlignment == desc.Player.Alignment)
                {
                    var priceMod = Convert.ToUInt32(item.BaseValue * 0.05);
                    price += priceMod;
                }
            }
            if (desc.Player.HasSkill("Mercenary"))
            {
                var mercMod = Convert.ToUInt32(item.BaseValue * 0.05);
                price += mercMod;
            }
            if (CurrentGold >= price)
            {
                desc.Send($"You hand over {item.ShortDescription} and pocket the {price:N0} gold from the shopkeeper{Constants.NewLine}");
                CurrentGold -= price;
                desc.Player.Gold += price;
                desc.Player.Inventory.Remove(item);
                Game.LogMessage($"SHOP: {desc.Player.Name} sold {item.Name} ({item.ID}) to Shop {ShopName} ({ID}) in Room {desc.Player.CurrentRoom} for {price} gold", LogLevel.Shop, true);
                if (!InventoryItems.ContainsKey(item.ID))
                {
                    InventoryItems.Add(item.ID, 1);
                }
                else
                {
                    if (InventoryItems[item.ID] != -1)
                    {
                        InventoryItems[item.ID]++;
                    }
                }
            }
            else
            {
                desc.Send($"\"I'd normally pay about {price:N0} gold for that, but I can't afford that right now, sorry.\"{Constants.NewLine}");
            }
        }

        internal void AppraiseItemForSale(ref Descriptor desc, InventoryItem item)
        {
            var price = Helpers.GetNewSalePrice(ref desc, item.BaseValue);
            if (ShopAlignment != Alignment.Neutral)
            {
                if (ShopAlignment != desc.Player.Alignment)
                {
                    var priceMod = Convert.ToUInt32(item.BaseValue * 0.1);
                    price = price - priceMod <= 0 ? 1 : price - priceMod;
                }
                else
                {
                    var priceMod = Convert.ToUInt32(item.BaseValue * 0.05);
                    price += priceMod;
                }
            }
            if (desc.Player.HasSkill("Mercenary"))
            {
                var mercMod = Convert.ToUInt32(item.BaseValue * 0.05);
                price += mercMod;
            }
            desc.Send($"The shopkeepr smiles and says, \"I'd say that's worth about {price:N0} gold.\"{ Constants.NewLine}");
        }

        internal void ListShopInventory(ref Descriptor desc, string criteria = null)
        {
            StringBuilder sb = new StringBuilder();
            if (InventoryItems != null && InventoryItems.Count > 0)
            {
                var availableItems = InventoryItems.Where(x => x.Value > 0 || x.Value == -1).ToList();
                if (availableItems.Count > 0)
                {
                    bool matchingItem = false;
                    sb.AppendLine("The shopkeeper gives you a smile and shows you their wares...");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Item");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        foreach (var i in availableItems)
                        {
                            var item = ItemManager.Instance.GetItemByID(i.Key);
                            if (item != null)
                            {
                                if (Regex.IsMatch(item.Name, criteria, RegexOptions.IgnoreCase))
                                {
                                    var purchasePrice = Helpers.GetNewPurchasePrice(ref desc, item.BaseValue);
                                    if (ShopAlignment != Alignment.Neutral)
                                    {
                                        switch (ShopAlignment == desc.Player.Alignment)
                                        {
                                            case true:
                                                var priceMod = Convert.ToUInt32(item.BaseValue * 0.1);
                                                purchasePrice = purchasePrice - priceMod <= 0 ? 1 : purchasePrice - priceMod;
                                                break;

                                            case false:
                                                priceMod = Convert.ToUInt32(item.BaseValue * 0.1);
                                                purchasePrice += priceMod;
                                                break;
                                        }
                                    }
                                    sb.AppendLine($"|| {purchasePrice}{Constants.TabStop}{Constants.TabStop}|| {(i.Value == -1 ? "Unlimited" : i.Value.ToString())} x {item.Name}, {item.ShortDescription}");
                                    matchingItem = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var i in availableItems)
                        {
                            var item = ItemManager.Instance.GetItemByID(i.Key);
                            if (item != null)
                            {
                                var purchasePrice = Helpers.GetNewPurchasePrice(ref desc, item.BaseValue);
                                if (ShopAlignment != Alignment.Neutral)
                                {
                                    switch (ShopAlignment == desc.Player.Alignment)
                                    {
                                        case true:
                                            var priceMod = Convert.ToUInt32(item.BaseValue * 0.1);
                                            purchasePrice = purchasePrice - priceMod <= 0 ? 1 : purchasePrice - priceMod;
                                            break;

                                        case false:
                                            priceMod = Convert.ToUInt32(item.BaseValue * 0.1);
                                            purchasePrice += priceMod;
                                            break;
                                    }
                                }
                                sb.AppendLine($"|| {purchasePrice}{Constants.TabStop}{Constants.TabStop}|| {(i.Value == -1 ? "Unlimited" : i.Value.ToString())} x {item.Name}, {item.ShortDescription}");
                                matchingItem = true;
                            }
                        }
                    }
                    if (!matchingItem)
                    {
                        sb.AppendLine("|| Nothing that looks like what you're after");
                    }
                }
                else
                {
                    sb.AppendLine("\"I'm sorry,\" the shopkeeper says, \"I don't have anything in stock right now.\"");
                }
            }
            else
            {
                sb.AppendLine("Alas the shopkeeper has nothing for sale.");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }
    }
}
