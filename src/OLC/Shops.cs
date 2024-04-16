using Etrea2.Core;
using Etrea2.Entities;
using System.Collections.Generic;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create Shop
        private static void CreateNewShop(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Shops can be placed in Rooms and allow players to buy and sell items for gold.");
            Shop newShop = new Shop
            {
                BaseInventoryItems = new Dictionary<uint, int>(),
                InventoryItems = new Dictionary<uint, int>(),
                ShopAlignment = Alignment.Neutral
            };
            desc.Send(sb.ToString());
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Shop ID: {newShop.ID}");
                sb.AppendLine($"Shop Name: {newShop.ShopName}");
                sb.AppendLine($"Alignment: {newShop.ShopAlignment}");
                sb.AppendLine($"Starting Gold: {newShop.BaseGold:N0}");
                if (newShop.BaseInventoryItems != null && newShop.BaseInventoryItems.Count > 0)
                {
                    sb.AppendLine("Shop Inventory:");
                    foreach (var i in newShop.BaseInventoryItems)
                    {
                        var item = ItemManager.Instance.GetItemByID(i.Key);
                        if (item != null)
                        {
                            switch(i.Value == -1)
                            {
                                case true:
                                    sb.AppendLine($"Unlimited x {item.Name} for {item.BaseValue} gold");
                                    break;

                                case false:
                                    sb.AppendLine($"{i.Value} x {item.Name} for {item.BaseValue} gold");
                                    break;
                            }
                        }
                        else
                        {
                            sb.AppendLine($"[{i.Key}] - Unknown item");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Shop Inventory: Nothing");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Shop ID{Constants.TabStop}{Constants.TabStop}2. Set Shop Name");
                sb.AppendLine($"3. Add Item{Constants.TabStop}{Constants.TabStop}4. Remove Item");
                sb.AppendLine($"5. Set Alignment{Constants.TabStop}6. Set Starting Gold");
                sb.AppendLine($"7. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}8. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 8)
                    {
                        switch (result)
                        {
                            case 1:
                                newShop.ID = GetAssetUintValue(ref desc, "Enter Shop ID: ");
                                break;

                            case 2:
                                newShop.ShopName = GetAssetStringValue(ref desc, "Enter Shop Name: ");
                                break;

                            case 3:
                                var id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if (id > 0 && ItemManager.Instance.ItemExists(id))
                                {
                                    var cnt = GetAssetIntegerValue(ref desc, "Amount to sell: ");
                                    if (cnt == -1 || cnt > 0 && !newShop.BaseInventoryItems.ContainsKey(id))
                                    {
                                        newShop.BaseInventoryItems.Add(id, cnt);
                                    }
                                }
                                break;

                            case 4:
                                var iid = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if (newShop.BaseInventoryItems.ContainsKey(iid))
                                {
                                    newShop.BaseInventoryItems.Remove(iid);
                                }
                                break;

                            case 5:
                                newShop.ShopAlignment = GetAssetEnumValue<Alignment>(ref desc, "Enter Alignment: ");
                                break;

                            case 6:
                                newShop.BaseGold = GetAssetUintValue(ref desc, "Enter starting gold: ");
                                break;

                            case 7:
                                if (ValidateShopAsset(ref desc, ref newShop, true))
                                {
                                    if (DatabaseManager.AddNewShop(ref desc, ref newShop))
                                    {
                                        if (ShopManager.Instance.AddNewShop(ref desc, newShop))
                                        {
                                            ShopManager.Instance.GetShop(newShop.ID).RestockShop();
                                            desc.Send($"Successfully added Shop to World database and Shop Manager{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Could not add Shop to Shop Manager, it may not be available until the World is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Could not add Shop to the World database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 8:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Edit Shop
        private static void EditExistingShop(ref Descriptor desc)
        {
            desc.Send("Enter ID of Shop to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint sid))
            {
                if (ShopManager.Instance.ShopExists(sid))
                {
                    var s = ShopManager.Instance.GetShop(sid).ShallowCopy();
                    if (s != null)
                    {
                        bool okToReturn = false;
                        StringBuilder sb = new StringBuilder();
                        while (!okToReturn)
                        {
                            sb.Clear();
                            sb.AppendLine($"Shop ID: {s.ID}");
                            sb.AppendLine($"Shop Name: {s.ShopName}");
                            sb.AppendLine($"Alignment: {s.ShopAlignment}");
                            sb.AppendLine($"Starting Gold: {s.BaseGold:N0}");
                            if (s.BaseInventoryItems != null && s.BaseInventoryItems.Count > 0)
                            {
                                sb.AppendLine("Shop Inventory:");
                                foreach (var i in s.BaseInventoryItems)
                                {
                                    var item = ItemManager.Instance.GetItemByID(i.Key);
                                    if (item != null)
                                    {
                                        switch (i.Value == -1)
                                        {
                                            case true:
                                                sb.AppendLine($"Unlimited x {item.Name} for {item.BaseValue} gold");
                                                break;

                                            case false:
                                                sb.AppendLine($"{i.Value} x {item.Name} for {item.BaseValue} gold");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        sb.AppendLine($"[{i.Key}] - Unknown item");
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendLine("Shop Inventory: Nothing");
                            }
                            sb.AppendLine();
                            sb.AppendLine("Options:");
                            sb.AppendLine("1. Set Shop Name");
                            sb.AppendLine($"2. Add Item{Constants.TabStop}3. Remove Item");
                            sb.AppendLine($"4. Set Alignment{Constants.TabStop}5. Set Starting Gold");
                            sb.AppendLine($"6. Save {Constants.TabStop}{Constants.TabStop}7. Exit");
                            sb.Append("Selection: ");
                            desc.Send(sb.ToString());
                            var option = desc.Read().Trim();
                            if (Helpers.ValidateInput(option) && uint.TryParse(option, out uint opt))
                            {
                                if (opt >= 1 && opt <= 7)
                                {
                                    switch (opt)
                                    {
                                        case 1:
                                            s.ShopName = GetAssetStringValue(ref desc, "Enter Shop Name: ");
                                            break;

                                        case 2:
                                            var id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                            if (id > 0 && ItemManager.Instance.ItemExists(id))
                                            {
                                                var cnt = GetAssetIntegerValue(ref desc, "Amount to sell: ");
                                                if (cnt == -1 || cnt > 0 && !s.BaseInventoryItems.ContainsKey(id))
                                                {
                                                    s.BaseInventoryItems.Add(id, cnt);
                                                }
                                            }
                                            break;

                                        case 3:
                                            var iid = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                            if (s.BaseInventoryItems.ContainsKey(iid))
                                            {
                                                s.BaseInventoryItems.Remove(iid);
                                            }
                                            break;

                                        case 4:
                                            s.ShopAlignment = GetAssetEnumValue<Alignment>(ref desc, "Enter Alignment: ");
                                            break;

                                        case 5:
                                            s.BaseGold = GetAssetUintValue(ref desc, "Enter starting gold: ");
                                            break;

                                        case 6:
                                            if (ValidateShopAsset(ref desc, ref s, false))
                                            {
                                                if (DatabaseManager.UpdateExistingShop(ref desc, s))
                                                {
                                                    if (ShopManager.Instance.UpdateShop(ref desc, s))
                                                    {
                                                        ShopManager.Instance.GetShop(s.ID).RestockShop();
                                                        desc.Send($"Shop updated in World database and Shop Manager{Constants.NewLine}");
                                                        okToReturn = true;
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Failed to update Shop in Shop Manager{Constants.NewLine}");
                                                    }
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update Shop in World database{Constants.NewLine}");
                                                }
                                            }
                                            break;

                                        case 7:
                                            okToReturn = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"No Shop with that ID could be found in Shop Manager{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"No Shop with that ID could be found in Shop Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Delete Shop
        private static void DeleteShop(ref Descriptor desc)
        {
            desc.Send("Enter ID of Shop to remove or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint sid))
            {
                if (ShopManager.Instance.ShopExists(sid))
                {
                    var shopRooms = RoomManager.Instance.GetRoomsWithSpecifiedShop(sid);
                    if (shopRooms == null || shopRooms.Count == 0)
                    {
                        var sName = ShopManager.Instance.GetShop(sid).ShopName;
                        if (ShopManager.Instance.RemoveShop(ref desc, sid, sName))
                        {
                            if (DatabaseManager.DeleteShop(ref desc, sid))
                            {
                                desc.Send($"Shop successfully removed from Shop Manager and World database{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"Unable to remove shop from World database{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"Unable to remove Shop from Shop Manager{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"The specified shop is still in use on one or more rooms{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"No Shop with that ID could be found in Shop Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Validation Functions
        private static bool ValidateShopAsset(ref Descriptor desc, ref Shop s, bool isNewShop)
        {
            if (string.IsNullOrEmpty(s.ShopName))
            {
                desc.Send($"The Shop must have a name{Constants.NewLine}");
                return false;
            }
            if (s.ID == 0)
            {
                desc.Send($"Shop ID cannot be 0{Constants.NewLine}");
                return false;
            }
            if (s.BaseGold == 0)
            {
                desc.Send($"The Shop must have a non-zero amount of gold{Constants.NewLine}");
                return false;
            }
            if (isNewShop && ShopManager.Instance.ShopExists(s.ID))
            {
                desc.Send($"Specified Shop ID is already in use{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}