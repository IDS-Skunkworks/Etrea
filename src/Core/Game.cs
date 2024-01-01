using Kingdoms_of_Etrea.Entities;
using Kingdoms_of_Etrea.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kingdoms_of_Etrea.Core
{
    internal sealed class Game
    {
        private static ILoggingProvider _logProvider = new LoggingProvider();
        private bool _running;
        private static DateTime _gameStart;
        private static DateTime _lastBackup;
        private static bool _backupComplete = false;
        private static uint _backupsRetained;
        private static uint _zonePulse;
        private static uint _npcPulse;
        private static uint _combatPulse;
        private static uint _savePulse;
        private static uint _buffPulse;
        private static uint _backupPulse;
        private static uint _lastTickCount;

        public Game(uint zonePulse, uint npcPulse, uint combatPulse, uint savePulse, uint buffPulse, uint backupPulse, uint backupsRetained)
        {
            _zonePulse = zonePulse;
            _npcPulse = npcPulse;
            _combatPulse = combatPulse;
            _savePulse = savePulse;
            _lastTickCount = 0;
            _buffPulse = buffPulse;
            _backupPulse = backupPulse;
            _backupsRetained = backupsRetained;
        }

        internal static DateTime GetStartTime()
        {
            return _gameStart;
        }

        internal static bool GetBackupInfo(out DateTime backupTime, out uint backupTick)
        {
            backupTime = DateTime.UtcNow;
            backupTick = _backupPulse;
            if (_backupComplete)
            {
                backupTime = _lastBackup;
                return true;
            }
            return false;
        }

        internal static void LogMessage(string msg, LogLevel level, bool toScreen)
        {
            _logProvider.LogMessage(msg, level, toScreen);
        }

        public void Run()
        {
            bool zoneLoadErr = false;
            bool roomLoadErr = false;
            bool npcLoadError = false;
            bool itemLoadError = false;
            bool shopLoadError = false;
            bool emoteLoadError = false;
            bool nodeLoadError = false;
            bool recipeLoadError = false;
            bool questLoadError = false;

            _running = true;
            _gameStart = DateTime.UtcNow;
            ShopManager.Instance.LoadAllShops(out shopLoadError);
            RoomManager.Instance.LoadAllRooms(out roomLoadErr);
            ItemManager.Instance.LoadAllItems(out itemLoadError);
            ZoneManager.Instance.LoadAllZones(out zoneLoadErr);
            NPCManager.Instance.LoadAllNPCs(out npcLoadError);
            EmoteManager.Instance.LoadAllEmotes(out emoteLoadError);
            NodeManager.Instance.LoadAllResourceNodes(out nodeLoadError);
            RecipeManager.Instance.LoadAllCraftingRecipes(out recipeLoadError);
            QuestManager.Instance.LoadAllQuests(out questLoadError);

            if(RoomManager.Instance.GetRoomCount() == 0)
            {
                // No rooms, so add the default RID0 and 100
                Room limbo = new Room
                {
                    RoomName = "Limbo",
                    RoomID = 0,
                    ZoneID = 0,
                    ShortDescription = "Limbo, the world between worlds",
                    LongDescription = "White clouds swirl around carrying the faint echo of mortal voices. Through the mists, the city of Etrea is visible below.",
                    RoomExits = new List<Room.Exit>
                    { new Room.Exit
                    {
                        ExitDirection = "Down",
                        DestinationRoomID = 100
                    }
                    },
                    Flags = RoomFlags.Safe
                };
                Room townSquare = new Room
                {
                    RoomName = "Etrea City Square",
                    RoomID = 100,
                    ZoneID = 1,
                    ShortDescription = "The central square of the town of Etrea",
                    LongDescription = "A large and magnificently carved fountain dominates the central plaza and town square of Etrea. Rows of shops and buildings circle the square. To the north, Etrea Keep dominates the skyline. To the south are the main city gates, while the city streets spiral off to the west and east.",
                    Flags = RoomFlags.Safe,
                    RoomExits = new List<Room.Exit>()
                };
                if(!DatabaseManager.AddDefaultRoom(limbo) || !DatabaseManager.AddDefaultRoom(townSquare))
                {
                    roomLoadErr = true;
                }
                else
                {
                    RoomManager.Instance.LoadAllRooms(out roomLoadErr);
                }
            }

            if(!roomLoadErr && !npcLoadError)
            {
                var rooms = RoomManager.Instance.GetAllRooms();
                if(rooms.Count > 0)
                {
                    foreach (var r in RoomManager.Instance.GetAllRooms())
                    {
                        if (r.Value.SpawnNPCsAtStart != null && r.Value.SpawnNPCsAtStart.Count > 0)
                        {
                            foreach(var rnpc in r.Value.SpawnNPCsAtStart)
                            {
                                var numToSpawn = rnpc.Value;
                                for(int i = 0; i < numToSpawn; i++)
                                {
                                    NPCManager.Instance.AddNPCToWorld(rnpc.Key, r.Value.RoomID);
                                }
                            }
                        }
                    }
                }
            }

            if (zoneLoadErr || roomLoadErr || itemLoadError || shopLoadError || npcLoadError || emoteLoadError || nodeLoadError || recipeLoadError || questLoadError)
            {
                _logProvider.LogMessage("ERROR: Error loading data from World Database, game cannot start", LogLevel.Error, true);
                PerformShutdown();
            }

            // we only get here if we can read all required info from the database
            _logProvider.LogMessage($"INFO: Database loading: {ZoneManager.Instance.GetZoneCount()} Zone(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {RoomManager.Instance.GetRoomCount()} Room(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {DatabaseManager.GetPlayerCount()} Player(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {ItemManager.Instance.GetItemCount()} Item(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {ShopManager.Instance.GetShopCount()} Shop(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {NPCManager.Instance.GetNPCCount()} NPC(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {EmoteManager.Instance.GetEmoteCount()} Emote(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {NodeManager.Instance.GetNodeCount()} Resource Node(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {RecipeManager.Instance.GetRecipeCount()} Crafting Recipe(s)", LogLevel.Info, true);
            _logProvider.LogMessage($"INFO: Database loading: {QuestManager.Instance.GetQuestCount()} Quest(s)", LogLevel.Info, true);
            _logProvider.LogMessage("INFO: Entering main game loop", LogLevel.Info, true);
            while(_running)
            {
                Heartbeat();
            }
            _logProvider.LogMessage("INFO: Starting game shutdown", LogLevel.Info, true);
            PerformShutdown();
        }

        public void Shutdown()
        {
            _running = false;
        }

        private void PerformShutdown()
        {
            _logProvider.LogMessage("INFO: Shutdown complete", LogLevel.Info, true);
            Environment.Exit(0);
        }

        private void Heartbeat()
        {
            var uptimeSeconds = (uint)(DateTime.UtcNow - _gameStart).TotalSeconds;
            var totalUptime = DateTime.UtcNow - _gameStart;
            if(_lastTickCount != uptimeSeconds)
            {
                _lastTickCount = uptimeSeconds;
                if(uptimeSeconds % 300 == 0)
                {
                    _logProvider.LogMessage($"UPTIME: Game world has been up for {totalUptime.Days} days, {totalUptime.Hours} hours, {totalUptime.Minutes} minutes, {totalUptime.Seconds} seconds", LogLevel.Debug, true);
                }

                if (uptimeSeconds % 90 == 0) // 900
                {
                    CleanupDescriptors();
                }
                
                if (uptimeSeconds % _zonePulse == 0)
                {
                    PulseAllZones();
                }
                if (uptimeSeconds % _npcPulse == 0)
                {
                    PulseNPCs();
                }
                if (uptimeSeconds % _combatPulse == 0)
                {
                    PulseCombat();
                }
                if (uptimeSeconds % _savePulse == 0)
                {
                    PulseAutoSave();
                }
                if(uptimeSeconds % _buffPulse == 0)
                {
                    PulseBuffs();
                }
                if(uptimeSeconds % _backupPulse == 0)
                {
                    PulseBackups();
                }
            }
        }

        private void PulseBackups()
        {
            var backupLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
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
                _logProvider.LogMessage($"INFO: World backup completed successfully", LogLevel.Info, true);
                _lastBackup = backupTime;
                _backupComplete = true;
            }
            catch(Exception ex)
            {
                _logProvider.LogMessage($"ERROR: World backup failed: {ex.Message}", LogLevel.Error, true);
                return;
            }
            try
            {
                var files = Directory.GetFiles(backupLocation);
                var cutoff = (_backupPulse * _backupsRetained) * -1; // 3600 * 10 * -1
                _logProvider.LogMessage($"DEBUG: Backup cutoff time: {cutoff} seconds", LogLevel.Debug, true);
                _logProvider.LogMessage($"INFO: Processing old backups, retention cutoff: {backupTime.AddSeconds(cutoff):yyyy-MM-dd HH:mm:ss}", LogLevel.Info, true);
                foreach (var file in files)
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        if(fi.CreationTimeUtc <= backupTime.AddSeconds(cutoff))
                        {
                            File.Delete(file);
                            _logProvider.LogMessage($"INFO: Deleting file '{file}' which is beyond retention", LogLevel.Info, true);
                        }
                    }
                    catch(Exception ex)
                    {
                        _logProvider.LogMessage($"ERROR: Error deleting file '{file}': {ex.Message}", LogLevel.Error, true);
                    }
                }
            }
            catch(Exception ex)
            {
                _logProvider.LogMessage($"ERROR: Error deleting backups: {ex.Message}", LogLevel.Error, true);
            }
        }

        private void PulseBuffs()
        {
            _logProvider.LogMessage("TICK: Player buffs and player/NPC regen", LogLevel.Info, true);
            var connectedPlayers = SessionManager.Instance.GetAllPlayers().Where(x => x.IsConnected).ToList();
            foreach (var player in connectedPlayers)
            {
                if(player.Player.CombatSessionID == Guid.Empty)
                {
                    // only process basic regen on players that are alive and not fighting
                    if(player.Player.Position != ActorPosition.Dead && !RoomManager.Instance.GetRoom(player.Player.CurrentRoom).Flags.HasFlag(RoomFlags.NoHealing))
                    {
                        if(player.Player.Stats.CurrentHP < player.Player.Stats.MaxHP)
                        {
                            var regen = Helpers.RollDice(1, 4);
                            var bonus = ActorStats.CalculateAbilityModifier(player.Player.Stats.Constitution);
                            if(bonus > 0)
                            {
                                regen += Convert.ToUInt32(bonus);
                            }
                            if(player.Player.Position == ActorPosition.Resting)
                            {
                                // extra regen if the player is resting
                                regen += 2;
                            }
                            if(RoomManager.Instance.GetRoom(player.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Healing))
                            {
                                // double regen if the player is in a healing room
                                regen *= 2;
                            }
                            if(player.Player.Stats.CurrentHP + regen > player.Player.Stats.MaxHP)
                            {
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentHP = (int)player.Player.Stats.MaxHP;
                            }
                            else
                            {
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentHP += (int)regen;
                                _logProvider.LogMessage($"INFO: Restoring {player.Player.Name} {regen} HP", LogLevel.Info, true);
                            }
                        }
                        if(player.Player.Stats.CurrentMP < player.Player.Stats.MaxMP)
                        {
                            var regen = Helpers.RollDice(1, 3);
                            var bonus = ActorStats.CalculateAbilityModifier(player.Player.Stats.Intelligence);
                            if (bonus > 0)
                            {
                                regen += Convert.ToUInt32(bonus);
                            }
                            if(player.Player.Position == ActorPosition.Resting)
                            {
                                // extra regen if the player is resting
                                regen += 2;
                            }
                            if(RoomManager.Instance.GetRoom(player.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Healing))
                            {
                                regen *= 2;
                            }
                            if(player.Player.Stats.CurrentMP + regen > player.Player.Stats.MaxMP)
                            {
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentMP = (int)player.Player.Stats.MaxMP;
                            }
                            else
                            {
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentMP += (int)regen;
                                _logProvider.LogMessage($"INFO: Restoring {player.Player.Name} {regen} MP", LogLevel.Info, true);
                            }
                        }
                        if(player.Player.Stats.CurrentSP < player.Player.Stats.MaxSP)
                        {
                            var regen = Helpers.RollDice(1, 3);
                            var bonus = ActorStats.CalculateAbilityModifier(player.Player.Stats.Constitution);
                            if(bonus > 0)
                            {
                                regen += Convert.ToUInt32(bonus);
                            }
                            if(player.Player.Position == ActorPosition.Resting)
                            {
                                regen += 2;
                            }
                            if(RoomManager.Instance.GetRoom(player.Player.CurrentRoom).Flags.HasFlag(RoomFlags.Healing))
                            {
                                regen *= 2;
                            }
                            if(player.Player.Stats.CurrentSP + regen > player.Player.Stats.MaxSP)
                            {
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentSP = player.Player.Stats.CurrentSP;
                            }
                            else
                            {
                                SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentSP += regen;
                                _logProvider.LogMessage($"INFO: Restoring {player.Player.Name} {regen} SP", LogLevel.Info, true);
                            }
                        }
                    }
                }
            }
            var npcs = NPCManager.Instance.GetAllNPCIDS();
            foreach(var npc in npcs)
            {
                if(!CombatManager.Instance.IsNPCInCombat(npc.Key) || npc.Value.BehaviourFlags.HasFlag(NPCFlags.Regen))
                {
                    // only regen NPCs that are not in combat unless they have the regen behaviour flag
                    if(npc.Value.Stats.CurrentHP < npc.Value.Stats.MaxHP)
                    {
                        var regen = Helpers.RollDice(1, 3);
                        var bonus = ActorStats.CalculateAbilityModifier(npc.Value.Stats.Constitution);
                        if(bonus > 0)
                        {
                            regen += Convert.ToUInt32(bonus);
                        }
                        if(RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).Flags.HasFlag(RoomFlags.Healing))
                        {
                            regen *= 2;
                        }
                        if(npc.Value.Stats.CurrentHP + regen > npc.Value.Stats.MaxHP)
                        {
                            npc.Value.Stats.CurrentHP = (int)npc.Value.Stats.MaxHP;
                        }
                        else
                        {
                            npc.Value.Stats.CurrentHP += (int)regen;
                            _logProvider.LogMessage($"INFO: Restoring {npc.Value.Name} {regen} HP", LogLevel.Info, true);
                        }
                    }
                    if(npc.Value.Stats.CurrentMP < npc.Value.Stats.MaxMP)
                    {
                        var regen = Helpers.RollDice(1, 3);
                        var bonus = ActorStats.CalculateAbilityModifier(npc.Value.Stats.Intelligence);
                        if(bonus > 0)
                        {
                            regen += Convert.ToUInt32(bonus);
                        }
                        if(RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).Flags.HasFlag(RoomFlags.Healing))
                        {
                            regen *= 2;
                        }
                        if(npc.Value.Stats.CurrentMP + regen > npc.Value.Stats.MaxMP)
                        {
                            npc.Value.Stats.CurrentMP = (int)npc.Value.Stats.MaxMP;
                        }
                        else
                        {
                            npc.Value.Stats.CurrentMP += (int)regen;
                            _logProvider.LogMessage($"INFO: Restoring {npc.Value.Name} {regen} MP", LogLevel.Info, true);
                        }
                    }
                }
            }
            var playersWithBuff = SessionManager.Instance.GetAllPlayers().Where(x => x.IsConnected && x.Player.Buffs != null && x.Player.Buffs.Count > 0).ToList();
            List<Descriptor> playersToKill = new List<Descriptor>();
            if (playersWithBuff != null && playersWithBuff.Count > 0)
            {
                foreach(var player in playersWithBuff)
                {
                    List<string> buffsToRemove = new List<string>();
                    for(int i = 0; i < player.Player.Buffs.Count; i++)
                    {
                        var buff = player.Player.Buffs.ElementAt(i).Key;
                        if (buff == "Light")
                        {
                            RoomManager.Instance.GetRoom(player.Player.CurrentRoom).HasLightSource = true;
                        }
                        if (player.Player.Buffs.ElementAt(i).Value > -1)
                        {
                            if (player.Player.Buffs.ElementAt(i).Value -1 == 0)
                            {
                                buffsToRemove.Add(buff);
                            }
                            else
                            {
                                player.Player.Buffs[buff]--;
                            }
                        }
                    }
                    if(buffsToRemove.Count > 0)
                    {
                        foreach(var b in buffsToRemove)
                        {
                            player.Player.RemoveBuff(b);
                            _logProvider.LogMessage($"INFO: Removing buff {b} from player {player.Player.Name}", LogLevel.Info, true);
                            player.Send($"The magic of {b} fades away!{Constants.NewLine}");
                        }
                    }
                    buffsToRemove.Clear();
                    if(player.Player.Buffs.ContainsKey("Regen"))
                    {
                        var regenSpell = Spells.GetSpell("Regen");
                        var regenHP = Helpers.RollDice(regenSpell.NumOfDamageDice, regenSpell.SizeOfDamageDice);
                        if(player.Player.Stats.CurrentHP + regenHP >= player.Player.Stats.MaxHP)
                        {
                            player.Player.Stats.CurrentHP = (int)player.Player.Stats.MaxHP;
                            player.Send($"The magic of Regen has restored you to full health!{Constants.NewLine}");
                        }
                        else
                        {
                            player.Player.Stats.CurrentHP += (int)regenHP;
                            if(player.Player.Level >= Constants.ImmLevel || player.Player.ShowDetailedRollInfo)
                            {
                                player.Send($"The magic of Regen restores {regenHP} health to you!{Constants.NewLine}");
                            }
                            else
                            {
                                player.Send($"The magic of Regen restores some health to you!{Constants.NewLine}");
                            }
                        }
                    }
                    if(player.Player.Buffs.ContainsKey("Acid Arrow"))
                    {
                        var acidSpell = Spells.GetSpell("Acid Arrow");
                        var acidDmg = Helpers.RollDice(acidSpell.NumOfDamageDice, acidSpell.SizeOfDamageDice);
                        if(player.Player.Stats.CurrentHP - acidDmg <= 0)
                        {
                            // DOT killed the player
                            player.Send($"The magic of {acidSpell.SpellName} is too much and you finally succumb!{Constants.NewLine}");
                            playersToKill.Add(player);
                        }
                        else
                        {
                            player.Player.Stats.CurrentHP -= (int)acidDmg;
                            if(player.Player.Level >= Constants.ImmLevel || player.Player.ShowDetailedRollInfo)
                            {
                                player.Send($"The magic of {acidSpell.SpellName} deals {acidDmg} damage to you!{Constants.NewLine}");
                            }
                            else
                            {
                                player.Send($"The magic of {acidSpell.SpellName} hurts you!{Constants.NewLine}");
                            }
                        }
                    }
                    if(player.Player.Buffs.ContainsKey("Poison"))
                    {
                        var poisonSpell = Spells.GetSpell("Poison");
                        var pDamage = Helpers.RollDice(poisonSpell.NumOfDamageDice, poisonSpell.SizeOfDamageDice);
                        if(player.Player.Stats.CurrentHP - pDamage <= 0)
                        {
                            player.Send($"The poison in your veins is too much for you and you finally succumb!{Constants.NewLine}");
                            playersToKill.Add(player);
                        }
                        else
                        {
                            SessionManager.Instance.GetPlayerByGUID(player.Id).Player.Stats.CurrentHP -= (int)pDamage;
                            if(player.Player.Level >= Constants.ImmLevel || player.Player.ShowDetailedRollInfo)
                            {
                                player.Send($"The poison in your veins deals {pDamage} to you!{Constants.NewLine}");
                            }
                            else
                            {
                                player.Send($"The poison in your veins burns and causes you pain!{Constants.NewLine}");
                            }
                        }
                    }
                }
                if (playersToKill != null && playersToKill.Count > 0)
                {
                    // one or more players was dealt lethal damage by a DOT spell
                    for (int i = 0; i < playersToKill.Count; i++)
                    {
                        var p = playersToKill[i];
                        p.Player.Kill(ref p);
                    }
                }
                playersToKill.Clear();
            }
            var roomsWithPlayers = RoomManager.Instance.GetAllRooms().Values.Where(x => x.PlayersInRoom(x.RoomID) != null && x.PlayersInRoom(x.RoomID).Count > 0).ToList();
            if(roomsWithPlayers != null && roomsWithPlayers.Count > 0)
            {
                _logProvider.LogMessage($"INFO: Pulsing environment buffs on rooms with players", LogLevel.Info, true);
                foreach (var room in roomsWithPlayers)
                {
                    RoomManager.Instance.ProcessEnvironmentBuffs(room.RoomID);
                }
            }
            var npcsWithBuffs = NPCManager.Instance.GetAllNPCIDS().Values.Where(x => x.Buffs != null && x.Buffs.Count > 0).ToList();
            if(npcsWithBuffs != null &&  npcsWithBuffs.Count > 0)
            {
                List<NPC> npcsToKill = new List<NPC>();
                foreach(var npc in npcsWithBuffs)
                {
                    List<string> npcBuffsToRemove = new List<string>();
                    for(int i = 0; i < npc.Buffs.Count; i++)
                    {
                        var buff = npc.Buffs.ElementAt(i).Key;
                        if(buff == "Light")
                        {
                            RoomManager.Instance.GetRoom(npc.CurrentRoom).HasLightSource = true;
                        }
                        if(npc.Buffs.ElementAt(i).Value > -1)
                        {
                            if(npc.Buffs.ElementAt(i).Value - 1 == 0)
                            {
                                npcBuffsToRemove.Add(buff);
                            }
                            else
                            {
                                npc.Buffs[buff]--;
                            }
                        }
                    }
                    if(npcBuffsToRemove.Count > 0)
                    {
                        foreach(var b in npcBuffsToRemove)
                        {
                            npc.RemoveBuff(b);
                            _logProvider.LogMessage($"INFO: Removing buff {b} from NPC {npc.Name}", LogLevel.Info, true);
                        }    
                    }
                    npcBuffsToRemove.Clear();
                    if(npc.Buffs.ContainsKey("Regen"))
                    {
                        var regenSpell = Spells.GetSpell("Regen");
                        var regenHP = Helpers.RollDice(regenSpell.NumOfDamageDice, regenSpell.SizeOfDamageDice);
                        if(npc.Stats.CurrentHP + regenHP >= npc.Stats.MaxHP)
                        {
                            npc.Stats.CurrentHP = (int)npc.Stats.MaxHP;
                        }
                        else
                        {
                            npc.Stats.CurrentHP += (int)regenHP;
                        }
                    }
                    if(npc.Buffs.ContainsKey("Acid Arrow"))
                    {
                        var acidSpell = Spells.GetSpell("Acid Arrow");
                        var acidDmg = Helpers.RollDice(acidSpell.NumOfDamageDice, acidSpell.SizeOfDamageDice);
                        if(npc.Stats.CurrentHP <= acidDmg)
                        {
                            npcsToKill.Add(npc);
                        }
                        else
                        {
                            npc.Stats.CurrentHP -= (int)acidDmg;
                        }
                    }
                }
                if(npcsToKill != null && npcsToKill.Count > 0)
                {
                    for (int i = 0; i < npcsToKill.Count; i++)
                    {
                        var npc = npcsToKill[i];
                        npc.Kill(false);
                    }
                }
                npcsToKill.Clear();
            }
            var roomsWithNPCS = NPCManager.Instance.GetAllNPCIDS().Values.Select(x => x.CurrentRoom).Distinct().ToList();
            if(roomsWithNPCS != null && roomsWithNPCS.Count > 0)
            {
                _logProvider.LogMessage($"INFO: Pulsing environment buffs on rooms with NPCs", LogLevel.Info, true);
                foreach(var room in roomsWithNPCS)
                {
                    RoomManager.Instance.ProcessEnvironmentBuffs(room);
                }
            }
        }

        private void PulseAllZones()
        {
            foreach(var zone in ZoneManager.Instance.GetAllZones())
            {
                _logProvider.LogMessage($"TICK: Pulsing Zone {zone.Key} ({zone.Value.ZoneName})", LogLevel.Info, true);
                var npcsForZone = NPCManager.Instance.GetNPCsForZone(zone.Key);
                if(npcsForZone != null && npcsForZone.Count > 0)
                {
                    foreach (var room in RoomManager.Instance.GetRoomIDSForZone(zone.Key))
                    {
                        if(!RoomManager.Instance.GetRoom(room).Flags.HasFlag(RoomFlags.NoMobs)) // ensure we can't spawn an NPC in a NoMob room
                        {
                            foreach (var npc in npcsForZone)
                            {
                                var roll = Helpers.RollDice(1, 100);
                                if (roll < npc.AppearChance && (NPCManager.Instance.GetCountOfNPCsInWorld(npc.NPCID) + 1 <= npc.MaxNumber))
                                {
                                    NPCManager.Instance.AddNPCToWorld(npc.NPCID, room);
                                }
                            }
                        }
                        var items = RoomManager.Instance.GetRoom(room).SpawnItemsAtTick;
                        if (items != null && items.Count > 0)
                        {
                            foreach(var item in items)
                            {
                                var i = ItemManager.Instance.GetItemByID(item.Key);
                                if(i != null)
                                {
                                    for (var j = 0; j < item.Value; j++)
                                    {
                                        if(Helpers.RollDice(1,100) <= 38)
                                        {
                                            RoomManager.Instance.AddItemToRoomInventory(room, ref i);
                                        }
                                    }
                                }
                            }
                        }
                        var npcs = RoomManager.Instance.GetRoom(room).SpawnNPCsAtTick;
                        if(npcs != null && npcs.Count > 0)
                        {
                            foreach(var npc in npcs)
                            {
                                if(RoomManager.Instance.GetNPCsInRoom(room).Where(x => x.NPCID == npc.Value).Count() < npc.Value)
                                {
                                    NPCManager.Instance.AddNPCToWorld(npc.Key, room);
                                }
                            }
                        }
                    }
                }
                var caves = RoomManager.Instance.GetAllRooms().Values.Where(x => x.ZoneID == zone.Key && x.Flags.HasFlag(RoomFlags.Cave) && x.ResourceNode == null).ToList();
                foreach (var room in caves)
                {
                    var roll = Helpers.RollDice(1, 100);
                    var node = NodeManager.Instance.GetNode(roll);
                    if (node != null)
                    {
                        RoomManager.Instance.GetRoom(room.RoomID).ResourceNode = node;
                        _logProvider.LogMessage($"INFO: Added {node} node to RID {room.RoomID}", LogLevel.Info, true);
                    }
                    else
                    {
                        _logProvider.LogMessage($"INFO: Rolled Node was null, skipping adding Resource Node to Room {room.RoomID}", LogLevel.Info, true);
                    }
                }
            }
        }

        private void PulseNPCs()
        {
            _logProvider.LogMessage($"TICK: Pulsing NPCs", LogLevel.Info, true);
            var npcs = NPCManager.Instance.GetAllNPCIDS();
            foreach(var npc in npcs)
            {
                if(!npc.Value.BehaviourFlags.HasFlag(NPCFlags.Hostile))
                {
                    // non-hostile NPC actions, e.g. moving around, picking up random items etc
                    if(!npc.Value.BehaviourFlags.HasFlag(NPCFlags.Sentinel))
                    {
                        // npc is not a sentinel so we can move it around
                        var moveChance = Helpers.RollDice(1, 100);
                        if(moveChance <= 25)
                        {
                            // move the npc
                            var n = NPCManager.Instance.GetNPCByGUID(npc.Key);
                            if(n != null)
                            {
                                // only move the NPC if it isn't in an active combat session
                                if(!CombatManager.Instance.IsNPCInCombat(npc.Key))
                                {
                                    var roomExits = RoomManager.Instance.GetRoom(n.CurrentRoom).RoomExits;
                                    if (roomExits != null && roomExits.Count > 0)
                                    {
                                        var rnd = new Random(DateTime.Now.ToString().GetHashCode());
                                        var index = rnd.Next(0, roomExits.Count);
                                        // only allow the NPC to move if there is no door or the door is open
                                        if (roomExits[index].RoomDoor == null || roomExits[index].RoomDoor.IsOpen)
                                        {
                                            var destRID = roomExits[index].DestinationRoomID;
                                            if (RoomManager.Instance.RoomExists(destRID))
                                            {
                                                // Only move the NPC to the target room if the RID is in the zone the NPC appears in and isn't flagged NoMob - prevent NPCs from wandering too much
                                                if (ZoneManager.Instance.IsRIDInZone(destRID, n.AppearsInZone) && !RoomManager.Instance.GetRoom(destRID).Flags.HasFlag(RoomFlags.NoMobs))
                                                {
                                                    _logProvider.LogMessage($"INFO: Moving {n.Name} from {n.CurrentRoom} to {destRID}", LogLevel.Info, true);
                                                    npc.Value.Move(ref n, n.CurrentRoom, destRID, false);
                                                }
                                            }
                                            else
                                            {
                                                _logProvider.LogMessage($"WARN: Cannot move {npc.Value.Name} from {npc.Value.CurrentRoom} to {destRID}, room does not exist", LogLevel.Warning, true);
                                            }
                                        }
                                        else
                                        {
                                            _logProvider.LogMessage($"INFO: Cannot move {npc.Value.Name} from {npc.Value.CurrentRoom} to {roomExits[index].DestinationRoomID}, door is not open", LogLevel.Info, true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _logProvider.LogMessage($"ERROR: Cannot find NPC with GUID {npc.Key}", LogLevel.Error, true);
                                continue;
                            }
                        }
                    }
                    if (npc.Value.BehaviourFlags.HasFlag(NPCFlags.Scavenger))
                    {
                        // npc is a sentinel scavenger so see if there are items to take in the room
                        var pickupChance = Helpers.RollDice(1, 100);
                        if(pickupChance <= 33 && !CombatManager.Instance.IsNPCInCombat(npc.Key))
                        {
                            var items = RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).ItemsInRoom;
                            if (items != null && items.Count > 0)
                            {
                                var rnd = new Random(DateTime.Now.ToString().GetHashCode());
                                var index = rnd.Next(0, items.Count);
                                var item = items[index];
                                npc.Value.Inventory.Add(item);
                                RoomManager.Instance.RemoveItemFromRoomInventory(npc.Value.CurrentRoom, ref item);
                                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(npc.Value.CurrentRoom);
                                if(playersToNotify != null &&  playersToNotify.Count > 0)
                                {
                                    foreach(var player in playersToNotify)
                                    {
                                        player.Send($"{npc.Value.Name} greedily snatches up {item.ShortDescription}!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        var goldPickupChance = Helpers.RollDice(1, 100);
                        var gp = RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).GoldInRoom;
                        if (RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).GoldInRoom > 0)
                        {
                            if (goldPickupChance <= 33 && !CombatManager.Instance.IsNPCInCombat(npc.Key))
                            {
                                npc.Value.Stats.Gold += gp;
                                RoomManager.Instance.GetGoldFromRoom(npc.Value.CurrentRoom, gp);
                                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(npc.Value.CurrentRoom);
                                if(playersToNotify != null && playersToNotify.Count > 0)
                                {
                                    foreach(var player in playersToNotify)
                                    {
                                        player.Send($"{npc.Value.Name} snatches up a pile of gold!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // NPC has the hostile flag, so look to see if it is in a room where it can start a fight, if so, start the fight
                    // if not, do something based off another flag or skip its turn
                    if(!CombatManager.Instance.IsNPCInCombat(npc.Key))
                    {
                        bool startFight = false;
                        if (!RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).Flags.HasFlag(RoomFlags.Safe))
                        {
                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(npc.Value.CurrentRoom);
                            if (localPlayers != null && localPlayers.Count > 0)
                            {
                                var rnd = new Random(DateTime.Now.GetHashCode());
                                var tp = localPlayers[rnd.Next(localPlayers.Count)];
                                if (tp.Player.Visible && !tp.Player.IsInCombat)
                                {
                                    startFight = true;
                                    var myInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(tp.Player.Stats.Dexterity));
                                    var mobInit = Convert.ToUInt32(Helpers.RollDice(1, 20) + ActorStats.CalculateAbilityModifier(npc.Value.Stats.Dexterity));
                                    myInit = tp.Player.HasSkill("Awareness") ? myInit += 4 : myInit;
                                    mobInit = npc.Value.HasSkill("Awareness") ? mobInit += 4 : mobInit;
                                    var participants = new List<(uint Initiative, dynamic Participant, dynamic Target)>
                                    {
                                        (myInit, tp, npc.Value),
                                        (mobInit, npc.Value, tp)
                                    };
                                    var g = CombatManager.Instance.AddCombatSession(new CombatSession
                                    {
                                        Participants = participants
                                    });
                                    tp.Player.CombatSessionID = g;
                                    tp.Player.Position = ActorPosition.Fighting;
                                    tp.Send($"{npc.Value.Name} launches an attack on you!{Constants.NewLine}");
                                    _logProvider.LogMessage($"INFO: NPC {npc.Value.Name} starting combat session with {tp.Player.Name} in room {npc.Value.CurrentRoom}", LogLevel.Info, true);
                                    foreach(var lp in localPlayers)
                                    {
                                        if(lp.Player.Name != tp.Player.Name)
                                        {
                                            lp.Send($"{npc.Value.Name} suddenly launches an attack on {tp.Player.Name}!{Constants.NewLine}");
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var player in localPlayers)
                                    {
                                        player.Send($"{npc.Value.Name} looks to be after a fight!{Constants.NewLine}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            var localPlayers = RoomManager.Instance.GetPlayersInRoom(npc.Value.CurrentRoom);
                            if (localPlayers != null && localPlayers.Count > 0)
                            {
                                foreach (var player in localPlayers)
                                {
                                    player.Send($"{npc.Value.Name} looks to be after a fight!{Constants.NewLine}");
                                }
                            }
                        }
                        if (!startFight)
                        {
                            var moveChance = Helpers.RollDice(1, 100);
                            if(moveChance <= 25)
                            {
                                var n = NPCManager.Instance.GetNPCByGUID(npc.Key);
                                if(n != null)
                                {
                                    if(!CombatManager.Instance.IsNPCInCombat(npc.Key))
                                    {
                                        var roomExits = RoomManager.Instance.GetRoom(n.CurrentRoom).RoomExits;
                                        if(roomExits != null && roomExits.Count > 0)
                                        {
                                            var rnd = new Random(DateTime.Now.GetHashCode());
                                            var destRoom = roomExits[rnd.Next(roomExits.Count)];
                                            if(RoomManager.Instance.RoomExists(destRoom.DestinationRoomID) && ZoneManager.Instance.IsRIDInZone(destRoom.DestinationRoomID, n.AppearsInZone))
                                            {
                                                if(!RoomManager.Instance.GetRoom(destRoom.DestinationRoomID).Flags.HasFlag(RoomFlags.NoMobs))
                                                {
                                                    _logProvider.LogMessage($"INFO: Moving {n.Name} from {n.CurrentRoom} to {destRoom.DestinationRoomID}", LogLevel.Info, true);
                                                    npc.Value.Move(ref n, n.CurrentRoom, destRoom.DestinationRoomID, false);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _logProvider.LogMessage($"ERROR: Cannot find NPC with GUID {npc.Key}", LogLevel.Error, true);
                                    continue;
                                }
                            }
                            if(npc.Value.BehaviourFlags.HasFlag(NPCFlags.Scavenger))
                            {
                                var pickupChance = Helpers.RollDice(1, 100);
                                if (pickupChance <= 33 && !CombatManager.Instance.IsNPCInCombat(npc.Key))
                                {
                                    var items = RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).ItemsInRoom;
                                    if (items != null && items.Count > 0)
                                    {
                                        var rnd = new Random(DateTime.Now.GetHashCode());
                                        var item = items[rnd.Next(items.Count)];
                                        npc.Value.Inventory.Add(item);
                                        RoomManager.Instance.GetRoom(npc.Value.CurrentRoom).ItemsInRoom.Remove(item);
                                        var playersToNotify = RoomManager.Instance.GetPlayersInRoom(npc.Value.CurrentRoom);
                                        if (playersToNotify != null && playersToNotify.Count > 0)
                                        {
                                            foreach (var player in playersToNotify)
                                            {
                                                player.Send($"{npc.Value.Name} greedily snatches up {item.ShortDescription}!{Constants.NewLine}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PulseCombat()
        {
            List<Guid> CompletedCombatSessions = new List<Guid>();
            if(CombatManager.Instance.GetCombatQueue().Count > 0)
            {
                var combats = CombatManager.Instance.GetCombatQueue();
                foreach (var combat in combats)
                {
                    CombatManager.Instance.ProcessCombatRound(combat.Value, combat.Key, out bool combatOver);
                    if(combatOver)
                    {
                        CompletedCombatSessions.Add(combat.Key);
                    }
                }
                if(CompletedCombatSessions.Count > 0)
                {
                    foreach(var s in CompletedCombatSessions)
                    {
                        CombatManager.Instance.RemoveCombatSession(s);
                    }
                }
            }
            CompletedCombatSessions.Clear();
        }

        private void PulseAutoSave()
        {
            var playersInGame = SessionManager.Instance.GetAllPlayers().Where(x => x.IsConnected).ToList();
            if(playersInGame != null && playersInGame.Count > 0)
            {
                _logProvider.LogMessage($"AUTOSAVE: Processing auto-save of {playersInGame.Count} connected players...", LogLevel.Info, true);
                foreach(var p in playersInGame)
                {
                    var player = p;
                    SessionManager.Instance.GetPlayerByGUID(p.Id).Player.IdleTicks++;
                    if (DatabaseManager.SavePlayerNew(ref player, false))
                    {
                        _logProvider.LogMessage($"AUTOSAVE: Player {player.Player.Name} saved by autosave tick", LogLevel.Info, true);
                    }
                    else
                    {
                        _logProvider.LogMessage($"AUTOSAVE: Failed to save {player.Player.Name} on autosave tick", LogLevel.Error, true);
                    }
                }
            }
            else
            {
                _logProvider.LogMessage($"AUTOSAVE: No players to save on tick.", LogLevel.Info, true);
            }
            var idlePlayers = SessionManager.Instance.GetAllPlayers().Where(x => x.IsConnected && x.Player.IdleTicks > Constants.MaxIdleTickCount()).ToList();
            if(idlePlayers != null && idlePlayers.Count > 0)
            {
                _logProvider.LogMessage($"INFO: Disconnecting idle players; {idlePlayers.Count} to process", LogLevel.Info, true);
                foreach (var player in idlePlayers)
                {
                    if(player.Player.Level < Constants.ImmLevel || (player.Player.Level >= Constants.ImmLevel && Constants.DisconnectIdleImms()))
                    {
                        _logProvider.LogMessage($"INFO: {player.Player} has been idle for {player.Player.IdleTicks} ticks and will be disconnected", LogLevel.Info, true);
                        player.Send($"You have been disconnected due to an idle timeout.{Constants.NewLine}");
                        SessionManager.Instance.Close(player);
                    }
                }
            }
        }

        private void CleanupDescriptors()
        {
            uint clearedDescriptors = 0;
            while(SessionManager.Instance.GetDisconnectedSessions().Count() > 0)
            {
                var s = SessionManager.Instance.GetDisconnectedSessions().FirstOrDefault();
                if(s != null)
                {
                    SessionManager.Instance.Close(s);
                    clearedDescriptors++;
                }
            }
            _logProvider.LogMessage($"INFO: Cleared {clearedDescriptors} stale connections", LogLevel.Info, true);
        }
    }
}
