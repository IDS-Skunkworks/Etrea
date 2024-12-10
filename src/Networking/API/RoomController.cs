using System.Collections.Generic;
using System;
using System.Web.Http;
using Etrea3.Core;
using Etrea3.Objects;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class RoomController : ApiController
    {
        [Route("api/room/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var r = RoomManager.Instance.GetRoom(id);
                if (r == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<Room>(r));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomController.Get(id): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        public IHttpActionResult Get()
        {
            try
            {
                List<Room> allRooms = RoomManager.Instance.GetRoom();
                if (allRooms == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Room>>(allRooms));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in RoomController.Get(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Post() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var r = Helpers.DeserialiseEtreaObject<Room>(payload);
                if (!ValidateAsset(null, r, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Post() sent a Room which failed validation: {repl}", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                if (!RoomManager.Instance.AddOrUpdateRoom(r, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Post() failed to add a Room to Room Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to add the Room to Room Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Room {r.RoomName} ({r.ID}) to Room Manager", LogLevel.Info, true);
                return Ok($"Room {r.ID} ({r.RoomName}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in RoomController.Post(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Put() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var r = Helpers.DeserialiseEtreaObject<Room>(payload);
                if (!ValidateAsset(null, r, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Put() sent a Room which failed validation", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                var currentRoom = RoomManager.Instance.GetRoom(r.ID);
                if (currentRoom == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Put() sent a Room which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Room {r.ID}, it does not currently exist");
                }
                if (currentRoom.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Put() sent a Room which is locked in OLC ({r.ID})", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Room {r.ID} it is locked in OLC");
                }
                if (!RoomManager.Instance.AddOrUpdateRoom(r, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Put() failed to udpate Room {r.RoomName} ({r.ID}) in Room Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to update Room in Room Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Room {r.RoomName} ({r.ID}) in Room Manager", LogLevel.Info, true);
                return Ok($"Room {r.ID} ({r.RoomName}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in RoomController.Put(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        [Route("api/room/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Delete() requested deletion of Room 0 which is not allowed", LogLevel.Warning, true);
                    return BadRequest("Room 0 cannot be deleted");
                }
                var r = RoomManager.Instance.GetRoom(id);
                if (r == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Delete() requested deletion of Room {id} which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Room {id}, no such Room in Room Manager");
                }
                if (r.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Delete() requested deletion of Room {id} which is locked in OLC", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Room {id}, it is locked in OLC");
                }
                if (r.PlayersInRoom.Count > 0)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Delete() requested deletion of Room {id} but there are Players in the Rom", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Room {id}, there are Players in the Room");
                }
                if (r.NPCsInRoom.Count > 0)
                {
                    while (r.NPCsInRoom.Count > 0)
                    {
                        var npc = r.NPCsInRoom[0];
                        if (!NPCManager.Instance.RemoveNPCInstance(npc.ID))
                        {
                            Game.LogMessage($"WARN: Failed to remove NPC {npc.ID} from Room {id} as part of a Room deletion requested by {Request.GetClientIPAddress()}", LogLevel.Warning, true);
                            return BadRequest($"Cannot delete Room {id}, failed to remove all NPC instances");
                        }
                    }
                }
                if (r.ItemsInRoom.Count > 0)
                {
                    r.ItemsInRoom.Clear();
                }
                if (!RoomManager.Instance.RemoveRoom(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to RoomController.Delete() failed to remove Room {r.RoomName} ({r.ID}) from Room Manager", LogLevel.Warning, true);
                    return BadRequest($"Failed to remove Room {id} from Room Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} removed Room {r.RoomName} ({r.ID}) from Room Manager", LogLevel.Info, true);
                return Ok($"Room {r.RoomName} ({r.ID}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in RoomController.Delete(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }
    }
}
