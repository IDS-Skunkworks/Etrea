using Etrea3.Core;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateNode(Session session)
        {
            ResourceNode newNode = new ResourceNode();
            StringBuilder sb = new StringBuilder();
            while(true)
            {
                sb.Clear();
                sb.AppendLine($"Vein ID: {newNode.ID}{Constants.TabStop}{Constants.TabStop}Name: {newNode.Name}");
                sb.AppendLine($"Vein Type: {newNode.VeinType}");
                if (newNode.CanFind.Count > 0)
                {
                    sb.AppendLine($"Can Find:");
                    foreach(var i in newNode.CanFind)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{item.Name} (ID: {item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Item (ID: {item.ID})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"Can Find: Nothing");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Vein Type{Constants.TabStop}4. Manage Findable Items");
                sb.AppendLine($"5. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}6. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                string input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newNode.ID = GetValue<int>(session, "Enter Node ID: ");
                        break;

                    case 2:
                        newNode.Name = GetValue<string>(session, "Enter Node Name");
                        break;

                    case 3:
                        newNode.VeinType = GetEnumValue<ResourceVeinType>(session, "Enter Vein Type: ");
                        break;

                    case 4:
                        ManageFindableItems(session, ref newNode);
                        break;

                    case 5:
                        if (ValidateAsset(session, newNode, true, out _))
                        {
                            if (NodeManager.Instance.AddOrUpdateNode(newNode, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Resource Node: {newNode.Name} ({newNode.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The new Resource Node has been created successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to create new Resource Node {newNode.Name} ({newNode.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%Failed to save the new Resource Node.%PT%{Constants.NewLine}");
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Resource Node could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 6:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteNode(Session session)
        {
            while (true)
            {
                session.SendSystem($"%BRT%This is a permanent change to the Realms!%PT%{Constants.NewLine}");
                session.SendSystem($"Enter Node ID or END to return: ");
                string input = session.Read();
                if (string.IsNullOrEmpty(input))
                {
                    session.SendSystem($"%BRT%Sorry, that isn't a valid Resource Node ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int nodeID))
                {
                    session.SendSystem($"%BRT%Sorry, that isn't a valid Resource Node ID.%PT%{Constants.NewLine}");
                    continue;
                }
                var node = NodeManager.Instance.GetNode(nodeID);
                if (node == null)
                {
                    session.SendSystem($"%BRT%No Resource Node with that ID could be found in Node Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (node.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(node.LockHolder);
                    var msg = lockingSession != null ? $"%BRT%That Resource Node is currently locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                        $"%BRT%That Resource Node is currently locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.SendSystem(msg);
                    continue;
                }
                if (NodeManager.Instance.RemoveNode(node.ID))
                {
                    session.SendSystem($"%BGT%The specified Resource Node has been successfully removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed Resource Node {node.Name} ({node.ID})", LogLevel.OLC);
                    return;
                }
                else
                {
                    session.SendSystem($"%BRT%The specified Resource Node could not be removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Resource Node {node.Name} ({node.ID}) however the attempt failed", LogLevel.OLC);
                    continue;
                }
            }
        }

        private static void ChangeNode(Session session)
        {
            var nodeID = GetValue<int>(session, "Enter Node ID: ");
            if (!NodeManager.Instance.NodeExists(nodeID))
            {
                session.SendSystem($"%BRT%No Resource Node with that ID could be found in Node Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (NodeManager.Instance.GetNode(nodeID).OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(NodeManager.Instance.GetNode(nodeID).LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Resource Node is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Resource Node is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            NodeManager.Instance.SetNodeLockState(nodeID, true, session);
            var node = Helpers.Clone(NodeManager.Instance.GetNode(nodeID));
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Vein ID: {node.ID}{Constants.TabStop}{Constants.TabStop}Name: {node.Name}");
                sb.AppendLine($"Vein Type: {node.VeinType}");
                if (node.CanFind.Count > 0)
                {
                    sb.AppendLine($"Can Find:");
                    foreach (var i in node.CanFind)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{item.Name} (ID: {item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Item (ID: {item.ID})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"Can Find: Nothing");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name");
                sb.AppendLine($"2. Set Vein Type{Constants.TabStop}3. Manage Findable Items");
                sb.AppendLine($"4. Save{Constants.TabStop}{Constants.TabStop}5. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                string input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        node.Name = GetValue<string>(session, "Enter Node Name");
                        break;

                    case 2:
                        node.VeinType = GetEnumValue<ResourceVeinType>(session, "Enter Vein Type: ");
                        break;

                    case 3:
                        ManageFindableItems(session, ref node);
                        break;

                    case 4:
                        if (ValidateAsset(session, node, true, out _))
                        {
                            if (NodeManager.Instance.AddOrUpdateNode(node, true))
                            {
                                NodeManager.Instance.SetNodeLockState(nodeID, false, session);
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Resource Node: {node.Name} ({node.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The new Resource Node has been created successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to create new Resource Node {node.Name} ({node.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%Failed to save the new Resource Node.%PT%{Constants.NewLine}");
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Resource Node could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 5:
                        NodeManager.Instance.SetNodeLockState(nodeID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageFindableItems(Session session, ref ResourceNode node)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (node.CanFind.Count > 0)
                {
                    sb.AppendLine("Findable Items:");
                    foreach(var i in node.CanFind)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Findable Items: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                string input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var itemID = GetValue<int>(session, "Enter Item ID: ");
                        var item = ItemManager.Instance.GetItem(itemID);
                        if (item != null)
                        {
                            node.CanFind.TryAdd(item.ID, true);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        node.CanFind.TryRemove(itemID, out _);
                        break;

                    case 3:
                        node.CanFind.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}