namespace Watch.Toolkit.Hardware.Arduino
{
    public class Arduino : HardwarePlatform
    {
        private ArduinoDriver _arduinoDriver;

        private readonly string _comPort;
        public Arduino(string comPort)
        {
            _comPort = comPort;
        }
        public override void Start()
        {
            _arduinoDriver = ArduinoManager.GetArduinoDriver(_comPort);
            _arduinoDriver.Start();
            _arduinoDriver.MessageReceived += _arduinoDriver_MessageReceived;
        }
        public override void Stop()
        {
            _arduinoDriver.MessageReceived -= _arduinoDriver_MessageReceived;
        }
        void _arduinoDriver_MessageReceived(object sender, MessagesReceivedEventArgs e)
        {
            OnMessageReceived(sender,e);
        }
    }
}
