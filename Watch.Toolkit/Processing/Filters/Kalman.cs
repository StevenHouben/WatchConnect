namespace Watch.Toolkit.Processing.Filters
{
    public class Kalman
    {
        private const double Q = 0.000001;
        private const double R = 0.01;
        private static double _p = 1, _x, _k;

        private static void MeasurementUpdate()
        {
            _k = (_p + Q) / (_p + Q + R);
            _p = R * (_p + Q) / (R + _p + Q);
        }

        public static double Update(double measurement)
        {
            MeasurementUpdate();
            double result = _x + (measurement - _x) * _k;
            _x = result;
            return result;
        }
    }
}
