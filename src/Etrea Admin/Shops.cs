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
        private List<Shop> allShops = new List<Shop>();
        private static ListViewItemComparer shopComparer;

        #region Event Handlers
        private async void listViewShops_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewShops.SelectedItems.Count == 0)
            {
                return;
            }
            var shop = allShops.FirstOrDefault(x => x.ID == int.Parse(listViewShops.SelectedItems[0].SubItems[0].Text));
            if (shop == null)
            {
                return;
            }
            txtBxShopID.Text = shop.ID.ToString();
            txtBxShopName.Text = shop.ShopName;
            txtBxShopGold.Text = shop.BaseGold.ToString();
            listViewShopInventory.Items.Clear();
            if (shop.BaseInventory.Count > 0)
            {
                foreach (var obj in shop.BaseInventory)
                {
                    var item = await APIHelper.LoadAssets<InventoryItem>($"/item/{obj.Key}", true);
                    if (item != null)
                    {
                        listViewShopInventory.Items.Add(new ListViewItem(new[]
                        {
                            item.ID.ToString(),
                            item.Name,
                            item.ItemType.ToString(),
                            item.BaseValue.ToString(),
                            obj.Value.ToString()
                        }));
                    }
                    else
                    {
                        listViewShopInventory.Items.Add(new ListViewItem(new[]
                        {
                            obj.Key.ToString(),
                            "Invalid Item",
                            "Invalid Item",
                            "0",
                            obj.Value.ToString()
                        }));
                    }
                }
                foreach (ColumnHeader h in listViewShopInventory.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private void listViewShops_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (shopComparer == null)
            {
                shopComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == shopComparer.SortColumn)
            {
                shopComparer.SortOrder = shopComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                shopComparer.SortOrder = SortOrder.Ascending;
                shopComparer.SortColumn = e.Column;
            }
            listViewShops.ListViewItemSorter = shopComparer;
            listViewShops.Sort();
        }

        private void btnAddShopItem_Click(object sender, EventArgs e)
        {
            using (var si = new SelectInventoryItem(ItemType.Misc, true))
            {
                if (si.ShowDialog() == DialogResult.OK)
                {
                    bool updateItem = false;
                    int amt = si._amount;
                    var itm = si._item;
                    foreach (ListViewItem invItem in listViewShopInventory.Items)
                    {
                        if (int.Parse(invItem.SubItems[0].Text) == si._item.ID)
                        {
                            invItem.SubItems[4].Text = amt.ToString();
                            updateItem = true;
                            break;
                        }
                    }
                    if (!updateItem)
                    {
                        listViewShopInventory.Items.Add(new ListViewItem(new[]
                        {
                            itm.ID.ToString(),
                            itm.Name,
                            itm.ItemType.ToString(),
                            itm.BaseValue.ToString(),
                            amt.ToString(),
                        }));
                    }
                    foreach (ColumnHeader h in listViewShopInventory.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveShopItem_Click(object sender, EventArgs e)
        {
            if (listViewShopInventory.SelectedItems.Count == 0)
            {
                return;
            }
            var ind = listViewShopInventory.SelectedIndices[0];
            listViewShopInventory.Items.RemoveAt(ind);
        }

        private void btnClearShopItems_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the inventory for this Shop?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewShopInventory.Items.Clear();
            }
        }

        private void btnLoadShops_Click(object sender, EventArgs e)
        {
            LoadShops();
        }

        private async void btnAddShop_Click(object sender, EventArgs e)
        {
            if (!ValidateShopData())
            {
                return;
            }
            var newShop = GetShopFromFormData();
            if (newShop == null)
            {
                return;
            }
            var shopJson = Helpers.SerialiseEtreaObject<Shop>(newShop);
            btnAddShop.Enabled = false;
            if (await APIHelper.AddNewAsset("/shop", shopJson))
            {
                LoadShops();
            }
            btnAddShop.Enabled = true;
        }

        private async void btnUpdateShop_Click(object sender, EventArgs e)
        {
            if (!ValidateShopData())
            {
                return;
            }
            var newShop = GetShopFromFormData();
            if (newShop == null)
            {
                return;
            }
            var shopJson = Helpers.SerialiseEtreaObject<Shop>(newShop);
            btnUpdateShop.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/shop", shopJson))
            {
                LoadShops();
            }
            btnUpdateShop.Enabled = true;
        }

        private void btnClearShopForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearShopForm();
            }
        }

        private async void btnDeleteShop_Click(object sender, EventArgs e)
        {
            if (listViewShops.SelectedItems.Count == 0)
            {
                return;
            }
            var shop = allShops.FirstOrDefault(x => x.ID == int.Parse(listViewShops.SelectedItems[0].SubItems[0].Text));
            if (shop == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Shop? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteShop.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/shop/{shop.ID}"))
                {
                    LoadShops();
                }
                btnDeleteShop.Enabled = true;
            }
        }
        #endregion

        #region Functions
        private async void LoadShops()
        {
            allShops.Clear();
            listViewShops.Items.Clear();
            ClearShopForm();
            var result = await APIHelper.LoadAssets<List<Shop>>("/shop", false);
            if (result != null)
            {
                foreach (var shop in result.OrderBy(x => x.ID))
                {
                    allShops.Add(shop);
                    listViewShops.Items.Add(new ListViewItem(new[]
                    {
                        shop.ID.ToString(),
                        shop.ShopName,
                        shop.BaseGold.ToString(),
                        shop.BaseInventory.Count.ToString(),
                    }));
                }
                foreach (ColumnHeader h in listViewShops.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private bool ValidateShopData()
        {
            if (!int.TryParse(txtBxShopID.Text, out var shopID) || shopID < 1)
            {
                MessageBox.Show("The Shop ID must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxShopName.Text))
            {
                MessageBox.Show("The Shop must have a name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!ulong.TryParse(txtBxShopGold.Text, out ulong gp))
            {
                MessageBox.Show("The Shop Gold is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private Shop GetShopFromFormData()
        {
            var newShop = new Shop();
            try
            {
                newShop.ID = int.Parse(txtBxShopID.Text);
                newShop.ShopName = txtBxShopName.Text;
                newShop.BaseGold = ulong.Parse(txtBxShopGold.Text);
                if (listViewShopInventory.Items.Count > 0)
                {
                    foreach (ListViewItem invItem in listViewShopInventory.Items)
                    {
                        int itemID = int.Parse(invItem.SubItems[0].Text);
                        int itemAmount = int.Parse(invItem.SubItems[4].Text);
                        newShop.BaseInventory.TryAdd(itemID, itemAmount);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Shop object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newShop = null;
            }
            return newShop;
        }

        private void ClearShopForm()
        {
            txtBxShopID.Clear();
            txtBxShopName.Clear();
            txtBxShopGold.Clear();
            listViewShopInventory.Items.Clear();
        }
        #endregion
    }
}