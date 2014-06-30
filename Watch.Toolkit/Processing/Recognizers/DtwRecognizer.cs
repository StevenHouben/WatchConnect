using System;
using System.Collections.Generic;

namespace Watch.Toolkit.Processing.Recognizers
{
    public class DtwRecognizer
    {
        public Dictionary<string, double[]> Templates
        {
            get { return _templates; }
        }

        readonly Dictionary<string,double[]> _templates = new Dictionary<string, double[]>(); 
        readonly object _readlock = new object();
        public void AddTemplate(string label,double[] template)
        {
            lock (_readlock)
            {
                if (_templates.ContainsKey(label))
                    _templates[label] = template;
                else
                    _templates.Add(label,template);
            }

        }

        public void RemoveTemplate(string label)
        {
            lock (_readlock)
                _templates.Remove(label);
        }

        public string ComputeClosestLabel(double[] rawData)
        {
            lock (_readlock)
            {
                var label = "";
                var cost = Double.MaxValue;
                foreach (var template in _templates)
                {
                    var newCost = new Dtw.Dtw(template.Value, rawData).GetCost();
                    if (!(newCost < cost)) continue;
                    cost = newCost;
                    label = template.Key;
                }
                return label;
            } 
        }
        public Tuple<string,Dictionary<string,double>> ComputeClosestLabelAndCosts(double[] rawData)
        {
            lock (_readlock)
            {
                var label = "";
                var cost = Double.MaxValue;
                var costs = new Dictionary<string, double>();
                foreach (var template in _templates)
                {
                    var newCost = new Dtw.Dtw(template.Value, rawData).GetCost();
                    costs.Add(template.Key, newCost);
                    if (!(newCost < cost)) continue;
                    cost = newCost;
                    label = template.Key;
                }
                return new Tuple<string, Dictionary<string, double>>(label, costs);
            }

        }
        public Tuple<string,double> ComputerClosestLabelAndCost(double[] rawData)
        {
            lock (_readlock)
            {
                var label = "";
                var cost = Double.MaxValue;
                foreach (var template in _templates)
                {
                    var newCost = new Dtw.Dtw(template.Value, rawData).GetCost();
                    if (!(newCost < cost)) continue;
                    cost = newCost;
                    label = template.Key;
                }
                return new Tuple<string, double>(label, cost);
            }
        }
    }
}
