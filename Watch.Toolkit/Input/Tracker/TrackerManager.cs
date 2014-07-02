using System;
using System.Collections.Generic;
using System.Globalization;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;
using Watch.Toolkit.Processing.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Tracker
{
    public class TrackerManager:IInputManager
    {
        private readonly Arduino _arduino = new Arduino();
        private readonly DtwRecognizer _dtwRecognizer = new DtwRecognizer();
        private readonly Accelerometer _accelerometer = new Accelerometer();
        private TreeClassifier _classifier;
        private DtwClassifier _dtwClassifier;

        private string _lastDetection = "";
        private double _lastDetectionCost;
        private int _lastDetectedClassification;

        public event EventHandler<AccelerometerDataReceivedEventArgs> AccelerometerDataReceived = delegate { };
        private List<string> _labels; 
        public TrackerManager(List<string> labels)
        {
            _labels = labels;
        }

        public void Start()
        {
            // new List<string> { "Right", "Left Index", "Left Knuckle" }
            _classifier = new TreeClassifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording3.log", 3, _labels);

            _dtwClassifier = new DtwClassifier(
                AppDomain.CurrentDomain.BaseDirectory + "recording3.log", 3, _labels);

            _classifier.Run(MachineLearningAlgorithm.Id3);

            _dtwClassifier.Run();

            _arduino.MessageReceived += _arduino_MessageReceived;
            _arduino.Start();
        }

        public string GetLabel()
        {
            return "";
        }

        void _arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
        {
            if (!e.Message.StartsWith("A"))
                return;
            var data = e.Message.Split(',');

            if (data.Length != 11) return;

            _accelerometer.Update(
                Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[8], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[9], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[10], CultureInfo.InvariantCulture));

            AccelerometerDataReceived(this,new AccelerometerDataReceivedEventArgs(_accelerometer));

            var result = _dtwRecognizer.ComputeClosestLabelAndCosts(_accelerometer.FilteredValues.RawData);
            _lastDetection = result.Item1;
            var computedLabel = _classifier.ComputeValue(_accelerometer.FilteredValues.RawData);
            _lastDetectedClassification = computedLabel == -1 ? _lastDetectedClassification : computedLabel;
        }
        public void Stop()
        {
            _arduino.Stop();
            _arduino.MessageReceived -= _arduino_MessageReceived;
        }
    }
}
