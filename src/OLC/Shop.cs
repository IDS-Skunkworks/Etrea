using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Entities;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Delete
        private static void DeleteShop(ref Descriptor desc)
        {
            desc.Send("Enter ID of Shop to remove: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint sid))
            {
                if(ShopManager.Instance.ShopExists(sid))
                {
                    var shopRooms = RoomManager.Instance.GetRoomsWithSpecifiedShop(sid);
                    if(shopRooms == null || shopRooms.Count == 0)
                    {
                        var sName = ShopManager.Instance.GetShop(sid).ShopName;
                        if(ShopManager.Instance.RemoveShop(sid, sName))
                        {
                            if(DatabaseManager.DeleteShop(ref desc, sid))
                            {
                                desc.Send($"Shop successfully removed from Shop Manager and World database{Constants.NewLine}");
                                Game.LogMessage($"INFO: Player {desc.Player.Name} has removed Shop {sName} (ID: {sid}) from the World Database", LogLevel.Info, true);
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

        #region Edit
        private static void EditExistingShop(ref Descriptor desc)
        {
            desc.Send("Enter ID of Shop to edit: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint sid))
            {
                var s = ShopManager.Instance.GetShop(sid);
                if(s != null)
                {
                    bool okToReturn = false;
                    StringBuilder sb = new StringBuilder();
                    while(!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Shop ID: {s.ShopID}");
                        sb.AppendLine($"Shop Name: {s.ShopName}");
                        if (s.InventoryItems != null && s.InventoryItems.Count > 0)
                        {
                            sb.AppendLine("Shop Inventory:");
                            foreach (var i in s.InventoryItems)
                            {
                                var item = ItemManager.Instance.GetItemByID(i);
                                if (item != null)
                                {
                                    sb.AppendLine($"[{item.Id}] {item.Name} for {item.BaseValue} gold");
                                }
                                else
                                {
                                    sb.AppendLine($"[{i}] - Unknown item");
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
                        sb.AppendLine("2. Add Item");
                        sb.AppendLine("3. Remove Item");
                        sb.AppendLine("4. Save Shop");
                        sb.AppendLine("5. Exit without saving");
                        sb.Append("Selection: ");
                        desc.Send(sb.ToString());
                        var option = desc.Read().Trim();
                        if(Helpers.ValidateInput(option) && uint.TryParse(option, out uint opt))
                        {
                            if(opt >= 1 && opt <= 5)
                            {
                                switch(opt)
                                {
                                    case 1:
                                        s.ShopName = GetAssetStringValue(ref desc, "Enter Shop Name: ");
                                        break;

                                    case 2:
                                        var id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        if(s.InventoryItems != null && !s.InventoryItems.Contains(id) && id != 0)
                                        {
                                            s.InventoryItems.Add(id);
                                        }
                                        break;

                                    case 3:
                                        var iid = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        if(s.InventoryItems != null && iid != 0 && s.InventoryItems.Contains(iid))
                                        {
                                            s.InventoryItems.Remove(iid);
                                        }
                                        break;

                                    case 4:
                                        if(ValidateShopAsset(ref desc, ref s, false))
                                        {
                                            if(DatabaseManager.UpdateExistingShop(ref desc, s))
                                            {
                                                if(ShopManager.Instance.UpdateShop(ref desc, s))
                                                {
                                                    desc.Send($"Shop updated in World database and Shop Manager{Constants.NewLine}");
                                                    Game.LogMessage($"INFO: Player {desc.Player.Name} has updated Shop {s.ShopName} (ID: {s.ShopID})", LogLevel.Info, true);
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

                                    case 5:
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
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Create
        private static void CreateNewShop(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Shops can be placed in Rooms and allow players to buy and sell items for gold.");
            sb.AppendLine("Items are added to a Shop's inventory by their Item ID. Items added to a shop don't need to exist, however items that don't exist won't be shown to players when they browse a shop's inventory.");
            Shop newShop = new Shop();
            desc.Send(sb.ToString());
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Shop ID: {newShop.ShopID}");
                sb.AppendLine($"Shop Name: {newShop.ShopName}");
                if(newShop.InventoryItems != null && newShop.InventoryItems.Count > 0)
                {
                    sb.AppendLine("Shop Inventory:");
                    foreach(var i in newShop.InventoryItems)
                    {
                        var item = ItemManager.Instance.GetItemByID(i);
                        if(item != null)
                        {
                            sb.AppendLine($"[{item.Id}] {item.Name} for {item.BaseValue} gold");
                        }
                        else
                        {
                            sb.AppendLine($"[{i}] - Unknown item");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Shop Inventory: Nothing");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Shop ID");
                sb.AppendLine("2. Set Shop Name");
                sb.AppendLine("3. Add Item");
                sb.AppendLine("4. Remove Item");
                sb.AppendLine("5. Save Shop");
                sb.AppendLine("6. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 6)
                    {
                        switch(result)
                        {
                            case 1:
                                newShop.ShopID = GetAssetUintValue(ref desc, "Enter Shop ID: ");
                                break;

                            case 2:
                                newShop.ShopName = GetAssetStringValue(ref desc, "Enter Shop Name: ");
                                break;

                            case 3:
                                var id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if(newShop.InventoryItems != null && !newShop.InventoryItems.Contains(id) && id != 0)
                                {
                                    newShop.InventoryItems.Add(id);
                                }
                                break;

                            case 4:
                                var iid = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if(newShop.InventoryItems != null && newShop.InventoryItems.Contains(iid) && iid != 0)
                                {
                                    newShop.InventoryItems.Remove(iid);
                                }
                                break;

                            case 5:
                                if(ValidateShopAsset(ref desc, ref newShop, true))
                                {
                                    if(DatabaseManager.AddNewShop(ref desc, ref newShop))
                                    {
                                        if(ShopManager.Instance.AddNewShop(newShop))
                                        {
                                            desc.Send($"Successfully added Shop to World database and Shop Manager{Constants.NewLine}");
                                            Game.LogMessage($"INFO: Player {desc.Player.Name} has added Shop {newShop.ShopName} (ID: {newShop.ShopID}) to the World Database", LogLevel.Info, true);
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

                            case 6:
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

        #region Functions
        private static bool ValidateShopAsset(ref Descriptor desc, ref Shop s, bool isNewShop)
        {
            if(string.IsNullOrEmpty(s.ShopName))
            {
                desc.Send($"The Shop must have a name{Constants.NewLine}");
                return false;
            }
            if(s.ShopID == 0)
            {
                desc.Send($"Shop ID cannot be 0{Constants.NewLine}");
                return false;
            }
            if(isNewShop && ShopManager.Instance.ShopExists(s.ShopID))
            {
                desc.Send($"Specified Shop ID is already in use{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}
