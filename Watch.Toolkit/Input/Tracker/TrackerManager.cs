using System;
using System.Collections.Generic;
using System.Linq;
using Watch.Toolkit.Processing.MachineLearning;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Tracker
{
    public class TrackerManager : IInputManager
    {
        private readonly ClassifierConfiguration _classifierConfiguration;
        private ImuParser ImuParser { get; set; }
        public Imu Imu { get; private set; }
        public TreeClassifier TreeClassifier { get; set; }
        public DtwClassifier DtwClassifier { get; set; }

        public event EventHandler<TrackGestureEventArgs> RawTrackGestureDataUpdated= delegate { };

        public event EventHandler<LabelDetectedEventArgs> TrackGestureRecognized = delegate { };

        public void Simulate(TrackerEvents ev, EventArgs e)
        {
            switch (ev)
            {
                case TrackerEvents.RawTrackGestureDataUpdated:
                    RawTrackGestureDataUpdated(this, (TrackGestureEventArgs) e);
                    break;
                case TrackerEvents.TrackGestureRecognized:
                    TrackGestureRecognized(this, (LabelDetectedEventArgs) e);
                break;
            }
        }

        public TrackerManager(ClassifierConfiguration classifierConfiguration)
        {
            _classifierConfiguration = classifierConfiguration;

            ImuParser = new ImuParser();
            Imu = new Imu();

            ImuParser.AccelerometerDataReceived += _accelerometerParser_AccelerometerDataReceived;
        }

        public void Start()
        {
            TreeClassifier = new TreeClassifier(_classifierConfiguration);
            TreeClassifier.Run(MachineLearningAlgorithm.C45);

            DtwClassifier = new DtwClassifier(_classifierConfiguration);
            DtwClassifier.Run();

            ImuParser.Start();
        }

        private string _dtwLabel;
        private string _treeLabel;
        private int _lastDetectedClassification;
        void _accelerometerParser_AccelerometerDataReceived(object sender, 
            ImuDataReceivedEventArgs e)
        {
            Imu.Update(e.Accelerometer);

            var result = DtwClassifier.ComputeLabelAndCosts(Imu.YawPitchRollValues.RawData);
            _dtwLabel = result.Item1;

            var computedLabel = TreeClassifier.ComputeValue(Imu.YawPitchRollValues.RawData);
            _lastDetectedClassification = computedLabel == -1 ? _lastDetectedClassification : computedLabel;
            _treeLabel = _classifierConfiguration.GetLabel(_lastDetectedClassification);

            RawTrackGestureDataUpdated(this,
                   new TrackGestureEventArgs()
                   {
                       DtwLabel = _dtwLabel,
                       TreeLabel = _treeLabel,
                       ComputedDtwCosts = result.Item2
                   });

            TrackGestureRecognized(this,
                new LabelDetectedEventArgs
                {
                    Detection = _treeLabel == _dtwLabel ? _treeLabel : _classifierConfiguration.Labels.First()
                });
        }

        public void Stop()
        {
            ImuParser.Stop();
        }
    }

    public class TrackGestureEventArgs:EventArgs
    {
        public string DtwLabel { get; set; }
        public string TreeLabel { get; set; }

        public Dictionary<string,double> ComputedDtwCosts { get; set; }

    }
    public class LabelDetectedEventArgs : EventArgs
    {
        public string Detection { get; set; }

    }
}
