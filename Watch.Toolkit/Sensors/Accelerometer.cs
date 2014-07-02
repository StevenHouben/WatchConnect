using System;
using System.Security.Permissions;

namespace Watch.Toolkit.Sensors
{
    public class Accelerometer:ISimpleSensor
    {
        private Vector _storedValues;
        public double Delta { get; private set; }
        public Vector RawGyroValue { get; set; }
        public Vector RawAccelerometerValues{ get; set; }
        public Vector FilteredValues { get; set; }
        public Vector DistanceValues { get; set; }

        public void Update(double delta, double accX, double accY, double accZ,double gyrX, double gyrY, double gyrZ,double filX,double filY, double filZ)
        {
            Delta = delta;
            RawAccelerometerValues = new Vector(accX, accY, accZ);
            RawGyroValue = new Vector(gyrX,gyrY,gyrZ);
            FilteredValues =  new Vector(filX,filY,filZ);

            CalculateDistance();
        }

        private void CalculateDistance()
        {
            if (_storedValues == null)
                _storedValues = FilteredValues;
            DistanceValues = _storedValues.ComputeDistance(FilteredValues, 50);
            _storedValues = FilteredValues;
        }

        public string ToFormattedString()
        {
            return "X: " + FilteredValues.X +
                   "\nY: " + FilteredValues.Y +
                   "\nZ: " + FilteredValues.Z +
                   "\nAccX: " + RawAccelerometerValues.X +
                   "\nAccY: " + RawAccelerometerValues.Y +
                   "\nAccZ:" + RawAccelerometerValues.Z +
                   "\nGyrX: " + RawGyroValue.X +
                   "\nGyrY: " + RawGyroValue.Y +
                   "\nGyrZ: " + RawGyroValue.Z;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public double Value { get; set; }

     }
 }
