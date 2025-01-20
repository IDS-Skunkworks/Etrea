using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class SpellController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var Spells = SpellManager.Instance.GetSpell();
                if (Spells == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Spell>>(Spells));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SpellController.Get(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Post() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Spell>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Post() sent a Spell which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!SpellManager.Instance.AddOrUpdateSpell(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Post() failed to add a Spell to Spell Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the Spell to Spell Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Spell {n.Name} ({n.ID}) to Spell Manager", LogLevel.Info);
                return Ok($"Spell {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SpellController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Spell>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Put() sent a Spell which failed validation", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentSpell = SpellManager.Instance.GetSpell(n.ID);
                if (currentSpell == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Put() sent a Spell which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update Spell {n.ID}, it does not currently exist");
                }
                if (currentSpell.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Put() sent a Spell which is locked in OLC ({n.ID})", LogLevel.Warning);
                    return BadRequest($"Cannot update Spell {n.ID} it is locked in OLC");
                }
                if (!SpellManager.Instance.AddOrUpdateSpell(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Put() failed to udpate Spell {n.Name} ({n.ID}) in Spell Manager", LogLevel.Warning);
                    return BadRequest("Failed to update Spell in Spell Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Spell {n.Name} ({n.ID}) in Spell Manager", LogLevel.Info);
                return Ok($"Spell {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SpellController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = SpellManager.Instance.GetSpell(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Delete() requested deletion of Spell {id} which does not exist", LogLevel.Warning);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Delete() requested deletion of Spell {id} which is locked in OLC", LogLevel.Warning);
                    return BadRequest($"Cannot delete Spell {id} it is locked in OLC");
                }
                if (!SpellManager.Instance.RemoveSpell(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to SpellController.Delete() failed to remove Spell {id} from Spell Manager", LogLevel.Warning);
                    return BadRequest($"Failed to remove Spell {id} from Spell Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to SpellController.Delete() removed Spell {n.Name} ({n.ID}) from Spell Manager", LogLevel.Info);
                return Ok($"Spell {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SpellController.Delete(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
