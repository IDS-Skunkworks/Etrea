using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Etrea3.Objects;
using System.IO;

namespace Etrea3.Core
{
    public sealed class Game
    {
        private static DateTime startTime;
        private static DateTime lastBackupTime;
        private static bool backupCompleted = false;
        private static int backupsRetained;
        private static int zoneTick;
        private static int npcTick;
        private static int combatTick;
        private static int autoSaveTick;
        private static int buffTick;
        private static int backupTick;
        private static int donationRoomID;
        private static int startRoomID;
        private static int limboRoomID;
        private static int maxIdleSeconds;
        private static ulong tickCount = 0;
        private static bool disconnectIdleImms = false;

        private static Timer zoneTickTimer = new Timer();
        private static Timer npcTickTimer = new Timer();
        private static Timer combatTickTimer = new Timer();
        private static Timer autoSaveTimer = new Timer();
        private static Timer buffTickTimer = new Timer();
        private static Timer backupTickTimer = new Timer();
        private static Timer cleanupTimer = new Timer();

        private static TaskCompletionSource<bool> tcs;

        public static ulong TickCount => tickCount;
        public static DateTime StartTime => startTime;
        public static int DonationRoomID => donationRoomID;
        public static int PlayerStartRoom => startRoomID;
        public static int Limbo => limboRoomID;
        public static int MaxIdleSeconds => maxIdleSeconds;
        public static bool DisconnectIdleImms => disconnectIdleImms;

        public Game()
        {
            backupsRetained = int.Parse(ConfigurationManager.AppSettings["RetainBackupCount"]);
            zoneTick = int.Parse(ConfigurationManager.AppSettings["ZoneTick"]);
            npcTick = int.Parse(ConfigurationManager.AppSettings["NPCTick"]);
            combatTick = int.Parse(ConfigurationManager.AppSettings["CombatTick"]);
            autoSaveTick = int.Parse(ConfigurationManager.AppSettings["AutosaveTick"]);
            buffTick = int.Parse(ConfigurationManager.AppSettings["BuffTick"]);
            backupTick = int.Parse(ConfigurationManager.AppSettings["BackupTick"]);
            donationRoomID = int.Parse(ConfigurationManager.AppSettings["DonationRoom"]);
            startRoomID = int.Parse(ConfigurationManager.AppSettings["StartRoom"]);
            limboRoomID = int.Parse(ConfigurationManager.AppSettings["LimboRoom"]);
            maxIdleSeconds = int.Parse(ConfigurationManager.AppSettings["MaxIdleTime"]);
            disconnectIdleImms = bool.Parse(ConfigurationManager.AppSettings["DisconnectIdleImmortals"]);

            zoneTickTimer.Interval = zoneTick * 1000;
            npcTickTimer.Interval = npcTick * 1000;
            combatTickTimer.Interval = combatTick * 1000;
            autoSaveTimer.Interval = autoSaveTick * 1000;
            buffTickTimer.Interval = buffTick * 1000;
            backupTickTimer.Interval = backupTick * 1000;
            cleanupTimer.Interval = 120 * 1000;
        }

        public static void SetDonationRoom(int newRID)
        {
            if (newRID == 0)
            {
                donationRoomID = int.Parse(ConfigurationManager.AppSettings["DonationRoom"]);
                return;
            }
            donationRoomID = newRID;
        }

        public static bool GetBackupInfo(out DateTime backupTime, out int backupTimer)
        {
            backupTime = DateTime.UtcNow;
            backupTimer = backupTick;
            if (backupCompleted)
            {
                backupTime = lastBackupTime;
                return true;
            }
            return false;
        }

        public static void LogMessage(string message, LogLevel level, bool writeToScreen)
        {
            Logger.LogMessage(message, level, writeToScreen);
        }

