using System;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Interface;
using Watch.Toolkit.Interface.DefaultFaces;

namespace Watch.Toolkit
{
    public partial class WatchWindow:IVisualSharer
    {
        public TouchManager TouchManager { get; set; }
        public GestureManager GestureManager { get; set; }
        public TrackerManager TrackerManager { get; set; }
        public WatchVisual ActiveVisual { get; set; }

        private readonly WatchFaceManager _watchFaceManager = new WatchFaceManager();

        public string LastDetectedPosture { get; set; }
        public string LastDetectedGesture { get; set; }

        public WatchWindow()
        {
            BuildWatch(WatchConfiguration.DefaultWatchConfiguration);
        }
        public WatchWindow(WatchConfiguration watchConfiguration)
        {
            BuildWatch(watchConfiguration);
        }

        private void BuildWatch(WatchConfiguration watchConfiguration)
        {
            InitializeComponent();

            KeyDown += WatchWindow_KeyDown;
            Width = watchConfiguration.DisplaySize.Width;
            Height = watchConfiguration.DisplaySize.Height;

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;

            WindowManager.MaximizeToSecondaryMonitor(this);

            TouchManager = new TouchManager();
            TouchManager.Start();

            TrackerManager = new TrackerManager(watchConfiguration.ClassifierConfiguration);
            TrackerManager.TrackGestureRecognized += TrackerManager_TrackGestureRecognized;
            TrackerManager.Start();

            GestureManager = new GestureManager();
            GestureManager.GestureDetected += GestureManager_GestureDetected;
            GestureManager.Start();
        }

        void WatchWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                Environment.Exit(0);
        }

        void GestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            LastDetectedGesture = e.Gesture.ToString();
        }

        void TrackerManager_TrackGestureRecognized(object sender, LabelDetectedEventArgs e)
        {
            LastDetectedPosture = e.Detection;
        }

        public void AddWatchFace(WatchVisual visual)
        {
            _watchFaceManager.AddFace(visual);
            ContentHolder.Child = visual;
            ActiveVisual = visual;
        }

        public void RemoveWatchFace(Guid id)
        {
            _watchFaceManager.RemoveFace(id);
        }

        public object GetVisual(int id=-1)
        {
            var content = ContentHolder.Child as WatchVisual;
            ContentHolder.Child = null;

            if (content == null) return null;

            _watchFaceManager.RemoveFace(content.Id);
            var visualContent = FindNextWatchFace();
            ContentHolder.Child = visualContent;
            return content;
        }

        private WatchVisual FindNextWatchFace()
        {
            return _watchFaceManager.HasFaces() ? _watchFaceManager.GetNext() : new Clock();
        }

        public object GetThumbnail(int id)
        {
            return ActiveVisual.BuildThumbnail();
        }

        public void RemoveThumbnail(int id = 0)
        {
            Dispatcher.Invoke(() =>
            {
                ContentHolder.BorderBrush = Brushes.Transparent;
                ContentHolder.BorderThickness = new Thickness(0, 0, 0, 0);
            });
        }

        public void SendThumbnail(object thumbnail, int id = 0, double x = 0, double y = 0)
        {
            Dispatcher.Invoke(() =>
            {
                var rect = thumbnail as Rectangle;
                if (rect == null) return;
                ContentHolder.BorderBrush = rect.Fill;
                ContentHolder.BorderThickness = new Thickness(0, 0, 20, 0);
            });
        }
    }
}
