using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    static public class Config
    {
        // http://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        private static String userName = "";
        public static String UserName
        {
            get
            {
                return userName;
            }
            private set
            {
                userName = value;
                OnStaticPropertyChanged("UserName");
            }
        }

        private static String password = "";
        public static String Password
        {
            get
            {
                return password;
            }
            private set
            {
                password = value;
                OnStaticPropertyChanged("Password");
            }
        }

        private static String profileName = "";
        public static String ProfileName
        {
            get
            {
                return profileName;
            }
            private set
            {
                profileName = value;
                OnStaticPropertyChanged("ProfileName");
            }
        }

        private static String comPort = "";
        public static String COMPort
        {
            get
            {
                return comPort;
            }
            private set
            {
                comPort = value;
                OnStaticPropertyChanged("COMPort");
            }
        }

        public static String CommandScript { get; private set; }

        public static String StringPush { get; private set; }
        public static String StringPull { get; private set; }
        public static String StringRaise { get; private set; }
        public static String StringLower { get; private set; }

        public static int TakeoffAltitude { get; private set; }
        public static GeoCoordinates LocationA { get; private set; }
        public static GeoCoordinates LocationB { get; private set; }

        private static JObject ConfigJSON { get; set; }

        public static Boolean ConfigLoaded { get; private set; }

        public class GeoCoordinates
        {
            public double lat = 0.0;
            public double lon = 0.0;
            public double alt = 0.0;

            public GeoCoordinates(double inputLat = 0.0, double inputLon = 0.0, double inputAlt = 0.0)
            {
                lat = inputLat;
                lon = inputLon;
                alt = inputAlt;
            }
        }

        public static Boolean importConfig()
        {
            Boolean success = true;

            try
            {
                String configString = File.ReadAllText(Constants.configFileLoc + Constants.configFileName);
                ConfigJSON = (JObject)JObject.Parse(configString);
            }
            catch (Exception ex)
            {
                Logging.outputLine("JSON parsing exception: " + ex.Message);
                return false;
            }

            if (success)
                success = getCommandInfo();
            if (success)
                success = getUAVInfo();
            if (success)
                success = getUserInfo();
            if (success)
                success = getCommandThresholds();

            if (success)
                ConfigLoaded = true;
            else
                ConfigLoaded = false;

            return ConfigLoaded;
        }

        private static Boolean getCommandInfo()
        {
            Boolean success = true;

            try
            {
                CommandScript = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.configFieldScriptFileName];

                StringPush = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.cmdPush];
                StringPull = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.cmdPull];
                StringRaise = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.cmdRaise];
                StringLower = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.cmdLower];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - command info: " + ex.Message);
                return false;
            }

            // Test whether script file exists
            if (success)
                success = File.Exists(CommandScript) || (String.Compare(CommandScript, "") == 0);

            if (!success)
                Logging.outputLine("Error: configuration file command script could not be found!");

            return success;
        }

        private static Boolean getUAVInfo()
        {
            LocationA = new GeoCoordinates(
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationA][Constants.configFieldLat],
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationA][Constants.configFieldLon],
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationA][Constants.configFieldAlt]);
            try
            {
                COMPort = (String)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldComPort];
                TakeoffAltitude = (int)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldTakeoffAltitude];
                LocationA = new GeoCoordinates(
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationA][Constants.configFieldLat],
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationA][Constants.configFieldLon],
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationA][Constants.configFieldAlt]);
                LocationB = new GeoCoordinates(
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationB][Constants.configFieldLat],
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationB][Constants.configFieldLon],
                (double)ConfigJSON[Constants.configFieldUAVInfo][Constants.configFieldLocationB][Constants.configFieldAlt]);
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - UAV info: " + ex.Message);
                return false;
            }

            return true;
        }

        private static Boolean getUserInfo()
        {
            try
            {
                UserName = (String)ConfigJSON[Constants.configFieldUserInfo][Constants.configFieldUserName];
                Password = (String)ConfigJSON[Constants.configFieldUserInfo][Constants.configFieldPassword];
                ProfileName = (String)ConfigJSON[Constants.configFieldUserInfo][Constants.configFieldProfileName];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - user info: " + ex.Message);
                return false;
            }

            return true;
        }

        private static Boolean getCommandThresholds()
        {
            try
            {
                CommandProcessing.ActiveCommandThreshold = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldActiveCommandThreshold];
                CommandProcessing.InactiveCommandThreshold = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldInactiveCommandThreshold];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - command thresholds: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}
