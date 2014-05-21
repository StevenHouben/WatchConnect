using System;

namespace Watch.Input.Sensors
{
    public class InfraredSensor : Sensor
    {
        private bool _inRange;
        public InfraredSensor(int id, int treshold)
        {
            Id = id;
            Treshold = treshold;
        }

        public double CalculateDistance()
        {
            if ((Value < 500) && (Value > 80))
            {
                return 4800.0 / (Value - 20);
            }
            return -1;
        }

        public override bool CalculateRange()
        {
            return Value > Treshold;
        }

        public override void ProcessSensorData()
        {
            var localRange = CalculateRange();
            if (localRange == _inRange) return;
            _inRange = localRange;
            OnRangeChanged(new RangeChangedEventArgs(this, _inRange, DateTime.UtcNow));
        }
    }
}
