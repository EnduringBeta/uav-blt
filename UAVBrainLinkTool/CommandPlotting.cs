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
    public static class CommandPlotting
    {
        private const double thresholdViewMultiplier = 2;

        private static double latestDataPointTime = 0.0;

        public static PlotModel CommandPlotModel { get; private set; }
        // List of data series to accomodate multiple simultaneous command data
        public static List<FunctionSeries> EmotionPlotData { get; private set; }

        public static Boolean initPlot()
        {
            CommandPlotModel = new PlotModel { Title = "Mental Command Plot" };
            EmotionPlotData = new List<FunctionSeries>();

            // Create "Neutral" initial data
            //List<DataPoint> initDP = new List<DataPoint>();
            //addPlotData(initDP, Constants.cmdNeutral);

            // Set up axes
            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.AbsoluteMinimum = 0.0;
            xAxis.MinimumRange = Plotting.plotTimeWindow;
            xAxis.MaximumRange = Plotting.plotTimeWindow;
            xAxis.IsPanEnabled = false;
            xAxis.IsZoomEnabled = false;
            xAxis.Title = "Time (s)";
            CommandPlotModel.Axes.Add(xAxis);

            // Find highest command threshold
            double highestThreshold = 0.0;
            if (CommandProcessing.CommandThresholdPush > highestThreshold)
                highestThreshold = CommandProcessing.CommandThresholdPush;
            if (CommandProcessing.CommandThresholdPull > highestThreshold)
                highestThreshold = CommandProcessing.CommandThresholdPull;
            if (CommandProcessing.CommandThresholdLift > highestThreshold)
                highestThreshold = CommandProcessing.CommandThresholdLift;
            if (CommandProcessing.CommandThresholdDrop > highestThreshold)
                highestThreshold = CommandProcessing.CommandThresholdDrop;

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.AbsoluteMinimum = -0.5;
            yAxis.AbsoluteMaximum = highestThreshold * thresholdViewMultiplier;
            yAxis.MaximumRange = highestThreshold * thresholdViewMultiplier + 0.5;
            yAxis.MinimumRange = highestThreshold * thresholdViewMultiplier + 0.5;
            yAxis.IsPanEnabled = false;
            yAxis.IsZoomEnabled = false;
            yAxis.Title = "Command Power";
            CommandPlotModel.Axes.Add(yAxis);

            // Create threshold lines (create in reverse order so they will be updated/drawn in priority: Push, Pull, Lift, Drop)
            // If threshold is 0 for a command, it is not included in processing
            if (CommandProcessing.CommandThresholdDrop > 0)
            {
                FunctionSeries thresholdLineDrop = addDataSeries(Constants.thresholdDropTag);
                thresholdLineDrop.Points.Add(new DataPoint(0.0, CommandProcessing.CommandThresholdDrop));
                thresholdLineDrop.Points.Add(new DataPoint(Plotting.plotTimeWindow, CommandProcessing.CommandThresholdDrop));
            }
            if (CommandProcessing.CommandThresholdLift > 0)
            {
                FunctionSeries thresholdLineLift = addDataSeries(Constants.thresholdLiftTag);
                thresholdLineLift.Points.Add(new DataPoint(0.0, CommandProcessing.CommandThresholdLift));
                thresholdLineLift.Points.Add(new DataPoint(Plotting.plotTimeWindow, CommandProcessing.CommandThresholdLift));
            }
            if (CommandProcessing.CommandThresholdPull > 0)
            {
                FunctionSeries thresholdLinePull = addDataSeries(Constants.thresholdPullTag);
                thresholdLinePull.Points.Add(new DataPoint(0.0, CommandProcessing.CommandThresholdPull));
                thresholdLinePull.Points.Add(new DataPoint(Plotting.plotTimeWindow, CommandProcessing.CommandThresholdPull));
            }
            if (CommandProcessing.CommandThresholdPush > 0)
            {
                FunctionSeries thresholdLinePush = addDataSeries(Constants.thresholdPushTag);
                thresholdLinePush.Points.Add(new DataPoint(0.0, CommandProcessing.CommandThresholdPush));
                thresholdLinePush.Points.Add(new DataPoint(Plotting.plotTimeWindow, CommandProcessing.CommandThresholdPush));
            }

            return true;
        }

        private static FunctionSeries addDataSeries(String tag)
        {
            FunctionSeries newSeries = new FunctionSeries();
            newSeries.Tag = tag;
            newSeries.Color = getSeriesColor(tag);
            EmotionPlotData.Add(newSeries);
            CommandPlotModel.Series.Add(newSeries);

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
            CommandPlotModel.InvalidatePlot(true);

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
                        // If a threshold line, skip (update all threshold lines later if necessary)
                        if ((String.Compare((String)fs.Tag, Constants.thresholdPushTag) == 0) ||
                            (String.Compare((String)fs.Tag, Constants.thresholdPullTag) == 0) ||
                            (String.Compare((String)fs.Tag, Constants.thresholdLiftTag) == 0) ||
                            (String.Compare((String)fs.Tag, Constants.thresholdDropTag) == 0))
                        {
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
                updateThresholdSeries(newLatestTime);
            }

            return true;
        }

        private static Boolean adjustPlotWindow(double newLatestTime)
        {
            double panAmount = CommandPlotModel.DefaultXAxis.Transform(-(newLatestTime - latestDataPointTime) + CommandPlotModel.DefaultXAxis.Offset);
            CommandPlotModel.DefaultXAxis.Pan(panAmount);

            return true;
        }

        public static Boolean updateThresholdSeries(double newLatestTime)
        {
            double firstPoint  = (newLatestTime > Plotting.plotTimeWindow) ? newLatestTime - Plotting.plotTimeWindow : 0;
            double secondPoint = (newLatestTime > Plotting.plotTimeWindow) ? newLatestTime                           : Plotting.plotTimeWindow;

            // If threshold is 0 for a command, it is not included in processing
            FunctionSeries fs = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdPushTag) == 0);
            if (fs != null)
                updateLine(fs, firstPoint, secondPoint,
                    CommandProcessing.CommandThresholdPush, EmotionProcessing.IsStressed && CommandProcessing.MonitorStress);

            fs = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdPullTag) == 0);
            if (fs != null)
                updateLine(fs, firstPoint, secondPoint,
                    CommandProcessing.CommandThresholdPull, EmotionProcessing.IsStressed && CommandProcessing.MonitorStress);

            fs = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdLiftTag) == 0);
            if (fs != null)
                updateLine(fs, firstPoint, secondPoint,
                    CommandProcessing.CommandThresholdLift, EmotionProcessing.IsStressed && CommandProcessing.MonitorStress);

            fs = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdDropTag) == 0);
            if (fs != null)
                updateLine(fs, firstPoint, secondPoint,
                    CommandProcessing.CommandThresholdDrop, EmotionProcessing.IsStressed && CommandProcessing.MonitorStress);

            return true;
        }

        private static Boolean updateLine(FunctionSeries line, double firstPoint, double secondPoint, float threshold, Boolean addStress)
        {
            if (addStress)
            {
                line.Points[0] = new DataPoint(firstPoint,  threshold * (1 + (EmotionProcessing.StressFactor / Constants.maxPercent)));
                line.Points[1] = new DataPoint(secondPoint, threshold * (1 + (EmotionProcessing.StressFactor / Constants.maxPercent)));
            }
            else
            {
                line.Points[0] = new DataPoint(firstPoint,  threshold);
                line.Points[1] = new DataPoint(secondPoint, threshold);
            }

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
                case Constants.thresholdPushTag:
                    return Constants.colorPlotCmdPush;
                case Constants.thresholdPullTag:
                    return Constants.colorPlotCmdPull;
                case Constants.thresholdLiftTag:
                    return Constants.colorPlotCmdLift;
                case Constants.thresholdDropTag:
                    return Constants.colorPlotCmdDrop;
                default:
                    return Constants.colorPlotDefault;
            }
        }
    }
}
