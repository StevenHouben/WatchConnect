using System;
using System.Reflection;
using Watch.Faces;
using Watch.Toolkit.Input.Touch;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Watch.Toolkit.Interface;

namespace Watch
{
    /// <summary>
    /// Interaction logic for SimpleWatchFace.xaml
    /// </summary>
    public partial class SimpleWatchFace : IVisualSharer
    {
        private TouchManager _touchManager;

        public SimpleWatchFace(TouchManager touchManager)
        {
            InitializeComponent();
            _touchManager = touchManager;
            _touchManager.BevelUp += touchManager_BevelUp;
            
        }

        void touchManager_BevelUp(object sender, BevelTouchEventArgs e)
        {
            if (e.BevelSide == BevelSide.BottomSide)
            {
                Dispatcher.Invoke(() =>
                {
                    VisualContent.Background = PickBrush();
                });
            }
            
        }
        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

        public object GetVisual()
        {
            object content = VisualContent;
            Content.Child = null;

            VisualContent = new WatchVisual();
            Content.Child = VisualContent;
            return content;
        }

        public object GetThumbnail()
        {
            return VisualContent == null ? new Rectangle {Width = Height = 100, Fill = Brushes.Transparent} : new Rectangle{Width = Height = 100, Fill = VisualContent.Background};
        }

        public void SendThumbnail(object thumbnail)
        {
            Dispatcher.Invoke(() =>
            {
                var rect = thumbnail as Rectangle;
                if (rect == null) return;
                Content.BorderBrush = rect.Fill;
                Content.BorderThickness = new Thickness(0, 0, 200, 0);
            });
        }

        public void RemoveThumbnail()
        {
            Dispatcher.Invoke(() =>
            {
                Content.BorderBrush = Brushes.Transparent;
                Content.BorderThickness = new Thickness(0, 0, 0, 0);
            });
        }

        public void SendVisual(object visual)
        {
            Dispatcher.Invoke(() =>
            {
                VisualContent = visual as WatchVisual;
                Content.Child = VisualContent;
                Content.BorderThickness = new Thickness(0, 0, 0, 0);
            });

        }
    }
}
