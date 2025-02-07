using System;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public partial class GetIntegerPair : Form
    {
        public int id, amount;

        public GetIntegerPair(string title, string itemLabel, string amountLabel)
        {
            InitializeComponent();
            Text = title;
            labelAmount.Text = amountLabel;
            labelItem.Text = itemLabel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxAmount.Text))
            {
                MessageBox.Show("You must enter an amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            if (string.IsNullOrEmpty(txtBxID.Text))
            {
                MessageBox.Show("You must enter an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            if (!int.TryParse(txtBxID.Text, out id) || id < 0)
            {
                MessageBox.Show("ID must be a valid integer with a value greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            if (!int.TryParse(txtBxAmount.Text, out amount) || amount < 0)
            {
                MessageBox.Show("Amount must be a valid integer with a value greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
