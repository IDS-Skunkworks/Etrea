using Kingdoms_of_Etrea.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kingdoms_of_Etrea.Core
{
    internal class RecipeManager
    {
        private static readonly object _lockObject = new object();
        private static RecipeManager _instance = null;
        private Dictionary<uint, Crafting.Recipe> _recipes;

        private RecipeManager()
        {
            _recipes = new Dictionary<uint, Crafting.Recipe>();
        }

        internal static RecipeManager Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new RecipeManager();
                    }
                    return _instance;
                }
            }
        }

        internal bool AddRecipe(Crafting.Recipe recipe)
        {
            try
            {
                if (recipe != null)
                {
                    lock (_lockObject)
                    {
                        _recipes.Add(recipe.RecipeID, recipe);
                    }
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error adding Crafting Recipe '{recipe.RecipeName}' ({recipe.RecipeID}): {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool UpdateRecipe(ref Descriptor desc, Crafting.Recipe r)
        {
            try
            {
                if(Instance._recipes.ContainsKey(r.RecipeID))
                {
                    lock(_lockObject)
                    {
                        Instance._recipes.Remove(r.RecipeID);
                        Instance._recipes.Add(r.RecipeID, r);
                        Game.LogMessage($"INFO: Player {desc.Player} updated Recipe '{r.RecipeName}' ({r.RecipeID}) in RecipeManager", LogLevel.Info, true);
                    }
                    return true;
                }
                else
                {
                    Game.LogMessage($"WARN: RecipeManager does not contain a Recipe with ID {r.RecipeID}", LogLevel.Warning, true);
                    lock(_lockObject)
                    {
                        Instance._recipes.Add(r.RecipeID , r);
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error updating Recipe '{r.RecipeName}' ({r.RecipeID}) in the RecipeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal bool RemoveRecipe(uint id, ref Descriptor desc)
        {
            try
            {
                if(Instance._recipes.ContainsKey(id))
                {
                    lock(_lockObject)
                    {
                        _recipes.Remove(id);
                    }
                    Game.LogMessage($"INFO: Player {desc.Player} removed Recipe with ID {id} from RecipeManager", LogLevel.Info, true);
                    return true;
                }
                Game.LogMessage($"WARN: Player {desc.Player} was unable to remove Recipe with ID {id} from RecipeManager, the ID does not exist", LogLevel.Warning, true);
                return false;
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error removing Recipe with ID {id} from RecipeManager: {ex.Message}", LogLevel.Error, true);
                return false;
            }
        }

        internal Crafting.Recipe GetRecipe(uint id)
        {
            if(_recipes != null && _recipes.Count > 0)
            {
                if(_recipes.ContainsKey(id))
                {
                    return _recipes[id];
                }
            }
            return null;
        }

        internal Crafting.Recipe GetRecipe(string name)
        {
            return (from Crafting.Recipe r in _recipes.Values where Regex.Match(r.RecipeName, name, RegexOptions.IgnoreCase).Success select r).FirstOrDefault();
        }

        internal List<Crafting.Recipe> GetRecipeByNameOrDescription(string n)
        {
            return (from Crafting.Recipe r in _recipes.Values
                    where Regex.Match(r.RecipeName, n, RegexOptions.IgnoreCase).Success ||
                    Regex.Match(r.RecipeDescription, n, RegexOptions.IgnoreCase).Success
                    select r).ToList();
        }

        internal void LoadAllCraftingRecipes(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllCraftingRecipes(out hasErr);
            if(!hasErr && result != null && result.Count > 0)
            {
                Instance._recipes.Clear();
                Instance._recipes = result;
            }
        }

        internal List<Crafting.Recipe> GetAllCraftingRecipes(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var result = from r in _recipes.Values where Regex.Match(r.RecipeName, name, RegexOptions.IgnoreCase).Success ||
                             Regex.Match(r.RecipeDescription, name, RegexOptions.IgnoreCase).Success select r;
                return result.ToList();
            }
            return _recipes.Values.ToList();
        }

        internal int GetRecipeCount()
        {
            return _recipes.Count;
        }
    }
}
