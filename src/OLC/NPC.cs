using Etrea3.Core;
using System;
using System.Linq;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateNPC(Session session)
        {
            NPC newNPC = new NPC();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"%BYT%{new string('=', 77)}%PT%");
                sb.AppendLine($"ID: {newNPC.TemplateID}{Constants.TabStop}{Constants.TabStop}Zone: {newNPC.ZoneID}");
                sb.AppendLine($"Name: {newNPC.Name}{Constants.TabStop}Level: {newNPC.Level}{Constants.TabStop}Gender: {newNPC.Gender}");
                sb.AppendLine($"Short Description: {newNPC.ShortDescription}");
                if (string.IsNullOrEmpty(newNPC.LongDescription))
                {
                    sb.AppendLine($"Long Description: None");
                }
                else
                {
                    sb.AppendLine("Long Description:");
                    foreach(var dl in newNPC.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.TabStop}{dl}");
                    }
                }
                sb.AppendLine($"STR: {newNPC.Strength}{Constants.TabStop}{Constants.TabStop}DEX: {newNPC.Dexterity}{Constants.TabStop}{Constants.TabStop}CON: {newNPC.Constitution}");
                sb.AppendLine($"INT: {newNPC.Intelligence}{Constants.TabStop}{Constants.TabStop}WIS: {newNPC.Wisdom}{Constants.TabStop}{Constants.TabStop}CHA: {newNPC.Charisma}");
                sb.AppendLine($"Hit Dice: {newNPC.NumberOfHitDice}d{newNPC.HitDieSize} (Bonus Hit Die: {newNPC.BonusHitDice})");
                sb.AppendLine($"Exp: {newNPC.ExpAward}{Constants.TabStop}Gold: {newNPC.Gold}{Constants.TabStop}Attacks: {newNPC.NumberOfAttacks}");
                sb.AppendLine($"Base Armour Class: {newNPC.BaseArmourClass} (Natural: {newNPC.NaturalArmour}){Constants.TabStop}Armour Class: {newNPC.ArmourClass}");
                sb.AppendLine($"Frequency: {newNPC.AppearanceChance}{Constants.TabStop}Max. Number: {newNPC.MaxNumberInWorld}");
                sb.AppendLine($"Flags: {newNPC.Flags}");
                sb.AppendLine($"Shop ID: {newNPC.ShopID}");
                sb.AppendLine($"Arrival Message: {newNPC.ArrivalMessage}");
                sb.AppendLine($"Departure Message: {newNPC.DepatureMessage}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}3. Set Zone{Constants.TabStop}4. Set Level");
                sb.AppendLine($"5. Set Short Description{Constants.TabStop}6. Set Long Description");
                sb.AppendLine($"7. Set Stats{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}8.Set Exp{Constants.TabStop}9. Set Gold");
                sb.AppendLine($"10. Manage Equipment{Constants.TabStop}{Constants.TabStop}11. Manage Inventory");
                sb.AppendLine($"12. Set Hit Dice{Constants.TabStop}{Constants.TabStop}13. Set Appearance Chance{Constants.TabStop}14. Set Max. Number");
                sb.AppendLine($"15. Set Gender{Constants.TabStop}16. Set Flags{Constants.TabStop}17. Set Arrival Message");
                sb.AppendLine($"18. Set Departure Message{Constants.TabStop}19. Set Attacks{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}20. Manage Spells");
                sb.AppendLine($"21. Set Shop ID{Constants.TabStop}{Constants.TabStop}22. Manage MobProgs");
                sb.AppendLine($"23. Save{Constants.TabStop}24. Return");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newNPC.TemplateID = GetValue<int>(session, "Enter ID: ");
                        break;

                    case 2:
                        newNPC.Name = GetValue<string>(session, "Enter Name: ");
                        break;

                    case 3:
                        newNPC.ZoneID = GetValue<int>(session, "Enter Zone: ");
                        break;

                    case 4:
                        newNPC.Level = GetValue<int>(session, "Enter Level: ");
                        break;

                    case 5:
                        newNPC.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 6:
                        newNPC.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 7:
                        newNPC.Strength = Math.Max(0, GetValue<int>(session, "Enter Strength: "));
                        newNPC.Dexterity = Math.Max(0, GetValue<int>(session, "Enter Dexterity: "));
                        newNPC.Constitution = Math.Max(0, GetValue<int>(session, "Enter Constitution: "));
                        newNPC.Intelligence = Math.Max(0, GetValue<int>(session, "Enter Intelligence: "));
                        newNPC.Wisdom = Math.Max(0, GetValue<int>(session, "Enter Wisdom: "));
                        newNPC.Charisma = Math.Max(0, GetValue<int>(session, "Enter Charisma: "));
                        newNPC.BaseArmourClass = Math.Max(0, GetValue<int>(session, "Enter Base Armour Class: "));
                        newNPC.NaturalArmour = GetValue<bool>(session, "Natural Armour (true/false): ");
                        break;

                    case 8:
                        newNPC.ExpAward = Math.Max(0, GetValue<int>(session, "Enter Exp Award: "));
                        break;

                    case 9:
                        newNPC.Gold = GetValue<ulong>(session, "Enter Gold: ");
                        break;

                    case 10:
                        ManageNPCEquipment(session, ref newNPC);
                        break;

                    case 11:
                        ManageNPCInventory(session, ref newNPC);
                        break;

                    case 12:
                        newNPC.HitDieSize = GetValue<int>(session, "Enter Hit Die Size: ");
                        newNPC.BonusHitDice = GetValue<int>(session, "Enter Bonus Hit Die: ");
                        break;

                    case 13:
                        newNPC.AppearanceChance = GetValue<int>(session, "Enter Appearance Chance: ");
                        break;

                    case 14:
                        newNPC.MaxNumberInWorld = GetValue<int>(session, "Enter Max. Number: ");
                        break;

                    case 15:
                        newNPC.Gender = GetEnumValue<Gender>(session, "Enter Gender: ");
                        break;

                    case 16:
                        newNPC.Flags = GetEnumValue<NPCFlags>(session, "Enter Flags: ");
                        break;

                    case 17:
                        newNPC.ArrivalMessage = GetValue<string>(session, "Enter Arrival Message: ");
                        break;

                    case 18:
                        newNPC.DepatureMessage = GetValue<string>(session, "Enter Departure Message: ");
                        break;

                    case 19:
                        newNPC.NumberOfAttacks = GetValue<int>(session, "Enter Number of Attacks: ");
                        break;

                    case 20:
                        ManageNPCSpells(session, ref newNPC);
                        break;

                    case 21:
                        newNPC.ShopID = GetValue<int>(session, "Enter Shop ID: ");
                        break;

                    case 22:
                        ManageNPCMobProgs(session, ref newNPC);
                        break;

                    case 23:
                        if (ValidateAsset(session, newNPC, true, out _))
                        {
                            if (NPCManager.Instance.AddOrUpdateNPCTemplate(newNPC, true))
                            {
                                session.Send($"%BGT%The new NPC Template has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added a new NPC Template: {newNPC.Name} ({newNPC.TemplateID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%Failed to save the new NPC Template.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add a new NPC Template ({newNPC.Name} ({newNPC.TemplateID})) but the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The new NPC failed validation and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 24:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteNPC(Session session)
        {
            while(true)
            {
                session.Send($"%BRT%This is a permanent change to the Realms!%PT%{Constants.NewLine}");
                session.Send($"Enter NPC Template ID or END to return: ");
                string input = session.Read();
                if (string.IsNullOrEmpty(input))
                {
                    session.Send($"%BRT%Sorry, that is not a valid Template ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int value))
                {
                    session.Send($"%BRT%Sorry, that is not a valid Template ID.%PT%{Constants.NewLine}");
                    continue;
                }
                if (!NPCManager.Instance.NPCTemplateExists(value))
                {
                    session.Send($"%BRT%no NPC Template with that ID could be found in NPC Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                var npc = NPCManager.Instance.GetNPC(value);
                if (npc.OLCLocked)
                {
                    var lockingSession = SessionManager.Instance.GetSession(npc.LockHolder);
                    var msg = lockingSession != null ? $"%BRT%The specified NPC template is currently Locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                        $"%BRT%The specified NPC template is currently locked in OLC but the lock holder could not be found.%PT%{Constants.NewLine}";
                    session.Send(msg);
                    return;
                }
                if (NPCManager.Instance.RemoveNPCTemplate(value))
                {
                    session.Send($"BGT%The specified NPC Template has been successfully removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed NPC Template {npc.Name} ({npc.TemplateID})", LogLevel.OLC, true);
                }
                else
                {
                    session.Send($"%BRT%Failed to remove the specified NPC Template.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove NPC Template {npc.Name} ({npc.TemplateID}) however the attempt failed", LogLevel.OLC, true);
                }
            }
        }

        private static void ChangeNPC(Session session)
        {
            session.Send($"Enter NPC Template ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int npcID))
            {
                session.Send($"%BRT%That is not a valid NPC Template ID.%PT%{Constants.NewLine}");
                return;
            }
            if (!NPCManager.Instance.NPCTemplateExists(npcID))
            {
                session.Send($"%BRT%No NPC Template with that ID could be found.%PT%{Constants.NewLine}");
                return;
            }
            if (NPCManager.Instance.GetNPC(npcID).OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(NPCManager.Instance.GetNPC(npcID).LockHolder);
                string msg = lockingSession != null ? $"%BRT%The specified NPC Template is Locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified NPC Template is Locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.Send(msg);
                return;
            }
            NPCManager.Instance.SetNPCLockState(npcID, true, session);
            var npcTemplate = Helpers.Clone(NPCManager.Instance.GetNPC(npcID));
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"%BYT%{new string('=', 77)}%PT%");
                sb.AppendLine($"ID: {npcTemplate.TemplateID}{Constants.TabStop}{Constants.TabStop}Zone: {npcTemplate.ZoneID}");
                sb.AppendLine($"Name: {npcTemplate.Name}{Constants.TabStop}Level: {npcTemplate.Level}{Constants.TabStop}Gender: {npcTemplate.Gender}");
                sb.AppendLine($"Short Description: {npcTemplate.ShortDescription}");
                if (string.IsNullOrEmpty(npcTemplate.LongDescription))
                {
                    sb.AppendLine($"Long Description: None");
                }
                else
                {
                    sb.AppendLine("Long Description:");
                    foreach (var dl in npcTemplate.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.TabStop}{dl}");
                    }
                }
                sb.AppendLine($"STR: {npcTemplate.Strength}{Constants.TabStop}{Constants.TabStop}DEX: {npcTemplate.Dexterity}{Constants.TabStop}{Constants.TabStop}CON: {npcTemplate.Constitution}");
                sb.AppendLine($"INT: {npcTemplate.Intelligence}{Constants.TabStop}{Constants.TabStop}WIS: {npcTemplate.Wisdom}{Constants.TabStop}{Constants.TabStop}CHA: {npcTemplate.Charisma}");
                sb.AppendLine($"Hit Dice: {npcTemplate.NumberOfHitDice}d{npcTemplate.HitDieSize} (Bonus Hit Die: {npcTemplate.BonusHitDice})");
                sb.AppendLine($"Exp: {npcTemplate.ExpAward}{Constants.TabStop}Gold: {npcTemplate.Gold}{Constants.TabStop}Attacks: {npcTemplate.NumberOfAttacks}");
                sb.AppendLine($"Base Armour Class: {npcTemplate.BaseArmourClass} (Natural: {npcTemplate.NaturalArmour}){Constants.TabStop}Armour Class: {npcTemplate.ArmourClass}");
                sb.AppendLine($"Frequency: {npcTemplate.AppearanceChance}{Constants.TabStop}Max. Number: {npcTemplate.MaxNumberInWorld}");
                sb.AppendLine($"Flags: {npcTemplate.Flags}");
                sb.AppendLine($"Shop ID: {npcTemplate.ShopID}");
                sb.AppendLine($"Arrival Message: {npcTemplate.ArrivalMessage}");
                sb.AppendLine($"Departure Message: {npcTemplate.DepatureMessage}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}2. Set Zone{Constants.TabStop}3. Set Level");
                sb.AppendLine($"4. Set Short Description{Constants.TabStop}5. Set Long Description");
                sb.AppendLine($"6. Set Stats{Constants.TabStop}7.Set Exp{Constants.TabStop}8. Set Gold");
                sb.AppendLine($"9. Manage Equipment{Constants.TabStop}{Constants.TabStop}10. Manage Inventory");
                sb.AppendLine($"11. Set Hit Dice{Constants.TabStop}12. Set Appearance Chance{Constants.TabStop}13. Set Max. Number");
                sb.AppendLine($"14. Set Gender{Constants.TabStop}15. Set Flags{Constants.TabStop}16. Set Arrival Message");
                sb.AppendLine($"17. Set Departure Message{Constants.TabStop}18. Set Attacks{Constants.TabStop}19. Manage Spells");
                sb.AppendLine($"20. Set Shop ID{Constants.TabStop}{Constants.TabStop}21. Manage MobProgs");
                sb.AppendLine($"22. Save{Constants.TabStop}23. Return");
                session.Send(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        npcTemplate.Name = GetValue<string>(session, "Enter Name: ");
                        break;

                    case 2:
                        npcTemplate.ZoneID = GetValue<int>(session, "Enter Zone: ");
                        break;

                    case 3:
                        npcTemplate.Level = GetValue<int>(session, "Enter Level: ");
                        break;

                    case 4:
                        npcTemplate.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 5:
                        npcTemplate.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 6:
                        npcTemplate.Strength = Math.Max(0, GetValue<int>(session, "Enter Strength: "));
                        npcTemplate.Dexterity = Math.Max(0, GetValue<int>(session, "Enter Dexterity: "));
                        npcTemplate.Constitution = Math.Max(0, GetValue<int>(session, "Enter Constitution: "));
                        npcTemplate.Intelligence = Math.Max(0, GetValue<int>(session, "Enter Intelligence: "));
                        npcTemplate.Wisdom = Math.Max(0, GetValue<int>(session, "Enter Wisdom: "));
                        npcTemplate.Charisma = Math.Max(0, GetValue<int>(session, "Enter Charisma: "));
                        npcTemplate.BaseArmourClass = Math.Max(0, GetValue<int>(session, "Enter Base Armour Class: "));
                        npcTemplate.NaturalArmour = GetValue<bool>(session, "Natural Armour (true/false): ");
                        break;

                    case 7:
                        npcTemplate.ExpAward = Math.Max(0, GetValue<int>(session, "Enter Exp Award: "));
                        break;

                    case 8:
                        npcTemplate.Gold = GetValue<ulong>(session, "Enter Gold: ");
                        break;

                    case 9:
                        ManageNPCEquipment(session, ref npcTemplate);
                        break;

                    case 10:
                        ManageNPCInventory(session, ref npcTemplate);
                        break;

                    case 11:
                        npcTemplate.HitDieSize = GetValue<int>(session, "Enter Hit Die Size: ");
                        npcTemplate.BonusHitDice = GetValue<int>(session, "Enter Bonus Hit Die: ");
                        break;

                    case 12:
                        npcTemplate.AppearanceChance = GetValue<int>(session, "Enter Appearance Chance: ");
                        break;

                    case 13:
                        npcTemplate.MaxNumberInWorld = GetValue<int>(session, "Enter Max. Number: ");
                        break;

                    case 14:
                        npcTemplate.Gender = GetEnumValue<Gender>(session, "Enter Gender: ");
                        break;

                    case 15:
                        npcTemplate.Flags = GetEnumValue<NPCFlags>(session, "Enter Flags: ");
                        break;

                    case 16:
                        npcTemplate.ArrivalMessage = GetValue<string>(session, "Enter Arrival Message: ");
                        break;

                    case 17:
                        npcTemplate.DepatureMessage = GetValue<string>(session, "Enter Departure Message: ");
                        break;

                    case 18:
                        npcTemplate.NumberOfAttacks = GetValue<int>(session, "Enter Number of Attacks: ");
                        break;

                    case 19:
                        ManageNPCSpells(session, ref npcTemplate);
                        break;

                    case 20:
                        npcTemplate.ShopID = GetValue<int>(session, "Enter Shop ID: ");
                        break;

                    case 21:
                        ManageNPCMobProgs(session, ref npcTemplate);
                        break;

                    case 22:
                        if (ValidateAsset(session, npcTemplate, false, out _))
                        {
                            if (NPCManager.Instance.AddOrUpdateNPCTemplate(npcTemplate, false))
                            {
                                session.Send($"%BGT%The NPC Template has been updated successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updatedated NPC Template: {npcTemplate.Name} ({npcTemplate.TemplateID})", LogLevel.OLC, true);
                                NPCManager.Instance.SetNPCLockState(npcTemplate.TemplateID, false, session);
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%Failed to save the new NPC Template.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add a new NPC Template ({npcTemplate.Name} ({npcTemplate.TemplateID})) but the attempt failed", LogLevel.OLC, true);
                                continue;
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The new NPC failed validation and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 23:
                        NPCManager.Instance.SetNPCLockState(npcTemplate.TemplateID, false, session);
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageNPCInventory(Session session, ref NPC npc)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (npc.Inventory.Count > 0)
                {
                    sb.AppendLine("Inventory:");
                    foreach(var i in npc.Inventory.Values.Select(x => new { x.ID, x.Name, x.ShortDescription }).Distinct().OrderBy(x => x.ID))
                    {
                        var cnt = npc.Inventory.Values.Where(x => x.ID == i.ID).Count();
                        sb.AppendLine($"    {cnt} x {i.Name}, {i.ShortDescription} ({i.ID})");
                    }
                }
                else
                {
                    sb.AppendLine($"Inventory: None");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Inventory{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That doesn't look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        int itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (ItemManager.Instance.ItemExists(itemID))
                        {
                            var itemToAdd = ItemManager.Instance.GetItem(itemID).Clone();
                            itemToAdd.ItemID = Guid.NewGuid();
                            npc.Inventory.TryAdd(itemToAdd.ItemID, itemToAdd);
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        var itemToRemove = npc.Inventory.Values.FirstOrDefault(x => x.ID == itemID);
                        if (itemToRemove != null)
                        {
                            npc.Inventory.TryRemove(itemToRemove.ItemID, out _);
                        }
                        break;

                    case 3:
                        npc.Inventory.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That doesn't look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageNPCMobProgs(Session session, ref NPC npc)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (npc.MobProgs.Count > 0)
                {
                    sb.AppendLine("MobProgs:");
                    foreach (var mp in npc.MobProgs.Keys)
                    {
                        var mobProg = MobProgManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            sb.AppendLine($"  {mobProg.ID} - {mobProg.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"  {mp} - Unknown MobProg");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("MobProgs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add MobProg{Constants.TabStop}{Constants.TabStop}2. Remove MobProg");
                sb.AppendLine($"3. Clear MobProgs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice:");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int option))
                {
                    session.Send($"%BRT%That doesn't look like a valid option.%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var id = GetValue<int>(session, "Enter MobProg ID:");
                        if (MobProgManager.Instance.MobProgExists(id))
                        {
                            npc.MobProgs.TryAdd(id, true);
                        }
                        break;

                    case 2:
                        id = GetValue<int>(session, "Enter MobProg ID:");
                        npc.MobProgs.TryRemove(id, out _);
                        break;

                    case 3:
                        npc.MobProgs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That doesn't look like a valid option.%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageNPCSpells(Session session, ref NPC npc)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (npc.Spells.Count > 0)
                {
                    sb.AppendLine("Current Spells:");
                    foreach (var s in npc.Spells)
                    {
                        var spell = SpellManager.Instance.GetSpell(s.Key);
                        if (spell != null)
                        {
                            sb.AppendLine($"    {spell.Name} ({spell.SpellType})");
                        }
                        else
                        {
                            sb.AppendLine($"    {s.Key} (Unknown Spell)");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"Current Spells: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Spell{Constants.TabStop}2. Remove Spell{Constants.TabStop}3. Clear Spells");
                sb.AppendLine("4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        string spellName = GetValue<string>(session, "Enter Spell Name: ");
                        if (!string.IsNullOrEmpty(spellName))
                        {
                            var spell = SpellManager.Instance.GetSpell(spellName);
                            npc.Spells.TryAdd(spell.Name, true);
                        }
                        break;

                    case 2:
                        spellName = GetValue<string>(session, "Enter Spell Name: ");
                        if (!string.IsNullOrEmpty(spellName))
                        {
                            npc.Spells.TryRemove(spellName, out _);
                        }
                        break;

                    case 3:
                        npc.Spells.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageNPCEquipment(Session session, ref NPC npc)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine("Current Equipment:");
                sb.AppendLine($"Head: {npc.HeadEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Neck: {npc.NeckEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Armour: {npc.ArmourEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Weapon: {npc.WeaponEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Held: {npc.HeldEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Finger (R): {npc.RightFingerEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Finger (L): {npc.LeftFingerEquip?.Name ?? "Nothing"}");
                sb.AppendLine($"Feet: {npc.FeetEquip?.Name ?? "Nothing"}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Head{Constants.TabStop}2. Set Neck{Constants.TabStop}3. Set Armour");
                sb.AppendLine($"4. Set Weapon{Constants.TabStop}5. Set Held{Constants.TabStop}6. Set Finger (R)");
                sb.AppendLine($"7. Set Finger (L){Constants.TabStop}8. Set Feet{Constants.TabStop}9. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        int itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.HeadEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Armour)
                            {
                                if (((Armour)item).Slot == WearSlot.Head)
                                {
                                    npc.HeadEquip = (Armour)item;
                                }
                                else
                                {
                                    session.Send($"%BRT%That is not a valid item for that slot!%PT%{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as armour!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.NeckEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Armour)
                            {
                                if (((Armour)item).Slot == WearSlot.Neck)
                                {
                                    npc.NeckEquip = (Armour)item;
                                }
                                else
                                {
                                    session.Send($"%BRT%That is not a valid item for that slot!%PT%{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as armour!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 3:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.ArmourEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Armour)
                            {
                                if (((Armour)item).Slot == WearSlot.Body)
                                {
                                    npc.ArmourEquip = (Armour)item;
                                }
                                else
                                {
                                    session.Send($"%BRT%That is not a valid item for that slot!%PT%{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as armour!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 4:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.WeaponEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Weapon)
                            {
                                npc.WeaponEquip = (Weapon)item;
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be used as a weapon!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 5:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.HeldEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Armour)
                            {
                                if (((Armour)item).Slot == WearSlot.Held)
                                {
                                    npc.HeldEquip = (Armour)item;
                                }
                                else
                                {
                                    session.Send($"%BRT%That is not a valid item for that slot!%PT%{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as armour!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 6:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.ArmourEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Ring)
                            {
                                npc.RightFingerEquip = (Ring)item;
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as a ring!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 7:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.ArmourEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Ring)
                            {
                                npc.LeftFingerEquip = (Ring)item;
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as a ring!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 8:
                        itemID = GetValue<int>(session, "Enter Item ID (0 to remove): ");
                        if (itemID == 0)
                        {
                            npc.FeetEquip = null;
                        }
                        else
                        {
                            var item = ItemManager.Instance.GetItem(itemID);
                            if (item != null && item.ItemType == ItemType.Armour)
                            {
                                if (((Armour)item).Slot == WearSlot.Feet)
                                {
                                    npc.FeetEquip = (Armour)item;
                                }
                                else
                                {
                                    session.Send($"%BRT%That is not a valid item for that slot!%PT%{Constants.NewLine}");
                                }
                            }
                            else
                            {
                                session.Send($"%BRT%That item cannot be worn as armour!%PT%{Constants.NewLine}");
                            }
                        }
                        break;

                    case 9:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}