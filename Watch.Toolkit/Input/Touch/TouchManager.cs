using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Processing.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Touch
{
    public class TouchManager:IInputManager
    {
        public event EventHandler<RawTouchDataReceivedEventArgs> RawDataReceived;

        public event EventHandler<SliderTouchEventArgs> SliderTouchDown;
        public event EventHandler<SliderTouchEventArgs> SliderTouchUp;
        public event EventHandler<SliderTouchEventArgs> SlideDown;
        public event EventHandler<SliderTouchEventArgs> SlideUp;
        public event EventHandler<SliderTouchEventArgs> SliderDoubleTap;

        public event EventHandler<BevelTouchEventArgs> BevelDown;
        public event EventHandler<BevelTouchEventArgs> BevelUp;
        public event EventHandler<BevelTouchEventArgs> BevelDoubleTap;
        public event EventHandler<MultiBevelTouchEventArgs> BevelGrab;

        private readonly Queue<double> _pipeline = new Queue<double>();
        private bool _recording;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();
        private readonly EventMonitor _wristBandDoubleTapTimer = new EventMonitor(300);

        private readonly ConcurrentDictionary<BevelSide, DataEventMonitor<BevelSide>> _bevelDoubleTapTimers
            = new ConcurrentDictionary<BevelSide, DataEventMonitor<BevelSide>>();

        public HardwarePlatform Hardware { get; private set; }

        public BevelTouchSensor BevelTouchSensor{ get; private set; }
        public TouchSensor SlideSensor { get; private set; }

        public void Simulate(TouchEvents te, EventArgs e)
        {
            switch (te)
            {
                case TouchEvents.BevelDoubleTap:
                    OnBevelDoubleTapHandler((BevelTouchEventArgs)e);
                    break;
                case TouchEvents.BevelDown:
                    OnBevelTouchDownHandler((BevelTouchEventArgs)e);
                    break;
                case TouchEvents.BevelUp:
                    OnBevelTouchUpHandler((BevelTouchEventArgs)e);
                    break;
                case TouchEvents.BevelGrab:
                    OnBevelGrabHandler((MultiBevelTouchEventArgs) e);
                    break;
                case TouchEvents.SliderDoubleTap:
                    OnSliderDoubleTap((SliderTouchEventArgs)e);
                    break;
                case TouchEvents.SlideDown:
                    OnSlideDownHandler((SliderTouchEventArgs)e);
                    break;
                case TouchEvents.SlideUp:
                    OnSlideDownHandler((SliderTouchEventArgs)e);
                    break;
                case TouchEvents.SliderTouchDown:
                    OnSlideDownHandler((SliderTouchEventArgs)e);
                    break;
                case TouchEvents.SliderTouchUp:
                    OnSlideDownHandler((SliderTouchEventArgs)e);
                    break;
            }
        }

        public TouchManager(HardwarePlatform hardware)
        {
            Hardware = hardware;
        }

        public void Start()
        {
            SlideSensor =new LinearTouchSensor();
            BevelTouchSensor = new BevelTouchSensor();

            _gestureRecognizer.AddTemplate(
                "Up",
                new double[] { 173, 275, 409, 567, 732, 897, 1000 });

            _gestureRecognizer.AddTemplate(
                "Down",
                new double[] { 1000, 857, 723, 576, 276, 289, 120 });

            Hardware.AddPacketListener("Capacitive Touch",
                (message) =>
                {
                    if (message.StartsWith("T"))
                        return message.Split(',').Length == 9;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));

            Hardware.AddPacketListener("Slider Touch",
                (message) =>
                {
                    if (message.StartsWith("S"))
                        return message.Split(',').Length == 2;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));

            Hardware.DataPacketReceived += _arduino_DataPacketReceived;
            Hardware.Start();
        }

        void _arduino_DataPacketReceived(object sender, DataPacketReceivedEventArgs e)
        {
            switch (e.DataPacket.Header)
            {
                case "T":
                     var state = new BevelState()
                    {
                        BevelTop = e.DataPacket.Body[4] == "1",
                        BevelLeft = e.DataPacket.Body[5] == "1",
                        BevelBottom = e.DataPacket.Body[6] == "1",
                        BevelRight = e.DataPacket.Body[7] == "1"
                    };

                     HandleTouchStates(BevelSide.Top, state.BevelTop, BevelTouchSensor.TouchStates.BevelTop);
                     HandleTouchStates(BevelSide.Left, state.BevelLeft, BevelTouchSensor.TouchStates.BevelLeft);
                     HandleTouchStates(BevelSide.Right, state.BevelRight, BevelTouchSensor.TouchStates.BevelRight);
                     HandleTouchStates(BevelSide.Bottom, state.BevelBottom, BevelTouchSensor.TouchStates.BevelBottom);
                    OnBevelGrabHandler(new MultiBevelTouchEventArgs(state));

                    BevelTouchSensor.TouchStates = state;
                    break;
                case "S":
                    SlideSensor.Value = Convert.ToDouble(e.DataPacket.Body[0]);

                     if (_recording)
                        _pipeline.Enqueue(SlideSensor.Value);
                     if (SlideSensor.Down)
                     {
                         OnRawDataHandler(new RawTouchDataReceivedEventArgs(SlideSensor));
                         OnSliderTouchDownHandler(new SliderTouchEventArgs(SlideSensor, SlideSensor.Value));
                         _recording = true;
                         if (!_wristBandDoubleTapTimer.Enabled)
                         {
                             _wristBandDoubleTapTimer.Elapsed += _doubleTapTimer_Elapsed;
                             _wristBandDoubleTapTimer.Start();
                             _wristBandDoubleTapTimer.Trigger = true;
                         }
                     }
                     else
                     {
                         OnSliderTouchUpHandler(new SliderTouchEventArgs(SlideSensor, SlideSensor.Value));
                         if (_wristBandDoubleTapTimer.Trigger && _wristBandDoubleTapTimer.Enabled)
                         {
                             _wristBandDoubleTapTimer.Trigger = false;
                             OnSliderDoubleTap(new SliderTouchEventArgs(SlideSensor, SlideSensor.Value));
                         }
                         else if (_recording)
                         {
                             _recording = false;
                             AnalyseData();
                         }
                     }
                    break;
            }
        }


        public void Stop()
        {
            if (Hardware == null) return;
            if (Hardware.IsRunning)
                Hardware.Stop();
        }

        void HandleTouchStates(BevelSide side, bool stateNow, bool stateOld)
        {
            if (stateNow == stateOld) 
                return;
            if (stateNow)
            {
                OnBevelTouchDownHandler(new BevelTouchEventArgs(side, 1));
                if (_bevelDoubleTapTimers.ContainsKey(side))
                {
                    if (!_bevelDoubleTapTimers[side].Trigger || !_bevelDoubleTapTimers[side].Enabled) return;
                    OnBevelDoubleTapHandler(new BevelTouchEventArgs(side,2));
                }
                else
                {
                    var em = new DataEventMonitor<BevelSide>(300, (int)side, side);
                    em.MonitorTriggered += em_MonitorTriggered;
                    em.Trigger = true;
                    em.Start();
                    _bevelDoubleTapTimers.TryAdd(side, em);   
                }
            }
            else
            {
                OnBevelTouchUpHandler(new BevelTouchEventArgs(side, 0));
            }

        }

        void em_MonitorTriggered(object sender, DataTriggeredEventArgs<BevelSide> e)
        {
             _bevelDoubleTapTimers[e.Data].Stop();
            DataEventMonitor<BevelSide> dummyOut;
            _bevelDoubleTapTimers.TryRemove(e.Data, out dummyOut);
        }

        protected void OnSliderDoubleTap(SliderTouchEventArgs e)
        {
            if(SliderDoubleTap != null)
                SliderDoubleTap(this,e);
        }
        protected void OnBevelTouchDownHandler(BevelTouchEventArgs e)
        {
            if (BevelDown != null)
                BevelDown(this, e);
        }

        protected void OnBevelTouchUpHandler(BevelTouchEventArgs e)
        {
            if (BevelUp != null)
                BevelUp(this, e);
        }
        protected void OnBevelDoubleTapHandler(BevelTouchEventArgs e)
        {
            if (BevelDoubleTap != null)
                BevelDoubleTap(this, e);
        }

        protected void OnRawDataHandler(RawTouchDataReceivedEventArgs e)
        {
            if (RawDataReceived != null)
                RawDataReceived(this, e);
        }

        protected void OnSliderTouchDownHandler(SliderTouchEventArgs e)
        {
            if (SliderTouchDown != null)
                SliderTouchDown(this, e);
        }
        protected void OnSliderTouchUpHandler(SliderTouchEventArgs e)
        {
            if (SliderTouchUp != null)
                SliderTouchUp(this, e);
        }
        protected void OnSlideDownHandler(SliderTouchEventArgs e)
        {
            if (SlideDown != null)
                SlideDown(this, e);
        }
        protected void OnSlideUpHandler(SliderTouchEventArgs e)
        {
            if (SlideUp != null)
                SlideUp(this, e);
        }

        protected void OnBevelGrabHandler(MultiBevelTouchEventArgs e)
        {
            if (BevelGrab != null)
                BevelGrab(this, e);
        }
        void _doubleTapTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _wristBandDoubleTapTimer.Stop();
        }
        private void AnalyseData()
        {
            if (_pipeline.ToArray().Count() <= 1) return;
            var output = _gestureRecognizer.ComputeClosestLabel(_pipeline.ToArray());

            if (output == "Up")
                OnSlideUpHandler(new SliderTouchEventArgs(SlideSensor, -1));
            else
                OnSlideDownHandler(new SliderTouchEventArgs(SlideSensor, -1));
            _pipeline.Clear();
        }
    }
}
