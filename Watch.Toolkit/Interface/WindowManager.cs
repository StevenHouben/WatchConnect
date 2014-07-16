using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace Watch.Toolkit.Interface
{
    static public class WindowManager
    {
        public static void MaximizeToSecondaryMonitor(Window window)
        {
            var secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);

            if (secondaryScreen == null) return;
            if (!window.IsLoaded)
                window.WindowStartupLocation = WindowStartupLocation.Manual;

            var workingArea = secondaryScreen.WorkingArea;
            window.Left = workingArea.Left;
            window.Top = workingArea.Top;
            window.Width = workingArea.Width;
            window.Height = workingArea.Height;
            if ( window.IsLoaded )
                window.WindowState = WindowState.Maximized;
        }

        public static  bool HasWatchConnected()
        {
            return Screen.AllScreens.FirstOrDefault(s => !s.Primary) != null;
        }

        public static void Dock(Window window,DockLocation location)
        {
            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.ToolWindow;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            switch (location)
            {
                case DockLocation.Left:
                    window.Width = SystemParameters.WorkArea.Width / 2d;
                    window.Height = SystemParameters.WorkArea.Height;
                    window.Left = 0;
                    window.Top = 0;
                    break;
                case DockLocation.Right:
                    window.Width = SystemParameters.WorkArea.Width / 2d;
                    window.Height = SystemParameters.WorkArea.Height;
                    window.Left = SystemParameters.WorkArea.Width / 2d;
                    window.Top = 0;
                    break;
                case DockLocation.Top:
                    window.Width = SystemParameters.WorkArea.Width;
                    window.Height = SystemParameters.WorkArea.Height / 2d;
                    window.Left = 0;
                    window.Top = 0;
                    break;
                case DockLocation.Bottom:
                    window.Width = SystemParameters.WorkArea.Width;
                    window.Height = SystemParameters.WorkArea.Height / 2d;
                    window.Left = 0;
                    window.Top = SystemParameters.WorkArea.Height / 2d;
                    break;
                case DockLocation.Fill:
                    window.WindowState = WindowState.Maximized;
                    break;


            }
            
        }

        public enum DockLocation
        {
            Left,
            Right,
            Top,
            Bottom,
            Fill
        }
    }
}