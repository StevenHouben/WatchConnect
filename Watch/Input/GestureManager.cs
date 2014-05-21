using System;
using System.Collections.Generic;
using System.Timers;
using Phidgets;
using Phidgets.Events;
using Watch.Input.Sensors;

namespace Watch.Input
{
    public class GestureManager
    {
        public event EventHandler<GestureDetectedEventArgs> GestureDetected;
        public event EventHandler<RawDataReceivedEventArgs> RawDataReceived;

        public event EventHandler<GestureDetectedEventArgs> SwipeLeft;
        public event EventHandler<GestureDetectedEventArgs> SwipeRight;
        public event EventHandler<GestureDetectedEventArgs> HoverLeft;
        public event EventHandler<GestureDetectedEventArgs> HoverRight;
        public event EventHandler<GestureDetectedEventArgs> Glance;
        public event EventHandler<GestureDetectedEventArgs> Cover;

        InterfaceKit _kit;

        private const int InfraRedDistanceTheshold = 300;

        readonly Sensor _frontSensor = new InfraredSensor(0,InfraRedDistanceTheshold) { Name = "front-sensor" };
        readonly Sensor _topLeftSensor = new InfraredSensor(1,InfraRedDistanceTheshold) { Name = "top-left-sensor" };
        readonly Sensor _topRightSensor = new InfraredSensor(2,InfraRedDistanceTheshold) { Name = "top-right-sensor" };
        readonly Sensor _lightSensor = new LightSensor(3) { Name = "light-sensor" };

        Gesture _lastDetectedGesture;


        readonly Queue<int> _detectedGestures = new Queue<int>();

        private readonly Timer _swipeTimer = new Timer(500);
        private readonly Timer _holdTimer = new Timer(1000);
        private readonly Timer _topRightTimer = new Timer(1000);
        private readonly Timer _topLeftTimer = new Timer(1000);
        private readonly Timer _lightTimer = new Timer(500);

        public void Start()
        {
            _kit = new InterfaceKit();
            _kit.SensorChange += kit_SensorChange;
            _kit.open();

            _kit.waitForAttachment();

            _topLeftSensor.RangeChanged += _topLeftSensor_RangeChanged;
            _topRightSensor.RangeChanged +=_topRightSensor_RangeChanged;
            _frontSensor.RangeChanged += _frontSensor_RangeChanged;
            _lightSensor.RangeChanged += _lightSensor_RangeChanged;
        }

        void _lightSensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            _topLeftTimer.Stop();
            _topRightTimer.Stop();
            _swipeTimer.Stop();
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
            _topRightTimer.Stop();
            _topLeftTimer.Stop();

            if (e.InRange && !_holdTimer.Enabled)
            {
                _holdTimer.Elapsed += _holdTimer_Elapsed;
                _holdTimer.Start();
            }
            else if (!e.InRange && _holdTimer.Enabled)
            {
                _holdTimer.Stop();
            }
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
            else if (!e.InRange && _topRightTimer.Enabled)
            {
                _topRightTimer.Stop();
            }

            if (_detectedGestures.Count == 0)
            {
                _swipeTimer.Elapsed += _timer_Elapsed;
                _swipeTimer.Start();
            }
            _detectedGestures.Enqueue(e.InRange ? 3 : 4);
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
            _swipeTimer.Stop();
            _detectedGestures.Clear();
        }

        void _topLeftSensor_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            if (_topRightTimer.Enabled)
                _topRightTimer.Stop();

            if (_holdTimer.Enabled)
                _holdTimer.Stop();

            if (e.InRange && !_topLeftTimer.Enabled)
            {
                _topLeftTimer.Elapsed += _topLeftTimer_Elapsed;
                _topLeftTimer.Start();
            }
            else if (!e.InRange && _topLeftTimer.Enabled)
            {
                _topLeftTimer.Stop();
            }

            if (_detectedGestures.Count == 0)
            {
                _swipeTimer.Elapsed += _timer_Elapsed;
                _swipeTimer.Start();
            }
            _detectedGestures.Enqueue(e.InRange ? 1 : 2);
            CheckDetection();
        }

        void _topLeftTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lastDetectedGesture = Gesture.HoverLeft;
            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            OnHoverLeftHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            _topRightTimer.Stop();
        }

        void CheckDetection()
        {
            if (_detectedGestures.Count != 4) return;
            _swipeTimer.Stop();
            var n=_detectedGestures.Dequeue()+_detectedGestures.Dequeue();
            var m = _detectedGestures.Dequeue() + _detectedGestures.Dequeue();

            Console.WriteLine(n < m ? _lastDetectedGesture = Gesture.SwipeRight: _lastDetectedGesture = Gesture.SwipeLeft);
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

            OnRawDataHandler(new RawDataReceivedEventArgs(_frontSensor,_topLeftSensor,_topRightSensor,_lightSensor));
        }
      
        protected void OnGestureHandler(GestureDetectedEventArgs ge)
        {
            if (GestureDetected != null)
                GestureDetected(this, ge);
        }

        protected void OnSwipeLeftHandler(GestureDetectedEventArgs ge)
        {
            if (SwipeLeft != null)
                SwipeLeft(this, ge);
        }
        protected void OnSwipeRightHandler(GestureDetectedEventArgs ge)
        {
            if (SwipeRight != null)
                SwipeRight(this, ge);
        }
        protected void OnHoverLeftHandler(GestureDetectedEventArgs ge)
        {
            if (HoverLeft != null)
                HoverLeft(this, ge);
        }
        protected void OnHoverRightHandler(GestureDetectedEventArgs ge)
        {
            if (HoverRight != null)
                HoverRight(this, ge);
        }
        protected void OnCoverHandler(GestureDetectedEventArgs ge)
        {
            if (Cover != null)
                Cover(this, ge);
        }
        protected void OnGlanceHandler(GestureDetectedEventArgs ge)
        {
            if (Glance != null)
                Glance(this, ge);
        }

        protected void OnRawDataHandler(RawDataReceivedEventArgs e)
        {
            if (RawDataReceived != null)
                RawDataReceived(this, e);
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
    public class RawDataReceivedEventArgs : EventArgs
    {
        public Sensor FrontSensor { get; set; }

        public Sensor TopLeftSensor { get; set; }

        public Sensor TopRightSensor { get; set; }
        public Sensor LightSensor { get; set; }

        public RawDataReceivedEventArgs(Sensor frontSensor, Sensor topLeftSensor, Sensor topRightSensor, Sensor lightSensor)
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
