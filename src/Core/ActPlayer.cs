using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal static partial class CommandParser
    {
        // TODO: Update parsing code: instead of replacing the verb with an empty string, get verb and remove verb.length from the start of the string then trim()
        //       See PlayerQuests() for example code

        private static void MovePlayer(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim().ToLower();
            string direction = string.Empty;
            if(verb.Length <= 2)
            {
                switch(verb)
                {
                    case "s":
                        direction = "south";
                        break;
                    case "n":
                        direction = "north";
                        break;
                    case "w":
                        direction = "west";
                        break;
                    case "e":
                        direction = "east";
                        break;
                    case "ne":
                        direction = "northeast";
                        break;
                    case "se":
                        direction = "southeast";
                        break;
                    case "sw":
                        direction = "southwest";
                        break;
                    case "nw":
                        direction = "northwest";
                        break;
                    case "d":
                        direction = "down";
                        break;
                    case "u":
                        direction = "up";
                        break;
                }
            }
            else
            {
                direction = verb;
            }
            if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion(direction))
            {
                if(desc.Player.Position == ActorPosition.Standing)
                {
                    var s = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RequiredSkill;
                    if(s == null || desc.Player.HasSkill(s.Name))
                    {
                        var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor;
                        if (d == null || (d != null && d.IsOpen))
                        {
                            desc.Player.Move(desc.Player.CurrentRoom, RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).DestinationRoomID, false, ref desc);
                        }
                        else
                        {
                            desc.Send($"A doorway blocks your path in that direction!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You need the {s.Name} skill to go that way!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You are {desc.Player.Position.ToString().ToLower()}, and don't feel like moving right now...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You cannot go that way!{Constants.NewLine}");
            }
        }

        private static void OpenOrCloseDoor(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim().ToLower();
            string direction = input.Remove(0, verb.Length).Trim().ToLower();
            if (direction.Length <= 2)
            {
                switch (direction)
                {
                    case "s":
                        direction = "south";
                        break;
                    case "n":
                        direction = "north";
                        break;
                    case "w":
                        direction = "west";
                        break;
                    case "e":
                        direction = "east";
                        break;
                    case "ne":
                        direction = "northeast";
                        break;
                    case "se":
                        direction = "southeast";
                        break;
                    case "sw":
                        direction = "southwest";
                        break;
                    case "nw":
                        direction = "northwest";
                        break;
                    case "d":
                        direction = "down";
                        break;
                    case "u":
                        direction = "up";
                        break;
                }
            }
            if(!string.IsNullOrEmpty(direction))
            {
                var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
                if(r.HasExitInDiretion(direction))
                {
                    var d = r.GetRoomExit(direction).RoomDoor;
                    if(d != null)
                    {
                        if(verb.ToLower() == "open")
                        {
                            if(d.IsLocked)
                            {
                                desc.Send($"The door is locked!{Constants.NewLine}");
                            }
                            else
                            {
                                RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsOpen = true;
                                desc.Send($"You open the door {direction}.{Constants.NewLine}");
                                var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                if(localPlayers != null && localPlayers.Count > 1)
                                {
                                    var pn = desc.Player.Name;
                                    foreach(var p in localPlayers.Where(x => x.Player.Name != pn))
                                    {
                                        if(desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                        {
                                            p.Send($"{pn} opens the door {direction}.{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            p.Send($"Something opens the door {direction}.{Constants.NewLine}");
                                        }
                                    }
                                }
                            }
                        }
                        if(verb.ToLower() == "close")
                        {
                            RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsOpen = false;
                            desc.Send($"You close the door {direction}.{Constants.NewLine}");
                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                            if (localPlayers != null && localPlayers.Count > 1)
                            {
                                var pn = desc.Player.Name;
                                foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                {
                                    if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                    {
                                        p.Send($"{pn} closes the door {direction}.{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        p.Send($"Something closes the door {direction}.{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"There is no door on the exit {direction}...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"There is no exit {direction}...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void LockOrUnlockDoor(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim().ToLower();
            string direction = input.Remove(0, verb.Length).Trim().ToLower();
            if (direction.Length <= 2)
            {
                switch (direction)
                {
                    case "s":
                        direction = "south";
                        break;
                    case "n":
                        direction = "north";
                        break;
                    case "w":
                        direction = "west";
                        break;
                    case "e":
                        direction = "east";
                        break;
                    case "ne":
                        direction = "northeast";
                        break;
                    case "se":
                        direction = "southeast";
                        break;
                    case "sw":
                        direction = "southwest";
                        break;
                    case "nw":
                        direction = "northwest";
                        break;
                    case "d":
                        direction = "down";
                        break;
                    case "u":
                        direction = "up";
                        break;
                }
            }
            if (!string.IsNullOrEmpty(direction))
            {
                var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
                if(r.HasExitInDiretion(direction))
                {
                    var d = r.GetRoomExit(direction).RoomDoor;
                    if(d != null)
                    {
                        if(verb.ToLower() == "lock")
                        {
                            if(!d.IsOpen)
                            {
                                if(!d.IsLocked)
                                {
                                    if (d.RequiredItemID == 0 || desc.Player.HasItemInInventory(d.RequiredItemID))
                                    {
                                        RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsLocked = true;
                                        desc.Send($"You lock the door {direction}.{Constants.NewLine}");
                                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                        if (localPlayers != null && localPlayers.Count > 1)
                                        {
                                            var pn = desc.Player.Name;
                                            foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                            {
                                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                                {
                                                    p.Send($"{pn} locks the door {direction}.{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    p.Send($"Something locks the door {direction}.{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"You lack the correct key to lock the door...{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"The door is already locked...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"The door must be shut first...{Constants.NewLine}");
                            }
                        }
                        if(verb.ToLower() == "unlock")
                        {
                            if(d.IsOpen)
                            {
                                desc.Send($"The door is already open...{Constants.NewLine}");
                            }
                            else
                            {
                                if(!d.IsLocked)
                                {
                                    desc.Send($"The door is already unlocked...{Constants.NewLine}");
                                }
                                else
                                {
                                    if(d.RequiredItemID == 0 || desc.Player.HasItemInInventory(d.RequiredItemID))
                                    {
                                        RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit(direction).RoomDoor.IsLocked = false;
                                        desc.Send($"You unlock the door {direction}.{Constants.NewLine}");
                                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                        if (localPlayers != null && localPlayers.Count > 1)
                                        {
                                            var pn = desc.Player.Name;
                                            foreach (var p in localPlayers.Where(x => x.Player.Name != pn))
                                            {
                                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                                {
                                                    p.Send($"{pn} unlocks the door {direction}.{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    p.Send($"Something unlocks the door {direction}.{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"You lack the correct key to unlock the door...{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"There is no exit to the {direction}...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void PlayerQuests(ref Descriptor desc, ref string input)
        {
            // usage quest(s) - show current active quests
            // quest(s) <list | accept | abandon | return>
            var verb = GetVerb(ref input).Trim();
            var line = input.Remove(0, verb.Length).Trim();
            var elements = TokeniseInput(ref line);
            var operation = elements.Length >= 1 ? elements[0].ToLower().Trim() : string.Empty;
            if(!string.IsNullOrEmpty(operation))
            {
                var questList = QuestManager.Instance.GetQuestsForZone(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ZoneID);
                switch (operation)
                {
                    case "list":
                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                        {
                            var completedQuestIds = desc.Player.CompletedQuests;
                            var zoneQuests = new List<Quest>();
                            if (questList != null && questList.Count > 0)
                            {
                                foreach (var q in questList)
                                {
                                    if (!completedQuestIds.Contains(q.QuestGUID))
                                    {
                                        zoneQuests.Add(q);
                                    }
                                }
                            }
                            if (zoneQuests != null && zoneQuests.Count > 0)
                            {
                                int i = 0, n = 1;
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"  {new string('=', 77)}");
                                foreach (var q in zoneQuests)
                                {
                                    sb.AppendLine($"|| ID: {n}");
                                    sb.AppendLine($"|| Name: {q.QuestName}{Constants.TabStop}{Constants.TabStop}Quest Type: {q.QuestType}");
                                    sb.AppendLine($"|| Description:");
                                    var descLines = q.QuestText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                                    foreach (var l in descLines)
                                    {
                                        sb.AppendLine($"|| {l}");
                                    }
                                    if (q.Monsters != null && q.Monsters.Count > 0)
                                    {
                                        sb.AppendLine($"|| Kill Monsters:");
                                        foreach (var m in q.Monsters)
                                        {
                                            sb.AppendLine($"|| {m.Value} x {NPCManager.Instance.GetNPCByID(m.Key).Name}");
                                        }
                                    }
                                    if (q.FetchItems != null && q.FetchItems.Count > 0)
                                    {
                                        sb.AppendLine($"|| Obtain Items:");
                                        foreach (var item in q.FetchItems)
                                        {
                                            sb.AppendLine($"|| {item.Value} x {ItemManager.Instance.GetItemByID(item.Key).Name}");
                                        }
                                    }
                                    sb.AppendLine($"|| Gold: {q.RewardGold}{Constants.TabStop}Exp: {q.RewardExp}");
                                    if (q.RewardItems != null && q.RewardItems.Count > 0)
                                    {
                                        sb.AppendLine($"|| Reward Items:");
                                        foreach (var ri in q.RewardItems)
                                        {
                                            sb.AppendLine($"|| {ri.Name}");
                                        }
                                    }
                                    i++;
                                    n++;
                                    if (i < zoneQuests.Count)
                                    {
                                        sb.AppendLine($"||{new string('=', 77)}");
                                    }
                                }
                                sb.AppendLine($"  {new string('=', 77)}");
                                desc.Send(sb.ToString());
                            }
                            else
                            {
                                desc.Send($"There are no Quests available at the moment.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"There is no Questmaster here!{Constants.NewLine}");
                        }
                        break;

                    case "accept":
                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                        {
                            var iIndex = elements.Last();
                            if (int.TryParse(iIndex, out int qID))
                            {
                                if (qID > 0 && qID <= questList.Count)
                                {
                                    var q = questList[qID - 1];
                                    if (!desc.Player.ActiveQuests.Any(x => x.QuestGUID == q.QuestGUID))
                                    {
                                        desc.Player.ActiveQuests.Add(q);
                                        desc.Send($"You have accepted the quest!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"You have already accepted that Quest!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Sorry, that Quest doesn't seem to exist...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            }
                        }
                        break;

                    case "return":
                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.QuestMaster))
                        {
                            var iIndex = elements.Last();
                            if (int.TryParse(iIndex, out int qID))
                            {
                                if (qID > 0 && qID <= desc.Player.ActiveQuests.Count)
                                {
                                    var q = desc.Player.ActiveQuests[qID - 1];
                                    bool isComplete = true;
                                    if (q.Monsters != null && q.Monsters.Count > 0)
                                    {
                                        if (q.Monsters.Values.Any(x => x > 0))
                                        {
                                            isComplete = false;
                                        }
                                    }
                                    if (q.FetchItems != null && q.FetchItems.Count > 0)
                                    {
                                        foreach (var i in q.FetchItems)
                                        {
                                            if (desc.Player.Inventory.Where(x => x.Id == i.Key).Count() < i.Value)
                                            {
                                                isComplete = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (isComplete)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        desc.Player.CompletedQuests.Add(q.QuestGUID);
                                        desc.Player.ActiveQuests.Remove(q);
                                        desc.Player.AddExp(q.RewardExp, ref desc);
                                        desc.Player.AddGold(q.RewardGold, ref desc);
                                        if(q.FetchItems != null && q.FetchItems.Count > 0)
                                        {
                                            foreach(var i in q.FetchItems)
                                            {
                                                var item = desc.Player.Inventory.Where(x => x.Id == i.Key).FirstOrDefault();
                                                for(int n = 0; n < i.Value; n++)
                                                {
                                                    desc.Player.Inventory.Remove(item);
                                                }
                                            }
                                        }
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        sb.AppendLine($"|| Quest Completed!");
                                        sb.AppendLine($"|| Exp: {q.RewardExp}");
                                        sb.AppendLine($"|| Gold: {q.RewardGold}");
                                        if (q.RewardItems != null && q.RewardItems.Count > 0)
                                        {
                                            sb.AppendLine($"|| You are awarded the following items:");
                                            foreach (var i in q.RewardItems)
                                            {
                                                sb.AppendLine($"|| {i}");
                                                desc.Player.Inventory.Add(i);
                                            }
                                        }
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        desc.Send(sb.ToString());
                                    }
                                    else
                                    {
                                        desc.Send($"You have not met the requirements to complete this Quest!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Sorry, you don't seem to have that Quest active...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                            }
                        }
                        break;

                    case "abandon":
                        var inputIndex = elements.Last();
                        if (int.TryParse(inputIndex, out int qid))
                        {
                            if(qid > 0 && qid <= desc.Player.ActiveQuests.Count)
                            {
                                var q = desc.Player.ActiveQuests[qid - 1];
                                desc.Player.ActiveQuests.Remove(q);
                                desc.Send($"You have abandonned the Quest!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"Sorry, you don't seem to have that Quest active...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                        break;

                    default:
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        break;
                }
            }
            else
            {
                // no operation so show list of active quests the player has
                if(desc.Player.ActiveQuests != null && desc.Player.ActiveQuests.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"  {new string('=', 77)}");
                    int i = 0, n = 1;
                    foreach(var q in desc.Player.ActiveQuests)
                    {
                        sb.AppendLine($"|| Number: {n}");
                        sb.AppendLine($"|| Name: {q.QuestName}{Constants.TabStop}Zone: {ZoneManager.Instance.GetZone(q.QuestZone).ZoneName}");
                        if(q.Monsters != null && q.Monsters.Count > 0)
                        {
                            sb.AppendLine($"|| Kill Monsters:");
                            foreach(var m in q.Monsters)
                            {
                                sb.AppendLine($"|| {m.Value} x {NPCManager.Instance.GetNPCByID(m.Key).Name}");
                            }
                        }
                        if(q.FetchItems != null && q.FetchItems.Count > 0)
                        {
                            sb.AppendLine("|| Obtain Items:");
                            foreach(var item in q.FetchItems)
                            {
                                sb.AppendLine($"|| {item.Value} x {ItemManager.Instance.GetItemByID(item.Key).Name}");
                            }
                        }
                        sb.AppendLine($"|| Gold: {q.RewardGold}{Constants.TabStop}Exp: {q.RewardExp}");
                        if(q.RewardItems != null && q.RewardItems.Count > 0)
                        {
                            sb.AppendLine($"|| Items:");
                            foreach(var item in q.RewardItems)
                            {
                                sb.AppendLine($"|| {item.Name}");
                            }
                        }
                        i++;
                        n++;
                        if(i < desc.Player.ActiveQuests.Count)
                        {
                            sb.AppendLine($"||{new string('=', 77)}");
                        }
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    desc.Send($"You have no active Quests at the moment.{Constants.NewLine}");
                }
            }
        }


        private static void PlayerMail(ref Descriptor desc, ref string input)
        {
            if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.PostBox))
            {
                // usage: mail <list | read | write | delete>
                var elements = TokeniseInput(ref input);
                string operation = string.Empty;
                uint id = 0;
                if(elements.Length > 1)
                {
                    operation = elements[1].Trim();
                }
                if(elements.Length >= 3)
                {
                    if(!uint.TryParse(elements[2].Trim(), out id))
                    {
                        desc.Send($"The given ID could not be understood.{Constants.NewLine}");
                        return;
                    }
                }
                if (string.IsNullOrEmpty(operation))
                {
                    desc.Send($"Usage: mail <list | read | write | delete>{Constants.NewLine}");
                    desc.Send($"mail list - to list all your mails{Constants.NewLine}");
                    desc.Send($"mail read <id> - to read the specified mail{Constants.NewLine}");
                    desc.Send($"mail write - to start writing a mail to another player{Constants.NewLine}");
                    desc.Send($"mail delete <id> - to delete the specified mail{Constants.NewLine}");
                    return;
                }
                switch(operation.ToLower())
                {
                    case "list":
                        var allMails = DatabaseManager.GetAllPlayerMail(ref desc);
                        if(allMails != null && allMails.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"  {new string('=', 77)}");
                            int index = 0;
                            foreach(var m in allMails)
                            {
                                sb.AppendLine($"|| ID: {m.Key}{Constants.TabStop}{Constants.TabStop}From: {m.Value.MailFrom}{Constants.TabStop}Sent: {m.Value.MailSent}");
                                sb.AppendLine($"|| Subject: {m.Value.MailSubject}");
                                sb.AppendLine($"|| Attached Items: {m.Value.AttachedItems != null && m.Value.AttachedItems.Count > 0}");
                                sb.AppendLine($"|| Attached Gold: {m.Value.AttachedGold}");
                                index++;
                                if(index < allMails.Count)
                                {
                                    sb.AppendLine($"||{new string('=', 77)}");
                                }
                            }
                            sb.AppendLine($"  {new string('=', 77)}");
                            desc.Send(sb.ToString());
                        }
                        else
                        {
                            desc.Send($"It doesn't look like you have any mail...{Constants.NewLine}");
                        }
                        break;

                    case "read":
                        allMails = DatabaseManager.GetAllPlayerMail(ref desc);
                        var mailItem = allMails.ContainsKey(id) ? allMails[id] : null;
                        if(mailItem != null)
                        {
                            if(!mailItem.MailRead)
                            {
                                DatabaseManager.MarkMailAsRead(ref desc, mailItem.MailID);
                                if(mailItem.AttachedGold > 0)
                                {
                                    desc.Send($"You take {mailItem.AttachedGold} gold from the mail!{Constants.NewLine}");
                                    desc.Player.Stats.Gold += mailItem.AttachedGold;
                                }
                                if(mailItem.AttachedItems != null && mailItem.AttachedItems.Count > 0)
                                {
                                    foreach(var i in mailItem.AttachedItems)
                                    {
                                        desc.Player.Inventory.Add(i);
                                        desc.Send($"You take {i.Name} from the mail!{Constants.NewLine}");
                                    }
                                }
                            }
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"  {new string('=', 77)}");
                            sb.AppendLine($"|| From: {mailItem.MailFrom}{Constants.TabStop}{Constants.TabStop}Sent: {mailItem.MailSent}");
                            sb.AppendLine($"|| Subject: {mailItem.MailSubject}");
                            sb.AppendLine($"|| Message:");
                            if(!string.IsNullOrEmpty(mailItem.MailBody))
                            {
                                var bodyLines = mailItem.MailBody.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach(var line in bodyLines)
                                {
                                    sb.AppendLine($"||{Constants.TabStop}{line}");
                                }
                            }
                            sb.AppendLine($"  {new string('=', 77)}");
                            desc.Send(sb.ToString());
                        }
                        else
                        {
                            desc.Send($"Couldn't find a mail with that ID number.{Constants.NewLine}");
                        }
                        break;

                    case "write":
                        if(desc.Player.Stats.Gold < 5)
                        {
                            desc.Send($"It costs 5 gold to send a mail!{Constants.NewLine}");
                            return;
                        }
                        var newMail = Mail.Compose(ref desc);
                        if(newMail != null)
                        {
                            bool ok = false;
                            bool returnItems = false;
                            while(!ok)
                            {
                                desc.Send($"Send this mail (Y/N)?{Constants.NewLine}");
                                var response = desc.Read().Trim();
                                if(Helpers.ValidateInput(response))
                                {
                                    if(response.ToLower() == "y" || response.ToLower() == "yes")
                                    {
                                        // check we have 5 gold and send
                                        if(desc.Player.Stats.Gold >= 5)
                                        {
                                            newMail.MailSent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                                            if (DatabaseManager.SendNewMail(ref desc, ref newMail))
                                            {
                                                desc.Player.Stats.Gold -= 5;
                                                desc.Send($"Your mail has been sent successfully!{Constants.NewLine}");
                                                ok = true;
                                            }
                                            else
                                            {
                                                desc.Send($"There was a problem sending the mail, please check with an Imm.{Constants.NewLine}");
                                                returnItems = true;
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"You don't have enough gold to send a mail!{Constants.NewLine}");
                                            returnItems = true;
                                        }
                                    }
                                    if(response.ToLower() == "n" || response.ToLower() == "no")
                                    {
                                        returnItems = true;
                                    }
                                    if(returnItems)
                                    {
                                        // give the player back any attached items and gold
                                        if(newMail.AttachedGold > 0)
                                        {
                                            desc.Player.Stats.Gold += newMail.AttachedGold;
                                            desc.Send($"{newMail.AttachedGold} gold has been returned to you.{Constants.NewLine}");
                                        }
                                        if(newMail.AttachedItems != null && newMail.AttachedItems.Count > 0)
                                        {
                                            foreach(var i in newMail.AttachedItems)
                                            {
                                                desc.Player.Inventory.Add(i);
                                                desc.Send($"{i.Name} has been returned to you.{Constants.NewLine}");
                                            }
                                        }
                                        ok = true;
                                    }
                                }
                                else
                                {
                                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                                }
                            }
                        }
                        break;

                    case "delete":
                        allMails = DatabaseManager.GetAllPlayerMail(ref desc);
                        mailItem = allMails.ContainsKey(id) ? allMails[id] : null;
                        if(mailItem != null)
                        {
                            if(DatabaseManager.DeleteMailByID(ref desc, mailItem.MailID))
                            {
                                desc.Send($"The mail has been deleted.{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"There was a problem deleting the mail, please check with an Imm.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"Couldn't find a mail with that ID number{Constants.NewLine}");
                        }
                        break;

                    default:
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        break;
                }
            }
            else
            {
                desc.Send($"There is no Mailbox here...{Constants.NewLine}");
            }
        }

        private static void ShowOrTogglePVPFlag(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim();
            var op = input.Remove(0, verb.Length).Trim();
            if(!string.IsNullOrEmpty(op))
            {
                if(op.ToLower() == "on")
                {
                    desc.Player.PVP = true;
                    desc.Send($"PVP flag set to ON, PVP is enabled.{Constants.NewLine}");
                }
                else
                {
                    desc.Player.PVP = false;
                    desc.Send($"PVP flag set to OFF, PVP is disabled.{Constants.NewLine}");
                }
            }
            else
            {
                if(desc.Player.PVP)
                {
                    desc.Send($"Your PVP flag is currently enabled!{Constants.NewLine}");
                }
                else
                {
                    desc.Send($"Your PVP flag is currently disabled!{Constants.NewLine}");
                }
            }
        }

        private static void ShowFollowerInfo(ref Descriptor desc, ref string input)
        {
            StringBuilder sb = new StringBuilder();
            var n = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
            if(n != null)
            {
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var elements = TokeniseInput(ref line);
                if (elements.Length >= 2)
                {
                    if (elements[0].ToLower() == "show" || elements[0].ToLower() == "use" || elements[0].ToLower() == "give" || elements[0].ToLower() == "remove")
                    {
                        switch (elements.First().ToLower())
                        {
                            case "show":
                                switch (elements[1].ToLower())
                                {
                                    case "stats":
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        sb.AppendLine($"|| Name: {n.Name}");
                                        sb.AppendLine($"||");
                                        sb.AppendLine($"|| Stats:");
                                        sb.AppendLine($"|| Strength: {n.Stats.Strength} ({ActorStats.CalculateAbilityModifier(n.Stats.Strength)}){Constants.TabStop}{Constants.TabStop}Dexterity: {n.Stats.Dexterity} ({ActorStats.CalculateAbilityModifier(n.Stats.Dexterity)})");
                                        sb.AppendLine($"|| Constitution: {n.Stats.Constitution} ({ActorStats.CalculateAbilityModifier(n.Stats.Constitution)}){Constants.TabStop}{Constants.TabStop}Intelligence: {n.Stats.Intelligence} ({ActorStats.CalculateAbilityModifier(n.Stats.Intelligence)})");
                                        sb.AppendLine($"|| Wisdom: {n.Stats.Wisdom} ({ActorStats.CalculateAbilityModifier(n.Stats.Wisdom)}){Constants.TabStop} {Constants.TabStop}Charisma: {n.Stats.Charisma} ({ActorStats.CalculateAbilityModifier(n.Stats.Charisma)})");
                                        sb.AppendLine($"|| HP: {n.NumberOfHitDice}d{n.SizeOfHitDice} ({n.Stats.CurrentHP}/{n.Stats.MaxHP})");
                                        sb.AppendLine($"|| MP: {n.NumberOfHitDice}d8 ({n.Stats.CurrentMP}/{n.Stats.MaxMP})");
                                        sb.AppendLine($"|| Armour Class: {n.Stats.ArmourClass}{Constants.TabStop}{Constants.TabStop}No. Of Attacks: {n.NumberOfAttacks}");
                                        sb.AppendLine($"  {new string('=', 77)}");
                                        desc.Send(sb.ToString());
                                        break;

                                    case "equip":
                                    case "eq":
                                    case "equipment":
                                        if (n.EquippedItems != null)
                                        {
                                            sb.AppendLine($"  {new string('=', 77)}");
                                            sb.AppendLine($"|| Head: {n.EquippedItems.Head?.Name}");
                                            sb.AppendLine($"|| Neck: {n.EquippedItems.Neck?.Name}");
                                            sb.AppendLine($"|| Armour: {n.EquippedItems.Armour?.Name}");
                                            sb.AppendLine($"|| Finger (L): {n.EquippedItems.FingerLeft?.Name}");
                                            sb.AppendLine($"|| Finger (R): {n.EquippedItems.FingerRight?.Name}");
                                            sb.AppendLine($"|| Weapon: {n.EquippedItems.Weapon?.Name}");
                                            sb.AppendLine($"|| Held: {n.EquippedItems.Held?.Name}");
                                            sb.AppendLine($"  {new string('=', 77)}");
                                            desc.Send(sb.ToString());
                                        }
                                        break;

                                    case "inv":
                                    case "inventory":
                                        if (n.Inventory != null && n.Inventory.Count > 0)
                                        {
                                            sb.AppendLine($"  {new string('=', 77)}");
                                            sb.AppendLine($"|| {n.Name} is carrying:");
                                            foreach (var i in n.Inventory.Select(x => new { x.Id, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                                            {
                                                var cnt = n.Inventory.Where(y => y.Id == i.Id).Count();
                                                sb.AppendLine($"|| {cnt} x {i.Name}, {i.ShortDescription}");
                                            }
                                            sb.AppendLine($"  {new string('=', 77)}");
                                            desc.Send(sb.ToString());
                                        }
                                        else
                                        {
                                            desc.Send($"Your follower is not carrying any items.{Constants.NewLine}");
                                        }
                                        break;
                                }
                                break;

                            case "use":
                                var itemStr = line.Replace("use", string.Empty).Trim();
                                if(!string.IsNullOrEmpty(itemStr))
                                {
                                    var invItem = n.Inventory.Where(x => Regex.Match(x.Name, itemStr, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                                    if(invItem != null)
                                    {
                                        if (invItem.Slot != WearSlot.None)
                                        {
                                            switch (invItem.Slot)
                                            {
                                                case WearSlot.Head:
                                                    if(n.EquippedItems.Head == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquippedItems.Head = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wearing an item on their head!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.Neck:
                                                    if (n.EquippedItems.Neck == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquippedItems.Neck = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wearing an item on their neck!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.Weapon:
                                                    if (n.EquippedItems.Weapon == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquippedItems.Weapon = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wielding a weapon!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.Armour:
                                                    if (n.EquippedItems.Armour == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquippedItems.Armour = invItem;
                                                        n.CalculateArmourClass();
                                                        desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already wearing something as their armour!{Constants.NewLine}");
                                                    }
                                                    break;

                                                case WearSlot.FingerLeft:
                                                case WearSlot.FingerRight:
                                                    if(n.EquippedItems.FingerLeft == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquippedItems.FingerLeft = invItem;
                                                        n.CalculateArmourClass();
                                                        RoomManager.Instance.ProcessEnvironmentBuffs(n.CurrentRoom);
                                                        desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        if(n.EquippedItems.FingerRight == null)
                                                        {
                                                            n.Inventory.Remove(invItem);
                                                            n.EquippedItems.FingerRight = invItem;
                                                            n.CalculateArmourClass();
                                                            RoomManager.Instance.ProcessEnvironmentBuffs(n.CurrentRoom);
                                                            desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                        }
                                                        else
                                                        {
                                                            desc.Send($"Your follower is already wearing a ring on each hand!{Constants.NewLine}");
                                                        }
                                                    }
                                                    break;

                                                case WearSlot.Held:
                                                    if (n.EquippedItems.Held == null)
                                                    {
                                                        n.Inventory.Remove(invItem);
                                                        n.EquippedItems.Held = invItem;
                                                        n.CalculateArmourClass();
                                                        RoomManager.Instance.ProcessEnvironmentBuffs(n.CurrentRoom);
                                                        desc.Send($"{n.Name} start using {invItem.Name}.{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your follower is already holding something!{Constants.NewLine}");
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"Your follower can't use that item!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Your follower doesn't seem to be carrying that.{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Usage: follower use <item>{Constants.NewLine}");
                                }
                                break;

                            case "give":
                                itemStr = line.Replace("give", string.Empty).Trim();
                                if(!string.IsNullOrEmpty(itemStr))
                                {
                                    var tradeItem = n.Inventory.Where(x => Regex.Match(x.Name, itemStr, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                                    if(tradeItem != null)
                                    {
                                        n.Inventory.Remove(tradeItem);
                                        desc.Player.Inventory.Add(tradeItem);
                                        desc.Send($"{n.Name} gives you {tradeItem.Name}.{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"{n.Name} doesn't seem to be carrying that.{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Usage: follower give <item>{Constants.NewLine}");
                                }
                                break;

                            case "remove":
                                var slot = TokeniseInput(ref line).LastOrDefault();
                                if(!string.IsNullOrEmpty(slot))
                                {
                                    InventoryItem eqItem = null;
                                    switch(slot.Trim().ToLower())
                                    {
                                        case "head":
                                            eqItem = n.EquippedItems.Head;
                                            if(eqItem != null)
                                            {
                                                n.EquippedItems.Head = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything on their head!{Constants.NewLine}");
                                            }
                                            break;

                                        case "neck":
                                            eqItem = n.EquippedItems.Neck;
                                            if (eqItem != null)
                                            {
                                                n.EquippedItems.Neck = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything around their neck!{Constants.NewLine}");
                                            }
                                            break;

                                        case "armour":
                                            eqItem = n.EquippedItems.Armour;
                                            if (eqItem != null)
                                            {
                                                n.EquippedItems.Armour = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower isn't wearing any armour!{Constants.NewLine}");
                                            }
                                            break;

                                        case "weapon":
                                            eqItem = n.EquippedItems.Weapon;
                                            if (eqItem != null)
                                            {
                                                n.EquippedItems.Weapon = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower isn't using a weapon!{Constants.NewLine}");
                                            }
                                            break;

                                        case "held":
                                            eqItem = n.EquippedItems.Held;
                                            if (eqItem != null)
                                            {
                                                n.EquippedItems.Held = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower isn't holding anything!{Constants.NewLine}");
                                            }
                                            break;

                                        case "fingerleft":
                                            eqItem = n.EquippedItems.FingerLeft;
                                            if (eqItem != null)
                                            {
                                                n.EquippedItems.FingerLeft = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything on their fingers!{Constants.NewLine}");
                                            }
                                            break;

                                        case "fingerright":
                                            eqItem = n.EquippedItems.FingerRight;
                                            if (eqItem != null)
                                            {
                                                n.EquippedItems.FingerRight = null;
                                                n.Inventory.Add(eqItem);
                                                n.CalculateArmourClass();
                                                desc.Send($"{n.Name} stops using {eqItem.Name}{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"Your follower doesn't have anything on their fingers!{Constants.NewLine}");
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    desc.Send($"Remove an item from which equipment slot?{Constants.NewLine}");
                                }
                                break;
                        }
                    }
                    else
                    {
                        sb.Clear();
                        sb.AppendLine($"Usage: follower <show | use | give | remove> <command specific args>");
                        sb.AppendLine($"Usage: follower show <stats | equip | inv>: show follower stats, equipment or inventory");
                        sb.AppendLine($"Usage: follower use <item>: have your follower equip an item in their inventory");
                        sb.AppendLine($"Usage: follower give <item>: have your follower hand an item from their inventory to you");
                        sb.AppendLine($"Usage: follower remove <slot>: remove an equipped item back to the inventory");
                        desc.Send(sb.ToString());
                    }
                }
                else
                {
                    sb.Clear();
                    sb.AppendLine($"Usage: follower <show | use | give> <command specific args>");
                    sb.AppendLine($"Usage: follower show <stats | equip | inv>: show follower stats, equipment or inventory");
                    sb.AppendLine($"Usage: follower use <item>: have your follower equip an item in their inventory");
                    sb.AppendLine($"Usage: follower give <item>: have your follower hand an item from their inventory to you");
                    desc.Send(sb.ToString());
                }
            }
            else
            {
                desc.Send($"You don't have a follower at the moment.{Constants.NewLine}");
            }
        }

        private static void MineResourceNode(ref Descriptor desc, ref string input)
        {
            if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode != null)
            {
                var nodeName = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeName;
                if (desc.Player.HasSkill("Mining"))
                {
                    var i = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.Mine();
                    if(i != null)
                    {
                        desc.Player.Inventory.Add(i);
                        RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeDepth--;
                        desc.Send($"You mine the {nodeName} node and find {i.Name}!{Constants.NewLine}");
                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeDepth == 0)
                        {
                            RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode = null;
                        }
                    }
                }
                else
                {
                    desc.Send($"You lack the skills to do that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is nothing to mine here!{Constants.NewLine}");
            }
        }

        private static void CraftItem(ref Descriptor desc, ref string input)
        {
            var recipeName = input.Replace(GetVerb(ref input), string.Empty).Trim();
            if(!string.IsNullOrEmpty(recipeName))
            {
                var recipe = RecipeManager.Instance.GetRecipe(recipeName);
                if(recipe != null)
                {
                    if(desc.Player.KnowsRecipe(recipe.RecipieName))
                    {
                        bool canCraft = false;
                        switch(recipe.RecipeType)
                        {
                            case RecipeType.Jewelcrafting:
                                canCraft = desc.Player.HasSkill("Jewelcrafting");
                                break;

                            case RecipeType.Scribing:
                                canCraft = desc.Player.HasSkill("Scribing");
                                break;

                            case RecipeType.Blacksmithing:
                                canCraft = desc.Player.HasSkill("Blacksmithing");
                                break;

                            case RecipeType.Alchemy:
                                canCraft = desc.Player.HasSkill("Alchemy");
                                break;
                        }
                        if(canCraft)
                        {
                            bool hasMats = true;
                            foreach(var mat in recipe.RequiredMaterials)
                            {
                                if(desc.Player.HasItemInInventory(mat.Key))
                                {
                                    var cnt = Convert.ToUInt32((from i in desc.Player.Inventory where i.Id == mat.Key select i).Count());
                                    if(cnt < mat.Value)
                                    {
                                        hasMats = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    hasMats = false;
                                    break;
                                }
                            }
                            if(hasMats)
                            {
                                foreach(var mat in recipe.RequiredMaterials)
                                {
                                    for(int i = 0; i < mat.Value; i++)
                                    {
                                        var remItem = (from invItem in desc.Player.Inventory where invItem.Id == mat.Key select invItem).FirstOrDefault();
                                        desc.Player.Inventory.Remove(remItem);
                                    }
                                }
                                var item = ItemManager.Instance.GetItemByID(recipe.RecipeResult);
                                desc.Player.Inventory.Add(item);
                                desc.Send($"You have successfully crafted {item.Name}!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"You don't have all the materials needed to craft that.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"You don't have the skill to craft that!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You don't know the recipe for that!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"That Recipe doesn't seem to exist.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void HireFollower(ref Descriptor desc, ref string input)
        {
            var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
            if(string.IsNullOrEmpty(target))
            {
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Mercenary))
                {
                    if(desc.Player.FollowerID == Guid.Empty)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("The Mercenary Commander grins. 'You want hired muscle? Sure...'");
                        sb.AppendLine($"  {new string('=', 77)}");
                        sb.AppendLine($"|| Price{Constants.TabStop}|| Mercenary");
                        sb.AppendLine($"||==============||{new string('=', 61)}");
                        var p = Helpers.GetNewPurchasePrice(ref desc, desc.Player.Level * 1000);
                        if (p.ToString().Length > 4)
                        {
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Fighter");
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Thief");
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Mage");
                            sb.AppendLine($"|| {p}{Constants.TabStop}|| Mercenary Priest");
                        }
                        else
                        {
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Fighter");
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Thief");
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Mage");
                            sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| Mercenary Priest");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"You already have a follower, you can't hire more than one!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"There is no Mercenary Commander to hire from!{Constants.NewLine}");
                }
            }
            else
            {
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Mercenary))
                {
                    if(desc.Player.FollowerID == Guid.Empty)
                    {
                        var p = Helpers.GetNewPurchasePrice(ref desc, desc.Player.Level * 1000);
                        if(desc.Player.Stats.Gold >= p)
                        {
                            NPC hireling = null;
                            if (Regex.Match(target, "fighter", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "fighter");
                            }
                            if (Regex.Match(target, "thief", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "thief");
                            }
                            if (Regex.Match(target, "mage", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "mage");
                            }
                            if (Regex.Match(target, "priest", RegexOptions.IgnoreCase).Success)
                            {
                                hireling = NPCManager.Instance.GetHirelingNPC(ref desc, "priest");
                            }
                            if(hireling != null)
                            {
                                desc.Player.Stats.Gold -= p;
                                NPCManager.Instance.AddNPCToWorld(hireling, desc.Player.CurrentRoom);
                                desc.Player.FollowerID = hireling.NPCGuid;
                                desc.Send($"You have hired {hireling.Title} {hireling.Name} as your follower!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"Something went wrong hiring a follower, please check with an Imm!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"The Mercenary Commander laughs. 'No gold! You must have gold!'{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You already have a follower, you can't hire more than one!{Constants.NewLine}");
                    }
                }
                else
                {
                    var n = GetTargetNPC(ref desc, target);
                    if(n != null)
                    {
                        if(desc.Player.FollowerID == Guid.Empty)
                        {
                            if(n.BehaviourFlags.HasFlag(NPCFlags.Mercenary))
                            {
                                var baseCost = n.NumberOfHitDice * 1000;
                                var modCost = Helpers.GetNewPurchasePrice(ref desc, baseCost);
                                if(desc.Player.Stats.Gold >= modCost)
                                {
                                    desc.Player.Stats.Gold -= modCost;
                                    desc.Player.FollowerID = n.NPCGuid;
                                    NPCManager.Instance.SetNPCFollowing(ref desc, true);
                                    desc.Send($"You hand over {modCost} gold and hire {n.Name} as your follower!{Constants.NewLine}");
                                }
                                else
                                {
                                    desc.Send($"You can't afford to hire {n.Name} as your follower!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"{n.Name} isn't interested in being your follower!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"You already have a follower, you can't hire more than one!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That doesn't seem to be here...{Constants.NewLine}");
                    }
                }
            }
        }

        private static void DismissFollower(ref Descriptor desc, ref string input)
        {
            if(desc.Player.FollowerID != Guid.Empty)
            {
                var n = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                desc.Send($"You dismiss {n.Name} from your service.{Constants.NewLine}");
                NPCManager.Instance.RemoveNPCFromWorld(desc.Player.FollowerID, n, desc.Player.CurrentRoom);
                desc.Player.FollowerID = Guid.Empty;
                desc.Send($"{n.Name} is swallowed by the Winds of Magic!{Constants.NewLine}");
            }
            else
            {
                desc.Send($"You don't have a follower to dismiss right now.{Constants.NewLine}");
            }
        }

        private static void ChangeCharacterTitle(ref Descriptor desc, ref string input)
        {
            desc.Send($"Your current Title is: {desc.Player.Title}{Constants.NewLine}");
            bool titleOK = false;
            while(!titleOK)
            {
                desc.Send($"Enter new Title (exit to abort): ");
                var newTitle = desc.Read().Trim();
                if(ValidateInput(newTitle))
                {
                    if(newTitle.ToLower() == "exit")
                    {
                        titleOK = true;
                    }
                    else
                    {
                        if(newTitle.Length <= 15)
                        {
                            desc.Player.Title = newTitle;
                            titleOK = true;
                        }
                        else
                        {
                            desc.Send($"That is too long to be a title. Titles must be 15 characters or less.{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void ChangePlayerPassword(ref Descriptor desc, ref string input)
        {
            bool pwOK = false;
            while(!pwOK)
            {
                desc.Send($"Enter current password: ");
                var curPW = desc.Read().Trim();
                if(ValidateInput(curPW))
                {
                    if (DatabaseManager.ValidatePlayerPassword(desc.Player.Name, curPW))
                    {
                        desc.Send($"Enter new password: ");
                        var newPW = desc.Read().Trim();
                        if(ValidateInput(newPW))
                        {
                            if(DatabaseManager.UpdatePlayerPassword(ref desc, newPW))
                            {
                                desc.Send($"Your password has been updated successfully.{Constants.NewLine}");
                                Game.LogMessage($"INFO: Player {desc.Player} has successfully changed their password", LogLevel.Info, true);
                                pwOK = true;
                            }
                            else
                            {
                                desc.Send($"Failed to updated your password. Please see an Imm for advice.{Constants.NewLine}");
                                Game.LogMessage($"WARN: Failed to update password for {desc.Player}", LogLevel.Warning, true);
                                pwOK = true;
                            }
                        }
                        else
                        {
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Validation failed, this has been logged.{Constants.NewLine}");
                        Game.LogMessage($"WARN: Player {desc.Player} failed password validation while trying to change their password", LogLevel.Warning, true);
                        pwOK = true;
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void ShowPlayerRecipies(ref Descriptor desc, ref string input)
        {
            if(desc.Player.KnownRecipes != null && desc.Player.KnownRecipes.Count > 0)
            {
                var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if(string.IsNullOrEmpty(target))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"  {new string('=', 77)}");
                    var sk = (from kr in desc.Player.KnownRecipes select kr.RecipeType.ToString()).Distinct().ToList();
                    bool first = true;
                    foreach(var skill in sk)
                    {
                        if(first)
                        {
                            sb.AppendLine($"|| {Constants.GreenText}{skill}{Constants.PlainText}");
                            sb.AppendLine($"||{new string('=', 77)}");
                            foreach (var r in desc.Player.KnownRecipes.Where(x => x.RecipeType.ToString() == skill).ToList())
                            {
                                sb.AppendLine($"|| {r.RecipieName}");
                            }
                            first = false;
                        }
                        else
                        {
                            sb.AppendLine($"||{new string('=', 77)}");
                            sb.AppendLine($"|| {Constants.GreenText}{skill}{Constants.PlainText}");
                            sb.AppendLine($"||{new string('=', 77)}");
                            foreach (var r in desc.Player.KnownRecipes.Where(x => x.RecipeType.ToString() == skill).ToList())
                            {
                                sb.AppendLine($"|| {r.RecipieName}");
                            }
                        }
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    var r = (from kr in desc.Player.KnownRecipes where Regex.Match(kr.RecipieName, target, RegexOptions.IgnoreCase).Success select kr).FirstOrDefault();
                    if(r != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"  {new string('=', 77)}");
                        sb.AppendLine($"|| Name: {r.RecipieName}");
                        sb.AppendLine($"|| Description: {r.RecipieDescription}");
                        sb.AppendLine($"|| Produces: {ItemManager.Instance.GetItemByID(r.RecipeResult).Name}");
                        sb.AppendLine($"|| Requires:");
                        foreach(var req in r.RequiredMaterials)
                        {
                            sb.AppendLine($"||{Constants.TabStop}{req.Value} x {ItemManager.Instance.GetItemByID(req.Key).Name}");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"You don't know that recipe!{Constants.NewLine}");
                    }
                }
            }
            else
            {
                desc.Send($"You don't know any recipes!{Constants.NewLine}");
            }
        }

        private static void ReadScroll(ref Descriptor desc, ref string input)
        {
            if(desc.Player.HasSkill("Read"))
            {
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var target = TokeniseInput(ref input).Last().Trim();
                var scrollName = line.Replace(target, string.Empty).Trim();
                var scr = GetTargetItem(ref desc, scrollName, true);
                if (scr != null && scr.ItemType == ItemType.Scroll)
                {
                    if (!string.IsNullOrEmpty(target))
                    {
                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe) || RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoMagic))
                        {
                            desc.Send($"Some mystical force prevents you from reading the scroll...{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Player.Inventory.Remove(scr);
                            CastSpell(ref desc, ref input, true);
                        }
                    }
                    else
                    {
                        desc.Send($"Cast the scroll at what?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You don't have that scroll in your inventory!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You don't know how to read the magic of the scrolls{Constants.NewLine}");
            }
        }

        private static void ChangeCharacterLongDesc(ref Descriptor desc)
        {
            desc.Send($"Your current Long Description is:{Constants.NewLine}{desc.Player.LongDescription}");
            desc.Player.LongDescription = Helpers.GetLongDescription(ref desc);
        }

        private static void ChangeCharacterShortDesc(ref Descriptor desc)
        {
            desc.Send($"Your current Short Description is: {desc.Player.ShortDescription}");
            desc.Send($"Enter a new short description for your character. This should be");
            desc.Send($"no longer than 30 characters:");
            var sDesc = desc.Read().Trim();
            if(Helpers.ValidateInput(sDesc))
            {
                desc.Player.ShortDescription = sDesc;
            }
        }

        private static void Backstab(ref Descriptor desc, ref string input)
        {
            if(desc.Player.HasSkill("Backstab"))
            {
                if(desc.Player.EquippedItems != null && desc.Player.EquippedItems.Weapon != null)
                {
                    if (!desc.Player.Visible && !desc.Player.IsInCombat && desc.Player.Position == ActorPosition.Standing)
                    {
                        if (!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                        {
                            var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                            if (!string.IsNullOrEmpty(target))
                            {
                                var npc = GetTargetNPC(ref desc, target);
                                if (npc != null)
                                {
                                    if(!npc.BehaviourFlags.HasFlag(NPCFlags.NoAttack) && !CombatManager.Instance.IsNPCInCombat(npc.NPCGuid))
                                    {
                                        if(desc.Player.Stats.CurrentMP >= Skills.GetSkill("Backstab").MPCost)
                                        {
                                            bool startCombat = false;
                                            desc.Player.Visible = true;
                                            desc.Send($"You become visible again{Constants.NewLine}");
                                            var hitRoll = Helpers.RollDice(1, 20);
                                            var modHitRoll = hitRoll + 5;
                                            if (ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity) > 0)
                                            {
                                                modHitRoll += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                            }
                                            if (hitRoll == 20 || modHitRoll >= npc.Stats.ArmourClass)
                                            {
                                                // player hit, check for critical, do damage and start combat if NPC survives
                                                var w = desc.Player.EquippedItems.Weapon;
                                                var damRoll = Helpers.RollDice(w.NumberOfDamageDice, w.SizeOfDamageDice);
                                                if (w.IsRanged)
                                                {
                                                    if (ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity) > 0)
                                                    {
                                                        damRoll += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                                    }
                                                }
                                                else
                                                {
                                                    if (ActorStats.CalculateAbilityModifier(desc.Player.Stats.Strength) > 0)
                                                    {
                                                        damRoll += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(desc.Player.Stats.Strength));
                                                    }
                                                }
                                                if (hitRoll == 20)
                                                {
                                                    damRoll *= 4;
                                                }
                                                desc.Player.Visible = true;
                                                if (damRoll >= npc.Stats.CurrentHP)
                                                {
                                                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                                                    {
                                                        desc.Send($"You roll {hitRoll} (Modified: {modHitRoll}) and deal {damRoll} damage killing {npc.Name} instantly!{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        desc.Send($"Your backstab destroys {npc.Name}, killing them instantly!{Constants.NewLine}");
                                                    }
                                                    desc.Send($"You earn {npc.BaseExpAward} Exp and {npc.Stats.Gold} gold!{Constants.NewLine}");
                                                    desc.Player.AddExp(npc.BaseExpAward, ref desc);
                                                    desc.Player.AddGold(npc.Stats.Gold, ref desc);
                                                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                                    if (localPlayers != null && localPlayers.Count > 1)
                                                    {
                                                        foreach (var p in localPlayers)
                                                        {
                                                            if (p.Player.Name != desc.Player.Name)
                                                            {
                                                                p.Send($"There is a sickening scream as {npc.Name} is slaughtered!{Constants.NewLine}");
                                                            }
                                                        }
                                                    }
                                                    npc.Kill(true);
                                                    if(desc.Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(npc.NPCID)))
                                                    {
                                                        for (int n = 0; n < desc.Player.ActiveQuests.Count; n++)
                                                        {
                                                            if (desc.Player.ActiveQuests[n].Monsters.Keys.Contains(npc.NPCID))
                                                            {
                                                                if (desc.Player.ActiveQuests[n].Monsters[npc.NPCID] <= 1)
                                                                {
                                                                    desc.Player.ActiveQuests[n].Monsters[npc.NPCID] = 0;
                                                                }
                                                                else
                                                                {
                                                                    desc.Player.ActiveQuests[n].Monsters[npc.NPCID]--;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    NPCManager.Instance.AdjustNPCHealth(npc.NPCGuid, (int)damRoll);
                                                    startCombat = true;
                                                    if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                                                    {
                                                        desc.Send($"You roll {hitRoll} (Modified: {modHitRoll}) and deal {damRoll} damage to {npc.Name}!{Constants.NewLine}");
                                                    }
                                                    else
                                                    {
                                                        var percDamage = (uint)Math.Round((double)(damRoll / npc.Stats.CurrentHP) * 100);
                                                        desc.Send($"Your backstab {Helpers.GetDamageString(percDamage)} {npc.Name}!{Constants.NewLine}");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // backstab missed, make player visible and start combat
                                                startCombat = true;
                                                desc.Player.Visible = true;
                                                desc.Send($"You become visible again{Constants.NewLine}");
                                                if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                                                {
                                                    desc.Send($"You roll {hitRoll} (Modified: {modHitRoll}) and your Backstab misses {npc.Name}!{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    desc.Send($"Your Backstab misses {npc.Name}!{Constants.NewLine}");
                                                }
                                            }
                                            if (startCombat)
                                            {
                                                if (!desc.Player.Visible)
                                                {
                                                    desc.Player.Visible = true;
                                                    desc.Send($"You shimmer and become visible again.{Constants.NewLine}");
                                                }
                                                var myInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                                var mobInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(npc.Stats.Dexterity));
                                                myInit = desc.Player.HasSkill("Awareness") ? myInit += 4 : myInit;
                                                mobInit = npc.HasSkill("Awareness") ? mobInit += 4 : mobInit;
                                                var participants = new List<(uint Initiative, dynamic Participant, dynamic Target)>
                                                {
                                                    (myInit, desc, npc),
                                                    (mobInit, npc, desc)
                                                };
                                                if(desc.Player.FollowerID != Guid.Empty)
                                                {
                                                    var f = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                    if (f != null)
                                                    {
                                                        var followerInit = Helpers.RollDice(1, 20);
                                                        if (ActorStats.CalculateAbilityModifier(f.Stats.Dexterity) > 0)
                                                        {
                                                            followerInit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(f.Stats.Dexterity));
                                                        }
                                                        participants.Add((followerInit, f, npc));
                                                        participants.Add((mobInit, npc, f));
                                                    }
                                                    else
                                                    {
                                                        desc.Player.FollowerID = Guid.Empty;
                                                        Game.LogMessage($"DEBUG: Setting {desc.Player}'s FollowerID to Guid.Empty as no matching NPC could be found", LogLevel.Debug, true);
                                                    }
                                                }
                                                var g = CombatManager.Instance.AddCombatSession(new CombatSession
                                                {
                                                    Participants = participants
                                                });
                                                desc.Player.CombatSessionID = g;
                                                desc.Player.Position = ActorPosition.Fighting;
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"You don't have enough MP to use that skill!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Some mysical force prevents you from attacking {npc.Name}...{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"Backstab what, exactly?{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"Backstab what, exactly?{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"Some mystical force prevents this, it is impossible to fight here!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"You're not in a position to do that right now{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"That skill requires you to have an equipped weapon{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You don't know how to do that!{Constants.NewLine}");
            }
        }

        private static void Pickpocket(ref Descriptor desc, ref string input)
        {
            if(desc.Player.HasSkill("Pickpocket") && desc.Player.Position == ActorPosition.Standing)
            {
                if (!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                {
                    var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                    if(!string.IsNullOrEmpty(target))
                    {
                        var npc = GetTargetNPC(ref desc, target);
                        if(npc != null)
                        {
                            if(!npc.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
                            {
                                if (npc.Inventory != null && npc.Inventory.Count > 0)
                                {
                                    if(desc.Player.Stats.CurrentMP >= Skills.GetSkill("Pickpocket").MPCost)
                                    {
                                        var skillRoll = Helpers.RollDice(1, 20);
                                        var modSkillRoll = skillRoll;
                                        if (ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity) > 0)
                                        {
                                            modSkillRoll += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                        }
                                        if (!desc.Player.Visible)
                                        {
                                            // bonus if the player is not visible
                                            modSkillRoll += 4;
                                        }
                                        var npcRoll = Helpers.RollDice(1, 20);
                                        var modNpcRoll = npcRoll;
                                        if (ActorStats.CalculateAbilityModifier(npc.Stats.Dexterity) > 0)
                                        {
                                            modSkillRoll += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(npc.Stats.Dexterity));
                                        }
                                        bool success = false;
                                        if (skillRoll == 20 || modSkillRoll > modNpcRoll)
                                        {
                                            success = true;
                                        }
                                        if (success)
                                        {
                                            var rnd = new Random(DateTime.Now.GetHashCode());
                                            var item = npc.Inventory[rnd.Next(npc.Inventory.Count)];
                                            npc.Inventory.Remove(item);
                                            desc.Player.Inventory.Add(item);
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                                            {
                                                desc.Send($"You rolled {skillRoll} (Modified: {modSkillRoll}) against {npc.Name}'s roll of {npcRoll} (Modified: {modNpcRoll}){Constants.NewLine}");
                                                
                                            }
                                            desc.Send($"You successfully steal {item.Name} from {npc.Name}!{Constants.NewLine}");
                                        }
                                        else
                                        {
                                            // player failed to steal, so make them visible (if necessary) and start a fight with the target NPC
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.ShowDetailedRollInfo)
                                            {
                                                desc.Send($"You rolled {skillRoll} (Modified: {modSkillRoll}) against {npc.Name}'s roll of {npcRoll} (Modified: {modNpcRoll}){Constants.NewLine}");
                                            }
                                            desc.Send($"You failed to steal an item and have been noticed!{Constants.NewLine}");
                                            if (!desc.Player.Visible)
                                            {
                                                desc.Send($"You become visible again.{Constants.NewLine}");
                                                desc.Player.Visible = true;
                                            }
                                            desc.Send($"{npc.Name} notices you trying to steal from them and attacks!{Constants.NewLine}");
                                            var myInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                            var mobInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(npc.Stats.Dexterity));
                                            myInit = desc.Player.HasSkill("Awareness") ? myInit += 4 : myInit;
                                            mobInit = npc.HasSkill("Awareness") ? mobInit += 4 : mobInit;
                                            var participants = new List<(uint Initiative, dynamic Participant, dynamic Target)>
                                            {
                                                (myInit, desc, npc),
                                                (mobInit, npc, desc)
                                            };
                                            if(desc.Player.FollowerID != Guid.Empty)
                                            {
                                                var f = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                if (f != null)
                                                {
                                                    var followerInit = Helpers.RollDice(1, 20);
                                                    if (ActorStats.CalculateAbilityModifier(f.Stats.Dexterity) > 0)
                                                    {
                                                        followerInit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(f.Stats.Dexterity));
                                                    }
                                                    participants.Add((followerInit, f, npc));
                                                    participants.Add((mobInit, npc, f));
                                                }
                                                else
                                                {
                                                    desc.Player.FollowerID = Guid.Empty;
                                                    Game.LogMessage($"DEBUG: Setting {desc.Player}'s FollowerID to Guid.Empty as no matching NPC could be found", LogLevel.Debug, true);
                                                }
                                            }
                                            var g = CombatManager.Instance.AddCombatSession(new CombatSession
                                            {
                                                Participants = participants
                                            });
                                            desc.Player.CombatSessionID = g;
                                            desc.Player.Position = ActorPosition.Fighting;
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"You don't have enough MP to use that skill!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"{npc.Name} isn't carrying anything you can steal...{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"Some mystical force prevents you from taking things from {npc.Name}{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That doesn't seem to be here...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Pickpocket who, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You cannot do that here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You don't know how to do that!{Constants.NewLine}");
            }
        }

        private static void TrainPlayerStat(ref Descriptor desc, ref string input)
        {
            var stat = input.Replace(GetVerb(ref input), string.Empty).Trim();
            StringBuilder sb = new StringBuilder();
            if(!string.IsNullOrEmpty(stat))
            {
                uint cost = 0;
                switch(stat.ToLower())
                {
                    case "str":
                    case "strength":
                        cost = Convert.ToUInt32((desc.Player.Stats.Strength + 1) * 1000);
                        break;

                    case "dex":
                    case "dexterity":
                        cost = Convert.ToUInt32((desc.Player.Stats.Dexterity + 1) * 1000);
                        break;

                    case "int":
                    case "intelligence":
                        cost = Convert.ToUInt32((desc.Player.Stats.Intelligence + 1) * 1000);
                        break;

                    case "wisdom":
                    case "wis":
                        cost = Convert.ToUInt32((desc.Player.Stats.Wisdom + 1) * 1000);
                        break;

                    case "constitution":
                    case "con":
                        cost = Convert.ToUInt32((desc.Player.Stats.Constitution + 1) * 1000);
                        break;

                    case "charisma":
                    case "cha":
                        cost = Convert.ToUInt32((desc.Player.Stats.Charisma + 1) * 1000);
                        break;

                    case "hp":
                    case "mp":
                        cost = 20000;
                        break;

                    default:
                        desc.Send($"'I can't help you train that,' the gym master says.{Constants.NewLine}");
                        return;
                }
                if(desc.Player.Stats.Gold >= cost)
                {
                    desc.Send($"The gym master smiles. 'Certainly! Follow me...'{Constants.NewLine}");
                    desc.Player.Stats.Gold -= cost;
                    switch (stat.ToLower())
                    {
                        case "str":
                        case "strength":
                            desc.Player.Stats.Strength++;
                            desc.Send($"Your Strength increases to {desc.Player.Stats.Strength}{Constants.NewLine}");
                            break;

                        case "dex":
                        case "dexterity":
                            desc.Player.Stats.Dexterity++;
                            desc.Send($"Your Dexterity increases to {desc.Player.Stats.Dexterity}{Constants.NewLine}");
                            break;

                        case "int":
                        case "intelligence":
                            desc.Player.Stats.Intelligence++;
                            desc.Send($"Your Intelligence increases to {desc.Player.Stats.Intelligence}{Constants.NewLine}");
                            break;

                        case "wisdom":
                        case "wis":
                            desc.Player.Stats.Wisdom++;
                            desc.Send($"Your Wisdom increases to {desc.Player.Stats.Wisdom}{Constants.NewLine}");
                            break;

                        case "constitution":
                        case "con":
                            desc.Player.Stats.Constitution++;
                            desc.Send($"Your Constitution increases to {desc.Player.Stats.Constitution}{Constants.NewLine}");
                            break;

                        case "charisma":
                        case "cha":
                            desc.Player.Stats.Charisma++;
                            desc.Send($"Your Charisma increases to {desc.Player.Stats.Charisma}{Constants.NewLine}");
                            break;

                        case "hp":
                            switch(desc.Player.Class)
                            {
                                case ActorClass.Wizard:
                                    var hpInc = Helpers.RollDice(1, 4) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Constitution);
                                    hpInc = hpInc <= 0 ? 1: hpInc;
                                    desc.Player.Stats.CurrentHP += (int)hpInc;
                                    desc.Player.Stats.MaxHP += Convert.ToUInt32(hpInc);
                                    desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                    break;

                                case ActorClass.Cleric:
                                    hpInc = Helpers.RollDice(1, 8) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Constitution);
                                    hpInc = hpInc <= 0 ? 1 : hpInc;
                                    desc.Player.Stats.CurrentHP += (int)hpInc;
                                    desc.Player.Stats.MaxHP += Convert.ToUInt32(hpInc);
                                    desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                    break;

                                case ActorClass.Thief:
                                    hpInc = Helpers.RollDice(1, 6) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Constitution);
                                    hpInc = hpInc <= 0 ? 1 : hpInc;
                                    desc.Player.Stats.CurrentHP += (int)hpInc;
                                    desc.Player.Stats.MaxHP += Convert.ToUInt32(hpInc);
                                    desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                    break;

                                case ActorClass.Fighter:
                                    hpInc = Helpers.RollDice(1, 10) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Constitution);
                                    hpInc = hpInc <= 0 ? 1 : hpInc;
                                    desc.Player.Stats.CurrentHP += (int)hpInc;
                                    desc.Player.Stats.MaxHP += Convert.ToUInt32(hpInc);
                                    desc.Send($"Your health increases by {hpInc}!{Constants.NewLine}");
                                    break;
                            }
                            break;

                        case "mp":
                            switch (desc.Player.Class)
                            {
                                case ActorClass.Wizard:
                                    var mpInc = Helpers.RollDice(1, 10) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence);
                                    mpInc = mpInc <= 0 ? 1 : mpInc;
                                    desc.Player.Stats.CurrentMP += (int)mpInc;
                                    desc.Player.Stats.MaxMP += Convert.ToUInt32(mpInc);
                                    desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                    break;

                                case ActorClass.Cleric:
                                    mpInc = Helpers.RollDice(1, 8) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Wisdom);
                                    mpInc = mpInc <= 0 ? 1 : mpInc;
                                    desc.Player.Stats.CurrentMP += (int)mpInc;
                                    desc.Player.Stats.MaxMP += Convert.ToUInt32(mpInc);
                                    desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                    break;

                                case ActorClass.Thief:
                                    mpInc = Helpers.RollDice(1, 6) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence);
                                    mpInc = mpInc <= 0 ? 1 : mpInc;
                                    desc.Player.Stats.CurrentMP += (int)mpInc;
                                    desc.Player.Stats.MaxMP += Convert.ToUInt32(mpInc);
                                    desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                    break;

                                case ActorClass.Fighter:
                                    mpInc = Helpers.RollDice(1, 4) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence);
                                    mpInc = mpInc <= 0 ? 1 : mpInc;
                                    desc.Player.Stats.CurrentMP += (int)mpInc;
                                    desc.Player.Stats.MaxMP += Convert.ToUInt32(mpInc);
                                    desc.Send($"Your magic increases by {mpInc}!{Constants.NewLine}");
                                    break;
                            }
                            break;
                    }
                    desc.Player.CalculateArmourClass();
                }
                else
                {
                    desc.Send($"The gym master frowns. 'Looks like you're short of funds!'{Constants.NewLine}");
                }
            }
            else
            {
                // no stat specified so show price for increasing all stats
                sb.AppendLine($"The gym master flexes. 'Sure I can help you improve, but it will cost you...'");
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| Price{Constants.TabStop}|| Stat");
                var strIncPrice = (desc.Player.Stats.Strength + 1) * 1000;
                var dexIncPrice = (desc.Player.Stats.Dexterity + 1) * 1000;
                var intIncPrice = (desc.Player.Stats.Intelligence + 1) * 1000;
                var wisIncPrice = (desc.Player.Stats.Wisdom + 1) * 1000;
                var conIncPrice = (desc.Player.Stats.Constitution + 1) * 1000;
                var chaIncPrice = (desc.Player.Stats.Charisma + 1) * 1000;
                sb.AppendLine($"||==============||{new string('=', 61)}");
                if(strIncPrice.ToString().Length > 4)
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Strength + 1) * 1000}{Constants.TabStop}|| Strength");
                }
                else
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Strength + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Strength");
                }
                if(dexIncPrice.ToString().Length > 4)
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Dexterity + 1) * 1000}{Constants.TabStop}|| Dexterity");
                }
                else
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Dexterity + 1) * 1000}{Constants.TabStop}{Constants.NewLine}|| Dexterity");
                }
                if(intIncPrice.ToString().Length > 4)
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Intelligence + 1) * 1000}{Constants.TabStop}|| Intelligence");
                }
                else
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Intelligence + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Intelligence");
                }
                if(wisIncPrice.ToString().Length > 4)
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Wisdom + 1) * 1000}{Constants.TabStop}|| Wisdom");
                }
                else
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Wisdom + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Wisdom");
                }
                if(conIncPrice.ToString().Length > 4)
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Constitution + 1) * 1000}{Constants.TabStop}|| Constitution");
                }
                else
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Constitution + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Constitution");
                }
                if(chaIncPrice.ToString().Length > 4)
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Charisma + 1) * 1000}{Constants.TabStop}|| Charisma");
                }
                else
                {
                    sb.AppendLine($"|| {(desc.Player.Stats.Charisma + 1) * 1000}{Constants.TabStop}{Constants.TabStop}|| Charisma");
                }
                sb.AppendLine($"|| 20000{Constants.TabStop}|| Extra HP");
                sb.AppendLine($"|| 20000{Constants.TabStop}|| Extra MP");
                sb.AppendLine($"  {new string('=', 77)}");
                desc.Send(sb.ToString());
            }
        }

        private static void ShowPlayerSkills(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            if(desc.Player.Skills.Count > 0)
            {
                sb.AppendLine("|| You know the following skills:");
                foreach(var s in desc.Player.Skills)
                {
                    sb.AppendLine($"|| Name: {s.Name}{Constants.TabStop}MP: {s.MPCost}");
                    sb.AppendLine($"|| Effect: {s.Description}");
                }
            }
            else
            {
                sb.AppendLine("|| No skills known");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void ShowPlayerSpells(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            if(desc.Player.Spells.Count > 0)
            {
                sb.AppendLine("|| You know the following spells:");
                foreach(var s in desc.Player.Spells)
                {
                    sb.AppendLine($"|| Name: {s.SpellName}{Constants.TabStop}Mana: {s.MPCost}");
                    sb.AppendLine($"|| Effect: {s.Description}");
                }
            }
            else
            {
                sb.AppendLine("|| No spells known");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void ShowBuffs(ref Descriptor desc, ref string input)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            if(desc.Player.Buffs != null && desc.Player.Buffs.Count > 0)
            {
                foreach(var b in desc.Player.Buffs)
                {
                    if(b.Value == -1)
                    {
                        sb.AppendLine($"|| {b.Key}: Permanent");
                    }
                    else
                    {
                        var line = b.Value > 1 ? $"|| {b.Key}: {b.Value} ticks remaining" : $"|| {b.Key}: {b.Value} tick remaining";
                        sb.AppendLine(line);
                    }
                }
            }
            else
            {
                sb.AppendLine("|| No buffs active");
            }
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void CastSpell(ref Descriptor desc, ref string input, bool overrideSkillCheck = false)
        {
            if(!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoMagic))
            {
                var spellName = GetSkillOrSpellName(ref input);
                var elements = TokeniseInput(ref input);
                if(elements.Length < 2)
                {
                    desc.Send($"Usage: cast \"<spellname>\" <target>{Constants.NewLine}");
                    return;
                }
                spellName = string.IsNullOrEmpty(spellName) ? elements[1].Trim() : spellName.Trim();
                if(!string.IsNullOrEmpty(spellName))
                {
                    if(Spells.SpellExists(spellName))
                    {
                        if(desc.Player.HasSpell(spellName) || overrideSkillCheck)
                        {
                            var s = Spells.GetSpell(spellName);
                            bool okToCast = true;
                            if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                            {
                                // Do some checks to make sure we can't cast a damaging spell in a safe room
                                if(s.SpellType == SpellType.Damage || (s.NumOfDamageDice > 0 && s.SpellType != SpellType.Healing))
                                {
                                    okToCast = false;
                                }
                            }
                            if(okToCast)
                            {
                                if(desc.Player.Stats.CurrentMP >= s.MPCost)
                                {
                                    var target = input.Replace(GetVerb(ref input), string.Empty).Replace(spellName, string.Empty).Replace("\"", string.Empty).Trim();
                                    Descriptor tPlayer = null;
                                    NPC tNPC = null;
                                    Guid targetGUID = Guid.Empty;
                                    if (desc.Player.IsInCombat)
                                    {
                                        targetGUID = CombatManager.Instance.GetNPCGuidFromCombatSession(desc.Player.CombatSessionID);
                                        tNPC = targetGUID != Guid.Empty ? NPCManager.Instance.GetNPCByGUID(targetGUID) : null;
                                    }
                                    tPlayer = target.ToLower() == "self" ? desc : null;
                                    bool targetFound = tNPC != null || tPlayer != null;
                                    if(!targetFound)
                                    {
                                        tNPC = GetTargetNPC(ref desc, target);
                                        if (tNPC == null)
                                        {
                                            if (tPlayer == null)
                                            {
                                                tPlayer = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                                                if (tPlayer != null)
                                                {
                                                    targetFound = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            targetFound = true;
                                        }
                                        if (s.RequiresTarget && !targetFound)
                                        {
                                            desc.Send($"The target of your magic cannot be found!{Constants.NewLine}");
                                            return;
                                        }
                                    }
                                    if(tNPC != null && tNPC.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
                                    {
                                        desc.Send($"Some mystical force prevents you from casting that on {tNPC.Name}!{Constants.NewLine}");
                                        return;
                                    }
                                    if(tPlayer != null && (s.SpellType == SpellType.Damage || s.NumOfDamageDice > 0))
                                    {
                                        if(s.SpellType == SpellType.Healing)
                                        {
                                            var toHeal = Helpers.RollDice(s.NumOfDamageDice, s.SizeOfDamageDice);
                                            var statMod = desc.Player.Class == ActorClass.Cleric ? ActorStats.CalculateAbilityModifier(desc.Player.Stats.Wisdom)
                                                : ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence);
                                            if(statMod > 0)
                                            {
                                                toHeal += Convert.ToUInt32((s.NumOfDamageDice * statMod));
                                            }
                                            if(tPlayer.Player.Stats.CurrentHP + toHeal > tPlayer.Player.Stats.MaxHP)
                                            {
                                                tPlayer.Player.Stats.CurrentHP = (int)tPlayer.Player.Stats.MaxHP;
                                                if(tPlayer.Player.Name == desc.Player.Name)
                                                {
                                                    desc.Send($"Summoning holy power you restore yourself to full health!{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    desc.Send($"Summoning holy power you restore {tPlayer.Player.Name} to full health!{Constants.NewLine}");
                                                    tPlayer.Send($"{desc.Player.Name} calls on holy power and restores you to full health!{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                tPlayer.Player.Stats.CurrentHP += (int)toHeal;
                                                if(tPlayer.Player.Name == desc.Player.Name)
                                                {
                                                    desc.Send($"Calling on holy power, you heal {toHeal} points of damage!{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    desc.Send($"Calling on holy power, you heal {tPlayer.Player.Name} {toHeal} points of damage!{Constants.NewLine}");
                                                    tPlayer.Send($"{desc.Player.Name} calls on holy power, healing you {toHeal} points of damage!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (tPlayer.Player.Name == desc.Player.Name)
                                            {
                                                // player has tried to target an offensive spell at themselves...
                                                desc.Send($"You can't cast that on yourself!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                desc.Send($"PvP combat is not yet implemented...{Constants.NewLine}");
                                            }
                                        }
                                        return;
                                    }
                                    if(s.SpellType == SpellType.Buff && tPlayer != null)
                                    {
                                        var b = Buffs.GetBuff(s.SpellName);
                                        if(b != null)
                                        {
                                            // successfully passed all checks and cast the spell on the player
                                            desc.Player.Stats.CurrentMP -= (int)s.MPCost;
                                            tPlayer.Player.AddBuff(b.BuffName);
                                            desc.Send($"The Winds of Magic swirl, granting {tPlayer.Player.Name} the buff of {s.SpellName}!");
                                            tPlayer.Send($"{desc.Player.Name} summons the Winds of Magic and grants you the buff of {s.SpellName}!");
                                            RoomManager.Instance.ProcessEnvironmentBuffs(tPlayer.Player.CurrentRoom);
                                        }
                                        else
                                        {
                                            // we should always have a buff with the same name as the spell
                                            Game.LogMessage($"WARN: Unable to find a buff for spell '{s.SpellName}'!", LogLevel.Warning, true);
                                            desc.Send($"The spell {s.SpellName} appears to be broken, please check with an Imm!{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        // target is an NPC and we're OK to cast the spell.
                                        var b = Buffs.GetBuff(s.SpellName);
                                        desc.Player.Stats.CurrentMP -= (int)s.MPCost;
                                        if(b != null)
                                        {
                                            // spell has an associate buff, so apply that to the target
                                            tNPC.AddBuff(b.BuffName);
                                        }
                                        if(s.NumOfDamageDice > 0)
                                        {
                                            if(s.SpellType == SpellType.Healing)
                                            {
                                                var toHeal = Helpers.RollDice(s.NumOfDamageDice, s.SizeOfDamageDice);
                                                var healMod = desc.Player.Class == ActorClass.Cleric ? ActorStats.CalculateAbilityModifier(desc.Player.Stats.Wisdom)
                                                    : ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence);
                                                if(healMod > 0)
                                                {
                                                    toHeal += Convert.ToUInt32(s.NumOfDamageDice * healMod);
                                                }
                                                if(tNPC.Stats.CurrentHP + toHeal > tNPC.Stats.MaxHP)
                                                {
                                                    NPCManager.Instance.SetNPCHealthToMax(tNPC.NPCGuid);
                                                    desc.Send($"You heal {tNPC.Name} back to full health!{Constants.NewLine}");
                                                    return;
                                                }
                                                else
                                                {
                                                    NPCManager.Instance.AdjustNPCHealth(tNPC.NPCGuid, (int)toHeal);
                                                    desc.Send($"You heal {tNPC.Name} for {toHeal} points of damage!{Constants.NewLine}");
                                                    return;
                                                }
                                            }
                                            var damRoll = Helpers.RollDice(s.NumOfDamageDice, s.SizeOfDamageDice);
                                            var abilityMod = desc.Player.Class == ActorClass.Cleric ? ActorStats.CalculateAbilityModifier(desc.Player.Stats.Wisdom)
                                                : ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence);
                                            abilityMod *= Convert.ToInt32(s.NumOfDamageDice);
                                            var result = damRoll + abilityMod;
                                            result = result <= 0 ? 1 : result;
                                            var toHit = Helpers.RollDice(1, 20);
                                            var toHitFinal = toHit + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity);
                                            bool startCombatSession = false;
                                            if(s.AutoHitTarget || toHitFinal >= tNPC.Stats.ArmourClass)
                                            {
                                                // spell hit the target
                                                if (result < tNPC.Stats.CurrentHP)
                                                {
                                                    NPCManager.Instance.AdjustNPCHealth(tNPC.NPCGuid, ((int)result * -1));
                                                    desc.Send($"Your {s.SpellName} strikes {tNPC.Name} for {result} damage!{Constants.NewLine}");
                                                    startCombatSession = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Your {s.SpellName} deals lethal damage, killing {tNPC.Name}!{Constants.NewLine}");
                                                    desc.Send($"You have killed {tNPC.Name} and obtained {tNPC.BaseExpAward} Exp and {tNPC.Stats.Gold} gold!{Constants.NewLine}");
                                                    desc.Player.AddExp(tNPC.BaseExpAward, ref desc);
                                                    desc.Player.AddGold(tNPC.Stats.Gold, ref desc);
                                                    var pn = desc.Player.Name;
                                                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => !Regex.Match(x.Player.Name, pn, RegexOptions.IgnoreCase).Success).ToList();
                                                    if (localPlayers != null && localPlayers.Count > 0)
                                                    {
                                                        foreach (var p in localPlayers)
                                                        {
                                                            p.Send($"There is a sickening scream as {tNPC.Name} is slaughtered!{Constants.NewLine}");
                                                        }
                                                    }
                                                    tNPC.Kill(true);
                                                    if (desc.Player.ActiveQuests.Any(x => x.Monsters.Keys.Contains(tNPC.NPCID)))
                                                    {
                                                        for (int n = 0; n < desc.Player.ActiveQuests.Count; n++)
                                                        {
                                                            if (desc.Player.ActiveQuests[n].Monsters.Keys.Contains(tNPC.NPCID))
                                                            {
                                                                if (desc.Player.ActiveQuests[n].Monsters[tNPC.NPCID] <= 1)
                                                                {
                                                                    desc.Player.ActiveQuests[n].Monsters[tNPC.NPCID] = 0;
                                                                }
                                                                else
                                                                {
                                                                    desc.Player.ActiveQuests[n].Monsters[tNPC.NPCID]--;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if(startCombatSession)
                                                {
                                                    // only start a combat session if there isn't one already running
                                                    if (targetGUID == Guid.Empty)
                                                    {
                                                        var myInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                                        var mobInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(tNPC.Stats.Dexterity));
                                                        myInit = desc.Player.HasSkill("Awareness") ? myInit += 4 : myInit;
                                                        mobInit = tNPC.HasSkill("Awareness") ? mobInit += 4 : mobInit;
                                                        var participants = new List<(uint Initiative, dynamic Participant, dynamic Target)>
                                                        {
                                                            (myInit, desc, tNPC),
                                                            (mobInit, tNPC, desc)
                                                        };
                                                        if(desc.Player.FollowerID != Guid.Empty)
                                                        {
                                                            var f = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                                            if (f != null)
                                                            {
                                                                var followerInit = Helpers.RollDice(1, 20);
                                                                if (ActorStats.CalculateAbilityModifier(f.Stats.Dexterity) > 0)
                                                                {
                                                                    followerInit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(f.Stats.Dexterity));
                                                                }
                                                                participants.Add((followerInit, f, tNPC));
                                                                participants.Add((mobInit, tNPC, f));
                                                            }
                                                            else
                                                            {
                                                                desc.Player.FollowerID = Guid.Empty;
                                                                Game.LogMessage($"DEBUG: Setting {desc.Player}'s FollowerID to Guid.Empty as no matching NPC could be found", LogLevel.Debug, true);
                                                            }
                                                        }
                                                        var g = CombatManager.Instance.AddCombatSession(new CombatSession
                                                        {
                                                            Participants = participants
                                                        });
                                                        desc.Player.CombatSessionID = g;
                                                        desc.Player.Position = ActorPosition.Fighting;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Your magic fizzles and misses its target!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    desc.Send($"You don't have enough MP to cast that spell!{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"You cannot cast that here!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"You don't know that spell!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That spell doesn't exist!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Some mystical force prevents the use of magic here...{Constants.NewLine}");
            }
        }

        private static void FleeCombat(ref Descriptor desc)
        {
            if(desc.Player.CombatSessionID != Guid.Empty && desc.Player.Position == ActorPosition.Fighting)
            {
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).RoomExits.Count > 0)
                {
                    CombatManager.Instance.RemoveCombatSession(desc.Player.CombatSessionID);
                    desc.Player.CombatSessionID = Guid.Empty;
                    desc.Player.Position = ActorPosition.Standing;
                    var rndExit = Helpers.GetRandomExit(desc.Player.CurrentRoom);
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    desc.Player.Move(desc.Player.CurrentRoom, rndExit.DestinationRoomID, false, ref desc);
                    desc.Send($"The fight is too much and you flee towards the {rndExit.ExitDirection}...{Constants.NewLine}");
                    if (localPlayers != null && localPlayers.Count > 1)
                    {
                        foreach (var p in localPlayers)
                        {
                            if (!Regex.Match(desc.Player.Name, p.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"The fight is too much for {desc.Player.Name}, and they flee towards the {rndExit.ExitDirection}{Constants.NewLine}"
                                : $"The fight is too much for something and it flees towards the {rndExit.ExitDirection} like a chicken!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"There is nowhere to flee to, you must keep fighting!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Flee from what? You're not fighting!{Constants.NewLine}");
            }
        }

        private static void StartPVPCombat(ref Descriptor desc, ref string input)
        {
            if(desc.Player.PVP)
            {
                if(!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                {
                    var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                    var tPlayer = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    if(tPlayer != null)
                    {
                        if(tPlayer.Player.PVP)
                        {
                            var myInit = Helpers.RollDice(1, 20);
                            var tInit = Helpers.RollDice(1, 20);
                            var participants = new List<(uint Initiative, dynamic Participant, dynamic Target)>
                            {
                                (myInit, desc, tPlayer),
                                (tInit, tPlayer, desc)
                            };
                            if(desc.Player.FollowerID != Guid.Empty)
                            {
                                var fInit = Helpers.RollDice(1, 20);
                                var follower = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                participants.Add((fInit, follower, tPlayer));
                                participants.Add((tInit, tPlayer, follower));
                            }
                            if(tPlayer.Player.FollowerID != Guid.Empty)
                            {
                                var fInit = Helpers.RollDice(1, 20);
                                var follower = NPCManager.Instance.GetNPCByGUID(tPlayer.Player.FollowerID);
                                participants.Add((fInit, follower, desc));
                                participants.Add((myInit, desc, follower));
                            }
                            if(desc.Player.FollowerID != Guid.Empty && tPlayer.Player.FollowerID != Guid.Empty)
                            {
                                var myFInit = Helpers.RollDice(1, 20);
                                var tFInit = Helpers.RollDice(1, 20);
                                var myFollower = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                var tFollower = NPCManager.Instance.GetNPCByGUID(tPlayer.Player.FollowerID);
                                participants.Add((myFInit, myFollower, tFollower));
                                participants.Add((tFInit, tFollower, myFollower));
                            }
                            var g = CombatManager.Instance.AddCombatSession(new CombatSession
                            {
                                Participants = participants
                            });
                            desc.Player.CombatSessionID = g;
                            tPlayer.Player.CombatSessionID = g;
                        }
                        else
                        {
                            desc.Send($"That person doesn't want to fight you right now...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That person doesn't seem to be here...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Some mystical force prevents fighting here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Please enable your PVP flag to start PVP combat.{Constants.NewLine}");
            }
        }

        private static void StartCombat(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Position == ActorPosition.Standing)
            {
                if (!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                {
                    string target = string.Empty;
                    var verb = GetVerb(ref input).Trim();
                    if(verb.Length == 1)
                    {
                        target = input.Remove(0, 1).Trim();
                    }
                    else
                    {
                        target = input.Replace(verb, string.Empty).Trim();
                    }
                    if (!string.IsNullOrEmpty(target))
                    {
                        var t = GetTargetNPC(ref desc, target);
                        if (t != null && t.FollowingPlayer == Guid.Empty)
                        {
                            if (!t.BehaviourFlags.HasFlag(NPCFlags.NoAttack) && t.Position != ActorPosition.Fighting)
                            {
                                if (!desc.Player.Visible)
                                {
                                    desc.Player.Visible = true;
                                    desc.Send($"You shimmer and become visible again.{Constants.NewLine}");
                                }
                                var myInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity));
                                var mobInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(t.Stats.Dexterity));
                                myInit = desc.Player.HasSkill("Awareness") ? myInit += 4 : myInit;
                                mobInit = t.HasSkill("Awareness") ? mobInit += 4 : mobInit;
                                var participants = new List<(uint Initiative, dynamic Participant, dynamic Target)>
                                {
                                    (myInit, desc, t),
                                    (mobInit, t, desc)
                                };
                                if (desc.Player.FollowerID != Guid.Empty)
                                {
                                    var f = NPCManager.Instance.GetNPCByGUID(desc.Player.FollowerID);
                                    if (f != null)
                                    {
                                        var followerInit = Helpers.RollDice(1, 20);
                                        if (ActorStats.CalculateAbilityModifier(f.Stats.Dexterity) > 0)
                                        {
                                            followerInit += Convert.ToUInt32(ActorStats.CalculateAbilityModifier(f.Stats.Dexterity));
                                        }
                                        participants.Add((followerInit, f, t));
                                        participants.Add((mobInit, t, f));
                                    }
                                    else
                                    {
                                        desc.Player.FollowerID = Guid.Empty;
                                        Game.LogMessage($"DEBUG: Setting {desc.Player}'s FollowerID to Guid.Empty as no matching NPC could be found", LogLevel.Debug, true);
                                    }
                                }
                                var g = CombatManager.Instance.AddCombatSession(new CombatSession
                                {
                                    Participants = participants
                                });
                                desc.Player.CombatSessionID = g;
                                desc.Player.Position = ActorPosition.Fighting;
                            }
                            else
                            {
                                desc.Send($"Some otherworldly force prevents you from attacking {t.Name}...{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            if(t == null)
                            {
                                desc.Send($"That doesn't seem to be here...{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"You cannot attack someone's follower!{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"Start a fight with what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"A feeling of peace comes over you... It would be impossible to fight here!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You're not in a position to do that right now.{Constants.NewLine}");
            }
        }

        private static void DescribeSkill(ref Descriptor desc, ref string input)
        {
            var skillName = input.Replace(GetVerb(ref input), string.Empty).Trim();
            if(Skills.SkillExists(skillName))
            {
                var s = Skills.GetSkill(skillName);
                desc.Send($"{s.Name} {s.Description.ToLower()}{Constants.NewLine}");
            }
            else
            {
                if(Spells.SpellExists(skillName))
                {
                    var s = Spells.GetSpell(skillName);
                    desc.Send($"{s.SpellName} {s.Description.ToLower()}{Constants.NewLine}");
                }
                else
                {
                    var r = RecipeManager.Instance.GetRecipe(skillName);
                    if(r != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Name: {r.RecipieName}");
                        sb.AppendLine($"Description: {r.RecipieDescription}");
                        sb.AppendLine($"Produces: {ItemManager.Instance.GetItemByID(r.RecipeResult).Name}");
                        sb.AppendLine("Requires:");
                        foreach(var m in r.RequiredMaterials)
                        {
                            sb.AppendLine($"{Constants.TabStop}{m.Value} x {ItemManager.Instance.GetItemByID(m.Key).Name}");
                        }
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"No such skill, spell or recipe could be found.{Constants.NewLine}");
                    }
                }
            }
        }

        private static void DoDiceGamble(ref Descriptor desc, ref string input)
        {
            if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Gambler))
            {
                var amount = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if (!string.IsNullOrEmpty(amount))
                {
                    if (uint.TryParse(amount, out uint gpBet))
                    {
                        if (gpBet > desc.Player.Stats.Gold)
                        {
                            desc.Send($"You can't wager more gold than you have!{Constants.NewLine}");
                        }
                        else
                        {
                            var playerRoll1 = Helpers.RollDice(1, 100);
                            var playerRoll2 = Helpers.RollDice(1, 100);
                            var dicerRoll = Helpers.RollDice(1, 100);
                            uint playerFinalRoll = 0;
                            if (desc.Player.HasSkill("Gambling"))
                            {
                                playerFinalRoll = playerRoll1 <= playerRoll2 ? playerRoll2 : playerRoll1;
                            }
                            else
                            {
                                playerFinalRoll = playerRoll1;
                            }
                            if (playerFinalRoll > dicerRoll)
                            {
                                var winnings = Convert.ToUInt32(Math.Round((double)gpBet / 2, 0));
                                desc.Player.Stats.Gold += winnings;
                                desc.Send($"You rolled {playerFinalRoll}, the Dicer rolled {dicerRoll}! You win {winnings} gold!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Player.Stats.Gold -= gpBet;
                                desc.Send($"You rolled {playerFinalRoll}, the Dicer rolled {dicerRoll}! You lose your wager of {gpBet} gold!{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"You can't bet that, whole numbers of GP only!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You must place a wager to win anything!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no one here to wager with!{Constants.NewLine}");
            }
        }

        private static void ShowAllEmotes(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            var emotes = EmoteManager.Instance.GetAllEmotes(string.Empty).OrderBy(x => x.EmoteName).ToList();
            if(emotes != null && emotes.Count > 0)
            {
                int i = 0;
                string l = "|| ";
                for(int t = 0; t < emotes.Count; t++)
                {
                    l = $"{l}{emotes[t].EmoteName}{Constants.TabStop}{Constants.TabStop}";
                    i++;
                    if(i >= 5 && t < emotes.Count)
                    {
                        i = 0;
                        sb.AppendLine(l.Trim());
                        l = "|| ";
                    }
                    if(i == emotes.Count)
                    {
                        sb.AppendLine(l.Trim());
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                desc.Send(sb.ToString());
            }
            else
            {
                desc.Send($"No Emotes are currently in the game{Constants.NewLine}");
            }
        }

        private static void DoEmote(ref Descriptor desc, ref string input, Emote e)
        {
            var targetString = input.Replace(GetVerb(ref input), string.Empty).Trim();
            e.ShowEmoteMessage(ref desc, targetString);
        }

        private static void DoRecall(ref Descriptor desc)
        {
            var r = RoomManager.Instance.GetRoom(Constants.PlayerStartRoom());
            if(r != null && !r.Flags.HasFlag(RoomFlags.NoTeleport))
            {
                var cr = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
                if(!cr.Flags.HasFlag(RoomFlags.NoTeleport))
                {
                    desc.Player.Move(desc.Player.CurrentRoom, Constants.PlayerStartRoom(), true, ref desc);
                    if(cr.PlayersInRoom(cr.RoomID) != null && cr.PlayersInRoom(cr.RoomID).Count > 0)
                    {
                        foreach(var p in cr.PlayersInRoom(cr.RoomID))
                        {
                            if(!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} offers a prayer to the Gods and vanishes!{Constants.NewLine}"
                                    : $"The Winds of Magic swirl and something is taken away!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                    if(r.PlayersInRoom(r.RoomID) != null && r.PlayersInRoom(r.RoomID).Count > 1)
                    {
                        foreach(var p in r.PlayersInRoom(r.RoomID))
                        {
                            if(!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} appears in a flash of magic!{Constants.NewLine}"
                                    : $"There is a flash of magic as something arrives!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"The gods refuse to answer your prayers... You're on your own!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"The gods refuse to answer your prayers... You're on your own!{Constants.NewLine}");
            }
        }

        private static void DonateItem(ref Descriptor desc, ref string input)
        {
            var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
            var i = GetTargetItem(ref desc, target, true);
            var donRoom = RoomManager.Instance.GetRoom(Constants.DonationRoomRid());
            if(donRoom != null)
            {
                if(i != null)
                {
                    desc.Player.Inventory.Remove(i);
                    RoomManager.Instance.AddItemToRoomInventory(Constants.DonationRoomRid(), ref i);
                    Game.LogMessage($"INFO: Player {desc.Player.Name} donated item {i.Name} ({i.Id})", LogLevel.Info, true);
                    desc.Send($"You offer up {i.ShortDescription} to the Winds of Magic!{Constants.NewLine}");
                    var donRoomPlayers = donRoom.PlayersInRoom(donRoom.RoomID);
                    var localPlayers = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom);
                    if(localPlayers != null && localPlayers.Count > 1)
                    {
                        foreach(var p in localPlayers)
                        {
                            if(!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} offers {i.ShortDescription} to the Winds of Magic!{Constants.NewLine}"
                                    : $"Something offers {i.ShortDescription} to the Winds of Magic!";
                                p.Send(msg);
                            }
                        }
                    }
                    if(donRoomPlayers != null && donRoomPlayers.Count > 0)
                    {
                        foreach(var p in donRoomPlayers)
                        {
                            p.Send($"The Winds of Magic swirl, depositing {i.ShortDescription} on the ground!{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"You don't seem to be carrying that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no donation room in the world!{Constants.NewLine}");
                Game.LogMessage($"WARN: Player {desc.Player.Name} tried to donate an item, but there is no Donation Room in the game", LogLevel.Warning, true);
            }
        }

        private static void AppraiseItemForSale(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if(r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                if(s != null)
                {
                    var criteria = input.Replace(GetVerb(ref input), string.Empty).Trim();
                    if(!string.IsNullOrWhiteSpace(criteria))
                    {
                        var i = GetTargetItem(ref desc, criteria, true);
                        if(i != null)
                        {
                            desc.Send($"The shopkeeper smiles and says, 'I'll give you {Helpers.GetNewSalePrice(ref desc, i.BaseValue)} gold for that.'{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void ListShopWares(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if(r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var criteria = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                if(s != null)
                {
                    s.ListShopInventory(ref desc, criteria);
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void SellItemToShop(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if(r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                if(s != null)
                {
                    var criteria = input.Replace(GetVerb(ref input), string.Empty).Trim();
                    if(!string.IsNullOrEmpty(criteria))
                    {
                        var i = GetTargetItem(ref desc, criteria, true);
                        if(i != null)
                        {
                            var salePrice = Helpers.GetNewSalePrice(ref desc, i.BaseValue);
                            desc.Player.Inventory.Remove(i);
                            desc.Player.Stats.Gold += salePrice;
                            desc.Send($"You hand over {i.ShortDescription} and pocket the {salePrice} gold from the shopkeeper{Constants.NewLine}");
                            var playersInRoom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom);
                            if(playersInRoom != null && playersInRoom.Count > 1)
                            {
                                foreach(var p in playersInRoom)
                                {
                                    if(!Regex.Match(desc.Player.Name, p.Player.Name, RegexOptions.IgnoreCase).Success)
                                    {
                                        var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} hands {i.ShortDescription} to the shopkeeper and gets a stack of gold in return!{Constants.NewLine}"
                                            : $"Something hands over {i.ShortDescription} to the shopkeeper and gets a stack of gold in return!{Constants.NewLine}";
                                        p.Send(msg);
                                    }
                                }
                            }
                        }
                        else
                        {
                            desc.Send($"You don't seem to be carrying anything like that...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Sell what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void BuyItemFromShop(ref Descriptor desc, ref string input)
        {
            var r = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom);
            if(r.Flags.HasFlag(RoomFlags.Shop) && r.ShopID.HasValue)
            {
                var s = ShopManager.Instance.GetShop(r.ShopID.Value);
                var criteria = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if(s != null)
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        s.BuyItem(ref desc, ref criteria);
                    }
                    else
                    {
                        desc.Send($"Buy what, exactly?{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"The store appears broken - check with an Imm!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"There is no shop here...{Constants.NewLine}");
            }
        }

        private static void LearnSkillOrSpell(ref Descriptor desc, ref string input)
        {
            // learn <skill | spell | recipe> <name>
            var elements = TokeniseInput(ref input);
            if(elements.Length == 1)
            {
                // no criteria specified so show what we can learn here, if anything
                if(!RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasTrainer())
                {
                    desc.Send($"There is no one here to teach you!{Constants.NewLine}");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.SkillTrainer))
                {
                    sb.AppendLine("The trainer smiles and says: 'I can teach you... For a price!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Skill");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint skillsAvailable = 0;
                    foreach (var s in Skills.GetAllSkills(string.Empty).OrderBy(x => x.Name).ToList())
                    {
                        if(!desc.Player.HasSkill(s.Name) || (s.Name == "Extra Attack" && desc.Player.NumberOfAttacks + 1 <= 5))
                        {
                            skillsAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {s.Name}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {s.Name}");
                            }
                        }
                    }
                    if(skillsAvailable == 0)
                    {
                        sb.AppendLine("|| No skills available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine();
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.MagicTrainer))
                {
                    sb.AppendLine("The sorceror smiles. 'Magic? I can teach you... For a price!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Spell");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint spellsAvailable = 0;
                    foreach(var s in Spells.GetAllSpells(string.Empty).OrderBy(x => x.SpellName).ToList())
                    {
                        if(!desc.Player.HasSpell(s.SpellName))
                        {
                            spellsAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                            if(p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {s.SpellName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {s.SpellName}");
                            }
                        }
                    }
                    if (spellsAvailable == 0)
                    {
                        sb.AppendLine("|| No spells available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Scribe))
                {
                    // show scribe recipes
                    sb.AppendLine("The scribe flashes a toothy grin. 'Certainly! Sit! Learn!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Scribing).ToList();
                    foreach(var recipe in r)
                    {
                        if(!desc.Player.KnowsRecipe(recipe.RecipieName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if(p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Scribe recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Alchemist))
                {
                    // show alchemy recipes
                    sb.AppendLine("The alchemist gives you a sickly grin. 'Certainly! Sit! Learn!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Alchemy).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipieName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Alchemy recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Blacksmith))
                {
                    // show blacksmith recipes
                    sb.AppendLine("The blacksmith flexes his mighty arms. 'The secrets of the Forge can be yours...'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Blacksmithing).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipieName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Blacksmithing recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Jeweler))
                {
                    sb.AppendLine("The jeweler smiles broadly. 'Certainly! Sit! Learn!'");
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Price{Constants.TabStop}|| Recipe");
                    sb.AppendLine($"||==============||{new string('=', 61)}");
                    uint recipesAvailable = 0;
                    var r = RecipeManager.Instance.GetAllCraftingRecipes(string.Empty).Where(x => x.RecipeType == RecipeType.Jewelcrafting).ToList();
                    foreach (var recipe in r)
                    {
                        if (!desc.Player.KnowsRecipe(recipe.RecipieName))
                        {
                            recipesAvailable++;
                            var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                            if (p.ToString().Length > 4)
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p}{Constants.TabStop}{Constants.TabStop}|| {recipe.RecipieName}");
                            }
                        }
                    }
                    if (recipesAvailable == 0)
                    {
                        sb.AppendLine("|| No Jewelcrafting recipes available");
                    }
                    sb.AppendLine($"  {new string('=', 77)}");
                }
                desc.Send(sb.ToString());
            }
            else
            {
                if(elements.Length >= 2)
                {
                    // we are learning a skill or a spell
                    var t = elements[1];
                    var toLearn = input.Replace(elements[0], string.Empty).Replace(elements[1], string.Empty).Trim();
                    if(t.ToLower() == "skill")
                    {
                        if(Skills.SkillExists(toLearn))
                        {
                            if(!desc.Player.HasSkill(toLearn) || (toLearn.ToLower() == "extra attack" && desc.Player.NumberOfAttacks + 1 <= 5))
                            {
                                var s = Skills.GetSkill(toLearn);
                                var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                                if(desc.Player.Stats.Gold >= p)
                                {
                                    // skill exists, player does not know it and has enough gold to buy it
                                    desc.Send($"The trainer smiles. 'Certainly I can teach you that!'{Constants.NewLine}");
                                    desc.Player.Stats.Gold -= p;
                                    if(s.Name == "Extra Attack")
                                    {
                                        desc.Player.NumberOfAttacks++;
                                    }
                                    else
                                    {
                                        desc.Player.AddSkill(s.Name);
                                    }
                                }
                                else
                                {
                                    // skill exists, player does not know it but does not have enough gold to buy it
                                    desc.Send($"The trainer laughs, 'You're a little short of gold, my friend!'{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                // skill exists but the player already knows it
                                desc.Send($"'You already know all that I can teach about that,' the trainer says.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            // skill does not exist
                            desc.Send($"The trainer shakes their head. 'I don't think I can teach you that.'{Constants.NewLine}");
                        }
                    }
                    if(t.ToLower() == "spell")
                    {
                        if(Spells.SpellExists(toLearn))
                        {
                            if (!desc.Player.HasSpell(toLearn))
                            {
                                var s = Spells.GetSpell(toLearn);
                                var p = Helpers.GetNewPurchasePrice(ref desc, s.GoldToLearn);
                                if(desc.Player.Stats.Gold >= p)
                                {
                                    desc.Send($"The sorceror smiles. 'Certainly I can teach you that!'{Constants.NewLine}");
                                    desc.Player.Stats.Gold -= p;
                                    desc.Player.AddSpell(s.SpellName);
                                }
                                else
                                {
                                    desc.Send($"The sorceror smiles. 'Come back with more gold.'{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"'You already know all I can teach about that spell,' the sorceror says.{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"The sorceror shakes his head. 'I don't think I can teach you that.'{Constants.NewLine}");
                        }
                    }
                    if(t.ToLower() == "recipe")
                    {
                        var r = RecipeManager.Instance.GetRecipe(toLearn);
                        if(r != null)
                        {
                            if(!desc.Player.KnowsRecipe(r.RecipieName))
                            {
                                bool canLearn = false;
                                switch(r.RecipeType)
                                {
                                    case RecipeType.Scribing:
                                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Scribe) && desc.Player.HasSkill("Scribing"))
                                        {
                                            canLearn = true;
                                        }
                                        break;

                                    case RecipeType.Jewelcrafting:
                                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Jeweler) && desc.Player.HasSkill("Jewelcrafting"))
                                        {
                                            canLearn = true;
                                        }
                                        break;

                                    case RecipeType.Blacksmithing:
                                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Blacksmith) && desc.Player.HasSkill("Blacksmithing"))
                                        {
                                            canLearn = true;
                                        }
                                        break;

                                    case RecipeType.Alchemy:
                                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Alchemist) && desc.Player.HasSkill("Alchemy"))
                                        {
                                            canLearn = true;
                                        }
                                        break;
                                }
                                if(canLearn)
                                {
                                    var p = Helpers.GetNewPurchasePrice(ref desc, 2000);
                                    if(desc.Player.Stats.Gold >= p)
                                    {
                                        desc.Player.Stats.Gold -= p;
                                        desc.Player.KnownRecipes.Add(r);
                                        desc.Send($"You gain knowledge of crafting {r.RecipieName}{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        desc.Send($"You don't have enough gold!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    desc.Send($"You cannot learn that recipe right now.{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                desc.Send($"You already know that recipe!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That recipe doesn't exist!{Constants.NewLine}");
                        }
                    }
                    if(t.ToLower() != "spell" && t.ToLower() != "skill" && t.ToLower() != "recipe")
                    {
                        desc.Send($"Usage: learn <skill | spell | recipe> <name>{Constants.NewLine}");
                    }
                }
            }
        }

        private static void DrinkPotion(ref Descriptor desc, ref string input)
        {
            var potionName = input.Replace(GetVerb(ref input), string.Empty).Trim();
            var i = GetTargetItem(ref desc, potionName, true);
            string msgToSendToPlayer = string.Empty;
            string msgToSendToOthers = string.Empty;
            if (i != null)
            {
                if(i.ItemType == ItemType.Potion)
                {
                    var pn = desc.Player.Name;
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => x.Player.Name != pn).ToList();
                    desc.Send($"With a few gulps you drink the {i.Name}{Constants.NewLine}");
                    desc.Player.Inventory.Remove(i);
                    if (localPlayers != null && localPlayers.Count > 0)
                    {
                        foreach (var lp in localPlayers)
                        {
                            if (desc.Player.Visible || lp.Player.Level >= Constants.ImmLevel)
                            {
                                lp.Send($"{desc.Player} gulps down the {i.Name}.{Constants.NewLine}");
                            }
                            else
                            {
                                lp.Send($"There is a strange noise as something gulps down a drink!{Constants.NewLine}");
                            }
                        }
                    }
                    if (i.AppliesBuff)
                    {
                        desc.Send($"You feel the magic of the potion coursing through you!{Constants.NewLine}");
                        foreach (var b in i.AppliedBuffs)
                        {
                            var buff = Buffs.GetBuff(b);
                            if (buff != null)
                            {
                                desc.Player.AddBuff(buff.BuffName);
                            }
                        }
                    }
                    if (i.NumberOfDamageDice > 0)
                    {
                        var result = Helpers.RollDice(i.NumberOfDamageDice, i.SizeOfDamageDice);
                        if (i.IsToxic)
                        {
                            if (desc.Player.Stats.CurrentHP - result <= 0)
                            {
                                desc.Send($"{i.Name} is toxic and burns its way through you, killing you!{Constants.NewLine}");
                                desc.Player.Kill(ref desc);
                            }
                            else
                            {
                                desc.Send($"{i.Name} is toxic and burns its way through you causing {result} damage!{Constants.NewLine}");
                                desc.Player.Stats.CurrentHP -= (int)result;
                            }
                        }
                        else
                        {
                            if (desc.Player.Stats.CurrentHP + result > desc.Player.Stats.MaxHP)
                            {
                                desc.Send($"{i.Name} restores your vitality!{Constants.NewLine}");
                                desc.Player.Stats.CurrentHP = (int)desc.Player.Stats.MaxHP;
                            }
                            else
                            {
                                desc.Send($"{i.Name} restores your vitality, healing {result} damage!{Constants.NewLine}");
                                desc.Player.Stats.CurrentHP += (int)result;
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"You can't drink that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You don't seem to be carrying that...{Constants.NewLine}");
            }
        }

        private static void GiveItemToTarget(ref Descriptor desc, ref string input)
        {
            var targetPlayer = TokeniseInput(ref input).Last().Trim();
            var verb = GetVerb(ref input).Trim();
            var itemName = input.Replace(verb, string.Empty).Replace(targetPlayer, string.Empty).Trim();
            var i = GetTargetItem(ref desc, itemName, true);
            if (i != null)
            {
                var p = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, targetPlayer, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if (p != null)
                {
                    if (desc.Player.Level >= Constants.ImmLevel || p.Player.Visible)
                    {
                        desc.Player.Inventory.Remove(i);
                        p.Player.Inventory.Add(i);
                        desc.Send($"You give {i.Name} to {p.Player.Name}{Constants.NewLine}");
                        string msgToSendToTarget = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} has given you {i.Name}! How kind!{Constants.NewLine}"
                            : $"Something has given you {i.Name}. How strange!{Constants.NewLine}";
                        p.Send(msgToSendToTarget);
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (localPlayers != null && localPlayers.Count - 2 >= 1)
                        {
                            foreach (var lp in localPlayers)
                            {
                                if (lp.Player.Name != desc.Player.Name && lp.Player.Name != p.Player.Name)
                                {
                                    if (lp.Player.Level >= Constants.ImmLevel)
                                    {
                                        lp.Send($"{desc.Player.Name} hands {i.Name} to {p.Player.Name}{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        var g = desc.Player.Visible ? desc.Player.Name : "Something";
                                        var r = p.Player.Visible ? p.Player.Name : "something";
                                        lp.Send($"{g} hands {i.Name} to {r}{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"That person isn't around right now...{Constants.NewLine}");
                    }
                }
                else
                {
                    var n = GetTargetNPC(ref desc, targetPlayer);
                    if(n != null)
                    {
                        desc.Send($"You hand over {i.Name} to {n.Name}!{Constants.NewLine}");
                        n.Inventory.Add(i);
                        desc.Player.Inventory.Remove(i);
                    }
                    else
                    {
                        desc.Send($"That person isn't around right now...{Constants.NewLine}");
                    }
                }
            }
            else
            {
                desc.Send($"You don't seem to be carrying that...{Constants.NewLine}");
            }
        }

        private static void RemoveEquippedItem(ref Descriptor desc, ref string input)
        {
            try
            {
                if (Enum.TryParse(TokeniseInput(ref input).Last(), true, out WearSlot targetSlot))
                {
                    string msgToSendToPlayer = string.Empty;
                    string[] msgToSendToOthers = { string.Empty, string.Empty };
                    switch (targetSlot)
                    {
                        case WearSlot.Head:
                            if (desc.Player.EquippedItems.Head != null)
                            {
                                var i = desc.Player.EquippedItems.Head;
                                desc.Player.EquippedItems.Head = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from your head{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from their head{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if(i.AppliesBuff)
                                {
                                    foreach(var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything on your head!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Neck:
                            if (desc.Player.EquippedItems.Neck != null)
                            {
                                var i = desc.Player.EquippedItems.Neck;
                                desc.Player.EquippedItems.Neck = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from around your neck{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from around their neck{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything around your neck!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Armour:
                            if (desc.Player.EquippedItems.Armour != null)
                            {
                                var i = desc.Player.EquippedItems.Armour;
                                desc.Player.EquippedItems.Armour = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You stop wearing {i.Name} as your armour{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} stops wearing {i.Name} as their armour{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing any armour!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.FingerLeft:
                            if (desc.Player.EquippedItems.FingerLeft != null)
                            {
                                var i = desc.Player.EquippedItems.FingerLeft;
                                desc.Player.EquippedItems.FingerLeft = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from a finger on your left hand{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from a finger on their left hand{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything on the fingers of your left hand!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.FingerRight:
                            if (desc.Player.EquippedItems.FingerRight != null)
                            {
                                var i = desc.Player.EquippedItems.FingerRight;
                                desc.Player.EquippedItems.FingerRight = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You remove {i.Name} from a finger on your right hand{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} removes {i.Name} from a finger on the right hand{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't wearing anything on the fingers of your right hand!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Weapon:
                            if (desc.Player.EquippedItems.Weapon != null)
                            {
                                var i = desc.Player.EquippedItems.Weapon;
                                desc.Player.EquippedItems.Weapon = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You stop wielding {i.Name} as your weapon{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} stops using {i.Name} as their weapon{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't using anything as a weapon!{Constants.NewLine}";
                            }
                            break;

                        case WearSlot.Held:
                            if (desc.Player.EquippedItems.Held != null)
                            {
                                var i = desc.Player.EquippedItems.Held;
                                desc.Player.EquippedItems.Held = null;
                                desc.Player.Inventory.Add(i);
                                msgToSendToPlayer = $"You stop holding {i.Name}{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} stops holding {i.Name}{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                if (i.AppliesBuff)
                                {
                                    foreach (var b in i.AppliedBuffs)
                                    {
                                        desc.Player.RemoveBuff(b);
                                    }
                                    RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                }
                                desc.Player.CalculateArmourClass();
                            }
                            else
                            {
                                msgToSendToPlayer = $"You aren't holding anything!{Constants.NewLine}";
                            }
                            break;

                        default:
                            msgToSendToPlayer = $"Sorry, I didn't understand that{Constants.NewLine}";
                            break;
                    }
                    desc.Send(msgToSendToPlayer);
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    if (localPlayers != null && localPlayers.Count - 1 >= 1)
                    {
                        foreach (var p in localPlayers)
                        {
                            if (p != desc)
                            {
                                if (desc.Player.Visible)
                                {
                                    p.Send(msgToSendToOthers[0]);
                                }
                                else
                                {
                                    if (p.Player.Level >= Constants.ImmLevel)
                                    {
                                        p.Send(msgToSendToOthers[0]);
                                    }
                                    else
                                    {
                                        p.Send(msgToSendToOthers[1]);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    desc.Send($"Remove that from where, exactly?{Constants.NewLine}");
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"Player {desc.Player.Name} had an error removing an item: {ex.Message}", LogLevel.Error, true);
                desc.Send($"That didn't work!{Constants.NewLine}");
            }
        }

        private static void EquipItem(ref Descriptor desc, ref string input)
        {
            try
            {
                if (Enum.TryParse(TokeniseInput(ref input).Last(), true, out WearSlot targetSlot))
                {
                    var itemName = input.Replace(GetVerb(ref input), string.Empty).Replace(targetSlot.ToString().ToLower(), string.Empty).Trim();
                    var item = desc.Player.Inventory.Where(x => Regex.Match(x.Name, itemName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    string msgToSendToPlayer = string.Empty;
                    string[] msgToSendToOthers = { string.Empty, string.Empty };
                    if (item != null && (item.ItemType == ItemType.Armour || item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Ring))
                    {
                        switch (targetSlot)
                        {
                            case WearSlot.Head:
                                if(item.CanPlayerEquip(ref desc, WearSlot.Head, out string reply))
                                {
                                    desc.Player.EquippedItems.Head = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} on your head{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} on their head{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach(var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Neck:
                                if(item.CanPlayerEquip(ref desc, WearSlot.Neck, out reply))
                                {
                                    desc.Player.EquippedItems.Neck = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} around your neck{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} around their neck{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Armour:
                                if(item.CanPlayerEquip(ref desc, WearSlot.Armour, out reply))
                                {
                                    desc.Player.EquippedItems.Armour = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You don {item.Name} as your armour{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} dons {item.Name} as their armour{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.FingerLeft:
                                if(item.CanPlayerEquip(ref desc, WearSlot.FingerLeft, out reply))
                                {
                                    desc.Player.EquippedItems.FingerLeft = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} on a finger on your left hand{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} on a finger on their left hand{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.FingerRight:
                                if(item.CanPlayerEquip(ref desc, WearSlot.FingerRight, out reply))
                                {
                                    desc.Player.EquippedItems.FingerRight = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You put {item.Name} on a finger on your right hand{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} puts {item.Name} on a finger on their right hand{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Weapon:
                                if(item.CanPlayerEquip(ref desc, WearSlot.Weapon, out reply))
                                {
                                    desc.Player.EquippedItems.Weapon = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You wield {(item).Name} as your weapon{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} wields {(item).Name} as their weapon{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            case WearSlot.Held:
                                if(item.CanPlayerEquip(ref desc, WearSlot.Held, out reply))
                                {
                                    desc.Player.EquippedItems.Held = item;
                                    desc.Player.Inventory.Remove(item);
                                    msgToSendToPlayer = $"You hold {(item).Name} in your off-hand{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} holds {(item).Name} in their off-hand{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"There is a slight shimmer as something moves{Constants.NewLine}";
                                    if (item.AppliesBuff)
                                    {
                                        foreach (var b in item.AppliedBuffs)
                                        {
                                            desc.Player.AddBuff(b, true);
                                        }
                                        RoomManager.Instance.ProcessEnvironmentBuffs(desc.Player.CurrentRoom);
                                    }
                                    desc.Player.CalculateArmourClass();
                                }
                                else
                                {
                                    desc.Send(reply);
                                }
                                break;

                            default:
                                desc.Send($"That doesn't seem to work...{Constants.NewLine}");
                                break;
                        }
                        desc.Send(msgToSendToPlayer);
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (localPlayers != null && localPlayers.Count - 1 >= 1)
                        {
                            foreach (var p in localPlayers)
                            {
                                if (p != desc)
                                {
                                    if (desc.Player.Visible)
                                    {
                                        p.Send(msgToSendToOthers[0]);
                                    }
                                    else
                                    {
                                        if (p.Player.Level >= Constants.ImmLevel)
                                        {
                                            p.Send(msgToSendToOthers[0]);
                                        }
                                        else
                                        {
                                            p.Send(msgToSendToOthers[1]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"Try as you might, you can't seem to make that work...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Equip that where, exactly?{Constants.NewLine}");
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"Player {desc.Player.Name} encountered an error equipping an item: {ex.Message}", LogLevel.Error, true);
                desc.Send($"Sorry, I didn't understand that{Constants.NewLine}");
            }
        }

        private static void ShowEquippedItems(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine("|| You are wearing the following equipment:");
            sb.AppendLine($"|| Head:{Constants.TabStop}{desc.Player.EquippedItems.Head}");
            sb.AppendLine($"|| Neck:{Constants.TabStop}{desc.Player.EquippedItems.Neck}");
            sb.AppendLine($"|| Armour:{Constants.TabStop}{desc.Player.EquippedItems.Armour}");
            sb.AppendLine($"|| Finger (L):{Constants.TabStop}{desc.Player.EquippedItems.FingerLeft}");
            sb.AppendLine($"|| Finger (R):{Constants.TabStop}{desc.Player.EquippedItems.FingerRight}");
            sb.AppendLine($"|| Weapon:{Constants.TabStop}{desc.Player.EquippedItems.Weapon}");
            sb.AppendLine($"|| Held:{Constants.TabStop}{desc.Player.EquippedItems.Held}");
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void DeleteCharacter(ref Descriptor desc)
        {
            bool delChar = false;
            bool validInput = false;
            Game.LogMessage($"{desc.Player.Name} has requested character deletion", LogLevel.Info, true);
            while (!validInput)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("This will delete your character from the game and disconnect your session.");
                sb.AppendLine("This cannot be undone and deleted characters cannot be restored.");
                sb.Append("Do you wish to continue (yes/no): ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (ValidateInput(input))
                {
                    validInput = input.ToLower() == "yes" || input.ToLower() == "no";
                    delChar = validInput && input.ToLower() == "yes";
                }
            }
            if (delChar)
            {
                Game.LogMessage($"{desc.Player.Name} has confirmed requested deletion of character data.", LogLevel.Info, true);
                if (DatabaseManager.DeleteCharacter(ref desc))
                {
                    SessionManager.Instance.Close(desc);
                }
                else
                {
                    desc.Send($"There was an error deleting your character.{Constants.NewLine}");
                    desc.Send($"Please contact an Immortal for assistance.{Constants.NewLine}");
                }
            }
        }

        private static void GetItemFromRoom(ref Descriptor desc, ref string input)
        {
            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom != null && RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom.Count > 0)
            {
                string obj = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if (!string.IsNullOrEmpty(obj))
                {
                    var i = GetTargetItem(ref desc, obj, false);
                    if (i != null)
                    {
                        RoomManager.Instance.RemoveItemFromRoomInventory(desc.Player.CurrentRoom, ref i);
                        desc.Player.Inventory.Add(i);
                        desc.Send($"You pick up {i.ShortDescription}{Constants.NewLine}");
                        var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (playersToNotify != null && playersToNotify.Count > 1)
                        {
                            foreach (var p in playersToNotify)
                            {
                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                {
                                    if (p.Player.Name != desc.Player.Name)
                                    {
                                        p.Send($"{desc.Player.Name} takes {i.ShortDescription}... Hope no one needed that!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    p.Send($"Something takes {i.ShortDescription}...{Constants.NewLine}");
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"That item doesn't seem to be here...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"That item doesn't seem to be here...{Constants.NewLine}");
            }
        }

        private static void DropCharacterItem(ref Descriptor desc, ref string input)
        {
            if (desc.Player.Inventory != null && desc.Player.Inventory.Count > 0)
            {
                string obj = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if (!string.IsNullOrEmpty(obj))
                {
                    var i = GetTargetItem(ref desc, obj, true);
                    if (i != null)
                    {
                        // we have an object from the inventory to drop, remove from player and add to room
                        desc.Player.Inventory.Remove(i);
                        RoomManager.Instance.AddItemToRoomInventory(desc.Player.CurrentRoom, ref i);
                        desc.Send($"You drop {i.Name} on the floor{Constants.NewLine}");
                        var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (playersToNotify.Count > 1)
                        {
                            foreach (var p in playersToNotify)
                            {
                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                {
                                    if (p.Player.Name != desc.Player.Name)
                                    {
                                        p.Send($"{desc.Player.Name} drops {i.ShortDescription} to the floor. What a litterbug!{Constants.NewLine}");
                                    }
                                }
                                else
                                {
                                    p.Send($"Something drops {i.ShortDescription} to the floor... How strange!{Constants.NewLine}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // couldn't find a matching item in the player's inventory to drop
                        desc.Send($"You don't seem to be carrying that with you...{Constants.NewLine}");
                    }
                }
                else
                {
                    // not able to determine the object to drop based off player input
                    desc.Send($"Sorry, I didn't understand that...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"You aren't carrying anything to drop...{Constants.NewLine}");
            }
        }

        private static void DoHideSkill(ref Descriptor desc)
        {
            if (desc.Player.Position == ActorPosition.Standing && desc.Player.HasSkill("Hide") && desc.Player.CombatSessionID == Guid.Empty)
            {
                var pname = desc.Player.Name;
                if (desc.Player.Visible)
                {
                    if (desc.Player.Stats.CurrentMP > Skills.GetSkill("Hide").MPCost)
                    {
                        desc.Player.Visible = false;
                        desc.Player.Stats.CurrentMP -= (int)Skills.GetSkill("Hide").MPCost;
                        desc.Send($"With cunning and skill you hide yourself from view!{Constants.NewLine}");
                        var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        if (playersToNotify != null && playersToNotify.Count > 1)
                        {
                            foreach(var p in playersToNotify.Where(x => x.Player.Name != pname))
                            {
                                p.Send($"{pname} hides and becomes impossible to see!{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"You don't have the energy to do that!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Player.Visible = true;
                    desc.Send($"You stop hiding and become visible again!{Constants.NewLine}");
                    var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    if (playersToNotify != null && playersToNotify.Count > 1)
                    {
                        foreach (var p in playersToNotify.Where(x => x.Player.Name != pname))
                        {
                            p.Send($"{pname} stops hiding and becomes visible again!{Constants.NewLine}");
                        }
                    }
                }
            }
            else
            {
                desc.Send($"You cannot use that skill right now.{Constants.NewLine}");
            }
        }

        private static void ShowCharInventory(ref Descriptor desc)
        {
            if (desc.Player.Inventory != null && desc.Player.Inventory.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                sb.AppendLine($"|| You are carrying:");
                foreach (var i in desc.Player.Inventory.Select(x => new { x.Id, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                {
                    var cnt = desc.Player.Inventory.Where(y => y.Id == i.Id).Count();
                    sb.AppendLine($"|| {cnt} x {i.Name}, {i.ShortDescription}");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                desc.Send(sb.ToString());
            }
            else
            {
                desc.Send($"You are not carrying anything{Constants.NewLine}");
            }
        }

        private static void Who(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| The world currently contains the following beings:");
            var allPlayers = SessionManager.Instance.GetAllPlayers();
            bool isImm = desc.Player.Level >= Constants.ImmLevel;
            if (allPlayers != null && allPlayers.Count > 0)
            {
                foreach (var p in SessionManager.Instance.GetAllPlayers().OrderByDescending(x => x.Player.Level).ToList())
                {
                    switch (isImm)
                    {
                        case true:
                            if (p.Player.Visible)
                            {
                                sb.AppendLine($"|| {p.Player.Name}, the level {p.Player.Level} {p.Player.Class} in room {p.Player.CurrentRoom}");
                            }
                            else
                            {
                                sb.AppendLine($"|| {p.Player.Name}, the level {p.Player.Level} {p.Player.Class} in room {p.Player.CurrentRoom} (Invisible)");
                            }
                            break;

                        case false:
                            if (p.Player.Name == desc.Player.Name || p.Player.Visible)
                            {
                                var playerZone = ZoneManager.Instance.GetZoneForRID(p.Player.CurrentRoom).ZoneName;
                                if (p.Player.Name == desc.Player.Name && !p.Player.Visible)
                                {
                                    // player doing who is not an Imm and is invisible
                                    sb.AppendLine($"|| {p.Player.Name}, the {p.Player.Class} (Invisible) in {playerZone}");
                                }
                                else
                                {
                                    sb.AppendLine($"|| {p.Player.Name}, the {p.Player.Class} in {playerZone}");
                                }
                            }
                            break;
                    }
                }
            }
            sb.AppendLine($"  {new string('=', 77)}{Constants.NewLine}");
            desc.Send(sb.ToString());
        }

        internal static void PushTarget(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).Trim();
            var line = input.Remove(0, verb.Length).ToLower().Trim();
            //var line = input.Replace(GetVerb(ref input), string.Empty).ToLower().Trim();
            string target = string.Empty;
            string direction = string.Empty;
            if (line.IndexOf('\"') > -1)
            {
                target = GetSkillOrSpellName(ref line);
                direction = line.Replace(target, string.Empty).Replace("\"", string.Empty).Trim();
            }
            else
            {
                var elements = TokeniseInput(ref line);
                if(elements != null)
                {
                    try
                    {
                        target = elements[0];
                        direction = elements[elements.Length - 1];
                    }
                    catch
                    {
                        desc.Send($"Usage: Push <target> <direction>{Constants.NewLine}");
                    }
                }
            }
            if(!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(direction))
            {
                // get a reference to the target player or NPC
                object objTgt = null;
                var playersInRoom = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                var npcsInRoom = RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom);
                int targetRID = -1;
                string roomDirection = string.Empty;
                if(playersInRoom != null && playersInRoom.Count > 1)
                {
                    var p = playersInRoom.Where(x => Regex.Match(target, x.Player.Name, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    if(p != null && (p.Player.Visible || desc.Player.Level >= Constants.ImmLevel))
                    {
                        objTgt = p;
                    }
                }
                if(objTgt == null)
                {
                    objTgt = GetTargetNPC(ref desc, target);
                }
                if(objTgt != null)
                {
                    // check to see if we have an exit in the specified direction
                    switch(direction)
                    {
                        case "u":
                        case "up":
                            if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("up"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").ExitDirection;
                                } 
                            }
                            break;

                        case "d":
                        case "down":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("down"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").ExitDirection;
                                }
                            }
                            break;

                        case "n":
                        case "north":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("north"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").ExitDirection;
                                }
                            }
                            break;

                        case "nw":
                        case "northwest":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("northwest"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").ExitDirection;
                                }
                            }
                            break;

                        case "w":
                        case "west":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("west"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").ExitDirection;
                                }
                            }
                            break;

                        case "sw":
                        case "southwest":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("southwest"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").ExitDirection;
                                }
                            }
                            break;

                        case "s":
                        case "south":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("south"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").ExitDirection;
                                }
                            }
                            break;

                        case "se":
                        case "southeast":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("southeast"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").ExitDirection;
                                }
                            }
                            break;

                        case "e":
                        case "east":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("east"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").ExitDirection;
                                }
                            }
                            break;

                        case "ne":
                        case "northeast":
                            if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("northeast"))
                            {
                                var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").RoomDoor;
                                if(d == null || (d != null && d.IsOpen))
                                {
                                    targetRID = Convert.ToInt32(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").DestinationRoomID);
                                    roomDirection = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").ExitDirection;
                                }
                            }
                            break;

                        default:
                            desc.Send($"You can't push something in that direction!{Constants.NewLine}");
                            return;

                    }
                    if(targetRID > -1)
                    {
                        uint newRID = Convert.ToUInt32(targetRID);
                        if(RoomManager.Instance.RoomExists(newRID))
                        {
                            bool destRoomIsNoMob = RoomManager.Instance.GetRoom(newRID).Flags.HasFlag(RoomFlags.NoMobs);
                            var playerRoll = Helpers.RollDice(1, 20);
                            int playerFinalRoll = Convert.ToInt32(playerRoll);
                            var playerStrModifier = ActorStats.CalculateAbilityModifier(desc.Player.Stats.Strength);
                            playerFinalRoll += Convert.ToInt32(playerStrModifier);
                            playerFinalRoll = playerFinalRoll < 1 ? 1 : playerFinalRoll;
                            var targetRoll = Helpers.RollDice(1, 20);
                            int targetFinalRoll = Convert.ToInt32(targetRoll);
                            bool okToPush = false;
                            bool targetIsPlayer = objTgt.GetType() == typeof(Descriptor);
                            string targetName = string.Empty;
                            if (playerRoll > 1 && playerRoll < 20)
                            {
                                // player rolled between 2 and 19 inclusive so compare STR rolls to see who wins
                                if (targetIsPlayer)
                                {
                                    // target is a player
                                    targetName = (objTgt as Descriptor).Player.Name;
                                    var targetStrModifier = ActorStats.CalculateAbilityModifier((objTgt as Descriptor).Player.Stats.Strength);
                                    targetFinalRoll += targetStrModifier;
                                    targetFinalRoll = targetFinalRoll < 1 ? 1 : targetFinalRoll;
                                    okToPush = playerFinalRoll > targetFinalRoll;
                                }
                                else
                                {
                                    // target is an npc
                                    targetName = (objTgt as NPC).Name;
                                    var targetStrModifier = ActorStats.CalculateAbilityModifier((objTgt as NPC).Stats.Strength);
                                    targetFinalRoll += targetStrModifier;
                                    targetFinalRoll = targetFinalRoll < 1 ? 1 : targetFinalRoll;
                                    okToPush = playerFinalRoll > targetFinalRoll;
                                }
                            }
                            if (playerRoll == 20)
                            {
                                // player rolled a nautral 20 so automatically wins
                                okToPush = true;
                            }
                            if (okToPush && targetIsPlayer || (okToPush && !destRoomIsNoMob))
                            {
                                // push the target in the specified direction
                                desc.Send($"With a mighty effort you push {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                if(targetIsPlayer)
                                {
                                    // notify the target player
                                    if (desc.Player.Visible || ((objTgt as Descriptor).Player.Level >= Constants.ImmLevel))
                                    {
                                        (objTgt as Descriptor).Send($"{desc.Player.Name} pushes you {roomDirection.ToLower()}!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        (objTgt as Descriptor).Send($"Something pushes you {roomDirection.ToLower()}!{Constants.NewLine}");
                                    }
                                    if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom).Count > 2)
                                    {
                                        foreach(var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom))
                                        {
                                            if (p.Player.Name != desc.Player.Name && p.Player.Name != (objTgt as Descriptor).Player.Name)
                                            {
                                                if (!desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something pushes something else {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                                if (!desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} pushes something {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // target is an NPC so just notify any other players in the room
                                    foreach (var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom))
                                    {
                                        if (p.Player.Name != desc.Player.Name)
                                        {
                                            if (desc.Player.Visible)
                                            {
                                                p.Send($"{desc.Player.Name} pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                p.Send($"Something pushes {targetName} {roomDirection.ToLower()}!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                                if(targetIsPlayer)
                                {
                                    var p = SessionManager.Instance.GetPlayer((objTgt as Descriptor).Player.Name);
                                    p.Player.Move(p.Player.CurrentRoom, newRID, false, ref p);
                                }
                                else
                                {
                                    var n = NPCManager.Instance.GetNPCByGUID((objTgt as NPC).NPCGuid);
                                    n.Move(ref n, desc.Player.CurrentRoom, newRID, false);
                                }
                            }
                            else
                            {
                                // we failed
                                if(!targetIsPlayer && destRoomIsNoMob)
                                {
                                    desc.Send($"Some mysterious force prevents you from doing that!{Constants.NewLine}");
                                }
                                else
                                {
                                    desc.Send($"Try as you might, you just can't summon the strength to do that!{Constants.NewLine}");
                                }
                                if(targetIsPlayer)
                                {
                                    // notify the target player
                                    if(desc.Player.Visible || ((objTgt as Descriptor).Player.Level >= Constants.ImmLevel))
                                    {
                                        (objTgt as Descriptor).Send($"{desc.Player.Name} tries to push you {roomDirection.ToLower()} but isn't strong enough!{Constants.NewLine}");
                                    }
                                    else
                                    {
                                        (objTgt as Descriptor).Send($"Something tries to push you {roomDirection.ToLower()} but isn't strong enough!{Constants.NewLine}");
                                    }
                                    if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom).Count > 2)
                                    {
                                        // if we have more than the player and the target here, notify them of what happened
                                        foreach (var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom))
                                        {
                                            if (p.Player.Name != desc.Player.Name && p.Player.Name != (objTgt as Descriptor).Player.Name)
                                            {
                                                if (!desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something tries to push something else {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                                if (!desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"Something tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && !(objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} tries to push something {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                                if (desc.Player.Visible && (objTgt as Descriptor).Player.Visible)
                                                {
                                                    p.Send($"{desc.Player.Name} tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // target is an NPC so just notify any other players in the room
                                    foreach(var p in RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom))
                                    {
                                        if(p.Player.Name != desc.Player.Name)
                                        {
                                            if(desc.Player.Visible)
                                            {
                                                p.Send($"{desc.Player.Name} tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                            }
                                            else
                                            {
                                                p.Send($"Something tries to push {targetName} {roomDirection.ToLower()}, but isn't strong enough!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            desc.Send($"Some mysterious force prevents you from pushing {target} that way...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"There is no way to push {target} that way!{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You don't see {target} here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Usage: Push <target> <direction>{Constants.NewLine}");
            }
        }

        internal static void ChangePlayerPosition(ref Descriptor desc, ref string input)
        {
            var verb = GetVerb(ref input).ToLower();
            switch(verb)
            {
                case "sit":
                    desc.Player.Position = ActorPosition.Sitting;
                    desc.Send($"You sit down to take the load off your feet for a while.{Constants.NewLine}");
                    break;

                case "rest":
                    desc.Player.Position = ActorPosition.Resting;
                    desc.Send($"You stop and take a nice rest.{Constants.NewLine}");
                    break;

                case "stand":
                    SessionManager.Instance.GetPlayerByGUID(desc.Id).Player.Position = ActorPosition.Standing;
                    desc.Send($"You stand up, ready to go!{Constants.NewLine}");
                    break;
            }
        }

        private static void Look(ref Descriptor desc, ref string input)
        {
            string verb = GetVerb(ref input).ToLower().Trim();
            string target = input.Remove(0, verb.Length).Trim().ToLower();
            string msgToSendToPlayer = string.Empty;
            string msgToSendToTarget = string.Empty;
            string[] msgToSendToOthers = new string[] { string.Empty, string.Empty };
            if (!string.IsNullOrEmpty(target))
            {
                switch (target.ToLower())
                {
                    case "up":
                    case "u":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("up"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").RoomDoor;
                            if(d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("up").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look up...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view up...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes upwards{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes upwards...{Constants.NewLine}";
                            }
                        }
                        break;

                    case "down":
                    case "d":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("down"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("down").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as your look down...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view down...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes towards the ground{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes towards the ground...{Constants.NewLine}";
                            }
                        }
                        break;

                    case "west":
                    case "w":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("west"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("west").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look towards the west...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view west...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the west{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the west{Constants.NewLine}";
                            }
                        }
                        break;

                    case "east":
                    case "e":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("east"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("east").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look towards the east...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view east...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the east{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the east{Constants.NewLine}";
                            }
                        }
                        break;

                    case "north":
                    case "n":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("north"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("north").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look north...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view north...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the north{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the north{Constants.NewLine}";
                            }
                        }
                        break;

                    case "south":
                    case "s":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("south"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("south").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look south...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view south...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the south{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the south{Constants.NewLine}";
                            }
                        }
                        break;

                    case "northwest":
                    case "nw":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("northwest"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northwest").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look northwest...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view northwest...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northwest{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northwest{Constants.NewLine}";
                            }
                        }
                        break;

                    case "northeast":
                    case "ne":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("northeast"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("northeast").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look northeast...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view northeast...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the northeast{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the northeast{Constants.NewLine}";
                            }
                        }
                        break;

                    case "southeast":
                    case "se":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("southeast"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southeast").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look southeast...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view southeast...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southeast{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southeast{Constants.NewLine}";
                            }
                        }
                        break;

                    case "southwest":
                    case "sw":
                        if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).HasExitInDiretion("southwest"))
                        {
                            var d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").RoomDoor;
                            if (d == null || (d != null && d.IsOpen))
                            {
                                var rid = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).GetRoomExit("southwest").DestinationRoomID;
                                var targetRoom = RoomManager.Instance.GetRoom(rid);
                                if (targetRoom != null)
                                {
                                    if (targetRoom.Flags.HasFlag(RoomFlags.Dark))
                                    {
                                        if (targetRoom.HasLightSource)
                                        {
                                            msgToSendToPlayer = targetRoom.LongDescription;
                                            msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                            msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            if (desc.Player.Level >= Constants.ImmLevel || desc.Player.HasSkill("Darkvision"))
                                            {
                                                msgToSendToPlayer = targetRoom.LongDescription;
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToSendToPlayer = $"You see only darkness as you look southwest...{Constants.NewLine}";
                                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                                msgToSendToOthers[1] = $"Something shifts in the darkness...{Constants.NewLine}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        msgToSendToPlayer = targetRoom.LongDescription;
                                        msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                                    }
                                }
                                else
                                {
                                    msgToSendToPlayer = "That way lies only the void...";
                                }
                            }
                            else
                            {
                                msgToSendToPlayer = $"A doorway blocks your view southwest...{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gazes off towards the southwest{Constants.NewLine}";
                                msgToSendToOthers[1] = $"There is a shift in the air as something gazes off towards the southwest{Constants.NewLine}";
                            }
                        }
                        break;

                    case "node":
                        if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode != null)
                        {
                            string amount = string.Empty;
                            uint d = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode.NodeDepth;
                            switch(d)
                            {
                                case 1:
                                    amount = "almost no";
                                    break;

                                case 2:
                                    amount = "a little bit of";
                                    break;

                                case 3:
                                    amount = "quite a bit of";
                                    break;

                                default:
                                    amount = "a lot of";
                                    break;
                            }
                            if(desc.Player.Level >= Constants.ImmLevel)
                            {
                                desc.Send($"The {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode} node can be mined {d} more times.{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"The {RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode} node has {amount} resources left{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"There is no resource node here!{Constants.NewLine}");
                        }
                        break;

                    default:
                        // we aren't looking in a direction so try looking at other things, first players, then NPCs then items
                        var p = desc.Player.Level >= Constants.ImmLevel ?
                            RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault() :
                            RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => x.Player.Visible && Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                        if (p != null)
                        {
                            if (Regex.Match(desc.Player.Name, target, RegexOptions.IgnoreCase).Success)
                            {
                                msgToSendToPlayer = $"You look yourself up and down. Vain, much?{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} looks themselves up and down. How vain.{Constants.NewLine}";
                                msgToSendToOthers[1] = $"Something looks at itself... Very creepy...{Constants.NewLine}";
                            }
                            else
                            {
                                var targetHP = (double)p.Player.Stats.CurrentHP / p.Player.Stats.MaxHP * 100.0;
                                string stateMsg = targetHP >= 90 ? $"{p.Player.Name} is in excellent health" : targetHP < 90 && targetHP >= 30 ? $"{p.Player.Name} looks a bit rough" : $"{p.Player.Name} looks on the verge of death";
                                msgToSendToPlayer = $"{p.Player.LongDescription}{Constants.NewLine}{stateMsg}{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} looks {p.Player.Name} up and down...{Constants.NewLine}";
                                msgToSendToOthers[1] = $"Something looks {p.Player.Name} up and down...{Constants.NewLine}";
                                msgToSendToTarget = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} gives you a studious look. Strange person.{Constants.NewLine}" : $"You feel a chill as though something was looking at you...{Constants.NewLine}";
                                p.Send(msgToSendToTarget);
                            }
                        }
                        else
                        {
                            // p was null so the target string doesn't match a player in the room, try finding a matching NPC
                            var n = GetTargetNPC(ref desc, target);
                            if(n != null)
                            {
                                var targetHP = (double)n.Stats.CurrentHP / n.Stats.MaxHP * 100.0;
                                string stateMsg = targetHP >= 90 ? $"{n.Name} is in excellent health" : targetHP < 90 && targetHP >= 30 ? $"{n.Name} looks a bit rough" : $"{n.Name} looks on the verge of death";
                                msgToSendToPlayer = $"{n.LongDescription}{Constants.NewLine}{stateMsg}{Constants.NewLine}";
                                msgToSendToOthers[0] = $"{desc.Player.Name} gives {n.Name} a studious look{Constants.NewLine}";
                                msgToSendToOthers[1] = $"Something gives {n.Name} a studious look{Constants.NewLine}";
                            }
                            else
                            {
                                // n was null, try looking for an item in the room
                                var i = GetTargetItem(ref desc, target, false);
                                if(i != null)
                                {
                                    msgToSendToPlayer = $"{i.LongDescription}{Constants.NewLine}";
                                    msgToSendToOthers[0] = $"{desc.Player.Name} looks longingly at {i.Name}{Constants.NewLine}";
                                    msgToSendToOthers[1] = $"Something looks longingly at {i.Name}";
                                }
                                else
                                {
                                    // no matching item in the room, so look in player inventory instead
                                    var ii = GetTargetItem(ref desc, target, true);
                                    if(ii != null)
                                    {
                                        msgToSendToPlayer = $"{ii.LongDescription}{Constants.NewLine}";
                                        msgToSendToOthers[0] = $"{desc.Player.Name} looks longling at {ii.Name} that they're holding{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"Something looks longingly at something else... Very strange{Constants.NewLine}";
                                    }
                                    else
                                    {
                                        // no matching player, npc, room or inventory item...
                                        msgToSendToPlayer = $"You look about, but you can't find anything like that!{Constants.NewLine}";
                                        msgToSendToOthers[0] = $"{desc.Player.Name} looks about, searching for something they can't seem to find...{Constants.NewLine}";
                                        msgToSendToOthers[1] = $"The air shifts as something looks around, searching...{Constants.NewLine}";
                                    }
                                }
                            }
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(msgToSendToPlayer))
                {
                    desc.Send(msgToSendToPlayer);
                }
                var playersInRoom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom);
                if(playersInRoom != null && playersInRoom.Count > 1)
                {
                    foreach(var p in playersInRoom)
                    {
                        if(!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success && !Regex.Match(p.Player.Name, target, RegexOptions.IgnoreCase).Success)
                        {
                            if(desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                            {
                                p.Send(msgToSendToOthers[0]);
                            }
                            else
                            {
                                p.Send(msgToSendToOthers[1]);
                            }
                        }
                    }
                }
            }
            else
            {
                Room.DescribeRoom(ref desc);
            }
        }

        private static void ShowCharSheet(ref Descriptor desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {new string('=', 77)}");
            sb.AppendLine($"|| Name: {desc.Player.Name}{Constants.TabStop}{Constants.TabStop}Gender: {desc.Player.Gender}{Constants.TabStop}Class: {desc.Player.Class}{Constants.TabStop}Race: {desc.Player.Race}");
            sb.AppendLine($"|| Level: {desc.Player.Level}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}Exp: {desc.Player.Stats.Exp}{Constants.TabStop}Next: {LevelTable.GetExpForNextLevel(desc.Player.Level, desc.Player.Stats.Exp)}");
            sb.AppendLine($"|| Alignment: {desc.Player.Alignment}{Constants.TabStop}{Constants.TabStop}Gold: {desc.Player.Stats.Gold}");
            sb.AppendLine($"|| Position: {desc.Player.Position}");
            sb.AppendLine($"||");
            sb.AppendLine($"|| Stats:");
            sb.AppendLine($"|| Strength: {desc.Player.Stats.Strength} ({ActorStats.CalculateAbilityModifier(desc.Player.Stats.Strength)}){Constants.TabStop}{Constants.TabStop}Dexterity: {desc.Player.Stats.Dexterity} ({ActorStats.CalculateAbilityModifier(desc.Player.Stats.Dexterity)})");
            sb.AppendLine($"|| Constitution: {desc.Player.Stats.Constitution} ({ActorStats.CalculateAbilityModifier(desc.Player.Stats.Constitution)}){Constants.TabStop}{Constants.TabStop}Intelligence: {desc.Player.Stats.Intelligence} ({ActorStats.CalculateAbilityModifier(desc.Player.Stats.Intelligence)})");
            sb.AppendLine($"|| Wisdom: {desc.Player.Stats.Wisdom} ({ActorStats.CalculateAbilityModifier(desc.Player.Stats.Wisdom)}){Constants.TabStop} {Constants.TabStop}Charisma: {desc.Player.Stats.Charisma} ({ActorStats.CalculateAbilityModifier(desc.Player.Stats.Charisma)})");
            sb.AppendLine($"|| Current HP: {desc.Player.Stats.CurrentHP}{Constants.TabStop}{Constants.TabStop}Max HP: {desc.Player.Stats.MaxHP}");
            sb.AppendLine($"|| Current MP: {desc.Player.Stats.CurrentMP}{Constants.TabStop}{Constants.TabStop}Max MP: {desc.Player.Stats.MaxMP}");
            sb.AppendLine($"|| Armour Class: {desc.Player.Stats.ArmourClass}{Constants.TabStop}No. Of Attacks: {desc.Player.NumberOfAttacks}");
            sb.AppendLine($"  {new string('=', 77)}");
            desc.Send(sb.ToString());
        }

        private static void SayToRoom(ref Descriptor desc, ref string line)
        {
            var verb = GetVerb(ref line).Trim();
            var msg = line.Remove(0, verb.Length).Trim();
            foreach (var p in RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom))
            {
                if (Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                {
                    p.Send($"You say \"{msg}\"{Constants.NewLine}");
                }
                else
                {
                    string msgToSend = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} says, \"{msg}\"{Constants.NewLine}"
                        : $"Something says, \"{msg}\"{Constants.NewLine}";
                    p.Send(msgToSend);
                }
            }
        }

        private static void SayToCharacter(ref Descriptor desc, ref string line)
        {
            var inputElements = TokeniseInput(ref line);
            string target = inputElements.Length > 1 ? inputElements[1].Trim() : string.Empty;
            var playersInRoom = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
            if (Regex.Match(desc.Player.Name, target, RegexOptions.IgnoreCase).Success)
            {
                desc.Send($"You mumble incoherently to yourself. Strange person.{Constants.NewLine}");
                if (playersInRoom.Count - 1 > 1)
                {
                    foreach (var pd in playersInRoom)
                    {
                        if (!Regex.Match(pd.Player.Name, desc.Player.Name, RegexOptions.IgnorePatternWhitespace).Success)
                        {
                            var msgToSend = desc.Player.Visible || pd.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} mumbles to themselves like a weirdo.{Constants.NewLine}"
                                : $"Something mumbles to itself incoherently. Strange.{Constants.NewLine}";
                            pd.Send(msgToSend);
                        }
                    }
                }
                return;
            }
            var toSend = line.TrimStart(GetVerb(ref line).ToCharArray()).Trim().Trim(target.ToCharArray()).Trim();
            var p = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
            if (p != null)
            {
                var msgToPlayer = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} whispers \"{toSend}\"{Constants.NewLine}"
                    : $"Something whispers \"{toSend}\"{Constants.NewLine}";
                p.Send(msgToPlayer);
                if (playersInRoom.Count > 2)
                {
                    foreach (var player in playersInRoom)
                    {
                        string msgToOthers = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} whispers something to "
                            : $"Something whispers something to ";
                        msgToOthers = p.Player.Visible || player.Player.Level >= Constants.ImmLevel ? $"{msgToOthers}{p.Player.Name}.{Constants.NewLine}"
                            : $"{msgToOthers}something else.{Constants.NewLine}";
                        if (!Regex.Match(player.Player.Name, p.Player.Name, RegexOptions.IgnoreCase).Success && !Regex.Match(desc.Player.Name, player.Player.Name, RegexOptions.IgnoreCase).Success)
                        {
                            player.Send(msgToOthers);
                        }
                    }
                }
            }
            else
            {
                desc.Send($"That person isn't here...{Constants.NewLine}");
                if (playersInRoom.Count - 1 > 1)
                {
                    foreach (var player in playersInRoom)
                    {
                        if (!Regex.Match(player.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                        {
                            var msg = desc.Player.Visible || player.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} mumbles something to themselves... What a strange person.{Constants.NewLine}"
                                : $"Something mumbles to itself. How strange.{Constants.NewLine}";
                        }
                    }
                }
            }
        }
    }
}