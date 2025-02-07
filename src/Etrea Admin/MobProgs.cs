using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Newtonsoft.Json;
using Etrea3;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        List<MobProg> allMobProgs = new List<MobProg>();
        private static ListViewItemComparer mobProgListViewComparer;

        #region Event Handlers
        private void listViewMobProgs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMobProgs.SelectedItems.Count == 0)
            {
                return;
            }
            var mp = allMobProgs.FirstOrDefault(x => x.ID == Convert.ToInt32(listViewMobProgs.SelectedItems[0].SubItems[0].Text));
            if (mp == null)
            {
                return;
            }
            txtBxMobProgID.Text = mp.ID.ToString();
            txtBxMobProgName.Text = mp.Name.ToString();
            txtBxMobProgDescription.Text = mp.Description.ToString();
            txtBxMobProgTrigger.Text = mp.Triggers.ToString();
            rTxtBxMobProgScript.Text = mp.Script;
        }

        private void listViewMobProgs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (mobProgListViewComparer == null)
            {
                mobProgListViewComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == mobProgListViewComparer.SortColumn)
            {
                mobProgListViewComparer.SortOrder = mobProgListViewComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                mobProgListViewComparer.SortOrder = SortOrder.Ascending;
                mobProgListViewComparer.SortColumn = e.Column;
            }
            listViewMobProgs.ListViewItemSorter = mobProgListViewComparer;
            listViewMobProgs.Sort();
        }

        private void btnMobProgLoad_Click(object sender, EventArgs e)
        {
            GetMobProgs();
        }

        private async void btnMobProgAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateMobProgData())
            {
                return;
            }
            MobProg mp = GetMobProgFromFormData();
            if (mp == null)
            {
                return;
            }
            btnMobProgAdd.Enabled = false;
            var mobProgJson = JsonConvert.SerializeObject(mp);
            if (await APIHelper.AddNewAsset("/mobprog", mobProgJson))
            {
                GetMobProgs();
            }
            btnMobProgAdd.Enabled = true;
        }

        private async void btnMobProgUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateMobProgData())
            {
                return;
            }
            MobProg mp = GetMobProgFromFormData();
            if (mp == null)
            {
                return;
            }
            btnMobProgUpdate.Enabled = false;
            var mobProgJson = JsonConvert.SerializeObject(mp);
            if (await APIHelper.UpdateExistingAsset("/mobprog", mobProgJson))
            {
                GetMobProgs();
            }
            btnMobProgUpdate.Enabled = true;
        }

        private void btnMobProgClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the form fields?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearMobProgForm();
            }
        }

        private async void btnMobProgDelete_Click(object sender, EventArgs e)
        {
            if (listViewMobProgs.SelectedItems.Count == 0)
            {
                return;
            }
            var mp = allMobProgs.FirstOrDefault(x => x.ID == Convert.ToInt32(listViewMobProgs.SelectedItems[0].SubItems[0].Text));
            if (mp == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected MobProg? This action cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnMobProgDelete.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/mobprog/{mp.ID}"))
                {
                    GetMobProgs();
                }
                btnMobProgDelete.Enabled = true;
            }
        }
        #endregion

        #region Functions
        private async void GetMobProgs()
        {
            btnMobProgLoad.Enabled = false;
            ClearMobProgForm();
            listViewMobProgs.Items.Clear();
            allMobProgs.Clear();
            var result = await APIHelper.LoadAssets<List<MobProg>>("/mobprog", false);
            if (result != null)
            {
                foreach (var mp in result.OrderBy(x => x.ID))
                {
                    allMobProgs.Add(mp);
                    listViewMobProgs.Items.Add(new ListViewItem(new[]
                    {
                        mp.ID.ToString(),
                        mp.Name,
                        mp.Description,
                        mp.Triggers.ToString(),
                    }));
                }
                foreach(ColumnHeader h in listViewMobProgs.Columns)
                {
                    h.Width = -2;
                }
            }
            btnMobProgLoad.Enabled = true;
        }

        private MobProg GetMobProgFromFormData()
        {
            MobProg mp = new MobProg();
            try
            {
                mp.ID = Convert.ToInt32(txtBxMobProgID.Text);
                mp.Name = txtBxMobProgName.Text;
                mp.Description = txtBxMobProgDescription.Text;
                mp.Script = rTxtBxMobProgScript.Text;
                Enum.TryParse(txtBxMobProgTrigger.Text, true, out MobProgTrigger trigger);
                mp.Triggers = trigger;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating MobProg object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mp = null;
            }
            return mp;
        }

        private bool ValidateMobProgData()
        {
            if (string.IsNullOrEmpty(txtBxMobProgID.Text))
            {
                MessageBox.Show("The MobProg must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxMobProgID.Text, out int mpID) || mpID < 0)
            {
                MessageBox.Show("The MobProg ID must be a valid integer with a value greater than 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMobProgName.Text))
            {
                MessageBox.Show("The MobProg must have a name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMobProgDescription.Text))
            {
                MessageBox.Show("The MobProg must have a description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rTxtBxMobProgScript.Text))
            {
                MessageBox.Show("The MobProg must have a Script.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMobProgTrigger.Text))
            {
                MessageBox.Show("The MobProg must have a Trigger.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxMobProgTrigger.Text, true, out MobProgTrigger trigger) || trigger == MobProgTrigger.None)
            {
                MessageBox.Show("The MobProg must have a valid Trigger.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void ClearMobProgForm()
        {
            txtBxMobProgID.Clear();
            txtBxMobProgName.Clear();
            txtBxMobProgDescription.Clear();
            txtBxMobProgTrigger.Clear();
            rTxtBxMobProgScript.Clear();
        }
        #endregion
    }
}