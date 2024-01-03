using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdoms_of_Etrea.Entities
{
    internal class CombatSessionNew
    {
        internal Guid SessionID { get; set; }
        internal bool IsCompleted { get; set; }
        internal List<(uint Initiative, dynamic Attacker, dynamic Target)> Participants { get; set; }
        internal CombatSessionNew()
        {
            SessionID = Guid.NewGuid();
            IsCompleted = false;
            Participants = new List<(uint Initiative, dynamic Attacker, dynamic Target)>();
        }

        internal List<(uint Initiative, dynamic Attacker, dynamic Target)> SortParticipantsByInitiative()
        {
            return Participants.OrderByDescending(p => p.Initiative).ToList();
        }
    }
}
