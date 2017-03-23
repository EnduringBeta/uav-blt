﻿using OxyPlot;
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

        private static double latestDataPointTime = 0.0;

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

            // Set up axes
            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.AbsoluteMinimum = 0.0;
            xAxis.MinimumRange = commandPlotTimeWindow;
            xAxis.MaximumRange = commandPlotTimeWindow;
            xAxis.IsPanEnabled = false;
            xAxis.IsZoomEnabled = false;
            xAxis.Title = "Time (s)";
            CommandPlotModel.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.AbsoluteMinimum = -0.5;
            yAxis.AbsoluteMaximum = CommandProcessing.ActiveCommandThreshold * thresholdViewMultiplier;
            yAxis.MaximumRange = CommandProcessing.ActiveCommandThreshold * thresholdViewMultiplier + 0.5;
            yAxis.MinimumRange = CommandProcessing.ActiveCommandThreshold * thresholdViewMultiplier + 0.5;
            yAxis.IsPanEnabled = false;
            yAxis.IsZoomEnabled = false;
            yAxis.Title = "Command Power";
            CommandPlotModel.Axes.Add(yAxis);

            // Create threshold line
            FunctionSeries thresholdLine = addDataSeries(Constants.thresholdTag);
            thresholdLine.Points.Add(new DataPoint(0.0, CommandProcessing.ActiveCommandThreshold));
            thresholdLine.Points.Add(new DataPoint(commandPlotTimeWindow, CommandProcessing.ActiveCommandThreshold));

            return true;
        }

        private static FunctionSeries addDataSeries(String tag)
        {
            FunctionSeries newSeries = new FunctionSeries();
            newSeries.Tag = tag;
            newSeries.Color = getSeriesColor(tag);
            CommandPlotData.Add(newSeries);
            CommandPlotModel.Series.Add(newSeries);

            return newSeries;
        }

        public static Boolean addPlotData(List<DataPoint> newData, String tag)
        {
            // Find proper series
            FunctionSeries series = CommandPlotData.Find(x => String.Compare((String)x.Tag, tag) == 0);

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
            foreach (FunctionSeries fs in CommandPlotData)
            {
                int lastOldIndex = -1;
                foreach (DataPoint dp in fs.Points)
                {
                    if (dp.X < newLatestTime - commandPlotTimeWindow)
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
            double panAmount = CommandPlotModel.DefaultXAxis.Transform(-(newLatestTime - latestDataPointTime) + CommandPlotModel.DefaultXAxis.Offset);
            CommandPlotModel.DefaultXAxis.Pan(panAmount);

            return true;
        }

        private static Boolean updateThresholdSeries(double newLatestTime)
        {
            FunctionSeries thresholdLine = CommandPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdTag) == 0);
            thresholdLine.Points[0] = new DataPoint(newLatestTime - commandPlotTimeWindow, CommandProcessing.ActiveCommandThreshold);
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
                case Constants.cmdRaise:
                    return Constants.colorPlotCmdRaise;
                case Constants.cmdLower:
                    return Constants.colorPlotCmdLower;
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
