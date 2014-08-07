using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.PrivateZone
{
    public partial class MainWindow
    {

        private WatchRuntime _watchWindow;
        private WatchConfiguration _watchConfiguration;
        private readonly Image _myZone;
        private Visualizer _vis;
        private Color _color;
        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;

            KeyDown += MainWindow_KeyDown;

            _watchWindow = new WatchRuntime(
      new WatchConfiguration
      {
          //Define our hardwareplatform
          Hardware = new Arduino("COM4"),

          //Add a classifierConfiguration for the TrackerManager
          ClassifierConfiguration = new ClassifierConfiguration(
              //Label for the classifier
              new List<string> { "Right Hand", "Left Hand", "Left Knuckle", "Hand" },
              //Path to training data
              "recording.data")
      });
            _vis = new Visualizer();
            _watchWindow.AddWatchFace(_vis);
            _watchWindow.Show();


            //Event: Listen to BevelDown event
            _watchWindow.TouchManager.BevelDown += (sender, e)  
                //Handler: Write to console
                => Console.WriteLine(@"{0} was touched",e.BevelSide);

            //Grab the BevelTouchSensor
            _watchWindow.TouchManager.BevelTouchSensor
                //Event: Add a custom event
                .AddEvent("My Custom Event",
                //Condition: If the bottom and left bevel are touched
                    (touchsensor) =>
                        touchsensor.TouchStates.BevelBottom
                        && touchsensor.TouchStates.BevelLeft).
                //Handler: Write the event to console
                EventTriggered += (sender, e) => 
                    Console.WriteLine(@"My Custom Event is triggered");


            PreviewTouchDown += View_PreviewTouchDown;
            PreviewTouchUp += View_PreviewTouchUp;
            PreviewTouchMove += View_PreviewTouchMove;

            _myZone = new Image()
            {
                Width = 500,
                Height = 700
            };

        }


        void View_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (_watchWindow.LastDetectedPosture != "Left Hand")
                return;
            var point = e.GetTouchPoint(View).Position;
            Canvas.SetLeft(_myZone, point.X);
            Canvas.SetTop(_myZone, point.Y - _myZone.Height);
        }

        void View_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if(Canvas.Children.Contains(_myZone))
                Canvas.Children.Remove(_myZone);
            _vis.PushUi();
        }

        void View_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (_watchWindow.LastDetectedPosture != "Left Hand")
                return;
            if (Canvas.Children.Contains(_myZone))
                return;

            _myZone.Source = _vis.GrabUi();
            var point = e.GetTouchPoint(View).Position;
            Canvas.Children.Add(_myZone);
            Canvas.SetLeft(_myZone, point.X);
            Canvas.SetTop(_myZone, point.Y - _myZone.Height);

        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Environment.Exit(0);
        }
    }
}
