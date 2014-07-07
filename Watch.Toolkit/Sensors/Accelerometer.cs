namespace Watch.Toolkit.Sensors
{
    public class Accelerometer:ISimpleSensor
    {
        private Vector _storedValues;
        public Vector RawGyroValue { get; set; }
        public Vector RawAccelerometerValues{ get; set; }
        public Vector RealWorldAccelerationValues { get; set; }
        public Vector YawPitchRollValues { get; set; }
        public Vector DistanceValues { get; set; }

        public bool Update(Accelerometer acc)
        {
            RawAccelerometerValues = acc.RawAccelerometerValues;
            RawGyroValue = acc.RawGyroValue;
            YawPitchRollValues = acc.YawPitchRollValues;
            RealWorldAccelerationValues = acc.RealWorldAccelerationValues;

            CalculateDistance();
            return true;
        }
        public bool Update(Vector rawAccData, Vector rawGyroData, Vector yawPitchRoll, Vector worldAcceleration)
        {
            RawAccelerometerValues = rawAccData;
            RawGyroValue = rawGyroData;
            YawPitchRollValues = yawPitchRoll;
            RealWorldAccelerationValues = worldAcceleration;

            CalculateDistance();
            return true;
        }

        private void CalculateDistance()
        {
            if (_storedValues == null)
                _storedValues = YawPitchRollValues;
            DistanceValues = _storedValues.ComputeDistance(YawPitchRollValues, 50);
            _storedValues = YawPitchRollValues;
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
