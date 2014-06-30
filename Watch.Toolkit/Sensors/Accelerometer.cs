using System;

namespace Watch.Toolkit.Sensors
{
    public class Accelerometer:ISimpleSensor
    {
        private Vector _storedValues;

        public Vector RawValues { get; set; }
        public Vector FilteredValues { get; set; }
        public Vector DistanceValues { get; set; }
        public double Roll { get; set; }
        public double Pitch { get; set; }

        public void Update(double x, double y, double z,double fx, double fy, double fz,double roll, double pitch)
        {
            RawValues =  new Vector(x,y,z);
            FilteredValues =  new Vector(fx,fy,fz);
            Roll = roll;
            Pitch = pitch;

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
            return "X: " + 
                RawValues.X + "\nfX: " + 
                FilteredValues.X + "\nY: " + 
                RawValues.Y + "\nfY: " + 
                FilteredValues.Y +"\nZ: " +
                RawValues.Z +"\nfZ: " + 
                FilteredValues.Z +"\nRoll: "
                +Roll + "\nPitch: "
                +Pitch;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public double Value { get; set; }

     }
 }
