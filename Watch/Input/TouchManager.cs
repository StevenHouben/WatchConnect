using System;

namespace Watch.Input.Sensors
{
    public class TouchManager
    {
        public event EventHandler<TouchEventArgs> TouchEventHandler;
        public void Start()
        {

        }
        public void Stop()
        {

        }
        protected void OnTouchEventHandler(TouchEventArgs e)
        {
            if (TouchEventHandler != null)
                TouchEventHandler(this, e);
        }
    }
    public class TouchEventArgs:EventArgs
    {
        public Touch Touch{get;set;}
        public Location Location{get;set;}
        public TouchEventArgs(Touch touch, Location loc)
        {
            Touch = touch;
            Location = loc;
        }
    }
    public enum Location
    {
        Top,
        Left,
        Bottom,
        Right
    }
    public enum Touch
    {
        Bevel,
        Screen,
        Strap
    }
}
