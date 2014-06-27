using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using GestureTouch;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Sensors;
using Watch.Toolkit.Sensors.MachineLearning;

namespace Watch.Toolkit.Applications
{
    public partial class MainWindow
    {
        private readonly Accelerometer _accelerometer = new Accelerometer();
        private readonly TreeClassifier _classifier;
        private readonly DtwClassifier _dtwClassifier;

        private string _lastDetection = "";
        private double _lastDetectionCost;
        private int _lastDetectedClassification;
        private int _aX, _aY, _aZ;
        private int _pX, _pY, _pZ;

        public MainWindow()
        {
            InitializeComponent();

            _classifier = new TreeClassifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording3.log",3, new List<string>{"Right","Left Index","Left Knuckle"});

            _dtwClassifier = new DtwClassifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording3.log", 3, new List<string>{"Right","Left Index","Left Knuckle"});

            _classifier.Run(MachineLearningAlgorithm.Id3);

            _dtwClassifier.Run();

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            TouchVisualizer.GestureTouchUp += MainWindow_GestureTouchUp;
            TouchVisualizer.GestureTouchDown += TouchVisualizer_GestureTouchDown;
            TouchVisualizer.GestureTouchMove += TouchVisualizer_GestureTouchMove;

            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start();

            cbGestureList.ItemsSource = 
                new List<string> { "Right", "Left Index ", "Left Middle", "Left Pinky", "Left Knuckle" };

            cbGestureList.SelectedIndex = 0;

            KeyDown += MainWindow_KeyDown;

        }

        void TouchVisualizer_GestureTouchMove(object sender, GestureTouchEventArgs e)
        {
            var avg = (_aX + _aY + _aZ)/1.5;
            //var inRange = (_lastDetectionCost < 650);
            var isTouching = _lastDetectedClassification;
            if(LabelClassifierLookUpTable.ContainsKey(_lastDetection))
                Label.Content =  isTouching == LabelClassifierLookUpTable[_lastDetection] ? _lastDetection : "Right Hand";
            Label.Content = _lastDetection;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                Environment.Exit(0);
        }

        void TouchVisualizer_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            
          
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

                _accelerometer.Update(
                    Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[8], CultureInfo.InvariantCulture));
                    var result = _dtwClassifier.ComputeLabelAndCosts(_accelerometer.RawData);
                    _lastDetection = result.Item1;
                    var computedLabel = _classifier.ComputeValue(_accelerometer.RawData);
                    _lastDetectedClassification = computedLabel == -1 ? _lastDetectedClassification : computedLabel;

                    Dispatcher.Invoke(() =>
                    {
                        lblRaw.Content = _accelerometer.ToFormattedString();
                        lblDTW.Content = "";
                        lblDT.Content = _lastDetectedClassification;
                        foreach (var item in result.Item2)
                        {
                            lblDTW.Content += item.Key + " " + item.Value + "\n";
                            if (item.Key == result.Item1)
                                _lastDetectionCost = item.Value;
                        }
                    });

                    CalculateDistance(_accelerometer);

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
            listGesture.Items.Add(cbGestureList.Text + " " + _accelerometer);
            LabelClassifierLookUpTable.Add(cbGestureList.Text,_lastDetectedClassification);
            _dtwClassifier.AddTemplate(cbGestureList.Text,_accelerometer.RawData);

            cbGestureList.SelectedIndex++;
        }
        public Dictionary<string,int> LabelClassifierLookUpTable = new Dictionary<string, int>(); 
    }
}
