using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Newtonsoft.Json;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Emote> allEmotes = new List<Emote>();

        #region Event Handlers
        private void listBxEmotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 0 = no target, 1 = with target, 2 = target not found, 3 = target == performer
            if (listBxEmotes.SelectedItem != null)
            {
                Emote item = (Emote)listBxEmotes.SelectedItem;
                txtBxEmoteID.Text = item.ID.ToString();
                txtBxEmoteName.Text = item.Name.ToString();
                txtBxMsgPerformerNoTarget.Text = item.MessageToPerformer[0];
                txtBxMsgPerformerWithTarget.Text = item.MessageToPerformer[1];
                txtBxMsgPerformerTargtNotFound.Text = item.MessageToPerformer[2];
                txtBxMsgPerformerTargetIsPerformer.Text = item.MessageToPerformer[3];
                txtBxEmoteMsgToTarget.Text = item.MessageToTarget;
                txtBxMsgOthersNoTarget.Text = item.MessageToOthers[0];
                txtBxMsgOthersWithTarget.Text = item.MessageToOthers[1];
                txtBxMsgOthersTargetNotFound.Text = item.MessageToOthers[2];
                txtBxMsgOthersTargetIsPerformer.Text= item.MessageToOthers[3];
            }
        }

        private void btnEmoteLoad_Click(object sender, EventArgs e)
        {
            GetEmotes();
        }

        private async void btnEmoteAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateEmote())
            {
                return;
            }
            Emote emote = GetEmoteFromForm();
            if (emote == null)
            {
                return;
            }
            btnEmoteAdd.Enabled = false;
            var emoteJson = JsonConvert.SerializeObject(emote);
            if (await APIHelper.AddNewAsset($"/emote", emoteJson))
            {
                GetEmotes();
            }
            btnEmoteAdd.Enabled = true;
        }

        private async void btnEmoteUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateEmote())
            {
                return;
            }
            Emote emote = GetEmoteFromForm();
            if (emote == null)
            {
                return;
            }
            btnEmoteUpdate.Enabled = false;
            var emoteJson = JsonConvert.SerializeObject(emote);
            if (await APIHelper.UpdateExistingAsset($"/emote", emoteJson))
            {
                GetEmotes();
            }
            btnEmoteUpdate.Enabled = true;
        }

        private void btnEmoteClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the form fields?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearEmoteForm();
            }
        }

        private async void btnEmoteDelete_Click(object sender, EventArgs e)
        {
            if (listBxEmotes.SelectedItem == null)
            {
                return;
            }
            btnEmoteDelete.Enabled = false;
            Emote emote = (Emote)listBxEmotes.SelectedItem;
            if (MessageBox.Show("Delete the selected Emote? This action cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (await APIHelper.DeleteExistingAsset($"/emote/{emote.ID}"))
                {
                    GetEmotes();
                }
            }
            btnEmoteDelete.Enabled = true;
        }
        #endregion

        #region Functions
        private async void GetEmotes()
        {
            btnEmoteLoad.Enabled = false;
            ClearEmoteForm();
            listBxEmotes.DataSource = null;
            var result = await APIHelper.LoadAssets<List<Emote>>("/emote", false);
            if (result != null)
            {
                allEmotes.Clear();
                allEmotes.AddRange(result.OrderBy(x => x.ID));
            }
            listBxEmotes.DataSource = allEmotes;
            btnEmoteLoad.Enabled = true;
        }

        private void ClearEmoteForm()
        {
            txtBxEmoteID.Clear();
            txtBxEmoteName.Clear();
            txtBxMsgPerformerNoTarget.Clear();
            txtBxMsgPerformerWithTarget.Clear();
            txtBxMsgPerformerTargtNotFound.Clear();
            txtBxMsgPerformerTargetIsPerformer.Clear();
            txtBxEmoteMsgToTarget.Clear();
            txtBxMsgOthersNoTarget.Clear();
            txtBxMsgOthersWithTarget.Clear();
            txtBxMsgOthersTargetNotFound.Clear();
            txtBxMsgOthersTargetIsPerformer.Clear();
        }

        private Emote GetEmoteFromForm()
        {
            Emote emote = new Emote();
            try
            {
                // 0 = no target, 1 = with target, 2 = target not found, 3 = target == performer
                emote.ID = Convert.ToInt32(txtBxEmoteID.Text);
                emote.Name = txtBxEmoteName.Text;
                emote.MessageToPerformer[0] = txtBxMsgPerformerNoTarget.Text;
                emote.MessageToPerformer[1] = txtBxMsgPerformerWithTarget.Text;
                emote.MessageToPerformer[2] = txtBxMsgPerformerTargtNotFound.Text;
                emote.MessageToPerformer[3] = txtBxMsgPerformerTargetIsPerformer.Text;
                emote.MessageToTarget = txtBxEmoteMsgToTarget.Text;
                emote.MessageToOthers[0] = txtBxMsgOthersNoTarget.Text;
                emote.MessageToOthers[1] = txtBxMsgOthersWithTarget.Text;
                emote.MessageToOthers[2] = txtBxMsgOthersTargetNotFound.Text;
                emote.MessageToOthers[3] = txtBxMsgOthersTargetIsPerformer.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Emote object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emote = null;
            }
            return emote;
        }

        private bool ValidateEmote()
        {
            if (string.IsNullOrEmpty(txtBxEmoteID.Text))
            {
                MessageBox.Show("You must provide an Emote ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxEmoteName.Text))
            {
                MessageBox.Show("You must provide an Emote name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgPerformerNoTarget.Text))
            {
                MessageBox.Show("You must provide a message for the Performer when there is no Target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgPerformerWithTarget.Text))
            {
                MessageBox.Show("You must provide a message for the Performer when there is a Target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgPerformerTargtNotFound.Text))
            {
                MessageBox.Show("You must provide a message for the Performer when the Target is not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgPerformerTargetIsPerformer.Text))
            {
                MessageBox.Show("You must provide a message for the Performer when the Target is the Performer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxEmoteMsgToTarget.Text))
            {
                MessageBox.Show("You must provide a message for the Target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgOthersNoTarget.Text))
            {
                MessageBox.Show("You must provide a message for Others when the Performer did not provide a Target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgOthersTargetNotFound.Text))
            {
                MessageBox.Show("You must provide a message for Others when the Target was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgOthersWithTarget.Text))
            {
                MessageBox.Show("You must provide a message for Others when the Performer specified a Target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxMsgOthersTargetIsPerformer.Text))
            {
                MessageBox.Show("You must provide a message for Others when the Performer and the Target are the same.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxEmoteID.Text, out int emoteID) || emoteID < 0)
            {
                MessageBox.Show("Emote ID must be an integer number with a value greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        #endregion
    }
}