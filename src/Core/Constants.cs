using System.Configuration;

namespace Etrea2.Core
{
    internal static class Constants
    {
        //internal const string EchoOff = "\xFF\xFB\x01\xFF\xFB\x03";     // echo off (testing)
        internal const string TabStop = "\x09";                         // tell client to print a tab
        internal const string BoldText = "\x1b[1m";                     // tell client to display bold
        internal const string PlainText = "\x1b[0m";                    // tell client to remove all formatting - \x1b[0m
        internal const string RedText = "\u001b[31m";                   // red console text
        internal const string BlueText = "\u001b[34m";
        internal const string YellowText = "\u001b[33m";
        internal const string GreenText = "\u001b[32m";
        internal const string WhiteText = "\u001b[37m";
        internal const string BrightWhiteText = "\u001b[37;1m";
        internal const string BrightYellowText = "\u001b[33;1m";
        internal const string BrightRedText = "\u001b[31;1m";
        internal const string BrightGreenText = "\u001b[32;1m";
        internal const string BrightBluetext = "\u001b[34;1m";
        internal const string EchoOff = "\xff\xfb\x01";               // tell client not to echo input to screen
        internal const string EchoOn = "\xff\xfc\x01";                  // tell client to resume echo input
        internal const string NewLine = "\r\n";                         // carriage return/line-feed
        internal const string ClearScreen = "\u001B[2J";                // clear telnet client window
        internal const char ASCIIDel = (char)0x7f;                      // delete
        internal const uint ImmLevel = 200;                              // characters level 200 or more have access to additional Imm commands
        internal const uint MaxPlayerLevel = 150;                       // the highest level a player character can reach
        internal const uint MaxAuthErrors = 5;                          // number of times a user can fail login before being kicked

        internal static uint PlayerStartRoom()                          // Get the RID of the start room for players or return the default 100 if we can't read it
        {
            var rid = ConfigurationManager.AppSettings["playerStartRoom"];
            if (!string.IsNullOrEmpty(rid))
            {
                if (uint.TryParse(rid, out uint startRoom))
                {
                    return startRoom;
                }
            }
            return 100;
        }

        internal static uint LimboRID()                                 // Get the RID of Limbo, or return the default of 0 if we can't read it
        {
            var rid = ConfigurationManager.AppSettings["limboRID"];
            if (!string.IsNullOrEmpty(rid))
            {
                if (uint.TryParse(rid, out uint limbRID))
                {
                    return limbRID;
                }
            }
            return 0;
        }

        //internal static uint DonationRoomRid()                          // Get the RID of the donation room, or return the default of 150 if we can't read it
        //{
        //    var rid = ConfigurationManager.AppSettings["donationRoomRID"];
        //    if (!string.IsNullOrEmpty(rid))
        //    {
        //        if (uint.TryParse(rid, out uint donationRoom))
        //        {
        //            return donationRoom;
        //        }
        //    }
        //    return 150;
        //}

        internal static uint MaxIdleTickCount()                         // Get the max number of autosave ticks a player can be idle for before being disconnected, default 20
        {
            var tickCount = ConfigurationManager.AppSettings["maxIdleTickCount"];
            if (!string.IsNullOrEmpty(tickCount))
            {
                if (uint.TryParse(tickCount, out uint maxIdleTickCount))
                {
                    return maxIdleTickCount;
                }
            }
            return 20;
        }

        internal static bool DisconnectIdleImms()                       // Get the setting to determine if we should disconnect idle Immortal players, default false
        {
            var disconImms = ConfigurationManager.AppSettings["disconIdleImms"];
            if (!string.IsNullOrEmpty(disconImms))
            {
                if (bool.TryParse(disconImms, out bool disconnected))
                {
                    return disconnected;
                }
            }
            return false;
        }

        internal const string PropertyMissingValue = "One or more required properties are missing values...";
        internal const string DidntUnderstand = "Sorry, I didn't understand that...";
        internal const string InvalidChoice = "Sorry, that doesn't look like a valid choice...";
    }
}
