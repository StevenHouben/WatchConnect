namespace Watch.Toolkit.Hardware.Arduino
{
    class ArduinoManager:AbstractHardwarePlatform
    {
        private static Arduino _instance;

        public static Arduino InterfaceKit 
        {
            get { return _instance ?? (_instance = new Arduino()); }
        }
    }
}
