using System;
using System.Collections.Generic;
using Watch.Toolkit.Sensors.Dtw;

namespace Watch.Toolkit.Input.Recognizers
{
    public class DtwRecognizer
    {
        readonly Dictionary<string,double[]> _templates = new Dictionary<string, double[]>(); 
        public void AddTemplate(string label,double[] template)
        {
            _templates.Add(label,template);
        }

        public void RemoveTemplate(string label)
        {
            _templates.Remove(label);
        }

        public string FindClosestLabel(double[] rawData)
        {
            var label = "";
            var cost = Double.MaxValue;
            foreach (var template in _templates)
            {
                var newCost = new Dtw(template.Value, rawData).GetCost();
                if (!(newCost < cost)) continue;
                cost = newCost;
                label = template.Key;
            }
            return label;
        }
        public Tuple<string,double> FindClosestLabelAndCost(double[] rawData)
        {
            var label = "";
            var cost = Double.MaxValue;
            foreach (var template in _templates)
            {
                var newCost = new Dtw(template.Value, rawData).GetCost();
                if (!(newCost < cost)) continue;
                cost = newCost;
                label = template.Key;
            }
            return new Tuple<string, double>(label,cost);
        }
    }
}
