using System.Runtime.InteropServices;

namespace NativeTouchSupport.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTS
    {
        public short x;
        public short y;
    }
}
