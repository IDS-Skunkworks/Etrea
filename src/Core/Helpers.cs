using System.Text;
using Kingdoms_of_Etrea.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Kingdoms_of_Etrea.Core
{
    internal static class Helpers
    {
        private static Random rnd = new Random();
        private static int VowelMask = (1 << 1) | (1 << 5) | (1 << 9) | (1 << 15) | (1 << 21);

        #region JsonSerialise
        internal static string SerialiseQuest(Quest q)
        {
            return JsonConvert.SerializeObject(q);
        }

        internal static string SerialiseCraftingRecipe(Crafting.Recipe r)
        {
            return JsonConvert.SerializeObject(r);
        }

        internal static string SerialiseEmoteObject(Emote e)
        {
            return JsonConvert.SerializeObject(e);
        }

        internal static string SerialisePlayerObject(Player p)
        {
            return JsonConvert.SerializeObject(p);
        }

        internal static string SerialiseNPC(NPC npc)
        {
            return JsonConvert.SerializeObject(npc);
        }

        internal static string SerialiseRoomObject(Room room)
        {
            return JsonConvert.SerializeObject(room);
        }

        internal static string SerialiseItemObject(InventoryItem i)
        {
            return JsonConvert.SerializeObject(i);
        }

        internal static string SerialiseShopObject(Shop shop)
        {
            return JsonConvert.SerializeObject(shop);
        }

        internal static string SerialiseResourceNode(ResourceNode node)
        {
            return JsonConvert.SerializeObject(node);
        }

        internal static string SerialiseMail(Mail mail)
        {
            return JsonConvert.SerializeObject(mail);
        }
        #endregion

        #region JsonDeSerialise
        internal static Quest DeserialiseQuest(string q)
        {
            return JsonConvert.DeserializeObject<Quest>(q);
        }

        internal static Mail DeserialiseMail(string mail)
        {
            return JsonConvert.DeserializeObject<Mail>(mail);
        }

        internal static Crafting.Recipe DeserialiseRecipe(string r)
        {
            return JsonConvert.DeserializeObject<Crafting.Recipe>(r);
        }

        internal static ResourceNode DeserialiseResourceNode(string n)
        {
            return JsonConvert.DeserializeObject<ResourceNode>(n);
        }

        internal static Shop DeserialiseShopObject(string shop)
        {
            return JsonConvert.DeserializeObject<Shop>(shop);
        }

        internal static NPC DeserialiseNPC(string npc)
        {
            return JsonConvert.DeserializeObject<NPC>(npc);
        }

        internal static Room DeserialiseRoomObject(string room)
        {
            return JsonConvert.DeserializeObject<Room>(room);
        }

        internal static Player DeserialisePlayerObject(string p)
        {
            return JsonConvert.DeserializeObject<Player>(p);
        }

        internal static InventoryItem DeserialiseItemObject(string p)
        {
            return JsonConvert.DeserializeObject<InventoryItem>(p);
        }

        internal static Emote DeserialiseEmoteObject(string p)
        {
            return JsonConvert.DeserializeObject<Emote>(p);
        }

        internal static List<Room.Exit> DeserialiseRoomExits(string exits)
        {
            return JsonConvert.DeserializeObject<List<Room.Exit>>(exits);
        }

        internal static Shop DeserialiseRoomShop(string shop)
        {
            return JsonConvert.DeserializeObject<Shop>(shop);
        }

        internal static RoomFlags DeserialiseRoomFlags(string flags)
        {
            return JsonConvert.DeserializeObject<RoomFlags>(flags);
        }
        #endregion

        internal static uint RollDice(uint numOfDice, uint sizeOfDice)
        {
            var maxRoll = numOfDice * sizeOfDice;
            var retval = rnd.Next(Convert.ToInt32(numOfDice), Convert.ToInt32(maxRoll));
            return Convert.ToUInt32(retval);
        }

        internal static string GetActorStateMessage(string actorName, double hp)
        {
            if (hp <= 100 && hp >= 90)
            {
                return $"{actorName} appears in perfect health, showing no sign of injury";
            }
            if (hp < 90 && hp >= 80)
            {
                return $"{actorName} looks healthy and energetic with no visible sign of injury";
            }
            if (hp < 80 && hp >= 70)
            {
                return $"{actorName} has a few scratches and bruises, but is in otherwise good condition";
            }
            if (hp < 70 && hp >= 60)
            {
                return $"{actorName} has several noticable cuts and bruises";
            }
            if (hp < 60 && hp >= 50)
            {
                return $"{actorName} is visibly injured, the stress of battle clear to see";
            }
            if (hp < 50 && hp >= 40)
            {
                return $"{actorName} is covered in cuts and wounds";
            }
            if (hp < 40 && hp >= 30)
            {
                return $"{actorName} is badly wounded";
            }
            if (hp < 30 && hp >= 20)
            {
                return $"{actorName} is in critical condition";
            }
            if( hp < 20 &&  hp >= 10)
            {
                return $"{actorName} is carrying critical wounds and near to bleeding out";
            }
            return $"{actorName} is on the brink of death";
        }

        internal static bool IsCharAVowel(char c)
        {
            return (c > 64) && ((VowelMask & (1 << ((c | 0x20) % 32))) != 0);
        }

        internal static Room.Exit GetRandomExit(uint rid)
        {
            var exitList = RoomManager.Instance.GetRoom(rid).RoomExits;
            var n = rnd.Next(exitList.Count);
            return exitList[n];
        }

        internal static uint GetNewSalePrice(ref Descriptor desc, uint basePrice)
        {
            var charismaModifier = ActorStats.CalculateAbilityModifier(desc.Player.Stats.Charisma);
            if(desc.Player.HasSkill("Mercenary"))
            {
                charismaModifier += 2;
            }
            int modPrice = Convert.ToInt32(basePrice);
            if (charismaModifier < 0)
            {
                // decrease offer due to low charisma
                int posMod = charismaModifier * -1;     // convert negative modifier to positive for calculations
                for (int i = 0; i < posMod; i++)
                {
                    modPrice -= Convert.ToInt32(Math.Round(modPrice * 0.025, 0));
                }
                modPrice = modPrice < 0 ? 0 : modPrice;
                return Convert.ToUInt32(modPrice);
            }
            if (charismaModifier > 0)
            {
                // increase offer due to high charisma
                for (int i = 0; i < charismaModifier; i++)
                {
                    modPrice += Convert.ToInt32(Math.Round(modPrice * 0.025, 0));
                }
                modPrice = modPrice < 0 ? 0 : modPrice;
                var purchasePrice = GetNewPurchasePrice(ref desc, basePrice);
                modPrice = modPrice > purchasePrice ? Convert.ToInt32(purchasePrice) : modPrice;
                return Convert.ToUInt32(modPrice);
            }
            return basePrice;
        }

        internal static uint GetNewPurchasePrice(ref Descriptor desc, uint basePrice)
        {
            var charismaModifier = ActorStats.CalculateAbilityModifier(desc.Player.Stats.Charisma);
            if(desc.Player.HasSkill("Mercenary"))
            {
                charismaModifier += 2;
            }
            int modPrice = Convert.ToInt32(basePrice);
            if(charismaModifier < 0)
            {
                // increase price due to low charisma
                int posMod = charismaModifier * -1;     // convert negative modifier to positive for calculations
                for(int i = 0; i < posMod; i++)
                {
                    modPrice += Convert.ToInt32(Math.Round(modPrice * 0.025, 0));
                }
                modPrice = modPrice < 0 ? 0 : modPrice;
                return Convert.ToUInt32(modPrice);
            }
            if(charismaModifier > 0)
            {
                // drop price due to high charisma
                for (int i = 0; i < charismaModifier; i++)
                {
                    modPrice -= Convert.ToInt32(Math.Round(modPrice * 0.025, 0));
                }
                modPrice = modPrice < 0 ? 0 : modPrice;
                return Convert.ToUInt32(modPrice);
            }
            return basePrice;
        }

        internal static string GetDamageString(uint percDmg)
        {
            if(percDmg > 90)
            {
                return "destroys";
            }
            if(percDmg <=90 && percDmg > 80)
            {
                return "devastates";
            }
            if(percDmg <= 80 && percDmg > 50)
            {
                return "eviscerates";
            }
            if(percDmg <= 50 && percDmg > 20)
            {
                return "smashes";
            }
            if(percDmg <=20 && percDmg > 10)
            {
                return "hurts";
            }
            if(percDmg <= 10 && percDmg > 5)
            {
                return "wounds";
            }
            return "grazes";
        }

        internal static bool ValidateInput(string input)
        {
            return !string.IsNullOrEmpty(input) && Encoding.UTF8.GetByteCount(input) == input.Length;
        }

        internal static string GetMailBody(ref Descriptor desc)
        {
            uint row = 1;
            StringBuilder body = new StringBuilder();
            desc.Send($"Enter the message you would like to send. This should be no more than 30 lines{Constants.NewLine}");
            desc.Send($"long and no more than 80 characters per line. Enter END on a new line to finish.{Constants.NewLine}");
            desc.Send($"{new string('=', 80)}{Constants.NewLine}");
            bool valid = false;
            while (!valid)
            {
                desc.Send($"[{row}] ");
                var input = desc.Read().Trim();
                if(ValidateInput(input) && input.Length <= 80)
                {
                    if(input.ToUpper() == "END")
                    {
                        valid = true;
                    }
                    else
                    {
                        body.AppendLine(input);
                        row++;
                        if(row > 30)
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
            return body.ToString();
        }

        internal static string GetLongDescription(ref Descriptor desc)
        {
            uint row = 1;
            StringBuilder longDesc = new StringBuilder();
            bool valid = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Enter a long description. This should be no more than 30 lines long and each");
            sb.AppendLine("line should be no longer than 80 characters.");
            sb.AppendLine("Descriptions may be changed later in the OLC.");
            sb.AppendLine("Enter END on a new line to finish editing.");
            sb.AppendLine($"{new string('=', 80)}");
            desc.Send(sb.ToString());
            while (!valid)
            {
                desc.Send($"[{row}] ");
                var input = desc.Read().Trim();
                if (ValidateInput(input) && input.Length <= 80)
                {
                    if (input.ToUpper() == "END" && row >= 2)
                    {
                        valid = true;
                    }
                    else
                    {
                        longDesc.AppendLine(input);
                        row++;
                        if (row > 30)
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
            return longDesc.ToString();
        }

        internal static string CapitaliseFirstLetter(string src)
        {
            if(string.IsNullOrEmpty(src))
            {
                return string.Empty;
            }
            char[] letters = src.ToCharArray();
            char[] retval = new char[letters.Length];
            for (int i = 0; i < letters.Length; i++)
            {
                if(i == 0)
                {
                    retval[i] = char.ToUpper(letters[i]);
                }
                else
                {
                    retval[i] = char.ToLower(letters[i]);
                }
            }
            return new string(retval);
        }

        internal static string PrettifyMessageToSend(string msgIn)
        {
            return msgIn;

            // This was originally designed to split messages to clients into lines of 80 characters but never really worked properly.
            // Consequently leaving the process of screen wrapping to MUD clients themselves instead of trying to do it for them.
            // Code left here in case anyone else wants to have a look at fixing the wrapping.
            const int maxLineLength = 80;

            StringBuilder sb = new StringBuilder();
            StringBuilder line = new StringBuilder();

            string[] descWords = msgIn.Split(' ');
            foreach (string word in descWords)
            {
                if (line.Length + word.Length + 1 <= maxLineLength)
                {
                    if (line.Length > 0)
                        line.Append(' ');
                    line.Append(word);
                }
                else
                {
                    sb.AppendLine(line.ToString());
                    line.Clear();
                    line.Append(word);
                }
            }

            if (line.Length > 0)
                sb.AppendLine(line.ToString());

            return sb.ToString();
        }
    }
}
