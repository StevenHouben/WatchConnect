using System;

namespace Watch.Toolkit.Input.Touch
{
    public class MultiBevelTouchEventArgs : EventArgs
    {
        public BevelState BevelState { get; set; }

        public MultiBevelTouchEventArgs(BevelState bevelState)
        {
            BevelState = bevelState;
        }
    }
}
