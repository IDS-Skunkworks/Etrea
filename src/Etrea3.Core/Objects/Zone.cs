using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Etrea3.Objects
{
    [Serializable]
    public class Zone
    {
        [JsonProperty]
        public int ZoneID { get; set; }
        [JsonProperty]
        public string ZoneName { get; set; }
        [JsonProperty]
        public int MinRoom { get; set; }
        [JsonProperty]
        public int MaxRoom { get; set; }
        [JsonProperty]
        public ResourceVeinType AllowedVeinTypes { get; set; } = ResourceVeinType.Common;
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;

        public void PulseZone()
        {
            Game.LogMessage($"INFO: Pulsing Zone {ZoneName} ({ZoneID})", LogLevel.Info);
            var npcsForZone = NPCManager.Instance.GetNPCsForZone(ZoneID);
            var roomsForZone = RoomManager.Instance.GetRoomsForZone(ZoneID);
            // spawn new NPCs - spawn at tick first, then random npc generation
            foreach (var r in roomsForZone.Where(x => x.SpawnNPCsOnTick.Count > 0 && !x.Flags.HasFlag(RoomFlags.NoMobs)))
            {
                foreach (var n in r.SpawnNPCsOnTick)
                {
                    var npcTemplate = NPCManager.Instance.GetNPC(n.Key);
                    if (npcTemplate != null)
                    {
                        if (r.NPCsInRoom.Where(x => x.TemplateID == npcTemplate.TemplateID).Count() < n.Value)
                        {
                            if (NPCManager.Instance.GetNPCInstanceCount(npcTemplate.TemplateID) < npcTemplate.MaxNumberInWorld)
                            {
                                NPCManager.Instance.AddNewNPCInstance(npcTemplate.TemplateID, r.ID);
                            }
                            else
                            {
                                Game.LogMessage($"INFO: Not spawing NPC {npcTemplate.TemplateID} in Room {r.ID}, max number in world reached", LogLevel.Info);
                            }
                        }
                        else
                        {
                            Game.LogMessage($"INFO: Not spawing NPC {npcTemplate.TemplateID} in Room {r.ID}, max number in Room reached", LogLevel.Info);
                        }
                    }
                    else
                    {
                        Game.LogMessage($"ERROR: Cannot spawn instance of NPC {n.Key} in Room {r.ID}: No such NPC Template in NPC Manager", LogLevel.Error);
                    }
                }
            }
            foreach (var r in roomsForZone.Where(x => !x.Flags.HasFlag(RoomFlags.NoMobs)))
            {
                foreach (var npc in npcsForZone)
                {
                    var roll = Helpers.RollDice<int>(1, 100);
                    if (roll < npc.AppearanceChance && NPCManager.Instance.GetNPCInstanceCount(npc.TemplateID) + 1 <= npc.MaxNumberInWorld)
                    {
                        NPCManager.Instance.AddNewNPCInstance(npc.TemplateID, r.ID);
                    }
                }
            }
            // spawn tick items, if any
            foreach (var r in roomsForZone.Where(x => x.SpawnItemsOnTick.Count > 0))
            {
                foreach (var i in r.SpawnItemsOnTick)
                {
                    while (r.ItemsInRoom.Values.Count(x => x.ID == i.Key) < i.Value)
                    {
                        if (RoomManager.Instance.AddItemToRoomInventory(r.ID, i.Key))
                        {
                            Game.LogMessage($"INFO: Spawning Item {i.Key} in Room {r.ID}", LogLevel.Info);
                        }
                    }
                }
            }
            // spawn nodes in caves, if there are any
            var caves = roomsForZone.Where(x => x.Flags.HasFlag(RoomFlags.Cave)).ToList();
            if (caves.Count > 0)
            {
                var nodesForZone = NodeManager.Instance.GetNode(AllowedVeinTypes);
                if (nodesForZone == null || nodesForZone.Count == 0)
                {
                    return;
                }
                foreach (var r in caves)
                {
                    var node = nodesForZone.GetRandomElement();
                    if (node != null)
                    {
                        var newNodes = Helpers.Clone(node);
                        newNodes.Depth = Helpers.RollDice<int>(1, 4);
                        r.RSSNode = newNodes;
                        Game.LogMessage($"INFO: Spawning Resource Vein {newNodes.Name} in Room {r.ID}", LogLevel.Info);
                    }
                }
            }
        }
    }
}
