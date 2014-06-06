using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using NativeTouchSupport;
using Watch.Toolkit.Input;
using Watch.Toolkit.Interface;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Watch
{
    public partial class LaptopWindow :IVisualSharer
    {
        public event EventHandler<TouchEventArgs> ObjectTouchDown = delegate { };
        public event EventHandler<TouchEventArgs> ObjectTouchUp = delegate { };

        public event EventHandler<TouchEventArgs> CanvasDown = delegate { };
        public event EventHandler<TouchEventArgs> CanvasUp = delegate { };

        private readonly EventMonitor _holdMonitor = new EventMonitor(500);


        public LaptopWindow()
        {
            InitializeComponent();
            
            View.PreviewTouchDown += View_PreviewTouchDown;
            View.PreviewTouchUp += View_PreviewTouchUp;
            View.PreviewTouchMove += View_PreviewTouchMove;

            PreviewTouchDown += LaptopWindow_PreviewTouchDown;

            var a = Tablet.TabletDevices;

        }

        void LaptopWindow_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            throw new NotImplementedException();
        }

        void View_PreviewTouchMove(object sender, TouchEventArgs e)
        {

            var pe = e.GetTouchPoint(this);

            var pts = e.GetIntermediateTouchPoints(this);

            if(!CheckIfPointsAreInRange(pts.First().Position,pts.Last().Position,3))
            {
                _holdMonitor.Stop();
            }
        }

        static Boolean CheckIfPointsAreInRange(Point p1, Point p2, int range)
        {
            var r1 = Math.Abs(p1.X - p2.X);
            var r2 = Math.Abs(p1.Y - p2.Y);

            return !(r1 > range) && !(r2 > range);
        }

        void View_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            _touching = false;
            _holdMonitor.Stop();
            CanvasUp(sender, e);
        }

        void View_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (_holdMonitor.Enabled) 
                return;

            _holdMonitor.Elapsed += _holdMonitor_Elapsed;

             var p = e.GetTouchPoint(this);
                _lastTouch = new Point(p.Position.X, p.Position.Y - 60);

            _lastEvent = new EventCache
            {
                Event = e,
                TouchPoint = _lastTouch
            };

            _holdMonitor.Start();
        }

        private EventCache _lastEvent;

        internal class EventCache
        {
            public Point TouchPoint { get; set; }
            public TouchEventArgs Event { get; set; }
        }

        void _holdMonitor_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_touching) return;
                _touching = true;
                CanvasDown(sender, _lastEvent.Event);
            });
        }

        private bool _touching;
        private Point _lastTouch = new Point(0,0);

        private Control _visual;
        private Rectangle _thumb;
        private ScatterViewItem _item;

        public object GetVisual()
        {
            var visualObj = ((ScatterViewItem) _visual).Content;
            ((ScatterViewItem) _visual).Content = null;
            _visual.PreviewTouchDown -= _item_PreviewTouchDown;
            _item.PreviewTouchUp -= _item_PreviewTouchUp;
            View.Items.Remove(_visual);
            _visual = null;
            return visualObj;
        }

        public object GetThumbnail()
        {
            if (_visual == null)
                return _visual;
            return new Rectangle { Width = Height = 60, Fill = ((Control)((ScatterViewItem)_visual).Content).Background };
        }

        public void SendThumbnail(object thumbnail)
        {
            _thumb = thumbnail as Rectangle;

            if (_thumb == null)
                return;

            if (!_touching) return;

            _item = new ScatterViewItem
            {
                Content = _thumb,
                Center = _lastTouch,
                Orientation = 0,
                CanRotate = false,
                CanMove = false,
                CanScale = false
            };
            _item.PreviewTouchDown += _item_PreviewTouchDown;
            _item.PreviewTouchUp += _item_PreviewTouchUp;
            View.Items.Add(_item);
        }

        void _item_PreviewTouchUp(object sender, TouchEventArgs e)
        {
                ObjectTouchUp(sender, e);

            _visual = null;

        }

        void _item_PreviewTouchDown(object sender,TouchEventArgs e)
        {
            _visual = sender as Control;
            ObjectTouchDown(sender, e);

            _holdMonitor.Stop();

        }
        public void RemoveThumbnail()
        {
            if (_thumb == null) return;
            _thumb = null;
            View.Items.Remove(_item);
        }

        public void SendVisual(object visual)
        {
            var item = new ScatterViewItem
            {
                Content = visual,
                Center = _lastTouch
            };
            item.PreviewTouchDown += _item_PreviewTouchDown;
            item.PreviewTouchUp += _item_PreviewTouchUp;


            View.Items.Add(item);
        }
    }
}
