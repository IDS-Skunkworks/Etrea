using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Etrea3.Core
{
    public static class ActImmortal
    {
        public static void ShowFlags(Session session)
        {
            if (!session.Player.IsImmortal)
            {
                return;
            }
            session.Send($"%BYT%You have the following flags: {session.Player.Flags}{Constants.NewLine}");
        }

        public static void SnoopConnection(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to start a snooping session but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You must specify the name of a player or the ID of a connection!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.Snooping != Guid.Empty)
            {
                session.Send($"%BRT%You are already snooping!%PT%{Constants.NewLine}");
                return;
            }
            if (Guid.TryParse(arg, out Guid gReseult))
            {
                if (session.ID == gReseult)
                {
                    session.Send($"%BRT%You can't snoop yourself!%PT%{Constants.NewLine}");
                    return;
                }
                if (gReseult == Guid.Empty)
                {
                    session.Send($"%BRT%That isn't a valid connection ID!%PT%{Constants.NewLine}");
                    return;
                }
                var tSession = SessionManager.Instance.GetSession(gReseult);
                if (tSession == null)
                {
                    Game.LogMessage($"GOD: Player {session.Player.Name} tried to snoop connection {gReseult} but no matching session was found", LogLevel.God);
                    session.Send($"%BRT%No connection with that ID was found.%PT%{Constants.NewLine}");
                    return;
                }
                if (tSession.SetSnooper(session.ID, false, out string repl))
                {
                    session.Player.Snooping = gReseult;
                    session.Send($"%BGT%You are now snooping connection {gReseult}: use NOSNOOP to stop snooping.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} is now snooping connection {gReseult}", LogLevel.God);
                }
                else
                {
                    session.Send($"%BRT%Failed to snoop the connection: {repl}%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} failed to snoop connection {gReseult}: {repl}", LogLevel.God);
                }
            }
            else
            {
                var tSession = SessionManager.Instance.GetSession(arg);
                if (tSession == null)
                {
                    Game.LogMessage($"GOD: Player {session.Player.Name} tried to snoop connection {arg} but no matching session was found", LogLevel.God);
                    session.Send($"%BRT%No connection matching that name was found.%PT%{Constants.NewLine}");
                    return;
                }
                if (tSession.SetSnooper(session.ID, false, out string repl))
                {
                    session.Player.Snooping = tSession.ID;
                    session.Send($"%BGT%You are now snooping the connection of player {tSession.Player.Name}: use NOSNOOP to stop snooping.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} is now snooping the connection of player {tSession.Player.Name}", LogLevel.God);
                    return;
                }
                else
                {
                    session.Send($"%BRT%Failed to snoop the connection: {repl}%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} failed to snoop the connection of {tSession.Player.Name}: {repl}", LogLevel.God);
                }
            }
        }

        public static void StopSnoop(Session session)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to end a snoop session but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (session.Player.Snooping == Guid.Empty)
            {
                session.Send($"%BYT%You aren't snooping anyone!%PT%{Constants.NewLine}");
                return;
            }
            if (SessionManager.Instance.GetSession(session.Player.Snooping).SetSnooper(session.ID, true, out string reply))
            {
                Game.LogMessage($"GOD: Player {session.Player.Name} stopped snooping connection {session.Player.Snooping}", LogLevel.God);
                session.Player.Snooping = Guid.Empty;
                return;
            }
            else
            {
                Game.LogMessage($"GOD: Player {session.Player.Name} failed to stop snooping connection {session.Player.Snooping}: {reply}", LogLevel.God);
                return;
            }
        }

        public static void MutePlayer(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to mute someone but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to specify who you want to mute!%PT%{Constants.NewLine}");
                return;
            }
            var tPlayer = SessionManager.Instance.GetSession(arg);
            if (tPlayer == null)
            {
                session.Send($"%BRT%No player by that name could be found.%PT%{Constants.NewLine}");
                return;
            }
            if (tPlayer.ID == session.ID)
            {
                session.Send($"%BRT%You can't mute yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (tPlayer.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You can't mute someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (tPlayer.Player.Flags.HasFlag(PlayerFlags.Mute))
            {
                session.Send($"%BRT%{tPlayer.Player.Name} is already muted!%PT%{Constants.NewLine}");
                return;
            }
            tPlayer.Player.Flags |= PlayerFlags.Mute;
            session.Send($"%BGT%You have muted {tPlayer.Player.Name}, they will only be able to whisper to Immortals.%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} muted {tPlayer.Player.Name}", LogLevel.God);
            tPlayer.SendSystem($"%BMT%You have been muted by {session.Player.Name}. You will only be able to WHISPER to Immortals.%PT%{Constants.NewLine}");
        }

        public static void UnMutePlayer(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to unmute a player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to specify who you want to unmute!%PT%{Constants.NewLine}");
                return;
            }
            var tPlayer = SessionManager.Instance.GetSession(arg);
            if (tPlayer == null)
            {
                session.Send($"%BRT%No player with that name could be found.%PT%{Constants.NewLine}");
                return;
            }
            if (tPlayer.ID == session.ID)
            {
                session.Send($"%BRT%You can't unmute yourself!%PT%{Constants.NewLine}");
                Game.LogMessage($"GOD: Player {session.Player.Name} attempted to unmute themselves!", LogLevel.God);
                return;
            }
            if (tPlayer.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You can't unmute someone more powerful than yourself!%PT%{Constants.NewLine}");
                Game.LogMessage($"GOD: Player {session.Player.Name} attempted to unmute {tPlayer.Player.Name} but that person is more powerful", LogLevel.God);
                return;
            }
            if (!tPlayer.Player.Flags.HasFlag(PlayerFlags.Mute))
            {
                session.Send($"%BRT%{tPlayer.Player.Name} is not muted!%PT%{Constants.NewLine}");
                return;
            }
            tPlayer.Player.Flags &= ~PlayerFlags.Mute;
            session.Send($"%BGT%You have unmuted {tPlayer.Player.Name}!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} has unmuted {tPlayer.Player.Name}", LogLevel.God);
            tPlayer.SendSystem($"%BMT%{session.Player.Name} has unmuted you, you may now talk normally again!%PT%{Constants.NewLine}");
        }

        public static void FreezePlayer(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to freeze a player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: freeze <player> - prevent a player from doing anything%PT%{Constants.NewLine}");
                session.Send($"%BRT%freeze <player> <freeze time> - freeze a player for the number of minutes%PT%{Constants.NewLine}");
                session.Send($"%BRT%The target player must be online.%PT%{Constants.NewLine}");
                session.Send($"%BRT%If no time is specified the player will be frozen for 5 minutes%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            var targetPlayer = SessionManager.Instance.GetSession(args[0].Trim());
            if (targetPlayer == null)
            {
                session.Send($"%BRT%That person can't be found in the Realms right now...%PT%{Constants.NewLine}");
                return;
            }
            if (targetPlayer.ID == session.ID)
            {
                session.Send($"%BRT%You cannot freeze yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (targetPlayer.Player.Level >= session.Player.Level)
            {
                session.Send($"%BRT%You cannot freeze someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            int freezeMinutes = 5;
            if (args.Length == 2)
            {
                if (!int.TryParse(args[1].Trim(), out freezeMinutes))
                {
                    session.Send($"%BRT%If specifying a time, you must give a valid number of minutes!%PT%{Constants.NewLine}");
                    return;
                }
                if (freezeMinutes <= 0)
                {
                    session.Send($"%BRT%The freeze duration cannot be less than 1%PT%{Constants.NewLine}");
                    return;
                }
            }
            var thawTime = DateTime.UtcNow.AddMinutes(freezeMinutes);
            targetPlayer.Player.FreezePlayer(thawTime);
            targetPlayer.Send($"%BMT%{session.Player.Name} calls on divine power and freezes you to the spot!%PT%{Constants.NewLine}");
            session.Send($"%BYT%You have frozen {targetPlayer.Player.Name}. They will be able to act again after {thawTime:dd-MM-yyyy HH:mm:ss} UTC%PT%{Constants.NewLine}");
            session.Send($"%BYT%You can use the thaw command to relase them early.%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} has Frozen {targetPlayer.Player.Name} until {thawTime}", LogLevel.God);
        }

        public static void ThawPlayer(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to thaw a player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You must provide the name of the person to thaw. That person must be in the Realms.%PT%{Constants.NewLine}");
                return;
            }
            var targetPlayer = SessionManager.Instance.GetSession(arg.Trim());
            if (targetPlayer == null)
            {
                session.Send($"%BRT%That person could not be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (targetPlayer.Player.Level >= session.Player.Level)
            {
                session.Send($"%BRT%You cannot thaw someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            targetPlayer.Player.ThawPlayer();
            targetPlayer.Send($"%BYT%{session.Player.Name} calls on holy power and you can now move again!%PT%{Constants.NewLine}");
            session.Send($"%BYT%Calling on holy power, you free {targetPlayer.Player.Name} enabling them to move again!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: {session.Player.Name} has thawed {targetPlayer.Player.Name}", LogLevel.God);
        }

        public static void ZoneReset(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to perform a Zone Reset but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: zreset < zone ID | all | this >%PT%{Constants.NewLine}");
                session.Send($"%BRT%zreset 2 - Tick Zone 2%PT%{Constants.NewLine}");
                session.Send($"%BRT%zreset all - Tick all Zones%PT%{Constants.NewLine}");
                session.Send($"%BRT%zreset this - Tick the Zone you are currently in%PT%{Constants.NewLine}");
                return;
            }
            if (int.TryParse(arg, out int zid))
            {
                var tZone = ZoneManager.Instance.GetZone(zid);
                if (tZone == null)
                {
                    session.Send($"%BRT%No Zone with ID {zid} was found in Zone Manager.%PT%{Constants.NewLine}");
                    return;
                }
                Game.LogMessage($"GOD: Player {session.Player.Name} has forced Zone {tZone.ZoneName} (ID: {tZone.ZoneID}) to pulse", LogLevel.God);
                Task.Run(() =>
                {
                    tZone.PulseZone();
                });
                return;
            }
            if (arg.ToLower() == "all")
            {
                Game.LogMessage($"GOD: Player {session.Player.Name} has forced all Zones to pulse", LogLevel.God);
                ZoneManager.Instance.PulseAllZones();
                return;
            }
            if (arg.ToLower() == "this")
            {
                var z = ZoneManager.Instance.GetZoneForRID(session.Player.CurrentRoom);
                Game.LogMessage($"GOD: Player {session.Player.Name} has forced Zone {z.ZoneName} (ID: {z.ZoneID}) to pulse", LogLevel.God);
                Task.Run(() =>
                {
                    z.PulseZone();
                });
                return;
            }
            session.Send($"%BRT%Usage: zreset < zone ID | all | this >%PT%{Constants.NewLine}");
        }

        public static void ReleaseOLCLock(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release a lock in OLC but they are not Immortal", LogLevel.Warning);
                return;
            }
            // releaselock npc 10
            var args = arg.Split(' ');
            if (args.Length != 2)
            {
                session.Send($"%BRT%Usage: releaselock <type> <id> - force-release a lock in OLC%PT%{Constants.NewLine}");
                session.Send($"%BRT%Exmaple: releaselock item 10%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(args[1].Trim(), out int objID))
            {
                session.Send($"%BRT%That is not a valid ID.%PT%{Constants.NewLine}");
                return;
            }
            switch(args[0].Trim().ToLower())
            {
                case "item":
                    if (ItemManager.Instance.ItemExists(objID))
                    {
                        ItemManager.Instance.SetItemLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Item {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Item {objID} but no such Item was found", LogLevel.Warning);
                        session.Send($"%BRT%No Item with ID {objID} was found in Item Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "npc":
                    if (NPCManager.Instance.NPCTemplateExists(objID))
                    {
                        NPCManager.Instance.SetNPCLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of NPC {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of NPC {objID} but no such NPC was found", LogLevel.Warning);
                        session.Send($"%BRT%No NPC with ID {objID} was found in NPC Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "room":
                    if (RoomManager.Instance.RoomExists(objID))
                    {
                        RoomManager.Instance.SetRoomLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Room {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Room {objID} but no such Room was found", LogLevel.Warning);
                        session.Send($"%BRT%No Room with ID {objID} was found in Room Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "spell":
                    if (SpellManager.Instance.SpellExists(objID))
                    {
                        SpellManager.Instance.SetSpellLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Spell {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Spell {objID} but no such Spell was found", LogLevel.Warning);
                        session.Send($"%BRT%No Spell with ID {objID} was found in Spell Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "shop":
                    if (ShopManager.Instance.ShopExists(objID))
                    {
                        ShopManager.Instance.SetShopLockStatus(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Shop {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Shop {objID} but no such Shop was found", LogLevel.Warning);
                        session.Send($"%BRT%No Shop with ID {objID} was found in Shop Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "zone":
                    if (ZoneManager.Instance.ZoneExists(objID))
                    {
                        ZoneManager.Instance.SetZoneLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Zone {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Zone {objID} but no such Zone was found", LogLevel.Warning);
                        session.Send($"%BRT%No Zone with ID {objID} was found in Zone Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "quest":
                    if (QuestManager.Instance.QuestExists(objID))
                    {
                        QuestManager.Instance.SetQuestLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Quest {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Quest {objID} but no such Quest was found", LogLevel.Warning);
                        session.Send($"%BRT%No Quest with ID {objID} was found in Quest Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "recipe":
                case "craftingrecipe":
                    if (RecipeManager.Instance.RecipeExists(objID))
                    {
                        RecipeManager.Instance.SetRecipeLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of Crafting Recipe {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Crafting Recipe {objID} but no such Recipe was found", LogLevel.Warning);
                        session.Send($"%BRT%No Crafting Recipe with ID {objID} was found in Recipe Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "mobprog":
                    if (MobProgManager.Instance.MobProgExists(objID))
                    {
                        MobProgManager.Instance.SetMobProgLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock of MobProg {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of MobProg {objID} but no such MobProg was found", LogLevel.Warning);
                        session.Send($"%BRT%No MobProg with ID {objID} was found in MobProg Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "node":
                case "resourcenode":
                case "rssnode":
                    if (NodeManager.Instance.NodeExists(objID))
                    {
                        NodeManager.Instance.SetNodeLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock on Resource Node {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Resource Node {objID} but no such Node was found", LogLevel.Warning);
                        session.Send($"%BRT%No Resource Node with ID {objID} was found in Node Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                case "emote":
                    if (EmoteManager.Instance.EmoteExists(objID))
                    {
                        EmoteManager.Instance.SetEmoteLockState(objID, false, session);
                        Game.LogMessage($"GOD: Player {session.Player.Name} force-released the OLC lock on Emote {objID}", LogLevel.God);
                    }
                    else
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to release the OLC lock of Emote {objID} but no such Emote was found", LogLevel.Warning);
                        session.Send($"%BRT%No Emote with ID {objID} was found in Emote Manager.%PT%{Constants.NewLine}");
                    }
                    break;

                default:
                    Game.LogMessage($"DEBUG: releaselock called with unsupported asset type: {args[0]}", LogLevel.Debug);
                    session.Send($"%BRT%Unknown asset type: {args[0]}%PT%{Constants.NewLine}");
                    return;
            }
        }

        public static void CheckMobMemory(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to check an NPC's memory but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: checkmemory <target> - display a list of players the target NPC remembers%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(arg, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType != ActorType.NonPlayer)
            {
                session.Send($"%BRT%You can only use this power on NPCs!%PT%{Constants.NewLine}");
                return;
            }
            var n = (NPC)target;
            if (n.PlayersRemembered.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"%BYT%  {new string('=', 77)}");
                var ct = Game.TickCount;
                foreach(var pr in n.PlayersRemembered)
                {
                    sb.AppendLine($"%BYT%||%PT% {pr.Key} - {pr.Value} ({ct - pr.Value})");
                }
                sb.AppendLine($"%BYT%  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            session.Send($"%BGT%{n.Name} doesn't remember anyone!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: {session.Player.Name} checked the memory of {n.Name} ({n.ID})", LogLevel.God);
        }

        public static void GenerateAPIKey(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to query or generate an API key, but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: apikey <query | generate> <player>%PT%{Constants.NewLine}");
                session.Send($"%BRT%apikey query <player> will show the API key for the player, if there is one%PT%{Constants.NewLine}");
                session.Send($"%BRT%apikey generate <player> will generate an API key for the player, if that player is Immortal%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length != 2)
            {
                session.Send($"%BRT%Usage: apikey <query | generate> <player>%PT%{Constants.NewLine}");
                session.Send($"%BRT%apikey query <player> will show the API key for the player, if there is one%PT%{Constants.NewLine}");
                session.Send($"%BRT%apikey generate <player> will generate an API key for the player, if that player is Immortal%PT%{Constants.NewLine}");
                return;
            }
            switch(args[0].Trim().ToLower())
            {
                case "query":
                    if (session.Player.Level < 110)
                    {
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to query the API Key of player {args[1]} but they are not Level 110", LogLevel.Warning);
                        session.Send($"%BRT%You aren't a high enough level to do that!%PT%{Constants.NewLine}");
                        return;
                    }
                    var key = DatabaseManager.GetPlayerAPIKey(args[1]);
                    if (string.IsNullOrEmpty(key))
                    {
                        session.Send($"%BRT%That player does not exist or does not have an API Key!%PT%{Constants.NewLine}");
                        Game.LogMessage($"INFO: Player {session.Player.Name} queried the API key for {args[1]} but no player or key was found", LogLevel.Info);
                        return;
                    }
                    session.Send($"%BGT%API Key: {key}%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} queried the API key for {args[1]}", LogLevel.God);
                    break;

                case "generate":
                    var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.Player.Name.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();
                    if (target == null)
                    {
                        session.Send($"%BRT%That person isn't here!%PT%{Constants.NewLine}");
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to generate an API key for {args[1]} but no such player was found", LogLevel.Warning);
                        return;
                    }
                    if (!target.Player.IsImmortal)
                    {
                        session.Send($"%BRT%{target.Player.Name} is not Immortal!%PT%{Constants.NewLine}");
                        Game.LogMessage($"WARN: Player {session.Player.Name} attempted to generate an API key for {target.Player.Name} but that player is not Immortal", LogLevel.Warning);
                        return;
                    }
                    using (SHA512 sha = SHA512.Create())
                    {
                        var nBytes = Encoding.UTF8.GetBytes(target.Player.Name);
                        var hash = sha.ComputeHash(nBytes);
                        var hashString = Convert.ToBase64String(hash);
                        if (DatabaseManager.UpdatePlayerAPIKey(target.Player.Name, hashString))
                        {
                            Game.LogMessage($"GOD: Player {session.Player.Name} generated an API key for {target.Player.Name}", LogLevel.God);
                            session.Send($"%BGT%API key for {target.Player.Name} generated successfully.%PT%{Constants.NewLine}");
                        }
                        else
                        {
                            Game.LogMessage($"%ERROR: Player {session.Player.Name} encountered an error generating an API key for {target.Player.Name}", LogLevel.Error);
                            session.Send($"%BRT%Failed to create an API key for {target.Player.Name}, check error logs for more details.%PT%{Constants.NewLine}");
                        }
                    }
                    break;

                default:
                    session.Send($"%BRT%Usage: apikey <query | generate> <player>%PT%{Constants.NewLine}");
                    session.Send($"%BRT%apikey query <player> will show the API key for the player, if there is one%PT%{Constants.NewLine}");
                    session.Send($"%BRT%apikey generate <player> will generate an API key for the player, if that player is Immortal%PT%{Constants.NewLine}");
                    return;
            }
        }

        public static void RemoveNodeFromRoom(Session session)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to destroy the Resource Node in Room {session.Player.CurrentRoom} but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).RSSNode == null)
            {
                session.Send($"%BRT%There is no Resource Node here to destroy!%PT%{Constants.NewLine}");
                return;
            }
            RoomManager.Instance.GetRoom(session.Player.CurrentRoom).RSSNode = null;
            session.Send($"%BYT%Calling on mystic energies you destroy the resource node in this area!%PT%{Constants.NewLine}");
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
            Game.LogMessage($"GOD: Player {session.Player.Name} destroyed the Resource Node in Room {session.Player.CurrentRoom}", LogLevel.God);
            if (localPlayers.Count > 0)
            {
                foreach (var player in localPlayers)
                {
                    var msg = session.Player.CanBeSeenBy(player.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesture and the resource node is swallowed by the Winds of Magic!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic swirl, swallowing the resource node!%PT%{Constants.NewLine}";
                    player.Send(msg);
                }
            }
        }

        public static void AddNodeToRoom(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to create a Resource Node in Room {session.Player.CurrentRoom} but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).RSSNode != null)
            {
                session.Send($"%BRT%There is already a Node here!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg) || !int.TryParse(arg.Trim(), out int nodeID))
            {
                session.Send($"%BRT%That isn't a valid Node ID!%PT%{Constants.NewLine}");
                return;
            }
            if (!NodeManager.Instance.NodeExists(nodeID))
            {
                session.Send($"%BRT%No Resource Node with that ID was found in Node Manager!%PT%{Constants.NewLine}");
                return;
            }
            var n = NodeManager.Instance.GetNode(nodeID);
            if (n != null)
            {
                var newNode = Helpers.Clone(n);
                newNode.Depth = Helpers.RollDice<int>(1, 4);
                RoomManager.Instance.GetRoom(session.Player.CurrentRoom).RSSNode = newNode;
                session.Send($"%BYT%You have create a {newNode.Name} here to mine!%PT%{Constants.NewLine}");
                Game.LogMessage($"GOD: Player {session.Player.Name} created Resource Node {newNode.Name} ({newNode.ID}) in Room {session.Player.CurrentRoom}", LogLevel.God);
                var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
                if (localPlayers.Count > 0)
                {
                    foreach (var player in localPlayers)
                    {
                        var msg = session.Player.CanBeSeenBy(player.Player) ? $"%BYT%{session.Player} makes an arcane gesture and creates {newNode.Name}!%PT%{Constants.NewLine}" :
                            $"%BYT%The Winds of Magic shift about and create {newNode.Name}!%PT%{Constants.NewLine}";
                        player.Send(msg);
                    }
                }
            }
        }

        public static void ShowShopInfo(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to review Shop information but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: shopinfo <id>%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(arg, out int shopID))
            {
                session.Send($"%BRT%Usage: shopinfo <id>%PT%{Constants.NewLine}");
                return;
            }
            var shop = ShopManager.Instance.GetShop(shopID);
            if (shop == null)
            {
                session.Send($"%BRT%No Shop with that ID could be found in Shop Manager.%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%  {new string('=', 77)}");
            sb.AppendLine($"%BYT%|| Name: {shop.ShopName}{Constants.TabStop}ID: {shop.ID}");
            sb.AppendLine($"%BYT%|| Gold: {shop.CurrentGold:N0} / {shop.BaseGold:N0}");
            if (shop.CurrentInventory.Count > 0)
            {
                sb.AppendLine($"%BYT%|| Current Inventory:");
                foreach(var i in shop.CurrentInventory)
                {
                    var item = ItemManager.Instance.GetItem(i.Key);
                    if (item == null)
                    {
                        sb.AppendLine($"%BYT%|| {i.Value} x Unknown Item ({i.Key})");
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| {i.Value} x {item.Name} ({item.ID})");
                    }
                }
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}");
            session.Send(sb.ToString());
            Game.LogMessage($"GOD: Player {session.Player.Name} reviewed stats for Shop {shop.ID}", LogLevel.God);
        }

        public static void TickShop(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to force a Shop refresh but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: tickshop <id>%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(arg, out int shopID))
            {
                session.Send($"%BRT%Usage: tickshop <id>%PT%{Constants.NewLine}");
                return;
            }
            if (!ShopManager.Instance.ShopExists(shopID))
            {
                session.Send($"%BRT%No Shop with that ID could be found in Shop Manager.%PT%{Constants.NewLine}");
                return;
            }
            var shop = ShopManager.Instance.GetShop(shopID);
            Game.LogMessage($"GOD: Player {session.Player.Name} forced a refresh of Shop {shop.ShopName}", LogLevel.God);
            shop.RestockShop();
            session.Send($"%BGT%Shop {shop.ShopName} refreshed succesfully.%PT%{Constants.NewLine}");
        }

        public static void ShutDownGame(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to initiate a shutdown but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (session.Player.Level < 110)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to initiate a shutdown but they are not Level 110", LogLevel.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(arg) && bool.TryParse(arg, out bool force))
            {
                Game.LogMessage($"GOD: {session.Player.Name} has initiated a shutdown", LogLevel.God);
                Game.ImmShutdown(session, force);
            }
            else
            {
                session.Send($"%BRT%Usage: shutdown <true | false>%PT%{Constants.NewLine}");
                session.Send($"%BRT%true = shutdown even if all players cannot be saved.%PT%{Constants.NewLine}");
                session.Send($"%BRT%false = abort shutdown if all players cannot be saved.%PT%{Constants.NewLine}");
            }
        }

        public static void MessageOfTheDay(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to perform an MOTD operation but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                var motd = DatabaseManager.GetMOTD();
                if (string.IsNullOrEmpty(motd))
                {
                    session.Send($"%BYT%No MOTD has been configured.%PT%{Constants.NewLine}");
                }
                else
                {
                    session.Send($"%BYT%Current MOTD:{Constants.NewLine}");
                    session.Send(motd);
                }
                return;
            }
            if (arg.ToLower() == "clear")
            {
                if (DatabaseManager.ClearMOTD())
                {
                    session.Send($"%BYT%MOTD has been cleared.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} has cleared the Message of The Day", LogLevel.God);
                }
                else
                {
                    session.Send($"%BRT%Unable to clear the Message of the Day.%PT%{Constants.NewLine}");
                }
                return;
            }
            if (arg.ToLower() == "set")
            {
                var motd = Helpers.GetMOTD(session);
                if (string.IsNullOrEmpty(motd))
                {
                    session.Send($"%BRT%MOTD was empty. If you want to clear the MOTD use motd clear%PT%{Constants.NewLine}");
                }
                else
                {
                    if (DatabaseManager.SetMOTD(motd))
                    {
                        session.Send($"%BYT%Message of the Day updated successfully.%PT%{Constants.NewLine}");
                        Game.LogMessage($"GOD: Player {session.Player.Name} has updated the Message of The Day", LogLevel.God);
                    }
                    else
                    {
                        session.Send($"%BRT%Could not update Message of the Day.%PT%{Constants.NewLine}");
                    }
                }
                return;
            }
            session.Send($"%BRT%Usage: motd - show the current MOTD%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: motd clear - clear the current MOTD%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: motd set - set a new MOTD%PT%{Constants.NewLine}");
        }

        public static void ShowBackupInfo(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to perform a backup operation but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                if (Game.GetBackupInfo(out var backupTime, out var backupTimer))
                {
                    var nextBackup = backupTime.AddSeconds(backupTimer);
                    session.Send($"%BYT%Last Bacup: {backupTime}%PT%{Constants.NewLine}");
                    session.Send($"%BYT%Next Backup: {nextBackup}%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} queried the World backup times", LogLevel.God);
                }
                else
                {
                    session.Send($"%BYT%The World has not yet been backed up. The next Backup is due {Game.StartTime.AddSeconds(backupTimer)}%PT%{Constants.NewLine}");
                    return;
                }
            }
            if (arg.ToLower() == "backupnow")
            {
                Game.BackupNow();
                Game.LogMessage($"GOD: Player {session.Player.Name} triggered a backup of World databases", LogLevel.God);
                return;
            }
            session.Send($"%BRT%Usage: backupinfo - show backup information%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: backupinfo backupnow - force an immediate backup%PT%{Constants.NewLine}");
        }

        public static void ToggleImmInvis(Session session)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to use ImmInvis but they are not Immortal", LogLevel.Warning);
                return;
            }
            session.Player.Visible = !session.Player.Visible;
            var msg = session.Player.Visible ? $"%BYT%You become visible again.%PT%{Constants.NewLine}" : $"%BYT%You slowly fade from view...%PT%{Constants.NewLine}";
            session.Send(msg);
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
            if (localPlayers != null && localPlayers.Count > 0)
            {
                var lpMsg = session.Player.Visible ? $"%BYT%The air shimmers as {session.Player.Name} becomes visible.%PT%{Constants.NewLine}" :
                    $"The air shimmers and {session.Player.Name} vanishes!%PT%{Constants.NewLine}";
                foreach(var lp in localPlayers)
                {
                    lp.Send(lpMsg);
                }
            }
        }

        public static void ShowUpTime(Session session)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to query uptime but they are not Immortal", LogLevel.Warning);
                return;
            }
            var upTime = DateTime.UtcNow - Game.StartTime;
            session.Send($"%BYT%The World came to life on {Game.StartTime} and has been up for {upTime.Days} day(s), {upTime.Hours:00}:{upTime.Minutes:00}:{upTime.Seconds:00}%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} queried the age of the Realms", LogLevel.God);
        }

        public static void SetDonationRoom(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to change the Donation Room but they are not Immortal", LogLevel.Warning);
                return;
            }
            var args = arg.Split(' ');
            if (args.Length == 1)
            {
                session.Send($"%BYT%The current Donation Room is {Game.DonationRoomID}%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(args[1].Trim(), out int rid))
            {
                session.Send($"%BRT%That is not a valid setting for the Donation Room.%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: donroom - show the current Donation Room%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: donroom 200 - set the Donation Room to Room 200%PT%{Constants.NewLine}");
                return;
            }
            if (!RoomManager.Instance.RoomExists(rid))
            {
                session.Send($"%BRT%The specified Room does not exist.%PT%{Constants.NewLine}");
                return;
            }
            Game.LogMessage($"GOD: Player {session.Player.Name} has changed the Donation Room from {Game.DonationRoomID} to {rid}", LogLevel.God);
            Game.SetDonationRoom(rid);
            session.Send($"%BYT%The Donation Room has been updated successfully.%PT%{Constants.NewLine}");
        }

        public static void LogAction(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to perform a log action but they are not Immortal", LogLevel.Warning);
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 1)
            {
                session.Send($"%BRT%Usage: log read <info | connection | combat | warn | error | debug | shop | god | olc> <amount>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: log clear%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: log read olc 10%PT%{Constants.NewLine}");
                return;
            }
            if (args[0].Trim().ToLower() == "clear")
            {
                if (DatabaseManager.ClearLogTable(out int rCount))
                {
                    session.Send($"%BYT%Logs cleared, {rCount:N0} entries removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} cleared the World Log", LogLevel.God);
                }
                else
                {
                    session.Send($"BRT%Failed to clear the Logs table.%PT%{Constants.NewLine}");
                }
                return;
            }
            if (args[0].Trim().ToLower() == "read")
            {
                if (args.Length < 3)
                {
                    session.Send($"%BRT%Usage: log read <type> <amount>%PT%{Constants.NewLine}");
                    return;
                }
                var lType = args[1].Trim().ToLower();
                if (!int.TryParse(args[2].Trim(), out int lAmount))
                {
                    session.Send($"%BRT%That is not a valid value for amount.%PT%{Constants.NewLine}");
                    return;
                }
                var results = DatabaseManager.GetLogEntries(lType, lAmount);
                if (results == null || results.Count == 0)
                {
                    session.Send($"%BRT%No Log Entries found.%PT%{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                foreach(var log in results)
                {
                    sb.AppendLine($"%BYT%||%PT% {log.LogDate} - {log.LogMessage}");
                }
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                session.Send(sb.ToString());
                Game.LogMessage($"GOD: Player {session.Player.Name} queried the World Log", LogLevel.God);
                return;
            }
            session.Send($"%BRT%Usage: log read <info | connection | combat | warn | error | debug | olc> <amount>%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: log clear%PT%{Constants.NewLine}");
            session.Send($"%BRT%Example: log read olc 10%PT%{Constants.NewLine}");
            return;
        }

        public static void AddPlayerLanguage(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to modify the languages of another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: addlang <player> <language>%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: addlang <player> <language>%PT%{Constants.NewLine}");
                return;
            }
            var tPlayer = SessionManager.Instance.GetSession(args[0].Trim());
            if (tPlayer == null)
            {
                session.Send($"%BRT%That person cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            var argLang = arg.Remove(0, tPlayer.Player.Name.Length).Trim();
            var lang = Constants.Languages.FirstOrDefault(x => x.IndexOf(argLang, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrEmpty(lang))
            {
                session.Send($"%BRT%You can't add a language that doesn't exist!%PT%{Constants.NewLine}");
                return;
            }
            if (tPlayer.Player.KnowsLanguage(lang))
            {
                session.Send($"%BRT%{tPlayer.Player.Name} already knows the {lang} language!%PT%{Constants.NewLine}");
                return;
            }
            tPlayer.Player.AddLanguage(lang);
            var msg = session.Player.CanBeSeenBy(tPlayer.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic and grants you the ability to use the {lang} language!%PT%{Constants.NewLine}" :
                $"%BYT%Something calls upon the Winds of Magic and you find you can suddenly use the {lang} langauge!%PT%{Constants.NewLine}";
            tPlayer.Send(msg);
            session.Send($"%BYT%You have granted {tPlayer.Player.Name} the ability to use the {lang} language!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: {session.Player} has added the {lang} language to {tPlayer.Player.Name}", LogLevel.God);
        }

        public static void RemovePlayerLanguage(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to modify the languages of another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: removelang <player> <language>%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: removelang <player> <language>%PT%{Constants.NewLine}");
                return;
            }
            var tPlayer = SessionManager.Instance.GetSession(args[0].Trim());
            if (tPlayer == null)
            {
                session.Send($"%BRT%That person cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            var argLang = arg.Remove(0, tPlayer.Player.Name.Length).Trim();
            var lang = Constants.Languages.FirstOrDefault(x => x.IndexOf(argLang, StringComparison.OrdinalIgnoreCase) > 0);
            if (string.IsNullOrEmpty(lang))
            {
                session.Send($"%BRT%You can't remove a language that doesn't exist!%PT%{Constants.NewLine}");
                return;
            }
            if (!tPlayer.Player.KnowsLanguage(lang))
            {
                session.Send($"%BRT%{tPlayer.Player.Name} doesn't know the {lang} language!%PT%{Constants.NewLine}");
                return;
            }
            tPlayer.Player.AddLanguage(lang);
            var msg = session.Player.CanBeSeenBy(tPlayer.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic and removes your ability to use the {lang} language!%PT%{Constants.NewLine}" :
                $"%BYT%Something calls upon the Winds of Magic and you find you can no longer use the {lang} langauge!%PT%{Constants.NewLine}";
            tPlayer.Send(msg);
            session.Send($"%BYT%You have removed {tPlayer.Player.Name}'s ability to use the {lang} language!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: {session.Player} has removed the {lang} language from {tPlayer.Player.Name}", LogLevel.God);
        }

        public static void GiveRecipe(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to award a Recipe to another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: giverecipe <target> <recipe name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: giverecipe fred tower shield%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: giverecipe <target> <recipe name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: giverecipe fred tower shield%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            var recipeName = arg.Remove(0, args[0].Length).Trim();
            var recipe = RecipeManager.Instance.GetRecipe(recipeName).FirstOrDefault();
            if (recipe == null)
            {
                session.Send($"%BRT%No such Recipe exists in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot grant Recipes to those more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.AddRecipe(recipe.ID))
            {
                session.Send($"%BYT%You have granted the {recipe.Name} Recipe to {target.Player.Name}!%PT%{Constants.NewLine}");
                var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic to grant you knowledge of the {recipe.Name} Recipe!%PT%{Constants.NewLine}" :
                    $"%BYT%The Winds of Magic swirl about you, granting you the {recipe.Name} Recipe!%PT%{Constants.NewLine}";
                target.Send(msg);
                Game.LogMessage($"GOD: Player {session.Player.Name} has granted {target.Player.Name} the {recipe.Name} Recipe", LogLevel.God);
            }
            else
            {
                session.Send($"%BRT%You failed to grant the Recipe to {target.Player.Name}!%PT%{Constants.NewLine}");
            }
        }

        public static void RemoveRecipe(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"%WARN: Player {session.Player.Name} attempted to remove a Recipe from another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: removerecipe <target> <recipe name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: removerecipe fred tower shield%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: removerecipe <target> <recipe name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: removerecipe fred tower shield%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot remove Recipes from those more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            var recipeName = arg.Remove(0, args[0].Length).Trim();
            if (target.Player.RemoveRecipe(recipeName))
            {
                session.Send($"%BYT%You have removed the Recipe from {target.Player.Name}!%PT%{Constants.NewLine}");
                var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic to remove your knowledge of the {recipeName} Recipe!%PT%{Constants.NewLine}" :
                    $"%BYT%The Winds of Magic swirl about you, removing the knowledge of the {recipeName} Recipe!%PT%{Constants.NewLine}";
                target.Send(msg);
                Game.LogMessage($"GOD: Player {session.Player.Name} has removed the {recipeName} Recipe from {target.Player.Name}", LogLevel.God);
            }
            else
            {
                session.Send($"%BRT%You failed to remove that Spell from {target.Player.Name}!%PT%{Constants.NewLine}");
            }
        }

        public static void GiveSpell(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to award a Spell to another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: awardspell <target> <spell name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: awardspell fred magic missile%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: awardspell <target> <spell name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: awardspell fred magic missile%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            var spellName = arg.Remove(0, args[0].Length).Trim();
            var spell = SpellManager.Instance.GetSpell(spellName);
            if (spell == null)
            {
                session.Send($"%BRT%No such Spell exists within the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot grant Spells to those more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.AddSpell(spellName))
            {
                session.Send($"%BYT%You have granted the {spell.Name} Spell to {target.Player.Name}!%PT%{Constants.NewLine}");
                var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic to grant you knowledge of the {spell.Name} Spell!%PT%{Constants.NewLine}" :
                    $"%BYT%The Winds of Magic swirl about you, granting you the {spell.Name} Spell!%PT%{Constants.NewLine}";
                target.Send(msg);
                Game.LogMessage($"GOD: Player {session.Player.Name} has granted {target.Player.Name} the {spell.Name} Spell", LogLevel.God);
            }
            else
            {
                session.Send($"%BRT%You failed to grant that Spell to {target.Player.Name}, they may already have it!%PT%{Constants.NewLine}");
            }
        }

        public static void ShowImmInventory(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to view the inventory of someone else but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: imminv <player | npc> <target>%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: imminv <player | npc> <target>%PT%{Constants.NewLine}");
                return;
            }
            var targetType = args[0].Trim();
            var targetName = args[1].Trim();
            switch(targetType.ToLower())
            {
                case "player":
                    ShowPlayerInventory(session, targetName);
                    break;

                case "npc":
                    ShowNPCInventory(session, targetName);
                    break;

                default:
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} called ShowImmInventory() with unsupported TargetType: {targetType}", LogLevel.Debug);
                    session.Send($"%BRT%Only Players and NPCs can be a target of this power!%PT%{Constants.NewLine}");
                    return;
            }
        }

        public static void ShowImmCharSheet(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to view the character sheet of someone else but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: immstat <player | npc> <target>%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: immstat <player | npc> <target>%PT%{Constants.NewLine}");
                return;
            }
            var targetType = args[0].Trim();
            var targetName = args[1].Trim();
            switch (targetType.ToLower())
            {
                case "player":
                    ShowPlayerCharSheet(session, targetName);
                    break;

                case "npc":
                    ShowNPCCharSheet(session, targetName);
                    break;

                default:
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} called ShowImmCharSheet() with unsupported TargetType: {targetType}", LogLevel.Debug);
                    session.Send($"%BRT%Only Players and NPCs can be a target of this power!%PT%{Constants.NewLine}");
                    return;
            }
        }

        public static void RemoveSpell(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"%WARN: Player {session.Player.Name} attempted to remove a Spell from another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: removespell <target> <spell name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: removespell fred magic missile%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: removespell <target> <spell name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: removespell fred magic missile%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot remove Spells from those more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            var spellName = arg.Remove(0, args[0].Length).Trim();
            if (target.Player.RemoveSpell(spellName))
            {
                session.Send($"%BYT%You have removed the Spell from {target.Player.Name}!%PT%{Constants.NewLine}");
                var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic to remove your knowledge of the {spellName} Spell!%PT%{Constants.NewLine}" :
                    $"%BYT%The Winds of Magic swirl about you, removing the knowledge of the {spellName} Spell!%PT%{Constants.NewLine}";
                target.Send(msg);
                Game.LogMessage($"GOD: Player {session.Player.Name} has removed the {spellName} Spell from {target.Player.Name}", LogLevel.God);
            }
            else
            {
                session.Send($"%BRT%You failed to remove that Spell from {target.Player.Name}!%PT%{Constants.NewLine}");
            }
        }

        public static void RemoveSkill(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"%WARN: Player {session.Player.Name} attempted to remove a Skill from another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: removeskill <target> <skill name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: removeskill fred heavy armour%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: removeskill <target> <skill name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: removeskill fred heavy armour%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot remove Skills from those more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            var skillName = arg.Remove(0, args[0].Length).Trim();
            if (target.Player.RemoveSkill(skillName))
            {
                session.Send($"%BYT%You have removed the Skill from {target.Player.Name}!%PT%{Constants.NewLine}");
                var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic to remove your knowledge of the {skillName} Skill!%PT%{Constants.NewLine}" : 
                    $"%BYT%The Winds of Magic swirl about you, removing the knowledge of the {skillName} Skill!%PT%{Constants.NewLine}";
                target.Send(msg);
                Game.LogMessage($"GOD: Player {session.Player.Name} has removed the {skillName} Skill from {target.Player.Name}", LogLevel.God);
            }
            else
            {
                session.Send($"%BRT%You failed to remove that Skill from {target.Player.Name}!%PT%{Constants.NewLine}");
            }
        }

        public static void GiveSkill(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to award a Skill to another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: awardskill <target> <skill name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: awardskill fred heavy armour%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: awardskill <target> <spell name>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: awardskill fred heavy armour%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            var skillName = arg.Remove(0, args[0].Length).Trim();
            var skill = SkillManager.Instance.GetSkill(skillName);
            if (skill == null)
            {
                session.Send($"%BRT%No such Skill exists within the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot grant Skills to those more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.AddSkill(skillName))
            {
                session.Send($"%BYT%You have granted the {skill.Name} Skill to {target.Player.Name}!%PT%{Constants.NewLine}");
                var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls upon the Winds of Magic to grant you the {skill.Name} Skill!%PT%{Constants.NewLine}" :
                    $"%BYT%The Winds of Magic swirl about you, granting you the {skill.Name} Skill!%PT%{Constants.NewLine}";
                target.Send(msg);
                Game.LogMessage($"GOD: Player {session.Player.Name} has granted {target.Player.Name} the {skill.Name} Skill", LogLevel.God);
            }
            else
            {
                session.Send($"%BRT%You failed to grant that Skill to {target.Player.Name}, they may already have it!%PT%{Constants.NewLine}");
            }
        }

        public static void ImmHeal(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to use ImmHeal to restore someone but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: immheal <target>%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(arg);
            if (target == null)
            {
                session.Send($"%BRT%You can't restore someone that isn't in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            target.Player.Restore();
            var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} channels the Winds of Magic through you, restoring you!%PT%{Constants.NewLine}" :
                $"%BYT%You feel the Winds of Magic course through you, restoring you!%PT%{Constants.NewLine}";
            target.Send(msg);
            session.Send($"%BYT%You restore {target.Player.Name}!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} Restored {target.Player.Name}", LogLevel.God);
        }

        public static void ChangeExp(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to modify the Exp of another player but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: addexp <target> <amount>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: addexp fred 1000%PT%");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: addexp <target> <amount>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: addexp fred 1000%PT%");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0]);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot modify the Exp of someone more powerful than yourself.%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(args[1], out int exp))
            {
                session.Send($"%BRT%That is not a valid value for Exp.%PT%{Constants.NewLine}");
                return;
            }
            exp = Math.Max(exp, 0);
            target.Player.AdjustExp(exp, true, false);
            var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%Calling on the Winds of Magic, {session.Player.Name} grants you {exp} Exp!%PT%{Constants.NewLine}" :
                $"%BYT%The Winds of Magic swirl about you, granting {exp} Exp!%PT%{Constants.NewLine}";
            target.Send(msg);
            session.Send($"%BYT%You have granted {target.Player.Name} {exp} Exp!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} granted {target.Player.Name} {exp} Exp", LogLevel.God);
        }

        public static void VoiceOfGod(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to use the Voice of God, but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to have something to say in order to use the Voice of God!%PT%{Constants.NewLine}");
                return;
            }
            SessionManager.Instance.SendToAllPlayers($"An ethereal voice echoes across the Realms saying \"{arg}\"{Constants.NewLine}");
        }

        public static void FindAsset(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to locate something in the Realms but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: <where | find> <item | npc> <id | criteria>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: where item 10%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: where npc fred%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: <where | find> <item | npc> <id | criteria>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: where item 10%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: where npc fred%PT%{Constants.NewLine}");
                return;
            }
            var assetType = args[0].Trim();
            var targetString = arg.Remove(0, assetType.Length).Trim();
            switch(assetType.ToLower())
            {
                case "item":
                    FindItemInWorld(session, targetString);
                    break;

                case "npc":
                    FindNPCInWorld(session, targetString);
                    break;

                default:
                    session.Send($"%BRT%You can only use this power to find Items or NPCs!%PT%{Constants.NewLine}");
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} called FindAsset() with unspported asset type: {assetType}", LogLevel.Debug);
                    return;
            }
        }

        public static void SetActorAttribute(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to modify attributes of another actor but they are not Immortal", LogLevel.Warning);
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 3)
            {
                session.Send($"%BRT%Usage: set <target> <attribute> <value>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: set fred currenthp 100%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0].Trim());
            if (target == null)
            {
                session.Send($"%BRT%You cannot change the attributes of someone that doesn't exist!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot change the attributes of someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            switch (args[1].Trim().ToLower())
            {
                case "hp":
                case "currenthp":
                    if (!int.TryParse(args[2], out int hpVal))
                    {
                        session.Send($"%BRT%That is not a valid value for HP!%PT%{Constants.NewLine}");
                        return;
                    }
                    hpVal = Math.Max(hpVal, 1);
                    target.Player.CurrentHP = hpVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Current HP to {hpVal:N0}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s HP to {hpVal}", LogLevel.God);
                    var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic to change your HP!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and your HP changes!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "mp":
                case "currentmp":
                    if (!int.TryParse(args[2], out int mpVal))
                    {
                        session.Send($"%BRT%That is not a valid value for MP!%PT%{Constants.NewLine}");
                        return;
                    }
                    mpVal = Math.Max(mpVal, 1);
                    target.Player.CurrentMP = mpVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Current MP to {mpVal:N0}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s MP to {mpVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic to change your MP!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and your MP changes!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "maxhp":
                    if (!int.TryParse(args[2], out int mhpVal))
                    {
                        session.Send($"%BRT%That is not a valid value for HP!%PT%{Constants.NewLine}");
                        return;
                    }
                    mhpVal = Math.Max(mhpVal, 1);
                    target.Player.MaxHP = mhpVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Max HP to {mhpVal:N0}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Max HP to {mhpVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic to change your Max HP!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and your Max HP changes!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "maxmp":
                    if (!int.TryParse(args[2], out int mmpVal))
                    {
                        session.Send($"%BRT%That is not a valid value for MP!%PT%{Constants.NewLine}");
                        return;
                    }
                    mmpVal = Math.Max(mmpVal, 1);
                    target.Player.MaxMP = mmpVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Max MP to {mmpVal:N0}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Max MP to {mmpVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic to change your Max MP!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and your Max MP changes!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "sp":
                case "currentsp":
                    if (!int.TryParse(args[2], out int spVal))
                    {
                        session.Send($"%BRT%That is not a valid value for SP!%PT%{Constants.NewLine}");
                        return;
                    }
                    spVal = Math.Max(spVal, 1);
                    target.Player.CurrentSP = spVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Current SP to {spVal:N0}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s SP to {spVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic to change your SP!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and your SP changes!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "maxsp":
                    if (!int.TryParse(args[2], out int mspVal))
                    {
                        session.Send($"%BRT%That is not a valid value for SP!%PT%{Constants.NewLine}");
                        return;
                    }
                    mspVal = Math.Max(mspVal, 1);
                    target.Player.MaxSP = mspVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Max SP to {mspVal:N0}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Max SP to {mspVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic to change your Max SP!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and your Max SP changes!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "str":
                case "strength":
                    if (!int.TryParse(args[2], out int strVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Strength!%PT%{Constants.NewLine}");
                        return;
                    }
                    strVal = Math.Max(strVal, 1);
                    target.Player.Strength = strVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Strength to {strVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Strength to {strVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Strength changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Strength changing...%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "dex":
                case "dexterity":
                    if (!int.TryParse(args[2], out int dexVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Dexterity!%PT%{Constants.NewLine}");
                        return;
                    }
                    dexVal = Math.Max(dexVal, 1);
                    target.Player.Dexterity = dexVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Dexterity to {dexVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Dexterity to {dexVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Dexterity changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Dexterity changing...%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "con":
                case "constitution":
                    if (!int.TryParse(args[2], out int conVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Constitution!%PT%{Constants.NewLine}");
                        return;
                    }
                    conVal = Math.Max(conVal, 1);
                    target.Player.Constitution = conVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Constitution to {conVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Constitution to {conVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Constitution changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Constitution changing...%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "int":
                case "intelligence":
                    if (!int.TryParse(args[2], out int intVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Intelligence!%PT%{Constants.NewLine}");
                        return;
                    }
                    intVal = Math.Max(intVal, 1);
                    target.Player.Intelligence = intVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Intelligence to {intVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Intelligence to {intVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Intelligence changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Intelligence changing...%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "wis":
                case "wisdom":
                    if (!int.TryParse(args[2], out int wisVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Wisdom!%PT%{Constants.NewLine}");
                        return;
                    }
                    wisVal = Math.Max(wisVal, 1);
                    target.Player.Wisdom = wisVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Wisdom to {wisVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Wisdom to {wisVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Wisdom changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Wisdom changing...%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "cha":
                case "charisma":
                    if (!int.TryParse(args[2], out int chaVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Charisma!%PT%{Constants.NewLine}");
                        return;
                    }
                    chaVal = Math.Max(chaVal, 1);
                    target.Player.Charisma = chaVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Charisma to {chaVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Charisma to {chaVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Charisma changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Charisma changing...%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "gold":
                case "gp":
                    if (!ulong.TryParse(args[2], out ulong gpVal))
                    {
                        session.Send($"%BRT%That is not a valid value for Gold!%PT%{Constants.NewLine}");
                        return;
                    }
                    gpVal = Math.Max(gpVal, 0);
                    target.Player.Gold = gpVal;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Gold to {gpVal}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Gold to {gpVal}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Gold balance changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Gold balance changing!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                case "level":
                case "lvl":
                    if (!int.TryParse(args[2], out int lvl))
                    {
                        session.Send($"%BRT%That is not a valid value for Level!%PT%{Constants.NewLine}");
                        return;
                    }
                    if (lvl < 1 || lvl > 109)
                    {
                        session.Send($"%BRT%Level can only be set between 1 and 109.%PT%{Constants.NewLine}");
                        return;
                    }
                    if (target.Player.Level > session.Player.Level)
                    {
                        session.Send($"%BRT%You cannot set the Level of someone higher than yourself!%PT%{Constants.NewLine}");
                        return;
                    }
                    target.Player.Level = lvl;
                    session.Send($"%BYT%You have changed {target.Player.Name}'s Level to {lvl}.%PT%{Constants.NewLine}");
                    Game.LogMessage($"GOD: Player {session.Player.Name} changed {target.Player.Name}'s Level to {lvl}", LogLevel.God);
                    msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} summons the Winds of Magic and you feel your Level changing!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift and you feel your Level changing!%PT%{Constants.NewLine}";
                    target.Send(msg);
                    break;

                default:
                    session.Send($"%BRT%It doesn't look like you can change that... %PT%{Constants.NewLine}");
                    break;
            }
        }

        public static void TransferTarget(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to transport a target but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%That power requires a target!%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2 || !int.TryParse(args[1].Trim(), out int rid))
            {
                session.Send($"%BRT%Usage: transport <target> <room id>%PT%{Constants.NewLine}");
                return;
            }
            if (!RoomManager.Instance.RoomExists(rid))
            {
                session.Send($"%BRT%No Room with that ID was found in Room Manager.%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(args[0].Trim());
            if (target == null)
            {
                session.Send($"%BRT%You cannot transport someone that doesn't exist!%PT%{Constants.NewLine}");
                return;
            }
            if (!target.Player.CanMove())
            {
                session.Send($"%BRT%{target.Player.Name} cannot be teleported right now!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot transfer someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            session.Send($"%BYT%Calling on the Winds of Magic, you shift {target.Player.Name}'s position in the world!%PT%{Constants.NewLine}");
            var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} calls on the Winds of Magic and your find yourself transported!%PT%{Constants.NewLine}" :
                $"%BYT%You feel the Winds of Magic shift and you find yourself transported elsewhere!%PT%{Constants.NewLine}";
            target.Send(msg);
            Game.LogMessage($"GOD: Player {session.Player.Name} transported {target.Player.Name} from RID {target.Player.CurrentRoom} to {rid}", LogLevel.God);
            target.Player.Move(rid, true);
        }

        public static void ForceActor(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to FORCE another to perform an action but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%That power requires a target!%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: force <target> <action>%PT%{Constants.NewLine}");
                return;
            }
            var target = args[0].Trim();
            var actionString = arg.Remove(0, target.Length).Trim();
            if (string.IsNullOrEmpty(actionString))
            {
                session.Send($"%BRT%You must provide an action to perform!%PT%{Constants.NewLine}");
                return;
            }
            var targetActor = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(target, session.Player);
            if (targetActor == null)
            {
                var tSession = SessionManager.Instance.GetSession(target);
                if (tSession == null)
                {
                    session.Send($"%BRT%The target of your power cannot be found!%PT%{Constants.NewLine}");
                    return;
                }
                targetActor = tSession.Player;
            }
            if (targetActor.ActorType == ActorType.Player)
            {
                var player = (Player)targetActor;
                if (player.ID == session.Player.ID)
                {
                    session.Send($"%BRT%You cannot use that power on yourself!%PT%{Constants.NewLine}");
                    return;
                }
                var tSession = SessionManager.Instance.GetSession(player.ID);
                if (tSession == null)
                {
                    session.Send($"%BRT%The target of your power cannot be found!%PT%{Constants.NewLine}");
                    return;
                }
                if (tSession.Player.Level > session.Player.Level)
                {
                    session.Send($"%BRT%You cannot force someone more powerful than yourself!%PT%{Constants.NewLine}");
                    return;
                }
                var msg = session.Player.CanBeSeenBy(tSession.Player) ? $"%BYT%{session.Player.Name}'s power overcomes you and you act against your will!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic flood your mind, forcing you to act against your will!%PT%{Constants.NewLine}";
                tSession.Send(msg);
                CommandParser.Parse(tSession, ref actionString);
                Game.LogMessage($"GOD: Player {session.Player.Name} used FORCE on {tSession.Player.Name}: {actionString}", LogLevel.God);
                return;
            }
            if (targetActor.ActorType == ActorType.NonPlayer)
            {
                ActMob.ParseCommand((NPC)targetActor, actionString, session);
                Game.LogMessage($"GOD: Player {session.Player.Name} used FORCE on NPC {targetActor.Name}: {actionString}", LogLevel.God);
                return;
            }
            Game.LogMessage($"DEBUG: {session.Player.Name} called ForceActor() on an unsupported Actor Type: {targetActor.ActorType}", LogLevel.Debug);
            session.Send($"%BRT%You cannot use FORCE on {targetActor.ActorType}!%PT%{Constants.NewLine}");
        }

        public static void CreateAsset(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to spawn in-game items but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: <create | spawn> <item | npc> <name | id>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: create npc fred%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: create item 10%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            var assetType = args[0];
            var assetId = arg.Remove(0, assetType.Length).Trim();
            if (string.IsNullOrEmpty(assetType) || string.IsNullOrEmpty(assetId))
            {
                session.Send($"%BRT%Usage: <create | spawn> <item | npc> <name | id>%PT%{Constants.NewLine}");
                return;
            }
            switch(assetType.ToLower())
            {
                case "item":
                    CreateItem(session, assetId);
                    break;

                case "npc":
                    CreateNPC(session, assetId);
                    break;

                default:
                    session.Send($"%BRT%This power can only be used to create items or NPCs.%PT%{Constants.NewLine}");
                    return;
            }
        }

        public static void SummonTarget(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to summon a target to their location but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You must provide a target for this power!%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(arg);
            if (target == null)
            {
                session.Send($"%BRT%That person doesn't seem to be in the Realms right now!%PT%{Constants.NewLine}");
                return;
            }
            if (!target.Player.CanMove())
            {
                session.Send($"%BRT%{target.Player.Name} cannot be teleported right now!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot summon someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            var msg = session.Player.CanBeSeenBy(target.Player) ? $"%BYT%{session.Player.Name} reaches through the Universe, summoning you to them!%PT%{Constants.NewLine}" :
                $"%BYT%You feel a strange sensation as some power pulls you through the very fabric of reality!%PT%{Constants.NewLine}";
            Game.LogMessage($"GOD: Player {session.Player.Name} transported {target.Player.Name} from Room {target.Player.CurrentRoom} to Room {session.Player.CurrentRoom}", LogLevel.God);
            target.Player.Move(session.Player.CurrentRoom, true);
        }

        public static void Destroy(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to destroy items but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                RoomManager.Instance.GetRoom(session.Player.CurrentRoom).ItemsInRoom.Clear();
                session.Send($"%BYT%Bathing the area in holy fire, you burn any stray items from the world!%PT%{Constants.NewLine}");
                Game.LogMessage($"GOD: Player {session.Player.Name} destroyed all items in Room {session.Player.CurrentRoom}", LogLevel.God);
                var players = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
                if (players != null && players.Count > 1)
                {
                    foreach(var lp in players.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesture, purging stray items from the world!%PT%{Constants.NewLine}" :
                            $"%BYT%The air shifts suddenly, and stray items are burned from the world!%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetItem(arg);
            if (target == null)
            {
                session.Send($"%BRT%There is nothing like that here!%PT%{Constants.NewLine}");
                return;
            }
            RoomManager.Instance.GetRoom(session.Player.CurrentRoom).ItemsInRoom.TryRemove(target.ItemID, out _);
            session.Send($"%BYT%With an arcane gesure you burn {target.ShortDescription} from the world!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} destroyed Item {target.Name} ({target.ID}) in Room {session.Player.CurrentRoom}", LogLevel.God);
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            if (localPlayers != null || localPlayers.Count > 1)
            {
                foreach(var lp in localPlayers.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesure and burns {target.ShortDescription} from the world!%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts suddenly and {target.ShortDescription} is burned from the world!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
        }

        public static void TeleportToTarget(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: {session.Player.Name} attempted to teleport to a target but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You must provide a target for this power!%PT%{Constants.NewLine}");
                return;
            }
            if (!session.Player.CanMove())
            {
                session.Send($"%BRT%You aren't in a position to teleport anywhere right now!%PT%{Constants.NewLine}");
                return;
            }
            if (int.TryParse(arg, out int rid))
            {
                var room = RoomManager.Instance.GetRoom(rid);
                if (room == null)
                {
                    session.Send($"%BRT%No Room with the specified ID could be found in Room Manager.%PT%{Constants.NewLine}");
                    return;
                }
                Game.LogMessage($"GOD: Player {session.Player.Name} teleported from Room {session.Player.CurrentRoom} to Room {room.ID}", LogLevel.God);
                session.Player.Move(room.ID, true);
                return;
            }
            var target = SessionManager.Instance.GetSession(arg);
            if (target == null)
            {
                session.Send($"%BRT%You can't find anyone of that name in the Realms right now.%PT%{Constants.NewLine}");
                return;
            }
            Game.LogMessage($"GOD: Player {session.Player.Name} teleported to the same room as {target.Player.Name} (From: {session.Player.CurrentRoom} To: {target.Player.CurrentRoom})", LogLevel.God);
            session.Player.Move(target.Player.CurrentRoom, true);
        }

        public static void Slay(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to slay someone but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You must provide a target for this power!%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(arg, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%The target of your power cannot be found!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType != ActorType.Player)
            {
                session.Send($"%BRT%You cannot use this power on that target!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot smite someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            Game.LogMessage($"GOD: Player {session.Player.Name} used holy power to kill {target.Name} in Room {target.CurrentRoom}", LogLevel.God);
            ((Player)target).Kill(session.Player, false);
            session.Send($"%BYT%Calling on holy power, you smite {target.Name}, killing them instantly!%PT%{Constants.NewLine}");
        }

        public static void Purge(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to purge with holy fire but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                var npcs = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).NPCsInRoom;
                if (npcs == null || npcs.Count == 0)
                {
                    session.Send($"%BRT%There is nothing to Purge!%PT%{Constants.NewLine}");
                    return;
                }
                foreach(var npc in npcs)
                {
                    npc.Kill(session.Player, false);
                }
                Game.LogMessage($"GOD: Player {session.Player.Name} purged Room {session.Player.CurrentRoom} killing all NPCs", LogLevel.God);
                session.Send($"%BYT%You bathe the area in purifying holy fire, burning the area clean!%PT%{Constants.NewLine}");
                var roomPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
                if (roomPlayers != null && roomPlayers.Count > 0)
                {
                    foreach(var lp in roomPlayers.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} bathes the area in holy fire, purging the area clean!%PT%{Constants.NewLine}" :
                            $"%BYT%The air shifts and the area is purged in holy fire and burned clean!%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(arg, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%The target of your wrath cannot be found!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType == ActorType.Player)
            {
                session.Send($"%BRT%You cannot Purge Players, use SLAY or SMITE instead.%PT%{Constants.NewLine}");
                return;
            }
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            session.Send($"%BYT%You bathe {target.Name} in holy fire, burning them from the fabric of the world!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} purged NPC {target.Name} in Room {target.CurrentRoom}", LogLevel.God);
            if (localPlayers != null & localPlayers.Count > 1)
            {
                foreach(var lp in localPlayers.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesure and {target.Name} is consumed in holy fire!%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts and {target.Name} is consumed in holy fire!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            ((NPC)target).Kill(session.Player, false);
        }

        public static void ListAssets(Session session, string arg)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to list World assets but they are not Immortal", LogLevel.Warning);
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: list <asset type> <search criteria>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: list room 100%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: list npc etrea guard%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: list item 100-150%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 1)
            {
                session.Send($"%BRT%Incorrect number of arguments.%PT%{Constants.NewLine}");
                return;
            }
            var assetType = args[0];
            var criteria = arg.Remove(0, assetType.Length).Trim();
            switch(assetType.ToLower())
            {
                case "connection":
                case "connections":
                case "session":
                case "sessions":
                    ListConnections(session);
                    break;

                case "mobprog":
                    ListMobProgs(session, criteria);
                    break;

                case "room":
                    ListRooms(session, criteria);
                    break;

                case "npc":
                    ListNPCs(session, criteria);
                    break;

                case "npcinstance":
                    ListNPCInstances(session, criteria);
                    break;

                case "item":
                    ListItems(session, criteria);
                    break;

                case "emote":
                    ListEmotes(session, criteria);
                    break;

                case "zone":
                    ListZones(session, criteria);
                    break;

                case "quest":
                    ListQuests(session, criteria);
                    break;

                case "recipe":
                    ListRecipes(session, criteria);
                    break;

                case "buff":
                    ListBuffs(session, criteria);
                    break;

                case "spell":
                    ListSpells(session, criteria);
                    break;

                case "skill":
                    ListSkills(session, criteria);
                    break;

                case "node":
                case "resourcenode":
                    ListNodes(session, criteria);
                    break;

                case "shop":
                    ListShops(session, criteria);
                    break;

                default:
                    session.Send($"%BRT%Unsupported asset type: {assetType}%PT%{Constants.NewLine}");
                    Game.LogMessage($"WARN: LIST was called with an unsupported asset type: {assetType}", LogLevel.Warning);
                    break;
            }
        }

        #region Private Functions
        private static void ShowPlayerCharSheet(Session session, string targetName)
        {
            var targetPlayer = SessionManager.Instance.GetSession(targetName);
            if (targetPlayer == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (targetPlayer.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot use this on someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            sb.AppendLine($"%BYT%||%PT% Name: {targetPlayer.Player.Name}{Constants.TabStop}Level: {targetPlayer.Player.Level}{Constants.TabStop}Race: {targetPlayer.Player.Race}");
            sb.AppendLine($"%BYT%||%PT% Alignment: {targetPlayer.Player.Alignment}{Constants.TabStop}Class: {targetPlayer.Player.Class}");
            sb.AppendLine($"%BYT%||%PT% STR: {targetPlayer.Player.Strength} ({Helpers.CalculateAbilityModifier(targetPlayer.Player.Strength)}){Constants.TabStop}DEX: {targetPlayer.Player.Dexterity} ({Helpers.CalculateAbilityModifier(targetPlayer.Player.Dexterity)}){Constants.TabStop}CON: {targetPlayer.Player.Constitution} ({Helpers.CalculateAbilityModifier(targetPlayer.Player.Constitution)})");
            sb.AppendLine($"%BYT%||%PT% INT: {targetPlayer.Player.Intelligence} ({Helpers.CalculateAbilityModifier(targetPlayer.Player.Intelligence)}){Constants.TabStop}WIS: {targetPlayer.Player.Wisdom} ({Helpers.CalculateAbilityModifier(targetPlayer.Player.Wisdom)}){Constants.TabStop}CHA: {targetPlayer.Player.Charisma} ({Helpers.CalculateAbilityModifier(targetPlayer.Player.Charisma)})");
            sb.AppendLine($"%BYT%||%PT% HP: %BRT%{targetPlayer.Player.CurrentHP:N0}%PT% / %BRT%{targetPlayer.Player.MaxHP:N0}%PT%{Constants.TabStop}MP: %BGT%{targetPlayer.Player.CurrentMP:N0}%PT% / %BGT%{targetPlayer.Player.MaxMP:N0}%PT%{Constants.TabStop}SP: %BYT%{targetPlayer.Player.CurrentSP:N0}%PT% / %BYT%{targetPlayer.Player.MaxSP:N0}%PT%");
            sb.AppendLine($"%BYT%||%PT% Armour Class: {targetPlayer.Player.ArmourClass}{Constants.TabStop}Attacks: {targetPlayer.Player.NumberOfAttacks}");
            sb.AppendLine($"%BYT%||%PT% Languages: {string.Join(", ", targetPlayer.Player.KnownLanguages.Keys)}");
            sb.AppendLine($"%BYT%||%PT% Exp: {targetPlayer.Player.Exp:N0}{Constants.TabStop}Next: {LevelTable.ExpForNextLevel(targetPlayer.Player.Level, targetPlayer.Player.Exp)}");
            sb.AppendLine($"%BYT%||%PT% Gold: %YT%{targetPlayer.Player.Gold:N0}%PT%");
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            session.Send(sb.ToString());
            Game.LogMessage($"GOD: Player {session.Player.Name} viewed the character sheet of {targetPlayer.Player.Name}", LogLevel.God);
        }

        private static void ShowNPCCharSheet(Session session, string targetName)
        {
            var targetNPC = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(targetName, session.Player);
            if (targetNPC == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (targetNPC.ActorType != ActorType.NonPlayer)
            {
                session.Send($"%BRT%That is not a valid target for this power!%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            sb.AppendLine($"%BYT%||%PT% Name: {targetNPC.Name}{Constants.TabStop}Level: {targetNPC.Level}");
            sb.AppendLine($"%BYT%||%PT% Alignment: {targetNPC.Alignment}");
            sb.AppendLine($"%BYT%||%PT% STR: {targetNPC.Strength} ({Helpers.CalculateAbilityModifier(targetNPC.Strength)}){Constants.TabStop}DEX: {targetNPC.Dexterity} ({Helpers.CalculateAbilityModifier(targetNPC.Dexterity)}){Constants.TabStop}CON: {targetNPC.Constitution} ({Helpers.CalculateAbilityModifier(targetNPC.Constitution)})");
            sb.AppendLine($"%BYT%||%PT% INT: {targetNPC.Intelligence} ({Helpers.CalculateAbilityModifier(targetNPC.Intelligence)}){Constants.TabStop}WIS: {targetNPC.Wisdom} ({Helpers.CalculateAbilityModifier(targetNPC.Wisdom)}){Constants.TabStop}CHA: {targetNPC.Charisma} ({Helpers.CalculateAbilityModifier(targetNPC.Charisma)})");
            sb.AppendLine($"%BYT%||%PT% HP: %BRT%{targetNPC.CurrentHP:N0}%PT% / %BRT%{targetNPC.MaxHP:N0}%PT%{Constants.TabStop}MP: %BGT%{targetNPC.CurrentMP:N0}%PT% / %BGT%{targetNPC.MaxMP:N0}%PT%");
            sb.AppendLine($"%BYT%||%PT% Armour Class: {targetNPC.ArmourClass}{Constants.TabStop}Attacks: {targetNPC.NumberOfAttacks}");
            sb.AppendLine($"%BYT%||%PT% Gold: %YT%{targetNPC.Gold:N0}%PT%");
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            session.Send(sb.ToString());
            Game.LogMessage($"GOD: Player {session.Player.Name} viewed the character sheet of NPC {targetNPC.Name}", LogLevel.God);
        }

        private static void ShowPlayerInventory(Session session, string targetName)
        {
            var targetPlayer = SessionManager.Instance.GetSession(targetName);
            if (targetPlayer == null)
            {
                session.Send($"%BRT%The target of your power cannot be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (targetPlayer.Player.Level > session.Player.Level)
            {
                session.Send($"%BRT%You cannot use this on someone more powerful than yourself!%PT%{Constants.NewLine}");
                return;
            }
            if (targetPlayer.Player.Inventory == null || targetPlayer.Player.Inventory.Count == 0)
            {
                session.Send($"%BYT%{targetPlayer.Player.Name} is not carrying anything!%PT%{Constants.NewLine}");
                if (targetPlayer.Player.Gold > 0)
                {
                    session.Send($"%BYT%{targetPlayer.Player.Name} has {targetPlayer.Player.Gold:N0} Gold%PT%");
                }
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%{targetPlayer.Player.Name} is carrying:%PT%");
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            foreach (var i in targetPlayer.Player.Inventory.Values.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.ID))
            {
                var cnt = targetPlayer.Player.Inventory.Values.Where(x => x.ID == i.ID).Count();
                sb.AppendLine($"%BYT%||%PT% {cnt} x {i.Name}, {i.ShortDescription}");
            }
            if (targetPlayer.Player.Gold > 0)
            {
                sb.AppendLine($"%BYT%|| Gold: {targetPlayer.Player.Gold:N0}%PT%");
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
            Game.LogMessage($"GOD: Player {session.Player.Name} viewed the inventory of player {targetPlayer.Player.Name}", LogLevel.God);
        }

        private static void ShowNPCInventory(Session session, string targetName)
        {
            var targetNPC = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(targetName, session.Player);
            if (targetNPC == null)
            {
                session.Send($"%BRT%The target of your power could not be found in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (targetNPC.ActorType != ActorType.NonPlayer)
            {
                session.Send($"%BRT%That is not a valid target for this power!%PT%{Constants.NewLine}");
                return;
            }
            if (targetNPC.Inventory == null || targetNPC.Inventory.Count == 0)
            {
                session.Send($"%BYT%{targetNPC.Name} is not carrying anything!%PT%{Constants.NewLine}");
                if (targetNPC.Gold > 0)
                {
                    session.Send($"%BYT%{targetNPC.Name} has {targetNPC.Gold:N0} Gold%PT%");
                }
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%{targetNPC.Name} is carrying:%PT%");
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            foreach (var i in targetNPC.Inventory.Values.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.ID))
            {
                var cnt = targetNPC.Inventory.Values.Where(x => x.ID == i.ID).Count();
                sb.AppendLine($"%BYT%||%PT% {cnt} x {i.Name}, {i.ShortDescription}");
            }
            if (targetNPC.Gold > 0)
            {
                sb.AppendLine($"%BYT%|| Gold: {targetNPC.Gold:N0}%PT%");
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
            Game.LogMessage($"GOD: Player {session.Player.Name} viewed the inventory of NPC {targetNPC.Name}", LogLevel.God);
        }

        private static void FindItemInWorld(Session session, string targetString)
        {
            if (int.TryParse(targetString, out int itemID))
            {
                var roomsWithItems = RoomManager.Instance.GetRoomsWithItems();
                if (roomsWithItems == null || roomsWithItems.Count == 0)
                {
                    session.Send($"%BRT%No Item with that ID could be found in the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                bool matchFound = false;
                foreach(var room in roomsWithItems)
                {
                    var matchingItems = room.ItemsInRoom.Values.Where(x => x.ID == itemID).ToList();
                    if (matchingItems != null && matchingItems.Count > 0)
                    {
                        matchFound = true;
                        foreach(var item in matchingItems)
                        {
                            sb.AppendLine($"|| {item.ID} - {item.Name}: Room: {room.ID}");
                        }
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                if (matchFound)
                {
                    session.Send(sb.ToString());
                }
                else
                {
                    session.Send($"%BRT%No Item with that ID could be found in the Realms!%PT%{Constants.NewLine}");
                }
            }
            else
            {
                var roomsWithItems = RoomManager.Instance.GetRoomsWithItems();
                if (roomsWithItems == null || roomsWithItems.Count == 0)
                {
                    session.Send($"%BRT%No Item matching that criteria could be found in the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                bool matchFound = false;
                foreach (var room in roomsWithItems)
                {
                    var matchingItems = room.ItemsInRoom.Values.Where(x => x.Name.IndexOf(targetString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    if (matchingItems != null && matchingItems.Count > 0)
                    {
                        matchFound = true;
                        foreach (var item in matchingItems)
                        {
                            sb.AppendLine($"|| {item.ID} - {item.Name}: Room: {room.ID}");
                        }
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                if (matchFound)
                {
                    session.Send(sb.ToString());
                }
                else
                {
                    session.Send($"%BRT%No Item with that ID could be found in the Realms!%PT%{Constants.NewLine}");
                }
            }
        }

        private static void FindNPCInWorld(Session session, string targetString)
        {
            if (int.TryParse(targetString, out int npcID))
            {
                var npcs = NPCManager.Instance.AllNPCInstances.Where(x => x.TemplateID == npcID).ToList();
                if (npcs == null || npcs.Count == 0)
                {
                    session.Send($"%BRT%No NPCs with that ID could be found in the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var npc in npcs)
                {
                    sb.AppendLine($"|| {npc.ID}: {npc.TemplateID} - {npc.Name} in Room {npc.CurrentRoom}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
            }
            else
            {
                var npcs = NPCManager.Instance.AllNPCInstances.Where(x => x.Name.IndexOf(targetString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (npcs == null || npcs.Count == 0)
                {
                    session.Send($"%BRT%No NPCs matching that criteria could be found in the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var npc in npcs)
                {
                    sb.AppendLine($"|| {npc.ID}: {npc.TemplateID} - {npc.Name} in Room {npc.CurrentRoom}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
            }
        }

        private static void CreateNPC(Session session, string id)
        {
            if (int.TryParse(id, out int npcID))
            {
                var npc = NPCManager.Instance.GetNPC(npcID);
                if (npc == null)
                {
                    session.Send($"%BRT%No NPC with that ID could be found in NPC Manager.%PT%{Constants.NewLine}");
                    return;
                }
                NPCManager.Instance.AddNewNPCInstance(npc.TemplateID, session.Player.CurrentRoom);
                session.Send($"%BYT%Calling on the Winds of Magic, you bring life to {npc.ShortDescription}!%PT%{Constants.NewLine}");
                Game.LogMessage($"GOD: Player {session.Player.Name} created NPC {npc.TemplateID} in Room {session.Player.CurrentRoom}", LogLevel.God);
                var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
                if (localPlayers != null && localPlayers.Count > 1)
                {
                    foreach(var lp in localPlayers.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesture and summons {npc.ShortDescription}!%PT%{Constants.NewLine}" :
                            $"%BYT%The Winds of Magic shift, bringing life to {npc.ShortDescription}!%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            var tNPC = NPCManager.Instance.GetNPC(id).FirstOrDefault();
            if (tNPC == null)
            {
                session.Send($"%BRT%No NPC matching that name could be found in NPC Manager.%PT%{Constants.NewLine}");
                return;
            }
            NPCManager.Instance.AddNewNPCInstance(tNPC.TemplateID, session.Player.CurrentRoom);
            session.Send($"%BYT%Calling on the Winds of Magic, you bring life to {tNPC.ShortDescription}!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} created NPC {tNPC.TemplateID} in Room {session.Player.CurrentRoom}", LogLevel.God);
            var players = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            if (players != null && players.Count > 1)
            {
                foreach (var lp in players.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesture and summons {tNPC.ShortDescription}!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift, bringing life to {tNPC.ShortDescription}!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
        }

        private static void CreateItem(Session session, string id)
        {
            if (int.TryParse(id, out int itemID))
            {
                var item = ItemManager.Instance.GetItem(itemID);
                if (item == null)
                {
                    session.Send($"%BRT%No Item with that ID was found in Item Manager.%PT%{Constants.NewLine}");
                    return;
                }
                dynamic newItem = null;
                switch(item.ItemType)
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
                newItem.ItemID = Guid.NewGuid();
                RoomManager.Instance.AddItemToRoomInventory(session.Player.CurrentRoom, newItem);
                session.Send($"%BYT%Calling on the Winds of Magic, you summon {newItem.ShortDescription} into existence!%PT%{Constants.NewLine}");
                Game.LogMessage($"GOD: Player {session.Player.Name} created item {newItem.Name} ({newItem.ID}) in Room {session.Player.CurrentRoom}", LogLevel.God);
                var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
                if (localPlayers != null && localPlayers.Count > 1)
                {
                    foreach (var lp in localPlayers.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesture and summons {item.ShortDescription}!%PT%{Constants.NewLine}" :
                            $"%BYT%The Winds of Magic shift, creating {item.ShortDescription}!%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            var mItem = ItemManager.Instance.GetItem(id);
            if (mItem == null)
            {
                session.Send($"%BRT%No Item matching that criteria was found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            dynamic mItemNew = null;
            switch (mItem.ItemType)
            {
                case ItemType.Misc:
                    mItemNew = Helpers.Clone<InventoryItem>(mItem);
                    break;

                case ItemType.Weapon:
                    mItemNew = Helpers.Clone<Weapon>(mItem);
                    break;

                case ItemType.Consumable:
                    mItemNew = Helpers.Clone<Consumable>(mItem);
                    break;

                case ItemType.Armour:
                    mItemNew = Helpers.Clone<Armour>(mItem);
                    break;

                case ItemType.Ring:
                    mItemNew = Helpers.Clone<Ring>(mItem);
                    break;

                case ItemType.Scroll:
                    mItemNew = Helpers.Clone<Scroll>(mItem);
                    break;
            }
            mItemNew.ItemID = Guid.NewGuid();
            RoomManager.Instance.AddItemToRoomInventory(session.Player.CurrentRoom, mItemNew);
            session.Send($"%BYT%Calling on the Winds of Magic, you summon {mItemNew.ShortDescription} into existence!%PT%{Constants.NewLine}");
            Game.LogMessage($"GOD: Player {session.Player.Name} created item {mItemNew.Name} ({mItemNew.ID}) in Room {session.Player.CurrentRoom}", LogLevel.God);
            var players = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            if (players != null && players.Count > 1)
            {
                foreach (var lp in players.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} makes an arcane gesture and summons {mItemNew.ShortDescription}!%PT%{Constants.NewLine}" :
                        $"%BYT%The Winds of Magic shift, creating {mItemNew.ShortDescription}!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
        }

        private static void ListShops(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var shops = ShopManager.Instance.GetShop();
                if (shops == null || shops.Count == 0)
                {
                    session.Send($"%BRT%No Shops were found in Shop Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var shop in shops)
                {
                    sb.AppendLine($"|| {shop.ID} - {shop.ShopName}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int shopID))
            {
                var shop = ShopManager.Instance.GetShop(shopID);
                if (shop == null)
                {
                    session.Send($"%BRT%No Shop with that ID was found in Shop Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {shop.ID}");
                sb.AppendLine($"|| Name: {shop.ShopName}");
                sb.AppendLine($"|| Base Gold: {shop.BaseGold}");
                if (shop.BaseInventory.Count == 0)
                {
                    sb.AppendLine($"|| Base Inventory: Nothing");
                }
                else
                {
                    sb.AppendLine($"|| Base Inventory:");
                    foreach (var i in shop.BaseInventory)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item == null)
                        {
                            sb.AppendLine($"|| {i.Value} x Unknown Item ({i.Key})");
                        }
                        else
                        {
                            sb.AppendLine($"|| {i.Value} x {item.Name} ({item.ID})");
                        }
                    }
                }
                if (shop.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(shop.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {shop.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {shop.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {shop.OLCLocked}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var shops = ShopManager.Instance.GetShop(start, end);
                if (shops == null || shops.Count == 0)
                {
                    session.Send($"%BRT%No Shops in the ID range could be found in Shop Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var shop in shops)
                {
                    sb.AppendLine($"|| {shop.ID} - {shop.ShopName}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingShops = ShopManager.Instance.GetShop(criteria);
            if (matchingShops == null || matchingShops.Count == 0)
            {
                session.Send($"%BRT%No Shops matching the specified criteria were found in Shop Manager.%PT%{Constants.NewLine}" );
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var shop in matchingShops)
            {
                sb.AppendLine($"|| {shop.ID} - {shop.ShopName}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListNodes(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var nodes = NodeManager.Instance.GetNode();
                if (nodes == null || nodes.Count == 0)
                {
                    session.Send($"%BRT%No Resource Nodes were found in Node Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var node in nodes)
                {
                    sb.AppendLine($"|| {node.ID} - {node.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int nodeID))
            {
                var node = NodeManager.Instance.GetNode(nodeID);
                if (node == null)
                {
                    session.Send($"%BRT%No Resource Node with that ID could be found in Node Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {node.ID}");
                sb.AppendLine($"|| Name: {node.Name}");
                sb.AppendLine($"|| Appearance Chance: {node.ApperanceChance}");
                if (node.CanFind.Count == 0)
                {
                    sb.AppendLine($"|| Findable Items: None");
                }
                else
                {
                    sb.AppendLine($"|| Findable Items:");
                    foreach(var i in node.CanFind)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item == null)
                        {
                            sb.AppendLine($"||   Unknown Item ({i.Key})");
                        }
                        else
                        {
                            sb.AppendLine($"||   {item.Name} ({item.ID})");
                        }
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingNodes = NodeManager.Instance.GetNode(criteria);
            if (matchingNodes == null || matchingNodes.Count == 0)
            {
                session.Send($"%BRT%No Resource Nodes matching the given criteria were found in Node Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var node in matchingNodes)
            {
                sb.AppendLine($"|| {node.ID} - {node.Name}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListSkills(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var skills = SkillManager.Instance.GetSkill();
                if (skills == null || skills.Count == 0)
                {
                    session.Send($"%BRT%No Skills were found in Skill Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var skill in skills)
                {
                    sb.AppendLine($"|| {skill.Name} ({skill.LearnCost} gold)");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (Enum.TryParse<ActorClass>(criteria, true, out var actorClass))
            {
                var skills = SkillManager.Instance.GetSkill(actorClass);
                if (skills == null || skills.Count == 0)
                {
                    session.Send($"%BRT%No Skills were found in Skill Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| Skills available to the {actorClass} Class:");
                foreach(var skill in skills)
                {
                    sb.AppendLine($"|| {skill.Name} ({skill.LearnCost} gold)");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingSkill = SkillManager.Instance.GetSkill(criteria);
            if (matchingSkill == null)
            {
                session.Send($"%BRT%No Skill with that name in Skill Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| Name: {matchingSkill.Name}{Constants.TabStop}Cost: {matchingSkill.LearnCost:N0} gold");
            sb.AppendLine($"|| Description: {matchingSkill.Description}");
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListSpells(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var spells = SpellManager.Instance.GetSpell();
                if (spells == null || spells.Count == 0)
                {
                    session.Send($"%BRT%No Spells were found in Spell Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var spell in spells)
                {
                    sb.AppendLine($"|| {spell.ID} - {spell.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int spellID))
            {
                var spell = SpellManager.Instance.GetSpell(spellID);
                if (spell == null)
                {
                    session.Send($"%BRT%No Spell with that ID could be found in Spell Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {spell.ID}{Constants.TabStop}Type: {spell.SpellType}{Constants.TabStop}Auto Hit: {spell.AutoHitTarget}");
                sb.AppendLine($"|| Name: {spell.Name}");
                sb.AppendLine($"|| Available To: {spell.AvailableToClass}");
                sb.AppendLine($"|| MP Cost: {spell.MPCostExpression}");
                sb.AppendLine($"|| Learn Cost: {spell.LearnCost}{Constants.TabStop}AOE: {spell.IsAOE}{Constants.TabStop}Ability Modifier: {spell.ApplyAbilityModifier}");
                if (!string.IsNullOrEmpty(spell.DamageExpression))
                {
                    sb.AppendLine($"|| HP Effect: {spell.DamageExpression}");
                }
                if (spell.AppliedBuffs.Count > 0)
                {
                    sb.AppendLine($"|| Applied Buffs:");
                    foreach(var b in spell.AppliedBuffs)
                    {
                        var buff = BuffManager.Instance.GetBuff(b.Key);
                        if (buff != null)
                        {
                            sb.AppendLine($"||    {buff.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"||    Unknown Buff ({b.Key})");
                        }
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var spells = SpellManager.Instance.GetSpell(start, end);
                if (spells == null || spells.Count == 0)
                {
                    session.Send($"%BRT%No Spells in that ID range were found in Spell Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var spell in spells)
                {
                    sb.AppendLine($"|| {spell.ID} - {spell.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingSpells = SpellManager.Instance.GetSpell(criteria, true);
            if (matchingSpells == null || matchingSpells.Count == 0)
            {
                session.Send($"%BRT%No Spells matching the criteria were found in Spell Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var spell in matchingSpells)
            {
                sb.AppendLine($"|| {spell.ID} - {spell.Name}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListBuffs(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var buffs = BuffManager.Instance.GetBuff();
                if (buffs == null || buffs.Count == 0)
                {
                    session.Send($"%BRT%No Buffs were found in Buff Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var buff in buffs)
                {
                    sb.AppendLine($"|| Name: {buff.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingBuff = BuffManager.Instance.GetBuff(criteria);
            if (matchingBuff == null)
            {
                session.Send($"%BRT%No Buff with that name could be found in Buff Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| Name: {matchingBuff.Name}");
            sb.AppendLine($"|| Description: {matchingBuff.Description}");
            sb.AppendLine($"|| Duration: {matchingBuff.Duration}");
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListRecipes(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var recipes = RecipeManager.Instance.GetRecipe();
                if (recipes == null || recipes.Count == 0)
                {
                    session.Send($"%BRT%No Recipes were found in Recipe Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var recipe in recipes)
                {
                    sb.AppendLine($"|| {recipe.ID} - {recipe.Name} ({recipe.RecipeType})");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int recipieID))
            {
                var recipe = RecipeManager.Instance.GetRecipe(recipieID);
                if (recipe == null)
                {
                    session.Send($"%BRT%No Recipe with that ID was found in Recipe Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {recipe.ID}{Constants.TabStop}Type: {recipe.RecipeType}");
                sb.AppendLine($"|| Description: {recipe.Description}");
                if (recipe.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(recipe.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {recipe.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {recipe.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {recipe.OLCLocked}");
                }
                var recipeResult = ItemManager.Instance.GetItem(recipe.RecipeResult);
                if (recipeResult == null)
                {
                    sb.AppendLine($"|| Result: Unknown Item ({recipe.RecipeResult})");
                }
                else
                {
                    sb.AppendLine($"|| Result: {recipeResult.Name} ({recipeResult.ID})");
                }
                if (recipe.RequiredItems.Count > 0)
                {
                    sb.AppendLine("|| Required Items:");
                    foreach(var i in recipe.RequiredItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"||    {i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"||    {i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Required Items: None");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var recipes = RecipeManager.Instance.GetRecipe(start, end);
                if (recipes == null || recipes.Count == 0)
                {
                    session.Send($"%BRT%No Recipes with IDs in the specified range were found in Recipe Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var recipe in recipes)
                {
                    sb.AppendLine($"|| {recipe.ID} - {recipe.Name} ({recipe.RecipeType})");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingRecipes = RecipeManager.Instance.GetRecipe(criteria);
            if (matchingRecipes == null || matchingRecipes.Count == 0)
            {
                session.Send($"%BRT%No Recipes matching the specified criteria were found in Recipe Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var recipe in matchingRecipes)
            {
                sb.AppendLine($"|| {recipe.ID} - {recipe.Name} ({recipe.RecipeType})");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListQuests(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var quests = QuestManager.Instance.GetQuest();
                if (quests == null || quests.Count == 0)
                {
                    session.Send($"%BRT%No Quests were found in Quest Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var quest in quests)
                {
                    sb.AppendLine($"|| {quest.ID} - {quest.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int questID))
            {
                var quest = QuestManager.Instance.GetQuest(questID);
                if (quest == null)
                {
                    session.Send($"%BRT%No Quest with that ID could be found in Quest Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {quest.ID}{Constants.TabStop}Zone: {quest.Zone}{Constants.TabStop}Type: {quest.QuestType}");
                sb.AppendLine($"|| Name: {quest.Name}");
                sb.AppendLine($"|| Flavour Text: {quest.FlavourText}");
                sb.AppendLine($"|| Exp: {quest.RewardExp}{Constants.TabStop}Gold: {quest.RewardGold}");
                if (quest.RequiredMonsters.Count > 0)
                {
                    sb.AppendLine($"|| Required Monsters:");
                    foreach(var m in quest.RequiredMonsters)
                    {
                        var monster = NPCManager.Instance.GetNPC(m.Key);
                        if (monster != null)
                        {
                            sb.AppendLine($"||   {m.Value} x {monster.Name} ({monster.TemplateID})");
                        }
                        else
                        {
                            sb.AppendLine($"||   {m.Value} x Unknown Monster ({m.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Required Monsters: None");
                }
                if (quest.RequiredItems.Count > 0)
                {
                    sb.AppendLine($"|| Required Items:");
                    foreach(var i in quest.RequiredItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"||    {i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"||    {i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Required Items: None");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') >= -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var quests = QuestManager.Instance.GetQuest(start, end);
                if (quests == null || quests.Count == 0)
                {
                    session.Send($"%BRT%No Quests in the specified ID range were found in Quest Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var quest in quests)
                {
                    sb.AppendLine($"|| {quest.ID} - {quest.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingQuests = QuestManager.Instance.GetQuest(criteria);
            if (matchingQuests == null || matchingQuests.Count == 0)
            {
                session.Send($"%BRT%No Quests matching the specified criteria were found in Quest Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var quest in matchingQuests)
            {
                sb.AppendLine($"|| {quest.ID} - {quest.Name}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListZones(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var zones = ZoneManager.Instance.GetZone();
                if (zones == null || zones.Count == 0)
                {
                    session.Send($"%BRT%No Zones were found in Zone Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var zone in zones)
                {
                    sb.AppendLine($"|| {zone.ZoneID} - {zone.ZoneName}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int zoneId))
            {
                var zone = ZoneManager.Instance.GetZone(zoneId);
                if (zone == null)
                {
                    session.Send($"%BRT%No Zone with that ID was found in Zone Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {zone.ZoneID}{Constants.TabStop}Name: {zone.ZoneName}");
                sb.AppendLine($"|| Start RID: {zone.MinRoom}{Constants.TabStop}End RID: {zone.MaxRoom}");
                if (zone.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(zone.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {zone.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {zone.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {zone.OLCLocked}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var zones = ZoneManager.Instance.GetZone(start, end);
                if (zones == null || zones.Count == 0)
                {
                    session.Send($"%BRT%No Zones in the specified ID range could be found in Zone Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var zone in zones)
                {
                    sb.AppendLine($"|| {zone.ZoneID} - {zone.ZoneName}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingZone = ZoneManager.Instance.GetZone(criteria);
            if (matchingZone == null)
            {
                session.Send($"%BRT%No Zone matching the specified criteria was found in Zone Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| ID: {matchingZone.ZoneID}{Constants.TabStop}Name: {matchingZone.ZoneName}");
            sb.AppendLine($"|| Start RID: {matchingZone.MinRoom}{Constants.TabStop}End RID: {matchingZone.MaxRoom}");
            if (matchingZone.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(matchingZone.LockHolder);
                if (lockingSession != null)
                {
                    sb.AppendLine($"|| OLC Locked: {matchingZone.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {matchingZone.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                }
            }
            else
            {
                sb.AppendLine($"|| OLC Locked: {matchingZone.OLCLocked}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListEmotes(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var emotes = EmoteManager.Instance.GetEmote();
                if (emotes == null || emotes.Count == 0)
                {
                    session.Send($"%BRT%No Emotes were found in Emote Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var emote in emotes)
                {
                    sb.AppendLine($"|| {emote.ID} - {emote.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int emoteID))
            {
                var emote = EmoteManager.Instance.GetEmote(emoteID);
                if (emote == null)
                {
                    session.Send($"%BRT%No Emote with that ID could be found in Emote Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {emote.ID}{Constants.TabStop}Name: {emote.Name}");
                sb.AppendLine($"|| Messages to Performer:");
                foreach(var m in emote.MessageToPerformer)
                {
                    sb.AppendLine($"|| {m}");
                }
                sb.AppendLine($"|| Messages to Others:");
                foreach(var m in emote.MessageToOthers)
                {
                    sb.AppendLine($"|| {m}");
                }
                sb.AppendLine($"|| Message to Target:");
                sb.AppendLine($"||  {emote.MessageToTarget}");
                if (emote.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(emote.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"OLC Locked: {emote.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {emote.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {emote.OLCLocked}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var emotes = EmoteManager.Instance.GetEmote(start, end);
                if (emotes == null || emotes.Count == 0)
                {
                    session.Send($"%BRT%No Emotes in the specified range could be found in Emote Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var emote in emotes)
                {
                    sb.AppendLine($"|| {emote.ID} - {emote.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingEmote = EmoteManager.Instance.GetEmote(criteria);
            if (matchingEmote == null)
            {
                session.Send($"%BRT%No Emote matching that criteria could be found in Emote Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| ID: {matchingEmote.ID}{Constants.TabStop}Name: {matchingEmote.Name}");
            sb.AppendLine($"|| Messages to Performer:");
            foreach (var m in matchingEmote.MessageToPerformer)
            {
                sb.AppendLine($"|| {m}");
            }
            sb.AppendLine($"|| Messages to Others:");
            foreach (var m in matchingEmote.MessageToOthers)
            {
                sb.AppendLine($"|| {m}");
            }
            sb.AppendLine($"|| Message to Target:");
            sb.AppendLine($"||  {matchingEmote.MessageToTarget}");
            if (matchingEmote.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(matchingEmote.LockHolder);
                if (lockingSession != null)
                {
                    sb.AppendLine($"OLC Locked: {matchingEmote.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {matchingEmote.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                }
            }
            else
            {
                sb.AppendLine($"|| OLC Locked: {matchingEmote.OLCLocked}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListItems(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var items = ItemManager.Instance.GetItem();
                if (items == null || items.Count == 0)
                {
                    session.Send($"%BRT%No Items found in Item Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var item in items)
                {
                    sb.AppendLine($"|| {item.ID} - {item.Name} ({item.ItemType})");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int itemID))
            {
                var item = ItemManager.Instance.GetItem(itemID);
                if (item == null)
                {
                    session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {item.ID}{Constants.TabStop}Type: {item.ItemType}{Constants.TabStop}Value: {item.BaseValue}");
                sb.AppendLine($"|| Name: {item.Name}");
                sb.AppendLine($"|| Short Desc: {item.ShortDescription}");
                foreach(var ln in item.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"|| {ln}");
                }
                if (item.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {item.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {item.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {item.OLCLocked}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var items = ItemManager.Instance.GetItem(start, end);
                if (items == null || items.Count == 0)
                {
                    session.Send($"%BRT%No Items in the specified range could be found in Item Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var item in items)
                {
                    sb.AppendLine($"|| {item.ID} - {item.Name} ({item.ItemType})");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingItems = ItemManager.Instance.GetItems(criteria);
            if (matchingItems == null || matchingItems.Count == 0)
            {
                session.Send($"%BRT%No Items matching the specified criteria could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var item in matchingItems)
            {
                sb.AppendLine($"|| {item.ID} - {item.Name} ({item.ItemType})");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
            return;
        }

        private static void ListNPCInstances(Session session, string criteria)
        {
            List<NPC> npcInstances = new List<NPC>();
            if (!int.TryParse(criteria, out var templateID))
            {
                npcInstances = string.IsNullOrEmpty(criteria) ? NPCManager.Instance.AllNPCInstances.OrderBy(x => x.Name).OrderBy(x => x.CurrentRoom).ToList() :
                        NPCManager.Instance.AllNPCInstances.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.Name).OrderBy(x => x.CurrentRoom).ToList();
            }
            else
            {
                npcInstances = NPCManager.Instance.AllNPCInstances.Where(x => x.TemplateID == templateID).OrderBy(x => x.Name).OrderBy(x => x.CurrentRoom).ToList();
            }
            if (npcInstances == null || npcInstances.Count == 0)
            {
                session.Send($"%BRT%No NPC Instances could be found.%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            foreach (var npc in npcInstances)
            {
                sb.AppendLine($"%BYT%||%PT%Name: {npc.Name} (ID: {npc.ID}) in Room {npc.CurrentRoom}");
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        private static void ListNPCs(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var npcs = NPCManager.Instance.GetNPC();
                if (npcs == null || npcs.Count == 0)
                {
                    session.Send($"%BRT%No NPC Templates found in NPC Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var npc in npcs)
                {
                    sb.AppendLine($"|| {npc.TemplateID} - {npc.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int id))
            {
                var npc = NPCManager.Instance.GetNPC(id);
                if (npc == null)
                {
                    session.Send($"%BRT%No NPC with that Template ID could be found in NPC Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| Template ID: {npc.TemplateID}{Constants.TabStop}Zone: {npc.ZoneID}");
                sb.AppendLine($"|| Name: {npc.Name}{Constants.TabStop}Flags: {npc.Flags}");
                sb.AppendLine($"|| Short Desc: {npc.ShortDescription}");
                sb.AppendLine($"|| STR: {npc.Strength}{Constants.TabStop}DEX: {npc.Dexterity}{Constants.TabStop}CON: {npc.Constitution}");
                sb.AppendLine($"|| INT: {npc.Intelligence}{Constants.TabStop}WIS: {npc.Wisdom}{Constants.TabStop}CHA: {npc.Charisma}");
                sb.AppendLine($"|| Hit Die: {npc.NumberOfHitDice}D{npc.HitDieSize}{Constants.TabStop}Base AC: {npc.BaseArmourClass}");
                sb.AppendLine($"|| Exp: {npc.ExpAward:N0}{Constants.TabStop}Gold: {npc.Gold:N0}");
                sb.AppendLine($"|| Appear Chance: {npc.AppearanceChance}{Constants.TabStop}Max Number: {npc.MaxNumberInWorld}");
                sb.AppendLine($"|| Arrival Msg: {npc.ArrivalMessage}");
                sb.AppendLine($"|| Departure Msg: {npc.DepatureMessage}");
                if (npc.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(npc.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {npc.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {npc.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {npc.OLCLocked}");
                }
                sb.AppendLine($"|| Long Description:");
                foreach (var ln in npc.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"||  {ln}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var npcs = NPCManager.Instance.GetNPC(start, end);
                if (npcs == null || npcs.Count == 0)
                {
                    session.Send($"%BRT%No NPC Templates in the specified range were found in NPC Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var npc in npcs)
                {
                    sb.AppendLine($"|| {npc.TemplateID} - {npc.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingNPCs = NPCManager.Instance.GetNPC(criteria);
            if (matchingNPCs == null || matchingNPCs.Count == 0)
            {
                session.Send($"%BRT%No NPC Templates matching the criteria could be found in NPC Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var npc in matchingNPCs)
            {
                sb.AppendLine($"|| {npc.TemplateID} - {npc.Name}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }

        private static void ListMobProgs(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var mobprogs = MobProgManager.Instance.GetMobProg();
                if (mobprogs == null || mobprogs.Count == 0)
                {
                    session.Send($"%BRT%No MobProgs found in MobProg Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var mp in mobprogs)
                {
                    sb.AppendLine($"|| {mp.ID} - {mp.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int mpID))
            {
                var mp = MobProgManager.Instance.GetMobProg(mpID);
                if (mp == null)
                {
                    session.Send($"%BRT%No MobProg with that ID was found in MobProg Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| ID: {mp.ID}{Constants.TabStop}Name: {mp.Name}");
                sb.AppendLine($"|| Description: {mp.Description}");
                sb.AppendLine($"|| Triggers: {mp.Triggers}");
                sb.AppendLine($"|| Script:");
                foreach(var ln in mp.Script.Split(new[] {Constants.NewLine}, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"|| {ln}");
                }
                if (mp.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(mp.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {mp.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {mp.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > 1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var mobProgs = MobProgManager.Instance.GetMobProg(start, end);
                if (mobProgs == null || mobProgs.Count == 0)
                {
                    session.Send($"%BRT%No MobProgs in the specified range were found in MobProg Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var mp in mobProgs)
                {
                    sb.AppendLine($"|| {mp.ID} - {mp.Name}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
        }

        private static void ListRooms(Session session, string criteria)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                var rooms = RoomManager.Instance.GetRoom();
                if (rooms == null || rooms.Count == 0)
                {
                    session.Send($"%BRT%No Rooms found in Room Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach(var room in rooms)
                {
                    sb.AppendLine($"|| {room.ID} - {room.RoomName}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (int.TryParse(criteria, out int rid))
            {
                var room = RoomManager.Instance.GetRoom(rid);
                if (room == null)
                {
                    session.Send($"%BRT%No Room with that ID could be found in Room Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| Room ID: {room.ID}{Constants.TabStop}Flags: {room.Flags}");
                if (room.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(room.LockHolder);
                    if (lockingSession != null)
                    {
                        sb.AppendLine($"|| OLC Locked: {room.OLCLocked}{Constants.TabStop}Lock Holder: {lockingSession.Player.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"|| OLC Locked: {room.OLCLocked}{Constants.TabStop}Lock Holder: Not Found");
                    }
                }
                else
                {
                    sb.AppendLine($"|| OLC Locked: {room.OLCLocked}");
                }
                sb.AppendLine($"|| Room Name: {room.RoomName}");
                sb.AppendLine($"|| Short Desc: {room.ShortDescription}");
                sb.AppendLine($"|| Long Desc:");
                foreach(var ln in room.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"|| {ln}");
                }
                sb.AppendLine($"|| Exits: {string.Join(", ", room.RoomExits.OrderBy(x => x.Value.ExitDirection).Select(x => x.Value.ExitDirection).ToArray())}");
                if (room.StartingNPCs.Count > 0)
                {
                    sb.AppendLine($"|| Starting NPCs:");
                    foreach(var n in room.StartingNPCs)
                    {
                        var npc = NPCManager.Instance.GetNPC(n.Key);
                        if (npc != null)
                        {
                            sb.AppendLine($"||{Constants.TabStop}{n.Value} x {npc.Name} ({npc.TemplateID})");
                        }
                        else
                        {
                            sb.AppendLine($"||{Constants.TabStop}{n.Value} x Unknown NPC ({n.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Starting NPCs: None");
                }
                if (room.StartingItems.Count > 0)
                {
                    sb.AppendLine($"|| Starting Items:");
                    foreach(var i in room.StartingItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"||{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"||{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Starting Items: None");
                }
                if (room.SpawnNPCsOnTick.Count > 0)
                {
                    sb.AppendLine($"|| Tick NPCs:");
                    foreach (var n in room.SpawnNPCsOnTick)
                    {
                        var npc = NPCManager.Instance.GetNPC(n.Key);
                        if (npc != null)
                        {
                            sb.AppendLine($"||{Constants.TabStop}{n.Value} x {npc.Name} ({npc.TemplateID})");
                        }
                        else
                        {
                            sb.AppendLine($"||{Constants.TabStop}{n.Value} x Unknown NPC ({n.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Tick NPCs: None");
                }
                if (room.SpawnItemsOnTick.Count > 0)
                {
                    sb.AppendLine($"|| Tick Items:");
                    foreach (var i in room.SpawnItemsOnTick)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"||{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"||{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"|| Tick Items: None");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            if (criteria.IndexOf('-') > -1)
            {
                var rangeParts = criteria.Split('-');
                if (!int.TryParse(rangeParts[0].Trim(), out int start) || !int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    session.Send($"%BRT%Cannot parse ID range.%PT%{Constants.NewLine}");
                    return;
                }
                if (start < 0 || end < 0)
                {
                    session.Send($"%BRT%Start and End must be greater than 0. End must be higher than Start.%PT%{Constants.NewLine}");
                    return;
                }
                if (end < start)
                {
                    session.Send($"%BRT%End must be greater than Start.%PT%{Constants.NewLine}");
                    return;
                }
                var rooms = RoomManager.Instance.GetRoom(start, end);
                if (rooms == null || rooms.Count == 0)
                {
                    session.Send($"%BRT%No Rooms in the specified ID range were found in Room Manager.%PT%{Constants.NewLine}");
                    return;
                }
                sb.AppendLine($"  {new string('=', 77)}");
                foreach (var room in rooms)
                {
                    sb.AppendLine($"|| {room.ID} - {room.RoomName}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                session.Send(sb.ToString());
                return;
            }
            var matchingRooms = RoomManager.Instance.GetRoom(criteria);
            if (matchingRooms == null || matchingRooms.Count == 0)
            {
                session.Send($"%BRT%No Rooms matching the given criteria were found in Room Manager.%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var room in matchingRooms)
            {
                sb.AppendLine($"|| {room.ID} - {room.RoomName}");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
            return;
        }

        private static void ListConnections(Session session)
        {
            var connections = SessionManager.Instance.AllSessions;
            if (connections == null || connections.Count == 0)
            {
                session.Send($"%BRT%No connected sessions.%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            foreach (var con in connections)
            {
                sb.AppendLine($"|| ID: {con.ID}");
                sb.AppendLine($"|| Remote IP: {con.Client?.Client?.RemoteEndPoint}");
                sb.AppendLine($"|| Connect Time: {con.ConnectionTime}");
                sb.AppendLine($"|| Player: {con.Player?.Name ?? "None"}");
                sb.AppendLine($"||{new string('=', 77)}");
            }
            sb.AppendLine($"|| {connections.Count} connections");
            sb.AppendLine($"  {new string('=', 77)}");
            session.Send(sb.ToString());
        }
        #endregion
    }
}
