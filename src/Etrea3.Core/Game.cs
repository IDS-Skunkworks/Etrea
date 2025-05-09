﻿using System;
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
        private static TimeOfDay currentTOD;
        private static TimeOfDay previousTOD;

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
        public static TimeOfDay CurrentTOD => currentTOD;
        public static TimeOfDay PreviousTOD => previousTOD;

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
            currentTOD = Helpers.GetTimeOfDay();
            previousTOD = TimeOfDay.None;

            zoneTickTimer.Interval = zoneTick * 1000;
            npcTickTimer.Interval = npcTick * 1000;
            combatTickTimer.Interval = combatTick * 1000;
            autoSaveTimer.Interval = autoSaveTick * 1000;
            buffTickTimer.Interval = buffTick * 1000;
            backupTickTimer.Interval = backupTick * 1000;
            cleanupTimer.Interval = 60 * 1000;
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

        public static void LogMessage(string message, LogLevel level)
        {
            Logger.LogMessage(message, level);
            PlayerFlags pFlag = PlayerFlags.None;
            switch(level)
            {
                case LogLevel.Error:
                    pFlag = PlayerFlags.MUDLogError;
                    break;

                case LogLevel.Warning:
                    pFlag = PlayerFlags.MUDLogWarn;
                    break;

                case LogLevel.Connection:
                    pFlag = PlayerFlags.MUDLogConnection;
                    break;

                case LogLevel.Debug:
                    pFlag = PlayerFlags.MUDLogDebug;
                    break;

                case LogLevel.Info:
                    pFlag = PlayerFlags.MUDLogInfo;
                    break;

                case LogLevel.Combat:
                    pFlag = PlayerFlags.MUDLogCombat;
                    break;

                case LogLevel.Shop:
                    pFlag = PlayerFlags.MUDLogShops;
                    break;

                case LogLevel.OLC:
                    pFlag = PlayerFlags.MUDLogOLC;
                    break;

                case LogLevel.God:
                    pFlag = PlayerFlags.MUDLogGod;
                    break;
            }
            if (pFlag == PlayerFlags.None)
            {
                return;
            }
            var connectedImms = SessionManager.Instance.Immortals.Where(x => x.Player.Flags.HasFlag(pFlag)).ToList();
            if (connectedImms != null && connectedImms.Count > 0)
            {
                foreach(var imm in connectedImms)
                {
                    imm.Send($"%BMT%{message}%PT%{Constants.NewLine}");
                }
            }
        }

        public async Task Run()
        {
            tcs = new TaskCompletionSource<bool>();
            if (DatabaseManager.ClearLogTable(out int rowCount))
            {
                LogMessage($"INFO: Log Table cleared, {rowCount} items removed.", LogLevel.Info);
            }
            else
            {
                LogMessage($"ERROR: Failed to clear Log Table", LogLevel.Error);
            }
            startTime = DateTime.UtcNow;
            if (!LoadDatabase())
            {
                LogMessage($"ERROR: Cannot load from database, check logs for more information. Performing shutdown...", LogLevel.Error);
                Shutdown();
            }
            if (RoomManager.Instance.Count > 0)
            {
                LogMessage($"INFO: Spawning default NPCs and Items", LogLevel.Info);
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
                                LogMessage($"INFO: Spawning NPC {id} in Room {room.ID}", LogLevel.Info);
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
                                LogMessage($"INFO: Spawning Item {i.Key} in Room {room.ID}", LogLevel.Info);
                                RoomManager.Instance.AddItemToRoomInventory(room.ID, i.Key);
                                //if (ItemManager.Instance.ItemExists(i.Key))
                                //{
                                    
                                //    dynamic spawmItem = null;
                                //    var baseItem = ItemManager.Instance.GetItem(i.Key);
                                //    switch(baseItem.ItemType)
                                //    {
                                //        case ItemType.Misc:
                                //            spawmItem = Helpers.Clone<InventoryItem>(baseItem);
                                //            break;

                                //        case ItemType.Weapon:
                                //            spawmItem = Helpers.Clone<Weapon>(baseItem);
                                //            break;

                                //        case ItemType.Consumable:
                                //            spawmItem = Helpers.Clone<Consumable>(baseItem);
                                //            break;

                                //        case ItemType.Armour:
                                //            spawmItem = Helpers.Clone<Armour>(baseItem);
                                //            break;

                                //        case ItemType.Ring:
                                //            spawmItem = Helpers.Clone<Ring>(baseItem);
                                //            break;

                                //        case ItemType.Scroll:
                                //            spawmItem = Helpers.Clone<Scroll>(baseItem);
                                //            break;
                                //    }
                                //    RoomManager.Instance.AddItemToRoomInventory(room.ID, spawmItem);
                                //}
                                //else
                                //{
                                //    LogMessage($"ERROR: Cannot Spawn Item {i.Key} in Room {room.ID}, no such Item in Item Manager", LogLevel.Error);
                                //}
                            }
                        }
                    }
                }
            }
            LogMessage($"INFO: Setting default inventories for Shops", LogLevel.Info);
            foreach(var s in ShopManager.Instance.GetShop())
            {
                s.RestockShop();
            }
            if (bool.TryParse(ConfigurationManager.AppSettings["TickZonesOnStartup"], out bool startZoneTick) && startZoneTick)
            {
                LogMessage("INFO: Performing startup Zone tick...", LogLevel.Info);
                ZoneManager.Instance.PulseAllZones();
            }
            LogMessage("INFO: Starting timers and entering main game loop", LogLevel.Info);
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
            LogMessage($"INFO: Shutting down...", LogLevel.Info);
        }

        public void Shutdown()
        {
            LogMessage($"INFO: Shutdown in progress, saving all connected players...", LogLevel.Info);
            SaveAllPlayers(true, out _);
            tcs.SetResult(true);
        }

        public static void ImmShutdown(Session session, bool force)
        {
            LogMessage($"INFO: Game shutdown has been initiated by {session.Player.Name}", LogLevel.Info);
            SaveAllPlayers(force, out bool saveErr);
            if (saveErr)
            {
                LogMessage($"WARN: Failed to save all connected players and FORCE was not specified, aborting shutdown", LogLevel.Warning);
                return;
            }
            LogMessage($"INFO: Processed save of all connected players", LogLevel.Info);
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
                    LogMessage($"BACKUP: World and Player database backup complete", LogLevel.Info);
                    lastBackupTime = backupTime;
                    backupCompleted = true;
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR: Error in Game.BackupTick() while backing up databases: {ex.Message}", LogLevel.Error);
                    return;
                }
                try
                {
                    var files = Directory.GetFiles(backupLocation);
                    var cutOff = (backupTick * backupsRetained) * -1;
                    LogMessage($"BACKUP: Pruning old backups, retention cutoff: {backupTime.AddSeconds(cutOff):yyyy-MM-dd HH-mm-ss}", LogLevel.Info);
                    foreach (var file in files)
                    {
                        try
                        {
                            var fi = new FileInfo(file);
                            if (fi.CreationTimeUtc <= backupTime.AddSeconds(cutOff))
                            {
                                File.Delete(file);
                                LogMessage($"BACKUP: Deleted backup file: {file}", LogLevel.Info);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"ERROR: Error removing backup file '{file}': {ex.Message}", LogLevel.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR: Error in Game.BackupTick() while processing old backups: {ex.Message}", LogLevel.Error);
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
            LogMessage($"INFO: Tick count: {tickCount:N0}; Uptime: {upTime.Days:0} day(s), {upTime.Hours:00}:{upTime.Minutes:00}:{upTime.Seconds:00}", LogLevel.Info);
            Task.Run(() =>
            {
                NPCManager.Instance.TickAllNPCs(tickCount);
            });
        }

        private void ZoneTick(object sender, ElapsedEventArgs e)
        {
            if (currentTOD != Helpers.GetTimeOfDay())
            {
                previousTOD = currentTOD;
                currentTOD = Helpers.GetTimeOfDay();
                RoomManager.Instance.RunRoomProgs(tickCount, RoomProgTrigger.TimeOfDayChange);
            }
            ZoneManager.Instance.PulseAllZones();
            RoomManager.Instance.RunRoomProgs(tickCount, RoomProgTrigger.MudTick);
        }

        private void CleanUpTick(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                int clearedConnections = 0;
                LogMessage($"INFO: Clearing dead peers and idle connections...", LogLevel.Info);
                var deadPeers = SessionManager.Instance.DeadPeers;
                if (deadPeers.Count > 0)
                {
                    foreach(var deadPeer in deadPeers)
                    {
                        var conID = deadPeer.Key.ID;
                        LogMessage($"CONNECTION: Disconnecting {conID}: {deadPeer.Value}", LogLevel.Connection);
                        if (SessionManager.Instance.Close(deadPeer.Key))
                        {
                            clearedConnections++;
                            LogMessage($"CONNECTION: Connection {conID} closed successfully", LogLevel.Connection);
                        }
                        else
                        {
                            LogMessage($"CONNECTION: Connection {conID} could not be closed, please see logs for errors or warnings", LogLevel.Connection);
                        }    
                    }
                    LogMessage($"INFO: Dead peer and idle connection cleanup completed: {clearedConnections} connections dropped", LogLevel.Info);
                }
                else
                {
                    LogMessage($"INFO: No dead peers or idle connections to process", LogLevel.Info);
                }
            });
        }

        private static void SaveAllPlayers(bool force, out bool saveErr)
        {
            saveErr = false;
            var connectedPlayers = SessionManager.Instance.ActivePlayers;
            if (connectedPlayers != null && connectedPlayers.Count > 0)
            {
                LogMessage($"INFO: Starting Autosave of {connectedPlayers.Count} connected players", LogLevel.Info);
                foreach (var p in connectedPlayers)
                {
                    var result = DatabaseManager.SavePlayer(p, false);
                    if (result)
                    {
                        LogMessage($"AUTOSAVE: Successfully saved Player {p.Player.Name}", LogLevel.Info);
                    }
                    else
                    {
                        saveErr = true;
                        LogMessage($"AUTOSAVE: Failed to save Player {p.Player.Name}", LogLevel.Error);
                    }
                }
            }
            else
            {
                LogMessage($"INFO: No connected players to save", LogLevel.Info);
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
            });
        }

        private void BuffTick(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                LogMessage($"TICK: Pulsing HP/MP/SP regen on all Actors", LogLevel.Info);
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
                    p.Player.AdjustHP(hpRegen, out _);
                    p.Player.AdjustMP(mpRegen);
                    p.Player.AdjustSP(spRegen);
                }
                LogMessage($"TICK: Pulsing buffs on all Actors", LogLevel.Info);
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
            // Load the list of blocked IP addresses - connections from these to the MUD server or the API will be automatically discarded, existing connections will be dropped
            if (BlockManager.Instance.LoadBlockList())
            {
                LogMessage($"INFO: Loading Database, {BlockManager.Instance.Count} blocked IP addresses loaded", LogLevel.Info);
            }
            else
            {
                LogMessage($"WARN: Failed to load the Block List, please check the log for relevant messages", LogLevel.Warning);
            }
            // Load the World Zones
            if (!ZoneManager.Instance.LoadAllZones())
            {
                return false;
            }
            if (ZoneManager.Instance.Count == 0)
            {
                if (AddDefaultZone())
                {
                    LoadDatabase(); // restart the load process if we've added default zones
                }
                else
                {
                    LogMessage($"ERROR: Failed to create Default Zone, aborting laod process", LogLevel.Error);
                    return false;
                }
            }
            LogMessage($"INFO: Loading Database, {ZoneManager.Instance.Count} Zones loaded", LogLevel.Info);
            // Load Rooms
            if (!RoomManager.Instance.LoadAllRooms())
            {
                return false;
            }
            if (RoomManager.Instance.Count == 0)
            {
                if (AddDefaultRoom())
                {
                    LoadDatabase(); // restart the load process if we've added default rooms
                }
                else
                {
                    LogMessage($"ERROR: Failed to create Default Room, aborting load process", LogLevel.Error);
                    return false;
                }
            }
            LogMessage($"INFO: Loading Database, {RoomManager.Instance.Count} Rooms loaded", LogLevel.Info);
            // Load Items
            if (!ItemManager.Instance.LoadAllItems())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {ItemManager.Instance.Count} Items loaded", LogLevel.Info);
            // Load Shops
            if (!ShopManager.Instance.LoadAllShops())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {ShopManager.Instance.Count} Shops loaded", LogLevel.Info);
            // Load MobProgs
            if (!ScriptObjectManager.Instance.LoadAllScripts())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {ScriptObjectManager.Instance.Count} Scripts loaded", LogLevel.Info);
            // Load NPCs
            if (!NPCManager.Instance.LoadAllNPCs())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {NPCManager.Instance.TemplateCount} NPCs loaded", LogLevel.Info);
            // Load Emotes
            if (!EmoteManager.Instance.LoadAllEmotes())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {EmoteManager.Instance.Count} Emotes loaded", LogLevel.Info);
            // Load Resource Nodes
            if (!NodeManager.Instance.LoadAllNodes())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {NodeManager.Instance.Count} Resource Nodes loaded", LogLevel.Info);
            // Load Crafting Recipes
            if (!RecipeManager.Instance.LoadAllRecipes())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {RecipeManager.Instance.Count} Crafting Recipes loaded", LogLevel.Info);
            // Load Quests
            if (!QuestManager.Instance.LoadAllQuests())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {QuestManager.Instance.Count} Quests loaded", LogLevel.Info);
            // Load Spells
            if (!SpellManager.Instance.LoadAllSpells())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {SpellManager.Instance.Count} Spells loaded", LogLevel.Info);
            // Load Help Articles
            if (!HelpManager.Instance.LoadAllArticles())
            {
                return false;
            }
            LogMessage($"INFO: Loading Database, {HelpManager.Instance.Count} Help articles loaded", LogLevel.Info);
            LogMessage($"INFO: Player Database check... {DatabaseManager.GetPlayerCount()} Players in database", LogLevel.Info);
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
                MorningDescription = "White clouds swirl around, carrying the faint echo of mortal voices. Through a ghostly mist, the city of Etrea is visible below.",
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
