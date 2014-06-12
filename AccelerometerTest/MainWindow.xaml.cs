using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using GestureTouch;
using Microsoft.Surface.Presentation.Controls;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Input.Recognizers;

namespace AccelerometerTest
{
    public partial class MainWindow
    {
        private readonly DtwRecognizer _dtwRecognizer = new DtwRecognizer();
        private AccelerometerData _accelerometerData;

        readonly Dictionary<int, ScatterViewItem> _touches = new Dictionary<int, ScatterViewItem>();

        private const bool ShowDetails = false;

        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            var wrap = new GestureTouchPipeline(this);
            wrap.GestureTouchDown += MainWindow_GestureTouchDown;
            wrap.GestureTouchUp += MainWindow_GestureTouchUp;
            wrap.GestureTouchMove += MainWindow_GestureTouchMove;

            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start("COM4");
        }
        void MainWindow_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            var item = new ScatterViewItem
            {
                Width = e.TouchPoint.Size.Width * 9.6,
                Height = e.TouchPoint.Size.Height * 9.6,
                Background = Brushes.White,
                Orientation = 0
            };
            item.CanMove = item.CanRotate = item.CanScale = false;
            item.Center = e.TouchPoint.Position;

            switch (_lastDetection.ToLower())
            {
                case "touch":
                    Label.Content = "LEFT HAND";
                    break;
                case "knuckle":
                    Label.Content = "KNUCKLE LEFT HAND";
                    break;
                case "pinky":
                    Label.Content = "PINKY LEFT HAND";
                    break;
                default:
                    Label.Content = "RIGHT HAND";
                    break;
            }

            _touches.Add(e.Id, item);

            View.Items.Add(item);

            UpdateVisualTouchPoint(e.Id, e.TouchPoint);
        }
        void MainWindow_GestureTouchUp(object sender, GestureTouchEventArgs e)
        {
            if (!_touches.ContainsKey(e.Id))
                return;
            View.Items.Remove(_touches[e.Id]);
            _touches.Remove(e.Id);

            Label.Content = "";
        }

        void MainWindow_GestureTouchMove(object sender, GestureTouchEventArgs e)
        {
            UpdateVisualTouchPoint(e.Id, e.TouchPoint);
        }

        private void UpdateVisualTouchPoint(int id, GestureTouchPoint point)
        {
            if (!_touches.ContainsKey(id))
                return;
            _touches[id].Width = point.Size.Width * 9.6;
            _touches[id].Height = point.Size.Height * 9.6;
            _touches[id].Center = point.Position;

            if (ShowDetails)
            {
                if (_touches[id].Content == null)
                {
                    var label = new Label { Content = "", Margin = new Thickness(-60, 0, 0, 0) };
                    _touches[id].Content = label;
                }
                ((Label)_touches[id].Content).Content =
                "x: " + point.Position.X +
                "\ny: " + point.Position.Y +
                "\nw: " + point.Size.Width * 9.6 +
                "\nh: " + point.Size.Height * 9.6;
            }
            else
            {
                if (((Label)_touches[id].Content) != null)
                    _touches[id].Content = null;
            }


            if (TouchRanges.TinyTouch.ContainsValue(_touches[id].Width)
                && TouchRanges.TinyTouch.ContainsValue(_touches[id].Width))
            {
                _touches[id].Background = Brushes.White;
            }

            else if (TouchRanges.SmallTouch.ContainsValue(_touches[id].Width)
                || TouchRanges.SmallTouch.ContainsValue(_touches[id].Width))
            {
                _touches[id].Background = Brushes.Green;
            }
            else if (TouchRanges.MediumTouch.ContainsValue(_touches[id].Width)
                && TouchRanges.MediumTouch.ContainsValue(_touches[id].Width))
            {
                _touches[id].Background = Brushes.Yellow;
            }
            else if (TouchRanges.LargeTouch.ContainsValue(_touches[id].Width)
                && TouchRanges.LargeTouch.ContainsValue(_touches[id].Width))
            {
                _touches[id].Background = Brushes.Orange;
            }
            else if (TouchRanges.VeryLargeTouch.ContainsValue(_touches[id].Width)
                && TouchRanges.VeryLargeTouch.ContainsValue(_touches[id].Width))
            {
                _touches[id].Background = Brushes.Red;
            }
        }
        void arduino_MessageReceived(object sender, Watch.Toolkit.Hardware.MessagesReceivedEventArgs e)
        {
            var data = e.Message.Split('|');

            if (data.Length != 5) return;

            _accelerometerData = new AccelerometerData(
                Convert.ToInt32(data[0]),
                Convert.ToInt32(data[1]),
                Convert.ToInt32(data[2]));

                if (_count > 0)
                {
                    _lastDetection = _dtwRecognizer.FindClosestLabel(_accelerometerData.RawData);
                }
        }

        private string _lastDetection = "";
        readonly List<AccelerometerData> _collectedData = new List<AccelerometerData>();
        private int _count;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _collectedData.Add(_accelerometerData);
            listGesture.Items.Add(_accelerometerData);

            switch (_count)
            {
                case 0:
                    _dtwRecognizer.AddTemplate("Hold",
                        new double[]
                        {
                            _accelerometerData.X,
                            _accelerometerData.Y,
                            _accelerometerData.Z
                        });
                    break;
                case 1:
                    _dtwRecognizer.AddTemplate("Touch",
                        new double[]
                        {
                            _accelerometerData.X,
                            _accelerometerData.Y,
                            _accelerometerData.Z
                        });
                    break;
                case 2:
                    _dtwRecognizer.AddTemplate("Knuckle",
                        new double[]
                        {
                            _accelerometerData.X,
                            _accelerometerData.Y,
                            _accelerometerData.Z
                        });
                    break;
                case 3:
                    _dtwRecognizer.AddTemplate("pinky",
                        new double[]
                        {
                            _accelerometerData.X,
                            _accelerometerData.Y,
                            _accelerometerData.Z
                        });
                    break;
            }
            _count++;

        }
    }
    public class TouchRanges
    {
        public static Range<double> TinyTouch = new Range<double>(0, 25);
        public static Range<double> SmallTouch = new Range<double>(26, 50);
        public static Range<double> MediumTouch = new Range<double>(51, 100);
        public static Range<double> LargeTouch = new Range<double>(101, 200);
        public static Range<double> VeryLargeTouch = new Range<double>(201, 1000);
    }
    public class AccelerometerData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public double[] RawData
        {
            get { return new double[] {X, Y, Z}; }
        }

        public AccelerometerData() { }

        public AccelerometerData(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return "X: " + X + " Y: " + Y + " Z: " + Z;
        }
    }
}
