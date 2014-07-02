using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Sensors;
using Timer = System.Timers.Timer;

namespace Watch.Toolkit.Utils
{
    public partial class MainWindow
    {
        private readonly StringBuilder _logger = new StringBuilder();
        private readonly Accelerometer _accelerometer = new Accelerometer();
        private Timer _recorder = new Timer(500);
        private readonly Arduino _arduino;

        public DataType _recordingDatatype;
        public MainWindow()
        {
            InitializeComponent();

            _recordingDatatype = DataType.Filtered;

            Closing += MainWindow_Closing;

            InitializeComboBoxes();

            BtnStart.Click += BtnStart_Click;
            BtnStop.Click += BtnStop_Click;
            BtnPause.Click += BtnPause_Click;
            _arduino = new Arduino();
            _arduino.MessageReceived += arduino_MessageReceived;
            _arduino.Start();

        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _arduino.Stop();
        }

        void BtnPause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_recorder.Enabled)
            {
                _recorder.Stop();
            }
            else
            {
                _recorder.Start();
            }
        }
        private void InitializeComboBoxes()
        {
            for (var i = 0; i < 100; i++)
                CbLabel.Items.Add(i);
            CbLabel.Text = "0";

            CbData.Items.Add(DataType.Accelerometer);
            CbData.Items.Add(DataType.Filtered);
            CbData.Items.Add(DataType.Gyroscope);
            CbData.SelectionChanged += CbData_SelectionChanged;

            CbData.SelectedItem = _recordingDatatype = DataType.Filtered;
        }

        void CbData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CbData.Text == "") return;
            _recordingDatatype = (DataType)Enum.Parse(typeof(DataType), CbData.Text, true);
        }

        void BtnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _recorder.Stop();
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
            ListBox.Items.Clear();
            
        }

        void BtnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            _recorder = new Timer(Convert.ToInt32(TxTSampleRate.Text));
            _recorder.Elapsed += _recorder_Elapsed;
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

                if (data.Length != 11) return;

                try
                {
                    _accelerometer.Update(
                  Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[8], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[9], CultureInfo.InvariantCulture),
                  Convert.ToDouble(data[10], CultureInfo.InvariantCulture));

                    Dispatcher.Invoke(
                        () => 
                            LblData.Content = _accelerometer.ToFormattedString()
                            );
                }
                catch (FormatException)
                {
                  
                }
             
            }
            catch (Exception)
            {
                Console.WriteLine("");
            }
        }

        private int counter = 0;
        public void AddPoint(int label)
        {
            _logger.AppendLine(
                (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds+","+
                _accelerometer.RawAccelerometerValues.X + "," +
                _accelerometer.RawAccelerometerValues.Y + "," +
                _accelerometer.RawAccelerometerValues.Z + "," +
                _accelerometer.RawGyroValue.X+","+
                _accelerometer.RawGyroValue.Y + "," +
                _accelerometer.RawGyroValue.Z + "," +
                label);

            ListBox.Items.Insert(0, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + "," +
                _accelerometer.RawAccelerometerValues.X + "," +
                _accelerometer.RawAccelerometerValues.Y + "," +
                _accelerometer.RawAccelerometerValues.Z + "," +
                _accelerometer.RawGyroValue.X + "," +
                _accelerometer.RawGyroValue.Y + "," +
                _accelerometer.RawGyroValue.Z + "," +
                label);
        }
        //public void AddPoint(int label)
        //{
        //    _logger.AppendLine((
        //        int)_accelerometer.FilteredValues.X + "," +
        //        (int)_accelerometer.FilteredValues.Y + "," +
        //        (int)_accelerometer.FilteredValues.Z + "," + label);

        //    ListBox.Items.Insert(0,
        //        (int)_accelerometer.FilteredValues.X + "," +
        //        (int)_accelerometer.FilteredValues.Y + "," +
        //        (int)_accelerometer.FilteredValues.Z + "," + label);
        //}

        public void Save(string name)
        {
            _recorder.Stop();

            if (!File.Exists(name))
                File.AppendAllText(name, "X,Y,Z,LABEL\n");
            File.AppendAllText(name, _logger.ToString());
            _logger.Clear();

        }
    }

    public enum DataType
    {
        Filtered,
        Accelerometer,
        Gyroscope
    }
    public class HiResDateTime
    {
        private static long lastTimeStamp = DateTime.UtcNow.Ticks;
        public static long UtcNowTicks
        {
            get
            {
                long orig, newval;
                do
                {
                    orig = lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks;
                    newval = Math.Max(now, orig + 1);
                } while (Interlocked.CompareExchange
                             (ref lastTimeStamp, newval, orig) != orig);

                return newval;
            }
        }
    }
}
