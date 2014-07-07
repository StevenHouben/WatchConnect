using System;
using System.Collections.Generic;
using System.Data;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using AForge;

namespace Watch.Toolkit.Processing.MachineLearning
{
    public class TreeClassifier
    {
        private DecisionTree _tree;
        private readonly DataTable _data;
        private Func<double[], int> _classifier;

        private readonly int _classes;
        private readonly List<string> _classLabels; 

        public TreeClassifier(ClassifierConfiguration configuration)
        {
            _data = Helper.ReadCsvToDataTable(configuration.TrainingDataPath,' ');
            _classes = configuration.Labels.Count;
            _classLabels = configuration.Labels;
        }

        public void Run(MachineLearningAlgorithm algorithm)
        {
            Compute(_data, algorithm);
        }

        public int ComputeValue(double[] input)
        {
            return _classifier(input);
        }
        public string ComputeLabel(double[] input)
        {
            var res = _classifier(input);
            return (res == -1) ? "none" : _classLabels[res];
        }

        private void Compute(DataTable data,MachineLearningAlgorithm algorithm)
        {
            DecisionVariable[] attributes =
            {
                new DecisionVariable("X",new DoubleRange(-180,180)), 
                new DecisionVariable("Y",new DoubleRange(-180,180)),
                new DecisionVariable("Z",new DoubleRange(-180,180)) 
            };

            _tree = new DecisionTree(attributes, _classes);


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
                case MachineLearningAlgorithm.Id3:
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
        Id3
    }
}
