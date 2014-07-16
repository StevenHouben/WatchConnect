using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Watch.Toolkit;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Tracker;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.HelloWorld
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        readonly StackPanel _view = new StackPanel();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Create a new configuration
            var configuration = new WatchConfiguration
            {
                DisplaySize = new Size(800, 600),
                ClassifierConfiguration = new ClassifierConfiguration(
                     new List<string> { "Idle", "Left Index", "Left Knuckle", "Hand" },
                     AppDomain.CurrentDomain.BaseDirectory + "recording16.log")
            };

            //Create a new instance of the WatchRuntime
            var watchWindow = new WatchRuntime(configuration);

            //Add a default Watchface to the watch
            watchWindow.AddWatchFace( new Toolkit.Interface.DefaultFaces.Clock());

            //Listen for new gestures
            watchWindow.GestureManager.GestureDetected += (sender,gestureEventArgs) =>
            {
                //If we detect a swipe right event
                if (gestureEventArgs.Gesture != Gesture.SwipeRight) 
                    return;

                //Move the active visual from the watch interface
                //to the main application
                var view = (UIElement)watchWindow.GetVisual();
                _view.Children.Add(view);
            };

            //Listen for new postures
            watchWindow.TrackerManager.TrackGestureRecognized += (sender, trackEventArgs) =>
            {
                //if we detect a left knuckle posture
                if (trackEventArgs.Detection != "Left Knuckle")
                    return;

                //Move a thumnail of the watch face to the view
                var thumb = (UIElement) watchWindow.GetThumbnail(0);
                _view.Children.Add(thumb);

            };

            watchWindow.TouchManager.BevelGrab += (sender, touchEventArgs) =>
            {
                //If the user grabs the top and bottom bevel
                if (!(touchEventArgs.BevelState.BevelTop && touchEventArgs.BevelState.BevelBottom))
                    return;
                
                //Move to the next watchface
                watchWindow.NextFace();
            };
            
            watchWindow.Show();
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
            Console.WriteLine(@"Bevel Grab on Sides: Left: {0} - Top: {1} - Right: {2} - Bottom {3} with value: {1}", e.BevelState.BevelLeft, e.BevelState.BevelTop, e.BevelState.BevelRight, e.BevelState.BevelBottom);
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
