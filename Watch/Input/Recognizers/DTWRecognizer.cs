using System;
using System.Collections.Generic;
using Watch.Input.Sensors.Dtw;

namespace Watch.Input.Recognizers
{
    public class DtwRecognizer
    {
        readonly Dictionary<string,double[]> _templates = new Dictionary<string, double[]>(); 
        public void AddTemplate(double[] template, string label)
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
                Console.WriteLine(template.Key + " -> "+ newCost);
                if (!(newCost < cost)) continue;
                cost = newCost;
                label = template.Key;
            }
            return label;
        }
    }
}
