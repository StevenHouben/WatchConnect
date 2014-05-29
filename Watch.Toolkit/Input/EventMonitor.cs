using System.Timers;

namespace Watch.Toolkit.Input
{
    public class EventMonitor:Timer
    {
        public bool Trigger { get; set; }

        public EventMonitor(int time):base(time)
        {
            
        }
    }
}
