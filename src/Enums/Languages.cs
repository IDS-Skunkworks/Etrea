using System;

namespace Etrea2
{
    [Flags]
    internal enum Languages
    {
        Common = 0,
        Elvish = 1 << 0,
        Dwarvish = 1 << 1,
        Orcish = 1 << 2,
        Draconic = 1 << 3,
        Infernal = 1 << 4,
        Celestial = 1 << 5,
    }
}