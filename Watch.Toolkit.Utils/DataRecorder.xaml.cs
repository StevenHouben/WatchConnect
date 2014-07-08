using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Watch.Toolkit.Sensors;
using Timer = System.Timers.Timer;

namespace Watch.Toolkit.Utils
{
    public partial class MainWindow
    {
        private readonly StringBuilder _logger = new StringBuilder();
        private readonly Imu _accelerometer = new Imu();
        private readonly ImuParser _accelerometerParser = new ImuParser();
        private Timer _recorder = new Timer(500);

        public DataType RecordingDatatype;

        public MainWindow()
        {
            InitializeComponent();

            RecordingDatatype = DataType.Filtered;

            Closing += MainWindow_Closing;

            InitializeComboBoxes();

            BtnStart.Click += BtnStart_Click;
            BtnStop.Click += BtnStop_Click;
            BtnPause.Click += BtnPause_Click;

            _accelerometerParser.AccelerometerDataReceived += _accelerometerParser_AccelerometerDataReceived;
            _accelerometerParser.Start();
        }

        void _accelerometerParser_AccelerometerDataReceived(object sender, ImuDataReceivedEventArgs e)
        {
            _accelerometer.Update(e.Accelerometer);
            Dispatcher.Invoke(
                () =>
                    LblData.Content = _accelerometer.ToFormattedString()
                    );
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _accelerometerParser.Stop();
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

            CbData.Items.Add(DataType.AllRawData);
            CbData.Items.Add(DataType.Accelerometer);
            CbData.Items.Add(DataType.Filtered);
            CbData.Items.Add(DataType.Gyroscope);
            CbData.SelectionChanged += CbData_SelectionChanged;

            CbData.SelectedItem = RecordingDatatype = DataType.Filtered;
        }

        void CbData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CbData.Text == "") return;
            RecordingDatatype = (DataType)Enum.Parse(typeof(DataType), CbData.Text, true);
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

        private int counter = 0;
        public void AddPoint(int label)
        {
            switch (RecordingDatatype)
            {
                case DataType.Accelerometer:
                    _logger.AppendLine(
                        (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds+" "+
                        _accelerometer.RawAccelerometerValues.X + " " +
                        _accelerometer.RawAccelerometerValues.Y + " " +
                        _accelerometer.RawAccelerometerValues.Z + " " +
                        label);

                    ListBox.Items.Insert(0, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + " " +
                        _accelerometer.RawAccelerometerValues.X + " " +
                        _accelerometer.RawAccelerometerValues.Y + " " +
                        _accelerometer.RawAccelerometerValues.Z + " " +
                        label);
                    break;
                case DataType.Filtered:
                     _logger.AppendLine(
                        _accelerometer.YawPitchRollValues.X + " " +
                        _accelerometer.YawPitchRollValues.Y + " " +
                        _accelerometer.YawPitchRollValues.Z + " " +
                        label);

                    ListBox.Items.Insert(0, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + " " +
                        _accelerometer.YawPitchRollValues.X + " " +
                        _accelerometer.YawPitchRollValues.Y + " " +
                        _accelerometer.YawPitchRollValues.Z + " " +
                        label);
                    break;
                case DataType.Gyroscope:
                    _logger.AppendLine(
                        (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds+" "+
                        _accelerometer.RawGyroValue.X + " " +
                        _accelerometer.RawGyroValue.Y + " " +
                        _accelerometer.RawGyroValue.Z + " " +
                        label);

                    ListBox.Items.Insert(0, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + " " +
                        _accelerometer.RawGyroValue.X + " " +
                        _accelerometer.RawGyroValue.Y + " " +
                        _accelerometer.RawGyroValue.Z + " " +
                        label);
                    break;
                case DataType.AllRawData:
                    _logger.AppendLine(
                (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds+" "+
                _accelerometer.RawAccelerometerValues.X + " " +
                _accelerometer.RawAccelerometerValues.Y + " " +
                _accelerometer.RawAccelerometerValues.Z + " " +
                _accelerometer.RawGyroValue.X+" "+
                _accelerometer.RawGyroValue.Y + " " +
                _accelerometer.RawGyroValue.Z + " " +
                label);

            ListBox.Items.Insert(0, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + " " +
                _accelerometer.RawAccelerometerValues.X + " " +
                _accelerometer.RawAccelerometerValues.Y + " " +
                _accelerometer.RawAccelerometerValues.Z + " " +
                _accelerometer.RawGyroValue.X + " " +
                _accelerometer.RawGyroValue.Y + " " +
                _accelerometer.RawGyroValue.Z + " " +
                label);
                    break;
            }         
        }

        public void Save(string name)
        {
            _recorder.Stop();

            if (!File.Exists(name))
                File.AppendAllText(name, "X Y Z LABEL\n");
            File.AppendAllText(name, _logger.ToString());
            _logger.Clear();

        }
    }

    public enum DataType
    {
        Filtered,
        Accelerometer,
        Gyroscope,
        AllRawData
    }
    public class HiResDateTime
    {
        private static long _lastTimeStamp = DateTime.UtcNow.Ticks;
        public static long UtcNowTicks
        {
            get
            {
                long orig, newval;
                do
                {
                    orig = _lastTimeStamp;
                    var now = DateTime.UtcNow.Ticks;
                    newval = Math.Max(now, orig + 1);
                } while (Interlocked.CompareExchange
                             (ref _lastTimeStamp, newval, orig) != orig);

                return newval;
            }
        }
    }
}
