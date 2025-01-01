using System;

namespace Etrea3
{
    [Flags]
    public enum PlayerFlags : uint
    {
        None = 0,
        WritingMail = 1 << 0,
        UsingOLC = 1 << 1,
        Frozen = 1 << 2,
    }
}