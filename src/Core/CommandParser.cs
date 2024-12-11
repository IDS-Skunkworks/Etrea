using static Etrea3.OLC.OLC;
using static Etrea3.Core.ActImmortal;
using static Etrea3.Core.ActPlayer;

namespace Etrea3.Core
{
    public static class CommandParser
    {
        public static void Parse(Session session, ref string input)
        {
            string arg = string.Empty;
            var verb = Helpers.GetVerb(ref input);
            if (string.IsNullOrEmpty(verb))
            {
                session.Send($"I'm sorry, I didn't understand that...{Constants.NewLine}");
                return;
            }
            switch(verb.ToLower())
            {
                #region Movement
                case "n":
                case "north":
                case "w":
                case "west":
                case "e":
                case "east":
                case "s":
                case "south":
                case "u":
                case "up":
                case "d":
                case "down":
                case "nw":
                case "northwest":
                case "ne":
                case "northeast":
                case "se":
                case "southeast":
                case "sw":
                case "southwest":
                    Move(session, verb);
                    break;

                case "sit":
                case "stand":
                case "sleep":
                case "rest":
                    ChangePosition(session, verb);
                    break;

                case "recall":
                    PlayerRecall(session);
                    break;

                case "push":
                case "shove":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerPushTarget(session, arg);
                    break;
                #endregion

                #region Inventory
                case "use":
                case "wear":
                case "wield":
                    arg = input.Remove(0, verb.Length).Trim();
                    EquipItem(session, arg);
                    break;

                case "eq":
                case "equip":
                case "equipment":
                    ShowEquipment(session);
                    break;

                case "inv":
                case "inventory":
                case "i":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowInventory(session, verb, arg);
                    break;

                case "vault":
                    arg = input.Remove(0, verb.Length).Trim();
                    DoVaultAction(session, arg);
                    break;

                case "bank":
                    arg = input.Remove(0, verb.Length).Trim();
                    DoBankingAction(session, arg);
                    break;

                case "get":
                case "take":
                    arg = input.Remove(0, verb.Length).Trim();
                    GetItem(session, arg);
                    break;

                case "drop":
                    arg = input.Remove(0, verb.Length).Trim();
                    DropItem(session, arg);
                    break;

                case "give":
                case "trade":
                    arg = input.Remove(0, verb.Length).Trim();
                    TradeItem(session, arg);
                    break;

                case "donate":
                case "don":
                    arg = input.Remove(0, verb.Length).Trim();
                    DonateItem(session, arg);
                    break;

                case "sacrifice":
                case "sac":
                    arg = input.Remove(0, verb.Length).Trim();
                    SacrificeItem(session, arg);
                    break;

                case "remove":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveEquipment(session, arg);
                    break;

                case "read":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerReadScrolls(session, arg);
                    break;

                case "drink":
                case "eat":
                case "quoff":
                case "consume":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerConsumeItem(session, arg);
                    break;
                #endregion

                #region Combat
                case "kill":
                case "k":
                case "attack":
                    arg = input.Remove(0, verb.Length).Trim();
                    StartCombat(session, arg);
                    break;

                case "rollinfo":
                    arg = input.Remove(0, verb.Length).Trim();
                    ToggleRollInfo(session, arg);
                    break;

                case "backstab":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerBackstab(session, arg);
                    break;

                case "flee":
                    PlayerFleeCombat(session);
                    break;
                #endregion

                #region Communication
                case "say":
                    arg = input.Remove(0, verb.Length).Trim();
                    CharSayRoom(session, arg);
                    break;

                case "whisper":
                case "sayto":
                case "tell":
                    arg = input.Remove(0, verb.Length).Trim();
                    CharWhisper(session, arg);
                    break;

                case "shout":
                case "yell":
                    arg = input.Remove(0, verb.Length).Trim();
                    CharShout(session, arg);
                    break;

                case "lang":
                case "language":
                case "languages":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerLanguages(session, arg);
                    break;

                case "mail":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerMailAction(session, arg);
                    break;
                #endregion

                #region Magic
                case "cast":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerCastSpell(session, arg);
                    break;
                #endregion

                #region Shops
                case "shop":
                    arg = input.Remove(0, verb.Length).Trim();
                    EnterShop(session, arg);
                    break;

                case "leave":
                    LeaveShop(session);
                    break;

                case "browse":
                case "peruse":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowShopInventory(session, arg);
                    break;

                case "appraise":
                case "value":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShopAppraiseItem(session, arg);
                    break;

                case "buy":
                    arg = input.Remove(0, verb.Length).Trim();
                    PurchaseItem(session, arg);
                    break;

                case "sell":
                    arg = input.Remove(0, verb.Length).Trim();
                    SellItem(session, arg);
                    break;
                #endregion

                #region Misc
                case "help":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowHelp(session, arg);
                    break;

                case "quit":
                    QuitGame(session);
                    break;

                case "save":
                    SaveCharacter(session);
                    break;

                case "delete":
                    DeleteCharacter(session);
                    break;

                case "prompt":
                    ChangePrompt(session);
                    break;

                case "mine":
                    PlayerMineNode(session);
                    break;

                case "emotes":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerListEmotes(session, arg);
                    break;

                case "learn":
                    arg = input.Remove(0, verb.Length).Trim();
                    LearnSkill(session, arg);
                    break;

                case "train":
                    arg = input.Remove(0, verb.Length).Trim();
                    TrainStat(session, arg);
                    break;

                case "study":
                    arg = input.Remove(0, verb.Length).Trim();
                    StudyMagic(session, arg);
                    break;

                case "look":
                case "l":
                    arg = input.Remove(0, verb.Length).Trim().ToLower();
                    Look(session, arg);
                    break;

                case "who":
                    arg = input.Remove(0, verb.Length).Trim().ToLower();
                    ShowWho(session, arg);
                    break;

                case "pickpocket":
                case "steal":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerPickPocket(session, arg);
                    break;

                case "hide":
                    PlayerHide(session);
                    break;

                case "skills":
                case "skill":
                    ShowSkills(session);
                    break;

                case "spells":
                case "spell":
                    ShowSpells(session);
                    break;

                case "buffs":
                case "buff":
                    ShowBuffs(session);
                    break;

                case "recipe":
                case "recipes":
                    ShowRecipes(session);
                    break;

                case "sdesc":
                case "shortdesc":
                case "shortdescription":
                    PlayerChangeShortDesc(session);
                    break;

                case "ldesc":
                case "longdesc":
                case "description":
                case "longdescription":
                    PlayerChangeLongDesc(session);
                    break;

                case "title":
                    PlayerChangeTitle(session);
                    break;

                case "chpasswd":
                    PlayerChangePassword(session);
                    break;

                case "quest":
                case "quests":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowQuests(session, arg);
                    break;

                case "score":
                case "stats":
                case "charsheet":
                    ShowCharSheet(session);
                    break;

                case "alias":
                    arg = input.Remove(0, verb.Length).Trim();
                    ManagePlayerAliases(session, arg);
                    break;

                case "bet":
                case "gamble":
                case "dice":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayDice(session, arg);
                    break;

                case "craft":
                case "make":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerCraftItem(session, arg);
                    break;
                #endregion

                #region Immortal
                case "olc":
                    StartOLC(session);
                    break;

                case "apikey":
                    arg = input.Remove(0, verb.Length).Trim();
                    GenerateAPIKey(session, arg);
                    break;

                case "shutdown":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShutDownGame(session, arg);
                    break;

                case "motd":
                    arg = input.Remove(0, verb.Length).Trim();
                    MessageOfTheDay(session, arg);
                    break;

                case "spawnnode":
                case "addnode":
                    arg = input.Remove(0, verb.Length).Trim();
                    AddNodeToRoom(session, arg);
                    break;

                case "removenode":
                    RemoveNodeFromRoom(session);
                    break;

                case "donroom":
                    arg = input.Remove(0, verb.Length).Trim();
                    SetDonationRoom(session, arg);
                    break;

                case "tickshop":
                case "refreshshop":
                    arg = input.Remove(0, verb.Length).Trim();
                    TickShop(session, arg);
                    break;

                case "shopinfo":
                case "shopstats":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowShopInfo(session, arg);
                    break;

                case "uptime":
                    ShowUpTime(session);
                    break;

                case "imminvis":
                    ToggleImmInvis(session);
                    break;

                case "backupinfo":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowBackupInfo(session, arg);
                    break;

                case "log":
                    arg = input.Remove(0, verb.Length).Trim();
                    LogAction(session, arg);
                    break;

                case "immheal":
                case "immrestore":
                    arg = input.Remove(0, verb.Length).Trim();
                    ImmHeal(session, arg);
                    break;

                case "vog":
                    arg = input.Remove(0, verb.Length).Trim();
                    VoiceOfGod(session, arg);
                    break;

                case "addexp":
                    arg = input.Remove(0, verb.Length).Trim();
                    ChangeExp(session, arg);
                    break;

                case "giverecipe":
                case "awardrecipe":
                    arg = input.Remove(0, verb.Length).Trim();
                    GiveRecipe(session, arg);
                    break;

                case "removerecipe":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveRecipe(session, arg);
                    break;

                case "giveskill":
                case "awardskill":
                    arg = input.Remove(0, verb.Length).Trim();
                    GiveSkill(session, arg);
                    break;

                case "removeskill":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveSkill(session, arg);
                    break;

                case "givespell":
                case "awardspell":
                    arg = input.Remove(0, verb.Length).Trim();
                    GiveSpell(session, arg);
                    break;

                case "removespell":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveSpell(session, arg);
                    break;

                case "addlang":
                case "addlanguage":
                case "givelang":
                case "givelanguage":
                    arg = input.Remove(0, verb.Length).Trim();
                    AddPlayerLanguage(session, arg);
                    break;

                case "removelang":
                case "removelanguage":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemovePlayerLanguage(session, arg);
                    break;

                case "imminv":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowImmInventory(session, arg);
                    break;

                case "immscore":
                case "immstat":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowImmCharSheet(session, arg);
                    break;

                case "where":
                case "find":
                    arg = input.Remove(0, verb.Length).Trim();
                    FindAsset(session, arg);
                    break;

                case "set":
                    arg = input.Remove(0, verb.Length).Trim();
                    SetActorAttribute(session, arg);
                    break;

                case "list":
                    arg = input.Remove(0, verb.Length).Trim();
                    ListAssets(session, arg);
                    break;

                case "purge":
                    arg = input.Remove(0, verb.Length).Trim();
                    Purge(session, arg);
                    break;

                case "destroy":
                    arg = input.Remove(0, verb.Length).Trim();
                    Destroy(session, arg);
                    break;

                case "slay":
                case "smite":
                    arg = input.Remove(0, verb.Length).Trim();
                    Slay(session, arg);
                    break;

                case "port":
                case "teleport":
                    arg = input.Remove(0, verb.Length).Trim();
                    TeleportToTarget(session, arg);
                    break;

                case "summon":
                    arg = input.Remove(0, verb.Length).Trim();
                    SummonTarget(session, arg);
                    break;

                case "trans":
                case "transfer":
                case "transport":
                    arg = input.Remove(0, verb.Length).Trim();
                    TransferTarget(session, arg);
                    break;

                case "create":
                case "spawn":
                    arg = input.Remove(0, verb.Length).Trim();
                    CreateAsset(session, arg);
                    break;

                case "force":
                    arg = input.Remove(0, verb.Length).Trim();
                    ForceActor(session, arg);
                    break;
                #endregion

                #region Default
                default:
                    if (verb.Length > 1)
                    {
                        if (session.Player.CommandAliases.ContainsKey(verb))
                        {
                            var cmd = session.Player.CommandAliases[verb];
                            var line = $"{cmd} {input.Remove(0, verb.Length).Trim()}";
                            Parse(session, ref line);
                        }
                        else
                        {
                            if (EmoteManager.Instance.EmoteExists(verb))
                            {
                                arg = input.Remove(0, verb.Length).Trim();
                                PlayerEmote(session, verb, arg);
                            }
                            else
                            {
                                session.Send($"%BRT%Sorry, I didn't understand that.%PT%{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        session.Send($"%BRT%Sorry, I didn't understand that.%PT%{Constants.NewLine}");
                    }
                    break;
                #endregion
            }
        }
    }
}
