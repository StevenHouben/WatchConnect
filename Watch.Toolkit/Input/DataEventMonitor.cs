using System;
using System.Timers;

namespace Watch.Toolkit.Input
{
    public class DataEventMonitor<T> : Timer
    {
        public event EventHandler<DataTriggeredEventArgs<T>> MonitorTriggered = delegate { };
        public bool Trigger { get; set; }

        public int Counter { get; set; }
        public T Data { get; set; }
        public int Id { get; set; }

        public DataEventMonitor(int time, int id, T data)
            : base(time)
        {
            Elapsed += EventMonitor_Elapsed;
            Id = id;
            Data = data;
        }

        void EventMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trigger = true;
            MonitorTriggered(this, new DataTriggeredEventArgs<T> { Id = Id, Trigger = Trigger,Data = Data});
        }
    }
    public class DataTriggeredEventArgs<T> : EventArgs
    {
        public bool Trigger { get; set; }
        public int Id { get; set; }
        public T Data { get; set; }
    }
}
