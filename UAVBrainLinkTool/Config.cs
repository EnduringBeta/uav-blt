﻿using Newtonsoft.Json.Linq;
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
        public static String StringLift { get; private set; }
        public static String StringDrop { get; private set; }

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
                success = getLogging();
            if (success)
                success = getCommandInfo();
            if (success)
                success = getUAVInfo();
            if (success)
                success = getUserInfo();
            if (success)
                success = getCommandPower();
            if (success)
                success = getStress();

            if (success)
                ConfigLoaded = true;
            else
                ConfigLoaded = false;

            return ConfigLoaded;
        }

        private static Boolean getLogging()
        {
            Boolean success = true;

            try
            {
                Logging.LogIndividualEmotivCommandEvents = (Boolean)ConfigJSON[Constants.configFieldLogging][Constants.configFieldLogIndividualEmotivCommandEvents];
                Logging.LogIndividualEmotivEmotionEvents = (Boolean)ConfigJSON[Constants.configFieldLogging][Constants.configFieldLogIndividualEmotivEmotionEvents];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - logging: " + ex.Message);
                return false;
            }

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
                StringLift = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.cmdLift];
                StringDrop = (String)ConfigJSON[Constants.configFieldCommandScriptInfo][Constants.cmdDrop];
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

        private static Boolean getCommandPower()
        {
            try
            {
                CommandProcessing.CommandThresholdPush = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldCommandThresholdPush];
                CommandProcessing.CommandThresholdPull = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldCommandThresholdPull];
                CommandProcessing.CommandThresholdLift = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldCommandThresholdLift];
                CommandProcessing.CommandThresholdDrop = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldCommandThresholdDrop];
                CommandProcessing.CommandSentPowerPercentage = (int)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldCommandSentPowerPercentage];
                CommandProcessing.SampleTimeWindow = (Single)ConfigJSON[Constants.configFieldCommandThresholds][Constants.configFieldSampleTimeWindow];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - command power: " + ex.Message);
                return false;
            }

            return true;
        }

        private static Boolean getStress()
        {
            try
            {
                EmotionProcessing.ActiveStressThreshold = (Single)ConfigJSON[Constants.configFieldStress][Constants.configFieldActiveStressThreshold];
                EmotionProcessing.InactiveStressThreshold = (Single)ConfigJSON[Constants.configFieldStress][Constants.configFieldInactiveStressThreshold];
                EmotionProcessing.StressFactor = (Single)ConfigJSON[Constants.configFieldStress][Constants.configFieldStressFactor];
                EmotionProcessing.StressTimeWindow = (Single)ConfigJSON[Constants.configFieldStress][Constants.configFieldStressTimeWindow];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - stress: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}
