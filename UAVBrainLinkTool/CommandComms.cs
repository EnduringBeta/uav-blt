using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class CommandComms
    {
        public static Boolean sendCommand(String commandString, Single commandPower)
        {
            if (String.Compare(commandString, Constants.cmdNeutral) == 0)
            {
                Logging.outputLine(String.Format("Attempted to send command {0}, which is not allowed!", Constants.cmdNeutral));
                return false;
            }

            Logging.outputLine(String.Format("Send Command:\t{0,15}\t{1,10:N2}\t\t\t\t\t\t\t!", commandString, commandPower));

            return true;
        }
    }
}
