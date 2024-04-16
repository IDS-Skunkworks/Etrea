using Etrea2.Core;
using Etrea2.Entities;
using System.Collections.Generic;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create
        private static void CreateNewResourceNode(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("A Resource Node is a feature that can appear in Rooms that have the CAVE flag.");
            sb.AppendLine("Resource Nodes can be mined by players with the appropriate skill to gain resources for crafting.");
            desc.Send(sb.ToString());
            ResourceNode n = new ResourceNode
            {
                CanFind = new List<InventoryItem>()
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Node ID: {n.ID}");
                sb.AppendLine($"Node Name: {n.NodeName}");
                sb.AppendLine($"Appearance Chance: {n.AppearanceChance}");
                sb.AppendLine($"Can Find:");
                if (n.CanFind != null && n.CanFind.Count > 0)
                {
                    foreach (var cf in n.CanFind)
                    {
                        sb.AppendLine($"{cf.Name}");
                    }
                }
                else
                {
                    sb.AppendLine("Nothing");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Node ID{Constants.TabStop}{Constants.TabStop}2. Set Node Name");
                sb.AppendLine($"3. Set Appearance Chance{Constants.TabStop}4. Add Item to Node{Constants.TabStop}5. Remove Item from Node");
                sb.AppendLine($"6. Save{Constants.TabStop}7. Exit");
                sb.AppendLine("Selection:");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 7)
                    {
                        switch (option)
                        {
                            case 1:
                                n.ID = GetAssetUintValue(ref desc, "Enter Node ID: ");
                                break;

                            case 2:
                                n.NodeName = GetAssetStringValue(ref desc, "Enter Node Name: ");
                                break;

                            case 3:
                                n.AppearanceChance = GetAssetUintValue(ref desc, "Enter Appearance Chance: ");
                                break;

                            case 4:
                                var i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                var item = ItemManager.Instance.GetItemByID(i);
                                if (item != null)
                                {
                                    n.CanFind.Add(item);
                                }
                                else
                                {
                                    desc.Send($"No Item with that ID could be found.{Constants.NewLine}");
                                }
                                break;

                            case 5:
                                i = GetAssetUintValue(ref desc, "Enter ID of Item to remove: ");
                                item = ItemManager.Instance.GetItemByID(i);
                                if (item != null)
                                {
                                    if (n.CanFind.Contains(item))
                                    {
                                        n.CanFind.Remove(item);
                                    }
                                }
                                break;

                            case 6:
                                if (ValidateResourceNode(ref desc, ref n, true))
                                {
                                    if (DatabaseManager.AddNewResourceNode(ref desc, ref n))
                                    {
                                        if (NodeManager.Instance.AddNode(ref desc, n))
                                        {
                                            desc.Send($"Resource Node successfully added to NodeManager and World Database.{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add Resource Node to the NodeManager.{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to add Resource Node to the World Database.{Constants.NewLine}");
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
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Edit
        private static void EditExistingNode(ref Descriptor desc)
        {
            desc.Send($"Enter the ID of the Node to edit: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint nid))
            {
                if (NodeManager.Instance.NodeExists(nid))
                {
                    ResourceNode n = NodeManager.Instance.GetNode(nid).ShallowCopy();
                    bool okToReturn = false;
                    StringBuilder sb = new StringBuilder();
                    while (!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Node ID: {n.ID}");
                        sb.AppendLine($"Node Name: {n.NodeName}");
                        sb.AppendLine($"Appearance Chance: {n.AppearanceChance}");
                        sb.AppendLine($"Can Find:");
                        if (n.CanFind != null && n.CanFind.Count > 0)
                        {
                            foreach (var cf in n.CanFind)
                            {
                                sb.AppendLine($"{cf.Name}");
                            }
                        }
                        else
                        {
                            sb.AppendLine("Nothing");
                        }
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine("1. Set Node Name");
                        sb.AppendLine("2. Set Appearance Chance");
                        sb.AppendLine("3. Add Item to Node");
                        sb.AppendLine("4. Remove Item from Node");
                        sb.AppendLine("5. Save Node");
                        sb.AppendLine("6. Exit without Saving");
                        sb.AppendLine("Selection:");
                        desc.Send(sb.ToString());
                        input = desc.Read().Trim();
                        if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                        {
                            if (option >= 1 && option <= 6)
                            {
                                switch (option)
                                {
                                    case 1:
                                        n.NodeName = GetAssetStringValue(ref desc, "Enter Node Name: ");
                                        break;

                                    case 2:
                                        n.AppearanceChance = GetAssetUintValue(ref desc, "Enter Appearance Chance: ");
                                        break;

                                    case 3:
                                        var i = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        var item = ItemManager.Instance.GetItemByID(i);
                                        if (item != null)
                                        {
                                            n.CanFind.Add(item);
                                        }
                                        else
                                        {
                                            desc.Send($"No Item with that ID could be found.{Constants.NewLine}");
                                        }
                                        break;

                                    case 4:
                                        i = GetAssetUintValue(ref desc, "Enter Item ID to remove: ");
                                        item = ItemManager.Instance.GetItemByID(i);
                                        if (item != null)
                                        {
                                            if (n.CanFind.Contains(item))
                                            {
                                                n.CanFind.Remove(item);
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"No Item with that ID could be found.{Constants.NewLine}");
                                        }
                                        break;

                                    case 5:
                                        if (ValidateResourceNode(ref desc, ref n, false))
                                        {
                                            if (DatabaseManager.UpdateResourceNode(ref desc, ref n))
                                            {
                                                if (NodeManager.Instance.UpdateNode(ref desc, n))
                                                {
                                                    desc.Send($"Updated Node in NodeManager and World Database.{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update Node in NodeManager. Changes may not be available until restart.{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update Node in World Database.{Constants.NewLine}");
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
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
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
                    desc.Send($"No Resource Node with ID could be found.{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Delete
        private static void DeleteResourceNode(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a database backup is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send("Enter the ID of the Resource Node or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint nid))
            {
                ResourceNode n = NodeManager.Instance.GetNodeByID(nid);
                if (n != null)
                {
                    if (DatabaseManager.DeleteResourceNodeByID(ref desc, ref n))
                    {
                        if (NodeManager.Instance.RemoveNode(ref desc, n))
                        {
                            desc.Send($"Resource Node removed from NodeManager and World Database.{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"Unable to remove Resource Node from NodeManager.{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Unable to remove Resource Node from World Database.{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Unable to find a Resource Node with that name or ID.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Validation Functions
        private static bool ValidateResourceNode(ref Descriptor desc, ref ResourceNode n, bool IsNewNode)
        {
            if (n.ID == 0)
            {
                desc.Send($"You must provide a valid ID for the Node{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(n.NodeName))
            {
                desc.Send($"You must provide a name for the Node{Constants.NewLine}");
                return false;
            }
            if (n.AppearanceChance > 100)
            {
                desc.Send($"Appearance chance cannot be higher than 100%{Constants.NewLine}");
                return false;
            }
            if (IsNewNode && DatabaseManager.IsNodeIDInUse(n.ID))
            {
                desc.Send($"The specified Node ID is already in use.{Constants.NewLine}");
                return false;
            }
            if (n.CanFind == null || n.CanFind.Count == 0)
            {
                desc.Send($"You must add at least one item to the Node.{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}