using System;
using System.Collections.Generic;
using System.Windows;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Toolkit
{
    public class WatchConfiguration
    {
        public HardwarePlatform Hardware { get; set; }
        public Size DisplaySize { get; set; }

        public ClassifierConfiguration ClassifierConfiguration { get; set; }

        public static WatchConfiguration DefaultWatchConfiguration {
            get
            {
                return new WatchConfiguration
                {
                    Hardware = new Arduino(),
                    DisplaySize = new Size(800,600),
                    ClassifierConfiguration = new ClassifierConfiguration(
                         new List<string> { "Normal Mode", "Left Index", "Left Knuckle", "Hand" }, AppDomain.CurrentDomain.BaseDirectory + "recording16.log")
                };
            }
        }
    }
}
