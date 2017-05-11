using Emotiv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAVBrainLinkTool
{
    public static class EmotionProcessing
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        // Set in config file
        public static double ActiveStressThreshold { get; set; }
        public static double InactiveStressThreshold { get; set; }
        public static double StressFactor { get; set; }
        public static Single StressTimeWindow { get; set; } // Seconds

        private static Boolean isStressed = false;
        public static Boolean IsStressed
        {
            get
            {
                return isStressed;
            }
            set
            {
                isStressed = value;
                OnStaticPropertyChanged("IsStressed");
            }
        }

        private static double stressLevel = 0.0;
        public static double StressLevel
        {
            get
            {
                return stressLevel;
            }
            set
            {
                stressLevel = value;
                OnStaticPropertyChanged("StressFactor");
            }
        }

        // From "AverageBandPowers" C# example project

        /* EEG and system data channel description
        public enum IEE_DataChannel_t
        {
            IED_COUNTER = 0,        //!< Sample counter
            IED_INTERPOLATED,       //!< Indicate if data is interpolated
            IED_RAW_CQ,             //!< Raw contact quality value
            IED_AF3,                //!< Channel AF3
            IED_F7,                 //!< Channel F7
            IED_F3,                 //!< Channel F3
            IED_FC5,                //!< Channel FC5
            IED_T7,                 //!< Channel T7
            IED_P7,                 //!< Channel P7
            IED_O1,                 //!< Channel O1 = Pz
            IED_O2,                 //!< Channel O2
            IED_P8,                 //!< Channel P8
            IED_T8,                 //!< Channel T8
            IED_FC6,                //!< Channel FC6
            IED_F4,                 //!< Channel F4
            IED_F8,                 //!< Channel F8
            IED_AF4,                //!< Channel AF4
            IED_GYROX,              //!< Gyroscope X-axis
            IED_GYROY,              //!< Gyroscope Y-axis
            IED_TIMESTAMP,          //!< System timestamp
            IED_MARKER_HARDWARE,    //!< Marker from extender
            IED_ES_TIMESTAMP,       //!< EmoState timestamp
            IED_FUNC_ID,            //!< Reserved function id
            IED_FUNC_VALUE,         //!< Reserved function value
            IED_MARKER,             //!< Marker value from hardware
            IED_SYNC_SIGNAL         //!< Synchronisation signal
        } ;
        */

        private static EdkDll.IEE_DataChannel_t[] emotivChannelList = new EdkDll.IEE_DataChannel_t[5] {
            EdkDll.IEE_DataChannel_t.IED_AF3,
            EdkDll.IEE_DataChannel_t.IED_AF4,
            EdkDll.IEE_DataChannel_t.IED_T7,
            EdkDll.IEE_DataChannel_t.IED_T8,
            EdkDll.IEE_DataChannel_t.IED_O1
        };

        public static EdkDll.IEE_DataChannel_t[] EmotivChannelList
        {
            get
            {
                return emotivChannelList;
            }
            private set
            {
                emotivChannelList = value;
                OnStaticPropertyChanged("EmotivChannelList");
            }
        }

        public class EmotionDataPoint
        {
            public double[] theta      = new double[1] { 0.0 }; // Theta band value     ( 4- 8 Hz)
            public double[] alpha      = new double[1] { 0.0 }; // Alpha band value     ( 8-12 Hz)
            public double[] lowBeta    = new double[1] { 0.0 }; // Low-beta value       (12-16 Hz)
            public double[] highBeta   = new double[1] { 0.0 }; // High-beta value      (16-25 Hz)
            public double[] gamma      = new double[1] { 0.0 }; // Gamma value          (25-45 Hz)

            public EmotionDataPoint()
            {
                ;
            }

            public EmotionDataPoint(double inputTheta, double inputAlpha, double inputLowBeta, double inputHighBeta, double inputGamma)
            {
                theta[0]       = inputTheta;
                alpha[0]       = inputAlpha;
                lowBeta[0]     = inputLowBeta;
                highBeta[0]    = inputHighBeta;
                gamma[0]       = inputGamma;
            }
        }

        // Process data to determine if stressed state is active, then add to plot data
        public static Boolean addNewSample(EmotionDataPoint dp, float latestSampleTime)
        {
            updateStressFactor(dp);

            // Update UI plot for appropriate command
            EmotionPlotting.addHeatData(dp, latestSampleTime);

            return true;
        }

        // Update stress status with new data
        private static Boolean updateStressFactor(EmotionDataPoint dp)
        {
            // TODO: Implement algorithm to accumulate stress
            StressLevel = 0.0;

            updateExceedsThreshold();

            return true;
        }

        private static Boolean updateExceedsThreshold()
        {
            if (IsStressed && (StressLevel < ActiveStressThreshold))
            {
                IsStressed = false;
                CommandPlotting.updateThresholdSeries(CommandProcessing.LatestSampleTime);
            }
            else if (!IsStressed && (StressLevel > InactiveStressThreshold))
            {
                IsStressed = true;
                CommandPlotting.updateThresholdSeries(CommandProcessing.LatestSampleTime);
            }

            return true;
        }
    }
}
