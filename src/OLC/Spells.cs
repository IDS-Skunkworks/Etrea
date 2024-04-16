using Etrea2.Core;
using Etrea2.Entities;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create
        private static void CreateNewSpell(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            desc.Send($"A Spell is a magical ability that allows a player or NPC to harm, heal or hinder someone.{Constants.NewLine}");
            Spell newSpell = new Spell
            {
                SpellName = "New Spell"
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Name: {newSpell.SpellName}");
                sb.AppendLine($"Description: {newSpell.Description}");
                sb.AppendLine($"Type: {newSpell.SpellType}{Constants.TabStop}Element: {newSpell.SpellElement}");
                sb.AppendLine($"MP Cost: {newSpell.MPCost}{Constants.TabStop}Damage Dice: {newSpell.NumOfDamageDice}{Constants.TabStop}Damage Dice Size: {newSpell.SizeOfDamageDice}{Constants.TabStop}Additional Damage: {newSpell.AdditionalDamage}");
                sb.AppendLine($"Auto Hit?: {newSpell.AutoHitTarget}{Constants.TabStop}AOE Spell?: {newSpell.AOESpell}{Constants.TabStop}{Constants.TabStop}Gold to Learn: {newSpell.GoldToLearn:N0}");
                sb.AppendLine($"Bypass Resistance?: {newSpell.BypassResistCheck}{Constants.TabStop}Apply Ability Modifier?: {newSpell.ApplyAbilityModifier}");
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name{Constants.TabStop}2. Set Description{Constants.TabStop}3. Set Type{Constants.TabStop}4. Set Element");
                sb.AppendLine($"5. Set MP Cost{Constants.TabStop}6. Set Number of Damage Dice{Constants.TabStop}7. Set Size of Damage Dice");
                sb.AppendLine($"8. Toggle Auto Hit{Constants.TabStop}9. Toggle AOE{Constants.TabStop}10. Set Gold to Learn");
                sb.AppendLine($"11. Toggle Bypass Resistance{Constants.TabStop}12. Toggle Apply Ability Modifier");
                sb.AppendLine($"13. Set Additional Damage{Constants.TabStop}14. Save{Constants.TabStop}15. Exit");
                sb.AppendLine("Selection:");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 15)
                    {
                        switch(option)
                        {
                            case 1:
                                newSpell.SpellName = GetAssetStringValue(ref desc, "Enter Spell Name: ");
                                break;

                            case 2:
                                newSpell.Description = GetAssetStringValue(ref desc, "Enter Spell Description");
                                break;

                            case 3:
                                newSpell.SpellType = GetAssetEnumValue<SpellType>(ref desc, "Enter Spell Type: ");
                                break;

                            case 4:
                                newSpell.SpellElement = GetAssetEnumValue<SpellElement>(ref desc, "Enter Spell Element: ");
                                break;

                            case 5:
                                newSpell.MPCost = GetAssetUintValue(ref desc, "Enter MP Cost: ");
                                break;

                            case 6:
                                newSpell.NumOfDamageDice = GetAssetUintValue(ref desc, "Enter number of Damage Dice: ");
                                break;

                            case 7:
                                newSpell.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter size of Damage Dice: ");
                                break;

                            case 8:
                                newSpell.AutoHitTarget = !newSpell.AutoHitTarget;
                                break;

                            case 9:
                                newSpell.AOESpell = !newSpell.AOESpell;
                                break;

                            case 10:
                                newSpell.GoldToLearn = GetAssetUintValue(ref desc, "Enter Gold to Learn: ");
                                break;

                            case 11:
                                newSpell.BypassResistCheck = !newSpell.BypassResistCheck;
                                break;

                            case 12:
                                newSpell.ApplyAbilityModifier = !newSpell.ApplyAbilityModifier;
                                break;

                            case 13:
                                newSpell.AdditionalDamage = GetAssetUintValue(ref desc, "Enter Additional Damage: ");
                                break;

                            case 14:
                                if (ValidateSpell(ref desc, ref newSpell, true))
                                {
                                    if (DatabaseManager.AddNewSpell(ref newSpell))
                                    {
                                        if (SpellManager.Instance.AddSpell(ref desc, newSpell))
                                        {
                                            desc.Send($"New Spell added to SpellManager and World database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to add the new Spell to the World database{Constants.NewLine}");
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
                        desc.Send($"{option} is not a valid choice.{Constants.NewLine}");
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
        private static void EditExistingSpell(ref Descriptor desc)
        {
            desc.Send($"Enter the name of the Spell to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (SpellManager.Instance.SpellExists(input))
            {
                var s = SpellManager.Instance.GetSpell(input).ShallowCopy();
                bool okToReturn = false;
                StringBuilder sb = new StringBuilder();
                while (!okToReturn)
                {
                    sb.Clear();
                    sb.AppendLine($"Name: {s.SpellName}");
                    sb.AppendLine($"Description: {s.Description}");
                    sb.AppendLine($"Type: {s.SpellType}{Constants.TabStop}Element: {s.SpellElement}");
                    sb.AppendLine($"MP Cost: {s.MPCost}{Constants.TabStop}Damage Dice: {s.NumOfDamageDice}{Constants.TabStop}Damage Dice Size: {s.SizeOfDamageDice}{Constants.TabStop}Additional Damage: {s.AdditionalDamage}");
                    sb.AppendLine($"Auto Hit?: {s.AutoHitTarget}{Constants.TabStop}AOE Spell?: {s.AOESpell}{Constants.TabStop}Gold to Learn: {s.GoldToLearn:N0}");
                    sb.AppendLine($"Bypass Resistance?: {s.BypassResistCheck}{Constants.TabStop}Apply Ability Modifier?: {s.ApplyAbilityModifier}");
                    sb.AppendLine();
                    sb.AppendLine("Options:");
                    sb.AppendLine($"1. Set Description{Constants.TabStop}2. Set Type{Constants.TabStop}3. Set Element");
                    sb.AppendLine($"4. Set MP Cost{Constants.TabStop}5. Set Number of Damage Dice{Constants.TabStop}6. Set Size of Damage Dice");
                    sb.AppendLine($"7. Toggle Auto Hit{Constants.TabStop}8. Toggle AOE{Constants.TabStop}9. Set Gold to Learn");
                    sb.AppendLine($"10. Toggle Bypass Resistance{Constants.TabStop}11. Toggle Apply Ability Modifier");
                    sb.AppendLine($"12. Set Additional Damage{Constants.TabStop}13. Save{Constants.TabStop}14. Exit");
                    sb.AppendLine("Selection:");
                    desc.Send(sb.ToString());
                    input = desc.Read().Trim();
                    if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                    {
                        if (option >= 1 && option <= 14)
                        {
                            switch (option)
                            {
                                case 1:
                                    s.Description = GetAssetStringValue(ref desc, "Enter Spell Description");
                                    break;

                                case 2:
                                    s.SpellType = GetAssetEnumValue<SpellType>(ref desc, "Enter Spell Type: ");
                                    break;

                                case 3:
                                    s.SpellElement = GetAssetEnumValue<SpellElement>(ref desc, "Enter Spell Element: ");
                                    break;

                                case 4:
                                    s.MPCost = GetAssetUintValue(ref desc, "Enter MP Cost: ");
                                    break;

                                case 5:
                                    s.NumOfDamageDice = GetAssetUintValue(ref desc, "Enter number of Damage Dice: ");
                                    break;

                                case 6:
                                    s.SizeOfDamageDice = GetAssetUintValue(ref desc, "Enter size of Damage Dice: ");
                                    break;

                                case 7:
                                    s.AutoHitTarget = !s.AutoHitTarget;
                                    break;

                                case 8:
                                    s.AOESpell = !s.AOESpell;
                                    break;

                                case 9:
                                    s.GoldToLearn = GetAssetUintValue(ref desc, "Enter Gold to Learn: ");
                                    break;

                                case 10:
                                    s.BypassResistCheck = !s.BypassResistCheck;
                                    break;

                                case 11:
                                    s.ApplyAbilityModifier = !s.ApplyAbilityModifier;
                                    break;

                                case 12:
                                    s.AdditionalDamage = GetAssetUintValue(ref desc, "Enter Additional Damage: ");
                                    break;

                                case 13:
                                    if (ValidateSpell(ref desc, ref s, true))
                                    {
                                        if (DatabaseManager.UpdateSpell(ref s))
                                        {
                                            if (SpellManager.Instance.UpdateSpell(ref desc, s))
                                            {
                                                desc.Send($"Spell updated in SpellManager and World database{Constants.NewLine}");
                                                okToReturn = true;
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to update the Spell in the World database{Constants.NewLine}");
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
                            desc.Send($"{option} is not a valid choice.{Constants.NewLine}");
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
                desc.Send($"No Spell with that name could be found{Constants.TabStop}");
            }
        }
        #endregion

        #region Delete
        private static void DeleteSpell(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a database backup is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send("Enter the name of the Spell to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (SpellManager.Instance.SpellExists(input))
            {
                var s = SpellManager.Instance.GetSpell(input);
                if (DatabaseManager.DeleteSpell(s.SpellName))
                {
                    if (SpellManager.Instance.RemoveSpell(ref desc, s))
                    {
                        desc.Send($"Spell successfully removed from SpellManager and World database{Constants.TabStop}");
                    }
                    else
                    {
                        desc.Send($"Unable to remove Spell from SpellManager{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Unable to remove Spell from World database{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"No Spell with that name could be found{Constants.NewLine}");
            }
        }
        #endregion

        #region Validation Functions
        private static bool ValidateSpell(ref Descriptor desc, ref Spell _spell, bool isNewSpell)
        {
            if (string.IsNullOrEmpty(_spell.SpellName) || string.IsNullOrEmpty(_spell.Description))
            {
                desc.Send($"Spell Name and Description must be provided{Constants.NewLine}");
                return false;
            }
            if (isNewSpell && SpellManager.Instance.SpellExists(_spell.SpellName))
            {
                desc.Send($"A Spell with the same name already exists{Constants.TabStop}");
                return false;
            }
            if (_spell.SpellType == SpellType.Damage || _spell.SpellType == SpellType.Healing)
            {
                if (_spell.NumOfDamageDice == 0 || _spell.SizeOfDamageDice == 0)
                {
                    desc.Send($"If a spell heals or causes damage, the number and size of damage dice must be provided{Constants.NewLine}");
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}