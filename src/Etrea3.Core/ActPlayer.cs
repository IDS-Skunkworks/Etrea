using Etrea3.Objects;
using System;
using System.Linq;
using System.Text;

namespace Etrea3.Core
{
    public static class ActPlayer
    {
        #region Communication
        public static void PlayerMailAction(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.PostBox))
            {
                session.Send($"%BRT%There is no Mailbox here!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: mail <list | read | write | delete>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail list - list all your mail, if any%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail read <id> - read the specified mail%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail delete <id> - delete the specified mail%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail write - write a mail to another player%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length == 0)
            {
                session.Send($"%BRT%Usage: mail <list | read | write | delete>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail list - list all your mail, if any%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail read <id> - read the specified mail%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail delete <id> - delete the specified mail%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: mail write - write a mail to another player%PT%{Constants.NewLine}");
                return;
            }
            var operation = args[0].Trim().ToLower();
            if (operation == "list")
            {
                PlayerMail.ListMail(session);
                return;
            }
            if (operation == "read")
            {
                if (args.Length < 2)
                {
                    session.Send($"%BRT%Usage: mail <list | read | write | delete>%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail list - list all your mail, if any%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail read <id> - read the specified mail%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail delete <id> - delete the specified mail%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail write - write a mail to another player%PT%{Constants.NewLine}");
                    return;
                }
                if (!int.TryParse(args[1].Trim(), out int mailID))
                {
                    session.Send($"%BRT%That isn't a valid Mail ID!%PT%{Constants.NewLine}");
                    return;
                }
                PlayerMail.ReadMail(session, mailID);
                return;
            }
            if (operation == "write")
            {
                PlayerMail.ComposeMail(session);
                return;
            }
            if (operation == "delete")
            {
                if (args.Length < 2)
                {
                    session.Send($"%BRT%Usage: mail <list | read | write | delete>%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail list - list all your mail, if any%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail read <id> - read the specified mail%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail delete <id> - delete the specified mail%PT%{Constants.NewLine}");
                    session.Send($"%BRT%Usage: mail write - write a mail to another player%PT%{Constants.NewLine}");
                    return;
                }
                if (!int.TryParse(args[1].Trim(), out int mailID))
                {
                    session.Send($"%BRT%That isn't a valid Mail ID!%PT%{Constants.NewLine}");
                    return;
                }
                PlayerMail.DeleteMail(session, mailID);
                return;
            }
            session.Send($"%BRT%Usage: mail <list | read | write | delete>%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: mail list - list all your mail, if any%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: mail read <id> - read the specified mail%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: mail delete <id> - delete the specified mail%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: mail write - write a mail to another player%PT%{Constants.NewLine}");
        }

        public static void CharShout(Session session, ref string arg)
        {
            if (session.Player.Flags.HasFlag(PlayerFlags.Mute))
            {
                session.Send($"%BMT%No can do, you have been muted!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"Shout what, exactly?{Constants.NewLine}");
                return;
            }
            var currentZone = ZoneManager.Instance.GetZoneForRID(session.Player.CurrentRoom);
            var zonePlayers = SessionManager.Instance.ActivePlayers.Where(x => x.ID != session.ID && ZoneManager.Instance.GetZoneForRID(x.Player.CurrentRoom) == currentZone).ToList();
            session.Send($"%BGT%You bellow \"{arg}\" as loudly as you can! Hope everyone can hear!{Constants.NewLine}%PT%");
            if (zonePlayers != null && zonePlayers.Count > 0)
            {
                foreach (var p in zonePlayers)
                {
                    string pName = session.Player.CanBeSeenBy(p.Player) ? session.Player.Name : "Someone";
                    string msg = string.Empty;
                    switch(p.Player.KnowsLanguage(session.Player.SpokenLanguage))
                    {
                        case true:
                            msg = $"%BGT%{pName} bellows \"{arg}\"{Constants.NewLine}%PT%";
                            break;

                        case false:
                            msg = $"%BGT%{pName} bellows something in {session.Player.SpokenLanguage} which you don't understand.{Constants.NewLine}%PT%";
                            break;
                    }
                    p.Send(msg);
                }
            }
        }

        public static void CharWhisper(Session session, ref string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                session.Send($"Whisper what to who?{Constants.NewLine}");
                return;
            }
            string[] elements = line.Split(' ');
            if (elements.Length == 0)
            {
                session.Send($"Whisper what to who?{Constants.NewLine}");
                return;
            }
            string targetName = elements[0].Trim();
            string toSay = line.Remove(0, targetName.Length).Trim();
            if (string.IsNullOrEmpty(toSay))
            {
                session.Send($"Whisper what, exactly?{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(targetName, session.Player);
            if (target != null && target.ActorType == ActorType.NonPlayer)
            {
                if (session.Player.Flags.HasFlag(PlayerFlags.Mute))
                {
                    session.SendSystem($"%BMT%No can do, you have been muted!%PT%{Constants.NewLine}");
                    return;
                }
                session.Send($"You whisper \"{toSay}\" to {target.Name}{Constants.NewLine}");
                foreach(var lp in RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom)
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"{session.Player.Name} whispers something to {target.Name}{Constants.NewLine}" :
                        $"Something whispers something to {target.Name}{Constants.NewLine}";
                    lp.Send(msg);
                }
                var nTarget = (NPC)target;
                if (nTarget.MobProgs.Count > 0)
                {
                    foreach(var mp in nTarget.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerWhisper, new { mob = nTarget.ID.ToString(), player = session.ID.ToString(), whisper = toSay });
                        }
                    }
                }
            }
            else
            {
                var tp = SessionManager.Instance.ActivePlayers.Where(x => x.Player.CanBeSeenBy(session.Player) && x.Player.Name.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();
                if (tp == null)
                {
                    session.Send($"That person doesn't seem to be in the Realms right now...{Constants.NewLine}");
                    return;
                }
                if (session.Player.Flags.HasFlag(PlayerFlags.Mute) && !tp.Player.IsImmortal)
                {
                    session.SendSystem($"%BMT%No can do, you have been muted!%PT%{Constants.NewLine}");
                    return;
                }
                session.Send($"You whisper \"{toSay}\" to {tp.Player.Name}{Constants.NewLine}");
                string pName = session.Player.CanBeSeenBy(tp.Player) ? session.Player.Name : "Someone";
                tp.SendSystem($"%BYT%{pName} whispers \"{toSay}\"{Constants.NewLine}%PT%");
            }
        }

        public static void CharSayRoom(Session session, ref string saying)
        {
            if (string.IsNullOrEmpty(saying))
            {
                session.Send($"Say what, exactly?{Constants.NewLine}");
                return;
            }
            if (session.Player.Flags.HasFlag(PlayerFlags.Mute))
            {
                session.SendSystem($"%BMT%No can do, you have been muted!%PT%{Constants.NewLine}");
                return;
            }
            session.Send($"%BGT%You say \"{saying}\"{Constants.NewLine}%PT%");
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
            if (localPlayers != null && localPlayers.Count > 0)
            {
                foreach(var lp in localPlayers)
                {
                    string msg = string.Empty;
                    string pName = session.Player.CanBeSeenBy(lp.Player) ? session.Player.Name : "Something";
                    switch(lp.Player.KnowsLanguage(session.Player.SpokenLanguage))
                    {
                        case true:
                            msg = $"%BGT%{pName} says \"{saying}\"%PT%{Constants.NewLine}";
                            break;

                        case false:
                            msg = $"%BGT%{pName} says something in {session.Player.SpokenLanguage} which you don't understand!%PT%{Constants.NewLine}";
                            break;
                    }
                    lp.Send(msg);
                }
            }
            foreach(var n in RoomManager.Instance.GetRoom(session.Player.CurrentRoom).NPCsInRoom)
            {
                if (n.MobProgs.Count > 0)
                {
                    foreach(var mp in n.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerSay, new { mob = n.ID.ToString(), player = session.ID.ToString(), say = saying });
                        }
                    }
                }
            }
        }

        public static void PlayerLanguages(Session session, string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BGT%You are currently speaking {session.Player.SpokenLanguage}.%PT%{Constants.NewLine}");
                session.Send($"%BGT%You know the following lanuages: {string.Join(", ", session.Player.KnownLanguages.Keys)}%PT%{Constants.NewLine}");
                return;
            }
            var lang = Constants.Languages.FirstOrDefault(x => x.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0) ?? string.Empty;
            if (string.IsNullOrEmpty(lang))
            {
                session.Send($"%BRT%No such language exists within the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (!session.Player.KnowsLanguage(lang))
            {
                session.Send($"%BRT%You don't know {lang}!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.SpokenLanguage = lang;
            session.Send($"%BGT%You're now speaking {lang}!%PT%{Constants.NewLine}");
        }
        #endregion

        #region Misc
        public static void ShowWorldTime(Session session)
        {
            var day = DateTime.UtcNow.Day;
            var month = Constants.MonthNames[(DateTime.UtcNow.Month - 1) % 12];
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Clock))
            {
                var tod = DateTime.UtcNow.ToString("HH:mm");
                session.Send($"%BYT%It is {tod} on the {Helpers.GetOrdinal(day)} of {month}.%PT%{Constants.NewLine}");
            }
            else
            {
                var tod = Helpers.GetTimeOfDay().ToString().ToLower();
                session.Send($"%BYT%It is the {tod} of the {Helpers.GetOrdinal(day)} of {month}.%PT%{Constants.NewLine}");
            }
        }

        public static void PlayerSummon(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to say who you're summoning!%PT%{Constants.NewLine}");
                return;
            }
            if (!session.Player.HasSkill("Summon"))
            {
                session.Send($"%BRT%You lack the skill to summon other people!%PT%{Constants.NewLine}");
                return;
            }
            var target = SessionManager.Instance.GetSession(arg);
            if (target == null)
            {
                session.Send($"%BRT%That person doesn't seem to be in the Realms right now...%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.CurrentMP < 10)
            {
                session.Send($"%BRT%You lack the magical reserves to summon anyone!%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.IsImmortal)
            {
                session.Send($"%BRT%Attempting to summon the Gods would be a bad idea...%PT%{Constants.NewLine}");
                return;
            }
            if (!target.Player.CanMove())
            {
                session.Send($"%BRT%{target.Player.Name} cannot be teleported right now...%PT%{Constants.NewLine}");
                return;
            }
            if (target.Player.Flags.HasFlag(PlayerFlags.NoSummon))
            {
                session.Send($"%BRT%{target.Player.Name} refuses to be summoned!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.AdjustMP(-10);
            target.Send($"%BMT%{session.Player.Name} is summoning you!%PT%{Constants.NewLine}");
            session.Send($"%BYT%Calling on the Winds of Magic you reach through reality and transport them!%PT%{Constants.NewLine}");
            Game.LogMessage($"INFO: Player {session.Player.Name} summoned {target.Player.Name} from Room {target.Player.CurrentRoom} to {session.Player.CurrentRoom}", LogLevel.Info);
            target.Player.Move(session.Player.CurrentRoom, true);
        }

        public static void TogglePlayerFlag(Session session, ref string arg)
        {
            var args = arg.Split(' ');
            if (args.Length != 2)
            {
                session.Send($"%BRT%Usage: toggle <flag> <on | off> - turn the specified flag on or off%PT%{Constants.NewLine}");
                return;
            }
            if (!Enum.TryParse<PlayerFlags>(args[0].Trim(), true, out var flag))
            {
                session.Send($"%BRT%{args[0]} is not a valid flag!%PT%{Constants.NewLine}");
                return;
            }
            bool flagEnabled = args[1].Trim().ToLower() == "on";
            switch(flag)
            {
                case PlayerFlags.NoShowExits:
                    session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                    string msg = flagEnabled ? $"%BGT%You will no longer see exits in room descriptions%PT%{Constants.NewLine}" :
                        $"%BGT%You will now see exits in room descriptions%PT%{Constants.NewLine}";
                    session.Send(msg);
                    break;

                case PlayerFlags.NoSummon:
                    session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                    msg = flagEnabled ? $"%BGT%Other players will no longer be able to summon you%PT%{Constants.NewLine}" :
                        $"%BGT%You can now be summoned by other players%PT%{Constants.NewLine}";
                    session.Send(msg);
                    break;

                case PlayerFlags.NoHassle:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will no longer be attacked by hostile NPCS%PT%{Constants.NewLine}" :
                            $"%BGT%You can now be attacked by hostile NPCS%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogError:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live error logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live error logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogWarn:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live warning logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live warning logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogConnection:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live connection logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live connection logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogCombat:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live combat logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live combat logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogShops:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live shop logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live shop logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogInfo:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live info logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live info logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogOLC:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live OLC logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live OLC logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogGod:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live God logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live God logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.MUDLogDebug:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will now see live debug logs%PT%{Constants.NewLine}" :
                            $"%BGT%You will no longer see live debug logs%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                case PlayerFlags.NoShowRoomFlags:
                    if (session.Player.IsImmortal)
                    {
                        session.Player.Flags = flagEnabled ? session.Player.Flags |= flag : session.Player.Flags &= ~flag;
                        msg = flagEnabled ? $"%BGT%You will no longer see room flags in descriptions%PT%{Constants.NewLine}" :
                            $"%BGT%You will now see room flags in descriptions%PT%{Constants.NewLine}";
                        session.Send(msg);
                    }
                    else
                    {
                        session.Send($"%BRT%That flag can only be toggled by the Gods!%PT%{Constants.NewLine}");
                    }
                    break;

                default:
                    session.Send($"%BRT%The flag is either invalid or cannot be toggled!%PT%{Constants.NewLine}");
                    break;
            }
        }

        public static void ShowHelp(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: help <subject> - show help on the subject%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: help shop - shop help on shops and shopping%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: help basics - show help on the basics of playing%PT%{Constants.NewLine}");
                return;
            }
            var article = HelpManager.Instance.GetArticle(arg.ToLower());
            if (article == null)
            {
                session.Send($"%BRT%No Help Articles found for that subject. Please contact an Imm to have one created!%PT%{Constants.NewLine}");
                return;
            }
            if (article.ImmOnly && !session.Player.IsImmortal)
            {
                session.Send($"%BRT%Only the Gods can read that!%PT%{Constants.NewLine}");
                return;
            }
            session.Send(article.ArticleText);
        }

        public static void PlayerListEmotes(Session session, string arg)
        {
            var matchingEmotes = string.IsNullOrEmpty(arg) ? EmoteManager.Instance.GetEmote().OrderBy(x => x.Name).ToList() : EmoteManager.Instance.GetEmote().Where(x => x.Name.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.Name).ToList();
            if (matchingEmotes == null || matchingEmotes.Count == 0)
            {
                session.Send($"%BRT%No matching Emotes found!%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            string l = "%BYT%||%PT% ";
            int c = 0;
            for (int i = 0; i < matchingEmotes.Count; i++)
            {
                if (matchingEmotes[i].Name.Length < 4)
                {
                    l = $"{l}{matchingEmotes[i].Name}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}";
                }
                else
                {
                    l = $"{l}{matchingEmotes[i].Name}{Constants.TabStop}{Constants.TabStop}";
                }
                
                c++;
                if (c >= 4 && i < matchingEmotes.Count)
                {
                    c = 0;
                    sb.AppendLine(l.Trim());
                    l = "%BYT%||%PT% ";
                }
                if (c == matchingEmotes.Count)
                {
                    sb.AppendLine(l.Trim());
                }
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void PlayerEmote(Session session, string verb, ref string arg)
        {
            if (verb.ToLower() == "emote")
            {
                if (string.IsNullOrEmpty(arg))
                {
                    session.Send($"%BRT%Usage: emote <emote string> - perform a custom emote%PT%{Constants.NewLine}");
                    return;
                }
                foreach(var lp in RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID))
                {
                    var pName = session.Player.CanBeSeenBy(lp.Player) ? lp.Player.Name : "Someone";
                    lp.Send($"{pName} {arg}{Constants.NewLine}");
                }
            }
            else
            {
                var emote = EmoteManager.Instance.GetEmote(verb);
                if (emote == null)
                {
                    session.Send($"%BRT%You can't do that, no such Emote exists!%PT%{Constants.NewLine}");
                    return;
                }
                if (!string.IsNullOrEmpty(arg))
                {
                    var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(arg, session.Player);
                    emote.Perform(session.Player, target, !string.IsNullOrEmpty(arg), arg);
                }
                else
                {
                    emote.Perform(session.Player, null, !string.IsNullOrEmpty(arg), arg);
                }
            }
        }

        public static void PlayerChangeShortDesc(Session session)
        {
            session.Send($"%BYT%Your current short description is:%PT%{Constants.NewLine}");
            session.Send($"%BYT%{session.Player.ShortDescription}%PT%{Constants.NewLine}");
            session.Send($"%BYT%Enter new Short Description (max. 50 Characters):%PT%");
            var input = session.Read();
            if (string.IsNullOrEmpty(input))
            {
                session.Send($"%BRT%Aborting description change.%PT%{Constants.NewLine}");
                return;
            }
            if (input.Trim().Length <= 50)
            {
                session.Player.ShortDescription = input.Trim();
            }
            else
            {
                session.Send($"%BRT%That is too long to be a Short Description!%PT%{Constants.NewLine}");
            }
        }

        public static void PlayerChangeLongDesc(Session session)
        {
            var lDesc = Helpers.GetLongDescription(session);
            session.Player.LongDescription = lDesc;
            session.Send($"%BGT%Your Long Description has been updated successfully.%PT%{Constants.NewLine}");
        }

        public static void PlayerChangeTitle(Session session)
        {
            session.Send($"%BYT%Your current title is:%PT%{Constants.NewLine}");
            session.Send($"%BYT%{session.Player.Title}%PT%{Constants.NewLine}");
            session.Send($"%BYT%Enter new Title:%PT%");
            var input = session.Read();
            if (string.IsNullOrEmpty(input))
            {
                session.Send($"%BRT%Aborting title change.%PT%{Constants.NewLine}");
                return;
            }
            if (input.Trim().Length <= 50)
            {
                session.Player.Title = input.Trim();
            }
            else
            {
                session.Send($"%BRT%That is too long to be a Title!%PT%{Constants.NewLine}");
            }
        }

        public static void PlayerChangePassword(Session session)
        {
            Game.LogMessage($"INFO: Player {session.Player.Name} connecting from {session.Client.Client.RemoteEndPoint} has started the password change process", LogLevel.Info);
            while (true)
            {
                session.Send($"%BRT%*** PASSWORD CHANGE IN PROGRESS ***%PT%{Constants.NewLine}");
                session.Send($"%BRT%*** Type ***END*** to abort the process%PT%{Constants.NewLine}");
                session.Send($"Enter your current password: ");
                var curPwd = session.Read();
                if (string.IsNullOrEmpty(curPwd))
                {
                    continue;
                }
                if (curPwd.Trim() == "***END***")
                {
                    session.Send($"%BYT%Aborting password change process.%PT%{Constants.NewLine}");
                    return;
                }
                if (!DatabaseManager.ValidatePlayerPassword(session.Player.Name, curPwd.Trim()))
                {
                    session.Send($"%BRT%Invalid password, reset process cannot continue. This has been logged.%PT%{Constants.NewLine}");
                    Game.LogMessage($"WARN: Player {session.Player.Name} connecting from {session.Client.Client.RemoteEndPoint} provided an invalid password ({curPwd.Trim()}) during the password reset process", LogLevel.Warning);
                    return;
                }
                session.Send($"%BRT%Enter new password: ");
                var newPwd = session.Read();
                if (string.IsNullOrEmpty(newPwd))
                {
                    continue;
                }
                if (newPwd.Trim() == "***END***")
                {
                    session.Send($"%BYT%Aborting password change process.%PT%{Constants.NewLine}");
                    return;
                }
                if (DatabaseManager.ChangePlayerPassword(session.Player.Name, newPwd.Trim()))
                {
                    Game.LogMessage($"INFO: Player {session.Player.Name} connecting from {session.Client.Client.RemoteEndPoint} has changed their password", LogLevel.Info);
                    session.Send($"%BGT%Your password has been changed!%PT%{Constants.NewLine}");
                    return;
                }
                else
                {
                    session.Send($"%BRT%Failed to change password. Please retry or see an Imm!%PT%{Constants.NewLine}");
                    return;
                }
            }
        }

        public static void PlayerCraftItem(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Craft what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            CraftingRecipe r = null;
            if (int.TryParse(arg.Trim(), out int recipeID))
            {
                r = RecipeManager.Instance.GetRecipe(recipeID);
            }
            else
            {
                r = RecipeManager.Instance.GetRecipe(arg).FirstOrDefault();
            }
            if (r == null)
            {
                session.Send($"%BRT%No such Crafting Recipe exists in the Realms!%PT%{Constants.NewLine}");
                return;
            }
            r.Craft(session);
        }

        public static void PlayerMineNode(Session session)
        {
            var r = RoomManager.Instance.GetRoom(session.Player.CurrentRoom);
            if (r.RSSNode == null || r.RSSNode.Depth == 0)
            {
                session.Send($"%BRT%There is nothing here you can mine!%PT%{Constants.NewLine}");
                return;
            }
            if (!session.Player.HasSkill("Mining"))
            {
                session.Send($"%BRT%You don't know how to mine for resources!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.GetInventoryItem("Mining Tools") == null)
            {
                session.Send($"%BRT%You don't have the right tools to do that!%PT%{Constants.NewLine}");
                return;
            }
            var itemID = r.RSSNode.Mine(out bool depleted);
            var item = (InventoryItem)ItemManager.Instance.GetItem(itemID);
            session.Player.AddItemToInventory(itemID);
            session.Send($"%BGT%You mine the resource node and discover {item.ShortDescription}!%PT%{Constants.NewLine}");
            if (depleted)
            {
                RoomManager.Instance.GetRoom(session.Player.CurrentRoom).RSSNode = null;
            }
        }

        public static void PlayerPickPocket(Session session, ref string arg)
        {
            if (!session.Player.HasSkill("Pickpocket"))
            {
                session.Send($"%BRT%You don't know how to do that!%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
            {
                session.Send($"%BYT%Some mystical force prevents you from doing that!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Pickpocket who, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(arg, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%The target of your thievery isn't here right now!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType == ActorType.Player)
            {
                session.Send($"%BYT%Some mysical force prevents you from stealing from {target.Name}!%PT%{Constants.NewLine}");
                return;
            }
            var tNPC = (NPC)target;
            if (tNPC.Flags.HasFlag(NPCFlags.NoPickpocket) || tNPC.Flags.HasFlag(NPCFlags.NoAttack))
            {
                session.Send($"%BYT%Some mystical force prevents you from stealing from {target.Name}!%PT%{Constants.NewLine}");
                return;
            }
            var pRoll = Math.Max(1, Helpers.RollDice<int>(1, 20) + Helpers.CalculateAbilityModifier(session.Player.Dexterity));
            var tRoll = Math.Max(1, Helpers.RollDice<int>(1, 20) + Helpers.CalculateAbilityModifier(tNPC.Intelligence));
            if (pRoll >= tRoll)
            {
                if (tNPC.Gold == 0 && (tNPC.Inventory == null || tNPC.Inventory.Count == 0))
                {
                    session.Send($"BGT%{tNPC.Name} doesn't have anything to steal!%PT%{Constants.NewLine}");
                    return;
                }
                if (tNPC.Gold > 0)
                {
                    var stealGold = Helpers.RollDice<ulong>(1, Convert.ToInt32(tNPC.Gold));
                    tNPC.Gold -= stealGold;
                    session.Player.AdjustGold((long)stealGold, true, false);
                    session.Send($"%BGT%You manage to swipe {stealGold:N0} gold from {tNPC.Name}!%PT%{Constants.NewLine}");
                }
                if (tNPC.Inventory != null && tNPC.Inventory.Count > 0)
                {
                    var itemID = tNPC.Inventory.GetRandomElement();
                    tNPC.Inventory.TryGetValue(itemID, out var item);
                    if (item != null)
                    {
                        tNPC.RemoveItemFromInventory(item);
                        session.Player.AddItemToInventory(item);
                        session.Send($"%BGT%You have stolen {item.ShortDescription} from {tNPC.Name}!%PT%{Constants.NewLine}");
                    }
                }
            }
            else
            {
                session.Send($"%BRT%{tNPC.Name} notices you trying to steal from them and makes ready to fight!%PT%{Constants.NewLine}");
                tNPC.TargetQueue.TryAdd(session.ID, true);
                session.Player.TargetQueue.TryAdd(tNPC.ID, true);
            }
        }

        public static void PlayerHide(Session session)
        {
            if (!session.Player.HasSkill("Hide"))
            {
                session.Send($"%BRT%You don't know how to do that!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.Visible)
            {
                session.Send($"%BGT%You shroud yourself and hide from view!%PT%{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
                if (localPlayers.Count > 0)
                {
                    foreach (var player in localPlayers)
                    {
                        player.Send($"%BGT%{session.Player.Name} shrouds themselves from view.%PT%{Constants.NewLine}");
                    }
                }
                session.Player.Visible = false;
            }
            else
            {
                session.Send($"%BGT%Your shroud falls away and you become visible again!%PT%{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
                if (localPlayers.Count > 0)
                {
                    foreach(var player in localPlayers)
                    {
                        player.Send($"%BGT%{session.Player.Name} shimmers and becomes visible again!%PT%{Constants.NewLine}");
                    }
                }
                session.Player.Visible = true;
            }
        }

        public static void LearnSkill(Session session, ref string arg)
        {
            var r = RoomManager.Instance.GetRoom(session.Player.CurrentRoom);
            if (!r.HasTrainer)
            {
                session.Send($"%BRT%There is no one here to teach you!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                StringBuilder sb = new StringBuilder();
                int matchingSkills = 0;
                int matchingRecipes = 0;
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                if (r.Flags.HasFlag(RoomFlags.SkillTrainer))
                {
                    var knownSkills = from skillName in session.Player.Skills.Keys select SkillManager.Instance.GetSkill(skillName);
                    var availableSkills = SkillManager.Instance.GetSkill().Except(knownSkills);
                    matchingSkills = availableSkills.Count();
                    if (matchingSkills > 0)
                    {
                        sb.AppendLine($"%BYT%|| Available Skills:%PT%");
                        foreach(var sk in availableSkills)
                        {
                            sb.AppendLine($"%BYT%|| {sk.Name} - {Helpers.GetPurchasePrice(session, sk.LearnCost):N0} gold%PT%");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| No Skills available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                if (r.Flags.HasFlag(RoomFlags.Alchemist))
                {
                    var knownRecipes = from recipeID in session.Player.Recipes.Keys select RecipeManager.Instance.GetRecipe(recipeID);
                    var availableRecipes = RecipeManager.Instance.GetRecipe(RecipeType.Alchemy).Except(knownRecipes);
                    matchingRecipes += availableRecipes.Count();
                    if (availableRecipes.Count() > 0)
                    {
                        sb.AppendLine($"%BYT%|| Available Alchemy Recipes%PT%");
                        foreach(var recipe in availableRecipes)
                        {
                            sb.AppendLine($"%BYT%|| {recipe.Name} - {Helpers.GetPurchasePrice(session, recipe.LearnCost):N0} gold%PT%");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| No Alchemy recipes available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                if (r.Flags.HasFlag(RoomFlags.Blacksmith))
                {
                    var knownRecipes = from recipeID in session.Player.Recipes.Keys select RecipeManager.Instance.GetRecipe(recipeID);
                    var availableRecipes = RecipeManager.Instance.GetRecipe(RecipeType.Blacksmith).Except(knownRecipes);
                    matchingRecipes += availableRecipes.Count();
                    if (availableRecipes.Count() > 0)
                    {
                        sb.AppendLine($"%BYT%|| Available Blacksmith Recipes%PT%");
                        foreach(var recipe in availableRecipes)
                        {
                            sb.AppendLine($"%BYT%|| {recipe.Name} - {Helpers.GetPurchasePrice(session, recipe.LearnCost):N0} gold%PT%");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| No Blacksmith recipes available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                if (r.Flags.HasFlag(RoomFlags.Chef))
                {
                    var knownRecipes = from recipeID in session.Player.Recipes.Keys select RecipeManager.Instance.GetRecipe(recipeID);
                    var availableRecipes = RecipeManager.Instance.GetRecipe(RecipeType.Cooking).Except(knownRecipes);
                    matchingRecipes += availableRecipes.Count();
                    if (availableRecipes.Count() > 0)
                    {
                        sb.AppendLine($"%BYT%|| Available Cooking Recipes%PT%");
                        foreach (var recipe in availableRecipes)
                        {
                            sb.AppendLine($"%BYT%|| {recipe.Name} - {Helpers.GetPurchasePrice(session, recipe.LearnCost):N0} gold%PT%");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| No Cooking recipes available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                if (r.Flags.HasFlag(RoomFlags.Jeweler))
                {
                    var knownRecipes = from recipeID in session.Player.Recipes.Keys select RecipeManager.Instance.GetRecipe(recipeID);
                    var availableRecipes = RecipeManager.Instance.GetRecipe(RecipeType.Jewelcraft).Except(knownRecipes);
                    matchingRecipes += availableRecipes.Count();
                    if (availableRecipes.Count() > 0)
                    {
                        sb.AppendLine($"%BYT%|| Available Jewelcraft Recipes%PT%");
                        foreach (var recipe in availableRecipes)
                        {
                            sb.AppendLine($"%BYT%|| {recipe.Name} - {Helpers.GetPurchasePrice(session, recipe.LearnCost):N0} gold%PT%");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| No Jewelcraft recipes available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                if (r.Flags.HasFlag(RoomFlags.Scribe))
                {
                    var knownRecipes = from recipeID in session.Player.Recipes.Keys select RecipeManager.Instance.GetRecipe(recipeID);
                    var availableRecipes = RecipeManager.Instance.GetRecipe(RecipeType.Scribe).Except(knownRecipes);
                    matchingRecipes += availableRecipes.Count();
                    if (availableRecipes.Count() > 0)
                    {
                        sb.AppendLine($"%BYT%|| Available Scribe Recipes%PT%");
                        foreach (var recipe in availableRecipes)
                        {
                            sb.AppendLine($"%BYT%|| {recipe.Name} - {Helpers.GetPurchasePrice(session, recipe.LearnCost):N0} gold%PT%");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%|| No Scribe recipes available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                if (r.Flags.HasFlag(RoomFlags.LanguageTrainer))
                {
                    bool newLangs = false;
                    foreach(var lang in Constants.Languages)
                    {
                        if (!session.Player.KnowsLanguage(lang))
                        {
                            sb.AppendLine($"%BYT%|| {lang} - {Helpers.GetPurchasePrice(session, 5000):N0} gold%PT%");
                            newLangs = true;
                        }
                    }
                    if (!newLangs)
                    {
                        sb.AppendLine($"%BTYT%|| No Languages available%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                }
                sb.AppendLine($"%BRT%Usage: learn <skill | recipe | language> <name>%PT%");
                session.Send(sb.ToString());
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: learn <skill | recipe | language> <name>%PT%{Constants.NewLine}");
                return;
            }
            var skName = arg.Remove(0, args[0].Length).Trim();
            if (string.IsNullOrEmpty(skName))
            {
                session.Send($"%BRT%Usage: learn <skill | recipe | language> <name>%PT%{Constants.NewLine}");
                return;
            }
            if (args[0].Trim().ToLower() == "skill")
            {
                var skill = SkillManager.Instance.GetSkill(skName);
                if (skill == null)
                {
                    session.Send($"%BRT%No such Skill exists in the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                if (session.Player.HasSkill(skill.Name))
                {
                    session.Send($"%BRT%You already know that skill!%PT%{Constants.NewLine}");
                    return;
                }
                var pPrice = Helpers.GetPurchasePrice(session, skill.LearnCost);
                if (session.Player.Gold < (ulong)pPrice)
                {
                    session.Send($"%BRT%You don't have enough gold to learn that Skill!%PT%{Constants.NewLine}");
                    return;
                }
                if (skill.Name != "Extra Attack")
                {
                    session.Player.AddSkill(skill.Name);
                }
                else
                {
                    if ((session.Player.Class == ActorClass.Fighter && session.Player.NumberOfAttacks == 5) || (session.Player.Class != ActorClass.Fighter && session.Player.NumberOfAttacks == 2))
                    {
                        session.Send($"%BRT%You have reached the maximum number of times you can learn that skill!%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.NumberOfAttacks++;
                    session.Player.AdjustGold(pPrice * -1, true, false);
                    session.Send($"%BGT%You hand over {pPrice:N0} gold and feel your combat prowess increase!%PT%{Constants.NewLine}");
                }
                session.Player.AdjustGold(pPrice * -1, true, false);
                session.Send($"%BGT%You hand over {pPrice:N0} gold and are blessed with knowledge granting you the {skill.Name} Skill!%PT%{Constants.NewLine}");
                return;
            }
            if (args[0].Trim().ToLower() == "recipe")
            {
                var recipe = RecipeManager.Instance.GetRecipe(skName).FirstOrDefault();
                if (recipe == null)
                {
                    session.Send($"%BRT%No such Recipe exists within the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                var pPrice = Helpers.GetPurchasePrice(session, recipe.LearnCost);
                if (session.Player.Gold < (ulong)pPrice)
                {
                    session.Send($"%BRT%You don't have enough gold to learn that Recipe!%PT%{Constants.NewLine}");
                    return;
                }
                if (session.Player.KnowsRecipe(recipe.ID))
                {
                    session.Send($"%BRT%You already know that recipe!%PT%{Constants.NewLine}");
                    return;
                }
                switch(recipe.RecipeType)
                {
                    case RecipeType.Alchemy:
                        if (!session.Player.HasSkill("Alchemist"))
                        {
                            session.Send($"%BRT%You need a basic knowledge of Alchemy to learn that Recipe!%PT%{Constants.NewLine}");
                            return;
                        }
                        break;

                    case RecipeType.Blacksmith:
                        if (!session.Player.HasSkill("Blacksmithing"))
                        {
                            session.Send($"%BRT%You need a basic knowledge of Blacksmithing to learn that Recipe!%PT%{Constants.NewLine}");
                            return;
                        }
                        break;

                    case RecipeType.Cooking:
                        if (!session.Player.HasSkill("Cooking"))
                        {
                            session.Send($"%BRT%You need a basic knowledge of Cooking to learn that Recipe!%PT%{Constants.NewLine}");
                            return;
                        }
                        break;

                    case RecipeType.Jewelcraft:
                        if (!session.Player.HasSkill("Jewelcrafting"))
                        {
                            session.Send($"%BRT%You need a basic knowledge of Jewelcrafting to learn that Recipe!%PT%{Constants.NewLine}");
                            return;
                        }
                        break;

                    case RecipeType.Scribe:
                        if (!session.Player.HasSkill("Scribe"))
                        {
                            session.Send($"%BRT%You need a basic knowledge of Scribing to learn that Recipe!%PT%{Constants.NewLine}");
                            return;
                        }
                        break;
                }
                session.Player.AddRecipe(recipe.ID);
                session.Player.AdjustGold(pPrice * -1, true, false);
                session.Send($"%BGT%You hand over {pPrice:N0} gold and are granted the knowledge of the {recipe.Name} Recipe!%PT%{Constants.NewLine}");
                return;
            }
            if (args[0].Trim().ToLower() == "language")
            {
                var lang = Constants.Languages.FirstOrDefault(x => x.IndexOf(skName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (string.IsNullOrEmpty(lang))
                {
                    session.Send($"%BRT%No such language exists within the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                if (session.Player.KnowsLanguage(lang))
                {
                    session.Send($"%BRT%You already know the {lang} language!%PT%{Constants.NewLine}");
                    return;
                }
                var pPrice = Helpers.GetPurchasePrice(session, 5000);
                if (session.Player.Gold < (ulong)pPrice)
                {
                    session.Send($"%BRT%You don't have enough gold to learn that language!%PT%{Constants.NewLine}");
                    return;
                }
                session.Player.AddLanguage(lang);
                session.Player.AdjustGold(pPrice * -1, true, false);
                session.Send($"%BGT%You hand over {pPrice:N0} gold and are filled with knowledge of the {lang} language!%PT%{Constants.NewLine}");
                return;
            }
            session.Send($"%BRT%Usage: learn <skill | recipe | language> <name>%PT%{Constants.NewLine}");
        }

        public static void TrainStat(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).HasTrainer || !RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.StatTrainer))
            {
                session.Send($"%BRT%There is no one here to teach you!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                sb.AppendLine($"%BYT%|| The trainer looks you up and down. \"Self-improvement is an investment...\"%PT%");
                sb.AppendLine($"%BYT%|| Strength:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, (session.Player.Strength + 1) * 1000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Dexterity:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, (session.Player.Dexterity + 1) * 1000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Constitution:{Constants.TabStop}{Helpers.GetPurchasePrice(session, (session.Player.Constitution + 1) * 1000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Intelligence:{Constants.TabStop}{Helpers.GetPurchasePrice(session, (session.Player.Intelligence + 1) * 1000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Wisdom:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, (session.Player.Wisdom + 1) * 1000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Charisma:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, (session.Player.Charisma + 1) * 1000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Max HP:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, 10000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Max MP:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, 10000):N0} gold%PT%");
                sb.AppendLine($"%BYT%|| Max SP:{Constants.TabStop}{Constants.TabStop}{Helpers.GetPurchasePrice(session, 10000):N0} gold%PT%");
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                session.Send(sb.ToString());
                return;
            }
            switch(arg.ToLower())
            {
                case "strength":
                case "str":
                    if (session.Player.Strength == 25)
                    {
                        session.Send($"%BRT%\"I've improved your Strength as much as I can,\" the trainer says.%PT%{Constants.NewLine}");
                        return;
                    }
                    int pPrice = Helpers.GetPurchasePrice(session, (session.Player.Strength + 1) * 1000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.Strength++;
                    session.Send($"%BGT%Your Strength has improved!%PT%{Constants.NewLine}");
                    break;

                case "dexterity":
                case "dex":
                    if (session.Player.Dexterity == 25)
                    {
                        session.Send($"%BRT%\"I've improved your Dexterity as much as I can,\" the trainer says.%PT%{Constants.NewLine}");
                        return;
                    }
                    pPrice = Helpers.GetPurchasePrice(session, (session.Player.Dexterity + 1) * 1000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.Dexterity++;
                    session.Send($"%BGT%Your Dexterity has improved!%PT%{Constants.NewLine}");
                    break;

                case "constitution":
                case "con":
                    if (session.Player.Constitution == 25)
                    {
                        session.Send($"%BRT%\"I've improved your Constitution as much as I can,\" the trainer says.%PT%{Constants.NewLine}");
                        return;
                    }
                    pPrice = Helpers.GetPurchasePrice(session, (session.Player.Constitution + 1) * 1000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.Constitution++;
                    session.Send($"%BGT%Your Constitution has improved!%PT%{Constants.NewLine}");
                    break;

                case "intelligence":
                case "int":
                    if (session.Player.Intelligence == 25)
                    {
                        session.Send($"%BRT%\"I've improved your Intelligence as much as I can,\" the trainer says.%PT%{Constants.NewLine}");
                        return;
                    }
                    pPrice = Helpers.GetPurchasePrice(session, (session.Player.Intelligence + 1) * 1000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.Intelligence++;
                    session.Send($"%BGT%Your Intelligence has improved!%PT%{Constants.NewLine}");
                    break;

                case "wisdom":
                case "wis":
                    if (session.Player.Wisdom == 25)
                    {
                        session.Send($"%BRT%\"I've improved your Wisdom as much as I can,\" the trainer says.%PT%{Constants.NewLine}");
                        return;
                    }
                    pPrice = Helpers.GetPurchasePrice(session, (session.Player.Wisdom + 1) * 1000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.Wisdom++;
                    session.Send($"%BGT%Your Wisdom has improved!%PT%{Constants.NewLine}");
                    break;

                case "charisma":
                case "cha":
                    if (session.Player.Charisma == 25)
                    {
                        session.Send($"%BRT%\"I've improved your Charisma as much as I can,\" the trainer says.%PT%{Constants.NewLine}");
                        return;
                    }
                    pPrice = Helpers.GetPurchasePrice(session, (session.Player.Charisma + 1) * 1000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.Charisma++;
                    session.Send($"%BGT%Your Charisma has improved!%PT%{Constants.NewLine}");
                    break;

                case "maxhp":
                case "hp":
                    pPrice = Helpers.GetPurchasePrice(session, 10000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    int hpBonus = Math.Max(0, Helpers.CalculateAbilityModifier(session.Player.Constitution));
                    switch (session.Player.Class)
                    {
                        case ActorClass.Wizard:
                            hpBonus += Helpers.RollDice<int>(1, 4);
                            break;

                        case ActorClass.Thief:
                            hpBonus += Helpers.RollDice<int>(1, 6);
                            break;

                        case ActorClass.Cleric:
                            hpBonus += Helpers.RollDice<int>(1, 8);
                            break;

                        case ActorClass.Fighter:
                            hpBonus += Helpers.RollDice<int>(1, 10);
                            break;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.AdjustMaxHP(hpBonus);
                    session.Send($"%BGT%You hand {pPrice:N0} gold to the trainer and your HP increases by {hpBonus}!%PT%{Constants.NewLine}");
                    break;

                case "maxmp":
                case "mp":
                    pPrice = Helpers.GetPurchasePrice(session, 10000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    int mpBonus = session.Player.Class == ActorClass.Cleric ? Math.Max(0, Helpers.CalculateAbilityModifier(session.Player.Wisdom)) : Math.Max(0, Helpers.CalculateAbilityModifier(session.Player.Intelligence));
                    switch(session.Player.Class)
                    {
                        case ActorClass.Wizard:
                            mpBonus += Helpers.RollDice<int>(1, 10);
                            break;

                        case ActorClass.Thief:
                            mpBonus += Helpers.RollDice<int>(1, 6);
                            break;

                        case ActorClass.Cleric:
                            mpBonus += Helpers.RollDice<int>(1, 8);
                            break;

                        case ActorClass.Fighter:
                            mpBonus += Helpers.RollDice<int>(1, 4);
                            break;
                    }
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.AdjustMaxMP(mpBonus);
                    session.Send($"%BGT%You hand the trainer {pPrice:N0} gold and your MP increases by {mpBonus}!%PT%{Constants.NewLine}");
                    break;

                case "maxsp":
                case "sp":
                    pPrice = Helpers.GetPurchasePrice(session, 10000);
                    if (session.Player.Gold < (ulong)pPrice)
                    {
                        session.Send($"%BRT%The trainer tuts. \"Looks like you're short on coin, my friend!\"%PT%{Constants.NewLine}");
                        return;
                    }
                    int spBonus = Math.Max(0, Helpers.CalculateAbilityModifier(session.Player.Constitution));
                    spBonus += Helpers.RollDice<int>(1, 10);
                    session.Player.AdjustGold((long)pPrice * -1, true, false);
                    session.Player.AdjustMaxSP(spBonus);
                    session.Send($"%BGT%You hand over {pPrice:N0} gold and your SP increases by {spBonus}!%PT%{Constants.NewLine}");
                    break;

                default:
                    session.Send($"%BRT%\"I don't think I'm the right person to help you with that,\" the trainer states.%PT%{Constants.NewLine}");
                    break;
            }
        }

        public static void StudyMagic(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.MagicTrainer))
            {
                session.Send($"%BRT%There is no one here to teach you!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                StringBuilder sb = new StringBuilder();
                int matchingSpells = 0;
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                var knownSpells = (from spellName in session.Player.Spells.Keys select SpellManager.Instance.GetSpell(spellName)).Where(x => x.AvailableToClass.HasFlag(session.Player.Class));
                var availableSpells = SpellManager.Instance.GetSpell().Except(knownSpells);
                matchingSpells = availableSpells.Count();
                if (matchingSpells > 0)
                {
                    sb.AppendLine($"%BYT%||%PT% Available Spells:");
                    foreach(var spell in availableSpells)
                    {
                        sb.AppendLine($"%BYT%||%PT% Name: {spell.Name}{Constants.TabStop}Type: {spell.SpellType}");
                        sb.AppendLine($"%BYT%||%PT% Description: {spell.Description}");
                        sb.AppendLine($"%BYT%||%PT% MP: {spell.MPCost(session.Player)}");
                        if (!string.IsNullOrEmpty(spell.DamageExpression))
                        {
                            sb.AppendLine($"%BYT%||%PT% Damage: {spell.DamageExpression}");
                        }
                        sb.AppendLine($"%BYT%||%PT% Cost: {Helpers.GetPurchasePrice(session, spell.LearnCost):N0} gold");
                    }
                }
                else
                {
                    sb.AppendLine($"%BYT%||%PT% No Spells available");
                }
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                session.Send(sb.ToString());
                return;
            }
            else
            {
                var s = SpellManager.Instance.GetSpell(arg);
                if (s == null)
                {
                    session.Send($"%BRT%No such Spell exists within the Realms!%PT%{Constants.NewLine}");
                    return;
                }
                if (!s.AvailableToClass.HasFlag(session.Player.Class))
                {
                    session.Send($"%BRT%You lack the spiritual connection required to learn that Spell!%PT%{Constants.NewLine}");
                    return;
                }
                if (session.Player.KnowsSpell(s.Name))
                {
                    session.Send($"%BRT%You already know that Spell!%PT%{Constants.NewLine}");
                    return;
                }
                var purchasePrice = Helpers.GetPurchasePrice(session, s.LearnCost);
                if (session.Player.Gold < (ulong)purchasePrice)
                {
                    session.Send($"%BRT%You don't have enough gold to learn that Spell!%PT%{Constants.NewLine}");
                    return;
                }
                session.Player.AdjustGold((long)purchasePrice * -1, true, false);
                session.Player.AddSpell(s.ID);
                session.Send($"%BGT%The Winds of Magic fill you with knowledge of the {s.Name} Spell!%PT%{Constants.NewLine}");
            }
        }

        public static void DeleteCharacter(Session session)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine("%BRT%This is a permanent change!%PT%");
                sb.AppendLine("%BRT%If you delete your character you will be disconnected and will need%PT%");
                sb.AppendLine("%BRT%to create a new character to play again.%PT%");
                sb.AppendLine("%BYT%Enter%PT%%BRT% ***DELETE***%BYT% to confirm or %BGT%***END***%BYT% to abort:%PT%");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }
                if (input.Trim() == "***END***")
                {
                    return;
                }
                if (input.Trim() == "***DELETE***")
                {
                    if (DatabaseManager.DeleteCharacter(session.Player.Name))
                    {
                        Game.LogMessage($"INFO: Player {session.Player.Name} has deleted their character from the database", LogLevel.Info);
                        session.Send($"%BRT%Character deleted successfully. Goodbye!%PT%{Constants.NewLine}");
                        SessionManager.Instance.Close(session);
                    }
                    else
                    {
                        session.Send($"%BRT%Failed to delete character. Please contact an Immortal.%PT%{Constants.NewLine}");
                        Game.LogMessage($"ERROR: Player {session.Player.Name} failed to delete their character", LogLevel.Error);
                        return;
                    }
                }
                continue;
            }
        }

        public static void PlayDice(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Gambler))
            {
                session.Send($"%BRT%There is no gambler here!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg) || arg.Trim().ToLower() == "help")
            {
                session.Send($"%BRT%Usage:%PT%{Constants.NewLine}");
                session.Send($"%BRT%<bet | gamble | dice> <rules | help | amount>%PT%{Constants.NewLine}");
                session.Send($"%BRT%<bet | gabmle | dice> rules - show the game rules%PT%{Constants.NewLine}");
                session.Send($"%BRT%<bet | gamble | dice> help - show this message%PT%{Constants.NewLine}");
                session.Send($"%BRT%<bet | gamble | dice> 1000 - play the game with a bet of 1000 gold%PT%{Constants.NewLine}");
                session.Send($"%BRT%You need to say how much you want to bet!%PT%{Constants.NewLine}");
                return;
            }
            if (arg.Trim().ToLower() == "rules")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"%BYT%\"You want to know how its played?\" the dicer asks. \"Well then, pay attention, {session.Player.Name}.\"%PT%");
                sb.AppendLine($"%BYT%The dicer picks up three six-sided dice. \"You roll these. If two dice match, the third one is your score.\"%PT%");
                sb.AppendLine($"%BYT%\"Then,\" he continues, \"I roll them. Same rules. Whoever scores highest wins. If you win, you get your stake%PT%");
                sb.AppendLine($"%BYT%back, plus half. If I win, I keep your money. If neither of us score, you get your stake back. There are some%PT%");
                sb.AppendLine($"%BYT%special cases though,\" he says, picking up the dice again.%PT%");
                sb.AppendLine($"%BYT%\"If you roll three fours, fives or sixes, you get your stake plus triple. If you roll four-five-six, you get%PT%");
                sb.AppendLine($"%BYT%your stake plus double. Its not all good news, though,\" he says, turning the dice to show one-two-three.%PT%");
                sb.AppendLine($"%BYT%\"If you roll this, I keep your stake and take double the amount again. If you're really unlucky and you%PT%");
                sb.AppendLine($"%BYT%roll three ones, twos or threes, I keep your stake and take triple. Same rules for me, though, so its all");
                sb.AppendLine($"%BYT%down to luck. Got it? Good! Lets roll some dice!\"%PT%");
                session.Send(sb.ToString());
                return;
            }
            if (!long.TryParse(arg.Trim(), out long betAmount) || betAmount < 0)
            {
                session.Send($"%BRT%That isn't a valid amount to bet.%PT%{Constants.NewLine}");
                return;
            }
            if ((ulong)betAmount > session.Player.Gold)
            {
                session.Send($"%BRT%You don't have that much gold!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.AdjustGold(betAmount * -1, true, false);
            bool playerValidScore, dicerValidScore;
            int playerScore, dicerScore;
            long winAmount, lossAmount;
            int[] playerRolls = new int[]
            {
                session.Player.HasSkill("Gambling") ? Math.Max(Helpers.RollDice<int>(1, 6), Helpers.RollDice<int>(1, 6)) : Helpers.RollDice<int>(1, 6),
                session.Player.HasSkill("Gambling") ? Math.Max(Helpers.RollDice<int>(1, 6), Helpers.RollDice<int>(1, 6)) : Helpers.RollDice<int>(1, 6),
                session.Player.HasSkill("Gambling") ? Math.Max(Helpers.RollDice<int>(1, 6), Helpers.RollDice<int>(1, 6)) : Helpers.RollDice<int>(1, 6)
            };
            int[] dicerRolls = new int[]
            {
                Helpers.RollDice<int>(1, 6),
                Helpers.RollDice<int>(1, 6),
                Helpers.RollDice<int>(1, 6)
            };
            if (Helpers.AutoWinsDiceGame(playerRolls, out bool playerCritWin))
            {
                if (playerCritWin)
                {
                    winAmount = betAmount + (betAmount * 3);
                    session.Send($"%BYT%The dicer laughs, \"You rolled {string.Join(", ", playerRolls)} which is a critical win!\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%The dicer hands you {winAmount:N0} gold coins!%PT%{Constants.NewLine}");
                    session.Player.AdjustGold(winAmount, true, false);
                    return;
                }
                winAmount = betAmount + (betAmount * 2);
                session.Send($"%BYT%The dicer grins, \"Luck is with you today! You rolled {string.Join(", ", playerRolls)} which is an instant win!%PT%{Constants.NewLine}");
                session.Send($"%BYT%The dicer hands you {winAmount:N0} gold coins!%PT%{Constants.NewLine}");
                session.Player.AdjustGold(winAmount, true, false);
                return;
            }
            if (Helpers.AutoLosesDiceGame(playerRolls, out bool playerCritLoss))
            {
                if (playerCritLoss)
                {
                    lossAmount = Math.Min((long)session.Player.Gold, betAmount * 3);
                    session.Send($"%BYT%The dicer tuts. \"Not your day, is it? You rolled {string.Join(", ", playerRolls)} which is a critical loss!\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%The dicer takes {lossAmount:N0} gold from you!%PT%{Constants.NewLine}");
                    session.Player.AdjustGold(lossAmount * -1, true, false);
                    return;
                }
                lossAmount = Math.Min((long)session.Player.Gold, betAmount * 2);
                session.Send($"%BYT%The dicer laughs, \"It's my lucky day! You rolled {string.Join(", ", playerRolls)} which is an instant loss!\"%PT%{Constants.NewLine}");
                session.Send($"%BYT%The dicer takes {lossAmount:N0} gold from you!%PT%{Constants.NewLine}");
                session.Player.AdjustGold(lossAmount * -1, true, false);
                return;
            }
            if (Helpers.AutoWinsDiceGame(dicerRolls, out bool dicerCritWin))
            {
                if (dicerCritWin)
                {
                    lossAmount = Math.Min((long)session.Player.Gold, betAmount * 3);
                    session.Send($"%BYT%\"Oh yes!\" the dicer laughs. \"I rolled {string.Join(", ", dicerRolls)} which is a critical win!\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%The dicer take {lossAmount:N0} gold from you!%PT%{Constants.NewLine}");
                    session.Player.AdjustGold(lossAmount * -1, true, false);
                    return;
                }
                lossAmount = Math.Min((long)session.Player.Gold, betAmount * 2);
                session.Send($"%BYT%\"My lucky day!\" the dicer laughs. \"I rolled {string.Join(", ", dicerRolls)} which is an instant win!\"%PT%{Constants.NewLine}");
                session.Send($"%BYT%The dicer takes {lossAmount:N0} gold from you!%PT%{Constants.NewLine}");
                session.Player.AdjustGold(lossAmount * -1, true, false);
                return;
            }
            if (Helpers.AutoLosesDiceGame(dicerRolls, out bool dicerCritLoss))
            {
                if (dicerCritLoss)
                {
                    winAmount = betAmount + (betAmount * 3);
                    session.Send($"%BYT%\"Ah. Luck is not with me today!\" the dicer says. \"I rolled {string.Join(", ", dicerRolls)} which is a critical loss!\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%\"My bad luck means you win {winAmount:N0} gold!\"%PT%{Constants.NewLine}");
                    session.Player.AdjustGold(winAmount, true, false);
                    return;
                }
                winAmount = betAmount + (betAmount * 2);
                session.Send($"%BYT%\"Blast these dice!\" the dicer says. \"I rolled {string.Join(", ", dicerRolls)} which is an instant loss!\"%PT%{Constants.NewLine}");
                session.Send($"%BYT%\"You win {winAmount:N0} gold, enjoy!\"%PT%{Constants.NewLine}");
                session.Player.AdjustGold(winAmount, true, false);
                return;
            }
            playerValidScore = Helpers.ValidDiceScore(playerRolls, out playerScore);
            dicerValidScore = Helpers.ValidDiceScore(dicerRolls, out dicerScore);
            if (playerValidScore && dicerValidScore)
            {
                if (playerScore > dicerScore)
                {
                    winAmount = betAmount + (betAmount / 2);
                    session.Send($"%BYT%The dicer looks at the dice. \"You rolled {string.Join(", ", playerRolls)}, so you score {playerScore}.\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%\"I rolled {string.Join(", ", dicerRolls)}, so I score {dicerScore}, whcih means you win!\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%\"My luck had to run out eventually!\" he says, handing you {winAmount:N0} gold.%PT%{Constants.NewLine}");
                    session.Player.AdjustGold(winAmount, true, false);
                    return;
                }
                if (playerScore < dicerScore)
                {
                    session.Send($"%BYT%The dicer looks at the dice. \"You rolled {string.Join(", ", playerRolls)}, so you score {playerScore}.\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%\"I rolled {string.Join(", ", dicerRolls)}, so I score {dicerScore}, which means I win and keep your stake.\"%PT%{Constants.NewLine}");
                    session.Send($"%BYT%The dicer smiles. \"Better luck next time!\"%PT%{Constants.NewLine}");
                    return;
                }
                // player and dicer scored the same = draw
                session.Send($"%BYT%The dicer looks at the dice. \"Well, here's something! We both scored {playerScore}, so its a draw!\"%PT%{Constants.NewLine}");
                session.Send($"%BYT%The dicer hands you {betAmount:N0} gold. \"Here, you can keep your stake.\"%PT%{Constants.NewLine}");
                session.Player.AdjustGold(betAmount, true, false);
                return;
            }
            if (playerValidScore && !dicerValidScore)
            {
                winAmount = betAmount + (betAmount / 2);
                session.Send($"%BYT%The dicer laughs as he looks at the dice. \"You rolled {string.Join(", ", playerRolls)}, so score {playerScore}, but, alas,%PT%{Constants.NewLine}");
                session.Send($"%BYT%I rolled {string.Join(", ", dicerRolls)}, and score nothing. Here: you win {winAmount:N0} gold, enjoy!\"%PT%{Constants.NewLine}");
                session.Player.AdjustGold(winAmount, true, false);
                return;
            }
            if (!playerValidScore && dicerValidScore)
            {
                session.Send($"%BYT%\"Well now,\" the dicer says. \"You rolled {string.Join(", ", playerRolls)} which doesn't score anything!\"%PT%{Constants.NewLine}");
                session.Send($"%BYT%\"I rolled {string.Join(", ", dicerRolls)}, which scores me {dicerScore}, so I win and keep your stake!\"%PT%{Constants.NewLine}");
                return;
            }
            // neither player or dicer had a valid score = draw
            session.Send($"%BYT%The dicer sneers at the dice. \"You rolled {string.Join(", ", playerRolls)}, and don't score anything.\"%PT%{Constants.NewLine}");
            session.Send($"%BYT%\"I rolled {string.Join(", ", dicerRolls)}, which also scores nothing, so this round is draw.\"%PT%{Constants.NewLine}");
            session.Send($"%BYT%\"Here,\" the dicer says, handing you {betAmount:N0} gold coins. \"Returning your stake. Lets play again some time.\"%PT%{Constants.NewLine}");
            session.Player.AdjustGold(betAmount, true, false);
            return;
        }

        public static void ManagePlayerAliases(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                if (session.Player.CommandAliases != null && session.Player.CommandAliases.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                    sb.AppendLine($"%BYT%|| Alias{Constants.TabStop}{Constants.TabStop}Command%PT%");
                    sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                    foreach(var alias in session.Player.CommandAliases)
                    {
                        sb.AppendLine($"%BYT%||%PT%%BGT%{alias.Key}{Constants.TabStop}{Constants.TabStop}{alias.Value}%PT%");
                    }
                    sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                    session.Send(sb.ToString());
                }
                else
                {
                    session.Send($"%BRT%You don't have any aliases defined.%PT%{Constants.NewLine}");
                }
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: alias - list your current aliases%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: alias create <alias> <command> - create a new alias%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: alias remove <alias> - delete an alias%PT%{Constants.NewLine}");
                return;
            }
            var operation = args[0].ToLower().Trim();
            var cmdAlias = args[1].ToLower().Trim();
            var command = arg.Remove(0, operation.Length).Trim().Remove(0, cmdAlias.Length).Trim();
            if (operation == "remove" || operation == "delete")
            {
                if (session.Player.CommandAliases.TryRemove(cmdAlias, out _))
                {
                    session.Send($"%BGT%The Alias was successfully removed.%PT%{Constants.NewLine}");
                }
                else
                {
                    session.Send($"%BRT%The Alias could not be removed.%PT%{Constants.NewLine}");
                }
                return;
            }
            if (string.IsNullOrEmpty(command))
            {
                session.Send($"%BRT%Usage: alias - list your current aliases%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: alias create <alias> <command> - create a new alias%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: alias remove <alias> - delete an alias%PT%{Constants.NewLine}");
                return;
            }
            if (operation == "add" || operation == "create")
            {
                session.Player.CommandAliases.AddOrUpdate(cmdAlias, command, (k, v) => command);
                session.Send($"%BGT%The Alias was created or updated successfully.%PT%{Constants.NewLine}");
                return;
            }
            session.Send($"%BRT%Usage: alias - list your current aliases%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: alias create <alias> <command> - create a new alias%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: alias remove <alias> - delete an alias%PT%{Constants.NewLine}");
            return;
        }

        public static void SaveCharacter(Session session)
        {
            if (DatabaseManager.SavePlayer(session, false))
            {
                session.Send($"%BGT%Character successfully saved.%PT%{Constants.NewLine}");
            }
            else
            {
                session.Send($"%BGT%Failed to save character, please let an Immortal know!%PT%{Constants.NewLine}");
            }
        }

        public static void QuitGame(Session session)
        {
            session.Send($"Good bye! We hope to see you again soon!{Constants.NewLine}");
            SessionManager.Instance.Close(session);
        }

        public static void ChangePrompt(Session session)
        {
            session.Player.PromptStyle = session.Player.PromptStyle == PlayerPrompt.Normal ? PlayerPrompt.Percentage : PlayerPrompt.Normal;
        }

        public static void ShowCharSheet(Session session)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            sb.AppendLine($"%BYT%||%PT% Name: {session.Player.Name}{Constants.TabStop}Level: {session.Player.Level}{Constants.TabStop}Race: {session.Player.Race}");
            sb.AppendLine($"%BYT%||%PT% Alignment: {session.Player.Alignment}{Constants.TabStop}Class: {session.Player.Class}");
            sb.AppendLine($"%BYT%||%PT% STR: {session.Player.Strength} ({Helpers.CalculateAbilityModifier(session.Player.Strength)}){Constants.TabStop}DEX: {session.Player.Dexterity} ({Helpers.CalculateAbilityModifier(session.Player.Dexterity)}){Constants.TabStop}CON: {session.Player.Constitution} ({Helpers.CalculateAbilityModifier(session.Player.Constitution)})");
            sb.AppendLine($"%BYT%||%PT% INT: {session.Player.Intelligence} ({Helpers.CalculateAbilityModifier(session.Player.Intelligence)}){Constants.TabStop}WIS: {session.Player.Wisdom} ({Helpers.CalculateAbilityModifier(session.Player.Wisdom)}){Constants.TabStop}CHA: {session.Player.Charisma} ({Helpers.CalculateAbilityModifier(session.Player.Charisma)})");
            sb.AppendLine($"%BYT%||%PT% HP: %BRT%{session.Player.CurrentHP:N0}%PT% / %BRT%{session.Player.MaxHP:N0}%PT%{Constants.TabStop}MP: %BGT%{session.Player.CurrentMP:N0}%PT% / %BGT%{session.Player.MaxMP:N0}%PT%{Constants.TabStop}SP: %BYT%{session.Player.CurrentSP:N0}%PT% / %BYT%{session.Player.MaxSP:N0}%PT%");
            sb.AppendLine($"%BYT%||%PT% Armour Class: {session.Player.ArmourClass}{Constants.TabStop}Attacks: {session.Player.NumberOfAttacks}");
            sb.AppendLine($"%BYT%||%PT% Languages: {string.Join(", ", session.Player.KnownLanguages.Keys)}");
            sb.AppendLine($"%BYT%||%PT% Exp: {session.Player.Exp:N0}{Constants.TabStop}Next: {LevelTable.ExpForNextLevel(session.Player.Level, session.Player.Exp)}");
            sb.AppendLine($"%BYT%||%PT% Gold: %YT%{session.Player.Gold:N0}%PT%");
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        private static void ShowCompletedQuests(Session session)
        {
            StringBuilder sb = new StringBuilder();
            if (session.Player.CompletedQuests.Count == 0)
            {
                session.Send($"BRT%You haven't completed any Quests yet!%PT%{Constants.NewLine}");
                return;
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            sb.AppendLine($"%BYT%|| You have completed the following Quetss:%PT%");
            foreach (var kvp in session.Player.CompletedQuests)
            {
                var quest = QuestManager.Instance.GetQuest(kvp.Key);
                if (quest != null)
                {
                    sb.AppendLine($"%BYT%|| {quest.Name} in {ZoneManager.Instance.GetZone(quest.Zone).ZoneName}%PT%");
                }
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
            return;
        }

        private static void ShowActiveQuests(Session session)
        {
            StringBuilder sb = new StringBuilder();
            if (session.Player.ActiveQuests.Count == 0)
            {
                session.Send($"%BGT%You don't have any Quests right now.%PT%{Constants.NewLine}");
            }
            else
            {
                int i = 0;
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                foreach (var qGuid in session.Player.ActiveQuests.Keys)
                {
                    i++;
                    var q = QuestManager.Instance.GetQuest(qGuid);
                    sb.AppendLine($"%BYT%|| ID: {i}{Constants.TabStop}{Constants.TabStop}Zone: {ZoneManager.Instance.GetZone(q.Zone).ZoneName}%PT%");
                    sb.AppendLine($"%BYT%|| Name: {q.Name}%PT%");
                    if (q.RequiredMonsters != null && q.RequiredMonsters.Count > 0)
                    {
                        sb.AppendLine($"%BYT%|| Required Monsters:");
                        foreach (var mid in q.RequiredMonsters)
                        {
                            var monster = NPCManager.Instance.GetNPC(mid.Key);
                            string monsterName = monster != null ? monster.Name : "Unknown Monster";
                            sb.AppendLine($"%BYT%||{Constants.TabStop}{mid.Value} x {monsterName}");
                        }
                    }
                    if (q.RequiredItems != null && q.RequiredItems.Count > 0)
                    {
                        sb.AppendLine($"%BYT%|| Required Items:");
                        foreach (var iid in q.RequiredItems)
                        {
                            InventoryItem item = ItemManager.Instance.GetItem(iid.Key);
                            string itemName = item != null ? item.Name : "Unknown Item";
                            sb.AppendLine($"%BYT%||{Constants.TabStop}{iid.Value} x {itemName}");
                        }
                    }
                    if (q.RewardItems != null && q.RewardItems.Count > 0)
                    {
                        sb.AppendLine($"%BYT%|| Reward Items:");
                        foreach (var iid in q.RewardItems)
                        {
                            InventoryItem item = ItemManager.Instance.GetItem(iid.Key);
                            string itemName = item != null ? item.Name : "Unknown Item";
                            sb.AppendLine($"%BYT%||{Constants.TabStop}{iid.Value} x {itemName}");
                        }
                    }
                    sb.AppendLine($"%BYT%|| Reward Gold: {q.RewardGold:N0}{Constants.TabStop}Exp: {q.RewardExp:N0}");
                    sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                }
                sb.AppendLine($"%BYT%|| {i} Active Quest(s)%PT%");
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                session.Send(sb.ToString());
            }
            return;
        }

        private static void AbandonActiveQuest(Session session, ref string arg)
        {
            var args = arg.Split(' ');
            StringBuilder sb = new StringBuilder();
            if (args.Length < 2 || string.IsNullOrEmpty(args[1]) || !int.TryParse(args[1], out int id))
            {
                session.Send($"%BRT%Usage: quest abandon <id>%PT%{Constants.NewLine}");
                return;
            }
            int qid = id - 1;
            if (qid < 0 || qid > session.Player.ActiveQuests.Count)
            {
                session.Send($"%BRT%That ID isn't valid!%PT%{Constants.NewLine}");
                return;
            }
            var questToAbandon = QuestManager.Instance.GetQuest(session.Player.ActiveQuests.Keys.ToList()[qid]);
            if (questToAbandon == null)
            {
                session.Send($"%BRT%Couldn't find a Quest with that ID!%PT%{Constants.NewLine}");
                return;
            }
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"%BRT%Abandon Quest: {questToAbandon.Name}");
                sb.AppendLine($"1. Yes{Constants.TabStop}{Constants.TabStop}2. No");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var abChoice = session.Read();
                if (string.IsNullOrEmpty(abChoice) || !int.TryParse(abChoice, out int option))
                {
                    continue;
                }
                switch (option)
                {
                    case 1:
                        if (session.Player.ActiveQuests.TryRemove(questToAbandon.QuestGUID, out _))
                        {
                            session.Send($"%BGT%You have abandoned the Quest!%PT%{Constants.NewLine}");
                            return;
                        }
                        session.Send($"%BRT%Failed to abandon the Quest, please see an Imm!%PT%{Constants.NewLine}");
                        return;

                    case 2:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option.%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ListAvailableQuests(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
            {
                session.Send($"%BRT%There is no Quest Master here!%PT%{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            var args = arg.Split(' ');
            sb.Clear();
            int qid = -1;
            if (args.Length == 2 && !int.TryParse(args[1].Trim(), out qid))
            {
                session.Send($"%BRT%Usage: quest list - list all available quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest list <id> - show more info on a specific quest%PT%{Constants.NewLine}");
                return;
            }
            var currentZone = ZoneManager.Instance.GetZoneForRID(session.Player.CurrentRoom).ZoneID;
            var availableQuests = QuestManager.Instance.GetQuestsForZone(currentZone)
                .Where(x => !session.Player.CompletedQuests.ContainsKey(x.QuestGUID) && !session.Player.ActiveQuests.ContainsKey(x.QuestGUID)).ToList();
            if (availableQuests == null || availableQuests.Count == 0)
            {
                session.Send($"%BGT%There are no Quests available at the moment!%PT%{Constants.NewLine}");
                return;
            }
            if (args.Length == 2 && qid < 0 || qid > availableQuests.Count)
            {
                session.Send($"%BRT%That isn't a valid ID!%PT%{Constants.NewLine}");
                return;
            }
            if (args.Length == 2)
            {
                qid = Math.Max(0, (--qid));
                var selectedQuest = availableQuests[qid];
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                sb.AppendLine($"%BYT%|| Name: {selectedQuest.Name}{Constants.TabStop}Zone: {ZoneManager.Instance.GetZone(selectedQuest.Zone).ZoneName}%PT%");
                sb.AppendLine($"%BYT%|| Type: {selectedQuest.QuestType}{Constants.TabStop}Exp: {selectedQuest.RewardExp}{Constants.TabStop}Gold: {selectedQuest.RewardGold:N0}%PT%");
                if (selectedQuest.RequiredItems != null && selectedQuest.RequiredItems.Count > 0)
                {
                    sb.AppendLine($"%BYT%|| Reward Items:");
                    foreach (var kvp in selectedQuest.RewardItems)
                    {
                        InventoryItem item = ItemManager.Instance.GetItem(kvp.Key);
                        var itemName = item != null ? item.Name : "Unknown Item";
                        sb.AppendLine($"%BYT%||   {kvp.Value} x {itemName}");
                    }
                }
                sb.AppendLine($"%BYT%|| Quest Info:%PT%");
                foreach (var l in selectedQuest.FlavourText.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                {
                    sb.AppendLine($"%BYT%||  {l}%PT%");
                }
                if (selectedQuest.RequiredMonsters != null && selectedQuest.RequiredMonsters.Count > 0)
                {
                    sb.AppendLine($"%BYT%|| Required Monsters:");
                    foreach (var kvp in selectedQuest.RequiredMonsters)
                    {
                        var monster = NPCManager.Instance.GetNPC(kvp.Key);
                        var monsterName = monster != null ? monster.Name : "Unknown Monster";
                        sb.AppendLine($"%BYT%||    {kvp.Value} x {monsterName}");
                    }
                }
                if (selectedQuest.RequiredItems != null && selectedQuest.RequiredItems.Count > 0)
                {
                    sb.AppendLine($"%BYT%|| Required Items:");
                    foreach (var kvp in selectedQuest.RequiredItems)
                    {
                        InventoryItem item = ItemManager.Instance.GetItem(kvp.Key);
                        var itemName = item != null ? item.Name : "Unknown Item";
                        sb.AppendLine($"%BYT%||    {kvp.Value} x {itemName}");
                    }
                }
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                session.Send(sb.ToString());
                return;
            }
            int qCount = 0;
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            foreach (var quest in availableQuests)
            {
                qCount++;
                sb.AppendLine($"%BYT%|| ID: {qCount}{Constants.TabStop}{Constants.TabStop}Zone: {ZoneManager.Instance.GetZone(quest.Zone).ZoneName}%PT%");
                sb.AppendLine($"%BYT%|| Name: {quest.Name}%PT%");
                sb.AppendLine($"%BYT%|| Type: {quest.QuestType}%PT%");
                sb.AppendLine($"%BYT%|| Quest Info:%PT%");
                sb.AppendLine($"%BYT%|| Exp: {quest.RewardExp}{Constants.TabStop}Gold: {quest.RewardGold:N0}{Constants.TabStop}Items: {(quest.RewardItems.Count > 0 ? "Yes" : "No")}");
                sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
            }
            sb.AppendLine($"%BYT%|| {qCount} Quest(s) available here%PT%");
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
            return;
        }

        private static void AcceptNewQuest(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
            {
                Console.WriteLine($"%BRT%There is no Quest Master here!%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: quest accept <id>%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(args[1].Trim(), out int qid))
            {
                session.Send($"%BRT%That isn't a valid ID!%PT%{Constants.NewLine}");
                return;
            }
            var currentZone = ZoneManager.Instance.GetZoneForRID(session.Player.CurrentRoom).ZoneID;
            var availableQuests = QuestManager.Instance.GetQuestsForZone(currentZone)
                .Where(x => !session.Player.CompletedQuests.ContainsKey(x.QuestGUID) && !session.Player.ActiveQuests.ContainsKey(x.QuestGUID)).ToList();
            if (availableQuests == null || availableQuests.Count == 0)
            {
                session.Send($"%BGT%There are no Quests available right now!%PT%{Constants.NewLine}");
                return;
            }
            qid = Math.Max(0, --qid);
            if (qid > availableQuests.Count)
            {
                session.Send($"%BRT%That isn't a valid ID!%PT%{Constants.NewLine}");
                return;
            }
            var quest = availableQuests[qid];
            if (session.Player.ActiveQuests.TryAdd(quest.QuestGUID, true))
            {
                session.Send($"%BGT%Quest accepted!%PT%{Constants.NewLine}");
            }
            else
            {
                session.Send($"%BRT%Dark forces prevent you from accepting the Quest! See an Imm!%PT%{Constants.NewLine}");
            }
            return;
        }

        private static void ReturnCompletedQuest(Session session, ref string arg)
        {
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
            {
                Console.WriteLine($"%BRT%There is no Quest Master here!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.ActiveQuests.Count == 0)
            {
                session.Send($"%BGT%You don't have any active Quests right now!%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: quest return <id>%PT%{Constants.NewLine}");
                return;
            }
            if (!int.TryParse(args[1].Trim(), out int qid))
            {
                session.Send($"%BRT%That isn't a valid ID!%PT%{Constants.NewLine}");
                return;
            }
            qid = Math.Max(0, --qid);
            if (qid > session.Player.ActiveQuests.Count)
            {
                session.Send($"%BRT%That isn't a valid ID!%PT%{Constants.NewLine}");
                return;
            }
            var quest = QuestManager.Instance.GetQuest(session.Player.ActiveQuests.Keys.ToList()[qid]);
            if (quest == null)
            {
                session.Send($"%BRT%Couldn't find an active Quest with that ID.%PT%{Constants.NewLine}");
                return;
            }
            if (quest.IsComplete(session))
            {
                session.Player.ActiveQuests.TryRemove(quest.QuestGUID, out _);
                session.Player.CompletedQuests.TryAdd(quest.QuestGUID, true);
                session.Player.AdjustExp((int)quest.RewardExp, true, true);
                session.Player.AdjustGold((long)quest.RewardGold, true, true);
                foreach (var kvp in quest.RequiredItems)
                {
                    for (int n = 0; n < kvp.Value; n++)
                    {
                        session.Player.RemoveItemFromInventory(kvp.Key);
                    }
                }
                if (quest.RewardItems != null && quest.RewardItems.Count > 0)
                {
                    foreach (var kvp in quest.RewardItems)
                    {
                        for (int n = 0; n < kvp.Value; n++)
                        {
                            var item = ItemManager.Instance.GetItem(kvp.Key);
                            if (item != null)
                            {
                                session.Player.AddItemToInventory(kvp.Key);
                                session.Send($"%BGT%The Quest Master smiles and hands you {item.ShortDescription}.%PT%{Constants.NewLine}");
                            }
                        }
                    }
                }
            }
            else
            {
                session.Send($"%BYT%\"I'm sorry,\" the Quest Master says, \"you haven't met the requirements of that Quest yet.\"%PT%{Constants.NewLine}");
            }
            return;
        }

        public static void DoQuestOperation(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: quest <show | list | accept | abandon | return>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest show - show your current Quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest list - show available Quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest completed - show completed Quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest accept <id> - accept the specified Quest%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest abandon <id> - abandon the specified Quest%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest return <id> - turn in a completed Quest%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 1)
            {
                session.Send($"%BRT%Usage: quest <show | list | accept | abandon | return>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest show - show your current Quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest list - show available Quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest completed - show completed Quests%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest accept <id> - accept the specified Quest%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest abandon <id> - abandon the specified Quest%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: quest return <id> - turn in a completed Quest%PT%{Constants.NewLine}");
                return;
            }
            var operation = args[0].Trim().ToLower();
            switch (operation)
            {
                case "completed":
                    ShowCompletedQuests(session);
                    break;

                case "show":
                    ShowActiveQuests(session);
                    break;

                case "abandon":
                    AbandonActiveQuest(session, ref arg);
                    break;

                case "list":
                    ListAvailableQuests(session, ref arg);
                    break;

                case "accept":
                    AcceptNewQuest(session, ref arg);
                    break;

                case "return":
                    ReturnCompletedQuest(session, ref arg);
                    break;

                default:
                    session.Send($"%BRT%That is not a valid Quest operation!%PT%{Constants.NewLine}");
                    break;
            }
        }

        public static void ShowSkills(Session session)
        {
            if (session.Player.Skills.Count == 0)
            {
                session.Send($"You don't know any Skills!{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            foreach (var skill in session.Player.Skills)
            {
                var s = SkillManager.Instance.GetSkill(skill.Key);
                if (s != null)
                {
                    sb.AppendLine($"%BYT%||%PT% Name: {s.Name}");
                    sb.AppendLine($"%BYT%||%PT% Description: {s.Description}");
                    sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                }
                else
                {
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} has Skill '{skill.Key}' which is not in Skill Manager", LogLevel.Debug);
                }
            }
            sb.AppendLine($"%BYT%||%PT% You know {session.Player.Skills.Count} Skill(s)");
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void ShowSpells(Session session)
        {
            if (session.Player.Spells.Count == 0)
            {
                session.Send($"You don't know any Spells!{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            foreach (var spell in session.Player.Spells)
            {
                var s = SpellManager.Instance.GetSpell(spell.Key);
                if (s != null)
                {
                    sb.AppendLine($"%BYT%||%PT% Name: {s.Name}{Constants.TabStop}MP: {s.MPCost(session.Player)}");
                    sb.AppendLine($"%BYT%||%PT% Type: {s.SpellType}");
                    sb.AppendLine($"%BYT%||%PT% Description: {s.Description}");
                    sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                }
                else
                {
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} has Spell '{spell.Key}' which is not in Spell Manager", LogLevel.Debug);
                }
            }
            sb.AppendLine($"%BYT%||%PT% You know {session.Player.Spells.Count} Spell(s)");
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void ShowRecipes(Session session)
        {
            if (session.Player.Recipes.Count == 0)
            {
                session.Send($"You don't know any Recipes!{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            foreach(var recipe in session.Player.Recipes)
            {
                var r = RecipeManager.Instance.GetRecipe(recipe.Key);
                if (r != null)
                {
                    var result = (InventoryItem)ItemManager.Instance.GetItem(r.RecipeResult);
                    sb.AppendLine($"%BYT%||%PT% Name: {r.Name}");
                    sb.AppendLine($"%BYT%||%PT% Type: {r.RecipeType}");
                    sb.AppendLine($"%BYT%||%PT% Description: {r.Description}");
                    if (result != null)
                    {
                        sb.AppendLine($"%BYT%||%PT% Produces: {result.Name}");
                    }
                    else
                    {
                        Game.LogMessage($"DEBUG: Player {session.Player.Name} has Recipe '{recipe.Key}' which produces {r.RecipeResult} but no such Item exists in Item Manager", LogLevel.Debug);
                    }
                    sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                }
                else
                {
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} has Recipe '{recipe.Key}' which is not in Recipe Manager", LogLevel.Debug);
                }
            }
            sb.AppendLine($"%BYT%||%PT% You know {session.Player.Recipes.Count} Crafting Recipe(s)");
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void ShowBuffs(Session session)
        {
            if (session.Player.Buffs.Count == 0)
            {
                session.Send($"You don't have any Buffs!{Constants.NewLine}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            foreach (var buff in session.Player.Buffs)
            {
                var b = BuffManager.Instance.GetBuff(buff.Key);
                if (b != null)
                {
                    sb.AppendLine($"%BYT%||%PT% Name: {b.Name}{Constants.TabStop}Duration: {buff.Value}");
                    sb.AppendLine($"%BYT%||%PT% Description: {b.Description}");
                    sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                }
                else
                {
                    Game.LogMessage($"DEBUG: Player {session.Player.Name} has Buff '{buff.Key}' which is not in Buff Manager", LogLevel.Debug);
                }
            }
            sb.AppendLine($"%BYT%||%PT% You have {session.Player.Buffs.Count} Buffs");
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void ShowWho(Session session, string target)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
            var players = string.IsNullOrEmpty(target) ? SessionManager.Instance.ActivePlayers :
                SessionManager.Instance.ActivePlayers.Where(x => x.Player.Name.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (players != null && players.Count > 0)
            {
                sb.AppendLine($"%BYT%||%PT% You sense the following beings in the Realms:");
                foreach (var p in players)
                {
                    if (p.Player.Visible)
                    {
                        if (session.Player.IsImmortal)
                        {
                            sb.AppendLine($"%BYT%||%PT% {p.Player.Title} {p.Player.Name}, the level {p.Player.Level} {p.Player.Class} in Room {p.Player.CurrentRoom}");
                        }
                        else
                        {
                            sb.AppendLine($"%BYT%||%PT% {p.Player.Title} {p.Player.Name}, the {p.Player.Class} in {ZoneManager.Instance.GetZoneForRID(p.Player.CurrentRoom).ZoneName}");
                        }
                    }
                    else
                    {
                        if (session.Player.IsImmortal)
                        {
                            sb.AppendLine($"%BYT%||%PT% {p.Player.Title} {p.Player.Name}, the level {p.Player.Level} {p.Player.Class} in Room {p.Player.CurrentRoom} %BBT%(Invisible)%PT%");
                        }
                        else
                        {
                            if (p.Player.CanBeSeenBy(session.Player))
                            {
                                sb.AppendLine($"%BYT%||%PT% {p.Player.Title} {p.Player.Name}, the {p.Player.Class} in {ZoneManager.Instance.GetZoneForRID(p.Player.CurrentRoom).ZoneName} %BBT%(Invisible)%PT%");
                            }
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine($"%BYT%||%PT% You sense no one in the Realms right now!");
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void Look(Session session, ref string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                RoomManager.Instance.GetRoom(session.Player.CurrentRoom).DescribeRoom(session);
                return;
            }
            if (target.ToLower() == "node")
            {
                var n = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).RSSNode;
                if (n != null)
                {
                    switch(n.Depth)
                    {
                        case 0:
                            session.Send($"%BYT%The {n.Name} looks depleted!%PT%{Constants.NewLine}");
                            return;

                        case 1:
                            session.Send($"%BYT%The {n.Name} looks almost used up!%PT%{Constants.NewLine}");
                            return;

                        case 2:
                            session.Send($"%BYT%The {n.Name} has a bit of value left in it!%PT%{Constants.NewLine}");
                            return;

                        case 3:
                            session.Send($"%BYT%The {n.Name} has plenty of value left in it!%PT%{Constants.NewLine}");
                            return;

                        case 4:
                            session.Send($"%BYT%The {n.Name} looks full of riches!%PT%{Constants.NewLine}");
                            return;

                        default:
                            Game.LogMessage($"ERROR: Node {n.ID} in Room {session.Player.CurrentRoom} returned unsupported Depth property: {n.Depth}", LogLevel.Error);
                            session.Send($"%BRT%Something is wrong with this Node! Tell an Imm!%PT%{Constants.NewLine}");
                            return;
                    }
                }
                session.Send($"%BRT%There is no resource node here!%PT%{Constants.NewLine}");
                return;
            }
            var targetActor = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(target, session.Player);
            if (targetActor != null)
            {
                if (targetActor.CanBeSeenBy(session.Player))
                {
                    session.Send(targetActor.LongDescription);
                    session.Send($"{Constants.NewLine}{targetActor.GetDiagnosis()}{Constants.NewLine}");
                    if (targetActor.ActorType == ActorType.Player)
                    {
                        string msg = session.Player.CanBeSeenBy(targetActor) ? $"{session.Player.Name} gives you a studious look.{Constants.NewLine}" :
                            $"You feel a shiver as something gives you a studious look.{Constants.NewLine}";
                        ((Player)targetActor).Send(msg);
                    }
                    else
                    {
                        var tNPC = (NPC)targetActor;
                        if (tNPC.MobProgs.Count > 0)
                        {
                            foreach(var mp in tNPC.MobProgs.Keys)
                            {
                                var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                                if (mobProg != null)
                                {
                                    mobProg.Init();
                                    mobProg.TriggerEvent(MobProgTrigger.PlayerLook, new { mob = tNPC.ID.ToString(), player = session.ID.ToString() });
                                }
                            }
                        }
                    }
                    var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID && x.ID != targetActor.ID).ToList();
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        foreach (var lp in localPlayers)
                        {
                            string msg = string.Empty;
                            if (targetActor.CanBeSeenBy(lp.Player) && session.Player.CanBeSeenBy(lp.Player))
                            {
                                msg = $"{session.Player.Name} gives {targetActor.Name} a studious look.{Constants.NewLine}";
                            }
                            if (!targetActor.CanBeSeenBy(lp.Player) && session.Player.CanBeSeenBy(lp.Player))
                            {
                                msg = $"{session.Player.Name} gives something a long and studious look.{Constants.NewLine}";
                            }
                            if (!targetActor.CanBeSeenBy(lp.Player) && !session.Player.CanBeSeenBy(lp.Player))
                            {
                                msg = $"The air shifts as something gives something else a studious look.{Constants.NewLine}";
                            }
                            if (targetActor.CanBeSeenBy(lp.Player) && !session.Player.CanBeSeenBy(lp.Player))
                            {
                                msg = $"Something gives {targetActor.Name} a studious look.{Constants.NewLine}";
                            }
                            lp.Send(msg);
                        }
                    }
                }
                else
                {
                    session.Send($"You look about but you can't see anything like that!{Constants.NewLine}");
                }
                return;
            }
            var tItem = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetItem(target);
            if (tItem != null)
            {
                session.Send(tItem.LongDescription);
                if (tItem.IsMagical)
                {
                    session.Send($"%BYT%The item seems somehow mystical!%PT%{Constants.NewLine}");
                }
                if (tItem.IsCursed)
                {
                    session.Send($"%BRT%The item exudes an aura of menace!%PT%{Constants.NewLine}");
                }
                foreach(var lp in RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.Player.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} gives {tItem.ShortDescription} a studious look.%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts about as something gives {tItem.ShortDescription} a studious look.%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
                return;
            }
            string direction = Helpers.GetFullDirectionString(target);
            var rExit = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetRoomExit(direction);
            if (rExit == null)
            {
                session.Send($"You gaze wistfully off into the distance...{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
                if (localPlayers != null && localPlayers.Count > 0)
                {
                    foreach (var lp in localPlayers)
                    {
                        string msg = session.Player.CanBeSeenBy(lp.Player) ? $"{session.Player.Name} gazes wistfully off into the distance...{Constants.NewLine}" :
                            $"Something gazes wistfully off into the distance...{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            var tRoom = RoomManager.Instance.GetRoom(rExit.DestinationRoomID);
            if (tRoom == null)
            {
                session.Send($"That way lies only the void...{Constants.NewLine}");
                return;
            }
            RoomManager.Instance.GetRoom(rExit.DestinationRoomID).DescribeRoom(session);
        }
        #endregion

        #region Inventory
        public static void PlayerConsumeItem(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Consume what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var item = session.Player.GetInventoryItem(arg);
            if (item == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            if (item.ItemType != ItemType.Consumable)
            {
                session.Send($"%BRT%You can't consume that!%PT%{Constants.NewLine}");
                return;
            }
            var potion = (Consumable)item;
            potion.Consume(session.Player);
            session.Player.RemoveItemFromInventory(item);
        }

        public static void PlayerReadScrolls(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: read '<scroll name>' <target>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: read 'scroll of lightning bolt' orc warboss%PT%{Constants.NewLine}");
                session.Send($"%BRT%To read a sign in a room: read sign%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length == 1 && args[0].ToLower() == "sign")
            {
                if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Sign))
                {
                    session.Send(RoomManager.Instance.GetRoom(session.Player.CurrentRoom).SignText);
                }
                else
                {
                    session.Send($"%BRT%There is no sign here to read!%PT%{Constants.NewLine}");
                }
                return;
            }
            if (!session.Player.HasSkill("Read Scroll"))
            {
                session.Send($"%BYT%The symbols on the scroll appear to swim before your eyes. Nothing makes sense!%PT%{Constants.NewLine}");
                return;
            }
            var scrollName = Helpers.GetQuotedString(arg);
            if (string.IsNullOrEmpty(scrollName))
            {
                session.Send($"%BRT%Usage: read '<scroll name>' <target>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: read 'scroll of lightning bolt' orc warboss%PT%{Constants.NewLine}");
                return;
            }
            var targetName = arg.Remove(0, scrollName.Length + 2).Trim();
            if (string.IsNullOrEmpty(targetName))
            {
                session.Send($"%BRT%Usage: read '<scroll name>' <target>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: read 'scroll of lightning bolt' orc warboss%PT%{Constants.NewLine}");
                return;
            }
            var invItem = session.Player.GetInventoryItem(scrollName);
            if (invItem == null)
            {
                session.Send($"%BRT%You don't seem to be carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoMagic))
            {
                session.Send($"%BRT%Some mystic force prevents you from reading the scroll!%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(targetName, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%No one like that is here...%PT%{Constants.NewLine}");
                return;
            }
            var scroll = (Scroll)invItem;
            var spell = SpellManager.Instance.GetSpell(scroll.CastsSpell);
            if (spell == null)
            {
                session.Send($"%BRT%This scroll seems to be defective! Check with an Imm!%PT%{Constants.NewLine}");
                return;
            }
            if (spell.SpellType == SpellType.Damage || spell.SpellType == SpellType.Debuff)
            {
                if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe | RoomFlags.NoMagic))
                {
                    session.Send($"%BRT%Some mystical force prevents that from happening...%PT%{Constants.NewLine}");
                    return;
                }
                if (target.ActorType == ActorType.Player)
                {
                    session.Send($"%BRT%Some power protects {target.Name}, that would be impossible.%PT%{Constants.NewLine}");
                    return;
                }
                if (((NPC)target).Flags.HasFlag(NPCFlags.NoAttack))
                {
                    session.Send($"%BRT%Some mystical force prevents that from happneing. You cannot harm {target.Name}!%PT%{Constants.NewLine}");
                    return;
                }
            }
            spell.Cast(session.Player, target);
            session.Player.RemoveItemFromInventory(scroll);
        }

        public static void DoBankingAction(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: bank <balance> - show your current balance%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: bank <deposit | withdraw> <amount> - deposit or withdraw the amount of gold%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            var operation = args.FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(operation))
            {
                session.Send($"%BRT%Usage: bank <balance> - show your current balance%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: bank <deposit | withdraw> <amount> - deposit or withdraw the amount of gold%PT%{Constants.NewLine}");
                return;
            }
            switch(operation.ToLower().Trim())
            {
                case "balance":
                    session.Send($"%BYT%The bank clerk checks some paperwork. \"Your current balance is {session.Player.VaultGold:N0} gold.\"%PT%{Constants.NewLine}");
                    break;

                case "deposit":
                    var strAmount = arg.Remove(0, operation.Length).Trim();
                    if (string.IsNullOrEmpty(strAmount))
                    {
                        session.Send($"%BRT%Usage: bank deposit <amount>%PT%{Constants.NewLine}");
                        return;
                    }
                    if (!ulong.TryParse(strAmount, out var amount))
                    {
                        session.Send($"%BRT%That isn't a valid amount!%PT%{Constants.NewLine}");
                        return;
                    }
                    if (amount > session.Player.Gold)
                    {
                        session.Send($"%BRT%You can't deposit more gold than you have!%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AddGoldToVault(amount);
                    session.Send($"%BYT%\"Certainly,\" the clerk says, \"your new balance is {session.Player.VaultGold:N0} gold.\"%PT%{Constants.NewLine}");
                    break;

                case "withdraw":
                    strAmount = arg.Remove(0, operation.Length).Trim();
                    if (string.IsNullOrEmpty(strAmount))
                    {
                        session.Send($"%BRT%Usage: bank withdraw <amount>%PT%{Constants.NewLine}");
                        return;
                    }
                    if (!ulong.TryParse(strAmount, out amount))
                    {
                        session.Send($"%BRT%That isn't a valid amount!%PT%{Constants.NewLine}");
                        return;
                    }
                    if (amount > session.Player.VaultGold)
                    {
                        session.Send($"%BRT%You can't withdraw more than your balance!%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.RemoveGoldFromVault(amount);
                    session.Send($"%BYT%\"Certainly,\" the clerk says, \"here are your coins, and your new balance is {session.Player.VaultGold:N0} gold.\"%PT%{Constants.NewLine}");
                    break;
            }
        }

        public static void DoVaultAction(Session session, ref string arg)
        {
            StringBuilder sb = new StringBuilder();
            if (!RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Vault))
            {
                session.Send($"%BRT%There is no Vault here!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                sb.AppendLine($"%BRT%Usage: vault <check | browse> - show stored items%PT%");
                sb.AppendLine($"%BRT%Usage: vault <deposit | store> <item> - store an item from your inventory%PT%");
                sb.AppendLine($"%BRT%Usage: vault <withdraw | take> <item> - transfer an item from your vault to your inventory%PT%");
                session.Send(sb.ToString());
                return;
            }
            var args = arg.Split(' ');
            var operation = args[0].ToLower().Trim();
            var itemName = arg.Remove(0, operation.Length).Trim();
            switch (operation)
            {
                case "check":
                case "browse":
                    if (session.Player.VaultItems.Count > 0)
                    {
                        sb.AppendLine($"%BYT%The warden checks his papers. \"This is what you have in storage,\" he says, showing a list:%PT%");
                        sb.AppendLine($"%BYT%  {new string('=', 77)}");
                        foreach (var i in session.Player.VaultItems.Values.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.ID))
                        {
                            var cnt = session.Player.VaultItems.Values.Where(x => x.ID == i.ID).Count();
                            sb.AppendLine($"%BYT%|| {cnt} x {i.Name}, {i.ShortDescription}%PT%");
                        }
                        sb.AppendLine($"%BYT%  {new string('=', 77)}");
                    }
                    else
                    {
                        sb.AppendLine($"%BYT%The warden checks through his papers. \"You don't have anything in storage right now,\" he says.%PT%");
                    }
                    session.Send(sb.ToString());
                    break;

                case "deposit":
                case "store":
                    if (string.IsNullOrEmpty(itemName))
                    {
                        session.Send($"%BRT%Store what, exactly?%PT%{Constants.NewLine}");
                        return;
                    }
                    var item = session.Player.GetInventoryItem(itemName);
                    if (item == null)
                    {
                        session.Send($"%BRT%Store what, exactly?%PT%{Constants.NewLine}");
                        return;
                    }
                    session.Player.AddItemToVault(item);
                    session.Send($"%BYT%\"Certainly,\" the vault warden says. \"We'll keep this safe for you.\"%PT%{Constants.NewLine}");
                    break;

                case "withdraw":
                case "take":
                    if (string.IsNullOrEmpty(itemName))
                    {
                        session.Send($"%BRT%Withdraw what, exactly?%PT%{Constants.NewLine}");
                        return;
                    }
                    else
                    {
                        item = session.Player.GetVaultItem(itemName);
                        if (item == null)
                        {
                            session.Send($"%BRT%The warden tuts. \"You don't have anything like that in your vault,\" he says.%PT%{Constants.NewLine}");
                            return;
                        }
                        session.Player.RemoveItemFromVault(item);
                        session.Send($"%BYT%\"Certainly,\" the warden says, \"I'll get that for you now.\"%PT%{Constants.NewLine}");
                    }
                    break;

                default:
                    session.Send($"%BRT%That doesn't look like something you can do in the vault!%PT%{Constants.NewLine}");
                    return;
            }
        }

        public static void ShowEquipment(Session session)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            sb.AppendLine($"%BYT%||%PT% You are using:");
            sb.AppendLine($"%BYT%||%PT% Head: {session.Player.HeadEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Neck: {session.Player.NeckEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Armour: {session.Player.ArmourEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Weapon: {session.Player.WeaponEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Held: {session.Player.HeldEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Feet: {session.Player.FeetEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Right Finger: {session.Player.RightFingerEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%||%PT% Left Finger: {session.Player.LeftFingerEquip?.Name ?? "Nothing"}");
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void RemoveEquipment(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: remove <slot>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: remove weapon%PT%{Constants.NewLine}");
                session.Send($"%BRT%Valid Slots: {string.Join(", ", Enum.GetNames(typeof(WearSlot)).Where(name => name != "None"))}%PT%{Constants.NewLine}");
                return;
            }
            if (!Enum.TryParse(arg, true, out WearSlot slot))
            {
                session.Send($"%BRT%That isn't a valid equipment slot!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.RemoveEquipment(slot, out dynamic i))
            {
                var msg = slot == WearSlot.Weapon ? $"%BGT%You stop using {i.ShortDescription} as your {slot}!%PT%{Constants.NewLine}" :
                    $"%BGT%You stop using {i.ShortDescription} on your {slot}!%PT%{Constants.NewLine}";
                session.Send(msg);
            }
            else
            {
                session.Send($"%BRT%You cannot remove equipment from your {slot}!%PT%{Constants.NewLine}");
            }
        }

        public static void EquipItem(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: equip <item> <slot>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: equip helmet head%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: equip ring of protection leftfinger%PT%{Constants.NewLine}");
                session.Send($"%BRT%Valid Slots: {string.Join(", ", Enum.GetNames(typeof(WearSlot)).Where(name => name != "None"))}%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            var slot = args.Last() ?? string.Empty;
            if (string.IsNullOrEmpty(slot))
            {
                session.Send($"%BRT%You need to specify where to equip the item.%PT%{Constants.NewLine}");
                return;
            }
            var itemName = arg.Remove(arg.Length - slot.Length, slot.Length).Trim();
            if (string.IsNullOrEmpty(itemName))
            {
                session.Send($"%BRT%You need to specify an item to equip.%PT%{Constants.NewLine}");
                return;
            }
            if (!Enum.TryParse(slot, true, out WearSlot slotResult))
            {
                session.Send($"%BRT%That doesn't look like a valid equipment slot!%PT%{Constants.NewLine}");
                return;
            }
            var item = session.Player.GetInventoryItem(itemName);
            if (item == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.CanEquip(item, slotResult, out string result))
            {
                session.Player.EquipItem(item, slotResult);
                var msg = slotResult == WearSlot.Weapon ? $"%BGT%You start using {item.ShortDescription} as your {slotResult}!%PT%{Constants.NewLine}" :
                    $"%BGT%You start using {item.ShortDescription} on your {slotResult}!%PT%{Constants.NewLine}";
                session.Send(msg);
            }
            else
            {
                session.Send($"%BRT%You cannot equip that: {result}%PT%{Constants.NewLine}");
            }
        }

        public static void SacrificeItem(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to specify an item to do that!%PT%{Constants.NewLine}");
                return;
            }
            var item = session.Player.GetInventoryItem(arg);
            if (item == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            if (item.BaseValue == 0)
            {
                session.Send($"%BRT%The Gods are not interested in such worthless trinkets.%PT%{Constants.NewLine}");
                return;
            }
            session.Player.RemoveItemFromInventory(item);
            var chance = item.BaseValue / 100;
            var result = Helpers.RollDice<int>(1, 100);
            if (result > chance)
            {
                session.Send($"%BRT%You offer {item.ShortDescription} to the Gods, but they do not respond.%PT%{Constants.NewLine}");
                var lPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
                if (lPlayers != null && lPlayers.Count > 1)
                {
                    foreach(var lp in lPlayers.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BRT%{session.Player.Name} offers {item.ShortDescription} as a sacrifice to the Gods, but they do not respond.%PT%{Constants.NewLine}" :
                            $"Something offers {item.ShortDescription} as a sacrifice to the Gods, but they do not respond.%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            session.Send($"%BYT%You offer {item.ShortDescription} to the Gods and their power bathes you in holy light!%PT%{Constants.NewLine}");
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            if (localPlayers != null && localPlayers.Count > 1)
            {
                foreach (var lp in localPlayers.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} is bathed in holy light after offering {item.ShortDescription} as a sacrifice to the Gods!%PT%" :
                        $"%BYT%Something is bathed in holy light after offering {item.ShortDescription} as a sacrifice to the Gods!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            if (item.BaseValue >= 1000)
            {
                var s = SpellManager.Instance.GetSpell(SpellType.Buff).GetRandomElement();
                foreach(var b in s.AppliedBuffs.Keys)
                {
                    var buff = BuffManager.Instance.GetBuff(b);
                    if (buff != null)
                    {
                        session.Player.ApplyBuff(buff.Name, buff.Duration);
                        session.Send($"%BYT%The Gods accept your sacrifice and the Winds of Magic bless you with {buff.Name}!%PT%{Constants.NewLine}");
                    }
                }
            }
            else
            {
                var gpAmount = Helpers.RollDice<long>(1, item.BaseValue);
                session.Player.AdjustGold(gpAmount, true, true);
                session.Send($"%BYT%The Gods accept your sacrifice and reward you with {gpAmount:N0} gold!%PT%{Constants.NewLine}");
            }
        }

        public static void DonateItem(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to specify and item to do this...%PT%{Constants.NewLine}");
                return;
            }
            var item = session.Player.GetInventoryItem(arg);
            if (item == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that...%PT%{Constants.NewLine}");
                return;
            }
            if (!RoomManager.Instance.RoomExists(Game.DonationRoomID))
            {
                session.Send($"%BRT%The Gods refuse to answer...%PT%{Constants.NewLine}");
                return;
            }
            session.Player.RemoveItemFromInventory(item);
            session.Send($"%BYT%You offer {item.ShortDescription} to the Gods and it vanishes with a flash!%PT%{Constants.NewLine}");
            RoomManager.Instance.AddItemToRoomInventory(Game.DonationRoomID, item);
            Game.LogMessage($"INFO: Player {session.Player.Name} donated {item.Name} ({item.ID})", LogLevel.Info);
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            if (localPlayers != null && localPlayers.Count > 1)
            {
                foreach(var lp in localPlayers.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} offers {item.ShortDescription} to the Gods and it vanishes in a flash of light!%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts as {item.ShortDescription} is swallowed by the Winds of Magic!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            var donRoomPlayers = RoomManager.Instance.GetRoom(Game.DonationRoomID).PlayersInRoom;
            if (donRoomPlayers != null && donRoomPlayers.Count > 0)
            {
                foreach(var lp in donRoomPlayers)
                {
                    lp.Send($"%BYT%There is a bright flash of light as {item.ShortDescription} clatters to the floor!%PT%{Constants.NewLine}");
                }
            }
        }

        public static void TradeItem(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: give <target> <item>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: give <target> <amount>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: give fred chain shirt%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: give fred 500%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: give <target> <item>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: give fred chain shirt%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(args[0], session.Player);
            if (target == null)
            {
                session.Send($"%BRT%You can't give something to someone that isn't here!%PT%{Constants.NewLine}");
                return;
            }
            var itemName = arg.Remove(0, args[0].Length).Trim();
            if (ulong.TryParse(itemName, out ulong gp))
            {
                if (session.Player.Gold < gp)
                {
                    session.Send($"%BRT%You don't have that much gold!%PT%{Constants.NewLine}");
                    return;
                }
                session.Player.AdjustGold((long)gp * -1, true, false);
                target.Gold += gp;
                session.Send($"%BYT%You hand over {gp:N0} gold to {target.Name}!%PT%{Constants.NewLine}");
                if (target.ActorType == ActorType.Player)
                {
                    var tMsg = session.Player.CanBeSeenBy(target) ? $"%BYT%{session.Player.Name} hands you the sum of {gp:N0} gold!%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts as something hands you {gp:N0} gold!%PT%{Constants.NewLine}";
                    ((Player)target).Send(tMsg);
                }
                else
                {
                    var tNPC = (NPC)target;
                    if (tNPC.MobProgs.Count > 0)
                    {
                        foreach(var mp in tNPC.MobProgs.Keys)
                        {
                            var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                            if (mobProg != null)
                            {
                                mobProg.Init();
                                mobProg.TriggerEvent(MobProgTrigger.ReceiveGold, new { mob = tNPC.ID.ToString(), player = session.ID.ToString(), amount = gp });
                            }
                        }
                    }
                }
                var lPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
                if (lPlayers != null && lPlayers.Count > 1)
                {
                    foreach(var lp in lPlayers.Where(x => x.ID != session.ID && x.ID != target.ID))
                    {
                        var tName = target.CanBeSeenBy(lp.Player) ? target.Name : "something";
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} hands {tName} the sum of {gp:N0} gold!%PT%{Constants.NewLine}" :
                            $"The air shifts as something hands {gp:N0} gold to {tName}.%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                return;
            }
            var item = session.Player.GetInventoryItem(itemName);
            if (item == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.RemoveItemFromInventory(item);
            target.AddItemToInventory(item);
            session.Send($"%BYT%You hand {item.ShortDescription} to {target.Name}. Hope they like it!%PT%{Constants.NewLine}");
            if (target.ActorType == ActorType.Player)
            {
                var msgToTarget = session.Player.CanBeSeenBy(target) ? $"%BYT%{session.Player.Name} hands you {item.ShortDescription}.%PT%{Constants.NewLine}" :
                    $"%BYT%The air around you shifts as something hands you {item.ShortDescription}.%PT%{Constants.NewLine}";
                ((Player)target).Send(msgToTarget);
            }
            else
            {
                var tNPC = (NPC)target;
                if (tNPC.MobProgs.Count > 0)
                {
                    foreach(var mp in tNPC.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.ReceiveItem, new {mob = tNPC.ID.ToString(), player = session.ID.ToString(), itemID = item.ID });
                        }
                    }
                }
            }
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom;
            if (localPlayers != null && localPlayers.Count > 1)
            {
                foreach(var lp in localPlayers.Where(x => x.ID != session.ID && x.ID != target.ID))
                {
                    var tName = target.CanBeSeenBy(lp.Player) ? target.Name : "something";
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} hands {item.ShortDescription} to {tName}.%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts as something hands {item.ShortDescription} to {tName}.%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
        }

        public static void DropItem(Session session, string arg)
        {
            var r = RoomManager.Instance.GetRoom(session.Player.CurrentRoom);
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: drop <amount> gold%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: drop <item>%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (ulong.TryParse(args[0], out ulong gp))
            {
                if (session.Player.Gold == 0)
                {
                    session.Send($"%BRT%You don't have any gold to drop!%PT%{Constants.NewLine}");
                    return;
                }
                var amount = Math.Min(gp, session.Player.Gold);
                session.Player.AdjustGold((long)amount * -1, true, false);
                RoomManager.Instance.AddGoldToRoom(r.ID, amount);
                session.Send($"%BYT%You drop {amount:N0} gold to the floor! Litter-bug!%PT%{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetRoom(r.ID).PlayersInRoom;
                if (localPlayers != null && localPlayers.Count > 1)
                {
                    foreach(var lp in localPlayers.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} drops {amount:N0} gold to the floor!%PT%{Constants.NewLine}" :
                            $"%BYT%The air shifts and suddenly {amount:N0} gold coins fall to the floor!%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                foreach(var n in r.NPCsInRoom)
                {
                    if (n.MobProgs.Count > 0)
                    {
                        foreach(var mp in n.MobProgs.Keys)
                        {
                            var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                            if (mobProg != null)
                            {
                                mobProg.Init();
                                mobProg.TriggerEvent(MobProgTrigger.PlayerDropGold, new { player = session.ID.ToString(), mob = n.ID.ToString(), amount });
                            }
                        }
                    }
                }
                return;
            }
            var item = session.Player.Inventory.Values.FirstOrDefault(x => x.Name.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0);
            if (item == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.RemoveItemFromInventory(item);
            RoomManager.Instance.AddItemToRoomInventory(r.ID, item);
            session.Send($"%BYT%You drop {item.ShortDescription} onto the floor.%PT%{Constants.NewLine}");
            var players = RoomManager.Instance.GetRoom(r.ID).PlayersInRoom;
            if (players != null && players.Count > 1)
            {
                foreach(var lp in players.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} drops {item.ShortDescription} to the floor.%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts as something drops {item.ShortDescription} onto the floor.%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            foreach(var n in r.NPCsInRoom)
            {
                if (n.MobProgs.Count > 0)
                {
                    foreach(var mp in n.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerDropItem, new { mob = n.ID.ToString(), player = session.ID.ToString(), itemID = item.ID });
                        }
                    }
                }
            }
        }

        public static void GetItem(Session session, ref string arg)
        {
            var r = RoomManager.Instance.GetRoom(session.Player.CurrentRoom);
            if (r.ItemsInRoom.Count == 0 && r.GoldInRoom == 0)
            {
                session.Send($"%BRT%There is nothing here to take!%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to specify something to take!%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: get long sword%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: get 500 gold%PT%{Constants.NewLine}");
            }
            var args = arg.Split(' ');
            if (ulong.TryParse(args[0].Trim(), out ulong gp))
            {
                if (r.GoldInRoom == 0)
                {
                    session.Send($"%BRT%There isn't any gold here!%PT%{Constants.NewLine}");
                    return;
                }
                var amount = Math.Min(r.GoldInRoom, gp);
                RoomManager.Instance.RemoveGoldFromRoom(r.ID, amount);
                session.Player.AdjustGold((int)amount, true, true);
                session.Send($"%BYT%You swipe {amount:N0} gold and stuff it in your pockets!%PT%{Constants.NewLine}");
                var localPlayers = RoomManager.Instance.GetRoom(r.ID).PlayersInRoom;
                if (localPlayers != null && localPlayers.Count > 1)
                {
                    foreach(var lp in localPlayers.Where(x => x.ID != session.ID))
                    {
                        var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} greedily snatches up {amount:N0} gold coins!%PT%{Constants.NewLine}" :
                            $"%BYT%The air shifts and {amount:N0} gold coins vanish into something's pockets!%PT%{Constants.NewLine}";
                        lp.Send(msg);
                    }
                }
                foreach(var n in r.NPCsInRoom)
                {
                    if (n.MobProgs.Count > 0)
                    {
                        foreach(var mp in n.MobProgs.Keys)
                        {
                            var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                            if (mobProg != null)
                            {
                                mobProg.Init();
                                mobProg.TriggerEvent(MobProgTrigger.PlayerTakeGold, new { mob = n.ID.ToString(), player = session.ID.ToString(), amount });
                            }
                        }
                    }
                }
                return;
            }
            var item = r.GetItem(arg);
            if (item == null)
            {
                session.Send($"%BRT%There is nothing like that to take!%PT%{Constants.NewLine}");
                return;
            }
            RoomManager.Instance.RemoveItemFromRoomInventory(r.ID, item);
            session.Player.AddItemToInventory(item);
            session.Send($"%BYT%You take {item.ShortDescription}, hope no one needed that!%PT%{Constants.NewLine}");
            var players = RoomManager.Instance.GetRoom(r.ID).PlayersInRoom;
            if (players != null && players.Count > 1)
            {
                foreach(var lp in players.Where(x => x.ID != session.ID))
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} takes {item.ShortDescription} off the floor.%PT%{Constants.NewLine}" :
                        $"%BYT%The air shifts as something snatches up {item.ShortDescription}!%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            foreach(var n in r.NPCsInRoom)
            {
                if (n.MobProgs.Count > 0)
                {
                    foreach(var mp in n.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerTakeItem, new { mob = n.ID.ToString(), player = session.ID.ToString(), itemID = item.ID });
                        }
                    }
                }
            }
        }

        public static void ShowInventory(Session session, ref string verb, string criteria)
        {
            if (session.Player.Inventory.Count == 0)
            {
                session.Send($"You aren't carrying anything right now.{Constants.NewLine}");
            }
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(criteria))
            {
                sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
                sb.AppendLine($"%BYT%||%PT% You are carrying:");
                foreach (var i in session.Player.Inventory.Values.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.ID))
                {
                    var cnt = session.Player.Inventory.Values.Where(x => x.ID == i.ID).Count();
                    sb.AppendLine($"%BYT%||%PT% {cnt} x {i.Name}, {i.ShortDescription}");
                }
                sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
                session.Send(sb.ToString());
                return;
            }
            // show filtered inventory
            var matchingItems = session.Player.Inventory.Values.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.ID).ToList();
            if (matchingItems != null && matchingItems.Count > 0)
            {
                sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
                sb.AppendLine($"%BYT%||%PT% You are carrying:");
                foreach (var item in matchingItems)
                {
                    var cnt = session.Player.Inventory.Values.Where(x => x.ID == item.ID).Count();
                    sb.AppendLine($"%BYT%||%PT% {cnt} x {item.Name}, {item.ShortDescription}");
                }
                sb.AppendLine($"  %BYT%{new string('=', 77)}%PT%");
                session.Send(sb.ToString());
                return;
            }
            else
            {
                session.Send($"You aren't carrying anything like that...{Constants.NewLine}");
            }
        }
        #endregion

        #region Movement
        public static void PlayerPushTarget(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Push what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var args = arg.Split(' ');
            if (args.Length < 2)
            {
                session.Send($"%BRT%Usage: push '<target>' <direction>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: push 'orc warrior' north%PT%{Constants.NewLine}");
                return;
            }
            var targetName = Helpers.GetQuotedString(arg);
            if (string.IsNullOrEmpty(targetName))
            {
                session.Send($"%BRT%Usage: push '<target>' <direction>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: push 'orc warrior' north%PT%{Constants.NewLine}");
                return;
            }
            var dir = args.Last();
            var fullDirString = Helpers.GetFullDirectionString(dir);
            if (string.IsNullOrEmpty(fullDirString))
            {
                session.Send($"%BRT%That doesn't seem to be a valid direction!%PT%{Constants.NewLine}");
                return;
            }
            var exit = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetRoomExit(fullDirString);
            if (exit == null)
            {
                session.Send($"%BRT%It doesn't look like you can push anything in that direction!%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(targetName, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%You can't push someone that isn't here!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType == ActorType.Player)
            {
                session.Send($"%BRT%Some mystic force prevents you from pushing {target.Name}!%PT%{Constants.NewLine}");
                return;
            }
            var nTarget = (NPC)target;
            if (nTarget.Flags.HasFlag(NPCFlags.NoPush))
            {
                session.Send($"%BYT%Some mystical force prevents that from happening, you cannot push {nTarget.Name}!%PT%{Constants.NewLine}");
                return;
            }
            if (!nTarget.CanMove())
            {
                session.Send($"%BRT%You can't force {nTarget.Name} to move right now!%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(exit.DestinationRoomID).ZoneID != nTarget.ZoneID)
            {
                session.Send($"%BYT%Some mystical force prevents that from happening.%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(exit.DestinationRoomID).Flags.HasFlag(RoomFlags.NoMobs) || RoomManager.Instance.GetRoom(exit.DestinationRoomID).Flags.HasFlag(RoomFlags.GodRoom))
            {
                session.Send($"%BYT%Some mystical force prevents that from happening...%PT%{Constants.NewLine}");
                return;
            }
            var pRoll = Helpers.RollDice<int>(1, 20);
            var nRoll = Helpers.RollDice<int>(1, 20);
            var pFinal = Math.Max(1, pRoll + Helpers.CalculateAbilityModifier(session.Player.Strength));
            var nFinal = Math.Max(1, nRoll + Helpers.CalculateAbilityModifier(nTarget.Strength));
            if (pFinal > nFinal)
            {
                session.Send($"%BGT%With great strength, you push {nTarget.Name} {fullDirString}!%PT%{Constants.NewLine}");
                nTarget.Move(exit.DestinationRoomID, false);
            }
            else
            {
                session.Send($"%BRT%Try as you might, can't push {nTarget.Name} {fullDirString}!%PT%{Constants.NewLine}");
                return;
            }
        }

        public static void PlayerRecall(Session session)
        {
            if (!session.Player.CanMove())
            {
                session.Send($"%BRT%You are not able to do that right now!%PT%{Constants.NewLine}");
                return;
            }
            if (!RoomManager.Instance.RoomExists(Game.PlayerStartRoom))
            {
                session.Send($"%BRT%The Gods will not answer your prayers! You are on your own!%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoTeleport))
            {
                session.Send($"%BRT%The Gods refuse your prayers. You must make your own way.%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.CurrentSP < 5)
            {
                session.Send($"%BRT%The Gods find you lacking and will not offer you their help!%PT%{Constants.NewLine}");
                return;
            }
            var localPlayers = SessionManager.Instance.GetPlayersInRoom(session.Player.CurrentRoom).Where(x => x.ID != session.ID).ToList();
            if (localPlayers != null && localPlayers.Count > 0)
            {
                foreach(var lp in localPlayers)
                {
                    var msg = session.Player.CanBeSeenBy(lp.Player) ? $"%BYT%{session.Player.Name} offers a prayer to the Gods...%PT%{Constants.NewLine}" :
                        $"%BYT%The air shimmers as something offers a prayer to the Gods...%PT%{Constants.NewLine}";
                    lp.Send(msg);
                }
            }
            var spCost = session.Player.CurrentSP / 2;
            session.Player.AdjustSP(spCost * -1);
            session.Player.Move(Game.PlayerStartRoom, true);
        }

        public static void ChangePosition(Session session, string newPos)
        {
            if (session.Player.InCombat)
            {
                session.Send($"You're too busy fighting to do that right now!{Constants.NewLine}");
                return;
            }
            string msgToRoom = string.Empty;
            switch(newPos)
            {
                case "sit":
                    session.Player.Position = ActorPosition.Sitting;
                    session.Send($"You sit down and take the load off.{Constants.NewLine}");
                    msgToRoom = $"%N% sits down and takes the load off.{Constants.NewLine}";
                    break;

                case "stand":
                    session.Player.Position = ActorPosition.Standing;
                    session.Send($"You stand up, ready to continue your adventures!{Constants.NewLine}");
                    msgToRoom = $"%N% stands up, ready to continue adventuring!{Constants.NewLine}";
                    break;

                case "sleep":
                    session.Player.Position = ActorPosition.Sleeping;
                    session.Send($"You curl up and have a nap. ZZZZZZZZZZ.{Constants.NewLine}");
                    msgToRoom = $"%N% curls up and catches some sleep!{Constants.NewLine}";
                    break;

                case "rest":
                    session.Player.Position = ActorPosition.Resting;
                    session.Send($"You decide to take it easy for a while.{Constants.NewLine}");
                    msgToRoom = $"%N% decides to take things easy for a while and has a rest.{Constants.NewLine}";
                    break;

                default:
                    session.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                    return;
            }
            var localPlayers = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).PlayersInRoom.Where(x => x.ID != session.ID).ToList();
            if (localPlayers != null && localPlayers.Count > 0)
            {
                foreach(var lp in localPlayers)
                {
                    msgToRoom = session.Player.CanBeSeenBy(lp.Player) ? msgToRoom.Replace("%N%", session.Player.Name) : msgToRoom.Replace("%N%", "Something");
                    lp.Send(msgToRoom);
                }
            }
        }

        public static void Move(Session session, string direction)
        {
            if (!session.Player.CanMove())
            {
                session.Send($"You're not in a position to move right now!{Constants.NewLine}");
                return;
            }
            var exitDir = Helpers.GetFullDirectionString(direction);
            var exitObj = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetRoomExit(exitDir);
            if (exitObj == null)
            {
                session.Send($"You cannot go that way!{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(exitObj.DestinationRoomID) == null)
            {
                session.Send($"That way lies only the void! You cannot go that way.{Constants.NewLine}");
                return;
            }
            if (!string.IsNullOrEmpty(exitObj.RequiredSkill) && !session.Player.HasSkill(exitObj.RequiredSkill))
            {
                session.Send($"You lack the skill to go that way!{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(exitObj.DestinationRoomID).Flags.HasFlag(RoomFlags.GodRoom) && !session.Player.IsImmortal)
            {
                session.Send($"%BYT%Some mystical force prevents you from going that way...%PT%{Constants.NewLine}");
                return;
            }
            int spCost = 1;
            bool landWalker = session.Player.HasSkill("Land Walker") || session.Player.HasBuff("Land Walker");
            if (!landWalker && RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.HardTerrain))
            {
                spCost += 2;
            }
            if (!landWalker && RoomManager.Instance.GetRoom(exitObj.DestinationRoomID).Flags.HasFlag(RoomFlags.HardTerrain))
            {
                spCost += 2;
            }
            if (session.Player.CurrentSP < spCost)
            {
                session.Send($"You lack the stamina to move that far. Maybe you should rest a while?{Constants.NewLine}");
                return;
            }
            session.Player.AdjustSP(spCost * -1);
            session.Player.Move(exitObj.DestinationRoomID, false);
        }
        #endregion

        #region Shopping
        public static void EnterShop(Session session, string arg)
        {
            if (session.Player.ShopContext != null)
            {
                session.Send($"%BRT%You are already a customer of {session.Player.ShopContext.ShopName}!%PT%{Constants.NewLine}");
                return;
            }
            var r = RoomManager.Instance.GetRoom(session.Player.CurrentRoom);
            var shopNPCs = NPCManager.Instance.GetShopNPCsInRoom(r.ID);
            if (shopNPCs == null || shopNPCs.Count == 0)
            {
                session.Send($"%BRT%There are no shops here...%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%You need to specify which shop you want to do business with!%PT%{Constants.NewLine}");
                return;
            }
            var shops = from npc in shopNPCs select ShopManager.Instance.GetShop(npc.ShopID);
            var tShop = shops.FirstOrDefault(x => x.ShopName.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0);
            var tNPC = shopNPCs.FirstOrDefault(x => x.ShopID == tShop.ID);
            if (tShop != null)
            {
                session.Player.ShopContext = tShop;
                session.Send($"%BGT%You are now a customer of {tShop.ShopName}%PT%{Constants.NewLine}");
                if (tNPC.MobProgs.Count > 0)
                {
                    foreach(var mp in tNPC.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.PlayerEnterShop, new { mob = tNPC.ID.ToString(), player = session.Player.ID.ToString() });
                        }
                    }
                }
            }
            else
            {
                session.Send($"%BRT%Something went wrong with the Shopping system - tell an Imm!%PT%{Constants.NewLine}");
            }
        }

        public static void LeaveShop(Session session)
        {
            if (session.Player.ShopContext == null)
            {
                session.Send($"%BRT%You're not doing business with any shop!%PT%{Constants.NewLine}");
                return;
            }
            session.Send($"%BGT%You stop doing business with {session.Player.ShopContext.ShopName}!%PT%{Constants.NewLine}");
            var r = RoomManager.Instance.GetRoom(session.Player.CurrentRoom);
            var tNPC = r.NPCsInRoom.FirstOrDefault(x => x.ShopID == session.Player.ShopContext.ID);
            if (tNPC.MobProgs.Count > 0)
            {
                foreach (var mp in tNPC.MobProgs.Keys)
                {
                    var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                    if (mobProg != null)
                    {
                        mobProg.Init();
                        mobProg.TriggerEvent(MobProgTrigger.PlayerLeaveShop, new { mob = tNPC.ID.ToString(), player = session.Player.ID.ToString() });
                    }
                }
            }
            session.Player.ShopContext = null;
        }

        public static void ShowShopInventory(Session session, ref string arg)
        {
            if (session.Player.ShopContext == null)
            {
                session.Send($"%BRT%You're not doing business with any shop...%PT%{Constants.NewLine}");
                return;
            }
            session.Player.ShopContext.ShowInventory(session, arg);
        }

        public static void ShopAppraiseItem(Session session, ref string arg)
        {
            if (session.Player.ShopContext == null)
            {
                session.Send($"%BRT%You're not doing busiess with any shop...%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Appraise what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var i = session.Player.GetInventoryItem(arg);
            if (i == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.ShopContext.AppraiseItem(session, i);
        }

        public static void PurchaseItem(Session session, ref string arg)
        {
            if (session.Player.ShopContext == null)
            {
                session.Send($"%BRT%You're not doing business with any shop...%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Purchase what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            session.Player.ShopContext.PlayerBuyItem(session, arg);
        }

        public static void SellItem(Session session, ref string arg)
        {
            if (session.Player.ShopContext == null)
            {
                session.Send($"%BRT%You're not doing business with any shop...%PT%{Constants.NewLine}");
                return;
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Sell what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            var i = session.Player.GetInventoryItem(arg);
            if (i == null)
            {
                session.Send($"%BRT%You aren't carrying anything like that!%PT%{Constants.NewLine}");
                return;
            }
            session.Player.ShopContext.PlayerSellItem(session, i);
        }
        #endregion

        #region Combat
        public static void PlayerBackstab(Session session, ref string arg)
        {
            // usual room checks, no hostile checks, no backstab checks. Do hit roll and deal 4* damage if successfull, start combat if the target (NPC only) isn't killed
            // also ensure player is hidden and has backstab skill
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
            {
                session.Send($"%BYT%Some mystical force prevents your hostility...%PT%{Constants.NewLine}");
                return;
            }
            if (!session.Player.HasSkill("Backstab"))
            {
                session.Send($"%BRT%You don't know how to do that!%PT%{Constants.NewLine}");
            }
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Backstabbing requires a target...%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.WeaponEquip == null)
            {
                session.Send($"%BRT%Executing a backstab requires a weapon...%PT%{Constants.NewLine}");
                return;
            }
            else
            {
                Weapon w = (Weapon)session.Player.WeaponEquip;
                if (w.WeaponType != WeaponType.Dagger && w.WeaponType != WeaponType.Sword)
                {
                    session.Send($"%BRT%You need to be using a sword or a dagger to execute a backstab!%PT%{Constants.NewLine}");
                    return;
                }
            }
            if (session.Player.Visible)
            {
                session.Send($"%BRT%You need to be hidden to do that!%PT%{Constants.NewLine}");
                return;
            }
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(arg, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%The target of your wrath isn't here!%PT%{Constants.NewLine}");
                return;
            }
            if (target.ActorType != ActorType.NonPlayer)
            {
                session.Send($"%BYT%Some mystical force prevents your hostility towards {target.Name}!%PT%{Constants.NewLine}");
                return;
            }
            var npcTarget = (NPC)target;
            if (session.Player.CanBeSeenBy(npcTarget))
            {
                session.Send($"%BRT%Despite your best efforts, {npcTarget.Name} can still see you... A backstab would be impossible!%PT%{Constants.NewLine}");
                return;
            }
            if (npcTarget.Flags.HasFlag(NPCFlags.NoBackstab) || npcTarget.Flags.HasFlag(NPCFlags.NoAttack))
            {
                session.Send($"%BYT%Some mystical force prevents your hostility towards {target.Name}!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.HitsTarget(npcTarget, out bool isCrit, out int baseHit, out int modHit))
            {
                var baseDmg = session.Player.CalculateHitDamage(npcTarget, isCrit);
                var modDmg = baseDmg * 4;
                npcTarget.AdjustHP((modDmg * -1), out bool isKilled);
                if (isKilled)
                {
                    session.Send($"%BYT%You become visible as your backstab strikes {npcTarget.Name} for {modDmg} damage, killing them instantly!%PT%{Constants.NewLine}");
                    session.Player.Visible = true;
                    npcTarget.Kill(session.Player, true);
                    Game.LogMessage($"COMBAT: Player {session.Player.Name} has killed {npcTarget.Name} in Room {session.Player.CurrentRoom} with a backstab", LogLevel.Combat);
                }
                else
                {
                    session.Send($"%BYT%You become visible as your backstab strikes {npcTarget.Name} for {modDmg} damage!%PT%{Constants.NewLine}");
                    session.Player.Visible = true;
                    npcTarget.TargetQueue.TryAdd(session.ID, true);
                    session.Player.TargetQueue.TryAdd(npcTarget.ID, true);
                }
            }
            else
            {
                session.Player.Visible = true;
                session.Send($"%BYT%You become visible as your backstab misses!%PT%{Constants.NewLine}");
                npcTarget.TargetQueue.TryAdd(session.ID, true);
                session.Player.TargetQueue.TryAdd(npcTarget.ID, true);
            }
        }

        public static void ToggleRollInfo(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                if (session.Player.ShowDetailedRollInfo)
                {
                    session.Send($"%BGT%Detailed roll information is currently on!%PT%{Constants.NewLine}");
                }
                else
                {
                    session.Send($"%BGT%Detailed roll information is currently off!%PT%{Constants.NewLine}");
                }
                session.Send($"%BRT%Usage: rollinfo - show your current setting%PT%{Constants.NewLine}");
                session.Send($"%BRT%Usage: rollinfo <on | off> - turn detailed roll info on or off%PT%{Constants.NewLine}");
                return;
            }
            if (arg.ToLower() == "on")
            {
                session.Player.ShowDetailedRollInfo = true;
                session.Send($"%BGT%Detailed roll info is now ON!%PT%{Constants.NewLine}");
                return;
            }
            if (arg.ToLower() == "off")
            {
                session.Player.ShowDetailedRollInfo = false;
                session.Send($"%BGT%Detailed roll info is now OFF!%PT%{Constants.NewLine}");
                return;
            }
            session.Send($"%BRT%Usage: rollinfo - show your current setting%PT%{Constants.NewLine}");
            session.Send($"%BRT%Usage: rollinfo <on | off> - turn detailed roll info on or off%PT%{Constants.NewLine}");
        }

        public static void StartCombat(Session session, ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Kill what, exactly?%PT%{Constants.NewLine}");
                return;
            }
            if (RoomManager.Instance.GetRoom(session.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
            {
                session.Send($"%BYT%Some mystical force prevents your hostility...%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.Position != ActorPosition.Standing)
            {
                session.Send($"%BRT%You're not in a position to do that right now.%PT%{Constants.NewLine}");
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
                session.Send($"%BYT%The Gods forbid this action!%PT%{Constants.NewLine}");
                return;
            }
            var tNPC = (NPC)target;
            if (tNPC.Flags.HasFlag(NPCFlags.NoAttack))
            {
                session.Send($"%BYT%Some mystical force prevents you from harming {tNPC.Name}...%PT%{Constants.NewLine}");
                return;
            }
            session.Player.TargetQueue.TryAdd(tNPC.ID, true);
            tNPC.TargetQueue.TryAdd(session.ID, true);
            if (tNPC.MobProgs.Count > 0)
            {
                foreach(var mp in tNPC.MobProgs.Keys)
                {
                    var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                    if (mobProg != null)
                    {
                        mobProg.Init();
                        mobProg.TriggerEvent(MobProgTrigger.MobAttacked, new { mob = tNPC.ID.ToString(), player = session.ID.ToString() });
                    }
                }
            }
        }

        public static void PlayerFleeCombat(Session session)
        {
            if (!session.Player.InCombat)
            {
                session.Send($"%BRT%Flee from what? You aren't fighting right now!%PT%{Constants.NewLine}");
                return;
            }
            if (Helpers.FleeCombat(session.Player, out int rid))
            {
                session.Send($"%BYT%Sometimes it is better to live and fight another day! You flee...%PT%{Constants.NewLine}");
                session.Player.Move(rid, false);
            }
            else
            {
                session.Send($"%BRT%Try as you might, you cannot flee!%PT%{Constants.NewLine}");
            }
        }
        #endregion

        #region Magic
        public static void PlayerCastSpell(Session session, ref string arg)
        {
            // cast <spell name> target
            if (string.IsNullOrEmpty(arg))
            {
                session.Send($"%BRT%Usage: cast '<spell name>' <target>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: cast 'magic missile' orc warrior%PT%{Constants.NewLine}");
                return;
            }
            var spellName = Helpers.GetQuotedString(arg);
            if (string.IsNullOrEmpty(spellName))
            {
                session.Send($"%BRT%Usage: cast '<spell name>' <target>%PT%{Constants.NewLine}");
                session.Send($"%BRT%Example: cast 'magic missile' orc warrior%PT%{Constants.NewLine}");
                return;
            }
            var spell = SpellManager.Instance.GetSpell(spellName);
            if (spell == null)
            {
                session.Send($"%BRT%No such Spell exists within the Realms!%PT%{Constants.NewLine}");
                return;
            }
            if (!session.Player.KnowsSpell(spell.Name))
            {
                session.Send($"%BRT%You don't know how to cast that spell!%PT%{Constants.NewLine}");
                return;
            }
            var strTarget = arg.Remove(0, spellName.Length + 2).Trim();
            var target = RoomManager.Instance.GetRoom(session.Player.CurrentRoom).GetActor(strTarget, session.Player);
            if (target == null)
            {
                session.Send($"%BRT%The target of your magic cannot be found!%PT%{Constants.NewLine}");
                return;
            }
            spell.Cast(session.Player, target);
        }
        #endregion
    }
}
