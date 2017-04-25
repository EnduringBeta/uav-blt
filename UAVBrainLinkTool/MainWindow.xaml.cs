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
        private Boolean configLoaded = false;

        public MainWindow()
        {
            // Load config file first for necessary plot info
            configLoaded = Config.importConfig();
            if (!configLoaded)
                Logging.outputLine("Error: failure when reading configuration file!");

            // Initialize plot before referencing it in XAML
            Plotting.initPlot();

            InitializeComponent();

            // Color the command buttons!
            ButtonPush.Background = Constants.colorButtonCmdPush;
            ButtonPull.Background = Constants.colorButtonCmdPull;
            ButtonRaise.Background = Constants.colorButtonCmdRaise;
            ButtonLower.Background = Constants.colorButtonCmdLower;

            enableAllButtons(false);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            EmotivDeviceComms.initialize();
            EmotivServerComms.initialize();

            TextBlockCurrentCommands.Text = "Connecting to Emotiv device...";

            EmotivDeviceComms.hookEvents();
            EmotivDeviceComms.connectToDevice();

            if (configLoaded)
            {
                TextBlockUsername.Text = Config.UserName;
                TextBlockProfile.Text = Config.ProfileName;
                TextBlockAntenna.Text = Config.COMPort;

                CommandComms.initCommandComms();

                // TODO: If information not available, prompt user
                EmotivServerComms.logIn(Config.UserName, Config.Password);
                EmotivServerComms.loadUserProfile(Config.ProfileName);

                ButtonListen.Content = Constants.startListening;

                enableButtonsLoaded();

                TextBlockCurrentCommands.Text = "Emotiv device connected!";
            }
            else
            {
                // TODO: Improve this hacky method of showing errors.
                TextBlockCurrentCommands.Text = "Error: failure when reading configuration file!";
            }
        }

        // Disable buttons while loading
        private Boolean enableAllButtons(Boolean enable = true)
        {
            ButtonPush.IsEnabled       = enable;
            ButtonPull.IsEnabled       = enable;
            ButtonRaise.IsEnabled      = enable;
            ButtonLower.IsEnabled      = enable;
            ButtonListen.IsEnabled     = enable;
            ButtonTransmit.IsEnabled   = enable;
            ButtonConnectUAV.IsEnabled = enable;

            return true;
        }

        private Boolean enableButtonsLoaded(Boolean enable = true)
        {
            ButtonListen.IsEnabled = enable;
            ButtonConnectUAV.IsEnabled = enable;

            return true;
        }

        private void ButtonPush_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdPush, CommandProcessing.ActiveCommandThreshold);
            EmotivDeviceComms.ActiveCommandsText = Constants.cmdPush;
        }

        private void ButtonPull_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdPull, CommandProcessing.ActiveCommandThreshold);
            EmotivDeviceComms.ActiveCommandsText = Constants.cmdPull;
        }

        private void ButtonRaise_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdRaise, CommandProcessing.ActiveCommandThreshold);
            EmotivDeviceComms.ActiveCommandsText = Constants.cmdRaise;
        }

        private void ButtonLower_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdLower, CommandProcessing.ActiveCommandThreshold);
            EmotivDeviceComms.ActiveCommandsText = Constants.cmdLower;
        }

        private void ButtonListen_Click(object sender, RoutedEventArgs e)
        {
            TextBlockCurrentCommands.Text = "";

            // TODO: Clear other active UI elements when stopped?
            if (EmotivDeviceComms.IsListening)
            {
                if (EmotivDeviceComms.stopListening())
                    ButtonListen.Content = Constants.startListening;
            }
            else
            {
                if (EmotivDeviceComms.startListening())
                    ButtonListen.Content = Constants.stopListening;
            }
        }

        private void ButtonTransmit_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.IsTransmitting = true;
        }

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
