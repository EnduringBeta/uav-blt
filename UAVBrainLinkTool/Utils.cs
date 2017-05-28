using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class Utils
    {
        // http://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        private static String windowTitle = "UAV Brain Link Tool";
        public static String WindowTitle
        {
            get
            {
                return windowTitle;
            }
            set
            {
                windowTitle = value;
                OnStaticPropertyChanged("WindowTitle");
            }
        }

        private static String statusBarText = "";
        public static String StatusBarText
        {
            get
            {
                return statusBarText;
            }
            private set
            {
                statusBarText = value;
                OnStaticPropertyChanged("StatusBarText");
            }
        }

        private static Boolean emotivReady = false;
        public static Boolean EmotivReady
        {
            get
            {
                return emotivReady;
            }
            set
            {
                emotivReady = value;
                OnStaticPropertyChanged("EmotivReady");
            }
        }

        private static Boolean uavScriptReady = false;
        public static Boolean UAVScriptReady
        {
            get
            {
                return uavScriptReady;
            }
            set
            {
                uavScriptReady = value;
                OnStaticPropertyChanged("UAVScriptReady");
            }
        }

        // For button on UI
        private static Boolean enableTransmit = false;
        public static Boolean EnableTransmit
        {
            get
            {
                return enableTransmit;
            }
            set
            {
                enableTransmit = value;
                OnStaticPropertyChanged("EnableTransmit");
            }
        }

        public static Boolean updateStatusBarText(String text = "")
        {
            // Only output to log and update if new status is different from old
            // This prevents constant reports of no commands from filling up the log
            if (String.Compare(StatusBarText, text) != 0)
            {
                Logging.outputLine(text);
                StatusBarText = text;
                return true;
            }

            // Returns false if not updated
            return false;
        }

        public static Boolean initAll(Boolean forceConfigReload = false, Boolean forcePlotReload = false)
        {
            // TODO: These do not update the UI because they are running on the main thread
            updateStatusBarText("Initializing...");

            Boolean success = true;

            if (!Config.ConfigLoaded || forceConfigReload)
                success = Config.importConfig();

            if (forcePlotReload)
                success = CommandPlotting.initPlot();

            if (success)
                success = initEmotivDevice();

            if (success)
            {
                updateStatusBarText("Connecting to Emotiv server and device...");
                success = initEmotivServer();
            }

            if (success)
            {
                updateStatusBarText("BLT initialized!");
                EmotivReady = true;
            }

            updateStatusBarText("Starting UAV command script...");
            UAVScriptReady = CommandComms.initCommandComms();

            if (!(EmotivReady && UAVScriptReady))
            {
                updateStatusBarText("Error: failure during initialization!");
                return false;
            }
            else
            {
                updateStatusBarText("Initialization complete!");
                return true;
            }
        }

        public static Boolean initEmotivDevice()
        {
            Boolean success = true;

            if (success)
                success = EmotivDeviceComms.initialize();
            if (success)
                success = EmotivServerComms.initialize();

            if (success)
                success = EmotivDeviceComms.hookEvents();
            if (success)
                success = EmotivDeviceComms.connectToDevice();

            return success;
        }

        public static Boolean initEmotivServer()
        {
            Boolean success = true;

            // TODO: If information not available, prompt user
            if (success)
                success = EmotivServerComms.logIn(Config.UserName, Config.Password);
            if (success)
                success = EmotivServerComms.loadUserProfile(Config.ProfileName);

            return success;
        }
    }
}
