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
        private static double emotionPlotRange = 500.0;

        private static double latestDataPointTime = 0.0;

        public static PlotModel EmotionPlotModel { get; private set; }
        // List of data series to accomodate multiple simultaneous emotion data
        public static List<FunctionSeries> EmotionPlotData { get; private set; }

        public static Boolean initPlot()
        {
            EmotionPlotModel = new PlotModel { Title = "Frequency Intensity Plot" };
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
            yAxis.Title = "Frequency Intensity";
            EmotionPlotModel.Axes.Add(yAxis);

            // TODO: Adjust and re-implement
            // Create threshold line
            //FunctionSeries thresholdLine = addDataSeries(Constants.thresholdTag);
            //thresholdLine.Points.Add(new DataPoint(0.0, CommandProcessing.ActiveCommandThreshold));
            //thresholdLine.Points.Add(new DataPoint(Plotting.plotTimeWindow, CommandProcessing.ActiveCommandThreshold));

            addDataSeries(Constants.alphaTag);
            addDataSeries(Constants.thetaTag);
            addDataSeries(Constants.lowBetaTag);
            addDataSeries(Constants.highBetaTag);
            addDataSeries(Constants.gammaTag);

            // Create initial data to display graph properly
            addPlotData(new EmotionProcessing.EmotionDataPoint(0.0, 0.0, 0.0, 0.0, 0.0), (float)latestDataPointTime);

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

        public static Boolean addPlotData(EmotionProcessing.EmotionDataPoint fullDP, float latestSampleTime)
        {
            // For each set of data tracking different brain wave frequency ranges
            foreach (FunctionSeries series in EmotionPlotData)
            {
                DataPoint dp = new DataPoint(0.0, 0.0);
                switch ((String)series.Tag)
                {
                    case Constants.thetaTag:
                        dp = new DataPoint(latestSampleTime, fullDP.theta[0]);
                        break;
                    case Constants.alphaTag:
                        dp = new DataPoint(latestSampleTime, fullDP.alpha[0]);
                        break;
                    case Constants.lowBetaTag:
                        dp = new DataPoint(latestSampleTime, fullDP.lowBeta[0]);
                        break;
                    case Constants.highBetaTag:
                        dp = new DataPoint(latestSampleTime, fullDP.highBeta[0]);
                        break;
                    case Constants.gammaTag:
                        dp = new DataPoint(latestSampleTime, fullDP.gamma[0]);
                        break;
                    default:
                        break;
                }

                // Add particular frequency range data to its series
                series.Points.Add(dp);
            }

            // Remove data outside of plot window using new timestamp
            removeOldData(latestDataPointTime);

            // Save latest timestamp
            latestDataPointTime = latestSampleTime;

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
                        lastOldIndex = fs.Points.IndexOf(dp);
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

        /*
        private static Boolean updateThresholdSeries(double newLatestTime)
        {
            FunctionSeries thresholdLine = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdTag) == 0);
            thresholdLine.Points[0] = new DataPoint(newLatestTime - Plotting.plotTimeWindow, CommandProcessing.ActiveCommandThreshold);
            thresholdLine.Points[1] = new DataPoint(newLatestTime, CommandProcessing.ActiveCommandThreshold);

            return true;
        }
        */

        private static OxyColor getSeriesColor(String tag)
        {
            switch (tag)
            {
                case Constants.thetaTag:
                    return Constants.colorPlotEmoTheta;
                case Constants.alphaTag:
                    return Constants.colorPlotEmoAlpha;
                case Constants.lowBetaTag:
                    return Constants.colorPlotEmoLowBeta;
                case Constants.highBetaTag:
                    return Constants.colorPlotEmoHighBeta;
                case Constants.gammaTag:
                    return Constants.colorPlotEmoGamma;
                default:
                    return Constants.colorPlotDefault;
            }
        }
    }
}
