using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using Kingdoms_of_Etrea.Entities;

namespace Kingdoms_of_Etrea.Core
{
    internal static partial class CommandParser
    {
        private static void AddRecipeToPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var lineElements = TokeniseInput(ref line);
                if(lineElements.Length >= 2)
                {
                    var tp = SessionManager.Instance.GetPlayer(lineElements[0].Trim());
                    if(tp != null)
                    {
                        var recipeName = line.Replace(lineElements[0], string.Empty).Trim();
                        if(RecipeManager.Instance.GetRecipe(recipeName) != null)
                        {
                            if (!tp.Player.KnowsRecipe(recipeName))
                            {
                                var r = RecipeManager.Instance.GetRecipe(recipeName);
                                tp.Player.KnownRecipes.Add(r);
                                tp.Send($"{desc.Player} has granted you knowledge of crafting {r.RecipieName}!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"{tp.Player} already knows that recipe!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That recipe doesn't exist!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That person doesn't seem to be in the world right now.{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Usage: addrecipe <player> <recipe name>{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void RemoveRecipeFromPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var lineElements = TokeniseInput(ref line);
                if (lineElements.Length >= 2)
                {
                    var tpName = lineElements[0].Trim();
                    var tp = SessionManager.Instance.GetPlayer(tpName);
                    if(tp != null)
                    {
                        var recipeName = line.Replace(tpName, string.Empty).Trim();
                        var r = RecipeManager.Instance.GetRecipe(recipeName);
                        if(r != null)
                        {
                            if(tp.Player.KnowsRecipe(recipeName))
                            {
                                SessionManager.Instance.GetPlayerByGUID(tp.Id).Player.KnownRecipes.Remove(r);
                                tp.Send($"{desc.Player} has removed your knowledge of crafting {r.RecipieName}!{Constants.NewLine}");
                            }
                            else
                            {
                                desc.Send($"{tp.Player} doesn't know that recipe!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That recipe doesn't exist!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That person doesn't seem to be around right now.{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Usage: removerecipe <player> <recipe name>{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void AddResouceNodeToRoom(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                if(RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode == null)
                {
                    var n = NodeManager.Instance.GetNode(Helpers.RollDice(1,100));
                    RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ResourceNode = n;
                    desc.Send($"Calling on the Winds of Magic you create a {n.NodeName} node for mining!{Constants.NewLine}");
                    Game.LogMessage($"INFO: Player {desc.Player} created a {n.NodeName} node (Depth: {n.NodeDepth}) in Room {desc.Player.CurrentRoom}", LogLevel.Info, true);
                }
                else
                {
                    desc.Send($"There is already a Resource Node in this room!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ClearCharacterDescription(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var target = TokeniseInput(ref input).Last().Trim();
                var p = SessionManager.Instance.GetPlayer(target);
                if (p != null)
                {
                    p.Player.LongDescription = string.Empty;
                    p.Send($"{desc.Player.Name} has wiped your character's long description!{Constants.NewLine}");
                    desc.Send($"You have wiped {p.Player.Name}'s long description!{Constants.NewLine}");
                }
                else
                {
                    desc.Send($"No player with that name could be found in the world!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ListAllNPCS(ref Descriptor desc)
        {
            if (desc.Player.Level >= Constants.ImmLevel)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                var npcsInGame = NPCManager.Instance.GetAllNPCIDS();
                if (npcsInGame != null && npcsInGame.Count > 0)
                {
                    foreach (var n in npcsInGame)
                    {
                        sb.AppendLine($"|| {n.Value.Name} (ID: {n.Value.NPCID}) in Room {n.Value.CurrentRoom}");
                    }
                }
                else
                {
                    sb.AppendLine("|| No NPCs are currently in the world");
                }
                sb.AppendLine($"  {new string('=', 77)}");
                desc.Send(sb.ToString());
            }
            else
            {
                desc.Send($"Only the Gods have that power...{Constants.NewLine}");
            }
        }

        private static void TransferPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var elements = TokeniseInput(ref input);
                if(elements.Length < 2)
                {
                    desc.Send($"Usage: transport <target> <rid>{Constants.NewLine}");
                    return;
                }
                var target = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, elements[1], RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if(target != null)
                {
                    if (uint.TryParse(elements[2].Trim(), out uint tRid))
                    {
                        if(RoomManager.Instance.RoomExists(tRid))
                        {
                            var tCurrentRID = target.Player.CurrentRoom;
                            target.Send($"{desc.Player.Name} has transported you elsewhere!{Constants.NewLine}");
                            target.Player.Move(tCurrentRID, tRid, true, ref target);
                            Game.LogMessage($"INFO: Player {desc.Player.Name} transferred player {target.Player.Name} to RID {tRid}", LogLevel.Info, true);
                        }
                        else
                        {
                            desc.Send($"No Room with ID {tRid} could be found{Constants.NewLine}");
                            Game.LogMessage($"WARN: Player {desc.Player.Name} tried to teleport player {target.Player.Name} to RID {tRid} but no room with that ID could be found", LogLevel.Warning, true);
                        }
                    }
                    else
                    {
                        desc.Send($"The RID {elements[2]} doesn't seem to be valid...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"That person doesn't seem to be in the world...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
            }    
        }

        private static void AddSkillToPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var elements = TokeniseInput(ref input);
                if(elements.Length < 2)
                {
                    desc.Send($"Usage: addskill <target> <skill name>{Constants.NewLine}");
                    return;
                }
                var target = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, elements[1], RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if (target != null)
                {
                    if(target.Player.Level >= desc.Player.Level && target.Player.Name != desc.Player.Name)
                    {
                        desc.Send($"You cannot set the skills of a higher level character!{Constants.NewLine}");
                    }
                    else
                    {
                        var skill = input.Replace(GetVerb(ref input), string.Empty).Replace(elements[1], string.Empty).Trim();
                        if (Skills.SkillExists(skill))
                        {
                            if (!target.Player.HasSkill(skill))
                            {
                                target.Player.AddSkill(skill);
                                desc.Send($"You have granted knowledge of {skill} to {target.Player.Name}{Constants.NewLine}");
                                target.Send($"{desc.Player.Name} has granted you knowledge of {skill}!{Constants.NewLine}");
                                Game.LogMessage($"INFO: Player {desc.Player.Name} has given {target.Player.Name} the skill: {skill}", LogLevel.Info, true);
                            }
                            else
                            {
                                desc.Send($"{target.Player.Name} alreayd knows that skill!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That skill does not exist!{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"That person doesn't seem to be here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
            }
        }

        private static void RemoveSkillFromPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var elements = TokeniseInput(ref input);
                if (elements.Length < 2)
                {
                    desc.Send($"Usage: removeskill <target> <skillname>{Constants.NewLine}");
                    return;
                }
                var target = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, elements[1], RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if (target != null)
                {
                    if(target.Player.Level >= desc.Player.Level && target.Player.Name != desc.Player.Name)
                    {
                        desc.Send($"You cannot set the skills of a higher level character!{Constants.NewLine}");
                    }
                    else
                    {
                        var skill = input.Replace(GetVerb(ref input), string.Empty).Replace(elements[1], string.Empty).Trim();
                        if (Skills.SkillExists(skill))
                        {
                            if (target.Player.HasSkill(skill))
                            {
                                target.Player.RemoveSkill(skill);
                                desc.Send($"You have removed {target.Player.Name}'s knowledge of {skill}!{Constants.NewLine}");
                                target.Send($"{desc.Player.Name} has removed your knowledge of {skill}!{Constants.NewLine}");
                                Game.LogMessage($"INFO: {desc.Player.Name} has removed the skill '{skill}' from player {target.Player.Name}", LogLevel.Info, true);
                            }
                            else
                            {
                                desc.Send($"{target.Player.Name} does not know that skill!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That skill does not exist!{Constants.NewLine}");
                        }
                    }    
                }
                else
                {
                    desc.Send($"That person doesn't seem to be here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
            }
        }

        private static void AddSpellToPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var elements = TokeniseInput(ref input);
                if(elements.Length < 2)
                {
                    desc.Send($"Usage: addspell <target> <spellname>{Constants.NewLine}");
                    return;
                }
                var target = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, elements[1], RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if (target != null)
                {
                    if(target.Player.Level >= desc.Player.Level && target.Player.Name != desc.Player.Name)
                    {
                        desc.Send($"You cannot set the spells of a higher level character!{Constants.NewLine}");
                    }
                    else
                    {
                        var spell = input.Replace(GetVerb(ref input), string.Empty).Replace(elements[1], string.Empty).Trim();
                        if(Spells.SpellExists(spell))
                        {
                            if(!target.Player.HasSpell(spell))
                            {
                                target.Player.AddSpell(spell);
                                desc.Send($"You have granted knowledge of the spell {spell} to {target.Player.Name}{Constants.NewLine}");
                                target.Send($"{desc.Player.Name} has granted you knowledge of the spell {spell}!{Constants.NewLine}");
                                Game.LogMessage($"INFO: Player {desc.Player.Name} has given {target.Player.Name} the spell: {spell}", LogLevel.Info, true);
                            }
                            else
                            {
                                desc.Send($"{target.Player.Name} already knows that spell!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That spell does not exist!{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"That person doesn't seem to be here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
            }
        }

        private static void RemoveSpellFromPlayer(ref Descriptor desc, ref string input)
        {
            if (desc.Player.Level >= Constants.ImmLevel)
            {
                var elements = TokeniseInput(ref input);
                if(elements.Length < 2)
                {
                    desc.Send($"Usage: removespell <target> <spellname>{Constants.NewLine}");
                    return;
                }
                var target = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, elements[1], RegexOptions.IgnoreCase).Success).FirstOrDefault();
                if (target != null)
                {
                    if(target.Player.Level >= desc.Player.Level && target.Player.Name != desc.Player.Name)
                    {
                        desc.Send($"You cannot set the spells of a higher level character!{Constants.NewLine}");
                    }
                    else
                    {
                        var spell = input.Replace(GetVerb(ref input), string.Empty).Replace(elements[1], string.Empty).Trim();
                        if(Spells.SpellExists(spell))
                        {
                            if(target.Player.HasSpell(spell))
                            {
                                target.Player.RemoveSpell(spell);
                                desc.Send($"You have removed {target.Player.Name}'s knowledge of the spell {spell}!{Constants.NewLine}");
                                target.Send($"{desc.Player.Name} has removed your knowledge of the spell {spell}!{Constants.NewLine}");
                                Game.LogMessage($"INFO: {desc.Player.Name} has removed the spell '{spell}' from player {target.Player.Name}", LogLevel.Info, true);
                            }   
                            else
                            {
                                desc.Send($"{target.Player.Name} does not know that spell!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"That spell does not exist!{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"That person doesn't seem to be here...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
            }
        }

        private static void GetCurrentConnections(ref Descriptor desc)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  {new string('=', 77)}");
                int i = 0;
                foreach(var con in SessionManager.Instance.Descriptors)
                {
                    sb.AppendLine($"|| Endpoint: {con.Client.Client.RemoteEndPoint}");
                    sb.AppendLine($"|| Connect Time: {con.ConnectionTime} UTC");
                    sb.AppendLine($"|| Player: {(con.Player == null ? "No Player" : con.Player.Name)}");
                    i++;
                    if(i < SessionManager.Instance.Descriptors.Count)
                    {
                        sb.AppendLine($"||{new string('=', 77)}");
                    }
                }
                sb.AppendLine($"  {new string('=', 77)}");
                desc.Send(sb.ToString());
                Game.LogMessage($"INFO: Player {desc.Player} queried current connections to the game", LogLevel.Info, true);
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
                Game.LogMessage($"WARN: Player {desc.Player} attempted to query current connections to the game", LogLevel.Warning, true);
            }
        }

        private static void ShowLastBackup(ref Descriptor desc)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                if (Game.GetBackupInfo(out DateTime bkTime, out uint bkTick))
                {
                    var nxtBackup = bkTime.AddSeconds(bkTick);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"  {new string('=', 77)}");
                    sb.AppendLine($"|| Last Backup: {bkTime:dd-MM-yyyy HH:mm:ss} UTC");
                    sb.AppendLine($"|| Next Backup: {nxtBackup:dd-MM-yyyy HH:mm:ss} UTC");
                    sb.AppendLine($"  {new string('=', 77)}");
                    desc.Send(sb.ToString());
                }
                else
                {
                    desc.Send($"The World and Player databases have not been backed up this session{Constants.NewLine}");
                    desc.Send($"The first backup this session is due at approx: {Game.GetStartTime().AddSeconds(bkTick):yyyy-MM-dd HH:mm:ss} UTC");
                }
            }
            else
            {
                desc.Send($"Only the Gods may look into the past!{Constants.NewLine}");
            }
        }

        private static void DoImmInv(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                // imminv <npc | player> <target>
                //var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var elements = TokeniseInput(ref input);
                if(elements.Length < 3)
                {
                    desc.Send($"Usage: imminv <npc | player> <target>{Constants.NewLine}");
                    return;
                }
                var target = elements.Last().Trim();
                if(!string.IsNullOrEmpty(target))
                {
                    if (elements[1].ToLower() == "npc")
                    {
                        var objNpc = GetTargetNPC(ref desc, target);
                        if (objNpc != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"  {new string('=', 77)}");
                            if (objNpc.Inventory != null && objNpc.Inventory.Count > 0)
                            {
                                sb.AppendLine($"|| {objNpc.Name} is carrying:");
                                foreach (var i in objNpc.Inventory.Select(x => new { x.Id, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                                {
                                    var cnt = objNpc.Inventory.Where(y => y.Id == i.Id).Count();
                                    sb.AppendLine($"|| {cnt} x {i.Name}, {i.ShortDescription} ({i.Id})");
                                }
                                sb.AppendLine($"  {new string('=', 77)}");
                                desc.Send(sb.ToString());
                            }
                            else
                            {
                                desc.Send($"{objNpc.Name} is not carrying any items!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"You can't see anything like that here!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        var objTgt = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                        if (objTgt != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"  {new string('=', 77)}");
                            if (objTgt.Player.Inventory != null && objTgt.Player.Inventory.Count > 0)
                            {
                                sb.AppendLine($"|| {objTgt.Player.Name} is carrying:");
                                foreach (var i in objTgt.Player.Inventory.Select(x => new { x.Id, x.Name, x.ShortDescription }).Distinct().OrderBy(j => j.Name))
                                {
                                    var cnt = objTgt.Player.Inventory.Where(y => y.Id == i.Id).Count();
                                    sb.AppendLine($"|| {cnt} x {i.Name}, {i.ShortDescription} ({i.Id})");
                                }
                                sb.AppendLine($"  {new string('=', 77)}");
                                desc.Send(sb.ToString());
                            }
                            else
                            {
                                desc.Send($"{objTgt.Player.Name} is not carrying any items!{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            desc.Send($"No player with that name could be found.{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"Look for who, exactly?{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ShowUptimeInfo(ref Descriptor desc)
        {
            if (desc.Player.Level >= Constants.ImmLevel)
            {
                var startTime = Game.GetStartTime();
                var uptime = DateTime.UtcNow - startTime;
                Game.LogMessage($"INFO: Player {desc.Player.Name} queried uptime", LogLevel.Info, true);
                desc.Send($"The world was started at {startTime:HH:mm:ss} UTC on {startTime:dd/MM/yyyy}{Constants.NewLine}");
                desc.Send($"The world has been online for {uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes and {uptime.Seconds} seconds{Constants.NewLine}");
            }
            else
            {
                desc.Send($"Only the Gods can know how long the world has truly existed!{Constants.NewLine}");
            }
        }

        private static void DoWhereCheck(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                // usage: where <item | npc | node> <search string>
                var elements = TokeniseInput(ref input);
                if (elements.Length < 2 || (elements[1].ToLower() != "item" && elements[1].ToLower() != "npc" && elements[1].ToLower() != "node"))
                {
                    desc.Send($"Usage: where <item | npc | node> <search string>{Constants.NewLine}");
                    return;
                }
                if (elements[1].ToLower() == "npc")
                {
                    var target = input.Replace(elements[0], string.Empty).Replace(elements[1], string.Empty).Trim();
                    var result = NPCManager.Instance.GetAllNPCIDS().Values.Where(x => Regex.Match(x.Name, target, RegexOptions.IgnoreCase).Success).ToList();
                    if (result != null && result.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"  {new string('=', 77)}");
                        foreach (var r in result)
                        {
                            sb.AppendLine($"|| {r.NPCID} - {r.Name} in room {r.CurrentRoom}");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"Nothing like that appears in the world...{Constants.NewLine}");
                    }
                    return;
                }
                if (elements[1].ToLower() == "item")
                {
                    var target = input.Replace(elements[0], string.Empty).Replace(elements[1], string.Empty).Trim();
                    var result = RoomManager.Instance.GetAllRooms().Values.Where(x => x.ItemsInRoom.Count > 0).ToList();
                    uint matchingItems = 0;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"  {new string('=', 77)}");
                    foreach (var (r, i) in from r in result
                                           from i in
                                               from i in r.ItemsInRoom
                                               where Regex.Match(i.Name, target, RegexOptions.IgnoreCase).Success
                                               select i
                                           select (r, i))
                    {
                        sb.AppendLine($"|| {i.Id} - {i.Name} in room {r.RoomID}");
                        matchingItems++;
                    }

                    sb.AppendLine($"  {new string('=', 77)}");
                    if (matchingItems > 0)
                    {
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"Nothing like that appears in the world...{Constants.NewLine}");
                    }
                    return;
                }
                if (elements[1].ToLower() == "node")
                {
                    var target = input.Replace(elements[0], string.Empty).Replace(elements[1], string.Empty).Trim();
                    var result = RoomManager.Instance.GetAllRooms().Values.Where(x => x.ResourceNode != null && Regex.Match(x.ResourceNode.NodeName, target, RegexOptions.IgnoreCase).Success).ToList();
                    if(result != null && result.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"  {new string('=', 77)}");
                        foreach(var r in result)
                        {
                            sb.AppendLine($"|| {r.ResourceNode.NodeName} in Room {r.RoomID}");
                        }
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        desc.Send($"You cannot sense any such Nodes in the world right now.{Constants.NewLine}");
                    }
                    return;
                }
                desc.Send($"Usage: where <item | npc | node> <search string>{Constants.NewLine}");
                return;
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ImmSight(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                if (!string.IsNullOrEmpty(target))
                {
                    StringBuilder sb = new StringBuilder();
                    var p = SessionManager.Instance.GetAllPlayers().Where(x => Regex.Match(x.Player.Name, target, RegexOptions.IgnoreCase).Success).FirstOrDefault();
                    if (p != null)
                    {
                        sb.AppendLine($"  {new string('=', 77)}");
                        sb.AppendLine($"|| Name: {p.Player.Name}{Constants.TabStop}{Constants.TabStop}Gender: {p.Player.Gender}{Constants.TabStop}Class: {p.Player.Class}{Constants.TabStop}Race: {p.Player.Race}");
                        sb.AppendLine($"|| Level: {p.Player.Level}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}Exp: {p.Player.Stats.Exp}{Constants.TabStop}Next: {LevelTable.GetExpForNextLevel(p.Player.Level, p.Player.Stats.Exp)}");
                        sb.AppendLine($"|| Alignment: {p.Player.Alignment}{Constants.TabStop}{Constants.TabStop}Gold: {p.Player.Stats.Gold}");
                        sb.AppendLine($"||");
                        sb.AppendLine($"|| Stats:");
                        sb.AppendLine($"|| Strength: {p.Player.Stats.Strength} ({ActorStats.CalculateAbilityModifier(p.Player.Stats.Strength)}){Constants.TabStop}{Constants.TabStop}Dexterity: {p.Player.Stats.Dexterity} ({ActorStats.CalculateAbilityModifier(p.Player.Stats.Dexterity)})");
                        sb.AppendLine($"|| Constitution: {p.Player.Stats.Constitution} ({ActorStats.CalculateAbilityModifier(p.Player.Stats.Constitution)}){Constants.TabStop}{Constants.TabStop}Intelligence: {p.Player.Stats.Intelligence} ({ActorStats.CalculateAbilityModifier(p.Player.Stats.Intelligence)})");
                        sb.AppendLine($"|| Wisdom: {p.Player.Stats.Wisdom} ({ActorStats.CalculateAbilityModifier(p.Player.Stats.Wisdom)}){Constants.TabStop} {Constants.TabStop}Charisma: {p.Player.Stats.Charisma} ({ActorStats.CalculateAbilityModifier(p.Player.Stats.Charisma)})");
                        sb.AppendLine($"|| Current HP: {p.Player.Stats.CurrentHP}{Constants.TabStop}{Constants.TabStop}Max HP: {p.Player.Stats.MaxHP}");
                        sb.AppendLine($"|| Current MP: {p.Player.Stats.CurrentMP}{Constants.TabStop}{Constants.TabStop}Max MP: {p.Player.Stats.MaxMP}");
                        sb.AppendLine($"|| Armour Class: {p.Player.Stats.ArmourClass}{Constants.TabStop}No. Of Attacks: {p.Player.NumberOfAttacks}");
                        sb.AppendLine($"  {new string('=', 77)}");
                        desc.Send(sb.ToString());
                    }
                    else
                    {
                        var n = GetTargetNPC(ref desc, target);
                        if (n != null)
                        {
                            sb.AppendLine($"  {new string('=', 77)}");
                            sb.AppendLine($"|| Name: {n.Name}{Constants.TabStop}GUID: {n.NPCGuid}");
                            sb.AppendLine($"|| Template: {n.NPCID}");
                            sb.AppendLine($"||");
                            sb.AppendLine($"|| Stats:");
                            sb.AppendLine($"|| Strength: {n.Stats.Strength} ({ActorStats.CalculateAbilityModifier(n.Stats.Strength)}){Constants.TabStop}{Constants.TabStop}Dexterity: {n.Stats.Dexterity} ({ActorStats.CalculateAbilityModifier(n.Stats.Dexterity)})");
                            sb.AppendLine($"|| Constitution: {n.Stats.Constitution} ({ActorStats.CalculateAbilityModifier(n.Stats.Constitution)}){Constants.TabStop}{Constants.TabStop}Intelligence: {n.Stats.Intelligence} ({ActorStats.CalculateAbilityModifier(n.Stats.Intelligence)})");
                            sb.AppendLine($"|| Wisdom: {n.Stats.Wisdom} ({ActorStats.CalculateAbilityModifier(n.Stats.Wisdom)}){Constants.TabStop} {Constants.TabStop}Charisma: {n.Stats.Charisma} ({ActorStats.CalculateAbilityModifier(n.Stats.Charisma)})");
                            sb.AppendLine($"|| Hit Dice: {n.NumberOfHitDice}d{n.SizeOfHitDice}");
                            sb.AppendLine($"|| Current HP: {n.Stats.CurrentHP}{Constants.TabStop}{Constants.TabStop}Max HP: {n.Stats.MaxHP}");
                            sb.AppendLine($"|| Current MP: {n.Stats.CurrentMP}{Constants.TabStop}{Constants.TabStop}Max MP: {n.Stats.MaxMP}");
                            sb.AppendLine($"|| Armour Class: {n.Stats.ArmourClass}{Constants.TabStop}Exp: {n.BaseExpAward}{Constants.TabStop}No. Of Attacks: {n.NumberOfAttacks}");
                            sb.AppendLine($"  {new string('=', 77)}");
                            desc.Send(sb.ToString());
                        }
                        else
                        {
                            desc.Send($"Even with the sight of Gods, you cannot see that...{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"Use ImmSight on what, exactly?{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void GetObjectList(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                // list <item | room | zone | npc | skills | spells | node | recipe> <id | string>
                var line = input.Replace(GetVerb(ref input), string.Empty).Trim();
                var objectType = TokeniseInput(ref line).FirstOrDefault();
                var target = string.IsNullOrEmpty(objectType) ? string.Empty : line.Replace(objectType, string.Empty).Trim();
                if (!string.IsNullOrEmpty(objectType))
                {
                    bool targetIsID = uint.TryParse(target, out var targetID);
                    StringBuilder sb = new StringBuilder();
                    if (targetIsID)
                    {
                        switch (objectType.ToLower())
                        {
                            case "item":
                                var i = ItemManager.Instance.GetItemByID(targetID);
                                if (i != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var p in i.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {p.Name}: {p.GetValue(i, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No such Item could be found{Constants.NewLine}");
                                }
                                break;

                            case "room":
                                var r = RoomManager.Instance.GetRoom(targetID);
                                if (r != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var p in r.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {p.Name}: {p.GetValue(r, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No such Room could be found{Constants.NewLine}");
                                }
                                break;

                            case "zone":
                                var z = ZoneManager.Instance.GetZone(targetID);
                                if (z != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var p in z.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {p.Name}: {p.GetValue(z, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No such Zone could be found{Constants.NewLine}");
                                }
                                break;

                            case "npc":
                                var n = NPCManager.Instance.GetNPCByID(targetID);
                                if (n != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var p in n.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {p.Name}: {p.GetValue(n, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No such NPC could be found{Constants.NewLine}");
                                }
                                break;

                            case "emote":
                                var emote = EmoteManager.Instance.GetEmoteByID(targetID);
                                if (emote != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var e in emote.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {e.Name}: {e.GetValue(emote, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No Emote with that ID could be found{Constants.NewLine}");
                                }
                                break;

                            case "node":
                                var node = NodeManager.Instance.GetNodeByID(targetID);
                                if(node != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var ni in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {ni.Name}: {ni.GetValue(node, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No Resource Node with that ID could be found{Constants.NewLine}");
                                }
                                break;

                            case "recipe":
                                var recipe = RecipeManager.Instance.GetRecipe(targetID);
                                if (recipe != null)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var ri in recipe.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                    {
                                        sb.AppendLine($"|| {ri.Name}: {ri.GetValue(recipe, null)}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No Recipe with that ID could be found{Constants.NewLine}");
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (objectType.ToLower())
                        {
                            case "item":
                                var matchingItems = ItemManager.Instance.GetItemByNameOrDescription(target).OrderBy(x => x.Id).ToList();
                                if (matchingItems != null && matchingItems.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var i in matchingItems)
                                    {
                                        sb.AppendLine($"|| {i.Id} - {i.Name}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Items could be found{Constants.NewLine}");
                                }
                                break;

                            case "room":
                                var matchingRooms = RoomManager.Instance.GetRoomByNameOrDescription(target).OrderBy(x => x.RoomID).ToList();
                                if (matchingRooms != null && matchingRooms.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var i in matchingRooms)
                                    {
                                        sb.AppendLine($"|| {i.RoomID} - {i.RoomName}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Rooms could be found{Constants.NewLine}");
                                }
                                break;

                            case "zone":
                                var matchingZones = ZoneManager.Instance.GetZoneByName(target).OrderBy(x => x.ZoneID).ToList();
                                if (matchingZones != null && matchingZones.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var i in matchingZones)
                                    {
                                        sb.AppendLine($"|| {i.ZoneID} - {i.ZoneName}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Zones could be found{Constants.NewLine}");
                                }
                                break;

                            case "npc":
                                var matchingNPCs = NPCManager.Instance.GetNPCByNameOrDescription(target).OrderBy(x => x.NPCID).ToList();
                                if (matchingNPCs != null && matchingNPCs.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var i in matchingNPCs)
                                    {
                                        sb.AppendLine($"|| {i.NPCID} - {i.Name}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching NPCs could be found{Constants.NewLine}");
                                }
                                break;

                            case "shop":
                                var matchingShops = ShopManager.Instance.GetShopsByName(target).OrderBy(x => x.ShopID).ToList();
                                if (matchingShops != null && matchingShops.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var i in matchingShops)
                                    {
                                        sb.AppendLine($"|| ID: {i.ShopID}{Constants.TabStop}Name: {i.ShopName}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No mathcing Shops could be found.{Constants.NewLine}");
                                }
                                break;

                            case "emote":
                                var matchingEmotes = EmoteManager.Instance.GetAllEmotes(target).OrderBy(x => x.EmoteName).ToList();
                                if (matchingEmotes != null && matchingEmotes.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var e in matchingEmotes)
                                    {
                                        sb.AppendLine($"|| {e.EmoteID} - {e.EmoteName}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Emotes could be found{Constants.NewLine}");
                                }
                                break;

                            case "skills":
                            case "skill":
                                var skills = Skills.GetAllSkills(target).OrderBy(x => x.Name).ToList();
                                if(skills != null && skills.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var skill in skills)
                                    {
                                        sb.AppendLine($"|| {skill.Name} - {skill.Description}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Skill could be found.{Constants.NewLine}");
                                }
                                break;

                            case "spells":
                            case "spell":
                                var matchingSpells = Spells.GetAllSpells(target).OrderBy(x => x.SpellName).ToList();
                                if(matchingSpells != null && matchingSpells.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach (var s in matchingSpells)
                                    {
                                        sb.AppendLine($"|| {s.SpellName} - {s.Description}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Spell could be found.{Constants.NewLine}");
                                }
                                break;

                            case "node":
                                var matchingNodes = NodeManager.Instance.GetAllResourceNodes(target).OrderBy(x => x.NodeName).ToList();
                                if(matchingNodes != null && matchingNodes.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach(var n in matchingNodes)
                                    {
                                        sb.AppendLine($"|| ID: {n.Id}{Constants.TabStop}Name: {n.NodeName}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Resource Node could be found{Constants.NewLine}");
                                }
                                break;

                            case "recipe":
                                var matchingRecipes = RecipeManager.Instance.GetAllCraftingRecipes(target).OrderBy(x => x.RecipieName).ToList();
                                if(matchingRecipes != null  && matchingRecipes.Count > 0)
                                {
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    foreach(var r in matchingRecipes)
                                    {
                                        sb.AppendLine($"|| ID: {r.RecipieID}{Constants.TabStop}Name: {r.RecipieName}");
                                    }
                                    sb.AppendLine($"  {new string('=', 77)}");
                                    desc.Send(sb.ToString());
                                }
                                else
                                {
                                    desc.Send($"No matching Crafting Recipe could be found.{Constants.NewLine}");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    desc.Send($"List what, exactly?{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void Purge(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var target = input.Replace(GetVerb(ref input), string.Empty).Trim();
                string msgToPlayer = string.Empty;
                string[] msgToOthers = new string[] { string.Empty, string.Empty };
                if (!string.IsNullOrEmpty(target))
                {
                    bool targetFound = false;
                    var t = GetTargetItem(ref desc, target, false);
                    if (t != null)
                    {
                        targetFound = true;
                        RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom.Remove(t);
                        msgToPlayer = $"With arcane fire you burn {t.Name} from the fabric of the world!{Constants.NewLine}";
                        msgToOthers[0] = $"{desc.Player.Name} makes occult gestures and {t.Name} is consumed by arcane fire!{Constants.NewLine}";
                        msgToOthers[1] = $"The Winds of Magic shift and {t.Name} is burned from the world by arcane fire!{Constants.NewLine}";
                    }
                    else
                    {
                        var n = GetTargetNPC(ref desc, target);
                        if (n != null)
                        {
                            targetFound = true;
                            n.Kill(false);
                            msgToPlayer = $"With arcane fire you burn {n.Name} from the fabric of the world!{Constants.NewLine}";
                            msgToOthers[0] = $"{desc.Player.Name} makes occult gestures and {n.Name} is consumed by arcane fire!{Constants.NewLine}";
                            msgToOthers[1] = $"The Winds of Magic shift and {n.Name} is burned from the world by arcane fire!{Constants.NewLine}";
                        }
                    }
                    if (targetFound)
                    {
                        desc.Send(msgToPlayer);
                        var playersInRoom = RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).PlayersInRoom(desc.Player.CurrentRoom);
                        if (playersInRoom != null && playersInRoom.Count > 1)
                        {
                            foreach (var p in playersInRoom)
                            {
                                if (!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                                {
                                    if (p.Player.Level >= Constants.ImmLevel || desc.Player.Visible)
                                    {
                                        p.Send(msgToOthers[0]);
                                    }
                                    else
                                    {
                                        p.Send(msgToOthers[1]);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"That doesn't seem to exist...{Constants.NewLine}");
                    }
                }
                else
                {
                    var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                    if (localPlayers != null && localPlayers.Count > 1)
                    {
                        foreach (var p in localPlayers)
                        {
                            if (p.Player.Name != desc.Player.Name)
                            {
                                var msg = desc.Player.Visible || p.Player.Level >= Constants.ImmLevel ? $"{desc.Player.Name} purges the area with holy fire!{Constants.NewLine}"
                                    : $"The Winds of Magic shift and the area is purged with holy fire!{Constants.NewLine}";
                                p.Send(msg);
                            }
                        }
                    }
                    if (RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom != null && RoomManager.Instance.GetRoom(desc.Player.CurrentRoom).ItemsInRoom.Count > 0)
                    {
                        RoomManager.Instance.ClearRoomInventory(desc.Player.CurrentRoom);
                    }
                    if (NPCManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom) != null && NPCManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom).Count > 0)
                    {
                        while (NPCManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom).Count > 0)
                        {
                            var n = NPCManager.Instance.GetNPCsInRoom(desc.Player.CurrentRoom)[0];
                            if(n.FollowingPlayer == Guid.Empty)
                            {
                                NPCManager.Instance.RemoveNPCFromWorld(n.NPCGuid, n, desc.Player.CurrentRoom);
                            }
                        }
                    }
                    desc.Send($"Calling on the Winds of Magic, you purge the area with holy fire!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ImmInvis(ref Descriptor desc)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var playersToNotify = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                string toPlayer = desc.Player.Visible ? $"You shimmer and fade from view...{Constants.NewLine}" :
                    $"You burst into full view with a blast of light!{Constants.NewLine}";
                desc.Send(toPlayer);
                if (playersToNotify != null && playersToNotify.Count > 1)
                {
                    foreach (var p in playersToNotify)
                    {
                        if (p.Player.Name != desc.Player.Name)
                        {
                            string msgToPlayer = desc.Player.Visible ? $"{desc.Player.Name} shimmers and fades away...{Constants.NewLine}" :
                                $"With a blast of light {desc.Player.Name} becomes visible again!{Constants.NewLine}";
                            p.Send(msgToPlayer);
                        }
                    }
                }
                desc.Player.Visible = !desc.Player.Visible;
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ImmSetStat(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var inputElements = TokeniseInput(ref input);
                if (inputElements.Length != 4)
                {
                    desc.Send($"Usage: set <target> <attribute> <value>{Constants.NewLine}");
                    return;
                }
                var targetPlayer = SessionManager.Instance.GetPlayer(inputElements[1].Trim());
                uint value = 0;
                string msgToPlayer = string.Empty;
                string msgToTarget = string.Empty;
                if (inputElements.Length == 4)
                {
                    if (targetPlayer != null)
                    {
                        if (targetPlayer.Player.Level < desc.Player.Level || targetPlayer.Player.Name == desc.Player.Name)
                        {
                            switch (inputElements[2].Trim().ToLower())
                            {
                                case "hp":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.CurrentHP = (int)value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s current HP to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your current HP to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s HP to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for HP{Constants.NewLine}");
                                    }
                                    break;

                                case "maxhp":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.MaxHP = value;
                                        targetPlayer.Player.Stats.CurrentMaxHP = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s max HP to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your max HP to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s Max HP to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for max HP{Constants.NewLine}");
                                    }
                                    break;

                                case "mp":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.CurrentMP = (int)value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s current MP to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your current MP to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s MP to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for MP{Constants.NewLine}");
                                    }
                                    break;

                                case "maxmp":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.MaxMP = value;
                                        targetPlayer.Player.Stats.CurrentMaxMP = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s max MP to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your max MP to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s max MP to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for max MP{Constants.NewLine}");
                                    }
                                    break;

                                case "strength":
                                case "str":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Strength = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s strength to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your strengh to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s strength to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for strength{Constants.NewLine}");
                                    }
                                    break;

                                case "dexterity":
                                case "dex":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Dexterity = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s dexterity to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your dexterity to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s dexterity to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for dexterity{Constants.NewLine}");
                                    }
                                    break;

                                case "constitution":
                                case "con":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Constitution = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s constitution to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your constitution to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s constitution to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for constitution{Constants.NewLine}");
                                    }
                                    break;

                                case "intelligence":
                                case "int":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Intelligence = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s intelligence to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your intelligence to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s intelligence to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for intelligence{Constants.NewLine}");
                                    }
                                    break;

                                case "wisdom":
                                case "wis":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Wisdom = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s wisdom to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your wisdom to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s wisdom to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for wisdom{Constants.NewLine}");
                                    }
                                    break;

                                case "charisma":
                                case "cha":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Charisma = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s charisma to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your charisma to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s charisma to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for charisma{Constants.NewLine}");
                                    }
                                    break;

                                case "gold":
                                case "gp":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Gold = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s gold balance to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your gold balance to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s gold balance to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for gold coins{Constants.NewLine}");
                                    }
                                    break;

                                case "level":
                                case "lvl":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        if (value >= desc.Player.Level)
                                        {
                                            // can't set someone to be a higher level than you
                                            msgToPlayer = $"You cannot make someone more powerful than you!{Constants.NewLine}";
                                        }
                                        else
                                        {
                                            targetPlayer.Player.Level = value;
                                            msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s level to {value}{Constants.NewLine}";
                                            if (value >= Constants.ImmLevel)
                                            {
                                                msgToTarget = $"{desc.Player.Name} has changed your level to {value}!{Constants.NewLine}You now have access to God Powers, please see an Immortal for advice on using them!{Constants.NewLine}";
                                            }
                                            else
                                            {
                                                msgToTarget = $"{desc.Player.Name} has changed your level to {value}!{Constants.NewLine}";
                                            }
                                            Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s level to {value}", LogLevel.Info, true);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for gold coins{Constants.NewLine}");
                                    }
                                    break;

                                case "exp":
                                case "xp":
                                case "experience":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.Exp = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s experience points to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your experience points to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s experience points to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for experience points{Constants.NewLine}");
                                    }
                                    break;

                                case "armourclass":
                                case "ac":
                                    if (uint.TryParse(inputElements[3], out value))
                                    {
                                        targetPlayer.Player.Stats.ArmourClass = value;
                                        msgToPlayer = $"You have changed {targetPlayer.Player.Name}'s armour class to {value}{Constants.NewLine}";
                                        msgToTarget = $"{desc.Player.Name} has changed your armour class to {value}!{Constants.NewLine}";
                                        Game.LogMessage($"INFO: {desc.Player.Name} set {targetPlayer.Player.Name}'s armour class to {value}", LogLevel.Info, true);
                                    }
                                    else
                                    {
                                        desc.Send($"That is not a valid value for armour class{Constants.NewLine}");
                                    }
                                    break;

                                default:
                                    desc.Send($"{inputElements[2]} doesn't look like something you can change...{Constants.NewLine}");
                                    break;
                            }
                            if (!string.IsNullOrEmpty(msgToPlayer))
                            {
                                desc.Send(msgToPlayer);
                            }
                            if (!string.IsNullOrEmpty(msgToTarget))
                            {
                                targetPlayer.Send(msgToTarget);
                            }
                        }
                        else
                        {
                            desc.Send($"You don't have the power to change {targetPlayer.Player.Name}'s stats!{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"That person doesn't seem to be in the realms at the moment{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Set what to what on who now?{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static bool SaveAllPlayers(ref Descriptor desc)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                bool OK = true;
                var connectedPlayers = SessionManager.Instance.GetAllPlayers().Where(x => x.IsConnected).ToList();
                if (connectedPlayers != null && connectedPlayers.Count > 0)
                {
                    foreach (var player in connectedPlayers)
                    {
                        var p = player;
                        if (DatabaseManager.SavePlayerNew(ref p, false))
                        {
                            Game.LogMessage($"INFO: SaveAllPlayers called by {desc.Player.Name}: Successfully saved player {p.Player.Name}", LogLevel.Info, true);
                        }
                        else
                        {
                            OK = false;
                            Game.LogMessage($"ERROR: SaveAllPlayers called by {desc.Player.Name}: Failed to save player {p.Player.Name}", LogLevel.Error, true);
                        }
                    }
                    return OK;
                }
                else
                {
                    Game.LogMessage($"INFO: SaveAllPlayers called by {desc.Player.Name}: No players to save.", LogLevel.Info, true);
                    desc.Send($"No connected players to save{Constants.NewLine}");
                    return OK;
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
                return false;
            }
        }

        private static void ImmSummonPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var inputElements = TokeniseInput(ref input);
                string target = inputElements.Length > 1 ? inputElements[1] : string.Empty;
                if (!string.IsNullOrEmpty(target))
                {
                    var p = SessionManager.Instance.GetPlayer(target);
                    if (p != null)
                    {
                        if (p.Player.Level <= desc.Player.Level)
                        {
                            var msg = desc.Player.Visible ? $"{Constants.NewLine}The universe swirls as {desc.Player.Name} yanks you through the fabric of reality...{Constants.NewLine}" :
                                $"{Constants.NewLine}The universe swirls as something yanks you through the fabric of reality...{Constants.NewLine}";
                            p.Send(msg);
                            p.Player.Move(p.Player.CurrentRoom, desc.Player.CurrentRoom, true, ref p);
                        }
                        else
                        {
                            desc.Send($"You are not powerful enough to summon {p.Player.Name}...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"{target} doesn't seem to be in the world at the moment.{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"You can't summon that!{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods may do that!{Constants.NewLine}");
            }
        }

        private static void ImmTeleportToPlayer(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var inputElements = TokeniseInput(ref input);
                string target = inputElements.Length > 1 ? inputElements[1] : string.Empty;
                if (!string.IsNullOrEmpty(target))
                {
                    if (uint.TryParse(target, out uint targetRID))
                    {
                        // target is a number, so assume its a RID and try to move there, but check the RID exists first
                        if (RoomManager.Instance.RoomExists(targetRID))
                        {
                            var currentRID = desc.Player.CurrentRoom;
                            desc.Player.Move(currentRID, targetRID, true, ref desc);
                        }
                        else
                        {
                            desc.Send($"Sorry, but that doesn't seem to exist...{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        // target is a string or otherwise not a valid RID, so assume we're porting to a player's location
                        var p = SessionManager.Instance.GetPlayer(target);
                        if (p != null)
                        {
                            desc.Player.Move(desc.Player.CurrentRoom, p.Player.CurrentRoom, true, ref desc);
                        }
                        else
                        {
                            desc.Send($"{target} doesn't seem to be in the world at the moment.{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"Teleport to what, exactly...?{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void Slay(ref Descriptor desc, ref string line)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var inputElements = TokeniseInput(ref line);
                string target = inputElements.Length > 1 ? inputElements[1].Trim() : string.Empty;
                if (!string.IsNullOrEmpty(target))
                {
                    var targetPlayer = SessionManager.Instance.GetPlayer(target);
                    if (targetPlayer != null)
                    {
                        if (targetPlayer.Player.Name == desc.Player.Name)
                        {
                            // player is trying to SLAY themselves...
                            desc.Send($"Suicide isn't the answer...{Constants.NewLine}");
                        }
                        else
                        {
                            if (targetPlayer.Player.Level <= desc.Player.Level)
                            {
                                var playersInTargetRoom = RoomManager.Instance.GetPlayersInRoom(targetPlayer.Player.CurrentRoom);
                                if (targetPlayer.Player.CurrentRoom == desc.Player.CurrentRoom)
                                {
                                    if (playersInTargetRoom != null && playersInTargetRoom.Count > 2)
                                    {
                                        // killer and target are in the same room with other players present, so notify them of the killing
                                        foreach (var p in playersInTargetRoom)
                                        {
                                            if (p.Player.Name.ToLower() != targetPlayer.Player.Name.ToLower() && p.Player.Name.ToLower() != desc.Player.Name.ToLower())
                                            {
                                                string deathMessage = string.Empty;
                                                if (p.Player.Level >= Constants.ImmLevel)
                                                {
                                                    // player being notified is an Immortal so can see everything
                                                    deathMessage = $"{desc.Player.Name} makes an arcane gesture, forcing {targetPlayer.Player.Name}'s life from their body...{Constants.NewLine}";
                                                }
                                                else
                                                {
                                                    // player being notified is not an Immortal so check visibility of killer and target
                                                    if (desc.Player.Visible)
                                                    {
                                                        // killer visible
                                                        if (targetPlayer.Player.Visible)
                                                        {
                                                            // killer and target visible
                                                            deathMessage = $"{desc.Player.Name} makes an arcane gesture, forcing {targetPlayer.Player.Name}'s life from their body...{Constants.NewLine}";
                                                        }
                                                        else
                                                        {
                                                            // killer visible, target invisible
                                                            deathMessage = $"{desc.Player.Name} makes an arcane gesture, forcing something to die horribly...{Constants.NewLine}";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // killer invisible
                                                        if (targetPlayer.Player.Visible)
                                                        {
                                                            // killer invisible, target visible
                                                            deathMessage = $"You feel a sudden chill as {targetPlayer.Player.Name}'s life is forced from their body...{Constants.NewLine}";
                                                        }
                                                        else
                                                        {
                                                            // killer and target invisible
                                                            deathMessage = $"There is a strange sound as something dies and is taken by the Winds of Magic...{Constants.NewLine}";
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(deathMessage))
                                                {
                                                    p.Send(deathMessage);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var playersInSlayerRoom = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                                    if (playersInSlayerRoom != null && playersInSlayerRoom.Count > 1)
                                    {
                                        // killer and target are in different rooms, killer has other players present so notify them of something strange...
                                        foreach (var p in playersInSlayerRoom)
                                        {
                                            if (p.Player.Name.ToLower() != desc.Player.Name.ToLower())
                                            {
                                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                                {
                                                    p.Send($"{desc.Player.Name} makes an arcane gesture and the world feels colder...{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    p.Send($"The Winds of Magic swirl and the world feels colder...{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                    if (playersInTargetRoom != null && playersInTargetRoom.Count > 1)
                                    {
                                        // there are other players around the target so let them know what's happened...
                                        foreach (var p in playersInTargetRoom)
                                        {
                                            if (p.Player.Name.ToLower() != targetPlayer.Player.Name.ToLower())
                                            {
                                                if (targetPlayer.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                                {
                                                    p.Send($"{targetPlayer.Player.Name} suddenly falls down dead!{Constants.NewLine}");
                                                }
                                                else
                                                {
                                                    p.Send($"There is a strange sound as something dies and is taken by the Winds of Magic...{Constants.NewLine}");
                                                }
                                            }
                                        }
                                    }
                                }
                                targetPlayer.Send($"You feel the gaze of {desc.Player.Name} upon you as your life ebbs away...{Constants.NewLine}");
                                targetPlayer.Player.Stats.CurrentHP = 0;
                                targetPlayer.Player.Kill(ref targetPlayer);
                            }
                            else
                            {
                                desc.Send($"That doesn't seem like a good idea...{Constants.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        desc.Send($"The target of your wrath does not seem to exist...{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"The target of your wrath does not seem to exist...{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ShutdownWorld(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                if (SaveAllPlayers(ref desc))
                {
                    Game.LogMessage($"SHUTDOWN: {desc.Player.Name} has shut down the game world, all players were successfully saved.", LogLevel.Info, true);
                    Program.Stop();
                }
                else
                {
                    if (input.ToLower() == "shutdown force")
                    {
                        Game.LogMessage($"SHUTDOWN: {desc.Player.Name} has forced shutdown of the game world, players may not have been saved.", LogLevel.Warning, true);
                        Program.Stop();
                    }
                    else
                    {
                        desc.Send($"Failed to save all connected players, cannot shut down safely.{Constants.NewLine}");
                        desc.Send($"Use: shutdown force to shutdown without saving players first.{Constants.NewLine}");
                    }
                }
            }
            else
            {
                desc.Send($"Only the Gods may stop the world...{Constants.NewLine}");
            }
        }

        private static void VoiceOfGod(ref Descriptor desc, ref string line)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var verb = GetVerb(ref line).Trim();
                var msg = line.Remove(0, verb.Length).Trim();
                var toSend = $"The voice of {desc.Player.Name} echoes throughout the realms, saying \"{msg}\"{Constants.NewLine}";
                SessionManager.Instance.SendToAllClients(toSend);
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void DoForce(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                var elements = TokeniseInput(ref input);
                if (elements.Length < 3)
                {
                    desc.Send($"Usage: force <player> <command>{Constants.NewLine}");
                }
                else
                {
                    var cmd = string.Empty;
                    for (int i = 2; i < elements.Length; i++)
                    {
                        cmd = $"{cmd} {elements[i]}";
                    }
                    cmd = cmd.Trim();
                    var target = elements[1];
                    var p = SessionManager.Instance.GetPlayer(target);
                    if (p != null)
                    {
                        if (p.Player.Level >= desc.Player.Level)
                        {
                            desc.Send($"You cannot use that on someone more powerful than yourself!{Constants.NewLine}");
                        }
                        else
                        {
                            p.Send($"{desc.Player.Name}'s power overcomes you and you act against your will!{Constants.NewLine}");
                            desc.Send($"You compell {p.Player.Name} to act against their will!{Constants.NewLine}");
                            ParseCommand(ref p, cmd);
                            Game.LogMessage($"INFO: {desc.Player.Name} FORCED {p.Player.Name}: {cmd}", LogLevel.Info, true);
                        }
                    }
                    else
                    {
                        desc.Send($"That person doesn't seem to be in the world right now...{Constants.NewLine}");
                    }
                }
            }
            else
            {
                desc.Send($"Only the Gods have that power!{Constants.NewLine}");
            }
        }

        private static void ImmHeal(ref Descriptor desc, ref string input)
        {
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                string targetPlayer = TokeniseInput(ref input).LastOrDefault();
                string msgToPlayer = string.Empty;
                string msgToTarget = string.Empty;
                if (!string.IsNullOrEmpty(targetPlayer))
                {
                    var tp = SessionManager.Instance.GetPlayer(targetPlayer);
                    if (tp != null)
                    {
                        tp.Send($"You feel a swell of energy as {desc.Player.Name} restores you with holy power!{Constants.NewLine}");
                        desc.Send($"With holy power you restore {tp.Player.Name}{Constants.NewLine}");
                        SessionManager.Instance.GetPlayerByGUID(tp.Id).Player.Stats.CurrentHP = (int)tp.Player.Stats.MaxHP;
                        SessionManager.Instance.GetPlayerByGUID(tp.Id).Player.Stats.CurrentMP = (int)tp.Player.Stats.MaxMP;
                        SessionManager.Instance.GetPlayerByGUID(tp.Id).Player.Position = ActorPosition.Standing;
                        Game.LogMessage($"INFO: {desc.Player.Name} has restored {tp.Player.Name}", LogLevel.Info, true);
                    }
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }

        private static void ImmSpawnItem(ref Descriptor desc, ref string input)
        {
            // create <item | npc> <id | string>
            if(desc.Player.Level >= Constants.ImmLevel)
            {
                string msgToPlayer = string.Empty;
                bool targetCreated = false;
                string[] msgToOthers = { string.Empty, string.Empty };
                var lineElements = TokeniseInput(ref input);
                if (lineElements.Length < 3)
                {
                    desc.Send($"Proper usage: create <item | npc> <id | string>");
                    return;
                }
                var spawnObjectType = lineElements[1];
                if (spawnObjectType.ToLower() != "item" && spawnObjectType.ToLower() != "npc")
                {
                    desc.Send($"Proper usage: create <item | npc> <id | string>");
                    return;
                }
                string target = input.Replace(GetVerb(ref input), string.Empty).Replace(spawnObjectType, string.Empty).Trim();
                if (uint.TryParse(target, out uint uid))
                {
                    // creation target is a uid so attempt to spawn the relevant item or npc in the player's room
                    if (spawnObjectType.ToLower() == "item")
                    {
                        if (ItemManager.Instance.ItemExists(uid))
                        {
                            var i = ItemManager.Instance.GetItemByID(uid);
                            RoomManager.Instance.AddItemToRoomInventory(desc.Player.CurrentRoom, ref i);
                            msgToPlayer = $"With arcane power you create {i.Name}{Constants.NewLine}";
                            msgToOthers[0] = $"{desc.Player.Name} makes an arcane gesture and creates {i.Name}{Constants.NewLine}";
                            msgToOthers[1] = $"Something moves, and the shifting Winds of Magic create {i.Name}{Constants.NewLine}";
                            targetCreated = true;
                        }
                        else
                        {
                            desc.Send($"That doesn't seem to exist...{Constants.NewLine}");
                            Game.LogMessage($"WARN: {desc.Player.Name} attempted to load item {uid} but this was not found in Item Manager", LogLevel.Warning, true);
                        }

                    }
                    else
                    {
                        if (NPCManager.Instance.NPCExists(uid))
                        {
                            var n = NPCManager.Instance.GetNPCByID(uid);
                            if (n != null)
                            {
                                NPCManager.Instance.AddNPCToWorld(n.NPCID, desc.Player.CurrentRoom);
                                msgToPlayer = $"With arcane power you summon forth {n.Name}{Constants.NewLine}";
                                msgToOthers[0] = $"{desc.Player.Name} makes arcane gestures and summons forth {n.Name}{Constants.NewLine}";
                                msgToOthers[1] = $"Something moves and the shifting Winds of Magic bring forth {n.Name}{Constants.NewLine}";
                                targetCreated = true;
                            }
                            else
                            {
                                desc.Send($"Try as you might, that just doesn't work!{Constants.NewLine}");
                                Game.LogMessage($"WARN: {desc.Player.Name} attempted to load NPC {uid} but this was not found in NPC Manager", LogLevel.Warning, true);
                            }
                        }
                    }
                }
                else
                {
                    // creation target is a string so attempt to look up the relevant object and spawn it in the player's room
                    if (spawnObjectType.ToLower() == "item")
                    {
                        var i = ItemManager.Instance.GetItemByName(target);
                        if (i != null)
                        {
                            RoomManager.Instance.AddItemToRoomInventory(desc.Player.CurrentRoom, ref i);
                            msgToPlayer = $"With arcane power you create {i.Name}{Constants.NewLine}";
                            msgToOthers[0] = $"{desc.Player.Name} makes an arcane gesture and creates {i.Name}{Constants.NewLine}";
                            msgToOthers[1] = $"Something moves, and the shifting Winds of Magic create {i.Name}{Constants.NewLine}";
                            targetCreated = true;
                        }
                        else
                        {
                            desc.Send($"That doesn't seem to exist...{Constants.NewLine}");
                            Game.LogMessage($"WARN: {desc.Player.Name} attempted to load item '{target}' but this was not found in Item Manager", LogLevel.Warning, true);
                        }
                    }
                    else
                    {
                        var n = NPCManager.Instance.GetNPCByNameOrDescription(target).FirstOrDefault();
                        if (n != null)
                        {
                            NPCManager.Instance.AddNPCToWorld(n.NPCID, desc.Player.CurrentRoom);
                            msgToPlayer = $"With arcane power you summon forth {n.Name}{Constants.NewLine}";
                            msgToOthers[0] = $"{desc.Player.Name} makes arcane gestures and summons forth {n.Name}{Constants.NewLine}";
                            msgToOthers[1] = $"Something moves and the shifting Winds of Magic bring forth {n.Name}{Constants.NewLine}";
                            targetCreated = true;
                        }
                        else
                        {
                            desc.Send($"That doesn't seem to exist...{Constants.NewLine}");
                            Game.LogMessage($"WARN: {desc.Player.Name} attempted to load NPC '{target}' but this was not found in NPC Manager", LogLevel.Warning, true);
                        }
                    }
                }
                if (targetCreated)
                {
                    desc.Send(msgToPlayer);
                    if (RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom).Count > 1)
                    {
                        var localPlayers = RoomManager.Instance.GetPlayersInRoom(desc.Player.CurrentRoom);
                        foreach (var p in localPlayers)
                        {
                            if (!Regex.Match(p.Player.Name, desc.Player.Name, RegexOptions.IgnoreCase).Success)
                            {
                                if (desc.Player.Visible || p.Player.Level >= Constants.ImmLevel)
                                {
                                    p.Send(msgToOthers[0]);
                                }
                                else
                                {
                                    p.Send(msgToOthers[1]);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                desc.Send($"Only the Gods can do that!{Constants.NewLine}");
            }
        }
    }
}
