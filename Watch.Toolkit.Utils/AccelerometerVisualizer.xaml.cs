using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GestureTouch;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Interface.DefaultFaces;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Toolkit.Utils
{
    public partial class AccelerometerVisualizer
    {
        private readonly TrackerManager _trackerManager;
        private readonly TouchManager _touchManager;
        private readonly GestureManager _gestureManager;
        private string _detection;

        private readonly WatchRuntime _watchWindow;
        readonly WatchConfiguration _configuration = new WatchConfiguration();
        readonly SensorVisualizer _visualizer = new SensorVisualizer();

        public AccelerometerVisualizer()
        {
            InitializeComponent();

             var classifierConfiguration = new ClassifierConfiguration(
                 new List<string> {"Normal Mode", "Left Index", "Left Knuckle", "Hand"}, AppDomain.CurrentDomain.BaseDirectory + "recording17.log");

            _trackerManager = new TrackerManager(classifierConfiguration);
            _trackerManager.RawTrackGestureDataUpdated += _trackerManager_RawTrackGestureDataUpdated;
            _trackerManager.Start();

            _touchManager = new TouchManager();
            _touchManager.BevelGrab += _touchManager_BevelGrab;
            _touchManager.Start();

            _gestureManager = new GestureManager();
            _gestureManager.RawDataReceived += _gestureManager_RawDataReceived;
            _gestureManager.Start();

        
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

            _configuration.ClassifierConfiguration = new ClassifierConfiguration(
                new List<string> { "Right Hand", "Left Hand", "Left Knuckle", "Hand" },
                AppDomain.CurrentDomain.BaseDirectory + "recording16.log");

            _watchWindow = new WatchRuntime(_configuration);
            _watchWindow.AddWatchFace(_visualizer);
            _watchWindow.GestureManager.RawDataReceived += GestureManager_RawDataReceived;
            _watchWindow.GestureManager.GestureDetected += GestureManager_GestureDetected;

            _watchWindow.Show();

        }

        void GestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            _visualizer.UpdateEvents(e.Gesture.ToString());
        }

        void GestureManager_RawDataReceived(object sender, Sensors.RawSensorDataReceivedEventArgs e)
        {
            _visualizer.UpdateVisualization(e.TopLeftSensor,e.TopRightSensor,e.FrontSensor,e.LightSensor);
        }

        void _gestureManager_RawDataReceived(object sender, Sensors.RawSensorDataReceivedEventArgs e)
        {
        }

        void _touchManager_BevelGrab(object sender, MultiBevelTouchEventArgs e)
        {
            Dispatcher.Invoke(()=>
            {
                Rect1.Fill = e.BevelState.BevelLeft ? Brushes.Red : Brushes.Transparent;
                Rect2.Fill = e.BevelState.BevelTop ? Brushes.Red : Brushes.Transparent;
                Rect3.Fill = e.BevelState.BevelRight ? Brushes.Red : Brushes.Transparent;
                Rect4.Fill = e.BevelState.BevelBottom ? Brushes.Red : Brushes.Transparent; 

                _visualizer.UpdateBevels(e.BevelState);
            });

        }

        void _trackerManager_RawTrackGestureDataUpdated(object sender, TrackGestureEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lblRaw.Content = _trackerManager.Accelerometer.ToFormattedString();
                lblDTW.Content = "";

                lblDT.Content = e.TreeLabel;
                _detection = e.TreeLabel == e.DtwLabel ? e.TreeLabel : "Normal Mode";

                foreach (var item in e.ComputedDtwCosts)
                {
                    lblDTW.Content += item.Key + " " + item.Value + "\n";
                }
            });
        }

        void TouchVisualizer_GestureTouchMove(object sender, GestureTouchEventArgs e)
        {
            //Label.Content = _detection;
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
