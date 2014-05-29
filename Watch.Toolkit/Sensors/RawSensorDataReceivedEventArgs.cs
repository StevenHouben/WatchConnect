using System;

namespace Watch.Toolkit.Sensors
{
    public class RawSensorDataReceivedEventArgs : EventArgs
    {
        public ProximitySensor FrontSensor { get; set; }

        public ProximitySensor TopLeftSensor { get; set; }

        public ProximitySensor TopRightSensor { get; set; }

        public ProximitySensor LightSensor { get; set; }

        public RawSensorDataReceivedEventArgs(ProximitySensor frontSensor, ProximitySensor topLeftSensor, ProximitySensor topRightSensor, ProximitySensor lightSensor)
        {
            FrontSensor = frontSensor;
            TopLeftSensor = topLeftSensor;
            TopRightSensor = topRightSensor;
            LightSensor = lightSensor;

        }
    }
}
