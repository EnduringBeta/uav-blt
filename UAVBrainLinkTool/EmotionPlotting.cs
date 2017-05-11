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
        //private static double emotionPlotRange = 500.0;
        private static double emotionHeatRange = 5;
        private static int emotionHeatArrayLength = (int)(Plotting.plotTimeWindow / (EmotivDeviceComms.ListeningMillisecondInterval / Constants.millisecondsInSeconds));
        private static double heatDataOffset = 1.0;

        //private static double latestDataPointTime = 0.0;
        private static int latestRawHeatIndex = 0;

        private class HeatMapDataPoint
        {
            public int xIndex = 0;
            public int yIndex = 0;
            public double value = 0.0;

            public HeatMapDataPoint()
            {
                ;
            }

            public HeatMapDataPoint(int inputX, int inpuxY, double inputValue)
            {
                xIndex = inputX;
                yIndex = inpuxY;
                value = inputValue;
            }
        }
        private static HeatMapDataPoint highestValue = new HeatMapDataPoint();

        public static PlotModel EmotionPlotModel { get; private set; }

        public static HeatMapSeries EmotionHeatMapSeries;

        // List of data series to accomodate multiple simultaneous emotion data
        //public static List<FunctionSeries> EmotionPlotData { get; private set; }

        // 2D array of data representing brain wave frequency range intensity over time
        public static double[,] EmotionHeatData { get; private set; }

        public static Boolean initHeatPlot()
        {
            EmotionPlotModel = new PlotModel { Title = "Brain Wave Spectrogram" };

            // Set up axes
            EmotionPlotModel.Axes.Add(new LinearColorAxis
            {
                Palette = OxyPalettes.Jet(100),
                Title = "Intensity vs Frequency vs Time",
            });

            // Dimensions of 2D array of data:
            // X: Plot window (s) / sampling period (s)
            // Y: Emotion plot range
            EmotionHeatData = new double[emotionHeatArrayLength, (int)emotionHeatRange];

            // Fill array to be empty
            for (int x = 0; x < EmotionHeatData.GetLength(0); x++)
                for (int y = 0; y < EmotionHeatData.GetLength(1); y++)
                    EmotionHeatData[x, y] = 0.0;

            EmotionHeatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = Plotting.plotTimeWindow,
                Y0 = 0,
                Y1 = emotionHeatRange,
                Interpolate = false,
                RenderMethod = HeatMapRenderMethod.Rectangles,
                Data = EmotionHeatData
            };

            EmotionPlotModel.Series.Add(EmotionHeatMapSeries);

            return true;
        }

        public static Boolean addHeatData(EmotionProcessing.EmotionDataPoint fullDP, float newSampleTime)
        {
            // Calculate time the latest index represents
            float latestIndexTime = latestRawHeatIndex * (EmotivDeviceComms.ListeningMillisecondInterval / Constants.millisecondsInSeconds);
            // Calculate number of indexed entries to shift (if any)
            int newIndexShift = (int)((newSampleTime - latestIndexTime) / (EmotivDeviceComms.ListeningMillisecondInterval / Constants.millisecondsInSeconds));

            if (newIndexShift > 0)
            {
                // Adjust highest value and reset if removed
                highestValue.xIndex -= newIndexShift;
                if (highestValue.xIndex < 0)
                    highestValue.value = 0.0;

                // Record total shifts for future data point placement
                latestRawHeatIndex += newIndexShift;

                // If necessary, shift all data to the left to mark passage of time
                if (latestRawHeatIndex >= emotionHeatArrayLength)
                {
                    int arrayWidth = EmotionHeatData.GetLength(0);
                    int arrayHeight = EmotionHeatData.GetLength(1);

                    // Shift data
                    for (int x = newIndexShift; x < arrayWidth; x++)
                        for (int y = 0; y < arrayHeight; y++)
                            EmotionHeatData[x - newIndexShift, y] = EmotionHeatData[x, y];

                    // Empty final columns
                    for (int x = newIndexShift; x > 0; x--)
                        for (int y = 0; y < arrayHeight; y++)
                            EmotionHeatData[arrayWidth - x, y] = 0.0;
                }
            }

            int dataInputIndex = (latestRawHeatIndex >= emotionHeatArrayLength) ? (emotionHeatArrayLength - 1) : latestRawHeatIndex;

            EmotionHeatData[dataInputIndex, 0] += Math.Log10(fullDP.theta[0])    + heatDataOffset;   // Theta band value     ( 4- 8 Hz)
            EmotionHeatData[dataInputIndex, 1] += Math.Log10(fullDP.alpha[0])    + heatDataOffset;   // Alpha band value     ( 8-12 Hz)
            EmotionHeatData[dataInputIndex, 2] += Math.Log10(fullDP.lowBeta[0])  + heatDataOffset;   // Low-beta value       (12-16 Hz)
            EmotionHeatData[dataInputIndex, 3] += Math.Log10(fullDP.highBeta[0]) + heatDataOffset;   // High-beta value      (16-25 Hz)
            EmotionHeatData[dataInputIndex, 4] += Math.Log10(fullDP.gamma[0])    + heatDataOffset;   // Gamma value          (25-45 Hz)

            // Need to track and artificially adjust highest value to keep heat map updating?
            HeatMapDataPoint newHighestValue = new HeatMapDataPoint();

            // Check if any values are higher than previous high
            if (EmotionHeatData[dataInputIndex, 0] > newHighestValue.value)
                newHighestValue = new HeatMapDataPoint(dataInputIndex, 0, EmotionHeatData[dataInputIndex, 0]);
            if (EmotionHeatData[dataInputIndex, 1] > newHighestValue.value)
                newHighestValue = new HeatMapDataPoint(dataInputIndex, 1, EmotionHeatData[dataInputIndex, 1]);
            if (EmotionHeatData[dataInputIndex, 2] > newHighestValue.value)
                newHighestValue = new HeatMapDataPoint(dataInputIndex, 2, EmotionHeatData[dataInputIndex, 2]);
            if (EmotionHeatData[dataInputIndex, 3] > newHighestValue.value)
                newHighestValue = new HeatMapDataPoint(dataInputIndex, 3, EmotionHeatData[dataInputIndex, 3]);
            if (EmotionHeatData[dataInputIndex, 4] > newHighestValue.value)
                newHighestValue = new HeatMapDataPoint(dataInputIndex, 4, EmotionHeatData[dataInputIndex, 4]);

            if (newHighestValue.value > highestValue.value)
                highestValue = newHighestValue;
            else
                ;//EmotionHeatData[highestValue.xIndex, highestValue.yIndex] += 0.01;

            // Save latest timestamp
            //latestDataPointTime = newSampleTime;

            // Update plot
            EmotionPlotModel.InvalidatePlot(true);

            return true;
        }

        // TODO: Tooltip on this graph crashes program because of changing data
        /*
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

            addDataSeries(Constants.alphaTag);
            addDataSeries(Constants.thetaTag);
            addDataSeries(Constants.lowBetaTag);
            addDataSeries(Constants.highBetaTag);
            addDataSeries(Constants.gammaTag);

            // Create initial data to display graph properly
            addPlotData(new EmotionProcessing.EmotionDataPoint(0.0, 0.0, 0.0, 0.0, 0.0), (float)latestDataPointTime);

            return true;
        }
        */

        /*
        private static FunctionSeries addDataSeries(String tag)
        {
            FunctionSeries newSeries = new FunctionSeries();
            newSeries.Tag = tag;
            newSeries.Color = getSeriesColor(tag);
            EmotionPlotData.Add(newSeries);
            EmotionPlotModel.Series.Add(newSeries);

            return newSeries;
        }
        */

        /*
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
        */

        /*
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
        */

        /*
        private static Boolean adjustPlotWindow(double newLatestTime)
        {
            double panAmount = EmotionPlotModel.DefaultXAxis.Transform(-(newLatestTime - latestDataPointTime) + EmotionPlotModel.DefaultXAxis.Offset);
            EmotionPlotModel.DefaultXAxis.Pan(panAmount);

            return true;
        }
        */

        /*
        private static Boolean updateThresholdSeries(double newLatestTime)
        {
            FunctionSeries thresholdLine = EmotionPlotData.Find(x => String.Compare((String)x.Tag, Constants.thresholdTag) == 0);
            thresholdLine.Points[0] = new DataPoint(newLatestTime - Plotting.plotTimeWindow, CommandProcessing.ActiveCommandThreshold);
            thresholdLine.Points[1] = new DataPoint(newLatestTime, CommandProcessing.ActiveCommandThreshold);

            return true;
        }
        */

        /*
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
        */
    }
}
