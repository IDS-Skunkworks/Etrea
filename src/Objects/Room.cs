using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etrea3.Objects
{
    [Serializable]
    public class Room
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public int ZoneID { get; set; }
        [JsonProperty]
        public string RoomName { get; set; }
        [JsonProperty]
        public string ShortDescription { get; set; }
        [JsonProperty]
        public string LongDescription { get; set; }
        [JsonProperty]
        public string SignText { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<string, RoomExit> RoomExits { get; set; }
        [JsonProperty]
        public RoomFlags Flags { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int, int> StartingNPCs { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int, int> StartingItems { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int, int> SpawnItemsOnTick { get; set; }
        [JsonProperty]
        public ConcurrentDictionary<int, int> SpawnNPCsOnTick { get; set; }
        [JsonIgnore]
        public ResourceNode RSSNode { get; set; }
        [JsonIgnore]
        public ulong GoldInRoom { get; set; }
        [JsonIgnore]
        public ConcurrentDictionary<Guid, InventoryItem> ItemsInRoom { get; set; }
        [JsonIgnore]
        public List<NPC> NPCsInRoom => NPCManager.Instance.GetNPCsInRoom(ID);
        [JsonIgnore]
        public List<Session> PlayersInRoom => SessionManager.Instance.GetPlayersInRoom(ID);
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; }
        [JsonIgnore]
        public List<Actor> AllActorsInRoom
        {
            get
            {
                List<Actor> result = new List<Actor>();
                result.AddRange(NPCsInRoom);
                result.AddRange(PlayersInRoom.Select(x => x.Player));
                return result;
            }
        }
        [JsonIgnore]
        public bool HasLightSource
        {
            get
            {
                if (!Flags.HasFlag(RoomFlags.Dark))
                {
                    return true;
                }
                if (NPCsInRoom.Any(x => x.HasBuff("Light")) || PlayersInRoom.Any(x => x.Player.HasBuff("Light")))
                {
                    return true;
                }
                return false;
            }
        }
        [JsonIgnore]
        public bool HasTrainer
        {
            get
            {
                return Flags.HasFlag(RoomFlags.MagicTrainer) || Flags.HasFlag(RoomFlags.StatTrainer) || Flags.HasFlag(RoomFlags.SkillTrainer)
                    || Flags.HasFlag(RoomFlags.Scribe) || Flags.HasFlag(RoomFlags.Jeweler) || Flags.HasFlag(RoomFlags.Alchemist) || Flags.HasFlag(RoomFlags.Blacksmith)
                    || Flags.HasFlag(RoomFlags.LanguageTrainer) || Flags.HasFlag(RoomFlags.Chef);
            }
        }

        public Room()
        {
            RoomName = "New Room";
            ShortDescription = "A new blank room";
            LongDescription = "An empty and featureless room, ready for development";
            RoomExits = new ConcurrentDictionary<string, RoomExit>();
            ItemsInRoom = new ConcurrentDictionary<Guid, InventoryItem>();
            StartingNPCs = new ConcurrentDictionary<int, int>();
            SpawnNPCsOnTick = new ConcurrentDictionary<int, int>();
            StartingItems = new ConcurrentDictionary<int, int>();
            SpawnItemsOnTick = new ConcurrentDictionary<int, int>();
            LockHolder = Guid.Empty;
            OLCLocked = false;
            Flags = RoomFlags.None;
            RSSNode = null;
        }

        public Actor GetActor(string name, Actor viewer)
        {
            return AllActorsInRoom.FirstOrDefault(x => x.CanBeSeenBy(viewer) && x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public InventoryItem GetItem(string name)
        {
            return ItemsInRoom.Values.FirstOrDefault(x => x.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public bool HasExitInDirection(string direction)
        {
            return RoomExits != null && RoomExits.ContainsKey(direction);
        }

        public RoomExit GetRoomExit(string direction)
        {
            return RoomExits != null && RoomExits.ContainsKey(direction) ? RoomExits[direction] : null;
        }

        public void DescribeRoom(Session session)
        {
            bool playerInRoom = session.Player.CurrentRoom == ID;
            bool playerIsImm = session.Player.Level >= Constants.ImmLevel;
            bool playerHasDarkvision = session.Player.HasBuff("Darkvision") || session.Player.HasBuff("Truesight");
            StringBuilder roomDesc = new StringBuilder();
            if (playerIsImm)
            {
                roomDesc.AppendLine($"Room ID: {ID}{Constants.TabStop}{Constants.TabStop}Zone: {ZoneManager.Instance.GetZone(ZoneID).ZoneName} (ID: {ZoneID})");
                roomDesc.AppendLine($"Flags: {Flags}");
            }
            if (HasLightSource || (playerHasDarkvision || playerIsImm))
            {
                if (playerInRoom)
                {
                    roomDesc.AppendLine($"You are in {RoomName}, {ShortDescription}");
                }
                var descLines = LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.None);
                for (int l = 0; l < descLines.Length; l++)
                {
                    if (!string.IsNullOrWhiteSpace(descLines[l]))
                    {
                        roomDesc.AppendLine(descLines[l]);
                    }
                    else if (l + 1 < descLines.Length)
                    {
                        if (string.IsNullOrWhiteSpace(descLines[l + 1]))
                        {
                            break;
                        }
                        continue;
                    }
                }
                if (playerInRoom)
                {
                    if (Flags.HasFlag(RoomFlags.Safe))
                    {
                        roomDesc.AppendLine("A feeling of safety comes over you.");
                    }
                    if (Flags.HasFlag(RoomFlags.Healing))
                    {
                        roomDesc.AppendLine("A refreshing feeling washes over you and you feel invigorated!");
                    }
                    if (Flags.HasFlag(RoomFlags.SkillTrainer))
                    {
                        roomDesc.AppendLine("A wise master is here, offering tutoring in many skills!");
                    }
                    if (Flags.HasFlag(RoomFlags.MagicTrainer))
                    {
                        roomDesc.AppendLine("A sorceror is here, offering tutelage in the arcane arts!");
                    }
                    if (Flags.HasFlag(RoomFlags.StatTrainer))
                    {
                        roomDesc.AppendLine("A gym master is here, offering to help improve your physique!");
                    }
                    if (Flags.HasFlag(RoomFlags.Gambler))
                    {
                        roomDesc.AppendLine("A dicer is here, taking wagers on the rolls!");
                    }
                    if (Flags.HasFlag(RoomFlags.Scribe))
                    {
                        roomDesc.AppendLine("A wizened scholar is here, eager to teach the arts of the Scribe!");
                    }
                    if (Flags.HasFlag(RoomFlags.Alchemist))
                    {
                        roomDesc.AppendLine("An alchemist is here, ready to teach the suble arts of potion making!");
                    }
                    if (Flags.HasFlag(RoomFlags.Blacksmith))
                    {
                        roomDesc.AppendLine("A heavily muscled blacksmith is here, ready to teach the secrets of the forge!");
                    }
                    if (Flags.HasFlag(RoomFlags.Jeweler))
                    {
                        roomDesc.AppendLine("A master of jewels and gemcraft is here, ready to pass on their skills to the worthy!");
                    }
                    if (Flags.HasFlag(RoomFlags.Chef))
                    {
                        roomDesc.AppendLine("A master chef is here, offering to pass on the secrets of the kitchen!");
                    }
                    if (Flags.HasFlag(RoomFlags.QuestMaster))
                    {
                        roomDesc.AppendLine("A Questmeister is here, dealing in bounties and rewards!");
                    }
                    if (Flags.HasFlag(RoomFlags.PostBox))
                    {
                        roomDesc.AppendLine("A Royal Mail postbox is here, ready to send and receive messages!");
                    }
                    if (Flags.HasFlag(RoomFlags.Banker))
                    {
                        roomDesc.AppendLine("A bank teller is here, dealing with accounts and balances!");
                    }
                    if (Flags.HasFlag(RoomFlags.Vault))
                    {
                        roomDesc.AppendLine("A vault warden is here, helping to manage the storage and retreival of items!");
                    }
                    if (Flags.HasFlag(RoomFlags.LanguageTrainer))
                    {
                        roomDesc.AppendLine("A retired diplomat is here, ready to teach the languages of the Realms!");
                    }
                    if (Flags.HasFlag(RoomFlags.Exorcist))
                    {
                        roomDesc.AppendLine("An Exorcist cleric is here, helping to deal with curses and ailments!");
                    }
                    if (Flags.HasFlag(RoomFlags.Sign))
                    {
                        roomDesc.AppendLine("A sign here beckons you to %BYT%read%PT% it.");
                    }
                    if (RSSNode != null)
                    {
                        var node = RSSNode.Name;
                        var article = Helpers.IsCharAVowel(node[0]) ? "an" : "a";
                        roomDesc.AppendLine($"There is {article} {node} here, waiting to be mined!");
                    }
                    var shopNPCs = NPCManager.Instance.GetShopNPCsInRoom(ID);
                    if (shopNPCs != null && shopNPCs.Count > 0)
                    {
                        roomDesc.AppendLine();
                        foreach(var shopNPC in shopNPCs)
                        {
                            var s = ShopManager.Instance.GetShop(shopNPC.ShopID);
                            if (s != null)
                            {
                                roomDesc.AppendLine($"{s.ShopName} is selling wares here");
                            }
                        }
                    }
                }
                var localPlayers = PlayersInRoom.Where(x => x.ID != session.ID && x.Player.CanBeSeenBy(session.Player)).ToList();
                if (localPlayers != null && localPlayers.Count > 0)
                {
                    roomDesc.AppendLine();
                    roomDesc.AppendLine("You can see the following people:");
                    foreach (var player in localPlayers)
                    {
                        if (!player.Player.Visible)
                        {
                            roomDesc.AppendLine($"{player.Player.Title} {player.Player.Name}, {player.Player.ShortDescription} (Invisibile)");
                        }
                        else
                        {
                            roomDesc.AppendLine($"{player.Player.Title} {player.Player.Name}, {player.Player.ShortDescription}");
                        }
                    }
                }
                var localNPCs = NPCsInRoom.Where(x => x.CanBeSeenBy(session.Player)).ToList();
                if (localNPCs != null && localNPCs.Count > 0)
                {
                    roomDesc.AppendLine();
                    roomDesc.AppendLine("You can see:");
                    foreach (var n in localNPCs.Select(x => new { x.TemplateID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.Name))
                    {
                        var cnt = localNPCs.Where(x => x.TemplateID == n.TemplateID).Count();
                        switch(cnt == 1)
                        {
                            case true:
                                roomDesc.AppendLine($"{n.Name}, {n.ShortDescription}");
                                break;

                            case false:
                                roomDesc.AppendLine($"{cnt} x {n.Name}, {n.ShortDescription}");
                                break;
                        }
                    }
                }
                if (ItemsInRoom.Count > 0)
                {
                    roomDesc.AppendLine();
                    roomDesc.AppendLine("You can see the following items:");
                    foreach (var item in ItemsInRoom.Values.Select(x => new { x.ID, x.Name, x.ShortDescription}).Distinct().OrderBy(x => x.ID))
                    {
                        var cnt = ItemsInRoom.Values.Where(x => x.ID == item.ID).Count();
                        roomDesc.AppendLine($" {cnt} x {item.Name}, {item.ShortDescription}");
                    }
                }
                if (GoldInRoom > 0)
                {
                    roomDesc.AppendLine($"%BYT%There is a pile of {GoldInRoom:N0} gold coins here!%PT%");
                }
                if (playerInRoom)
                {
                    if (RoomExits != null && RoomExits.Count > 0)
                    {
                        roomDesc.AppendLine();
                        roomDesc.AppendLine($"%BGT%Exits: {string.Join(", ", RoomExits.OrderBy(x => x.Value.ExitDirection).Select(x => x.Value.ExitDirection).ToArray())}%PT%");
                    }
                }
            }
            else
            {
                roomDesc.AppendLine($"The area is shrouded in darkness and nothing can be seen!{Constants.NewLine}");
            }
            session.Send(roomDesc.ToString());
        }
    }
}
