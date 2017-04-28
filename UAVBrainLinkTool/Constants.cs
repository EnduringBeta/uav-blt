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
        public const Boolean logIndividualEmotivDataPoints = false;

        // Config fields

        public const String configFileLoc = "./";
        public const String configFileName = "configBLT.json";

        public const String configFieldCommandScriptInfo = "commandScriptInfo";
        public const String configFieldUAVInfo = "uavInfo";
        public const String configFieldUserInfo = "userInfo";
        public const String configFieldCommandThresholds = "commandPower";

        public const String configFieldScriptFileName = "scriptFileName";

        public const String configFieldComPort = "comPort";
        public const String configFieldTakeoffAltitude = "takeoffAltitude";
        public const String configFieldLocationA = "locationA";
        public const String configFieldLocationB = "locationB";
        public const String configFieldLat = "lat";
        public const String configFieldLon = "lon";
        public const String configFieldAlt = "alt";

        public const String configFieldUserName = "userName";
        public const String configFieldPassword = "password";
        public const String configFieldProfileName = "profileName";

        public const String configFieldActiveCommandThreshold = "activeCommandThreshold";
        public const String configFieldInactiveCommandThreshold = "inactiveCommandThreshold";
        public const String configFieldCommandSentPowerPercentage = "commandSentPowerPercentage";

        // Command scripts

        public const String callPython = "python ";

        // Command-specific data

        public const String cmdNeutral  = "MC_NEUTRAL";
        public const String cmdPush     = "MC_PUSH";
        public const String cmdPull     = "MC_PULL";
        public const String cmdRaise    = "MC_RAISE"; // TODO: Confirm correct string
        public const String cmdLower    = "MC_LOWER"; // TODO: Confirm correct string

        public const String thresholdTag = "THRESHOLD";

        // Python script commands

        // These are only defined in the configuration file,
        // but they are included for reference.
        public const String scriptTargetSelectA = "BLT_TARGETSELECTA";
        public const String scriptTargetSelectB = "BLT_TARGETSELECTB";
        public const String scriptFollow        = "BLT_FOLLOW";
        public const String scriptTakeOff       = "BLT_TAKEOFF";
        public const String scriptLand          = "BLT_LAND";
        public const String scriptReturnLand    = "BLT_RETURNLAND";

        // These are used to configure the script by this program
        public const String scriptConnect       = "BLT_CONNECT";
        public const String scriptDisconnect    = "BLT_DISCONNECT";
        public const String scriptAttributes    = "BLT_ATTRIBUTES";
        public const String scriptExit          = "BLT_EXIT";

        public const String scriptConfigComPort         = "BLT_COMPORT";
        public const String scriptConfigTakeoffAltitude = "BLT_TAKEOFFALTITUDE";
        public const String scriptConfigLocationA       = "BLT_LOCATIONA";
        public const String scriptConfigLocationB       = "BLT_LOCATIONB";

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
        public const String startListening = "Listen to Emotiv";
        public const String stopListening = "Stop Listening";

        public const String connectToUAV = "Connect to UAV";
        public const String disconnectFromUAV = "Disconnect from UAV";
        public const String startTransmitting = "Transmit to UAV";
        public const String stopTransmitting  = "Stop Transmitting";

        // General

        public const String lineBreak = "\r\n";
        public const String dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";

        public const Single millisecondsInSeconds = 1000;

        public const int maxPercent = 100;
    }
}
