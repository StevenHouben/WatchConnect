using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Statistics.Filters;
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

            BtnStart.Click += BtnStart_Click;
            BtnStop.Click += BtnStop_Click;
            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            _recorder.Elapsed += _recorder_Elapsed;
            arduino.Start();

        }

        void BtnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
           Save(TxtFileName.Text);

        }

        void BtnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddPoint(Convert.ToInt32(TxtLabel.Text));
            _recorder.Start();
        }

        void _recorder_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => AddPoint(Convert.ToInt32(TxtLabel.Text)));
            
        }

        void arduino_MessageReceived(object sender, Watch.Toolkit.Hardware.MessagesReceivedEventArgs e)
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
            _logger.AppendLine(_accelerometer.X + "," + _accelerometer.Y + "," + _accelerometer.Z + "," + label);
            ListBox.Items.Add(_accelerometer.ToString());

        }

        public void Save(string name)
        {
            _recorder.Stop();
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory +"/Logs/"+ name+".log", _logger.ToString());
            _logger.Clear();
            ListBox.Items.Clear();
        }
        public void Compute()
        {
            var data = new DataTable("Mitchell's Tennis Example");
            data.Columns.Add("x", typeof(double));
            data.Columns.Add("y", typeof(double));
            data.Columns.Add("z", typeof(double));

            var codebook = new Codification(data);

            DecisionVariable[] attributes =
            {
                new DecisionVariable("Outlook",   3), // 3 possible values (Sunny, overcast, rain)
                new DecisionVariable("Temperature", 3), // 3 possible values (Hot, mild, cool)  
                new DecisionVariable("Humidity",    2), // 2 possible values (High, normal)    
                new DecisionVariable("Wind",        2)  // 2 possible values (Weak, strong) 
            };

            var classCount = 2; // 2 possible output values for playing tennis: yes or no

            var tree = new DecisionTree(attributes, classCount);

            var id3Learning = new ID3Learning(tree);

            // Translate our training data into integer symbols using our codebook:
            DataTable symbols = codebook.Apply(data);
            int[][] inputs = symbols.ToIntArray("Outlook", "Temperature", "Humidity", "Wind");
            int[] outputs = symbols.ToIntArray("PlayTennis").GetColumn(0);

            // Learn the training instances!
            id3Learning.Run(inputs, outputs);

            // Convert to an expression tree
            var expression = tree.ToExpression();

            // Compiles the expression to IL
            var func = expression.Compile();
        }
    }
}
