using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Tracker
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
