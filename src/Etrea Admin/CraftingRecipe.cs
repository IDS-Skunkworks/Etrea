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
        private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
        private static ListViewItemComparer recipeComparer;

        #region Event Handlers
        private async void listViewRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewRecipes.SelectedItems.Count == 0)
            {
                return;
            }
            var recipe = allRecipes.FirstOrDefault(x => x.ID == int.Parse(listViewRecipes.SelectedItems[0].SubItems[0].Text));
            if (recipe == null)
            {
                return;
            }
            listViewRecipeIngredients.Items.Clear();
            txtBxRecipeID.Text = recipe.ID.ToString();
            txtBxRecipeName.Text = recipe.Name;
            txtBxRecipeType.Text = recipe.RecipeType.ToString();
            txtBxRecipeLearnCost.Text = recipe.LearnCost.ToString();
            txtBxRecipeDescription.Text = recipe.Description;
            var recipeResult = await APIHelper.LoadAssets<InventoryItem>($"/item/{recipe.RecipeResult}", true);
            txtBxRecipeResult.Text = recipeResult == null ? "Invalid Item" : $"{recipeResult.ID}: {recipeResult.Name}";
            if (recipe.RequiredItems.Count > 0)
            {
                foreach(var item in recipe.RequiredItems)
                {
                    var ingredient = await APIHelper.LoadAssets<InventoryItem>($"/item/{item.Key}", true);
                    if (ingredient != null)
                    {
                        listViewRecipeIngredients.Items.Add(new ListViewItem(new[]
                        {
                            ingredient.ID.ToString(),
                            ingredient.Name,
                            item.Value.ToString()
                        }));
                    }
                    else
                    {
                        listViewRecipeIngredients.Items.Add(new ListViewItem(new[]
                        {
                            "0",
                            "Invalid Item",
                            "None"
                        }));
                    }
                }
                foreach (ColumnHeader h in listViewRecipeIngredients.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private void btnLoadRecipes_Click(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        private async void btnAddRecipe_Click(object sender, EventArgs e)
        {
            if (!ValidateRecipeForm())
            {
                return;
            }
            var newRecipe = GetRecipeFromFormData();
            if (newRecipe == null)
            {
                return;
            }
            var recipeJson = Helpers.SerialiseEtreaObject<CraftingRecipe>(newRecipe);
            btnAddRecipe.Enabled = false;
            if (await APIHelper.AddNewAsset("/recipe", recipeJson))
            {
                ClearRecipeForm();
                LoadRecipes();
            }
            btnAddRecipe.Enabled = true;
        }

        private async void btnUpdateRecipe_Click(object sender, EventArgs e)
        {
            if (!ValidateRecipeForm())
            {
                return;
            }
            var newRecipe = GetRecipeFromFormData();
            if (newRecipe == null)
            {
                return;
            }
            var recipeJson = Helpers.SerialiseEtreaObject<CraftingRecipe>(newRecipe);
            btnUpdateRecipe.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/recipe", recipeJson))
            {
                ClearRecipeForm();
                LoadRecipes();
            }
            btnUpdateRecipe.Enabled = true;
        }

        private void btnClearRecipeForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearRecipeForm();
            }
        }

        private async void btnDeleteRecipe_Click(object sender, EventArgs e)
        {
            if (listViewRecipes.SelectedItems.Count == 0)
            {
                return;
            }
            var recipe = allRecipes.FirstOrDefault(x => x.ID == int.Parse(listViewRecipes.SelectedItems[0].SubItems[0].Text));
            if (recipe == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Recipe? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteRecipe.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/recipe/{recipe.ID}"))
                {
                    ClearRecipeForm();
                    LoadRecipes();
                }
                btnDeleteRecipe.Enabled = true;
            }
        }

        private void btnSetRecipeResult_Click(object sender, EventArgs e)
        {
            using (var si = new SelectInventoryItem(ItemType.Misc, false))
            {
                if (si.ShowDialog() == DialogResult.OK)
                {
                    txtBxRecipeResult.Text = $"{si._item.ID}: {si._item.Name}";
                }
            }
        }

        private void btnAddIngredient_Click(object sender, EventArgs e)
        {
            using (var si = new SelectInventoryItem(ItemType.Misc, false))
            {
                if (si.ShowDialog() == DialogResult.OK)
                {
                    var i = si._item;
                    bool foundIngredient = false;
                    foreach(ListViewItem ingredient in listViewRecipeIngredients.Items)
                    {
                        if (int.Parse(ingredient.SubItems[0].Text) == i.ID)
                        {
                            var amount = int.Parse(ingredient.SubItems[2].Text) + 1;
                            ingredient.SubItems[2].Text = amount.ToString();
                            foundIngredient = true;
                            break;
                        }
                    }
                    if (!foundIngredient)
                    {
                        listViewRecipeIngredients.Items.Add(new ListViewItem(new[]
                        {
                            i.ID.ToString(),
                            i.Name,
                            "1"
                        }));
                    }
                    foreach(ColumnHeader h in listViewRecipeIngredients.Columns)
                    {
                        h.Width = -2;
                    }
                }
            }
        }

        private void btnRemoveIngredient_Click(object sender, EventArgs e)
        {
            if (listViewRecipeIngredients.SelectedItems.Count > 0)
            {
                var obj = listViewRecipeIngredients.SelectedItems[0];
                listViewRecipeIngredients.Items.Remove(obj);
            }
        }

        private void btnClearIngredients_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the ingredients for this recipe?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewRecipeIngredients.Items.Clear();
            }
        }

        private void listViewRecipes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (recipeComparer == null)
            {
                recipeComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == recipeComparer.SortColumn)
            {
                recipeComparer.SortOrder = recipeComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                recipeComparer.SortOrder = SortOrder.Ascending;
                recipeComparer.SortColumn = e.Column;
            }
            listViewRecipes.ListViewItemSorter = recipeComparer;
            listViewRecipes.Sort();
        }
        #endregion

        #region Functions
        private async void LoadRecipes()
        {
            listViewRecipes.Items.Clear();
            allRecipes.Clear();
            var result = await APIHelper.LoadAssets<List<CraftingRecipe>>("/recipe", false);
            if (result != null)
            {
                foreach(var item in result.OrderBy(x => x.ID))
                {
                    allRecipes.Add(item);
                    var recipeResult = await APIHelper.LoadAssets<InventoryItem>($"/item/{item.RecipeResult}", true);
                    listViewRecipes.Items.Add(new ListViewItem(new[]
                    {
                        item.ID.ToString(),
                        item.Name,
                        item.RecipeType.ToString(),
                        recipeResult == null ? "Invalid Item" : recipeResult.Name,
                        item.LearnCost.ToString(),
                        item.RequiredItems.Count.ToString(),
                        item.Description,
                    }));
                }
                foreach(ColumnHeader h in listViewRecipes.Columns)
                {
                    h.Width = -2;
                }
            }
        }

        private void ClearRecipeForm()
        {
            txtBxRecipeID.Clear();
            txtBxRecipeName.Clear();
            txtBxRecipeType.Clear();
            txtBxRecipeResult.Clear();
            txtBxRecipeLearnCost.Clear();
            txtBxRecipeDescription.Clear();
            listViewRecipeIngredients.Items.Clear();
        }

        private CraftingRecipe GetRecipeFromFormData()
        {
            CraftingRecipe newRecipe = new CraftingRecipe();
            try
            {
                newRecipe.ID = int.Parse(txtBxRecipeID.Text);
                newRecipe.Name = txtBxRecipeName.Text;
                newRecipe.Description = txtBxRecipeDescription.Text;
                Enum.TryParse(txtBxRecipeType.Text, true, out RecipeType type);
                newRecipe.RecipeType = type;
                int recipeResult = int.Parse(txtBxRecipeResult.Text.Split(':')[0].Trim());
                newRecipe.RecipeResult = recipeResult;
                newRecipe.LearnCost = int.Parse(txtBxRecipeLearnCost.Text);
                foreach(ListViewItem ing in listViewRecipeIngredients.Items)
                {
                    newRecipe.RequiredItems.TryAdd(int.Parse(ing.SubItems[0].Text), int.Parse(ing.SubItems[2].Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Recipe object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newRecipe = null;
            }
            return newRecipe;
        }

        private bool ValidateRecipeForm()
        {
            if (!int.TryParse(txtBxRecipeID.Text, out int recipeID) || recipeID < 1)
            {
                MessageBox.Show("The Recipe ID must be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRecipeName.Text))
            {
                MessageBox.Show("The Recipe must have a name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRecipeDescription.Text))
            {
                MessageBox.Show("The Recipe must have a description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRecipeResult.Text))
            {
                MessageBox.Show("The Recipe must have a result item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtBxRecipeLearnCost.Text, out int learnCost) || learnCost < 1)
            {
                MessageBox.Show("The Recipe must have a learn cost which should be a positive non-zero integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (listViewRecipeIngredients.Items.Count == 0)
            {
                MessageBox.Show("The Recipe must have at least one ingredient.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Enum.TryParse(txtBxRecipeType.Text, true, out RecipeType rType) || rType == RecipeType.Undefined)
            {
                MessageBox.Show("The Recipe type is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        #endregion
    }
}