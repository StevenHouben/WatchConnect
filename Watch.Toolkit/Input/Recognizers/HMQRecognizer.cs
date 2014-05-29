using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Input.Recognizers
{
    public class HmqRecognizer
    {
        public Monotonicity ComputerMonotonicity(double[] rawdata)
        {
            if (rawdata.Length == 0)
                throw new InvalidOperationException("data is empty");

            var li = new List<int>();
            var storage = new List<double>();

            foreach (var v in rawdata)
            {
                if (storage.Count == 0 || v > storage[storage.Count - 1])
                    li.Add(0);
                else
                    li.Add(1);
                storage.Add(v);
            }

            var output = li.GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

            return output == 1 ? Monotonicity.Increasing : Monotonicity.Decreasing;
        }
    }

    public enum Monotonicity
    {
        Increasing,
        Decreasing,
        StrictlyIncreasing,
        StrictlyDecreasing
    }
}
