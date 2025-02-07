using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class SelectSpell : Form
    {
        public int _spellID;
        public string _spellName;

        public SelectSpell()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void SelectSpell_Load(object sender, EventArgs e)
        {
            var spells = await APIHelper.LoadAssets<List<Spell>>("/spell", false);
            if (spells == null)
            {
                return;
            }
            foreach (var spell in spells.Where(x => !x.IsAOE).OrderBy(x => x.ID))
            {
                listViewItems.Items.Add(new ListViewItem(new[]
                {
                    spell.ID.ToString(),
                    spell.Name,
                    spell.SpellType.ToString(),
                }));
            }
            foreach(ColumnHeader h in listViewItems.Columns)
            {
                h.Width = -2;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select a Spell to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            var obj = listViewItems.SelectedItems[0];
            _spellID = int.Parse(obj.SubItems[0].Text);
            _spellName = obj.SubItems[1].Text;
        }
    }
}
