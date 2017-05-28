using Emotiv;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class CommandProcessing
    {
        // http://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        // Set in config file
        public static Single CommandThresholdPush { get; set; }
        public static Single CommandThresholdPull { get; set; }
        public static Single CommandThresholdLift { get; set; }
        public static Single CommandThresholdDrop { get; set; }
        public static int CommandSentPowerPercentage { get; set; }
        public static Single SampleTimeWindow { get; set; } // Seconds

        private static List<CommandObject> commandObjectList = new List<CommandObject>();
        public static List<CommandObject> CommandObjectList
        {
            get
            {
                return commandObjectList;
            }
            private set
            {
                commandObjectList = value;
            }
        }

        private static Single latestSampleTime = 0; // Seconds
        public static Single LatestSampleTime
        {
            get
            {
                return latestSampleTime;
            }
            private set
            {
                latestSampleTime = value;
            }
        }

        private static Boolean monitorStress = false;
        public static Boolean MonitorStress
        {
            get
            {
                return monitorStress;
            }
            set
            {
                monitorStress = value;
                OnStaticPropertyChanged("MonitorStress");
            }
        }

        public class CommandObject
        {
            public EdkDll.IEE_MentalCommandAction_t command;
            public Single power = 0;
            public Single threshold = 0; // Each command can have a different threshold
            public Single latestSample = 0;
            public Boolean exceedsThreshold = false;

            public CommandObject(EdkDll.IEE_MentalCommandAction_t inputCommand, Single inputPower = 0, Single inputThreshold = 0, Single inputLatestSample = 0, Boolean inputExceedsThreshold = false)
            {
                command = inputCommand;

                power = inputPower;
                threshold = inputThreshold;
                latestSample = inputLatestSample;
                exceedsThreshold = inputExceedsThreshold;
            }

            public Boolean resetPower()
            {
                power = 0;
                updateExceedsThreshold();

                return true;
            }

            // When command is active and sent, the power is significantly cut to reduce likelihood of spam
            public Boolean reducePower()
            {
                power = power * (CommandSentPowerPercentage / Constants.maxPercent);
                updateExceedsThreshold();

                return true;
            }

            // "newSamplePower" is used when sample is for this command. Otherwise it does nothing.
            public Boolean addNewSample(Single newSampleTime, Single newSamplePower = 0)
            {
                // Skip sample if (presumably) received out of order
                if (LatestSampleTime > newSampleTime)
                {
                    // NOTE: Occasionally occurs when samples received out of order
                    Logging.outputLine("Warning: time not increasing linearly! Check for relativity issues.");
                    return false;
                }

                power = calcPowerDecrease(newSampleTime - LatestSampleTime) * power + newSamplePower;

                // Only update latestSample if this command's sample
                if (newSamplePower > 0)
                {
                    latestSample = newSampleTime;
                }

                // Update UI plot for appropriate command
                List<DataPoint> plotDP = new List<DataPoint>();
                plotDP.Add(new DataPoint((double)newSampleTime, (double)power));
                CommandPlotting.addPlotData(plotDP, this.command.ToString());

                updateExceedsThreshold();

                return true;
            }

            private Single calcPowerDecrease(Single timeDelta)
            {
                // Using linear function that is 1 at 0s and 0 at 3s
                Single result = (-1 / SampleTimeWindow) * timeDelta + 1;

                // Return calculated result, minimum 0
                return result > 0 ? result : 0;
            }

            private Boolean updateExceedsThreshold()
            {
                // If threshold is 0, command will never be active
                if (threshold == 0)
                {
                    exceedsThreshold = false;
                    return true;
                }

                if (EmotionProcessing.IsStressed && CommandProcessing.MonitorStress)
                {
                    exceedsThreshold = power > (threshold * (1 + (EmotionProcessing.StressFactor / Constants.maxPercent)));
                }
                else
                {
                    exceedsThreshold = power > threshold;
                }

                return true;
            }
        }

        // This version is called when a command event occurs
        public static Boolean updateCommandObjects(Single timeFromStart, EdkDll.IEE_MentalCommandAction_t cogAction, Single power, Boolean exceedsThreshold)
        {
            // If command is MC_NEUTRAL, reset command cumulative powers and do not add to CommandObjectList
            if (String.Compare(cogAction.ToString(), Constants.cmdNeutral) == 0)
            {
                foreach (CommandObject cmdObj in CommandObjectList)
                {
                    cmdObj.resetPower();
                }
            }
            // If command is anything else, update all command objects appropriately
            else
            {
                Boolean foundCommand = false;
                foreach (CommandObject cmdObj in CommandObjectList)
                {
                    // If command object matches the current sample
                    if (String.Compare(cmdObj.command.ToString(), cogAction.ToString()) == 0)
                    {
                        // Add its power while decreasing existing power
                        cmdObj.addNewSample(timeFromStart, power);
                        foundCommand = true;
                    }
                    // If command object does not match the current sample
                    else
                    {
                        // Decrease existing power
                        cmdObj.addNewSample(timeFromStart);
                    }
                }

                // If command object is new and not empty
                if (!foundCommand)
                {
                    // Find appropriate threshold to initialize object
                    Single threshold = 0;
                    switch (cogAction.ToString())
                    {
                        case Constants.cmdPush:
                            threshold = CommandProcessing.CommandThresholdPush;
                            break;
                        case Constants.cmdPull:
                            threshold = CommandProcessing.CommandThresholdPull;
                            break;
                        case Constants.cmdLift:
                            threshold = CommandProcessing.CommandThresholdLift;
                            break;
                        case Constants.cmdDrop:
                            threshold = CommandProcessing.CommandThresholdDrop;
                            break;
                        default:
                            threshold = 0;
                            break;
                    }

                    // "exceedsThreshold" from device used differently than "exceedsThreshold" in this program
                    CommandObjectList.Add(new CommandObject(cogAction, power, threshold, timeFromStart));
                }
            }

            // Always update LatestSampleTime after processing
            LatestSampleTime = timeFromStart;

            return true;
        }

        // This version is called when not triggered by a command event
        public static Boolean updateCommandObjects(Single timeFromStart)
        {
            // Decrease existing power from passage of time
            foreach (CommandObject cmdObj in CommandObjectList)
                cmdObj.addNewSample(timeFromStart);

            // Always update LatestSampleTime after processing
            LatestSampleTime = timeFromStart;

            return true;
        }

        public static List<CommandObject> getActiveCommands()
        {
            List<CommandObject> activeCommandList = new List<CommandObject>();

            foreach (CommandObject cmdObj in CommandObjectList)
                if (cmdObj.exceedsThreshold)
                    activeCommandList.Add(cmdObj);

            return activeCommandList;
        }

        public static Boolean printCommandPowers()
        {
            foreach (CommandObject cmdObj in CommandObjectList)
            {
                Logging.outputLine(String.Format("Command power:\t{0,15}\t{1,10:N2}\t\t\t\t{2,8}", cmdObj.command, cmdObj.power, cmdObj.exceedsThreshold ? "Active" : "Inactive"));
            }

            return true;
        }
    }
}
