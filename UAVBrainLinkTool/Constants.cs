using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

// Class for holding various constants for this program

namespace UAVBrainLinkTool
{
    public static class Constants
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        public const int version = -1;

        // Logging

        public const Boolean logToConsole = true;

        // Config fields

        public const String configFileLoc = "./";
        public const String configFileName = "configBLT.json";

        public const String configFieldLogging = "logging";
        public const String configFieldCommandScriptInfo = "commandScriptInfo";
        public const String configFieldUAVInfo = "uavInfo";
        public const String configFieldUserInfo = "userInfo";
        public const String configFieldCommandThresholds = "commandPower";
        public const String configFieldStress = "stress";

        public const String configFieldLogIndividualEmotivCommandEvents = "logIndividualEmotivCommandEvents";
        public const String configFieldLogIndividualEmotivEmotionEvents = "logIndividualEmotivEmotionEvents";

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

        public const String configFieldCommandThreshold = "commandThreshold";
        public const String configFieldCommandSentPowerPercentage = "commandSentPowerPercentage";
        public const String configFieldSampleTimeWindow = "sampleTimeWindow";

        public const String configFieldActiveStressThreshold = "activeStressThreshold";
        public const String configFieldInactiveStressThreshold = "inactiveStressThreshold";
        public const String configFieldStressFactor = "stressFactor";
        public const String configFieldStressTimeWindow = "stressTimeWindow";

        // Command scripts

        public const String callPython = "python ";

        // Command-specific data

        public const String cmdNeutral  = "MC_NEUTRAL";
        public const String cmdPush     = "MC_PUSH";
        public const String cmdPull     = "MC_PULL";
        public const String cmdLift     = "MC_LIFT";
        public const String cmdDrop     = "MC_DROP";

        public const String thresholdTag = "THRESHOLD";

        // Emotion-specific data

        public const String alphaTag    = "Alpha";
        public const String thetaTag    = "Theta";
        public const String lowBetaTag  = "Low Beta";
        public const String highBetaTag = "High Beta";
        public const String gammaTag    = "Gamma";

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

        public static OxyPlot.OxyColor colorPlotCmdPush     = OxyPlot.OxyColors.ForestGreen;
        public static OxyPlot.OxyColor colorPlotCmdPull     = OxyPlot.OxyColors.IndianRed;
        public static OxyPlot.OxyColor colorPlotCmdLift     = OxyPlot.OxyColors.SkyBlue;
        public static OxyPlot.OxyColor colorPlotCmdDrop     = OxyPlot.OxyColors.SandyBrown;
        public static OxyPlot.OxyColor colorPlotCmdNeutral  = OxyPlot.OxyColors.Black;
        public static OxyPlot.OxyColor colorPlotThreshold   = OxyPlot.OxyColors.Gray;
        public static OxyPlot.OxyColor colorPlotDefault     = OxyPlot.OxyColors.Black;

        public static OxyPlot.OxyColor colorPlotEmoTheta    = OxyPlot.OxyColors.DarkRed;
        public static OxyPlot.OxyColor colorPlotEmoAlpha    = OxyPlot.OxyColors.LightGoldenrodYellow;
        public static OxyPlot.OxyColor colorPlotEmoLowBeta  = OxyPlot.OxyColors.LightGreen;
        public static OxyPlot.OxyColor colorPlotEmoHighBeta = OxyPlot.OxyColors.CadetBlue;
        public static OxyPlot.OxyColor colorPlotEmoGamma    = OxyPlot.OxyColors.MediumPurple;

        // Button colors (properties for XAML)

        // Commands

