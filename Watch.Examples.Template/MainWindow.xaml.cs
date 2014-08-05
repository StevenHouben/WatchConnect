using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.Template
{
    public partial class MainWindow
    {

        private WatchRuntime _watchWindow;
        private WatchConfiguration _watchConfiguration;
        public MainWindow()
        {
            InitializeComponent();

            KeyDown += MainWindow_KeyDown;
            //Change machine learning data and hardware input here
            _watchConfiguration = new WatchConfiguration
            {
                Hardware = new Arduino("COM4"),
                ClassifierConfiguration = new ClassifierConfiguration(
                    new List<string> {"Right Hand", "Left Hand", "Left Knuckle", "Hand"},
                    AppDomain.CurrentDomain.BaseDirectory + "recording19.log")
            };

            _watchWindow = new WatchRuntime(_watchConfiguration);
            _watchWindow.AddWatchFace(new WatchFaceExample());
            _watchWindow.Show();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
           if(e.Key == Key.Escape)
               Environment.Exit(0);
        }

    }
}
