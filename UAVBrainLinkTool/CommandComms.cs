using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class CommandComms
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        private static List<CommandStatus> commandStatusList = new List<CommandStatus>();
        public static List<CommandStatus> CommandStatusList
        {
            get
            {
                return commandStatusList;
            }
            private set
            {
                commandStatusList = value;
                OnStaticPropertyChanged("CommandStatusList");
            }
        }

        private static Process pythonScriptExecution = new Process();
        public static Process PythonScriptExecution
        {
            get
            {
                return pythonScriptExecution;
            }
            private set
            {
                pythonScriptExecution = value;
                OnStaticPropertyChanged("PythonScriptExecution");
            }
        }

        private static Boolean isScriptActive = false;
        public static Boolean IsScriptActive
        {
            get
            {
                return isScriptActive;
            }
            private set
            {
                isScriptActive = value;
                OnStaticPropertyChanged("IsScriptActive");
            }
        }

        private static Boolean isDeviceConnected = false;
        public static Boolean IsDeviceConnected
        {
            get
            {
                return isDeviceConnected;
            }
            private set
            {
                isDeviceConnected = value;
                OnStaticPropertyChanged("IsDeviceConnected");

                Utils.EnableTransmit = IsDeviceConnected && EmotivDeviceComms.IsListening;
            }
        }

        private static Boolean isTransmitting = false;
        public static Boolean IsTransmitting
        {
            get
            {
                return isTransmitting;
            }
            set
            {
                isTransmitting = value;
                OnStaticPropertyChanged("IsTransmitting");
            }
        }

        public class CommandStatus
        {
            public String command = "";
            public Boolean isActive = false;

            public CommandStatus(String inputCommand, Boolean inputIsActive = false)
            {
                command = inputCommand;
                isActive = inputIsActive;
            }
        }

        public static Boolean initCommandComms()
        {
            CommandStatusList = new List<CommandStatus>();

            startPythonScript();

            return true;
        }

        public static Boolean startPythonScript()
        {
            Boolean success = false;

            String arguments = "";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Constants.callPython;
            startInfo.Arguments = String.Format("{0} {1}", Config.CommandScript, arguments);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            // TODO: Create task to read standard output and error from Python script
            // Currently only getting output when sending commands
            try
            {
                // Setup the process with info above and add handlers
                PythonScriptExecution = new Process();
                PythonScriptExecution.StartInfo = startInfo;
                PythonScriptExecution.ErrorDataReceived += PythonScriptExecution_ErrorDataReceived;
                PythonScriptExecution.OutputDataReceived += PythonScriptExecution_OutputDataReceived;
                PythonScriptExecution.Exited += PythonScriptExecution_Exited;
                PythonScriptExecution.EnableRaisingEvents = true;

                // Toggle flag indicating script is active
                IsScriptActive = true;

                // Start the process
                PythonScriptExecution.Start();
                PythonScriptExecution.BeginErrorReadLine();
                PythonScriptExecution.BeginOutputReadLine();

                // Configure script with commands
                success = configurePythonScript();
            }
            catch (Exception ex)
            {
                Logging.outputLine("[Python] Command sending exception:\r\n" + ex.Message);
                Logging.outputLine("[Python] Script working directory: " + startInfo.WorkingDirectory);
                Logging.outputLine("[Python] Script file name: " + startInfo.FileName);
                Logging.outputLine("[Python] Script calling arguments: " + startInfo.Arguments);

                return false;
            }

            return success;
        }

        static void PythonScriptExecution_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            String newLine = e.Data;
            if (!String.IsNullOrEmpty(newLine))
                Logging.outputLine("[Python] Script error: " + newLine);
        }

        static void PythonScriptExecution_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            String newLine = e.Data;
            if (!String.IsNullOrEmpty(newLine))
                Logging.outputLine("[Python] Script output: " + newLine);
        }

        static void PythonScriptExecution_Exited(object sender, EventArgs e)
        {
            IsScriptActive = false;
            IsDeviceConnected = false;
            Logging.outputLine("Warning: script has exited.");
        }

        private static Boolean configurePythonScript()
        {
            Boolean success = sendPythonString(Constants.scriptConfigComPort + " " + Config.COMPort);
            if (success)
                success = sendPythonString(Constants.scriptConfigTakeoffAltitude + " " + Config.TakeoffAltitude);
            if (success)
                success = sendPythonString(Constants.scriptConfigLocationA + " " + Config.LocationA.lat + " " + Config.LocationA.lon + " " + Config.LocationA.alt);
            if (success)
                success = sendPythonString(Constants.scriptConfigLocationB + " " + Config.LocationB.lat + " " + Config.LocationB.lon + " " + Config.LocationB.alt);

            return success;
        }

        public static Boolean endPythonScript()
        {
            return sendPythonString(Constants.scriptExit);

            // Set in PythonScriptExecution_Exited
            //IsScriptActive = false;
            //IsDeviceConnected = false;
        }

        public static Boolean connectUAV()
        {
            if (IsDeviceConnected)
            {
                Logging.outputLine("Warning: attempting to connect device when already connected.");
                return false;
            }

            if (sendPythonString(Constants.scriptConnect))
            {
                IsDeviceConnected = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean disconnectUAV()
        {
            if (!IsDeviceConnected)
            {
                Logging.outputLine("Warning: attempting to disconnect device when not connected.");
                return false;
            }

            if (sendPythonString(Constants.scriptDisconnect))
            {
                IsDeviceConnected = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean attributesUAV()
        {
            return sendPythonString(Constants.scriptAttributes);
        }

        public static Boolean sendCommand(String commandString, Single commandPower)
        {
            CommandStatus cmdObj = CommandStatusList.Find(x => String.Compare(x.command.ToString(), commandString) == 0);

            if (cmdObj == null)
            {
                cmdObj = new CommandStatus(commandString, true);
                CommandStatusList.Add(cmdObj);
            }
            else if (cmdObj.isActive)
            {
                Logging.outputLine(String.Format("Warning: command {0} is already active.", commandString));
                return false;
            }
            else if (String.Compare(commandString, Constants.cmdNeutral) == 0)
            {
                Logging.outputLine(String.Format("Warning: attempted to send command {0}, which is not allowed.", Constants.cmdNeutral));
                return false;
            }

            cmdObj.isActive = true;

            if (sendPythonString(getCommandScriptString(commandString), commandString))
                Logging.outputLine(String.Format("Sent Command:\t{0,15}\t{1,10:N2}\t\t\t\t\t\t\t!", commandString, commandPower));
            else
                Logging.outputLine(String.Format("Command Fail:\t{0,15}\t{1,10:N2}\t\t\t\t\t\t\t?", commandString, commandPower));

            return cmdObj.isActive;
        }

        private static String getCommandScriptString(String commandString)
        {
            switch (commandString)
            {
                case Constants.cmdPush:
                    return Config.StringPush;
                case Constants.cmdPull:
                    return Config.StringPull;
                case Constants.cmdRaise:
                    return Config.StringRaise;
                case Constants.cmdLower:
                    return Config.StringLower;
                default:
                    Logging.outputLine(String.Format("Warning: attempted to execute unknown command - {0}", commandString));
                    return "";
            }
        }

        // TODO: Ensure sent only once, deal with multiple simultaneous commands?
        private static Boolean sendPythonString(String scriptString, String commandString = "")
        {
            Boolean success = false;

            if (String.Compare(scriptString, "") == 0)
            {
                Logging.outputLine("Warning: no string specified to execute command.");
                return false;
            }
            else if (PythonScriptExecution.HasExited)
            {
                Logging.outputLine("Warning: Python script has exited. Attempting to restart it...");
                if (!startPythonScript())
                    return false;
            }

            // Send string to Python script to allow it to execute command
            try
            {
                PythonScriptExecution.StandardInput.WriteLine(scriptString);

                success = true;
            }
            catch (Exception ex)
            {
                Logging.outputLine("[Python] Command sending exception for " + scriptString + ":\r\n" + ex.Message);

                success = false;
            }
            finally
            {
                // Reset "isActive" if mental command (commandString should be non-empty)
                if (String.Compare(commandString, "") != 0)
                {
                    CommandStatus cmdObj = CommandStatusList.Find(x => String.Compare(x.command.ToString(), commandString) == 0);
                    cmdObj.isActive = false;
                }
            }

            return success;
        }
    }
}
