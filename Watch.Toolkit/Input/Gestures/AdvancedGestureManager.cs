using System;
using System.Collections.Generic;
using System.Text;
using Watch.Toolkit.Hardware.Phidget;
using Watch.Toolkit.Processing.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Gestures
{
    public class AdvancedGestureManager:AbstractGestureManager
    {
       private const int InfraRedDistanceTheshold = 300;

        private readonly ProximitySensor _frontSensor = new InfraredSensor(0, InfraRedDistanceTheshold)
        {
            Name = "front-sensor"
        };

        private readonly ProximitySensor _topLeftSensor = new InfraredSensor(1, InfraRedDistanceTheshold)
        {
            Name = "top-left-sensor"
        };

        private readonly ProximitySensor _topRightSensor = new InfraredSensor(2, InfraRedDistanceTheshold)
        {
            Name = "top-right-sensor"
        };

        private readonly ProximitySensor _lightSensor = new LightSensor(3) {Name = "light-sensor"};

        private Gesture _lastDetectedGesture;     
 
        private readonly List<double> _dataRightSensor = new List<double>();
        private readonly List<double> _dataLeftSensor = new List<double>();
        private readonly List<double> _dataFrontSensor = new List<double>();
        private readonly List<double> _dataLightSensor = new List<double>();

        private bool _recording;
        private int _frameSize = 6;
        private int _counter = 0;

        private readonly DtwRecognizer _leftMatcher = new DtwRecognizer();
        private readonly DtwRecognizer _rightMatcher = new DtwRecognizer();
        private readonly DtwRecognizer _frontMatcher = new DtwRecognizer();
        private readonly DtwRecognizer _lightMatcher = new DtwRecognizer();

        private readonly DtwRecognizer _vecMatcher = new DtwRecognizer();

        private Phidget _manager;

        public override void Start()
        {
            _manager = new Phidget();
            _manager.Start(157002);

            _manager.AnalogDataReceived += _manager_AnalogDataReceived;

            _topLeftSensor.RangeChanged += _sensor_RangeChanged;
            _topRightSensor.RangeChanged += _sensor_RangeChanged;
            _frontSensor.RangeChanged += _sensor_RangeChanged;
            _lightSensor.RangeChanged += _sensor_RangeChanged;

            _vecMatcher.AddTemplate("leftToRight", TemplateData.LeftToRight.Vector);
            _vecMatcher.AddTemplate("rightToLeft", TemplateData.RightToLeft.Vector);
            //_vecMatcher.AddTemplate("topToBottom", TemplateData.TopToBottom.Vector);
            //_vecMatcher.AddTemplate("bottomToTop", TemplateData.BottomToTop.Vector);

            _leftMatcher.AddTemplate("leftToRight",TemplateData.LeftToRight.Left);
            _leftMatcher.AddTemplate("rightToLeft", TemplateData.RightToLeft.Left);
            _leftMatcher.AddTemplate("topToBottom", TemplateData.TopToBottom.Left);
            _leftMatcher.AddTemplate("bottomToTop", TemplateData.BottomToTop.Left);

            _rightMatcher.AddTemplate("leftToRight", TemplateData.LeftToRight.Right);
            _rightMatcher.AddTemplate("rightToLeft", TemplateData.RightToLeft.Right);
            _rightMatcher.AddTemplate("topToBottom", TemplateData.TopToBottom.Right);
            _rightMatcher.AddTemplate("bottomToTop", TemplateData.BottomToTop.Right);

            _frontMatcher.AddTemplate("leftToRight", TemplateData.LeftToRight.Front);
            _frontMatcher.AddTemplate("rightToLeft", TemplateData.RightToLeft.Front);
            _frontMatcher.AddTemplate("topToBottom", TemplateData.TopToBottom.Front);
            _frontMatcher.AddTemplate("bottomToTop", TemplateData.BottomToTop.Front);

            _lightMatcher.AddTemplate("leftToRight", TemplateData.LeftToRight.Light);
            _lightMatcher.AddTemplate("rightToLeft", TemplateData.RightToLeft.Light);
            _lightMatcher.AddTemplate("topToBottom", TemplateData.TopToBottom.Light);
            _lightMatcher.AddTemplate("bottomToTop", TemplateData.BottomToTop.Light);
        }

        public override void Stop()
        {
            if(_manager.IsRunning)
                _manager.Stop();
        }

        void _manager_AnalogDataReceived(object sender, Hardware.AnalogDataReceivedEventArgs e)
        {
            switch (e.Id)
            {
                case 0:
                    _frontSensor.Value = e.Value;
                    break;
                case 1:
                    _topLeftSensor.Value = e.Value;
                    break;
                case 2:
                    _lightSensor.Value = e.Value;
                    break;
                case 3:
                    _topRightSensor.Value = e.Value;
                    break;
            }

            OnRawDataHandler(new RawSensorDataReceivedEventArgs(_frontSensor, _topLeftSensor, _topRightSensor, _lightSensor));
        }

        void _sensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            if (!_recording)
            {
                _counter = 0;
                _recording = !_recording;
            }

            if (_recording && _counter < _frameSize)
            {
                _dataFrontSensor.Add(_frontSensor.Value);
                _dataLeftSensor.Add(_topLeftSensor.Value);
                _dataRightSensor.Add(_topRightSensor.Value);
                _dataLightSensor.Add(_lightSensor.Value);
                _counter++;
            }
            if(_recording && _counter==_frameSize)
            {
                if (_dataFrontSensor.Count > 0)
                {
                    var recordedTemplate = new Template("", 
                        _dataFrontSensor.ToArray(), 
                        _dataLeftSensor.ToArray(), 
                        _dataRightSensor.ToArray(),
                        _dataLightSensor.ToArray());

                    var sb = new StringBuilder();

                    foreach (var item in _dataFrontSensor)
                        sb.Append(item + @","); // Replace this with your version of printing
                    sb.Append("\n");

                    foreach (var item in _dataLeftSensor)
                        sb.Append(item + @","); // Replace this with your version of printing
                    sb.Append("\n");

                    foreach (var item in _dataRightSensor)
                        sb.Append(item + @","); // Replace this with your version of printing
                    sb.Append("\n");

                    foreach (var item in _dataLightSensor)
                        sb.Append(item + @","); // Replace this with your version of printing
                    sb.Append("\n");
                    sb.Append("\n");

                    Helper.WriteToFile(sb,"left-to-right.txt");
                    sb.Clear();
                    
                    var output = _vecMatcher.ComputerClosestLabelAndCost(recordedTemplate.Vector);

                    //var li = new List<string>();
                    //li.Add(_leftMatcher.FindClosestLabel(_dataLeftSensor.ToArray()));
                    //li.Add(_rightMatcher.FindClosestLabel(_dataLightSensor.ToArray()));
                    //li.Add(_frontMatcher.FindClosestLabel(_dataFrontSensor.ToArray()));
                    //li.Add(_lightMatcher.FindClosestLabel(_dataLightSensor.ToArray()));

                    //Console.WriteLine(li.GroupBy(v => v)
                    //.OrderByDescending(g => g.Count())
                    //.First()
                    //.Key);
                    //PrintCollection(_dataFrontSensor);
                    //PrintCollection(_dataLeftSensor);
                    //PrintCollection(_dataRightSensor);
                    //PrintCollection(_dataLightSensor);

                    _dataFrontSensor.Clear();
                    _dataLeftSensor.Clear();
                    _dataLightSensor.Clear();
                    _dataRightSensor.Clear();
                }
                _counter = 0;
            }
        }

        public void PrintCollection<T>(IEnumerable<T> col)
        {
            foreach (var item in col)
                Console.Write(item + @","); // Replace this with your version of printing
            Console.WriteLine("");
        }
    }
}
