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
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;

        public void PulseZone()
        {
            Game.LogMessage($"INFO: Pulsing Zone {ZoneName} ({ZoneID})", LogLevel.Info, true);
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
                                Game.LogMessage($"INFO: Not spawing NPC {npcTemplate.TemplateID} in Room {r.ID}, max number in world reached", LogLevel.Info, true);
                            }
                        }
                        else
                        {
                            Game.LogMessage($"INFO: Not spawing NPC {npcTemplate.TemplateID} in Room {r.ID}, max number in Room reached", LogLevel.Info, true);
                        }
                    }
                    else
                    {
                        Game.LogMessage($"ERROR: Cannot spawn instance of NPC {n.Key} in Room {r.ID}: No such NPC Template in NPC Manager", LogLevel.Error, true);
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
                    var item = ItemManager.Instance.GetItem(i.Key);
                    if (item != null)
                    {
                        while (r.ItemsInRoom.Values.Where(x => x.ID == item.ID).Count() < i.Value)
                        {
                            dynamic newItem = null;
                            switch (item.ItemType)
                            {
                                case ItemType.Misc:
                                    newItem = Helpers.Clone<InventoryItem>(item);
                                    break;

                                case ItemType.Weapon:
                                    newItem = Helpers.Clone<Weapon>(item);
                                    break;

                                case ItemType.Consumable:
                                    newItem = Helpers.Clone<Consumable>(item);
                                    break;

                                case ItemType.Armour:
                                    newItem = Helpers.Clone<Armour>(item);
                                    break;

                                case ItemType.Ring:
                                    newItem = Helpers.Clone<Ring>(item);
                                    break;

                                case ItemType.Scroll:
                                    newItem = Helpers.Clone<Scroll>(item);
                                    break;
                            }
                            Game.LogMessage($"INFO: Spawning Item {i.Key} in Room {r.ID}", LogLevel.Info, true);
                            newItem.ItemID = Guid.NewGuid();
                            RoomManager.Instance.AddItemToRoomInventory(r.ID, newItem);
                        }
                    }
                    else
                    {
                        Game.LogMessage($"ERROR: Not spawning Item {i.Key} in Room {r.ID}: No such Item in Item Manager", LogLevel.Error, true);
                    }
                }
            }
            // spawn nodes in caves, if there are any
            var caves = roomsForZone.Where(x => x.Flags.HasFlag(RoomFlags.Cave)).ToList();
            foreach (var r in caves)
            {
                if (r.RSSNode == null)
                {
                    var roll = Helpers.RollDice<int>(1, 100);
                    var node = NodeManager.Instance.GetRandomNode(roll);
                    if (node != null)
                    {
                        var newNode = Helpers.Clone(node);
                        newNode.Depth = Helpers.RollDice<int>(1, 4);
                        r.RSSNode = newNode;
                        Game.LogMessage($"INFO: Spawning Resource Node {newNode.Name} in Room {r.ID}", LogLevel.Info, true);
                    }
                }
            }
        }
    }
}
