using System.Reflection;
using System.Windows.Input;

namespace NativeTouchSupport
{
    public class Helper
    {
       
        public static void DisableStylusDevice()
        {
            var devices = Tablet.TabletDevices;

            if (devices.Count <= 0) return;
            var inputManagerType = typeof(InputManager);

            var stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, InputManager.Current, null);

            if (stylusLogic == null) return;

            var stylusLogicType = stylusLogic.GetType();

            while (devices.Count > 0)
            {
                stylusLogicType.InvokeMember("OnTabletRemoved",
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, stylusLogic, new object[] { (uint)0 });
            }
        }
    }
}
