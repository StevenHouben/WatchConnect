using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Hardware
{
    public abstract class HardwarePlatform
    {
        public bool IsRunning { get; set; }

        public event EventHandler<AnalogDataReceivedEventArgs> AnalogDataReceived = delegate { };
        public event EventHandler<DigitalDataReivedHandler> DigitalInReceived = delegate { };
        public event EventHandler<DigitalDataReivedHandler> DigitalOutReceived = delegate { };
        public event EventHandler<MessagesReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<DataPacketReceivedEventArgs> DataPacketReceived = delegate { };

        private readonly Dictionary<Guid, Func<string, bool>> _events
            = new Dictionary<Guid, Func<string, bool>>();
        private readonly Dictionary<Guid, Func<string, DataPacket>> _callbacks
            = new Dictionary<Guid, Func<string, DataPacket>>();

        public abstract void Start();
        public abstract void Stop();

        protected HardwarePlatform()
        {
            MessageReceived += AbstractHardwarePlatform_MessageReceived;
        }

        void AbstractHardwarePlatform_MessageReceived(object sender, MessagesReceivedEventArgs e)
        {
            foreach (var ev in _events.ToList().Where(ev => ev.Value(e.Message)))
            {
                OnDataPacketReceived(this,
                    new DataPacketReceivedEventArgs(e.Id, _callbacks[ev.Key](e.Message)));
            }
        }

        public Guid AddPacketListener(string name, Func<string, bool> predicate, Func<string, DataPacket> callback)
        {
            var id = Guid.NewGuid();
            _events.Add(id, predicate);
            _callbacks.Add(id, callback);

            return id;
        }

        public void RemoveEvent(Guid id)
        {
            _events.Remove(id);
            _callbacks.Remove(id);
        }

        protected void OnMessageReceived(object sender, MessagesReceivedEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(sender, e);
        }

        protected void OnDataPacketReceived(object sender, DataPacketReceivedEventArgs e)
        {
            if (DataPacketReceived != null)
                DataPacketReceived(sender, e);
        }
        protected void OnAnalogDataReceived(object sender, AnalogDataReceivedEventArgs e)
        {
            if (AnalogDataReceived != null)
                AnalogDataReceived(sender, e);
        }

        protected void OnDigitalInReceived(object sender, DigitalDataReivedHandler e)
        {
            if (DigitalInReceived != null)
                DigitalInReceived(sender, e);
        }

        protected void OnDigitalOutReceived(object sender, DigitalDataReivedHandler e)
        {
            if (DigitalOutReceived != null)
                DigitalOutReceived(sender, e);
        }
    } 
}
