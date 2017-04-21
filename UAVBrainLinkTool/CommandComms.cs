using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class CommandComms
    {
        public static List<CommandStatus> CommandStatusList { get; private set; }

        private static Process PythonScriptExecution { get; set; }

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
                // Start the process with info above
                PythonScriptExecution = Process.Start(startInfo);

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

        private static Boolean configurePythonScript()
        {
            Boolean success = sendPythonString(Constants.configFieldComPort + " " + Config.COMPort);
            if (success)
                sendPythonString(Constants.configFieldTakeoffAltitude + " " + Config.TakeoffAltitude);
            if (success)
                sendPythonString(Constants.configFieldLocationA + " " + Config.LocationA.lat + " " + Config.LocationA.lon + " " + Config.LocationA.alt);
            if (success)
                sendPythonString(Constants.configFieldLocationB + " " + Config.LocationB.lat + " " + Config.LocationB.lon + " " + Config.LocationB.alt);

            return success;
        }

        private static Boolean getPythonScriptOutput()
        {
            Boolean success = false;

            // Capture output for printing to log while avoiding hangup based on this advice
            // (http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why)
            String stdError = PythonScriptExecution.StandardError.ReadToEnd(); // FREEZING HERE: LOOK AT NEXT
            String stdOutput = PythonScriptExecution.StandardOutput.ReadToEnd();

            if (!String.IsNullOrEmpty(stdError))
            {
                Logging.outputLine("[Python] Script error:\r\n" + stdError);
                success = false;
            }
            if (!String.IsNullOrEmpty(stdOutput))
            {
                Logging.outputLine("[Python] Script output:\r\n" + stdOutput);
                success = true;
            }

            return success;
        }

        public static Boolean endPythonScript()
        {
            sendPythonString(Constants.scriptExit);
            return true;
        }

        public static Boolean connectUAV()
        {
            sendPythonString(Constants.scriptConnect);
            return true;
        }

        public static Boolean disconnectUAV()
        {
            sendPythonString(Constants.scriptDisconnect);
            return true;
        }

        public static Boolean attributesUAV()
        {
            sendPythonString(Constants.scriptAttributes);
            return true;
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
            }
            catch (Exception ex)
            {
                Logging.outputLine("[Python] Command sending exception for " + scriptString + ":\r\n" + ex.Message);

                success = false;
            }
            finally
            {
                success = getPythonScriptOutput();

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
