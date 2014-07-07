using System;
namespace Watch.Toolkit.Sensors
{
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public double[] RawData
        {
            get { return new[] { X, Y, Z }; }
        }
        public Vector Copy()
        {
            return new Vector(X,Y,Z);
        }
        public Vector Map(double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return new Vector(
                X.Map(fromSource, toSource, fromTarget, toTarget),
                Y.Map(fromSource, toSource, fromTarget, toTarget),
                Z.Map(fromSource, toSource, fromTarget, toTarget)
                );
        }
        public double Sum()
        {
            return X + Y + Z;
        }
        public Vector ComputeDistance(Vector v,int treshold)
        {
            return new Vector(
                Math.Abs(((Math.Abs(X - v.X)) / treshold)),
                Math.Abs(((Math.Abs(Y - v.Y)) / treshold)),
                Math.Abs(((Math.Abs(Z - v.Z)) / treshold)));
        }
        public override string ToString()
        {
            return "(X: " + X + " Y: " + Y + " Z: " + Z+")";
        }
    }
    public static class ExtensionMethods
    {
        public static double Map(this double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
}
