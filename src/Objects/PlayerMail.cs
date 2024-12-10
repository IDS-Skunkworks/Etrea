using Etrea3.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etrea3.Objects
{
    [Serializable]
    public class PlayerMail
    {
        [JsonProperty]
        public Guid MailGuid { get; set; }
        [JsonProperty]
        public DateTime SentDate { get; set; }
        [JsonProperty]
        public string Sender { get; set; }
        [JsonProperty]
        public string Recipient { get; set; }
        [JsonProperty]
        public string Subject { get; set; }
        [JsonProperty]
        public string Body { get; set; }
        [JsonProperty]
        List<dynamic> Attachments { get; set; }
        [JsonProperty]
        public int AttachedGold { get; set; }
        [JsonProperty]
        public bool IsRead { get; set; }

        public PlayerMail()
        {
            MailGuid = Guid.NewGuid();
            Attachments = new List<dynamic>();
            SentDate = DateTime.UtcNow;
        }

        public static void ComposeMail(Session session)
        {
            if (session.Player.Gold < 5)
            {
                session.Send($"%BRT%You don't have enough gold! It costs 5 gold to send a mail!%PT%{Constants.NewLine}");
                return;
            }
            PlayerMail newMail = new PlayerMail
            {
                Sender = session.Player.Name
            };
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Message To: {newMail.Recipient}");
                sb.AppendLine($"Subject: {newMail.Subject}");
                sb.AppendLine($"Attached Gold: {newMail.AttachedGold:N0}{Constants.TabStop}Attached Items: {newMail.Attachments.Count}");
                sb.AppendLine($"Message:");
                if (!string.IsNullOrEmpty(newMail.Body))
                {
                    foreach (var l in newMail.Body.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                    {
                        sb.AppendLine($"  {l}");
                    }
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Recipient{Constants.TabStop}2. Set Subject");
                sb.AppendLine($"3. Set Message{Constants.TabStop}{Constants.TabStop}4. Attach Gold");
                sb.AppendLine($"5. Manage Attachments{Constants.TabStop}6. Send{Constants.TabStop}7. Return");
                sb.AppendLine("Choice:");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option.%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        session.Send("Enter Recipient: ");
                        var recipient = session.Read();
                        if (!string.IsNullOrEmpty(recipient))
                        {
                            newMail.Recipient = recipient.Trim();
                        }
                        break;

                    case 2:
                        session.Send("Enter Subject: ");
                        var subj = session.Read();
                        if (!string.IsNullOrEmpty(subj))
                        {
                            newMail.Subject = subj.Trim();
                        }
                        break;

                    case 3:
                        newMail.Body = ComposeMailBody(session);
                        break;

                    case 4:
                        session.Send("Enter Gold to include:");
                        var g = session.Read();
                        if (!string.IsNullOrEmpty(g) && uint.TryParse(g.Trim(), out uint includeGold))
                        {
                            if (session.Player.Gold >= (includeGold + 5))
                            {
                                newMail.AttachedGold = Convert.ToInt32(includeGold);
                                break;
                            }
                            session.Send($"%BRT%You don't have enough gold!%PT%{Constants.NewLine}");
                        }
                        break;

                    case 5:
                        ManageMessageAttachments(session, ref newMail);
                        break;

                    case 6:
                        if (ValidateMail(session, ref newMail))
                        {
                            if (DatabaseManager.SendMail(newMail))
                            {
                                session.Player.AdjustGold(-5, true, false);
                                if (newMail.AttachedGold > 0)
                                {
                                    session.Player.AdjustGold(newMail.AttachedGold * -1, true, false);
                                }
                                session.Send($"%BGT%The Winds of Magic swirl and your message vanishes!%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%The Winds of Magic die down and will not deliver your message! Please see an Imm!%PT%{Constants.NewLine}");
                            }
                            return;
                        }
                        break;

                    case 7:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option.%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        public static void ListMail(Session session)
        {
            session.Player.CurrentMail = DatabaseManager.GetPlayerMail(session.Player.Name);
            if (session.Player.CurrentMail == null)
            {
                session.Send($"%BRT%Request to get Mail returned Null, see an Imm!%PT%{Constants.NewLine}");
                return;
            }
            if (session.Player.CurrentMail.Count == 0)
            {
                session.Send($"%BGT%You don't have any mail to read right now!%PT%{Constants.NewLine}");
                return;
            }
            int counter = 1;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            foreach(var m in session.Player.CurrentMail)
            {
                sb.AppendLine($"%BYT%|| From: {m.Sender}{Constants.TabStop}Sent: {m.SentDate}{Constants.TabStop}Read: {m.IsRead}{Constants.TabStop}ID: {counter}%PT%");
                sb.AppendLine($"%BYT%|| Subject: {m.Subject}%PT%");
                sb.AppendLine($"%BYT%|| Gold: {m.AttachedGold:N0}");
                sb.AppendLine($"%BYT%|| Attachments: {m.Attachments.Count}");
                sb.AppendLine($"%BYT%||{new string('=', 77)}%PT%");
                counter++;
            }
            sb.AppendLine($"%BYT%|| You have {session.Player.CurrentMail.Count} mail(s)%PT%");
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        public static void DeleteMail(Session session, int id)
        {
            var mID = id - 1;
            if (mID < 0 || mID > session.Player.CurrentMail.Count)
            {
                session.Send($"%BRT%That ID isn't valid!%PT%{Constants.NewLine}");
                return;
            }
            var m = session.Player.CurrentMail[mID];
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"%BRT%The following mail will be deleted - this cannot be undone!%PT%");
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                sb.AppendLine($"%BYT%|| From: {m.Sender}{Constants.TabStop}Sent: {m.SentDate}{Constants.TabStop}Read: {m.IsRead}%PT%");
                sb.AppendLine($"%BYT%|| Subject: {m.Subject}%PT%");
                sb.AppendLine($"%BYT%|| Gold: {m.AttachedGold:N0}");
                sb.AppendLine($"%BYT%|| Attachments: {m.Attachments.Count}");
                sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
                sb.AppendLine($"Options:");
                sb.AppendLine($"1. Delete{Constants.TabStop}{Constants.TabStop}2. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That doesn't look like a valid option!%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        if (DatabaseManager.DeleteMail(m.MailGuid.ToString()))
                        {
                            session.Send($"%BGT%The Winds of Magic swirl about, and suddenly your mail is gone!%PT%{Constants.NewLine}");
                            session.Player.CurrentMail.Remove(m);
                            return;
                        }
                        else
                        {
                            session.Send($"BRT%The Winds of Magic do not seem to respond... Something is wrong!%PT%{Constants.NewLine}");
                            return;
                        }

                    case 2:
                        return;

                    default:
                        session.Send($"%BRT%That doesn't look like a valid option!%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        public static void ReadMail(Session session, int id)
        {
            var mID = id - 1;
            if (mID < 0 || mID > session.Player.CurrentMail.Count)
            {
                session.Send($"%BRT%That ID isn't valid!%PT%{Constants.NewLine}");
                return;
            }
            var m = session.Player.CurrentMail[mID];
            if (!m.IsRead)
            {
                m.IsRead = true;
                DatabaseManager.MarkMailAsRead(m);
                if (m.AttachedGold > 0)
                {
                    session.Send($"%BYT%The Winds of Magic swirl about this message, depositing {m.AttachedGold:N0} gold with you!%PT%{Constants.NewLine}");
                    session.Player.AdjustGold(m.AttachedGold, true, false);
                }
                if (m.Attachments != null && m.Attachments.Count > 0)
                {
                    session.Send($"%BYT%The Winds of Magic swirl about this message, depositing some items with you!%PT%{Constants.NewLine}");
                    foreach(var i in m.Attachments)
                    {
                        session.Player.AddItemToInventory(i.ID);
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            sb.AppendLine($"%BYT%|| From: {m.Sender}{Constants.TabStop}Sent: {m.SentDate}%PT%");
            sb.AppendLine($"%BYT%|| Subject: {m.Subject}%PT%");
            foreach (var l in m.Body.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
            {
                sb.AppendLine($"%BYT%||   {l}");
            }
            if (m.AttachedGold > 0)
            {
                sb.AppendLine($"%BYT%|| Enclosed Gold: {m.AttachedGold:N0}");
            }
            if (m.Attachments != null && m.Attachments.Count > 0)
            {
                sb.AppendLine($"%BYT%|| Attached Items: {m.Attachments.Count}");
            }
            sb.AppendLine($"%BYT%  {new string('=', 77)}%PT%");
            session.Send(sb.ToString());
        }

        private static bool ValidateMail(Session session, ref PlayerMail mail)
        {
            if (string.IsNullOrEmpty(mail.Subject))
            {
                session.Send($"%BRT%Your mail needs a subject!%PT%{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(mail.Body))
            {
                session.Send($"%BRT%Your mail needs a message body!%PT%{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(mail.Recipient))
            {
                session.Send($"%BRT%You need to specify a recipient!%PT%{Constants.NewLine}");
                return false;
            }
            if (!DatabaseManager.CharacterExists(mail.Recipient))
            {
                session.Send($"%BRT%The recipient doesn't exist within the Realms!%PT%{Constants.NewLine}");
                return false;
            }
            if ((ulong)mail.AttachedGold > (session.Player.Gold -5))
            {
                session.Send($"%BRT%You don't have enough gold!%PT%{Constants.NewLine}");
                return false;
            }
            return true;
        }

        private static void ManageMessageAttachments(Session session, ref PlayerMail mail)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (mail.Attachments.Count > 0)
                {
                    sb.AppendLine("Attachments:");
                    foreach(var i in mail.Attachments)
                    {
                        sb.AppendLine($"  {i.Name} ({i.ID})");
                    }
                }
                else
                {
                    sb.AppendLine("Attachments: None");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That doesn't look like a valid option.%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        session.Send($"Enter Item Name or ID: ");
                        var itemID = session.Read();
                        if (string.IsNullOrEmpty(itemID))
                        {
                            continue;
                        }
                        if (int.TryParse(itemID.Trim(), out int id))
                        {
                            if (id <= 0)
                            {
                                session.Send($"%BRT%That isn't a valid Item ID!%PT%{Constants.NewLine}");
                                continue;
                            }
                            var item = session.Player.GetInventoryItem(id);
                            if (item == null)
                            {
                                session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                                continue;
                            }
                            session.Player.RemoveItemFromInventory(item);
                            mail.Attachments.Add(item);
                        }
                        else
                        {
                            var item = session.Player.GetInventoryItem(input.Trim());
                            if (item == null)
                            {
                                session.Send($"%BRT%No Item with that name could be found in Item Manager.%PT%{Constants.NewLine}");
                                continue;
                            }
                            session.Player.RemoveItemFromInventory(item);
                            mail.Attachments.Add(item);
                        }
                        break;

                    case 2:
                        session.Send("Enter Item ID: ");
                        itemID = session.Read();
                        if (string.IsNullOrEmpty(itemID) || !int.TryParse(itemID.Trim(), out id))
                        {
                            session.Send($"%BRT%That isn't a valid Item ID!%PT%{Constants.NewLine}");
                            continue;
                        }
                        var remItem = mail.Attachments.FirstOrDefault(x => x.ID == id);
                        if (remItem == null)
                        {
                            session.Send($"%BRT%No Attachment with that Item ID could be found.%PT%{Constants.NewLine}");
                            continue;
                        }
                        mail.Attachments.Remove(remItem);
                        session.Player.AddItemToInventory(remItem);
                        break;

                    case 3:
                        foreach(var i in mail.Attachments)
                        {
                            session.Player.AddItemToInventory(i);
                        }
                        mail.Attachments.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That doesn't look like a valid option.%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static string ComposeMailBody(Session session)
        {
            StringBuilder sb = new StringBuilder();
            int line = 1;
            session.Send($"%BGT%Enter your message, try to keep each line to a max of 80 characters.%PT%{Constants.NewLine}");
            session.Send($"%BGT%Enter END on a new line to finish.%PT%{Constants.NewLine}");
            session.Send($"%BYT%{new string('=', 77)}%PT%{Constants.NewLine}");
            while (true)
            {
                session.Send($"[{line}] ");
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
    }
}
