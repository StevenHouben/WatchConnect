using System;

namespace Watch.Toolkit.Sensors
{
    public abstract class TouchSensor:ISimpleSensor
    {
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

        public bool Down { get; set; }
        protected abstract void ProcessSensorData();
    }
}
