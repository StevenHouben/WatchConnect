namespace Watch.Toolkit.Sensors
{
    public class AccelerometerDataReceivedEventArgs
    {
        public Accelerometer Accelerometer { get; private set; }

        public AccelerometerDataReceivedEventArgs(Accelerometer acc)
        {
            Accelerometer = acc;
        }

    }
}
