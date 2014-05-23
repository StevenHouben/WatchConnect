using System;
using Watch.Toolkit.Input;

namespace Watch.Toolkit.Sensors
{
    public abstract class AbstractGestureManager
    {
        public event EventHandler<GestureDetectedEventArgs> GestureDetected;
        public event EventHandler<RawSensorDataReceivedEventArgs> RawDataReceived;
        public event EventHandler<GestureDetectedEventArgs> SwipeLeft;
        public event EventHandler<GestureDetectedEventArgs> SwipeRight;
        public event EventHandler<GestureDetectedEventArgs> HoverLeft;
        public event EventHandler<GestureDetectedEventArgs> HoverRight;
        public event EventHandler<GestureDetectedEventArgs> Glance;
        public event EventHandler<GestureDetectedEventArgs> Cover;


        public abstract void Start();

        protected void OnRawDataHandler(RawSensorDataReceivedEventArgs e)
        {
            if (RawDataReceived != null)
                RawDataReceived(this, e);
        }
        protected void OnGestureHandler(GestureDetectedEventArgs ge)
        {
            if (GestureDetected != null)
                GestureDetected(this, ge);
        }

        protected void OnSwipeLeftHandler(GestureDetectedEventArgs ge)
        {
            if (SwipeLeft != null)
                SwipeLeft(this, ge);
        }
        protected void OnSwipeRightHandler(GestureDetectedEventArgs ge)
        {
            if (SwipeRight != null)
                SwipeRight(this, ge);
        }
        protected void OnHoverLeftHandler(GestureDetectedEventArgs ge)
        {
            if (HoverLeft != null)
                HoverLeft(this, ge);
        }
        protected void OnHoverRightHandler(GestureDetectedEventArgs ge)
        {
            if (HoverRight != null)
                HoverRight(this, ge);
        }
        protected void OnCoverHandler(GestureDetectedEventArgs ge)
        {
            if (Cover != null)
                Cover(this, ge);
        }
        protected void OnGlanceHandler(GestureDetectedEventArgs ge)
        {
            if (Glance != null)
                Glance(this, ge);
        }

    }
}
