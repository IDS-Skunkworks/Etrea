using System;

namespace Etrea3
{
    [Flags]
    public enum ActorClass
    {
        Undefined = 0,
        Wizard = 1,
        Thief = 2,
        Cleric = 4,
        Fighter = 8,
    }
}