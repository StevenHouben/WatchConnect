using System;
using System.Data;
using System.IO;
using System.Linq;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using AForge;

namespace Watch.Toolkit.Sensors.MachineLearning
{
    public class Classifier
    {
        private DecisionTree _tree;
        private readonly DataTable _data;
        private Func<double[], int> _classifier;
        public int Classes { get;private set; }

        public Classifier(string filePath,int classes)
        {
            Classes = classes;
            _data = BuildTableFromLogs(filePath);
        }

        public void Run(MachineLearningAlgorithm algorithm)
        {
            Compute(_data, algorithm);
        }

        public int ComputeLabel(double[] input)
        {
            return _classifier(input);
        }
        private DataTable BuildTableFromLogs(string filePath)
        {
            var filename = filePath;
            var reader = File.ReadAllLines(filename);

            var data = new DataTable();

            var headers = reader.First().Split(',');
            foreach (var header in headers)
                data.Columns.Add(header);

            var records = reader.Skip(1);
            foreach (var record in records.Where(record => record != null))
                data.Rows.Add(record.Split(','));
            return data;
        }
        private void Compute(DataTable data,MachineLearningAlgorithm algorithm)
        {
            DecisionVariable[] attributes =
            {
                new DecisionVariable("X",new IntRange(-2000,2000)), 
                new DecisionVariable("Y",new IntRange(-2000,2000)),
                new DecisionVariable("Z",new IntRange(-2000,2000)) 
            };

            _tree = new DecisionTree(attributes, Classes);


            switch (algorithm)
            {
                case MachineLearningAlgorithm.C45:
                {
                    var learning = new C45Learning(_tree);
                    var inputs = data.ToArray<double>("X", "Y", "Z");
                    var outputs = data.ToIntArray("LABEL").GetColumn(0);
                    learning.Run(inputs, outputs);
                    var expression = _tree.ToExpression();
                    _classifier = expression.Compile();
                }
                break;
                case MachineLearningAlgorithm.ID3:
                {
                    var learning = new ID3Learning(_tree);
                    var inputs = data.ToArray<int>("X", "Y", "Z");
                    var outputs = data.ToIntArray("LABEL").GetColumn(0);
                    learning.Run(inputs, outputs);
                    var expression = _tree.ToExpression();
                    _classifier = expression.Compile();
                }
                break;
            }
        }
    }

    public enum MachineLearningAlgorithm
    {
        C45,
        ID3
    }
}
