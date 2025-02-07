using Etrea3;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class SelectInventoryItem : Form
    {
        private ItemType _itemType;
        private List<InventoryItem> allItems = new List<InventoryItem>();
        private bool _showAmountField = false;
        public InventoryItem _item;
        public int _amount;

        public SelectInventoryItem(ItemType type, bool showAmmount)
        {
            InitializeComponent();
            _itemType = type;
            _showAmountField = showAmmount;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select an item from the list to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            if (_showAmountField && (!int.TryParse(txtBxAmount.Text, out _amount) || _amount < -1 || _amount == 0))
            {
                MessageBox.Show("The amount must be -1 or a non-zero positive integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            var selectedObject = listViewItems.SelectedItems[0];
            if (selectedObject.SubItems[0].Text == "0")
            {
                _item = null;
            }
            else
            {
                var itemNo = int.Parse(selectedObject.SubItems[0].Text);
                var selecteItem = allItems.FirstOrDefault(x => x.ID == itemNo);
                switch (selecteItem.ItemType)
                {
                    case ItemType.Weapon:
                        _item = (Weapon)selecteItem;
                        break;

                    case ItemType.Armour:
                        _item = (Armour)selecteItem;
                        break;

                    case ItemType.Ring:
                        _item = (Ring)selecteItem;
                        break;

                    case ItemType.Consumable:
                        _item = (Consumable)selecteItem;
                        break;

                    case ItemType.Scroll:
                        _item = (Scroll)selecteItem;
                        break;

                    default:
                        _item = selecteItem;
                        break;
                }
            }
        }

        private async void SelectInventoryItem_Load(object sender, EventArgs e)
        {
            var items = await APIHelper.LoadAssets<List<InventoryItem>>("/item", false);
            if (items == null)
            {
                return;
            }
            allItems.Clear();
            listViewItems.Items.Add(new ListViewItem(new[] { "0", "Clear Selection" }));
            foreach (var item in items.OrderBy(x => x.ID))
            {
                if (_itemType == ItemType.Misc || _itemType == item.ItemType)
                {
                    allItems.Add(item);
                    listViewItems.Items.Add(new ListViewItem(new[]
                    {
                        item.ID.ToString(),
                        item.Name,
                        item.ItemType.ToString()
                    }));
                }
            }
            foreach(ColumnHeader h in listViewItems.Columns)
            {
                h.Width = -2;
            }
            lblAmount.Visible = _showAmountField;
            txtBxAmount.Visible = _showAmountField;
        }
    }
}
