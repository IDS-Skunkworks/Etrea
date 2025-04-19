using Etrea3.Objects;
using System;
using System.Linq;

namespace Etrea3.Core
{
    public static partial class ActMob
    {
        public static void MobMove(NPC mob, string args, Session session)
        {
            if (!mob.CanMove())
            {
                session?.Send($"%BRT%{mob.Name} cannot move right now!%PT%{Constants.NewLine}");
                return;
            }
            var fullDirection = Helpers.GetFullDirectionString(args);
            var r = RoomManager.Instance.GetRoom(mob.CurrentRoom);
            if (!r.HasExitInDirection(fullDirection))
            {
                session?.Send($"%BRT%{mob.Name} cannot move {fullDirection}, there is no such exit!%PT%{Constants.NewLine}");
                return;
            }
            var exit = r.GetRoomExit(fullDirection);
            var destRoom = RoomManager.Instance.GetRoom(exit.DestinationRoomID);
            if (destRoom == null)
            {
                session?.Send($"%BRT%{mob.Name} cannot move {fullDirection}, there is only the void...%PT%{Constants.NewLine}");
                return;
            }
            if (ZoneManager.Instance.GetZoneForRID(destRoom.ID).ZoneID != mob.ZoneID)
            {
                session?.Send($"%BRT%{mob.Name} cannot move {fullDirection}, that is outside its assigned Zone!%PT%{Constants.NewLine}");
                return;
            }
            if (destRoom.Flags.HasFlag(RoomFlags.NoMobs))
            {
                session?.Send($"%BRT%Some mystical force prevents {mob.Name} from moving {fullDirection}!%PT%{Constants.NewLine}");
                return;
            }
            mob.Move(exit.DestinationRoomID, false);
        }

