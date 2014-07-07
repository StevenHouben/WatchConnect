using System;
using System.Collections.Generic;
using Watch.Toolkit.Processing.MachineLearning;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Tracker
{
    public class TrackerManager : IInputManager
    {
        private readonly ClassifierConfiguration _classifierConfiguration;
        private readonly AccelerometerParser _accelerometerParser = new AccelerometerParser();

        public Accelerometer Accelerometer { get; private set; }
        public TreeClassifier TreeClassifier { get; set; }
        public DtwClassifier DtwClassifier { get; set; }

        public event EventHandler<TrackGestureEventArgs> TrackGestureRecognized = delegate { }; 

        public TrackerManager(ClassifierConfiguration classifierConfiguration)
        {
            _classifierConfiguration = classifierConfiguration;

            Accelerometer = new Accelerometer();

            _accelerometerParser.AccelerometerDataReceived += _accelerometerParser_AccelerometerDataReceived;
        }

        public void Start()
        {
            TreeClassifier = new TreeClassifier(_classifierConfiguration);
            TreeClassifier.Run(MachineLearningAlgorithm.C45);

            DtwClassifier = new DtwClassifier(_classifierConfiguration);
            DtwClassifier.Run();

            _accelerometerParser.Start();
        }

        private string _dtwLabel;
        private string _treeLabel;
        private int _lastDetectedClassification;
        void _accelerometerParser_AccelerometerDataReceived(object sender, 
            AccelerometerDataReceivedEventArgs e)
        {
            Accelerometer.Update(e.Accelerometer);

            var result = DtwClassifier.ComputeLabelAndCosts(Accelerometer.YawPitchRollValues.RawData);
            _dtwLabel = result.Item1;

            var computedLabel = TreeClassifier.ComputeValue(Accelerometer.YawPitchRollValues.RawData);
            _lastDetectedClassification = computedLabel == -1 ? _lastDetectedClassification : computedLabel;
            _treeLabel = _classifierConfiguration.GetLabel(_lastDetectedClassification);

            //if (_dtwLabel == _treeLabel)
                TrackGestureRecognized(this,
                    new TrackGestureEventArgs()
                    {
                        DtwLabel = _dtwLabel,
                        TreeLabel = _treeLabel,
                        ComputedDtwCosts = result.Item2
                    });
        }

        public void Stop()
        {
            _accelerometerParser.Stop();
        }
    }

    public class TrackGestureEventArgs
    {
        public string DtwLabel { get; set; }
        public string TreeLabel { get; set; }

        public Dictionary<string,double> ComputedDtwCosts { get; set; }

    }
}
