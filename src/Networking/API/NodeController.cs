using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class NodeController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var nodes = NodeManager.Instance.GetNode();
                if (nodes == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<ResourceNode>>(nodes));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NodeController.Get(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Post() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<ResourceNode>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Post() sent a Resource Node which failed validation: {repl}", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                if (!NodeManager.Instance.AddOrUpdateNode(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Post() failed to add a Resource Node to Node Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to add the Quest to Node Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Resource Node {n.Name} ({n.ID}) to Node Manager", LogLevel.Info, true);
                return Ok($"Node {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NodeController.Post(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Put() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<ResourceNode>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Put() sent a Resource Node which failed validation", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                var currentNode = NodeManager.Instance.GetNode(n.ID);
                if (currentNode == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Put() sent a Resource Node which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Node {n.ID}, it does not currently exist");
                }
                if (currentNode.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Put() sent a Resource Node which is locked in OLC ({n.ID})", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Node {n.ID} it is locked in OLC");
                }
                if (!NodeManager.Instance.AddOrUpdateNode(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Put() failed to udpate Node {n.Name} ({n.ID}) in Node Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to update Node in Node Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Node {n.Name} ({n.ID}) in Node Manager", LogLevel.Info, true);
                return Ok($"Node {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NodeController.Put(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = NodeManager.Instance.GetNode(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Delete() requested deletion of Resource Node {id} which does not exist", LogLevel.Warning, true);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Delete() requested deletion of Resource Node {id} which is locked in OLC", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Node {id} it is locked in OLC");
                }
                if (!NodeManager.Instance.RemoveNode(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to NodeController.Delete() failed to remove Node {id} from Node Manager", LogLevel.Warning, true);
                    return BadRequest($"Failed to remove Node {id} from Node Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to NodeController.Delete() removed Node {n.Name} ({n.ID}) from Node Manager", LogLevel.Info, true);
                return Ok($"Node {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in NodeController.Delete(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }
    }
}
