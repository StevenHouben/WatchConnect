using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Watch.Faces;
using Watch.Toolkit.Input;
using Watch.Toolkit.Input.Gestures;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Interface;
using Watch.Toolkit.Sensors;

namespace Watch
{
    public partial class WatchFace:IVisualSharer
    {
        private const bool RunOnWatch = false;

        private SensorVisualizer _vis;

        private AbstractGestureManager gestureManager;
        private TouchManager touchManager;

        public WatchFace(GestureManager _gestureManager,TouchManager _touchManager)
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            Closing += MainWindow_Closing;

            gestureManager = _gestureManager;
            gestureManager.GestureDetected += input_GestureHandler;
            gestureManager.RawDataReceived += _input_RawDataReceived;
            gestureManager.Glance += _input_event;
            gestureManager.HoverLeft += _input_event;
            gestureManager.HoverRight += _input_event;
            gestureManager.SwipeLeft += _input_event;
            gestureManager.SwipeRight += _input_event;
            gestureManager.Cover += _input_event;
            //gestureManager.Start();

  
            touchManager = _touchManager;
            touchManager.RawDataReceived += touchManager_RawDataReceived;
            touchManager.SliderTouchDown += touchManager_SliderTouchDown;
            touchManager.SliderTouchUp += touchManager_SliderTouchUp;
            touchManager.SlideUp += touchManager_SlideUp;
            touchManager.SlideDown += touchManager_SlideDown;
            touchManager.DoubleTap += touchManager_DoubleTap;
            touchManager.BevelDown += touchManager_BevelDown;
            touchManager.BevelUp += touchManager_BevelUp;
            //touchManager.Start();
        }

        readonly BevelState _bevelState = new BevelState();
        void touchManager_BevelUp(object sender, BevelTouchEventArgs e)
        {
           _bevelState.UpdateState(e.BevelSide,false);

            if(_vis!=null)
                _vis.UpdateBevels(_bevelState);
        }

        void touchManager_BevelDown(object sender, BevelTouchEventArgs e)
        {
            _bevelState.UpdateState(e.BevelSide, true);
            if (_vis != null)
                _vis.UpdateBevels(_bevelState);
        }

        void touchManager_DoubleTap(object sender, SliderTouchEventArgs e)
        {
           if(_vis != null)
               _vis.UpdateEvents("DoubleTap");
        }

        void touchManager_SlideDown(object sender, SliderTouchEventArgs e)
        {
            if(_vis !=null)
                _vis.UpdateEvents("SlideDown");
        }

        void touchManager_SlideUp(object sender, SliderTouchEventArgs e)
        {
            if(_vis !=null)
                _vis.UpdateEvents("SlideUp");
        }

        void touchManager_SliderTouchUp(object sender, SliderTouchEventArgs e)
        {
            if(_vis !=null)
                _vis.UpdateLinearTouch(e.Sensor);
        }

        void touchManager_SliderTouchDown(object sender, SliderTouchEventArgs e)
        {
            if (_vis != null)
                _vis.UpdateLinearTouch(e.Sensor);
        }

        void touchManager_RawDataReceived(object sender, RawTouchDataReceivedEventArgs e)
        {
            if(_vis !=null)
                _vis.UpdateLinearTouch(e.LinearTouch);
        }

        void _input_event(object sender, GestureDetectedEventArgs e)
        {
            _vis.UpdateEvents(e.Gesture.ToString());
        }

        void _input_RawDataReceived(object sender, RawSensorDataReceivedEventArgs e)
        {
            if(_vis !=null)
             _vis.UpdateVisualization(e.TopLeftSensor,e.TopRightSensor,e.FrontSensor,e.LightSensor);

        }

        static void WriteToOutput(string text)
        {
            //Console.WriteLine(text);
        }

        void input_GestureHandler(object sender, GestureDetectedEventArgs e)
        {
            if(_vis !=null)
                _vis.UpdateDetection(e.Gesture.ToString());
            Dispatcher.Invoke(()=>
            {
                switch (e.Gesture)
                {
                    case Gesture.Cover:
                        Border.BorderBrush = new LinearGradientBrush(Colors.Brown, Colors.DarkRed, 90);
                        WriteToOutput("Cover Gesture");
                        break;
                    case Gesture.HoverLeft:
                        Border.BorderBrush = new LinearGradientBrush(Colors.Goldenrod, Colors.Peru, 90);
                        WriteToOutput("Hover Left");
                        break;
                    case Gesture.HoverRight:
                        Border.BorderBrush = new LinearGradientBrush(Colors.Goldenrod, Colors.Peru, 90);
                        WriteToOutput("Hover Right");
                        break;
                    case Gesture.SwipeLeft:
                        Border.BorderBrush = new LinearGradientBrush(Colors.WhiteSmoke, Colors.SlateGray, 90);
                        WriteToOutput("Swipe Left");
                        break;
                    case Gesture.SwipeRight:
                        Border.BorderBrush = new LinearGradientBrush(Colors.WhiteSmoke, Colors.SlateGray, 90);
                        WriteToOutput("Swipe Right");
                        break;
                    case Gesture.Glance:
                        Border.BorderBrush = new LinearGradientBrush(Colors.DarkGreen, Colors.Olive, 90);
                        WriteToOutput("Glance Gesture");
                        break;
                    case Gesture.Neutral:
                        Border.BorderBrush = Brushes.Black;
                        WriteToOutput("");
                        break;
                }
            });                            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWindow();
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher.InvokeShutdown();
            //gestureManager.Stop();
            touchManager.Stop();

            Environment.Exit(0);
        }

        private void InitializeWindow()
        {
            ResizeMode = ResizeMode.CanResize;

            if (RunOnWatch)
            {
                Topmost = true;
                WindowStyle = WindowStyle.None;
                SetDisplayMode(DisplayMode.Duplicate);
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Normal;
            }

            _vis = new SensorVisualizer
            {
                Background =  new LinearGradientBrush(Colors.CadetBlue,Colors.LightSlateGray,45.0),
                Width = 1024,
                Height = 768
            };
            Body.Children.Add(_vis);
        }

        private static void SetDisplayMode(DisplayMode mode)
        {
            var proc = new Process {StartInfo = {FileName = "DisplaySwitch.exe"}};
            switch (mode)
            {
                case DisplayMode.External:
                    proc.StartInfo.Arguments = "/external";
                    break;
                case DisplayMode.Internal:
                    proc.StartInfo.Arguments = "/internal";
                    break;
                case DisplayMode.Extend:
                    proc.StartInfo.Arguments = "/extend";
                    break;
                case DisplayMode.Duplicate:
                    proc.StartInfo.Arguments = "/clone";
                    break;
            }
            proc.Start();
        }
        enum DisplayMode
        {
            Internal,
            External,
            Extend,
            Duplicate
        }

        public object GetVisual()
        {
            return new Rectangle() {Width = 50, Height = 50, Fill = Brushes.Red};
        }

        public object GetThumbnail()
        {
            throw new NotImplementedException();
        }

        public void SendThumbnail(object thumbnail)
        {
            throw new NotImplementedException();
        }

        public void SendVisual(object visual)
        {
            throw new NotImplementedException();
        }
    }
}
