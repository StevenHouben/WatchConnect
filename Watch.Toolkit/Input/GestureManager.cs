using System;
using System.Collections.Generic;
using System.Timers;
using Phidgets;
using Phidgets.Events;
using Watch.Toolkit.Input.Recognizers;
using Watch.Toolkit.Sensors;
using Timer = System.Timers.Timer;

namespace Watch.Toolkit.Input
{
    public class GestureManager:AbstractGestureManager
    {

        private const int InfraRedDistanceTheshold = 300;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();

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

        private readonly Queue<int> _detectedSideGestures = new Queue<int>();
        private readonly Queue<int> _detectedTopGestures = new Queue<int>();

        private readonly Timer _swipeSideTimer = new Timer(500);
        private readonly Timer _swipeTopTimer = new Timer(500);
        private readonly Timer _holdTimer = new Timer(1000);
        private readonly Timer _topRightTimer = new Timer(1000);
        private readonly Timer _topLeftTimer = new Timer(1000);
        private readonly Timer _lightTimer = new Timer(500);
        
        private readonly List<double> _dataRight = new List<double>();
        private readonly List<double> _dataLeft = new List<double>(); 

        private InterfaceKit _kit;
        public override void Start()
        {
            _kit = Hardware.Hardware.InterfaceKit;
            _kit.SensorChange += kit_SensorChange;
            _kit.open(157002);

            _kit.waitForAttachment();

            _topLeftSensor.RangeChanged += _topLeftSensor_RangeChanged;
            _topRightSensor.RangeChanged +=_topRightSensor_RangeChanged;
            _frontSensor.RangeChanged += _frontSensor_RangeChanged;
            _lightSensor.RangeChanged += _lightSensor_RangeChanged;

            var data = new double[2][];
            data[0] = new double[] { 46, 49, 353, 101 };
            data[1] = new double[] { 409, 99, 49, 42 };

            _gestureRecognizer.AddTemplate("left",data[0]);
            _gestureRecognizer.AddTemplate("right", data[1]);
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

            PrintRow(_dataLeft.ToArray());
            PrintRow(_dataRight.ToArray());

            var output = _gestureRecognizer.FindClosestLabel(_dataLeft.ToArray());

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

        void kit_SensorChange(object sender, SensorChangeEventArgs e)
        {
            switch (e.Index)
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

            OnRawDataHandler(new RawSensorDataReceivedEventArgs(_frontSensor,_topLeftSensor,_topRightSensor,_lightSensor));
        }
    
    }
    public class GestureDetectedEventArgs:EventArgs
    {
        public Gesture Gesture{get;set;}

        public GestureDetectedEventArgs(Gesture detectedGesture)
        {
            Gesture = detectedGesture;
        }
    }
    public class RawSensorDataReceivedEventArgs : EventArgs
    {
        public ProximitySensor FrontSensor { get; set; }

        public ProximitySensor TopLeftSensor { get; set; }

        public ProximitySensor TopRightSensor { get; set; }
        public ProximitySensor LightSensor { get; set; }

        public RawSensorDataReceivedEventArgs(ProximitySensor frontSensor, ProximitySensor topLeftSensor, ProximitySensor topRightSensor, ProximitySensor lightSensor)
        {
            FrontSensor = frontSensor;
            TopLeftSensor = topLeftSensor;
            TopRightSensor = topRightSensor;
            LightSensor = lightSensor;

        }
    }

    public enum Gesture
    {
        Glance,
        HoverLeft,
        HoverRight,
        SwipeRight,
        SwipeLeft,
        Cover,
        Neutral
    }
    public static class MyListExtensions
    {
        public static double Mean(this List<double> values)
        {
            return values.Count == 0 ? 0 : values.Mean(0, values.Count);
        }

        public static double Mean(this List<double> values, int start, int end)
        {
            double s = 0;

            for (var i = start; i < end; i++)
            {
                s += values[i];
            }

            return s / (end - start);
        }

        public static double Variance(this List<double> values)
        {
            return values.Variance(values.Mean(), 0, values.Count);
        }

        public static double Variance(this List<double> values, double mean)
        {
            return values.Variance(mean, 0, values.Count);
        }

        public static double Variance(this List<double> values, double mean, int start, int end)
        {
            double variance = 0;

            for (var i = start; i < end; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            var n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static double StandardDeviation(this List<double> values)
        {
            return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
        }

        public static double StandardDeviation(this List<double> values, int start, int end)
        {
            var mean = values.Mean(start, end);
            var variance = values.Variance(mean, start, end);

            return Math.Sqrt(variance);
        }
    }

}
