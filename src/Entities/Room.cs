using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Etrea2.Core;
using Newtonsoft.Json;

namespace Etrea2.Entities
{
    internal class Room
    {
        [JsonProperty]
        internal uint RoomID { get; set; }
        [JsonProperty]
        internal uint ZoneID { get; set; }
        [JsonProperty]
        internal string RoomName { get; set; }
        [JsonProperty]
        internal string ShortDescription { get; set; }
        [JsonProperty]
        internal string LongDescription { get; set; }
        [JsonProperty]
        internal List<Exit> RoomExits { get; set; }
        [JsonProperty]
        internal RoomFlags Flags { get; set; }
        [JsonProperty]
        internal ulong GoldInRoom { get; set; }
        internal List<InventoryItem> ItemsInRoom { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> SpawnNPCsAtStart { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> SpawnItemsAtTick { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> SpawnNPCsAtTick { get; set; }
        [JsonProperty]
        internal uint? ShopID { get; set; }
        internal bool HasLightSource { get; set; }
        internal ResourceNode ResourceNode { get; set; }
        internal List<Descriptor> PlayersInRoom => SessionManager.Instance.GetPlayersInRoom(RoomID);
        internal List<NPC> NPCsInRoom => NPCManager.Instance.GetNPCsInRoom(RoomID);
        internal bool IsShopRoom => ShopID != null && ShopID.HasValue && ShopID.Value > 0;
        internal bool HasTrainer => Flags.HasFlag(RoomFlags.MagicTrainer) || Flags.HasFlag(RoomFlags.StatTrainer) || Flags.HasFlag(RoomFlags.SkillTrainer)
            || Flags.HasFlag(RoomFlags.Scribe) || Flags.HasFlag(RoomFlags.Jeweler) || Flags.HasFlag(RoomFlags.Alchemist) || Flags.HasFlag(RoomFlags.Blacksmith)
            || Flags.HasFlag(RoomFlags.LanguageTrainer) || Flags.HasFlag(RoomFlags.Chef);

        internal Room()
        {
            ItemsInRoom = new List<InventoryItem>();
            SpawnNPCsAtStart = new Dictionary<uint, uint>();
            SpawnItemsAtTick = new Dictionary<uint, uint>();
            SpawnNPCsAtTick = new Dictionary<uint, uint>();
            RoomExits = new List<Exit>();
        }

        internal Room ShallowCopy()
        {
            var r = (Room)this.MemberwiseClone();
            return r;
        }

        internal bool HasExitInDirection(string direction)
        {
            if (RoomExits.Count > 0)
            {
                return RoomExits.Any(x => x.ExitDirection.ToLower() == direction.ToLower());
            }
            return false;
        }

        internal Exit GetRoomExit(string direction)
        {
            if (RoomExits.Count > 0)
            {
                return RoomExits.Where(x => x.ExitDirection.ToLower() == direction.ToLower()).FirstOrDefault();
            }
            return null;
        }

        internal void DescribeRoom(ref Descriptor desc, bool playerInRoom)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string pName = desc.Player.Name;
                string lDesc = string.Empty;
                bool showFullRoomInfo = true;
                if (desc.Player.Level >= Constants.ImmLevel)
                {
                    sb.AppendLine($"RID: {RoomID}{Constants.TabStop}{Constants.TabStop}ZID: {ZoneID}");
                    sb.AppendLine($"Flags: {Flags}");
                }
                if (Flags.HasFlag(RoomFlags.Dark))
                {
                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision") || HasLightSource)
                    {
                        sb.AppendLine($"You are in {RoomName}, {ShortDescription}");
                        foreach(var l in LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                        {
                            if (!string.IsNullOrEmpty(l))
                            {
                                sb.AppendLine(l);
                            }
                        }
                    }
                    else
                    {
                        showFullRoomInfo = false;
                        sb.AppendLine("You are shrouded in darkness!");
                        lDesc = "It is impossible to see anything in this darkness!";
                    }
                }
                else
                {
                    sb.AppendLine($"You are in {RoomName}, {ShortDescription}");
                    lDesc = LongDescription;
                }
                sb.Append(lDesc);
                sb.AppendLine();
                if (playerInRoom && Flags.HasFlag(RoomFlags.Safe))
                {
                    sb.AppendLine("A feeling of safety comes over you here.");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Healing))
                {
                    sb.AppendLine("A refreshing feeling washes over you and you feel invigorated!");
                }
                if (playerInRoom && IsShopRoom)
                {
                    sb.AppendLine($"{ShopManager.Instance.GetShop(ShopID.Value).ShopName} is selling wares here.");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.SkillTrainer))
                {
                    sb.AppendLine("There is a trainer here, offering to teach you their skills!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.MagicTrainer))
                {
                    sb.AppendLine("There is a sorceror here, offering tutelage in the arcane arts!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.StatTrainer))
                {
                    sb.AppendLine("There is a gym master here, offering to help you improve your physique!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Gambler))
                {
                    sb.AppendLine("There is a dicer here, taking wagers on the rolls!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Scribe))
                {
                    sb.AppendLine("There is a scholar here, eager to teach the arts of the Scribe!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Alchemist))
                {
                    sb.AppendLine("There is an alchemist here, ready to teach the potionmaster's craft!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Blacksmith))
                {
                    sb.AppendLine("A heavily muscled blacksmith is here, ready to teach the secrets of the forge!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Jeweler))
                {
                    sb.AppendLine("A master of jewels and gemcraft is here, ready to pass on their skills to the worthy!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Mercenary))
                {
                    sb.AppendLine("A mercenary commander is here, ready to hire out muscle to those with coin!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.QuestMaster))
                {
                    sb.AppendLine("A Royal Questmaster is here, dealing in bounties and rewards!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.PostBox))
                {
                    sb.AppendLine("A Royal Mailbox is here, ready to send a recieve messages!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Banker))
                {
                    sb.AppendLine("A bank teller is here, dealing with accounts and balances!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.ItemVault))
                {
                    sb.AppendLine("A vault warden is here, helping to manage the storage of items!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.LanguageTrainer))
                {
                    sb.AppendLine("A retired Royal Diplomat is here, offering to teach the languages of the realms!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Chef))
                {
                    sb.AppendLine("A master chef is here, ready to pass on the secrets of the kitchen!");
                }
                if (playerInRoom && Flags.HasFlag(RoomFlags.Exorcist))
                {
                    sb.AppendLine("An Exorcist cleric is here, dealing with curses and ailments!");
                }
                if (ResourceNode != null)
                {
                    var node = ResourceNode.ToString();
                    var nodeMsg = Helpers.IsCharAVowel(node[0]) ? $"There is an {node} node here, waiting to be mined!" : $"There is a {node} node here, waiting to be mined!";
                    sb.AppendLine(nodeMsg);
                }
                if (showFullRoomInfo)
                {
                    var localPlayers = PlayersInRoom.Where(x => x.Player.Name != pName).ToList();
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("You can see the following people:");
                        foreach(var p in localPlayers)
                        {
                            if (!p.Player.Visible && desc.Player.Level >= Constants.ImmLevel)
                            {
                                sb.AppendLine($"{p.Player.Title} {p.Player.Name}, {p.Player.ShortDescription} (Invisible)");
                            }
                            if (p.Player.Visible)
                            {
                                sb.AppendLine($"{p.Player.Title} {p.Player.Name}, {p.Player.ShortDescription}");
                            }
                        }
                    }
                    if (ItemsInRoom != null && ItemsInRoom.Count > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("You can see the following items laying around:");
                        foreach (var i in ItemsInRoom.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.Name))
                        {
                            var cnt = ItemsInRoom.Where(x => x.ID == i.ID).Count();
                            sb.AppendLine($"{cnt} x {i.Name}, {i.ShortDescription}");
                        }
                    }
                    if (GoldInRoom > 0)
                    {
                        sb.AppendLine($"You can see a pile of {GoldInRoom} gold coins!");
                    }
                    if (NPCsInRoom != null && NPCsInRoom.Count > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("You can see:");
                        foreach (var n in NPCsInRoom.Select(x => new { x.NPCID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.Name))
                        {
                            var cnt = NPCsInRoom.Where(x => x.NPCID == n.NPCID).Count();
                            switch(cnt == 1)
                            {
                                case true:
                                    sb.AppendLine($"{n.Name}, {n.ShortDescription}");
                                    break;

                                case false:
                                    sb.AppendLine($"{cnt} x {n.Name}, {n.ShortDescription}");
                                    break;
                            }
                        }
                    }
                    if (RoomExits != null && RoomExits.Count > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Available exits: {string.Join(", ", RoomExits.OrderBy(x => x.ExitDirection).Select(x => x.ExitDirection).ToArray())}");
                    }
                }
                sb.RemoveEmptyLines();
                desc.Send(sb.ToString());
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error sending description of Room {RoomID} to client {desc.Client.Client.RemoteEndPoint}: {ex.Message}", LogLevel.Error, true);
            }
        }
    }
}
