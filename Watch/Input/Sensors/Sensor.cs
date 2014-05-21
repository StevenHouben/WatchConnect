using System;

namespace Watch.Input.Sensors
{
    public abstract class Sensor
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
        public abstract bool CalculateRange();
        public abstract void ProcessSensorData();
    }

    public class RangeChangedEventArgs : EventArgs
    {
        public Sensor Sensor { get; set; }
        public bool InRange { get; set; }
        public DateTime TimeStamp { get; set; }

        public RangeChangedEventArgs(Sensor sensor, bool inRange, DateTime timeStamp)
        {
            Sensor = sensor;
            TimeStamp = timeStamp;
            InRange = inRange;
        }
    }
}
