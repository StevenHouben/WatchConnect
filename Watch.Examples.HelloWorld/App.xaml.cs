using System;
using System.Windows;
using Watch.Toolkit;
using Watch.Toolkit.Input.Tracker;

namespace Watch.Examples.HelloWorld
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Create new watch window, this window will automatically appear on
            //the second monitor or watch.
            var watchWindow = new WatchWindow();

            //Attaching a new watchface will automatically will make it active.
            //A watchface is a UserControl component that inherits from
            //WatchVisual, which can be found in the toolkit
            watchWindow.AddWatchFace( new Toolkit.Interface.DefaultFaces.Clock());

            //Connect to the GestureManager and listen for any gestures. These events will
            //be triggered depending on the availability of the sensors.
            watchWindow.GestureManager.GestureDetected += GestureManager_Detected;
            watchWindow.GestureManager.Cover += GestureManager_Detected;
            watchWindow.GestureManager.Glance += GestureManager_Detected;
            watchWindow.GestureManager.HoverLeft += GestureManager_Detected;
            watchWindow.GestureManager.HoverRight += GestureManager_Detected;
            watchWindow.GestureManager.SwipeLeft += GestureManager_Detected;
            watchWindow.GestureManager.SwipeRight += GestureManager_Detected;

            //Connect to the TrackerManager and listen for any posture detection, by default
            //the tracker manager will use four postures, but by loadin a custom configuration
            //developers can determines their own labels and training data for the machine
            //learning component.
            watchWindow.TrackerManager.TrackGestureRecognized += TrackerManager_TrackGestureRecognized;
            watchWindow.TrackerManager.RawTrackGestureDataUpdated += TrackerManager_RawTrackGestureDataUpdated;

            //Connect to the TouchManager and listen for any touch events. These events will
            //be triggered depending on the availability of the sensors.
            watchWindow.TouchManager.BevelDown += TouchManager_BevelDown;
            watchWindow.TouchManager.BevelDoubleTap += TouchManager_BevelDoubleTap;
            watchWindow.TouchManager.BevelGrab += TouchManager_BevelGrab;
            watchWindow.TouchManager.BevelUp += TouchManager_BevelUp;

            watchWindow.TouchManager.DoubleTap += TouchManager_DoubleTap;
            watchWindow.TouchManager.SlideDown += TouchManager_SlideDown;
            watchWindow.TouchManager.SlideUp += TouchManager_SlideUp;
            watchWindow.TouchManager.SliderTouchDown += TouchManager_SliderTouchDown;
            watchWindow.TouchManager.SliderTouchUp += TouchManager_SliderTouchUp;
            
            //Launch the window
            watchWindow.Show();

            //We can access the watchface data
            
        }

        static void TrackerManager_RawTrackGestureDataUpdated(object sender, Toolkit.Input.Tracker.TrackGestureEventArgs e)
        {
            Console.WriteLine(@"Raw Tracker Data: Decision Tree label {0}, Dynamic Time Warping label {1}",e.TreeLabel,e.DtwLabel);

            foreach (var label in e.ComputedDtwCosts)
            {
                Console.WriteLine(@"Label = {0}, cost = {1}",
                    label.Key, label.Value);
            }
        }

        void GestureManager_Detected(object sender, Toolkit.Input.Gestures.GestureDetectedEventArgs e)
        {
            Console.WriteLine(@"Detected gesture: {0}", e.Gesture);
        }

        void TouchManager_SliderTouchUp(object sender, Toolkit.Input.Touch.SliderTouchEventArgs e)
        {
            Console.WriteLine(@"Touch Up on TouchDevice: {0} with value: {1}", e.Sensor, e.Value);
        }

        void TouchManager_SliderTouchDown(object sender, Toolkit.Input.Touch.SliderTouchEventArgs e)
        {
            Console.WriteLine(@"Touch Down on TouchDevice: {0} with value: {1}", e.Sensor, e.Value);
        }

        void TouchManager_SlideUp(object sender, Toolkit.Input.Touch.SliderTouchEventArgs e)
        {
            Console.WriteLine(@"Slide Up on TouchDevice: {0} with value: {1}", e.Sensor, e.Value);
        }

        void TouchManager_SlideDown(object sender, Toolkit.Input.Touch.SliderTouchEventArgs e)
        {
            Console.WriteLine(@"Slide Down on TouchDevice: {0} with value: {1}", e.Sensor, e.Value);
        }

        void TouchManager_DoubleTap(object sender, Toolkit.Input.Touch.SliderTouchEventArgs e)
        {
            Console.WriteLine(@"Double Tap on TouchDevice: {0} with value: {1}", e.Sensor, e.Value);
        }

        void TouchManager_BevelUp(object sender, Toolkit.Input.Touch.BevelTouchEventArgs e)
        {
            Console.WriteLine(@"Bevel Up on Side: {0} with value: {1}", e.BevelSide, e.Value);
        }

        void TouchManager_BevelGrab(object sender, Toolkit.Input.Touch.MultiBevelTouchEventArgs e)
        {
            Console.WriteLine(@"Bevel Grab on Sides: Left: {0} - Top: {1} - Right: {2} - Bottom {3} with value: {1}", e.BevelLeft, e.BevelTop, e.BevelRight, e.BevelBottom);
        }

        void TouchManager_BevelDoubleTap(object sender, Toolkit.Input.Touch.BevelTouchEventArgs e)
        {
            Console.WriteLine(@"Bevel double tap on Side: {0} with value: {1}", e.BevelSide, e.Value);
        }

        static void TouchManager_BevelDown(object sender, Toolkit.Input.Touch.BevelTouchEventArgs e)
        {
            Console.WriteLine(@"Detected touch down on:{0}", e.BevelSide);
        }

        static void TrackerManager_TrackGestureRecognized(object sender, LabelDetectedEventArgs e)
        {
            Console.WriteLine(@"Detected posture:{0}",e.Detection);
        }
    }
}
