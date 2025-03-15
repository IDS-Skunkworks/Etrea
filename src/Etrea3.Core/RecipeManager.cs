using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Etrea3.Objects;

namespace Etrea3.Core
{
    public class RecipeManager
    {
        private static RecipeManager instance = null;
        private ConcurrentDictionary<int, CraftingRecipe> Recipes { get; set; }
        public int Count => Recipes.Count;

        private RecipeManager()
        {
            Recipes = new ConcurrentDictionary<int, CraftingRecipe>();
        }

        public static RecipeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RecipeManager();
                }
                return instance;
            }
        }

        public CraftingRecipe GetRecipe(int id)
        {
            return Instance.Recipes.ContainsKey(id) ? Instance.Recipes[id] : null;
        }

        public List<CraftingRecipe> GetRecipe(int start, int end)
        {
            return Instance.Recipes.Values.Where(x => x.ID >= start && x.ID <= end).ToList();
        }

        public List<CraftingRecipe> GetRecipe(string criteria)
        {
            return Instance.Recipes.Values.Where(x => x.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public List<CraftingRecipe> GetRecipe()
        {
            return Instance.Recipes.Values.ToList();
        }

        public List<CraftingRecipe> GetRecipe(RecipeType recipeType)
        {
            return Instance.Recipes.Values.Where(x => x.RecipeType == recipeType).ToList();
        }

        public bool RecipeExists(int id)
        {
            return Instance.Recipes.ContainsKey(id);
        }

        public void SetRecipeLockState(int id, bool locked, Session session)
        {
            if (Instance.Recipes.ContainsKey(id))
            {
                Instance.Recipes[id].OLCLocked = locked;
                Instance.Recipes[id].LockHolder = locked ? session.ID : Guid.Empty;
            }
        }

        public bool GetRecipeLockState(int id, out Guid lockHolder)
        {
            lockHolder = Guid.Empty;
            if (Instance.Recipes.ContainsKey(id))
            {
                lockHolder = Instance.Recipes[id].LockHolder;
                return Instance.Recipes[id].OLCLocked;
            }
            return false;
        }

        public bool AddOrUpdateRecipe(CraftingRecipe recipe, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveRecipeToWorldDatabase(recipe, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Recipe {recipe.Name} ({recipe.ID}) to the World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Recipes.TryAdd(recipe.ID, recipe))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Recipe {recipe.Name} ({recipe.ID}) to Recipe Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Recipes.TryGetValue(recipe.ID, out CraftingRecipe existingRecipe))
                    {
                        Game.LogMessage($"ERROR: Recipe {recipe.ID} not found in Recipe Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Recipes.TryUpdate(recipe.ID, recipe, existingRecipe))
                    {
                        Game.LogMessage($"ERROR: Failed to update Recipe {recipe.ID} in Recipe Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RecipeManager.AddOrUpdateRecipe(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveRecipe(int id)
        {
            if (Instance.Recipes.ContainsKey(id))
            {
                return Instance.Recipes.TryRemove(id, out _) && DatabaseManager.RemoveRecipe(id);
            }
            Game.LogMessage($"ERROR: Error removing Recipe with ID {id}: No such Recipe in RecipeManager", LogLevel.Error);
            return false;
        }

        public bool LoadAllRecipes()
        {
            if (!DatabaseManager.LoadAllRecipes(out var recipes) || recipes == null)
            {
                return false;
            }
            foreach (var recipe in recipes)
            {
                Instance.Recipes.AddOrUpdate(recipe.Key, recipe.Value, (k, v) => recipe.Value);
            }
            return true;
        }
    }
}
