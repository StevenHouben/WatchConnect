using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.PasswordField
{
    public partial class MainWindow
    {

        private WatchRuntime _watchWindow;
        private WatchConfiguration _watchConfiguration;
        private WatchFaceExample _passwordViewer;

        private string _lastDetection;
        public MainWindow()
        {
            InitializeComponent();

            KeyDown += MainWindow_KeyDown;
            //Change machine learning data and hardware input here
            _watchConfiguration = new WatchConfiguration
            {
                Hardware = new Arduino("COM4"),
                ClassifierConfiguration = new ClassifierConfiguration(
                    new List<string> { "Right Hand", "Left Hand", "Left Knuckle", "Hand" },
                    AppDomain.CurrentDomain.BaseDirectory + "recording19.log")
            };

            _watchWindow = new WatchRuntime(_watchConfiguration);
            _passwordViewer = new WatchFaceExample();
            _watchWindow.AddWatchFace(_passwordViewer);
            _watchWindow.Show();

            PreviewTouchDown += MainWindow_PreviewTouchDown;
            PreviewTouchUp += MainWindow_PreviewTouchUp;
        }

        void MainWindow_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            PasswordBox.PasswordChar = '*';
            Box.Text = "";
            Box.Visibility = Visibility.Hidden;
            PasswordBox.Visibility = Visibility.Visible;
            Button.Background = Brushes.Gray;
            Output.Content = "";
            _passwordViewer.SetText("");
        }

        void MainWindow_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            Title = _watchWindow.LastDetectedPosture;
            if (_watchWindow.LastDetectedPosture == "Hand")
            {
                Box.Text = PasswordBox.Password;
                Box.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Hidden;
                Button.Background = Output.Foreground = Brushes.Orange;
                Output.Content = "Password Revealed!";
            }
            else if (_watchWindow.LastDetectedPosture == "Left Knuckle")
            {
                PasswordBox.Password = "";
                Button.Background = Output.Foreground = Brushes.Gray;
                Output.Content = "Password Reset";
            }
            else if (_watchWindow.LastDetectedPosture == "Left Hand")
            {
                Box.Text = PasswordBox.Password;
                Button.Background = Output.Foreground = Brushes.Orange;
                Output.Content = "Password Revealed!";
                _passwordViewer.SetText(Box.Text);
            }
            else
            {
                if (PasswordBox.Password != "qwertyu")
                {
                    Button.Background = Output.Foreground = Brushes.DarkRed;
                    Output.Content = "Incorrect Password";
                }
                else
                {
                    Button.Background = Output.Foreground = Brushes.DarkOliveGreen;
                    Output.Content = "Password Accepted";
                }
            }
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Environment.Exit(0);
        }

        private void PasswordBox_OnPreviewTouchDown(object sender, TouchEventArgs e)
        {
            Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
        }
    }
}
