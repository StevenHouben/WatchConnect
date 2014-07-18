using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Sensors
{
    public abstract class TouchSensor:ISensor
    {
        public event EventHandler TouchSensorUpdated = delegate { };
        public event EventHandler<String> EventTriggered = delegate { };
        private readonly Dictionary<string, Func<TouchSensor, bool>> _events = new Dictionary<string, Func<TouchSensor, bool>>();

        public void AddEvent(string name, Func<TouchSensor, bool> condition)
        {
            _events.Add(name, condition);
        }

        public void RemoveEvent(string name)
        {
            _events.Remove(name);
        }
        public string Name { get; set; }

        public int Id { get; set; }

        protected double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                TouchSensorUpdated(this, new EventArgs());
                ProcessSensorData();
                foreach (var ev in _events.ToList().Where(ev => ev.Value(this)).Where(ev => EventTriggered != null))
                {
                    EventTriggered(this, ev.Key);
                }
            }
        }

        public bool Down { get; set; }

        protected abstract void ProcessSensorData();

    }
}
