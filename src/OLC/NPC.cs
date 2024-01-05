using Kingdoms_of_Etrea.Core;
using Kingdoms_of_Etrea.Entities;
using System.Text;

namespace Kingdoms_of_Etrea.OLC
{
    internal static partial class OLC
    {
        #region Delete
        private static void DeleteNPC(ref Descriptor desc)
        {
            desc.Send("Enter ID of NPC to delete (0 to return): ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint id))
            {
                if(id > 0)
                {
                    var npc = NPCManager.Instance.GetNPCByID(id);
                    if(npc != null)
                    {
                        if(DatabaseManager.DeleteNPCByID(ref desc, npc.NPCID))
                        {
                            if(NPCManager.Instance.RemoveNPCByID(ref desc, npc.NPCID))
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
                    else
                    {
                        desc.Send($"No NPC with ID {id} could be found in the NPC Manager{Constants.NewLine}");
                    }
                }
            }
        }
        #endregion

        #region Edit
        private static void EditExistingNPC(ref Descriptor desc)
        {
            bool okToReturn = false;
            desc.Send("Enter ID of NPC to edit: ");
            var input = desc.Read().Trim();
            if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint npcid))
            {
                var npc = NPCManager.Instance.GetNPCByID(npcid);
                if(npc != null)
                {
                    StringBuilder sb = new StringBuilder();
                    while(!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine();
                        sb.AppendLine($"NPC ID: {npc.NPCID}{Constants.TabStop}NPC Name: {npc.Name}");
                        sb.AppendLine($"Gender: {npc.Gender}{Constants.TabStop}Alignment: {npc.Alignment}");
                        sb.AppendLine($"NPC Zone: {npc.AppearsInZone}");
                        sb.AppendLine($"Frequency: {npc.AppearChance}{Constants.TabStop}{Constants.TabStop}Max Number: {npc.MaxNumber}");
                        sb.AppendLine($"Short Description: {npc.ShortDescription}");
                        sb.AppendLine($"Long Description: {npc.LongDescription}");
                        sb.AppendLine($"Number of Hit Dice: {npc.NumberOfHitDice}{Constants.TabStop}Size of Hit Dice: {npc.SizeOfHitDice}");
                        sb.AppendLine($"STR: {npc.Stats.Strength}{Constants.TabStop}DEX: {npc.Stats.Dexterity}");
                        sb.AppendLine($"CON: {npc.Stats.Constitution}{Constants.TabStop}INT: {npc.Stats.Intelligence}");
                        sb.AppendLine($"WIS: {npc.Stats.Wisdom}{Constants.TabStop}CHA: {npc.Stats.Charisma}");
                        sb.AppendLine($"Armour Class: {npc.Stats.ArmourClass}{Constants.TabStop}Base Exp: {npc.BaseExpAward}");
                        sb.AppendLine($"Gold: {npc.Stats.Gold}{Constants.TabStop}NPC Flags: {npc.BehaviourFlags}");
                        sb.AppendLine($"No. Of Attacks: {npc.NumberOfAttacks}");
                        sb.AppendLine($"Spells: {string.Join(", ", npc.Spells)}");
                        sb.AppendLine($"Skills: {string.Join(", ", npc.Skills)}");
                        sb.AppendLine($"Arrival Message: {npc.ArrivalMessage}");
                        sb.AppendLine($"Departure Message: {npc.DepartMessage}");
                        sb.AppendLine();
                        sb.AppendLine("Equipment:");
                        sb.AppendLine($"Head: {npc.EquippedItems.Head}");
                        sb.AppendLine($"Neck: {npc.EquippedItems.Neck}");
                        sb.AppendLine($"Armour: {npc.EquippedItems.Armour}");
                        sb.AppendLine($"Left Finger: {npc.EquippedItems.FingerLeft}");
                        sb.AppendLine($"Right Finger: {npc.EquippedItems.FingerRight}");
                        sb.AppendLine($"Weapon: {npc.EquippedItems.Weapon}");
                        sb.AppendLine($"Held: {npc.EquippedItems.Held}");
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine("1. Set NPC Name");
                        sb.AppendLine($"2. Set Short Description{Constants.TabStop}3. Set Long Description");
                        sb.AppendLine($"4. Set number of Hit Dice{Constants.TabStop}5. Set size of Hit Dice");
                        sb.AppendLine($"6. Set Strength{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}7. Set Dexterity");
                        sb.AppendLine($"8. Set Constitution{Constants.TabStop}{Constants.TabStop}9. Set Intelligence");
                        sb.AppendLine($"10. Set Wisdom{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}11. Set Charisma");
                        sb.AppendLine($"12. Set Base Exp{Constants.TabStop}{Constants.TabStop}13. Set Gold");
                        sb.AppendLine($"14. Add Flag{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}15. Remove Flag");
                        sb.AppendLine($"16. Set Arrival Message{Constants.TabStop}{Constants.TabStop}17. Set Departure Message");
                        sb.AppendLine($"18. Set Max Number{Constants.TabStop}{Constants.TabStop}19. Set Appearance Frequency");
                        sb.AppendLine($"20. Set Armour Class{Constants.TabStop}21. Set NPC Zone{Constants.TabStop}22. Set Gender");
                        sb.AppendLine($"23. Set Number of Attacks{Constants.TabStop}24. Set Head Equip");
                        sb.AppendLine($"25. Set Neck Equip{Constants.TabStop}26. Set Armour Equip");
                        sb.AppendLine($"27. Set Left Finger Equip{Constants.TabStop}28. Set Right Finger Equip");
                        sb.AppendLine($"29. Set Weapon Equip{Constants.TabStop}30. Set Held Equip");
                        sb.AppendLine($"31 Add Skill{Constants.TabStop}32. Remove Skill");
                        sb.AppendLine($"33. Add Spell{Constants.TabStop}34. Remove Spell");
                        sb.AppendLine("35. Set Alignment");
                        sb.AppendLine($"36. Save NPC{Constants.TabStop}37. Exit without saving");
                        sb.Append("Selection: ");
                        desc.Send(sb.ToString());
                        var opt = desc.Read().Trim();
                        if(Helpers.ValidateInput(opt) && uint.TryParse(opt, out uint option))
                        {
                            if(option >= 1 && option <= 37)
                            {
                                switch (option)
                                {
                                    case 1:
                                        npc.Name = GetAssetStringValue(ref desc, "Enter NPC Name: ");
                                        break;

                                    case 2:
                                        npc.ShortDescription = GetAssetStringValue(ref desc, "Enter Short Description: ");
                                        break;

                                    case 3:
                                        npc.LongDescription = Helpers.GetLongDescription(ref desc);
                                        break;

                                    case 4:
                                        npc.NumberOfHitDice = GetAssetUintValue(ref desc, "Enter number of Hit Dice: ");
                                        break;

                                    case 5:
                                        npc.SizeOfHitDice = GetAssetUintValue(ref desc, "Enter size of Hit Dice: ");
                                        break;

                                    case 6:
                                        npc.Stats.Strength = GetAssetUintValue(ref desc, "Set NPC Strength: ");
                                        break;

                                    case 7:
                                        npc.Stats.Dexterity = GetAssetUintValue(ref desc, "Set NPC Dexterity: ");
                                        break;

                                    case 8:
                                        npc.Stats.Constitution = GetAssetUintValue(ref desc, "Set NPC Constitution: ");
                                        break;

                                    case 9:
                                        npc.Stats.Intelligence = GetAssetUintValue(ref desc, "Set NPC Intelligence: ");
                                        break;

                                    case 10:
                                        npc.Stats.Wisdom = GetAssetUintValue(ref desc, "Set NPC Wisdom: ");
                                        break;

                                    case 11:
                                        npc.Stats.Charisma = GetAssetUintValue(ref desc, "Set NPC Charisma: ");
                                        break;

                                    case 12:
                                        npc.BaseExpAward = GetAssetUintValue(ref desc, "Set Base Exp: ");
                                        break;

                                    case 13:
                                        npc.Stats.Gold = GetAssetUintValue(ref desc, "Set Gold: ");
                                        break;

                                    case 14:
                                        var nf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                        if (nf != NPCFlags.None && !npc.BehaviourFlags.HasFlag(nf))
                                        {
                                            npc.BehaviourFlags |= nf;
                                        }
                                        else
                                        {
                                            npc.BehaviourFlags = NPCFlags.None;
                                        }
                                        break;

                                    case 15:
                                        var rf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                        if (rf != NPCFlags.None && npc.BehaviourFlags.HasFlag(rf))
                                        {
                                            npc.BehaviourFlags &= ~rf;
                                        }
                                        break;

                                    case 16:
                                        npc.ArrivalMessage = GetAssetStringValue(ref desc, "Enter Arrival Message: ");
                                        break;

                                    case 17:
                                        npc.DepartMessage = GetAssetStringValue(ref desc, "Enter Departure Message: ");
                                        break;

                                    case 18:
                                        npc.MaxNumber = GetAssetUintValue(ref desc, "Enter Max Number: ");
                                        break;

                                    case 19:
                                        npc.AppearChance = GetAssetUintValue(ref desc, "Enter Appearance Chance: ");
                                        break;

                                    case 20:
                                        npc.Stats.ArmourClass = GetAssetUintValue(ref desc, "Enter Armour Class: ");
                                        break;

                                    case 21:
                                        npc.AppearsInZone = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                        break;

                                    case 22:
                                        npc.Gender = GetAssetEnumValue<Gender>(ref desc, "Enter Gender: ");
                                        break;

                                    case 23:
                                        npc.NumberOfAttacks = GetAssetUintValue(ref desc, "Enter Number of Attacks: ");
                                        break;

                                    case 24:
                                        var itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if(itemNo == 0)
                                        {
                                            npc.EquippedItems.Head = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if(item != null)
                                            {
                                                if(item.Slot.HasFlag(WearSlot.Head))
                                                {
                                                    npc.EquippedItems.Head = item;
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

                                    case 25:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            npc.EquippedItems.Neck = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Neck))
                                                {
                                                    npc.EquippedItems.Neck = item;
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

                                    case 26:
                                        itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                        if (itemNo == 0)
                                        {
                                            npc.EquippedItems.Armour = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Armour))
                                                {
                                                    npc.EquippedItems.Armour = item;
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
                                            npc.EquippedItems.FingerLeft = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.FingerLeft))
                                                {
                                                    npc.EquippedItems.FingerLeft = item;
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
                                            npc.EquippedItems.FingerRight = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.FingerRight))
                                                {
                                                    npc.EquippedItems.FingerRight = item;
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
                                            npc.EquippedItems.Weapon = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Weapon))
                                                {
                                                    npc.EquippedItems.Weapon = item;
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
                                            npc.EquippedItems.Held = null;
                                        }
                                        else
                                        {
                                            var item = ItemManager.Instance.GetItemByID(itemNo);
                                            if (item != null)
                                            {
                                                if (item.Slot.HasFlag(WearSlot.Held))
                                                {
                                                    npc.EquippedItems.Held = item;
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
                                        var sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                        if(Skills.SkillExists(sk) && !npc.HasSkill(sk))
                                        {
                                            npc.AddSkill(sk);
                                        }
                                        break;

                                    case 32:
                                        sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                        if(Skills.SkillExists(sk) && npc.HasSkill(sk))
                                        {
                                            npc.RemoveSkill(sk);
                                        }
                                        break;

                                    case 33:
                                        var sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                        if(Spells.SpellExists(sp) && !npc.HasSpell(sp))
                                        {
                                            npc.AddSpell(sp);
                                        }
                                        break;

                                    case 34:
                                        sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                        if(Spells.SpellExists(sp) && npc.HasSpell(sp))
                                        {
                                            npc.RemoveSpell(sp);
                                        }
                                        break;

                                    case 35:
                                        npc.Alignment = GetAssetEnumValue<ActorAlignment>(ref desc, "Enter Alignment: ");
                                        break;

                                    case 36:
                                        if (ValidateNPCAsset(ref desc, ref npc, false))
                                        {
                                            if(DatabaseManager.UpdateNPCByID(ref desc, ref npc))
                                            {
                                                if(NPCManager.Instance.UpdateNPC(ref desc, npc))
                                                {
                                                    desc.Send($"NPC updated in World database and NPC Manager{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update NPC in NPC Manager, it may not be available until the World is restarted{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update NPC in the World database{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 37:
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
                else
                {
                    desc.Send($"No NPC with ID could be found{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Create
        private static void CreateNewNPC(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("NPCs, or non-player characters populate the world. They can be anything from shopkeepers to dragons, rabbits to royal guards.");
            sb.AppendLine("An NPC shares similar attributes to a Player Character. They can be spawned into a zone periodically, or set to be loaded into Rooms when the World starts.");
            sb.AppendLine("NPCs will move around the Zone they are assigned to (unless they have the Sentinel flag).");
            desc.Send(sb.ToString());
            NPC newNPC = new NPC();
            newNPC.EquippedItems = new EquippedItems();
            newNPC.Type = ActorType.NonPlayer;
            while(!okToReturn)
            {
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine($"NPC ID: {newNPC.NPCID}{Constants.TabStop}NPC Zone: {newNPC.AppearsInZone}");
                sb.AppendLine($"NPC Name: {newNPC.Name}{Constants.TabStop}Gender: {newNPC.Gender}");
                sb.AppendLine($"Frequency: {newNPC.AppearChance}{Constants.TabStop}{Constants.TabStop}Max Number: {newNPC.MaxNumber}");
                sb.AppendLine($"Short Description: {newNPC.ShortDescription}");
                sb.AppendLine($"Long Description: {newNPC.LongDescription}");
                sb.AppendLine($"Number of Hit Dice: {newNPC.NumberOfHitDice}{Constants.TabStop}Size of Hit Dice: {newNPC.SizeOfHitDice}");
                sb.AppendLine($"Strength: {newNPC.Stats.Strength}{Constants.TabStop}Dexterity: {newNPC.Stats.Dexterity}");
                sb.AppendLine($"Constitution: {newNPC.Stats.Constitution}{Constants.TabStop}Intelligence: {newNPC.Stats.Intelligence}");
                sb.AppendLine($"Wisdom: {newNPC.Stats.Wisdom}{Constants.TabStop}Charisma: {newNPC.Stats.Charisma}");
                sb.AppendLine($"Armour Class: {newNPC.Stats.ArmourClass}{Constants.TabStop}Base Exp: {newNPC.BaseExpAward}");
                sb.AppendLine($"Gold: {newNPC.Stats.Gold}{Constants.TabStop}NPC Flags: {newNPC.BehaviourFlags}");
                sb.AppendLine($"Alginment: {newNPC.Alignment}{Constants.TabStop}No. Of Attacks: {newNPC.NumberOfAttacks}");
                sb.AppendLine($"Skills: {string.Join(", ", newNPC.Skills)}");
                sb.AppendLine($"Spells: {string.Join(", ", newNPC.Spells)}");
                sb.AppendLine($"Arrival Message: {newNPC.ArrivalMessage}");
                sb.AppendLine($"Departure Message: {newNPC.DepartMessage}");
                sb.AppendLine();
                sb.AppendLine("Equipment:");
                sb.AppendLine($"Head: {newNPC.EquippedItems.Head}");
                sb.AppendLine($"Neck: {newNPC.EquippedItems.Neck}");
                sb.AppendLine($"Armour: {newNPC.EquippedItems.Armour}");
                sb.AppendLine($"Left Finger: {newNPC.EquippedItems.FingerLeft}");
                sb.AppendLine($"Right Finger: {newNPC.EquippedItems.FingerRight}");
                sb.AppendLine($"Weapon: {newNPC.EquippedItems.Weapon}");
                sb.AppendLine($"Held: {newNPC.EquippedItems.Held}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set NPC ID{Constants.TabStop}2. Set NPC Name");
                sb.AppendLine($"3. Set Short Description{Constants.TabStop}4. Set Long Description");
                sb.AppendLine($"5. Set number of Hit Dice{Constants.TabStop}6. Set size of Hit Dice");
                sb.AppendLine($"7. Set Strength{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}8. Set Dexterity");
                sb.AppendLine($"9. Set Constitution{Constants.TabStop}{Constants.TabStop}10. Set Intelligence");
                sb.AppendLine($"11. Set Wisdom{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}12. Set Charisma");
                sb.AppendLine($"13. Set Base Exp{Constants.TabStop}{Constants.TabStop}14. Set Gold");
                sb.AppendLine($"15. Add Flag{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}16. Remove Flag");
                sb.AppendLine($"17. Set Arrival Message{Constants.TabStop}{Constants.TabStop}18. Set Departure Message");
                sb.AppendLine($"19. Set Max Number{Constants.TabStop}{Constants.TabStop}20. Set Appearance Frequency");
                sb.AppendLine($"21. Set Armour Class{Constants.TabStop}22. Set NPC Zone{Constants.TabStop}23. Set NPC Gender");
                sb.AppendLine($"24. Set Number of Attacks{Constants.TabStop}25. Set Head Equip");
                sb.AppendLine($"26. Set Neck Equip{Constants.TabStop}27. Set Armour Equip");
                sb.AppendLine($"28. Set Left Finger Equip{Constants.TabStop}29. Set Right Finger Equip");
                sb.AppendLine($"30. Set Weapon Equip{Constants.TabStop}31. Set Held Equip");
                sb.AppendLine($"32. Add Skill{Constants.TabStop}33. Remove Skill");
                sb.AppendLine($"34. Add Spell{Constants.TabStop}35. Remove Spell");
                sb.AppendLine("36. Set Alignment");
                sb.AppendLine($"37. Save NPC{Constants.TabStop}38. Exit without saving");
                sb.Append("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if(Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if(option >= 1 && option <= 38)
                    {
                        switch(option)
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
                                newNPC.NumberOfHitDice = GetAssetUintValue(ref desc, "Enter number of Hit Dice: ");
                                break;

                            case 6:
                                newNPC.SizeOfHitDice = GetAssetUintValue(ref desc, "Enter size of Hit Dice: ");
                                break;

                            case 7:
                                newNPC.Stats.Strength = GetAssetUintValue(ref desc, "Set NPC Strength: ");
                                break;

                            case 8:
                                newNPC.Stats.Dexterity = GetAssetUintValue(ref desc, "Set NPC Dexterity: ");
                                break;

                            case 9:
                                newNPC.Stats.Constitution = GetAssetUintValue(ref desc, "Set NPC Constitution: ");
                                break;

                            case 10:
                                newNPC.Stats.Intelligence = GetAssetUintValue(ref desc, "Set NPC Intelligence: ");
                                break;

                            case 11:
                                newNPC.Stats.Wisdom = GetAssetUintValue(ref desc, "Set NPC Wisdom: ");
                                break;

                            case 12:
                                newNPC.Stats.Charisma = GetAssetUintValue(ref desc, "Set NPC Charisma: ");
                                break;

                            case 13:
                                newNPC.BaseExpAward = GetAssetUintValue(ref desc, "Set Base Exp: ");
                                break;

                            case 14:
                                newNPC.Stats.Gold = GetAssetUintValue(ref desc, "Set Gold: ");
                                break;

                            case 15:
                                var nf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                if(nf != NPCFlags.None && !newNPC.BehaviourFlags.HasFlag(nf))
                                {
                                    newNPC.BehaviourFlags |= nf;
                                }
                                else
                                {
                                    newNPC.BehaviourFlags = NPCFlags.None;
                                }
                                break;

                            case 16:
                                var rf = GetAssetEnumValue<NPCFlags>(ref desc, "Enter Flag: ");
                                if(rf != NPCFlags.None && newNPC.BehaviourFlags.HasFlag(rf))
                                {
                                    newNPC.BehaviourFlags &= ~rf;
                                }
                                break;

                            case 17:
                                newNPC.ArrivalMessage = GetAssetStringValue(ref desc, "Enter Arrival Message: ");
                                break;

                            case 18:
                                newNPC.DepartMessage = GetAssetStringValue(ref desc, "Enter Departure Message: ");
                                break;

                            case 19:
                                newNPC.MaxNumber = GetAssetUintValue(ref desc, "Enter Max Number: ");
                                break;

                            case 20:
                                newNPC.AppearChance = GetAssetUintValue(ref desc, "Enter Appearance Chance: ");
                                break;

                            case 21:
                                newNPC.Stats.ArmourClass = GetAssetUintValue(ref desc, "Enter Armour Class: ");
                                break;

                            case 22:
                                newNPC.AppearsInZone = GetAssetUintValue(ref desc, "Enter Zone ID: ");
                                break;

                            case 23:
                                newNPC.Gender = GetAssetEnumValue<Gender>(ref desc, "Enter Gender: ");
                                break;

                            case 24:
                                newNPC.NumberOfAttacks = GetAssetUintValue(ref desc, "Enter Number of Attacks: ");
                                break;

                            case 25:
                                var itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquippedItems.Head = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Head))
                                        {
                                            newNPC.EquippedItems.Head = item;
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

                            case 26:
                                itemNo = GetAssetUintValue(ref desc, "Enter Item Number (0 to clear): ");
                                if (itemNo == 0)
                                {
                                    newNPC.EquippedItems.Neck = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Neck))
                                        {
                                            newNPC.EquippedItems.Neck = item;
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
                                    newNPC.EquippedItems.Armour = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Armour))
                                        {
                                            newNPC.EquippedItems.Armour = item;
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
                                    newNPC.EquippedItems.FingerLeft = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.FingerLeft))
                                        {
                                            newNPC.EquippedItems.FingerLeft = item;
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
                                    newNPC.EquippedItems.FingerRight = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.FingerRight))
                                        {
                                            newNPC.EquippedItems.FingerRight = item;
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
                                    newNPC.EquippedItems.Weapon = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Weapon))
                                        {
                                            newNPC.EquippedItems.Weapon = item;
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
                                    newNPC.EquippedItems.Held = null;
                                }
                                else
                                {
                                    var item = ItemManager.Instance.GetItemByID(itemNo);
                                    if (item != null)
                                    {
                                        if (item.Slot.HasFlag(WearSlot.Held))
                                        {
                                            newNPC.EquippedItems.Held = item;
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
                                var sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if(Skills.SkillExists(sk) && !newNPC.HasSkill(sk))
                                {
                                    newNPC.AddSkill(sk);
                                }
                                break;

                            case 33:
                                sk = GetAssetStringValue(ref desc, "Enter Skill Name: ");
                                if(Skills.SkillExists(sk) && newNPC.HasSkill(sk))
                                {
                                    newNPC.RemoveSkill(sk);
                                }
                                break;

                            case 34:
                                var sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                if(Spells.SpellExists(sp) && !newNPC.HasSpell(sp))
                                {
                                    newNPC.AddSpell(sp);
                                }
                                break;

                            case 35:
                                sp = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                if(Spells.SpellExists(sp) && newNPC.HasSpell(sp))
                                {
                                    newNPC.RemoveSpell(sp);
                                }
                                break;

                            case 36:
                                newNPC.Alignment = GetAssetEnumValue<ActorAlignment>(ref desc, "Enter Alignment: ");
                                break;

                            case 37:
                                if(ValidateNPCAsset(ref desc, ref newNPC, true))
                                {
                                    if(DatabaseManager.AddNewNPC(ref desc, ref newNPC))
                                    {
                                        if(NPCManager.Instance.AddNewNPC(newNPC))
                                        {
                                            desc.Send($"New NPC added to NPC Manager and World database{Constants.NewLine}");
                                            Game.LogMessage($"Player {desc.Player.Name} successfully created new NPC: {newNPC.Name} ({newNPC.NPCID})", LogLevel.Info, true);
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

                            case 38:
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

        #region Functions
        private static bool ValidateNPCAsset(ref Descriptor desc, ref NPC n, bool isNewNPC)
        {
            if(string.IsNullOrEmpty(n.Name) || string.IsNullOrEmpty(n.ShortDescription) || string.IsNullOrEmpty(n.LongDescription))
            {
                desc.Send($"One or more required attributes are missing values{Constants.NewLine}");
                return false;
            }
            if((isNewNPC && NPCManager.Instance.NPCExists(n.NPCID)) || n.NPCID == 0)
            {
                desc.Send($"NPC ID already in use or was 0{Constants.NewLine}");
                return false;
            }
            if(n.AppearsInZone == 0)
            {
                desc.Send($"NPC must be assigned a Zone{Constants.NewLine}");
                return false;
            }
            if(n.Stats.Strength == 0 || n.Stats.Dexterity == 0 || n.Stats.Constitution == 0 || n.Stats.Intelligence == 0 || n.Stats.Wisdom == 0 || n.Stats.Charisma== 0)
            {
                desc.Send($"One or more stats are 0 but should have a value{Constants.NewLine}");
                return false;
            }
            if(n.BaseExpAward == 0)
            {
                desc.Send($"Base EXP for this NPC is 0 but should have a value{Constants.NewLine}");
                return false;
            }
            if(n.BehaviourFlags.HasFlag(NPCFlags.Hostile) && n.BehaviourFlags.HasFlag(NPCFlags.NoAttack))
            {
                desc.Send($"An NPC cannot have the Hostile and NoAttack flags at the same time{Constants.NewLine}");
                return false;
            }
            if(string.IsNullOrEmpty(n.DepartMessage) || string.IsNullOrEmpty(n.ArrivalMessage))
            {
                desc.Send($"Arrive or Depart message is blank but should have a value{Constants.NewLine}");
                return false;
            }
            if(n.NumberOfAttacks == 0)
            {
                desc.Send($"The Number of Attacks value must be at least 1{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}
