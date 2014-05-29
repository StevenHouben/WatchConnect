using System;

namespace Watch.Toolkit.Hardware
{
    public class DigitalDataReivedHandler:EventArgs
    {
        public bool Value { get; set; }
        public int Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public DigitalDataReivedHandler(int id, bool value)
        {
            TimeStamp = DateTime.UtcNow;
            Value = value;
            Id = id;
        }
    }
}
