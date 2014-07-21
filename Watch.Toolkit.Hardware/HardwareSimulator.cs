using System;
using System.Linq.Expressions;

namespace Watch.Toolkit.Hardware
{
    public class HardwareSimulator:HardwarePlatform
    {
        public override void Start()
        {
           
        }

        public override void Stop()
        {
           
        }

        public void SendPackage(string message)
        {
            OnMessageReceived(this, new MessagesReceivedEventArgs(-1,message));
        }
    }
}
