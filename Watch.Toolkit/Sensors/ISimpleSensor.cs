namespace Watch.Toolkit.Sensors
{
    public interface ISimpleSensor
    {
        string Name { get; set; }
        int Id { get; set; }
        double Value { get; set; }
    }
}
