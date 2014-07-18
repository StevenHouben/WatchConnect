using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Touch
{
    public class TouchManager:IInputManager
    {
        public const int Id = 28157;
        public event EventHandler<RawTouchDataReceivedEventArgs> RawDataReceived;

        public event EventHandler<SliderTouchEventArgs> SliderTouchDown;
        public event EventHandler<SliderTouchEventArgs> SliderTouchUp;
        public event EventHandler<SliderTouchEventArgs> SlideDown;
        public event EventHandler<SliderTouchEventArgs> SlideUp;
        public event EventHandler<SliderTouchEventArgs> DoubleTap;

        public event EventHandler<BevelTouchEventArgs> BevelDown;
        public event EventHandler<BevelTouchEventArgs> BevelUp;
        public event EventHandler<BevelTouchEventArgs> BevelDoubleTap;  //Todo: implement

        public event EventHandler<MultiBevelTouchEventArgs> BevelGrab;

        readonly TouchSensor _linearTouch = new LinearTouchSensor();

        private readonly Queue<double> _pipeline = new Queue<double>();
        private bool _recording;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();

        private readonly EventMonitor _wristBandDoubleTapTimer = new EventMonitor(300);

        private readonly Dictionary<BevelSide, DataEventMonitor<BevelSide>> _bevelDoubleTapTimers
            = new Dictionary<BevelSide, DataEventMonitor<BevelSide>>(); 


        private Arduino _arduino;

        private BevelState _bevelState = new BevelState();

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
                case TouchEvents.DoubleTap:
                    OnDoubleTap((SliderTouchEventArgs)e);
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

        public void Start()
        {
            _gestureRecognizer.AddTemplate(
                "Up",
                new double[] { 173, 275, 409, 567, 732, 897, 1000 });

            _gestureRecognizer.AddTemplate(
                "Down",
                new double[] { 1000, 857, 723, 576, 276, 289, 120 });


            _arduino = new Arduino();

            _arduino.AddPacketListener("Capacitive Touch",
                (message) =>
                {
                    if (message.StartsWith("T"))
                        return message.Split(',').Length == 9;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));

            _arduino.AddPacketListener("Slider Touch",
                (message) =>
                {
                    if (message.StartsWith("S"))
                        return message.Split(',').Length == 2;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));

            _arduino.DataPacketReceived += _arduino_DataPacketReceived;

            _arduino.Start();
        }

        void _arduino_DataPacketReceived(object sender, Hardware.DataPacketReceivedEventArgs e)
        {
            switch (e.DataPacket.Header)
            {
                case "T":
                     var state = new BevelState()
                    {
                        BevelTop = e.DataPacket.Body[1] == "1",
                        BevelLeft = e.DataPacket.Body[2] == "1",
                        BevelBottom = e.DataPacket.Body[3] == "1",
                        BevelRight = e.DataPacket.Body[4] == "1"
                    };

                    HandleTouchStates(BevelSide.TopSide, state.BevelTop, _bevelState.BevelTop);
                    HandleTouchStates(BevelSide.LeftSide, state.BevelLeft, _bevelState.BevelLeft);
                    HandleTouchStates(BevelSide.RightSide, state.BevelRight, _bevelState.BevelRight);
                    HandleTouchStates(BevelSide.BottomSide, state.BevelBottom, _bevelState.BevelBottom);
                    OnBevelGrabHandler(new MultiBevelTouchEventArgs(state));

                    _bevelState = state;
                    break;
            }
        }


        public void Stop()
        {
            if (_arduino == null) return;
            if (_arduino.IsRunning)
                _arduino.Stop();
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
                    _bevelDoubleTapTimers.Add(side,em);
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
            _bevelDoubleTapTimers.Remove(e.Data);
        }

        void _manager_DigitalInReceived(object sender, Hardware.DigitalDataReivedHandler e)
        {
            switch (e.Id)
            {
                case 0:
                    _linearTouch.Down = e.Value;
                    if (_linearTouch.Down)
                    {
                        OnSliderTouchDownHandler(new SliderTouchEventArgs(_linearTouch, _linearTouch.Value));
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
                        OnSliderTouchUpHandler(new SliderTouchEventArgs(_linearTouch, _linearTouch.Value));
                        if (_wristBandDoubleTapTimer.Trigger && _wristBandDoubleTapTimer.Enabled)
                        {
                            _wristBandDoubleTapTimer.Trigger = false;
                            OnDoubleTap(new SliderTouchEventArgs(_linearTouch, _linearTouch.Value));
                            _wristBandDoubleTapTimer.Stop();

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

      
        void _manager_AnalogDataReceived(object sender, Hardware.AnalogDataReceivedEventArgs e)
        {
            switch (e.Id)
            {
                case 0:
                    _linearTouch.Value = e.Value;
                    if (_recording)

                    {
                        _pipeline.Enqueue(e.Value);

                    }
                    break;
            }
            if (_linearTouch.Down)
                OnRawDataHandler(new RawTouchDataReceivedEventArgs(_linearTouch));
        }


    
        protected void OnDoubleTap(SliderTouchEventArgs e)
        {
            if(DoubleTap != null)
                DoubleTap(this,e);
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
        public void PrintCollection<T>(IEnumerable<T> col)
        {
            foreach (var item in col)
                Console.Write(item + @","); // Replace this with your version of printing
            Console.Write(@"\n");
        }

        private void AnalyseData()
        {
            if (_pipeline.ToArray().Count() <= 1) return;
            var output = _gestureRecognizer.ComputeClosestLabel(_pipeline.ToArray());

            if (output == "Up")
                OnSlideUpHandler(new SliderTouchEventArgs(_linearTouch, -1));
            else
                OnSlideDownHandler(new SliderTouchEventArgs(_linearTouch, -1));
            _pipeline.Clear();
        }
    }
}
