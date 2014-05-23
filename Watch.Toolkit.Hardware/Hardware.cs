using Phidgets;

namespace Watch.Toolkit.Hardware
{
    public class HardwarePlatform
    {
        private static InterfaceKit _instance;

        public static InterfaceKit InterfaceKit 
        {
            get { return _instance ?? (_instance = new InterfaceKit()); }
        }
    }
}
