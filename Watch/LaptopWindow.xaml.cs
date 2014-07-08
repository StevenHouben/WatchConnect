using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using Watch.Toolkit.Input;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Watch
{
    public partial class LaptopWindow 
    {
        public event EventHandler<TouchTrackEventArgs> ObjectTouchDown = delegate { };
        public event EventHandler<TouchTrackEventArgs> ObjectTouchUp = delegate { };

        public event EventHandler<TouchTrackEventArgs> CanvasDown = delegate { };
        public event EventHandler<TouchTrackEventArgs> CanvasUp = delegate { };

        private readonly Dictionary<int, List<Size>> _pointTrackers = 
            new Dictionary<int, List<Size>>();
        private readonly Dictionary<int, EventMonitor> _timeTrackers = 
            new Dictionary<int, EventMonitor>();
        private readonly Dictionary<int, TouchTrackEventArgs> _cachedEvents = 
            new Dictionary<int, TouchTrackEventArgs>();

        private readonly Dictionary<int, UIElement> _uiThumbnails = 
            new Dictionary<int, UIElement>();
        private readonly Dictionary<int,Size> _lastSize = new Dictionary<int, Size>(); 

        public LaptopWindow()
        {
            InitializeComponent();
            
            PreviewStylusDown += LaptopWindow_PreviewStylusDown;
            PreviewStylusMove += LaptopWindow_PreviewStylusMove;
            PreviewTouchUp += LaptopWindow_PreviewTouchUp;
        }

        void LaptopWindow_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (!_timeTrackers.ContainsKey(e.TouchDevice.Id)) 
                return;
          
            if (_timeTrackers[e.TouchDevice.Id].Trigger)
            {
                CanvasUp(sender, new TouchTrackEventArgs() { Id = e.TouchDevice.Id, Position = e.GetTouchPoint(this).Position });
            }
            _pointTrackers.Remove(e.TouchDevice.Id);
            _timeTrackers[e.TouchDevice.Id].Stop();
            _timeTrackers.Remove(e.TouchDevice.Id);
            _pointTrackers.Remove(e.TouchDevice.Id);
            _cachedEvents.Remove(e.TouchDevice.Id);
            _lastSize.Remove(e.TouchDevice.Id);
        }

        private void LaptopWindow_PreviewStylusMove(object sender, StylusEventArgs e)
        {
            var pts = e.GetStylusPoints(this);

            if (_pointTrackers.ContainsKey(e.StylusDevice.Id))
                _pointTrackers[e.StylusDevice.Id].Add(GetSize(pts[0]));

            if (_lastSize.ContainsKey(e.StylusDevice.Id))
                _lastSize[e.StylusDevice.Id] = GetSize(pts[0]);

            if (CheckIfPointsAreInRange(pts.First().ToPoint(), pts.Last().ToPoint(), 3)) return;

            if (!_timeTrackers.ContainsKey(e.StylusDevice.Id)) 
                return;

            _timeTrackers[e.StylusDevice.Id].Stop();
            _timeTrackers.Remove(e.StylusDevice.Id);
            _pointTrackers.Remove(e.StylusDevice.Id);
            _cachedEvents.Remove(e.StylusDevice.Id);
            if (_uiThumbnails.ContainsKey(e.StylusDevice.Id))
            {
                CanvasUp(this, new TouchTrackEventArgs(){Id=e.StylusDevice.Id});
            }
        }

        void LaptopWindow_PreviewStylusDown(object sender, StylusEventArgs e)
        {
            if (_timeTrackers.ContainsKey(e.StylusDevice.Id))
            {
                _timeTrackers[e.StylusDevice.Id].Stop();
                _timeTrackers.Remove(e.StylusDevice.Id);
            }
            if (_pointTrackers.ContainsKey(e.StylusDevice.Id))
                _pointTrackers.Remove(e.StylusDevice.Id);
            if(_cachedEvents.ContainsKey(e.StylusDevice.Id))
                _cachedEvents.Remove(e.StylusDevice.Id);

            _cachedEvents.Add(e.StylusDevice.Id, new TouchTrackEventArgs
                { Id = e.StylusDevice.Id, Position = e.GetPosition(this) });
            _pointTrackers.Add(e.StylusDevice.Id, new List<Size>());
            if (_lastSize.ContainsKey(e.StylusDevice.Id))
                _lastSize[e.StylusDevice.Id] = GetSize(e.GetStylusPoints(this)[0]);
            else
                _lastSize.Add(e.StylusDevice.Id,GetSize(e.GetStylusPoints(this)[0]));

            var monitor = new EventMonitor(500,e.StylusDevice.Id);
            monitor.MonitorTriggered += monitor_MonitorTriggered;
            _timeTrackers.Add(e.StylusDevice.Id, monitor);

            monitor.Start();
        }

        void monitor_MonitorTriggered(object sender, TriggeredEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CanvasDown(sender, _cachedEvents[e.Id]);
                _timeTrackers[e.Id].Trigger = true;
                _timeTrackers[e.Id].Stop();
            });
        }
        private static Size GetSize(StylusPoint stylusPoint)
        {
            return new Size(
                stylusPoint.GetPropertyValue(StylusPointProperties.Width), 
                stylusPoint.GetPropertyValue(StylusPointProperties.Height));
        }

        static Boolean CheckIfPointsAreInRange(Point p1, Point p2, int range)
        {
            var r1 = Math.Abs(p1.X - p2.X);
            var r2 = Math.Abs(p1.Y - p2.Y);

            return !(r1 > range) && !(r2 > range);
        }

        private EventCache _lastEvent;

        internal class EventCache
        {
            public Point TouchPoint { get; set; }
            public StylusEventArgs Event { get; set; }
        }

        private Rectangle _thumb;

        public object GetVisual(object uiElement)
        {
            var item = ((ScatterViewItem) uiElement);
            var visualObj = item.Content;
            View.Items.Remove(item);

            return visualObj;
        }

        public object GetThumbnail(object uiElement)
        {
            if (uiElement == null)
                return uiElement;
            var visualObj = ((ScatterViewItem)uiElement); 
                return new Rectangle { Width = Height = 60, Fill = ((Control)visualObj.Content).Background };
        }

        public void SendThumbnail(object thumbnail,int id, double x, double y)
        {
            _thumb = thumbnail as Rectangle;

            if (_thumb == null)
                return;

            var item = new ScatterViewItem
            {
                Background = _thumb.Fill,
                Center = new Point(x,y),
                Width = _lastSize[id].Width*10,
                Height = _lastSize[id].Height*10,
                Orientation = 0,
                CanRotate = false,
                CanMove = false,
                CanScale = false
            };
            item.PreviewStylusDown += _item_PreviewStylusDown;
            item.PreviewStylusUp += _item_PreviewStylusUp;
            View.Items.Add(item);

            _uiThumbnails.Add(id,item);
        }

        void _item_PreviewStylusUp(object sender, StylusEventArgs e)
        {
            ObjectTouchUp(sender, new TouchTrackEventArgs{ Id = e.StylusDevice.Id, Position = e.GetPosition(this) });
        }

        void _item_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            ObjectTouchDown(sender, new TouchTrackEventArgs() { Id = e.StylusDevice.Id, Position = e.GetPosition(this) });

            _timeTrackers[e.StylusDevice.Id].Stop();
            _timeTrackers.Remove(e.StylusDevice.Id);
            _pointTrackers.Remove(e.StylusDevice.Id);
        }

        public void RemoveThumbnail(int id)
        {
            View.Items.Remove(_uiThumbnails[id]);
            _uiThumbnails.Remove(id);
        }

        public void SendVisual(object visual, int id, double x, double y)
        {
            var item = new ScatterViewItem
            {
                Content = visual,
                Center = new Point(x,y)
            };
            item.PreviewStylusDown += _item_PreviewStylusDown;
            item.PreviewStylusUp += _item_PreviewStylusUp;


            View.Items.Add(item);
        }
    }

    public class TouchTrackEventArgs : EventArgs
    {
        public int Id { get; set; }
        public Point Position { get; set; }
    }
}
