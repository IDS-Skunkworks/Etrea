using System;

namespace Etrea3
{
    [Flags]
    public enum PlayerFlags : uint
    {
        None = 0,                                       // no flags, not settable
        WritingMail = 1 << 0,                           // writing mail - automatically set and removed
        UsingOLC = 1 << 1,                              // using OLC - automatically set and removed
        Frozen = 1 << 2,                                // frozen by an Imm, no input passed to parser, set/removed by an Imm using FREEZE or THAW
        NoShowExits = 1 << 3,                           // don't show exits at the end of room descriptions, player settable
        NoShowRoomFlags = 1 << 4,                       // don't show room flags at the start of descriptions, Imm only
        NoSummon = 1 << 5,                              // player cannot be summoned by other players, doesn't apply to Imm summon command
        MUDLogError = 1 << 6,                           // show errors as they are logged, Imm only
        MUDLogWarn = 1 << 7,                            // show warnings as they are logged, Imm only
        MUDLogConnection = 1 << 8,                      // show connection messages as they are logged, Imm only
        MUDLogInfo = 1 << 9,                            // show info messages as they are logged, Imm only
        MUDLogCombat = 1 << 10,                         // show combat messages as they are logged, Imm only
        MUDLogShops = 1 << 11,                          // show shop messages as they are logged, Imm only
        MUDLogOLC = 1 << 12,                            // show OLC messages as they are logged, Imm only
        MUDLogGod = 1 << 13,                            // show usage of god-like powers, Imm only
        MUDLogDebug = 1 << 14,                          // show debug messages as they are logged
    }
}