using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Sensors;

namespace AccelerometerVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<PathFigure> _rawdata = new List<PathFigure>();
        public MainWindow()
        {
            InitializeComponent();
            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start("COM4");

            Graph.Data = new PathGeometry(_rawdata);
        }

        private int count = 0;
        void arduino_MessageReceived(object sender, Watch.Toolkit.Hardware.MessagesReceivedEventArgs e)
        {
            var data = e.Message.Split('|');

            if (data.Length != 8) return;

             var accelerometerData = new Accelerometer(
                Convert.ToDouble(data[0], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[7], CultureInfo.InvariantCulture));
            if (_rawdata.Count < 100)
            {
                _rawdata.Add(new PathFigure(new Point(count * 10, accelerometerData.X)));
            }
            else
            {
                Graph.Data = new PathGeometry(new List<PathFigure>(_rawdata));
                _rawdata.Clear();
                ;
            }
        }
    }
}
