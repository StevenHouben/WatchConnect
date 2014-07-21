namespace Watch.Toolkit.Hardware.Arduino
{
    class ArduinoManager
    {
        private static ArduinoDriver _instance;

        public static ArduinoDriver GetArduinoDriver(string port)
        {
            return _instance ?? (_instance = new ArduinoDriver(port));
        }
    }
}
