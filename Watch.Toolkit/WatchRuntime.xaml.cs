using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Interface;
using Watch.Toolkit.Interface.DefaultFaces;

namespace Watch.Toolkit
{
    public partial class WatchRuntime:IVisualSharer
    {
        private int _peekSize;
        public TouchManager TouchManager { get; set; }
        public GestureManager GestureManager { get; set; }
        public TrackerManager TrackerManager { get; set; }
        public WatchVisual ActiveVisual { get; set; }

        private readonly WatchFaceManager _watchFaceManager = new WatchFaceManager();

        public string LastDetectedPosture { get; set; }
        public string LastDetectedGesture { get; set; }

        public WatchRuntime()
        {
            BuildWatch(WatchConfiguration.DefaultWatchConfiguration);
        }
        public WatchRuntime(WatchConfiguration watchConfiguration)
        {
            BuildWatch(watchConfiguration);
        }

        void WatchRuntime_Loaded(object sender, RoutedEventArgs e)
        {
            WindowManager.MaximizeToSecondaryMonitor(this);

            _peekSize = Convert.ToInt32(Width*0.08);
        }

        private void BuildWatch(WatchConfiguration watchConfiguration)
        {
            InitializeComponent();

            Loaded += WatchRuntime_Loaded;
            KeyDown += WatchWindow_KeyDown;

            TouchManager = new TouchManager(watchConfiguration.Hardware);
            TouchManager.Start();

            TrackerManager = new TrackerManager(watchConfiguration.Hardware,watchConfiguration.ClassifierConfiguration);
            TrackerManager.TrackGestureRecognized += TrackerManager_TrackGestureRecognized;
            TrackerManager.Start();

            GestureManager = new GestureManager(watchConfiguration.Hardware);
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

        public void NextFace()
        {
            ActiveVisual = _watchFaceManager.GetNext();

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
                _thumbnails.Remove(id);
                DrawThumbnails();
            });
        }

        public void SendThumbnail(object thumbnail, int id = 0, double x = 0, double y = 0)
        {
            Dispatcher.Invoke(() =>
            {
                var rect = thumbnail as Rectangle;
                if (rect == null) return;

                if (_thumbnails.ContainsKey(id))
                    return;
                _thumbnails.Add(id,((SolidColorBrush)rect.Fill).Color);
                DrawThumbnails();
            });
        }

        private void DrawThumbnails()
        {

            if (_thumbnails.Count == 0)
            {
                ContentHolder.BorderBrush = OuterBorder.BorderBrush= Brushes.Transparent;
                ContentHolder.BorderThickness = OuterBorder.BorderThickness =  new Thickness(0, 0, 0, 0);
                return;
            }
            var grad = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0), 
                EndPoint = new Point(0, 1)
            };

            var count = 0d;
            foreach (var gs in _thumbnails.Values.Select(c => new GradientStop
            {
                Color = c, 
                Offset = (1/(double)_thumbnails.Values.Count)*count++
            }))
            {
                grad.GradientStops.Add(gs);
            }

            OuterBorder.BorderBrush = grad;
            OuterBorder.BorderThickness = new Thickness(0, 0, _peekSize, 0);
            ContentHolder.BorderBrush = Brushes.Black;
            ContentHolder.BorderThickness = new Thickness(0, 0, _peekSize/4, 0);
        }
        private readonly Dictionary<int, Color> _thumbnails = new Dictionary<int, Color>();
    }
}
