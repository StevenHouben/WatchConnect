using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GestureTouch;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Hardware.Phidget;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Interface;
using Watch.Toolkit.Interface.DefaultFaces;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Toolkit.Utils
{
    public partial class AccelerometerVisualizer
    {
        private string _detection;

        private readonly WatchRuntime _watchRuntime;
        readonly WatchConfiguration _configuration = new WatchConfiguration();
        readonly SensorVisualizer _visualizer = new SensorVisualizer();

        public AccelerometerVisualizer()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            Loaded += AccelerometerVisualizer_Loaded;
            Closing += MainWindow_Closing;

            TouchVisualizer.GestureTouchUp += MainWindow_GestureTouchUp;
            TouchVisualizer.GestureTouchMove += TouchVisualizer_GestureTouchMove;
            TouchVisualizer.GestureTouchDown += TouchVisualizer_GestureTouchDown;

            cbGestureList.ItemsSource =
                new List<string> { "None", "Left Index ", "Left Middle", "Left Pinky", "Left Knuckle" };
            cbGestureList.SelectedIndex = 0;
            KeyDown += MainWindow_KeyDown;

            _configuration.ClassifierConfiguration = new ClassifierConfiguration(
                new List<string> { "Right Hand", "Left Hand", "Left Knuckle", " Flat Hand" },
                AppDomain.CurrentDomain.BaseDirectory + "recording19.log");

            //_configuration.ClassifierConfiguration = new ClassifierConfiguration(
            //    new List<string> {"Neutral", "Left", "Top", "Right", "Down" },
            //    AppDomain.CurrentDomain.BaseDirectory + "recording20.log");

            _configuration.Hardware = new Arduino("COM4");

            _watchRuntime = new WatchRuntime(_configuration);
            _watchRuntime.AddWatchFace(_visualizer);
            _watchRuntime.GestureManager.RawDataReceived += GestureManager_RawDataReceived;
            _watchRuntime.GestureManager.GestureDetected += GestureManager_GestureDetected;
            _watchRuntime.GestureManager.Cover += GestureManager_Cover;

            _watchRuntime.TrackerManager = new TrackerManager(_configuration.Hardware, _configuration.ClassifierConfiguration);
            _watchRuntime.TrackerManager.RawTrackGestureDataUpdated += _trackerManager_RawTrackGestureDataUpdated;
            _watchRuntime.TrackerManager.Start();

            //_watchRuntime.TrackerManager.Imu.EventTriggered += ImuParser_EventTriggered;
            //_watchRuntime.TrackerManager.Imu.AddEvent("Rotate", imu => imu.YawPitchRollValues.Z < -10);
           
            _watchRuntime.TouchManager.BevelGrab += _touchManager_BevelGrab;
            _watchRuntime.TouchManager.BevelDoubleTap += _touchManager_BevelDoubleTap;
            _watchRuntime.TouchManager.SlideDown += _touchManager_SlideDown;
            _watchRuntime.TouchManager.SlideUp += _touchManager_SlideUp;
            _watchRuntime.TouchManager.SliderTouchDown += _touchManager_SliderTouchDown;
            _watchRuntime.TouchManager.SliderTouchUp += _touchManager_SliderTouchUp;
            _watchRuntime.TouchManager.SliderDoubleTap += _touchManager_DoubleTap;


            foreach (var template in _watchRuntime.TrackerManager.DtwClassifier.GetTemplates())
            {
                listGesture.Items.Add(template.Key + " - " + String.Join(",", template.Value.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray()));
            }

            _watchRuntime.Show();

            //var imuSensor = _watchRuntime.TrackerManager.Imu;

            //imuSensor.AddEvent("ImuTilted",
            //    (imu) => imu.YawPitchRollValues.Z > 10
            //    ).EventTriggered  += (sender, e) => Console.WriteLine("");

            //var touchSensor = _watchRuntime.TouchManager.BevelTouchSensor;

            //touchSensor.AddEvent("SideGrab",
            //    (touch) => touch.TouchStates.BevelLeft && touch.TouchStates.BevelRight
            //    ).EventTriggered += (sender, e) =>
            //    {
            //        if(e == "SideGrab")
            //            Console.WriteLine(@"{0} detected",e);
            //    };

        }

        void GestureManager_Cover(object sender, GestureDetectedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Label.Content = "Covered";
            });
        }

        void _touchManager_DoubleTap(object sender, SliderTouchEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Label.Content = "Slider Double Tap";
            });
        }

        void _touchManager_SliderTouchUp(object sender, SliderTouchEventArgs e)
        {
            _visualizer.UpdateLinearTouch(e.Sensor);
        }

        void _touchManager_SliderTouchDown(object sender, SliderTouchEventArgs e)
        {
            _visualizer.UpdateLinearTouch(e.Sensor);
        }

        void _touchManager_SlideUp(object sender, SliderTouchEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Label.Content = "Slide Up";
            });
        }

        void _touchManager_SlideDown(object sender, SliderTouchEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Label.Content = "Slide Down";
            });
        }

        void _touchManager_BevelDoubleTap(object sender, BevelTouchEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.BevelSide)
                {
                    case BevelSide.Left:
                        Rect1.Fill = Brushes.Blue;
                        break;
                    case BevelSide.Top:
                        Rect2.Fill = Brushes.Blue;
                        break;
                    case BevelSide.Right:
                        Rect3.Fill = Brushes.Blue;
                        break;
                    case BevelSide.Bottom:
                        Rect4.Fill = Brushes.Blue;
                        break;
                }
            });
            
        }
        void ImuParser_EventTriggered(object sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                Label.Content = e;
            });
        }

        void AccelerometerVisualizer_Loaded(object sender, RoutedEventArgs e)
        {
            if (WindowManager.HasWatchConnected()) return;
            WindowManager.Dock(_watchRuntime,WindowManager.DockLocation.Left);
            WindowManager.Dock(this,WindowManager.DockLocation.Right);
        }

        void GestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            _visualizer.UpdateEvents(e.Gesture.ToString());
            _detection = e.Gesture.ToString();
            Dispatcher.Invoke(() =>
            {
                Label.Content = _detection;
            });
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
                lblRaw.Content = _watchRuntime.TrackerManager.Imu.ToFormattedString();
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

        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //foreach (string item in listGesture.Items)
                //{
                //    File.AppendAllText("log.log", item+"\n");
                //}
                //_trackerManager.Stop();
                //_gestureManager.Stop();
                //_touchManager.Stop();
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
