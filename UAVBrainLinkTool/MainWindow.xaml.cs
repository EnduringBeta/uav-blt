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
            if (Config.importConfig())
                setDetailedWindowTitle();
            else
                Logging.outputLine("Error: failure when reading configuration file!");

            // Initialize plot before referencing it in XAML
            Plotting.init();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            setDisabledCommandButtonTextColors();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (Utils.initAll())
                Utils.ProgramLoaded = true;
        }

        private Boolean setDetailedWindowTitle()
        {
            Utils.WindowTitle = Utils.WindowTitle + " - " + Config.UserName + " (" + Config.ProfileName + ")" + " - " + Config.COMPort;

            return true;
        }

        // ContentPresenter results in "null" for some machines while the window is loading
        // Checking if null to protect against early crash, but cannot color disabled button text at start if so
        private Boolean setDisabledCommandButtonTextColors()
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/0d3d9b06-6855-4a91-bc2e-f1f0973e3b31/how-to-change-the-foreground-color-of-a-disabled-button-in-wpf
            ContentPresenter cp = ButtonPush.Template.FindName("contentPresenter", ButtonPush) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonPush.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdPush);

            cp = ButtonPull.Template.FindName("contentPresenter", ButtonPull) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonPull.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdPull);

            cp = ButtonLift.Template.FindName("contentPresenter", ButtonLift) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonLift.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdLift);

            cp = ButtonDrop.Template.FindName("contentPresenter", ButtonDrop) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonDrop.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdDrop);

            return true;
        }

        private void ButtonPush_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdPush, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdPush);

            ContentPresenter cp = ButtonPush.Template.FindName("contentPresenter", ButtonPush) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonPush.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdPush);
        }

        private void ButtonPull_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdPull, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdPull);

            ContentPresenter cp = ButtonPull.Template.FindName("contentPresenter", ButtonPull) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonPull.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdPull);
        }

        private void ButtonLift_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdLift, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdLift);

            ContentPresenter cp = ButtonLift.Template.FindName("contentPresenter", ButtonLift) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonLift.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdLift);
        }

        private void ButtonDrop_Click(object sender, RoutedEventArgs e)
        {
            CommandComms.sendCommand(Constants.cmdDrop, CommandProcessing.ActiveCommandThreshold);
            Utils.updateStatusBarText(Constants.cmdDrop);

            ContentPresenter cp = ButtonDrop.Template.FindName("contentPresenter", ButtonDrop) as ContentPresenter;
            if (cp != null)
                cp.SetValue(TextElement.ForegroundProperty, ButtonDrop.IsEnabled ? Constants.ColorButtonCmdDefault : Constants.ColorButtonCmdDrop);
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

        private void ButtonMonitorStress_Click(object sender, RoutedEventArgs e)
        {
            if (CommandProcessing.MonitorStress)
            {
                CommandProcessing.MonitorStress = false;
            }
            else
            {
                CommandProcessing.MonitorStress = true;
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

    // Convert Boolean for "Monitor Stress" button to text
    public class BoolToMonitorStressTextConverter : IValueConverter
    {
        // Flag: true for connected, false for disconnected
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.stopMonitoringStress : Constants.startMonitoringStress;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
