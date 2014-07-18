using System;

namespace Watch.Toolkit.Hardware
{
    public abstract class AbstractHardwarePlatform
    {
        public bool IsRunning { get; set; }

        public event EventHandler<AnalogDataReceivedEventArgs> AnalogDataReceived = delegate { };
        public event EventHandler<DigitalDataReivedHandler> DigitalInReceived = delegate { };
        public event EventHandler<DigitalDataReivedHandler> DigitalOutReceived = delegate { };
        public event EventHandler<MessagesReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<DataPacketReceivedEventArgs> DataPacketReceived = delegate { };

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
