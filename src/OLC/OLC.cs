using Etrea3.Core;
using System.Data.SQLite;
using System.Text;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        public static void StartOLC(Session session)
        {
            if (!session.Player.IsImmortal)
            {
                Game.LogMessage($"WARN: Player {session.Player.Name} attempted to start OLC but they are not Immortal", LogLevel.Warning, true);
                return;
            }
            Game.LogMessage($"OLC: Player {session.Player.Name} has started OLC", LogLevel.OLC, true);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.AppendLine($"%BGT%Weldome to OLC {session.Player.Name}!%PT%");
                sb.AppendLine("OLC allows you to create, change and remove the fabric of the Realms");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Create{Constants.TabStop}2. Change");
                sb.AppendLine($"3. Remove{Constants.TabStop}4. Return");
                session.Send(sb.ToString());
                session.Send("Choice: ");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && int.TryParse(input.Trim(), out int option))
                {
                    switch (option)
                    {
                        case 1:
                            Create(session);
                            break;

                        case 2:
                            Change(session);
                            break;

                        case 3:
                            Delete(session);
                            break;

                        case 4:
                            Game.LogMessage($"OLC: Player {session.Player.Name} has exited OLC", LogLevel.OLC, true);
                            return;

                        default:
                            session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                }
            }
        }

        private static void Create(Session session)
        {
            StringBuilder sb = new StringBuilder();
            while(true)
            {
                sb.Clear();
                sb.AppendLine($"%BGT%Create which type of asset?%PT%");
                sb.AppendLine($"1. Item{Constants.TabStop}{Constants.TabStop}2. Shop{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}3. NPC");
                sb.AppendLine($"4. Recipe{Constants.TabStop}5. Emote{Constants.TabStop}{Constants.TabStop}6. Quest");
                sb.AppendLine($"7. Room{Constants.TabStop}{Constants.TabStop}8. Zone{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}9. Spell");
                sb.AppendLine($"10. MobProg{Constants.TabStop}11. Resource Node{Constants.TabStop}12. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && int.TryParse(input.Trim(), out int option))
                {
                    switch (option)
                    {
                        case 1:
                            CreateItem(session);
                            break;

                        case 2:
                            CreateShop(session);
                            break;

                        case 3:
                            CreateNPC(session);
                            break;

                        case 4:
                            CreateRecipe(session);
                            break;

                        case 5:
                            CreateEmote(session);
                            break;

                        case 6:
                            CreateQuest(session);
                            break;

                        case 7:
                            CreateRoom(session);
                            break;

                        case 8:
                            CreateZone(session);
                            break;

                        case 9:
                            CreateSpell(session);
                            break;

                        case 10:
                            CreateMobProg(session);
                            break;

                        case 11:
                            CreateNode(session);
                            break;

                        case 12:
                            return;

                        default:
                            session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                }
            }
        }

        private static void Delete(Session session)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"%BGT%Remove which type of asset?%PT%");
                sb.AppendLine($"1. Item{Constants.TabStop}{Constants.TabStop}2. Shop{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}3. NPC");
                sb.AppendLine($"4. Recipe{Constants.TabStop}5. Emote{Constants.TabStop}{Constants.TabStop}6. Quest");
                sb.AppendLine($"7. Room{Constants.TabStop}{Constants.TabStop}8. Zone{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}9. Spell");
                sb.AppendLine($"10. MobProg{Constants.TabStop}11. Resource Node{Constants.TabStop}12. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && int.TryParse(input.Trim(), out int option))
                {
                    switch (option)
                    {
                        case 1:
                            DeleteItem(session);
                            break;

                        case 2:
                            DeleteShop(session);
                            break;

                        case 3:
                            DeleteNPC(session);
                            break;

                        case 4:
                            DeleteRecipe(session);
                            break;

                        case 5:
                            DeleteEmote(session);
                            break;

                        case 6:
                            DeleteQuest(session);
                            break;

                        case 7:
                            DeleteRoom(session);
                            break;

                        case 8:
                            DeleteZone(session);
                            break;

                        case 9:
                            DeleteSpell(session);
                            break;

                        case 10:
                            DeleteMobProg(session);
                            break;

                        case 11:
                            DeleteNode(session);
                            break;

                        case 12:
                            return;

                        default:
                            session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                }
            }
        }

        private static void Change(Session session)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"%BGT%Change which type of asset?%PT%");
                sb.AppendLine($"1. Item{Constants.TabStop}{Constants.TabStop}2. Shop{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}3. NPC");
                sb.AppendLine($"4. Recipe{Constants.TabStop}5. Emote{Constants.TabStop}{Constants.TabStop}6. Quest");
                sb.AppendLine($"7. Room{Constants.TabStop}{Constants.TabStop}8. Zone{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}9. Spell");
                sb.AppendLine($"10. MobProg{Constants.TabStop}11. Resource Node{Constants.TabStop}12. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && int.TryParse(input.Trim(), out int option))
                {
                    switch (option)
                    {
                        case 1:
                            ChangeItem(session);
                            break;

                        case 2:
                            ChangeShop(session);
                            break;

                        case 3:
                            ChangeNPC(session);
                            break;

                        case 4:
                            ChangeRecipe(session);
                            break;

                        case 5:
                            ChangeEmote(session);
                            break;

                        case 6:
                            ChangeQuest(session);
                            break;

                        case 7:
                            ChangeRoom(session);
                            break;

                        case 8:
                            ChangeZone(session);
                            break;

                        case 9:
                            ChangeSpell(session);
                            break;

                        case 10:
                            ChangeMobProg(session);
                            break;

                        case 11:
                            ChangeNode(session);
                            break;

                        case 12:
                            return;

                        default:
                            session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                            break;
                    }
                }
                else
                {
                    session.Send($"Sorry, that isn't a valid selection!{Constants.NewLine}");
                }
            }
        }
    }
}
