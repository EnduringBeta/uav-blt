using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    // Class for holding variables that are the same between CommandPlotting and EmotionPlotting
    public static class Plotting
    {
        // Past seconds to show in plot window
        public const double plotTimeWindow = 30.0; // Seconds

        // Initialize both plots
        static public Boolean init()
        {
            return CommandPlotting.initPlot() && EmotionPlotting.initPlot();
        }
    }
}
