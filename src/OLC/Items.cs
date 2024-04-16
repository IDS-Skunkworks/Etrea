using Etrea2.Core;
using Etrea2.Entities;
using System.Collections.Generic;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        private static void EditExistingItem(ref Descriptor desc)
        {
            desc.Send("Enter the ID of the item to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint id))
            {
                if (ItemManager.Instance.ItemExists(id))
                {
                    var item = ItemManager.Instance.GetItemByID(id).ShallowCopy();
                    if (item.AppliedBuffs == null)
                    {
                        item.AppliedBuffs = new List<string>();
                    }
                    if (item != null)
                    {
                        switch (item.ItemType)
                        {
                            case ItemType.Ring:
                                EditRing(ref desc, item);
                                break;

                            case ItemType.Consumable:
                                EditConsumable(ref desc, item);
                                break;

                            case ItemType.Armour:
                                EditArmour(ref desc, item);
                                break;

                            case ItemType.Weapon:
                                EditWeapon(ref desc, item);
                                break;

                            case ItemType.Misc:
                                EditMiscItem(ref desc, item);
                                break;

                            case ItemType.Scroll:
                                EditScroll(ref desc, item);
                                break;
                        }
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
                sb.AppendLine($"1. Misc{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}2. Weapon");
                sb.AppendLine($"3. Armour{Constants.TabStop}{Constants.TabStop}4. Ring");
                sb.AppendLine($"5. Scroll{Constants.TabStop}{Constants.TabStop}6. Potion");
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
                                CreateNewMiscItem(ref desc);
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
                                CreateNewConsumable(ref desc);
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

        #region Create Items
        internal static void CreateNewConsumable(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Consumables are single-use items which are usable by all players.");
            InventoryItem newPotion = new InventoryItem
            {
                ItemType = ItemType.Consumable,
                AppliedBuffs = new List<string>()
            };
            desc.Send(sb.ToString());
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newPotion.ID}{Constants.TabStop}Item Name: {newPotion.Name}");
                sb.AppendLine($"Short Description: {newPotion.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{newPotion.LongDescription}");
                sb.AppendLine($"Value: {newPotion.BaseValue}{Constants.TabStop}{Constants.TabStop}Potion Effect: {newPotion.ConsumableEffect}");
                sb.AppendLine($"Number of Damage Dice: {newPotion.NumberOfDamageDice}{Constants.TabStop}Size of Damage Dice: {newPotion.SizeOfDamageDice}");
                sb.AppendLine($"Is Magical?: {newPotion.IsMagical}");
                sb.AppendLine($"Buffs: {string.Join(", ", newPotion.AppliedBuffs)}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}2. Set Item Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Edit Long Description{Constants.TabStop}6. Set Value{Constants.TabStop}7. Set Potion Effect");
                sb.AppendLine($"8. Set Number of Damage Dice{Constants.TabStop}9. Set Size of Damage Dice");
                sb.AppendLine("10. Toggle Magical flag");
                sb.AppendLine($"111. Add Buff{Constants.TabStop}12. Remove Buff");
                sb.AppendLine($"13. Save{Constants.TabStop}{Constants.TabStop}14. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 14)
                    {
                        switch (result)
                        {
                            case 1:
                                newPotion.ID = GetAssetUintValue(ref desc, "Enter Item ID: ");
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
                                newPotion.LongDescription = Helpers.EditLongDescription(ref desc, newPotion.LongDescription);
                                break;

                            case 6:
                                newPotion.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 7:
                                newPotion.ConsumableEffect = GetAssetEnumValue<ConsumableEffect>(ref desc, "Enter Effect: ");
                                break;

                            case 8:
                                newPotion.NumberOfDamageDice = GetAssetUintValue(ref desc, "Enter number of Damage Dice: ");
                                break;

                            case 9:
                                newPotion.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter size of Damage Dice: ");
                                break;

                            case 10:
                                newPotion.IsMagical = !newPotion.IsMagical;
                                break;

                            case 11:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!newPotion.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newPotion.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newPotion.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newPotion.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 13:
                                if (ValidateConsumableItem(ref desc, ref newPotion, true))
                                {
                                    if (DatabaseManager.AddNewItem(newPotion, ref desc))
                                    {
                                        if (ItemManager.Instance.AddItem(ref desc, newPotion))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
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

        internal static void CreateNewScroll(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Scrolls are single-use pieces of parchment containing the essence of a spell.");
            InventoryItem newScroll = new InventoryItem
            {
                ItemType = ItemType.Scroll,
                CastsSpell = string.Empty,
                Slot = WearSlot.None
            };
            desc.Send(sb.ToString());
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newScroll.ID}{Constants.TabStop}Item Name: {newScroll.Name}");
                sb.AppendLine($"Short Description: {newScroll.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{newScroll.LongDescription}");
                sb.AppendLine($"Value: {newScroll.BaseValue}");
                sb.AppendLine($"Casts Spell: {newScroll.CastsSpell}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}{Constants.TabStop}2. Set Item Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Edit Long Description{Constants.TabStop}6. Set Spell");
                sb.AppendLine($"7. Save{Constants.TabStop}8. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option > 0 && option <= 8)
                    {
                        switch (option)
                        {
                            case 1:
                                newScroll.ID = GetAssetUintValue(ref desc, "Enter Scroll ID: ");
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
                                newScroll.LongDescription = Helpers.EditLongDescription(ref desc, newScroll.LongDescription);
                                break;

                            case 6:
                                var spn = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                var sp = SpellManager.Instance.GetSpell(spn);
                                if (sp != null)
                                {
                                    newScroll.CastsSpell = sp.SpellName;
                                }
                                break;

                            case 7:
                                if (ValidateScrollItem(ref desc, ref newScroll, true))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref newScroll))
                                    {
                                        if (ItemManager.Instance.UpdateItem(newScroll.ID, ref desc, newScroll))
                                        {
                                            desc.Send($"Scroll added successfully.{Constants.NewLine}");
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

                            case 8:
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
            sb.AppendLine("Rings are items of jewelry that can be worn on the fingers.");
            InventoryItem newRing = new InventoryItem
            {
                ItemType = ItemType.Ring,
                Slot = WearSlot.FingerLeft | WearSlot.FingerRight,
                AppliedBuffs = new List<string>()
            };
            desc.Send(sb.ToString());
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newRing.ID}{Constants.TabStop}Item Name: {newRing.Name}");
                sb.AppendLine($"Short Description: {newRing.ShortDescription}");
                sb.AppendLine($"Long Description: {newRing.LongDescription}");
                sb.AppendLine($"Value: {newRing.BaseValue}{Constants.TabStop}Armour Class Modifier: {newRing.ArmourClassModifier}");
                sb.AppendLine($"Hit Bonus: {newRing.HitModifier}{Constants.TabStop}Damage Bonus: {newRing.DamageModifier}");
                sb.AppendLine($"Is Magical?: {newRing.IsMagical}{Constants.TabStop}Monster Only?: {newRing.IsMonsterItem}");
                sb.AppendLine($"IS Cursed?: {newRing.IsCursed}");
                sb.AppendLine($"Buffs: {string.Join(", ", newRing.AppliedBuffs)}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}2. Set Item Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Edit Long Description{Constants.TabStop}{Constants.TabStop}6. Set Value{Constants.TabStop}7. Set Armour Class Modifier");
                sb.AppendLine($"8. Set Hit Bonus{Constants.TabStop}9. Set Damage Bonus");
                sb.AppendLine($"10. Toggle Magical flag{Constants.TabStop}11. Toggle Monster Only flag");
                sb.AppendLine($"12. Add Buff{Constants.TabStop}13. Remove Buff");
                sb.AppendLine($"14. Toggle Curse flag");
                sb.AppendLine($"15. Save{Constants.TabStop}16. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 0 && result <= 16)
                    {
                        switch (result)
                        {
                            case 1:
                                newRing.ID = GetAssetUintValue(ref desc, "Enter Item ID: ");
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
                                newRing.LongDescription = Helpers.EditLongDescription(ref desc, newRing.LongDescription);
                                break;

                            case 6:
                                newRing.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 7:
                                newRing.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class Modifier: ");
                                break;

                            case 8:
                                newRing.HitModifier = GetAssetIntegerValue(ref desc, "Enter Hit Modifier: ");
                                break;

                            case 9:
                                newRing.DamageModifier = GetAssetIntegerValue(ref desc, "Enter Damage Modifier: ");
                                break;

                            case 10:
                                newRing.IsMagical = !newRing.IsMagical;
                                break;

                            case 11:
                                newRing.IsMonsterItem = !newRing.IsMonsterItem;
                                break;

                            case 12:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!newRing.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newRing.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 13:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newRing.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newRing.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 14:
                                newRing.IsCursed = !newRing.IsCursed;
                                break;

                            case 15:
                                if (ValidateRingItem(ref desc, ref newRing, true))
                                {
                                    if (DatabaseManager.AddNewItem(newRing, ref desc))
                                    {
                                        if (ItemManager.Instance.AddItem(ref desc, newRing))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 16:
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
            sb.AppendLine("Armour is designed to protect during combat by making the wearer harder to hit and damage.");
            sb.AppendLine("Valid entries for armour equipment slots are: Head, Neck, Torso, Hands, Waist, Legs, Feet, Held and Shield.");
            InventoryItem newArmour = new InventoryItem
            {
                ItemType = ItemType.Armour,
                AppliedBuffs = new List<string>()
            };
            desc.Send(sb.ToString());
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newArmour.ID}{Constants.TabStop}Item Name: {newArmour.Name}");
                sb.AppendLine($"Short Description: {newArmour.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{newArmour.LongDescription}");
                sb.AppendLine($"Value: {newArmour.BaseValue}{Constants.TabStop}Armour Class Modifier: {newArmour.ArmourClassModifier}");
                sb.AppendLine($"Is Magical?: {newArmour.IsMagical}{Constants.TabStop}Armour Type: {newArmour.BaseArmourType}");
                sb.AppendLine($"Equip Slot: {newArmour.Slot}{Constants.TabStop}Required Skill: {newArmour.RequiredSkill}");
                sb.AppendLine($"Damage Reduction: {newArmour.DamageReductionModifier}");
                sb.AppendLine($"Buffs: {string.Join(", ", newArmour.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {newArmour.IsMonsterItem}{Constants.TabStop}Is Cursed?: {newArmour.IsCursed}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}2. Set Item Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Edit Long Description{Constants.TabStop}6. Set value{Constants.TabStop}7. Set Armour Class Modifier");
                sb.AppendLine($"8. Toggle Magical flag{Constants.NewLine}9. Set Armour Type");
                sb.AppendLine($"10. Set Equip Slot{Constants.TabStop}11. Set Required Skill");
                sb.AppendLine($"12. Add Buff{Constants.TabStop}13. Remove Buff");
                sb.AppendLine($"14. Set Damage Reduction{Constants.TabStop}15. Toggle Monster Only");
                sb.AppendLine($"16. Toggle Cursed flag");
                sb.AppendLine($"17. Save{Constants.TabStop}18. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 18)
                    {
                        switch (result)
                        {
                            case 1:
                                newArmour.ID = GetAssetUintValue(ref desc, "Enter Item ID: ");
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
                                newArmour.LongDescription = Helpers.EditLongDescription(ref desc, newArmour.LongDescription);
                                break;

                            case 6:
                                newArmour.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 7:
                                newArmour.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class modifier: ");
                                break;

                            case 8:
                                newArmour.IsMagical = !newArmour.IsMagical;
                                break;

                            case 9:
                                newArmour.BaseArmourType = GetAssetEnumValue<ArmourType>(ref desc, "Enter Armour Type: ");
                                break;

                            case 10:
                                newArmour.Slot = GetAssetEnumValue<WearSlot>(ref desc, "Enter Equip Slot: ");
                                break;

                            case 11:
                                var skillName = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if (!string.IsNullOrEmpty(skillName) && SkillManager.Instance.SkillExists(skillName))
                                {
                                    newArmour.RequiredSkill = SkillManager.Instance.GetSkill(skillName);
                                }
                                break;

                            case 12:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!newArmour.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newArmour.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 13:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newArmour.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newArmour.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 14:
                                newArmour.DamageReductionModifier = GetAssetUintValue(ref desc, "Enter Damage Reduction Modifier: ");
                                break;

                            case 15:
                                newArmour.IsMonsterItem = !newArmour.IsMonsterItem;
                                break;

                            case 16:
                                newArmour.IsCursed = !newArmour.IsCursed;
                                break;

                            case 17:
                                if (ValidateArmourItem(ref desc, ref newArmour, true))
                                {
                                    if (DatabaseManager.AddNewItem(newArmour, ref desc))
                                    {
                                        if (ItemManager.Instance.AddItem(ref desc, newArmour))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
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

        private static void CreateNewWeapon(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Weapons are used in combat by players and NPCs. They come in various types from small daggers to huge greatswords and polearms.");
            sb.AppendLine("Valid types for weapons are: Dagger, Sword, GreatSword, Axe, Staff, Club, Bow, Crossbow and Polearm.");
            desc.Send(sb.ToString());
            InventoryItem newWeapon = new InventoryItem
            {
                IsMagical = false,
                ItemType = ItemType.Weapon,
                Slot = WearSlot.Weapon,
                AppliedBuffs = new List<string>()
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newWeapon.ID}{Constants.TabStop}{Constants.TabStop}Item Name: {newWeapon.Name}");
                sb.AppendLine($"Short Description: {newWeapon.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{newWeapon.LongDescription}");
                sb.AppendLine($"Value: {newWeapon.BaseValue}");
                sb.AppendLine($"No. of Damage Dice: {newWeapon.NumberOfDamageDice}{Constants.TabStop}Size of Damage Dice: {newWeapon.SizeOfDamageDice}");
                sb.AppendLine($"Hit Bonus: {newWeapon.HitModifier}{Constants.TabStop}Damage Bonus: {newWeapon.DamageModifier}");
                sb.AppendLine($"Is Magical?: {newWeapon.IsMagical}{Constants.TabStop}Two-Handed: {newWeapon.IsTwoHanded}");
                sb.AppendLine($"Weapon Type: {newWeapon.BaseWeaponType}{Constants.TabStop}Required Skill: {newWeapon.RequiredSkill}");
                sb.AppendLine($"Buffs: {string.Join(", ", newWeapon.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {newWeapon.IsMonsterItem}{Constants.TabStop}Is Cursed?: {newWeapon.IsCursed}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}{Constants.TabStop}2. Set Item Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Edit Long Description{Constants.TabStop}{Constants.TabStop}6. Set value");
                sb.AppendLine($"7. Set number of damage dice{Constants.TabStop}8. Set Size of Damage Dice");
                sb.AppendLine($"9. Set hit bonus{Constants.TabStop}10. Set Damage Bonus");
                sb.AppendLine($"11. Toggle Magical flag{Constants.TabStop}12. Toggle Two-Handed");
                sb.AppendLine($"13. Set Weapon Type{Constants.TabStop}14. Set Required Skill");
                sb.AppendLine($"15. Add Buff{Constants.TabStop}16. Remove Buff");
                sb.AppendLine($"17. Toggle Monster Only flag{Constants.TabStop}18. Toggle Curse Flag");
                sb.AppendLine($"19. Save{Constants.TabStop}{Constants.TabStop}20. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 20)
                    {
                        switch (result)
                        {
                            case 1:
                                newWeapon.ID = GetAssetUintValue(ref desc, "Enter Item ID: ");
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
                                newWeapon.LongDescription = Helpers.EditLongDescription(ref desc, newWeapon.LongDescription);
                                break;

                            case 6:
                                newWeapon.BaseValue = GetAssetUintValue(ref desc, "Enter value: ");
                                break;

                            case 7:
                                newWeapon.NumberOfDamageDice = GetAssetUintValue(ref desc, "Number of damage dice: ");
                                break;

                            case 8:
                                newWeapon.SizeOfDamageDice = GetAssetUintValue(ref desc, "Size of damage dice: ");
                                break;

                            case 9:
                                newWeapon.HitModifier = GetAssetIntegerValue(ref desc, "Hit modifier: ");
                                break;

                            case 10:
                                newWeapon.DamageModifier = GetAssetIntegerValue(ref desc, "Damage modifier: ");
                                break;

                            case 11:
                                newWeapon.IsMagical = !newWeapon.IsMagical;
                                break;

                            case 12:
                                newWeapon.IsTwoHanded = !newWeapon.IsTwoHanded;
                                break;

                            case 13:
                                newWeapon.BaseWeaponType = GetAssetEnumValue<WeaponType>(ref desc, "Enter weapon type: ");
                                break;

                            case 14:
                                var skillName = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if (!string.IsNullOrEmpty(skillName) && SkillManager.Instance.SkillExists(skillName))
                                {
                                    newWeapon.RequiredSkill = SkillManager.Instance.GetSkill(skillName);
                                }
                                break;

                            case 15:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!newWeapon.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newWeapon.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 16:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (newWeapon.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        newWeapon.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 17:
                                newWeapon.IsMonsterItem = !newWeapon.IsMonsterItem;
                                break;

                            case 18:
                                newWeapon.IsCursed = !newWeapon.IsCursed;
                                break;

                            case 19:
                                if (ValidateWeaponItem(ref desc, ref newWeapon, true))
                                {
                                    if (DatabaseManager.AddNewItem(newWeapon, ref desc))
                                    {
                                        if (ItemManager.Instance.AddItem(ref desc, newWeapon))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 20:
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

        private static void CreateNewMiscItem(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Miscellaneous items can be anything from keys to junk. They may be important, or may serve no purpose at all.");
            desc.Send(sb.ToString());
            InventoryItem newItem = new InventoryItem
            {
                Slot = WearSlot.None,
                ItemType = ItemType.Misc
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {newItem.ID}{Constants.TabStop}Item Name: {newItem.Name}");
                sb.AppendLine($"Short Description: {newItem.ShortDescription}");
                sb.AppendLine($"Long Description: {newItem.LongDescription}");
                sb.AppendLine($"Value: {newItem.BaseValue}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item ID{Constants.TabStop}{Constants.TabStop}{Constants.TabStop} 2. Set Item Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Edit Long Description{Constants.TabStop}{Constants.TabStop}6.Set Value"); // TODO: Edit long desc
                sb.AppendLine($"7. Save{Constants.TabStop}{Constants.TabStop}8. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 8)
                    {
                        switch (result)
                        {
                            case 1:
                                newItem.ID = GetAssetUintValue(ref desc, "Enter Item ID: ");
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
                                newItem.LongDescription = Helpers.EditLongDescription(ref desc, newItem.LongDescription);
                                break;

                            case 6:
                                newItem.BaseValue = GetAssetUintValue(ref desc, "Enter value: ");
                                break;

                            case 7:
                                if (ValidateJunkItem(ref desc, ref newItem, true))
                                {
                                    if (DatabaseManager.AddNewItem(newItem, ref desc))
                                    {
                                        if (ItemManager.Instance.AddItem(ref desc, newItem))
                                        {
                                            desc.Send($"New item successfully added to ItemManager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new item to ItemManager, it may not be available until the game is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to store item in database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 8:
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

        #region Edit Items
        private static void EditScroll(ref Descriptor desc, InventoryItem s)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {s.ID}{Constants.TabStop}{Constants.TabStop} 2. Item Name: {s.Name}");
                sb.AppendLine($"Short Description: {s.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{s.LongDescription}");
                sb.AppendLine($"Value: {s.BaseValue}");
                sb.AppendLine($"Casts Spell: {s.CastsSpell}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}{Constants.TabStop}4. Edit Long Description");
                sb.AppendLine("5. Set Spell");
                sb.AppendLine($"6. Save{Constants.TabStop}{Constants.TabStop}7. Exit");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 7)
                    {
                        switch (option)
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
                                s.LongDescription = Helpers.EditLongDescription(ref desc, s.LongDescription);
                                break;

                            case 5:
                                var spn = GetAssetStringValue(ref desc, "Enter Spell Name:");
                                var sp = SpellManager.Instance.GetSpell(spn);
                                if (sp != null)
                                {
                                    s.CastsSpell = sp.SpellName;
                                }
                                break;

                            case 6:
                                if (ValidateScrollItem(ref desc, ref s, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref s))
                                    {
                                        if (ItemManager.Instance.UpdateItem(s.ID, ref desc, s))
                                        {
                                            desc.Send($"Scroll updated successfully.{Constants.NewLine}");
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

        private static void EditWeapon(ref Descriptor desc, InventoryItem w)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {w.ID}{Constants.TabStop}{Constants.TabStop}Item Name: {w.Name}");
                sb.AppendLine($"Short Description: {w.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{w.LongDescription}");
                sb.AppendLine($"Value: {w.BaseValue}");
                sb.AppendLine($"No. of Damage Dice: {w.NumberOfDamageDice}{Constants.TabStop}Size of Damage Dice: {w.SizeOfDamageDice}");
                sb.AppendLine($"Hit Bonus: {w.HitModifier}{Constants.TabStop}Damage Bonus: {w.DamageModifier}");
                sb.AppendLine($"Is Magical?: {w.IsMagical}{Constants.TabStop}Two-Handed: {w.IsTwoHanded}");
                sb.AppendLine($"Weapon Type: {w.BaseWeaponType}{Constants.TabStop}Required Skill: {w.RequiredSkill}");
                sb.AppendLine($"Finesse Weapon?: {w.IsFinesseWeapon}{Constants.TabStop}Is Cursed?: {w.IsCursed}");
                sb.AppendLine($"Buffs: {string.Join(", ", w.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {w.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}{Constants.TabStop}4. Edit Long Description");
                sb.AppendLine($"5. Set Value{Constants.TabStop}{Constants.TabStop}6. Set number of Damage Dice");
                sb.AppendLine($"7. Set Size of Damage Dice{Constants.TabStop}{Constants.TabStop}8. Set hit bonus");
                sb.AppendLine($"9. Set Damage Bonus{Constants.TabStop}{Constants.TabStop}10. Toggle Magical");
                sb.AppendLine($"11. Toggle Two-Handed{Constants.TabStop}{Constants.TabStop}12. Set Weapon Type");
                sb.AppendLine($"13. Set Required Skill{Constants.TabStop}{Constants.TabStop}14. Add Buff");
                sb.AppendLine($"15. Remove Buff{Constants.TabStop}{Constants.TabStop}16. Toggle Monster Only");
                sb.AppendLine($"17. Toggle Finesse flag{Constants.TabStop}{Constants.TabStop}18. Toggle Curse");
                sb.AppendLine($"19. Save{Constants.TabStop}{Constants.TabStop}20. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 20)
                    {
                        switch (result)
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
                                w.LongDescription = Helpers.EditLongDescription(ref desc, w.LongDescription);
                                break;

                            case 5:
                                w.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                w.NumberOfDamageDice = GetAssetUintValue(ref desc, "Enter Number of Damage Dice: ");
                                break;

                            case 7:
                                w.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter Size of Damage Dice: ");
                                break;

                            case 8:
                                w.HitModifier = GetAssetIntegerValue(ref desc, "Enter Hit Modifier: ");
                                break;

                            case 9:
                                w.DamageModifier = GetAssetIntegerValue(ref desc, "Enter Damage Modifier: ");
                                break;

                            case 10:
                                w.IsMagical = !w.IsMagical;
                                break;

                            case 11:
                                w.IsTwoHanded = !w.IsTwoHanded;
                                break;

                            case 12:
                                w.BaseWeaponType = GetAssetEnumValue<WeaponType>(ref desc, "Enter Weapon Type: ");
                                break;

                            case 13:
                                var skillName = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if (!string.IsNullOrEmpty(skillName) && SkillManager.Instance.SkillExists(skillName))
                                {
                                    w.RequiredSkill = SkillManager.Instance.GetSkill(skillName);
                                }
                                break;

                            case 14:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!w.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        w.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 15:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (w.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        w.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 16:
                                w.IsMonsterItem = !w.IsMonsterItem;
                                break;

                            case 17:
                                w.IsFinesseWeapon = !w.IsFinesseWeapon;
                                break;

                            case 18:
                                w.IsCursed = !w.IsCursed;
                                break;

                            case 19:
                                if (ValidateWeaponItem(ref desc, ref w, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref w))
                                    {
                                        if (ItemManager.Instance.UpdateItem(w.ID, ref desc, w))
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

                            case 20:
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
                sb.AppendLine($"Item ID: {a.ID}{Constants.TabStop}{Constants.TabStop}Item Name: {a.Name}");
                sb.AppendLine($"Short Description: {a.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{a.LongDescription}");
                sb.AppendLine($"Value: {a.BaseValue}{Constants.TabStop}Armour Class Modifier: {a.ArmourClassModifier}");
                sb.AppendLine($"Is Magical?: {a.IsMagical}{Constants.NewLine}Armour Type: {a.BaseArmourType}");
                sb.AppendLine($"Required Skill: {a.RequiredSkill}{Constants.TabStop}Equip Slot: {a.Slot}");
                sb.AppendLine($"Buffs: {string.Join(", ", a.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {a.IsMonsterItem}{Constants.TabStop}Is Cursed?: {a.IsCursed}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}{Constants.TabStop}4. Edit Long Description");
                sb.AppendLine($"5. Set value{Constants.TabStop}6. Set Armour Class Modifier");
                sb.AppendLine($"6. Toggle Magical flag{Constants.TabStop}8. Set Armour Type");
                sb.AppendLine($"7. Set Required Skill{Constants.TabStop}10. Set Equip Slot");
                sb.AppendLine($"11. Add Buff{Constants.TabStop}12. Remove Buff");
                sb.AppendLine($"13. Toggle Monster Only{Constants.TabStop}14. Toggle Curse flag");
                sb.AppendLine($"15. Save{Constants.TabStop}{Constants.TabStop}16. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 16)
                    {
                        switch (result)
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
                                a.LongDescription = Helpers.EditLongDescription(ref desc, a.LongDescription);
                                break;

                            case 5:
                                a.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                a.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class Modifier: ");
                                break;

                            case 7:
                                a.IsMagical = !a.IsMagical;
                                break;

                            case 8:
                                a.BaseArmourType = GetAssetEnumValue<ArmourType>(ref desc, "Enter Armour Type: ");
                                break;

                            case 9:
                                var skillName = GetAssetStringValue(ref desc, "Enter Require Skill: ");
                                if (!string.IsNullOrEmpty(skillName) && SkillManager.Instance.SkillExists(skillName))
                                {
                                    a.RequiredSkill = SkillManager.Instance.GetSkill(skillName);
                                }
                                break;

                            case 10:
                                a.Slot = GetAssetEnumValue<WearSlot>(ref desc, "Enter Equip Slot: ");
                                break;

                            case 11:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!a.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        a.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (a.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        a.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 13:
                                a.IsMonsterItem = !a.IsMonsterItem;
                                break;

                            case 14:
                                a.IsCursed = !a.IsCursed;
                                break;

                            case 15:
                                if (ValidateArmourItem(ref desc, ref a, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref a))
                                    {
                                        if (ItemManager.Instance.UpdateItem(a.ID, ref desc, a))
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

                            case 16:
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

        private static void EditConsumable(ref Descriptor desc, InventoryItem p)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {p.ID}{Constants.TabStop}{Constants.TabStop}Item Name: {p.Name}");
                sb.AppendLine($"Short Description: {p.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{p.LongDescription}");
                sb.AppendLine($"Value: {p.BaseValue}{Constants.TabStop}{Constants.TabStop}Potion Effect: {p.ConsumableEffect}");
                sb.AppendLine($"Number of Damage Dice: {p.NumberOfDamageDice}{Constants.TabStop}Size of Damage Dice: {p.SizeOfDamageDice}");
                sb.AppendLine($"Is Magical?: {p.IsMagical}");
                sb.AppendLine($"Buffs: {string.Join(", ", p.AppliedBuffs)}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}{Constants.TabStop}4. Edit Long Description");
                sb.AppendLine($"5. Set Value{Constants.TabStop}{Constants.TabStop}6. Set Potion Effect");
                sb.AppendLine($"7. Set Number of Damage Dice{Constants.TabStop}8. Set Size of Damage Dice");
                sb.AppendLine("9. Toggle Magical flag");
                sb.AppendLine($"10. Add Buff{Constants.TabStop}{Constants.TabStop}11. Remove Buff");
                sb.AppendLine($"12. Save{Constants.TabStop}{Constants.TabStop}13. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 13)
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
                                p.LongDescription = Helpers.EditLongDescription(ref desc, p.LongDescription);
                                break;

                            case 5:
                                p.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                p.ConsumableEffect = GetAssetEnumValue<ConsumableEffect>(ref desc, "Enter Potion Effect: ");
                                break;

                            case 7:
                                p.NumberOfDamageDice = GetAssetUintValue(ref desc, "Enter Number of Damage Dice: ");
                                break;

                            case 8:
                                p.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter Size of Damage Dice: ");
                                break;

                            case 9:
                                p.IsMagical = !p.IsMagical;
                                break;

                            case 10:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!p.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        p.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 11:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (p.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        p.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                if (ValidateConsumableItem(ref desc, ref p, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref p))
                                    {
                                        if (ItemManager.Instance.UpdateItem(p.ID, ref desc, p))
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

        private static void EditRing(ref Descriptor desc, InventoryItem r)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {r.ID}{Constants.TabStop}{Constants.TabStop}Item Name: {r.Name}");
                sb.AppendLine($"Short Description: {r.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{r.LongDescription}");
                sb.AppendLine($"Value: {r.BaseValue}{Constants.TabStop}Armour Class Modifier: {r.ArmourClassModifier}");
                sb.AppendLine($"Hit Bonus: {r.HitModifier}{Constants.TabStop}Damage Bonus: {r.DamageModifier}");
                sb.AppendLine($"Is Magical?: {r.IsMagical}{Constants.TabStop}Is Cursed?: {r.IsCursed}");
                sb.AppendLine($"Buffs: {string.Join(", ", r.AppliedBuffs)}");
                sb.AppendLine($"Monster Only?: {r.IsMonsterItem}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Set Long Description{Constants.TabStop}{Constants.TabStop}4. Edit Long Description");
                sb.AppendLine($"5. Set Value{Constants.TabStop}{Constants.TabStop}6. Set Armour Class Modifier");
                sb.AppendLine($"7. Set Hit Bonus{Constants.TabStop}{Constants.TabStop}8. Set Damage Bonus");
                sb.AppendLine($"9. Toggle Magical flag{Constants.TabStop}{Constants.TabStop}10.Add Buff");
                sb.AppendLine($"11. Remove Buff{Constants.TabStop}{Constants.TabStop}12. Toggle Monster Only");
                sb.AppendLine($"13. Toggle Curse Flag");
                sb.AppendLine($"14. Save{Constants.TabStop}{Constants.TabStop}15. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 15)
                    {
                        switch (result)
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
                                r.LongDescription = Helpers.EditLongDescription(ref desc, r.LongDescription);
                                break;

                            case 5:
                                r.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                r.ArmourClassModifier = GetAssetIntegerValue(ref desc, "Enter Armour Class Modifier: ");
                                break;

                            case 7:
                                r.HitModifier = GetAssetIntegerValue(ref desc, "Enter Hit Modifier: ");
                                break;

                            case 8:
                                r.DamageModifier = GetAssetIntegerValue(ref desc, "Enter Damage Modifier: ");
                                break;

                            case 9:
                                r.IsMagical = !r.IsMagical;
                                break;

                            case 10:
                                var b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                var buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (!r.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        r.AppliedBuffs.Add(buff.BuffName);
                                    }
                                }
                                break;

                            case 11:
                                b = GetAssetStringValue(ref desc, "Enter Buff Name: ");
                                buff = BuffManager.Instance.GetBuff(b);
                                if (buff != null)
                                {
                                    if (r.AppliedBuffs.Contains(buff.BuffName))
                                    {
                                        r.AppliedBuffs.Remove(buff.BuffName);
                                    }
                                }
                                break;

                            case 12:
                                r.IsMonsterItem = !r.IsMonsterItem;
                                break;

                            case 13:
                                r.IsCursed = !r.IsCursed;
                                break;

                            case 14:
                                if (ValidateRingItem(ref desc, ref r, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref r))
                                    {
                                        if (ItemManager.Instance.UpdateItem(r.ID, ref desc, r))
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

        private static void EditMiscItem(ref Descriptor desc, InventoryItem j)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"Item ID: {j.ID}{Constants.TabStop}{Constants.TabStop}Item Name: {j.Name}");
                sb.AppendLine($"Short Description: {j.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{j.LongDescription}");
                sb.AppendLine($"Value: {j.BaseValue}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Item Name{Constants.TabStop}{Constants.TabStop}2. Set Short Description");
                sb.AppendLine($"3. Edit Long Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Set Value{Constants.TabStop}{Constants.TabStop}6. Save{Constants.TabStop}{Constants.TabStop}7. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
                {
                    if (result >= 1 && result <= 7)
                    {
                        switch (result)
                        {
                            case 1:
                                j.Name = GetAssetStringValue(ref desc, "Enter Item Name: ");
                                break;

                            case 2:
                                j.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 3:
                                j.LongDescription = Helpers.EditLongDescription(ref desc, j.LongDescription);
                                break;

                            case 4:
                                j.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                j.BaseValue = GetAssetUintValue(ref desc, "Enter Value: ");
                                break;

                            case 6:
                                if (ValidateJunkItem(ref desc, ref j, false))
                                {
                                    if (DatabaseManager.UpdateItemByID(ref desc, ref j))
                                    {
                                        if (ItemManager.Instance.UpdateItem(j.ID, ref desc, j))
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

        #region Delete Items
        private static void DeleteItem(ref Descriptor desc)
        {
            desc.Send("Enter the ID of the item to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint result))
            {
                var i = ItemManager.Instance.GetItemByID(result);
                if (i != null)
                {
                    if (DatabaseManager.DeleteItemByID(ref desc, i.ID))
                    {
                        if (ItemManager.Instance.RemoveItem(ref desc, i.ID))
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

        #region Validation Functions
        private static bool ValidateScrollItem(ref Descriptor desc, ref InventoryItem s, bool isNewItem)
        {
            bool isValid = true;
            if (ItemManager.Instance.ItemExists(s.ID) && isNewItem)
            {
                isValid = false;
                desc.Send($"The specified ID is already in use.{Constants.NewLine}");
            }
            if (s.ID == 0)
            {
                isValid = false;
                desc.Send($"The specified ID is not valid.{Constants.NewLine}");
            }
            if (string.IsNullOrEmpty(s.Name) || string.IsNullOrEmpty(s.ShortDescription) || string.IsNullOrEmpty(s.LongDescription) || string.IsNullOrEmpty(s.CastsSpell))
            {
                isValid = false;
                desc.Send($"One or more attributes are missing values.{Constants.NewLine}");
            }
            return isValid;
        }

        private static bool ValidateWeaponItem(ref Descriptor desc, ref InventoryItem w, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(w.ID) && w.ID != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(w.Name) || string.IsNullOrEmpty(w.ShortDescription) || string.IsNullOrEmpty(w.LongDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
                else
                {
                    if (w.NumberOfDamageDice == 0 || w.SizeOfDamageDice == 0)
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
            if (w.IsCursed && !w.IsMagical)
            {
                desc.Send($"If the weapon is cursed it must also be magical.{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }

        private static bool ValidateArmourItem(ref Descriptor desc, ref InventoryItem a, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(a.ID) && a.ID != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(a.Name) || string.IsNullOrEmpty(a.ShortDescription) || string.IsNullOrEmpty(a.LongDescription))
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
            if (a.IsCursed && !a.IsMagical)
            {
                desc.Send($"If the armour is cursed, it must also be magical.{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }

        private static bool ValidateConsumableItem(ref Descriptor desc, ref InventoryItem p, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(p.ID) && p.ID != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(p.Name) || string.IsNullOrEmpty(p.ShortDescription) || string.IsNullOrEmpty(p.LongDescription))
                {
                    desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                    isValid = false;
                }
                else
                {
                    if (p.ConsumableEffect.HasFlag(ConsumableEffect.Healing) || p.ConsumableEffect.HasFlag(ConsumableEffect.SPHealing) || p.ConsumableEffect.HasFlag(ConsumableEffect.MPHealing) || p.ConsumableEffect.HasFlag(ConsumableEffect.Poison))
                    {
                        if (p.NumberOfDamageDice == 0 || p.SizeOfDamageDice == 0)
                        {
                            desc.Send($"If a Consumable heals or poisons it needs to have damage dice{Constants.NewLine}");
                            isValid = false;
                        }
                    }
                    else
                    {
                        if (p.ConsumableEffect == ConsumableEffect.None || p.ConsumableEffect == ConsumableEffect.Buff)
                        {
                            if (p.NumberOfDamageDice > 0 || p.SizeOfDamageDice > 0)
                            {
                                desc.Send($"Consumable which apply buffs or have no effect do not need damage dice{Constants.NewLine}");
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

        private static bool ValidateJunkItem(ref Descriptor desc, ref InventoryItem i, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(i.ID) && i.ID != 0) || !isNewItem)
            {
                if (string.IsNullOrEmpty(i.Name) || string.IsNullOrEmpty(i.LongDescription) || string.IsNullOrEmpty(i.ShortDescription))
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

        private static bool ValidateRingItem(ref Descriptor desc, ref InventoryItem r, bool isNewItem)
        {
            bool isValid = true;
            if ((isNewItem && !ItemManager.Instance.ItemExists(r.ID) && r.ID != 0) || !isNewItem)
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
            if (r.IsCursed && !r.IsMagical)
            {
                desc.Send($"If the Ring has the Cursed flag, it should also have the Magical flag{Constants.NewLine}");
                isValid = false;
            }
            return isValid;
        }
        #endregion
    }
}