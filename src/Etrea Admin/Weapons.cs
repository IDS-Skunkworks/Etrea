using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3;
using Etrea3.Core;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Weapon> allWeapons = new List<Weapon>();
        private static ListViewItemComparer weaponComparer;

        #region Event Handlers
        private void rtxtBxWeaponLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxWeaponLongDesc.GetLineFromCharIndex(rtxtBxWeaponLongDesc.SelectionStart);
            string currentLine = rtxtBxWeaponLongDesc.Lines.Length > currentLineIndex ? rtxtBxWeaponLongDesc.Lines[currentLineIndex] : string.Empty;
            lblWeaponDescLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private void btnLoadWeapons_Click(object sender, EventArgs e)
        {
            GetWeapons();
        }

        private async void btnAddWeapon_Click(object sender, EventArgs e)
        {
            if (!ValidateWeaponData())
            {
                return;
            }
            var newWeapon = GetWeaponFromFormData();
            if (newWeapon == null)
            {
                return;
            }
            btnAddWeapon.Enabled = false;
            var wpnJson = Helpers.SerialiseEtreaObject<Weapon>(newWeapon);
            if (await APIHelper.AddNewAsset("/item", wpnJson))
            {
                GetWeapons();
            }
            btnAddWeapon.Enabled = true;
        }

        private async void btnUpdateWeapon_Click(object sender, EventArgs e)
        {
            if (!ValidateWeaponData())
            {
                return;
            }
            var newWeapon = GetWeaponFromFormData();
            if (newWeapon == null)
            {
                return;
            }
            btnUpdateWeapon.Enabled = false;
            var wpnJson = Helpers.SerialiseEtreaObject<Weapon>(newWeapon);
            if (await APIHelper.UpdateExistingAsset("/item", wpnJson))
            {
                GetWeapons();
            }
            btnUpdateWeapon.Enabled = true;
        }

        private void btnClearWeaponForm_Click(object sender, EventArgs e)
        {
            ClearWeaponForm();
        }

        private async void btnDeleteWeapon_Click(object sender, EventArgs e)
        {
            if (listViewWeapons.SelectedItems.Count == 0)
            {
                return;
            }
            var wpn = allWeapons.FirstOrDefault(x => x.ID == int.Parse(listViewWeapons.SelectedItems[0].SubItems[0].Text));
            if (wpn == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Weapon? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteWeapon.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/item/{wpn.ID}"))
                {
                    GetWeapons();
                }
                btnDeleteWeapon.Enabled = true;
            }
        }

        private void listViewWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewWeapons.SelectedItems.Count == 0)
            {
                return;
            }
            var obj = listViewWeapons.SelectedItems[0];
            var wpn = allWeapons.FirstOrDefault(x => x.ID == int.Parse(obj.SubItems[0].Text));
            if (wpn != null)
            {
                ClearWeaponForm();
                txtBxWeaponID.Text = wpn.ID.ToString();
                txtBxWeaponName.Text = wpn.Name;
                txtBxWeaponValue.Text = wpn.BaseValue.ToString();
                txtBxWeaponShortDesc.Text = wpn.ShortDescription;
                chkBxWeaponMagical.Checked = wpn.IsMagical;
                chkBxWeaponCursed.Checked = wpn.IsCursed;
                chkBxWeaponTwoHanded.Checked = wpn.IsTwoHanded;
                txtBxWeaponType.Text = wpn.WeaponType.ToString();
                txtBxWeaponNoOfDamDice.Text = wpn.NumberOfDamageDice.ToString();
                txtBxWeaponSizeOfDamDice.Text = wpn.SizeOfDamageDice.ToString();
                txtBxWeaponHitMod.Text = wpn.HitModifier.ToString();
                txtBxWeaponDamageMod.Text = wpn.DamageModifier.ToString();
                if (wpn.RequiredSkills.Count > 0)
                {
                    foreach(string skill in wpn.RequiredSkills)
                    {
                        lstBxWeaponSkills.Items.Add(skill);
                    }
                }
                if (wpn.AppliedBuffs.Count > 0)
                {
                    foreach(string buff in wpn.AppliedBuffs)
                    {
                        lstBxWeaponBuffs.Items.Add(buff);
                    }
                }
                rtxtBxWeaponLongDesc.Text = wpn.LongDescription;
            }
        }

        private void btnAddWeaponSkill_Click(object sender, EventArgs e)
        {
            using (var ss = new SkillSelector())
            {
                if (ss.ShowDialog() == DialogResult.OK)
                {
                    string selectedSkill = ss._selectedSkill;
                    lstBxWeaponSkills.Items.Add (selectedSkill);
                }
            }
        }

        private void btnRemoveWeaponSkill_Click(object sender, EventArgs e)
        {
            if (lstBxWeaponSkills.SelectedItem != null)
            {
                var obj = lstBxWeaponSkills.SelectedItem;
                lstBxWeaponSkills.Items.Remove(obj);
            }
        }

        private void btnClearWeaponSkills_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all required Skills for this Weapon?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lstBxWeaponSkills.Items.Clear();
            }
        }

        private void btnWeaponAddBuff_Click(object sender, EventArgs e)
        {
            using (var bs = new BuffSelector())
            {
                if (bs.ShowDialog() == DialogResult.OK)
                {
                    string selectedBuff = bs._selectedBuff;
                    lstBxWeaponBuffs.Items.Add (selectedBuff);
                }
            }
        }

        private void btnWeaponRemoveBuff_Click(object sender, EventArgs e)
        {
            if (lstBxWeaponBuffs.SelectedItem != null)
            {
                var obj = lstBxWeaponBuffs.SelectedItem;
                lstBxWeaponBuffs.Items.Remove(obj);
            }
        }

        private void btnWeaponClearBuffs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all Buffs for this Weapon?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lstBxWeaponBuffs.Items.Clear();
            }
        }
        #endregion

        #region Functions
        private async void GetWeapons()
        {
            allWeapons.Clear();
            listViewWeapons.Items.Clear();
            ClearWeaponForm();
            btnLoadWeapons.Enabled = false;
            int nextID = 1;
            var result = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (result != null)
            {
                nextID = result.Max(x => x.ID) + 1;
                foreach (var item in result.OrderBy(x => x.ID))
                {
                    if (item.ItemType == ItemType.Weapon)
                    {
                        Weapon wpn = (Weapon)item;
                        allWeapons.Add(wpn);
                        listViewWeapons.Items.Add(new ListViewItem(new[]
                        {
                            wpn.ID.ToString(),
                            wpn.Name,
                            wpn.BaseValue.ToString(),
                            wpn.ShortDescription,
                            $"{wpn.NumberOfDamageDice}D{wpn.SizeOfDamageDice}",
                            wpn.IsMagical.ToString(),
                            wpn.MonsterOnly.ToString(),
                            wpn.IsTwoHanded.ToString(),
                            wpn.WeaponType.ToString(),
                            wpn.AppliesBuffs.ToString(),
                            wpn.IsCursed.ToString(),
                            wpn.DamageModifier.ToString(),
                            wpn.HitModifier.ToString(),
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewWeapons.Columns)
                {
                    h.Width = -2;
                }
            }
            lblNextWeaponID.Text = $"Next ID: {nextID}";
            btnLoadWeapons.Enabled = true;
        }

        private void ClearWeaponForm()
        {
            txtBxWeaponID.Clear();
            txtBxWeaponName.Clear();
            txtBxWeaponValue.Clear();
            txtBxWeaponShortDesc.Clear();
            chkBxWeaponMagical.Checked = false;
            chkBxWeaponCursed.Checked = false;
            chkBxWeaponTwoHanded.Checked = false;
            txtBxWeaponType.Clear();
            txtBxWeaponNoOfDamDice.Clear();
            txtBxWeaponSizeOfDamDice.Clear();
            txtBxWeaponHitMod.Clear();
            txtBxWeaponDamageMod.Clear();
            lstBxWeaponBuffs.Items.Clear();
            lstBxWeaponSkills.Items.Clear();
            rtxtBxWeaponLongDesc.Clear();
        }

        private bool ValidateWeaponData()
        {
            if (string.IsNullOrEmpty(txtBxWeaponID.Text))
            {
                MessageBox.Show("The Weapon must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxWeaponID.Text, out int wpnID) || wpnID < 1)
            {
                MessageBox.Show("The Weapon ID must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponName.Text))
            {
                MessageBox.Show("The Weapon must have a name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponValue.Text))
            {
                MessageBox.Show("The Weapon must have a value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxWeaponValue.Text, out int wpnValue) || wpnValue < 0)
            {
                MessageBox.Show("The Weapon value must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponShortDesc.Text))
            {
                MessageBox.Show("The Weapon must have a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxWeaponLongDesc.Text))
            {
                MessageBox.Show("The Weapon must have a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBxWeaponLongDesc.Checked)
            {
                foreach(var ln in rtxtBxWeaponLongDesc.Lines)
                {
                    if (ln.Length > 80)
                    {
                        MessageBox.Show("One or more lines in the Long Description are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            if (chkBxWeaponCursed.Checked && !chkBxWeaponMagical.Checked)
            {
                MessageBox.Show("If the Weapon has the Curse flag it must also have the Magical flag.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxWeaponType.Text, true, out WeaponType wpnType) || wpnType == WeaponType.Undefined)
            {
                MessageBox.Show("The Weapon Type is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponNoOfDamDice.Text))
            {
                MessageBox.Show("The Weapon must have a number of Damage Dice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxWeaponNoOfDamDice.Text, out int numDamDice) || numDamDice < 1)
            {
                MessageBox.Show("The Weapon must have at least 1 die for damage.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponSizeOfDamDice.Text))
            {
                MessageBox.Show("The Weapon must specify a size for the Damage Dice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxWeaponSizeOfDamDice.Text, out int sizeDamDice) ||  sizeDamDice < 1)
            {
                MessageBox.Show("The Damage Dice must have at least 1 side.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponHitMod.Text))
            {
                txtBxWeaponHitMod.Text = "0";
            }
            if (!int.TryParse(txtBxWeaponHitMod.Text, out int hitMod) || hitMod < 0)
            {
                MessageBox.Show("The Hit Modifier for the Weapon must be 0 or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxWeaponDamageMod.Text))
            {
                txtBxWeaponDamageMod.Text = "0";
            }
            if (!int.TryParse(txtBxWeaponDamageMod.Text, out int damageMod) || damageMod < 0)
            {
                MessageBox.Show("The Damage Modifier for the Weapon must be 0 or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private Weapon GetWeaponFromFormData()
        {
            Weapon newWeapon = new Weapon();
            try
            {
                newWeapon.ID = int.Parse(txtBxWeaponID.Text);
                newWeapon.Name = txtBxWeaponName.Text;
                newWeapon.ShortDescription = txtBxWeaponShortDesc.Text;
                newWeapon.BaseValue = int.Parse(txtBxWeaponValue.Text);
                newWeapon.IsMagical = chkBxWeaponMagical.Checked;
                newWeapon.MonsterOnly = chkBxWeaponMonster.Checked;
                newWeapon.IsTwoHanded = chkBxWeaponTwoHanded.Checked;
                newWeapon.IsCursed = chkBxWeaponCursed.Checked;
                newWeapon.NumberOfDamageDice = int.Parse(txtBxWeaponNoOfDamDice.Text);
                newWeapon.SizeOfDamageDice = int.Parse(txtBxWeaponSizeOfDamDice.Text);
                Enum.TryParse(txtBxWeaponType.Text, true, out WeaponType wpnType);
                newWeapon.WeaponType = wpnType;
                newWeapon.HitModifier = int.Parse(txtBxWeaponHitMod.Text);
                newWeapon.DamageModifier = int.Parse(txtBxWeaponDamageMod.Text);
                newWeapon.LongDescription = rtxtBxWeaponLongDesc.Lines.ConvertToString();
                if (lstBxWeaponSkills.Items.Count > 0)
                {
                    foreach(string skill in lstBxWeaponSkills.Items)
                    {
                        newWeapon.RequiredSkills.Add(skill);
                    }
                }
                if (lstBxWeaponBuffs.Items.Count > 0)
                {
                    foreach(string buff in lstBxWeaponBuffs.Items)
                    {
                        newWeapon.AppliedBuffs.Add(buff);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error constructing Weapon object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newWeapon = null;
            }
            return newWeapon;
        }
        #endregion
    }
}