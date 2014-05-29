using System;

namespace Watch.Toolkit.Input.Touch
{
    public class MultiBevelTouchEventArgs : EventArgs
    {
        public bool BevelTop { get; set; }
        public bool BevelRight { get; set; }
        public bool BevelLeft { get; set; }
        public bool BevelBottom { get; set; }

        public MultiBevelTouchEventArgs(bool bevelTop, bool bevelRight, bool bevelLeft, bool bevelBottom)
        {
            BevelTop = bevelTop;
            BevelRight = bevelRight;
            BevelLeft = bevelLeft;
            BevelBottom = bevelBottom;
        }
    }
}
