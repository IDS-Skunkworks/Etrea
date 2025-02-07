using Etrea3.Objects;
using System;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class AddRoomExit : Form
    {
        public RoomExit roomExit;

        public AddRoomExit()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxDestRID.Text))
            {
                MessageBox.Show("Destination Room ID cannot be empty and must be a valid integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            if (string.IsNullOrEmpty(txtBxExitDirection.Text))
            {
                MessageBox.Show("Direction cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            if (!int.TryParse(txtBxDestRID.Text, out int rid) || rid < 0)
            {
                MessageBox.Show("Destination Room ID must be a valid integer and cannot be less than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            roomExit = new RoomExit
            {
                DestinationRoomID = Convert.ToInt32(txtBxDestRID.Text),
                ExitDirection = txtBxExitDirection.Text.ToLower(),
                RequiredSkill = txtBxRequiredSkill.Text
            };
        }
    }
}
