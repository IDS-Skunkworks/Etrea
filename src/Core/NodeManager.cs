using System;
using System.Collections.Generic;
using Etrea2.Entities;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class NodeManager
    {
        private static NodeManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, ResourceNode> _nodes;

        private NodeManager()
        {
            _nodes = new Dictionary<uint, ResourceNode>();
        }

        internal static NodeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NodeManager();
                }
                return _instance;
            }
        }

        internal bool NodeExists(uint nodeId)
        {
            return Instance._nodes.ContainsKey(nodeId);
        }

        internal bool AddNode(ref Descriptor desc, ResourceNode node)
        {
            try
            {
                lock (_lock)
                {
                    Instance._nodes.Add(node.ID, node);
                    Game.LogMessage($"OLC: {desc.Player.Name} has added ResourceNode {node.ID} ({node.NodeName}) to NodeManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: {desc.Player.Name} encountered an error adding ResourceNode {node.NodeName} ({node.ID}) to NodeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveNode(ref Descriptor desc, ResourceNode node)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._nodes.ContainsKey(node.ID))
                    {
                        Instance._nodes.Remove(node.ID);
                        Game.LogMessage($"OLC: {desc.Player.Name} removed ResourceNode {node.ID} ({node.NodeName}) from NodeManager", LogLevel.OLC, true);
                        return true;
                    }
                    Game.LogMessage($"OLC: {desc.Player.Name} was unable to remove ResourceNode {node.ID} from NodeManager, the ID does not exist", LogLevel.OLC, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing ResourceNode {node.ID} from NodeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateNode(ref Descriptor desc, ResourceNode node)
        {
            try
            {
                lock (_lock)
                {
                    if (Instance._nodes.ContainsKey(node.ID))
                    {
                        Instance._nodes.Remove(node.ID);
                        Instance._nodes.Add(node.ID, node);
                        Game.LogMessage($"OLC: {desc.Player.Name} updated ResourceNode {node.ID} ({node.NodeName}) in NodeManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: NodeManager does not contain a ResourceNode with ID {node.ID} to update, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddNode(ref desc, node);
                        return OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating ResourceNode {node.ID} ({node.NodeName}) in NodeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal ResourceNode GetNode(uint roll)
        {
            if (Instance._nodes != null && Instance._nodes.Count > 0)
            {
                var nodes = Instance._nodes.Values.Where(x => x.AppearanceChance >= roll).ToList();
                if (nodes.Count > 0)
                {
                    var rnd = new Random(DateTime.UtcNow.GetHashCode());
                    var n = nodes[rnd.Next(nodes.Count)].ShallowCopy();
                    n.NodeDepth = Helpers.RollDice(1, 4);
                    return n;
                }
            }
            return null;
        }

        internal ResourceNode GetNodeByID(uint id)
        {
            lock (_lock)
            {
                if (Instance._nodes.ContainsKey(id))
                {
                    return Instance._nodes[id];
                }
            }
            return null;
        }

        internal List<ResourceNode> GetNodeByNameOrDescription(string name)
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return Instance._nodes.Values.Where(x => Regex.IsMatch(x.NodeName, name, RegexOptions.IgnoreCase)).ToList();
                }
                return Instance._nodes.Values.ToList();
            }
        }

        internal void LoadAllNodes(out bool hasError)
        {
            var result = DatabaseManager.LoadAllResourceNodes(out hasError);
            if (!hasError && result != null)
            {
                Instance._nodes.Clear();
                Instance._nodes = result;
            }
        }

        internal int GetNodeCount()
        {
            return Instance._nodes.Count;
        }
    }
}
