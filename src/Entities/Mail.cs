using Kingdoms_of_Etrea.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Entities
{
    [Serializable]
    internal class Mail
    {
        [JsonProperty]
        internal Guid MailID { get; set; }
        [JsonProperty]
        internal string MailFrom { get; set; }
        [JsonProperty]
        internal string MailTo { get; set; }
        [JsonProperty]
        internal string MailSubject { get; set; }
        [JsonProperty]
        internal string MailBody { get; set; }
        [JsonProperty]
        internal string MailSent { get; set; }
        [JsonProperty]
        internal bool MailRead { get; set; }
        [JsonProperty]
        internal List<InventoryItem> AttachedItems { get; set; }
        [JsonProperty]
        internal uint AttachedGold { get; set; }

        internal static Mail Compose(ref Descriptor desc)
        {
            try
            {
                Mail mail = new Mail();
                mail.AttachedItems = new List<InventoryItem>();
                mail.MailID = Guid.NewGuid();
                mail.MailFrom = desc.Player.Name;
                mail.MailRead = false;
                bool valid = false;
                while(!valid)
                {
                    desc.Send($"Send to (END to abort): ");
                    var to = desc.Read().Trim();
                    if(Helpers.ValidateInput(to) && DatabaseManager.CharacterExistsInDatabase(to))
                    {
                        if(to.ToUpper() == "END")
                        {
                            return null;
                        }
                        mail.MailTo = to;
                        valid = true;
                    }
                    else
                    {
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                    }
                }
                valid = false;
                while(!valid)
                {
                    desc.Send("Subject (END to abort): ");
                    var subj = desc.Read().Trim();
                    if(Helpers.ValidateInput(subj))
                    {
                        if(subj.ToUpper() == "END")
                        {
                            return null;
                        }
                        mail.MailSubject = subj;
                        valid = true;
                    }
                }
                mail.MailBody = Helpers.GetMailBody(ref desc);
                valid = false;
                while(!valid)
                {
                    desc.Send($"Amount of gold to send: ");
                    var gp = desc.Read().Trim();
                    if(Helpers.ValidateInput(gp) && uint.TryParse(gp, out uint _sendGP))
                    {
                        // if we don't have the amount of gold we're trying to send + 5 gold for the cost of sending, we can't do it
                        // we can send with 0 gold attached as long as we have the 5 gold to cover the sending cost
                        if(_sendGP > 0 && _sendGP + 5 <= desc.Player.Stats.Gold)
                        {
                            mail.AttachedGold = _sendGP;
                            desc.Player.Stats.Gold -= _sendGP;
                            valid = true;
                        }
                        else
                        {
                            if(_sendGP > 0)
                            {
                                desc.Send($"You cannot affort to send that much gold!{Constants.NewLine}");
                            }
                            else
                            {
                                valid = true;
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                    }
                }
                valid = false;
                while(!valid)
                {
                    desc.Send($"Attach Item (END to skip): ");
                    var itemName = desc.Read().Trim();
                    if(Helpers.ValidateInput(itemName))
                    {
                        if(itemName.ToUpper() == "END")
                        {
                            valid = true;
                        }
                        else
                        {
                            var i = desc.Player.Inventory.Where(x => Regex.Match(x.Name, itemName, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                            if(i != null)
                            {
                                desc.Send($"Attach '{i.Name}' to the mail?{Constants.NewLine}");
                                var conf = desc.Read().Trim();
                                if(Helpers.ValidateInput(conf))
                                {
                                    if(conf.ToUpper() == "Y" || conf.ToUpper() == "YES")
                                    {
                                        mail.AttachedItems.Add(i);
                                        desc.Player.Inventory.Remove(i);
                                    }
                                }
                            }
                        }
                    }
                }
                return mail;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error composing a mail: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }
    }
}
