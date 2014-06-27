
namespace Watch.Toolkit.Hardware.Arduino
{
    public class Arduino : AbstractHardwarePlatform
    {
        private ArduinoDriver _arduinoDriver;

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
        }
    }
}
