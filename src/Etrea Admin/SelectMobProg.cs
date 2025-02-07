using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class SelectMobProg : Form
    {
        public int _mobProgID;

        public SelectMobProg()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void SelectMobProg_Load(object sender, EventArgs e)
        {
            var mobProgs = await APIHelper.LoadAssets<List<MobProg>>("/mobprog", false);
            if (mobProgs == null)
            {
                return;
            }
            foreach (var mobProg in mobProgs.OrderBy(x => x.ID))
            {
                listViewItems.Items.Add(new ListViewItem(new[]
                {
                    mobProg.ID.ToString(),
                    mobProg.Name,
                    mobProg.Description,
                    mobProg.Triggers.ToString(),
                }));
            }
            foreach (ColumnHeader h in listViewItems.Columns)
            {
                h.Width = -2;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select a MobProg to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            var obj = listViewItems.SelectedItems[0];
            _mobProgID = int.Parse(obj.SubItems[0].Text);
        }
    }
}
