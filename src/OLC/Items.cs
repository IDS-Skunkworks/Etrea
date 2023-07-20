using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Entities;
using System.Collections.Generic;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Menus
        private static void EditExistingItem(ref Descriptor desc)
        {
            desc.Send("Enter the ID of the item to edit: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint id))
            {
                var item = ItemManager.Instance.GetItemByID(id);
                if(item.AppliedBuffs ==  null)
                {
                    item.AppliedBuffs = new List<string>();
                }
                if(item != null)
                {
                    switch(item.ItemType)
                    {
                        case ItemType.Ring:
                            EditRing(ref desc, item);
                            break;

                        case ItemType.Potion:
                            EditPotion(ref desc, item);
                            break;

                        case ItemType.Armour:
                            EditArmour(ref desc, item);
                            break;

                        case ItemType.Weapon:
                            EditWeapon(ref desc, item);
                            break;

                        case ItemType.Junk:
                            EditJunk(ref desc, item);
                            break;

                        case ItemType.Scroll:
                            EditScroll(ref desc, item);
                            break;
                    }
                }
                else
                {
                    desc.Send($"Unable to find an item with this ID in the Item Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }

        private static void CreateNewItem(ref Descriptor desc)
        {
            bool validOption = false;
            while (!validOption)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("An item can be anything from a piece of equipment to a scroll, potion or");
                sb.AppendLine("just a piece of junk. Please select the type of item you wish to create:");
                sb.AppendLine("1. Junk");
                sb.AppendLine("2. Weapon");
                sb.AppendLine("3. Armour");
                sb.AppendLine("4. Ring");
                sb.AppendLine("5. Scroll");
                sb.AppendLine("6. Potion");
                sb.AppendLine("7. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 7)
                    {
                        switch (option)
                        {
                            case 1:
                                CreateNewJunkItem(ref desc);
                                break;

                            case 2:
                                CreateNewWeapon(ref desc);
                                break;

                            case 3:
                                CreateNewArmour(ref desc);
                                break;

                            case 4:
                                CreateNewRing(ref desc);
                                break;

                            case 5:
                                CreateNewScroll(ref desc);
                                break;

                            case 6:
                                CreateNewPotion(ref desc);
                                break;

                            case 7:
                                validOption = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region CreateItems
        internal static void CreateNewPotion(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Potions are small phials of liquid which may be drunk by players. Their effects range from healing, applying buffs and causing damage. Some may have no effect at all.");
            sb.AppendLine("All potions are single-use items and will be removed from the player inventory when they are used.");
            sb.AppendLine("A potion has an ID number, name, short and long descriptions, base value, effect, damage dice and flags to indicate if the potion is magical or not.");
            sb.AppendLine("Valid effects for potions are: None, Poison, Healing or Buff.");
            sb.AppendLine("Where a potion effect is Healing, its damage role is added to the drinker's HP instead of subtraced from it.");
            InventoryItem newPotion = new InventoryItem();
            newPotion.ItemType = ItemType.Potion;
            newPotion.AppliedBuffs = new List<string>();
            desc.Send(sb.ToString());
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newPotion.Id}");
                sb.AppendLine($"Item Name: {newPotion.Name}");
                sb.AppendLine($"Short Description: {newPotion.ShortDescription}");
                sb.AppendLine($"Long Description: {newPotion.LongDescription}");
                sb.AppendLine($"Value: {newPotion.BaseValue}{Constants.TabStop}{Constants.TabStop}Potion Effect: {newPotion.PotionEffect}");
                sb.AppendLine($"Number of Damage Dice: {newPotion.NumberOfDamageDice}{Constants.TabStop}Size of Damage Dice: {newPotion.SizeOfDamageDice}");
                sb.AppendLine($"Is Magical?: {newPotion.IsMagical}");
                sb.AppendLine($"Buffs: {string.Join(", ", newPotion.AppliedBuffs)}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}2. Set Item Name");
                sb.AppendLine("3. Set Short Description");
                sb.AppendLine("4. Set Long Description");
                sb.AppendLine($"5. Set Value{Constants.TabStop}6. Set Potion Effect");
                sb.AppendLine($"7. Set Number of Damage Dice{Constants.TabStop}8. Set Size of Damage Dice");
                sb.AppendLine("9. Toggle Magical flag");
                sb.AppendLine($"10. Add Buff{Constants.TabStop}11. Remove Buff");
                sb.AppendLine("12. Save Potion");
                sb.AppendLine("13. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 13)
                    {
                        switch (result)
                        {
                            case 1:
                                newPotion.Id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                break;

                            case 2:
                                newPotion.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 3:
                                newPotion.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 4:
                                newPotion.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                newPotion.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                newPotion.PotionEffect = GetAssetEnumValue<PotionEffect>(ref desc, "Enter Effect: ");
                                break;

                            case 7:
                                newPotion.NumberOfDamageDice = GetAssetUintValue(ref desc, "Enter number of Damage Dice: ");
                                break;

                            case 8:
                                newPotion.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter size of Damage Dice: ");
                                break;

                            case 9:
                                newPotion.IsMagical = !newPotion.IsMagical;
                                break;

                            case 10:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!newPotion.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newPotion.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 11:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newPotion.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newPotion.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                if (ValidatePotionItem(ref desc, ref newPotion, true))
                                {
                                    if (DatabaseManager.AddNewItem(newPotion))
                                    {
                                        if (ItemManager.Instance.AddItem(newPotion.Id, newPotion))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully created new Item: {newPotion.Name} ({newPotion.Id})", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} failed to add new item {newPotion.Name} ({newPotion.Id}) to the ItemManager, it may not be available until the game restarts.", LogLevel.Warning, true);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                        Game.LogMessage($"Player {desc.Player.Name} failed to store new item {newPotion.Name} ({newPotion.Id}) in the World database.", LogLevel.Error, true);
                                    }
                                }
                                break;

                            case 13:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        internal static void CreateNewScroll(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Scrolls are pieces of parchment containing the words required to complete a magical incantation. A Player needs the Read skill to be able to use scrolls.");
            sb.AppendLine("Scrolls are single-use items that are removed from the Player's inventory once they have been successfully used.");
            InventoryItem newScroll = new InventoryItem();
            newScroll.ItemType = ItemType.Scroll;
            newScroll.CastsSpell = string.Empty;
            newScroll.Slot = WearSlot.None;
            desc.Send(sb.ToString());
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newScroll.Id}");
                sb.AppendLine($"Item Name: {newScroll.Name}");
                sb.AppendLine($"Short Description: {newScroll.ShortDescription}");
                sb.AppendLine($"Long Description: {newScroll.LongDescription}");
                sb.AppendLine($"Value: {newScroll.BaseValue}");
                sb.AppendLine($"Casts Spell: {newScroll.CastsSpell}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}2. Set Item Name");
                sb.AppendLine("3. Set Short Description");
                sb.AppendLine("4. Set Long Description");
                sb.AppendLine("5. Set Spell");
                sb.AppendLine($"6. Save Scroll{Constants.TabStop}7. Exit Without Saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if(option > 0 && option <= 7)
                    {
                        switch(option)
                        {
                            case 1:
                                newScroll.Id = GetAssetUintValue(ref desc, "Enter Scroll ID: ");
                                break;

                            case 2:
                                newScroll.Name = GetAssetStringValue(ref desc, "Enter Scroll Name: ");
                                break;

                            case 3:
                                newScroll.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 4:
                                newScroll.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                var spn = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                var sp = Spells.GetSpell(spn);
                                if(sp != null)
                                {
                                    newScroll.CastsSpell = sp.SpellName;
                                }
                                break;

                            case 6:
                                if (ValidateScrollItem(ref desc, ref newScroll, true))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref newScroll))
                                    {
                                        if (ItemManager.Instance.UpdateItemByID(newScroll.Id, ref desc, newScroll))
                                        {
                                            desc.Send($"Scroll added successfully.{Constants.NewLine}");
                                            Game.LogMessage($"INFO: Player {desc.Player} added Scroll '{newScroll.Name}' (ID: {newScroll.Id}) to World Database and ItemManager", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Unable to add item in ItemManager, it may not be available until after a restart.{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Unable to add item in World Database.{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 7:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        internal static void CreateNewRing(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rings are items of jewelry that can be worn on the fingers. Some are exquisite and expensive, others are magical and offer a variety of bonuses while others are simple bands of common metal.");
            sb.AppendLine("A ring has an ID number, name, short and long descriptions, base value, flag to determine if the item is magical and bonuses for Armour Class, hit rolls and damage. They may also apply buffs.");
            InventoryItem newRing = new InventoryItem();
            newRing.ItemType = ItemType.Ring;
            newRing.Slot = WearSlot.FingerLeft | WearSlot.FingerRight;
            newRing.AppliedBuffs = new List<string>();
            desc.Send(sb.ToString());
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newRing.Id}");
                sb.AppendLine($"Item Name: {newRing.Name}");
                sb.AppendLine($"Short Description: {newRing.ShortDescription}");
                sb.AppendLine($"Long Description: {newRing.LongDescription}");
                sb.AppendLine($"Value: {newRing.BaseValue}");
                sb.AppendLine($"Armour Class modifier: {newRing.ArmourClassModifier}");
                sb.AppendLine($"Hit Bonus: {newRing.HitModifier}{Constants.TabStop}Damage Bonus: {newRing.DamageModifier}");
                sb.AppendLine($"Is Magical?: {newRing.IsMagical}{Constants.TabStop}Monster Only?: {newRing.IsMonsterItem}");
                sb.AppendLine($"Buffs: {string.Join(", ", newRing.AppliedBuffs)}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item ID");
                sb.AppendLine("2. Set Item Name");
                sb.AppendLine("3. Set Short Description");
                sb.AppendLine("4. Set Long Description");
                sb.AppendLine($"5. Set Value{Constants.TabStop}6. Set Armour Class Modifier");
                sb.AppendLine($"7. Set Hit Bonus{Constants.TabStop}8. Set Damage Bonus");
                sb.AppendLine($"9. Toggle Magical flag{Constants.TabStop}10. Toggle Monster Only flag");
                sb.AppendLine($"11. Add Buff{Constants.TabStop}12. Remove Buff");
                sb.AppendLine("13. Save Ring");
                sb.AppendLine("14. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 0 && result <= 14)
                    {
                        switch (result)
                        {
                            case 1:
                                newRing.Id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                break;

                            case 2:
                                newRing.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 3:
                                newRing.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 4:
                                newRing.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                newRing.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                newRing.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class Modifier: ");
                                break;

                            case 7:
                                newRing.HitModifier = GetAssetIntegerValue(ref desc, "Enter Hit Modifier: ");
                                break;

                            case 8:
                                newRing.DamageModifier = GetAssetIntegerValue(ref desc, "Enter Damage Modifier: ");
                                break;

                            case 9:
                                newRing.IsMagical = !newRing.IsMagical;
                                break;

                            case 10:
                                newRing.IsMonsterItem = !newRing.IsMonsterItem;
                                break;

                            case 11:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!newRing.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newRing.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newRing.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newRing.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 13:
                                if (ValidateRingItem(ref desc, ref newRing, true))
                                {
                                    if (DatabaseManager.AddNewItem(newRing))
                                    {
                                        if (ItemManager.Instance.AddItem(newRing.Id, newRing))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully created new Item: {newRing.Name} ({newRing.Id})", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} failed to add new item {newRing.Name} ({newRing.Id}) to the ItemManager, it may not be available until the game restarts.", LogLevel.Warning, true);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                        Game.LogMessage($"Player {desc.Player.Name} failed to store new item {newRing.Name} ({newRing.Id}) in the World database.", LogLevel.Error, true);
                                    }
                                }
                                break;

                            case 14:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void CreateNewArmour(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Armour is designed to protect from weapons. Wearing armour increases a player's Armour Class, making them harder to hit during combat.");
            sb.AppendLine("Armour can be worn in several locations, each piece giving its own set of bonuses to the player wearing it. Bonuses from any equipped armour are cumulative.");
            sb.AppendLine("Broadly speaking, Armour can be divided into three categories: light, medium and heavy. Light armour would include robes and leather jerkins, medium armour would include chainmail while heavy armour would be full plate.");
            sb.AppendLine("Characters can wear armour they are not skilled in, but will suffer penalties for doing so.");
            sb.AppendLine("Magical armour will give players the full Armour Class bonus even if they are not skilled at wearing that type of armour. Non-magical armour will have its Armour Class bonus reduced if worn by a player unskilled in wearing that type of armour.");
            sb.AppendLine("In both cases, unskilled players will also suffer a penalty to their Armour Class for wearing armour they are not skilled with.");
            sb.AppendLine("A piece of armour has an ID number, name, short and long descriptions, base value, Armour Class modifier and flag to determine if the item is magical or not.");
            sb.AppendLine("Valid entries for armour equipment slots are: Head, Neck, Torso, Hands, Waist, Legs, Feet, Held and Shield.");
            InventoryItem newArmour = new InventoryItem();
            newArmour.ItemType = ItemType.Armour;
            newArmour.AppliedBuffs = new List<string>();
            desc.Send(sb.ToString());
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newArmour.Id}");
                sb.AppendLine($"Item Name: {newArmour.Name}");
                sb.AppendLine($"Short Description: {newArmour.ShortDescription}");
                sb.AppendLine($"Long Description: {newArmour.LongDescription}");
                sb.AppendLine($"Value: {newArmour.BaseValue}{Constants.TabStop}Armour Class Modifier: {newArmour.ArmourClassModifier}");
                sb.AppendLine($"Is Magical?: {newArmour.IsMagical}{Constants.TabStop}Armour Type: {newArmour.BaseArmourType}");
                sb.AppendLine($"Equip Slot: {newArmour.Slot}{Constants.TabStop}Required Skill: {newArmour.RequiredSkill}");
                sb.AppendLine($"Buffs: {string.Join(", ", newArmour.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {newArmour.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item ID");
                sb.AppendLine("2. Set Item Name");
                sb.AppendLine("3. Set Short Description");
                sb.AppendLine("4. Set Long Description");
                sb.AppendLine($"5. Set value{Constants.TabStop}6. Set Armour Class Modifier");
                sb.AppendLine($"7. Toggle Magical flag{Constants.NewLine}8. Set Armour Type");
                sb.AppendLine($"9. Set Equip Slot{Constants.TabStop}10. Set Required Skill");
                sb.AppendLine($"11. Add Buff{Constants.TabStop}12. Remove Buff");
                sb.AppendLine("13. Toggle Monster Only flag");
                sb.AppendLine("14. Save Armour");
                sb.AppendLine("15. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 15)
                    {
                        switch (result)
                        {
                            case 1:
                                newArmour.Id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                break;

                            case 2:
                                newArmour.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 3:
                                newArmour.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 4:
                                newArmour.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                newArmour.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                newArmour.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class modifier: ");
                                break;

                            case 7:
                                newArmour.IsMagical = !newArmour.IsMagical;
                                break;

                            case 8:
                                newArmour.BaseArmourType = GetAssetEnumValue<ArmourType>(ref desc, "Enter Armour Type: ");
                                break;

                            case 9:
                                newArmour.Slot = GetAssetEnumValue<WearSlot>(ref desc, "Enter Equip Slot: ");
                                break;

                            case 10:
                                var skillName = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if(!string.IsNullOrEmpty(skillName) && Skills.SkillExists(skillName))
                                {
                                    newArmour.RequiredSkill = Skills.GetSkill(skillName);
                                }
                                break;

                            case 11:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!newArmour.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newArmour.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newArmour.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newArmour.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 13:
                                newArmour.IsMonsterItem = !newArmour.IsMonsterItem;
                                break;

                            case 14:
                                if (ValidateArmourItem(ref desc, ref newArmour, true))
                                {
                                    if (DatabaseManager.AddNewItem(newArmour))
                                    {
                                        if (ItemManager.Instance.AddItem(newArmour.Id, newArmour))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully created new Item: {newArmour.Name} ({newArmour.Id})", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} failed to add new item {newArmour.Name} ({newArmour.Id}) to the ItemManager, it may not be available until the game restarts.", LogLevel.Warning, true);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                        Game.LogMessage($"Player {desc.Player.Name} failed to store new item {newArmour.Name} ({newArmour.Id}) in the World database.", LogLevel.Error, true);
                                    }
                                }
                                break;

                            case 15:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void CreateNewWeapon(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Weapons are used in combat by players and NPCs. They come in various types from small daggers to huge greatswords and polearms.");
            sb.AppendLine("Players can use weapons that they do not have skill with, but will suffer penalties for doing so. Bonuses are given where a player uses a weapon they are skilled with.");
            sb.AppendLine("A weapon has an ID number, name, long and short descriptions, base value, weapon type, damage dice, bonus values and a flag to determine if the item is magical or not.");
            sb.AppendLine("Magical weapons will give players the specified bonuses even if the player is not skilled with that weapon. Non-magical weapons will not give players bonuses if the player is not skilled.");
            sb.AppendLine("In both cases, penalties will apply if the player is not skilled.");
            sb.AppendLine("Valid types for weapons are: Dagger, Sword, GreatSword, Axe, Staff, Club, Bow, Crossbow and Polearm.");
            desc.Send(sb.ToString());
            InventoryItem newWeapon = new InventoryItem();
            newWeapon.IsMagical = false;
            newWeapon.ItemType = ItemType.Weapon;
            newWeapon.Slot = WearSlot.Weapon;
            newWeapon.AppliedBuffs = new List<string>();
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newWeapon.Id}");
                sb.AppendLine($"Item Name: {newWeapon.Name}");
                sb.AppendLine($"Short Description: {newWeapon.ShortDescription}");
                sb.AppendLine($"Long Description: {newWeapon.LongDescription}");
                sb.AppendLine($"Value: {newWeapon.BaseValue}");
                sb.AppendLine($"No. of Damage Dice: {newWeapon.NumberOfDamageDice}");
                sb.AppendLine($"Size of Damage Dice: {newWeapon.SizeOfDamageDice}");
                sb.AppendLine($"Hit Bonus: {newWeapon.HitModifier}{Constants.TabStop}Damage Bonus: {newWeapon.DamageModifier}");
                sb.AppendLine($"Is Magical?: {newWeapon.IsMagical}{Constants.TabStop}Two-Handed: {newWeapon.IsTwoHanded}");
                sb.AppendLine($"Weapon Type: {newWeapon.BaseWeaponType}{Constants.TabStop}Required Skill: {newWeapon.RequiredSkill}");
                sb.AppendLine($"Buffs: {string.Join(", ", newWeapon.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {newWeapon.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item ID");
                sb.AppendLine("2. Set Item Name");
                sb.AppendLine("3. Set Short Description");
                sb.AppendLine("4. Set Long Description");
                sb.AppendLine("5. Set value");
                sb.AppendLine($"6. Set number of damage dice{Constants.TabStop}7. Set Size of Damage Dice");
                sb.AppendLine($"8. Set hit bonus{Constants.TabStop}9. Set Damage Bonus");
                sb.AppendLine($"10. Toggle Magical flag{Constants.TabStop}11. Toggle Two-Handed");
                sb.AppendLine($"12. Set Weapon Type{Constants.TabStop}13. Set Required Skill");
                sb.AppendLine($"14. Add Buff{Constants.TabStop}15. Remove Buff");
                sb.AppendLine("16. Toggle Monster Only flag");
                sb.AppendLine("17. Save Weapon");
                sb.AppendLine("18. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 18)
                    {
                        switch (result)
                        {
                            case 1:
                                newWeapon.Id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                break;

                            case 2:
                                newWeapon.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 3:
                                newWeapon.ShortDescription = GetAssetStringValue(ref desc, "Enter short description: ");
                                break;

                            case 4:
                                newWeapon.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                newWeapon.BaseValue = GetAssetUintValue(ref desc, "Enter value: ");
                                break;

                            case 6:
                                newWeapon.NumberOfDamageDice = GetAssetUintValue(ref desc, "Number of damage dice: ");
                                break;

                            case 7:
                                newWeapon.SizeOfDamageDice = GetAssetUintValue(ref desc, "Size of damage dice: ");
                                break;

                            case 8:
                                newWeapon.HitModifier = GetAssetIntegerValue(ref desc, "Hit modifier: ");
                                break;

                            case 9:
                                newWeapon.DamageModifier = GetAssetIntegerValue(ref desc, "Damage modifier: ");
                                break;

                            case 10:
                                newWeapon.IsMagical = !newWeapon.IsMagical;
                                break;

                            case 11:
                                newWeapon.IsTwoHanded = !newWeapon.IsTwoHanded;
                                break;

                            case 12:
                                newWeapon.BaseWeaponType = GetAssetEnumValue<WeaponType>(ref desc, "Enter weapon type: ");
                                break;

                            case 13:
                                var skillName = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if(!string.IsNullOrEmpty(skillName) && Skills.SkillExists(skillName))
                                {
                                    newWeapon.RequiredSkill = Skills.GetSkill(skillName);
                                }
                                break;

                            case 14:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!newWeapon.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newWeapon.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 15:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newWeapon.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newWeapon.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 16:
                                newWeapon.IsMonsterItem = !newWeapon.IsMonsterItem;
                                break;

                            case 17:
                                if (ValidateWeaponItem(ref desc, ref newWeapon, true))
                                {
                                    if (DatabaseManager.AddNewItem(newWeapon))
                                    {
                                        if (ItemManager.Instance.AddItem(newWeapon.Id, newWeapon))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully created new Item: {newWeapon.Name} ({newWeapon.Id})", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} failed to add new item {newWeapon.Name} ({newWeapon.Id}) to the ItemManager, it may not be available until the game restarts.", LogLevel.Warning, true);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                        Game.LogMessage($"Player {desc.Player.Name} failed to store new item {newWeapon.Name} ({newWeapon.Id}) in the World database.", LogLevel.Error, true);
                                    }
                                }
                                break;

                            case 18:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void CreateNewJunkItem(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Junk items are things that serve no real purpose to the adventurers of Etrea, but they can still be traded or given away, or sold to top up one's coin purse.");
            sb.AppendLine("A junk item has an ID number, name, long and short descriptions and a base value. All properties except the ID can be changed later in other areas of OLC.");
            desc.Send(sb.ToString());
            InventoryItem newItem = new InventoryItem();
            newItem.Slot = WearSlot.None;
            newItem.ItemType = ItemType.Junk;
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newItem.Id}");
                sb.AppendLine($"Item Name: {newItem.Name}");
                sb.AppendLine($"Short Description: {newItem.ShortDescription}");
                sb.AppendLine($"Long Description: {newItem.LongDescription}");
                sb.AppendLine($"Value: {newItem.BaseValue}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item ID");
                sb.AppendLine("2. Set Item Name");
                sb.AppendLine("3. Set Short Description");
                sb.AppendLine("4. Set Long Description");
                sb.AppendLine("5. Set Value");
                sb.AppendLine("6. Save Item");
                sb.AppendLine("7. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 7)
                    {
                        switch (result)
                        {
                            case 1:
                                newItem.Id = GetAssetUintValue(ref desc, "Enter Item ID: ");
                                break;

                            case 2:
                                newItem.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 3:
                                newItem.ShortDescription = GetAssetStringValue(ref desc, "Enter short description: ");
                                break;

                            case 4:
                                newItem.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                newItem.BaseValue = GetAssetUintValue(ref desc, "Enter value: ");
                                break;

                            case 6:
                                if (ValidateJunkItem(ref desc, ref newItem, true))
                                {
                                    if (DatabaseManager.AddNewItem(newItem))
                                    {
                                        if (ItemManager.Instance.AddItem(newItem.Id, newItem))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully created new Item: {newItem.Name} ({newItem.Id})", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} failed to add new item {newItem.Name} ({newItem.Id}) to the ItemManager, it may not be available until the game restarts.", LogLevel.Warning, true);
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                        Game.LogMessage($"Player {desc.Player.Name} failed to store new item {newItem.Name} ({newItem.Id}) in the World database.", LogLevel.Error, true);
                                    }
                                }
                                break;

                            case 7:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region EditItems
        private static void EditScroll(ref Descriptor desc, InventoryItem s)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {s.Id}");
                sb.AppendLine($"Item Name: {s.Name}");
                sb.AppendLine($"Short Description: {s.ShortDescription}");
                sb.AppendLine($"Long Description: {s.LongDescription}");
                sb.AppendLine($"Value: {s.BaseValue}");
                sb.AppendLine($"Casts Spell: {s.CastsSpell}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item Name");
                sb.AppendLine("2. Set Short Description");
                sb.AppendLine("3. Set Long Description");
                sb.AppendLine("4. Set Spell");
                sb.AppendLine("5. Save Scroll");
                sb.AppendLine("6. Exit Without Saving");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if(option >= 1 && option <= 6)
                    {
                        switch(option)
                        {
                            case 1:
                                s.Name = GetAssetStringValue(ref desc, "Enter Scroll Name:");
                                break;

                            case 2:
                                s.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description:");
                                break;

                            case 3:
                                s.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 4:
                                var spn = GetAssetStringValue(ref desc, "Enter Spell Name:");
                                var sp = Spells.GetSpell(spn);
                                if(sp != null)
                                {
                                    s.CastsSpell = sp.SpellName;
                                }
                                break;

                            case 5:
                                if(ValidateScrollItem(ref desc, ref s, false))
                                {
                                    if(DatabaseManager.UpdateItemByID(ref desc, ref s))
                                    {
                                        if(ItemManager.Instance.UpdateItemByID(s.Id, ref desc, s))
                                        {
                                            desc.Send($"Scroll updated successfully.{Constants.NewLine}");
                                            Game.LogMessage($"INFO: Player {desc.Player} updated Scroll '{s.Name}' (ID: {s.Id}) in World Database and ItemManager", LogLevel.Info, true);
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Unable to update item in ItemManager, it may not be available until after a restart.{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Unable to update item in World Database.{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 6:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void EditJunk(ref Descriptor desc, InventoryItem j)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {j.Id}");
                sb.AppendLine($"Item Name: {j.Name}");
                sb.AppendLine($"Short Description: {j.ShortDescription}");
                sb.AppendLine($"Long Description: {j.LongDescription}");
                sb.AppendLine($"Value: {j.BaseValue}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item Name");
                sb.AppendLine("2. Set Short Description");
                sb.AppendLine("3. Set Long Description");
                sb.AppendLine("4. Set Value");
                sb.AppendLine("5. Save Item");
                sb.AppendLine("6. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 6)
                    {
                        switch(result)
                        {
                            case 1:
                                j.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 2:
                                j.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 3:
                                j.ShortDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 4:
                                j.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 5:
                                if (ValidateJunkItem(ref desc, ref j, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref j))
                                    {
                                        if (ItemManager.Instance.UpdateItemByID(j.Id, ref desc, j))
                                        {
                                            desc.Send($"Item has been updated successfully{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"There was an error updating the item in the World database{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"There was an error updating the item in Item Manager{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 6:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void EditWeapon(ref Descriptor desc, InventoryItem w)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {w.Id}");
                sb.AppendLine($"Item Name: {w.Name}");
                sb.AppendLine($"Short Description: {w.ShortDescription}");
                sb.AppendLine($"Long Description: {w.LongDescription}");
                sb.AppendLine($"Value: {w.BaseValue}");
                sb.AppendLine($"No. of Damage Dice: {w.NumberOfDamageDice}");
                sb.AppendLine($"Size of Damage Dice: {w.SizeOfDamageDice}");
                sb.AppendLine($"Hit Bonus: {w.HitModifier}{Constants.TabStop}Damage Bonus: {w.DamageModifier}");
                sb.AppendLine($"Is Magical?: {w.IsMagical}{Constants.TabStop}Two-Handed: {w.IsTwoHanded}");
                sb.AppendLine($"Weapon Type: {w.BaseWeaponType}{Constants.TabStop}Required Skill: {w.RequiredSkill}");
                sb.AppendLine($"Buffs: {string.Join(", ", w.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {w.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item Name");
                sb.AppendLine("2. Set Short Description");
                sb.AppendLine("3. Set Long Description");
                sb.AppendLine("4. Set value");
                sb.AppendLine($"5. Set number of damage dice{Constants.TabStop}6. Set Size of Damage Dice");
                sb.AppendLine($"7. Set hit bonus{Constants.TabStop}8. Set Damage Bonus");
                sb.AppendLine($"9. Toggle Magical flag{Constants.TabStop}10. Toggle Two-Handed");
                sb.AppendLine($"11. Set Weapon Type{Constants.TabStop}12. Set Required Skill");
                sb.AppendLine($"13. Add Buff{Constants.TabStop}14. Remove Buff");
                sb.AppendLine("15. Toggle Monster Only flag");
                sb.AppendLine("16. Save Weapon");
                sb.AppendLine("17. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 17)
                    {
                        switch(result)
                        {
                            case 1:
                                w.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 2:
                                w.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 3:
                                w.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 4:
                                w.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 5:
                                w.NumberOfDamageDice = GetAssetUintValue(ref desc, "Enter Number of Damage Dice: ");
                                break;

                            case 6:
                                w.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter Size of Damage Dice: ");
                                break;

                            case 7:
                                w.HitModifier = GetAssetIntegerValue(ref desc, "Enter Hit Modifier: ");
                                break;

                            case 8:
                                w.DamageModifier = GetAssetIntegerValue(ref desc, "Enter Damage Modifier: ");
                                break;

                            case 9:
                                w.IsMagical = !w.IsMagical;
                                break;

                            case 10:
                                w.IsTwoHanded = !w.IsTwoHanded;
                                break;

                            case 11:
                                w.BaseWeaponType = GetAssetEnumValue<WeaponType>(ref desc, "Enter Weapon Type: ");
                                break;

                            case 12:
                                var skillName = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if(!string.IsNullOrEmpty(skillName) && Skills.SkillExists(skillName))
                                {
                                    w.RequiredSkill = Skills.GetSkill(skillName);
                                }
                                break;

                            case 13:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!w.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        w.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 14:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (w.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        w.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 15:
                                w.IsMonsterItem = !w.IsMonsterItem;
                                break;

                            case 16:
                                if (ValidateWeaponItem(ref desc, ref w, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref w))
                                    {
                                        if (ItemManager.Instance.UpdateItemByID(w.Id, ref desc, w))
                                        {
                                            desc.Send($"Item has been updated successfully{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"There was an error updating the item in the World database{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"There was an error updating the item in Item Manager{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 17:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void EditArmour(ref Descriptor desc, InventoryItem a)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {a.Id}");
                sb.AppendLine($"Item Name: {a.Name}");
                sb.AppendLine($"Short Description: {a.ShortDescription}");
                sb.AppendLine($"Long Description: {a.LongDescription}");
                sb.AppendLine($"Value: {a.BaseValue}{Constants.TabStop}Armour Class Modifier: {a.ArmourClassModifier}");
                sb.AppendLine($"Is Magical?: {a.IsMagical}{Constants.NewLine}Armour Type: {a.BaseArmourType}");
                sb.AppendLine($"Required Skill: {a.RequiredSkill}{Constants.TabStop}Equip Slot: {a.Slot}");
                sb.AppendLine($"Buffs: {string.Join(", ", a.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {a.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item Name");
                sb.AppendLine("2. Set Short Description");
                sb.AppendLine("3. Set Long Description");
                sb.AppendLine($"4. Set value{Constants.TabStop}5. Set Armour Class Modifier");
                sb.AppendLine($"6. Toggle Magical flag{Constants.TabStop}7. Set Armour Type");
                sb.AppendLine($"8. Set Required Skill{Constants.TabStop}9. Set Equip Slot");
                sb.AppendLine($"10. Add Buff{Constants.TabStop}11. Remove Buff");
                sb.AppendLine("12. Toggle Monster Only flag");
                sb.AppendLine("13. Save Armour");
                sb.AppendLine("14. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 14)
                    {
                        switch(result)
                        {
                            case 1:
                                a.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 2:
                                a.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 3:
                                a.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 4:
                                a.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 5:
                                a.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class Modifier: ");
                                break;

                            case 6:
                                a.IsMagical = !a.IsMagical;
                                break;

                            case 7:
                                a.BaseArmourType = GetAssetEnumValue<ArmourType>(ref desc, "Enter Armour Type: ");
                                break;

                            case 8:
                                var skillName = GetAssetStringValue(ref desc, "Enter Require Skill: ");
                                if(!string.IsNullOrEmpty(skillName) && Skills.SkillExists(skillName))
                                {
                                    a.RequiredSkill = Skills.GetSkill(skillName);
                                }
                                break;

                            case 9:
                                a.Slot = GetAssetEnumValue<WearSlot>(ref desc, "Enter Equip Slot: ");
                                break;

                            case 10:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!a.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        a.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 11:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (a.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        a.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                a.IsMonsterItem = !a.IsMonsterItem;
                                break;

                            case 13:
                                if (ValidateArmourItem(ref desc, ref a, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref a))
                                    {
                                        if (ItemManager.Instance.UpdateItemByID(a.Id, ref desc, a))
                                        {
                                            desc.Send($"Item has been updated successfully{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"There was an error updating the item in the World database{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"There was an error updating the item in Item Manager{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 14:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void EditPotion(ref Descriptor desc, InventoryItem p)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {p.Id}");
                sb.AppendLine($"Item Name: {p.Name}");
                sb.AppendLine($"Short Description: {p.ShortDescription}");
                sb.AppendLine($"Long Description: {p.LongDescription}");
                sb.AppendLine($"Value: {p.BaseValue}");
                sb.AppendLine($"Potion Effect: {p.PotionEffect}");
                sb.AppendLine($"Number of Damage Dice: {p.NumberOfDamageDice}{Constants.TabStop}Size of Damage Dice: {p.SizeOfDamageDice}");
                sb.AppendLine($"Is Magical?: {p.IsMagical}");
                sb.AppendLine($"Buffs: {string.Join(", ", p.AppliedBuffs)}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item Name");
                sb.AppendLine("2. Set Short Description");
                sb.AppendLine("3. Set Long Description");
                sb.AppendLine($"4. Set Value{Constants.TabStop}5. Set Potion Effect");
                sb.AppendLine($"6. Set Number of Damage Dice{Constants.TabStop}7. Set Size of Damage Dice");
                sb.AppendLine("8. Toggle Magical flag");
                sb.AppendLine($"9. Add Buff{Constants.TabStop}10. Remove Buff");
                sb.AppendLine("11. Save Potion");
                sb.AppendLine("12. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 12)
                    {
                        switch (result)
                        {
                            case 1:
                                p.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 2:
                                p.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 3:
                                p.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 4:
                                p.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 5:
                                p.PotionEffect = GetAssetEnumValue<PotionEffect>(ref desc, "Enter Potion Effect: ");
                                break;

                            case 6:
                                p.NumberOfDamageDice = GetAssetUintValue(ref desc, "Enter Number of Damage Dice: ");
                                break;

                            case 7:
                                p.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter Size of Damage Dice: ");
                                break;

                            case 8:
                                p.IsMagical = !p.IsMagical;
                                break;

                            case 9:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!p.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        p.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 10:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (p.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        p.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 11:
                                if (ValidatePotionItem(ref desc, ref p, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref p))
                                    {
                                        if (ItemManager.Instance.UpdateItemByID(p.Id, ref desc, p))
                                        {
                                            desc.Send($"Item has been updated successfully{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"There was an error updating the item in the World database{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"There was an error updating the item in Item Manager{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 12:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }

        private static void EditRing(ref Descriptor desc, InventoryItem r)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {r.Id}");
                sb.AppendLine($"Item Name: {r.Name}");
                sb.AppendLine($"Short Description: {r.ShortDescription}");
                sb.AppendLine($"Long Description: {r.LongDescription}");
                sb.AppendLine($"Value: {r.BaseValue}{Constants.TabStop}Armour Class Modifier: {r.ArmourClassModifier}");
                sb.AppendLine($"Hit Bonus: {r.HitModifier}{Constants.TabStop}Damage Bonus: {r.DamageModifier}");
                sb.AppendLine($"Is Magical?: {r.IsMagical}");
                sb.AppendLine($"Buffs: {string.Join(", ", r.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {r.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine("1. Set Item Name");
                sb.AppendLine("2. Set Short Description");
                sb.AppendLine("3. Set Long Description");
                sb.AppendLine($"4. Set Value{Constants.TabStop}5. Set Armour Class Modifier");
                sb.AppendLine($"6. Set Hit Bonus{Constants.TabStop}7. Set Damage Bonus");
                sb.AppendLine("8. Toggle Magical flag");
                sb.AppendLine($"9. Add Buff{Constants.TabStop}10. Remove Buff");
                sb.AppendLine("11. Toggle Monster Only flag");
                sb.AppendLine("12. Save changes");
                sb.AppendLine("13. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if(result >= 1 && result <= 13)
                    {
                        switch(result)
                        {
                            case 1:
                                r.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 2:
                                r.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 3:
                                r.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 4:
                                r.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 5:
                                r.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class Modifier: ");
                                break;

                            case 6:
                                r.HitModifier = GetAssetIntegerValue(ref desc, "Enter Hit Modifier: ");
                                break;

                            case 7:
                                r.DamageModifier = GetAssetIntegerValue(ref desc, "Enter Damage Modifier: ");
                                break;

                            case 8:
                                r.IsMagical = !r.IsMagical;
                                break;

                            case 9:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = Buffs.GetBuff(b);
                                if(buff != null)
                                {
                                    if(!r.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        r.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 10:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = Buffs.GetBuff(b);
                                if (buff != null)
                                {
                                    if (r.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        r.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 11:
                                r.IsMonsterItem = !r.IsMonsterItem;
                                break;

                            case 12:
                                if(ValidateRingItem(ref desc, ref r, false))
                                {
                                    if(DatabaseManager.UpdateItemByID(ref desc, ref r))
                                    {
                                        if(ItemManager.Instance.UpdateItemByID(r.Id, ref desc, r))
                                        {
                                            desc.Send($"Item has been updated successfully{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"There was an error updating the item in the World database{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"There was an error updating the item in Item Manager{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 13:
                                okToReturn = true;
                                break;
                        }
                    }
                    else
                    {
                        desc.Send($"{Constants.InvalidChoice}{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region DeleteItems
        private static void DeleteItem(ref Descriptor desc)
        {
            desc.Send("Enter the ID of the item to delete: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
            {
                var i = ItemManager.Instance.GetItemByID(result);
                if(i != null)
                {
                    if(DatabaseManager.DeleteItemByID(ref desc, i.Id))
                    {
                        if(ItemManager.Instance.RemoveItemByID(i.Id, ref desc))
                        {
                            desc.Send($"Item has been successfully removed from Item Manager and the World database{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"Could not delete item from Item Manager{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Could not delete item from World database{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"No item with that ID could be found in Item Manager{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Functions
        private static bool ValidateScrollItem(ref Descriptor desc, ref InventoryItem s, bool isNewItem)
        {
            bool isValid = true;
            if(ItemManager.Instance.ItemExists(s.Id) && isNewItem)
            {
                isValid = false;
                desc.Send($"The specified ID is already in use.{Constants.NewLine}");
            }
            if(s.Id == 0)
            {
                isValid = false;
                desc.Send($"The specified ID is not valid.{Constants.NewLine}");
            }
            if(string.IsNullOrEmpty(s.Name) || string.IsNullOrEmpty(s.ShortDescription) || string.IsNullOrEmpty(s.LongDescription) || string.IsNullOrEmpty(s.CastsSpell))
            {
                isValid = false;
                desc.Send($"One or more attributes are missing values.{Constants.NewLine}");
            }
            return isValid;
        }

        private static bool ValidatePotionItem(ref Descriptor desc, ref InventoryItem p, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(p.Id) && p.Id != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(p.Name) || string.IsNullOrEmpty(p.ShortDescription) || string.IsNullOrEmpty(p.LongDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
                else
                {
                    if(p.PotionEffect == PotionEffect.Healing || p.PotionEffect == PotionEffect.Poison)
                    {
                        if(p.NumberOfDamageDice == 0 || p.SizeOfDamageDice == 0)
                        {
                            desc.Send($"If a Potion poisons or heals it needs to have damage dice{Constants.NewLine}");
                            isValid = false;
                        }
                    }
                    else
                    {
                        if(p.PotionEffect == PotionEffect.None || p.PotionEffect == PotionEffect.Buff)
                        {
                            if(p.NumberOfDamageDice > 0 || p.SizeOfDamageDice > 0)
                            {
                                desc.Send($"Potions which apply buffs or have no effect do not need damage dice{Constants.NewLine}");
                                isValid = false;
                            }
                        }
                    }
                }
            }
            else
            {
                desc.Send($"Item ID was 0 or the Item ID is already in use{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }

        private static bool ValidateRingItem(ref Descriptor desc, ref InventoryItem r, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(r.Id) && r.Id != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(r.Name) || string.IsNullOrEmpty(r.ShortDescription) || string.IsNullOrEmpty(r.LongDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
            }
            else
            {
                desc.Send($"Item ID was 0 or the Item ID is already in use{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }

        private static bool ValidateArmourItem(ref Descriptor desc, ref InventoryItem a, bool isNewItem)
        {
            bool isValid = true;
            if((isNewItem && !ItemManager.Instance.ItemExists(a.Id) && a.Id != 0) || !isNewItem)
            {
                if(string.IsNullOrEmpty(a.Name) || string.IsNullOrEmpty(a.ShortDescription) || string.IsNullOrEmpty(a.LongDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
            }
            else
            {
                desc.Send($"Item ID was 0 or the Item ID is already in use{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }

        private static bool ValidateWeaponItem(ref Descriptor desc, ref InventoryItem w, bool isNewItem)
        {
            bool isValid = true;
            if((isNewItem && !ItemManager.Instance.ItemExists(w.Id) && w.Id != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(w.Name) || string.IsNullOrEmpty(w.ShortDescription) || string.IsNullOrEmpty(w.LongDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
                else
                {
                    if(w.NumberOfDamageDice == 0 || w.SizeOfDamageDice == 0)
                    {
                        desc.Send($"Number and size of damage dice must be greater than 0{Constants.NewLine}");
                        isValid = false;
                    }
                }
            }
            else
            {
                desc.Send($"Item ID was 0 or the Item ID is already in use{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }

        private static bool ValidateJunkItem(ref Descriptor desc, ref InventoryItem i, bool isNewItem)
        {
            bool isValid = true;
            if((isNewItem && !ItemManager.Instance.ItemExists(i.Id) && i.Id != 0) || !isNewItem)
            {
                if(string.IsNullOrEmpty(i.Name) || string.IsNullOrEmpty(i.LongDescription) || string.IsNullOrEmpty(i.ShortDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
            }
            else
            {
                desc.Send($"Item ID was 0 or the Item ID is already in use{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }
        #endregion
    }
}