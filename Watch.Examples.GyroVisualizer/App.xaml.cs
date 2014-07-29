using System;
using System.Collections.Generic;
using System.Windows;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.GyroVisualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Create a new configuration
            var configuration = new WatchConfiguration
            {
                DisplaySize = new Size(800, 600),
                Hardware = new Arduino("COM4"),
                ClassifierConfiguration = new ClassifierConfiguration(
                     new List<string> { "Idle", "Left Index", "Left Knuckle" },
                     AppDomain.CurrentDomain.BaseDirectory + "recording18.log")
            };

            var gyroBall = new GyroBall();

            //Create a new instance of the WatchRuntime
            var watchWindow = new WatchRuntime(configuration);

            //Add a default Watchface to the watch
            watchWindow.AddWatchFace(gyroBall);

            watchWindow.TrackerManager.Imu.ImuUpdated += (sender, ev) => gyroBall.UpdatePosition(watchWindow.TrackerManager.Imu);
            


            watchWindow.Show();

        }
    }
}
