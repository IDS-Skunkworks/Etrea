using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3.Core;
using Etrea3;
using System.Text;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Quest> allQuests = new List<Quest>();
        private static ListViewItemComparer questComparer;

        #region Event Handlers
        private void btnLoadQuests_Click(object sender, EventArgs e)
        {
            LoadQuests();
        }

        private async void btnAddQuest_Click(object sender, EventArgs e)
        {
            if (!ValidateQuestData())
            {
                return;
            }
            var newQuest = GetQuestFromFormData();
            if (newQuest == null)
            {
                return;
            }
            var questJson = Helpers.SerialiseEtreaObject<Quest>(newQuest);
            btnAddQuest.Enabled = false;
            if (await APIHelper.AddNewAsset("/quest", questJson))
            {
                LoadQuests();
            }
            btnAddQuest.Enabled = true;
        }

        private async void btnUpdateQuest_Click(object sender, EventArgs e)
        {
            if (!ValidateQuestData())
            {
                return;
            }
            var newQuest = GetQuestFromFormData();
            if (newQuest == null)
            {
                return;
            }
            var questJson = Helpers.SerialiseEtreaObject<Quest>(newQuest);
            btnUpdateQuest.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/quest", questJson))
            {
                LoadQuests();
            }
            btnUpdateQuest.Enabled = true;
        }

        private void btnClearQuestForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearQuestForm();
            }
        }

        private async void btnDeleteQuest_Click(object sender, EventArgs e)
        {
            if (listViewQuests.SelectedItems.Count == 0)
            {
                return;
            }
            var quest = allQuests.FirstOrDefault(x => x.ID == int.Parse(listViewQuests.SelectedItems[0].SubItems[0].Text));
            if (quest == null)
            {
                return;
            }
            btnDeleteQuest.Enabled = false;
            if (MessageBox.Show("Delete the selected Quest? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (await APIHelper.DeleteExistingAsset($"/quest/{quest.ID}"))
                {
                    LoadQuests();
                }
            }
            btnDeleteQuest.Enabled = true;
        }

        private void btnAddQuestRequiredItem_Click(object sender, EventArgs e)
        {
            using (var si = new SelectInventoryItem(ItemType.Misc, false))
            {
                if (si.ShowDialog() == DialogResult.OK)
                {
                    var item = si._item;
                    bool newItem = true;
                    foreach (ListViewItem i in listViewQuestRequiredItems.Items)
                    {
                        if (int.Parse(i.SubItems[0].Text) == item.ID)
                        {
                            newItem = false;
                            var amount = int.Parse(i.SubItems[2].Text) + 1;
                            i.SubItems[2].Text = amount.ToString();
                            break;
                        }
                    }
                    if (newItem)
                    {
                        listViewQuestRequiredItems.Items.Add(new ListViewItem(new[]
                        {
                            item.ID.ToString(),
                            item.Name,
                            "1"
                        }));
                    }
                    foreach(ColumnHeader h in listViewQuestRequiredItems.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveQuestRequiredItem_Click(object sender, EventArgs e)
        {
            if (listViewQuestRequiredItems.SelectedItems.Count == 0)
            {
                return;
            }
            var i = listViewQuestRequiredItems.SelectedIndices[0];
            listViewQuestRequiredItems.Items.RemoveAt(i);
        }

        private void btnQuestClearRequiredItems_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Remove all Required Items for this Quest?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewQuestRequiredItems.Items.Clear();
            }
        }

        private void btnAddQuestMonster_Click(object sender, EventArgs e)
        {
            using (var sm = new SelectNPC())
            {
                if (sm.ShowDialog() == DialogResult.OK)
                {
                    var npc = sm._npc;
                    bool newNPC = true;
                    foreach (ListViewItem i in listViewQuestRequiredMonsters.Items)
                    {
                        if (npc.TemplateID == int.Parse(i.SubItems[0].Text))
                        {
                            var amount = int.Parse(i.SubItems[2].Text) + 1;
                            i.SubItems[2].Text = amount.ToString();
                            newNPC = false;
                        }
                    }
                    if (newNPC)
                    {
                        listViewQuestRequiredMonsters.Items.Add(new ListViewItem(new[]
                        {
                            npc.TemplateID.ToString(),
                            npc.Name,
                            "1"
                        }));
                    }
                    foreach (ColumnHeader h in listViewQuestRequiredMonsters.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveQuestMonster_Click(object sender, EventArgs e)
        {
            if (listViewQuestRequiredMonsters.SelectedItems.Count == 0)
            {
                return;
            }
            var i = listViewQuestRequiredMonsters.SelectedIndices[0];
            listViewQuestRequiredMonsters.Items.RemoveAt(i);
        }

        private void btnClearQuestMonsters_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all Required Monsters for this Quest?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewQuestRequiredMonsters.Items.Clear();
            }
        }

        private void btnAddQuestRewardItem_Click(object sender, EventArgs e)
        {
            using (var si = new SelectInventoryItem(ItemType.Misc, false))
            {
                if (si.ShowDialog() == DialogResult.OK)
                {
                    var item = si._item;
                    bool newItem = true;
                    foreach (ListViewItem i in listViewQuestRewardItems.Items)
                    {
                        if (item.ID == int.Parse(i.SubItems[0].Text))
                        {
                            var amount = int.Parse(i.SubItems[2].Text) + 1;
                            i.SubItems[2].Text = amount.ToString();
                            newItem = false;
                            break;
                        }
                    }
                    if (newItem)
                    {
                        listViewQuestRewardItems.Items.Add(new ListViewItem(new[]
                        {
                            item.ID.ToString(),
                            item.Name,
                            "1"
                        }));
                    }
                    foreach (ColumnHeader h in listViewQuestRewardItems.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveQuestRewardItem_Click(object sender, EventArgs e)
        {
            if (listViewQuestRewardItems.SelectedItems.Count == 0)
            {
                return;
            }
            var i = listViewQuestRewardItems.SelectedIndices[0];
            listViewQuestRewardItems.Items.RemoveAt(i);
        }

        private void btnClearQuestRewardItems_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Reward Items for this Quest?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewQuestRewardItems.Items.Clear();
            }
        }

        private void rtxtBxQuestFlavourText_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxQuestFlavourText.GetLineFromCharIndex(rtxtBxQuestFlavourText.SelectionStart);
            string currentLine = rtxtBxQuestFlavourText.Lines.Length > currentLineIndex ? rtxtBxQuestFlavourText.Lines[currentLineIndex] : string.Empty;
            lblQuestFlavourLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private async void listViewQuests_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewQuests.SelectedItems.Count == 0)
            {
                return;
            }
            var quest = allQuests.FirstOrDefault(x => x.ID == int.Parse(listViewQuests.SelectedItems[0].SubItems[0].Text));
            if (quest == null)
            {
                return;
            }
            txtBxQuestID.Text = quest.ID.ToString();
            txtBxQuestName.Text = quest.Name;
            txtBxQuestType.Text = quest.QuestType.ToString();
            txtBxQuestRewardExp.Text = quest.RewardExp.ToString();
            txtBxQuestRewardGP.Text = quest.RewardGold.ToString();
            txtBxQuestZone.Text = quest.Zone.ToString();
            if (quest.RequiredItems.Count > 0)
            {
                listViewQuestRequiredItems.Items.Clear();
                foreach (var item in quest.RequiredItems)
                {
                    var i = await APIHelper.LoadAssets<InventoryItem>($"/item/{item.Key}", true);
                    if (i != null)
                    {
                        listViewQuestRequiredItems.Items.Add(new ListViewItem(new[]
                        {
                            i.ID.ToString(),
                            i.Name,
                            item.Value.ToString()
                        }));
                    }
                    else
                    {
                        listViewQuestRequiredItems.Items.Add(new ListViewItem(new[]
                        {
                            item.Key.ToString(),
                            "Invalid Item",
                            item.Value.ToString()
                        }));
                    }
                }
                foreach (ColumnHeader h in listViewQuestRequiredItems.Columns)
                {
                    h.Width = -2;
                }
            }
            if (quest.RequiredMonsters.Count > 0)
            {
                listViewQuestRequiredMonsters.Items.Clear();
                foreach (var monster in quest.RequiredMonsters)
                {
                    var m = await APIHelper.LoadAssets<NPC>($"/npc/{monster.Key}", true);
                    if (m != null)
                    {
                        listViewQuestRequiredMonsters.Items.Add(new ListViewItem(new[]
                        {
                            m.TemplateID.ToString(),
                            m.Name,
                            monster.Value.ToString()
                        }));
                    }
                    else
                    {
                        listViewQuestRequiredMonsters.Items.Add(new ListViewItem(new[]
                        {
                            monster.Key.ToString(),
                            "Invalid NPC",
                            monster.Value.ToString()
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewQuestRequiredMonsters.Columns)
                {
                    h.Width = -2;
                }
            }
            if (quest.RewardItems.Count > 0)
            {
                listViewQuestRewardItems.Items.Clear();
                foreach (var item in quest.RewardItems)
                {
                    var i = await APIHelper.LoadAssets<InventoryItem>($"/item/{item.Key}", true);
                    if (i != null)
                    {
                        listViewQuestRewardItems.Items.Add(new ListViewItem(new[]
                        {
                            i.ID.ToString(),
                            i.Name,
                            item.Value.ToString()
                        }));
                    }
                    else
                    {
                        listViewQuestRewardItems.Items.Add(new ListViewItem(new[]
                        {
                            item.Key.ToString(),
                            "Invalid Item",
                            item.Value.ToString()
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewQuestRewardItems.Columns)
                {
                    h.Width = -2;
                }
            }
            rtxtBxQuestFlavourText.Text = quest.FlavourText;
        }

        private void listViewQuests_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (questComparer == null)
            {
                questComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == questComparer.SortColumn)
            {
                questComparer.SortOrder = questComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                questComparer.SortOrder = SortOrder.Ascending;
                questComparer.SortColumn = e.Column;
            }
            listViewQuests.ListViewItemSorter = questComparer;
            listViewQuests.Sort();
        }
        #endregion

        #region Functions
        private void ClearQuestForm()
        {
            txtBxQuestID.Clear();
            txtBxQuestName.Clear();
            txtBxQuestType.Clear();
            txtBxQuestRewardExp.Clear();
            txtBxQuestRewardGP.Clear();
            txtBxQuestZone.Clear();
            listViewQuestRequiredItems.Items.Clear();
            listViewQuestRequiredMonsters.Items.Clear();
            listViewQuestRewardItems.Items.Clear();
            rtxtBxQuestFlavourText.Clear();
        }

        private bool ValidateQuestData()
        {
            if (!int.TryParse(txtBxQuestID.Text, out int questID) || questID < 1)
            {
                MessageBox.Show("The Quest ID must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxQuestName.Text))
            {
                MessageBox.Show("The Quest must have a name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxQuestType.Text, true, out QuestType type) || type == QuestType.Undefined)
            {
                MessageBox.Show("The Quest Type is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxQuestRewardExp.Text, out int questRewardExp) || questRewardExp < 0)
            {
                MessageBox.Show("The Reward Exp must be a positive integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxQuestRewardGP.Text, out int questRewardGP) || questRewardGP < 0)
            {
                MessageBox.Show("The Reward GP must be a positive integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxQuestZone.Text, out int questZone) || questZone < 0)
            {
                MessageBox.Show("The Quest Zone is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (type == QuestType.Kill && listViewQuestRequiredMonsters.Items.Count == 0)
            {
                MessageBox.Show("Kill Quests require at least one Monster.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (type == QuestType.Fetch && listViewQuestRequiredItems.Items.Count == 0)
            {
                MessageBox.Show("Fetch Quests require at least one Item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxQuestFlavourText.Text))
            {
                MessageBox.Show("The Quest must have Flavour Text.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBxQuestFlavourTextLength.Checked)
            {
                foreach (var ln in rtxtBxQuestFlavourText.Lines)
                {
                    if (ln.Length > 80)
                    {
                        MessageBox.Show("One or more lines in the Flavour Text are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private Quest GetQuestFromFormData()
        {
            Quest newQuest = new Quest();
            try
            {
                newQuest.QuestGUID = Guid.NewGuid();
                newQuest.ID = int.Parse(txtBxQuestID.Text);
                newQuest.Name = txtBxQuestName.Text;
                newQuest.RewardGold = ulong.Parse(txtBxQuestRewardGP.Text);
                newQuest.RewardExp = uint.Parse(txtBxQuestRewardExp.Text);
                newQuest.Zone = int.Parse(txtBxQuestZone.Text);
                newQuest.FlavourText = rtxtBxQuestFlavourText.Lines.ConvertToString();
                Enum.TryParse(txtBxQuestType.Text, true, out QuestType type);
                newQuest.QuestType = type;
                if (listViewQuestRequiredItems.Items.Count > 0)
                {
                    foreach (ListViewItem i in listViewQuestRequiredItems.Items)
                    {
                        int itemID = int.Parse(i.SubItems[0].Text);
                        int amount = int.Parse(i.SubItems[2].Text);
                        newQuest.RequiredItems.Add(itemID, amount);
                    }
                }
                if (listViewQuestRequiredMonsters.Items.Count > 0)
                {
                    foreach (ListViewItem i in listViewQuestRequiredMonsters.Items)
                    {
                        int npcID = int.Parse(i.SubItems[0].Text);
                        int amount = int.Parse(i.SubItems[2].Text);
                        newQuest.RequiredMonsters.Add(npcID, amount);
                    }
                }
                if (listViewQuestRewardItems.Items.Count > 0)
                {
                    foreach (ListViewItem i in listViewQuestRewardItems.Items)
                    {
                        int itemID = int.Parse(i.SubItems[0].Text);
                        int amount = int.Parse(i.SubItems[2].Text);
                        newQuest.RewardItems.Add(itemID, amount);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Quest object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newQuest = null;
            }
            return newQuest;
        }

        private async void LoadQuests()
        {
            allQuests.Clear();
            listViewQuests.Items.Clear();
            ClearQuestForm();
            var result = await APIHelper.LoadAssets<List<Quest>>("/quest", false);
            if (result != null)
            {
                foreach(var q in result.OrderBy(x => x.ID))
                {
                    allQuests.Add(q);
                    listViewQuests.Items.Add(new ListViewItem(new[]
                    {
                        q.ID.ToString(),
                        q.Name,
                        q.Zone.ToString(),
                        q.QuestType.ToString(),
                        q.RewardGold.ToString(),
                        q.RewardExp.ToString(),
                        q.RewardItems.Count.ToString(),
                        q.RequiredItems.Count.ToString(),
                        q.RequiredMonsters.Count.ToString(),
                    }));
                }
                foreach(ColumnHeader h in listViewQuests.Columns)
                {
                    h.Width = -2;
                }
            }
        }
        #endregion
    }
}