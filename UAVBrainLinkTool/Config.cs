using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    class Config
    {
        public static String CommandScript { get; private set; }

        public static String StringPush { get; private set; }
        public static String StringPull { get; private set; }
        public static String StringRaise { get; private set; }
        public static String StringLower { get; private set; }

        public static String COMPort { get; private set; }
        public static int TakeoffAltitude { get; private set; }
        public static GeoCoordinates LocationA { get; private set; }
        public static GeoCoordinates LocationB { get; private set; }

        public static String UserName { get; private set; }
        public static String Password { get; private set; }
        public static String ProfileName { get; private set; }

        private static JObject ConfigJSON { get; set; }

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

            return success;
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
