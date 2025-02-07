using Etrea3.Core;
using System;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        #region Create
        private static void CreateItem(Session session)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine("Create which type of Item:");
                sb.AppendLine($"1. Misc{Constants.TabStop}{Constants.TabStop}2. Weapon{Constants.TabStop}3. Armour");
                sb.AppendLine($"4. Held{Constants.TabStop}{Constants.TabStop}5. Ring{Constants.TabStop}{Constants.TabStop}6. Scroll");
                sb.AppendLine($"7. Consumable{Constants.TabStop}8. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        CreateMiscItem(session);
                        break;

                    case 2:
                        CreateWeapon(session);
                        break;

                    case 3:
                        CreateArmour(session);
                        break;

                    case 4:
                        CreateHeldItem(session);
                        break;

                    case 5:
                        CreateRing(session);
                        break;

                    case 6:
                        CreateScroll(session);
                        break;

                    case 7:
                        CreateConsumable(session);
                        break;

                    case 8:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateMiscItem(Session session)
        {
            var newItem = new InventoryItem();
            newItem.ItemType = ItemType.Misc;
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newItem.ID}{Constants.TabStop}Name: {newItem.Name}");
                sb.AppendLine($"Short Description: {newItem.ShortDescription}");
                sb.AppendLine($"Base Value: {newItem.BaseValue}");
                sb.AppendLine($"Long Description:");
                if (!string.IsNullOrEmpty(newItem.LongDescription))
                {
                    foreach (var ln in newItem.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.TabStop}{ln}");
                    }
                }
                sb.AppendLine($"Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Set Base Value");
                sb.AppendLine($"6. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}7. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newItem.ID = GetValue<int>(session, "Enter Item ID: ");
                        break;

                    case 2:
                        newItem.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 3:
                        newItem.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        newItem.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        newItem.BaseValue = GetValue<int>(session, "Enter Base Gold Value: ");
                        break;

                    case 6:
                        if (ValidateAsset(session, newItem, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newItem, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Item {newItem.Name} ({newItem.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The new Item has been created successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Item {newItem.Name} ({newItem.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The new Item could not be created.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The Item could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 7:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateWeapon(Session session)
        {
            var newWeapon = new Weapon();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newWeapon.ID}{Constants.TabStop}Name: {newWeapon.Name}");
                sb.AppendLine($"Short Description: {newWeapon.ShortDescription}");
                sb.AppendLine($"Base Value: {newWeapon.BaseValue}{Constants.TabStop}Damage: {newWeapon.NumberOfDamageDice}D{newWeapon.SizeOfDamageDice}");
                sb.AppendLine($"Long Description:");
                if (!string.IsNullOrEmpty(newWeapon.LongDescription))
                {
                    foreach (var ln in newWeapon.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.TabStop}{ln}");
                    }
                }
                sb.AppendLine($"Magical: {newWeapon.IsMagical}{Constants.TabStop}Cursed: {newWeapon.IsCursed}{Constants.TabStop}Two-Handed: {newWeapon.IsTwoHanded}");
                sb.AppendLine($"Monster Only: {newWeapon.MonsterOnly}{Constants.TabStop}Weapon Type: {newWeapon.WeaponType}");
                sb.AppendLine($"Hit Modifier: {newWeapon.HitModifier}{Constants.TabStop}Damage Modifier: {newWeapon.DamageModifier}");
                sb.AppendLine($"Required Skills: {newWeapon.RequiredSkills.Count}{Constants.TabStop}Applied Buffs: {newWeapon.AppliedBuffs.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Value{Constants.TabStop}{Constants.TabStop}5. Set Damage{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}6. Set Long Description");
                sb.AppendLine($"7. Set Magical{Constants.TabStop}{Constants.TabStop}8. Set Cursed{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}9. Set Two-Handed");
                sb.AppendLine($"10. Set Monster Only{Constants.TabStop}11. Set Weapon Type{Constants.TabStop}{Constants.TabStop}12. Set Hit Modifier");
                sb.AppendLine($"13. Set Damage Modifier{Constants.TabStop}14. Manage Required Skills{Constants.TabStop}15. Manage Applied Buffs");
                sb.AppendLine($"16. Save{Constants.TabStop}{Constants.TabStop}17. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newWeapon.ID = GetValue<int>(session, "Enter Weapon ID: ");
                        break;

                    case 2:
                        newWeapon.Name = GetValue<string>(session, "Enter Weapon Name: ");
                        break;

                    case 3:
                        newWeapon.ShortDescription = GetValue<string>(session, "Enter Weapon Short Description: ");
                        break;

                    case 4:
                        newWeapon.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 5:
                        newWeapon.NumberOfDamageDice = GetValue<int>(session, "Enter Number of Dice: ");
                        newWeapon.SizeOfDamageDice = GetValue<int>(session, "Enter Size of Dice: ");
                        break;

                    case 6:
                        newWeapon.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 7:
                        newWeapon.IsMagical = GetValue<bool>(session, "Is Magical (true/false): ");
                        break;

                    case 8:
                        newWeapon.IsCursed = GetValue<bool>(session, "Is Cursed (true/false): ");
                        break;

                    case 9:
                        newWeapon.IsTwoHanded = GetValue<bool>(session, "Is Two-handed (true/false): ");
                        break;

                    case 10:
                        newWeapon.MonsterOnly = GetValue<bool>(session, "Monster Only (true/false): ");
                        break;

                    case 11:
                        newWeapon.WeaponType = GetEnumValue<WeaponType>(session, "Enter Weapon Type: ");
                        break;

                    case 12:
                        newWeapon.HitModifier = GetValue<int>(session, "Enter Hit Modifier: ");
                        break;

                    case 13:
                        newWeapon.DamageModifier = GetValue<int>(session, "Enter Damage Modifier: ");
                        break;

                    case 14:
                        ManageWeaponSkills(session, ref newWeapon);
                        break;

                    case 15:
                        ManageWeaponBuffs(session, ref newWeapon);
                        break;

                    case 16:
                        if (ValidateAsset(session, newWeapon, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newWeapon, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Weapon {newWeapon.Name} ({newWeapon.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The new Weapon has been created successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Weapon {newWeapon.Name} ({newWeapon.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The new Weapon could not be saved.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Weapon could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 17:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateArmour(Session session)
        {
            var newArmour = new Armour();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newArmour.ID}{Constants.TabStop}Name: {newArmour.Name}");
                sb.AppendLine($"Short Description: {newArmour.ShortDescription}");
                sb.AppendLine($"Base Value: {newArmour.BaseValue}{Constants.TabStop}AC Modifier: {newArmour.ACModifier}{Constants.TabStop}Damage Reduction: {newArmour.DamageReduction}");
                sb.AppendLine($"Long Description:");
                if (!string.IsNullOrEmpty(newArmour.LongDescription))
                {
                    foreach (var ln in newArmour.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                    {
                        sb.AppendLine($"{Constants.TabStop}{ln}");
                    }
                }
                sb.AppendLine($"Wear Slot: {newArmour.Slot}{Constants.TabStop}Magical: {newArmour.IsMagical}{Constants.TabStop}Cursed: {newArmour.IsCursed}");
                sb.AppendLine($"Armour Type: {newArmour.ArmourType}");
                sb.AppendLine($"Required Skills: {newArmour.RequiredSkills.Count}{Constants.TabStop}Applied Buffs: {newArmour.AppliedBuffs.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Long Description{Constants.TabStop}{Constants.TabStop}5. Set Value{Constants.TabStop}{Constants.TabStop}6. Set AC Modifier");
                sb.AppendLine($"7. Set Damage Reduction{Constants.TabStop}{Constants.TabStop}8. Set Wear Slot{Constants.TabStop}9. Set Magical");
                sb.AppendLine($"10. Set Cursed{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}11. Set Armour Type{Constants.TabStop}12. Manage Required Skills");
                sb.AppendLine($"13. Manage Applied Buffs");
                sb.AppendLine($"14. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}15. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newArmour.ID = GetValue<int>(session, "Enter Armour ID: ");
                        break;

                    case 2:
                        newArmour.Name = GetValue<string>(session, "Enter Armour Name: ");
                        break;

                    case 3:
                        newArmour.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        newArmour.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        newArmour.BaseValue = GetValue<int>(session, "Enter Armour Gold Value: ");
                        break;

                    case 6:
                        newArmour.ACModifier = GetValue<int>(session, "Enter Armour AC Modifier: ");
                        break;

                    case 7:
                        newArmour.DamageReduction = GetValue<int>(session, "Enter Armour Damage Reduction: ");
                        break;

                    case 8:
                        newArmour.Slot = GetEnumValue<WearSlot>(session, "Enter Equipment Slot: ");
                        break;

                    case 9:
                        newArmour.IsMagical = GetValue<bool>(session, "Magical (true/false): ");
                        break;

                    case 10:
                        newArmour.IsCursed = GetValue<bool>(session, "Cursed (true/false): ");
                        break;

                    case 11:
                        newArmour.ArmourType = GetEnumValue<ArmourType>(session, "Enter Armour Type: ");
                        break;

                    case 12:
                        ManageArmourSkills(session, ref newArmour);
                        break;

                    case 13:
                        ManageArmourBuffs(session, ref newArmour);
                        break;

                    case 14:
                        if (ValidateAsset(session, newArmour, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newArmour, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Armour {newArmour.Name} ({newArmour.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The new Armour has been created successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Armour {newArmour.Name} ({newArmour.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The new Armour was not successfully created.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Armour failed validation and cannot be created.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 15:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateHeldItem(Session session)
        {
            var newArmour = new Armour
            {
                ArmourType = ArmourType.Light,
                Slot = WearSlot.Held
            };
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newArmour.ID}{Constants.TabStop}Name: {newArmour.Name}");
                sb.AppendLine($"Short Description: {newArmour.ShortDescription}");
                sb.AppendLine($"Base Value: {newArmour.BaseValue}{Constants.TabStop}AC Modifier: {newArmour.ACModifier}{Constants.TabStop}Damage Reduction: {newArmour.DamageReduction}");
                sb.AppendLine($"Long Description:");
                if (!string.IsNullOrEmpty(newArmour.LongDescription))
                {
                    foreach (var ln in newArmour.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                    {
                        sb.AppendLine($"{Constants.TabStop}{ln}");
                    }
                }
                sb.AppendLine($"Magical: {newArmour.IsMagical}{Constants.TabStop}Cursed: {newArmour.IsCursed}");
                sb.AppendLine($"Required Skills: {newArmour.RequiredSkills.Count}{Constants.TabStop}Applied Buffs: {newArmour.AppliedBuffs.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Long Description{Constants.TabStop}{Constants.TabStop}5. Set Value{Constants.TabStop}6. Set AC Modifier");
                sb.AppendLine($"7. Set Damage Reduction{Constants.TabStop}{Constants.TabStop}8. Set Magical{Constants.TabStop}9. Set Cursed");
                sb.AppendLine($"10. Manage Required Skills{Constants.TabStop}11. Manage Applied Buffs");
                sb.AppendLine($"12. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}13. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newArmour.ID = GetValue<int>(session, "Enter Armour ID: ");
                        break;

                    case 2:
                        newArmour.Name = GetValue<string>(session, "Enter Armour Name: ");
                        break;

                    case 3:
                        newArmour.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        newArmour.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        newArmour.BaseValue = GetValue<int>(session, "Enter Armour Gold Value: ");
                        break;

                    case 6:
                        newArmour.ACModifier = GetValue<int>(session, "Enter Armour AC Modifier: ");
                        break;

                    case 7:
                        newArmour.DamageReduction = GetValue<int>(session, "Enter Armour Damage Reduction: ");
                        break;

                    case 8:
                        newArmour.IsMagical = GetValue<bool>(session, "Magical (true/false): ");
                        break;

                    case 9:
                        newArmour.IsCursed = GetValue<bool>(session, "Cursed (true/false): ");
                        break;

                    case 10:
                        ManageArmourSkills(session, ref newArmour);
                        break;

                    case 11:
                        ManageArmourBuffs(session, ref newArmour);
                        break;

                    case 12:
                        if (ValidateAsset(session, newArmour, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newArmour, true))
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Armour {newArmour.Name} ({newArmour.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The new Armour has been created successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Armour {newArmour.Name} ({newArmour.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The new Armour was not successfully created.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Armour failed validation and cannot be created.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 13:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateRing(Session session)
        {
            Ring newRing = new Ring();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newRing.ID}{Constants.TabStop}Item Name: {newRing.Name}");
                sb.AppendLine($"Short Description: {newRing.ShortDescription}");
                sb.AppendLine($"Value: {newRing.BaseValue}{Constants.TabStop}Magical: {newRing.IsMagical}{Constants.TabStop}Cursed: {newRing.IsCursed}");
                sb.AppendLine($"Damage Modifier: {newRing.DamageModifier}{Constants.TabStop}Hit Modifier: {newRing.HitModifier}{Constants.TabStop}Damage Reduction: {newRing.DamageReduction}");
                sb.AppendLine($"AC Modifier: {newRing.ACModifier}{Constants.TabStop}{Constants.TabStop}Applied Buffs: {newRing.AppliedBuffs.Count}");
                sb.AppendLine($"Long Description:");
                if (!string.IsNullOrEmpty(newRing.LongDescription))
                {
                    foreach (var ln in newRing.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.TabStop}{ln}");
                    }
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Long Description{Constants.TabStop}{Constants.TabStop}5. Set Value{Constants.TabStop}{Constants.TabStop}6. Set Magical");
                sb.AppendLine($"7. Set Cursed{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}8. Set Damage Modifier{Constants.TabStop}9. Set Hit Modifier");
                sb.AppendLine($"10. Set Damage Reduction{Constants.TabStop}11. Manage Applied Buffs{Constants.TabStop}12. Set AC Modifier");
                sb.AppendLine($"13. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}14. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newRing.ID = GetValue<int>(session, "Enter Item ID: ");
                        break;

                    case 2:
                        newRing.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 3:
                        newRing.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        newRing.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        newRing.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 6:
                        newRing.IsMagical = GetValue<bool>(session, "Magical (true/false): ");
                        break;

                    case 7:
                        newRing.IsCursed = GetValue<bool>(session, "Cursed (true/false): ");
                        break;

                    case 8:
                        newRing.DamageModifier = GetValue<int>(session, "Enter Damage Modifier: ");
                        break;

                    case 9:
                        newRing.HitModifier = GetValue<int>(session, "Enter Hit Modifier: ");
                        break;

                    case 10:
                        newRing.DamageReduction = GetValue<int>(session, "Enter Damage Reduction: ");
                        break;

                    case 11:
                        ManageRingBuffs(session, ref newRing);
                        break;

                    case 12:
                        newRing.ACModifier = GetValue<int>(session, "Enter AC Modifier: ");
                        break;

                    case 13:
                        if (ValidateAsset(session, newRing, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newRing, true))
                            {
                                session.SendSystem($"%BGT%The new Ring was saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Ring {newRing.Name} ({newRing.ID})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The new Ring was not successfully saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Ring {newRing.Name} ({newRing.ID}) but the attempt failed", LogLevel.OLC);
                            }
                        }
                        else
                        {
                            session.SendSystem($"BRT%The new Ring could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 14:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateScroll(Session session)
        {
            Scroll newScroll = new Scroll();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newScroll.ID}{Constants.TabStop}Name: {newScroll.Name}");
                sb.AppendLine($"Short Description: {newScroll.ShortDescription}");
                sb.AppendLine($"Base Value: {newScroll.BaseValue}{Constants.TabStop}Spell: {newScroll.CastsSpell}");
                sb.AppendLine($"Long Description: ");
                if (!string.IsNullOrEmpty(newScroll.LongDescription))
                {
                    foreach (var ln in newScroll.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.TabStop}{ln}");
                    }
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Long Description{Constants.TabStop}5. Set Value{Constants.TabStop}{Constants.TabStop}6. Set Spell");
                sb.AppendLine($"7. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}8. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newScroll.ID = GetValue<int>(session, "Enter Item ID: ");
                        break;

                    case 2:
                        newScroll.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 3:
                        newScroll.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        newScroll.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        newScroll.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 6:
                        string spellName = GetValue<string>(session, "Enter Spell Name: ");
                        var spell = SpellManager.Instance.GetSpell(spellName);
                        if (spell != null)
                        {
                            newScroll.CastsSpell = spell.Name;
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Spell with that name could be found in Spell Manager.%PT%{Constants.NewLine}");
                            newScroll.CastsSpell = string.Empty;
                        }
                        break;

                    case 7:
                        if (ValidateAsset(session, newScroll, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newScroll, true))
                            {
                                session.SendSystem($"%BGT%The new Scroll was saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Scroll {newScroll.Name} ({newScroll.ID})", LogLevel.OLC);
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The new Scroll was not saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add a new Scroll {newScroll.Name} ({newScroll.ID}) but the attempt failed", LogLevel.OLC);
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Scroll could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 8:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void CreateConsumable(Session session)
        {
            Consumable newConsumable = new Consumable();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {newConsumable.ID}{Constants.TabStop}Name: {newConsumable.Name}");
                sb.AppendLine($"Effect: {newConsumable.Effects}{Constants.TabStop}Base Value: {newConsumable.BaseValue}");
                sb.AppendLine($"Effect Dice: {newConsumable.NumberOfDamageDice}D{newConsumable.SizeofDamageDice}");
                sb.AppendLine($"Short Description: {newConsumable.ShortDescription}");
                sb.AppendLine($"Applied Buffs: {newConsumable.AppliedBuffs.Count}");
                sb.AppendLine($"Long Description:");
                if (!string.IsNullOrEmpty(newConsumable.LongDescription))
                {
                    foreach (var ln in newConsumable.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine($"{Constants.NewLine}{ln}");
                    }
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}3. Set Base Value");
                sb.AppendLine($"4. Set Effects{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}5. Set Short Description{Constants.TabStop}6. Set Long Description");
                sb.AppendLine($"7. Manage Applied Buffs{Constants.TabStop}{Constants.TabStop}8. Set Effect Dice");
                sb.AppendLine($"9. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}10. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        newConsumable.ID = GetValue<int>(session, "Enter Item ID: ");
                        break;

                    case 2:
                        newConsumable.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 3:
                        newConsumable.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 4:
                        newConsumable.Effects = GetValue<ConsumableEffect>(session, "Enter Consumable Effects: ");
                        break;

                    case 5:
                        newConsumable.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 6:
                        newConsumable.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 7:
                        ManageConsumableBuffs(session, ref newConsumable);
                        break;

                    case 8:
                        newConsumable.NumberOfDamageDice = GetValue<int>(session, "Enter Number of Effect Dice: ");
                        newConsumable.SizeofDamageDice = GetValue<int>(session, "Enter Size of Effect Dice: ");
                        break;

                    case 9:
                        if (ValidateAsset(session, newConsumable, true, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(newConsumable, true))
                            {
                                session.SendSystem($"BGT%The new Consumable has been added successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has added new Consumable {newConsumable.Name} ({newConsumable.ID})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The new Consumable could not be saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to add new Consumable {newConsumable.Name} ({newConsumable.ID}) but the attempt failed", LogLevel.OLC);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The new Consumable could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 10:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
        #endregion

        #region Edit
        private static void ChangeItem(Session session)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine("Change which type of Item:");
                sb.AppendLine($"1. Misc{Constants.TabStop}{Constants.TabStop}2. Weapon{Constants.TabStop}3. Armour");
                sb.AppendLine($"4. Held{Constants.TabStop}{Constants.TabStop}5. Ring{Constants.TabStop}{Constants.TabStop}6. Scroll");
                sb.AppendLine($"7. Consumable{Constants.TabStop}8. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        ChangeMiscItem(session);
                        break;

                    case 2:
                        ChangeWeapon(session);
                        break;

                    case 3:
                        ChangeArmour(session);
                        break;

                    case 4:
                        ChangeHeldItem(session);
                        break;

                    case 5:
                        ChangeRing(session);
                        break;

                    case 6:
                        ChangeScroll(session);
                        break;

                    case 7:
                        ChangeConsumable(session);
                        break;

                    case 8:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeMiscItem(Session session)
        {
            session.SendSystem($"Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var item = ItemManager.Instance.GetItem(itemID);
            if (item == null)
            {
                session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (item.ItemType != ItemType.Misc)
            {
                session.SendSystem($"%BRT%The specified Item is not of the Misc type ({item.ItemType})%PT%{Constants.NewLine}");
                return;
            }
            if(item.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Item is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Item is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            InventoryItem modItem = Helpers.Clone<InventoryItem>(item);
            ItemManager.Instance.SetItemLockState(itemID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modItem.ID}{Constants.TabStop}Name: {modItem.Name}");
                sb.AppendLine($"Short Description: {modItem.ShortDescription}");
                sb.AppendLine($"Base Value: {modItem.BaseValue}");
                sb.AppendLine($"Long Description:");
                foreach (var ln in modItem.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"{Constants.TabStop}{ln}");
                }
                sb.AppendLine($"Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}4. Set Base Value");
                sb.AppendLine($"5. Save{Constants.TabStop}{Constants.TabStop}6. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modItem.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 2:
                        modItem.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 3:
                        modItem.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 4:
                        modItem.BaseValue = GetValue<int>(session, "Enter Base Gold Value: ");
                        break;

                    case 5:
                        if (ValidateAsset(session, modItem, false, out string _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modItem, false))
                            {
                                ItemManager.Instance.SetItemLockState(modItem.ID, false, session);
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Item {modItem.Name} ({modItem.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The Item has been updated successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Item {modItem.Name} ({modItem.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The update to the Item failed.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The Item could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 6:
                        ItemManager.Instance.SetItemLockState(modItem.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeWeapon(Session session)
        {
            session.SendSystem("Enter Weapon ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int weaponID))
            {
                session.SendSystem($"%BRT%That is not a valid Weapon ID.%PT%{Constants.NewLine}");
                return;
            }
            var wpn = ItemManager.Instance.GetItem(weaponID);
            if (wpn == null)
            {
                session.SendSystem($"%BRT%No Weapon with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (wpn.ItemType != ItemType.Weapon)
            {
                session.SendSystem($"%BRT%The specified Item is not of the Weapon type.%PT%{Constants.NewLine}");
                return;
            }
            if (wpn.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(wpn.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Weapon is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"The specified Weapon is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            Weapon modWeapon = Helpers.Clone<Weapon>(wpn);
            ItemManager.Instance.SetItemLockState(modWeapon.ID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modWeapon.ItemID}{Constants.TabStop}Name: {modWeapon.Name}");
                sb.AppendLine($"Short Description: {modWeapon.ShortDescription}");
                sb.AppendLine($"Base Value: {modWeapon.BaseValue}{Constants.TabStop}Damage: {modWeapon.NumberOfDamageDice}D{modWeapon.SizeOfDamageDice}");
                sb.AppendLine($"Long Description:");
                foreach (var ln in modWeapon.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"{Constants.TabStop}{ln}");
                }
                sb.AppendLine($"Magical: {modWeapon.IsMagical}{Constants.TabStop}Cursed: {modWeapon.IsCursed}{Constants.TabStop}Two-Handed: {modWeapon.IsTwoHanded}");
                sb.AppendLine($"Monster Only: {modWeapon.MonsterOnly}{Constants.TabStop}Weapon Type: {modWeapon.WeaponType}");
                sb.AppendLine($"Hit Modifier: {modWeapon.HitModifier}{Constants.TabStop}Damage Modifier: {modWeapon.DamageModifier}");
                sb.AppendLine($"Required Skills: {modWeapon.RequiredSkills.Count}{Constants.TabStop}Applied Buffs: {modWeapon.AppliedBuffs.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Value{Constants.TabStop}4. Set Damage{Constants.TabStop}5. Set Long Description");
                sb.AppendLine($"6. Set Magical{Constants.TabStop}7. Set Cursed{Constants.TabStop}8. Set Two-Handed");
                sb.AppendLine($"9. Set Monster Only{Constants.TabStop}10. Set Weapon Type{Constants.TabStop}11. Set Hit Modifier");
                sb.AppendLine($"12. Set Damage Modifier{Constants.TabStop}13. Manage Required Skills{Constants.TabStop}14. Manage Applied Buffs");
                sb.AppendLine($"15. Save{Constants.TabStop}16. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modWeapon.Name = GetValue<string>(session, "Enter Weapon Name: ");
                        break;

                    case 2:
                        modWeapon.ShortDescription = GetValue<string>(session, "Enter Weapon Short Description: ");
                        break;

                    case 3:
                        modWeapon.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 4:
                        modWeapon.NumberOfDamageDice = GetValue<int>(session, "Enter Number of Dice: ");
                        modWeapon.SizeOfDamageDice = GetValue<int>(session, "Enter Size of Dice: ");
                        break;

                    case 5:
                        modWeapon.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 6:
                        modWeapon.IsMagical = GetValue<bool>(session, "Is Magical (true/false): ");
                        break;

                    case 7:
                        modWeapon.IsCursed = GetValue<bool>(session, "Is Cursed (true/false): ");
                        break;

                    case 8:
                        modWeapon.IsTwoHanded = GetValue<bool>(session, "Is Two-handed (true/false): ");
                        break;

                    case 9:
                        modWeapon.MonsterOnly = GetValue<bool>(session, "Monster Only (true/false): ");
                        break;

                    case 10:
                        modWeapon.WeaponType = GetEnumValue<WeaponType>(session, "Enter Weapon Type: ");
                        break;

                    case 11:
                        modWeapon.HitModifier = GetValue<int>(session, "Enter Hit Modifier: ");
                        break;

                    case 12:
                        modWeapon.DamageModifier = GetValue<int>(session, "Enter Damage Modifier: ");
                        break;

                    case 13:
                        ManageWeaponSkills(session, ref modWeapon);
                        break;

                    case 14:
                        ManageWeaponBuffs(session, ref modWeapon);
                        break;

                    case 15:
                        if (ValidateAsset(session, modWeapon, false, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modWeapon, false))
                            {
                                ItemManager.Instance.SetItemLockState(modWeapon.ID, false, session);
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Weapon {modWeapon.Name} ({modWeapon.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The updated Weapon has been saved successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Weapon {modWeapon.Name} ({modWeapon.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The updated Weapon could not be saved.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Weapon could not be validated and will not be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 16:
                        ItemManager.Instance.SetItemLockState(modWeapon.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeArmour(Session session)
        {
            session.SendSystem("Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var armour = ItemManager.Instance.GetItem(itemID);
            if (armour == null)
            {
                session.SendSystem($"%BRT%No Armour with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (armour.ItemType != ItemType.Armour)
            {
                session.SendSystem($"%BRT%The specified Item is not of the Armour type.%PT%{Constants.NewLine}");
                return;
            }
            if (armour.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(armour.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Armour is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%PT%The specified Armour is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            Armour modArmour = Helpers.Clone<Armour>(armour);
            ItemManager.Instance.SetItemLockState(modArmour.ID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modArmour.ID}{Constants.TabStop}Name: {modArmour.Name}");
                sb.AppendLine($"Short Description: {modArmour.ShortDescription}");
                sb.AppendLine($"Base Value: {modArmour.BaseValue}{Constants.TabStop}AC Modifier: {modArmour.ACModifier}{Constants.TabStop}Damage Reduction: {modArmour.DamageReduction}");
                sb.AppendLine($"Long Description:");
                foreach (var ln in modArmour.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                {
                    sb.AppendLine($"{Constants.TabStop}{ln}");
                }
                sb.AppendLine($"Wear Slot: {modArmour.Slot}{Constants.TabStop}Magical: {modArmour.IsMagical}{Constants.TabStop}Cursed: {modArmour.IsCursed}");
                sb.AppendLine($"Armour Type: {modArmour.ArmourType}");
                sb.AppendLine($"Required Skills: {modArmour.RequiredSkills.Count}{Constants.TabStop}Applied Buffs: {modArmour.AppliedBuffs.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}4. Set Value{Constants.TabStop}5. Set AC Modifier");
                sb.AppendLine($"6. Set Damage Reduction{Constants.TabStop}7. Set Wear Slot{Constants.TabStop}8. Set Magical");
                sb.AppendLine($"9. Set Cursed{Constants.TabStop}10. Set Armour Type{Constants.TabStop}11. Manage Required Skills");
                sb.AppendLine($"12. Manage Applied Buffs");
                sb.AppendLine($"13. Save{Constants.TabStop}{Constants.TabStop}14. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modArmour.Name = GetValue<string>(session, "Enter Armour Name: ");
                        break;

                    case 2:
                        modArmour.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 3:
                        modArmour.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 4:
                        modArmour.BaseValue = GetValue<int>(session, "Enter Armour Gold Value: ");
                        break;

                    case 5:
                        modArmour.ACModifier = GetValue<int>(session, "Enter Armour AC Modifier: ");
                        break;

                    case 6:
                        modArmour.DamageReduction = GetValue<int>(session, "Enter Armour Damage Reduction: ");
                        break;

                    case 7:
                        modArmour.Slot = GetEnumValue<WearSlot>(session, "Enter Equipment Slot: ");
                        break;

                    case 8:
                        modArmour.IsMagical = GetValue<bool>(session, "Magical (true/false): ");
                        break;

                    case 9:
                        modArmour.IsCursed = GetValue<bool>(session, "Cursed (true/false): ");
                        break;

                    case 10:
                        modArmour.ArmourType = GetEnumValue<ArmourType>(session, "Enter Armour Type: ");
                        break;

                    case 11:
                        ManageArmourSkills(session, ref modArmour);
                        break;

                    case 12:
                        ManageArmourBuffs(session, ref modArmour);
                        break;

                    case 13:
                        if (ValidateAsset(session, modArmour, false, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modArmour, false))
                            {
                                ItemManager.Instance.SetItemLockState(modArmour.ID, false, session);
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Armour {modArmour.Name} ({modArmour.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The Armour has been updated successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Armour {modArmour.Name} ({modArmour.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The Armour could not be successfully updated.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The Armour failed validation and cannot be created.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 14:
                        ItemManager.Instance.SetItemLockState(modArmour.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeHeldItem(Session session)
        {
            session.SendSystem($"Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var item = ItemManager.Instance.GetItem(itemID);
            if (item == null)
            {
                session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (item.ItemType != ItemType.Armour)
            {
                session.SendSystem($"The specified Item is not of the Armour type.%PT%{Constants.NewLine}");
                return;
            }
            if (item.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Item is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Item is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            Armour modArmour = Helpers.Clone<Armour>(item);
            ItemManager.Instance.SetItemLockState(modArmour.ID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modArmour.ID}{Constants.TabStop}Name: {modArmour.Name}");
                sb.AppendLine($"Short Description: {modArmour.ShortDescription}");
                sb.AppendLine($"Base Value: {modArmour.BaseValue}{Constants.TabStop}AC Modifier: {modArmour.ACModifier}{Constants.TabStop}Damage Reduction: {modArmour.DamageReduction}");
                sb.AppendLine($"Long Description:");
                foreach (var ln in modArmour.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.None))
                {
                    sb.AppendLine($"{Constants.TabStop}{ln}");
                }
                sb.AppendLine($"Magical: {modArmour.IsMagical}{Constants.TabStop}Cursed: {modArmour.IsCursed}");
                sb.AppendLine($"Required Skills: {modArmour.RequiredSkills.Count}{Constants.TabStop}Applied Buffs: {modArmour.AppliedBuffs.Count}");
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}4. Set Value{Constants.TabStop}5. Set AC Modifier");
                sb.AppendLine($"6. Set Damage Reduction{Constants.TabStop}7. Set Magical{Constants.TabStop}8. Set Cursed");
                sb.AppendLine($"9. Manage Required Skills{Constants.TabStop}10. Manage Applied Buffs");
                sb.AppendLine($"11. Save{Constants.TabStop}{Constants.TabStop}12. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modArmour.Name = GetValue<string>(session, "Enter Armour Name: ");
                        break;

                    case 2:
                        modArmour.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 3:
                        modArmour.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 4:
                        modArmour.BaseValue = GetValue<int>(session, "Enter Armour Gold Value: ");
                        break;

                    case 5:
                        modArmour.ACModifier = GetValue<int>(session, "Enter Armour AC Modifier: ");
                        break;

                    case 6:
                        modArmour.DamageReduction = GetValue<int>(session, "Enter Armour Damage Reduction: ");
                        break;

                    case 7:
                        modArmour.IsMagical = GetValue<bool>(session, "Magical (true/false): ");
                        break;

                    case 8:
                        modArmour.IsCursed = GetValue<bool>(session, "Cursed (true/false): ");
                        break;

                    case 9:
                        ManageArmourSkills(session, ref modArmour);
                        break;

                    case 10:
                        ManageArmourBuffs(session, ref modArmour);
                        break;

                    case 11:
                        if (ValidateAsset(session, modArmour, false, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modArmour, false))
                            {
                                ItemManager.Instance.SetItemLockState(modArmour.ID, false, session);
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Armour {modArmour.Name} ({modArmour.ID})", LogLevel.OLC);
                                session.SendSystem($"%BGT%The Armour has updated successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Armour {modArmour.Name} ({modArmour.ID}) but the attempt failed", LogLevel.OLC);
                                session.SendSystem($"%BRT%The Armour was not successfully updated.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Armour failed validation and cannot be created.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 12:
                        ItemManager.Instance.SetItemLockState(modArmour.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeRing(Session session)
        {
            session.SendSystem($"Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var item = ItemManager.Instance.GetItem(itemID);
            if (item == null)
            {
                session.SendSystem($"%BRT%No Ring with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (item.ItemType != ItemType.Ring)
            {
                session.SendSystem($"%BRT%The specified Item is not a Ring.%PT%{Constants.NewLine}");
                return;
            }
            if (item.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Item is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Item is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            ItemManager.Instance.SetItemLockState(item.ID, true, session);
            Ring modRing = Helpers.Clone<Ring>(item);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modRing.ID}{Constants.TabStop}Item Name: {modRing.Name}");
                sb.AppendLine($"Short Description: {modRing.ShortDescription}");
                sb.AppendLine($"Value: {modRing.BaseValue}{Constants.TabStop}Magical: {modRing.IsMagical}{Constants.TabStop}Cursed: {modRing.IsCursed}");
                sb.AppendLine($"Damage Modifier: {modRing.DamageModifier}{Constants.TabStop}Hit Modifier: {modRing.HitModifier}{Constants.TabStop}Damage Reduction: {modRing.DamageReduction}");
                sb.AppendLine($"AC Modifier: {modRing.ACModifier}{Constants.TabStop}{Constants.TabStop}Applied Buffs: {modRing.AppliedBuffs.Count}");
                sb.AppendLine($"Long Description:");
                foreach (var ln in modRing.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"{Constants.TabStop}{ln}");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}4. Set Value{Constants.TabStop}5. Set Magical");
                sb.AppendLine($"6. Set Cursed{Constants.TabStop}7. Set Damage Modifier{Constants.TabStop}8. Set Hit Modifier");
                sb.AppendLine($"9. Set Damage Reduction{Constants.TabStop}10. Manage Applied Buffs{Constants.TabStop}11. Set AC Modifier");
                sb.AppendLine($"12. Save{Constants.TabStop}13. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modRing.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 2:
                        modRing.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 3:
                        modRing.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 4:
                        modRing.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 5:
                        modRing.IsMagical = GetValue<bool>(session, "Magical (true/false): ");
                        break;

                    case 6:
                        modRing.IsCursed = GetValue<bool>(session, "Cursed (true/false): ");
                        break;

                    case 7:
                        modRing.DamageModifier = GetValue<int>(session, "Enter Damage Modifier: ");
                        break;

                    case 8:
                        modRing.HitModifier = GetValue<int>(session, "Enter Hit Modifier: ");
                        break;

                    case 9:
                        modRing.DamageReduction = GetValue<int>(session, "Enter Damage Reduction: ");
                        break;

                    case 10:
                        ManageRingBuffs(session, ref modRing);
                        break;

                    case 11:
                        modRing.ACModifier = GetValue<int>(session, "Enter AC Modifier: ");
                        break;

                    case 12:
                        if (ValidateAsset(session, modRing, false, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modRing, false))
                            {
                                ItemManager.Instance.SetItemLockState(modRing.ID, false, session);
                                session.SendSystem($"%BGT%The Ring was updated successfully.%PT%{Constants.NewLine}");
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The Ring was not successfully updated.%PT%{Constants.NewLine}");
                            }
                        }
                        else
                        {
                            session.SendSystem($"BRT%The updated Ring could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 13:
                        ItemManager.Instance.SetItemLockState(modRing.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeScroll(Session session)
        {
            session.SendSystem($"Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var item = ItemManager.Instance.GetItem(itemID);
            if (item == null)
            {
                session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (item.ItemType != ItemType.Scroll)
            {
                session.SendSystem($"%BRT%The specified Item is not of the Scroll type.%PT%{Constants.NewLine}");
                return;
            }
            if (item.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Item is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Item is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            ItemManager.Instance.SetItemLockState(item.ID, true, session);
            Scroll modScroll = Helpers.Clone<Scroll>(item);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modScroll.ID}{Constants.TabStop}Name: {modScroll.Name}");
                sb.AppendLine($"Short Description: {modScroll.ShortDescription}");
                sb.AppendLine($"Base Value: {modScroll.BaseValue}{Constants.TabStop}Spell: {modScroll.CastsSpell}");
                sb.AppendLine($"Long Description: ");
                foreach (var ln in modScroll.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"{Constants.TabStop}{ln}");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name{Constants.TabStop}{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Long Description{Constants.TabStop}5. Set Value{Constants.TabStop}6. Set Spell");
                sb.AppendLine($"7. Save{Constants.TabStop}{Constants.TabStop}8. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modScroll.ID = GetValue<int>(session, "Enter Item ID: ");
                        break;

                    case 2:
                        modScroll.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 3:
                        modScroll.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 4:
                        modScroll.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 5:
                        modScroll.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 6:
                        string spellName = GetValue<string>(session, "Enter Spell Name: ");
                        var spell = SpellManager.Instance.GetSpell(spellName);
                        if (spell != null)
                        {
                            modScroll.CastsSpell = spell.Name;
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Spell with that name could be found in Spell Manager.%PT%{Constants.NewLine}");
                            modScroll.CastsSpell = string.Empty;
                        }
                        break;

                    case 7:
                        if (ValidateAsset(session, modScroll, false, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modScroll, false))
                            {
                                ItemManager.Instance.SetItemLockState(item.ID, false, session);
                                session.SendSystem($"%BGT%The updated Scroll was saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Scroll {modScroll.Name} ({modScroll.ID})", LogLevel.OLC);
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The updated Scroll was not saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Scroll {modScroll.Name} ({modScroll.ID}) but the attempt failed", LogLevel.OLC);
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Scroll could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 8:
                        ItemManager.Instance.SetItemLockState(item.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeConsumable(Session session)
        {
            session.SendSystem($"Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToLower() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var item = ItemManager.Instance.GetItem(itemID);
            if (item == null)
            {
                session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                return;
            }
            if (item.ItemType != ItemType.Consumable)
            {
                session.SendSystem($"%BRT%The specified Item is not of the Consumable type.%PT%{Constants.NewLine}");
                return;
            }
            if (item.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Item is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Item is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            ItemManager.Instance.SetItemLockState(item.ID, true, session);
            Consumable modConsumable = Helpers.Clone<Consumable>(item);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Item ID: {modConsumable.ID}{Constants.TabStop}Name: {modConsumable.Name}");
                sb.AppendLine($"Effect: {modConsumable.Effects}{Constants.TabStop}Base Value: {modConsumable.BaseValue}");
                sb.AppendLine($"Effect Dice: {modConsumable.NumberOfDamageDice}D{modConsumable.SizeofDamageDice}");
                sb.AppendLine($"Short Description: {modConsumable.ShortDescription}");
                sb.AppendLine($"Applied Buffs: {modConsumable.AppliedBuffs.Count}");
                sb.AppendLine($"Long Description:");
                foreach (var ln in modConsumable.LongDescription.Split(new[] { Constants.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine($"{Constants.NewLine}{ln}");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}2. Set Name{Constants.TabStop}3. Set Base Value");
                sb.AppendLine($"4. Set Effects{Constants.TabStop}5. Set Short Description{Constants.TabStop}6. Set Long Description");
                sb.AppendLine($"7. Manage Applied Buffs{Constants.TabStop}8. Set Effect Dice");
                sb.AppendLine($"9. Save{Constants.TabStop}{Constants.TabStop}10. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        modConsumable.ID = GetValue<int>(session, "Enter Item ID: ");
                        break;

                    case 2:
                        modConsumable.Name = GetValue<string>(session, "Enter Item Name: ");
                        break;

                    case 3:
                        modConsumable.BaseValue = GetValue<int>(session, "Enter Base Value: ");
                        break;

                    case 4:
                        modConsumable.Effects = GetValue<ConsumableEffect>(session, "Enter Consumable Effects: ");
                        break;

                    case 5:
                        modConsumable.ShortDescription = GetValue<string>(session, "Enter Short Description: ");
                        break;

                    case 6:
                        modConsumable.LongDescription = Helpers.GetLongDescription(session);
                        break;

                    case 7:
                        ManageConsumableBuffs(session, ref modConsumable);
                        break;

                    case 8:
                        modConsumable.NumberOfDamageDice = GetValue<int>(session, "Enter Number of Effect Dice: ");
                        modConsumable.SizeofDamageDice = GetValue<int>(session, "Enter Size of Effect Dice: ");
                        break;

                    case 9:
                        if (ValidateAsset(session, modConsumable, false, out _))
                        {
                            if (ItemManager.Instance.AddOrUpdateItem(modConsumable, false))
                            {
                                ItemManager.Instance.SetItemLockState(modConsumable.ID, false, session);
                                session.SendSystem($"BGT%The updated Consumable has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Consumable {modConsumable.Name} ({modConsumable.ID})", LogLevel.OLC);
                                return;
                            }
                            else
                            {
                                session.SendSystem($"%BRT%The updated Consumable could not be saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to update Consumable {modConsumable.Name} ({modConsumable.ID}) but the attempt failed", LogLevel.OLC);
                                continue;
                            }
                        }
                        else
                        {
                            session.SendSystem($"%BRT%The updated Consumable could not be validated and cannot be saved.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 10:
                        ItemManager.Instance.SetItemLockState(modConsumable.ID, false, session);
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
        #endregion

        #region Delete
        private static void DeleteItem(Session session)
        {
            session.SendSystem($"Enter Item ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int itemID))
            {
                session.SendSystem($"%BRT%That is not a valid Item ID.%PT%{Constants.NewLine}");
                return;
            }
            var item = ItemManager.Instance.GetItem(itemID);
            if (item == null)
            {
                session.SendSystem($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.TabStop}");
                return;
            }
            if (item.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(item.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Item is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Item is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.SendSystem(msg);
                return;
            }
            if (ItemManager.Instance.RemoveItem(item.ID))
            {
                Game.LogMessage($"OLC: Player {session.Player.Name} removed Item {item.Name} ({item.ID})", LogLevel.OLC);
                session.SendSystem($"%BGT%The specified Item has been successfully removed.%PT%{Constants.NewLine}");
            }
            else
            {
                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Item {item.Name} ({item.ID}) but the attempt failed", LogLevel.OLC);
                session.SendSystem($"%BRT%The specified Item could not be removed.%PT%{Constants.NewLine}");
            }
        }
        #endregion

        #region Helpers
        private static void ManageConsumableBuffs(Session session, ref Consumable consumable)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (consumable.AppliedBuffs.Count > 0)
                {
                    sb.AppendLine("Applied Buffs:");
                    foreach (var b in consumable.AppliedBuffs)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{buff.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Buff ({b})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Applied Buffs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Buff{Constants.TabStop}{Constants.TabStop}2. Remove Buff");
                sb.AppendLine($"3. Clear Buffs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        string buffName = GetValue<string>(session, "Enter Buff Name: ");
                        var buff = BuffManager.Instance.GetBuff(buffName);
                        if (buff != null)
                        {
                            consumable.AppliedBuffs.Add(buff.Name);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Buff with that name could be found in Buff Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        buffName = GetValue<string>(session, "Enter Buff Name: ");
                        consumable.AppliedBuffs.Remove(buffName);
                        break;

                    case 3:
                        consumable.AppliedBuffs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageRingBuffs(Session session, ref Ring ring)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (ring.AppliedBuffs.Count > 0)
                {
                    sb.AppendLine("Applied Buffs:");
                    foreach(var b in ring.AppliedBuffs)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{buff.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Buff ({b})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Applied Buffs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Buff{Constants.TabStop}{Constants.TabStop}2. Remove Buff");
                sb.AppendLine($"3. Clear Buffs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        var buffName = GetValue<string>(session, "Enter Buff Name: ");
                        var buff = BuffManager.Instance.GetBuff(buffName);
                        if (buff != null)
                        {
                            ring.AppliedBuffs.Add(buff.Name);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Buff with that name could be found in Buff Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        buffName = GetValue<string>(session, "Enter Buff Name: ");
                        ring.AppliedBuffs.Remove(buffName);
                        break;

                    case 3:
                        ring.AppliedBuffs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageArmourSkills(Session session, ref Armour armour)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (armour.RequiredSkills.Count > 0)
                {
                    sb.AppendLine("Required Skills:");
                    foreach(var s in armour.RequiredSkills)
                    {
                        var skill = SkillManager.Instance.GetSkill(s);
                        if (skill != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{skill.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Skill ({s})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Skills: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Skill{Constants.TabStop}{Constants.TabStop}2. Remove Skill");
                sb.AppendLine($"3. Clear Skills{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var skillName = GetValue<string>(session, "Enter Skill Name: ");
                        var skill = SkillManager.Instance.GetSkill(skillName);
                        if (skill != null)
                        {
                            armour.RequiredSkills.Add(skill.Name);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Skill with that name was found in Skill Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        skillName = GetValue<string>(session, "Enter Skill Name: ");
                        armour.RequiredSkills.Remove(skillName);
                        break;

                    case 3:
                        armour.RequiredSkills.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageArmourBuffs(Session session, ref Armour armour)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (armour.AppliedBuffs.Count > 0)
                {
                    sb.AppendLine("Applied Buffs:");
                    foreach(var b in armour.AppliedBuffs)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{buff.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Buff ({b})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Applied Buffs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Buff{Constants.TabStop}{Constants.TabStop}2. Remove Buff");
                sb.AppendLine($"3. Clear Buffs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        var buffName = GetValue<string>(session, "Enter Buff Name: ");
                        var buff = BuffManager.Instance.GetBuff(buffName);
                        if (buff != null)
                        {
                            armour.AppliedBuffs.Add(buff.Name);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Buff with that name was found in Buff Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        buffName = GetValue<string>(session, "Enter Buff Name: ");
                        armour.AppliedBuffs.Remove(buffName);
                        break;

                    case 3:
                        armour.AppliedBuffs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageWeaponSkills(Session session, ref Weapon weapon)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (weapon.RequiredSkills.Count > 0)
                {
                    sb.AppendLine("Required Skills:");
                    foreach(var s in weapon.RequiredSkills)
                    {
                        var skill = SkillManager.Instance.GetSkill(s);
                        if (skill != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{skill.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Skill ({s})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Skills: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Skill{Constants.TabStop}{Constants.TabStop}2. Remove Skill");
                sb.AppendLine($"3. Clear Skills{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        var skillName = GetValue<string>(session, "Enter Skill Name: ");
                        var skill = SkillManager.Instance.GetSkill(skillName);
                        if (skill != null)
                        {
                            weapon.RequiredSkills.Add(skill.Name);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Skill with that name could be found in Skill Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        skillName = GetValue<string>(session, "Enter Skill Name: ");
                        weapon.RequiredSkills.Remove(skillName);
                        break;

                    case 3:
                        weapon.RequiredSkills.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ManageWeaponBuffs(Session session, ref Weapon weapon)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (weapon.AppliedBuffs.Count > 0)
                {
                    sb.AppendLine("Applied Buffs:");
                    foreach (var b in weapon.AppliedBuffs)
                    {
                        var buff = BuffManager.Instance.GetBuff(b);
                        if (buff != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{buff.Name}");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}Unknown Buff ({b})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Applied Buffs: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Buff{Constants.TabStop}{Constants.TabStop}2. Remove Buff");
                sb.AppendLine($"3. Clear Buffs{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice:");
                session.SendSystem(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var buffName = GetValue<string>(session, "Enter Buff Name: ");
                        var buff = BuffManager.Instance.GetBuff(buffName);
                        if (buff != null)
                        {
                            weapon.AppliedBuffs.Add(buff.Name);
                        }
                        else
                        {
                            session.SendSystem($"%BRT%No Buff with that name could be found in Buff Manager.%PT%{Constants.NewLine}");
                        }
                        break;

                    case 2:
                        buffName = GetValue<string>(session, "Enter Buff Name: ");
                        weapon.AppliedBuffs.Remove(buffName);
                        break;

                    case 3:
                        weapon.AppliedBuffs.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.SendSystem($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
        #endregion
    }
}