using Watch.Toolkit;

namespace Watch.Examples.Desktop
{
    public partial class DesktopWindow
    {
        readonly WatchWindow _watchWindow = new WatchWindow();
        public DesktopWindow()
        {
            InitializeComponent();
            _watchWindow.GestureManager.GestureDetected += GestureManager_GestureDetected;
            _watchWindow.TrackerManager.TrackGestureRecognized += TrackerManager_TrackGestureRecognized;
        }

        void TrackerManager_TrackGestureRecognized(object sender, Toolkit.Input.Tracker.TrackGestureEventArgs e)
        {
            //handle the detected labels from ML
        }

        void GestureManager_GestureDetected(object sender, Toolkit.Input.Gestures.GestureDetectedEventArgs e)
        {
           //handle around the device gestures here
        }
    }
}
