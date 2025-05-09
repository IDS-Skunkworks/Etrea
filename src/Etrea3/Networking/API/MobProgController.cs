﻿using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class MobProgController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var mobprogs = ScriptObjectManager.Instance.GetScriptObject<MobProg>();
                if (mobprogs == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<MobProg>>(mobprogs));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgController.Get(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
        [Route("api/mobprog/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var mobProg = ScriptObjectManager.Instance.GetScriptObject<MobProg>(id);
                if (mobProg == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<MobProg>(mobProg));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgController.Get(id): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Post() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<MobProg>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Post() sent an NPC which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!ScriptObjectManager.Instance.AddOrUpdateScriptObject<MobProg>(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Post() failed to add a MobProg to MobProg Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the MobProg to MobProg Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new MobProg {n.Name} ({n.ID}) to MobProg Manager", LogLevel.Info);
                return Ok($"MobProg {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<MobProg>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Put() sent a MobProg which failed validation", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentMobProg = ScriptObjectManager.Instance.GetScriptObject<MobProg>(n.ID);
                if (currentMobProg == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Put() sent a MobProg which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update MobProg {n.ID}, it does not currently exist");
                }
                if (currentMobProg.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Put() sent a MobProg which is locked in OLC ({n.ID})", LogLevel.Warning);
                    return BadRequest($"Cannot update MobProg {n.ID} it is locked in OLC");
                }
                if (!ScriptObjectManager.Instance.AddOrUpdateScriptObject<MobProg>(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Put() failed to udpate MobProg {n.Name} ({n.ID}) in MobProg Manager", LogLevel.Warning);
                    return BadRequest("Failed to update MobProg in MobProg Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated MobProg {n.Name} ({n.ID}) in MobProg Manager", LogLevel.Info);
                return Ok($"MobProg {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/mobprog/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = ScriptObjectManager.Instance.GetMobProg(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Delete() requested deletion of MobProg {id} which does not exist", LogLevel.Warning);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Delete() requested deletion of MobProg {id} which is locked in OLC", LogLevel.Warning);
                    return BadRequest($"Cannot delete MobProg {id} it is locked in OLC");
                }
                if (!ScriptObjectManager.Instance.RemoveScriptObject<MobProg>(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to MobProgController.Delete() failed to remove MobProg {id} from MobProg Manager", LogLevel.Warning);
                    return BadRequest($"Failed to remove MobProg {id} from MobProg Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to MobProgController.Delete() removed MobProg {n.Name} ({n.ID}) from MobProg Manager", LogLevel.Info);
                return Ok($"MobProg {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in MobProgController.Delete(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
