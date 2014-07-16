using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GestureTouch;
using Microsoft.Surface.Presentation.Controls;
using Watch.Examples.Desktop.Design;
using Watch.Toolkit;
using Watch.Toolkit.Input;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Interface;
using Watch.Toolkit.Interface.DefaultFaces;
using Watch.Toolkit.Processing.MachineLearning;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace Watch.Examples.Desktop
{
    public partial class DesktopWindow
    {
        const int TouchHoldTime = 100;              
        const int GestureFrameTime = 2000;

        private readonly WatchRuntime _watchWindow;
        readonly WatchConfiguration _configuration = new WatchConfiguration();

        readonly Dictionary<int,DataEventMonitor<ScatterViewItem>> _swipeGestureTrackers = 
            new Dictionary<int, DataEventMonitor<ScatterViewItem>>();

        readonly Dictionary<int, DataEventMonitor<ScatterViewItem>> _touchHoldMonitors =
            new Dictionary<int, DataEventMonitor<ScatterViewItem>>();

        readonly Dictionary<int, ScatterViewItem> _selectedItems =
            new Dictionary<int, ScatterViewItem>();


        public DesktopWindow()
        {
            InitializeComponent();

            KeyDown += DesktopWindow_KeyDown;

            _configuration.ClassifierConfiguration = new ClassifierConfiguration(
                new List<string> {"Right Hand", "Left Hand", "Left Knuckle", "Hand"},
                AppDomain.CurrentDomain.BaseDirectory + "recording16.log");

            _watchWindow = new WatchRuntime(_configuration);

            var touchPipeline = new GestureTouchPipeline(this);
            touchPipeline.GestureTouchDown += touchPipeline_GestureTouchDown;
            touchPipeline.GestureTouchMove += touchPipeline_GestureTouchMove;
            touchPipeline.GestureTouchUp += touchPipeline_GestureTouchUp;

            WindowState = WindowState.Normal;

            Left = 300;
            Top = 0;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Width = Screen.PrimaryScreen.WorkingArea.Width - 700;

            _watchWindow.GestureManager.GestureDetected += GestureManager_GestureDetected;

            _watchWindow.AddWatchFace(new Clock());

            //_watchWindow.Width = 300;
            //_watchWindow.Height = 300;
            //_watchWindow.WindowStyle = WindowStyle.ToolWindow;
            //_watchWindow.WindowState = WindowState.Normal;
            //_watchWindow.Left = 0;
            //_watchWindow.Top = 0;

            _watchWindow.Show();
        }

        private bool IsSelectMode()
        {
            return false;
            //Todo: this is debug code
            //return _watchWindow.LastDetectedPosture == _configuration.ClassifierConfiguration.Labels.First();
        }
        void DesktopWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    _watchWindow.GestureManager.SimulateEvent(
                        GestureEvents.GestureDetected,
                        new GestureDetectedEventArgs(Gesture.SwipeLeft));
                    break;
                case Key.Right:
                    _watchWindow.GestureManager.SimulateEvent(
                        GestureEvents.GestureDetected,
                        new GestureDetectedEventArgs(Gesture.SwipeRight));
                    break;
            }
        }

        void GestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {

            switch (e.Gesture)
            {
                case Gesture.SwipeRight:
                {
                    var toRemoveList = new List<int>();
                    foreach (var mon in _swipeGestureTrackers)
                    {
                        var localMon = mon;
                        Dispatcher.Invoke(() =>
                        {
                            var visual = _watchWindow.GetVisual();

                            _watchWindow.AddWatchFace(new WatchApplication { Background = Helper.RandomBrush() });

                            var floatingItem = new ScatterViewItem
                            {
                                Background = Brushes.Transparent,
                                Content = visual,
                                Center = localMon.Value.Data.Center,
                                Width = 200,
                                Height = 300,
                                CanScale = true,
                                CanRotate = false
                            };

                            floatingItem.PreviewTouchDown += floatingItem_PreviewTouchDown;
                            floatingItem.PreviewTouchUp += floatingItem_PreviewTouchUp;

                            View.Items.Remove(localMon.Value.Data);
                            toRemoveList.Add(localMon.Key);
                            View.Items.Add(floatingItem);
                        });
                    }

                    foreach (var item in toRemoveList)
                    {
                        _swipeGestureTrackers.Remove(item);
                    }
                }
                    break;
                case Gesture.SwipeLeft:
                    var toRemoveItems = new List<int>();
                    foreach (var item in _selectedItems)
                    {
                        var localItem = item;
                        Dispatcher.Invoke(() =>
                        {
                            var content = localItem.Value.Content as WatchVisual;
                            localItem.Value.Content = new Rectangle();
                            if (content != null)
                                _watchWindow.AddWatchFace(content);

                            _watchWindow.RemoveThumbnail(localItem.Key);
                            toRemoveItems.Add(localItem.Key);
                            View.Items.Remove(localItem.Value);
                        });
                    }
                    foreach (var item in toRemoveItems)
                    {
                        _selectedItems.Remove(item);
                    }
                    break;
            }
        }

        void floatingItem_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            _selectedItems.Remove(e.TouchDevice.Id);
            _watchWindow.RemoveThumbnail(e.TouchDevice.Id);
        }

        void floatingItem_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (_selectedItems.ContainsKey(e.TouchDevice.Id))
                _selectedItems.Remove(e.TouchDevice.Id);

            if (IsSelectMode())
                return;

            _selectedItems.Add(e.TouchDevice.Id,(ScatterViewItem) sender);
            _watchWindow.SendThumbnail(
                new Rectangle
                {
                    Fill = ((WatchVisual)((ScatterViewItem) sender).Content).Background
                },e.TouchDevice.Id);
        }
        void touchPipeline_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            LblMode.Content = _watchWindow.LastDetectedPosture;

            if (_selectedItems.ContainsKey(e.Id))
                return;

            if (IsSelectMode())
                return;

            var progress = new ProgressBar
            {
                Minimum = 0,
                Maximum = TouchHoldTime, 
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = _watchWindow.ActiveVisual.Background
            };

            var touchVisualization = new ScatterViewItem
            {
                Background = Brushes.Transparent,
                CanScale = false,
                CanRotate = false,
                Width = 100,
                Height = 20,
                Center = e.TouchPoint.Position,
                Content = progress
            };

            View.Items.Add(touchVisualization);
            var monitor = new DataEventMonitor<ScatterViewItem>(1, e.Id, touchVisualization);
            monitor.MonitorTriggered += monitor_MonitorTriggered;
            monitor.Start();

            _touchHoldMonitors.Add(e.Id,monitor);
        }

        readonly object _progressLock= new object();
        void monitor_MonitorTriggered(object sender, DataTriggeredEventArgs<ScatterViewItem> e)
        {
            if (!_touchHoldMonitors.ContainsKey(e.Id)) return;
            if (_touchHoldMonitors[e.Id].Counter == TouchHoldTime)
            {
                _touchHoldMonitors[e.Id].Stop();

                Dispatcher.Invoke(() =>
                {
                    e.Data.Content = null;
                    e.Data.Background = _watchWindow.ActiveVisual.Background;
                    e.Data.BorderThickness = new Thickness(1);
                    e.Data.BorderBrush = Brushes.Black;
                    var swipeMonitor = new DataEventMonitor<ScatterViewItem>(GestureFrameTime, e.Id, e.Data);
                    swipeMonitor.MonitorTriggered += gestureFrameMonitor_MonitorTriggered;
                    swipeMonitor.Start();

                    _swipeGestureTrackers.Add(e.Id, swipeMonitor);

                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    if (!_touchHoldMonitors.ContainsKey(e.Id)) return;
                    lock (_progressLock)
                    {
                        ((ProgressBar) _touchHoldMonitors[e.Id].Data.Content).Value =
                            _touchHoldMonitors[e.Id].Counter++;
                    }
                });
            }
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
            Dispatcher.Invoke(() => View.Items.Remove(_touchHoldMonitors[e.Id].Data));
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
                if (!CheckIfPointsAreInRange(e.TouchPoint.Position, _touchHoldMonitors[e.Id].Data.Center, 10))
                {
                    _touchHoldMonitors[e.Id].Stop();
                    Dispatcher.Invoke(() => View.Items.Remove(_touchHoldMonitors[e.Id].Data));
                }
            }
            if(!_swipeGestureTrackers.ContainsKey(e.Id))
                return;

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
