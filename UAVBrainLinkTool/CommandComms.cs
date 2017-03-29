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

            if (executePythonScript(commandString, getCommandScript(commandString)))
                Logging.outputLine(String.Format("Sent Command:\t{0,15}\t{1,10:N2}\t\t\t\t\t\t\t!", commandString, commandPower));
            else
                Logging.outputLine(String.Format("Command Fail:\t{0,15}\t{1,10:N2}\t\t\t\t\t\t\t?", commandString, commandPower));

            return cmdObj.isActive;
        }

        private static String getCommandScript(String commandString)
        {
            switch (commandString)
            {
                case Constants.cmdPush:
                    return Config.ScriptPush;
                case Constants.cmdPull:
                    return Config.ScriptPull;
                case Constants.cmdRaise:
                    return Config.ScriptRaise;
                case Constants.cmdLower:
                    return Config.ScriptLower;
                default:
                    Logging.outputLine(String.Format("Warning: attempted to execute unknown command - {0}", commandString));
                    return "";
            }
        }

        // TODO: Ensure called only once, deal with multiple simultaneous commands?
        private static Boolean executePythonScript(String commandString, String scriptString, String arguments = "")
        {
            Boolean success = false;

            if (String.Compare(scriptString, "") == 0)
            {
                Logging.outputLine("Warning: no script specified to execute command.");
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Constants.callPython;
            startInfo.Arguments = String.Format("{0} {1}", scriptString, arguments);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            String stdOutput = ""; String stdError = "";
            try
            {
                // Start the process with info above
                using (Process exeProcess = Process.Start(startInfo))
                {
                    // Capture output for printing to log while avoiding hangup based on this advice
                    // (http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why)
                    stdOutput = exeProcess.StandardOutput.ReadToEnd();
                    stdError = exeProcess.StandardError.ReadToEnd();

                    exeProcess.WaitForExit();
                }
                success = true;
            }
            catch (Exception ex)
            {
                Logging.outputLine("[Python] Event file parsing exception:\r\n" + ex.Message);
                Logging.outputLine("[Python] Script working directory: " + startInfo.WorkingDirectory);
                Logging.outputLine("[Python] Script file name: " + startInfo.FileName);
                Logging.outputLine("[Python] Script calling arguments: " + startInfo.Arguments);

                success = false;
            }
            finally
            {
                if (!String.IsNullOrEmpty(stdError))
                {
                    Logging.outputLine("[Python] Script error:\r\n" + stdError);
                    Logging.outputLine("[Python] Script file name: " + startInfo.FileName);
                    Logging.outputLine("[Python] Script calling arguments: " + startInfo.Arguments);

                    success = false;
                }
                if (!String.IsNullOrEmpty(stdOutput))
                    Logging.outputLine("[Python] Script output:\r\n" + stdOutput);

                // Reset "isActive"
                CommandStatus cmdObj = CommandStatusList.Find(x => String.Compare(x.command.ToString(), commandString) == 0);
                cmdObj.isActive = false;
            }

            return success;
        }
    }
}
