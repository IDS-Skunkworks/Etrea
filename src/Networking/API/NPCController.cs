using System;
using Etrea3.Core;
using Etrea3.Objects;
using System.Web.Http;
using static Etrea3.OLC.OLC;
using System.Collections.Generic;

namespace Etrea3.Networking.API
{
    public class NPCController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var npcs = NPCManager.Instance.GetNPC();
                if (npcs == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<NPC>>(npcs));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NPCController.Get(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/npc/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var n = NPCManager.Instance.GetNPC(id);
                if (n == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<NPC>(n));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NPCController.Get(id): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Post() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<NPC>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Post() sent an NPC which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!NPCManager.Instance.AddOrUpdateNPCTemplate(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Post() failed to add an NPC to NPC Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the NPC to NPC Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new NPC {n.Name} ({n.TemplateID}) to NPC Manager", LogLevel.Info);
                return Ok($"NPC {n.TemplateID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NPCController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<NPC>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Put() sent an NPC which failed validation", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentNPC = NPCManager.Instance.GetNPC(n.TemplateID);
                if (currentNPC == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Put() sent an NPC which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update NPC {n.TemplateID}, it does not currently exist");
                }
                if (currentNPC.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Put() sent an NPC which is locked in OLC ({n.TemplateID})", LogLevel.Warning);
                    return BadRequest($"Cannot update NPC {n.TemplateID} it is locked in OLC");
                }
                if (!NPCManager.Instance.AddOrUpdateNPCTemplate(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Put() failed to udpate NPC {n.Name} ({n.TemplateID}) in NPC Manager", LogLevel.Warning);
                    return BadRequest("Failed to update NPC in NPC Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated NPC {n.Name} ({n.TemplateID}) in NPC Manager", LogLevel.Info);
                return Ok($"NPC {n.TemplateID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NPCController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = NPCManager.Instance.GetNPC(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Delete() requested deletion of NPC {id} which does not exist", LogLevel.Warning);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Delete() requested deletion of NPC {id} which is locked in OLC", LogLevel.Warning);
                    return BadRequest($"Cannot delete NPC {id} it is locked in OLC");
                }
                if (!NPCManager.Instance.RemoveNPCTemplate(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NPCController.Delete() failed to remove NPC {id} from NPC Manager", LogLevel.Warning);
                    return BadRequest($"Failed to remove NPC {id} from NPC Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to NPCController.Delete() removed NPC {n.Name} ({n.TemplateID}) from NPC Manager", LogLevel.Info);
                return Ok($"NPC {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NPCController.Delete(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
