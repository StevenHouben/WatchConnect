namespace Watch.Toolkit.Processing.Dtw.Preprocessing
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
