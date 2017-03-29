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
        public static String ScriptPush { get; private set; }
        public static String ScriptPull { get; private set; }
        public static String ScriptRaise { get; private set; }
        public static String ScriptLower { get; private set; }

        public static String UserName { get; private set; }
        public static String Password { get; private set; }
        public static String ProfileName { get; private set; }

        private static JObject ConfigJSON { get; set; }

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
                success = getCommandScripts();
            if (success)
                success = getUserInfo();
            if (success)
                success = getCommandThresholds();

            return success;
        }

        private static Boolean getCommandScripts()
        {
            Boolean success = true;

            try
            {
                ScriptPush = (String)ConfigJSON[Constants.configFieldCommandScripts][Constants.cmdPush];
                ScriptPull = (String)ConfigJSON[Constants.configFieldCommandScripts][Constants.cmdPull];
                ScriptRaise = (String)ConfigJSON[Constants.configFieldCommandScripts][Constants.cmdRaise];
                ScriptLower = (String)ConfigJSON[Constants.configFieldCommandScripts][Constants.cmdLower];
            }
            catch (Exception ex)
            {
                Logging.outputLine("Config read exception - command scripts: " + ex.Message);
                return false;
            }

            // Test whether script files exist
            if (success)
                success = File.Exists(ScriptPush) || (String.Compare(ScriptPush, "") == 0);
            if (success)
                success = File.Exists(ScriptPull) || (String.Compare(ScriptPull, "") == 0);
            if (success)
                success = File.Exists(ScriptRaise) || (String.Compare(ScriptRaise, "") == 0);
            if (success)
                success = File.Exists(ScriptLower) || (String.Compare(ScriptLower, "") == 0);

            if (!success)
                Logging.outputLine("Error: not all configuration file command scripts could be found!");

            return success;
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
