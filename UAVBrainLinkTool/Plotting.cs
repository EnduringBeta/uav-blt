using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class Plotting
    {
        // Past seconds to show in plot window
        public const double commandPlotTimeWindow = 30.0;
        private const double thresholdViewMultiplier = 1.5;

        public static PlotModel CommandPlotModel { get; private set; }
        // List of data series to accomodate multiple simultaneous command data
        public static List<FunctionSeries> CommandPlotData { get; private set; }

        public static Boolean initPlot()
        {
            CommandPlotModel = new PlotModel { Title = "Mental Command Plot" };
            CommandPlotData = new List<FunctionSeries>();

            // Create "Neutral" initial data
            //List<DataPoint> initDP = new List<DataPoint>();
            //addPlotData(initDP, Constants.cmdNeutral);

            // Create threshold line
            // TODO: Create function that updates this over time
            FunctionSeries thresholdLine = new FunctionSeries((x) => CommandProcessing.ActiveCommandThreshold, 0.0, commandPlotTimeWindow, commandPlotTimeWindow);
            CommandPlotData.Add(thresholdLine);
            CommandPlotModel.Series.Add(thresholdLine);

            // Set up axes after data is put in
            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.AbsoluteMinimum = 0.0;
            xAxis.MinimumRange = commandPlotTimeWindow;
            xAxis.MaximumRange = commandPlotTimeWindow;
            xAxis.Title = "Time (s)";
            CommandPlotModel.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.AbsoluteMinimum = -0.5;
            yAxis.AbsoluteMaximum = CommandProcessing.ActiveCommandThreshold * thresholdViewMultiplier;
            yAxis.MaximumRange = CommandProcessing.ActiveCommandThreshold * thresholdViewMultiplier + 0.5;
            yAxis.MinimumRange = CommandProcessing.ActiveCommandThreshold * thresholdViewMultiplier + 0.5;
            yAxis.Title = "Command Power";
            CommandPlotModel.Axes.Add(yAxis);

            return true;
        }

        public static Boolean addPlotData(List<DataPoint> newData, String tag)
        {
            FunctionSeries series = null;
            // Find proper series
            foreach (FunctionSeries fs in CommandPlotData)
            {
                if (String.Compare((String)fs.Tag, tag) == 0)
                {
                    series = fs;
                    break;
                }
            }

            // If "series" still null, create new data series
            if (series == null)
            {
                FunctionSeries newFS = new FunctionSeries();
                newFS.Tag = tag;
                CommandPlotData.Add(newFS);
                series = CommandPlotData.Last();
                CommandPlotModel.Series.Add(series);
            }

            // Find most recent existing time for later window panning
            Double previousLatestTime = 0.0;
            if (series.Points.Count > 0)
                previousLatestTime = series.Points.Last().X;

            Double latestTime = 0.0;
            // Add new data points and
            // find most recent data point in time
            foreach (DataPoint newDP in newData)
            {
                series.Points.Add(newDP);

                if (newDP.X > latestTime)
                    latestTime = newDP.X;
            }

            // Remove old data points
            // Assuming chronologically, so once in window, done
            int lastOldIndex = -1;
            foreach (DataPoint dp in series.Points)
            {
                if (dp.X < latestTime - commandPlotTimeWindow)
                    lastOldIndex = series.Points.IndexOf(dp);
                else
                    break;
            }
            if (lastOldIndex >= 0)
            {
                // Calculate total time being removed
                double timePassed = series.Points[lastOldIndex + 1].X - series.Points.First().X;
                CommandPlotModel.DefaultXAxis.Pan(-timePassed);
                // TODO: Likely not panning enough because this code is only operating on one series!

                // Remove old data points
                series.Points.RemoveRange(0, lastOldIndex + 1);
            }

            // Update plot
            CommandPlotModel.InvalidatePlot(true);

            return true;
        }
    }
}
