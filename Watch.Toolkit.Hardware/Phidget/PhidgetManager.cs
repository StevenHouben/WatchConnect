using Phidgets;

namespace Watch.Toolkit.Hardware.Phidget
{
    internal class PhidgetManager
    {
        private static InterfaceKit _instance;

        public static InterfaceKit InterfaceKit 
        {
            get { return _instance ?? (_instance = new InterfaceKit()); }
        }
    }
}
