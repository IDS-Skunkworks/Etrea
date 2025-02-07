using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3.Core;
using Etrea3;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Consumable> allConsumables = new List<Consumable>();
        private static ListViewItemComparer consumableComparer;

        #region Event Handlers
        private void listViewConsumables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewConsumables.SelectedItems.Count == 0)
            {
                return;
            }
            var cons = allConsumables.FirstOrDefault(x => x.ID == int.Parse(listViewConsumables.SelectedItems[0].SubItems[0].Text));
            if (cons == null)
            {
                return;
            }
            ClearConsumableForm();
            txtBxConsumableID.Text = cons.ID.ToString();
            txtBxConsumableName.Text = cons.Name;
            txtBxConsumableEffect.Text = cons.Effects.ToString();
            txtBxConsumableShortDesc.Text = cons.ShortDescription;
            txtBxNoOfConsumableEffectDice.Text = cons.NumberOfDamageDice.ToString();
            txtBxConsumableDieSize.Text = cons.SizeofDamageDice.ToString();
            rtxtBxConsumableLongDesc.Text = cons.LongDescription;
            txtBxConsumableValue.Text = cons.BaseValue.ToString();
            if (cons.AppliedBuffs.Count > 0)
            {
                foreach (var buffs in cons.AppliedBuffs)
                {
                    listBoxConsumableBuffs.Items.Add(buffs);
                }
            }
        }

        private void listViewConsumables_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (consumableComparer == null)
            {
                consumableComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == consumableComparer.SortColumn)
            {
                consumableComparer.SortOrder = consumableComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                consumableComparer.SortOrder = SortOrder.Ascending;
                consumableComparer.SortColumn = e.Column;
            }
            listViewConsumables.ListViewItemSorter = consumableComparer;
            listViewConsumables.Sort();
        }

        private void btnAddConsumableBuff_Click(object sender, EventArgs e)
        {
            using (var bs = new BuffSelector())
            {
                if (bs.ShowDialog() == DialogResult.OK)
                {
                    listBoxConsumableBuffs.Items.Add(bs._selectedBuff);
                }    
            }
        }

        private void btnRemoveConsumableBuff_Click(object sender, EventArgs e)
        {
            if (listBoxConsumableBuffs.SelectedItems.Count > 0)
            {
                var obj = listBoxConsumableBuffs.SelectedItems[0];
                listBoxConsumableBuffs.Items.Remove(obj);
            }
        }

        private void btnClearConsumableBuffs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of Buffs for this Consumable?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBoxConsumableBuffs.Items.Clear();
            }
        }

        private void btnLoadConsumables_Click(object sender, EventArgs e)
        {
            LoadConsumables();
        }

        private async void btnAddConsumable_Click(object sender, EventArgs e)
        {
            if (!ValidateConsumableForm())
            {
                return;
            }
            var newConsumable = GetConsumableFromFormData();
            if (newConsumable == null)
            {
                return;
            }
            btnAddConsumable.Enabled = false;
            var consumableJson = Helpers.SerialiseEtreaObject<Consumable>(newConsumable);
            if (await APIHelper.AddNewAsset("/item", consumableJson))
            {
                LoadConsumables();
            }
            btnAddConsumable.Enabled = true;
        }

        private async void btnUpdateConsumable_Click(object sender, EventArgs e)
        {
            if (!ValidateConsumableForm())
            {
                return;
            }
            var newConsumable = GetConsumableFromFormData();
            if (newConsumable == null)
            {
                return;
            }
            var consumableJson = Helpers.SerialiseEtreaObject<Consumable>(newConsumable);
            btnUpdateConsumable.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/item", consumableJson))
            {
                LoadConsumables();
            }
            btnUpdateConsumable.Enabled = true;
        }

        private void btnClearConsumableForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearConsumableForm();
            }
        }

        private async void btnDeleteConsumable_Click(object sender, EventArgs e)
        {
            if (listViewConsumables.SelectedItems.Count == 0)
            {
                return;
            }
            var itemID = listViewConsumables.SelectedItems[0].SubItems[0].Text;
            if (MessageBox.Show("Delete the selected Consumable? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteConsumable.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/item/{itemID}"))
                {
                    LoadConsumables();
                }
                btnDeleteConsumable.Enabled = true;
            }
        }

        private void rtxtBxConsumableLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxConsumableLongDesc.GetLineFromCharIndex(rtxtBxConsumableLongDesc.SelectionStart);
            string currentLine = rtxtBxConsumableLongDesc.Lines.Length > currentLineIndex ? rtxtBxConsumableLongDesc.Lines[currentLineIndex] : string.Empty;
            lblConsumableLongDescLength.Text = $"Line Length: {currentLine.Length} characters";
        }
        #endregion

        #region Functions
        private void ClearConsumableForm()
        {
            rtxtBxConsumableLongDesc.Clear();
            listBoxConsumableBuffs.Items.Clear();
            txtBxConsumableID.Clear();
            txtBxConsumableName.Clear();
            txtBxConsumableEffect.Clear();
            txtBxNoOfConsumableEffectDice.Clear();
            txtBxConsumableDieSize.Clear();
            txtBxConsumableValue.Clear();
            txtBxConsumableShortDesc.Clear();
        }

        private bool ValidateConsumableForm()
        {
            if (string.IsNullOrEmpty(txtBxConsumableID.Text))
            {
                MessageBox.Show("The Consumable needs to have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxConsumableID.Text, out var consumableID) || consumableID < 1)
            {
                MessageBox.Show("The ID must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxConsumableName.Text))
            {
                MessageBox.Show("The Consumable must have a Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxConsumableEffect.Text))
            {
                MessageBox.Show("The Consumable must have a valid Effect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxConsumableEffect.Text, true, out ConsumableEffect effect) || effect == ConsumableEffect.Undefined)
            {
                MessageBox.Show("The Effect is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNoOfConsumableEffectDice.Text))
            {
                txtBxNoOfConsumableEffectDice.Text = "0";
            }
            if (string.IsNullOrEmpty(txtBxConsumableDieSize.Text))
            {
                txtBxConsumableDieSize.Text = "0";
            }
            if (!int.TryParse(txtBxNoOfConsumableEffectDice.Text, out int noOfDice) || noOfDice < 0)
            {
                MessageBox.Show("The number of dice must be zero or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxConsumableDieSize.Text, out int dieSize) || dieSize < 0)
            {
                MessageBox.Show("The size of dice must be zero or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxConsumableValue.Text))
            {
                MessageBox.Show("The Consumable must have a value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxConsumableValue.Text, out int consVal) || consVal < 0)
            {
                MessageBox.Show("The Value must be an integer with a value of zero or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxConsumableShortDesc.Text))
            {
                MessageBox.Show("The Consumable must have a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxConsumableLongDesc.Text))
            {
                MessageBox.Show("The Consumable must have a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBxConsumableLongDescLength.Checked)
            {
                foreach (var ln in rtxtBxConsumableLongDesc.Lines)
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

        private Consumable GetConsumableFromFormData()
        {
            Consumable newConsumable = new Consumable();
            try
            {
                newConsumable.ID = int.Parse(txtBxConsumableID.Text);
                newConsumable.Name = txtBxConsumableName.Text;
                newConsumable.ShortDescription = txtBxConsumableShortDesc.Text;
                newConsumable.BaseValue = int.Parse(txtBxConsumableValue.Text);
                newConsumable.NumberOfDamageDice = int.Parse(txtBxNoOfConsumableEffectDice.Text);
                newConsumable.SizeofDamageDice = int.Parse(txtBxConsumableDieSize.Text);
                newConsumable.LongDescription = rtxtBxConsumableLongDesc.Text;
                Enum.TryParse(txtBxConsumableEffect.Text, true, out ConsumableEffect effect);
                newConsumable.Effects = effect;
                if (listBoxConsumableBuffs.Items.Count > 0)
                {
                    foreach(var item in listBoxConsumableBuffs.Items)
                    {
                        newConsumable.AppliedBuffs.Add(item.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Consumable object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newConsumable = null;
            }
            return newConsumable;
        }

        private async void LoadConsumables()
        {
            allConsumables.Clear();
            listViewConsumables.Items.Clear();
            ClearConsumableForm();
            btnLoadConsumables.Enabled = false;
            var result = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (result != null)
            {
                int nextID = result.Max(x => x.ID) + 1;
                foreach (InventoryItem item in result)
                {
                    if (item.ItemType == ItemType.Consumable)
                    {
                        Consumable con = (Consumable)item;
                        allConsumables.Add(con);
                        listViewConsumables.Items.Add(new ListViewItem(new[]
                        {
                            con.ID.ToString(),
                            con.Name,
                            con.Effects.ToString(),
                            $"{con.NumberOfDamageDice}D{con.SizeofDamageDice}",
                            con.BaseValue.ToString(),
                            con.AppliesBuffs.ToString(),
                            con.ShortDescription,
                        }));
                    }
                }
                lblConsumableNextID.Text = $"Next ID: {nextID}";
                foreach(ColumnHeader h in listViewConsumables.Columns)
                {
                    h.Width = -2;
                }
            }
            btnLoadConsumables.Enabled = true;
        }
        #endregion
    }
}