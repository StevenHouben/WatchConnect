using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using AForge;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Sensors;

namespace Development.DecisionTreeTrainer
{
    class Program
    {
        private static readonly string _logDirectory = AppDomain.CurrentDomain.BaseDirectory + "/Logs/recording2.log";

        private static DecisionTree _tree;
        private static Func<double[], int> _classifier;
        static void Main(string[] args)
        {
            Console.WriteLine("Reading training data from csv file.");
            var dataTable = BuildTableFromLogs();
            Console.WriteLine("Training algorithm");
            Compute(dataTable);
            Console.WriteLine("Starting detector");
            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start("COM4");
            while (true) ;
        }

        static void arduino_MessageReceived(object sender, Watch.Toolkit.Hardware.MessagesReceivedEventArgs e)
        {
            try
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

                var a = _classifier(accelerometerData.RawData);
                if(a >=0)
                   Console.WriteLine(a);

            }
            catch (Exception)
            {
                Console.WriteLine("");
            }
        }
        private static DataTable BuildTableFromLogs()
        {
            var filename = _logDirectory + "recording2.log";
            var reader = File.ReadAllLines(filename);

            var data = new DataTable();

            //this assume the first record is filled with the column names
            var headers = reader.First().Split(',');
            foreach (var header in headers)
                data.Columns.Add(header);

            var records = reader.Skip(1);
            foreach (var record in records)
                data.Rows.Add(record.Split(','));
            return data;
        }
        public static void Compute(DataTable data)
        {
            var classCount = 3;
            DecisionVariable[] attributes =
            {
                new DecisionVariable("X",new IntRange(-2000,2000)), 
                new DecisionVariable("Y",new IntRange(-2000,2000)),
                new DecisionVariable("Z",new IntRange(-2000,2000)) 
            };

            _tree = new DecisionTree(attributes, classCount);

            var id3Learning = new C45Learning(_tree);

            // Translate our training data into integer symbols using our codebook:
            //var symbols = _codebook.Apply(data);
            var inputs = data.ToArray<double>("X", "Y", "Z");
            var outputs = data.ToIntArray("LABEL").GetColumn(0);

            // Learn the training instances!
            var a= id3Learning.Run(inputs, outputs);

            // Convert to an expression tree
            var expression = _tree.ToExpression();

            // Compiles the expression to IL
            _classifier = expression.Compile();

        }
    }
}
