using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Etrea3.Objects;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Etrea3.Core
{
    public static class Helpers
    {
        private static Random rnd = new Random(DateTime.UtcNow.GetHashCode());
        private static int VowelMask = (1 << 1) | (1 << 5) | (1 << 9) | (1 << 15) | (1 << 21);
        private static readonly ConcurrentDictionary<string, string> colourCodeReplacements = new ConcurrentDictionary<string, string>
        {
            { "%B%", Constants.BoldText },
            { "%PT%", Constants.PlainText },
            { "%RT%", Constants.RedText },
            { "%BT%", Constants.BlueText },
            { "%YT%", Constants.YellowText },
            { "%GT%", Constants.GreenText },
            { "%WT%", Constants.WhiteText },
            { "%MT%", Constants.MagentaText },
            { "%BMT%", Constants.BrightMagentaText },
            { "%BWT%", Constants.BrightWhiteText },
            { "%BYT%", Constants.BrightYellowText },
            { "%BGT%", Constants.BrightGreenText },
            { "%BBT%", Constants.BrightBlueText },
            { "%BRT%", Constants.BrightRedText }
        };

        public static string ParseColourCodes(string line)
        {
            var sb = new StringBuilder(line);
            foreach(var replacement in colourCodeReplacements)
            {
                sb.Replace(replacement.Key, replacement.Value);
            }
            return sb.ToString();
        }

        public static bool AutoWinsDiceGame(int[] rolls, out bool criticalWin)
        {
            Array.Sort(rolls);
            int d1 = rolls[0], d2 = rolls[1], d3 = rolls[2];
            if ((d1 == d2 && d2 == d3) && (d1 >= 4))
            {
                criticalWin = true;
                return true;
            }
            if (d1 == 4 && d2 == 5 && d3 == 6)
            {
                criticalWin = false;
                return true;
            }
            criticalWin = false;
            return false;
        }

        public static bool AutoLosesDiceGame(int[] rolls, out bool criticalLoss)
        {
            Array.Sort(rolls);
            int d1 = rolls[0], d2 = rolls[1], d3 = rolls[2];
            if ((d1 == d2 && d2 == d3) && (d1 <= 3))
            {
                criticalLoss = true;
                return true;
            }
            if (d1 == 1 && d2 == 2 && d3 == 3)
            {
                criticalLoss = false;
                return true;
            }
            criticalLoss = false;
            return false;
        }

        public static bool ValidDiceScore(int[] rolls, out int score)
        {
            Array.Sort(rolls);
            int d1 = rolls[0], d2 = rolls[1], d3 = rolls[2];
            if (d1 == d2)
            {
                score = d3;
                return true;
            }
            if (d2 == d3)
            {
                score = d1;
                return true;
            }
            if (d1 == d3)
            {
                score = d2;
                return true;
            }
            score = 0;
            return false;
        }

        public static bool FleeCombat(Actor fleeing, out int destRID)
        {
            destRID = -1;
            int fleeDC = 15 - fleeing.Level;
            var fleeRoll = RollDice<int>(1, 20);
            var modFleeRoll = Math.Max(1, fleeRoll + CalculateAbilityModifier(fleeing.Dexterity));
            if (modFleeRoll > fleeDC)
            {
                fleeing.TargetQueue.Clear();
                var allExits = RoomManager.Instance.GetRoom(fleeing.CurrentRoom).RoomExits.Values;
                if (fleeing.ActorType == ActorType.NonPlayer)
                {
                    var n = (NPC)fleeing;
                    var availExits = allExits.Where(x => ZoneManager.Instance.GetZoneForRID(x.DestinationRoomID).ZoneID == n.ZoneID).ToList();
                    if (availExits.Count() > 0)
                    {
                        var chosenExit = availExits.GetRandomElement();
                        destRID = chosenExit.DestinationRoomID;
                        return true;
                    }
                }
                else
                {
                    var chosenExit = allExits.ToList().GetRandomElement();
                    destRID = chosenExit.DestinationRoomID;
                    return true;
                }
            }
            return false;
        }

        public static string GetMOTD(Session session)
        {
            int row = 1;
            StringBuilder sb = new StringBuilder();
            bool validInput = false;
            session.Send($"Enter the new Message of the Day.{Constants.NewLine}");
            session.Send($"Enter END on a new line to finish.{Constants.NewLine}");
            while (!validInput)
            {
                session.Send($"[{row}] ");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && input.Trim().Length <= 80)
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        validInput = true;
                    }
                    else
                    {
                        sb.AppendLine(input.Trim());
                        row++;
                        if (row > 30)
                        {
                            validInput = true;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public static string GetMobProgScript(Session session)
        {
            int row = 1;
            StringBuilder sb = new StringBuilder();
            session.Send($"Enter the MobProg LUA Script{Constants.NewLine}");
            session.Send($"Enter //DONE on a new line to finish.{Constants.NewLine}");
            while (true)
            {
                session.Send($"[{row}] ");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Trim().ToUpper() == "//DONE")
                    {
                        break;
                    }
                    else
                    {
                        sb.AppendLine(input.Trim());
                        row++;
                    }
                }
            }
            return sb.ToString().Trim();
        }

        public static string GetLongDescription(Session session)
        {
            int row = 1;
            StringBuilder sb = new StringBuilder();
            session.Send($"Enter the long description. This should be 30 lines or less{Constants.NewLine}");
            session.Send($"and each line should be 80 characters or less.{Constants.NewLine}");
            session.Send($"Descriptions can be changed later if you want.{Constants.NewLine}");
            session.Send($"Enter END on a new line to finish.{Constants.NewLine}");
            while (true)
            {
                session.Send($"[{row}] ");
                var input = session.Read();
                if (!string.IsNullOrEmpty(input) && input.Trim().Length <= 80)
                {
                    if (input.Trim().ToUpper() == "END")
                    {
                        break;
                    }
                    else
                    {
                        sb.AppendLine(input.Trim());
                        row++;
                        if (row > 30)
                        {
                            break;
                        }
                    }
                }
            }
            return sb.ToString().Trim();
        }

        public static string GetFullDirectionString(string dir)
        {
            string direction = string.Empty;
            if (dir.Length == 1 || dir.Length == 2)
            {
                switch (dir)
                {
                    case "u":
                        direction = "up";
                        break;

                    case "d":
                        direction = "down";
                        break;

                    case "n":
                        direction = "north";
                        break;

                    case "s":
                        direction = "south";
                        break;

                    case "e":
                        direction = "east";
                        break;

                    case "w":
                        direction = "west";
                        break;

                    case "nw":
                        direction = "northwest";
                        break;

                    case "ne":
                        direction = "northeast";
                        break;

                    case "sw":
                        direction = "southwest";
                        break;

                    case "se":
                        direction = "southeast";
                        break;
                }
            }
            else
            {
                direction = dir;
            }
            return direction;
        }

        public static string GetVerb(ref string input)
        {
            string[] elements = input.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return elements.Length > 0 ? elements[0].Trim() : string.Empty;
        }

        public static string SerialiseEtreaObject<T>(object etreaObject)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.SerializeObject((T)etreaObject, settings);
        }

        public static T DeserialiseEtreaObject<T>(string etreaObject)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.DeserializeObject<T>(etreaObject, settings);
        }

        public static T Clone<T>(T src)
        {
            if (!typeof(T).IsSerializable)
            {
                Game.LogMessage($"ERROR: Cannot clone {src.GetType().Name}, it is not serialisable", LogLevel.Error);
                return default(T);
            }
            return DeserialiseEtreaObject<T>(SerialiseEtreaObject<T>(src));
        }

        public static T RollDice<T>(int numDice, int sides) where T : struct, IConvertible
        {
            if (!typeof(T).IsPrimitive || typeof(T) == typeof(bool) || typeof(T) == typeof(char) || typeof(T) == typeof(string))
            {
                Game.LogMessage($"ERROR: Error in Helpers.RollDice<T>(): T was not numeric ({typeof(T)})", LogLevel.Error);
                return default(T);
            }
            int result = 0;
            for (int i = 0; i < numDice; i++)
            {
                result += rnd.Next(1, sides + 1);
            }
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public static bool IsCharAVowel(char c)
        {
            return (c > 64) && ((VowelMask & (1 << ((c | 0x20) % 32))) != 0);
        }

        public static int CalculateAbilityModifier(int abilityScore)
        {
            return (abilityScore - 10) / 2;
        }

        public static int GetPurchasePrice(Session session, int basePrice)
        {
            var chaModifier = CalculateAbilityModifier(session.Player.Charisma);
            int modPrice = basePrice;
            if (session.Player.HasSkill("Salesman"))
            {
                chaModifier += 2;
            }
            if (chaModifier < 0)
            {
                // increase purchase price based on low CHA modifier
                int posMod = chaModifier * -1; // convert negative CHA modifier to positive for calculations
                for (int i = 0; i < posMod; i++)
                {
                    modPrice += Convert.ToInt32(Math.Round(basePrice * 0.025, 0));
                }
                modPrice = Math.Max(1, modPrice); // ensure the minimum price is always 1
            }
            if (chaModifier > 0)
            {
                // decrease price based on high CHA modifier
                for (int i = 0; i < chaModifier; i++)
                {
                    modPrice -= Convert.ToInt32(Math.Round(basePrice * 0.025, 0));
                }
                modPrice = Math.Max(1, modPrice); // ensure the minimum price is always 1
            }
            return modPrice;
        }

        public static int GetSalePrice(Session session, int basePrice)
        {
            var chaModifier = CalculateAbilityModifier(session.Player.Charisma);
            int modPrice = basePrice;
            if (session.Player.HasSkill("Salesman"))
            {
                chaModifier += 2;
            }
            if (chaModifier < 0)
            {
                // decrease price for low CHA modifier
                int posMod = chaModifier * -1; // convert negative CHA modifier to positive for calculations
                for (int i = 0; i < posMod; i++)
                {
                    modPrice -= Convert.ToInt32(Math.Round(basePrice * 0.025, 0));
                }
                modPrice = Math.Max(1, modPrice); // ensure the minimum price is always 1
            }
            if (chaModifier > 0)
            {
                // increase price for high CHA modifier
                for (int i = 0; i < chaModifier; i++)
                {
                    modPrice += Convert.ToInt32(Math.Round(basePrice * 0.025, 0));
                }
                modPrice = Math.Max(1, modPrice); // ensure the minimum price is always 1
            }
            var purchasePrice = GetPurchasePrice(session, basePrice);
            modPrice = Math.Min(purchasePrice, modPrice); // ensure our sale price can never exceed the purcase price to avoid expoits :)
            return modPrice;
        }

        public static string GetDamageString(int dmg, int maxHP)
        {
            var percDmg = dmg / maxHP * 100;
            if (percDmg >= 90)
            {
                return "destroys";
            }
            if (percDmg >= 80)
            {
                return "devastates";
            }
            if (percDmg >= 50)
            {
                return "eviscerates";
            }
            if (percDmg >= 20)
            {
                return "smashes";
            }
            if (percDmg >= 10)
            {
                return "hurts";
            }
            if (percDmg >= 5)
            {
                return "wounds";
            }
            return "grazes";
        }

        public static string GetQuotedString(string input)
        {
            Regex pattern = new Regex(@"(['""])(.*?)\1");
            var match = pattern.Match(input);
            if (match.Success)
            {
                return match.Groups[2].Value;
            }
            return null;
        }
    }
}
