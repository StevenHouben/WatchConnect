using System;

namespace Watch.Toolkit.Hardware
{
    public class DataPacketReceivedEventArgs : EventArgs
    {
        public DataPacket DataPacket { get; set; }
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }

        public DataPacketReceivedEventArgs(int id, DataPacket data)
        {
            TimeStamp = DateTime.UtcNow;
            DataPacket = data;
            Id = id;
        }
    }
}
