using System;

namespace Kingdoms_of_Etrea.Entities
{
    internal class CombatSessionNew
    {
        internal Guid SessionID { get; set; }
        internal dynamic Attacker { get; set; }
        internal dynamic Defender { get; set; }
        internal Guid AttackerID { get; set; }
        internal Guid DefenderID { get; set; }
        internal CombatSessionNew(dynamic _attacker, dynamic _defender, Guid attackerID, Guid defenderID)
        {
            SessionID = Guid.NewGuid();
            Attacker = _attacker;
            Defender = _defender;
            AttackerID = attackerID;
            DefenderID = defenderID;
        }
    }
}
