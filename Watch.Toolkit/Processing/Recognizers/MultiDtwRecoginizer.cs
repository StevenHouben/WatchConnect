using System;
using System.Collections.Generic;
using Watch.Toolkit.Processing.Dtw;

namespace Watch.Toolkit.Processing.Recognizers
{
    public class MultiDtwRecoginizer
    {
        readonly Dictionary<string, SeriesVariable> _templates = new Dictionary<string, SeriesVariable>();

        public void AddTemplate(string label, double[] x, double[] y)
        {
            _templates.Add(label,new SeriesVariable(x,y));
        }

        public void RemoveTemplate(string label)
        {
            _templates.Remove(label);
        }

        public string FindClosestLabel(double[] x, double[] y)
        {

            var label = "";
            var cost = Double.MaxValue;
            foreach (var template in _templates)
            {
                var newCost = new Dtw.Dtw(new [] { template.Value, new SeriesVariable(x, y) }).GetCost();
                Console.WriteLine(template.Key + " -> "+ newCost);
                if (!(newCost < cost)) continue;
                cost = newCost;
                label = template.Key;
            }
            return label;
        }
    }
}
