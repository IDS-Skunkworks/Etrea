using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class SelectNPC : Form
    {
        private List<NPC> npcList = new List<NPC>();
        public NPC _npc;

        public SelectNPC()
        {
            InitializeComponent();
        }

        private async void SelectNPC_Load(object sender, EventArgs e)
        {
            npcList.Clear();
            listViewNPCs.Items.Clear();
            var npcs = await APIHelper.LoadAssets<List<NPC>>("/npc", false);
            if (npcs != null)
            {
                foreach (var npc in npcs.OrderBy(x => x.ID))
                {
                    npcList.Add(npc);
                    listViewNPCs.Items.Add(new ListViewItem(new[]
                    {
                        npc.TemplateID.ToString(),
                        npc.ZoneID.ToString(),
                        npc.Name,
                        npc.ShortDescription,
                    }));
                }
                foreach (ColumnHeader h in listViewNPCs.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listViewNPCs.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select an NPC to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            int id = int.Parse(listViewNPCs.SelectedItems[0].SubItems[0].Text);
            _npc = npcList.FirstOrDefault(x => x.TemplateID == id);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
