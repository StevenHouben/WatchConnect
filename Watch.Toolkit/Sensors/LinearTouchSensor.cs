using System;

namespace Watch.Toolkit.Sensors
{
    public class LinearTouchSensor:TouchSensor
    {
        protected override void ProcessSensorData()
        {
            if (Value < 120)
            {
                Down = false;
                _value = 0;
            }
            else Down = true;

        }
    }
}
