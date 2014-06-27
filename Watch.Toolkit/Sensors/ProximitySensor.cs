using System;

namespace Watch.Toolkit.Sensors
{
    public abstract class ProximitySensor:ISimpleSensor
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

    
}
