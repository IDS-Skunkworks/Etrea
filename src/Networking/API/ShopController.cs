using Etrea3.Core;
using Etrea3.Objects;
using System;
using System.Collections.Generic;
using System.Web.Http;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class ShopController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var shops = ShopManager.Instance.GetShop();
                if (shops == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Shop>>(shops));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ShopController.Get(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Post() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Shop>(payload);
                if (!ValidateAsset(null, n, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Post() sent a Shop which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!ShopManager.Instance.AddOrUpdateShop(n, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Post() failed to add a Shop to Shop Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the Shop to Shop Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Shop {n.ShopName} ({n.ID}) to Shop Manager", LogLevel.Info);
                return Ok($"Shop {n.ID} ({n.ShopName}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ShopController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var n = Helpers.DeserialiseEtreaObject<Shop>(payload);
                if (!ValidateAsset(null, n, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Put() sent a Shop which failed validation", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentShop = ShopManager.Instance.GetShop(n.ID);
                if (currentShop == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Put() sent a Shop which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update Shop {n.ID}, it does not currently exist");
                }
                if (currentShop.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Put() sent a Shop which is locked in OLC ({n.ID})", LogLevel.Warning);
                    return BadRequest($"Cannot update Shop {n.ID} it is locked in OLC");
                }
                if (!ShopManager.Instance.AddOrUpdateShop(n, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Put() failed to udpate Shop {n.ShopName} ({n.ID}) in Shop Manager", LogLevel.Warning);
                    return BadRequest("Failed to update Shop in Shop Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Shop {n.ShopName} ({n.ID}) in Shop Manager", LogLevel.Info);
                return Ok($"Shop {n.ID} ({n.ShopName}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ShopController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var n = ShopManager.Instance.GetShop(id);
                if (n == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Delete() requested deletion of Shop {id} which does not exist", LogLevel.Warning);
                    return NotFound();
                }
                if (n.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Delete() requested deletion of Shop {id} which is locked in OLC", LogLevel.Warning);
                    return BadRequest($"Cannot delete Shop {id} it is locked in OLC");
                }
                if (!ShopManager.Instance.RemoveShop(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ShopController.Delete() failed to remove Shop {id} from Shop Manager", LogLevel.Warning);
                    return BadRequest($"Failed to remove Shop {id} from Shop Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to ShopController.Delete() removed Shop {n.ShopName} ({n.ID}) from Shop Manager", LogLevel.Info);
                return Ok($"Shop {n.ShopName} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ShopController.Delete(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
