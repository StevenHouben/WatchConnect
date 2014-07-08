namespace Watch.Toolkit.Sensors
{
    public class ImuDataReceivedEventArgs
    {
        public Imu Accelerometer { get; private set; }

        public ImuDataReceivedEventArgs(Imu acc)
        {
            Accelerometer = acc;
        }

    }
}
