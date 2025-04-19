using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Etrea3.Core.ActMob;

namespace Etrea3.Core
{
    public static class ScriptingFunctions
    {
        #region MobProg Functions
        // TODO: Check and update some of these for extra error handling and possible rename
        public static void MogProgMobSellPlayerItem(string mobID, string playerID, string item)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgMobSellPlayerItem called with no Mob ID", LogLevel.Debug);
                return;
            }
            if (string.IsNullOrEmpty(playerID))
            {
                Game.LogMessage($"DEBUG: MobProgMobSellPlayerItem called with no Player ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var gPlayerID = Guid.Parse(playerID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var player = SessionManager.Instance.GetSession(gPlayerID);
            InventoryItem sellItem = null;
            if (mob == null || player == null)
            {
                Game.LogMessage($"DEBUG: MobProgMobSellPlayerItem called with IDs that could not be matched to a Player or an NPC", LogLevel.Debug);
                return;
            }
            if (string.IsNullOrEmpty(item))
            {
                sellItem = ItemManager.Instance.GetItem().Where(x => x.BaseValue <= 10).ToList().GetRandomElement();
            }
            if (int.TryParse(item, out int itemID))
            {
                sellItem = ItemManager.Instance.GetItem(itemID);
            }
            else
            {
                sellItem = ItemManager.Instance.GetItem(item);
            }
            if (sellItem == null)
            {
                Game.LogMessage($"DEBUG: MobProgMobSellPlayerItem could not find a suitable item to sell to the player", LogLevel.Debug);
                return;
            }
            if (player.Player.Gold >= (ulong)sellItem.BaseValue)
            {
                player.Player.AddItemToInventory(sellItem);
                player.Player.AdjustGold(sellItem.BaseValue * -1);
                player.Send($"%BYT%Somehow {mob.Name} has managed to sell you {sellItem.ShortDescription} for {sellItem.BaseValue:N0} gold!%PT%{Constants.NewLine}");
            }
            else
            {
                Game.LogMessage($"DEBUG: MobProgMobSellPlayerItem cannot sell Item {sellItem.ID} to player {player.Player.Name}: value of {sellItem.BaseValue:N0} is higher than Player Gold of {player.Player.Gold:N0}", LogLevel.Debug);
                return;
            }
        }

        public static void MobProgEmote(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgEmote called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobEmote(mob, args, null);
        }

        public static void MobProgMove(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgMove called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobMove(mob, args, null);
        }

        public static void MobProgTakeItem(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgTakeItem called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobTakeItem(mob, args, null);
        }

        public static void MobProgDropItem(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgDropItem called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMogID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMogID);
            MobDropItem(mob, args, null);
        }

        public static void MobProgGiveItem(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgGiveItem called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobGiveItem(mob, args, null);
        }

        public static void MobProgAttack(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgAttack called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobAttack(mob, args, null);
        }

        public static void MobProgCastSpell(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgCastSpell called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobCastSpell(mob, args, null);
        }

        public static void MobProgSay(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgSay called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobSay(mob, args, null);
        }

        public static void MobProgYell(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgYell called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobYell(mob, args, null);
        }

        public static void MobProgWhisper(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgWhisper called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            MobWhisper(mob, args, null);
        }

