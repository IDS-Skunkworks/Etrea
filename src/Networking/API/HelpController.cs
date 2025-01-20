using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Etrea3.Objects;
using System.Threading.Tasks;
using System.Web.Http;
using Etrea3.Core;
using static Etrea3.OLC.OLC;

namespace Etrea3.Networking.API
{
    public class HelpController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                List<HelpArticle> helpArticles = HelpManager.Instance.GetArticle();
                if (helpArticles == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<HelpArticle>>(helpArticles));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in HelpController.Get(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() set a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var a = Helpers.DeserialiseEtreaObject<HelpArticle>(payload);
                if (!ValidateAsset(null, a, true, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() sent an Article which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                if (!HelpManager.Instance.AddOrUpdateArticle(a, true))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() failed to add Article to Help Manager", LogLevel.Warning);
                    return BadRequest("Failed to add the Article to Help Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} added new Article {a.Title} to Help Manager", LogLevel.Info);
                return Ok($"Article '{a.Title}' created successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in HelpController.Post(): {ex.Message}", LogLevel.Error);
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
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() sent a null payload", LogLevel.Warning);
                    return BadRequest("Payload was null or empty");
                }
                var z = Helpers.DeserialiseEtreaObject<HelpArticle>(payload);
                if (!ValidateAsset(null, z, false, out string repl))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() sent an Article which failed validation: {repl}", LogLevel.Warning);
                    return BadRequest(repl);
                }
                var currentArticle = HelpManager.Instance.GetArticle(z.Title);
                if (currentArticle == null)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() sent an Article which does not exist", LogLevel.Warning);
                    return BadRequest($"Cannot update Article {z.Title}, it does not currently exist");
                }
                if (currentArticle.OLCLocked)
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() sent an Article whcih was locked in OLC ({z.Title})", LogLevel.Warning);
                    return BadRequest($"Cannot update Article {z.Title}, it is locked in OLC");
                }
                if (!HelpManager.Instance.AddOrUpdateArticle(z, false))
                {
                    Game.LogMessage($"WARN: Connection from {Request.GetClientIPAddress()} to HelpController.Put() failed to update Article {z.Title} in Help Manager", LogLevel.Warning);
                    return BadRequest("Failed to update Article in Help Manager");
                }
                Game.LogMessage($"INFO: Connection from {Request.GetClientIPAddress()} updated Article {z.Title} in Help Manager", LogLevel.Info);
                return Ok($"Article {z.Title} updated successfully");
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Connection from {Request.GetClientIPAddress()} encountered an error in HelpController.Put(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}
