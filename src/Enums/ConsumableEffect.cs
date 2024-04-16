using System;

namespace Etrea2
{
    [Flags]
    internal enum ConsumableEffect
    {
        None = 0,
        Healing = 1 << 0,
        MPHealing = 1 << 1,
        Buff = 1 << 2,
        SPHealing = 1 << 3,
        Antidote = 1 << 4,
        Death = 1 << 5,
        Restoration = 1 << 6,
        Poison = 1 << 7,
        DrainMP = 1 << 8,
        DrainSP = 1 << 9,
    }
}