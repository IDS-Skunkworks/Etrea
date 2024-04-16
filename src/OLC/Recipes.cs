using Etrea2.Core;
using Etrea2.Entities;
using System.Collections.Generic;
using System.Text;

namespace Etrea2.OLC
{
    internal static partial class OLC
    {
        #region Create
        private static void CreateNewCraftingRecipe(ref Descriptor desc)
        {
            bool okToReturn = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("A Crafting Recipe allows characters to use items they have gathered to create a new item,");
            sb.AppendLine("assuming they have the correct skill to do so.");
            desc.Send(sb.ToString());
            Recipe r = new Recipe
            {
                RequiredMaterials = new Dictionary<uint, uint>()
            };
            while (!okToReturn)
            {
                sb.Clear();
                sb.AppendLine($"Recipe ID: {r.RecipeID}");
                sb.AppendLine($"Recipe Name: {r.RecipeName}");
                sb.AppendLine($"Recipe Type: {r.RecipeType}");
                sb.AppendLine($"Results In: {r.RecipeResult}");
                sb.AppendLine($"Recipe Description: {r.RecipeDescription}");
                sb.AppendLine($"Required Materials:");
                if (r.RequiredMaterials != null && r.RequiredMaterials.Count > 0)
                {
                    foreach (var m in r.RequiredMaterials)
                    {
                        sb.AppendLine($"{ItemManager.Instance.GetItemByID(m.Key).Name} ({m.Value})");
                    }
                }
                else
                {
                    sb.AppendLine("None");
                }
                sb.AppendLine();
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Recipe ID{Constants.TabStop}{Constants.TabStop}2. Set Recipe Name");
                sb.AppendLine($"3. Set Recpe Type{Constants.TabStop}{Constants.TabStop}4. Set Recipe Result");
                sb.AppendLine($"5. Set Recipe Description{Constants.TabStop}6. Add Required Item{Constants.TabStop}7. Remove Required Item");
                sb.AppendLine($"8. Save{Constants.TabStop}9. Exit");
                sb.AppendLine("Selection: ");
                desc.Send(sb.ToString());
                var input = desc.Read().Trim();
                if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint option))
                {
                    if (option >= 1 && option <= 9)
                    {
                        switch (option)
                        {
                            case 1:
                                r.RecipeID = GetAssetUintValue(ref desc, "Enter Recipe ID: ");
                                break;

                            case 2:
                                r.RecipeName = GetAssetStringValue(ref desc, "Enter Recipe Name: ");
                                break;

                            case 3:
                                r.RecipeType = GetAssetEnumValue<RecipeType>(ref desc, "Enter Recipe Type: ");
                                break;

                            case 4:
                                var id = GetAssetUintValue(ref desc, "Enter Recipe Result: ");
                                var item = ItemManager.Instance.GetItemByID(id);
                                if (item != null)
                                {
                                    r.RecipeResult = id;
                                }
                                break;

                            case 5:
                                r.RecipeDescription = GetAssetStringValue(ref desc, "Enter Recipe Description: ");
                                break;

                            case 6:
                                id = GetAssetUintValue(ref desc, "Enter ID of Material to add : ");
                                item = ItemManager.Instance.GetItemByID(id);
                                if (item != null)
                                {
                                    if (r.RequiredMaterials.ContainsKey(item.ID))
                                    {
                                        r.RequiredMaterials[item.ID]++;
                                    }
                                    else
                                    {
                                        r.RequiredMaterials.Add(item.ID, 1);
                                    }
                                }
                                else
                                {
                                    desc.Send($"No Item with that ID could be found.{Constants.NewLine}");
                                }
                                break;

                            case 7:
                                id = GetAssetUintValue(ref desc, "Enter ID of Material to remove : ");
                                item = ItemManager.Instance.GetItemByID(id);
                                if (item != null)
                                {
                                    if (r.RequiredMaterials.ContainsKey(item.ID))
                                    {
                                        if (r.RequiredMaterials[item.ID] - 1 == 0)
                                        {
                                            r.RequiredMaterials.Remove(item.ID);
                                        }
                                        else
                                        {
                                            r.RequiredMaterials[item.ID]--;
                                        }
                                    }
                                }
                                else
                                {
                                    desc.Send($"No item with that ID could be found.{Constants.NewLine}");
                                }
                                break;

                            case 8:
                                if (ValidateRecipe(ref desc, ref r, true))
                                {
                                    if (DatabaseManager.AddNewRecipe(ref desc, ref r))
                                    {
                                        if (RecipeManager.Instance.AddRecipe(ref desc, r))
                                        {
                                            desc.Send($"New Recipe successfully added to RecipeManager and World Database{Constants.NewLine}");
                                            okToReturn = true;
                                        }
                                        else
                                        {
                                            desc.Send($"Failed to add new Recipe to the RecipeManager, it may not be available until restart.{Constants.NewLine}");
                                        }
                                    }
                                    else
                                    {
                                        desc.Send($"Failed to add new Recipe to the World Database{Constants.NewLine}");
                                    }
                                }
                                break;

                            case 9:
                                okToReturn = true;
                                break;
                        }
                    }
                }
                else
                {
                    desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Edit
        private static void EditExistingRecipe(ref Descriptor desc)
        {
            desc.Send($"Enter the ID of the Recipe to edit or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint rid))
            {
                if (RecipeManager.Instance.RecipeExists(rid))
                {
                    var r = RecipeManager.Instance.GetRecipe(rid).ShallowCopy();
                    bool okToReturn = false;
                    StringBuilder sb = new StringBuilder();
                    while (!okToReturn)
                    {
                        sb.Clear();
                        sb.AppendLine($"Recipe ID: {r.RecipeID}");
                        sb.AppendLine($"Recipe Name: {r.RecipeName}");
                        sb.AppendLine($"Recipe Type: {r.RecipeType}");
                        sb.AppendLine($"Results In: {r.RecipeResult}");
                        sb.AppendLine($"Recipe Description: {r.RecipeDescription}");
                        sb.AppendLine($"Required Materials:");
                        if (r.RequiredMaterials != null && r.RequiredMaterials.Count > 0)
                        {
                            foreach (var m in r.RequiredMaterials)
                            {
                                sb.AppendLine($"{ItemManager.Instance.GetItemByID(m.Key).Name} ({m.Value})");
                            }
                        }
                        else
                        {
                            sb.AppendLine("None");
                        }
                        sb.AppendLine();
                        sb.AppendLine("Options:");
                        sb.AppendLine($"1. Set Recipe Name{Constants.TabStop}2. Set Recipe Type");
                        sb.AppendLine($"3. Set Recipe Result{Constants.TabStop}4. Set Recipe Description");
                        sb.AppendLine($"5. Add Required Item{Constants.TabStop}6. Remove Required Item");
                        sb.AppendLine($"7. Save{Constants.TabStop}8. Exit");
                        sb.AppendLine("Selection: ");
                        var choice = desc.Read().Trim();
                        if (Helpers.ValidateInput(choice) && uint.TryParse(choice, out var option))
                        {
                            if (option >= 1 && option <= 8)
                            {
                                switch (option)
                                {
                                    case 1:
                                        r.RecipeName = GetAssetStringValue(ref desc, "Enter Recipe Name: ");
                                        break;

                                    case 2:
                                        r.RecipeType = GetAssetEnumValue<RecipeType>(ref desc, "Enter Recipe Type: ");
                                        break;

                                    case 3:
                                        var id = GetAssetUintValue(ref desc, "Enter Recipe Result: ");
                                        var item = ItemManager.Instance.GetItemByID(id);
                                        if (item != null)
                                        {
                                            r.RecipeResult = id;
                                        }
                                        break;

                                    case 4:
                                        r.RecipeDescription = GetAssetStringValue(ref desc, "Enter Recipe Description: ");
                                        break;

                                    case 5:
                                        id = GetAssetUintValue(ref desc, "Enter ID of Material to add : ");
                                        item = ItemManager.Instance.GetItemByID(id);
                                        if (item != null)
                                        {
                                            if (r.RequiredMaterials.ContainsKey(item.ID))
                                            {
                                                r.RequiredMaterials[item.ID]++;
                                            }
                                            else
                                            {
                                                r.RequiredMaterials.Add(item.ID, 1);
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"No Item with that ID could be found.{Constants.NewLine}");
                                        }
                                        break;

                                    case 6:
                                        id = GetAssetUintValue(ref desc, "Enter ID of Material to remove : ");
                                        item = ItemManager.Instance.GetItemByID(id);
                                        if (item != null)
                                        {
                                            if (r.RequiredMaterials.ContainsKey(item.ID))
                                            {
                                                if (r.RequiredMaterials[item.ID] - 1 == 0)
                                                {
                                                    r.RequiredMaterials.Remove(item.ID);
                                                }
                                                else
                                                {
                                                    r.RequiredMaterials[item.ID]--;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            desc.Send($"No item with that ID could be found.{Constants.NewLine}");
                                        }
                                        break;

                                    case 7:
                                        if (ValidateRecipe(ref desc, ref r, false))
                                        {
                                            if (DatabaseManager.UpdateRecipe(ref desc, ref r))
                                            {
                                                if (RecipeManager.Instance.UpdateRecipe(ref desc, r))
                                                {
                                                    desc.Send($"Updated Recipe {r.RecipeID} in World Database and RecipeManager{Constants.NewLine}");
                                                    okToReturn = true;
                                                }
                                                else
                                                {
                                                    desc.Send($"Failed to update Recipe {r.RecipeID} in RecipeManager, changes may not be available until restart.{Constants.NewLine}");
                                                }
                                            }
                                            else
                                            {
                                                desc.Send($"Failed to update Recipe in the World Database{Constants.NewLine}");
                                            }
                                        }
                                        break;

                                    case 8:
                                        okToReturn = true;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
                        }
                    }
                }
                else
                {
                    desc.Send($"No matching Recipe could be found.{Constants.NewLine}");
                }
            }
        }
        #endregion

        #region Delete
        private static void DeleteCraftingRecipe(ref Descriptor desc)
        {
            desc.Send($"{Constants.RedText}This is a permanent change to the World and cannot be undone unless a database backup is restored!{Constants.PlainText}{Constants.NewLine}");
            desc.Send("Enter the ID of the Recipe to delete or END to return: ");
            var input = desc.Read().Trim();
            if (Helpers.ValidateInput(input) && input == "END")
            {
                return;
            }
            if (Helpers.ValidateInput(input) && uint.TryParse(input, out uint rid))
            {
                var recipe = RecipeManager.Instance.GetRecipe(rid);
                if (recipe != null)
                {
                    if (DatabaseManager.DeleteRecipeByID(ref desc, recipe.RecipeID))
                    {
                        if (RecipeManager.Instance.RemoveRecipe(ref desc, recipe.RecipeID, recipe.RecipeName))
                        {
                            desc.Send($"Recipe successfully removed from RecipeManager and World database.{Constants.NewLine}");
                        }
                        else
                        {
                            desc.Send($"Unable to remove Recipe from RecipeManager.{Constants.NewLine}");
                        }
                    }
                    else
                    {
                        desc.Send($"Unable to remove Recipe from World Database.{Constants.NewLine}");
                    }
                }
                else
                {
                    desc.Send($"Unable to find a Recipe with that name or ID.{Constants.NewLine}");
                }
            }
            else
            {
                desc.Send($"{Constants.DidntUnderstand}{Constants.NewLine}");
            }
        }
        #endregion

        #region Validation Functions
        private static bool ValidateRecipe(ref Descriptor desc, ref Recipe recipe, bool isNewRecipe)
        {
            if (string.IsNullOrEmpty(recipe.RecipeName) || string.IsNullOrEmpty(recipe.RecipeDescription))
            {
                desc.Send($"A name and description are required.{Constants.NewLine}");
                return false;
            }
            if (recipe.RecipeID == 0)
            {
                desc.Send($"A valid ID is required for a new recipe.{Constants.NewLine}");
                return false;
            }
            if (isNewRecipe && DatabaseManager.IsRecipeIDInUse(ref desc, recipe.RecipeID))
            {
                desc.Send($"The specified Recipe ID is already in use.{Constants.NewLine}");
                return false;
            }
            if (recipe.RequiredMaterials == null || recipe.RequiredMaterials.Count == 0)
            {
                desc.Send($"At least one material is required for a recipe.{Constants.NewLine}");
                return false;
            }
            if (recipe.RecipeResult == 0)
            {
                desc.Send($"A recipe must result in a valid item.{Constants.NewLine}");
                return false;
            }
            return true;
        }
        #endregion
    }
}