        public async Task Run()
        {
            tcs = new TaskCompletionSource<bool>();
            if (DatabaseManager.ClearLogTable(out int rowCount))
            {
                LogMessage($"INFO: Log Table cleared, {rowCount} items removed.", LogLevel.Info, true);
            }
            else
            {
                LogMessage($"ERROR: Failed to clear Log Table", LogLevel.Error, true);
            }
            startTime = DateTime.UtcNow;
            bool dbLoad = LoadDatabase();
            if (!dbLoad)
            {
                LogMessage($"ERROR: Cannot load from database, check logs for more information. Performing shutdown...", LogLevel.Error, true);
                Shutdown();
            }
            if (RoomManager.Instance.Count > 0)
            {
                LogMessage($"INFO: Spawning default NPCs and Items", LogLevel.Info, true);
                var rooms = RoomManager.Instance.GetRoom().Where(x => x.StartingNPCs.Count > 0).ToList();
                if (rooms.Count > 0)
                {
                    foreach (var room in rooms)
                    {
                        foreach(var n in room.StartingNPCs)
                        {
                            int id = n.Key;
                            int amount = n.Value;
                            for (int i = 0; i < amount; i++)
                            {
                                LogMessage($"INFO: Spawning NPC {id} in Room {room.ID}", LogLevel.Info, true);
                                NPCManager.Instance.AddNewNPCInstance(id, room.ID);
                            }
                        }
                    }
                }
                rooms = RoomManager.Instance.GetRoom().Where(x => x.StartingItems.Count > 0).ToList();
                if (rooms.Count > 0)
                {
                    foreach(var room in rooms)
                    {
                        foreach(var i in room.StartingItems)
                        {
                            for (int c = 0; c < i.Value; c++)
                            {
                                if (ItemManager.Instance.ItemExists(i.Key))
                                {
                                    LogMessage($"INFO: Spawning Item {i.Key} in Room {room.ID}", LogLevel.Info, true);
                                    dynamic spawmItem = null;
                                    var baseItem = ItemManager.Instance.GetItem(i.Key);
                                    switch(baseItem.ItemType)
                                    {
                                        case ItemType.Misc:
                                            spawmItem = Helpers.Clone<InventoryItem>(baseItem);
                                            break;

                                        case ItemType.Weapon:
                                            spawmItem = Helpers.Clone<Weapon>(baseItem);
                                            break;

                                        case ItemType.Consumable:
                                            spawmItem = Helpers.Clone<Consumable>(baseItem);
                                            break;

                                        case ItemType.Armour:
                                            spawmItem = Helpers.Clone<Armour>(baseItem);
                                            break;

                                        case ItemType.Ring:
                                            spawmItem = Helpers.Clone<Ring>(baseItem);
                                            break;

                                        case ItemType.Scroll:
                                            spawmItem = Helpers.Clone<Scroll>(baseItem);
                                            break;
                                    }
                                    RoomManager.Instance.AddItemToRoomInventory(room.ID, spawmItem);
                                }
                                else
                                {
                                    LogMessage($"ERROR: Cannot Spawn Item {i.Key} in Room {room.ID}, no such Item in Item Manager", LogLevel.Error, true);
                                }
                            }
                        }
                    }
                }
            }
            LogMessage($"INFO: Setting default inventories for Shops", LogLevel.Info, true);
            foreach(var s in ShopManager.Instance.GetShop())
            {
                s.RestockShop();
            }
            LogMessage("INFO: Starting timers and entering main game loop", LogLevel.Info, true);
            zoneTickTimer.Elapsed += ZoneTick;
            npcTickTimer.Elapsed += NPCTick;
            combatTickTimer.Elapsed += CombatTick;
            autoSaveTimer.Elapsed += AutoSaveTick;
            buffTickTimer.Elapsed += BuffTick;
            backupTickTimer.Elapsed += BackupTick;
            cleanupTimer.Elapsed += CleanUpTick;
            zoneTickTimer.Start();
            npcTickTimer.Start();
            combatTickTimer.Start();
            autoSaveTimer.Start();
            buffTickTimer.Start();
            backupTickTimer.Start();
            cleanupTimer.Start();

            await tcs.Task;
            LogMessage($"INFO: Shutting down...", LogLevel.Info, true);
        }

