using Kingdoms_of_Etrea.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace Kingdoms_of_Etrea.Entities
{
    internal class Shop
    {
        [JsonProperty]
        internal uint ShopID { get; set; }
        [JsonProperty]
        internal string ShopName { get; set; }
        [JsonProperty]
        internal List<uint> InventoryItems { get; set; }

        public Shop()
        {
            InventoryItems = new List<uint>();
            ShopID = 0;
            ShopName = string.Empty;
        }

        internal bool HasItemInInventory(uint itemID)
        {
            return InventoryItems.Contains(itemID);
        }

        internal void BuyItem(ref Descriptor desc, ref string input)
        {
            if(InventoryItems != null && InventoryItems.Count > 0)
            {
                bool matchingItem = false;
                foreach(var id in InventoryItems)
                {
                    var item = ItemManager.Instance.GetItemByID(id);
                    if(item != null && Regex.Match(item.Name, input, RegexOptions.IgnoreCase).Success)
                    {
                        matchingItem = true;
                        if(desc.Player.Stats.Gold >= item.BaseValue)
                        {
                            desc.Player.Inventory.Add(item);
                            desc.Player.Stats.Gold -= item.BaseValue;
                            desc.Send($"You purchase {item.Name} from the shopkeeper who eagerly pockets your gold!{Constants.NewLine}");
                            var playersInRoom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom);
                            if(playersInRoom != null && playersInRoom.Count > 1)
                            {
                                foreach(var p in playersInRoom)
                                {
                                    if(!Regex.Match(desc.Player.Name, p.Player.Name, RegexOptions.IgnoreCase).Success)
                                    {
                                        var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} hands over a wedge of gold and buys {item.Name}{Constants.NewLine}" : $"Something hands over a wedge of gold and buys {item.Name}{Constants.NewLine}";
                                        p.Send(msg);
                                    }
                                }
                            }
                            break;
                        }
                        else
                        {
                            desc.Send($"You cannot affort that!{Constants.NewLine}");
                            break;
                        }
                    }
                }
                if(!matchingItem)
                {
                    desc.Send($"The shopkeeper doesn't have anything like that in stock...{Constants.NewLine}");
                }
            }
        }

        internal void ListShopInventory(ref Descriptor desc, string criteria = null)
        {
            StringBuilder sb = new StringBuilder();
            if(InventoryItems != null && InventoryItems.Count > 0)
            {
                bool matchingItem = false;
                sb.AppendLine("The shopkeeper gives you a smile and shows you their wares...");
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| Price{Constants.TabStop}|| Item");
                sb.AppendLine($"||==============||{new string('=', 61)}");
                foreach (var i in from item in InventoryItems
                                  let i = ItemManager.Instance.GetItemByID(item)
                                  where i != null && (string.IsNullOrEmpty(criteria) || Regex.Match(i.Name, criteria, RegexOptions.IgnoreCase).Success || Regex.Match(i.ShortDescription, criteria, RegexOptions.IgnoreCase).Success || Regex.Match(i.LongDescription, criteria, RegexOptions.IgnoreCase).Success)
                                  select i)
                {
                    sb.AppendLine($"|| {Helpers.GetNewPurchasePrice(ref desc, i.BaseValue)}{Constants.TabStop}{Constants.TabStop}|| {i.Name}, {i.ShortDescription}");
                    matchingItem = true;
                }

                if (!matchingItem)
                {
                    sb.AppendLine("|| Nothing that looks like what you're after");
                }
            }
            else
            {
                sb.AppendLine("|| Alas, the shopkeeper has nothing for sale");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }
    }
}
