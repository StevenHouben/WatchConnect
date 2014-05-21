using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Watch.Examples;
using Watch.Input;

namespace Watch
{
    public partial class OutputWindow
    {
        private const bool RunOnWatch = false;

        private SensorVisualizer _vis;
        private GestureManager _input;
        public OutputWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            Closing += MainWindow_Closing;

            _input = new GestureManager();
            _input.GestureDetected += input_GestureHandler;
            _input.RawDataReceived += _input_RawDataReceived;
            _input.Glance += _input_event;
            _input.HoverLeft += _input_event;
            _input.HoverRight += _input_event;
            _input.SwipeLeft += _input_event;
            _input.SwipeRight += _input_event;
            _input.Cover += _input_event;
            _input.Start();
        }

        void _input_event(object sender, GestureDetectedEventArgs e)
        {
            _vis.UpdateEvents(e.Gesture.ToString());
        }

        void _input_RawDataReceived(object sender, RawDataReceivedEventArgs e)
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
