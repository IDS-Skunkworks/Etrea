using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.IO;
using Etrea3.Objects;
using System.Collections.Concurrent;

namespace Etrea3.Core
{
    public static class DatabaseManager
    {
        #region Config
        private static readonly string worldDBPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world")}\\world.db";
        private static readonly string playerDBPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world")}\\players.db";
        private static readonly string logDBPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world")}\\logs.db";
        private static readonly string worldDBConnectionString = $"URI=file:{worldDBPath}";
        private static readonly string playerDBConnectionString = $"URI=file:{playerDBPath}";
        private static readonly string logDBConnectionString = $"URI=file:{logDBPath}";

        public static void AddLogEntry(string message, LogLevel level)
        {
            try
            {
                using (var con = new SQLiteConnection(logDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblLogs (LogID, LogDate, LogType, LogMessage) VALUES (@i, @d, @t, @m);";
                        cmd.Parameters.Add(new SQLiteParameter("@i", Guid.NewGuid().ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@d", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                        cmd.Parameters.Add(new SQLiteParameter("@t", level.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@m", message));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch { }
        }

        public static List<LogEntry> GetLogEntries(string type, int amount)
        {
            try
            {
                var retval = new List<LogEntry>();
                using (var con = new SQLiteConnection(logDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblLogs WHERE LogType = @t COLLATE NOCASE;";
                        cmd.Parameters.Add(new SQLiteParameter("@t", type));
                        cmd.Prepare();
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                retval.Add(new LogEntry
                                {
                                    LogDate = DateTime.Parse(dr["LogDate"].ToString()),
                                    LogMessage = dr["LogMessage"].ToString(),
                                    LogType = dr["LogType"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
                if (retval.Count > 0)
                {
                    int returnAmount = Math.Min(amount, retval.Count);
                    return retval.OrderByDescending(x => x.LogDate).Take(returnAmount).ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in GetLogEntries(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        public static bool ClearMOTD()
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblMudOptions SET OptionValue = '' WHERE OptionName = 'MOTD';";
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.ClearMOTD(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SetMOTD(string motd)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UDPATE tblMudOptions SET OptionValue = @m WHERE OptionName = 'MOTD';";
                        cmd.Parameters.Add(new SQLiteParameter("@m", motd));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SetMOTD(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static string GetMOTD()
        {
            string result = string.Empty;
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT OptionValue FROM tblMudOptions WHERE OptionName = 'MOTD';";
                        result = cmd.ExecuteScalar().ToString();
                    }
                    con.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.GetMOTD(): {ex.Message}", LogLevel.Error, true);
                return string.Empty;
            }
        }

        public static bool ClearLogTable(out int rCount)
        {
            rCount = 0;
            try
            {
                using (var con = new SQLiteConnection(logDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblLogs;";
                        rCount = cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Players
        public static bool ValidateAPIKey(string apiKey, out string user)
        {
            user = string.Empty;
            try
            {
                int count = 0;
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblPlayers WHERE APIKey = @k;";
                        cmd.Parameters.Add(new SQLiteParameter("@k", apiKey));
                        cmd.Prepare();
                        count = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    if (count == 1)
                    {
                        using (var cmd = new SQLiteCommand(con))
                        {
                            cmd.CommandText = "SELECT PlayerName FROM tblPlayers WHERE APIKey = @k;";
                            cmd.Parameters.Add(new SQLiteParameter("@k", apiKey));
                            cmd.Prepare();
                            user = cmd.ExecuteScalar().ToString();
                        }
                    }
                    con.Close();
                }
                return count == 1;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.ValidateAPIKey(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static string GetPlayerAPIKey(string playerName)
        {
            try
            {
                string apiKey = string.Empty;
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT APIKey FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", playerName));
                        cmd.Prepare();
                        apiKey = cmd.ExecuteScalar()?.ToString() ?? string.Empty;
                    }
                    con.Close();
                }
                return apiKey;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.GetPlayerAPIKey(): {ex.Message}", LogLevel.Error, true);
                return string.Empty;
            }
        }

        public static bool UpdatePlayerAPIKey(string playerName, string apiKey)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblPlayers SET APIKey = @k WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@k", apiKey));
                        cmd.Parameters.Add(new SQLiteParameter("@n", playerName));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.UpdatePlayerAPIKey(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool ChangePlayerPassword(string charName, string newPwd)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblPlayers SET PlayerPassword = @p WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@p", newPwd.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@n", charName));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.ChangePlayerPassword(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool DeleteCharacter(string charName)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", charName));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblMail WHERE MailTo = @n COLLATE NOCASE;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", charName));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.DeleteCharacter(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool CharacterExists(string charName)
        {
            int cnt = 0;
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblPlayers WHERE PlayerName = @p;";
                        cmd.Parameters.Add(new SQLiteParameter("@p", charName));
                        cmd.Prepare();
                        cnt = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    con.Close();
                }
                return cnt > 0;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.CharacterExists() looking for '{charName}': {ex.Message}", LogLevel.Error, true);
                return true; // return true to assume the player exists and prevent issues
            }
        }

        public static bool ValidatePlayerPassword(string playerName, string password)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT PlayerPassword FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", playerName));
                        cmd.Prepare();
                        var result = cmd.ExecuteScalar().ToString();
                        con.Close();
                        return result == password;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.ValidatePlayerPassword() verifying password for player '{playerName}': {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static int GetPlayerCount()
        {
            int playerCount = 0;
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblPlayers;";
                        playerCount = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    con.Close();
                }
                return playerCount;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.GetPlayerCount(): {ex.Message}", LogLevel.Error, true);
                return 1; // prevent a character from becoming an Imm in the event of an exception in this function
            }
        }

        public static Player LoadPlayer(string playerName)
        {
            string playerJson = string.Empty;
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT PlayerObject FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", playerName));
                        cmd.Prepare();
                        playerJson = cmd.ExecuteScalar().ToString();
                    }
                    con.Close();
                }
                return Helpers.DeserialiseEtreaObject<Player>(playerJson);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error loading player '{playerName}': {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        public static bool SavePlayer(Session session, bool isNewChar, string pwd = null)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        if (isNewChar)
                        {
                            if (GetPlayerCount() == 0)
                            {
                                Game.LogMessage($"INFO: Player '{session.Player.Name}' is the first player and will be elevated to Godhood!", LogLevel.Info, true);
                                SessionManager.Instance.GetSession(session.Player.Name).Player.Level = (int)Constants.ImmLevel + 10;
                            }
                            cmd.CommandText = "INSERT INTO tblPlayers (PlayerName, PlayerPassword, PlayerObject) VALUES (@n, @p, @o);";
                            cmd.Parameters.Add(new SQLiteParameter("@n", session.Player.Name));
                            cmd.Parameters.Add(new SQLiteParameter("@p", pwd));
                            cmd.Parameters.Add(new SQLiteParameter("@o", Helpers.SerialiseEtreaObject<Player>(session.Player)));
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            cmd.CommandText = "UPDATE tblPlayers SET PlayerObject = @o WHERE PlayerName = @n;";
                            cmd.Parameters.Add(new SQLiteParameter("@o", Helpers.SerialiseEtreaObject<Player>(session.Player)));
                            cmd.Parameters.Add(new SQLiteParameter("@n", session.Player.Name));
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SavePlayer(): Failed to save player '{session.Player.Name}': {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Rooms
        public static bool RoomIDInUse(int roomID)
        {
            try
            {
                int result = 0;
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblRooms WHERE RoomID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", roomID));
                        cmd.Prepare();
                        result = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    con.Close();
                }
                return result > 0;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Exception in DatabaseManager.RoomIDInUse(): {ex.Message}", LogLevel.Error, true);
                return true;
            }
        }

        public static bool RemoveRoom(int roomID)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblRooms WHERE RoomID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", roomID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveRoom(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SaveRoomToWorldDatabase(Room r, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblRooms (RoomID, RoomData) VALUES (@i, @d);" :
                            "UPDATE tblRooms SET RoomData = @d WHERE RoomID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", r.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<Room>(r)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveRoomToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, Room> LoadAllRooms(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, Room> retval = new ConcurrentDictionary<int, Room>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblRooms;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var r = Helpers.DeserialiseEtreaObject<Room>(dr["RoomData"].ToString());
                                retval.TryAdd(r.ID, r);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllRooms(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Quests
        public static bool SaveQuestToWorldDatabase(Quest quest, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblQuests (QuestID, QuestData) VALUES (@i, @d);" :
                            "UPDATE tblQuests SET QuestData = @d WHERE QuestID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", quest.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<Quest>(quest)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveQuestToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool RemoveQuest(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblQuests WHERE QuestID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveQuest(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, Quest> LoadAllQuests(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, Quest> retval = new ConcurrentDictionary<int, Quest>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblQuests;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var q = Helpers.DeserialiseEtreaObject<Quest>(dr["QuestData"].ToString());
                                retval.TryAdd(q.ID, q);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllQuests(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Crafting Recipies
        public static bool RemoveRecipe(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblCraftingRecipes WHERE RecipeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveRecipe(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SaveRecipeToWorldDatabase(CraftingRecipe recipe, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblCraftingRecipes (RecipeID, RecipeData) VALUES (@i, @d);" :
                            "UPDATE tblCraftingRecipes SET RecipeData = @d WHERE RecipeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", recipe.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<CraftingRecipe>(recipe)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveRecipeToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, CraftingRecipe> LoadAllRecipes(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, CraftingRecipe> retval = new ConcurrentDictionary<int, CraftingRecipe>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblCraftingRecipes;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var r = Helpers.DeserialiseEtreaObject<CraftingRecipe>(dr["RecipeData"].ToString());
                                retval.TryAdd(r.ID, r);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllRecipes(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region NPCs
        public static bool RemoveNPC(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblNPCs WHERE NPCID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveNPC(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SaveNPCTemplateToWorldDatabase(NPC npc, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblNPCs (NPCID, NPCName, NPCData) VALUES (@i, @n, @d);" :
                            "UPDATE tblNPCs SET NPCName = @n, NPCData = @d WHERE NPCID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", npc.TemplateID));
                        cmd.Parameters.Add(new SQLiteParameter("@n", npc.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<NPC>(npc)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveNPCTemplateToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, NPC> LoadAllNPCTemplates(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, NPC> retval = new ConcurrentDictionary<int, NPC>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblNPCs;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var npc = Helpers.DeserialiseEtreaObject<NPC>(dr["NPCData"].ToString());
                                retval.TryAdd(npc.TemplateID, npc);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllNPCTemplates(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Resource Nodes
        public static bool RemoveResourceNode(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblResourceNodes WHERE NodeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveResourceNode(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SaveNodeToWorldDatabase(ResourceNode node, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblResourceNodes (NodeID, NodeData) VALUES (@i, @d);" :
                            "UPDATE tblResourceNodes SET NodeData = @d WHERE NodeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", node.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<ResourceNode>(node)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveNodeToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, ResourceNode> LoadAllNodes(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, ResourceNode> retval = new ConcurrentDictionary<int, ResourceNode>();
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblResourceNodes;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var node = Helpers.DeserialiseEtreaObject<ResourceNode>(dr["NodeData"].ToString());
                                retval.TryAdd(node.ID, node);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllNodes(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Spells
        public static bool SaveSpellToWorldDatabase(Spell spell, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblSpells (ID, SpellData) VALUES (@i, @d);" :
                            "UPDATE tblSpells SET SpellData = @d WHERE ID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", spell.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<Spell>(spell)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveSpellToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool RemoveSpell(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblSpells WHERE ID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveSpell(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, Spell> LoadAllSpells(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, Spell> retval = new ConcurrentDictionary<int, Spell>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblSpells;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var s = Helpers.DeserialiseEtreaObject<Spell>(dr["SpellData"].ToString());
                                retval.TryAdd(s.ID, s);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch(Exception ex)
            {
                hasErr = true;
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllSpells(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }
        #endregion

        #region Emotes
        public static bool RemoveEmote(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblEmotes WHERE EmoteID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveEmote(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SaveEmoteToWorldDatabase(Emote emote, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblEmotes (EmoteID, EmoteName, EmoteData) VALUES (@i, @n, @d);" :
                            "UPDATE tblEmotes SET EmoteName = @n, EmoteData = @d WHERE EmoteID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", emote.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@n", emote.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<Emote>(emote)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveEmoteToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, Emote> LoadAllEmotes(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, Emote> retval = new ConcurrentDictionary<int, Emote>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblEmotes;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var e = Helpers.DeserialiseEtreaObject<Emote>(dr["EmoteData"].ToString());
                                retval.TryAdd(e.ID, e);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllEmotes(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Zones
        public static ConcurrentDictionary<int, Zone> LoadAllZones(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, Zone> retval = new ConcurrentDictionary<int, Zone>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblZones;";
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                int zID = int.Parse(dr["ZoneID"].ToString());
                                Zone z = new Zone
                                {
                                    ZoneID = zID,
                                    ZoneName = dr["ZoneName"].ToString(),
                                    MinRoom = int.Parse(dr["ZoneMinRoom"].ToString()),
                                    MaxRoom = int.Parse(dr["ZoneMaxRoom"].ToString())
                                };
                                retval.TryAdd(zID, z);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllZones(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        public static bool SaveZoneToWorldDatabase(Zone z, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblZones (ZoneID, ZoneName, ZoneMinRoom, ZoneMaxRoom) VALUES (@i, @n, @min, @max);" :
                            "UPDATE tblZones SET ZoneName = @n, ZoneMinRoom = @min, ZoneMaxRoom = @max WHERE ZoneID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", z.ZoneID));
                        cmd.Parameters.Add(new SQLiteParameter("@n", z.ZoneName));
                        cmd.Parameters.Add(new SQLiteParameter("@min", z.MinRoom));
                        cmd.Parameters.Add(new SQLiteParameter("@max", z.MaxRoom));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Exception in DatabaseManager.SaveZoneToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool RemoveZone(int zoneID)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblZones WHERE ZoneID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", zoneID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveZone(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Inventory Items
        public static bool SaveItemToWorldDatabase(InventoryItem item, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblItems (ItemID, ItemData) VALUES (@i, @d);" :
                            "UPDATE tblItems SET ItemData = @d WHERE ItemID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", item.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<dynamic>(item)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveItemToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool RemoveItem(int itemID)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblItems WHERE ItemID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", itemID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveItem(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, InventoryItem> LoadAllItems(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, InventoryItem> retval = new ConcurrentDictionary<int, InventoryItem>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblItems;";
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                dynamic i = Helpers.DeserialiseEtreaObject<dynamic>(dr["ItemData"].ToString());
                                retval.TryAdd(i.ID, i);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllItems(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Shops
        public static bool RemoveShop(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblShops WHERE ShopID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveShop(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool SaveShopToWorldDatabase(Shop s, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblShops (ShopID, ShopData) VALUES (@i, @s);" :
                            "UPDATE tblShops SET ShopData = @s WHERE ShopID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", s.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@s", Helpers.SerialiseEtreaObject<Shop>(s)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveShopToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, Shop> LoadAllShops(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, Shop> retval = new ConcurrentDictionary<int, Shop>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblShops;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Shop s = Helpers.DeserialiseEtreaObject<Shop>(dr["ShopData"].ToString());
                                retval.TryAdd(s.ID, s);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllShops(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Mail
        public static List<PlayerMail> GetPlayerMail(string pName)
        {
            var retval = new List<PlayerMail>();
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT MailData FROM tblMail WHERE MailTo = @n COLLATE NOCASE;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", pName));
                        cmd.Prepare();
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                retval.Add(Helpers.DeserialiseEtreaObject<PlayerMail>(dr["MailData"].ToString()));
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.GetPlayerMail(): {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        public static bool SendMail(PlayerMail mail)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblMail (MailID, MailFrom, MailTo, MailRead, MailData) VALUES (@i, @f, @t, 'FALSE', @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@i", mail.MailGuid.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@f", mail.Sender));
                        cmd.Parameters.Add(new SQLiteParameter("@t", mail.Recipient));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<PlayerMail>(mail)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SendMail(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool MarkMailAsRead(PlayerMail mail)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblMail SET MailRead = 'TRUE', MailData = @d WHERE MailID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", mail.MailGuid.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<PlayerMail>(mail)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.MarkMailAsRead(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static bool DeleteMail(string mailID)
        {
            try
            {
                using (var con = new SQLiteConnection(playerDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblMail WHERE MailID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", mailID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.DeleteMail(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Help Articles
        public static bool RemoveArticle(string title)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblHelpEntries WHERE HelpName = @t;";
                        cmd.Parameters.Add(new SQLiteParameter("@t", title));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveArticle(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<string, HelpArticle> LoadAllArticles(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<string, HelpArticle> retval = new ConcurrentDictionary<string, HelpArticle>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblHelpEntries;";
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                retval.TryAdd(dr["HelpName"].ToString(), new HelpArticle
                                {
                                   Title = dr["HelpName"].ToString(),
                                   ArticleText = dr["HelpText"].ToString(),
                                   ImmOnly = bool.Parse(dr["ImmOnly"].ToString())
                                });
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllArticles(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        public static bool SaveArticleToWorldDatabase(HelpArticle article, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblHelpEntries (HelpName, HelpText, ImmOnly) VALUES (@n, @t, @i);" :
                            "UPDATE tblHelpEntries SET HelpText = @t, ImmOnly = @i WHERE HelpName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", article.Title));
                        cmd.Parameters.Add(new SQLiteParameter("@t", article.ArticleText));
                        cmd.Parameters.Add(new SQLiteParameter("@i", article.ImmOnly.ToString()));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveArticleToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region MobProgs
        public static bool RemoveMobProg(int id)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblMobProgs WHERE ID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.RemoveMobProg(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        public static ConcurrentDictionary<int, MobProg> LoadAllMobProgs(out bool hasErr)
        {
            hasErr = false;
            ConcurrentDictionary<int, MobProg> retval = new ConcurrentDictionary<int, MobProg>();
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblMobProgs;";
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var mobProgs = Helpers.DeserialiseEtreaObject<MobProg>(dr["MobProgData"].ToString());
                                retval.TryAdd(mobProgs.ID, mobProgs);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllMobProgs(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        public static bool SaveMobProgToWorldDatabase(MobProg mobProg, bool isNew)
        {
            try
            {
                using (var con = new SQLiteConnection(worldDBConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = isNew ? "INSERT INTO tblMobProgs (ID, MobProgData) VALUES (@i, @d);" :
                            "UPDATE tblMobProgs SET MobProgData = @d WHERE ID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", mobProg.ID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEtreaObject<MobProg>(mobProg)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.SaveMobProgToWorldDatabase(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion
    }
}