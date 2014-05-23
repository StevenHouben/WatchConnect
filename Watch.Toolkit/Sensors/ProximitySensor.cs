using System;

namespace Watch.Toolkit.Sensors
{
    public abstract class ProximitySensor:ISensor
    {
        public event EventHandler<RangeChangedEventArgs> RangeChanged;
        public string Name { get; set; }

        public int Id { get; set; }

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ProcessSensorData();
            }
        }
        public double Treshold { get; set; }

        public bool InRange
        {
            get { return CalculateRange(); }
        }

        protected void OnRangeChanged(RangeChangedEventArgs e)
        {
            if (RangeChanged != null)
                RangeChanged(this, e);
        }
        protected abstract bool CalculateRange();
        protected abstract void ProcessSensorData();
    }

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
