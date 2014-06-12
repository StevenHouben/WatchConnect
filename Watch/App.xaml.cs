using System.Collections.Generic;
using System.Windows;
using Watch.Toolkit.Input;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;

namespace Watch
{
    public partial class App
    {

        private readonly GestureManager _gestureManager = new GestureManager();
        private readonly TouchManager _touchManager = new TouchManager();

        private SimpleWatchFace _watchFace;
        private LaptopWindow _laptopWindow;

        private readonly Dictionary<int,EventMonitor> _canvasSynchronizers = new Dictionary<int, EventMonitor>(10);
        private readonly Dictionary<int, EventMonitor> _objectSynchronizers = new Dictionary<int, EventMonitor>(10);

        private readonly Dictionary<int, Point> _currentTouches = new Dictionary<int, Point>(); 
        private readonly Dictionary<int,object> _currentItems = new Dictionary<int, object>(); 
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _gestureManager.GestureDetected+=_gestureManager_GestureDetected;

            _gestureManager.Start();
            _touchManager.Start();

            _watchFace = new SimpleWatchFace(_touchManager);
            _watchFace.Loaded += _watchFace_Loaded;
            _watchFace.Show();

            _laptopWindow = new LaptopWindow();
            _laptopWindow.ObjectTouchDown += _laptopWindow_ObjectTouchDown;
            _laptopWindow.ObjectTouchUp += _laptopWindow_ObjectTouchUp;
            _laptopWindow.CanvasDown += _laptopWindow_CanvasDown;
            _laptopWindow.CanvasUp += _laptopWindow_CanvasUp;
            _laptopWindow.Show();
        }

        void _watchFace_Loaded(object sender, RoutedEventArgs e)
        {
            WindowExt.MaximizeToSecondaryMonitor(_watchFace);
        }

        void _laptopWindow_CanvasUp(object sender, TouchTrackEventArgs e)
        {
            _canvasSynchronizers.Remove(e.Id);
            _laptopWindow.RemoveThumbnail(e.Id);
        }

        void _laptopWindow_CanvasDown(object sender, TouchTrackEventArgs e)
        {
            _laptopWindow.SendThumbnail(_watchFace.GetThumbnail(),e.Id,e.Position.X,e.Position.Y);

            var monitor = new EventMonitor(2000, e.Id);
            monitor.MonitorTriggered += monitor_MonitorTriggered;
            _canvasSynchronizers.Add(e.Id,monitor);
            monitor.Start();

            if (_currentTouches.ContainsKey(e.Id))
            {
                _currentTouches[e.Id] = e.Position;

            }
            else
            {
                _currentTouches.Add(e.Id, e.Position);
            }
        }

        void monitor_MonitorTriggered(object sender, TriggeredEventArgs e)
        {
            _canvasSynchronizers.Remove(e.Id);
        }

        void _laptopWindow_ObjectTouchUp(object sender, TouchTrackEventArgs e)
        {
            _currentItems.Remove(e.Id);
            _objectSynchronizers.Remove(e.Id);
            _watchFace.RemoveThumbnail(e.Id);
            _currentTouches.Remove(e.Id);

        }

        void _laptopWindow_ObjectTouchDown(object sender, TouchTrackEventArgs e)
        {
           _watchFace.SendThumbnail(_laptopWindow.GetThumbnail(sender));

            var objectMonitor = new EventMonitor(2000, e.Id);
            objectMonitor.MonitorTriggered += objectMonitor_MonitorTriggered;
            _objectSynchronizers.Add(e.Id,objectMonitor);
            objectMonitor.Start();

            _currentItems.Add(e.Id,sender);
        }

        void objectMonitor_MonitorTriggered(object sender, TriggeredEventArgs e)
        {
            _objectSynchronizers.Remove(e.Id);
        }


        void _gestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            foreach (var mon in _currentTouches)
            {
                if (e.Gesture == Gesture.SwipeRight)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var visual = _watchFace.GetVisual(mon.Key);
                        _laptopWindow.SendVisual(visual, mon.Key, mon.Value.X, mon.Value.Y);
                    });

                }
            }
            var toBeDeleted = new List<int>();
            foreach (var mon in _currentItems)
            {

                if (e.Gesture == Gesture.SwipeLeft)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var visual = _laptopWindow.GetVisual(_currentItems[mon.Key]);
                        _watchFace.SendVisual(visual, mon.Key);
                        toBeDeleted.Add(mon.Key);
                    });
                }
            }
            foreach (var id in toBeDeleted)
            {
                if (_currentItems.ContainsKey(id))
                    _currentItems.Remove(id);
                if (_objectSynchronizers.ContainsKey(id))
                    _objectSynchronizers.Remove(id);
            }
        }
    }
}
