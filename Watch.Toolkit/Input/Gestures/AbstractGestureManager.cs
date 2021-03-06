﻿using System;
using Watch.Toolkit.Sensors;

namespace Watch.Toolkit.Input.Gestures
{
    public abstract class AbstractGestureManager : IInputManager
    {
        public event EventHandler<GestureDetectedEventArgs> GestureDetected;
        public event EventHandler<RawSensorDataReceivedEventArgs> RawDataReceived;
        public event EventHandler<GestureDetectedEventArgs> SwipeLeft;
        public event EventHandler<GestureDetectedEventArgs> SwipeRight;
        public event EventHandler<GestureDetectedEventArgs> HoverLeft;
        public event EventHandler<GestureDetectedEventArgs> HoverRight;
        public event EventHandler<GestureDetectedEventArgs> Glance;
        public event EventHandler<GestureDetectedEventArgs> Cover;

        public void SimulateEvent(GestureEvents ev, EventArgs e)
        {
            switch (ev)
            {
                case GestureEvents.Cover:
                    OnCoverHandler((GestureDetectedEventArgs)e);
                    break;
                case GestureEvents.GestureDetected:
                    OnGestureHandler((GestureDetectedEventArgs)e);
                    break;
                case GestureEvents.Glance:
                    OnGlanceHandler((GestureDetectedEventArgs)e);
                    break;
                case GestureEvents.HoverLeft:
                    OnHoverLeftHandler((GestureDetectedEventArgs)e);
                    break;
                case GestureEvents.HoverRight:
                    OnHoverRightHandler((GestureDetectedEventArgs)e);
                    break;
                case GestureEvents.RawDataReceived:
                    OnRawDataHandler((RawSensorDataReceivedEventArgs)e);
                    break;
                case GestureEvents.SwipeLeft:
                    OnSwipeLeftHandler((GestureDetectedEventArgs)e);
                    break;
                case GestureEvents.SwipeRight:
                    OnSwipeRightHandler((GestureDetectedEventArgs)e);
                    break;
            }
        }

        public abstract void Start();
        public abstract void Stop();

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
