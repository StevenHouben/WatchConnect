using System;

namespace Watch.Input.Sensors
{
    public class LightSensor : Sensor
    {
        double _maximumMeasuredLightIntensity;
        bool _inRange;
        public LightSensor(int id)
        {
            Id = id;
        }
        public override bool CalculateRange()
        {
            return Value < Treshold;
        }
        private void CalculateLightTreshold()
        {
            if (Value >= _maximumMeasuredLightIntensity)
            {
                _maximumMeasuredLightIntensity = Value;
                Treshold = _maximumMeasuredLightIntensity - 200;
            }


            var localRange = CalculateRange();
            if (localRange == _inRange) return;
            _inRange = localRange;
            OnRangeChanged(new RangeChangedEventArgs(this, _inRange, DateTime.UtcNow));
        }

        public override void ProcessSensorData()
        {
            CalculateLightTreshold();
        }
    }
}
