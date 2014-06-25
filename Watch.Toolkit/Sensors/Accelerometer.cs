namespace Watch.Toolkit.Sensors
{
    public class Accelerometer:ISensor
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double Fx { get; set; }
        public double Fy { get; set; }
        public double Fz { get; set; }

        public double Roll { get; set; }
        public double Pitch { get; set; }

        public double[] RawData
        {
            get { return new [] { X, Y, Z }; }
        }

        public double[] RawFilteredData
        {
            get { return new [] { Fx, Fy, Fz }; }
        }

        public Accelerometer() { }

        public Accelerometer(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Accelerometer(double x, double y, double z,double fx, double fy, double fz,double roll, double pitch)
        {
            X = x;
            Y = y;
            Z = z;
            Fx = fx;
            Fy = fy;
            Fz = fz;
            Roll = roll;
            Pitch = pitch;
        }

        public override string ToString()
        {
            return "X: " + X + " Y: " + Y + " Z: " + Z;
        }
        public string ToFormattedString()
        {
            return "X: " + X + "\nfX: " + Fx + "\nY: " + Y + "\nfY: " + Fy +"\nZ: " + Z
                +"\nfZ: " + Fz +"\nRoll: "+Roll + "\nPitch: "+Pitch;
        }

        public string Name { get; set; }

        public int Id { get; set; }

        public double Value { get; set; }
    }
    
}
