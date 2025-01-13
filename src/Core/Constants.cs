namespace Etrea3.Core
{
    public static class Constants
    {
        public const string TabStop = "\x09";                                                 // tell client to print a tab
        public const string BoldText = "\x1b[1m";                                             // tell client to display bold
        public const string PlainText = "\x1b[0m";                                            // tell client to remove all formatting - \x1b[0m
        public const string RedText = "\u001b[31m";                                           // red console text
        public const string BlueText = "\u001b[34m";                                          // blue console text
        public const string YellowText = "\u001b[33m";                                        // yellow console text
        public const string GreenText = "\u001b[32m";                                         // green console text
        public const string WhiteText = "\u001b[37m";                                         // white console text
        public const string MagentaText = "\u001b[35m";                                       // purple console text
        public const string BrightWhiteText = "\u001b[37;1m";                                 // bright white console text
        public const string BrightYellowText = "\u001b[33;1m";                                // bright yellow console text
        public const string BrightRedText = "\u001b[31;1m";                                   // bright red console text
        public const string BrightGreenText = "\u001b[32;1m";                                 // bright green console text
        public const string BrightBlueText = "\u001b[34;1m";                                  // bright blue console text
        public const string BrightMagentaText = "\u001b[35;1m";                               // bright purple console text
        public const string EchoOff = "\xff\xfb\x01";                                         // tell client not to echo input to screen
        public const string EchoOn = "\xff\xfc\x01";                                          // tell client to resume echo input
        public const string NewLine = "\r\n";                                                 // carriage return/line-feed
        public const string ClearScreen = "\u001B[2J";                                        // clear telnet client window
        public const char ASCIIDel = (char)0x7f;                                              // delete
        public const uint ImmLevel = 100;                                                     // characters level 100 or more have access to additional Imm commands
        public const uint MaxPlayerLevel = 99;                                                // the highest level a player character can reach, should be lower than ImmLevel and supported by entries in the LevelTable class
        public const uint MaxAuthErrors = 5;                                                  // number of times a user can fail login before being disconnected
        public static readonly string[] ObjectivePronouns = { "him", "her", "them" };         // objective pronouns for emotes
        public static readonly string[] PosessivePronouns = { "his", "her", "their" };        // posessive pronouns for emotes
        public static readonly string[] PersonalPronouns = { "he", "she", "they" };           // personal pronouns for emotes
        public static readonly string[] Languages = { "Common", "Orcish", "Dwarvish", "Elvish", "Draconic", "Infernal", "Celestial" };      // languages players can learn and speak
    }
}
