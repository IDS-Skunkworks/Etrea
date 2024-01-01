using Kingdoms_of_Etrea.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
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
        internal uint GoldInRoom { get; set; }
        internal List<InventoryItem> ItemsInRoom { get; set; }
        [JsonProperty]
        internal Dictionary<uint, uint> SpawnNPCsAtStart { get; set; }
        [JsonProperty]
        internal Dictionary<uint,uint> SpawnItemsAtTick { get; set; }
        [JsonProperty]
        internal Dictionary<uint,uint> SpawnNPCsAtTick { get; set; }
        [JsonProperty]
        internal uint? ShopID { get; set; }
        internal bool HasLightSource { get; set; }
        internal ResourceNode ResourceNode { get; set; }
        internal List<Descriptor> PlayersInRoom(uint rid) => SessionManager.Instance.GetPlayersInRoom(RoomID);

        public Room()
        {
            ItemsInRoom = new List<InventoryItem>();
            SpawnNPCsAtStart = new Dictionary<uint, uint>();
            SpawnItemsAtTick = new Dictionary<uint, uint>();
            RoomExits = new List<Exit>();
        }

        internal bool IsShopRoom()
        {
            return ShopID != null && ShopID.HasValue && ShopID.Value > 0;
        }

        internal bool HasExitInDiretion(string direction)
        {
            if(RoomExits.Count > 0)
            {
                return (from e in RoomExits where e.ExitDirection.ToLower() == direction.ToLower() select e).Count() > 0;
            }
            return false;
        }

        internal Exit GetRoomExit(string direction)
        {
            return (from e in RoomExits where e.ExitDirection.ToLower() == direction.ToLower() select e).First();
        }

        internal bool HasTrainer()
        {
            return Flags.HasFlag(RoomFlags.MagicTrainer) || Flags.HasFlag(RoomFlags.StatTrainer) || Flags.HasFlag(RoomFlags.SkillTrainer) ||
                Flags.HasFlag(RoomFlags.Scribe) || Flags.HasFlag(RoomFlags.Jeweler) || Flags.HasFlag(RoomFlags.Alchemist) || Flags.HasFlag(RoomFlags.Blacksmith) ||
                Flags.HasFlag(RoomFlags.LanguageTrainer);
        }

        internal static void DescribeRoom(ref Descriptor desc, bool playerInRoom = false)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string pName = desc.Player.Name;
                string lDesc = string.Empty;
                if(desc.Player.Level >= Constants.ImmLevel)
                {
                    sb.AppendLine($"RID: {desc.Player.CurrentRoom}{Constants.TabStop}{Constants.TabStop}Zone: {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ZoneID}");
                    sb.AppendLine($"Flags: {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags}");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Dark))
                {
                    if(desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision") || RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasLightSource)
                    {
                        sb.AppendLine($"You are in {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).RoomName}, {RoomManager.Instance.GetRoomShortDescription(desc.Player.CurrentRoom)}");
                        lDesc = RoomManager.Instance.GetRoomLongDescription(desc.Player.CurrentRoom);
                    }
                    else
                    {
                        sb.AppendLine("You are shrounded in darkness!");
                        lDesc = "It is impossible to see anything in this darkness!";
                    }
                }
                else
                {
                    sb.AppendLine($"You are in {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).RoomName}, {RoomManager.Instance.GetRoomShortDescription(desc.Player.CurrentRoom)}");
                    lDesc = RoomManager.Instance.GetRoomLongDescription(desc.Player.CurrentRoom);
                }
                
                sb.Append(lDesc);
                sb.AppendLine();
                if (playerInRoom && RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                {
                    sb.AppendLine("A feeling of safety comes over you here.");
                }
                if (playerInRoom && RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Healing))
                {
                    sb.AppendLine("A refreshing feeling washes over you and you feel invigorated.");
                }
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).IsShopRoom())
                {
                    sb.AppendLine($"{ShopManager.Instance.GetShop(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ShopID.Value).ShopName} is selling wares here.");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.SkillTrainer))
                {
                    sb.AppendLine($"There is a trainer here, offering to teach you their skills!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.MagicTrainer))
                {
                    sb.AppendLine($"There is a sorceror here, offering to teach you some new spells!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.StatTrainer))
                {
                    sb.AppendLine($"There is a gym master here, offering to help improve your physique!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Gambler))
                {
                    sb.AppendLine($"There is a dicer here, taking wagers on the rolls!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Scribe))
                {
                    sb.AppendLine($"A scholar is here, eager to teach the arts of the Scribe!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Alchemist))
                {
                    sb.AppendLine($"An alchemist is here, eager to teach the potionmaster's craft!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Blacksmith))
                {
                    sb.AppendLine($"A heavily muscled blacksmith is here, ready to teach the secrets of the forge!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Jeweler))
                {
                    sb.AppendLine($"A jeweler is here, ready to teach his skills to the worthy!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Mercenary))
                {
                    sb.AppendLine($"A Mercenary Commander is here, ready to hire out muscle!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                {
                    sb.AppendLine($"A Royal Questmaster is here, dealing in bounties and rewards!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.PostBox))
                {
                    sb.AppendLine($"A Royal Mailbox is here, for sending and receiving messages!");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Banker))
                {
                    sb.AppendLine($"A bank teller is here, dealing with accounts and balances.");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.ItemVault))
                {
                    sb.AppendLine($"A vault warden is here, managing the storage of goods.");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.LanguageTrainer))
                {
                    sb.AppendLine($"A retired Royal Diplomat is here, ready to teach the languages of the realms.");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode != null)
                {
                    var node = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.ToString();
                    if (Helpers.IsCharAVowel(node[0]))
                    {
                        sb.AppendLine($"There is an {node} node here waiting to be mined!");
                    }
                    else
                    {
                        sb.AppendLine($"There is a {node} node here waiting to be mined!");
                    }
                }
                bool showFullRoomInfo = true;
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Dark) && !RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasLightSource)
                {
                    if(desc.Player.Level < Constants.ImmLevel && !desc.Player.HasSkill("Darkvision"))
                    {
                        showFullRoomInfo = false;
                    }
                }
                if(showFullRoomInfo)
                {
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => x.Player.Visible && !Regex.Match(x.Player.Name, pName, RegexOptions.IgnoreCase).Success).ToList();
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        sb.AppendLine($"{Constants.NewLine}You can see the following people:");
                        foreach (var p in localPlayers)
                        {
                            if (p.Player.Name != desc.Player.Name)
                            {
                                if (p.Player.Visible)
                                {
                                    sb.AppendLine($"{p.Player.Title} {p.Player.Name}, {p.Player.ShortDescription}");
                                }
                                else
                                {
                                    if (desc.Player.Level >= Constants.ImmLevel)
                                    {
                                        sb.AppendLine($"{p.Player.Title} {p.Player.Name}, {p.Player.ShortDescription} (Invisible)");
                                    }
                                }
                            }
                        }
                    }
                    var itemsInRooom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom;
                    if (itemsInRooom != null && itemsInRooom.Count > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"You can see the following items laying around:");
                        foreach (var i in itemsInRooom.Select(x => new { x.Id, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                        {
                            var cnt = itemsInRooom.Where(y => y.Id == i.Id).Count();
                            sb.AppendLine($"{cnt} x {i.Name}, {i.ShortDescription}");
                        }
                    }
                    var goldInRoom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GoldInRoom;
                    if(goldInRoom > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"You can see a pile of {goldInRoom} gold coins!");
                    }
                    var npcs = NPCManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom);
                    if (npcs != null && npcs.Count > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("You see:");
                        foreach(var n in npcs.Select(x => new {x.NPCID, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                        {
                            var cnt = npcs.Where(y => y.NPCID == n.NPCID).Count();
                            if(cnt == 1)
                            {
                                sb.AppendLine($"{n.Name}, {n.ShortDescription}");
                            }
                            else
                            {
                                sb.AppendLine($"{cnt} x {n.Name}, {n.ShortDescription}");
                            }
                        }
                    }
                    var roomExits = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).RoomExits.OrderBy(x => x.ExitDirection).ToList();
                    if (roomExits != null && roomExits.Count > 0)
                    {
                        sb.Append($"{Constants.NewLine}Available exits: {string.Join(", ", roomExits.OrderBy(x => x.ExitDirection).Select(x => x.ExitDirection).ToArray())}");
                    }
                }
                desc.Send(sb.ToString());
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error sending room description to client: {ex.Message}", LogLevel.Error, true);
            }
        }

        internal struct Exit
        {
            [JsonProperty]
            internal uint DestinationRoomID { get; set; }
            [JsonProperty]
            internal string ExitDirection { get; set; }
            [JsonProperty]
            internal RoomDoor RoomDoor { get; set; }
            [JsonProperty]
            internal Skills.Skill RequiredSkill { get; set; }

            public override string ToString()
            {
                return $"{ExitDirection} ({DestinationRoomID})";
            }
        }

        // Room Door: can add to Exit or be null for open exit, add required item for lock, bool locked, bool open
        internal class RoomDoor
        {
            [JsonProperty]
            internal uint RequiredItemID { get; set; }
            [JsonProperty]
            internal bool IsLocked { get; set; }
            [JsonProperty]
            internal bool IsOpen { get; set; }
        }
    }
}
