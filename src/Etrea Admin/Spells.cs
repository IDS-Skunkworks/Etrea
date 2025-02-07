using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3.Core;
using Etrea3;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<Spell> allSpells = new List<Spell>();
        private static ListViewItemComparer spellComparer;

        #region Event Handlers
        private void listViewSpells_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewSpells.SelectedItems.Count == 0)
            {
                return;
            }
            var spl = allSpells.FirstOrDefault(x => x.ID == int.Parse(listViewSpells.SelectedItems[0].SubItems[0].Text));
            if (spl == null)
            {
                return;
            }
            ClearSpellForm();
            txtBxSpellID.Text = spl.ID.ToString();
            txtBxSpellName.Text = spl.Name;
            txtBxSpellClasses.Text = spl.AvailableToClass.ToString();
            txtBxSpellMPCost.Text = spl.MPCostExpression;
            txtBxSpellDamage.Text = spl.DamageExpression;
            chkBxSpellAutoHit.Checked = spl.AutoHitTarget;
            chkBxSpellAOE.Checked = spl.IsAOE;
            chkBxSpellAbilityMod.Checked = spl.ApplyAbilityModifier;
            txtBxSpellLearnCost.Text = spl.LearnCost.ToString();
            txtBxSpellDescription.Text = spl.Description;
            txtBxSpellType.Text = spl.SpellType.ToString();
            if (spl.AppliedBuffs.Count > 0)
            {
                foreach (var b in spl.AppliedBuffs)
                {
                    listBxSpellBuffs.Items.Add(b.Key);
                }
            }
        }

        private void listViewSpells_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (spellComparer == null)
            {
                spellComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == spellComparer.SortColumn)
            {
                spellComparer.SortOrder = spellComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                spellComparer.SortOrder = SortOrder.Ascending;
                spellComparer.SortColumn = e.Column;
            }
            listViewSpells.ListViewItemSorter = spellComparer;
            listViewSpells.Sort();
        }

        private void btnLoadSpells_Click(object sender, EventArgs e)
        {
            LoadSpells();
        }

        private async void btnAddSpell_Click(object sender, EventArgs e)
        {
            if (!ValidateSpellData())
            {
                return;
            }
            var newSpell = GetSpellFromFormData();
            if (newSpell == null)
            {
                return;
            }
            var spellJson = Helpers.SerialiseEtreaObject<Spell>(newSpell);
            btnAddSpell.Enabled = false;
            if (await APIHelper.AddNewAsset("/spell", spellJson))
            {
                LoadSpells();
            }
            btnAddSpell.Enabled = true;
        }

        private async void btnUpdateSpell_Click(object sender, EventArgs e)
        {
            if (!ValidateSpellData())
            {
                return;
            }
            var newSpell = GetSpellFromFormData();
            if (newSpell == null)
            {
                return;
            }
            var spellJson = Helpers.SerialiseEtreaObject<Spell>(newSpell);
            btnUpdateSpell.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/spell", spellJson))
            {
                LoadSpells();
            }
            btnUpdateSpell.Enabled = true;
        }

        private void btnClearSpellForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearSpellForm();
            }
        }

        private async void btnDeleteSpell_Click(object sender, EventArgs e)
        {
            if (listViewSpells.SelectedItems.Count == 0)
            {
                return;
            }
            var spellID = listViewSpells.SelectedItems[0].SubItems[0].Text;
            if (MessageBox.Show("Delete the selected Spell? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteSpell.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/spell/{spellID}"))
                {
                    LoadSpells();
                }
                btnDeleteSpell.Enabled = true;
            }
        }

        private void btnSpellAddBuff_Click(object sender, EventArgs e)
        {
            using (var bs = new BuffSelector())
            {
                if (bs.ShowDialog() == DialogResult.OK)
                {
                    listBxSpellBuffs.Items.Add(bs._selectedBuff);
                }
            }
        }

        private void btnSpellRemoveBuff_Click(object sender, EventArgs e)
        {
            if (listBxSpellBuffs.SelectedItems.Count == 0)
            {
                return;
            }
            var obj = listBxSpellBuffs.SelectedItems[0];
            listBxSpellBuffs.Items.Remove(obj);
        }

        private void btnSpellClearBuffs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear Buffs for the Spell?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBxSpellBuffs.Items.Clear();
            }
        }
        #endregion

        #region Functions
        private async void LoadSpells()
        {
            listViewSpells.Items.Clear();
            allSpells.Clear();
            ClearSpellForm();
            var result = await APIHelper.LoadAssets<List<Spell>>("/spell", false);
            if (result != null)
            {
                foreach (Spell spell in result.OrderBy(x => x.ID))
                {
                    allSpells.Add(spell);
                    listViewSpells.Items.Add(new ListViewItem(new[]
                    {
                        spell.ID.ToString(),
                        spell.Name,
                        spell.SpellType.ToString(),
                        spell.AvailableToClass.ToString(),
                        spell.MPCostExpression,
                        spell.DamageExpression,
                        spell.AutoHitTarget.ToString(),
                        spell.LearnCost.ToString(),
                        spell.IsAOE.ToString(),
                        spell.ApplyAbilityModifier.ToString(),
                        spell.AppliedBuffs.Count.ToString(),
                        spell.Description,
                    }));
                }
                foreach(ColumnHeader h in listViewSpells.Columns)
                {
                    h.Width = -2;
                }    
            }
        }

        private bool ValidateSpellData()
        {
            if (!int.TryParse(txtBxSpellID.Text, out int spellID) || spellID < 1)
            {
                MessageBox.Show("The Spell ID must be a valid integer with a value greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxSpellName.Text))
            {
                MessageBox.Show("The Spell must have a Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxSpellClasses.Text, true, out ActorClass cls) || cls == ActorClass.Undefined)
            {
                MessageBox.Show("The Spell must be available to one or more in-game player Classes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxSpellType.Text, true, out SpellType spellType) || spellType == SpellType.Undefined)
            {
                MessageBox.Show("The Spell must have a valid Type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if ((spellType == SpellType.Damage || spellType == SpellType.Healing) && string.IsNullOrEmpty(txtBxSpellDamage.Text))
            {
                MessageBox.Show("If the Spell Type is Healing or Damage, a Damage expression must be entered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if ((spellType == SpellType.Buff || spellType == SpellType.Debuff) && listBxSpellBuffs.Items.Count == 0)
            {
                MessageBox.Show("If the Spell Type is Buff or Debuff, one or more entries must be in the Buff list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxSpellLearnCost.Text, out int learnCost) || learnCost < 1)
            {
                MessageBox.Show("The Learn Cost must be a valid integer with a value of 1 or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxSpellDescription.Text))
            {
                MessageBox.Show("The Spell must have a description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private Spell GetSpellFromFormData()
        {
            var newSpell = new Spell();
            try
            {
                newSpell.ID = int.Parse(txtBxSpellID.Text);
                newSpell.Name = txtBxSpellName.Text;
                Enum.TryParse(txtBxSpellClasses.Text, true, out ActorClass actorClass);
                newSpell.AvailableToClass = actorClass;
                Enum.TryParse(txtBxSpellType.Text, true, out SpellType spellType);
                newSpell.SpellType = spellType;
                newSpell.MPCostExpression = txtBxSpellMPCost.Text;
                newSpell.DamageExpression = txtBxSpellDamage.Text;
                newSpell.AutoHitTarget = chkBxSpellAutoHit.Checked;
                newSpell.IsAOE = chkBxSpellAOE.Checked;
                newSpell.ApplyAbilityModifier = chkBxSpellAbilityMod.Checked;
                newSpell.LearnCost = int.Parse(txtBxSpellLearnCost.Text);
                newSpell.Description = txtBxSpellDescription.Text;
                if (listBxSpellBuffs.Items.Count > 0)
                {
                    foreach(var b in listBxSpellBuffs.Items)
                    {
                        newSpell.AppliedBuffs.TryAdd(b.ToString(), true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Spell object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newSpell = null;
            }
            return newSpell;
        }

        private void ClearSpellForm()
        {
            txtBxSpellID.Clear();
            txtBxSpellName.Clear();
            txtBxSpellClasses.Clear();
            txtBxSpellMPCost.Clear();
            txtBxSpellDamage.Clear();
            chkBxSpellAutoHit.Checked = false;
            chkBxSpellAOE.Checked = false;
            chkBxSpellAbilityMod.Checked = false;
            txtBxSpellLearnCost.Clear();
            txtBxSpellDescription.Clear();
            txtBxSpellType.Clear();
            listBxSpellBuffs.Items.Clear();
        }
        #endregion
    }
}