﻿using System;
using System.Linq;

namespace Etrea3.Core
{
    public static partial class ActMob
    {
        public static string MobProgGetRandomPlayerID(string mobID)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgGetRandomPlayerID called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgEmote called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgMove called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgTakeItem called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgDropItem called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgGiveItem called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgAttack called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgCastSpell called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgSay called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgYell called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgWhisper called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a null Mob ID", LogLevel.Debug, true);
                return;
            }
            if (string.IsNullOrEmpty(playerID))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a null Player ID", LogLevel.Debug, true);
                return;
            }
            if (string.IsNullOrEmpty(destination))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a null destination", LogLevel.Debug, true);
                return;
            }
            if (!int.TryParse(destination, out int rid))
            {
                Game.LogMessage($"DEBUG: MobProgTeleportPlayer was called with a destination that was not an int", LogLevel.Debug, true);
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

        public static string MobProgGetPlayerName(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                Game.LogMessage($"DEBUG: MobProgGetPlayerName called with no args", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgRememberPlayer called with no Mob ID", LogLevel.Debug, true);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.Player.Name == args).Select(x => x.Player).FirstOrDefault();
            mob.RememberPlayer(player, Game.TickCount);
        }

        public static void MobProgForgetPlayer(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgForgetPlayer called with no Mob ID", LogLevel.Debug, true);
                return;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.Player.Name == args).Select(x => x.Player).FirstOrDefault();
            mob.ForgetPlayer(player);
        }

        public static bool MobProgRemembersPlayer(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgRemembersPlayer called with no Mob ID", LogLevel.Debug, true);
                return false;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.Player.Name == args).Select(x => x.Player).FirstOrDefault();
            return mob.RemembersPlayer(player, out _);
        }

        public static ulong MobProgGetRememberPlayerTickCount(string mobID, string args)
        {
            if (string.IsNullOrEmpty(mobID))
            {
                Game.LogMessage($"DEBUG: MobProgGetRememberPlayerTickCount called with no Mob ID", LogLevel.Debug, true);
                return 0;
            }
            var gMobID = Guid.Parse(mobID);
            var mob = NPCManager.Instance.GetNPC(gMobID);
            var player = SessionManager.Instance.ActivePlayers.Where(x => x.Player.Name == args).Select(x => x.Player).FirstOrDefault();
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
                Game.LogMessage($"DEBUG: MobProgMobHasItem called with no Mob ID", LogLevel.Debug, true);
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
                Game.LogMessage($"DEBUG: MobProgCheckPlayerIsImm called with no player ID", LogLevel.Debug, true);
                return false;
            }
            var gPlayerID = Guid.Parse(args);
            var p = SessionManager.Instance.GetSession(gPlayerID);
            return p.Player.IsImmortal;
        }
    }
}