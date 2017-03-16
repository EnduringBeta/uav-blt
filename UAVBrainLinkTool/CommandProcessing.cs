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

        // TODO: Put into config file
        private static Single activeCommandThreshold = 11;
        public static Single ActiveCommandThreshold
        {
            get
            {
                return activeCommandThreshold;
            }
            private set
            {
                activeCommandThreshold = value;
            }
        }

        // TODO: Put into config file
        private static Single inactiveCommandThreshold = 9;
        public static Single InactiveCommandThreshold
        {
            get
            {
                return inactiveCommandThreshold;
            }
            private set
            {
                inactiveCommandThreshold = value;
            }
        }

        private static Single sampleTimeWindow = 3; // Seconds
        public static Single SampleTimeWindow
        {
            get
            {
                return sampleTimeWindow;
            }
            private set
            {
                sampleTimeWindow = value;
            }
        }

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

        public class CommandObject
        {
            public EdkDll.IEE_MentalCommandAction_t command;
            public Single power = 0;
            public Single latestSample = 0;
            public Boolean isActive = false;

            public CommandObject(EdkDll.IEE_MentalCommandAction_t inputCommand, Single inputPower = 0, Single inputLatestSample = 0, Boolean inputIsActive = false)
            {
                command = inputCommand;

                power = inputPower;
                latestSample = inputLatestSample;
                isActive = inputIsActive;
            }

            public Boolean resetPower()
            {
                power = 0;
                updateActive();

                return true;
            }

            // "newSamplePower" is used when sample is for this command. Otherwise it does nothing.
            public Boolean addNewSample(Single newSampleTime, Single newSamplePower = 0)
            {
                // Skip sample if (presumably) received out of order
                if (LatestSampleTime > newSampleTime)
                {
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
                Plotting.addPlotData(plotDP, this.command.ToString());

                updateActive();

                return true;
            }

            private Single calcPowerDecrease(Single timeDelta)
            {
                // Using linear function that is 1 at 0s and 0 at 3s
                Single result = (-1 / SampleTimeWindow) * timeDelta + 1;

                // Return calculated result, minimum 0
                return result > 0 ? result : 0;
            }

            private Boolean updateActive()
            {
                if (power > ActiveCommandThreshold)
                    isActive = true;
                else if (power < InactiveCommandThreshold)
                    isActive = false;

                return true;
            }
        }

        // This version is called when a command event occurs
        public static Boolean updateCommandObjects(Single timeFromStart, EdkDll.IEE_MentalCommandAction_t cogAction, Single power, Boolean isActive)
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
                    // "isActive" from device used differently than "isActive" in this program
                    CommandObjectList.Add(new CommandObject(cogAction, power, timeFromStart));
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
                if (cmdObj.isActive)
                    activeCommandList.Add(cmdObj);

            return activeCommandList;
        }

        public static Boolean printCommandPowers()
        {
            foreach (CommandObject cmdObj in CommandObjectList)
            {
                Logging.outputLine(String.Format("Command power:\t{0,15}\t{1,10:N2}\t\t\t\t{2,8}", cmdObj.command, cmdObj.power, cmdObj.isActive ? "Active" : "Inactive"));
            }

            return true;
        }
    }
}
