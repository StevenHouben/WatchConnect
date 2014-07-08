using System;
using System.Timers;

namespace Watch.Toolkit.Input
{
    public class EventMonitor:Timer
    {
        public event EventHandler<TriggeredEventArgs> MonitorTriggered = delegate { }; 
        public bool Trigger { get; set; }
        public int Id { get; set; }

        public EventMonitor(int time,int id=-1):base(time)
        {
            Elapsed += EventMonitor_Elapsed;
            Id = id;
        }

        void EventMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trigger = true;
            MonitorTriggered(this, new TriggeredEventArgs{Id = Id, Trigger=Trigger});
        }
    }
    public class TriggeredEventArgs:EventArgs
    {
        public bool Trigger { get; set; }
        public int Id { get; set; }
    }
}
