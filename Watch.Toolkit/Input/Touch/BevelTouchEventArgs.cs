using System;

namespace Watch.Toolkit.Input.Touch
{
    public class BevelTouchEventArgs : EventArgs
    {
        public BevelSide BevelSide { get; set; }
        public double Value { get; set; }

        public BevelTouchEventArgs(BevelSide bevelSide, double value)
        {
            BevelSide = bevelSide;
            Value = value;
        }
    }
}
