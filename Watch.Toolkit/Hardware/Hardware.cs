using Phidgets;

namespace Watch.Toolkit.Hardware
{
    public class Hardware
    {
        private static InterfaceKit _instance;

        public static InterfaceKit InterfaceKit 
        {
            get { return _instance ?? (_instance = new InterfaceKit()); }
        }
    }
}