        private static SolidColorBrush colorButtonCmdPush = Brushes.ForestGreen;
        public static SolidColorBrush ColorButtonCmdPush
        {
            get
            {
                return colorButtonCmdPush;
            }
            private set
            {
                colorButtonCmdPush = value;
                OnStaticPropertyChanged("ColorButtonCmdPush");
            }
        }
        private static SolidColorBrush colorButtonCmdPull = Brushes.IndianRed;
        public static SolidColorBrush ColorButtonCmdPull
        {
            get
            {
                return colorButtonCmdPull;
            }
            private set
            {
                colorButtonCmdPull = value;
                OnStaticPropertyChanged("ColorButtonCmdPull");
            }
        }
        private static SolidColorBrush colorButtonCmdLift = Brushes.SkyBlue;
        public static SolidColorBrush ColorButtonCmdLift
        {
            get
            {
                return colorButtonCmdLift;
            }
            private set
            {
                colorButtonCmdLift = value;
                OnStaticPropertyChanged("ColorButtonCmdLift");
            }
        }
        private static SolidColorBrush colorButtonCmdDrop = Brushes.SandyBrown;
        public static SolidColorBrush ColorButtonCmdDrop
        {
            get
            {
                return colorButtonCmdDrop;
            }
            private set
            {
                colorButtonCmdDrop = value;
                OnStaticPropertyChanged("ColorButtonCmdDrop");
            }
        }
        private static SolidColorBrush colorButtonCmdDefault = Brushes.Black;
        public static SolidColorBrush ColorButtonCmdDefault
        {
            get
            {
                return colorButtonCmdDefault;
            }
            private set
            {
                colorButtonCmdDefault = value;
                OnStaticPropertyChanged("ColorButtonCmdDefault");
            }
        }

        // Brain wave frequency ranges

        private static SolidColorBrush colorButtonEmoTheta = Brushes.DarkRed;
        public static SolidColorBrush ColorButtonEmoTheta
        {
            get
            {
                return colorButtonEmoTheta;
            }
            private set
            {
                colorButtonEmoTheta = value;
                OnStaticPropertyChanged("ColorButtonEmoTheta");
            }
        }
        private static SolidColorBrush colorButtonEmoAlpha = Brushes.LightGoldenrodYellow;
        public static SolidColorBrush ColorButtonEmoAlpha
        {
            get
            {
                return colorButtonEmoAlpha;
            }
            private set
            {
                colorButtonEmoAlpha = value;
                OnStaticPropertyChanged("ColorButtonEmoAlpha");
            }
        }
        private static SolidColorBrush colorButtonEmoLowBeta = Brushes.LightGreen;
        public static SolidColorBrush ColorButtonEmoLowBeta
        {
            get
            {
                return colorButtonEmoLowBeta;
            }
            private set
            {
                colorButtonEmoLowBeta = value;
                OnStaticPropertyChanged("ColorButtonEmoLowBeta");
            }
        }
        private static SolidColorBrush colorButtonEmoHighBeta = Brushes.CadetBlue;
        public static SolidColorBrush ColorButtonEmoHighBeta
        {
            get
            {
                return colorButtonEmoHighBeta;
            }
            private set
            {
                colorButtonEmoHighBeta = value;
                OnStaticPropertyChanged("ColorButtonEmoHighBeta");
            }
        }
        private static SolidColorBrush colorButtonEmoGamma = Brushes.MediumPurple;
        public static SolidColorBrush ColorButtonEmoGamma
        {
            get
            {
                return colorButtonEmoGamma;
            }
            private set
            {
                colorButtonEmoGamma = value;
                OnStaticPropertyChanged("ColorButtonEmoGamma");
            }
        }

        // Button text

        public const String noActiveCommands = "No Active Commands";
        public const String connecting = "Connecting...";

        public const String startListening = "Listen to Emotiv";
        public const String stopListening = "Stop Listening";

        public const String connectToUAV = "Connect to UAV";
        public const String disconnectFromUAV = "Disconnect from UAV";

        public const String startTransmitting = "Transmit to UAV";
        public const String stopTransmitting  = "Stop Transmitting";

        public const String startMonitoringStress = "Monitor Stress";
        public const String stopMonitoringStress = "Stop Monitoring";

        // General

        public const String lineBreak = "\r\n";
        public const String dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";

        public const Single millisecondsInSeconds = 1000;

        public const int maxPercent = 100;
    }
}
