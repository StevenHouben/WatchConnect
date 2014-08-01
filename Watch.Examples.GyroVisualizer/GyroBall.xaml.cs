using System;
using System.Windows.Controls;
using Watch.Toolkit.Sensors;

namespace Watch.Examples.GyroVisualizer
{
    public partial class GyroBall
    {
        private static double[] _xValues;
        private static double[] _yValues;
        private int _counter;
        private bool _initialized;
        public GyroBall()
        {
            InitializeComponent();
            _xValues = new double[100];
            _yValues = new double[100];

            Canvas.SetLeft(Ball, Width/2);
            Canvas.SetTop(Ball, Height/2);
        }

        internal void UpdateRelativePosition(Imu imu)
        {
            Dispatcher.Invoke(() => Console.WriteLine(imu.RawGyroValue.ToString()));
        }

        internal void UpdatePosition(Imu imu)
        {
            Dispatcher.Invoke(() =>
            {
                var valX = Map(imu.YawPitchRollValues.Z, -85, 85, 0, ActualWidth);
                var valY = Map(imu.YawPitchRollValues.Y, -85, 85, 0, ActualHeight);

                _xValues[_counter] = valX;
                _yValues[_counter] = valY;

                if (!_initialized)
                {
                    Canvas.SetLeft(Ball, valX);
                    Canvas.SetTop(Ball, valY);

                    if (_counter == _xValues.Length)
                        _initialized = true;
                }
                else
                {
                    double sumX = 0, sumY = 0;
                    for (var i = 0; i < _xValues.Length;i++)
                    {
                        sumX += _xValues[i];
                        sumY += _yValues[i];
                    }
                    var avgX = sumX / _xValues.Length;
                    var avgY = sumY / _yValues.Length;
                    Canvas.SetLeft(Ball, avgX);
                    Canvas.SetTop(Ball, avgY);
                }
                _counter++;
                if (_counter == _xValues.Length)
                    _counter = 0;
            });
        }

        public static double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
}
