using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Watch.Examples;
using Watch.Input;

namespace Watch
{
    public partial class OutputWindow
    {
        private const bool RunOnWatch = false;

        private SensorVisualizer _vis;

        private GestureManager gestureManager;
        private TouchManager touchManager;
        public OutputWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            Closing += MainWindow_Closing;

            Task.Factory.StartNew(() =>
            {
                gestureManager = new GestureManager();
                gestureManager.GestureDetected += input_GestureHandler;
                gestureManager.RawDataReceived += _input_RawDataReceived;
                gestureManager.Glance += _input_event;
                gestureManager.HoverLeft += _input_event;
                gestureManager.HoverRight += _input_event;
                gestureManager.SwipeLeft += _input_event;
                gestureManager.SwipeRight += _input_event;
                gestureManager.Cover += _input_event;
                gestureManager.Start();

            });

            Task.Factory.StartNew(() =>
            {
                touchManager = new TouchManager();
                touchManager.RawDataReceived += touchManager_RawDataReceived;
                touchManager.SliderTouchDown += touchManager_SliderTouchDown;
                touchManager.SliderTouchUp += touchManager_SliderTouchUp;
                touchManager.SlideUp += touchManager_SlideUp;
                touchManager.SlideDown += touchManager_SlideDown;
                touchManager.Start();
            });
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
            Console.WriteLine(text);
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
            //SetDisplayMode(DisplayMode.Extend);
        }

        private void InitializeWindow()
        {
            ResizeMode = ResizeMode.NoResize;

            if (RunOnWatch)
            {
                Topmost = true;
                WindowStyle = WindowStyle.None;
                SetDisplayMode(DisplayMode.Duplicate);
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.ToolWindow;
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
    }
}
