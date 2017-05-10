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
    public static class EmotionPlotting
    {
        // TODO: Adjust
        private static double emotionPlotRange = 10.0;

        private static double latestDataPointTime = 0.0;

        public static PlotModel EmotionPlotModel { get; private set; }
        // List of data series to accomodate multiple simultaneous emotion data
        public static List<FunctionSeries> EmotionPlotData { get; private set; }

        public static Boolean initPlot()
        {
            EmotionPlotModel = new PlotModel { Title = "Emotional State Plot" };
            EmotionPlotData = new List<FunctionSeries>();

            // Set up axes
            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.AbsoluteMinimum = 0.0;
            xAxis.MinimumRange = Plotting.plotTimeWindow;
            xAxis.MaximumRange = Plotting.plotTimeWindow;
            xAxis.IsPanEnabled = false;
            xAxis.IsZoomEnabled = false;
            xAxis.Title = "Time (s)";
            EmotionPlotModel.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.AbsoluteMinimum = -0.5;
            yAxis.AbsoluteMaximum = emotionPlotRange;
            yAxis.MaximumRange = emotionPlotRange + 0.5;
            yAxis.MinimumRange = emotionPlotRange + 0.5;
            yAxis.IsPanEnabled = false;
            yAxis.IsZoomEnabled = false;
            yAxis.Title = "Emotional Intensity";
            EmotionPlotModel.Axes.Add(yAxis);

            // TODO: Adjust
            // Create threshold line
            FunctionSeries thresholdLine = addDataSeries(Constants.thresholdTag);
            thresholdLine.Points.Add(new DataPoint(0.0, CommandProcessing.ActiveCommandThreshold));
            thresholdLine.Points.Add(new DataPoint(Plotting.plotTimeWindow, CommandProcessing.ActiveCommandThreshold));

            return true;
        }

        private static FunctionSeries addDataSeries(String tag)
        {
            FunctionSeries newSeries = new FunctionSeries();
            newSeries.Tag = tag;
            newSeries.Color = getSeriesColor(tag);
            EmotionPlotData.Add(newSeries);
            EmotionPlotModel.Series.Add(newSeries);

            return newSeries;
        }

        public static Boolean addPlotData(List<DataPoint> newData, String tag)
        {
            // Find proper series
            FunctionSeries series = EmotionPlotData.Find(x => String.Compare((String)x.Tag, tag) == 0);

            // If "series" still null, create new data series
            if (series == null)
                series = addDataSeries(tag);

            Double newLatestTime = 0.0;
            // Add new data points and
            // find most recent data point in time
            foreach (DataPoint newDP in newData)
            {
                series.Points.Add(newDP);

                if (newDP.X > newLatestTime)
                    newLatestTime = newDP.X;
            }

            removeOldData(newLatestTime);

            latestDataPointTime = newLatestTime;

            // Update plot
            EmotionPlotModel.InvalidatePlot(true);

            return true;
        }

        private static Boolean removeOldData(double newLatestTime)
        {
            Boolean shouldPan = false;

            // Remove old data points from all data series
            // Assuming chronologically, so once in window, done
            foreach (FunctionSeries fs in EmotionPlotData)
            {
                int lastOldIndex = -1;
                foreach (DataPoint dp in fs.Points)
                {
                    if (dp.X < newLatestTime - Plotting.plotTimeWindow)
                    {
                        if (String.Compare((String)fs.Tag, Constants.thresholdTag) == 0)
                        {
                            updateThresholdSeries(newLatestTime);
                            break;
                        }
                        else
                            lastOldIndex = fs.Points.IndexOf(dp);
                    }
                    else
                        break;
                }
                if (lastOldIndex >= 0)
                {
                    // Remove old data points
                    fs.Points.RemoveRange(0, lastOldIndex + 1);

                    shouldPan = true;
                }
            }

            // Pan window
            if (shouldPan)
            {
                adjustPlotWindow(newLatestTime);
            }

            return true;
        }

        private static Boolean adjustPlotWindow(double newLatestTime)
        {
            double panAmount = EmotionPlotModel.DefaultXAxis.Transform(-(newLatestTime - latestDataPointTime) + EmotionPlotModel.DefaultXAxis.Offset);
            EmotionPlotModel.DefaultXAxis.Pan(panAmount);

            return true;
        }

        private static Boolean updateThresholdSeries(double newLatestTime)
        {
            FunctionSeries thresholdLine = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdTag) == 0);
            thresholdLine.Points[0] = new DataPoint(newLatestTime - Plotting.plotTimeWindow, CommandProcessing.ActiveCommandThreshold);
            thresholdLine.Points[1] = new DataPoint(newLatestTime, CommandProcessing.ActiveCommandThreshold);

            return true;
        }

        private static OxyColor getSeriesColor(String tag)
        {
            switch (tag)
            {
                case Constants.cmdPush:
                    return Constants.colorPlotCmdPush;
                case Constants.cmdPull:
                    return Constants.colorPlotCmdPull;
                case Constants.cmdLift:
                    return Constants.colorPlotCmdLift;
                case Constants.cmdDrop:
                    return Constants.colorPlotCmdDrop;
                case Constants.cmdNeutral: // Not currently used
                    return Constants.colorPlotCmdNeutral;
                case Constants.thresholdTag:
                    return Constants.colorPlotThreshold;
                default:
                    return Constants.colorPlotDefault;
            }
        }
    }
}
