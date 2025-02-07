using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Etrea3.Objects;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class SkillSelector : Form
    {
        public string _selectedSkill;

        public SkillSelector()
        {
            InitializeComponent();
        }

        private async void SkillSelector_Load(object sender, EventArgs e)
        {
            var skills = await APIHelper.LoadAssets<List<Skill>>("/skill", false);
            if (skills != null)
            {
                foreach (var skill in skills.OrderBy(x => x.Name))
                {
                    listBoxSkills.Items.Add(skill.Name); 
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBoxSkills.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select an item to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            _selectedSkill = listBoxSkills.SelectedItem.ToString();
        }
    }
}
