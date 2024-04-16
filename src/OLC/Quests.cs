using Etrea2.Core;
using Etrea2.Entities;
using System.Linq;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create
        private static void CreateNewQuest(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("A Quest is a specific objective for a player to complete that grants rewards in the form");
            sb.AppendLine("of gold, experience or items.");
            desc.Send(sb.ToString());
            Quest q = new Quest();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Quest ID: {q.QuestID}{Constants.TabStop}Quest GUID: {q.QuestGUID}");
                sb.AppendLine($"Quest Name: {q.QuestName}");
                sb.AppendLine($"Quest Text: {q.QuestText}");
                sb.AppendLine($"Quest Zone: {q.QuestZone}{Constants.TabStop}Quest Type: {q.QuestType}");
                if (q.FetchItems != null && q.FetchItems.Count > 0)
                {
                    sb.AppendLine($"Required Items:");
                    foreach (var item in q.FetchItems)
                    {
                        sb.AppendLine($"{item.Value} x {ItemManager.Instance.GetItemByID(item.Key).Name}");
                    }
                }
                else
                {
                    sb.AppendLine($"Required Item: None");
                }
                if (q.Monsters != null && q.Monsters.Count > 0)
                {
                    sb.AppendLine($"Required Monsters:");
                    foreach (var m in q.Monsters)
                    {
                        sb.AppendLine($"{m.Value} x {NPCManager.Instance.GetNPCByID(m.Key).Name}");
                    }
                }
                else
                {
                    sb.AppendLine($"Required Monsters: None");
                }
                sb.AppendLine($"Reward Gold: {q.RewardGold}{Constants.TabStop}Reward Exp: {q.RewardExp}");
                if (q.RewardItems != null && q.RewardItems.Count > 0)
                {
                    sb.AppendLine($"Reward Items:");
                    foreach (var i in q.RewardItems)
                    {
                        sb.AppendLine($"{i.Name}");
                    }
                }
                else
                {
                    sb.AppendLine($"Reward Item: None");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Quest ID{Constants.TabStop}{Constants.TabStop}2. Set Quest Zone");
                sb.AppendLine($"3. Set Quest Name{Constants.TabStop}4. Set Quest Text");
                sb.AppendLine($"5. Set Quest Type{Constants.TabStop}6. Add Required Item{Constants.TabStop}7. Remove Required Item");
                sb.AppendLine($"8. Add Required Monster{Constants.TabStop}9. Remove Required Monster");
                sb.AppendLine($"10. Set Reward Gold{Constants.TabStop}11. Set Reward Exp");
                sb.AppendLine($"12. Add Reward Item{Constants.TabStop}13. Remove Reward Item");
                sb.AppendLine($"14. Save{Constants.TabStop}15. Exit");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 15)
                    {
                        switch (option)
                        {
                            case 1:
                                q.QuestID = GetAssetUintValue(ref desc, "Enter Quest ID: ");
                                break;

                            case 2:
                                q.QuestZone = GetAssetUintValue(ref desc, "Enter Quest Zone: ");
                                break;

                            case 3:
                                q.QuestName = GetAssetStringValue(ref desc, "Enter Quest Name: ");
                                break;

                            case 4:
                                q.QuestText = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                q.QuestType = GetAssetEnumValue<QuestType>(ref desc, "Enter Quest Type: ");
                                break;

                            case 6:
                                var itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                var item = ItemManager.Instance.GetItemByID(itemId);
                                if (item != null)
                                {
                                    if (q.FetchItems.ContainsKey(itemId))
                                    {
                                        q.FetchItems[itemId]++;
                                    }
                                    else
                                    {
                                        q.FetchItems.Add(item.ID, 1);
                                    }
                                }
                                break;

                            case 7:
                                itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                if (q.FetchItems.ContainsKey(itemId))
                                {
                                    if (q.FetchItems[itemId] - 1 == 0)
                                    {
                                        q.FetchItems.Remove(itemId);
                                    }
                                    else
                                    {
                                        q.FetchItems[itemId]--;
                                    }
                                }
                                break;

                            case 8:
                                var monsterID = GetAssetUintValue(ref desc, "Enter Monster ID: ");
                                var monster = NPCManager.Instance.GetNPCByID(monsterID);
                                if (monster != null)
                                {
                                    if (q.Monsters.ContainsKey(monsterID))
                                    {
                                        q.Monsters[monsterID]++;
                                    }
                                    else
                                    {
                                        q.Monsters.Add(monsterID, 1);
                                    }
                                }
                                break;

                            case 9:
                                monsterID = GetAssetUintValue(ref desc, "Enter Monster ID: ");
                                if (q.Monsters.ContainsKey(monsterID))
                                {
                                    if (q.Monsters[monsterID] - 1 == 0)
                                    {
                                        q.Monsters.Remove(monsterID);
                                    }
                                    else
                                    {
                                        q.Monsters[monsterID]--;
                                    }
                                }
                                break;

                            case 10:
                                q.RewardGold = GetAssetUintValue(ref desc, "Enter Gold Reward: ");
                                break;

                            case 11:
                                q.RewardExp = GetAssetUintValue(ref desc, "Enter Exp Reward: ");
                                break;

                            case 12:
                                itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                item = ItemManager.Instance.GetItemByID(itemId);
                                if (item != null)
                                {
                                    q.RewardItems.Add(item);
                                }
                                break;

                            case 13:
                                itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                item = q.RewardItems.Where(x => x.ID == itemId).FirstOrDefault();
                                if (item != null)
                                {
                                    q.RewardItems.Remove(item);
                                }
                                break;

                            case 14:
                                if (ValidateQuestObject(ref desc, ref q, true))
                                {
                                    if (DatabaseManager.AddQuest(ref desc, q))
                                    {
                                        if (QuestManager.Instance.AddQuest(ref desc, q))
                                        {
                                            desc.Send($"New Quest created successfully.{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add Quest to QuestManager, it may not be available until restart.{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to add Quest to the World database.{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 15:
                                okToReturn = true;
                                break;
                        }
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Edit
        private static void EditExistingQuest(ref Descriptor desc)
        {
            bool okToReturn = false;
            desc.Send($"Enter ID of Quest to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint questID))
            {
                if (QuestManager.Instance.QuestExists(questID))
                {
                    var q = QuestManager.Instance.GetQuest(questID).ShallowCopy();
                    StringBuilder sb = new StringBuilder();
                    while (!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Quest ID: {q.QuestID}{Constants.TabStop}Quest GUID: {q.QuestGUID}");
                        sb.AppendLine($"Quest Name: {q.QuestName}");
                        sb.AppendLine($"Quest Text: {q.QuestText}");
                        sb.AppendLine($"Quest Zone: {q.QuestZone}{Constants.TabStop}Quest Type: {q.QuestType}");
                        if (q.FetchItems != null && q.FetchItems.Count > 0)
                        {
                            sb.AppendLine($"Required Items:");
                            foreach (var item in q.FetchItems)
                            {
                                sb.AppendLine($"{item.Value} x {ItemManager.Instance.GetItemByID(item.Key).Name}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"Required Item: None");
                        }
                        if (q.Monsters != null && q.Monsters.Count > 0)
                        {
                            sb.AppendLine($"Required Monsters:");
                            foreach (var m in q.Monsters)
                            {
                                sb.AppendLine($"{m.Value} x {NPCManager.Instance.GetNPCByID(m.Key).Name}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"Required Monsters: None");
                        }
                        sb.AppendLine($"Reward Gold: {q.RewardGold}{Constants.TabStop}Reward Exp: {q.RewardExp}");
                        if (q.RewardItems != null && q.RewardItems.Count > 0)
                        {
                            sb.AppendLine($"Reward Items:");
                            foreach (var i in q.RewardItems)
                            {
                                sb.AppendLine($"{i.Name}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"Reward Item: None");
                        }
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine($"1. Set Quest Zone{Constants.TabStop}2. Set Quest Name");
                        sb.AppendLine($"3. Set Quest Text{Constants.TabStop}4. Set Quest Type");
                        sb.AppendLine($"5. Add Required Item{Constants.TabStop}6. Remove Required Item");
                        sb.AppendLine($"7. Add Required Monster{Constants.TabStop}8. Remove Required Monster");
                        sb.AppendLine($"9. Set Reward Gold{Constants.TabStop}10. Set Reward Exp");
                        sb.AppendLine($"11. Add Reward Item{Constants.TabStop}12. Remove Reward Item");
                        sb.AppendLine($"13. Save{Constants.TabStop}14. Exit");
                        desc.Send(sb.ToString());
                        input = desc.Read().Trim();
                        if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                        {
                            if (option >= 1 && option <= 14)
                            {
                                switch (option)
                                {
                                    case 1:
                                        q.QuestZone = GetAssetUintValue(ref desc, "Enter Quest Zone: ");
                                        break;

                                    case 2:
                                        q.QuestName = GetAssetStringValue(ref desc, "Enter Quest Name: ");
                                        break;

                                    case 3:
                                        q.QuestText = Helpers.GetLongDescription(ref desc);
                                        break;

                                    case 4:
                                        q.QuestType = GetAssetEnumValue<QuestType>(ref desc, "Enter Quest Type: ");
                                        break;

                                    case 5:
                                        var itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        var item = ItemManager.Instance.GetItemByID(itemId);
                                        if (item != null)
                                        {
                                            if (q.FetchItems.ContainsKey(itemId))
                                            {
                                                q.FetchItems[itemId]++;
                                            }
                                            else
                                            {
                                                q.FetchItems.Add(item.ID, 1);
                                            }
                                        }
                                        break;

                                    case 6:
                                        itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        if (q.FetchItems.ContainsKey(itemId))
                                        {
                                            if (q.FetchItems[itemId] - 1 == 0)
                                            {
                                                q.FetchItems.Remove(itemId);
                                            }
                                            else
                                            {
                                                q.FetchItems[itemId]--;
                                            }
                                        }
                                        break;

                                    case 7:
                                        var monsterID = GetAssetUintValue(ref desc, "Enter Monster ID: ");
                                        var monster = NPCManager.Instance.GetNPCByID(monsterID);
                                        if (monster != null)
                                        {
                                            if (q.Monsters.ContainsKey(monsterID))
                                            {
                                                q.Monsters[monsterID]++;
                                            }
                                            else
                                            {
                                                q.Monsters.Add(monsterID, 1);
                                            }
                                        }
                                        break;

                                    case 8:
                                        monsterID = GetAssetUintValue(ref desc, "Enter Monster ID: ");
                                        if (q.Monsters.ContainsKey(monsterID))
                                        {
                                            if (q.Monsters[monsterID] - 1 == 0)
                                            {
                                                q.Monsters.Remove(monsterID);
                                            }
                                            else
                                            {
                                                q.Monsters[monsterID]--;
                                            }
                                        }
                                        break;

                                    case 9:
                                        q.RewardGold = GetAssetUintValue(ref desc, "Enter Reward Gold: ");
                                        break;

                                    case 10:
                                        q.RewardExp = GetAssetUintValue(ref desc, "Enter Reward Exp: ");
                                        break;

                                    case 11:
                                        itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        item = ItemManager.Instance.GetItemByID(itemId);
                                        if (item != null)
                                        {
                                            q.RewardItems.Add(item);
                                        }
                                        break;

                                    case 12:
                                        itemId = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                        item = q.RewardItems.Where(x => x.ID == itemId).FirstOrDefault();
                                        if (item != null)
                                        {
                                            q.RewardItems.Remove(item);
                                        }
                                        break;

                                    case 13:
                                        if (ValidateQuestObject(ref desc, ref q, false))
                                        {
                                            if (DatabaseManager.UpdateQuest(ref desc, q))
                                            {
                                                if (QuestManager.Instance.UpdateQuest(ref desc, q))
                                                {
                                                    desc.Send($"Quest updated in World Database and QuestManager.{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update Quest in QuestManager, it may not be available until restart.{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update Quest in World Database.{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 14:
                                        okToReturn = true;
                                        break;
                                }
                            }
                            else
                            {
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"No Quest with that ID could be found.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Delete
        private static void DeleteQuest(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a database backup is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send("Enter the ID of the Quest to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint qid))
            {
                var q = QuestManager.Instance.GetQuest(qid);
                if (q != null)
                {
                    if (DatabaseManager.DeleteQuest(ref desc, q.QuestID))
                    {
                        if (QuestManager.Instance.RemoveQuest(ref desc, q.QuestGUID, q.QuestName))
                        {
                            desc.Send($"Quest successfully removed from QuestManager and World database.{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"Unable to remove Quest from QuestManager.{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Unable to remove Quest from World database.{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"No Quest matching that name or ID could be found.{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Validation Functions
        private static bool ValidateQuestObject(ref Descriptor desc, ref Quest q, bool isNewQuest)
        {
            if (string.IsNullOrEmpty(q.QuestText) || string.IsNullOrEmpty(q.QuestName))
            {
                desc.Send($"A Quest must have a Name and appropriate supporting text.{Constants.NewLine}");
                return false;
            }
            if (q.QuestType == QuestType.Fetch && (q.FetchItems == null || q.FetchItems.Count == 0))
            {
                desc.Send($"A Fetch Quest must have at least one Item to be returned.{Constants.NewLine}");
                return false;
            }
            if (q.QuestType == QuestType.Kill && (q.Monsters == null || q.Monsters.Count == 0))
            {
                desc.Send($"A Kill Quest must have at least one Monster to kill.{Constants.NewLine}");
                return false;
            }
            if (q.QuestZone == 0)
            {
                desc.Send($"A Quest must have an appropriate Zone.{Constants.NewLine}");
                return false;
            }
            if (isNewQuest)
            {
                if (DatabaseManager.IsQuestIDInUse(ref desc, q.QuestID))
                {
                    desc.Send($"The specified Quest ID is already in use.{Constants.NewLine}");
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}