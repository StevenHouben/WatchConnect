using Watch.Input.Sensors;

namespace Watch.Examples
{
    public partial class SensorVisualizer 
    {
        public SensorVisualizer()
        {
            InitializeComponent();
        }
        public void UpdateVisualization(Sensor topLeft,Sensor topRight, Sensor front, Sensor light)
        {
            Dispatcher.Invoke(() =>
            {
                FrontSensorBar.Value = front.Value;
                TopLeftSensorBar.Value = topLeft.Value;
                TopLeftSensorBarTreshold.Value = topLeft.InRange ? topLeft.Treshold : 0;
                TopRightSensorBar.Value = topRight.Value;
                TopRightSensorBarTreshold.Value = topRight.InRange ? topRight.Treshold : 0;
                LightSensorBar.Value = light.Value;
                FrontSensorBarTreshold.Value = front.InRange ? front.Treshold : 0;
                LightSensorBarTreshold.Value = light.InRange ? light.Treshold : 0;
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
    }
}
