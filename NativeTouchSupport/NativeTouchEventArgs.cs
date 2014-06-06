using System;
namespace NativeTouchSupport
{
    public class NativeTouchEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Id { get; set; }
        public int Flags { get; set; }
        public int Mask { get; set; }
        public int Time { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPrimaryContact
        {
            get { return (Flags & Native.TOUCHEVENTF_PRIMARY) != 0; }
        }
    }
}
