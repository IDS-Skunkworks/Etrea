using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Newtonsoft.Json;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private static List<Zone> allZones = new List<Zone>();
        private static ListViewItemComparer zoneListViewComparer;

        #region Event Handlers
        private void listViewZones_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (zoneListViewComparer == null)
            {
                zoneListViewComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == zoneListViewComparer.SortColumn)
            {
                zoneListViewComparer.SortOrder = zoneListViewComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                zoneListViewComparer.SortOrder = SortOrder.Ascending;
                zoneListViewComparer.SortColumn = e.Column;
            }
            listViewZones.ListViewItemSorter = zoneListViewComparer;
            listViewZones.Sort();
        }

        private void btZnLoadZones_Click (object sender, EventArgs e)
        {
            GetZones();
        }

        private void listViewZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewZones.SelectedItems.Count > 0)
            {
                int zNumber = Convert.ToInt32(listViewZones.SelectedItems[0].SubItems[0].Text);
                var z = allZones.FirstOrDefault(x => x.ZoneID == zNumber);
                if (z != null)
                {
                    txtBxZnZoneID.Text = z.ZoneID.ToString();
                    txtBxZnZoneName.Text = z.ZoneName.ToString();
                    txtBxZnStartRID.Text = z.MinRoom.ToString();
                    txtBxZnEndRID.Text = z.MaxRoom.ToString();
                }
            }
        }

        private void btZnClearZones_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the form fields?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearZoneForm();
            }
        }

        private async void btZnAddZone_Click(object sender, EventArgs e)
        {
            if (!ValidateZone())
            {
                return;
            }
            Zone z = GetZoneFromFormData();
            if (z == null)
            {
                return;
            }
            btZnAddZone.Enabled = false;
            var zoneJson = JsonConvert.SerializeObject(z);
            if (await APIHelper.AddNewAsset("/zone", zoneJson))
            {
                GetZones();
                ClearZoneForm();
            }
            btZnAddZone.Enabled = true;
        }

        private async void btZnUpdateZone_Click(object sender, EventArgs e)
        {
            if (!ValidateZone())
            {
                return;
            }
            Zone z = GetZoneFromFormData();
            if (z == null)
            {
                return;
            }
            btZnUpdateZone.Enabled = false;
            var zoneJson = JsonConvert.SerializeObject(z);
            if (await APIHelper.UpdateExistingAsset("/zone", zoneJson))
            {
                GetZones();
                ClearZoneForm();
            }
            btZnUpdateZone.Enabled = true;
        }

        private async void btnZnDeleteZone_Click(object sender, EventArgs e)
        {
            if (listViewZones.SelectedItems.Count == 0)
            {
                return;
            }
            btnZnDeleteZone.Enabled = false;
            var zn = allZones.FirstOrDefault(x => x.ZoneID == Convert.ToInt32(listViewZones.SelectedItems[0].SubItems[0].Text));
            if (zn != null && MessageBox.Show("Delete the selected Zone? This change cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (await APIHelper.DeleteExistingAsset($"/zone/{zn.ZoneID}"))
                {
                    GetZones();
                    ClearZoneForm();
                }
            }
            btnZnDeleteZone.Enabled = true;
        }
        #endregion

        #region Functions
        private void ClearZoneForm()
        {
            txtBxZnEndRID.Clear();
            txtBxZnStartRID.Clear();
            txtBxZnZoneID.Clear();
            txtBxZnZoneName.Clear();
        }

        private Zone GetZoneFromFormData()
        {
            Zone zone = new Zone();
            try
            {
                zone.ZoneName = txtBxZnZoneName.Text;
                zone.ZoneID = int.Parse(txtBxZnZoneID.Text);
                zone.MinRoom = int.Parse(txtBxZnStartRID.Text);
                zone.MaxRoom = int.Parse(txtBxZnEndRID.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Zone object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                zone = null;
            }
            return zone;
        }

        private async void GetZones()
        {
            btZnLoadZones.Enabled = false;
            listViewZones.Items.Clear();
            var result = await APIHelper.LoadAssets<List<Zone>>("/zone", false);
            if (result != null)
            {
                allZones.Clear();
                foreach(var zone in result.OrderBy(x => x.ZoneID))
                {
                    allZones.Add(zone);
                    listViewZones.Items.Add(new ListViewItem(new[]
                    {
                        zone.ZoneID.ToString(),
                        zone.ZoneName,
                        zone.MinRoom.ToString(),
                        zone.MaxRoom.ToString(),
                    }));
                }
                foreach (ColumnHeader h in listViewZones.Columns)
                {
                    h.Width = -2;
                }
            }
            btZnLoadZones.Enabled = true;
        }

        private bool ValidateZone()
        {
            if (string.IsNullOrEmpty(txtBxZnZoneID.Text))
            {
                MessageBox.Show("You must provide a Zone ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxZnZoneName.Text))
            {
                MessageBox.Show("You must provide a Zone Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxZnStartRID.Text))
            {
                MessageBox.Show("You must provide a Room ID for the start of the Zone.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxZnEndRID.Text))
            {
                MessageBox.Show("You must provide a Room ID for the end of the Zone.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        #endregion
    }
}