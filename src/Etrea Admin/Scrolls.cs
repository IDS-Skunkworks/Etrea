using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3.Core;
using System.Text;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Scroll> allScrolls = new List<Scroll>();
        private static ListViewItemComparer scrollComparer;

        #region Event Handlers
        private void listViewScrolls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewScrolls.SelectedItems.Count == 0)
            {
                return;
            }
            var s = allScrolls.FirstOrDefault(x => x.ID == int.Parse(listViewScrolls.SelectedItems[0].SubItems[0].Text));
            if (s == null)
            {
                return;
            }
            ClearScrollForm();
            txtBxScrollID.Text = s.ID.ToString();
            txtBxScrollName.Text = s.Name;
            txtBxScrollShortDesc.Text = s.ShortDescription;
            txtBxScrollSpell.Text = s.CastsSpell;
            txtBxScrollValue.Text = s.BaseValue.ToString();
            rtxtBxScrollLongDesc.Text = s.LongDescription;
        }

        private void listViewScrolls_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (scrollComparer == null)
            {
                scrollComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == scrollComparer.SortColumn)
            {
                scrollComparer.SortOrder = scrollComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                scrollComparer.SortOrder = SortOrder.Ascending;
                scrollComparer.SortColumn = e.Column;
            }
            listViewScrolls.ListViewItemSorter = scrollComparer;
            listViewScrolls.Sort();
        }

        private void btnSetScrollSpell_Click(object sender, EventArgs e)
        {
            using (var ss = new SelectSpell())
            {
                if (ss.ShowDialog() == DialogResult.OK)
                {
                    txtBxScrollSpell.Text = ss._spellName;
                }
            }
        }

        private void btnLoadScrolls_Click(object sender, EventArgs e)
        {
            LoadScrolls();
        }

        private async void btnAddScroll_Click(object sender, EventArgs e)
        {
            if (!ValidateScrollData())
            {
                return;
            }
            var newScroll = GetScrollFromFormData();
            if (newScroll == null)
            {
                return;
            }
            btnAddScroll.Enabled = false;
            var scrollJson = Helpers.SerialiseEtreaObject<Scroll>(newScroll);
            if (await APIHelper.AddNewAsset("/item", scrollJson))
            {
                LoadScrolls();
            }
            btnAddScroll.Enabled = true;
        }

        private async void btnUpdateScroll_Click(object sender, EventArgs e)
        {
            if (!ValidateScrollData())
            {
                return;
            }
            var newwScroll = GetScrollFromFormData();
            if (newwScroll == null)
            {
                return;
            }
            btnUpdateScroll.Enabled = false;
            var scrollJson = Helpers.SerialiseEtreaObject<Scroll>(newwScroll);
            if (await APIHelper.UpdateExistingAsset("/item", scrollJson))
            {
                LoadScrolls();
            }
            btnUpdateScroll.Enabled = true;
        }

        private void btnClearScrollForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearScrollForm();
            }
        }

        private async void btnDeleteScroll_Click(object sender, EventArgs e)
        {
            if (listViewScrolls.SelectedItems.Count == 0)
            {
                return;
            }
            var id = listViewScrolls.SelectedItems[0].SubItems[0].Text;
            if (MessageBox.Show("Delete the selected Scroll? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteScroll.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/item/{id}"))
                {
                    LoadScrolls();
                }
                btnDeleteScroll.Enabled = true;
            }
        }

        private void rtxtBxScrollLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxScrollLongDesc.GetLineFromCharIndex(rtxtBxScrollLongDesc.SelectionStart);
            string currentLine = rtxtBxScrollLongDesc.Lines.Length > currentLineIndex ? rtxtBxScrollLongDesc.Lines[currentLineIndex] : string.Empty;
            lblScrollLongDescLength.Text = $"Line Length: {currentLine.Length} characters";
        }
        #endregion

        #region Functions
        private async void LoadScrolls()
        {
            ClearScrollForm();
            allScrolls.Clear();
            listViewScrolls.Items.Clear();
            var result = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (result != null)
            {
                int nextID = result.Max(x => x.ID) + 1;
                foreach(var item in result.OrderBy(x => x.ID))
                {
                    if (item.ItemType == Etrea3.ItemType.Scroll)
                    {
                        Scroll s = (Scroll)item;
                        allScrolls.Add(s);
                        listViewScrolls.Items.Add(new ListViewItem(new[]
                        {
                            s.ID.ToString(),
                            s.Name,
                            s.ShortDescription,
                            s.CastsSpell,
                            s.BaseValue.ToString()
                        }));
                    }
                }
                lblScrollNextID.Text = $"Next ID: {nextID}";
                foreach(ColumnHeader h in listViewScrolls.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private void ClearScrollForm()
        {
            txtBxScrollID.Clear();
            txtBxScrollName.Clear();
            txtBxScrollShortDesc.Clear();
            txtBxScrollSpell.Clear();
            txtBxScrollValue.Clear();
            rtxtBxScrollLongDesc.Clear();
        }

        private bool ValidateScrollData()
        {
            if (string.IsNullOrEmpty(txtBxScrollID.Text))
            {
                MessageBox.Show("The Scroll must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxScrollID.Text, out int scrollID) || scrollID < 1)
            {
                MessageBox.Show("The Scroll ID must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxScrollName.Text))
            {
                MessageBox.Show("The Scroll must have a Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxScrollSpell.Text))
            {
                MessageBox.Show("You must set a Spell for the Scroll to cast.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxScrollShortDesc.Text))
            {
                MessageBox.Show("You must set a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxScrollLongDesc.Text))
            {
                MessageBox.Show("You must set a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxScrollValue.Text))
            {
                MessageBox.Show("You must set a Value for the Scroll.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxScrollValue.Text, out int scrollVal) || scrollVal < 0)
            {
                MessageBox.Show("The Value must be an integer of zero or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private Scroll GetScrollFromFormData()
        {
            Scroll newScroll = new Scroll();
            try
            {
                newScroll.ID = int.Parse(txtBxScrollID.Text);
                newScroll.Name = txtBxScrollName.Text;
                newScroll.ShortDescription = txtBxScrollShortDesc.Text;
                newScroll.LongDescription = rtxtBxScrollLongDesc.Lines.ConvertToString();
                newScroll.CastsSpell = txtBxScrollSpell.Text;
                newScroll.BaseValue = int.Parse(txtBxScrollValue.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Scroll object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newScroll = null;
            }
            return newScroll;
        }
        #endregion
    }
}