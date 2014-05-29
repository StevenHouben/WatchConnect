using System;

namespace Watch.Toolkit.Hardware
{
    public class AnalogDataReceivedEventArgs:EventArgs
    {
        public double Value { get; set; }
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }

        public AnalogDataReceivedEventArgs(int id, double value)
        {
            TimeStamp = DateTime.UtcNow;
            Value = value;
            Id = id;
        }
    }
}
