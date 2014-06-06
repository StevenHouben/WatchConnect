using System;
using System.Runtime.InteropServices;
using NativeTouchSupport.Structs;

namespace NativeTouchSupport
{
    public static class Native
    {
        public const int WM_TOUCH = 0x0240;
        public const int TOUCHEVENTF_MOVE = 0x0001;
        public const int TOUCHEVENTF_DOWN = 0x0002;
        public const int TOUCHEVENTF_UP = 0x0004;
        public const int TOUCHEVENTF_INRANGE = 0x0008;
        public const int TOUCHEVENTF_PRIMARY = 0x0010;
        public const int TOUCHEVENTF_NOCOALESCE = 0x0020;
        public const int TOUCHEVENTF_PEN = 0x0040;
        public const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 0x0001; 
        public const int TOUCHINPUTMASKF_EXTRAINFO = 0x0002; 
        public const int TOUCHINPUTMASKF_CONTACTAREA = 0x0004;



        

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterTouchWindow(System.IntPtr hWnd, uint ulFlags);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTouchInputInfo(System.IntPtr hTouchInput, int cInputs,
            [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern void CloseTouchInputHandle(System.IntPtr lParam);
    }
}
