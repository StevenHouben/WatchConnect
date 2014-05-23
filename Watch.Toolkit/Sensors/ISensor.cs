using System;

namespace Watch.Toolkit.Sensors
{
    public interface ISensor
    {
        string Name { get; set; }
        int Id { get; set; }
        double Value { get; set; }
    }
}
