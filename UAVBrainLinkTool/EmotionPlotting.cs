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
        private static double emotionHeatRange = 5;
        private static int emotionHeatArrayLength = (int)(Plotting.plotTimeWindow / (EmotivDeviceComms.ListeningEmotionMillisecondInterval / Constants.millisecondsInSeconds));

        private static double heatDataOffset = 1.0;
        private static double blendFraction = 0.25;

        private static int latestRawHeatIndex = 0;

        public static PlotModel EmotionPlotModel { get; private set; }

        // 2D array of data representing brain wave frequency range intensity over time
        public static double[,] EmotionHeatData { get; private set; }

        public static Boolean initHeatPlot()
        {
            EmotionPlotModel = new PlotModel { Title = "Brain Wave Spectrogram" };

            // Set up axes
            EmotionPlotModel.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Palette = OxyPalettes.Jet(100),
                Title = "Brain Wave Intensity"
            });

            EmotionPlotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "FrequencyAxis",
                ItemsSource = new[] {
                    "Theta",
                    "Alpha",
                    "Low Beta",
                    "High Beta",
                    "Gamma"
                }
            });

            EmotionPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Key = "TimeAxis",
                IsAxisVisible = false
            });

            // Dimensions of 2D array of data:
            // X: Plot window (s) / sampling period (s)
            // Y: Emotion plot range
            EmotionHeatData = new double[emotionHeatArrayLength, (int)emotionHeatRange];

            // Fill array to be empty
            for (int x = 0; x < EmotionHeatData.GetLength(0); x++)
                for (int y = 0; y < EmotionHeatData.GetLength(1); y++)
                {
                    // Set first column to a different value to make it initially blue
                    if (x == 0)
                        EmotionHeatData[x, y] = 1.0;
                    else
                        EmotionHeatData[x, y] = 0.0;
                }

            HeatMapSeries emotionHeatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = Plotting.plotTimeWindow,
                Y0 = 0,
                Y1 = emotionHeatRange - 1,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Rectangles,
                Data = EmotionHeatData
            };

            EmotionPlotModel.Series.Add(emotionHeatMapSeries);

            return true;
        }

        public static Boolean addHeatData(EmotionProcessing.EmotionDataPoint fullDP, float newSampleTime)
        {
            // Calculate time the latest index represents
            float latestIndexTime = latestRawHeatIndex * (EmotivDeviceComms.ListeningEmotionMillisecondInterval / Constants.millisecondsInSeconds);
            // Calculate number of indexed entries to shift (if any)
            int newIndexShift = (int)((newSampleTime - latestIndexTime) / (EmotivDeviceComms.ListeningEmotionMillisecondInterval / Constants.millisecondsInSeconds));

            if (newIndexShift > 0)
            {
                // Record total shifts for future data point placement
                latestRawHeatIndex += newIndexShift;

                int arrayWidth = EmotionHeatData.GetLength(0);
                int arrayHeight = EmotionHeatData.GetLength(1);

                // If listening paused for more than window length, clear out data
                if (newIndexShift > emotionHeatArrayLength)
                    for (int x = 0; x < arrayWidth; x++)
                        for (int y = 0; y < arrayHeight; y++)
                            EmotionHeatData[x, y] = 0.0;
                // If necessary, shift all data to the left to mark passage of time
                else if (latestRawHeatIndex >= emotionHeatArrayLength)
                {
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

            addBlendedData(fullDP.theta[0],    dataInputIndex, 0); // Theta band value     ( 4- 8 Hz)
            addBlendedData(fullDP.alpha[0],    dataInputIndex, 1); // Alpha band value     ( 8-12 Hz)
            addBlendedData(fullDP.lowBeta[0],  dataInputIndex, 2); // Low-beta value       (12-16 Hz)
            addBlendedData(fullDP.highBeta[0], dataInputIndex, 3); // High-beta value      (16-25 Hz)
            addBlendedData(fullDP.gamma[0],    dataInputIndex, 4); // Gamma value          (25-45 Hz)

            // Update plot
            EmotionPlotModel.InvalidatePlot(true);

            return true;
        }

        private static Boolean addBlendedData(double value, int xIndex, int yIndex)
        {
            double logValue = Math.Log10(value) + heatDataOffset;

            if (xIndex > 0)
            {
                EmotionHeatData[xIndex,     yIndex] += logValue * (1 - blendFraction);
                EmotionHeatData[xIndex - 1, yIndex] += logValue *      blendFraction;
            }
            else
            {
                EmotionHeatData[xIndex, yIndex] += logValue;
            }

            return true;
        }
    }
}
