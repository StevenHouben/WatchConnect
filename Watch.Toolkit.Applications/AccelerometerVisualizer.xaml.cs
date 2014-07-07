using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GestureTouch;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Toolkit.Applications
{
    public partial class MainWindow
    {
        private readonly TrackerManager _trackerManager;
        private string _detection;

        public MainWindow()
        {
            InitializeComponent();

             var classifierConfiguration = new ClassifierConfiguration(
                 new List<string> {"Normal Mode", "Left Index", "Left Knuckle", "Hand"}, AppDomain.CurrentDomain.BaseDirectory + "recording16.log");

            _trackerManager = new TrackerManager(classifierConfiguration);
            _trackerManager.TrackGestureRecognized += _trackerManager_GestureRecognized;
            _trackerManager.Start();

        
            foreach (var template in _trackerManager.DtwClassifier.GetTemplates())
            {
                listGesture.Items.Add(template.Key + " - " + String.Join(",", template.Value.Select(p => p.ToString()).ToArray()));
            }

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            Closing += MainWindow_Closing;

            TouchVisualizer.GestureTouchUp += MainWindow_GestureTouchUp;
            TouchVisualizer.GestureTouchMove += TouchVisualizer_GestureTouchMove;
            TouchVisualizer.GestureTouchDown += TouchVisualizer_GestureTouchDown;

            cbGestureList.ItemsSource = 
                new List<string> { "None", "Left Index ", "Left Middle", "Left Pinky", "Left Knuckle" };
            cbGestureList.SelectedIndex = 0;
            KeyDown += MainWindow_KeyDown;
        }

        void TouchVisualizer_GestureTouchMove(object sender, GestureTouchEventArgs e)
        {
            //Label.Content = _detection;
        }

        void _trackerManager_GestureRecognized(object sender, TrackGestureEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lblRaw.Content = _trackerManager.Accelerometer.ToFormattedString();
                lblDTW.Content = "";

                lblDT.Content = e.TreeLabel;



                _detection =  e.TreeLabel == e.DtwLabel ? e.TreeLabel: "Normal Mode";

                foreach (var item in e.ComputedDtwCosts)
                {
                    lblDTW.Content += item.Key + " " + item.Value + "\n";
                }
            });
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _trackerManager.Stop();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                foreach (string item in listGesture.Items)
                {
                    File.AppendAllText("log.log", item+"\n");
                }
                Environment.Exit(0);
            }

        }

        void TouchVisualizer_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            Label.Content = _detection;

        }

        void MainWindow_GestureTouchUp(object sender, GestureTouchEventArgs e)
        {
            Label.Content = "";
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
