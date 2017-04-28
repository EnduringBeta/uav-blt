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

        private static Boolean programLoaded = false;
        public static Boolean ProgramLoaded
        {
            get
            {
                return programLoaded;
            }
            set
            {
                programLoaded = value;
                OnStaticPropertyChanged("ProgramLoaded");
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
            StatusBarText = text;
            Logging.outputLine(text);

            return true;
        }

        public static Boolean initAll(Boolean forceConfigReload = false, Boolean forcePlotReload = false)
        {
            // TODO: These do not update the UI because they are running on the main thread
            updateStatusBarText("Initializing...");

            Boolean success = true;

            if (!Config.ConfigLoaded || forceConfigReload)
                success = Config.importConfig();

            if (forcePlotReload)
                success = Plotting.initPlot();

            if (success)
                success = initEmotivDevice();

            // TODO: Make text blocks bound
            //TextBlockUsername.Text = Config.UserName;
            //TextBlockProfile.Text = Config.ProfileName;
            //TextBlockAntenna.Text = Config.COMPort;

            if (success)
            {
                updateStatusBarText("Starting UAV command script...");
                success = CommandComms.initCommandComms();
            }

            if (success)
            {
                updateStatusBarText("Connecting to Emotiv server and device...");
                success = initEmotivServer();
            }

            if (success)
                updateStatusBarText("BLT initialized!");
            else
                updateStatusBarText("Error: failure during initialization!");

            return success;
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
