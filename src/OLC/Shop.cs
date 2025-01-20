using Etrea3.Core;
using System.Linq;
using System.Text;
using Etrea3.Objects;
using Microsoft.SqlServer.Server;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateShop(Session session)
        {
            Shop newShop = new Shop();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Shop ID: {newShop.ID}");
                sb.AppendLine($"Shop Name: {newShop.ShopName}");
                sb.AppendLine($"Starting Gold: {newShop.BaseGold}");
                sb.AppendLine($"Inventory: {newShop.BaseInventory.Count}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Gold{Constants.TabStop}{Constants.TabStop}4. Manage Inventory");
                sb.AppendLine($"5. Save Shop{Constants.TabStop}{Constants.TabStop}6. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That doesn't look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newShop.ID = GetValue<int>(session, "Enter Shop ID: ");
                        break;

                    case 2:
                        newShop.ShopName = GetValue<string>(session, "Enter Shop Name: ");
                        break;

                    case 3:
                        newShop.BaseGold = GetValue<ulong>(session, "Enter Starting Gold: ");
                        break;

                    case 4:
                        ManageShopInventory(session, ref newShop);
                        break;

                    case 5:
                        if (ValidateAsset(session, newShop, true, out _))
                        {
                            if (ShopManager.Instance.AddOrUpdateShop(newShop, true))
                            {
                                session.SendSystem($"%BGT%The new Shop has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added Shop {newShop.ShopName} ({newShop.ID})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%Failed to correctly save the new Shop!%PT%{Constants.NewLine}");
                                Game.LogMessage($"Player {session.Player.Name} attempted to add Shop {newShop.ShopName} ({newShop.ID}) but the attempt failed.", LogLevel.OLC);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%Failed to validate the Shop, it cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 6:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteShop(Session session)
        {
            while (true)
            {
                session.SendSystem($"%BRT%This is a permanent change to the Realms!%PT%{Constants.NewLine}");
                session.SendSystem($"Enter Shop ID or END to return: ");
                string input = session.Read();
                if (string.IsNullOrEmpty(input))
                {
                    session.SendSystem($"%BRT%That is not a valid Shop ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int value))
                {
                    session.SendSystem($"%BRT%That is not a valid Shop ID.%PT%{Constants.NewLine}");
                    continue;
                }
                var shop = ShopManager.Instance.GetShop(value);
                if (shop == null)
                {
                    session.SendSystem($"%BRT%No Shop with that ID could be found in Shop Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (shop.HasCustomers)
                {
                    session.SendSystem($"%BRT%The specified Shop currently has customers and cannot be removed.%PT%{Constants.NewLine}");
                    continue;
                }
                if (shop.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(shop.LockHolder);
                    var msg = lockingSession != null ? $"%BRT%The specified Shop is currently locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                        $"%BRT%The specified Shop is currently locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.SendSystem(msg);
                    continue;
                }
                if (NPCManager.Instance.GetShopNPCs(shop.ID).Count > 0)
                {
                    session.SendSystem($"%BRT%The specified Shop is currently attached to NPCs and cannot be removed.%PT%{Constants.NewLine}");
                    continue;
                }
                if (ShopManager.Instance.RemoveShop(shop.ID))
                {
                    session.SendSystem($"%BGT%The specified Shop has been successfully removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed Shop {shop.ShopName} ({shop.ID})", LogLevel.OLC);
                    return;
                }
                else
                {
                    session.SendSystem($"%BRT% Failed to remove the specified Shop.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Shop {shop.ShopName} ({shop.ID}) however the attempt failed", LogLevel.OLC);
                    continue;
                }
            }
        }

        private static void ChangeShop(Session session)
        {
            session.SendSystem($"Enter Shop ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int shopID))
            {
                session.SendSystem($"%BRT%That is not a valid Shop ID.%PT%{Constants.NewLine}");
                return;
            }
            if (!ShopManager.Instance.ShopExists(shopID))
            {
                session.SendSystem($"%BRT%No Shop with that ID could be found in Shop Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (ShopManager.Instance.GetShop(shopID).OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(ShopManager.Instance.GetShop(shopID).LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Shop is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Shop is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            ShopManager.Instance.SetShopLockStatus(shopID, true, session);
            var shop = Helpers.Clone(ShopManager.Instance.GetShop(shopID));
            StringBuilder sb = new StringBuilder();
            while (true)
            { 
                sb.Clear();
                sb.AppendLine($"Shop ID: {shop.ID}");
                sb.AppendLine($"Shop Name: {shop.ShopName}");
                sb.AppendLine($"Starting Gold: {shop.BaseGold}");
                sb.AppendLine($"Inventory: {shop.BaseInventory.Count}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name");
                sb.AppendLine($"2. Set Gold{Constants.TabStop}{Constants.TabStop}3. Manage Inventory");
                sb.AppendLine($"4. Save Shop{Constants.TabStop}{Constants.TabStop}5. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That doesn't look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        shop.ShopName = GetValue<string>(session, "Enter Shop Name: ");
                        break;

                    case 2:
                        shop.BaseGold = GetValue<ulong>(session, "Enter Starting Gold: ");
                        break;

                    case 3:
                        ManageShopInventory(session, ref shop);
                        break;

                    case 4:
                        if (ValidateAsset(session, shop, false, out _))
                        {
                            if (ShopManager.Instance.AddOrUpdateShop(shop, false))
                            {
                                session.SendSystem($"%BGT%Shop has been updated successfully.%PT%{Constants.NewLine}");
                                ShopManager.Instance.SetShopLockStatus(shopID, false, session);
                                Game.LogMessage($"Player {session.Player.Name} has updated Shop {shop.ID} ({shop.ShopName})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%Failed to update Shop.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Shop {shop.ID} ({shop.ShopName}) however the attempt failed", LogLevel.OLC);
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%Shop could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                            continue;
                        }
                        break;

                    case 5:
                        ShopManager.Instance.SetShopLockStatus(shopID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That doesn't look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageShopInventory(Session session, ref Shop shop)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (shop.BaseInventory.Count > 0)
                {
                    sb.AppendLine("Inventory:");
                    foreach (var (i, item) in from i in shop.BaseInventory
                                              let item = ItemManager.Instance.GetItem(i.Key)
                                              select (i, item))
                    {
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {i.Key}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Inventory: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        int itemID = GetValue<int>(session, "Enter Item ID: ");
                        InventoryItem item = ItemManager.Instance.GetItem(itemID);
                        if (item != null)
                        {
                            int amount = GetValue<int>(session, "Enter Amount: ");
                            if (amount == 0)
                            {
                                session.SendSystem($"%BRT%That isn't valid: only positive non-zero numbers or -1 are allowed!%PT%{Constants.NewLine}");
                                break;
                            }
                            shop.BaseInventory.AddOrUpdate(item.ID, amount, (k, v) => amount);
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (shop.BaseInventory.ContainsKey(itemID))
                        {
                            shop.BaseInventory.TryRemove(itemID, out _);
                        }
                        break;

                    case 3:
                        shop.BaseInventory.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not appear to be a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}