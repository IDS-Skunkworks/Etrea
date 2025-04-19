using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Newtonsoft.Json;
using Etrea3;
using System.Text;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        List<ScriptingObject> allScripts = new List<ScriptingObject>();
        private static ListViewItemComparer mobProgListViewComparer;

        #region Event Handlers
        private void listViewMobProgs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMobProgs.SelectedItems.Count == 0)
            {
                return;
            }
            var mp = allScripts.FirstOrDefault(x => x.ID == Convert.ToInt32(listViewMobProgs.SelectedItems[0].SubItems[0].Text));
            if (mp == null)
            {
                return;
            }
            txtBxMobProgID.Text = mp.ID.ToString();
            txtBxMobProgName.Text = mp.Name.ToString();
            txtBxMobProgDescription.Text = mp.Description.ToString();
            if (mp.GetType() == typeof(MobProg))
            {
                MobProg mobProg = (MobProg)mp;
                txtBxMobProgTrigger.Text = mobProg.Triggers.ToString();
                comboScriptType.Text = "MobProg";
            }
            if (mp.GetType() == typeof(RoomProg))
            {
                RoomProg roomProg = (RoomProg)mp;
                txtBxMobProgTrigger.Text = roomProg.Triggers.ToString();
                comboScriptType.Text = "RoomProg";
            }
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
            var mp = allScripts.FirstOrDefault(x => x.ID == Convert.ToInt32(listViewMobProgs.SelectedItems[0].SubItems[0].Text));
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
            allScripts.Clear();
            var mps = await APIHelper.LoadAssets<List<MobProg>>("/mobprog", false);
            var rps = await APIHelper.LoadAssets<List<RoomProg>>("/roomprog", false);
            if (mps != null)
            {
                allScripts.AddRange(mps);
            }
            if (rps != null)
            {
                allScripts.AddRange(rps);
            }
            allScripts = allScripts.OrderBy(x => x.ID).ToList();
            foreach (var script in allScripts)
            {
                listViewMobProgs.Items.Add(new ListViewItem(new[]
                {
                    script.ID.ToString(),
                    script.Name,
                    script.Description,
                    script.GetType().Name.ToString()
                }));
            }
            foreach (ColumnHeader h in listViewMobProgs.Columns)
            {
                h.Width = -2;
            }
            btnMobProgLoad.Enabled = true;
        }

        private dynamic GetMobProgFromFormData()
        {
            dynamic retval;
            if (comboScriptType.Text == "MobProg")
            {
                retval = new MobProg();
            }
            else
            {
                retval = new RoomProg();
            }
            try
            {
                retval.ID = Convert.ToInt32(txtBxMobProgID.Text);
                retval.Name = txtBxMobProgName.Text;
                retval.Description = txtBxMobProgDescription.Text;
                StringBuilder sb = new StringBuilder();
                foreach(string ln in rTxtBxMobProgScript.Lines)
                {
                    sb.AppendLine(ln);
                }
                retval.Script = sb.ToString();
                if (comboScriptType.Text == "MobProg")
                {
                    Enum.TryParse(txtBxMobProgTrigger.Text, true, out MobProgTrigger trigger);
                    retval.Trigger = trigger;
                }
                else
                {
                    Enum.TryParse(txtBxMobProgTrigger.Text, true, out RoomProgTrigger trigger);
                    retval.Trigger = trigger;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating MobProg object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                retval = null;
            }
            return retval;
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
            comboScriptType.SelectedIndex = -1;
        }
        #endregion
    }
}