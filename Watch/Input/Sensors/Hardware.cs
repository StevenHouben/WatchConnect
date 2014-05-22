﻿using Phidgets;

namespace Watch.Input.Sensors
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
