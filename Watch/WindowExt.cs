using System.Linq;
using System.Windows;

namespace Watch
{
    static public class WindowExt
    {
        public static void MaximizeToSecondaryMonitor(Window window)
        {
            var secondaryScreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(s => !s.Primary);

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
    }
}