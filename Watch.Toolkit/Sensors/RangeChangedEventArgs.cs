using System;
namespace Watch.Toolkit.Sensors
{
    public class RangeChangedEventArgs : EventArgs
    {
        public ProximitySensor Sensor { get; set; }
        public bool InRange { get; set; }
        public DateTime TimeStamp { get; set; }

        public RangeChangedEventArgs(ProximitySensor sensor, bool inRange, DateTime timeStamp)
        {
            Sensor = sensor;
            TimeStamp = timeStamp;
            InRange = inRange;
        }
    }
}
