using System;

namespace Watch.Toolkit.Hardware
{
     public class MessagesReceivedEventArgs:EventArgs
    {
        public string Message { get; set; }
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }

        public MessagesReceivedEventArgs(int id, string message)
        {
            TimeStamp = DateTime.UtcNow;
            Message = message;
            Id = id;
        }
    }
}
