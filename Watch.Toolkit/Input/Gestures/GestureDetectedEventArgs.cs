using System;

namespace Watch.Toolkit.Input.Gestures
{
    public class GestureDetectedEventArgs : EventArgs
    {
        public Gesture Gesture { get; set; }

        public GestureDetectedEventArgs(Gesture detectedGesture)
        {
            Gesture = detectedGesture;
        }
    }
}
