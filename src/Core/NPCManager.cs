using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class NPCManager
    {
        private static NPCManager instance = null;
        private ConcurrentDictionary<int, NPC> NPCTemplates { get; set; }
        private ConcurrentDictionary<Guid, NPC> NPCInstances { get; set; }
        public int TemplateCount => Instance.NPCTemplates.Count;
        public int InstanceCount => Instance.NPCInstances.Count;
        public List<NPC> AllNPCTemplates => Instance.NPCTemplates.Values.ToList();
        public List<NPC> AllNPCInstances => Instance.NPCInstances.Values.ToList();

        private NPCManager()
        {
            NPCTemplates = new ConcurrentDictionary<int, NPC>();
            NPCInstances = new ConcurrentDictionary<Guid, NPC>();
        }

        public static NPCManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NPCManager();
                }
                return instance;
            }
        }

        public List<NPC> GetNPCsInRoom(int roomId)
        {
            return (from n in NPCInstances.Values where n.CurrentRoom == roomId select n).ToList();
        }

        public List<NPC> GetShopNPCsInRoom(int roomId)
        {
            return (from n in NPCInstances.Values where n.CurrentRoom == roomId && n.ShopID != 0 select n).ToList();
        }

        public void SetNPCLockState(int id, bool locked, Session session)
        {
            if (Instance.NPCTemplates.ContainsKey(id))
            {
                Instance.NPCTemplates[id].OLCLocked = locked;
                Instance.NPCTemplates[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetNPCLockState(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.NPCTemplates.ContainsKey(id))
            {
                lockHolder = Instance.NPCTemplates[id].LockHolder;
                return Instance.NPCTemplates[id].OLCLocked;
            }
            return false;
        }

        public NPC GetNPC(int id)
        {
            return Instance.NPCTemplates.ContainsKey(id) ? Instance.NPCTemplates[id] : null;
        }

        public NPC GetNPC(Guid id)
        {
            return Instance.NPCInstances.ContainsKey(id) ? Instance.NPCInstances[id] : null;
        }

        public List<NPC> GetShopNPCs(int shopID)
        {
            return Instance.NPCInstances.Values.Where(x => x.ShopID == shopID).ToList();
        }

        public List<NPC> GetNPC(string criteria)
        {
            return string.IsNullOrEmpty(criteria) ? null : Instance.NPCTemplates.Values.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0
            || x.ShortDescription.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0
            || x.LongDescription.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<NPC> GetNPC(int start, int end)
        {
            return end <= start ? null : Instance.NPCTemplates.Values.Where(x => x.TemplateID >= start && x.TemplateID <= end).ToList();
        }

        public List<NPC> GetNPC()
        {
            return Instance.NPCTemplates.Values.ToList();
        }

        public List<NPC> GetNPCsForZone(int zoneId)
        {
            return NPCTemplates.Values.Where(x => x.ZoneID == zoneId).ToList();
        }

        public int GetNPCInstanceCount(int id)
        {
            return Instance.NPCInstances.Values.Where(x => x.TemplateID == id).Count();
        }

        public bool NPCTemplateExists(int id)
        {
            return Instance.NPCTemplates.ContainsKey(id);
        }

        public bool NPCInstanceExists(Guid id)
        {
            return Instance.NPCInstances.ContainsKey(id);
        }

        public bool AddOrUpdateNPCTemplate(NPC npc, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveNPCTemplateToWorldDatabase(npc, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save NPC Template {npc.Name} ({npc.TemplateID}) to the World Database", LogLevel.Error, true);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.NPCTemplates.TryAdd(npc.TemplateID, npc))
                    {
                        Game.LogMessage($"ERROR: Failed to add new NPC Template {npc.Name} ({npc.TemplateID}) to NPC Manager", LogLevel.Error, true);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.NPCTemplates.TryGetValue(npc.TemplateID, out NPC existingTemplate))
                    {
                        Game.LogMessage($"ERROR: NPC {npc.TemplateID} not found in NPC Manager for update", LogLevel.Error, true);
                        return false;
                    }
                    if (!Instance.NPCTemplates.TryUpdate(npc.TemplateID, npc, existingTemplate))
                    {
                        Game.LogMessage($"ERROR: Failed to update NPC Template {npc.TemplateID} in NPC Manager due to a value mismatch", LogLevel.Error, true);
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NPCManager.AddOrUpdateNPCTemplate(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public bool RemoveNPCTemplate(int id)
        {
            if (Instance.NPCTemplates.ContainsKey(id))
            {
                return Instance.NPCTemplates.TryRemove(id, out _) && DatabaseManager.RemoveNPC(id);
            }
            Game.LogMessage($"ERROR: Error removing NPC with Template ID {id}, no such NPC Template in NPCManager", LogLevel.Error, true);
            return false;
        }

        public bool RemoveNPCInstance(Guid id)
        {
            if (Instance.NPCInstances.ContainsKey(id))
            {
                Instance.NPCInstances.TryRemove(id, out _);
                SessionManager.Instance.RemovePlayerCombatTarget(id);
                return true;
            }
            Game.LogMessage($"ERROR: Error removing NPC with Instance ID {id}, no such NPC Instance in NPCManager", LogLevel.Error, true);
            return false;
        }

        public bool MoveNPCToNewRoom(Guid npcID, int roomID)
        {
            if (Instance.NPCInstances.ContainsKey(npcID))
            {
                Instance.NPCInstances[npcID].CurrentRoom = roomID;
                return true;
            }
            Game.LogMessage($"ERROR: Error updating NPC Instance {npcID}, no such NPC Instance in NPCManager", LogLevel.Error, true);
            return false;
        }

        public bool AddNewNPCInstance(int npcID, int roomID)
        {
            if (!Instance.NPCTemplates.ContainsKey(npcID) || !RoomManager.Instance.RoomExists(roomID))
            {
                Game.LogMessage($"ERROR: Cannot add an Instance of NPC {npcID} to Room {roomID}, either the NPC or the Room does not exist", LogLevel.Error, true);
                return false;
            }
            var newNPC = Helpers.Clone(GetNPC(npcID));
            newNPC.ID = Guid.NewGuid();
            newNPC.CurrentRoom = roomID;
            var hpMod = Math.Max(0, Helpers.CalculateAbilityModifier(newNPC.Constitution));
            var mpMod = Math.Max(0, Math.Max(Helpers.CalculateAbilityModifier(newNPC.Intelligence), Helpers.CalculateAbilityModifier(newNPC.Wisdom)));
            newNPC.MaxHP = Helpers.RollDice<int>(newNPC.NumberOfHitDice, newNPC.HitDieSize) + hpMod * newNPC.NumberOfHitDice;
            newNPC.MaxMP = Helpers.RollDice<int>(newNPC.NumberOfHitDice, 10) + mpMod * newNPC.NumberOfHitDice;
            newNPC.CurrentHP = newNPC.MaxHP;
            newNPC.CurrentMP = newNPC.MaxMP;
            Instance.NPCInstances.TryAdd(newNPC.ID, newNPC);
            Game.LogMessage($"INFO: Spawning NPC {newNPC.ID} ({newNPC.Name}) in Room {roomID}", LogLevel.Info, true);
            var playersInRoom = RoomManager.Instance.GetRoom(roomID).PlayersInRoom;
            if (playersInRoom != null && playersInRoom.Count > 0)
            {
                foreach (var player in playersInRoom)
                {
                    player.Send($"The Winds of Magic swirl and breathe life into {newNPC.Name}!{Constants.NewLine}");
                }
            }
            return true;
        }

        public void RemoveActorFromNPCCombatQueue(Guid id)
        {
            var actorsInCombat = Instance.NPCInstances.Values.Where(x => x.TargetQueue.ContainsKey(id)).ToList();
            if (actorsInCombat != null && actorsInCombat.Count > 0)
            {
                foreach(var a in actorsInCombat)
                {
                    a.TargetQueue.TryRemove(id, out _);
                }
            }
        }

        public void LoadAllNPCs(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllNPCTemplates(out hasErr);
            if (!hasErr && result != null)
            {
                foreach(var npc in result)
                {
                    Instance.NPCTemplates.AddOrUpdate(npc.Key, npc.Value, (k, v) => npc.Value);
                }
            }
        }

        public void TickAllNPCs(ulong tickCount)
        {
            foreach (var npc in Instance.NPCInstances.Values)
            {
                foreach(var mp in npc.MobProgs.Keys)
                {
                    var mobProg = MobProgManager.Instance.GetMobProg(mp);
                    if (mobProg != null)
                    {
                        mobProg.Init();
                        mobProg.TriggerEvent(MobProgTrigger.MudTick, new { mob = npc.ID.ToString(), tick = tickCount });
                    }
                }
                var actionRoll = Helpers.RollDice<int>(1, 12);
                switch(actionRoll)
                {
                    case 1:
                        // do nothing for this NPC
                        continue;

                    case 2:
                    case 3:
                        // take item/gold from the room
                        NPCTakeFromRoom(npc);
                        break;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        // move to another location if we can move
                        NPCMoveFromRoom(npc);
                        break;

                    case 8:
                    case 9:
                        // start a fight if the npc isn't fighting, there is a player here and the room isn't safe
                        NPCStartFight(npc);
                        break;

                    case 10:
                        // do two things (take and start fight / take and move / drop and start fight / drop and move)
                        var roll = Helpers.RollDice<int>(1, 4);
                        switch(roll)
                        {
                            case 1:
                                NPCTakeFromRoom(npc);
                                NPCStartFight(npc);
                                break;

                            case 2:
                                NPCTakeFromRoom(npc);
                                NPCMoveFromRoom(npc);
                                break;

                            case 3:
                                NPCDropInRoom(npc);
                                NPCStartFight(npc);
                                break;

                            case 4:
                                NPCDropInRoom(npc);
                                NPCMoveFromRoom(npc);
                                break;
                        }
                        break;

                    case 11:
                    case 12:
                        // drop an item/gold into the room
                        NPCDropInRoom(npc);
                        break;
                }
            }
        }

        private void NPCTakeFromRoom(NPC npc)
        {
            string[] msgs = { string.Empty, string.Empty };
            if (!npc.Flags.HasFlag(NPCFlags.Scavenger))
            {
                return;
            }
            if (npc.InCombat)
            {
                return;
            }
            var r = RoomManager.Instance.GetRoom(npc.CurrentRoom);
            if (r.ItemsInRoom.IsEmpty && r.GoldInRoom == 0)
            {
                return;
            }
            var roll = Helpers.RollDice<int>(1, 3);
            switch(roll)
            {
                case 1:
                    // take item
                    if (r.ItemsInRoom.Count > 0)
                    {
                        var itemID = r.ItemsInRoom.GetRandomElement();
                        var item = r.ItemsInRoom[itemID];
                        RoomManager.Instance.RemoveItemFromRoomInventory(r.ID, item);
                        npc.AddItemToInventory(item);
                        msgs[0] = $"%BYT%%N% snatches up {item.ShortDescription}!%PT%{Constants.NewLine}";
                    }
                    break;

                case 2:
                    // take gold
                    if (r.GoldInRoom > 0)
                    {
                        var g = r.GoldInRoom;
                        RoomManager.Instance.RemoveGoldFromRoom(r.ID, g);
                        npc.Gold += g;
                        msgs[0] = $"%BYT%%N% snatches up a pile of {g:N0} gold coins!%PT%{Constants.NewLine}";
                    }
                    break;

                case 3:
                    // take both
                    if (r.ItemsInRoom.Count > 0)
                    {
                        var itemID = r.ItemsInRoom.GetRandomElement();
                        var item = r.ItemsInRoom[itemID];
                        RoomManager.Instance.RemoveItemFromRoomInventory(r.ID, item);
                        npc.AddItemToInventory(item);
                        msgs[0] = $"%BYT%%N% snatches up {item.ShortDescription}!%PT%{Constants.NewLine}";
                    }
                    if (r.GoldInRoom > 0)
                    {
                        var g = r.GoldInRoom;
                        RoomManager.Instance.RemoveGoldFromRoom(r.ID, g);
                        npc.Gold += g;
                        msgs[1] = $"%BYT%%N% snatches up a pile of {g:N0} gold coins!%PT%{Constants.NewLine}";
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(msgs[0]))
            {
                foreach (var (lp, msgToPlayer) in from lp in r.PlayersInRoom
                                                  let msgToPlayer = npc.CanBeSeenBy(lp.Player) ? msgs[0].Replace("%N%", npc.Name) : msgs[0].Replace("%N%", "Something")
                                                  select (lp, msgToPlayer))
                {
                    lp.Send(msgToPlayer);
                }
            }
            if (!string.IsNullOrEmpty(msgs[1]))
            {
                foreach (var (lp, msgToPlayer) in from lp in r.PlayersInRoom
                                                  let msgToPlayer = npc.CanBeSeenBy(lp.Player) ? msgs[1].Replace("%N%", npc.Name) : msgs[1].Replace("%N%", "Something")
                                                  select (lp, msgToPlayer))
                {
                    lp.Send(msgToPlayer);
                }
            }
        }

        private void NPCMoveFromRoom(NPC npc)
        {
            // only move if not in combat, can only move to a room in the same zone as the NPC belongs to
            if (!npc.CanMove())
            {
                return;
            }
            var r = RoomManager.Instance.GetRoom(npc.CurrentRoom);
            var exit = r.RoomExits.Count > 0 ? r.RoomExits.Values.ToList().GetRandomElement() : null;
            if (exit != null)
            {
                if (ZoneManager.Instance.GetZoneForRID(exit.DestinationRoomID).ZoneID != npc.ZoneID)
                {
                    return;
                }
                if (!RoomManager.Instance.RoomExists(exit.DestinationRoomID))
                {
                    return;
                }
                if (RoomManager.Instance.GetRoom(exit.DestinationRoomID).Flags.HasFlag(RoomFlags.NoMobs) || RoomManager.Instance.GetRoom(exit.DestinationRoomID).Flags.HasFlag(RoomFlags.GodRoom))
                {
                    return;
                }
                npc.Move(exit.DestinationRoomID, false);
            }
        }


        private void NPCStartFight(NPC npc)
        {
            // only start a fight if the NPC is hostile, isn't fighting already (maybe?) and the room isn't safe
            var r = RoomManager.Instance.GetRoom(npc.CurrentRoom);
            if (r.Flags.HasFlag(RoomFlags.Safe))
            {
                return;
            }
            if (!npc.Flags.HasFlag(NPCFlags.Hostile))
            {
                return;
            }
            if (npc.InCombat)
            {
                return;
            }
            if (r.PlayersInRoom.Count == 0 || r.PlayersInRoom.Where(x => x.Player.CanBeSeenBy(npc)).Count() == 0)
            {
                return;
            }
            var targets = r.PlayersInRoom.Where(x => x.Player.CanBeSeenBy(npc)).ToList();
            var target = targets.GetRandomElement();
            target.Send($"%BRT%Suddenly {npc.Name} attacks!%PT%{Constants.NewLine}");
            var msgToOthers = $"%BRT%For seemingly no reason, %N% suddenly launches an attack on %P%!%PT%{Constants.NewLine}";
            foreach (var lp in r.PlayersInRoom.Where(x => x.ID != target.ID))
            {
                msgToOthers = npc.CanBeSeenBy(lp.Player) ? msgToOthers.Replace("%N%", npc.Name) : msgToOthers.Replace("%N%", "something");
                msgToOthers = target.Player.CanBeSeenBy(lp.Player) ? msgToOthers.Replace("%P%", target.Player.Name) : msgToOthers.Replace("%P%", "someone");
                lp.Send(msgToOthers);
            }
            npc.AddToTargetQueue(target.Player);
            target.Player.AddToTargetQueue(npc);
            Game.LogMessage($"COMBAT: NPC {npc.Name} in Room {r.ID} has started combat with {target.Player.Name}", LogLevel.Combat, true);
        }

        private void NPCDropInRoom(NPC npc)
        {
            // only drop in room if the NPC has the LitterBug flag
            if (!npc.Flags.HasFlag(NPCFlags.LitterBug))
            {
                return;
            }
            if (npc.Inventory.Count == 0 && npc.Gold == 0)
            {
                return;
            }
            var roll = Helpers.RollDice<int>(1, 3);
            string[] msgs = { string.Empty, string.Empty };
            switch(roll)
            {
                case 1:
                    // drop item
                    if (npc.Inventory.Count > 0)
                    {
                        var itemID = npc.Inventory.GetRandomElement();
                        var item = npc.Inventory[itemID];
                        npc.RemoveItemFromInventory(item);
                        RoomManager.Instance.AddItemToRoomInventory(npc.CurrentRoom, item);
                        msgs[0] = $"%BYT%%N% drops {item.ShortDescription} to the floor!%PT%{Constants.NewLine}";
                    }
                    break;

                case 2:
                    // drop gold
                    if (npc.Gold > 0)
                    {
                        var g = npc.Gold;
                        RoomManager.Instance.AddGoldToRoom(npc.CurrentRoom, g);
                        npc.Gold = 0;
                        msgs[0] = $"%BYT%%N% drops {g:N0} gold onto the floor! Hope they didn't need that!%PT%{Constants.NewLine}";
                    }
                    break;

                case 3:
                    // drop both
                    if (npc.Inventory.Count > 0)
                    {
                        var itemID = npc.Inventory.GetRandomElement();
                        var item = npc.Inventory[itemID];
                        npc.RemoveItemFromInventory(item);
                        RoomManager.Instance.AddItemToRoomInventory(npc.CurrentRoom, item);
                        msgs[0] = $"%BYT%%N% drops {item.ShortDescription} to the floor!%PT%{Constants.NewLine}";
                    }
                    if (npc.Gold > 0)
                    {
                        var g = npc.Gold;
                        RoomManager.Instance.AddGoldToRoom(npc.CurrentRoom, g);
                        npc.Gold = 0;
                        msgs[1] = $"%BYT%%N% drops {g:N0} gold onto the floor! Hope they didn't need that!%PT%{Constants.NewLine}";
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(msgs[0]))
            {
                foreach (var (lp, msgToPlayer) in from lp in RoomManager.Instance.GetRoom(npc.CurrentRoom).PlayersInRoom
                                                  let msgToPlayer = npc.CanBeSeenBy(lp.Player) ? msgs[0].Replace("%N%", npc.Name) : msgs[0].Replace("%N%", "Something")
                                                  select (lp, msgToPlayer))
                {
                    lp.Send(msgToPlayer);
                }
            }
            if (!string.IsNullOrEmpty(msgs[1]))
            {
                foreach (var (lp, msgToPlayer) in from lp in RoomManager.Instance.GetRoom(npc.CurrentRoom).PlayersInRoom
                                                  let msgToPlayer = npc.CanBeSeenBy(lp.Player) ? msgs[1].Replace("%N%", npc.Name) : msgs[1].Replace("%N%", "Something")
                                                  select (lp, msgToPlayer))
                {
                    lp.Send(msgToPlayer);
                }
            }
        }
    }
}
