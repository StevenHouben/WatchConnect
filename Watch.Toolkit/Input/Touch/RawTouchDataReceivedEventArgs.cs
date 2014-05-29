using System;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Touch
{
    public class RawTouchDataReceivedEventArgs : EventArgs
    {
        public TouchSensor LinearTouch { get; set; }

        public RawTouchDataReceivedEventArgs(TouchSensor linear)
        {
            LinearTouch = linear;

        }
    }
}
