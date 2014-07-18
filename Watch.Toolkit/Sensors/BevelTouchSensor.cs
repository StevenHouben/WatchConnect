using System;
using System.Collections.Generic;
using System.Linq;
using Watch.Toolkit.Input.Touch;

namespace Watch.Toolkit.Sensors
{
    public class BevelTouchSensor
    {
        private BevelState _touchStates;

        public BevelTouchSensor()
        {
            _touchStates= new BevelState();
        }
        public BevelState TouchStates
        {
            get { return _touchStates; }
            set
            {
                _touchStates = value;
                TouchSensorUpdated(this, new EventArgs());
                foreach (var ev in _events.ToList().Where(ev => ev.Value(this)).Where(ev => EventTriggered != null))
                {
                    EventTriggered(this, ev.Key);
                }
            }
        }
        public event EventHandler TouchSensorUpdated = delegate { };
        public event EventHandler<String> EventTriggered = delegate { };
        private readonly Dictionary<string, Func<BevelTouchSensor, bool>> _events = new Dictionary<string, Func<BevelTouchSensor, bool>>();

        public void AddEvent(string name, Func<BevelTouchSensor, bool> predicate)
        {
            _events.Add(name, predicate);
        }

        public void RemoveEvent(string name)
        {
            _events.Remove(name);
        }
        public string Name { get; set; }

        public int Id { get; set; }
    }
}
