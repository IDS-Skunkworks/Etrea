using System;
using Etrea3.Objects;
using System.Collections.Generic;
using System.Web.Http;
using Etrea3.Core;
using System.Linq;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class ZoneController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                List<Zone> allZones = ZoneManager.Instance.GetZone();
                if (allZones == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Zone>>(allZones));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ZoneController.Get(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Post() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var z = Helpers.DeserialiseEtreaObject<Zone>(payload);
                if (!ValidateAsset(null, z, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Post() sent a Zone which failed validation: {repl}", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                if (!ZoneManager.Instance.AddOrUpdateZone(z, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Post() failed to add a Zone to Zone Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to add the Zone to Zone Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Zone {z.ZoneName} ({z.ZoneID}) to Zone Manager", LogLevel.Info, true);
                return Ok($"Zone {z.ZoneID} ({z.ZoneName}) created successfully");
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in ZoneController.Post(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        public IHttpActionResult Put()
        {
            try
            {
                var payload = Request.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(payload))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Put() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var z = Helpers.DeserialiseEtreaObject<Zone>(payload);
                if (!ValidateAsset(null, z, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Put() sent a Zone which failed validation: {repl}", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                var currentZone = ZoneManager.Instance.GetZone(z.ZoneID);
                if (currentZone == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Put() sent a Zone which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Zone {z.ZoneID}, it does not currently exist");
                }
                if (currentZone.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Put() sent a Zone whcih was locked in OLC ({z.ZoneID})", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Zone {z.ZoneID}, it is locked in OLC");
                }
                if (!ZoneManager.Instance.AddOrUpdateZone(z, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Put() failed to update Zone {z.ZoneName} ({z.ZoneID}) in Zone Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to update Zone in Zone Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Zone {z.ZoneName} ({z.ZoneID}) in Zone Manager", LogLevel.Info, true);
                return Ok($"Zone {z.ZoneID} ({z.ZoneName}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in ZoneController.Put(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        public IHttpActionResult Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Delete() requested deletion of Zone 0 which is not allowed", LogLevel.Warning, true);
                    return BadRequest("Zone 0 cannot be deleted");
                }
                var zone = ZoneManager.Instance.GetZone(id);
                if (zone == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Delete() requested to delete Zone {id} which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Zone {id}, no such Zone in Zone Manager");
                }
                if (RoomManager.Instance.GetRoom().Any(x => x.ZoneID == zone.ZoneID))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Delete() attempted to remove Zone {zone.ZoneName} ({zone.ZoneID}) which still has Rooms", LogLevel.Warning, true);
                    return BadRequest($"Cannot remove Zone {zone.ZoneID}, the Zone still has Rooms");
                }
                if (zone.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Delete() requested deletion of Zone {id} which is locked in OLC", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Zone {id}, it is locked in OLC");
                }
                if (!ZoneManager.Instance.RemoveZone(zone.ZoneID))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ZoneController.Delete() failed to remove Zone {zone.ZoneName} ({zone.ZoneID}) from Zone Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to delete the Zone from Zone Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} removed Zone {zone.ZoneName} ({zone.ZoneID}) from Zone Manager", LogLevel.Info, true);
                return Ok($"Zone {zone.ZoneID} ({zone.ZoneName}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connecton from {Request.GetClientIPAddress()} encountered an error in ZoneController.Delete(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }
    }
}
