using Etrea3.Core;
using System;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateQuest(Session session)
        {
            Quest newQuest = new Quest();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Quest ID: {newQuest.ID}{Constants.TabStop}GUID: {newQuest.QuestGUID}");
                sb.AppendLine($"Name: {newQuest.Name}");
                sb.AppendLine($"Type: {newQuest.QuestType}{Constants.TabStop}Zone: {newQuest.Zone}");
                sb.AppendLine($"Reward Gold: {newQuest.RewardGold}{Constants.TabStop}Reward Exp: {newQuest.RewardExp}");
                sb.AppendLine($"Required Items: {newQuest.RequiredItems.Count}{Constants.TabStop}Required Monsters: {newQuest.RequiredMonsters.Count}");
                if (string.IsNullOrEmpty(newQuest.FlavourText))
                {
                    sb.AppendLine("Flavour Text: None");
                }
                else
                {
                    sb.AppendLine("Flavour Text");
                    foreach (var line in newQuest.FlavourText.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"  {line}");
                    }
                }
                sb.AppendLine($"Reward Items: {newQuest.RewardItems.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}3. Set Type");
                sb.AppendLine($"4. Set Zone{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}5. Set Gold{Constants.TabStop}{Constants.TabStop}6. Set Exp");
                sb.AppendLine($"7. Set Flavour Text{Constants.TabStop}{Constants.TabStop}8. Manage Required Items");
                sb.AppendLine($"9. Manage Required Monsters{Constants.TabStop}10. Manage Reward Items");
                sb.AppendLine($"11. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}12. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newQuest.ID = GetValue<int>(session, "Enter Quest ID: ");
                        break;

                    case 2:
                        newQuest.Name = GetValue<string>(session, "Enter Quest Name: ");
                        break;

                    case 3:
                        newQuest.QuestType = GetEnumValue<QuestType>(session, "Enter Quest Type: ");
                        break;

                    case 4:
                        newQuest.Zone = GetValue<int>(session, "Enter Quest Zone: ");
                        break;

                    case 5:
                        newQuest.RewardGold = GetValue<ulong>(session, "Enter Gold Reward: ");
                        break;

                    case 6:
                        newQuest.RewardExp = GetValue<uint>(session, "Enter Rewards Exp: ");
                        break;

                    case 7:
                        newQuest.FlavourText = ManageQuestFlavourText(session);
                        break;

                    case 8:
                        ManageQuestRequiredItems(session, ref newQuest);
                        break;

                    case 9:
                        ManageQuestRequiredMonsters(session, ref newQuest);
                        break;

                    case 10:
                        ManageQuestRewardItems(session, ref newQuest);
                        break;

                    case 11:
                        if (ValidateAsset(session, newQuest, true, out _))
                        {
                            if (QuestManager.Instance.AddOrUpdateQuest(newQuest, true))
                            {
                                session.SendSystem($"%BGT%The new Quest has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Quest: {newQuest.Name} ({newQuest.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The new Quest was not successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Quest: {newQuest.Name} ({newQuest.ID}) but the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Quest could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 12:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.TabStop}");
                        continue;
                }
            }
        }

        private static void DeleteQuest(Session session)
        {
            while (true)
            {
                session.SendSystem($"Enter Quest ID or END to return: ");
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int questID))
                {
                    session.SendSystem($"%BRT%That is not a valid Quest ID.%PT%{Constants.NewLine}");
                    continue;
                }
                var q = QuestManager.Instance.GetQuest(questID);
                if (q == null)
                {
                    session.SendSystem($"%BRT%No Quest with that ID could be found in Quest Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (q.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(q.LockHolder);
                    var msg = lockingSession != null ? $"%BRT%That Quest is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                        $"%BRT%That Quest is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.SendSystem(msg);
                    continue;
                }
                if (QuestManager.Instance.RemoveQuest(q.ID))
                {
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed Quest: {q.Name} ({q.ID})", LogLevel.OLC, true);
                    session.SendSystem($"%BGT%The specified Quest has been successfully removed.%PT%{Constants.NewLine}");
                    return;
                }
                else
                {
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Quest: {q.Name} ({q.ID}) but the attempt failed", LogLevel.OLC, true);
                    session.SendSystem($"%BRT%Failed to remove the specified Quest.%PT%{Constants.NewLine}");
                    continue;
                }
            }
        }

        private static void ChangeQuest(Session session)
        {
            session.SendSystem($"Enter Quest ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int questID))
            {
                session.SendSystem($"%BRT%That is not a valid Quest ID.%PT%{Constants.NewLine}");
                return;
            }
            var q = QuestManager.Instance.GetQuest(questID);
            if (q == null)
            {
                session.SendSystem($"%BRT%No Quest with that ID could be found in Quest Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (q.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(q.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Quest is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Quest is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            var modQuest = Helpers.Clone(QuestManager.Instance.GetQuest(questID));
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Quest ID: {modQuest.ID}{Constants.TabStop}GUID: {modQuest.QuestGUID}");
                sb.AppendLine($"Name: {modQuest.Name}");
                sb.AppendLine($"Type: {modQuest.QuestType}{Constants.TabStop}Zone: {modQuest.Zone}");
                sb.AppendLine($"Reward Gold: {modQuest.RewardGold}{Constants.TabStop}Reward Exp: {modQuest.RewardExp}");
                sb.AppendLine($"Required Items: {modQuest.RequiredItems.Count}{Constants.TabStop}Required Monsters: {modQuest.RequiredMonsters.Count}");
                if (string.IsNullOrEmpty(modQuest.FlavourText))
                {
                    sb.AppendLine("Flavour Text: None");
                }
                else
                {
                    sb.AppendLine("Flavour Text");
                    foreach (var line in modQuest.FlavourText.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"  {line}");
                    }
                }
                sb.AppendLine($"Reward Items: {modQuest.RewardItems.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}{Constants.TabStop}2. Set Type");
                sb.AppendLine($"3. Set Zone{Constants.TabStop}{Constants.TabStop}4. Set Gold{Constants.TabStop}{Constants.TabStop}5. Set Exp");
                sb.AppendLine($"6. Set Flavour Text{Constants.TabStop}7. Manage Required Items");
                sb.AppendLine($"8. Manage Required Monsters{Constants.TabStop}9. Manage Reward Items");
                sb.AppendLine($"10. Save{Constants.TabStop}{Constants.TabStop}11. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modQuest.Name = GetValue<string>(session, "Enter Quest Name: ");
                        break;

                    case 2:
                        modQuest.QuestType = GetEnumValue<QuestType>(session, "Enter Quest Type: ");
                        break;

                    case 3:
                        modQuest.Zone = GetValue<int>(session, "Enter Quest Zone: ");
                        break;

                    case 4:
                        modQuest.RewardGold = GetValue<ulong>(session, "Enter Gold Reward: ");
                        break;

                    case 5:
                        modQuest.RewardExp = GetValue<uint>(session, "Enter Rewards Exp: ");
                        break;

                    case 6:
                        modQuest.FlavourText = ManageQuestFlavourText(session);
                        break;

                    case 7:
                        ManageQuestRequiredItems(session, ref modQuest);
                        break;

                    case 8:
                        ManageQuestRequiredMonsters(session, ref modQuest);
                        break;

                    case 9:
                        ManageQuestRewardItems(session, ref modQuest);
                        break;

                    case 10:
                        if (ValidateAsset(session, modQuest, false, out _))
                        {
                            if (QuestManager.Instance.AddOrUpdateQuest(modQuest, false))
                            {
                                session.SendSystem($"%BGT%The Quest has been updated successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Quest: {modQuest.Name} ({modQuest.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The Quest was not successfully updated.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Quest: {modQuest.Name} ({modQuest.ID}) but the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Quest could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 11:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.TabStop}");
                        continue;
                }
            }
        }

        private static string ManageQuestFlavourText(Session session)
        {
            StringBuilder sb = new StringBuilder();
            int line = 1;
            session.SendSystem($"%BGT%Enter Quest flavour text, try to keep each line to a max of 80 characters.%PT%{Constants.NewLine}");
            session.SendSystem($"%BGT%Enter END on a new line to finish.%PT%{Constants.NewLine}");
            session.SendSystem($"%BYT%{new string('=', 77)}%PT%{Constants.NewLine}");
            while (true)
            {
                session.SendSystem($"[{line}] ");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && input.Trim().ToUpper() == "END")
                {
                    break;
                }
                sb.AppendLine(input.Trim());
                line++;
            }
            return sb.ToString();
        }

        private static void ManageQuestRequiredItems(Session session, ref Quest quest)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (quest.RequiredItems.Count > 0)
                {
                    sb.AppendLine("Required Items:");
                    foreach(var i in quest.RequiredItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Items: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        var itemID = GetValue<int>(session, "Enter Item ID: ");
                        InventoryItem item = ItemManager.Instance.GetItem(itemID);
                        if (item != null)
                        {
                            quest.RequiredItems.AddOrUpdate(item.ID, 1, (k, v) => v + 1);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (quest.RequiredItems.ContainsKey(itemID))
                        {
                            int n = quest.RequiredItems[itemID];
                            if (n - 1 == 0)
                            {
                                quest.RequiredItems.TryRemove(itemID, out _);
                            }
                            else
                            {
                                quest.RequiredItems[itemID]--;
                            }
                        }
                        break;

                    case 3:
                        quest.RequiredItems.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageQuestRequiredMonsters(Session session, ref Quest quest)
        {
            StringBuilder sb = new StringBuilder();
            while(true)
            {
                sb.Clear();
                if (quest.RequiredMonsters.Count > 0)
                {
                    sb.AppendLine("Required Monsters:");
                    foreach(var m in quest.RequiredMonsters)
                    {
                        var monster = NPCManager.Instance.GetNPC(m.Key);
                        if (monster != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{m.Value} x {monster.Name} ({monster.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{m.Value} x Unknown Monster ({m.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Monsters: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Monster{Constants.TabStop}{Constants.TabStop}2. Remove Monster");
                sb.AppendLine($"3. Clear Monsters{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        var mobID = GetValue<int>(session, "Enter Monster ID: ");
                        var mob = NPCManager.Instance.GetNPC(mobID);
                        if (mob != null)
                        {
                            quest.RequiredMonsters.AddOrUpdate(mob.TemplateID, 1, (k, v) => v + 1);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No NPC with that ID could be found in NPC Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        mobID = GetValue<int>(session, "Enter Monster ID: ");
                        if (quest.RequiredMonsters.ContainsKey(mobID))
                        {
                            var n = quest.RequiredMonsters[mobID];
                            if (n - 1 == 0)
                            {
                                quest.RequiredMonsters.TryRemove(mobID, out _);
                            }
                            else
                            {
                                quest.RequiredMonsters[mobID]--;
                            }
                        }
                        break;

                    case 3:
                        quest.RequiredMonsters.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageQuestRewardItems(Session session, ref Quest quest)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (quest.RewardItems.Count > 0)
                {
                    foreach(var i in quest.RewardItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Reward Items: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        var itemID = GetValue<int>(session, "Enter Item ID: ");
                        InventoryItem item = ItemManager.Instance.GetItem(itemID);
                        if (item != null)
                        {
                            quest.RewardItems.AddOrUpdate(item.ID, 1, (k, v) => v + 1);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (quest.RewardItems.ContainsKey(itemID))
                        {
                            var n = quest.RewardItems[itemID];
                            if (n - 1 == 0)
                            {
                                quest.RewardItems.TryRemove(itemID, out _);
                            }
                            else
                            {
                                quest.RewardItems[itemID]--;
                            }
                        }
                        break;

                    case 3:
                        quest.RewardItems.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}