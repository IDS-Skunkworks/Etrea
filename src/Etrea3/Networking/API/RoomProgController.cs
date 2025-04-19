using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    internal class RoomProgController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var RoomProgs = ScriptObjectManager.Instance.GetScriptObject<RoomProg>();
                if (RoomProgs == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<RoomProg>>(RoomProgs));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomProgController.Get(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
        [Route("api/RoomProg/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var RoomProg = ScriptObjectManager.Instance.GetScriptObject<RoomProg>(id);
                if (RoomProg == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<RoomProg>(RoomProg));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomProgController.Get(id): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Post() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<RoomProg>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Post() sent an NPC which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!ScriptObjectManager.Instance.AddOrUpdateScriptObject<RoomProg>(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Post() failed to add a RoomProg to RoomProg Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the RoomProg to RoomProg Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new RoomProg {n.Name} ({n.ID}) to RoomProg Manager", LogLevel.Info);
                return Ok($"RoomProg {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomProgController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<RoomProg>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Put() sent a RoomProg which failed validation", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentRoomProg = ScriptObjectManager.Instance.GetScriptObject<RoomProg>(n.ID);
                if (currentRoomProg == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Put() sent a RoomProg which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update RoomProg {n.ID}, it does not currently exist");
                }
                if (currentRoomProg.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Put() sent a RoomProg which is locked in OLC ({n.ID})", LogLevel.Warning);
                    return BadRequest($"Cannot update RoomProg {n.ID} it is locked in OLC");
                }
                if (!ScriptObjectManager.Instance.AddOrUpdateScriptObject<RoomProg>(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Put() failed to udpate RoomProg {n.Name} ({n.ID}) in RoomProg Manager", LogLevel.Warning);
                    return BadRequest("Failed to update RoomProg in RoomProg Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated RoomProg {n.Name} ({n.ID}) in RoomProg Manager", LogLevel.Info);
                return Ok($"RoomProg {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomProgController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/RoomProg/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = ScriptObjectManager.Instance.GetScriptObject<RoomProg>(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Delete() requested deletion of RoomProg {id} which does not exist", LogLevel.Warning);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Delete() requested deletion of RoomProg {id} which is locked in OLC", LogLevel.Warning);
                    return BadRequest($"Cannot delete RoomProg {id} it is locked in OLC");
                }
                if (!ScriptObjectManager.Instance.RemoveScriptObject<RoomProg>(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomProgController.Delete() failed to remove RoomProg {id} from RoomProg Manager", LogLevel.Warning);
                    return BadRequest($"Failed to remove RoomProg {id} from RoomProg Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to RoomProgController.Delete() removed RoomProg {n.Name} ({n.ID}) from RoomProg Manager", LogLevel.Info);
                return Ok($"RoomProg {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomProgController.Delete(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
