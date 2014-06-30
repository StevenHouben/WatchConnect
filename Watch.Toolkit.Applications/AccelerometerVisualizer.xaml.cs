using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GestureTouch;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;
using Watch.Toolkit.Sensors;

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

        public Dictionary<string, int> LabelClassifierLookUpTable = new Dictionary<string, int>(); 

        public MainWindow()
        {
            InitializeComponent();

            _classifier = new TreeClassifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording6.log", 5, new List<string> { "Normal Mode", "Left Index", "Left Knuckle", "Flat hand", "Touch hand", "Flat hand" });

            _dtwClassifier = new DtwClassifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording6.log", 5, new List<string> { "Normal Mode", "Left Index", "Left Knuckle", "Touch hand", "Flat hand"});

            _classifier.Run(MachineLearningAlgorithm.Id3);

            LabelClassifierLookUpTable.Add("Right",0);
            LabelClassifierLookUpTable.Add("Left Index", 1);
            LabelClassifierLookUpTable.Add("Left Knuckle", 2);

            _dtwClassifier.Run();

            foreach (var template in _dtwClassifier.GetTemplates())
            {
                listGesture.Items.Add(template.Key + " - " + String.Join(",", template.Value.Select(p => p.ToString()).ToArray()));
            }

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

            TouchVisualizer.GestureTouchUp += MainWindow_GestureTouchUp;
            TouchVisualizer.GestureTouchDown += TouchVisualizer_GestureTouchDown;
            TouchVisualizer.GestureTouchMove += TouchVisualizer_GestureTouchMove;

            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start();

            cbGestureList.ItemsSource = 
                new List<string> { "None", "Left Index ", "Left Middle", "Left Pinky", "Left Knuckle" };

            cbGestureList.SelectedIndex = 0;

            KeyDown += MainWindow_KeyDown;

        }

        void TouchVisualizer_GestureTouchMove(object sender, GestureTouchEventArgs e)
        {
            
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                Environment.Exit(0);
        }

        void TouchVisualizer_GestureTouchDown(object sender, GestureTouchEventArgs e)
        {
            var avg = (_accelerometer.DistanceValues.Sum()) / 1.5;
            var touch = _accelerometer.DistanceValues.Sum() > 1.5;
            var inRange = (_lastDetectionCost < 1000);
            var isTouching = _lastDetectedClassification;
            Label.Content = inRange ? _lastDetection : "Normal Mode";
          
        }

        void MainWindow_GestureTouchUp(object sender, GestureTouchEventArgs e)
        {
            Label.Content = "";
        }

        void arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
        {
            try
            {
                if (!e.Message.StartsWith("A"))
                    return;
                var data = e.Message.Split(',');

                if (data.Length != 9) return;

                try
                {
                    _accelerometer.Update(
                   Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                   Convert.ToDouble(data[8], CultureInfo.InvariantCulture));
                }
                catch (Exception)
                {
                    return;
                }
               
                var result = _dtwClassifier.ComputeLabelAndCosts(_accelerometer.RawValues.RawData);
                _lastDetection = result.Item1;

                var computedLabel = _classifier.ComputeValue(_accelerometer.RawValues.RawData);
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
            }
            catch (Exception)
            {
                Console.WriteLine("");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listGesture.Items.Add(cbGestureList.Text + " " + _accelerometer);
            _dtwClassifier.AddTemplate(cbGestureList.Text,_accelerometer.RawValues.RawData);

            cbGestureList.SelectedIndex++;
        }

    }
}
