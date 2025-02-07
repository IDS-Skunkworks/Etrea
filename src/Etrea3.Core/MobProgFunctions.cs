using Etrea3.Objects;
using System;
using System.Linq;

namespace Etrea3.Core
{
    public static partial class ActMob
    {
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

        public static bool MobProgItemInRoom(string mobID, string item)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgItemInRoom called with no Mob ID", LogLevel.Debug);
                return false;
            }
            if (string.IsNullOrEmpty(item))
            {
                Game.LogMessage($"DEBUG: MobProgItemInRoom called with no Item criteria", LogLevel.Debug);
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

        public static string MobProgGetRandomPlayerID(string mobID)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgGetRandomPlayerID called with no Mob ID", LogLevel.Debug);
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

        public static ulong MobProgGetMudTick()
        {
            return Game.TickCount;
        }

        public static int MobProgRollDice(int num, int size)
        {
            return Helpers.RollDice<int>(num, size);
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
    }
}