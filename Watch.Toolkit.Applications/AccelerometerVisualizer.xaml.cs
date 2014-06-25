using System;
using System.Globalization;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using GestureTouch;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Input.Recognizers;
using Watch.Toolkit.Sensors;
using Watch.Toolkit.Sensors.MachineLearning;

namespace AccelerometerTest
{
    public partial class MainWindow
    {
        private readonly DtwRecognizer _dtwRecognizer = new DtwRecognizer();
        private Accelerometer _accelerometerData;
        private readonly Classifier _classifier;

        private string _lastDetection = "";
        private double _lastDetectionCost;
        private int _lastDetectedClassification;
        private int _aX, _aY, _aZ;
        private int _pX, _pY, _pZ;

        public MainWindow()
        {
            InitializeComponent();

            _classifier = new Classifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording3.log",3);
            _classifier.Run(MachineLearningAlgorithm.ID3);

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            TouchVisualizer.GestureTouchUp += MainWindow_GestureTouchUp;
            TouchVisualizer.GestureTouchDown += TouchVisualizer_GestureTouchDown;

            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start("COM4");

            cbGestureList.ItemsSource = 
                new List<string> { "Right", "Left Index ", "Left Middle", "Left Pinky", "Left Knuckle" };

            cbGestureList.SelectedIndex = 0;

            KeyDown += MainWindow_KeyDown;

        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                Environment.Exit(0);
        }

        void TouchVisualizer_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            var avg = 1;// (_aX + _aY + _aZ)/1.5;
            var inRange = (_lastDetectionCost < 650);
            var isTouching = _lastDetectedClassification;
            Label.Content =  avg> 0 &&  inRange && isTouching==1 ? _lastDetection : "Right Hand";
        }

        void MainWindow_GestureTouchUp(object sender, GestureTouchEventArgs e)
        {
            Label.Content = "";
        }

        void arduino_MessageReceived(object sender, Watch.Toolkit.Hardware.MessagesReceivedEventArgs e)
        {
            try
            {
                if (!e.Message.StartsWith("A"))
                    return;
                var data = e.Message.Split(',');

                if (data.Length != 9) return;

                _accelerometerData = new Accelerometer(
                    Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[8], CultureInfo.InvariantCulture));
                    var result = _dtwRecognizer.ComputeClosestLabelAndCosts(_accelerometerData.RawData);
                    _lastDetection = result.Item1;
                    var computedLabel = _classifier.ComputeLabel(_accelerometerData.RawData);
                    _lastDetectedClassification = computedLabel == -1 ? _lastDetectedClassification : computedLabel;

                    Dispatcher.Invoke(() =>
                    {
                        lblRaw.Content = _accelerometerData.ToFormattedString();
                        lblDTW.Content = "";
                        lblDT.Content = _lastDetectedClassification;
                        foreach (var item in result.Item2)
                        {
                            lblDTW.Content += item.Key + " " + item.Value + "\n";
                            if (item.Key == result.Item1)
                                _lastDetectionCost = item.Value;
                        }
                    });

                CalculateDistance(_accelerometerData);

            }
            catch (Exception)
            {
                Console.WriteLine("");
            }
        }

        private void CalculateDistance(Accelerometer accelerometerData)
        {
            if (_pX == 0)
                _pX = (int)accelerometerData.Fx;
            if (_pY == 0)
                _pY = (int)accelerometerData.Fy;
            if (_pZ == 0)
                _pZ = (int)accelerometerData.Fz;

            _aX = Math.Abs((int)((Math.Abs(_pX) - Math.Abs(accelerometerData.Fx))/50));
            _aY = Math.Abs((int)((Math.Abs(_pY) - Math.Abs(accelerometerData.Fy))/50));
            _aZ = Math.Abs((int)((Math.Abs(_pZ) - Math.Abs(accelerometerData.Fz))/50));

            _pX = (int)accelerometerData.Fx;
            _pY = (int)accelerometerData.Fy;
            _pZ = (int)accelerometerData.Fz;

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listGesture.Items.Add(cbGestureList.Text+" "+ _accelerometerData);

            _dtwRecognizer.AddTemplate(cbGestureList.Text,
                          new []
                        {
                            _accelerometerData.X,
                            _accelerometerData.Y,
                            _accelerometerData.Z
                        });

            cbGestureList.SelectedIndex++;
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
    
}