        public void Shutdown()
        {
            LogMessage($"INFO: Shutdown in progress, saving all connected players...", LogLevel.Info, true);
            SaveAllPlayers(true, out _);
            tcs.SetResult(true);
        }

        public static void ImmShutdown(Session session, bool force)
        {
            LogMessage($"INFO: Game shutdown has been initiated by {session.Player.Name}", LogLevel.Info, true);
            SaveAllPlayers(force, out bool saveErr);
            if (saveErr)
            {
                LogMessage($"WARN: Failed to save all connected players and FORCE was not specified, aborting shutdown", LogLevel.Warning, true);
                return;
            }
            LogMessage($"INFO: Processed save of all connected players", LogLevel.Info, true);
            tcs.SetResult(true);
        }

        public static void BackupNow()
        {
            Task.Run(() =>
            {
                var backupLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
                var worldLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world");
                var backupTime = DateTime.UtcNow;
                try
                {
                    if (!Directory.Exists(backupLocation))
                    {
                        Directory.CreateDirectory(backupLocation);
                    }
                    File.Copy($"{worldLocation}\\players.db", $"{backupLocation}\\players-{backupTime:yyyy-MM-dd-HH-mm-ss}.db");
                    File.Copy($"{worldLocation}\\world.db", $"{backupLocation}\\world-{backupTime:yyyy-MM-dd-HH-mm-ss}.db");
                    LogMessage($"BACKUP: World and Player database backup complete", LogLevel.Info, true);
                    lastBackupTime = backupTime;
                    backupCompleted = true;
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR: Error in Game.BackupTick() while backing up databases: {ex.Message}", LogLevel.Error, true);
                    return;
                }
                try
                {
                    var files = Directory.GetFiles(backupLocation);
                    var cutOff = (backupTick * backupsRetained) * -1;
                    LogMessage($"BACKUP: Pruning old backups, retention cutoff: {backupTime.AddSeconds(cutOff):yyyy-MM-dd HH-mm-ss}", LogLevel.Info, true);
                    foreach (var file in files)
                    {
                        try
                        {
                            var fi = new FileInfo(file);
                            if (fi.CreationTimeUtc <= backupTime.AddSeconds(cutOff))
                            {
                                File.Delete(file);
                                LogMessage($"BACKUP: Deleted backup file: {file}", LogLevel.Info, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"ERROR: Error removing backup file '{file}': {ex.Message}", LogLevel.Error, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR: Error in Game.BackupTick() while processing old backups: {ex.Message}", LogLevel.Error, true);
                }
            });
        }

        private void CombatTick(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                CombatManager.ProcessCombat();
            });
        }

        private void NPCTick(object sender, ElapsedEventArgs e)
        {
            tickCount++;
            var upTime = DateTime.UtcNow - StartTime;
            LogMessage($"INFO: Tick count: {tickCount:N0}; Uptime: {upTime.Days:0} day(s), {upTime.Hours:00}:{upTime.Minutes:00}:{upTime.Seconds:00}", LogLevel.Info, true);
            Task.Run(() =>
            {
                NPCManager.Instance.TickAllNPCs(tickCount);
            });
        }

        private void ZoneTick(object sender, ElapsedEventArgs e)
        {
            ZoneManager.Instance.PulseAllZones();
        }

        private void CleanUpTick(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                int clearedConnections = 0;
                LogMessage($"INFO: Clearing stale connections", LogLevel.Info, true);
                while (SessionManager.Instance.DisconnectedSessions.Count > 0)
                {
                    var s = SessionManager.Instance.DisconnectedSessions.FirstOrDefault();
                    if (s != null)
                    {
                        SessionManager.Instance.Close(s);
                        clearedConnections++;
                    }
                }
                LogMessage($"INFO: Cleared {clearedConnections} stale sessions", LogLevel.Info, true);
            });
        }

