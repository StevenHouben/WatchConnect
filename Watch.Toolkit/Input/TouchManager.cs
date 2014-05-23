using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Phidgets;
using Phidgets.Events;
using Watch.Toolkit.Input.Recognizers;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input
{
    public class TouchManager
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

        readonly TouchSensor _linearTouch = new LinearTouchSensor();

        private InterfaceKit _kit;

        private readonly Queue<double> _pipeline = new Queue<double>();
        private bool _recording;

        private readonly DtwRecognizer _gestureRecognizer = new DtwRecognizer();

        private readonly Timer _doubleTapTimer = new Timer(300);
        private bool _doubleTapTrigger;

        public void Start()
        {
            _kit = Hardware.Hardware.InterfaceKit;
            _kit.SensorChange += kit_SensorChange;
            _kit.InputChange += kit_InputChange;
            _kit.open(Id);

            _kit.waitForAttachment();

            _gestureRecognizer.AddTemplate(
                "Up",
                new double[] { 173, 275, 409, 567, 732, 897, 1000 });

            _gestureRecognizer.AddTemplate(
                "Down",
                new double[] { 1000, 857, 723, 576, 276, 289, 120 });

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

        void kit_InputChange(object sender, InputChangeEventArgs e)
        {
            switch (e.Index)
            {
                case 0:
                    _linearTouch.Down = e.Value;
                    if (_linearTouch.Down)
                    {
                        OnSliderTouchDownHandler(new SliderTouchEventArgs(_linearTouch, _linearTouch.Value));
                        _recording = true;
                        if (!_doubleTapTimer.Enabled)
                        {
                            _doubleTapTimer.Elapsed += _doubleTapTimer_Elapsed;
                            _doubleTapTimer.Start();
                            _doubleTapTrigger = true;
                        }
                    }
                    else
                    {
                        OnSliderTouchUpHandler(new SliderTouchEventArgs(_linearTouch, _linearTouch.Value));
                        if (_doubleTapTrigger && _doubleTapTimer.Enabled)
                        {
                            _doubleTapTrigger = false;
                            OnDoubleTap(new SliderTouchEventArgs(_linearTouch,_linearTouch.Value));
                            _doubleTapTimer.Stop();

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

        void _doubleTapTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _doubleTapTimer.Stop();
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
            var output = _gestureRecognizer.FindClosestLabel(_pipeline.ToArray());

            if (output == "Up")
                OnSlideUpHandler(new SliderTouchEventArgs(_linearTouch, -1));
            else
                OnSlideDownHandler(new SliderTouchEventArgs(_linearTouch, -1));
            _pipeline.Clear();
        }

        void kit_SensorChange(object sender, SensorChangeEventArgs e)
        {
            switch (e.Index)
            {
                case 0:
                    _linearTouch.Value = e.Value;
                    if (_recording)
                    {
                        _pipeline.Enqueue(e.Value);
    
                    }
                    break;
            }
            if(_linearTouch.Down)
                OnRawDataHandler(new RawTouchDataReceivedEventArgs(_linearTouch));
           
        }
    }

    public class BevelTouchEventArgs : EventArgs
    {
        public BevelSide BevelSide { get; set; }
        public double Value { get; set; }

        public BevelTouchEventArgs(BevelSide bevelSide, double value)
        {
            BevelSide = bevelSide;
            Value = value;
        }
    }

    public enum BevelSide
    {
        LeftSide,
        LeftTop,
        RightSide,
        RightTop,
        BottomSide,
        BottomTop,
        TopSide,
        TopTop
    }

    public class SliderTouchEventArgs : EventArgs
    {
        public TouchSensor Sensor { get; set; }
        public double Value { get; set; }

        public SliderTouchEventArgs(TouchSensor sensor, double value)
        {
            Sensor = sensor;
            Value = value;
        }
    }
    public class RawTouchDataReceivedEventArgs : EventArgs
    {
        public TouchSensor LinearTouch { get; set; }

        public RawTouchDataReceivedEventArgs(TouchSensor linear)
        {
            LinearTouch = linear;

        }
    }
}
