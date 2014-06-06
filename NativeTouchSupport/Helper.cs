using System.Reflection;
using System.Windows.Input;

namespace NativeTouchSupport
{
    public class Helper
    {
        /// <summary>
        /// MSDN Code to allow for capture native Touch Messages
        /// http://msdn.microsoft.com/en-us/library/ee230087(v=vs.110).aspx
        /// </summary>
        public static void DisableWpfTabletSupport()
        {
            // Get a collection of the tablet devices for this window.  
            var devices = Tablet.TabletDevices;

            if (devices.Count <= 0) return;
            // Get the Type of InputManager.
            var inputManagerType = typeof(InputManager);

            // Call the StylusLogic method on the InputManager.Current instance.
            var stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, InputManager.Current, null);

            if (stylusLogic == null) return;

            //  Get the type of the stylusLogic returned from the call to StylusLogic.
            var stylusLogicType = stylusLogic.GetType();

            // Loop until there are no more devices to remove.
            while (devices.Count > 0)
            {
                // Remove the first tablet device in the devices collection.
                stylusLogicType.InvokeMember("OnTabletRemoved",
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, stylusLogic, new object[] { (uint)0 });
            }
        }
    }
}
