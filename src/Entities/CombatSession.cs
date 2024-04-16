using System;

namespace Etrea2.Entities
{
    internal class CombatSession
    {
        internal Guid SessionID { get; set; }
        internal dynamic Attacker { get; set; }
        internal dynamic Defender { get; set; }
        internal Guid AttackerID { get; set; }
        internal Guid DefenderID { get; set; }
        internal CombatSession(dynamic _attacker, dynamic _defender, Guid attackerID, Guid defenderID)
        {
            SessionID = Guid.NewGuid();
            Attacker = _attacker;
            Defender = _defender;
            AttackerID = attackerID;
            DefenderID = defenderID;
        }
    }
}
