using System.Linq;

namespace Watch.Input.Sensors.Dtw.Preprocessing
{
    public class NonePreprocessor : IPreprocessor
    {
        public double[] Preprocess(double[] data)
        {
            return data;
        }

        public override string ToString()
        {
            return "None";
        }
    }
}
