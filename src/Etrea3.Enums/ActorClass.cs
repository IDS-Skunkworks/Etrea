using System;

namespace Etrea3
{
    [Flags]
    public enum ActorClass
    {
        Undefined = 0,
        Wizard = 1 << 0,
        Thief = 1 << 1,
        Cleric = 1 << 2,
        Fighter = 1 << 3,
    }
}