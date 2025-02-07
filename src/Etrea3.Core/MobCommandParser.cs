using Etrea3.Objects;

namespace Etrea3.Core
{
    public static partial class ActMob
    {
        public static void ParseCommand(NPC npc, string command, Session session = null)
        {
            if (string.IsNullOrEmpty(command))
            {
                session?.Send($"%BRT%Command was null or empty!%PT%{Constants.NewLine}");
                return;
            }
            var verb = Helpers.GetVerb(ref command);
            if (string.IsNullOrEmpty(verb))
            {
                session?.Send($"%BRT%No verb in the command!%PT%{Constants.NewLine}");
                return;
            }
            string arg = command.Remove(0, verb.Length).Trim();
            switch(verb.ToLower())
            {
                case "n":
                case "north":
                case "w":
                case "west":
                case "e":
                case "east":
                case "s":
                case "south":
                case "nw":
                case "northwest":
                case "ne":
                case "northeast":
                case "sw":
                case "southwest":
                case "se":
                case "southeast":
                case "d":
                case "down":
                case "u":
                case "up":
                    MobMove(npc, verb, session);
                    break;

                case "get":
                case "take":
                    MobTakeItem(npc, arg, session);
                    break;

                case "drop":
                    MobDropItem(npc, arg, session);
                    break;

                case "give":
                case "trade":
                    MobGiveItem(npc, arg, session);
                    break;

                case "attack":
                case "kill":
                    MobAttack(npc, arg, session);
                    break;

                case "emote":
                    MobEmote(npc, arg, session);
                    break;

                case "cast":
                    MobCastSpell(npc, arg, session);
                    break;

                case "say":
                    MobSay(npc, arg, session);
                    break;

                case "whisper":
                case "tell":
                    MobWhisper(npc, arg, session);
                    break;

                case "yell":
                case "shout":
                    MobYell(npc, arg, session);
                    break;

                case "remember":
                    MobRememberPlayer(npc, arg, session);
                    break;

                case "forget":
                    MobForgetPlayer(npc, arg, session);
                    break;

                default:
                    session?.Send($"%BRT%{npc.Name} does not know how to {verb} and cannot comply!%PT%{Constants.NewLine}");
                    break;
            }
        }
    }
}