        public static void MobTakeItem(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%You want {mob.Name} to take what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            var room = RoomManager.Instance.GetRoom(mob.CurrentRoom);
            if (ulong.TryParse(argElements[0], out ulong amount))
            {
                // getting gold from the room
                if (room.GoldInRoom == 0)
                {
                    session?.Send($"%BRT%There is no gold here for {mob.Name} to take!%PT%{Constants.NewLine}");
                    return;
                }
                amount = Math.Min(amount, room.GoldInRoom);
                RoomManager.Instance.RemoveGoldFromRoom(room.ID, amount);
                mob.Gold += amount;
                foreach(var lp in room.PlayersInRoom)
                {
                    var msg = mob.CanBeSeenBy(lp.Player) ? $"%BYT%{mob.Name} snatches up {amount:N0} gold!%PT%{Constants.NewLine}" : $"%BYT%Something snatches up {amount:N0} gold!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            else
            {
                // getting an item from the room
                if (room.ItemsInRoom.Count == 0)
                {
                    session?.Send($"%BRT%There is nothing here for {mob.Name} to take!%PT%{Constants.NewLine}");
                    return;
                }
                var item = room.GetItem(args);
                if (item == null)
                {
                    session?.Send($"%BRT%{mob.Name} cannot take that, it doesn't seem to be here!%PT%{Constants.NewLine}");
                    return;
                }
                RoomManager.Instance.RemoveItemFromRoomInventory(room.ID, item);
                mob.AddItemToInventory(item);
                foreach(var lp in room.PlayersInRoom)
                {
                    var msg = mob.CanBeSeenBy(lp.Player) ? $"%BYT%{mob.Name} snatches up {item.ShortDescription}!%PT%{Constants.NewLine}" : $"%BYT%Something snatches up {item.ShortDescription}!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
        }

        public static void MobDropItem(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%You want {mob.Name} to drop what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            if (ulong.TryParse(argElements[0], out ulong gp))
            {
                // dropping gold
                if (mob.Gold == 0)
                {
                    session?.Send($"%BRT%{mob.Name} doesn't have any gold to drop!%PT%{Constants.NewLine}");
                    return;
                }
                var amount = Math.Min(gp, mob.Gold);
                RoomManager.Instance.AddGoldToRoom(mob.CurrentRoom, amount);
                mob.Gold -= amount;
                foreach(var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom)
                {
                    var msg = mob.CanBeSeenBy(lp.Player) ? $"%BYT%{mob.Name} drops {amount:N0} gold coins on the floor!%PT%{Constants.NewLine}" : $"%BYT%Something drops {amount:N0} gold coins on the floor!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            else
            {
                // dropping item
                if (mob.Inventory.Count == 0)
                {
                    session?.Send($"%BRT%{mob.Name} isn't carrying anything to drop!%PT%{Constants.NewLine}");
                    return;
                }
                var item = mob.Inventory.Values.FirstOrDefault(x => x.Name.IndexOf(args, StringComparison.OrdinalIgnoreCase) >= 0);
                if (item == null)
                {
                    session?.Send($"%BRT%{mob.Name} isn't carrying anything like that!%PT%{Constants.NewLine}");
                    return;
                }
                mob.RemoveItemFromInventory(item);
                RoomManager.Instance.AddItemToRoomInventory(mob.CurrentRoom, item);
                foreach (var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom)
                {
                    var msg = mob.CanBeSeenBy(lp.Player) ? $"%BYT%{mob.Name} drop {item.ShortDescription} onto the floor!%PT%{Constants.NewLine}" : $"%BGT%Something drops {item.ShortDescription} onto the floor!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }    
        }

        public static void MobGiveItem(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%Give what to who now?%PT%{Constants.NewLine}");
                return;
            }
            if (mob.Gold == 0 && mob.Inventory.Count == 0)
            {
                session?.Send($"%BRT%{mob.Name} doesn't have anything to give!%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            if (argElements.Length < 2)
            {
                session?.Send($"%BRT%{mob.Name} can't do that, the command isn't correct!%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(mob.CurrentRoom).GetActor(argElements[0], mob);
            if (target == null)
            {
                session?.Send($"%BRT%{mob.Name} can't do that, that person isn't here!%PT%{Constants.NewLine}");
                return;
            }
            if (ulong.TryParse(argElements[1], out ulong gp))
            {
                // giving gold
                if (mob.Gold == 0)
                {
                    session?.Send($"%BRT%{mob.Name} doesn't have any gold to give!%PT%{Constants.NewLine}");
                    return;
                }
                var amount = Math.Min(gp, mob.Gold);
                mob.Gold -= amount;
                target.Gold += amount;
                if (target.ActorType == ActorType.Player)
                {
                    var p = (Player)target;
                    var msg = mob.CanBeSeenBy(p) ? $"%BYT%{mob.Name} hands you {amount:N0} gold coins!%PT%{Constants.NewLine}" : $"%BYT%Something hands you {amount:N0} gold coins!%PT%{Constants.NewLine}";
                    p.Send(msg);
                }
                foreach (var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom.Where(x => x.ID != target.ID))
                {
                    var mobName = mob.CanBeSeenBy(lp.Player) ? mob.Name : "Something";
                    var tName = target.CanBeSeenBy(lp.Player) ? target.Name : "someone";
                    lp.Send($"%BYT%{mobName} hands {amount:N0} gold coins to {tName}!%PT%{Constants.NewLine}");
                }
            }
            else
            {
                // giving item
                var line = args.Remove(0, argElements[0].Length).Trim();
                var item = mob.Inventory.Values.FirstOrDefault(x => x.Name.IndexOf(line, StringComparison.OrdinalIgnoreCase) >= 0);
                if (item == null)
                {
                    session?.Send($"%BRT%{mob.Name} isn't carrying anything like that!%PT%{Constants.NewLine}");
                    return;
                }
                mob.RemoveItemFromInventory(item);
                target.AddItemToInventory(item);
                if (target.ActorType == ActorType.Player)
                {
                    var p = (Player)target;
                    var msg = mob.CanBeSeenBy(target) ? $"%BGT%{mob.Name} hands you {item.ShortDescription}!%PT%{Constants.NewLine}" : $"%BGT%Something hands you {item.ShortDescription}!%PT%{Constants.NewLine}";
                    p.Send(msg);
                }
                string lpMsg = $"%BGT%%N% hands %T% {item.ShortDescription}!%PT%{Constants.NewLine}";
                foreach(var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom.Where(x => x.ID != target.ID))
                {
                    var mobName = mob.CanBeSeenBy(lp.Player) ? mob.Name : "Something";
                    var tName = target.CanBeSeenBy(lp.Player) ? target.Name : "someone";
                    lp.Send($"%BGT%{mobName} hands {tName} {item.ShortDescription}!%PT%{Constants.NewLine}");
                }
            }
        }

        public static void MobAttack(NPC mob, string args, Session session)
        {
            if (mob.Flags.HasFlag(NPCFlags.NoAttack))
            {
                session?.Send($"%BRT%{mob.Name} has the NoAttack flag and cannot fight!%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(mob.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
            {
                session?.Send($"%BRT%{mob.Name} is in a safe room and cannot fight!%PT%{Constants.NewLine}");
                return;
            }
            if (mob.InCombat)
            {
                session?.Equals($"%BRT%{mob.Name} is already fighting something!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%You want {mob.Name} to fight who, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(mob.CurrentRoom).GetActor(args, mob);
            if (target == null)
            {
                session?.Send($"%BRT%{mob.Name} cannot start a fight with {args}, that person can't be found!%PT%{Constants.NewLine}");
                return;
            }
            if (target.InCombat)
            {
                session?.Send($"%BRT%{mob.Name} cannot start a fight with {target.Name}, they are already fighting!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType == ActorType.NonPlayer)
            {
                var n = (NPC)target;
                if (n.Flags.HasFlag(NPCFlags.NoAttack))
                {
                    session?.Send($"%BRT%{mob.Name} cannot start a fight with {target.Name}, that NPC has the NoAttack flag!%PT%{Constants.NewLine}");
                    return;
                }
            }
            if (target.ActorType == ActorType.Player)
            {
                var p = (Player)target;
                p.Send($"%BRT%{mob.Name} suddenly attacks you!%PT%{Constants.NewLine}");
            }
            mob.AddToTargetQueue(target);
            target.AddToTargetQueue(mob);
        }

        public static void MobEmote(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%{mob.Name} should Emote what?%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            if (argElements.Length <= 2)
            {
                var emote = EmoteManager.Instance.GetEmote(argElements[0]);
                if (emote == null)
                {
                    session?.Send($"%BRT%{mob.Name} can't do that: no such Emote was found!%PT%{Constants.NewLine}");
                    return;
                }
                var line = args.Remove(0, argElements[0].Length).Trim();
                var target = !string.IsNullOrEmpty(line) ? RoomManager.Instance.GetRoom(mob.CurrentRoom).GetActor(line, mob) : null;
                emote.Perform(mob, target, !string.IsNullOrEmpty(line), line);
            }
            else
            {
                foreach (var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom)
                {
                    var pName = mob.CanBeSeenBy(lp.Player) ? mob.Name : "Something";
                    lp.Send($"{pName} {args}{Constants.NewLine}");
                }
            }
        }

        public static void MobCastSpell(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%You want {mob.Name} to cast what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            if (mob.Spells.Count == 0)
            {
                session?.Send($"%BRT%{mob.Name} doesn't know any spells!%PT%{Constants.NewLine}");
                return;
            }
            var spellName = Helpers.GetQuotedString(args);
            var spell = SpellManager.Instance.GetSpell(spellName);
            if (spell == null)
            {
                session?.Send($"%BRT%{mob.Name} can't cast that, no such Spell exists in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (!mob.KnowsSpell(spell.Name))
            {
                session?.Send($"%BRT%{mob.Name} doesn't know the Spell {spell.Name}!%PT%{Constants.NewLine}");
                return;
            }
            var targetName = args.Remove(0, spellName.Length + 2);
            var target = RoomManager.Instance.GetRoom(mob.CurrentRoom).GetActor(targetName, mob);
            if (target == null)
            {
                session?.Send($"%BRT%{mob.Name} can't cast that Spell on {targetName}, they aren't here!%PT%{Constants.NewLine}");
                return;
            }
            spell.Cast(mob, target);
        }

        public static void MobSay(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%{mob.Name} can't comply, they have nothing to say!%PT%{Constants.NewLine}");
                return;
            }
            foreach (var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom)
            {
                lp.Send($"%BGT%{mob.Name} says \"{args}\"%PT%{Constants.NewLine}");
            }
        }

        public static void MobYell(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%You want {mob.Name} to yell what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var currentZone = ZoneManager.Instance.GetZoneForRID(mob.CurrentRoom);
            var zonePlayers = SessionManager.Instance.ActivePlayers.Where(x => ZoneManager.Instance.GetZoneForRID(x.Player.CurrentRoom) == currentZone).ToList();
            foreach (var player in zonePlayers)
            {
                switch(mob.CanBeSeenBy(player.Player))
                {
                    case true:
                        player.Send($"%BGT%{mob.Name} yells \"{args}\"%PT%{Constants.NewLine}");
                        break;

                    case false:
                        player.Send($"%BGT%Something yells \"{args}\"%PT%{Constants.NewLine}");
                        break;
                }
            }
        }

        public static void MobWhisper(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%Whisper what to who, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            if (argElements.Length < 1)
            {
                session?.Send($"%BRT%{mob.Name} cannot do that!%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(mob.CurrentRoom).GetActor(argElements[0].Trim(), mob);
            if (target == null)
            {
                session?.Send($"%BRT%{mob.Name} cannot whisper to {argElements[0].Trim()}, that person isn't here!%PT%{Constants.NewLine}");
                return;
            }
            var line = args.Remove(0, argElements[0].Length).Trim();
            if (target.ActorType == ActorType.Player)
            {
                ((Player)target).Send($"%BYT%{mob.Name} whispers \"{line}\"%PT%{Constants.NewLine}");
            }
            foreach (var lp in RoomManager.Instance.GetRoom(mob.CurrentRoom).PlayersInRoom.Where(x => x.ID != target.ID))
            {
                var mobName = mob.CanBeSeenBy(lp.Player) ? mob.Name : "Something";
                var tName = target.CanBeSeenBy(lp.Player) ? target.Name : "someone";
                lp.Send($"%BYT%{mobName} whisers something to {tName}.%PT%{Constants.NewLine}");
            }
        }

        public static void MobRememberPlayer(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%{mob.Name} should remember who, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            var target = SessionManager.Instance.GetSession(argElements[0].Trim());
            if (target == null)
            {
                session?.Send($"%BRT%{mob.Name} cannot remember {argElements[0]} that person isn't in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            mob.RememberPlayer(target.Player, Game.TickCount);
            session?.Send($"%BGT%{mob.Name} now remembers {target.Player.Name}%PT%{Constants.NewLine}");
        }

        public static void MobForgetPlayer(NPC mob, string args, Session session)
        {
            if (string.IsNullOrEmpty(args))
            {
                session?.Send($"%BRT%{mob.Name} should forget who, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var argElements = args.Split(' ');
            var target = SessionManager.Instance.GetSession(argElements[0].Trim());
            if (target != null)
            {
                mob.ForgetPlayer(target.Player);
                session?.Send($"%BGT%{mob.Name} has now forgotten {target.Player.Name}%PT%{Constants.NewLine}");
            }
        }
    }
}
