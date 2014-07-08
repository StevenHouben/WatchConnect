namespace Watch.Toolkit.Sensors
{
    public class Imu:ISimpleSensor
    {
        private Vector _storedValues;
        public Vector RawGyroValue { get; set; }
        public Vector RawAccelerometerValues{ get; set; }
        public Vector RealWorldAccelerationValues { get; set; }
        public Vector YawPitchRollValues { get; set; }
        public Vector RawMagnetometerValues { get; set; }

        public bool Update(Imu acc)
        {
            RawAccelerometerValues = acc.RawAccelerometerValues;
            RawGyroValue = acc.RawGyroValue;
            YawPitchRollValues = acc.YawPitchRollValues;
            RealWorldAccelerationValues = acc.RealWorldAccelerationValues;

            return true;
        }
        public bool Update(Vector rawAccData, Vector rawGyroData, Vector yawPitchRoll, Vector worldAcceleration)
        {
            RawAccelerometerValues = rawAccData;
            RawGyroValue = rawGyroData;
            YawPitchRollValues = yawPitchRoll;
            RealWorldAccelerationValues = worldAcceleration;
            return true;
        }


        public string ToFormattedString()
        {
            return "Yaw: " + YawPitchRollValues.X +
                   "\nPitch: " + YawPitchRollValues.Y +
                   "\nRoll: " + YawPitchRollValues.Z +
                   "\nAccX: " + RawAccelerometerValues.X +
                   "\nAccY: " + RawAccelerometerValues.Y +
                   "\nAccZ: " + RawAccelerometerValues.Z +
                   "\nGyrX: " + RawGyroValue.X +
                   "\nGyrY: " + RawGyroValue.Y +
                   "\nGyrZ:" + RawGyroValue.Z;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public double Value { get; set; }

     }
 }
