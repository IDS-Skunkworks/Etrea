using System.Collections.Generic;
using System;
using System.Web.Http;
using Etrea3.Core;
using Etrea3.Objects;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class ItemController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var items = ItemManager.Instance.GetItem();
                if (items == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<InventoryItem>>(items));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemController.Get(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var item = ItemManager.Instance.GetItem(id);
                if (item == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<dynamic>(item));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemController.Get(id): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Post() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var i = Helpers.DeserialiseEtreaObject<dynamic>(payload);
                if (!ValidateAsset(null, i, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Post() sent an Item which failed validation", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                if (!ItemManager.Instance.AddOrUpdateItem(i, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Post() failed to add an Item to Item Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to add Item to Item Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Item {i.Name} ({i.ID}) to Item Manager", LogLevel.Info, true);
                return Ok($"Item {i.ID} ({i.Name}) created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemController.Post(): {ex.Message}", LogLevel.Error, true);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Put() sent a null payload", LogLevel.Warning, true);
                    return BadRequest("Payload was null or empty");
                }
                var i = Helpers.DeserialiseEtreaObject<dynamic>(payload);
                if (!ValidateAsset(null, i, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Put() sent an Item which failed validation", LogLevel.Warning, true);
                    return BadRequest(repl);
                }
                var currentItem = ItemManager.Instance.GetItem(i.ID);
                if (currentItem == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Put() sent an Item which does not exist", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Item {i.ID}, it does not currently exist");
                }
                if (currentItem.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Put() sent an Item which is locked in OLC ({i.ID})", LogLevel.Warning, true);
                    return BadRequest($"Cannot update Item {i.ID}, it is locked in OLC");
                }
                if (!ItemManager.Instance.AddOrUpdateItem(i, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Put() failed to update Item {i.Name} ({i.ID}) in Item Manager", LogLevel.Warning, true);
                    return BadRequest("Failed to update Item in Item Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to ItemController.Put() updated Item {i.Name} ({i.ID}) in Item Manager", LogLevel.Info, true);
                return Ok($"Item {i.ID} ({i.Name}) updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemController.Put(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }

        [Route("api/item/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var item = ItemManager.Instance.GetItem(id);
                if (item == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Delete() requested deletion of Item {id} which does not exist", LogLevel.Warning, true);
                    return NotFound();
                }
                if (item.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Delete() requested deletion of Item {id} which is locked in OLC", LogLevel.Warning, true);
                    return BadRequest($"Cannot delete Item {id} it is locked in OLC");
                }
                if (!ItemManager.Instance.RemoveItem(id))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to ItemController.Delete() failed to remove Item {id} from Item Manager", LogLevel.Warning, true);
                    return BadRequest($"Failed to remove Item {id} from Item Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} to ItemController.Delete() removed Item {item.Name} ({item.ID}) from Item Manager", LogLevel.Info, true);
                return Ok($"Item {item.Name} ({id}) removed successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in ItemController.Delete(): {ex.Message}", LogLevel.Error, true);
                return InternalServerError(ex);
            }
        }
    }
}
