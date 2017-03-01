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
            Logging.outputLine(String.Format("Send Command: {0,15}\t{1,10}", commandString, commandPower));

            return true;
        }
    }
}
