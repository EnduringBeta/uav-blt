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
        static System.Timers.Timer listenCommandTimer = null;
        static System.Timers.Timer listenEmotionTimer = null;

        const int commandPowerPrintingPeriod = 5; // Print cumulative command powers once per second
        const int eventMillisecondProcessingTime = 100;

        // http://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        // Check for events 5 times per second
        private static Single listeningCommandMillisecondInterval = 200;
        public static Single ListeningCommandMillisecondInterval
        {
            get
            {
                return listeningCommandMillisecondInterval;
            }
            private set
            {
                listeningCommandMillisecondInterval = value;
                OnStaticPropertyChanged("ListeningCommandMillisecondInterval");
            }
        }

        // Check for EEG frequency 5 times per second
        private static Single listeningEmotionMillisecondInterval = 400;
        public static Single ListeningEmotionMillisecondInterval
        {
            get
            {
                return listeningEmotionMillisecondInterval;
            }
            private set
            {
                listeningEmotionMillisecondInterval = value;
                OnStaticPropertyChanged("ListeningEmotionMillisecondInterval");
            }
        }

        private static uint emotivUserID = 0;
        public static uint EmotivUserID
        {
            get
            {
                return emotivUserID;
            }
            set
            {
                emotivUserID = value;
                OnStaticPropertyChanged("EmotivUserID");
            }
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

                Utils.EnableTransmit = CommandComms.IsDeviceConnected && IsListening;
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

        private static int eventsProcessedThisInterval = 0;
        public static int EventsProcessedThisInterval
        {
            get
            {
                return eventsProcessedThisInterval;
            }
            set
            {
                eventsProcessedThisInterval = value;
                OnStaticPropertyChanged("EventsProcessedThisInterval");
            }
        }

        public static Boolean initialize()
        {
            engine = EmoEngine.Instance;
            Logging.outputLine("Emo engine initialized");
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

            /*
            engine.MentalCommandTrainingStarted +=
                new EmoEngine.MentalCommandTrainingStartedEventEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingStarted);
            engine.MentalCommandTrainingSucceeded +=
                new EmoEngine.MentalCommandTrainingSucceededEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingSucceeded);
            engine.MentalCommandTrainingCompleted +=
                new EmoEngine.MentalCommandTrainingCompletedEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingCompleted);
            engine.MentalCommandTrainingRejected +=
                new EmoEngine.MentalCommandTrainingRejectedEventHandler(EmotivDeviceEvents.engine_MentalCommandTrainingRejected);
            */

            Logging.outputLine("Emo engine events hooked");

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
            TotalListeningTicks = 0;

            if (listenCommandTimer == null)
                setupCommandTimer();
            if (listenEmotionTimer == null)
                setupEmotionTimer();

            IsListening = true;
            Logging.outputLine("Listening...");

            Logging.outputLine(String.Format("[Event]:\t\t{0,15}\t{1,10}\t{2,10}\t{3,8}", "[Command]", "[Power]", "[Seconds]", "[Active?]"));

            listenCommandTimer.Start();
            listenEmotionTimer.Start();

            return true;
        }

        public static Boolean stopListening()
        {
            if (!IsListening)
            {
                Logging.outputLine("Attempted to stop listening while not listening!");
                return false;
            }

            listenCommandTimer.Stop();
            listenEmotionTimer.Stop();

            IsListening = false;
            Logging.outputLine("Stopped listening");

            Utils.updateStatusBarText(Constants.noActiveCommands);

            return true;
        }

        private static Boolean setupCommandTimer()
        {
            listenCommandTimer = new System.Timers.Timer();
            listenCommandTimer.Elapsed += listenCommandTimer_Elapsed;
            listenCommandTimer.Interval = ListeningCommandMillisecondInterval;

            return true;
        }

        private static Boolean setupEmotionTimer()
        {
            listenEmotionTimer = new System.Timers.Timer();
            listenEmotionTimer.Elapsed += listenEmotionTimer_Elapsed;
            listenEmotionTimer.Interval = ListeningEmotionMillisecondInterval;

            return true;
        }

        // Periodically-executed function that processes Emotiv command events
        private static void listenCommandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EventsProcessedThisInterval = 0;

            // Process any raw events from device
            engine.ProcessEvents(eventMillisecondProcessingTime);

            // Increment counter tracking time
            TotalListeningTicks++;

            // If no events were processed
            if (EventsProcessedThisInterval == 0)
            {
                // Decay command power by ListeningCommandMillisecondInterval
                CommandProcessing.updateCommandObjects(CommandProcessing.LatestSampleTime
                    + (ListeningCommandMillisecondInterval / Constants.millisecondsInSeconds));
            }

            // Print out cumulative command powers occasionally
            if ((TotalListeningTicks % commandPowerPrintingPeriod) == 0)
                CommandProcessing.printCommandPowers();

            // Get list of commands that have exceeded threshold and are active
            List<CommandProcessing.CommandObject> activeCommands =  CommandProcessing.getActiveCommands();
            String commandsString = "";

            // For each active command
            foreach (CommandProcessing.CommandObject atvCmd in activeCommands)
            {
                // Make success by default true so if not transmitting it is still recorded as active in UI
                Boolean success = true;

                // If transmitting
                if (CommandComms.IsTransmitting)
                {
                    // Send command
                    success = CommandComms.sendCommand(atvCmd.command.ToString(), atvCmd.power);

                    // Fractionally reduce power to prevent spam when transmitting
                    atvCmd.reducePower();
                }

                if (success)
                {
                    // Prepare UI string
                    if (String.Compare(commandsString, "") == 0)
                        commandsString += atvCmd.command.ToString();
                    else
                        commandsString += ", " + atvCmd.command.ToString();
                }
                else
                {
                    Logging.outputLine("Error: failure when sending command - " + atvCmd.command.ToString());
                }
            }

            // Update UI
            if (String.Compare(commandsString, "") == 0)
                Utils.updateStatusBarText(Constants.noActiveCommands);
            else
                Utils.updateStatusBarText(commandsString);

            // If total listening time is set and has expired, stop
            if (ListeningMillisecondLength > 0 &&
                TotalListeningTicks * ListeningCommandMillisecondInterval >= ListeningMillisecondLength)
                stopListening();
        }

        // Periodically-executed function that processes Emotiv command events
        private static void listenEmotionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Process averaged band powers for emotional/stressed state
            for (int i = 0; i < EmotionProcessing.EmotivChannelList.Length; i++)
            {
                EmotionProcessing.EmotionDataPoint dp = new EmotionProcessing.EmotionDataPoint();

                // Get and process averaged EEG data for tracking emotional/stressed state
                if (engine.IEE_GetAverageBandPowers(EmotivUserID, EmotionProcessing.EmotivChannelList[i], dp.theta, dp.alpha, dp.lowBeta, dp.highBeta, dp.gamma) == EdkDll.EDK_OK)
                {
                    // Optionally print
                    if (Constants.logIndividualEmotivEmotionEvents)
                        Logging.outputLine(String.Format("Received EEG: [{5,7}]\tT - {0,8:N2}\tA - {1,8:N2}\tb - {2,8:N2}\tB - {3,8:N2}\tG - {4,8:N}",
                            dp.theta[0], dp.alpha[0], dp.lowBeta[0], dp.highBeta[0], dp.gamma[0], EmotionProcessing.EmotivChannelList[i]));

                    // Process and plot data
                    EmotionProcessing.addNewSample(dp, CommandProcessing.LatestSampleTime);
                }
            }

            // If total listening time is set and has expired, stop
            if (ListeningMillisecondLength > 0 &&
                TotalListeningTicks * ListeningEmotionMillisecondInterval >= ListeningMillisecondLength)
                stopListening();
        }
    }
}
