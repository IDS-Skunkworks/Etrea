using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class EmoteController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var emote = EmoteManager.Instance.GetEmote();
                if (emote == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Emote>>(emote));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in EmoteController.Get(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/emote/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var e = EmoteManager.Instance.GetEmote(id);
                if (e == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<Emote>(e));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in EmoteController.Get(id): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Post() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Emote>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Post() sent an Emote which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!EmoteManager.Instance.AddOrUpdateEmote(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Post() failed to add an Empte to Emote Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the Emote to Emote Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Emote {n.Name} ({n.ID}) to Emote Manager", LogLevel.Info);
                return Ok($"Emote {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in EmoteController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Emote>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Put() sent an Emote which failed validation", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentEmote = EmoteManager.Instance.GetEmote(n.ID);
                if (currentEmote == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Put() sent an Emote which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update Emote {n.Name}, it does not currently exist");
                }
                if (currentEmote.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Put() sent an Emote which is locked in OLC ({n.Name})", LogLevel.Warning);
                    return BadRequest($"Cannot update Emote {n.Name} it is locked in OLC");
                }
                if (!EmoteManager.Instance.AddOrUpdateEmote(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Put() failed to udpate Emote {n.Name} ({n.ID}) in Emote Manager", LogLevel.Warning);
                    return BadRequest("Failed to update Emote in Emote Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Emote {n.Name} ({n.ID}) in Emote Manager", LogLevel.Info);
                return Ok($"Emote {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in EmoteController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = EmoteManager.Instance.GetEmote(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Delete() requested deletion of Emote {id} which does not exist", LogLevel.Warning);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Delete() requested deletion of Emote {id} which is locked in OLC", LogLevel.Warning);
                    return BadRequest($"Cannot delete Emote {id} it is locked in OLC");
                }
                if (!EmoteManager.Instance.RemoveEmote(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to EmoteController.Delete() failed to remove Emote {id} from Emote Manager", LogLevel.Warning);
                    return BadRequest($"Failed to remove Emote {id} from Emote Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to EmoteController.Delete() removed Emote {n.Name} ({n.ID}) from Emote Manager", LogLevel.Info);
                return Ok($"Emote {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in EmoteController.Delete(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
