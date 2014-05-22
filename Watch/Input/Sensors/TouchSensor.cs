using System;

namespace Watch.Input.Sensors
{
    public abstract class TouchSensor:ISensor
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
