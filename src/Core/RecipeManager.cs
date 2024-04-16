using Etrea2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etrea2.Core
{
    internal class RecipeManager
    {
        private static RecipeManager _instance = null;
        private static readonly object _lock = new object();
        private Dictionary<uint, Recipe> Recipes;

        private RecipeManager()
        {
            Recipes = new Dictionary<uint, Recipe>();
        }

        internal static RecipeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RecipeManager();
                }
                return _instance;
            }
        }

        internal bool RecipeExists(uint id)
        {
            return Instance.Recipes.ContainsKey(id);
        }

        internal Recipe GetRecipe(uint id)
        {
            return Instance.Recipes.ContainsKey(id) ? Instance.Recipes[id] : null;
        }

        internal Recipe GetRecipe(string name)
        {
            lock (_lock)
            {
                return Instance.Recipes.Values.Where(x => Regex.IsMatch(x.RecipeName, name, RegexOptions.IgnoreCase)).FirstOrDefault();
            }
        }

        internal List<Recipe> GetAllCraftingRecipes(string criteria)
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    return Instance.Recipes.Values.Where(x => Regex.IsMatch(x.RecipeName, criteria, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(x.RecipeDescription, criteria, RegexOptions.IgnoreCase)).ToList();
                }
                return Instance.Recipes.Values.ToList();
            }
        }

        internal int GetRecipeCount()
        {
            lock(_lock)
            {
                return Instance.Recipes.Count;
            }
        }

        internal void LoadAllRecipes(out bool hasError)
        {
            var result = DatabaseManager.LoadAllRecipes(out hasError);
            if (!hasError && result != null)
            {
                lock(_lock)
                {
                    Instance.Recipes.Clear();
                    Instance.Recipes = result;
                }
            }
        }

        internal bool AddRecipe(ref Descriptor desc, Recipe recipe)
        {
            try
            {
                lock(_lock)
                {
                    Instance.Recipes.Add(recipe.RecipeID, recipe);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} added Recipe {recipe.RecipeID} ({recipe.RecipeName}) to RecipeManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error adding Recipe {recipe.RecipeID} ({recipe.RecipeName}) to RecipeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateRecipe(ref Descriptor desc, Recipe recipe)
        {
            try
            {
                lock(_lock)
                {
                    if (Instance.Recipes.ContainsKey(recipe.RecipeID))
                    {
                        Instance.Recipes.Remove(recipe.RecipeID);
                        Instance.Recipes.Add(recipe.RecipeID, recipe);
                        Game.LogMessage($"OLC: Player {desc.Player.Name} has updated Recipe {recipe.RecipeID} ({recipe.RecipeName}) in RecipeManager", LogLevel.OLC, true);
                        return true;
                    }
                    else
                    {
                        Game.LogMessage($"OLC: Player {desc.Player.Name} has tried to update Recipe {recipe.RecipeID} ({recipe.RecipeName}) in RecipeManager but the ID could not be found, it will be added instead", LogLevel.OLC, true);
                        bool OK = AddRecipe(ref desc, recipe);
                        return OK;
                    }
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error updating Recipe {recipe.RecipeID} ({recipe.RecipeName}) in RecipeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveRecipe(ref Descriptor desc, uint id, string name)
        {
            try
            {
                lock(_lock)
                {
                    Instance.Recipes.Remove(id);
                    Game.LogMessage($"OLC: Player {desc.Player.Name} removed Recipe {id} ({name}) from RecipeManager", LogLevel.OLC, true);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Player {desc.Player.Name} encountered an error removing Recipe {id} ({name}) from RecipeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }
    }
}
