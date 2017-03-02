using Emotiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class EmotivDeviceEvents
    {
        public static void engine_EmoEngineConnected(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine("Emo engine connected.");
        }

        public static void engine_EmoEngineDisconnected(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine("Emo engine disconnected.");
        }

        public static void engine_UserAdded(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine(String.Format("User added ({0})", e.userId));
        }

        public static void engine_UserRemoved(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine(String.Format("User removed ({0})", e.userId));
        }

        public static void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;

            Single timeFromStart = es.GetTimeFromStart();
        }

        public static void engine_EmoEngineEmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;

            Single timeFromStart = es.GetTimeFromStart();

            Int32 headsetOn = es.GetHeadsetOn();

            EdkDll.IEE_SignalStrength_t signalStrength = es.GetWirelessSignalStatus();
            Int32 chargeLevel = 0;
            Int32 maxChargeLevel = 0;
            es.GetBatteryChargeLevel(out chargeLevel, out maxChargeLevel);
        }

        public static void engine_MentalCommandEmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;

            Single timeFromStart = es.GetTimeFromStart();

            EdkDll.IEE_MentalCommandAction_t cogAction = es.MentalCommandGetCurrentAction();
            Single power = es.MentalCommandGetCurrentActionPower();
            Boolean isActive = es.MentalCommandIsActive();

            Logging.outputLine(String.Format("Received:\t\t{0,15}\t{1,10:N2}\t{2,10:N2}s\t{3,8}", cogAction, power, timeFromStart, isActive ? "Active" : "Inactive"));
            EmotivDeviceComms.EventsProcessedThisInterval++;

            CommandProcessing.updateCommandObjects(timeFromStart, cogAction, power, isActive);
        }

        public static void engine_MentalCommandTrainingStarted(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine("Start MentalCommand Training");
        }

        public static void engine_MentalCommandTrainingSucceeded(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine("MentalCommand Training Success. (A)ccept/Reject?");
            ConsoleKeyInfo cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.A)
            {
                Logging.outputLine("Accept!!!");
                EmoEngine.Instance.MentalCommandSetTrainingControl(0, EdkDll.IEE_MentalCommandTrainingControl_t.MC_ACCEPT);
            }
            else
            {
                EmoEngine.Instance.MentalCommandSetTrainingControl(0, EdkDll.IEE_MentalCommandTrainingControl_t.MC_REJECT);
            }
        }

        public static void engine_MentalCommandTrainingCompleted(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine("MentalCommand Training Completed.");
        }

        public static void engine_MentalCommandTrainingRejected(object sender, EmoEngineEventArgs e)
        {
            Logging.outputLine("MentalCommand Training Rejected.");
        }
    }
}
