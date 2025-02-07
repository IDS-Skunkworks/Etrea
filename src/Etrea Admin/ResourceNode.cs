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
        private List<ResourceNode> allNodes = new List<ResourceNode>();
        private static ListViewItemComparer nodeComparer;

        #region Event Handlers
        private void btnLoadNodes_Click(object sender, EventArgs e)
        {
            LoadNodes();
        }

        private async void btnAddNode_Click(object sender, EventArgs e)
        {
            if (!ValidateNodeForm())
            {
                return;
            }
            var node = GetNodeFromFormData();
            if (node == null)
            {
                return;
            }
            var nodeJson = Helpers.SerialiseEtreaObject<ResourceNode>(node);
            btnAddNode.Enabled = false;
            if (await APIHelper.AddNewAsset("/node", nodeJson))
            {
                ClearNodeForm();
                LoadNodes();
            }
            btnAddNode.Enabled = true;
        }

        private async void btnUpdateNode_Click(object sender, EventArgs e)
        {
            if (!ValidateNodeForm())
            {
                return;
            }
            var node = GetNodeFromFormData();
            if (node == null)
            {
                return;
            }
            var nodeJson = Helpers.SerialiseEtreaObject<ResourceNode>(node);
            btnUpdateNode.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/node", nodeJson))
            {
                ClearNodeForm();
                LoadNodes();
            }
            btnUpdateNode.Enabled = true;
        }

        private void btnClearNodeForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearNodeForm();
            }
        }

        private async void btnDeleteNode_Click(object sender, EventArgs e)
        {
            if (listViewResourceNodes.SelectedItems.Count == 0)
            {
                return;
            }
            var node = allNodes.FirstOrDefault(x => x.ID == int.Parse(listViewResourceNodes.SelectedItems[0].SubItems[0].Text));
            if (node == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Node? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteNode.Enabled = false;
                if(await APIHelper.DeleteExistingAsset($"/node/{node.ID}"))
                {
                    ClearNodeForm();
                    LoadNodes();
                }
                btnDeleteNode.Enabled = true;
            }
        }

        private void btnAddNodeItem_Click(object sender, EventArgs e)
        {
            using (var si = new SelectInventoryItem(ItemType.Misc, false))
            {
                if (si.ShowDialog() == DialogResult.OK)
                {
                    listBxNodeItems.Items.Add($"{si._item.ID}: {si._item.Name}");
                }
            }
        }

        private void btnRemoveNodeItem_Click(object sender, EventArgs e)
        {
            if (listBxNodeItems.SelectedItems.Count == 0)
            {
                return;
            }
            listBxNodeItems.Items.RemoveAt(listBxNodeItems.SelectedIndex);
        }

        private void btnClearNodeItems_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear findable items for this Node?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBxNodeItems.Items.Clear();
            }
        }

        private async void listViewResourceNodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewResourceNodes.SelectedItems.Count == 0)
            {
                return;
            }
            var node = allNodes.FirstOrDefault(x => x.ID == int.Parse(listViewResourceNodes.SelectedItems[0].SubItems[0].Text));
            if (node == null)
            {
                return;
            }
            txtBxNodeID.Text = node.ID.ToString();
            txtBxNodeName.Text = node.Name;
            txtBxNodeAppearChance.Text = node.ApperanceChance.ToString();
            listBxNodeItems.Items.Clear();
            foreach (var item in node.CanFind)
            {
                var cf = await APIHelper.LoadAssets<InventoryItem>($"/item/{item.Key}", true);
                if (cf != null)
                {
                    listBxNodeItems.Items.Add($"{cf.ID}: {cf.Name}");
                }
            }
        }

        private void listViewResourceNodes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (nodeComparer == null)
            {
                nodeComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == nodeComparer.SortColumn)
            {
                nodeComparer.SortOrder = nodeComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                nodeComparer.SortOrder = SortOrder.Ascending;
                nodeComparer.SortColumn = e.Column;
            }
            listViewResourceNodes.ListViewItemSorter = nodeComparer;
            listViewResourceNodes.Sort();
        }
        #endregion

        #region Functions
        private async void LoadNodes()
        {
            allNodes.Clear();
            listViewResourceNodes.Items.Clear();
            var result = await APIHelper.LoadAssets<List<ResourceNode>>("/node", false);
            if (result != null)
            {
                foreach (ResourceNode node in result.OrderBy(x => x.ID))
                {
                    allNodes.Add(node);
                    listViewResourceNodes.Items.Add(new ListViewItem(new[]
                    {
                        node.ID.ToString(),
                        node.Name,
                        node.ApperanceChance.ToString(),
                        node.CanFind.Count().ToString(),
                    }));
                }
                foreach(ColumnHeader h in listViewResourceNodes.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private void ClearNodeForm()
        {
            txtBxNodeID.Clear();
            txtBxNodeName.Clear();
            txtBxNodeAppearChance.Clear();
            listBxNodeItems.Items.Clear();
        }

        private ResourceNode GetNodeFromFormData()
        {
            ResourceNode newNode = new ResourceNode();
            try
            {
                newNode.Name = txtBxNodeName.Text;
                newNode.ID = int.Parse(txtBxNodeID.Text);
                newNode.ApperanceChance = int.Parse(txtBxNodeAppearChance.Text);
                foreach (var item in listBxNodeItems.Items)
                {
                    newNode.CanFind.TryAdd(int.Parse(item.ToString().Split(':')[0].Trim()), true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Node object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newNode = null;
            }
            return newNode;
        }

        private bool ValidateNodeForm()
        {
            if (!int.TryParse(txtBxNodeID.Text, out int nodeID) || nodeID < 1)
            {
                MessageBox.Show("The Node ID must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNodeName.Text))
            {
                MessageBox.Show("The Node must have name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNodeAppearChance.Text, out int nodeAppearChance) || nodeAppearChance < 1)
            {
                MessageBox.Show("The Appearance Chance must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (listBxNodeItems.Items.Count == 0)
            {
                MessageBox.Show("The Node must provide one or more items when mined.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        #endregion
    }
}