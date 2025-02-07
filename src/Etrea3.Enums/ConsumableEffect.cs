using System;

namespace Etrea3
{
    [Flags]
    public enum ConsumableEffect
    {
        Undefined = 0,
        None = 1 << 0,
        Healing = 1 << 1,
        MPRecovery = 1 << 2,
        Buff = 1 << 3,
        SPRecovery = 1 << 4,
        Antidote = 1 << 5,
        Death = 1 << 6,
        Restoration = 1 << 7,
        Poison = 1 << 8,
        DrainMP = 1 << 9,
        DrainSP = 1 << 10,
    }
}