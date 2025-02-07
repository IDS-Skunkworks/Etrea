using System.Collections.Generic;
using System;
using System.Web.Http;
using Etrea3.Core;
using Etrea3.Objects;

namespace Etrea3.Networking.API
{
    public class BuffController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var buffs = BuffManager.Instance.GetBuff();
                if (buffs == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Buff>>(buffs));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in BuffController.Get(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}