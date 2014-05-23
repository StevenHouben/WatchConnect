using System;

namespace Watch.Toolkit.Sensors
{
    public class LightSensor : ProximitySensor
    {
        double _maximumMeasuredLightIntensity;
        bool _inRange;
        public LightSensor(int id)
        {
            Id = id;
        }
        protected override bool CalculateRange()
        {
            return Value < Treshold;
        }
        private void CalculateLightTreshold()
        {
            if (Value >= _maximumMeasuredLightIntensity)
            {
                _maximumMeasuredLightIntensity = Value;
                Treshold = _maximumMeasuredLightIntensity - 300;
            }


            var localRange = CalculateRange();
            if (localRange == _inRange) return;
            _inRange = localRange;
            OnRangeChanged(new RangeChangedEventArgs(this, _inRange, DateTime.UtcNow));
        }

        protected override void ProcessSensorData()
        {
            CalculateLightTreshold();
        }
    }
}
