using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NativeTouchSupport
{
    public class TouchWindow : Window
    {
        public TouchWindow()
        {
            Helper.DisableStylusDevice();

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source == null) return;
            source.AddHook(WndProc);

            Native.RegisterTouchWindow(source.Handle, 0x00000001);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Native.WM_TOUCH:
                    DecodeTouch(wParam, lParam);
                    break;
            }
            return IntPtr.Zero;
        }

        private void DecodeTouch(IntPtr wParam, IntPtr lParam)
        {
            var inputCount = unchecked((short)wParam.ToInt32());
            var inputs = new Structs.TOUCHINPUT[inputCount];

            if (!Native.GetTouchInputInfo(lParam, inputCount, inputs, Marshal.SizeOf(new Structs.TOUCHINPUT())))
                return;

            foreach (var touchinput in inputs)
            {
                if ((touchinput.dwFlags & Native.TOUCHEVENTF_DOWN) != 0)
                {
                    NativeTouchDown(this, new NativeTouchEventArgs
                    {
                        Id = touchinput.dwID,
                        Time = touchinput.dwTime,
                        Height = touchinput.cyContact /100,
                        Width = touchinput.cxContact / 100,
                        X = touchinput.x / 100,
                        Y = touchinput.y / 100
                    });
                }
                else if ((touchinput.dwFlags & Native.TOUCHEVENTF_UP) != 0)
                {
                    NativeTouchUp(this, new NativeTouchEventArgs
                    {
                        Id = touchinput.dwID,
                        Time = touchinput.dwTime,
                        Height = touchinput.cyContact / 100,
                        Width = touchinput.cxContact / 100,
                        X = touchinput.x / 100,
                        Y = touchinput.y / 100
                    });
                }
                else if ((touchinput.dwFlags & Native.TOUCHEVENTF_MOVE) != 0)
                {
                    NativeTouchMove(this, new NativeTouchEventArgs
                    {
                        
                        Id = touchinput.dwID,
                        Time = touchinput.dwTime,
                        Height = touchinput.cyContact / 100,
                        Width = touchinput.cxContact / 100,
                        X = touchinput.x / 100,
                        Y = touchinput.y / 100
                    });
                }
            }
            Native.CloseTouchInputHandle(lParam);
        }

        public event EventHandler<NativeTouchEventArgs> NativeTouchDown = delegate { };
        public event EventHandler<NativeTouchEventArgs> NativeTouchUp = delegate { };
        public event EventHandler<NativeTouchEventArgs> NativeTouchMove = delegate { };

        
    }
}
