
using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Hardware.Arduino
{
    public class Arduino : AbstractHardwarePlatform
    {

        private ArduinoDriver _arduinoDriver;


        private readonly Dictionary<string, Func<string, bool>> _events
            = new Dictionary<string, Func<string, bool>>();
        private readonly Dictionary<string, Func<string, DataPacket>> _callbacks
            = new Dictionary<string, Func<string, DataPacket>>();

        public void AddPacketListener(string name, Func<string, bool> predicate, Func<string, DataPacket> callback)
        {
            _events.Add(name, predicate);
            _callbacks.Add(name,callback);
        }

        public void RemoveEvent(string name)
        {
            _events.Remove(name);
            _callbacks.Remove(name);
        }
        public void Start()
        {
            _arduinoDriver = ArduinoManager.Arduino;
            _arduinoDriver.MessageReceived += _arduinoDriver_MessageReceived;
        }
        public void Stop()
        {
            _arduinoDriver.MessageReceived -= _arduinoDriver_MessageReceived;
        }
        void _arduinoDriver_MessageReceived(object sender, MessagesReceivedEventArgs e)
        {
            OnMessageReceived(sender,e);

            foreach (var ev in _events.Where(ev => ev.Value(e.Message)))
            {
                OnDataPacketReceived(this, 
                    new DataPacketReceivedEventArgs(e.Id,_callbacks[ev.Key](e.Message)));
            }
        }
    }
}
