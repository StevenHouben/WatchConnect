using System;

namespace Watch.Input.Sensors
{
    public interface ISensor
    {
        string Name { get; set; }
        int Id { get; set; }
        double Value { get; set; }
    }
}
