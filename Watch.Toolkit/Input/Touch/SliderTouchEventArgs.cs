using System;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Touch
{
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
}
