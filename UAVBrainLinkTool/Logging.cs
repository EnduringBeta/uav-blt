using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class Logging
    {
        const String logsDirectory = ".\\Logs\\";
        static String logFileName = String.Format(logsDirectory + "BLT_Log_{0}.txt", DateTime.UtcNow.ToString(Constants.dateTimeFormat));
        static System.IO.StreamWriter log;

        public static Boolean LogIndividualEmotivCommandEvents { get; set; }
        public static Boolean LogIndividualEmotivEmotionEvents { get; set; }

        static Logging()
        {
            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            log = new System.IO.StreamWriter(logFileName);
        }

        public static Boolean outputLine(String text = Constants.lineBreak)
        {
            if (String.Compare(text, Constants.lineBreak) != 0)
                text = DateTime.UtcNow.ToString(Constants.dateTimeFormat) + ": " + text;

            log.WriteLine(text);
            log.Flush();

            if (Constants.logToConsole)
                Console.WriteLine(text);

            return true;
        }
    }
}
