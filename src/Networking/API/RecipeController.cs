using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class RecipeController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var recipes = RecipeManager.Instance.GetRecipe();
                if (recipes == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<CraftingRecipe>>(recipes));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RecipeController.Get(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        public IHttpActionResult Post()
        {
            try
            {
                var payload = Request.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(payload))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Post() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<CraftingRecipe>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Post() sent a Recipe which failed validation: {repl}", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                if (!RecipeManager.Instance.AddOrUpdateRecipe(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Post() failed to add a Recipe to Recipe Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to add the Recipe to Recipe Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Recipe {n.Name} ({n.ID}) to Recipe Manager", LogLevel.Info, true);
                return Ok($"Recipe {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RecipeController.Post(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        public IHttpActionResult Put()
        {
            try
            {
                var payload = Request.Content?.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(payload))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Put() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<CraftingRecipe>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Put() sent a Recipe which failed validation", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                var currentRecipe = RecipeManager.Instance.GetRecipe(n.ID);
                if (currentRecipe == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Put() sent a Recipe which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Recipe {n.ID}, it does not currently exist");
                }
                if (currentRecipe.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Put() sent a Recipe which is locked in OLC ({n.ID})", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Recipe {n.ID} it is locked in OLC");
                }
                if (!RecipeManager.Instance.AddOrUpdateRecipe(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Put() failed to udpate Recipe {n.Name} ({n.ID}) in Recipe Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to update Recipe in Recipe Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Recipe {n.Name} ({n.ID}) in Recipe Manager", LogLevel.Info, true);
                return Ok($"Recipe {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RecipeController.Put(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        [Route("api/recipe/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = RecipeManager.Instance.GetRecipe(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Delete() requested deletion of Recipe {id} which does not exist", LogLevel.Warning, true);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Delete() requested deletion of Recipe {id} which is locked in OLC", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Recipe {id} it is locked in OLC");
                }
                if (!RecipeManager.Instance.RemoveRecipe(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RecipeController.Delete() failed to remove Recipe {id} from Recipe Manager", LogLevel.Warning, true);
                    return BadRequest($"Failed to remove Recipe {id} from Recipe Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to RecipeController.Delete() removed Recipe {n.Name} ({n.ID}) from Recipe Manager", LogLevel.Info, true);
                return Ok($"Recipe {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RecipeController.Delete(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }
    }
}
