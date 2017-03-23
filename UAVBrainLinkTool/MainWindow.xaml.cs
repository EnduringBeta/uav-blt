using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
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
            // Initialize plot before referencing it in XAML
            Plotting.initPlot();

            InitializeComponent();

            // Color the command buttons!
            ButtonPush.Background = Constants.colorButtonCmdPush;
            ButtonPull.Background = Constants.colorButtonCmdPull;
            ButtonRaise.Background = Constants.colorButtonCmdRaise;
            ButtonLower.Background = Constants.colorButtonCmdLower;

            enableButtons(false);
        }

        // Disable buttons while loading
        private Boolean enableButtons(Boolean enable = true)
        {
            ButtonPush.IsEnabled   = enable;
            ButtonPull.IsEnabled   = enable;
            ButtonRaise.IsEnabled  = enable;
            ButtonLower.IsEnabled  = enable;
            ButtonListen.IsEnabled = enable;

            return true;
        }

        private void ButtonListen_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Clear other active UI elements when stopped?
            if (EmotivDeviceComms.IsListening)
            {
                EmotivDeviceComms.stopListening();
                ButtonListen.Content = Constants.startListening;
            }
            else
            {
                EmotivDeviceComms.startListening();
                ButtonListen.Content = Constants.stopListening;
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmotivDeviceComms.initialize();
            EmotivServerComms.initialize();

            EmotivDeviceComms.hookEvents();
            EmotivDeviceComms.connectToDevice();

            // TODO: Eventually this information will be taken from a file or entered in.
            TextBlockUsername.Text = Constants.userName;
            TextBlockProfile.Text = Constants.profileName;

            // TODO: If information not available, prompt user
            EmotivServerComms.logIn(Constants.userName, Constants.password);
            EmotivServerComms.loadUserProfile(Constants.profileName);

            ButtonListen.Content = Constants.startListening;

            enableButtons();
        }
    }
}
