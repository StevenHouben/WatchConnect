using System;
using System.Collections.Generic;
using System.Timers;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Processing.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Gestures
{
    public class GestureManager:AbstractGestureManager
    {
        private const int InfraRedDistanceThreshold = 400;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();

        public InfraredSensor FrontProximitySensor { get; private set; }
        public InfraredSensor LeftProximitySensor { get; private set; }
        public InfraredSensor RightProximitySensor { get; private set; }
        public LightSensor LightSensor { get; set; }

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

        public HardwarePlatform Hardware { get; private set;}

        public GestureManager(HardwarePlatform hardware)
        {
            Hardware = hardware;
        }

        public override void Start()
        {
            FrontProximitySensor = new InfraredSensor(0, InfraRedDistanceThreshold);
            LeftProximitySensor = new InfraredSensor(1, InfraRedDistanceThreshold);
            RightProximitySensor = new InfraredSensor(2, InfraRedDistanceThreshold);
            LightSensor = new LightSensor(3);

            LeftProximitySensor.RangeChanged += _topLeftSensor_RangeChanged;
            RightProximitySensor.RangeChanged +=_topRightSensor_RangeChanged;
            FrontProximitySensor.RangeChanged += _frontSensor_RangeChanged;
            LightSensor.RangeChanged += _lightSensor_RangeChanged;

            var data = new double[2][];
            data[0] = new double[] { 46, 49, 353, 101 };
            data[1] = new double[] { 409, 99, 49, 42 };

            _gestureRecognizer.AddTemplate("left",data[0]);
            _gestureRecognizer.AddTemplate("right", data[1]);

            Hardware.DataPacketReceived += _arduino_DataPacketReceived;

            Hardware.AddPacketListener("Light", 
                ( message) =>
                {
                    if (message.StartsWith("L"))
                        return message.Split(',').Length == 2;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));

            Hardware.AddPacketListener("Proximity",
                (message) =>
                {
                    if (message.StartsWith("P"))
                        return message.Split(',').Length == 3;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));

            Hardware.Start();
        }

        void _arduino_DataPacketReceived(object sender, DataPacketReceivedEventArgs e)
        {
            switch (e.DataPacket.Header)
            {
                case "L":
                    LightSensor.Value = Convert.ToDouble(e.DataPacket.Body[0]);
                    break;
                case "P":
                    LeftProximitySensor.Value = Convert.ToDouble(e.DataPacket.Body[0]);
                    RightProximitySensor.Value = Convert.ToDouble(e.DataPacket.Body[1]);
                    break;
            }
            OnRawDataHandler(new RawSensorDataReceivedEventArgs(FrontProximitySensor, LeftProximitySensor, RightProximitySensor, LightSensor));
        }

        public override void Stop()
        {
            Hardware.Stop();
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
            
            _dataRight.Add(RightProximitySensor.Value);
            _dataLeft.Add(LeftProximitySensor.Value);
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
            _dataRight.Add(RightProximitySensor.Value);
            _dataLeft.Add(LeftProximitySensor.Value);
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
            if (_detectedSideGestures.Count != 4) return;
            _swipeSideTimer.Stop();

            var output = _gestureRecognizer.ComputeClosestLabel(_dataLeft.ToArray());

            _lastDetectedGesture = output == "right" ? Gesture.SwipeRight : Gesture.SwipeLeft;

            _detectedSideGestures.Clear();

            _dataLeft.Clear();
            _dataRight.Clear();

            OnGestureHandler(new GestureDetectedEventArgs(_lastDetectedGesture));
            
            if(_lastDetectedGesture == Gesture.SwipeLeft)
                OnSwipeLeftHandler(new GestureDetectedEventArgs(Gesture.SwipeLeft));
            else
                OnSwipeRightHandler(new GestureDetectedEventArgs(Gesture.SwipeRight));
        }
    }
}