        private static void SaveAllPlayers(bool force, out bool saveErr)
        {
            saveErr = false;
            var connectedPlayers = SessionManager.Instance.ActivePlayers;
            if (connectedPlayers != null && connectedPlayers.Count > 0)
            {
                LogMessage($"INFO: Starting Autosave of {connectedPlayers.Count} connected players", LogLevel.Info, true);
                foreach (var p in connectedPlayers)
                {
                    var result = DatabaseManager.SavePlayer(p, false);
                    if (result)
                    {
                        LogMessage($"AUTOSAVE: Successfully saved Player {p.Player.Name}", LogLevel.Info, true);
                    }
                    else
                    {
                        saveErr = true;
                        LogMessage($"AUTOSAVE: Failed to save Player {p.Player.Name}", LogLevel.Error, true);
                    }
                }
            }
            else
            {
                LogMessage($"INFO: No connected players to save", LogLevel.Info, true);
            }
            if (force)
            {
                saveErr = false;
            }
        }

        private void AutoSaveTick(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                SaveAllPlayers(true, out _);
                var idlePlayers = SessionManager.Instance.IdleSessions;
                if (idlePlayers != null && idlePlayers.Count > 0)
                {
                    int disconCount = 0;
                    LogMessage($"INFO: Disconnecting idle players, {idlePlayers.Count} players to process", LogLevel.Info, true);
                    foreach (var p in idlePlayers)
                    {
                        bool okToDiscon = okToDiscon = p.Player.Level < Constants.ImmLevel || DisconnectIdleImms;
                        if (!okToDiscon)
                        {
                            LogMessage($"INFO: Not disconnecting player {p.Player.Name}", LogLevel.Info, true);
                            continue;
                        }
                        var idleTime = Convert.ToInt32((DateTime.UtcNow - p.LastInputTime).TotalSeconds);
                        LogMessage($"INFO: Player {p.Player.Name} has been idle for {idleTime:N0} seconds and will be disconnected", LogLevel.Info, true);
                        p.Send($"You have been idle for {idleTime:N0} seconds and will be disconnected{Constants.NewLine}");
                        SessionManager.Instance.Close(p);
                        disconCount++;
                    }
                    LogMessage($"INFO: {disconCount} idle players have been disconnected", LogLevel.Info, true);
                }
            });
        }

        private void BuffTick(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                LogMessage($"TICK: Pulsing HP/MP/SP regen on all Actors", LogLevel.Info, true);
                foreach (var n in NPCManager.Instance.AllNPCInstances.ToList())
                {
                    if (n.InCombat && !n.HasBuff("Regen") && !n.Flags.HasFlag(NPCFlags.Regeneration))
                    {
                        continue;
                    }
                    var hpRegen = Math.Max(1, Helpers.RollDice<int>(1, 4) + Helpers.CalculateAbilityModifier(n.Constitution));
                    var mpRegen = Math.Max(1, Helpers.RollDice<int>(1, 6) + Helpers.CalculateAbilityModifier(n.Intelligence));
                    if (n.HasBuff("Regen"))
                    {
                        hpRegen += Helpers.RollDice<int>(1, 6);
                        mpRegen += Helpers.RollDice<int>(1, 8);
                    }
                    LogMessage($"INFO: Restoring {hpRegen} HP and {mpRegen} MP to {n.Name}", LogLevel.Info, true);
                    n.AdjustHP(hpRegen, out _);
                    n.AdjustMP(mpRegen);
                }
                foreach (var p in SessionManager.Instance.ActivePlayers)
                {
                    if (p.Player.InCombat && !p.Player.HasBuff("Regen") || p.Player.Position == ActorPosition.Dead)
                    {
                        continue;
                    }
                    var hpRegen = Math.Max(1, Helpers.RollDice<int>(1, 4) + Helpers.CalculateAbilityModifier(p.Player.Constitution));
                    var mpRegen = Helpers.RollDice<int>(1, 8);
                    var spRegen = Math.Max(1, Helpers.RollDice<int>(1, 8) + Helpers.CalculateAbilityModifier(p.Player.Constitution));
                    if (p.Player.Class == ActorClass.Cleric)
                    {
                        mpRegen = Math.Max(1, mpRegen + Helpers.CalculateAbilityModifier(p.Player.Wisdom));
                    }
                    else
                    {
                        mpRegen = Math.Max(1, mpRegen + Helpers.CalculateAbilityModifier(p.Player.Intelligence));
                    }
                    if (p.Player.HasBuff("Regen"))
                    {
                        hpRegen += Helpers.RollDice<int>(1, 6);
                        mpRegen += Helpers.RollDice<int>(1, 8);
                        spRegen += Helpers.RollDice<int>(1, 6);
                    }
                    switch (p.Player.Position)
                    {
                        case ActorPosition.Resting:
                            hpRegen += Helpers.RollDice<int>(1, 3);
                            mpRegen += Helpers.RollDice<int>(1, 3);
                            spRegen += Helpers.RollDice<int>(1, 3);
                            break;

                        case ActorPosition.Sleeping:
                            hpRegen += Helpers.RollDice<int>(1, 6);
                            mpRegen += Helpers.RollDice<int>(1, 6);
                            spRegen += Helpers.RollDice<int>(1, 6);
                            break;
                    }
                    LogMessage($"INFO: Restoring {hpRegen} HP, {mpRegen} MP and {spRegen} SP to {p.Player.Name}", LogLevel.Info, true);
                    p.Player.AdjustHP(hpRegen, out _);
                    p.Player.AdjustMP(mpRegen);
                    p.Player.AdjustSP(spRegen);
                }
                LogMessage($"TICK: Pulsing buffs on all Actors", LogLevel.Info, true);
                foreach (var n in NPCManager.Instance.AllNPCInstances.Where(x => x.Buffs.Count > 0))
                {
                    foreach (var b in n.Buffs)
                    {
                        n.RemoveBuff(b.Key, false);
                        switch (b.Key)
                        {
                            case "Sprit Drain":
                                var mpLoss = Helpers.RollDice<int>(1, 4);
                                n.AdjustMP(mpLoss * -1);
                                break;

                            case "Spirit Fire":
                                var mpGain = Helpers.RollDice<int>(1, 4);
                                n.AdjustMP(mpGain);
                                break;

                            case "Poison":
                                var hpLoss = Helpers.RollDice<int>(1, 6);
                                n.AdjustHP(hpLoss * -1, out bool isKilled);
                                if (isKilled)
                                {
                                    n.Kill(null, false);
                                }
                                break;

                            case "Restoration":
                                n.Restore();
                                break;

                            case "Esuna":
                                n.Esuna();
                                break;
                        }
                    }
                }
                foreach (var p in SessionManager.Instance.ActivePlayers.Where(x => x.Player.Buffs.Count > 0))
                {
                    foreach (var b in p.Player.Buffs)
                    {
                        p.Player.RemoveBuff(b.Key, false);
                        switch (b.Key)
                        {
                            case "Spirit Drain":
                                var mpLoss = Helpers.RollDice<int>(1, 4);
                                p.Player.AdjustMP(mpLoss * -1);
                                break;

                            case "Spirit Fire":
                                var mpGain = Helpers.RollDice<int>(1, 4);
                                p.Player.AdjustMP(mpGain);
                                break;

                            case "Energy Drain":
                                var spLoss = Helpers.RollDice<int>(1, 3);
                                p.Player.AdjustSP(spLoss * -1);
                                break;

                            case "Energy Fire":
                                var spGain = Helpers.RollDice<int>(1, 3);
                                p.Player.AdjustSP(spGain);
                                break;

                            case "Poison":
                                var hpLoss = Helpers.RollDice<int>(1, 6);
                                p.Player.AdjustHP(hpLoss * -1, out bool isKilled);
                                if (isKilled)
                                {
                                    p.Player.Kill(null, false);
                                }
                                break;

                            case "Restoration":
                                p.Player.Restore();
                                break;

                            case "Esuna":
                                p.Player.Esuna();
                                break;
                        }
                    }
                }
            });
        }

        private void BackupTick(object sender, ElapsedEventArgs e)
        {
            BackupNow();
        }

        private bool LoadDatabase()
        {
            ZoneManager.Instance.LoadAllZones(out bool zErr);
            if (zErr)
            {
                return false;
            }
            if (ZoneManager.Instance.Count == 0)
            {
                if (AddDefaultZone())
                {
                    LoadDatabase(); // restart the load process if we've added a zone
                }
                else
                {
                    LogMessage($"ERROR: Failed to create Default Zone, aborting laod process", LogLevel.Error, true);
                    return false;
                }
            }
            LogMessage($"INFO: Loading Database, {ZoneManager.Instance.Count} Zones loaded", LogLevel.Info, true);
            RoomManager.Instance.LoadAllRooms(out bool roomErr);
            if (roomErr)
            {
                return false;
            }
            if (RoomManager.Instance.Count == 0)
            {
                if (AddDefaultRoom())
                {
                    LoadDatabase();
                }
                else
                {
                    LogMessage($"ERROR: Failed to create Default Room, aborting load process", LogLevel.Error, true);
                    return false;
                }
            }
            LogMessage($"INFO: Loading Database, {RoomManager.Instance.Count} Rooms loaded", LogLevel.Info, true);
            ItemManager.Instance.LoadAllItems(out bool itemErr);
            if (itemErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {ItemManager.Instance.Count} Items loaded", LogLevel.Info, true);
            ShopManager.Instance.LoadAllShops(out bool shopErr);
            if (shopErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {ShopManager.Instance.Count} Shops loaded", LogLevel.Info, true);
            MobProgManager.Instance.LoadAllMobProgs(out bool mobErr);
            if (mobErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {MobProgManager.Instance.Count} MobProgs loaded", LogLevel.Info, true);
            NPCManager.Instance.LoadAllNPCs(out bool npcErr);
            if (npcErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {NPCManager.Instance.TemplateCount} NPCs loaded", LogLevel.Info, true);
            EmoteManager.Instance.LoadAllEmotes(out bool emoteErr);
            if (emoteErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {EmoteManager.Instance.Count} Emotes loaded", LogLevel.Info, true);
            NodeManager.Instance.LoadAllNodes(out bool nodeErr);
            if (nodeErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {NodeManager.Instance.Count} Resource Nodes loaded", LogLevel.Info, true);
            RecipeManager.Instance.LoadAllRecipes(out bool recipeErr);
            if (recipeErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {RecipeManager.Instance.Count} Crafting Recipes loaded", LogLevel.Info, true);
            QuestManager.Instance.LoadAllQuests(out bool questErr);
            if (questErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {QuestManager.Instance.Count} Quests loaded", LogLevel.Info, true);
            SpellManager.Instance.LoadAllSpells(out bool spellErr);
            if (spellErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {SpellManager.Instance.Count} Spells loaded", LogLevel.Info, true);
            HelpManager.Instance.LoadAllArticles(out bool helpErr);
            if (helpErr)
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {HelpManager.Instance.Count} Help articles loaded", LogLevel.Info, true);
            return true;
        }

        private bool AddDefaultRoom()
        {
            var limbo = new Room
            {
                RoomName = "Limbo",
                ID = 0,
                ZoneID = 0,
                ShortDescription = "the world between worlds",
                LongDescription = "White clouds swirl around, carrying the faint echo of mortal voices. Through a ghostly mist, the city of Etrea is visible below.",
                Flags = RoomFlags.Safe
            };
            return DatabaseManager.SaveRoomToWorldDatabase(limbo, true);
        }

        private bool AddDefaultZone()
        {
            var highHeavens = new Zone
            {
                ZoneID = 0,
                ZoneName = "The High Heavens",
                MinRoom = 0,
                MaxRoom = 99,
                LockHolder = Guid.Empty,
                OLCLocked = false
            };
            return DatabaseManager.SaveZoneToWorldDatabase(highHeavens, true);
        }
    }
}
