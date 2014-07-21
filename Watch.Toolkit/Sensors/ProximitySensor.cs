using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Sensors
{
    public abstract class ProximitySensor:ISensor
    {
        public event EventHandler<RangeChangedEventArgs> RangeChanged;
        public event EventHandler ProximitySensorUpdated = delegate { };
        public event EventHandler<String> EventTriggered = delegate { };
        private readonly Dictionary<string, Func<ProximitySensor, bool>> _events = new Dictionary<string, Func<ProximitySensor, bool>>();

        public ProximitySensor AddEvent(string name, Func<ProximitySensor, bool> condition)
        {
            _events.Add(name, condition);
            return this;
        }

        public void RemoveEvent(string name)
        {
            _events.Remove(name);
        }
        
        public string Name { get; set; }

        public int Id { get; set; }

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ProximitySensorUpdated(this, new EventArgs());
                foreach (var ev in _events.ToList().Where(ev => ev.Value(this)).Where(ev => EventTriggered != null))
                {
                    EventTriggered(this, ev.Key);
                    Console.WriteLine("Triggered");
                }
                ProcessSensorData();
            }
        }
        public double Treshold { get; set; }

        public bool InRange
        {
            get { return CalculateRange(); }
        }

        protected void OnRangeChanged(RangeChangedEventArgs e)
        {
            if (RangeChanged != null)
                RangeChanged(this, e);
        }
        protected abstract bool CalculateRange();
        protected abstract void ProcessSensorData();
    }

    
}
