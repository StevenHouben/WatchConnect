namespace Watch.Input.Sensors.Filters
{
    public class Kalman
    {
        private static double Q = 0.000001;
        private static double R = 0.01;
        private static double P = 1, X = 0, K;

        private static void MeasurementUpdate()
        {
            K = (P + Q) / (P + Q + R);
            P = R * (P + Q) / (R + P + Q);
        }

        public static double Update(double measurement)
        {
            MeasurementUpdate();
            double result = X + (measurement - X) * K;
            X = result;
            return result;
        }
    }
}
