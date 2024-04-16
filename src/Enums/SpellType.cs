using System;

namespace Etrea2
{
    [Flags]
    internal enum SpellType
    {
        Buff = 1,
        Debuff = 2,
        Damage = 4,
        Healing = 8
    }
}