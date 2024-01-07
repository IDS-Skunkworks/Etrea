using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kingdoms_of_Etrea.Entities;
using static Kingdoms_of_Etrea.OLC.OLC;

namespace Kingdoms_of_Etrea.Core
{
    internal static partial class CommandParser
    {
        internal static void ParseCommand(ref Descriptor desc, string input)
        {
            switch(GetVerb(ref input).ToLower())
            {
                #region MiscCommands
                case "exorcise":
                    ExorciseCursedItem(ref desc, ref input);
                    break;

                case "alias":
                    DoPlayerAlias(ref desc, ref input);
                    break;

                case "lang":
                case "language":
                    PlayerLanguages(ref desc, ref input);
                    break;

                case "bank":
                    PlayerBanking(ref desc, ref input);
                    break;

                case "quest":
                case "quests":
                    PlayerQuests(ref desc, ref input);
                    break;

                case "mail":
                    PlayerMail(ref desc, ref input);
                    break;

                case "describe":
                    DescribeSkill(ref desc, ref input);
                    break;

                case "hire":
                    HireFollower(ref desc, ref input);
                    break;

                case "dismiss":
                    DismissFollower(ref desc, ref input);
                    break;

                case "follower":
                    ShowFollowerInfo(ref desc, ref input);
                    break;

                case "bet":
                case "gamble":
                case "dice":
                    DoDiceGamble(ref desc, ref input);
                    break;

                case "emote":
                case "emotes":
                    ShowAllEmotes(ref desc, ref input);
                    break;

                case "showrolls":
                    desc.Send($"Setting ShowRolls to {!desc.Player.ShowDetailedRollInfo}{Constants.NewLine}");
                    desc.Player.ShowDetailedRollInfo = !desc.Player.ShowDetailedRollInfo;
                    break;

                case "browse":
                case "peruse":
                    ListShopWares(ref desc, ref input);
                    break;

                case "appraise":
                case "value":
                    AppraiseItemForSale(ref desc, ref input);
                    break;

                case "buy":
                case "purchase":
                    BuyItemFromShop(ref desc, ref input);
                    break;

                case "learn":
                    LearnSkillOrSpell(ref desc, ref input);
                    break;

                case "train":
                    TrainPlayerStat(ref desc, ref input);
                    break;

                case "donate":
                    DonateItem(ref desc, ref input);
                    break;

                case "sell":
                    SellItemToShop(ref desc, ref input);
                    break;

                case "look":
                case "l":
                    Look(ref desc, ref input);
                    break;

                case "who":
                    Who(ref desc);
                    break;

                case "save":
                    desc.Send($"Saving your character...{DatabaseManager.SavePlayerNew(ref desc, false)}{Constants.NewLine}");
                    break;

                case "quit":
                    DatabaseManager.SavePlayerNew(ref desc, false);
                    desc.Send($"Goodbye, we hope to see you again soon!{Constants.NewLine}");
                    RoomManager.Instance.UpdatePlayersInRoom(desc.Player.CurrentRoom, ref desc, true, false, true, false);
                    SessionManager.Instance.Close(desc);
                    break;

                case "delete":
                    DeleteCharacter(ref desc);
                    break;

                case "unlock":
                case "lock":
                    LockOrUnlockDoor(ref desc, ref input);
                    break;

                case "open":
                case "close":
                    OpenOrCloseDoor(ref desc, ref input);
                    break;
                #endregion

                #region SkillCommands
                case "hide":
                    DoHideSkill(ref desc);
                    break;

                case "backstab":
                    Backstab(ref desc, ref input);
                    break;

                case "pickpocket":
                    Pickpocket(ref desc, ref input);
                    break;

                case "mine":
                    MineResourceNode(ref desc, ref input);
                    break;

                case "craft":
                    CraftItem(ref desc, ref input);
                    break;
                #endregion

                #region Movement
                case "stand":
                case "rest":
                case "sit":
                    ChangePlayerPosition(ref desc, ref input);
                    break;

                case "position":
                    desc.Send($"You are currently {desc.Player.Position.ToString().ToLower()}{Constants.NewLine}");
                    break;

                case "push":
                    PushTarget(ref desc, ref input);
                    break;

                case "recall":
                    DoRecall(ref desc);
                    break;

                case "down":
                case "d":
                case "up":
                case "u":
                case "west":
                case "w":
                case "east":
                case "e":
                case "north":
                case "n":
                case "south":
                case "s":
                case "northwest":
                case "nw":
                case "southwest":
                case "sw":
                case "northeast":
                case "ne":
                case "southeast":
                case "se":
                    MovePlayer(ref desc, ref input);
                    break;
                #endregion

                #region Inventory
                case "equipment":
                case "equip":
                case "eq":
                    ShowEquippedItems(ref desc);
                    break;

                case "use":
                case "wear":
                case "wield":
                    EquipItem(ref desc, ref input);
                    break;

                case "read":
                    ReadScroll(ref desc, ref input);
                    break;

                case "remove":
                    RemoveEquippedItem(ref desc, ref input);
                    break;

                case "inventory":
                case "i":
                case "inv":
                    ShowCharInventory(ref desc);
                    break;

                case "get":
                    GetItemFromRoom(ref desc, ref input);
                    break;

                case "drink":
                case "quoff":
                case "eat":
                case "consume":
                    ConsumeItem(ref desc, ref input);
                    break;

                case "drop":
                    DropCharacterItem(ref desc, ref input);
                    break;

                case "give":
                case "trade":
                    GiveItemToTarget(ref desc, ref input);
                    break;

                case "vault":
                    PlayerVault(ref desc, ref input);
                    break;
                #endregion

                #region CharInfo
                case "pvp":
                    ShowOrTogglePVPFlag(ref desc, ref input);
                    break;

                case "buff":
                case "buffs":
                    ShowBuffs(ref desc, ref input);
                    break;

                case "skill":
                case "skills":
                    ShowPlayerSkills(ref desc);
                    break;

                case "spell":
                case "spells":
                    ShowPlayerSpells(ref desc);
                    break;

                case "recipe":
                case "recipes":
                    ShowPlayerRecipes(ref desc, ref input);
                    break;

                case "ldesc":
                case "longdesc":
                    ChangeCharacterLongDesc(ref desc);
                    break;

                case "sdesc":
                case "shortdesc":
                    ChangeCharacterShortDesc(ref desc);
                    break;

                case "title":
                    ChangeCharacterTitle(ref desc, ref input);
                    break;

                case "passwd":
                    ChangePlayerPassword(ref desc, ref input);
                    break;

                case "score":
                case "stats":
                case "charsheet":
                    ShowCharSheet(ref desc);
                    break;
                #endregion

                #region Combat
                case "cast":
                    CastSpell(ref desc, ref input);
                    break;

                case "attack":
                case "kill":
                case "k":
                    StartCombat(ref desc, ref input);
                    break;

                case "pkill":
                    StartPVPCombat(ref desc, ref input);
                    break;

                case "flee":
                    FleeCombat(ref desc);
                    break;
                #endregion

                #region Communication
                case "say":
                    SayToRoom(ref desc, ref input);
                    break;

                case "whisper":
                case "tell":
                    SayToCharacter(ref desc, ref input);
                    break;
                #endregion

                // IMM only commands
                #region ImmOnlyCommands
                case "motd":
                    MOTD(ref desc, ref input);
                    break;

                case "givelang":
                    GivePlayerLanguage(ref desc, ref input);
                    break;

                case "removelang":
                    RemovePlayerLanguage(ref desc, ref input);
                    break;

                case "connection":
                case "connections":
                    GetCurrentConnections(ref desc);
                    break;

                case "addnode":
                    AddResouceNodeToRoom(ref desc, ref input);
                    break;

                case "addskill":
                    AddSkillToPlayer(ref desc, ref input);
                    break;

                case "addrecipe":
                    AddRecipeToPlayer(ref desc, ref input);
                    break;

                case "removerecipe":
                    RemoveRecipeFromPlayer(ref desc, ref input);
                    break;

                case "removeskill":
                case "remskill":
                    RemoveSkillFromPlayer(ref desc, ref input);
                    break;

                case "addspell":
                    AddSpellToPlayer(ref desc, ref input);
                    break;

                case "removespell":
                case "remspell":
                    RemoveSpellFromPlayer(ref desc, ref input);
                    break;

                case "transfer":
                case "transport":
                case "trans":
                    TransferPlayer(ref desc, ref input);
                    break;

                case "uptime":
                    ShowUptimeInfo(ref desc);
                    break;

                case "imminv":
                    DoImmInv(ref desc, ref input);
                    break;

                case "where":
                    DoWhereCheck(ref desc, ref input);
                    break;

                case "npclist":
                case "npcwho":
                    ListAllNPCS(ref desc);
                    break;

                case "immsight":
                case "immstat":
                    ImmSight(ref desc, ref input);
                    break;

                case "purge":
                    Purge(ref desc, ref input);
                    break;

                case "list":
                    GetObjectList(ref desc, ref input);
                    break;

                case "immheal":
                    ImmHeal(ref desc, ref input);
                    break;

                case "olc":
                case "constructor":
                    StartOLC(ref desc);
                    break;

                case "create":
                case "spawn":
                    ImmSpawnItem(ref desc, ref input);
                    break;

                case "saveall":
                    SaveAllPlayers(ref desc);
                    break;

                case "imminvis":
                    ImmInvis(ref desc);
                    break;

                case "slay":
                case "smite":
                    Slay(ref desc, ref input);
                    break;

                case "force":
                    DoForce(ref desc, ref input);
                    break;

                case "set":
                    ImmSetStat(ref desc, ref input);
                    break;

                case "cleardesc":
                    ClearCharacterDescription(ref desc, ref input);
                    break;

                case "shutdown":
                    ShutdownWorld(ref desc, ref input);
                    break;

                case "voiceofgod":
                case "vog":
                    VoiceOfGod(ref desc, ref input);
                    break;

                case "teleport":
                case "port":
                    ImmTeleportToPlayer(ref desc, ref input);
                    break;

                case "summon":
                    ImmSummonPlayer(ref desc, ref input);
                    break;

                case "lastbackup":
                case "backupinfo":
                    ShowLastBackup(ref desc);
                    break;

                default:
                    if(input.Length > 1)
                    {
                        var verb = GetVerb(ref input);
                        if(desc.Player.CommandAliases.ContainsKey(verb))
                        {
                            var command = desc.Player.CommandAliases[verb];
                            var line = input.Remove(0, verb.Length).Trim();
                            ParseCommand(ref desc, $"{command} {line}");
                        }
                        else
                        {
                            var emote = EmoteManager.Instance.GetEmoteByName(GetVerb(ref input));
                            if (emote != null)
                            {
                                DoEmote(ref desc, ref input, emote);
                            }
                            else
                            {
                                desc.Send($"Sorry, I didn't understand that.{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"Sorry, I didn't understand that.{Constants.NewLine}");
                    }
                    break;
                    #endregion
            }
        }

        #region Functions
        private static string[] TokeniseInput(ref string input)
        {
            List<string> tokens = new List<string>();
            int startIndex = 0;
            bool inToken = false;
            int length = input.Length;

            for (int i = 0; i < length; i++)
            {
                char c = input[i];
                if (char.IsSeparator(c))
                {
                    if (inToken)
                    {
                        tokens.Add(input.Substring(startIndex, i - startIndex).Trim());
                        inToken = false;
                    }
                }
                else if (!inToken)
                {
                    inToken = true;
                    startIndex = i;
                }
            }

            if (inToken)
            {
                tokens.Add(input.Substring(startIndex, length - startIndex).Trim());
            }

            return tokens.ToArray();
        }


        private static string GetSkillOrSpellName(ref string input)
        {
            Regex rx = new Regex("\"(.*?)\"");
            if(rx.Match(input).Success)
            {
                return rx.Match(input).Groups[1].Value.Trim();
            }
            return string.Empty;
        }

        private static string GetVerb(ref string line)
        {
            return line.Split(' ').First();
        }

        private static NPC GetTargetNPC(ref Descriptor desc, string target)
        {
            try
            {
                var targetNo = target.IndexOf(':') > -1 ? target.Split(':')[1].Trim() : string.Empty;
                if (!string.IsNullOrEmpty(targetNo))
                {
                    if (int.TryParse(targetNo, out int index))
                    {
                        target = target.Replace(targetNo, string.Empty).Replace(":", string.Empty).Trim();
                        target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                        var matchingNPCs = RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).ToList();
                        if (index >= 0)
                        {
                            if (matchingNPCs.Count >= 0 && matchingNPCs.Count >= index)
                            {
                                return matchingNPCs[index];
                            }
                        }
                    }
                }
                else
                {
                    target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                    return RoomManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom).Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error in CommandParser.GetTargetNPC(): {ex.Message}", LogLevel.Error, true);
            }
            return null;
        }

        private static InventoryItem GetTargetItem(ref Descriptor desc, string target, bool fromInventory, bool fromVault = false)
        {
            try
            {
                var targetNo = target.IndexOf(':') > -1 ? target.Split(':')[1].Trim() : string.Empty;
                if (fromInventory)
                {
                    if (!string.IsNullOrEmpty(targetNo))
                    {
                        if (int.TryParse(targetNo, out int index))
                        {
                            target = target.Replace(targetNo, string.Empty).Replace(":", string.Empty).Trim();
                            target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                            var invItems = desc.Player.Inventory.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).ToList();
                            if (index >= 0)
                            {
                                if (invItems.Count >= 0 && invItems.Count >= index)
                                {
                                    return invItems[index];
                                }
                            }
                        }
                    }
                    else
                    {
                        target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                        return desc.Player.Inventory.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    }
                }
                else
                {
                    if(fromVault)
                    {
                        if(!string.IsNullOrEmpty(targetNo))
                        {
                            if(int.TryParse(targetNo, out int index))
                            {
                                target = target.Replace(targetNo, string.Empty).Replace(":", string.Empty).Trim();
                                target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                                var vaultItems = desc.Player.VaultStore.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).ToList();
                                if(index >= 0)
                                {
                                    if(vaultItems.Count >= 0 && vaultItems.Count >= index)
                                    {
                                        return vaultItems[index];
                                    }
                                }
                            }
                        }
                        else
                        {
                            target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                            return desc.Player.VaultStore.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(targetNo))
                        {
                            if (int.TryParse(targetNo, out int index))
                            {
                                target = target.Replace(targetNo, string.Empty).Replace(":", string.Empty).Trim();
                                target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                                var roomItems = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).ToList();
                                if (index >= 0)
                                {
                                    if (roomItems.Count >= 0 && roomItems.Count >= index)
                                    {
                                        return roomItems[index];
                                    }
                                }
                            }
                        }
                        else
                        {
                            target = Regex.Replace(target, @"[^\w\d\s]", string.Empty);
                            return RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error in CommandParser.GetTargetItem(): {ex.Message}", LogLevel.Error, true);
            }
            return null;
        }

        private static bool ValidateInput(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && Encoding.UTF8.GetByteCount(input) == input.Length;
        }

        private static T ParseEnumValue<T>(ref string valIn) where T : struct, Enum
        {
            if (Enum.TryParse<T>(valIn, true, out T retval))
            {
                return retval;
            }
            return default;
        }
        #endregion
    }
}