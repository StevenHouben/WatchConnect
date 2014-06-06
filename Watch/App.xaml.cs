using System;
using System.Timers;
using System.Windows;
using Watch.Toolkit.Input;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;

namespace Watch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        private readonly GestureManager _gestureManager = new GestureManager();
        private readonly TouchManager _touchManager = new TouchManager();

        private SimpleWatchFace _watchFace;
        private LaptopWindow _laptopWindow;

        private readonly EventMonitor _canvasSynchronizer = new EventMonitor(2000);
        private readonly EventMonitor _objectSynchronizer = new EventMonitor(2000);
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

        void _laptopWindow_CanvasUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Console.WriteLine("Touch up");
            _laptopWindow.RemoveThumbnail();
            _canvasSynchronizer.Stop();
        }

        void _laptopWindow_CanvasDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Console.WriteLine("Touch down");
            _laptopWindow.SendThumbnail(_watchFace.GetThumbnail());
            _canvasSynchronizer.Elapsed += _synchronizer_Elapsed;
            _canvasSynchronizer.Start();
        }

        void _laptopWindow_ObjectTouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            _canvasSynchronizer.Stop();
            _watchFace.RemoveThumbnail();
        }

        void _laptopWindow_ObjectTouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
           _watchFace.SendThumbnail(_laptopWindow.GetThumbnail());
           _objectSynchronizer.Elapsed += _objectSynchronizer_Elapsed;
           _objectSynchronizer.Start();
        }

        void _objectSynchronizer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _objectSynchronizer.Stop();
        }

        void _gestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {

            if (e.Gesture == Gesture.SwipeLeft && _objectSynchronizer.Enabled)
            {
                Dispatcher.Invoke(() =>
                {
                    var visual = _laptopWindow.GetVisual();
                    _watchFace.SendVisual(visual);
                });

            }
            if (e.Gesture == Gesture.SwipeRight && _canvasSynchronizer.Enabled )
            {
                Dispatcher.Invoke(() =>
                {
                    var visual = _watchFace.GetVisual();
                    _laptopWindow.SendVisual(visual);
                });

            }
        }

       void _synchronizer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _canvasSynchronizer.Stop();
        }
    }
}
