using Etrea3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Etrea3.Objects;

namespace Etrea3.OLC
{
    public static partial class OLC
    {
        private static void CreateMobProg(Session session)
        {
            session.Send($"%BYT%Tell Zohar to add in MobProgs!%PT%{Constants.NewLine}");
        }

        private static void DeleteMobProg(Session session)
        {
            session.Send($"%BYT%Tell Zohar to add in MobProgs!%PT%{Constants.NewLine}");
        }

        private static void ChangeMobProg(Session session)
        {
            session.Send($"%BYT%Tell Zohar to add in MobProgs!%PT%{Constants.NewLine}");
        }
    }
}