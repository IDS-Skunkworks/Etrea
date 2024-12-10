using Etrea3.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Etrea3.Core
{
    public class NodeManager
    {
        private static NodeManager instance = null;
        private ConcurrentDictionary<int, ResourceNode> Nodes;
        public int Count => Instance.Nodes.Count;

        public NodeManager()
        {
            Nodes = new ConcurrentDictionary<int, ResourceNode>();
        }

        public static NodeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NodeManager();
                }
                return instance;
            }
        }

        public void SetNodeLockState(int id, bool locked, Session session)
        {
            if (Instance.Nodes.ContainsKey(id))
            {
                Instance.Nodes[id].OLCLocked = locked;
                Instance.Nodes[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetNodeLockState(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.Nodes.ContainsKey(id))
            {
                lockHolder = Instance.Nodes[id].LockHolder;
                return Instance.Nodes[id].OLCLocked;
            }
            return false;
        }

        public bool NodeExists(int id)
        {
            return Instance.Nodes.ContainsKey(id);
        }

        public ResourceNode GetRandomNode(int roll)
        {
            return Instance.Nodes.Values.Where(x => x.ApperanceChance <= roll).ToList().GetRandomElement();
        }

        public ResourceNode GetNode(int id)
        {
            return Instance.Nodes.ContainsKey(id) ? Instance.Nodes[id] : null;
        }

        public List<ResourceNode> GetNode(int start, int end)
        {
            return end <= start ? null : (from n in Instance.Nodes.Values where n.ID >= start && n.ID <= end select n).ToList();
        }

        public List<ResourceNode> GetNode(string criteria)
        {
            return Instance.Nodes.Values.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<ResourceNode> GetNode()
        {
            return Instance.Nodes.Values.ToList();
        }

        public bool AddOrUpdateNode(ResourceNode node, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveNodeToWorldDatabase(node, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Node {node.Name} ({node.ID}) to the World Database", LogLevel.Error, true);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Nodes.TryAdd(node.ID, node))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Node {node.Name} ({node.ID}) to Node Manager", LogLevel.Error, true);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Nodes.TryGetValue(node.ID, out ResourceNode existingNode))
                    {
                        Game.LogMessage($"ERROR: Node {node.ID} not found in Node Manager for update", LogLevel.Error, true);
                        return false;
                    }
                    if (!Instance.Nodes.TryUpdate(node.ID, node, existingNode))
                    {
                        Game.LogMessage($"ERROR: Failed to update Node {node.ID} in Node Manager due to a value mismatch", LogLevel.Error, true);
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NodeManager.AddOrUpdateNode(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public bool RemoveNode(int id)
        {
            if (Instance.Nodes.ContainsKey(id))
            {
                return Instance.Nodes.TryRemove(id, out _) && DatabaseManager.RemoveResourceNode(id);
            }
            Game.LogMessage($"ERROR: Error removing Resource Node with ID {id}: No such Node in Node Manager", LogLevel.Error, true);
            return false;
        }

        public void LoadAllNodes(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllNodes(out hasErr);
            if (!hasErr && result != null)
            {
                foreach(var node in result)
                {
                    Instance.Nodes.AddOrUpdate(node.Key, node.Value, (k, v) => node.Value);
                }
            }
        }
    }
}
