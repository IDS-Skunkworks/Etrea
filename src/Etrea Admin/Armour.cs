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
        private List<Armour> allArmour = new List<Armour>();
        private static ListViewItemComparer armourComparer;

        #region Event Handlers
        private void listViewArmour_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewArmour.SelectedItems.Count == 0)
            {
                return;
            }
            var armour = allArmour.FirstOrDefault(x => x.ID == int.Parse(listViewArmour.SelectedItems[0].SubItems[0].Text));
            ClearArmourForm();
            txtBxArmourID.Text = armour.ID.ToString();
            txtBxArmourName.Text = armour.Name;
            txtBxArmourValue.Text = armour.BaseValue.ToString();
            txtBxArmourShortDesc.Text = armour.ShortDescription;
            chkBxArmourMagical.Checked = armour.IsMagical;
            chkBxArmourCursed.Checked = armour.IsCursed;
            txtBxArmourACMod.Text = armour.ACModifier.ToString();
            txtBxArmourDamReduction.Text = armour.DamageReduction.ToString();
            txtBxArmourWearSlot.Text = armour.Slot.ToString();
            txtBxArmourType.Text = armour.ArmourType.ToString();
            if (armour.AppliedBuffs.Count > 0)
            {
                foreach (string buff in armour.AppliedBuffs)
                {
                    listBxArmourBuffs.Items.Add(buff);
                }
            }
            if (armour.RequiredSkills.Count > 0)
            {
                foreach (string skill in armour.RequiredSkills)
                {
                    listBxArmourSkills.Items.Add(skill);
                }
            }
            rTxtBxArmourLongDesc.Text = armour.LongDescription;
            chkBoxArmourDescLength.Checked = false;
        }

        private void listViewArmour_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (armourComparer == null)
            {
                armourComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == armourComparer.SortColumn)
            {
                armourComparer.SortOrder = armourComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                armourComparer.SortOrder = SortOrder.Ascending;
                armourComparer.SortColumn = e.Column;
            }
            listViewArmour.ListViewItemSorter = armourComparer;
            listViewArmour.Sort();
        }

        private void btnArmourAddSkill_Click(object sender, EventArgs e)
        {
            using (var ss = new SkillSelector())
            {
                if (ss.ShowDialog() == DialogResult.OK)
                {
                    listBxArmourSkills.Items.Add(ss._selectedSkill);
                }
            }
        }

        private void btnArmourRemoveSkill_Click(object sender, EventArgs e)
        {
            if (listBxArmourSkills.SelectedItems.Count > 0)
            {
                var obj = listBxArmourSkills.SelectedItems[0];
                listBxArmourSkills.Items.Remove(obj);
            }
        }

        private void btnArmourClearSkills_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Skills for this Weapon?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBxArmourSkills.Items.Clear();
            }
        }

        private void btnArmourAddBuff_Click(object sender, EventArgs e)
        {
            using (var sb = new BuffSelector())
            {
                if (sb.ShowDialog() == DialogResult.OK)
                {
                    listBxArmourBuffs.Items.Add(sb._selectedBuff);
                }
            }
        }

        private void btnArmourRemoveBuff_Click(object sender, EventArgs e)
        {
            if (listBxArmourBuffs.SelectedItems.Count > 0)
            {
                var obj = listBxArmourBuffs.SelectedItems[0];
                listBxArmourBuffs.Items.Remove(obj);
            }
        }

        private void btnArmourClearBuffs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Buffs for this Armour?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBxArmourBuffs.Items.Clear();
            }
        }

        private void btnLoadArmour_Click(object sender, EventArgs e)
        {
            LoadArmour();
        }

        private async void btnAddArmour_Click(object sender, EventArgs e)
        {
            if (!ValidateArmourData())
            {
                return;
            }
            var newArmour = GetArmourFromFormData();
            if (newArmour == null)
            {
                return;
            }
            var armourJson = Helpers.SerialiseEtreaObject<Armour>(newArmour);
            btnAddArmour.Enabled = false;
            if (await APIHelper.AddNewAsset("/item", armourJson))
            {
                LoadArmour();
            }
            btnAddArmour.Enabled = true;
        }

        private async void btnUpdateArmour_Click(object sender, EventArgs e)
        {
            if (!ValidateArmourData())
            {
                return;
            }
            var newArmour = GetArmourFromFormData();
            if (newArmour == null)
            {
                return;
            }
            var armourJson = Helpers.SerialiseEtreaObject<Armour>(newArmour);
            btnUpdateArmour.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/item", armourJson))
            {
                LoadArmour();
            }
            btnUpdateArmour.Enabled = true;
        }

        private void btnClearArmourForm_Click(object sender, EventArgs e)
        {
            ClearArmourForm();
        }

        private async void btnDeleteArmour_Click(object sender, EventArgs e)
        {
            if (listViewArmour.SelectedItems.Count > 0)
            {
                var armID = int.Parse(listViewArmour.SelectedItems[0].SubItems[0].Text);
                var armour = allArmour.FirstOrDefault(x => x.ID == armID);
                if (armour != null && MessageBox.Show("Delete the selected Armour? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    btnDeleteArmour.Enabled = false;
                    if (await APIHelper.DeleteExistingAsset($"/item/{armour.ID}"))
                    {
                        LoadArmour();
                    }
                    btnDeleteArmour.Enabled = true;
                }
            }
        }

        private void rTxtBxArmourLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rTxtBxArmourLongDesc.GetLineFromCharIndex(rTxtBxArmourLongDesc.SelectionStart);
            string currentLine = rTxtBxArmourLongDesc.Lines.Length > currentLineIndex ? rTxtBxArmourLongDesc.Lines[currentLineIndex] : string.Empty;
            lblArmourDescLength.Text = $"Line Length: {currentLine.Length} characters";
        }
        #endregion

        #region Functions
        private void ClearArmourForm()
        {
            txtBxArmourID.Clear();
            txtBxArmourName.Clear();
            txtBxArmourValue.Clear();
            txtBxArmourShortDesc.Clear();
            chkBxArmourMagical.Checked = false;
            chkBxArmourCursed.Checked = false;
            txtBxArmourACMod.Clear();
            txtBxArmourDamReduction.Clear();
            txtBxArmourWearSlot.Clear();
            txtBxArmourType.Clear();
            listBxArmourBuffs.Items.Clear();
            listBxArmourSkills.Items.Clear();
            rTxtBxArmourLongDesc.Clear();
            chkBoxArmourDescLength.Checked = false;
        }

        private bool ValidateArmourData()
        {
            if (string.IsNullOrEmpty(txtBxArmourID.Text))
            {
                MessageBox.Show("The Armour must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxArmourID.Text, out int armourID) || armourID < 1)
            {
                MessageBox.Show("The ID of the Armour cannot be less than 1.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourName.Text))
            {
                MessageBox.Show("The Armour must have a name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourValue.Text))
            {
                MessageBox.Show("The Armour must have a Value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxArmourValue.Text, out int armourValue) || armourValue < 0)
            {
                MessageBox.Show("The Value of the Armour cannot be less than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourShortDesc.Text))
            {
                MessageBox.Show("The Armour must have a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (chkBxArmourCursed.Checked && !chkBxArmourMagical.Checked)
            {
                MessageBox.Show("If the Armour has the Cursed flag it must also have the Magical flag.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourACMod.Text))
            {
                MessageBox.Show("The Armour must have an Armour Class modifier.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxArmourACMod.Text, out int armourACMod))
            {
                MessageBox.Show("The Armour's Armour Class modifier must be a valid integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourDamReduction.Text))
            {
                MessageBox.Show("The Armour must have a Damage Reduction value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxArmourDamReduction.Text, out int armourDamReduction) || armourDamReduction < 0)
            {
                MessageBox.Show("The Armour Damage Reduction must be a valid integer of zero or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourWearSlot.Text))
            {
                MessageBox.Show("The Armour must have a Wear Slot.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxArmourWearSlot.Text, true, out WearSlot armourWearSlot) || armourWearSlot == WearSlot.None)
            {
                MessageBox.Show("The Armour's Wear Slot is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxArmourType.Text))
            {
                MessageBox.Show("The Armour must have a Type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxArmourType.Text, true, out ArmourType armourType) || armourType == ArmourType.Undefined)
            {
                MessageBox.Show("The Armour Type is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rTxtBxArmourLongDesc.Text))
            {
                MessageBox.Show("The Armour must have a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBoxArmourDescLength.Checked)
            {
                foreach (var ln in rTxtBxArmourLongDesc.Lines)
                {
                    if (ln.Length > 80)
                    {
                        MessageBox.Show("One or more lines in the Long Description are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private Armour GetArmourFromFormData()
        {
            Armour newArmour = new Armour();
            try
            {
                newArmour.ID = int.Parse(txtBxArmourID.Text);
                newArmour.Name = txtBxArmourName.Text;
                newArmour.ShortDescription = txtBxArmourShortDesc.Text;
                newArmour.BaseValue = int.Parse(txtBxArmourValue.Text);
                newArmour.IsMagical = chkBxArmourMagical.Checked;
                newArmour.IsCursed = chkBxArmourCursed.Checked;
                newArmour.ACModifier = int.Parse(txtBxArmourACMod.Text);
                newArmour.DamageReduction = int.Parse(txtBxArmourDamReduction.Text);
                Enum.TryParse(txtBxArmourWearSlot.Text, true, out WearSlot armSlot);
                newArmour.Slot = armSlot;
                Enum.TryParse(txtBxArmourType.Text, true, out ArmourType armType);
                newArmour.ArmourType = armType;
                newArmour.LongDescription = rTxtBxArmourLongDesc.Lines.ConvertToString();
                if (listBxArmourBuffs.Items.Count > 0)
                {
                    foreach (var item in listBxArmourBuffs.Items)
                    {
                        newArmour.AppliedBuffs.Add(item.ToString());
                    }
                }
                if (listBxArmourSkills.Items.Count > 0)
                {
                    foreach(var item in listBxArmourSkills.Items)
                    {
                        newArmour.RequiredSkills.Add(item.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error constructing Armour object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newArmour = null;
            }
            return newArmour;
        }

        private async void LoadArmour()
        {
            allArmour.Clear();
            listViewArmour.Items.Clear();
            ClearArmourForm();
            var result = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (result != null)
            {
                var nextID = result.Max(x => x.ID) + 1;
                foreach(var item in result.OrderBy(x => x.ID))
                {
                    if (item.ItemType != ItemType.Armour)
                    {
                        continue;
                    }
                    Armour armour = (Armour)item;
                    allArmour.Add(armour);
                    listViewArmour.Items.Add(new ListViewItem(new[]
                    {
                        armour.ID.ToString(),
                        armour.Name,
                        armour.BaseValue.ToString(),
                        armour.ShortDescription,
                        armour.ACModifier.ToString(),
                        armour.DamageReduction.ToString(),
                        armour.Slot.ToString(),
                        armour.IsMagical.ToString(),
                        armour.IsCursed.ToString(),
                        armour.ArmourType.ToString(),
                    }));
                }
                foreach (ColumnHeader h in listViewArmour.Columns)
                {
                    h.Width = -2;
                }
                lblArmourNextID.Text = $"Next ID: {nextID}";
            }
        }
        #endregion
    }
}