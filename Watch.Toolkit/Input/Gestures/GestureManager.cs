using System;
using System.Collections.Generic;
using System.Timers;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Gestures
{
    public class GestureManager:AbstractGestureManager
    {
        private const int InfraRedDistanceThreshold = 300;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();

        private readonly ProximitySensor _frontSensor = new InfraredSensor(0, InfraRedDistanceThreshold)
        {
            Name = "front-sensor"
        };

        private readonly ProximitySensor _topLeftSensor = new InfraredSensor(1, InfraRedDistanceThreshold)
        {
            Name = "top-left-sensor"
        };

        private readonly ProximitySensor _topRightSensor = new InfraredSensor(2, InfraRedDistanceThreshold)
        {
            Name = "top-right-sensor"
        };

        private readonly ProximitySensor _lightSensor = new LightSensor(3) {Name = "light-sensor"};

        private Gesture _lastDetectedGesture;

        private readonly Queue<int> _detectedSideGestures = new Queue<int>();

        private readonly EventMonitor _swipeSideTimer = new EventMonitor(500);
        private readonly EventMonitor _swipeTopTimer = new EventMonitor(500);
        private readonly EventMonitor _holdTimer = new EventMonitor(1000);
        private readonly EventMonitor _topRightTimer = new EventMonitor(1000);
        private readonly EventMonitor _topLeftTimer = new EventMonitor(1000);
        private readonly EventMonitor _lightTimer = new EventMonitor(500);
        
        private readonly List<double> _dataRight = new List<double>();
        private readonly List<double> _dataLeft = new List<double>();


        private Arduino _arduino;
        public override void Start()
        {

            _topLeftSensor.RangeChanged += _topLeftSensor_RangeChanged;
            _topRightSensor.RangeChanged +=_topRightSensor_RangeChanged;
            _frontSensor.RangeChanged += _frontSensor_RangeChanged;
            _lightSensor.RangeChanged += _lightSensor_RangeChanged;

            var data = new double[2][];
            data[0] = new double[] { 46, 49, 353, 101 };
            data[1] = new double[] { 409, 99, 49, 42 };

            _gestureRecognizer.AddTemplate("left",data[0]);
            _gestureRecognizer.AddTemplate("right", data[1]);

            _arduino = new Arduino();
            _arduino.MessageReceived += _arduino_MessageReceived;
            _arduino.Start();
        }

        void _arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
        {
            if (e.Message.StartsWith("L"))
            {
                var data = e.Message.Split(',');

                if (data.Length != 2)
                    return;
                _lightSensor.Value = Convert.ToDouble(data[1]);
            }
            else if (e.Message.StartsWith("P"))
            {
                var data = e.Message.Split(',');

                if (data.Length != 3)
                    return;
                _topLeftSensor.Value = Convert.ToDouble(data[1]);
                _topRightSensor.Value = Convert.ToDouble(data[2]);
            }

            OnRawDataHandler(new RawSensorDataReceivedEventArgs(_frontSensor, _topLeftSensor, _topRightSensor, _lightSensor));

        }

        public override void Stop()
        {
            _arduino.Stop();
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

        void _lightSensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            _topLeftTimer.Stop();
            _topRightTimer.Stop();
            _swipeSideTimer.Stop();
            _holdTimer.Stop();

            if (e.InRange && !_lightTimer.Enabled)
            {
                _lightTimer.Elapsed += _lightTimer_Elapsed;
                _lightTimer.Start();
            }
            else if(!e.InRange && _lightTimer.Enabled)
                _lightTimer.Stop();
        }

        void _lightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lastDetectedGesture = Gesture.Cover;
            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            OnCoverHandler(new GestureDetectedEventArgs(Gesture.Cover));
            _lightTimer.Stop();
        }

        void _frontSensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            //_topRightTimer.Stop();
            //_topLeftTimer.Stop();

            if (e.InRange && !_holdTimer.Enabled)
            {
                _holdTimer.Elapsed += _holdTimer_Elapsed;
                _holdTimer.Start();
            }
            if (e.InRange && !_swipeTopTimer.Enabled)
            {
                _swipeTopTimer.Elapsed += _swipeTopTimer_Elapsed;
                _swipeTopTimer.Start();
            }
            if (!e.InRange && _holdTimer.Enabled)
            {
                _holdTimer.Stop();
            }

        }

        void _swipeTopTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _swipeTopTimer.Stop();
            _detectedSideGestures.Clear();
        }

        void _holdTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lastDetectedGesture = Gesture.Glance;
            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            OnGlanceHandler(new GestureDetectedEventArgs(Gesture.Glance));
            _holdTimer.Stop();
        }

        void _topRightSensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
           _holdTimer.Stop();
           _topLeftTimer.Stop();

            if (e.InRange && !_topRightTimer.Enabled)
            {
                _topRightTimer.Elapsed += _topRightTimer_Elapsed;
                _topRightTimer.Start();
            }
            if (!e.InRange && _topRightTimer.Enabled)
            {
                _topRightTimer.Stop();
            }

            if (_detectedSideGestures.Count == 0)
            {
                _swipeSideTimer.Elapsed += _timer_Elapsed;
                _swipeSideTimer.Start();
            }
            
            _dataRight.Add(_topRightSensor.Value);
            _dataLeft.Add(_topLeftSensor.Value);
            _detectedSideGestures.Enqueue(e.InRange ? 3 : 4);
            CheckDetection();
        }
        void _topRightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lastDetectedGesture = Gesture.HoverRight;
            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            OnHoverRightHandler(new GestureDetectedEventArgs(Gesture.HoverRight));
            _topRightTimer.Stop();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _swipeSideTimer.Stop();
            _detectedSideGestures.Clear();
        }

        void _topLeftSensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
                _topRightTimer.Stop();
                _holdTimer.Stop();

            if (e.InRange && !_topLeftTimer.Enabled)
            {
                _topLeftTimer.Elapsed += _topLeftTimer_Elapsed;
                _topLeftTimer.Start();
            }
            if (!e.InRange && _topLeftTimer.Enabled)
            {
                _topLeftTimer.Stop();
            }

            if (_detectedSideGestures.Count == 0)
            {
                _swipeSideTimer.Elapsed += _timer_Elapsed;
                _swipeSideTimer.Start();
            }
            _detectedSideGestures.Enqueue(e.InRange ? 1 : 2);
            _dataRight.Add(_topRightSensor.Value);
            _dataLeft.Add(_topLeftSensor.Value);
            CheckDetection();
        }

        void _topLeftTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lastDetectedGesture = Gesture.HoverLeft;
            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            OnHoverLeftHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            _topRightTimer.Stop();
        }

        static void PrintRow(double[] row)
        {
            foreach (var t in row)
                Console.Write(t + @",");
            Console.WriteLine("");
        }
        void CheckDetection()
        {
            if (_detectedSideGestures.Count != 4) return;
            _swipeSideTimer.Stop();

            //PrintRow(_dataLeft.ToArray());
            //PrintRow(_dataRight.ToArray());

            var output = _gestureRecognizer.ComputeClosestLabel(_dataLeft.ToArray());

            _lastDetectedGesture = output == "right" ? Gesture.SwipeRight : Gesture.SwipeLeft;

            _detectedSideGestures.Clear();

            _dataLeft.Clear();
            _dataRight.Clear();

            //var n = _detectedGestures.Dequeue() + _detectedGestures.Dequeue();
            //var m = _detectedGestures.Dequeue() + _detectedGestures.Dequeue();

            //Console.WriteLine(n < m ? _lastDetectedGesture = Gesture.SwipeRight : _lastDetectedGesture = Gesture.SwipeLeft);

            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            
            if(_lastDetectedGesture == Gesture.SwipeLeft)
                OnSwipeLeftHandler(new GestureDetectedEventArgs(Gesture.SwipeLeft));
            else
                OnSwipeRightHandler(new GestureDetectedEventArgs(Gesture.SwipeRight));
        }
    }
}
