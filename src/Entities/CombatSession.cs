using Kingdoms_of_Etrea.Core;
using System.Collections.Generic;
using System.Linq;

namespace Kingdoms_of_Etrea.Entities
{
    internal class CombatSession
    {
        internal List<Descriptor> PlayersInCombat;
        internal List<(uint Initiative, dynamic Attacker, dynamic Target)> Participants = new List<(uint Initiative, dynamic Attacker, dynamic Target)>();

        internal List<(uint Initiative, dynamic Attacker, dynamic Target)> SortCombatList()
        {
            return this.Participants.OrderByDescending(p => p.Initiative).ToList();
        }
    }
}
