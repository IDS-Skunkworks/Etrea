using System;

namespace Etrea2
{
    [Flags]
    internal enum WearSlot
    {
        None = 0,
        Head = 1,
        Neck = 2,
        Armour = 4,
        FingerLeft = 8,
        FingerRight = 16,
        Weapon = 32,
        Held = 64,
    }
}
