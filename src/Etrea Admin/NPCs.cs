using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Etrea3;
using Etrea3.Core;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<NPC> allNPCS = new List<NPC>();
        private static ListViewItemComparer npcListViewComparer;
        private static ListViewItemComparer npcInventoryViewComparer;

        #region Event Handlers
        private async void btnAddNPCSpell_Click(object sender, EventArgs e)
        {
            using (var sp = new SelectSpell())
            {
                if (sp.ShowDialog() == DialogResult.OK)
                {
                    var spell = await APIHelper.LoadAssets<Spell>($"/spell/{sp._spellID}", false);
                    if (spell != null)
                    {
                        listViewNPCSpells.Items.Add(new ListViewItem(new[]
                        {
                            spell.ID.ToString(),
                            spell.Name,
                            spell.SpellType.ToString(),
                        }));
                    }
                    foreach (ColumnHeader h in listViewNPCSpells.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveNPCSpell_Click(object sender, EventArgs e)
        {
            if (listViewNPCSpells.SelectedItems.Count > 0)
            {
                var obj = listViewNPCSpells.SelectedItems[0];
                listViewNPCSpells.Items.Remove(obj);
            }
        }

        private void btnClearNPCSpells_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of Spells for this NPC?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewNPCSpells.Items.Clear();
            }
        }

        private async void btnAddNPCMobProg_Click(object sender, EventArgs e)
        {
            using (var mp = new SelectMobProg())
            {
                if (mp.ShowDialog() == DialogResult.OK)
                {
                    var mobProg = await APIHelper.LoadAssets<MobProg>($"/mobprog/{mp._mobProgID}", false);
                    if (mobProg != null)
                    {
                        listViewNPCMobProgs.Items.Add(new ListViewItem(new[]
                        {
                            mobProg.ID.ToString(),
                            mobProg.Name,
                            mobProg.Triggers.ToString()
                        }));
                    }
                    foreach(ColumnHeader h in listViewNPCMobProgs.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveNPCMobProgs_Click(object sender, EventArgs e)
        {
            if (listViewNPCMobProgs.SelectedItems.Count > 0)
            {
                var obj = listViewNPCMobProgs.SelectedItems[0];
                listViewNPCMobProgs.Items.Remove(obj);
            }
        }

        private void btnClearNPCMobProgs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of MobProgs for this NPC?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewNPCMobProgs.Items.Clear();
            }
        }

        private void btnNPCAddInventoryItem_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Misc, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    var selectedItem = item._item;
                    if (selectedItem != null)
                    {
                        listViewNPCInventory.Items.Add(new ListViewItem(new[]
                        {
                            selectedItem.ID.ToString(),
                            selectedItem.Name,
                        }));
                    }
                    foreach (ColumnHeader h in listViewNPCInventory.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnNPCRemoveInventoryItem_Click(object sender, EventArgs e)
        {
            if (listViewNPCInventory.SelectedItems.Count > 0)
            {
                var obj = listViewNPCInventory.SelectedItems[0];
                listViewNPCInventory.Items.Remove(obj);
            }
        }

        private void btnNPCClearInventory_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the NPC's inventory?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewNPCInventory.Items.Clear();
            }
        }

        private void btnSetHeadEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Armour, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Armour helm = item._item as Armour;
                        if (!helm.Slot.HasFlag(WearSlot.Head))
                        {
                            MessageBox.Show("That item cannot be equipped on the Head!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblHeadEquip.Text = $"Head: ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblHeldEquip.Text = "Head:";
                    }
                }
            }
        }

        private void btnSetNeckEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Armour, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Armour helm = item._item as Armour;
                        if (!helm.Slot.HasFlag(WearSlot.Neck))
                        {
                            MessageBox.Show("That item cannot be equipped on the Neck!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblNeckEquip.Text = $"Neck: ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblNeckEquip.Text = "Neck:";
                    }
                }
            }
        }

        private void btnSetArmourEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Armour, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Armour helm = item._item as Armour;
                        if (!helm.Slot.HasFlag(WearSlot.Body))
                        {
                            MessageBox.Show("That item cannot be equipped as Armour!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblArmourEquip.Text = $"Armour: ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblArmourEquip.Text = "Armour:";
                    }
                }
            }
        }

        private void btnSetWeaponEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Weapon, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Weapon wpn = item._item as Weapon;
                        lblWeaponEquip.Text = $"Weapon: ({wpn.ID}) - {wpn.Name}";
                    }
                    else
                    {
                        lblWeaponEquip.Text = "Weapon:";
                    }
                }
            }
        }

        private void btnSetHeldEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Armour, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Armour helm = item._item as Armour;
                        if (!helm.Slot.HasFlag(WearSlot.Held))
                        {
                            MessageBox.Show("That item cannot be held!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblHeldEquip.Text = $"Held: ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblHeldEquip.Text = "Held:";
                    }
                }
            }
        }

        private void btnSetFeetEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Armour, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Armour helm = item._item as Armour;
                        if (!helm.Slot.HasFlag(WearSlot.Feet))
                        {
                            MessageBox.Show("That item cannot be worn on the Feet!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblFeetEquip.Text = $"Feet: ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblFeetEquip.Text = "Feet:";
                    }
                }
            }
        }

        private void btnSetRingLeftEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Ring, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Ring helm = item._item as Ring;
                        if (!helm.Slot.HasFlag(WearSlot.Finger))
                        {
                            MessageBox.Show("That item cannot be worn on the fingers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblLeftRingEquip.Text = $"Ring (L): ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblLeftRingEquip.Text = "Ring (L):";
                    }
                }
            }
        }

        private void btnSetRingRightEquip_Click(object sender, EventArgs e)
        {
            using (var item = new SelectInventoryItem(ItemType.Ring, false))
            {
                if (item.ShowDialog() == DialogResult.OK)
                {
                    if (item._item != null)
                    {
                        Ring helm = item._item as Ring;
                        if (!helm.Slot.HasFlag(WearSlot.Finger))
                        {
                            MessageBox.Show("That item cannot be worn on the fingers!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        lblRightRingEquip.Text = $"Ring (R): ({helm.ID}) - {helm.Name}";
                    }
                    else
                    {
                        lblRightRingEquip.Text = "Ring (R):";
                    }
                }
            }
        }

        private async void listViewNPCs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewNPCs.SelectedItems.Count == 0)
            {
                return;
            }
            var npc = allNPCS.FirstOrDefault(x => x.TemplateID == Convert.ToInt32(listViewNPCs.SelectedItems[0].SubItems[0].Text));
            if (npc == null)
            {
                return;
            }
            ClearNPCForm();
            txtBxNPCID.Text = npc.TemplateID.ToString();
            txtbxNPCName.Text = npc.Name.ToString();
            txtBxNPCShortDesc.Text = npc.ShortDescription;
            txtBxNPCZone.Text = npc.ZoneID.ToString();
            rtBoxNPCLongDesc.Text = npc.LongDescription;
            txtBxNPCMaxNumber.Text = npc.MaxNumberInWorld.ToString();
            txtBxNPCAppearChance.Text = npc.AppearanceChance.ToString();
            txtBxNPCGold.Text = npc.Gold.ToString();
            txtBxNPCExp.Text = npc.ExpAward.ToString();
            txtBxNPCGender.Text = npc.Gender.ToString();
            txtBxNPCShopID.Text = npc.ShopID.ToString();
            txtBxNPCFlags.Text = npc.Flags.ToString();
            txtBxNPCArrivalMessage.Text = npc.ArrivalMessage;
            txtBxNPCDepartureMessage.Text = npc.DepatureMessage;
            txtBxNPCSTR.Text = npc.Strength.ToString();
            txtBxNPCDEX.Text = npc.Dexterity.ToString();
            txtBxNPCCON.Text = npc.Constitution.ToString();
            txtBxNPCINT.Text = npc.Intelligence.ToString();
            txtBxNPCWIS.Text = npc.Wisdom.ToString();
            txtBxNPCCHA.Text = npc.Charisma.ToString();
            txtBxNPCArmourClass.Text = npc.ArmourClass.ToString();
            chkBxNPCNaturalArmour.Checked = npc.NaturalArmour;
            txtBxNPCLevel.Text = npc.Level.ToString();
            txtBxNPCBonusHitDie.Text = npc.BonusHitDice.ToString();
            txtBxNPCHitDieSize.Text = npc.HitDieSize.ToString();
            txtBxNPCBonusHP.Text = npc.BonusHP.ToString();

            if (npc.Inventory.Count > 0)
            {
                foreach(var item in npc.Inventory.Values.OrderBy(x => x.ID))
                {
                    listViewNPCInventory.Items.Add(new ListViewItem(new[]
                    {
                        item.ID.ToString(),
                        item.Name,
                    }));
                }
            }

            if (npc.MobProgs.Count > 0)
            {
                foreach(var mpID in npc.MobProgs.Keys)
                {
                    var mobProg = await APIHelper.LoadAssets<MobProg>($"/mobprog/{mpID}", true);
                    if (mobProg == null)
                    {
                        listViewNPCMobProgs.Items.Add(new ListViewItem(new[]
                        {
                            mpID.ToString(),
                            "Invalid MobProg",
                            "Invalid MobProg",
                        }));
                    }
                    else
                    {
                        listViewNPCMobProgs.Items.Add(new ListViewItem(new[]
                        {
                            mobProg.ID.ToString(),
                            mobProg.Name,
                            mobProg.Triggers.ToString(),
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewNPCMobProgs.Columns)
                {
                    h.Width = -2;
                }
            }

            if (npc.Spells.Count > 0)
            {
                foreach (var spellID in npc.Spells.Keys)
                {
                    string encodedName = Uri.EscapeDataString(spellID);
                    var spell = await APIHelper.LoadAssets<Spell>($"/spell/name/{encodedName}", false);
                    if (spell == null)
                    {
                        listViewNPCSpells.Items.Add(new ListViewItem(new[]
                        {
                            spellID.ToString(),
                            "Invalid Spell",
                            "Invalid Spell"
                        }));
                    }
                    else
                    {
                        listViewNPCSpells.Items.Add(new ListViewItem(new[]
                        {
                            spell.ID.ToString(),
                            spell.Name,
                            spell.SpellType.ToString(),
                        }));
                    }
                }
                foreach(ColumnHeader h in listViewNPCSpells.Columns)
                {
                    h.Width = -2;
                }
            }

            if (npc.HeadEquip != null)
            {
                lblHeadEquip.Text = $"Head: ({npc.HeadEquip.ID}) - {npc.HeadEquip.Name}";
            }
            if (npc.NeckEquip != null)
            {
                lblNeckEquip.Text = $"Neck: ({npc.NeckEquip.ID}) - {npc.NeckEquip.Name}";
            }
            if (npc.WeaponEquip != null)
            {
                lblWeaponEquip.Text = $"Weapon: ({npc.WeaponEquip.ID}) - {npc.WeaponEquip.Name}";
            }
            if (npc.ArmourEquip != null)
            {
                lblArmourEquip.Text = $"Armour: ({npc.ArmourEquip.ID}) - {npc.ArmourEquip.Name}";
            }
            if (npc.HeldEquip != null)
            {
                lblHeldEquip.Text = $"Held: ({npc.HeldEquip.ID}) - {npc.HeldEquip.Name}";
            }
            if (npc.FeetEquip != null)
            {
                lblFeetEquip.Text = $"Feet: ({npc.FeetEquip.ID}) - {npc.FeetEquip.Name}";
            }
            if (npc.LeftFingerEquip != null)
            {
                lblLeftRingEquip.Text = $"Ring (L): ({npc.LeftFingerEquip.ID}) - {npc.LeftFingerEquip.Name}";
            }
            if (npc.RightFingerEquip != null)
            {
                lblRightRingEquip.Text = $"Ring (R): ({npc.RightFingerEquip.ID}) - {npc.RightFingerEquip.Name}";
            }
        }

        private void listViewNPCInventory_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (npcInventoryViewComparer == null)
            {
                npcInventoryViewComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == npcInventoryViewComparer.SortColumn)
            {
                npcInventoryViewComparer.SortOrder = npcInventoryViewComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                npcInventoryViewComparer.SortOrder = SortOrder.Ascending;
                npcInventoryViewComparer.SortColumn = e.Column;
            }
            listViewNPCInventory.ListViewItemSorter = npcInventoryViewComparer;
            listViewNPCInventory.Sort();
        }

        private void listViewNPCs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (npcListViewComparer == null)
            {
                npcListViewComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == npcListViewComparer.SortColumn)
            {
                npcListViewComparer.SortOrder = npcListViewComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                npcListViewComparer.SortOrder = SortOrder.Ascending;
                npcListViewComparer.SortColumn = e.Column;
            }
            listViewNPCs.ListViewItemSorter = npcListViewComparer;
            listViewNPCs.Sort();
        }

        private void btnLoadNPCs_Click(object sender, EventArgs e)
        {
            GetNPCs();
        }

        private async void btnAddNPC_Click(object sender, EventArgs e)
        {
            if (!ValidateNPCData())
            {
                return;
            }
            var newNPC = await GetNPCFromFormData();
            if (newNPC == null)
            {
                return;
            }
            btnAddNPC.Enabled = false;
            var npcJson = Helpers.SerialiseEtreaObject<NPC>(newNPC);
            if (await APIHelper.AddNewAsset($"/npc", npcJson))
            {
                GetNPCs();
            }
            btnAddNPC.Enabled = true;
        }

        private async void btnNPCUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateNPCData())
            {
                return;
            }
            var newNPC = await GetNPCFromFormData();
            if (newNPC == null)
            {
                return;
            }
            btnNPCUpdate.Enabled = false;
            var npcJson = Helpers.SerialiseEtreaObject<NPC>(newNPC);
            if (await APIHelper.UpdateExistingAsset("/npc", npcJson))
            {
                GetNPCs();
            }
            btnNPCUpdate.Enabled = true;
        }

        private void btnClearNPC_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the form fields?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearNPCForm();
            }
        }

        private async void btnDeleteNPC_Click(object sender, EventArgs e)
        {
            if (listViewNPCs.SelectedItems.Count == 0)
            {
                return;
            }
            var npcID = listViewNPCs.SelectedItems[0].Text;
            if (MessageBox.Show($"Delete the selected NPC? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteNPC.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/npc/{npcID}"))
                {
                    GetNPCs();
                }
                btnDeleteNPC.Enabled = true;
            }
        }

        private void rtBoxNPCLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtBoxNPCLongDesc.GetLineFromCharIndex(rtBoxNPCLongDesc.SelectionStart);
            string currentLine = rtBoxNPCLongDesc.Lines.Length > currentLineIndex ? rtBoxNPCLongDesc.Lines[currentLineIndex] : string.Empty;
            lblNPCLineLength.Text = $"Line Length: {currentLine.Length} characters";
        }
        #endregion

        #region Functions
        private async Task<NPC> GetNPCFromFormData()
        {
            string pattern = @"\((\d+)\)";
            NPC newNPC = new NPC();
            try
            {
                newNPC.TemplateID = int.Parse(txtBxNPCID.Text);
                newNPC.Name = txtbxNPCName.Text;
                newNPC.ShortDescription = txtBxNPCShortDesc.Text;
                newNPC.LongDescription = rtBoxNPCLongDesc.Text;
                newNPC.ZoneID = int.Parse(txtBxNPCZone.Text);
                newNPC.MaxNumberInWorld = int.Parse(txtBxNPCMaxNumber.Text);
                newNPC.AppearanceChance = int.Parse(txtBxNPCAppearChance.Text);
                newNPC.Gold = ulong.Parse(txtBxNPCGold.Text);
                newNPC.ExpAward = int.Parse(txtBxNPCExp.Text);
                Enum.TryParse(txtBxNPCGender.Text, true, out Gender genderResult);
                newNPC.Gender = genderResult;
                newNPC.ShopID = int.Parse(txtBxNPCShopID.Text);
                Enum.TryParse(txtBxNPCFlags.Text, true, out NPCFlags flagsResult);
                newNPC.Flags = flagsResult;
                newNPC.ArrivalMessage = txtBxNPCArrivalMessage.Text;
                newNPC.DepatureMessage = txtBxNPCDepartureMessage.Text;
                newNPC.Strength = int.Parse(txtBxNPCSTR.Text);
                newNPC.Dexterity = int.Parse(txtBxNPCDEX.Text);
                newNPC.Constitution = int.Parse(txtBxNPCCON.Text);
                newNPC.Intelligence = int.Parse(txtBxNPCINT.Text);
                newNPC.Wisdom = int.Parse(txtBxNPCWIS.Text);
                newNPC.Charisma = int.Parse(txtBxNPCCHA.Text);
                newNPC.Level = int.Parse(txtBxNPCLevel.Text);
                newNPC.BaseArmourClass = int.Parse(txtBxNPCArmourClass.Text);
                newNPC.NaturalArmour = chkBxNPCNaturalArmour.Checked;
                newNPC.BonusHitDice = int.Parse(txtBxNPCBonusHitDie.Text);
                newNPC.HitDieSize = int.Parse(txtBxNPCHitDieSize.Text);
                newNPC.BonusHP = int.Parse(txtBxNPCBonusHP.Text);
                if (listViewNPCInventory.Items.Count > 0 )
                {
                    foreach(ListViewItem item in listViewNPCInventory.Items)
                    {
                        var i = await APIHelper.LoadAssets<dynamic>($"/item/{item.SubItems[0].Text}", true);
                        dynamic newItem = null;
                        var itemID = Guid.NewGuid();
                        switch(i.ItemType)
                        {
                            case ItemType.Armour:
                                newItem = (Armour)i;
                                newItem.ItemID = itemID;
                                newNPC.Inventory.TryAdd(itemID, newItem);
                                break;

                            case ItemType.Weapon:
                                newItem = (Weapon)i;
                                newItem.ItemID = itemID;
                                newNPC.Inventory.TryAdd(itemID, newItem);
                                break;

                            case ItemType.Ring:
                                newItem = (Ring)i;
                                newItem.ItemID = itemID;
                                newNPC.Inventory.TryAdd(itemID, newItem);
                                break;

                            case ItemType.Scroll:
                                newItem = (Scroll)i;
                                newItem.ItemID = itemID;
                                newNPC.Inventory.TryAdd(itemID, newItem);
                                break;

                            case ItemType.Consumable:
                                newItem = (Consumable)i;
                                newItem.ItemID = itemID;
                                newNPC.Inventory.TryAdd(itemID, newItem);
                                break;

                            default:
                                newItem = (InventoryItem)i;
                                newItem.ItemID = itemID;
                                newNPC.Inventory.TryAdd(itemID, newItem);
                                break;
                        }
                    }
                }
                if (listViewNPCMobProgs.Items.Count > 0)
                {
                    foreach(ListViewItem item in listViewNPCMobProgs.Items)
                    {
                        var mobProgID = int.Parse(item.SubItems[0].Text);
                        newNPC.MobProgs.TryAdd(mobProgID, true);
                    }
                }
                if (listViewNPCSpells.Items.Count > 0)
                {
                    foreach(ListViewItem item in listViewNPCSpells.Items)
                    {
                        newNPC.Spells.TryAdd(item.SubItems[1].Text, true);
                    }
                }
                if (lblHeadEquip.Text != "Head:")
                {
                    Match match = Regex.Match(lblHeadEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.HeadEquip = await APIHelper.LoadAssets<Armour>($"/item/{itemID}", false);
                    }
                }
                if (lblNeckEquip.Text != "Neck:")
                {
                    Match match = Regex.Match(lblNeckEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.NeckEquip = await APIHelper.LoadAssets<Armour>($"/item/{itemID}", false);
                    }
                }
                if (lblArmourEquip.Text != "Armour:")
                {
                    Match match = Regex.Match(lblArmourEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.ArmourEquip = await APIHelper.LoadAssets<Armour>($"/item/{itemID}", false);
                    }
                }
                if (lblWeaponEquip.Text != "Weapon:")
                {
                    Match match = Regex.Match(lblWeaponEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.WeaponEquip = await APIHelper.LoadAssets<Weapon>($"/item/{itemID}", false);
                    }
                }
                if (lblHeldEquip.Text != "Held:")
                {
                    Match match = Regex.Match(lblHeldEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.HeldEquip = await APIHelper.LoadAssets<Armour>($"/item/{itemID}", false);
                    }
                }
                if (lblFeetEquip.Text != "Feet:")
                {
                    Match match = Regex.Match(lblFeetEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.FeetEquip = await APIHelper.LoadAssets<Armour>($"/item/{itemID}", false);
                    }
                }
                if (lblLeftRingEquip.Text != "Ring (L):")
                {
                    Match match = Regex.Match(lblLeftRingEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.LeftFingerEquip = await APIHelper.LoadAssets<Ring>($"/item/{itemID}", false);
                    }
                }
                if (lblRightRingEquip.Text != "Ring (R):")
                {
                    Match match = Regex.Match(lblRightRingEquip.Text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int itemID = int.Parse(match.Groups[1].Value);
                        newNPC.RightFingerEquip = await APIHelper.LoadAssets<Ring>($"/item/{itemID}", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating NPC object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newNPC = null;
            }
            return newNPC;
        }

        private bool ValidateNPCData()
        {
            if (string.IsNullOrEmpty(txtBxNPCID.Text))
            {
                MessageBox.Show("The NPC must have an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCID.Text, out int npcID) || npcID < 1)
            {
                MessageBox.Show("The NPC ID must be an integer with a value greater than 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtbxNPCName.Text))
            {
                MessageBox.Show("The NPC must have a Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCShortDesc.Text))
            {
                MessageBox.Show("The NPC must have a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtBoxNPCLongDesc.Text))
            {
                MessageBox.Show("The NPC must have a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBxNPCOverrideDescLength.Checked)
            {
                foreach (var ln in rtBoxNPCLongDesc.Lines)
                {
                    if (ln.Length > 80)
                    {
                        MessageBox.Show("One or more lines in the NPC Long Description is longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            if (string.IsNullOrEmpty(txtBxNPCZone.Text))
            {
                MessageBox.Show("The NPC must be assigned to a Zone.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCZone.Text, out int z) || z < 0)
            {
                MessageBox.Show("The NPC Zone must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCMaxNumber.Text))
            {
                MessageBox.Show("You must provide a maximum number of this NPC that can be in the world at once.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCMaxNumber.Text, out int maxno) || maxno < 0)
            {
                MessageBox.Show("The max number for this NPC must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCAppearChance.Text))
            {
                MessageBox.Show("You must provide an Appearance Chance for this NPC.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCAppearChance.Text, out int appearChance) || appearChance < 0)
            {
                MessageBox.Show("The Appearance Chance must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCGold.Text))
            {
                MessageBox.Show("The NPC must be assigned an amount of gold to drop.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCGold.Text, out int gp) || gp < 0)
            {
                MessageBox.Show("The amount of gold must be an integer of 0 or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCExp.Text))
            {
                MessageBox.Show("The NPC must award an amount of Exp.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCExp.Text, out int exp) || exp < 0)
            {
                MessageBox.Show("The amount of Exp awarded must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCGender.Text))
            {
                MessageBox.Show("The NPC must have a gender.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxNPCGender.Text, true, out Gender gender) || gender == Gender.Undefined)
            {
                MessageBox.Show("The provided gender is not valid. Valid genders are Male, Female or NonBinary.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCShopID.Text))
            {
                txtBxNPCShopID.Text = "0";
            }
            if (!int.TryParse(txtBxNPCShopID.Text, out int shopID) || shopID < 0)
            {
                MessageBox.Show("The Shop ID for the NPC must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCArrivalMessage.Text))
            {
                MessageBox.Show("The NPC must have an arrival message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCDepartureMessage.Text))
            {
                MessageBox.Show("The NPC must have a departure message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCSTR.Text) || string.IsNullOrEmpty(txtBxNPCDEX.Text) || string.IsNullOrEmpty(txtBxNPCCON.Text) || string.IsNullOrEmpty(txtBxNPCINT.Text)
                || string.IsNullOrEmpty(txtBxNPCWIS.Text) || string.IsNullOrEmpty(txtBxNPCCHA.Text))
            {
                MessageBox.Show("The NPC must have values for STR, DEX, CON, INT, WIS and CHA.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCSTR.Text, out int str) || !int.TryParse(txtBxNPCDEX.Text, out int dex) || !int.TryParse(txtBxNPCCON.Text, out int con)
                || !int.TryParse(txtBxNPCINT.Text, out int intel) || !int.TryParse(txtBxNPCWIS.Text, out int wis) || !int.TryParse(txtBxNPCCHA.Text, out int cha))
            {
                MessageBox.Show("One or more required stats are not valid numbers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (str < 1 || con < 1 || dex < 1 || intel < 1 || wis < 1 || cha < 1)
            {
                MessageBox.Show("One or more required stats have a value of 0 or lower. All stats must be a minimum of 1.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCLevel.Text))
            {
                MessageBox.Show("The NPC must have a Level.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCLevel.Text, out int level) || level < 1)
            {
                MessageBox.Show("The NPC Level must be a valid integer with a value of 1 or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCArmourClass.Text))
            {
                MessageBox.Show("The NPC must have a base Armour Class.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCArmourClass.Text, out int armourClass) || armourClass < 1)
            {
                MessageBox.Show("The Armour Class must be a valid integer with a value of 1 or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!string.IsNullOrEmpty(txtBxNPCBonusHitDie.Text) && !int.TryParse(txtBxNPCBonusHitDie.Text, out int bonusHitDie) && bonusHitDie < 0)
            {
                MessageBox.Show("If Bonus Hit Die are applied to the NPC, the value must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxNPCHitDieSize.Text))
            {
                MessageBox.Show("The NPC must be given Hit Die Size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxNPCHitDieSize.Text, out int hdSize) || hdSize < 1)
            {
                MessageBox.Show("Hit Die Size must be a valid non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!string.IsNullOrEmpty(txtBxNPCBonusHP.Text) && !int.TryParse(txtBxNPCBonusHP.Text, out int bonusHP) && bonusHP < 0)
            {
                MessageBox.Show("If Bonus HP are applied for this NPC, the value must be a non-negative integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private async void GetNPCs()
        {
            allNPCS.Clear();
            listViewNPCs.Items.Clear();
            ClearNPCForm();
            btnLoadNPCs.Enabled = false;
            var result = await APIHelper.LoadAssets<List<NPC>>("/npc", false);
            if (result != null)
            {
                foreach (var npc in result.OrderBy(x => x.TemplateID))
                {
                    allNPCS.Add(npc);
                    listViewNPCs.Items.Add(new ListViewItem(new[]
                    {
                        npc.TemplateID.ToString(),
                        npc.Name,
                        npc.ZoneID.ToString(),
                        npc.AppearanceChance.ToString(),
                        npc.MaxNumberInWorld.ToString(),
                        npc.ShortDescription
                    }));
                }
                foreach (ColumnHeader h in listViewNPCs.Columns)
                {
                    h.Width = -2;
                }
            }
            btnLoadNPCs.Enabled = true;
        }

        private void ClearNPCForm()
        {
            txtBxNPCID.Clear();
            txtbxNPCName.Clear();
            txtBxNPCShortDesc.Clear();
            txtBxNPCZone.Clear();
            rtBoxNPCLongDesc.Clear();
            txtBxNPCMaxNumber.Clear();
            txtBxNPCAppearChance.Clear();
            txtBxNPCGold.Clear();
            txtBxNPCExp.Clear();
            txtBxNPCGender.Clear();
            txtBxNPCShopID.Clear();
            txtBxNPCFlags.Clear();
            txtBxNPCArrivalMessage.Clear();
            txtBxNPCDepartureMessage.Clear();
            txtBxNPCSTR.Clear();
            txtBxNPCDEX.Clear();
            txtBxNPCCON.Clear();
            txtBxNPCINT.Clear();
            txtBxNPCWIS.Clear();
            txtBxNPCCHA.Clear();
            txtBxNPCArmourClass.Clear();
            chkBxNPCNaturalArmour.Checked = false;
            txtBxNPCLevel.Clear();
            txtBxNPCBonusHitDie.Clear();
            txtBxNPCHitDieSize.Clear();
            txtBxNPCBonusHP.Clear();
            listViewNPCInventory.Items.Clear();
            listViewNPCSpells.Items.Clear();
            listViewNPCMobProgs.Items.Clear();
            lblHeadEquip.Text = "Head:";
            lblNeckEquip.Text = "Neck:";
            lblArmourEquip.Text = "Armour";
            lblWeaponEquip.Text = "Weapon:";
            lblHeldEquip.Text = "Held:";
            lblFeetEquip.Text = "Feet:";
            lblLeftRingEquip.Text = "Ring (L):";
            lblRightRingEquip.Text = "Ring (R):";
        }
        #endregion
    }
}