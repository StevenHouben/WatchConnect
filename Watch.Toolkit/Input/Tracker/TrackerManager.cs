using System;
using System.Data;
using System.Globalization;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Input.Recognizers;
using Watch.Toolkit.Sensors;
using Watch.Toolkit.Sensors.MachineLearning;

namespace Watch.Toolkit.Input.Tracker
{
    public class TrackerManager:IInputManager
    {
        private readonly Arduino _arduino = new Arduino();
        private readonly DtwRecognizer _dtwRecognizer = new DtwRecognizer();
        private readonly Accelerometer _accelerometer = new Accelerometer();
        private TreeClassifier _classifier;

        private string _lastDetection = "";
        private double _lastDetectionCost;
        private int _lastDetectedClassification;

        public event EventHandler<AccelerometerDataReceivedEventArgs> AccelerometerDataReceived = delegate { };
        public void Start()
        {
            _arduino.MessageReceived += _arduino_MessageReceived;
            _arduino.Start();

            //_classifier = new TreeClassifier(
            //   AppDomain.CurrentDomain.BaseDirectory + "recording3.log", 3);
            //_classifier.Run(MachineLearningAlgorithm.Id3);

   
        }

        void _arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
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

            AccelerometerDataReceived(this,new AccelerometerDataReceivedEventArgs(_accelerometer));

            var result = _dtwRecognizer.ComputeClosestLabelAndCosts(_accelerometer.RawData);
            _lastDetection = result.Item1;
            var computedLabel = _classifier.ComputeValue(_accelerometer.RawData);
            _lastDetectedClassification = computedLabel == -1 ? _lastDetectedClassification : computedLabel;
        }
        public void Stop()
        {
            _arduino.Stop();
            _arduino.MessageReceived -= _arduino_MessageReceived;
        }
    }
}
