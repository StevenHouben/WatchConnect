using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.UnlockScreen
{
    public partial class MainWindow
    {

        private WatchRuntime _watchWindow;
        private WatchConfiguration _watchConfiguration;
        private WatchFaceExample _feedback;
        private bool _down;

        public MainWindow()
        {
            AllowsTransparency = true;
            InitializeComponent();

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            KeyDown += MainWindow_KeyDown;
            //Change machine learning data and hardware input here
            _watchConfiguration = new WatchConfiguration
            {
                Hardware = new Arduino("COM4"),
                ClassifierConfiguration = new ClassifierConfiguration(
                    new List<string> {"Right Hand", "Left Hand", "Left Knuckle", "Hand"},
                    AppDomain.CurrentDomain.BaseDirectory + "recording19.log")
            };

            _feedback =  new WatchFaceExample();
            _watchWindow = new WatchRuntime(_watchConfiguration);
            _watchWindow.AddWatchFace(_feedback);
            _watchWindow.Show();

            TouchDown += MainWindow_TouchDown;

            _watchWindow.TrackerManager.Imu.AddEvent("Rotate",
                (imu) => imu.YawPitchRollValues.Z < -50
                ).EventTriggered += (sender, e) =>
                {
                    if (_down)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _feedback.SetColor(Brushes.DarkOliveGreen);
                            Background = Brushes.Transparent;
                            Image.Opacity = 0;
                        });

                    }
                };

        }

        void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            _down = true;
            _feedback.SetColor(Brushes.DarkRed);
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Environment.Exit(0);
        }
    }
}
