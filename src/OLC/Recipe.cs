using Etrea3.Core;
using System.Text;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateRecipe(Session session)
        {
            var newRecipe = new CraftingRecipe();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Recipe ID: {newRecipe.ID}{Constants.TabStop}{Constants.TabStop}Recipe Name: {newRecipe.Name}");
                sb.AppendLine($"Description: {newRecipe.Description}");
                sb.AppendLine($"Recipe Type: {newRecipe.RecipeType}");
                sb.AppendLine($"Learn Cost: {newRecipe.LearnCost:N0}");
                if (newRecipe.RequiredItems.Count > 0)
                {
                    sb.AppendLine("Required Items:");
                    foreach(var i in newRecipe.RequiredItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Items: None");
                }
                if (newRecipe.RecipeResult != 0)
                {
                    var craftingResult = ItemManager.Instance.GetItem(newRecipe.RecipeResult);
                    if (craftingResult != null)
                    {
                        sb.AppendLine($"Produces: {craftingResult.Name} ({craftingResult.ID})");
                    }
                    else
                    {
                        sb.AppendLine($"Produces: Unknown Item ({newRecipe.RecipeResult})");
                    }
                }
                else
                {
                    sb.AppendLine("Produces: Nothing");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set ID{Constants.TabStop}{Constants.TabStop}2. Set Name");
                sb.AppendLine($"3. Set Type{Constants.TabStop}{Constants.TabStop}4. Set Result{Constants.TabStop}5. Set Learn Cost");
                sb.AppendLine($"6. Set Description{Constants.TabStop}7. Manage Required Items");
                sb.AppendLine($"8. Save{Constants.TabStop}{Constants.TabStop}{Constants.TabStop}9. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch(option)
                {
                    case 1:
                        newRecipe.ID = GetValue<int>(session, "Enter Recipe ID: ");
                        break;

                    case 2:
                        newRecipe.Name = GetValue<string>(session, "Enter Recipe Name: ");
                        break;

                    case 3:
                        newRecipe.RecipeType = GetEnumValue<RecipeType>(session, "Enter Recipe Type: ");
                        break;

                    case 4:
                        var itemID = GetValue<int>(session, "Enter Result Item ID: ");
                        if (ItemManager.Instance.ItemExists(itemID))
                        {
                            newRecipe.RecipeResult = itemID;
                        }
                        else
                        {
                            session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                            newRecipe.RecipeResult = 0;
                        }
                        break;

                    case 5:
                        newRecipe.LearnCost = GetValue<int>(session, "Enter Learn Cost: ");
                        break;

                    case 6:
                        newRecipe.Description = GetValue<string>(session, "Enter Recipe Description: ");
                        break;

                    case 7:
                        ManageRecipeIngredients(session, ref newRecipe);
                        break;

                    case 8:
                        if (ValidateAsset(session, newRecipe, true, out _))
                        {
                            if (RecipeManager.Instance.AddOrUpdateRecipe(newRecipe, true))
                            {
                                session.Send($"%BGT%The new Crafting Recipe has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has created new Crafting Recipe: {newRecipe.Name} ({newRecipe.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%The new Crafting Recipe could not be saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to save new Crafting Recipe {newRecipe.Name} ({newRecipe.ID}) but the attempt failed.", LogLevel.OLC, true);
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The new Recipe failed validation and cannot be saved.%PT%{Constants.NewLine}");
                            continue;
                        }
                        break;

                    case 9:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void ChangeRecipe(Session session)
        {
            session.Send("Enter Recipe ID or END to return: ");
            var input = session.Read();
            if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
            {
                return;
            }
            if (!int.TryParse(input.Trim(), out int recipeID))
            {
                session.Send($"%BRT%That is not a valid Recipe ID.{Constants.NewLine}");
                return;
            }
            if (!RecipeManager.Instance.RecipeExists(recipeID))
            {
                session.Send($"%BRT%That is not a valid Recipe ID.{Constants.NewLine}");
                return;
            }
            var recipe = RecipeManager.Instance.GetRecipe(recipeID);
            if (recipe.OLCLocked)
            {
                var lockingSession = SessionManager.Instance.GetSession(recipe.LockHolder);
                var msg = lockingSession != null ? $"%BRT%The specified Recipe is locked in OLC by {lockingSession.Player.Name}.%PT%{Constants.NewLine}" :
                    $"%BRT%The specified Recipe is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                session.Send(msg);
                return;
            }
            var updateRecipe = Helpers.Clone(RecipeManager.Instance.GetRecipe(recipeID));
            RecipeManager.Instance.SetRecipeLockState(recipeID, true, session);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                sb.AppendLine($"Recipe ID: {updateRecipe.ID}{Constants.TabStop}{Constants.TabStop}Recipe Name: {updateRecipe.Name}");
                sb.AppendLine($"Description: {updateRecipe.Description}");
                sb.AppendLine($"Recipe Type: {updateRecipe.RecipeType}");
                sb.AppendLine($"Learn Cost: {updateRecipe.LearnCost:N0}");
                if (updateRecipe.RequiredItems.Count > 0)
                {
                    sb.AppendLine("Required Items:");
                    foreach (var i in updateRecipe.RequiredItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Items: None");
                }
                if (updateRecipe.RecipeResult != 0)
                {
                    var craftingResult = ItemManager.Instance.GetItem(updateRecipe.RecipeResult);
                    if (craftingResult != null)
                    {
                        sb.AppendLine($"Produces: {craftingResult.Name} ({craftingResult.ID})");
                    }
                    else
                    {
                        sb.AppendLine($"Produces: Unknown Item ({updateRecipe.RecipeResult})");
                    }
                }
                else
                {
                    sb.AppendLine("Produces: Nothing");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Set Name");
                sb.AppendLine($"2. Set Type{Constants.TabStop}{Constants.TabStop}3. Set Result");
                sb.AppendLine($"4. Set Description{Constants.TabStop}5. Manage Required Items{Constants.TabStop}6. Set Learn Cost");
                sb.AppendLine($"7. Save{Constants.TabStop}{Constants.TabStop}8. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        updateRecipe.Name = GetValue<string>(session, "Enter Recipe Name: ");
                        break;

                    case 2:
                        updateRecipe.RecipeType = GetEnumValue<RecipeType>(session, "Enter Recipe Type: ");
                        break;

                    case 3:
                        var itemID = GetValue<int>(session, "Enter Result Item ID: ");
                        if (ItemManager.Instance.ItemExists(itemID))
                        {
                            updateRecipe.RecipeResult = itemID;
                        }
                        else
                        {
                            session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.NewLine}");
                            updateRecipe.RecipeResult = 0;
                        }
                        break;

                    case 4:
                        updateRecipe.Description = GetValue<string>(session, "Enter Recipe Description: ");
                        break;

                    case 5:
                        ManageRecipeIngredients(session, ref updateRecipe);
                        break;

                    case 6:
                        updateRecipe.LearnCost = GetValue<int>(session, "Enter Learn Cost: ");
                        break;

                    case 7:
                        if (ValidateAsset(session, updateRecipe, false, out _))
                        {
                            if (RecipeManager.Instance.AddOrUpdateRecipe(updateRecipe, false))
                            {
                                RecipeManager.Instance.SetRecipeLockState(updateRecipe.ID, false, session);
                                session.Send($"%BGT%The updated Crafting Recipe has been saved successfully.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} has updated Crafting Recipe: {updateRecipe.Name} ({updateRecipe.ID})", LogLevel.OLC, true);
                                return;
                            }
                            else
                            {
                                session.Send($"%BRT%The updated Crafting Recipe could not be saved.%PT%{Constants.NewLine}");
                                Game.LogMessage($"OLC: Player {session.Player.Name} attempted to save updated Crafting Recipe {updateRecipe.Name} ({updateRecipe.ID}) but the attempt failed.", LogLevel.OLC, true);
                            }
                        }
                        else
                        {
                            session.Send($"%BRT%The updated Recipe failed validation and cannot be saved.%PT%{Constants.NewLine}");
                            continue;
                        }
                        break;

                    case 8:
                        RecipeManager.Instance.SetRecipeLockState(updateRecipe.ID, false, session);
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }

        private static void DeleteRecipe(Session session)
        {
            while (true)
            {
                session.Send($"Enter Recipe ID or END to return: ");
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || input.Trim().ToUpper() == "END")
                {
                    return;
                }
                if (!int.TryParse(input.Trim(), out int value))
                {
                    session.Send($"%BRT%That is not a valid Recipe ID.%PT%{Constants.NewLine}");
                    continue;
                }
                var recipe = RecipeManager.Instance.GetRecipe(value);
                if (recipe == null)
                {
                    session.Send($"%BRT%No Recipe with that ID could be found in Recipe Manager.%PT%{Constants.NewLine}");
                    continue;
                }
                if (recipe.OLCLocked)
                {
                    var lockHolder = SessionManager.Instance.GetSession(recipe.LockHolder);
                    var msg = lockHolder != null ? $"%BRT%The specified Recipe is locked in OLC by {lockHolder.Player.Name}.%PT%{Constants.NewLine}" :
                        $"%BRT%The specified Recipe is locked in OLC but the locking session could not be found.%PT%{Constants.NewLine}";
                    session.Send(msg);
                    continue;
                }
                if (RecipeManager.Instance.RemoveRecipe(recipe.ID))
                {
                    session.Send($"%BGT%The specified Recipe has been successfully removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} has removed Crafting Recipe {recipe.ID} ({recipe.Name})", LogLevel.OLC, true);
                    return;
                }
                else
                {
                    session.Send($"%BRT%The specified Recipe could not be removed.%PT%{Constants.NewLine}");
                    Game.LogMessage($"OLC: Player {session.Player.Name} attempted to remove Crafting Recipe {recipe.ID} ({recipe.Name}) however the attempt failed", LogLevel.OLC, true);
                    continue;
                }
            }
        }

        private static void ManageRecipeIngredients(Session session, ref CraftingRecipe recipe)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                sb.Clear();
                if (recipe.RequiredItems.Count > 0)
                {
                    sb.AppendLine($"Required Items:");
                    foreach(var i in recipe.RequiredItems)
                    {
                        var item = ItemManager.Instance.GetItem(i.Key);
                        if (item != null)
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x {item.Name} ({item.ID})");
                        }
                        else
                        {
                            sb.AppendLine($"{Constants.TabStop}{i.Value} x Unknown Item ({i.Key})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Required Items: None");
                }
                sb.AppendLine("Options:");
                sb.AppendLine($"1. Add Item{Constants.TabStop}{Constants.TabStop}2. Remove Item");
                sb.AppendLine($"3. Clear Items{Constants.TabStop}{Constants.TabStop}4. Return");
                sb.AppendLine("Choice: ");
                session.Send(sb.ToString());
                var input = session.Read();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input.Trim(), out int option))
                {
                    session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                    continue;
                }
                switch (option)
                {
                    case 1:
                        var itemID = GetValue<int>(session, "Enter Item ID: ");
                        InventoryItem item = ItemManager.Instance.GetItem(itemID);
                        if (item != null)
                        {
                            recipe.RequiredItems.AddOrUpdate(item.ID, 1, (k, v) => v + 1);
                        }
                        else
                        {
                            session.Send($"%BRT%No Item with that ID could be found in Item Manager.%PT%{Constants.TabStop}");
                        }
                        break;

                    case 2:
                        itemID = GetValue<int>(session, "Enter Item ID: ");
                        if (recipe.RequiredItems.ContainsKey(itemID))
                        {
                            if (recipe.RequiredItems[itemID] - 1 == 0)
                            {
                                recipe.RequiredItems.TryRemove(itemID, out _);
                            }
                            else
                            {
                                recipe.RequiredItems[itemID]--;
                            }
                        }
                        break;

                    case 3:
                        recipe.RequiredItems.Clear();
                        break;

                    case 4:
                        return;

                    default:
                        session.Send($"%BRT%That does not look like a valid option...%PT%{Constants.NewLine}");
                        continue;
                }
            }
        }
    }
}