using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Kingdoms_of_Etrea.Entities;
using System.IO;

namespace Kingdoms_of_Etrea.Core
{
    internal static class DatabaseManager
    {
        private static readonly string worldDBPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world")}\\world.db";
        private static readonly string playerDBPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world")}\\players.db";

        #region Config
        internal static bool SetMOTD(ref Descriptor desc, string msg)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblMudOptions SET OptionValue = @m WHERE OptionName = 'MOTD';";
                        cmd.Parameters.Add(new SQLiteParameter("@m", msg));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error updating the MOTD: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static string GetMOTD()
        {
            string cs = $"URI=file:{worldDBPath}";
            string result = string.Empty;
            try
            {
                using (var con = new SQLiteConnection(cs))
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
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error getting MOTD value from World Database: {ex.Message}", LogLevel.Error, true);
                return string.Empty;
            }
        }
        #endregion

        #region Quests
        internal static bool IsQuestIDInUse(ref Descriptor desc, uint id)
        {
            string cs = $"URI=file:{worldDBPath}";
            bool result = false;
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblQuests WHERE QuestID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        result = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
                    }
                    con.Close();
                }
                return result;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error checking if Quest ID {id} was already in use: {ex.Message}", LogLevel.Error, true);
                return true; // assume the ID is in use to avoid further issues
            }
        }

        internal static bool UpdateQuest(ref Descriptor desc, Quest q)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblQuests SET QuestData = @d WHERE QuestID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseQuest(q)));
                        cmd.Parameters.Add(new SQLiteParameter("@i", q.QuestID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player} has updated Quest {q.QuestID} in the World Database", LogLevel.Info, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error updating Quest {q.QuestID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool AddQuest(ref Descriptor desc, Quest q)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con =  new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblQuests (QuestGUID, QuestID, QuestData) VALUES (@g, @i, @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@g", q.QuestGUID));
                        cmd.Parameters.Add(new SQLiteParameter("@i", q.QuestID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseQuest(q)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player} added Quest {q.QuestID} to the World Database", LogLevel.Info, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error adding Quest {q.QuestID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool DeleteQuest(ref Descriptor desc, uint questID)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblQuests WHERE QuestID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", questID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player} has deleted Quest with ID {questID}", LogLevel.Info, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error deleting Quest {questID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static Dictionary<uint, Quest> LoadAllQuests(out bool hasErr)
        {
            hasErr = false;
            var retval = new Dictionary<uint, Quest>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new  SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblQuests;";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var q = Helpers.DeserialiseQuest(reader["QuestData"].ToString());
                                retval.Add(q.QuestID, q);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error loading Quests from World database: {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }
        #endregion

        #region Mail
        internal static Dictionary<uint, Mail> GetAllPlayerMail(ref Descriptor desc)
        {
            var mails = new Dictionary<uint, Mail>();
            string cs = $"URI=file:{playerDBPath}";
            uint index = 1;
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblMail WHERE MailTo = @p;";
                        cmd.Parameters.Add(new SQLiteParameter("@p", desc.Player.Name));
                        cmd.Prepare();
                        using(SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Mail m = Helpers.DeserialiseMail(dr["MailData"].ToString());
                                m.MailRead = bool.Parse(dr["MailRead"].ToString());
                                mails.Add(index, m);
                                index++;
                            }
                        }
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Getting mail for {desc.Player}: {mails.Count} items retrieved", LogLevel.Info, true);
                return mails;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error getting mail for {desc.Player}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal static bool SendNewMail(ref Descriptor desc, ref Mail toSend)
        {
            string cs = $"URI=file:{playerDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblMail (MailID, MailFrom, MailTo, MailRead, MailData) VALUES (@i, @f, @t, @r, @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@i", toSend.MailID.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@f", desc.Player.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@t", toSend.MailTo));
                        cmd.Parameters.Add(new SQLiteParameter("@r", "False"));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseMail(toSend)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player} sent a mail to {toSend.MailTo}", LogLevel.Info, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error sending mail from {desc.Player} to {toSend.MailTo}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool DeleteMailByID(ref Descriptor desc, Guid id)
        {
            string cs = $"URI=file:{playerDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblMail WHERE MailID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id.ToString()));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: {desc.Player} deleted mail with ID {id}", LogLevel.Info, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error deleting mail with ID {id}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static void MarkMailAsRead(ref Descriptor desc, Guid id)
        {
            string cs = $"URI=file:{playerDBPath}";
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblMail SET MailRead = 'True' WHERE MailID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id.ToString()));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Successfully marked mail with ID {id} as read", LogLevel.Info, true);
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error marking mail with ID {id} as read: {ex.Message}", LogLevel.Error, true);
            }
        }
        #endregion

        #region ResourceNodes
        internal static Dictionary<uint, ResourceNode> LoadAllResourceNodes(out bool hasErr)
        {
            hasErr = false;
            var nodes = new Dictionary<uint, ResourceNode>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblResourceNodes;";
                        using(SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while(dr.Read())
                            {
                                ResourceNode n = Helpers.DeserialiseResourceNode(dr["NodeData"].ToString());
                                nodes.Add(n.Id, n);
                            }
                        }
                    }
                    con.Close();
                }
                return nodes;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllResourceNodes(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool DeleteResourceNodeByID(ref Descriptor desc, ref ResourceNode n)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblResourceNodes WHERE NodeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", n.Id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing Resource Node {n.Id} from World Database: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool IsNodeIDInUse(ref Descriptor desc, uint nodeID)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                int nodeCount = 0;
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblResourceNodes WHERE NodeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", nodeID));
                        cmd.Prepare();
                        nodeCount = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    con.Close();
                    return nodeCount > 0;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error checking if Node ID is already in use: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool UpdateResourceNode(ref Descriptor desc, ref ResourceNode n)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblResourceNodes SET NodeData = @d WHERE NodeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseResourceNode(n)));
                        cmd.Parameters.Add(new SQLiteParameter("@i", n.Id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Resource Node {n.Id}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool AddNewResourceNode(ref Descriptor desc, ref ResourceNode n)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblResourceNodes (NodeID, NodeData) VALUES (@i, @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@i", n.Id));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseResourceNode(n)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding new Resource Node: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region CraftingRecipes
        internal static Dictionary<uint, Crafting.Recipe> LoadAllCraftingRecipes(out bool hasErr)
        {
            hasErr = false;
            string cs = $"URI=file:{worldDBPath}";
            var recipes = new Dictionary<uint, Crafting.Recipe>();
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblCraftingRecipes;";
                        using(SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while(dr.Read())
                            {
                                Crafting.Recipe r = Helpers.DeserialiseRecipe(dr["RecipeData"].ToString());
                                recipes.Add(r.RecipeID, r);
                            }
                        }
                    }
                    con.Close();
                }
                return recipes;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllCraftingRecipes(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool UpdateRecipe(ref Descriptor desc, ref Crafting.Recipe r)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblCraftingRecipes SET RecipeData = @d WHERE RecipeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseCraftingRecipe(r)));
                        cmd.Parameters.Add(new SQLiteParameter("@i", r.RecipeID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error updaing Recipe '{r.RecipeName}' ({r.RecipeID}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool AddNewRecipe(ref Descriptor desc, ref Crafting.Recipe r)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblCraftingRecipes (RecipeID, RecipeData) VALUES (@i, @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@i", r.RecipeID));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseCraftingRecipe(r)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding Recipe '{r.RecipeName}' ({r.RecipeID}) to the World Database: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool DeleteRecipeByID(ref Descriptor desc, uint id)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
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
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player} encountered an error trying to remove Recipe ID {id} from the World Database: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool IsRecipeIDInUse(ref Descriptor desc, uint id)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                bool inUse;
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblCraftingRecipes WHERE RecipeID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        var result = int.Parse(cmd.ExecuteScalar().ToString());
                        inUse = result > 0;
                    }
                    con.Close();
                    return inUse;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error checking if Recipe ID {id} is in use: {ex.Message}", LogLevel.Error, true);
                return true;
            }
        }
        #endregion

        #region Shop
        internal static Dictionary<uint, Shop> LoadAllShops(out bool hasErr)
        {
            hasErr = false;
            var retval = new Dictionary<uint, Shop>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblShops;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Shop s = Helpers.DeserialiseRoomShop(dr["ShopData"].ToString());
                                retval.Add(s.ShopID, s);
                            }
                        }
                        con.Close();
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error loading Shops from World database: {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool UpdateExistingShop(ref Descriptor desc, Shop s)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblShops SET ShopData = @d WHERE ShopID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseShopObject(s)));
                        cmd.Parameters.Add(new SQLiteParameter("@i", s.ShopID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: {desc.Player.Name} encountered an exception updating Shop {s.ShopID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool DeleteShop(ref Descriptor desc, uint sid)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblShops WHERE ShopID = @s;";
                        cmd.Parameters.Add(new SQLiteParameter("@s", sid));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: {desc.Player.Name} encountered an exception removing Shop {sid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool AddNewShop(ref Descriptor desc, ref Shop s)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblShops (ShopID, ShopData) VALUES (@id, @sd);";
                        cmd.Parameters.Add(new SQLiteParameter("@id", s.ShopID));
                        cmd.Parameters.Add(new SQLiteParameter("@sd", Helpers.SerialiseShopObject(s)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception adding shop {s.ShopID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region NPCs
        internal static bool DeleteNPCByID(ref Descriptor desc, uint id)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblNPCs WHERE NPCID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@id", id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception deleting NPC {id}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool UpdateNPCByID(ref Descriptor desc, ref NPC n)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblNPCs SET NPCName = @n, NPCData = @d WHERE NPCID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", n.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseNPC(n)));
                        cmd.Parameters.Add(new SQLiteParameter("@id", n.NPCID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception updating NPC {n.NPCID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static Dictionary<uint, NPC> LoadAllNPCS(out bool hasErr)
        {
            hasErr = false;
            Dictionary<uint, NPC> retval = new Dictionary<uint, NPC>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblNPCs;";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while(dr.Read())
                            {
                                NPC n = Helpers.DeserialiseNPC(dr["NPCData"].ToString());
                                n.Type = ActorType.NonPlayer;
                                // for compatibility with objects created in older versions - set values if they are null or zero
                                if(n.Skills == null)
                                {
                                    n.Skills = new List<Skills.Skill>();
                                }
                                if(n.Spells == null)
                                {
                                    n.Spells = new List<Spells.Spell>();
                                }
                                if(n.EquippedItems == null)
                                {
                                    n.EquippedItems = new EquippedItems();
                                }
                                if(n.NumberOfAttacks == 0)
                                {
                                    n.NumberOfAttacks = 1;
                                }
                                retval.Add(n.NPCID, n);
                            }
                        }
                    }
                    con.Close();
                }
                return retval;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllNPCS(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool AddNewNPC(ref Descriptor desc, ref NPC n)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblNPCs (NPCID, NPCName, NPCData) VALUES (@id, @n, @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@id", n.NPCID));
                        cmd.Parameters.Add(new SQLiteParameter("@n", n.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseNPC(n)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered exception saving new NPC: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Rooms
        internal static bool AddDefaultRoom(Room r)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblRooms (RoomID, RoomData) VALUES (@id, @rd);";
                        cmd.Parameters.Add(new SQLiteParameter("@id", r.RoomID));
                        cmd.Parameters.Add(new SQLiteParameter("@rd", Helpers.SerialiseRoomObject(r)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
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

        internal static bool UpdateRoom(ref Descriptor desc, ref Room r)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using(var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblRooms SET RoomData = @rd WHERE RoomID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@rd", Helpers.SerialiseRoomObject(r)));
                        cmd.Parameters.Add(new SQLiteParameter("@id", r.RoomID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player.Name} updated Room {r.RoomID} in the World database", LogLevel.Info, true);
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception updating Room {r.RoomID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static Dictionary<uint, Room> LoadAllRoomsNew(out bool hasErr)
        {
            hasErr = false;
            var retval = new Dictionary<uint, Room>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblRooms;";
                        using(var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var r = Helpers.DeserialiseRoomObject(dr["RoomData"].ToString());
                                retval.Add(r.RoomID, r);
                            }
                        }
                        con.Close();
                    }
                }
                return retval;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error loading Rooms from World database: {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool AddNewRoom(ref Descriptor desc, Room newRoom)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblRooms (RoomID, RoomData) VALUES (@id, @rd);";
                        cmd.Parameters.Add(new SQLiteParameter("@id", newRoom.RoomID));
                        cmd.Parameters.Add(new SQLiteParameter("@rd", Helpers.SerialiseRoomObject(newRoom)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player.Name} add Room {newRoom.RoomID} to the World database", LogLevel.Info, true);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error saving new Room {newRoom.RoomID} to the World database: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool DeleteRoomByID(ref Descriptor desc, uint rid)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblRooms WHERE RoomID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@id", rid));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                Game.LogMessage($"INFO: Player {desc.Player.Name} removed Room {rid} from the World database", LogLevel.Warning, true);
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception removing Room {rid}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Players
        internal static bool CharacterExistsInDatabase(string _name)
        {
            int cnt = 1;
            try
            {
                string cs = $"URI=file:{playerDBPath}";
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", _name));
                        cmd.Prepare();
                        cnt = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    con.Close();
                }
                return cnt > 0;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error checking if player '{_name}' exists: {ex.Message}", LogLevel.Error, true);
                return true; // assume the player exists to prevent errors
            }
        }

        internal static bool UpdatePlayerPassword(ref Descriptor desc, string newPW)
        {
            string cs = $"URI=file:{playerDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblPlayers SET PlayerPassword = @p WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@p", newPW));
                        cmd.Parameters.Add(new SQLiteParameter("@n", desc.Player.Name));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Failed to update password for Player {desc.Player}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool SavePlayerNew(ref Descriptor _desc, bool isNewChar, string playerPwd = null)
        {
            try
            {
                string cs = $"URI=file:{playerDBPath}";
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        if (isNewChar)
                        {
                            if (GetPlayerCount() == 0)
                            {
                                Game.LogMessage($"INFO: {_desc.Player.Name} is the first player and will be made a God", LogLevel.Info, true);
                                _desc.Player.Level = (int)Constants.ImmLevel + 10;
                            }
                            cmd.CommandText = "INSERT INTO tblPlayers (PlayerName, PlayerPassword, PlayerObject) VALUES (@pn, @pw, @po);";
                            cmd.Parameters.Add(new SQLiteParameter("@pn", _desc.Player.Name));
                            cmd.Parameters.Add(new SQLiteParameter("@pw", playerPwd));
                            cmd.Parameters.Add(new SQLiteParameter("@po", Helpers.SerialisePlayerObject(_desc.Player)));
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            cmd.CommandText = "UPDATE tblPlayers SET PlayerObject = @po WHERE PlayerName = @pn;";
                            cmd.Parameters.Add(new SQLiteParameter("@po", Helpers.SerialisePlayerObject(_desc.Player)));
                            cmd.Parameters.Add(new SQLiteParameter("@pn", _desc.Player.Name));
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error saving player {_desc.Player.Name}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static Player LoadPlayerNew(string playerName)
        {
            string cs = $"URI=file:{playerDBPath}";
            Player p = null;
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT PlayerObject FROM tblPlayers WHERE PlayerName = @pn;";
                        cmd.Parameters.Add(new SQLiteParameter("@pn", playerName));
                        cmd.Prepare();
                        p = Helpers.DeserialisePlayerObject(cmd.ExecuteScalar().ToString());
                    }
                    con.Close();
                }
                if(p.Skills == null)
                {
                    p.Skills = new List<Skills.Skill>(); // make sure skills are initialised to support loading players saved on previous code versions
                }
                if(p.Spells == null)
                {
                    p.Spells = new List<Spells.Spell>(); // make sure spells are initialised to support loading players saved on previous code versions
                }
                if(p.KnownRecipes == null)
                {
                    p.KnownRecipes = new List<Crafting.Recipe>(); // make sure known crafting recipes are initialised to support loading players saved on previous versions
                }
                if(p.NumberOfAttacks == 0)
                {
                    p.NumberOfAttacks = 1; // ensure the player always has at least one attack
                }
                if(p.CompletedQuests == null)
                {
                    p.CompletedQuests = new HashSet<Guid>();
                }
                if(p.ActiveQuests == null)
                {
                    p.ActiveQuests = new List<Quest>();
                }
                if(p.VaultStore == null)
                {
                    p.VaultStore = new List<InventoryItem>(); // ensure the player's vault is intialised, if loading a player that was created before this feature was added
                }
                if(p.CommandAliases == null)
                {
                    p.CommandAliases = new Dictionary<string, string>();
                }
                p.PVP = false;
                p.Type = ActorType.Player;
                return p;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error loading player {playerName}: {ex.Message}", LogLevel.Error, true);
                return null;
            }
        }

        internal static bool ValidatePlayerPassword(string pName, string pPwd)
        {
            string cs = $"URI=file:{playerDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT PlayerPassword FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", pName));
                        cmd.Prepare();
                        var result = cmd.ExecuteScalar().ToString();
                        con.Close();
                        return pPwd == result;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error verifying password for Player {pName}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static int GetPlayerCount()
        {
            string cs = $"URI=file:{playerDBPath}";
            int playerCount = 0;
            try
            {
                using (var con = new SQLiteConnection(cs))
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
                Game.LogMessage($"ERROR: Error loading player info from database: {ex.Message}", LogLevel.Error, true);
                return 0;
            }
        }

        internal static bool DeleteCharacter(ref Descriptor desc)
        {
            string cs = $"URI=file:{playerDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblPlayers WHERE PlayerName = @n;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", desc.Player.Name));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error deleting character {desc.Player.Name}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }


        #endregion

        #region Zones
        internal static bool AddNewZone(Zone z)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblZones (ZoneID, ZoneName, ZoneMinRoom, ZoneMaxRoom) VALUES (@id, @zn, @min, @max);";
                        cmd.Parameters.Add(new SQLiteParameter("@id", z.ZoneID));
                        cmd.Parameters.Add(new SQLiteParameter("@zn", z.ZoneName));
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
                Game.LogMessage($"ERROR: Error in DatabaseManager.AddNewZone(): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static Dictionary<uint, Zone> LoadAllZones(out bool hasErr)
        {
            hasErr = false;
            var zones = new Dictionary<uint, Zone>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblZones;";
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var zid = uint.Parse(dr["ZoneID"].ToString());
                                Zone z = new Zone
                                {
                                    ZoneID = zid,
                                    ZoneName = dr["ZoneName"].ToString(),
                                    MinRoom = uint.Parse(dr["ZoneMinRoom"].ToString()),
                                    MaxRoom = uint.Parse(dr["ZoneMaxRoom"].ToString())
                                };
                                zones.Add(zid, z);
                            }
                        }
                        con.Close();
                    }
                }
                return zones;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllZones(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool DeleteZoneByID(ref Descriptor desc, uint zoneID)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(cs))
                    {
                        cmd.CommandText = "DELETE FROM tblZones WHERE ZoneID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@id", zoneID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception removing Zone {zoneID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool UpdateZoneByID(ref Descriptor desc, ref Zone z)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblZones SET ZoneName = @zn, ZoneMinRoom = @min, ZoneMaxRoom = @max WHERE ZoneID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@zn", z.ZoneName));
                        cmd.Parameters.Add(new SQLiteParameter("@min", z.MinRoom));
                        cmd.Parameters.Add(new SQLiteParameter("@max", z.MaxRoom));
                        cmd.Parameters.Add(new SQLiteParameter("@id", z.ZoneID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception updating Zone {z.ZoneID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Emotes
        internal static Dictionary<uint, Emote> LoadAllEmotes(out bool hasErr)
        {
            hasErr = false;
            var emotes = new Dictionary<uint, Emote>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using(var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblEmotes;";
                        using(SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Emote e = Helpers.DeserialiseEmoteObject(dr["EmoteData"].ToString());
                                emotes.Add(uint.Parse(dr["EmoteID"].ToString()), e);
                            }
                        }
                    }
                    con.Close();
                }
                return emotes;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllEmotes(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool UpdateEmoteByID(ref Descriptor desc, ref Emote e)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblEmotes SET EmoteName = @n, EmoteData = @d WHERE EmoteID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", e.EmoteName));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEmoteObject(e)));
                        cmd.Parameters.Add(new SQLiteParameter("@i", e.EmoteID));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception updating Emote ID {e.EmoteID}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool AddNewEmote(ref Descriptor desc, Emote e)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblEmotes (EmoteID, EmoteName, EmoteData) VALUES (@i, @n, @d);";
                        cmd.Parameters.Add(new SQLiteParameter("@i", e.EmoteID));
                        cmd.Parameters.Add(new SQLiteParameter("@n", e.EmoteName));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseEmoteObject(e)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception adding an Emote to the World database: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool DeleteEmoteByID(ref Descriptor desc, uint id)
        {
            string cs = $"URI=file:{worldDBPath}";
            int rowCount;
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblEmotes WHERE EmoteID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@i", id));
                        cmd.Prepare();
                        rowCount = cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                if(rowCount == 1)
                {
                    Game.LogMessage($"INFO: Player {desc.Player.Name} delete Emote ID {id} from the World database.", LogLevel.Info, true);
                    return true;
                }
                if(rowCount == 0)
                {
                    Game.LogMessage($"WARN: Player {desc.Player.Name} attempted to delete Emote ID {id} from the World database but no matching Emote could be found", LogLevel.Warning, true);
                    return true;
                }
                Game.LogMessage($"ERROR: Player {desc.Player.Name} attempted to delete Emote ID {id} from the World database but the attempt may not have been successful. Please manually verify the Emote table.", LogLevel.Error, true);
                return false;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error deleting Emote ID {id} from the World database: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion

        #region Items
        internal static bool AddNewItem(InventoryItem newItem)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "INSERT INTO tblItems (ItemID, ItemName, ItemType, ItemData) VALUES (@id, @in, @it, @da);";
                        cmd.Parameters.Add(new SQLiteParameter("@id", newItem.Id));
                        cmd.Parameters.Add(new SQLiteParameter("@in", newItem.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@it", newItem.ItemType.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@da", Helpers.SerialiseItemObject(newItem)));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.AddNewItem(): {ex.Message}", LogLevel.Error, true);
                return true;
            }
        }

        internal static Dictionary<uint, InventoryItem> LoadAllItems(out bool hasErr)
        {
            hasErr = false;
            var items = new Dictionary<uint, InventoryItem>();
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "SELECT * FROM tblItems";
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                InventoryItem i = Helpers.DeserialiseItemObject(dr["ItemData"].ToString());
                                items.Add(uint.Parse(dr["ItemID"].ToString()), i);
                            }
                        }
                        con.Close();
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in DatabaseManager.LoadAllItems(): {ex.Message}", LogLevel.Error, true);
                hasErr = true;
                return null;
            }
        }

        internal static bool DeleteItemByID(ref Descriptor desc, uint id)
        {
            string cs = $"URI=file:{worldDBPath}";
            int rowCount;
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "DELETE FROM tblItems WHERE ItemID = @id;";
                        cmd.Parameters.Add(new SQLiteParameter("@id", id));
                        cmd.Prepare();
                        rowCount = cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                if (rowCount == 1)
                {
                    Game.LogMessage($"INFO: Player {desc.Player.Name} deleted item ID {id} from the World database", LogLevel.Info, true);
                    return true;
                }
                if (rowCount == 0)
                {
                    Game.LogMessage($"WARN: Player {desc.Player.Name} attempted to delete item ID {id} from the World database but no matching item could be found", LogLevel.Warning, true);
                    return true;
                }
                Game.LogMessage($"ERROR: Player {desc.Player.Name} attempted to delete item ID {id} from the World database but the attempt may not have been successful. Please manually verify the Item table.", LogLevel.Error, true);
                return false;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} was unable to delete item ID {id} from the World database, exception: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal static bool UpdateItemByID(ref Descriptor desc, ref InventoryItem item)
        {
            string cs = $"URI=file:{worldDBPath}";
            try
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = "UPDATE tblItems SET ItemName = @n, ItemType = @t, ItemData = @d WHERE ItemID = @i;";
                        cmd.Parameters.Add(new SQLiteParameter("@n", item.Name));
                        cmd.Parameters.Add(new SQLiteParameter("@t", item.ItemType.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@d", Helpers.SerialiseItemObject(item)));
                        cmd.Parameters.Add(new SQLiteParameter("@i", item.Id));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an exception updating item ID {item.Id}: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
        #endregion
    }
}
