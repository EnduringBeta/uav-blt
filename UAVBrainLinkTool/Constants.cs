using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

// Class for holding various constants for this program

namespace UAVBrainLinkTool
{
    public static class Constants
    {
        public const int version = -1;

        public const Boolean logToConsole = true;

        // Config fields

        public const String configFileLoc = "./";
        public const String configFileName = "configBLT.json";

        public const String configFieldCommandScripts = "commandScripts";
        public const String configFieldUserInfo = "userInfo";
        public const String configFieldCommandThresholds = "commandThresholds";

        public const String configFieldUserName = "userName";
        public const String configFieldPassword = "password";
        public const String configFieldProfileName = "profileName";

        public const String configFieldActiveCommandThreshold = "activeCommandThreshold";
        public const String configFieldInactiveCommandThreshold = "inactiveCommandThreshold";

        // Command scripts

        public const String callPython = "python ";

        // Command-specific data

        public const String cmdNeutral  = "MC_NEUTRAL";
        public const String cmdPush     = "MC_PUSH";
        public const String cmdPull     = "MC_PULL";
        public const String cmdRaise    = "MC_RAISE"; // TODO: Confirm correct string
        public const String cmdLower    = "MC_LOWER"; // TODO: Confirm correct string

        public const String thresholdTag = "THRESHOLD";

        public const String cmdTargetSelect = "BLT_TARGETSELECT";
        public const String cmdFollow       = "BLT_FOLLOW";
        public const String cmdTakeOff      = "BLT_TAKEOFF";
        public const String cmdLand         = "BLT_LAND";

        // Colors!

        public static OxyPlot.OxyColor colorPlotCmdPush    = OxyPlot.OxyColors.ForestGreen;
        public static OxyPlot.OxyColor colorPlotCmdPull    = OxyPlot.OxyColors.IndianRed;
        public static OxyPlot.OxyColor colorPlotCmdRaise   = OxyPlot.OxyColors.SkyBlue;
        public static OxyPlot.OxyColor colorPlotCmdLower   = OxyPlot.OxyColors.SandyBrown;
        public static OxyPlot.OxyColor colorPlotCmdNeutral = OxyPlot.OxyColors.Black;
        public static OxyPlot.OxyColor colorPlotThreshold  = OxyPlot.OxyColors.Gray;
        public static OxyPlot.OxyColor colorPlotDefault    = OxyPlot.OxyColors.Black;

        public static SolidColorBrush colorButtonCmdPush  = Brushes.ForestGreen;
        public static SolidColorBrush colorButtonCmdPull  = Brushes.IndianRed;
        public static SolidColorBrush colorButtonCmdRaise = Brushes.SkyBlue;
        public static SolidColorBrush colorButtonCmdLower = Brushes.SandyBrown;

        // Button text

        public const String noActiveCommands = "No Active Commands";
        public const String connecting = "Connecting...";
        public const String startListening = "Start Listening";
        public const String stopListening = "Stop Listening";

        // General

        public const String lineBreak = "\r\n";
        public const String dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";

        public const Single millisecondsInSeconds = 1000;
    }
}
