using static Etrea3.Core.ActImmortal;
using static Etrea3.Core.ActPlayer;
using static Etrea3.OLC.OLC;

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
                    PlayerPushTarget(session, ref arg);
                    break;
                #endregion

                #region Inventory
                case "use":
                case "wear":
                case "wield":
                    arg = input.Remove(0, verb.Length).Trim();
                    EquipItem(session, ref arg);
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
                    ShowInventory(session, ref verb, arg);
                    break;

                case "vault":
                    arg = input.Remove(0, verb.Length).Trim();
                    DoVaultAction(session, ref arg);
                    break;

                case "bank":
                    arg = input.Remove(0, verb.Length).Trim();
                    DoBankingAction(session, ref arg);
                    break;

                case "get":
                case "take":
                    arg = input.Remove(0, verb.Length).Trim();
                    GetItem(session, ref arg);
                    break;

                case "drop":
                    arg = input.Remove(0, verb.Length).Trim();
                    DropItem(session, arg);
                    break;

                case "give":
                case "trade":
                    arg = input.Remove(0, verb.Length).Trim();
                    TradeItem(session, ref arg);
                    break;

                case "donate":
                case "don":
                    arg = input.Remove(0, verb.Length).Trim();
                    DonateItem(session, ref arg);
                    break;

                case "sacrifice":
                case "sac":
                    arg = input.Remove(0, verb.Length).Trim();
                    SacrificeItem(session, ref arg);
                    break;

                case "remove":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveEquipment(session, ref arg);
                    break;

                case "read":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerReadScrolls(session, ref arg);
                    break;

                case "drink":
                case "eat":
                case "quoff":
                case "consume":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerConsumeItem(session, ref arg);
                    break;
                #endregion

                #region Combat
                case "kill":
                case "k":
                case "attack":
                    arg = input.Remove(0, verb.Length).Trim();
                    StartCombat(session, ref arg);
                    break;

                case "rollinfo":
                    arg = input.Remove(0, verb.Length).Trim();
                    ToggleRollInfo(session, ref arg);
                    break;

                case "backstab":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerBackstab(session, ref arg);
                    break;

                case "flee":
                    PlayerFleeCombat(session);
                    break;
                #endregion

                #region Communication
                case "say":
                    arg = input.Remove(0, verb.Length).Trim();
                    CharSayRoom(session, ref arg);
                    break;

                case "whisper":
                case "sayto":
                case "tell":
                    arg = input.Remove(0, verb.Length).Trim();
                    CharWhisper(session, ref arg);
                    break;

                case "shout":
                case "yell":
                    arg = input.Remove(0, verb.Length).Trim();
                    CharShout(session, ref arg);
                    break;

                case "lang":
                case "language":
                case "languages":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerLanguages(session, arg);
                    break;

                case "mail":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerMailAction(session, ref arg);
                    break;
                #endregion

                #region Magic
                case "cast":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerCastSpell(session, ref arg);
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
                    ShowShopInventory(session, ref arg);
                    break;

                case "appraise":
                case "value":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShopAppraiseItem(session, ref arg);
                    break;

                case "buy":
                    arg = input.Remove(0, verb.Length).Trim();
                    PurchaseItem(session, ref arg);
                    break;

                case "sell":
                    arg = input.Remove(0, verb.Length).Trim();
                    SellItem(session, ref arg);
                    break;
                #endregion

                #region Misc
                case "psummon":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerSummon(session, ref arg);
                    break;

                case "toggle":
                    arg = input.Remove(0, verb.Length).Trim();
                    TogglePlayerFlag(session, ref arg);
                    break;

                case "help":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowHelp(session, ref arg);
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
                    LearnSkill(session, ref arg);
                    break;

                case "train":
                    arg = input.Remove(0, verb.Length).Trim();
                    TrainStat(session, ref arg);
                    break;

                case "study":
                    arg = input.Remove(0, verb.Length).Trim();
                    StudyMagic(session, ref arg);
                    break;

                case "look":
                case "l":
                    arg = input.Remove(0, verb.Length).Trim().ToLower();
                    Look(session, ref arg);
                    break;

                case "who":
                    arg = input.Remove(0, verb.Length).Trim().ToLower();
                    ShowWho(session, arg);
                    break;

                case "pickpocket":
                case "steal":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerPickPocket(session, ref arg);
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
                    ShowQuests(session, ref arg);
                    break;

                case "score":
                case "stats":
                case "charsheet":
                    ShowCharSheet(session);
                    break;

                case "alias":
                    arg = input.Remove(0, verb.Length).Trim();
                    ManagePlayerAliases(session, ref arg);
                    break;

                case "bet":
                case "gamble":
                case "dice":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayDice(session, ref arg);
                    break;

                case "craft":
                case "make":
                    arg = input.Remove(0, verb.Length).Trim();
                    PlayerCraftItem(session, ref arg);
                    break;
                #endregion

                #region Immortal
                case "ban":
                case "block":
                    arg = input.Remove(0, verb.Length).Trim();
                    BanIPAddress(session, ref arg);
                    break;

                case "unban":
                case "unblock":
                    arg = input.Remove(0, verb.Length).Trim();
                    UnBanIPAddress(session, ref arg);
                    break;

                case "snoop":
                    arg = input.Remove(0, verb.Length).Trim();
                    SnoopConnection(session, ref arg);
                    break;

                case "nosnoop":
                    StopSnoop(session);
                    break;

                case "mute":
                    arg = input.Remove(0, verb.Length).Trim();
                    MutePlayer(session, ref arg);
                    break;

                case "unmute":
                    arg = input.Remove(0, verb.Length).Trim();
                    UnMutePlayer(session, ref arg);
                    break;
                    
                case "freeze":
                    arg = input.Remove(0, verb.Length).Trim();
                    FreezePlayer(session, ref arg);
                    break;

                case "thaw":
                    arg = input.Remove(0, verb.Length).Trim();
                    ThawPlayer(session, ref arg);
                    break;

                case "zreset":
                    arg = input.Remove(0, verb.Length).Trim();
                    ZoneReset(session, ref arg);
                    break;

                case "olc":
                    StartOLC(session);
                    break;

                case "apikey":
                    arg = input.Remove(0, verb.Length).Trim();
                    GenerateAPIKey(session, ref arg);
                    break;

                case "releaselock":
                    arg = input.Remove(0, verb.Length).Trim();
                    ReleaseOLCLock(session, ref arg);
                    break;

                case "shutdown":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShutDownGame(session, ref arg);
                    break;

                case "motd":
                    arg = input.Remove(0, verb.Length).Trim();
                    MessageOfTheDay(session, ref arg);
                    break;

                case "spawnnode":
                case "addnode":
                    arg = input.Remove(0, verb.Length).Trim();
                    AddNodeToRoom(session, ref arg);
                    break;

                case "removenode":
                    RemoveNodeFromRoom(session);
                    break;

                case "donroom":
                    arg = input.Remove(0, verb.Length).Trim();
                    SetDonationRoom(session, ref arg);
                    break;

                case "tickshop":
                case "refreshshop":
                    arg = input.Remove(0, verb.Length).Trim();
                    TickShop(session, ref arg);
                    break;

                case "shopinfo":
                case "shopstats":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowShopInfo(session, ref arg);
                    break;

                case "uptime":
                    ShowUpTime(session);
                    break;

                case "imminvis":
                    ToggleImmInvis(session);
                    break;

                case "backupinfo":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowBackupInfo(session, ref arg);
                    break;

                case "log":
                    arg = input.Remove(0, verb.Length).Trim();
                    LogAction(session, ref arg);
                    break;

                case "immheal":
                case "immrestore":
                    arg = input.Remove(0, verb.Length).Trim();
                    ImmHeal(session, ref arg);
                    break;

                case "vog":
                    arg = input.Remove(0, verb.Length).Trim();
                    VoiceOfGod(session, ref arg);
                    break;

                case "addexp":
                    arg = input.Remove(0, verb.Length).Trim();
                    ChangeExp(session, ref arg);
                    break;

                case "giverecipe":
                case "awardrecipe":
                    arg = input.Remove(0, verb.Length).Trim();
                    GiveRecipe(session, ref arg);
                    break;

                case "removerecipe":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveRecipe(session, ref arg);
                    break;

                case "giveskill":
                case "awardskill":
                    arg = input.Remove(0, verb.Length).Trim();
                    GiveSkill(session, ref arg);
                    break;

                case "removeskill":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveSkill(session, ref arg);
                    break;

                case "givespell":
                case "awardspell":
                    arg = input.Remove(0, verb.Length).Trim();
                    GiveSpell(session, ref arg);
                    break;

                case "removespell":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemoveSpell(session, ref arg);
                    break;

                case "addlang":
                case "addlanguage":
                case "givelang":
                case "givelanguage":
                    arg = input.Remove(0, verb.Length).Trim();
                    AddPlayerLanguage(session, ref arg);
                    break;

                case "removelang":
                case "removelanguage":
                    arg = input.Remove(0, verb.Length).Trim();
                    RemovePlayerLanguage(session, ref arg);
                    break;

                case "imminv":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowImmInventory(session, ref arg);
                    break;

                case "immscore":
                case "immstat":
                    arg = input.Remove(0, verb.Length).Trim();
                    ShowImmCharSheet(session, ref arg);
                    break;

                case "where":
                case "find":
                    arg = input.Remove(0, verb.Length).Trim();
                    FindAsset(session, ref arg);
                    break;

                case "set":
                    arg = input.Remove(0, verb.Length).Trim();
                    SetActorAttribute(session, ref arg);
                    break;

                case "list":
                    arg = input.Remove(0, verb.Length).Trim();
                    ListAssets(session, ref arg);
                    break;

                case "purge":
                    arg = input.Remove(0, verb.Length).Trim();
                    Purge(session, ref arg);
                    break;

                case "destroy":
                    arg = input.Remove(0, verb.Length).Trim();
                    Destroy(session, ref arg);
                    break;

                case "slay":
                case "smite":
                    arg = input.Remove(0, verb.Length).Trim();
                    Slay(session, ref arg);
                    break;

                case "port":
                case "teleport":
                    arg = input.Remove(0, verb.Length).Trim();
                    TeleportToTarget(session, ref arg);
                    break;

                case "summon":
                    arg = input.Remove(0, verb.Length).Trim();
                    SummonTarget(session, ref arg);
                    break;

                case "trans":
                case "transfer":
                case "transport":
                    arg = input.Remove(0, verb.Length).Trim();
                    TransferTarget(session, ref arg);
                    break;

                case "create":
                case "spawn":
                    arg = input.Remove(0, verb.Length).Trim();
                    CreateAsset(session, ref arg);
                    break;

                case "force":
                    arg = input.Remove(0, verb.Length).Trim();
                    ForceActor(session, ref arg);
                    break;

                case "flags":
                    ShowFlags(session);
                    break;

                case "checkmemory":
                    arg = input.Remove(0, verb.Length).Trim();
                    CheckMobMemory(session, ref arg);
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
                                PlayerEmote(session, verb, ref arg);
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
