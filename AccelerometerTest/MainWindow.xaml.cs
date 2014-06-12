using System;
using System.Windows;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            var wrap = new GestureTouchPipeline(View);
            wrap.GestureTouchDown += MainWindow_GestureTouchDown;
            wrap.GestureTouchUp += MainWindow_GestureTouchUp;
            wrap.GestureTouchMove += MainWindow_GestureTouchMove;

            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start("COM4");

            cbGestureList.ItemsSource = 
                new List<string> { "Right", "Left Index ", "Left Middle", "Left Pinky", "Left Knuckle" };

            cbGestureList.SelectedIndex = 0;
        }
        void MainWindow_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            var item = new ScatterViewItem
            {
                Width = e.TouchPoint.Size.Width * 9.6,
                Height = e.TouchPoint.Size.Height * 9.6,
                Background = Brushes.White,
                Orientation = 0,
                CanMove = false,
                CanRotate =false,
                CanScale = false,
                Center = e.TouchPoint.Position
            };

            Label.Content = _lastDetection;

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
            try
            {
                var data = e.Message.Split('|');

                if (data.Length != 5) return;

                _accelerometerData = new AccelerometerData(
                    Convert.ToInt32(data[0]),
                    Convert.ToInt32(data[1]),
                    Convert.ToInt32(data[2]));
                    var result = _dtwRecognizer.ComputeClosestLabelAndCosts(_accelerometerData.RawData);
                _lastDetection = result.Item1;

                Dispatcher.Invoke(() =>
                {
                    lblRaw.Content = _accelerometerData.ToString();
                    lblDTW.Content = "";
                    foreach (var item in result.Item2)
                    {
                        lblDTW.Content += item.Key + " " + item.Value + "\n";
                    }
                });
            }
            catch (Exception)
            {
                Console.WriteLine("");
            }
        }

        private string _lastDetection = "";
        readonly List<AccelerometerData> _collectedData = new List<AccelerometerData>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _collectedData.Add(_accelerometerData);
            listGesture.Items.Add(cbGestureList.Text+" "+ _accelerometerData);

            _dtwRecognizer.AddTemplate(cbGestureList.Text,
                          new double[]
                        {
                            _accelerometerData.X,
                            _accelerometerData.Y,
                            _accelerometerData.Z
                        });
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
