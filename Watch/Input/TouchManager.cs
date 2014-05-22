using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Phidgets;
using Phidgets.Events;
using Watch.Input.Recognizers;
using Watch.Input.Sensors;
using System.Windows.Input;

namespace Watch.Input
{
    public class TouchManager
    {
        public const int Id = 28157;
        public event EventHandler<RawTouchDataReceivedEventArgs> RawDataReceived;

        public event EventHandler<SliderTouchEventArgs> SliderTouchDown;
        public event EventHandler<SliderTouchEventArgs> SliderTouchUp;
        public event EventHandler<SliderTouchEventArgs> SlideDown;
        public event EventHandler<SliderTouchEventArgs> SlideUp;


        readonly TouchSensor _linearTouch = new LinearTouchSensor();

        private InterfaceKit _kit;

        private readonly Queue<double> _pipeline = new Queue<double>();
        private bool _recording;

        private DtwRecognizer _gestureRecognizer = new DtwRecognizer();

        public void Start()
        {
            _kit = Hardware.InterfaceKit;
            _kit.SensorChange += kit_SensorChange;
            _kit.InputChange += kit_InputChange;
            _kit.open(Id);

            _kit.waitForAttachment();

            _gestureRecognizer.AddTemplate(
                new double[] { 173, 275, 409, 567, 732, 897, 1000 },
                "Up");

            _gestureRecognizer.AddTemplate(
                new double[] { 1000, 857, 723, 576, 276, 289, 120 }, 
                "Down");

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
                    }
                    else
                    {
                        OnSliderTouchUpHandler(new SliderTouchEventArgs(_linearTouch, _linearTouch.Value));
                        if (_recording)
                        {
                            _recording = false;
                            AnalyseData();
                        }

                    }
                    break;
            }
        }
        public void PrintCollection<T>(IEnumerable<T> col)
        {
            foreach (var item in col)
                Console.Write(item + ","); // Replace this with your version of printing
            Console.Write("\n");
        }

        private void AnalyseData()
        {
            //if (_pipeline.Count == 0)
            //    return;
            //var built = new StringBuilder();
            //var li = new List<int>();
            //var storage = new List<int>();
            //while (_pipeline.Count != 0)
            //{
            //    var value = _pipeline.Dequeue();
            //    if (storage.Count == 0 || value > storage[storage.Count - 1])
            //        li.Add(0);
            //    else
            //        li.Add(1);
            //    storage.Add(value);
            //    built.Append(value);
            //    if (_pipeline.Count > 0)
            //        built.Append(",");
            //}
            //built.Append("\n");
            //File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", built.ToString());

            //var output = li.GroupBy(v => v)
            //.OrderByDescending(g => g.Count())
            //.First()
            //.Key;

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
