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
            InitializeComponent();

            EmotivDeviceComms.initialize();
            EmotivServerComms.initialize();

            // TODO: Eventually this information will be taken from a file or entered in.
            TextBlockUsername.Text = Constants.userName;
            TextBlockProfile.Text = Constants.profileName;

            EmotivDeviceComms.hookEvents();
            EmotivDeviceComms.connectToDevice();

            // TODO: If information not available, prompt user
            EmotivServerComms.logIn(Constants.userName, Constants.password);
            EmotivServerComms.loadUserProfile(Constants.profileName);
        }

        private void ButtonListen_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Clear other active UI elements when stopped?
            if (EmotivDeviceComms.IsListening)
            {
                EmotivDeviceComms.stopListening();
                ButtonListen.Content = "Start Listening";
            }
            else
            {
                EmotivDeviceComms.startListening();
                ButtonListen.Content = "Stop Listening";
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
    }
}
