using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class QuestController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var quests = QuestManager.Instance.GetQuest();
                if (quests == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Quest>>(quests));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in QuestController.Get(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Post() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Quest>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Post() sent an Quest which failed validation: {repl}", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                if (!QuestManager.Instance.AddOrUpdateQuest(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Post() failed to add a Quest to Quest Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to add the Quest to Quest Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Quest {n.Name} ({n.ID}) to Quest Manager", LogLevel.Info, true);
                return Ok($"Quest {n.ID} ({n.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in QuestController.Post(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Put() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Quest>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Put() sent an Quest which failed validation", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                var currentQuest = QuestManager.Instance.GetQuest(n.ID);
                if (currentQuest == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Put() sent an Quest which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Quest {n.ID}, it does not currently exist");
                }
                if (currentQuest.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Put() sent an Quest which is locked in OLC ({n.ID})", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Quest {n.ID} it is locked in OLC");
                }
                if (!QuestManager.Instance.AddOrUpdateQuest(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Put() failed to udpate Quest {n.Name} ({n.ID}) in Quest Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to update Quest in Quest Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Quest {n.Name} ({n.ID}) in Quest Manager", LogLevel.Info, true);
                return Ok($"Quest {n.ID} ({n.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in QuestController.Put(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = QuestManager.Instance.GetQuest(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Delete() requested deletion of Quest {id} which does not exist", LogLevel.Warning, true);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Delete() requested deletion of Quest {id} which is locked in OLC", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Quest {id} it is locked in OLC");
                }
                if (!QuestManager.Instance.RemoveQuest(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to QuestController.Delete() failed to remove Quest {id} from Quest Manager", LogLevel.Warning, true);
                    return BadRequest($"Failed to remove Quest {id} from Quest Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to QuestController.Delete() removed Quest {n.Name} ({n.ID}) from Quest Manager", LogLevel.Info, true);
                return Ok($"Quest {n.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in QuestController.Delete(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }
    }
}
