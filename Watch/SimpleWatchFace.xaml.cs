using System;
using System.Collections.Generic;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Interface;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch
{
    public partial class SimpleWatchFace : IVisualSharer
    {
        public TouchManager TouchManager { get; set; }
        public GestureManager GestureManager { get; set; }
        public TrackerManager TrackerManager { get; set; }
        public SimpleWatchFace()
        {
            InitializeComponent();
            TouchManager = new TouchManager();
            TouchManager.BevelUp += touchManager_BevelUp;
            TouchManager.Start();

            var classifierConfiguration = new ClassifierConfiguration(
                new List<string> { "Normal Mode", "Left Index", "Left Knuckle", "Hand" }, AppDomain.CurrentDomain.BaseDirectory + "recording16.log");

            TrackerManager = new TrackerManager(classifierConfiguration);
            TrackerManager.TrackGestureRecognized += _trackerManager_TrackGestureRecognized;
            TrackerManager.Start();

            GestureManager = new GestureManager();
            GestureManager.Start();
            
        }

        static void _trackerManager_TrackGestureRecognized(object sender, TrackGestureEventArgs e)
        {
            Console.WriteLine(e.DtwLabel + @" - " +e.TreeLabel);
        }

        void touchManager_BevelUp(object sender, BevelTouchEventArgs e)
        {
            if (e.BevelSide == BevelSide.BottomSide)
            {
                Dispatcher.Invoke(() =>
                {
                    if(VisualContent !=null)
                        VisualContent.Background = PickBrush();
                });
            }
            
        }
        private static Brush PickBrush()
        {
            var rnd = new Random();

            var properties = typeof(Brushes).GetProperties();

            return (SolidColorBrush) properties[rnd.Next(properties.Length)].GetValue(null, null);
        }

        public object GetVisual(int id=0)
        {
            object content = VisualContent;
            Content.Child = null;

            VisualContent = new WatchVisual();
            Content.Child = VisualContent;
            return content;
        }

        public object GetThumbnail(int id=0)
        {
            return VisualContent == null ? new Rectangle {Width = Height = 100, Fill = Brushes.Transparent} : new Rectangle{Width = Height = 100, Fill = VisualContent.Background, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch};
        }

        public void SendThumbnail(object thumbnail,int id=0, double x = 0, double y = 0)
        {
            Dispatcher.Invoke(() =>
            {
                var rect = thumbnail as Rectangle;
                if (rect == null) return;
                Content.BorderBrush = rect.Fill;
                Content.BorderThickness = new Thickness(0, 0, 200, 0);
            });
        }

        public void RemoveThumbnail(int id=0)
        {
            Dispatcher.Invoke(() =>
            {
                Content.BorderBrush = Brushes.Transparent;
                Content.BorderThickness = new Thickness(0, 0, 0, 0);
            });
        }

        public void SendVisual(object visual,int id=0, double x = 0, double y = 0)
        {
            Dispatcher.Invoke(() =>
            {
                VisualContent = visual as WatchVisual;
                if (VisualContent != null) Content.Child = VisualContent.Clone();
                Content.BorderThickness = new Thickness(0, 0, 0, 0);
            });

        }
    }
}
