using System;
using System.Windows;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Interface;

namespace Watch.Toolkit
{
    public partial class WatchWindow
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

        void GestureManager_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            LastDetectedGesture = e.Gesture.ToString();
        }

        void TrackerManager_TrackGestureRecognized(object sender, string e)
        {
            LastDetectedPosture = e;
        }

        public void AddWatchFace(WatchVisual visual)
        {
            _watchFaceManager.AddFace(visual);
            Content = visual;
            ActiveVisual = visual;
        }

        public void RemoveWatchFace(Guid id)
        {
            _watchFaceManager.RemoveFace(id);
        }
    }
}
