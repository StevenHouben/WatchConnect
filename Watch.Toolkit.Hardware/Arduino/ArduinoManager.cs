namespace Watch.Toolkit.Hardware.Arduino
{
    class ArduinoManager:AbstractHardwarePlatform
    {
        private static ArduinoDriver _instance;

        public static ArduinoDriver Arduino 
        {
            //Todo -> move the port name to configuration class
            get { return _instance ?? (_instance = new ArduinoDriver("COM4")); }
        }
    }
}
