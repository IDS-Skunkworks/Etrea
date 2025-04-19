using System;

namespace Etrea3
{
    public enum TimeOfDay : short
    {
        None = 0,
        Morning = 1 << 0,
        Afternoon = 1 << 1,
        Evening = 1 << 2,
        Night = 1 << 3,
    }
}