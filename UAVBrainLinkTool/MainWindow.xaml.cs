using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UAVBrainLinkTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Utils.ProgramLoaded = false;

            // Load config file first for necessary plot info
            if (!Config.importConfig())
                Logging.outputLine("Error: failure when reading configuration file!");

            // Initialize plot before referencing it in XAML
            Plotting.initPlot();

            InitializeComponent();

            // Color the command buttons!
            // Note: In general, be cautious about setting properties of UI elements.
            // Doing so will remove any bindings set for that property.
            // Use a property in code (like Utils.StatusBarText) for maximum flexibility.
            ButtonPush.Background = Constants.colorButtonCmdPush;
            ButtonPull.Background = Constants.colorButtonCmdPull;
            ButtonRaise.Background = Constants.colorButtonCmdRaise;
            ButtonLower.Background = Constants.colorButtonCmdLower;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (Utils.initAll())
                Utils.ProgramLoaded = true;
        }

        private void ButtonPush_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdPush, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdPush);
        }

        private void ButtonPull_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdPull, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdPull);
        }

        private void ButtonRaise_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdRaise, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdRaise);
        }

        private void ButtonLower_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdLower, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdLower);
        }

        private void ButtonListen_Click(object sender, RoutedEventArgs e)
        {
            Utils.updateStatusBarText();

            // TODO: Clear other active UI elements when stopped?
            if (EmotivDeviceComms.IsListening)
            {
                EmotivDeviceComms.stopListening();
            }
            else
            {
                EmotivDeviceComms.startListening();
            }
        }

        private void ButtonTransmit_Click(object sender, RoutedEventArgs e)
        {
            if (CommandComms.IsTransmitting)
            {
                CommandComms.IsTransmitting = false;
            }
            else
            {
                CommandComms.IsTransmitting = true;
            }
        }

        // TODO: Update status bar
        private void ButtonConnectUAV_Click(object sender, RoutedEventArgs e)
        {
            if (CommandComms.IsDeviceConnected)
            {
                CommandComms.disconnectUAV();
            }
            else
            {
                CommandComms.connectUAV();
            }
        }
    }

    // http://stackoverflow.com/questions/35336449/wpf-convert-boolean-to-colored-string/35336565
    // Convert Boolean for "Listen to Emotiv" button to text
    public class BoolToListeningTextConverter : IValueConverter
    {
        // Flag: true for listening, false for not listening
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.stopListening : Constants.startListening;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // Convert Boolean for "Transmit to UAV" button to text
    public class BoolToTransmittingTextConverter : IValueConverter
    {
        // Flag: true for listening, false for not listening
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.stopTransmitting : Constants.startTransmitting;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // Convert Boolean for "Connect to UAV" button to text
    public class BoolToConnectUAVTextConverter : IValueConverter
    {
        // Flag: true for connected, false for disconnected
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.disconnectFromUAV : Constants.connectToUAV;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
