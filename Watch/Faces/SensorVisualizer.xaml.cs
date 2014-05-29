using System;
using System.Windows.Media;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Sensors;

namespace Watch.Faces
{
    public partial class SensorVisualizer 
    {
        public SensorVisualizer()
        {
            InitializeComponent();
        }
        public void UpdateVisualization(ProximitySensor topLeft,ProximitySensor topRight, ProximitySensor front, ProximitySensor light)
        {
            Dispatcher.Invoke(() =>
            {
                FrontSensorBar.Value = front.Value;
                FrontSensorBarTreshold.Value = front.InRange ? front.Treshold : 0;

                TopLeftSensorBar.Value = topLeft.Value;
                TopLeftSensorBarTreshold.Value = topLeft.InRange ? topLeft.Treshold : 0;


                TopRightSensorBar.Value = topRight.Value;
                TopRightSensorBarTreshold.Value = topRight.InRange ? topRight.Treshold : 0;
                LightSensorBar.Value = light.Value;
                LightSensorBarTreshold.Value = light.InRange ? light.Treshold : 0;

            });
        }

        private int count = 0;


        public void UpdateBevels(BevelState state)
        {
            Dispatcher.Invoke(() =>
            {
                BevelRight.Fill = state.BevelRight ? Brushes.Red : Brushes.White;
                BevelLeft.Fill = state.BevelLeft ? Brushes.Red : Brushes.White;
                BevelTop.Fill = state.BevelTop ? Brushes.Red : Brushes.White;
                BevelBottom.Fill = state.BevelBottom ? Brushes.Red : Brushes.White;
            });
            
        }

        public void UpdateDetection(string name)
        {
            Dispatcher.Invoke(() =>
            {
                Output.Content = name;
                Archive.Text = name + "\n" + Archive.Text;
            });
        }
        public void UpdateEvents(string name)
        {
            Dispatcher.Invoke(() =>
            {
                GestureEvents.Content = name;
            });
        }

        public void UpdateLinearTouch(TouchSensor sensor)
        {
            Dispatcher.Invoke(() =>
            {
                //Console.WriteLine("Sensor down ->"+sensor.Down);
                TouchSlider.Value = sensor.Down ? sensor.Value : 0;
            });
        }
    }
}
