using System;
using System.Collections.Generic;
using System.Windows;
using GestureTouch;
using Microsoft.Surface.Presentation.Controls;
using Watch.Examples.Desktop.Design;
using Watch.Toolkit;
using Watch.Toolkit.Input;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.Desktop
{
    public partial class DesktopWindow
    {
        const int TouchHoldTime = 500;              
        const int GestureFrameTime = 2000;          

        readonly WatchWindow _watchWindow = new WatchWindow();
        readonly WatchConfiguration _configuration = new WatchConfiguration();

        readonly Dictionary<int,DataEventMonitor<ScatterViewItem>> _swipeGestureTrackers = 
            new Dictionary<int, DataEventMonitor<ScatterViewItem>>();

        readonly Dictionary<int,DataEventMonitor<GestureTouchPoint>> _touchHoldMonitors =
            new Dictionary<int, DataEventMonitor<GestureTouchPoint>>();

        public DesktopWindow()
        {
            InitializeComponent();

            _configuration.ClassifierConfiguration = new ClassifierConfiguration(
                new List<string> {"Normal Mode", "Left Index", "Left Knuckle", "Hand"},
                AppDomain.CurrentDomain.BaseDirectory + "recording16.log");

            var touchPipeline = new GestureTouchPipeline(View);
            touchPipeline.GestureTouchDown += touchPipeline_GestureTouchDown;
            touchPipeline.GestureTouchMove += touchPipeline_GestureTouchMove;
            touchPipeline.GestureTouchUp += touchPipeline_GestureTouchUp;

            _watchWindow.GestureManager.GestureDetected += GestureManager_GestureDetected;

            _watchWindow.AddWatchFace(new WatchApplication());

            _watchWindow.WindowState = WindowState.Minimized;
            _watchWindow.Show();
        }

        void GestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            //TODO check if any gesture frames are open and stop them

            switch (e.Gesture)
            {
                case Gesture.HoverRight:
                    //move information to laptop display
                    break;
                case Gesture.HoverLeft:
                    //move information to watch display
                    break;
            }
        }
        void touchPipeline_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            Console.WriteLine(_watchWindow.LastDetectedPosture);
            if (_watchWindow.LastDetectedPosture == "Normal Mode") 
                return;

            LblMode.Content = _watchWindow.LastDetectedPosture;

            var monitor = new DataEventMonitor<GestureTouchPoint>(TouchHoldTime, e.Id, e.TouchPoint);
            monitor.MonitorTriggered += monitor_MonitorTriggered;
            monitor.Start();

            _touchHoldMonitors.Add(e.Id,monitor);
        }

        void monitor_MonitorTriggered(object sender, DataTriggeredEventArgs<GestureTouchPoint> e)
        {
            _touchHoldMonitors[e.Id].Stop();

            Dispatcher.Invoke(() =>
            {
                var touchVisualization = new ScatterViewItem
                {
                    Background = _watchWindow.ActiveVisual.Background,
                    CanScale = false,
                    CanRotate = false,
                    Width = 50,
                    Height = 50,
                    Center = e.Data.Position
                };
                var swipeMonitor = new DataEventMonitor<ScatterViewItem>(GestureFrameTime, e.Id, touchVisualization);
                swipeMonitor.MonitorTriggered += gestureFrameMonitor_MonitorTriggered;
                swipeMonitor.Start();

                _swipeGestureTrackers.Add(e.Id, swipeMonitor);
               
                View.Items.Add(touchVisualization);
            });
        }

        void gestureFrameMonitor_MonitorTriggered(object sender, DataTriggeredEventArgs<ScatterViewItem> e)
        {
            if (!_swipeGestureTrackers.ContainsKey(e.Id)) return;
            _swipeGestureTrackers[e.Id].Stop();
        }

        void touchPipeline_GestureTouchUp(object sender, GestureTouchEventArgs e)
        {
            if (!_touchHoldMonitors.ContainsKey(e.Id)) return;
            _touchHoldMonitors[e.Id].Stop();
            _touchHoldMonitors.Remove(e.Id);
            LblMode.Content = "";

            if (!_swipeGestureTrackers.ContainsKey(e.Id)) return;

            Dispatcher.Invoke(() => View.Items.Remove(_swipeGestureTrackers[e.Id].Data));
            _swipeGestureTrackers.Remove(e.Id);
        }

        void touchPipeline_GestureTouchMove(object sender, GestureTouchEventArgs e)
        {
            if (_touchHoldMonitors.ContainsKey(e.Id))
            {
                if(!CheckIfPointsAreInRange(e.TouchPoint.Position,_touchHoldMonitors[e.Id].Data.Position,10))
                    _touchHoldMonitors[e.Id].Stop();
            }
            if(!_swipeGestureTrackers.ContainsKey(e.Id))return;

            _swipeGestureTrackers[e.Id].Data.Center = e.TouchPoint.Position;
        }
        static Boolean CheckIfPointsAreInRange(Point p1, Point p2, int treshold)
        {
            var r1 = Math.Abs(p1.X - p2.X);
            var r2 = Math.Abs(p1.Y - p2.Y);

            return !(r1 > treshold) && !(r2 > treshold);
        }


    }
}
