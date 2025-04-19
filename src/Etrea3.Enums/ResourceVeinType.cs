using System;

namespace Etrea3
{
    [Flags]
    public enum ResourceVeinType
    {
        None = 0,
        Common = 1 << 0,
        Uncommon = 1 << 1,
        Rare = 1 << 2,
        VeryRare = 1 << 3,
    }
}