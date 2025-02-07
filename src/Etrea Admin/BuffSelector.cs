using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class BuffSelector : Form
    {
        public string _selectedBuff;

        public BuffSelector()
        {
            InitializeComponent();
        }

        private async void BuffSelector_Load(object sender, EventArgs e)
        {
            var buffs = await APIHelper.LoadAssets<List<Buff>>("/buff", false);
            if (buffs != null)
            {
                foreach (var buff in buffs.OrderBy(x => x.Name))
                {
                    listBoxBuffs.Items.Add(buff.Name);
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBoxBuffs.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select an item to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            _selectedBuff = listBoxBuffs.SelectedItem.ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
