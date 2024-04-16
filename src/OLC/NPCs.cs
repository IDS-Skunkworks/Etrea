using Etrea2.Core;
using Etrea2.Entities;
using System.Collections.Generic;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create
        private static void CreateNewNPC(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("NPCs, or non-player characters populate the world. They can be anything from townsfolk to dragons, rabbits to royal guards.");
            desc.Send(sb.ToString());
            NPC newNPC = new NPC
            {
                ActorType = ActorType.NonPlayer,
                Inventory = new List<InventoryItem>(),
                Skills = new List<Skill>(),
                Spells = new List<Spell>(),
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"NPC ID: {newNPC.NPCID}{Constants.TabStop}NPC Zone: {newNPC.AppearsInZone}");
                sb.AppendLine($"NPC Name: {newNPC.Name}{Constants.TabStop}Gender: {newNPC.Gender}");
                sb.AppendLine($"Frequency: {newNPC.AppearChance}{Constants.TabStop}{Constants.TabStop}Max Number: {newNPC.MaxNumber}");
                sb.AppendLine($"Short Description: {newNPC.ShortDescription}");
                sb.AppendLine($"Long Description:{Constants.NewLine}{newNPC.LongDescription}");
                sb.AppendLine($"Level: {newNPC.Level}{Constants.TabStop}Number of Hit Dice: {newNPC.NumberOfHitDice}{Constants.TabStop}Bonus Hit Die: {newNPC.BonusHitDice}{Constants.TabStop}Hit Die Size: {newNPC.HitDieSize}");
                sb.AppendLine($"STR: {newNPC.Strength}{Constants.TabStop}{Constants.TabStop}DEX: {newNPC.Dexterity}{Constants.TabStop}{Constants.TabStop}CON: {newNPC.Constitution}");
                sb.AppendLine($"INT: {newNPC.Intelligence}{Constants.TabStop}{Constants.TabStop}WIS: {newNPC.Wisdom}{Constants.TabStop}{Constants.TabStop}CHA: {newNPC.Charisma}");
                sb.AppendLine($"Base Armour Class: {newNPC.BaseArmourClass}{Constants.TabStop}Armour Class: {newNPC.ArmourClass}{Constants.TabStop}{Constants.TabStop}Base Exp: {newNPC.BaseExpAward}");
                sb.AppendLine($"Gold: {newNPC.Gold}{Constants.TabStop}{Constants.TabStop}NPC Flags: {newNPC.BehaviourFlags}");
                sb.AppendLine($"Alginment: {newNPC.Alignment}{Constants.TabStop}{Constants.TabStop}No. Of Attacks: {newNPC.NumberOfAttacks}");
                sb.AppendLine($"Resistances:");
                sb.AppendLine($"Lightning: {newNPC.ResistLightning}{Constants.TabStop}Fire: {newNPC.ResistFire}{Constants.TabStop}Ice: {newNPC.ResistIce}");
                sb.AppendLine($"Earth: {newNPC.ResistEarth}{Constants.TabStop}Dark: {newNPC.ResistDark}{Constants.TabStop}Holy: {newNPC.ResistHoly}");
                sb.AppendLine($"Skills: {string.Join(", ", newNPC.Skills)}");
                sb.AppendLine($"Spells: {string.Join(", ", newNPC.Spells)}");
                sb.AppendLine($"Arrival Message: {newNPC.ArrivalMessage.Trim()}");
                sb.AppendLine($"Departure Message: {newNPC.DepartureMessage.Trim()}");
                sb.AppendLine();
                sb.AppendLine("Equipment:");
                sb.AppendLine($"Head: {newNPC.EquipHead?.Name ?? "Nothing"}{Constants.TabStop}Neck: {newNPC.EquipNeck?.Name ?? "Nothing"}");
                sb.AppendLine($"Armour: {newNPC.EquipArmour?.Name ?? "Nothing"}{Constants.TabStop}Finger (L): {newNPC.EquipLeftFinger?.Name ?? "Nothing"}");
                sb.AppendLine($"Right Finger: {newNPC.EquipRightFinger?.Name ?? "Nothing"}{Constants.TabStop}Weapon: {newNPC.EquipWeapon?.Name ?? "Nothing"}");
                sb.AppendLine($"Held: {newNPC.EquipHeld?.Name ?? "Nothing"}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set NPC ID{Constants.TabStop}2. Set NPC Name{Constants.TabStop}3. Set Short Description");
                sb.AppendLine($"4. Set Long Description{Constants.TabStop}5. Edit Long Description");
                sb.AppendLine($"6. Set NPC Level{Constants.TabStop}7. Set number of Bonus Hit Dice{Constants.TabStop}8. Set size of Hit Dice");
                sb.AppendLine($"9. Set STR{Constants.TabStop}10. Set DEX{Constants.TabStop}11. Set CON");
                sb.AppendLine($"12. Set INT{Constants.TabStop}13. Set WIS{Constants.TabStop}14. Set CHA");
                sb.AppendLine($"15. Set Base Exp{Constants.TabStop}16. Set Gold{Constants.TabStop}17. Add Flag{Constants.TabStop}18. Remove Flag");
                sb.AppendLine($"19. Set Arrival Message{Constants.TabStop}{Constants.TabStop}20. Set Departure Message");
                sb.AppendLine($"21. Set Max Number{Constants.TabStop}{Constants.TabStop}22. Set Appearance Frequency");
                sb.AppendLine($"23. Set Base Armour Class{Constants.TabStop}24. Set NPC Zone{Constants.TabStop}25. Set NPC Gender");
                sb.AppendLine($"26. Set Number of Attacks{Constants.TabStop}27. Set Head Equip{Constants.TabStop}28. Set Neck Equip");
                sb.AppendLine($"29. Set Armour Equip{Constants.TabStop}30. Set Finger (L) Equp{Constants.TabStop}{Constants.TabStop}31. Set Finger (R) Equip");
                sb.AppendLine($"32. Set Weapon Equip{Constants.TabStop}33. Set Held Equip");
                sb.AppendLine($"34. Add Skill{Constants.TabStop}35. Remove Skill{Constants.TabStop}36. Add Spell{Constants.TabStop}37. Remove Spell");
                sb.AppendLine($"38. Set Alignment{Constants.TabStop}39. Set Resistances{Constants.TabStop}40. Save{Constants.TabStop}41. Exit");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 41)
                    {
                        switch (option)
                        {
                            case 1:
                                newNPC.NPCID = GetAssetUintValue(ref desc, "Enter NPC ID: ");
                                break;

                            case 2:
                                newNPC.Name = GetAssetStringValue(ref desc, "Enter NPC Name: ");
                                break;

                            case 3:
                                newNPC.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                break;

                            case 4:
                                newNPC.LongDescription = Helpers.GetLongDescription(ref desc);
                                break;

                            case 5:
                                newNPC.LongDescription = Helpers.EditLongDescription(ref desc, newNPC.LongDescription);
                                break;

                            case 6:
                                newNPC.Level = GetAssetUintValue(ref desc, "Enter NPC Level: ");
                                break;

                            case 7:
                                newNPC.BonusHitDice = GetAssetUintValue(ref desc, "Enter amount of Bonus Hit Die: ");
                                break;

                            case 8:
                                newNPC.HitDieSize = GetAssetUintValue(ref desc, "Enter size of Hit Die: ");
                                break;

                            case 9:
                                newNPC.Strength = GetAssetUintValue(ref desc, "Enter NPC Strength: ");
                                break;

                            case 10:
                                newNPC.Dexterity = GetAssetUintValue(ref desc, "Enter NPC Dexterity: ");
                                break;

                            case 11:
                                newNPC.Constitution = GetAssetUintValue(ref desc, "Enter NPC Constitution: ");
                                break;

                            case 12:
                                newNPC.Intelligence = GetAssetUintValue(ref desc, "Enter NPC Intelligence: ");
                                break;

                            case 13:
                                newNPC.Wisdom = GetAssetUintValue(ref desc, "Enter NPC Wisdom: ");
                                break;

                            case 14:
                                newNPC.Charisma = GetAssetUintValue(ref desc, "Enter NPC Charisma: ");
                                break;

                            case 15:
                                newNPC.BaseExpAward = GetAssetUintValue(ref desc, "Set Base Exp: ");
                                break;

                            case 16:
                                newNPC.Gold = GetAssetUintValue(ref desc, "Set Gold: ");
                                break;

                            case 17:
                                var nf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                if (nf != NPCFlags.None && !newNPC.BehaviourFlags.HasFlag(nf))
                                {
                                    newNPC.BehaviourFlags |= nf;
                                }
                                else
                                {
                                    newNPC.BehaviourFlags = NPCFlags.None;
                                }
                                break;

                            case 18:
                                var rf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                if (rf != NPCFlags.None && newNPC.BehaviourFlags.HasFlag(rf))
                                {
                                    newNPC.BehaviourFlags &= ~rf;
                                }
                                break;

                            case 19:
                                newNPC.ArrivalMessage = GetAssetStringValue(ref desc, "Enter Arrival Message: ");
                                break;

                            case 20:
                                newNPC.DepartureMessage = GetAssetStringValue(ref desc, "Enter Departure Message: ");
                                break;

                            case 21:
                                newNPC.MaxNumber = GetAssetUintValue(ref desc, "Enter Max Number: ");
                                break;

                            case 22:
                                newNPC.AppearChance = GetAssetUintValue(ref desc, "Enter Appearance Chance: ");
                                break;

                            case 23:
                                newNPC.BaseArmourClass = GetAssetUintValue(ref desc, "Enter Base Armour Class: ");
                                break;

                            case 24:
                                newNPC.AppearsInZone = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                break;

                            case 25:
                                newNPC.Gender = GetAssetEnumValue<Gender>(ref desc, "Enter Gender: ");
                                break;

                            case 26:
                                newNPC.NumberOfAttacks = GetAssetUintValue(ref desc, "Enter Number of Attacks: ");
                                break;

                            case 27:
                                var itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipHead = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Head))
                                        {
                                            newNPC.EquipHead = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 28:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipNeck = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Neck))
                                        {
                                            newNPC.EquipNeck = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 29:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipArmour = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Armour))
                                        {
                                            newNPC.EquipArmour = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 30:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipLeftFinger = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.FingerLeft))
                                        {
                                            newNPC.EquipLeftFinger = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 31:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipRightFinger = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.FingerRight))
                                        {
                                            newNPC.EquipRightFinger = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 32:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipWeapon = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Weapon))
                                        {
                                            newNPC.EquipWeapon = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 33:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquipHeld = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Held))
                                        {
                                            newNPC.EquipHeld = item;
                                        }
                                        else
                                        {
                                            desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"No item matching that ID{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 34:
                                var sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if (SkillManager.Instance.SkillExists(sk) && !newNPC.HasSkill(sk))
                                {
                                    newNPC.AddSkill(sk);
                                }
                                break;

                            case 35:
                                sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if (SkillManager.Instance.SkillExists(sk) && newNPC.HasSkill(sk))
                                {
                                    newNPC.RemoveSkill(sk);
                                }
                                break;

                            case 36:
                                var sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                if (SpellManager.Instance.SpellExists(sp) && !newNPC.HasSpell(sp))
                                {
                                    newNPC.AddSpell(sp);
                                }
                                break;

                            case 37:
                                sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                if (SpellManager.Instance.SpellExists(sp) && newNPC.HasSpell(sp))
                                {
                                    newNPC.RemoveSpell(sp);
                                }
                                break;

                            case 38:
                                newNPC.Alignment = GetAssetEnumValue<Alignment>(ref desc, "Enter Alignment: ");
                                break;

                            case 39:
                                newNPC.ResistDark = GetAssetIntegerValue(ref desc, "Enter Dark Resistance: ");
                                newNPC.ResistEarth = GetAssetIntegerValue(ref desc, "Enter Earth Resistance: ");
                                newNPC.ResistFire = GetAssetIntegerValue(ref desc, "Enter Fire Resistance: ");
                                newNPC.ResistHoly = GetAssetIntegerValue(ref desc, "Enter Holy Resistance: ");
                                newNPC.ResistIce = GetAssetIntegerValue(ref desc, "Enter Ice Resistance: ");
                                newNPC.ResistLightning = GetAssetIntegerValue(ref desc, "Enter Lightning Resistance: ");
                                break;

                            case 40:
                                if (ValidateNPCAsset(ref desc, ref newNPC, true))
                                {
                                    if (DatabaseManager.AddNewNPC(ref desc, ref newNPC))
                                    {
                                        if (NPCManager.Instance.AddNPC(ref desc, newNPC))
                                        {
                                            desc.Send($"New NPC added to NPC Manager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new NPC to NPC Manager, it may not be available until the World is restarted{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to save new NPC to World database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 41:
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

        #region Edit
        private static void EditExistingNPC(ref Descriptor desc)
        {
            bool okToReturn = false;
            desc.Send("Enter the ID of the NPC to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint id))
            {
                if (NPCManager.Instance.NPCExists(id))
                {
                    var n = NPCManager.Instance.GetNPCByID(id).ShallowCopy();
                    StringBuilder sb = new StringBuilder();
                    while (!okToReturn)
                    {
                        sb.AppendLine($"NPC ID: {n.NPCID}{Constants.TabStop}NPC Zone: {n.AppearsInZone}");
                        sb.AppendLine($"NPC Name: {n.Name}{Constants.TabStop}Gender: {n.Gender}");
                        sb.AppendLine($"Frequency: {n.AppearChance}{Constants.TabStop}{Constants.TabStop}Max Number: {n.MaxNumber}");
                        sb.AppendLine($"Short Description: {n.ShortDescription}");
                        sb.AppendLine($"Long Description:{Constants.NewLine}{n.LongDescription}");
                        sb.AppendLine($"Level: {n.Level}{Constants.TabStop}Number of Hit Dice: {n.NumberOfHitDice}{Constants.TabStop}Bonus Hit Die: {n.BonusHitDice}{Constants.TabStop}Hit Die Size: {n.HitDieSize}");
                        sb.AppendLine($"STR: {n.Strength}{Constants.TabStop}{Constants.TabStop}DEX: {n.Dexterity}{Constants.TabStop}{Constants.TabStop}CON: {n.Constitution}");
                        sb.AppendLine($"INT: {n.Intelligence}{Constants.TabStop}{Constants.TabStop}WIS: {n.Wisdom}{Constants.TabStop}{Constants.TabStop}CHA: {n.Charisma}");
                        sb.AppendLine($"Base Armour Class: {n.BaseArmourClass}{Constants.TabStop}Armour Class: {n.ArmourClass}{Constants.TabStop}{Constants.TabStop}Base Exp: {n.BaseExpAward}");
                        sb.AppendLine($"Gold: {n.Gold}{Constants.TabStop}{Constants.TabStop}NPC Flags: {n.BehaviourFlags}");
                        sb.AppendLine($"Alginment: {n.Alignment}{Constants.TabStop}{Constants.TabStop}No. Of Attacks: {n.NumberOfAttacks}");
                        sb.AppendLine($"Resistances:");
                        sb.AppendLine($"Lightning: {n.ResistLightning}{Constants.TabStop}Fire: {n.ResistFire}{Constants.TabStop}Ice: {n.ResistIce}");
                        sb.AppendLine($"Earth: {n.ResistEarth}{Constants.TabStop}Dark: {n.ResistDark}{Constants.TabStop}Holy: {n.ResistHoly}");
                        sb.AppendLine($"Skills: {string.Join(", ", n.Skills)}");
                        sb.AppendLine($"Spells: {string.Join(", ", n.Spells)}");
                        sb.AppendLine($"Arrival Message: {n.ArrivalMessage.Trim()}");
                        sb.AppendLine($"Departure Message: {n.DepartureMessage.Trim()}");
                        sb.AppendLine();
                        sb.AppendLine("Equipment:");
                        sb.AppendLine($"Head: {n.EquipHead?.Name ?? "Nothing"}{Constants.TabStop}Neck: {n.EquipNeck?.Name ?? "Nothing"}");
                        sb.AppendLine($"Armour: {n.EquipArmour?.Name ?? "Nothing"}{Constants.TabStop}Finger (L): {n.EquipLeftFinger?.Name ?? "Nothing"}");
                        sb.AppendLine($"Right Finger: {n.EquipRightFinger?.Name ?? "Nothing"}{Constants.TabStop}Weapon: {n.EquipWeapon?.Name ?? "Nothing"}");
                        sb.AppendLine($"Held: {n.EquipHeld?.Name ?? "Nothing"}");
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine($"1. Set NPC Name{Constants.TabStop}2. Set Short Description");
                        sb.AppendLine($"3. Set Long Description{Constants.TabStop}4. Edit Long Description");
                        sb.AppendLine($"5. Set NPC Level{Constants.TabStop}6. Set number of Bonus Hit Dice{Constants.TabStop}7. Set size of Hit Dice");
                        sb.AppendLine($"8. Set Strength{Constants.TabStop}9. Set Dexterity{Constants.TabStop}10. Set Constitution");
                        sb.AppendLine($"11. Set Intelligence{Constants.TabStop}12. Set Wisdom{Constants.TabStop}13. Set Charisma");
                        sb.AppendLine($"14. Set Base Exp{Constants.TabStop}15. Set Gold{Constants.TabStop}16. Add Flag{Constants.TabStop}17. Remove Flag");
                        sb.AppendLine($"18. Set Arrival Message{Constants.TabStop}{Constants.TabStop}19. Set Departure Message");
                        sb.AppendLine($"20. Set Max Number{Constants.TabStop}{Constants.TabStop}21. Set Appearance Frequency");
                        sb.AppendLine($"22. Set Base Armour Class{Constants.TabStop}23. Set NPC Zone{Constants.TabStop}24. Set NPC Gender");
                        sb.AppendLine($"25. Set Number of Attacks{Constants.TabStop}26. Set Head Equip{Constants.TabStop}27. Set Neck Equip");
                        sb.AppendLine($"28. Set Armour Equip{Constants.TabStop}29. Set Finger (L) Equp{Constants.TabStop}30. Set Finger (R) Equip");
                        sb.AppendLine($"31. Set Weapon Equip{Constants.TabStop}32. Set Held Equip");
                        sb.AppendLine($"33. Add Skill{Constants.TabStop}34. Remove Skill{Constants.TabStop}35. Add Spell{Constants.TabStop}36. Remove Spell");
                        sb.AppendLine($"37. Set Alignment{Constants.TabStop}38. Set Resistances{Constants.TabStop}39. Save{Constants.TabStop}40. Exit");
                        sb.Append("Selection: ");
                        desc.Send(sb.ToString());
                        input = desc.Read().Trim();
                        if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                        {
                            if (option >= 1 && option <= 40)
                            {
                                switch (option)
                                {
                                    case 1:
                                        n.Name = GetAssetStringValue(ref desc, "Enter NPC Name: ");
                                        break;

                                    case 2:
                                        n.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                        break;

                                    case 3:
                                        n.LongDescription = Helpers.GetLongDescription(ref desc);
                                        break;

                                    case 4:
                                        n.LongDescription = Helpers.EditLongDescription(ref desc, n.LongDescription);
                                        break;

                                    case 5:
                                        n.Level = GetAssetUintValue(ref desc, "Enter NPC Level: ");
                                        break;

                                    case 6:
                                        n.BonusHitDice = GetAssetUintValue(ref desc, "Enter amount of Bonus Hit Die: ");
                                        break;

                                    case 7:
                                        n.HitDieSize = GetAssetUintValue(ref desc, "Enter size of Hit Die: ");
                                        break;

                                    case 8:
                                        n.Strength = GetAssetUintValue(ref desc, "Enter NPC Strength: ");
                                        break;

                                    case 9:
                                        n.Dexterity = GetAssetUintValue(ref desc, "Enter NPC Dexterity: ");
                                        break;

                                    case 10:
                                        n.Constitution = GetAssetUintValue(ref desc, "Enter NPC Constitution: ");
                                        break;

                                    case 11:
                                        n.Intelligence = GetAssetUintValue(ref desc, "Enter NPC Intelligence: ");
                                        break;

                                    case 12:
                                        n.Wisdom = GetAssetUintValue(ref desc, "Enter NPC Wisdom: ");
                                        break;

                                    case 13:
                                        n.Charisma = GetAssetUintValue(ref desc, "Enter NPC Charisma: ");
                                        break;

                                    case 14:
                                        n.BaseExpAward = GetAssetUintValue(ref desc, "Set Base Exp: ");
                                        break;

                                    case 15:
                                        n.Gold = GetAssetUintValue(ref desc, "Set Gold: ");
                                        break;

                                    case 16:
                                        var nf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                        if (nf != NPCFlags.None && !n.BehaviourFlags.HasFlag(nf))
                                        {
                                            n.BehaviourFlags |= nf;
                                        }
                                        else
                                        {
                                            n.BehaviourFlags = NPCFlags.None;
                                        }
                                        break;

                                    case 17:
                                        var rf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                        if (rf != NPCFlags.None && n.BehaviourFlags.HasFlag(rf))
                                        {
                                            n.BehaviourFlags &= ~rf;
                                        }
                                        break;

                                    case 18:
                                        n.ArrivalMessage = GetAssetStringValue(ref desc, "Enter Arrival Message: ");
                                        break;

                                    case 19:
                                        n.DepartureMessage = GetAssetStringValue(ref desc, "Enter Departure Message: ");
                                        break;

                                    case 20:
                                        n.MaxNumber = GetAssetUintValue(ref desc, "Enter Max Number: ");
                                        break;

                                    case 21:
                                        n.AppearChance = GetAssetUintValue(ref desc, "Enter Appearance Chance: ");
                                        break;

                                    case 22:
                                        n.BaseArmourClass = GetAssetUintValue(ref desc, "Enter Base Armour Class: ");
                                        break;

                                    case 23:
                                        n.AppearsInZone = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                        break;

                                    case 24:
                                        n.Gender = GetAssetEnumValue<Gender>(ref desc, "Enter Gender: ");
                                        break;

                                    case 25:
                                        n.NumberOfAttacks = GetAssetUintValue(ref desc, "Enter Number of Attacks: ");
                                        break;

                                    case 26:
                                        var itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipHead = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Head))
                                                {
                                                    n.EquipHead = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 27:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipNeck = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Neck))
                                                {
                                                    n.EquipNeck = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 28:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipArmour = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Armour))
                                                {
                                                    n.EquipArmour = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 29:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipLeftFinger = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.FingerLeft))
                                                {
                                                    n.EquipLeftFinger = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 30:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipRightFinger = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.FingerRight))
                                                {
                                                    n.EquipRightFinger = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 31:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipWeapon = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Weapon))
                                                {
                                                    n.EquipWeapon = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 32:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            n.EquipHeld = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Held))
                                                {
                                                    n.EquipHeld = item;
                                                }
                                                else
                                                {
                                                    desc.Send($"That is not appropriate for the selected equipment slot{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"No item matching that ID{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 33:
                                        var sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                        if (SkillManager.Instance.SkillExists(sk) && !n.HasSkill(sk))
                                        {
                                            n.AddSkill(sk);
                                        }
                                        break;

                                    case 34:
                                        sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                        if (SkillManager.Instance.SkillExists(sk) && n.HasSkill(sk))
                                        {
                                            n.RemoveSkill(sk);
                                        }
                                        break;

                                    case 35:
                                        var sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                        if (SpellManager.Instance.SpellExists(sp) && !n.HasSpell(sp))
                                        {
                                            n.AddSpell(sp);
                                        }
                                        break;

                                    case 36:
                                        sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                        if (SpellManager.Instance.SpellExists(sp) && n.HasSpell(sp))
                                        {
                                            n.RemoveSpell(sp);
                                        }
                                        break;

                                    case 37:
                                        n.Alignment = GetAssetEnumValue<Alignment>(ref desc, "Enter Alignment: ");
                                        break;

                                    case 38:
                                        n.ResistDark = GetAssetIntegerValue(ref desc, "Set Dark Resistance: ");
                                        n.ResistEarth = GetAssetIntegerValue(ref desc, "Set Earth Resistance: ");
                                        n.ResistFire = GetAssetIntegerValue(ref desc, "Set Fire Resistance: ");
                                        n.ResistHoly = GetAssetIntegerValue(ref desc, "Set Holy Resistance: ");
                                        n.ResistIce = GetAssetIntegerValue(ref desc, "Set Ice Resistance: ");
                                        n.ResistLightning = GetAssetIntegerValue(ref desc, "Set Lightning Resistance: ");
                                        break;

                                    case 39:
                                        if (ValidateNPCAsset(ref desc, ref n, false))
                                        {
                                            if (DatabaseManager.UpdateNPCByID(ref desc, ref n))
                                            {
                                                if (NPCManager.Instance.UpdateNPC(ref desc, n))
                                                {
                                                    desc.Send($"NPC updated in NPC Manager and World database{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update the NPC in NPC Manager, it may not be available until the World is restarted{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update the NPC in the World database{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 40:
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
            }
        }
        #endregion

        #region Delete
        private static void DeleteNPC(ref Descriptor desc)
        {
            desc.Send("Enter ID of NPC to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint id))
            {
                var npc = NPCManager.Instance.GetNPCByID(id);
                if (npc != null)
                {
                    if (DatabaseManager.DeleteNPCByID(ref desc, npc.NPCID))
                    {
                        if (NPCManager.Instance.RemoveNPC(ref desc, npc.NPCID, npc.Name))
                        {
                            desc.Send($"NPC successfully removed from World database and NPC Manager{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"Could not remove NPC from the NPC Manager{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Could not delete NPC from World database{Constants.NewLine}");
                    }
                }
            }
        }
        #endregion

        #region Validation Functions
        private static bool ValidateNPCAsset(ref Descriptor desc, ref NPC n, bool isNewNPC)
        {
            if (string.IsNullOrEmpty(n.Name) || string.IsNullOrEmpty(n.ShortDescription) || string.IsNullOrEmpty(n.LongDescription))
            {
                desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                return false;
            }
            if ((isNewNPC && NPCManager.Instance.NPCExists(n.NPCID)) || n.NPCID == 0)
            {
                desc.Send($"NPC ID already in use or was 0{Constants.NewLine}");
                return false;
            }
            if (n.AppearsInZone == 0)
            {
                desc.Send($"NPC must be assigned a Zone{Constants.NewLine}");
                return false;
            }
            if (n.Strength == 0 || n.Dexterity == 0 || n.Constitution == 0 || n.Intelligence == 0 || n.Wisdom == 0 || n.Charisma == 0)
            {
                desc.Send($"One or more stats are 0 but should have a value{Constants.NewLine}");
                return false;
            }
            if (n.BaseExpAward == 0)
            {
                desc.Send($"Base EXP for this NPC is 0 but should have a value{Constants.NewLine}");
                return false;
            }
            if (n.BehaviourFlags.HasFlag(NPCFlags.Hostile) && n.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
            {
                desc.Send($"An NPC cannot have the Hostile and NoAttack flags at the same time{Constants.NewLine}");
                return false;
            }
            if (string.IsNullOrEmpty(n.DepartureMessage) || string.IsNullOrEmpty(n.ArrivalMessage))
            {
                desc.Send($"Arrive or Depart message is blank but should have a value{Constants.NewLine}");
                return false;
            }
            if (n.NumberOfAttacks == 0)
            {
                desc.Send($"The Number of Attacks value must be at least 1{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}