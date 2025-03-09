using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3;
using Etrea3.Core;
using System.Text;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Ring> allRings = new List<Ring>();
        private static ListViewItemComparer ringComparer;

        #region Event Handlers
        private void listViewRings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewRings.SelectedItems.Count > 0)
            {
                var ringID = int.Parse(listViewRings.SelectedItems[0].SubItems[0].Text);
                var ring = allRings.FirstOrDefault(x => x.ID == ringID);
                if (ring != null)
                {
                    txtBxRingID.Text = ring.ID.ToString();
                    txtBxRingName.Text = ring.Name;
                    txtBxRingValue.Text = ring.BaseValue.ToString();
                    txtBxRingShortDesc.Text = ring.ShortDescription;
                    chkBxRingCursed.Checked = ring.IsCursed;
                    chkBxRingMagical.Checked = ring.IsMagical;
                    txtBxRingDamMod.Text = ring.DamageModifier.ToString();
                    txtBxRingHitMod.Text = ring.HitModifier.ToString();
                    txtBxRingDamReduction.Text = ring.DamageReduction.ToString();
                    txtBxRingACMod.Text = ring.ACModifier.ToString();
                    if (ring.AppliedBuffs.Count > 0)
                    {
                        foreach (var b in ring.AppliedBuffs)
                        {
                            listBxRingBuffs.Items.Add(b);
                        }
                    }
                    rtxtBxRingLongDesc.Text = ring.LongDescription;
                }
            }
        }

        private void listViewRings_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (ringComparer == null)
            {
                ringComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == ringComparer.SortColumn)
            {
                ringComparer.SortOrder = ringComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                ringComparer.SortOrder = SortOrder.Ascending;
                ringComparer.SortColumn = e.Column;
            }
            listViewRings.ListViewItemSorter = ringComparer;
            listViewRings.Sort();
        }

        private void rtxtBxRingLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxRingLongDesc.GetLineFromCharIndex(rtxtBxRingLongDesc.SelectionStart);
            string currentLine = rtxtBxRingLongDesc.Lines.Length > currentLineIndex ? rtxtBxRingLongDesc.Lines[currentLineIndex] : string.Empty;
            lblRingDescLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private void btnAddRingBuff_Click(object sender, EventArgs e)
        {
            using (var bs = new BuffSelector())
            {
                if (bs.ShowDialog() == DialogResult.OK)
                {
                    listBxRingBuffs.Items.Add(bs._selectedBuff);
                }
            }
        }

        private void btnRemoveRingBuff_Click(object sender, EventArgs e)
        {
            if (listBxRingBuffs.SelectedItems.Count > 0)
            {
                var obj = listBxRingBuffs.SelectedItem;
                listBxRingBuffs.Items.Remove(obj);
            }
        }

        private void btnClearRingBuffs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Buffs for this Ring?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBxRingBuffs.Items.Clear();
            }
        }

        private void btnLoadRings_Click(object sender, EventArgs e)
        {
            LoadRings();
        }

        private async void btnAddRing_Click(object sender, EventArgs e)
        {
            if (!ValidateRingData())
            {
                return;
            }
            var newRing = GetRingFromFormData();
            if (newRing == null)
            {
                return;
            }
            var ringJSON = Helpers.SerialiseEtreaObject<Ring>(newRing);
            btnAddRing.Enabled = false;
            if (await APIHelper.AddNewAsset("/item", ringJSON))
            {
                LoadRings();
            }
            btnAddRing.Enabled = true;
        }

        private async void btnUpdateRing_Click(object sender, EventArgs e)
        {
            if (!ValidateRingData())
            {
                return;
            }
            var newRing = GetRingFromFormData();
            if (newRing == null)
            {
                return;
            }
            var ringJSON = Helpers.SerialiseEtreaObject<Ring>(newRing);
            btnUpdateRing.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/item", ringJSON))
            {
                LoadRings();
            }
            btnUpdateRing.Enabled = true;
        }

        private void btnClearRingForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearRingForm();
            }
        }

        private async void btnDeleteRing_Click(object sender, EventArgs e)
        {
            if (listViewRings.SelectedItems.Count == 0)
            {
                return;
            }
            var ringID = int.Parse(listViewRings.SelectedItems[0].SubItems[0].Text);
            var ring = allRings.FirstOrDefault(x => x.ID == ringID);
            if (ring == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Ring? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (await APIHelper.DeleteExistingAsset($"/item/{ring.ID}"))
                {
                    LoadRings();
                }
            }
        }
        #endregion

        #region Functions
        private async void LoadRings()
        {
            listViewRings.Items.Clear();
            allRings.Clear();
            ClearRingForm();
            btnLoadRings.Enabled = false;
            var result = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (result != null)
            {
                int nextID = result.Max(x => x.ID) + 1;
                foreach(InventoryItem item in result)
                {
                    if (item.ItemType == ItemType.Ring)
                    {
                        Ring r = (Ring)item;
                        allRings.Add(r);
                        listViewRings.Items.Add(new ListViewItem(new[]
                        {
                            r.ID.ToString(),
                            r.Name,
                            r.BaseValue.ToString(),
                            r.IsMagical.ToString(),
                            r.IsCursed.ToString(),
                            r.DamageModifier.ToString(),
                            r.HitModifier.ToString(),
                            r.DamageReduction.ToString(),
                            r.ACModifier.ToString(),
                            r.ShortDescription
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewRings.Columns)
                {
                    h.Width = -2;
                }
                lblRingNextID.Text = $"Next ID: {nextID}";
            }
            btnLoadRings.Enabled = true;
        }

        private void ClearRingForm()
        {
            txtBxRingID.Clear();
            txtBxRingName.Clear();
            txtBxRingValue.Clear();
            txtBxRingShortDesc.Clear();
            chkBxRingCursed.Checked = false;
            chkBxRingMagical.Checked = false;
            txtBxRingDamMod.Clear();
            txtBxRingHitMod.Clear();
            txtBxRingDamReduction.Clear();
            txtBxRingACMod.Clear();
            listBxRingBuffs.Items.Clear();
            rtxtBxRingLongDesc.Clear();
        }

        private bool ValidateRingData()
        {
            if (string.IsNullOrEmpty(txtBxRingID.Text))
            {
                MessageBox.Show("The Ring must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxRingID.Text, out int ringID) || ringID < 1)
            {
                MessageBox.Show("The ID for the Ring must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRingName.Text))
            {
                MessageBox.Show("The Ring must have a Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRingValue.Text))
            {
                MessageBox.Show("The Ring must have a Value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxRingValue.Text, out int ringVal) || ringVal < 0)
            {
                MessageBox.Show("The Value must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRingShortDesc.Text))
            {
                MessageBox.Show("The Ring must have a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxRingLongDesc.Text))
            {
                MessageBox.Show("The Ring must have a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBxRingLongDescLength.Checked)
            {
                foreach(var ln in rtxtBxRingLongDesc.Lines)
                {
                    if (ln.Length > 80)
                    {
                        MessageBox.Show("One or more lines in the Long Description are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            if (chkBxRingCursed.Checked && !chkBxRingMagical.Checked)
            {
                MessageBox.Show("If the Cursed flag is applied to the Ring, the Magical flag must also be set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRingHitMod.Text))
            {
                txtBxRingHitMod.Text = "0";
            }
            if (string.IsNullOrEmpty(txtBxRingACMod.Text))
            {
                txtBxRingACMod.Text = "0";
            }
            if (string.IsNullOrEmpty(txtBxRingDamMod.Text))
            {
                txtBxRingDamMod.Text = "0";
            }
            if (string.IsNullOrEmpty(txtBxRingDamReduction.Text))
            {
                txtBxRingDamReduction.Text = "0";
            }
            if (!int.TryParse(txtBxRingHitMod.Text, out int ringHitMod))
            {
                MessageBox.Show("The Hit Modifier must be a valid integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxRingACMod.Text, out int ringACMod))
            {
                MessageBox.Show("The AC Modifier must be a valid integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxRingDamMod.Text, out int ringDamMod))
            {
                MessageBox.Show("The Damage Modifier must be a valid integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxRingDamReduction.Text, out int ringDamRed) || ringDamMod < 0)
            {
                MessageBox.Show("The Damage Reduction modifier must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private Ring GetRingFromFormData()
        {
            Ring newRing = new Ring();
            try
            {
                newRing.ID = int.Parse(txtBxRingID.Text);
                newRing.Name = txtBxRingName.Text;
                newRing.ShortDescription = txtBxRingShortDesc.Text;
                newRing.BaseValue = int.Parse(txtBxRingValue.Text);
                newRing.IsMagical = chkBxRingMagical.Checked;
                newRing.IsCursed = chkBxRingCursed.Checked;
                newRing.DamageReduction = int.Parse(txtBxRingDamReduction.Text);
                newRing.ACModifier = int.Parse(txtBxRingACMod.Text);
                newRing.HitModifier = int.Parse(txtBxRingHitMod.Text);
                newRing.DamageModifier = int.Parse(txtBxRingDamMod.Text);
                newRing.LongDescription = rtxtBxRingLongDesc.Lines.ConvertToString();
                if (listBxRingBuffs.Items.Count > 0)
                {
                    foreach (var item in listBxRingBuffs.Items)
                    {
                        newRing.AppliedBuffs.Add(item.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Ring object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newRing = null;
            }
            return newRing;
        }
        #endregion
    }
}