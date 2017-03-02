using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Class for holding various constants for this program

namespace UAVBrainLinkTool
{
    public static class Constants
    {
        // TODO: Read/write external file with this info
        public const String userName = "rossllewallyn";
        public const String password = "Bxq^VX!nOv23";
        public const String profileName = "My Profile";

        public const int version = -1;

        public const Boolean logToConsole = true;

        public const String lineBreak = "\r\n";
        public const String dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";

        public const String noActiveCommands = "No Active Commands";

        public const String cmdNeutral  = "MC_NEUTRAL";
        public const String cmdPush     = "MC_PUSH";
        public const String cmdPull     = "MC_PULL";
        public const String cmdRaise    = "MC_RAISE"; // TODO: Confirm correct string
        public const String cmdLower    = "MC_LOWER"; // TODO: Confirm correct string

        public const String cmdTargetSelect = "BLT_TARGETSELECT";
        public const String cmdFollow       = "BLT_FOLLOW";
        public const String cmdTakeOff      = "BLT_TAKEOFF";
        public const String cmdLand         = "BLT_LAND";

        public const Single millisecondsInSeconds = 1000;
    }
}
