using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using Microsoft.Win32;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Utils
{
    public partial class MainWindow
    {
        private readonly StringBuilder _logger = new StringBuilder();
        private readonly Accelerometer _accelerometer = new Accelerometer();
        private readonly Timer _recorder = new Timer(500);
        public MainWindow()
        {
            InitializeComponent();

            InitializeLabelBox();

            BtnStart.Click += BtnStart_Click;
            BtnStop.Click += BtnStop_Click;
            BtnPause.Click += BtnPause_Click;
            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            _recorder.Elapsed += _recorder_Elapsed;
            arduino.Start();

        }

        void BtnPause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_recorder.Enabled)
            {
                _recorder.Stop();
                BtnPause.Content = "Resume Recording";
            }
            else
            {
                _recorder.Start();
                BtnPause.Content = "Pause Recording";
            }
        }
        private void InitializeLabelBox()
        {
            for (var i = 0; i < 100; i++)
                CbLabel.Items.Add(i);
            CbLabel.Text = "0";
        }

        void BtnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = "Recording"+Guid.NewGuid(),
                DefaultExt = ".log",
                Filter = "Log (.log)|*.log"
            };

             var result = dlg.ShowDialog();

            if (result == true)
            {
                Save(dlg.FileName);
            }
        }

        void BtnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _recorder.Start();
        }

        void _recorder_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => AddPoint(Convert.ToInt32(CbLabel.Text)));
            
        }

        void arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
        {
            try
            {
                var data = e.Message.Split(',');

                if (data.Length != 9) return;

                _accelerometer.Update(
                    Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[8], CultureInfo.InvariantCulture));

            }
            catch (Exception)
            {
                Console.WriteLine("");
            }
        }

        public void AddPoint(int label)
        {
            _logger.AppendLine((
                int)_accelerometer.FilteredValues.X + "," + 
                (int)_accelerometer.FilteredValues.Y + "," + 
                (int)_accelerometer.FilteredValues.Z + "," + label);

            ListBox.Items.Add(
                (int)_accelerometer.FilteredValues.X + "," + 
                (int)_accelerometer.FilteredValues.Y + "," + 
                (int)_accelerometer.FilteredValues.Z + "," + label);
        }

        public void Save(string name)
        {
            _recorder.Stop();

            if (!File.Exists(name))
                File.AppendAllText(name, "X,Y,Z,LABEL\n");
            File.AppendAllText(name, _logger.ToString());
            _logger.Clear();
            ListBox.Items.Clear();
        }
    }
}
