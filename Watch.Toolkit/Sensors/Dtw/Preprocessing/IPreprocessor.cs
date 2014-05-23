namespace Watch.Toolkit.Sensors.Dtw.Preprocessing
{
    public interface IPreprocessor
    {
        double[] Preprocess(double[] data);
        string ToString();
    }
}