using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Hardware.Phidget;
using Watch.Toolkit.Input.Recognizers;
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
        public event EventHandler<BevelTouchEventArgs> BevelDoubleTap; 

        public event EventHandler<MultiBevelTouchEventArgs> BevelGrab;

        readonly TouchSensor _linearTouch = new LinearTouchSensor();

        private readonly Queue<double> _pipeline = new Queue<double>();
        private bool _recording;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();

        private readonly EventMonitor _wristBandDoubleTapTimer = new EventMonitor(300);
        private readonly EventMonitor _bevelMonitor = new EventMonitor(300);

        private Phidget _phidget;
        private Arduino _arduino;

        private BevelState _bevelState = new BevelState();

        public void Start()
        {
            _phidget = new Phidget();
            _phidget.Start(28157);
            _phidget.AnalogDataReceived += _manager_AnalogDataReceived;
            _phidget.DigitalInReceived += _manager_DigitalInReceived;

            _gestureRecognizer.AddTemplate(
                "Up",
                new double[] { 173, 275, 409, 567, 732, 897, 1000 });

            _gestureRecognizer.AddTemplate(
                "Down",
                new double[] { 1000, 857, 723, 576, 276, 289, 120 });


            _arduino = new Arduino();

            _arduino.MessageReceived += _arduino_MessageReceived;
            _arduino.Start("COM4");
        }

        void _arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
        {
            if (!e.Message.StartsWith("T"))
                return;

            if (e.Message.Length != 4) 
                return;

            var state = new BevelState()
            {
                BevelTop = e.Message[0] == '1',
                BevelBottom = e.Message[1] == '1',
                BevelRight = e.Message[2] == '1',
                BevelLeft = e.Message[3] == '1'
            };

            CompareStates(BevelSide.TopSide, state.BevelTop, _bevelState.BevelTop);
            CompareStates(BevelSide.LeftSide, state.BevelLeft, _bevelState.BevelLeft);
            CompareStates(BevelSide.RightSide, state.BevelRight, _bevelState.BevelRight);
            CompareStates(BevelSide.BottomSide, state.BevelBottom, _bevelState.BevelBottom);

            _bevelState = state;
        }

        public void Stop()
        {
            if (_arduino.IsRunning)
                _arduino.Stop();

            if(_phidget.IsRunning)
                _phidget.Stop();
        }

        void CompareStates(BevelSide side, bool stateNow, bool stateOld)
        {
            if (stateNow == stateOld) return;
            if(stateNow)
                OnBevelTouchDownHandler(new BevelTouchEventArgs(side,0));
            else
                OnBevelTouchUpHandler(new BevelTouchEventArgs(side,0));
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
