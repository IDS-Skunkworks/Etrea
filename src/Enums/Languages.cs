using System;

namespace Etrea3
{
    [Flags]
    internal enum Languages
    {
        Common = 0,
        Infernal = 1 << 0,
        Celestial = 1 << 1,
        Draconic = 1 << 2,
        Orcish = 1 << 3,
        Elvish = 1 << 4,
        Dwarvish = 1 << 5,
    }
}