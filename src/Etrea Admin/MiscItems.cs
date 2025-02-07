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
        private List<InventoryItem> allMiscItems = new List<InventoryItem>();
        private static ListViewItemComparer miscItemComparer;

        #region Event Handlers
        private void rtxtBxMiscItemLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxMiscItemLongDesc.GetLineFromCharIndex(rtxtBxMiscItemLongDesc.SelectionStart);
            string currentLine = rtxtBxMiscItemLongDesc.Lines.Length > currentLineIndex ? rtxtBxMiscItemLongDesc.Lines[currentLineIndex] : string.Empty;
            lblMiscItemLineLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private void btnLoadMiscItems_Click(object sender, EventArgs e)
        {
            GetMiscItems();
        }

        private async void btnDeleteMiscItem_Click(object sender, EventArgs e)
        {
            if (listViewMiscItems.SelectedItems.Count == 0)
            {
                return;
            }
            var item = allMiscItems.FirstOrDefault(x => x.ID == int.Parse(listViewMiscItems.SelectedItems[0].SubItems[0].Text));
            if (item == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Item? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteMiscItem.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/item/{item.ID}"))
                {
                    GetMiscItems();
                }
                btnDeleteMiscItem.Enabled = true;
            }
        }

        private async void btnAddMiscItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMiscItem())
            {
                return;
            }
            var newItem = GetMiscItemFromFormData();
            if (newItem == null)
            {
                return;
            }
            btnAddMiscItem.Enabled = false;
            var itemJson = Helpers.SerialiseEtreaObject<InventoryItem>(newItem);
            if (await APIHelper.AddNewAsset($"/item", itemJson))
            {
                GetMiscItems();
            }
            btnAddMiscItem.Enabled = true;
        }

        private async void btnUpdateMiscItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMiscItem())
            {
                return;
            }
            var newItem = GetMiscItemFromFormData();
            if (newItem == null)
            {
                return;
            }
            btnUpdateMiscItem.Enabled = false;
            var itemJson = Helpers.SerialiseEtreaObject<InventoryItem>(newItem);
            if (await APIHelper.UpdateExistingAsset("/item", itemJson))
            {
                GetMiscItems();
            }
            btnUpdateMiscItem.Enabled = true;
        }

        private void btnClearMiscItemForm_Click(object sender, EventArgs e)
        {
            ClearMiscItemForm();
        }

        private void listViewMiscItems_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (miscItemComparer == null)
            {
                miscItemComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == miscItemComparer.SortColumn)
            {
                miscItemComparer.SortOrder = miscItemComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                miscItemComparer.SortOrder = SortOrder.Ascending;
                miscItemComparer.SortColumn = e.Column;
            }
            listViewMiscItems.ListViewItemSorter = miscItemComparer;
            listViewMiscItems.Sort();
        }

        private void listViewMiscItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMiscItems.SelectedIndices.Count > 0)
            {
                var selectedItem = allMiscItems.FirstOrDefault(x => x.ID == int.Parse(listViewMiscItems.SelectedItems[0].SubItems[0].Text));
                if (selectedItem != null)
                {
                    txtBxMiscItemID.Text = selectedItem.ID.ToString();
                    txtBxMiscItemName.Text = selectedItem.Name.ToString();
                    txtBxMiscItemValue.Text = selectedItem.BaseValue.ToString();
                    txtBxMiscItemShortDesc.Text = selectedItem.ShortDescription;
                    rtxtBxMiscItemLongDesc.Text = selectedItem.LongDescription;
                }
            }
        }
        #endregion

        #region Functions
        private async void GetMiscItems()
        {
            allMiscItems.Clear();
            listViewMiscItems.Items.Clear();
            btnLoadMiscItems.Enabled = false;
            int nextID = 1;
            var result = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (result != null)
            {
                nextID = result.Max(x => x.ID) + 1;
                foreach (var item in result.OrderBy(x => x.ID))
                {
                    if (item.ItemType == ItemType.Misc)
                    {
                        allMiscItems.Add(item);
                        listViewMiscItems.Items.Add(new ListViewItem(new[]
                        {
                            item.ID.ToString(),
                            item.Name,
                            item.ShortDescription,
                            item.BaseValue.ToString()
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewMiscItems.Columns)
                {
                    h.Width = -2;
                }
            }
            lblNextMiscItemID.Text = $"Next ID: {nextID}";
            btnLoadMiscItems.Enabled = true;
        }

        private void ClearMiscItemForm()
        {
            txtBxMiscItemID.Clear();
            txtBxMiscItemShortDesc.Clear();
            txtBxMiscItemName.Clear();
            txtBxMiscItemValue.Clear();
            rtxtBxMiscItemLongDesc.Clear();
        }

        private bool ValidateMiscItem()
        {
            if (string.IsNullOrEmpty(txtBxMiscItemID.Text))
            {
                MessageBox.Show("The Item must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxMiscItemID.Text, out int id) || id < 1)
            {
                MessageBox.Show("The ID must be a valid integer with a value greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMiscItemShortDesc.Text))
            {
                MessageBox.Show("The Item must have a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMiscItemName.Text))
            {
                MessageBox.Show("The Item must have a Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMiscItemValue.Text))
            {
                MessageBox.Show("The Item must have a Value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxMiscItemValue.Text, out int val) || val < 0)
            {
                MessageBox.Show("The Value of the Item must be zero or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxMiscItemLongDesc.Text))
            {
                MessageBox.Show("The Item must have a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBoxMiscItemLongDescLength.Checked)
            {
                foreach(var ln in rtxtBxMiscItemLongDesc.Lines)
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

        private InventoryItem GetMiscItemFromFormData()
        {
            InventoryItem newItem = new InventoryItem();
            try
            {
                newItem.ID = int.Parse(txtBxMiscItemID.Text);
                newItem.Name = txtBxMiscItemName.Text;
                newItem.ShortDescription = txtBxMiscItemShortDesc.Text;
                newItem.LongDescription = rtxtBxMiscItemLongDesc.Text;
                newItem.BaseValue = int.Parse(txtBxMiscItemValue.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Misc Item object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newItem = null;
            }
            return newItem;
        }
        #endregion
    }
}