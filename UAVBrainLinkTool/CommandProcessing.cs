using Emotiv;
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

            // TODO: Confirm newSampleTime is in seconds, not milliseconds
            // "newSamplePower" is used when sample is for this command. Otherwise it does nothing.
            public Boolean addNewSample(Single newSampleTime, Single newSamplePower = 0)
            {
                power = calcPowerDecrease(newSampleTime - latestSample) * power + newSamplePower;
                latestSample = newSampleTime;

                updateActive();

                return true;
            }

            private Single calcPowerDecrease(Single timeDelta)
            {
                // Using linear function that is 1 at 0s and 0 at 3s
                Single result = -1 / SampleTimeWindow * timeDelta + 1;

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

        public static Boolean updateCommandObjects(Single timeFromStart, EdkDll.IEE_MentalCommandAction_t cogAction, Single power, Boolean isActive)
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

            if (!foundCommand)
                // "isActive" from device used differently than "isActive" in this program
                CommandObjectList.Add(new CommandObject(cogAction, power, timeFromStart));

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
                Logging.outputLine(String.Format("{0}:\t{1}\t{2}", cmdObj.command, cmdObj.power, cmdObj.isActive));
            }

            return true;
        }
    }
}
