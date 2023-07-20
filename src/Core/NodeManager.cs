using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kingdoms_of_Etrea.Entities;

namespace Kingdoms_of_Etrea.Core
{
    internal class NodeManager
    {
        private static readonly object _lockObject = new object();
        private static NodeManager _instance = null;
        private Dictionary<uint, ResourceNode> _nodes;

        private NodeManager()
        {
            _nodes = new Dictionary<uint, ResourceNode>();
        }

        internal static NodeManager Instance
        {
            get
            {
                lock(_lockObject)
                {
                    if(_instance == null)
                    {
                        _instance = new NodeManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool AddNode(ResourceNode node)
        {
            try
            {
                if (node != null)
                {
                    lock (_lockObject)
                    {
                        _nodes.Add(node.Id, node);
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Resource Node {node.NodeName} ({node.Id}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveNode(ResourceNode node)
        {
            try
            {
                if (node != null)
                {
                    lock (_lockObject)
                    {
                        _nodes.Remove(node.Id);
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing Resource Node {node.NodeName} ({node.Id}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal ResourceNode GetNode(uint roll)
        {
            if( _nodes != null && _nodes.Count > 0)
            {
                var nodes = _nodes.Where(x => x.Value.AppearanceChance >= roll).ToList();
                var rnd = new Random(DateTime.Now.GetHashCode());
                var n = nodes[rnd.Next(nodes.Count)].Value.ShallowCopy();
                n.NodeDepth = Helpers.RollDice(1, 4);
                return n;
            }
            return null;
        }

        internal ResourceNode GetNodeByID(uint id)
        {
            if(_nodes.ContainsKey(id))
            {
                return _nodes[id];
            }
            return null;
        }

        internal ResourceNode GetNodeByName(string name)
        {
            return (from n in _nodes.Values where Regex.Match(n.NodeName, name, RegexOptions.IgnoreCase).Success select n).FirstOrDefault();
        }

        internal bool UpdateNode(ref Descriptor desc, ResourceNode n)
        {
            try
            {
                if(Instance._nodes.ContainsKey(n.Id))
                {
                    lock(_lockObject)
                    {
                        Instance._nodes.Remove(n.Id);
                        Instance._nodes.Add(n.Id, n);
                        Game.LogMessage($"INFO: Player {desc.Player} updated Resource Node {n.NodeName} ({n.Id}) in NodeManager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: NodeManager does not contain a Resource Node with ID {n.Id}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance._nodes.Add(n.Id, n);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Resource Node {n.NodeName} ({n.Id}) in NodeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal void LoadAllResourceNodes(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllResourceNodes(out hasErr);
            if(!hasErr && result != null && result.Count > 0)
            {
                Instance._nodes.Clear();
                Instance._nodes = result;
            }
        }

        internal List<ResourceNode> GetAllResourceNodes(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var result = from n in Instance._nodes.Values where Regex.Match(n.NodeName, name, RegexOptions.IgnoreCase).Success select n;
                return result.ToList();
            }
            return _nodes.Values.ToList();
        }

        internal int GetNodeCount()
        {
            return _nodes.Count;
        }
    }
}
