using Emotiv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

// Wrapper and class for holding methods to communicate with device

namespace UAVBrainLinkTool
{
    public static class EmotivDeviceComms
    {
        static EmoEngine engine;
        static System.Timers.Timer listenTimer = null;

        const int listeningMillisecondInterval = 100; // Check for events 10 times per second
        const int eventProcessingTime = 50;

        // http://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        private static Boolean isListening = false;
        public static Boolean IsListening
        {
            get
            {
                return isListening;
            }
            private set
            {
                isListening = value;
                OnStaticPropertyChanged("IsListening");
            }
        }

        private static int listeningMillisecondLength = 0;
        public static int ListeningMillisecondLength
        {
            get
            {
                return listeningMillisecondLength;
            }
            private set
            {
                listeningMillisecondLength = value;
                OnStaticPropertyChanged("ListeningMillisecondLength");
            }
        }

        private static int totalListeningTicks = 0;
        public static int TotalListeningTicks
        {
            get
            {
                return totalListeningTicks;
            }
            private set
            {
                totalListeningTicks = value;
                OnStaticPropertyChanged("TotalListeningTicks");
            }
        }

        private static String activeCommandsText = Constants.noActiveCommands;
        public static String ActiveCommandsText
        {
            get
            {
                return activeCommandsText;
            }
            set
            {
                activeCommandsText = value;
                OnStaticPropertyChanged("ActiveCommandsText");
            }
        }

        public static Boolean initialize()
        {
            engine = EmoEngine.Instance;
            Logging.outputLine("Emo engine initialized.");
            return true;
        }

        public static Boolean hookEvents()
        {
            engine.EmoEngineConnected +=
            new EmoEngine.EmoEngineConnectedEventHandler(EmotivDeviceEvents.engine_EmoEngineConnected);
            engine.EmoEngineDisconnected +=
                new EmoEngine.EmoEngineDisconnectedEventHandler(EmotivDeviceEvents.engine_EmoEngineDisconnected);

            engine.UserAdded +=
                new EmoEngine.UserAddedEventHandler(EmotivDeviceEvents.engine_UserAdded);
            engine.UserRemoved +=
                new EmoEngine.UserRemovedEventHandler(EmotivDeviceEvents.engine_UserRemoved);

            engine.EmoStateUpdated +=
                new EmoEngine.EmoStateUpdatedEventHandler(EmotivDeviceEvents.engine_EmoStateUpdated);
            engine.EmoEngineEmoStateUpdated +=
                new EmoEngine.EmoEngineEmoStateUpdatedEventHandler(EmotivDeviceEvents.engine_EmoEngineEmoStateUpdated);

            engine.MentalCommandEmoStateUpdated +=
                new EmoEngine.MentalCommandEmoStateUpdatedEventHandler(EmotivDeviceEvents.engine_MentalCommandEmoStateUpdated);
            engine.MentalCommandTrainingStarted +=
                new EmoEngine.MentalCommandTrainingStartedEventEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingStarted);
            engine.MentalCommandTrainingSucceeded +=
                new EmoEngine.MentalCommandTrainingSucceededEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingSucceeded);
            engine.MentalCommandTrainingCompleted +=
                new EmoEngine.MentalCommandTrainingCompletedEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingCompleted);
            engine.MentalCommandTrainingRejected +=
                new EmoEngine.MentalCommandTrainingRejectedEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingRejected);

            Logging.outputLine("Emo engine events hooked.");

            return true;
        }

        public static Boolean connectToDevice()
        {
            try
            {
                engine.Connect();
            }
            catch (Exception e)
            {
                Logging.outputLine("Device connection exception: " + e.Message);
                return false;
            }
            return true;
        }

        public static Boolean disconnectFromDevice()
        {
            try
            {
                engine.Disconnect();
            }
            catch (Exception e)
            {
                Logging.outputLine("Device disconnection exception: " + e.Message);
                return false;
            }
            return true;
        }

        public static Boolean startListening(int millisecondLength = 0)
        {
            if (IsListening)
            {
                Logging.outputLine("Attempted to listen while already listening!");
                return false;
            }

            ListeningMillisecondLength = millisecondLength;

            if (listenTimer == null)
                setupTimer();

            IsListening = true;
            Logging.outputLine("Listening...");

            listenTimer.Start();

            return true;
        }

        public static Boolean stopListening()
        {
            if (!IsListening)
            {
                Logging.outputLine("Attempted to stop listening while not listening!");
                return false;
            }

            listenTimer.Stop();

            IsListening = false;
            Logging.outputLine("Stopped listening.");

            return true;
        }

        private static Boolean setupTimer()
        {
            listenTimer = new System.Timers.Timer();
            listenTimer.Elapsed += listenTimer_Elapsed;
            listenTimer.Interval = listeningMillisecondInterval;

            return true;
        }

        private static void listenTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Process any raw events from device
            engine.ProcessEvents(eventProcessingTime);
            // Increment counter tracking time
            TotalListeningTicks++;

            // Get list of commands that have exceeded threshold and are active
            List<CommandProcessing.CommandObject> activeCommands =  CommandProcessing.getActiveCommands();
            String commandsString = "";

            // For each active command
            foreach (CommandProcessing.CommandObject atvCmd in activeCommands)
            {
                // Send command
                CommandComms.sendCommand(atvCmd.command.ToString(), atvCmd.power);

                // Prepare UI string
                if (String.Compare(commandsString, "") == 0)
                    commandsString += atvCmd.command.ToString();
                else
                    commandsString += Constants.lineBreak + atvCmd.command.ToString();
            }

            // Update UI
            if (String.Compare(commandsString, "") == 0)
                ActiveCommandsText = Constants.noActiveCommands;
            else
                ActiveCommandsText = commandsString;

            // If total listening time is set and has expired, stop
            if (ListeningMillisecondLength > 0 &&
                TotalListeningTicks * listeningMillisecondInterval >= ListeningMillisecondLength)
                stopListening();
        }
    }
}
