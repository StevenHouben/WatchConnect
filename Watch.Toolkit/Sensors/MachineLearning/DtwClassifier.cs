using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Watch.Toolkit.Input.Recognizers;

namespace Watch.Toolkit.Sensors.MachineLearning
{
    public class DtwClassifier
    {
        private readonly DataTable _data;
        private readonly DtwRecognizer _recognizer = new DtwRecognizer();
        private readonly int _classes;
        private readonly List<string> _classLabels; 

        public DtwClassifier(string filePath,int classes,List<string> classLabels)
        {
            _data = Helper.ReadCsvToDataTable(filePath);
            _classes = classes;
            _classLabels = classLabels;

        }

        public void AddTemplate(string label,double[] template)
        {
            _recognizer.AddTemplate(label,template);
        }
        public void Run()
        {
            GenerateTemlates();
        }

        public string ComputeLabel(double[] input)
        {
            return _recognizer.ComputeClosestLabel(input);
        }

        public Tuple<string,double> ComputeLabelAndCost(double[] input)
        {
            return _recognizer.ComputerClosestLabelAndCost(input);
        }

        public Tuple<string, Dictionary<string,double>> ComputeLabelAndCosts(double[] input)
        {
            return _recognizer.ComputeClosestLabelAndCosts(input);
        }

        private void GenerateTemlates()
        {
            for (var i = 0; i < _classes; i++)
            {
                double x = 0, y = 0, z = 0;
                var labeledData = _data.Select("LABEL =" + i).ToArray();
                foreach (var dataPoint in labeledData)
                {
                    x += Convert.ToDouble(dataPoint.ItemArray[0]);
                    y += Convert.ToDouble(dataPoint.ItemArray[1]);
                    z += Convert.ToDouble(dataPoint.ItemArray[2]);
                }

                _recognizer.AddTemplate(_classLabels[i],
                    new []
                {
                    x/labeledData.Length,
                    y/labeledData.Length,
                    z/labeledData.Length,
                });

            }
        }

    }
}
