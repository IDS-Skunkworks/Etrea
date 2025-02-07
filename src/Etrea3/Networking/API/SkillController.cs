using System.Collections.Generic;
using System;
using System.Web.Http;
using Etrea3.Core;
using Etrea3.Objects;

namespace Etrea3.Networking.API
{
    public class SkillController : ApiController
    {
        public IHttpActionResult Get()
        {
            try
            {
                var skills = SkillManager.Instance.GetSkill();
                if (skills == null)
                {
                    return NotFound();
                }
                return Ok(Helpers.SerialiseEtreaObject<List<Skill>>(skills));
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in SkillController.Get(): {ex.Message}", LogLevel.Error);
                return InternalServerError(ex);
            }
        }
    }
}