        public static void MobProgTeleportPlayer(string mobID, string playerID, string destination, ulong cost)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a null Mob ID", LogLevel.Debug);
                return;
            }
            if (string.IsNullOrEmpty(playerID))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a null Player ID", LogLevel.Debug);
                return;
            }
            if (string.IsNullOrEmpty(destination))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a null destination", LogLevel.Debug);
                return;
            }
            if (!int.TryParse(destination, out int rid))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a destination that was not an int", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var gPlayerID = Guid.Parse(playerID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var sess = SessionManager.Instance.GetSession(gPlayerID);
            if (!sess.Player.CanMove())
            {
                MobSay(mob, $"Sorry, {sess.Player.Name}, you're not ready to be teleported just now!", null);
                return;
            }
            if (sess.Player.Gold < cost)
            {
                MobSay(mob, $"Sorry, {sess.Player.Name}, you can't afford that!", null);
                return;
            }
            MobSay(mob, $"Very well, {sess.Player.Name}, just a moment.", null);
            sess.Player.AdjustGold((long)cost * -1, true, false);
            sess.Player.Move(rid, true);
        }

        public static string MobProgGetItemName(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                Game.LogMessage($"DEBUG: MobProgGetItemName called with no args", LogLevel.Debug);
                return null;
            }
            if (int.TryParse(args, out int rid))
            {
                var item = ItemManager.Instance.GetItem(int.Parse(args));
                return item != null ? item.Name : null;
            }
            Game.LogMessage($"DEBUG: MobProgGetItemName called with args that could not be transformed to an integer", LogLevel.Debug);
            return null;
        }

        public static string MobProgGetPlayerName(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                Game.LogMessage($"DEBUG: MobProgGetPlayerName called with no args", LogLevel.Debug);
                return null;
            }
            var gPlayerID = Guid.Parse(args);
            var player = SessionManager.Instance.GetSession(gPlayerID).Player;
            return player == null ? null : player.Name;
        }

        public static void MobProgRememberPlayer(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgRememberPlayer called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var gPlayerID = Guid.Parse(args);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.ID == gPlayerID).Select(x => x.Player).FirstOrDefault();
            mob.RememberPlayer(player, Game.TickCount);
        }

        public static void MobProgForgetPlayer(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgForgetPlayer called with no Mob ID", LogLevel.Debug);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var gPlayerID = Guid.Parse(args);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.ID == gPlayerID).Select(x => x.Player).FirstOrDefault();
            mob.ForgetPlayer(player);
        }

        public static bool MobProgRemembersPlayer(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgRemembersPlayer called with no Mob ID", LogLevel.Debug);
                return false;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var gPlayerID = Guid.Parse(args);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.ID == gPlayerID).Select(x => x.Player).FirstOrDefault();
            return mob.RemembersPlayer(player, out _);
        }

        public static ulong MobProgGetRememberPlayerTickCount(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgGetRememberPlayerTickCount called with no Mob ID", LogLevel.Debug);
                return 0;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var gPlayerID = Guid.Parse(args);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.ID == gPlayerID).Select(x => x.Player).FirstOrDefault();
            if (mob.RemembersPlayer(player, out ulong tick))
            {
                return tick;
            }
            return 0;
        }

        public static bool MobProgMobHasItem(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgMobHasItem called with no Mob ID", LogLevel.Debug);
                return false;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            if (int.TryParse(args, out var item))
            {
                return mob.HasItemInInventory(item);
            }
            else
            {
                return mob.HasItemInInventory(args);
            }
        }

        public static bool MobProgCheckPlayerIsImm(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                Game.LogMessage($"DEBUG: MobProgCheckPlayerIsImm called with no player ID", LogLevel.Debug);
                return false;
            }
            var gPlayerID = Guid.Parse(args);
            var p = SessionManager.Instance.GetSession(gPlayerID);
            return p.Player.IsImmortal;
        }

        public static bool MobProgIsItemInRoom(string mobID, string item)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgIsItemInRoom called with no Mob ID", LogLevel.Debug);
                return false;
            }
            if (string.IsNullOrEmpty(item))
            {
                Game.LogMessage($"DEBUG: MobProgIsItemInRoom called with no Item criteria", LogLevel.Debug);
                return false;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var r = RoomManager.Instance.GetRoom(mob.CurrentRoom);
            if (int.TryParse(item, out int itemID))
            {
                return r.ItemsInRoom.Values.Any(x => x.ID == itemID);
            }
            else
            {
                return r.ItemsInRoom.Values.Any(x => x.Name.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        public static string GetRandomPlayerID(string mobID)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: GetRandomPlayerID called with no Mob ID", LogLevel.Debug);
                return null;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            if (RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom.Count > 0)
            {
                var player = RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom.GetRandomElement();
                return player.ID.ToString();
            }
            return null;
        }
        #endregion

        #region RoomProg Functions
        public static bool ToggleMobFlag(string mobID, string flag)
        {
            if (string.IsNullOrEmpty(mobID) || string.IsNullOrEmpty(flag))
            {
                Game.LogMessage($"DEBUG: ToggleMobFlag() called with null/empty mob ID or flag", LogLevel.Debug);
                return false;
            }
            if (!Enum.TryParse(flag, true, out NPCFlags npcFlag))
            {
                Game.LogMessage($"DEBUG: ToggleMobFlag() called with a flag value that could not be parsed: {flag}", LogLevel.Debug);
                return false;
            }
            if (!Guid.TryParse(mobID, out var npcGuid))
            {
                Game.LogMessage($"DEBUG: ToggleMobFlag() called with an NPC ID that could not be transformed to a GUID: {mobID}", LogLevel.Debug);
                return false;
            }
            return NPCManager.Instance.ToggleNPCFlag(npcGuid, npcFlag);
        }

        public static bool SetMobFlag(string mobID, string flag, bool enabled)
        {
            if (string.IsNullOrEmpty(mobID) || string.IsNullOrEmpty(flag))
            {
                Game.LogMessage($"DEBUG: SetMobFlag() called with null/empty mob ID or flag", LogLevel.Debug);
                return false;
            }
            if (!Enum.TryParse(flag, true, out NPCFlags npcFlag))
            {
                Game.LogMessage($"DEBUG: SetMobFlag() called with a flag value that could not be parsed: {flag}", LogLevel.Debug);
                return false;
            }
            if (!Guid.TryParse(mobID, out var npcGuid))
            {
                Game.LogMessage($"DEBUG: SetMobFlag() called with an NPC ID that could not be transformed to a GUID: {mobID}", LogLevel.Debug);
                return false;
            }
            return NPCManager.Instance.SetNPCFlag(npcGuid, npcFlag, enabled);
        }

        public static bool ToggleRoomFlag(int rid, string flag)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: ToggleRoomFlag() called with a RID that was not found in Room Manager ({rid})", LogLevel.Debug);
                return false;
            }
            if (!Enum.TryParse(flag, true, out RoomFlags rFlag))
            {
                Game.LogMessage($"DEBUG: ToggleRoomFlag() called with a flag value that could not be parsed: {flag}", LogLevel.Debug);
                return false;
            }
            if (rFlag == RoomFlags.None)
            {
                Game.LogMessage($"DEBUG: ToggleRoomFlag() was called with a flag value that was parsed as RoomFlags.None: {flag}", LogLevel.Debug);
                return false;
            }
            return RoomManager.Instance.ToggleRoomFlag(rid, rFlag);
        }

        public static bool SetRoomFlag(int rid, string flag, bool enable)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: SetRoomFlag() called with a RID that was not found in Room Manager ({rid})", LogLevel.Debug);
                return false;
            }
            if (!Enum.TryParse(flag, true, out RoomFlags rFlags))
            {
                Game.LogMessage($"DEBUG: SetRoomFlag() called with a flag value that could not be parsed: {flag}", LogLevel.Debug);
                return false;
            }
            if (rFlags == RoomFlags.None)
            {
                Game.LogMessage($"DEBUG: SetRoomFlag() was called with a flag value that was parsed as RoomFlags.None: {flag}", LogLevel.Debug);
                return false;
            }
            return RoomManager.Instance.SetRoomFlag(rid, rFlags, enable);
        }

        public static void SendEnvironmentMessage(int rid, string action)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: SendEnvironmentMessage() was called with a RID that was not found in Room Manager ({rid})", LogLevel.Debug);
                return;
            }
            if (action.IsNullEmptyOrWhiteSpace())
            {
                Game.LogMessage($"DEBUG: SendEnvironmentMessage() was called with a message that was null, empty or whitespace", LogLevel.Debug);
                return;
            }
            var players = SessionManager.Instance.GetPlayersInRoom(rid);
            if (players != null && players.Count > 0)
            {
                foreach (var player in players)
                {
                    player.Send(action);
                }
            }
        }

        public static bool SpawnItemInRoom(int rid, int itemID)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: SpawnItemInRoom() was called with a RID that was not found in Room Manager: {rid}", LogLevel.Debug);
                return false;
            }
            if (!ItemManager.Instance.ItemExists(itemID))
            {
                Game.LogMessage($"DEBUG: SpawnItemInRoom() was called with an item ID that was not found in Item Manager: {itemID}", LogLevel.Debug);
                return false;
            }
            return RoomManager.Instance.AddItemToRoomInventory(rid, itemID);
        }

        public static bool SpawnNPCInRoom(int rid, int npcID)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: SpawnNPCInRoom() was called with a RID that was not found in Room Manager: {rid}", LogLevel.Debug);
                return false;
            }
            if (!NPCManager.Instance.NPCTemplateExists(npcID))
            {
                Game.LogMessage($"DEBUG: SpawnNPCInRoom() was called with an NPC ID that was not found in NPC Manager {npcID}", LogLevel.Debug);
                return false;
            }
            return NPCManager.Instance.AddNewNPCInstance(rid, npcID);
        }

        public static bool DespawnItemInRoom(int rid, int itemID, bool all)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: DespawnItemInRoom() was called with a RID that was not found in Room Manager: {rid}", LogLevel.Debug);
                return false;
            }
            if (!RoomManager.Instance.GetRoom(rid).ItemsInRoom.Any(x => x.Value.ID == itemID))
            {
                Game.LogMessage($"DEBUG: DespawnItemInRoom() cannot remove item {itemID} from Room {rid}, no such item present", LogLevel.Debug);
                return false;
            }
            if (all)
            {
                bool OK = true;
                while (RoomManager.Instance.GetRoom(rid).ItemsInRoom.Values.Any(x => x.ID == itemID))
                {
                    var i = RoomManager.Instance.GetRoom(rid).ItemsInRoom.FirstOrDefault(x => x.Value.ID == itemID);
                    if (!RoomManager.Instance.RemoveItemFromRoomInventory(rid, i.Value))
                    {
                        Game.LogMessage($"ERROR: Failed to remove item {itemID} from Room {rid} in DespawnItemInRoom()", LogLevel.Error);
                        OK = false;
                    }
                }
                return OK;
            }
            else
            {
                var i = RoomManager.Instance.GetRoom(rid).ItemsInRoom.FirstOrDefault(x => x.Value.ID == itemID);
                return RoomManager.Instance.RemoveItemFromRoomInventory(rid, i.Value);
            }
        }

        public static bool DespawnNPCInRoom(int rid, int npcID, bool all)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: DespawnNPCInRoom() was called with a RID that was not found in Room Manager", LogLevel.Debug);
                return false;
            }
            if (!NPCManager.Instance.GetNPCsInRoom(rid).Any(x => x.TemplateID == npcID))
            {
                Game.LogMessage($"DEBUG: DespawnNPCInRoom() cannot remove NPC {npcID} from Room {rid}, no such NPC present", LogLevel.Debug);
                return false;
            }
            if (all)
            {
                bool OK = true;
                while (NPCManager.Instance.GetNPCsInRoom(rid).Any(x => x.TemplateID == npcID))
                {
                    var n = NPCManager.Instance.GetNPCsInRoom(rid).FirstOrDefault(x => x.TemplateID == npcID);
                    if (!NPCManager.Instance.RemoveNPCInstance(n.ID))
                    {
                        Game.LogMessage($"ERROR: DespawnNPCINRoom() failed to remove NPC {npcID} with Instance ID {n.ID}", LogLevel.Error);
                        OK = false;
                    }
                }
                return OK;
            }
            else
            {
                var n = NPCManager.Instance.GetNPCsInRoom(rid).FirstOrDefault(x => x.TemplateID == npcID);
                return NPCManager.Instance.RemoveNPCInstance(n.ID);
            }
        }

        public static void TeleportPlayer(int rid, string playerID, int dest)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: TeleportPlayer() was called with a RID that was not found in Room Manager", LogLevel.Debug);
                return;
            }
            if (!RoomManager.Instance.RoomExists(dest))
            {
                Game.LogMessage($"DEBUG: TeleportPlayer() was called with a dest that was not found in Room Manager", LogLevel.Debug);
                return;
            }
            if (!Guid.TryParse(playerID, out var gPlayerId))
            {
                Game.LogMessage($"DEBUG: TeleportPlayer() was called with a player ID that could not be parsed to a GUID", LogLevel.Debug);
                return;
            }
            var pSession = SessionManager.Instance.GetSession(gPlayerId);
            if (pSession == null)
            {
                Game.LogMessage($"DEBUG: TeleportPlayer() was called with a player ID that could not be matched in Session Manager", LogLevel.Debug);
                return;
            }
            if (!pSession.Player.CanMove())
            {
                Game.LogMessage($"DEBUG: TeleportPlayer() cannot teleport {pSession.Player.Name} to Room {dest}: the player cannot move", LogLevel.Debug);
                return;
            }
            pSession.Player.Move(dest, true);
        }

        public static void TeleportItem(int rid, int itemID, int dest)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: TeleportItem() was called with a RID that was not found in Room Manager", LogLevel.Debug);
                return;
            }
            if (!RoomManager.Instance.RoomExists(dest))
            {
                Game.LogMessage($"DEBUG: TeleportItem() was called with a dest that was not found in Room Manager", LogLevel.Debug);
                return;
            }
            var i = RoomManager.Instance.GetRoom(rid).GetItem(itemID);
            if (i == null)
            {
                Game.LogMessage($"DEBUG: TeleportItem() was called with an Item ID that was not found in RID {rid}", LogLevel.Debug);
                return;
            }
            RoomManager.Instance.RemoveItemFromRoomInventory(rid, i);
            RoomManager.Instance.AddItemToRoomInventory(dest, i);
        }

        public static void TeleportNPC(int rid, string npcID, int dest)
        {
            if (!RoomManager.Instance.RoomExists(rid))
            {
                Game.LogMessage($"DEBUG: TeleportNPC() was called with a RID that was not found in Room Manager", LogLevel.Debug);
                return;
            }
            if (!RoomManager.Instance.RoomExists(dest))
            {
                Game.LogMessage($"DEBUG: TeleportNPC() was called with a dest that was not found in Room Manager", LogLevel.Debug);
                return;
            }
            if (!Guid.TryParse(npcID, out Guid gNPCid))
            {
                Game.LogMessage($"DEBUG: TeleportNPC() was called with an NPC ID that could not be parsed to a GUID", LogLevel.Debug);
                return;
            }
            var npc = NPCManager.Instance.GetNPC(gNPCid);
            if (npc == null)
            {
                Game.LogMessage($"DEBUG: TeleportNPC() was called with an NPC ID that could not be matched in NPCManager", LogLevel.Debug);
                return;
            }
            if (!npc.CanMove())
            {
                Game.LogMessage($"DEBUG: TeleportNPC() cannot move NPC {npc.Name} to Room {dest}, the NPC cannot move", LogLevel.Debug);
                return;
            }
            npc.Move(dest, true);
        }
        #endregion

        #region General ScriptingObject Functions
        public static bool PlayerHasQuest(string player, string quest)
        {
            if (string.IsNullOrWhiteSpace(player) || string.IsNullOrWhiteSpace(quest))
            {
                Game.LogMessage($"DEBUG: PlayerHasQuest was called with a null or empty player or quest ID", LogLevel.Debug);
                return false;
            }
            if (!Guid.TryParse(player, out var pid))
            {
                Game.LogMessage($"DEBUG: PlayerHasQuest was called with a player ID that could not be transformed to a GUID", LogLevel.Debug);
                return false;
            }
            if (!Guid.TryParse(quest, out var qid))
            {
                Game.LogMessage($"DEBUG: PlayerHasQuest was called with a quest ID that could not be transformed to a GUID", LogLevel.Debug);
                return false;
            }
            if (!QuestManager.Instance.QuestExists(qid))
            {
                Game.LogMessage($"DEBUG: PlayerHasQuest was called with a quest ID that does match a quest in Quest Manager", LogLevel.Debug);
                return false;
            }
            var session = SessionManager.Instance.GetSession(pid);
            if (session == null)
            {
                Game.LogMessage($"DEBUG: PlayerHasQuest was called with a player ID that does not match the ID of a current session", LogLevel.Debug);
                return false;
            }
            return session.Player.ActiveQuests.ContainsKey(qid);
        }

        public static string GetCurrentTOD()
        {
            return Game.CurrentTOD.ToString();
        }

        public static string GetPreviousTOD()
        {
            return Game.PreviousTOD.ToString();
        }

        public static int RollDice(int num, int sides)
        {
            return Helpers.RollDice<int>(num, sides);
        }

        public static int GetRandomNumber(int start, int end)
        {
            return Helpers.GetRandomNumber(start, end);
        }

        public static ulong GetMudTick()
        {
            return Game.TickCount;
        }
        #endregion
    }
}
