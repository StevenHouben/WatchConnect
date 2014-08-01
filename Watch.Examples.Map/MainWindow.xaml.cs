using System;  
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.WPF;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.Map
{
    public partial class MainWindow
    {
        private readonly WatchMap _watchMap;
        private readonly WatchConfiguration _configuration = new WatchConfiguration();

        private bool _down;
        private Location _location;
        private Point _offset;
        private Location _offsetCenter;
        private int _mode = 1;
        private bool _polyVisible;
        private readonly Border _poly;

        public MainWindow()
        {
            InitializeComponent();

            Map.PreviewTouchDown += Map_PreviewTouchDown;
            Map.PreviewTouchUp += Map_PreviewTouchUp;

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;

            KeyDown += MainWindow_KeyDown;

            _configuration.Hardware = new Arduino("COM4");
            _configuration.ClassifierConfiguration = new ClassifierConfiguration(
                new List<string> {"Right Hand", "Left Hand", "Left Knuckle", "Hand"},
                AppDomain.CurrentDomain.BaseDirectory + "recording19.log");

            var watchWindow = new WatchRuntime(_configuration);
            watchWindow.GestureManager.SwipeRight += (sender, e) => Dispatcher.Invoke(() =>
            {
                Map.ZoomLevel = _watchMap.Map.ZoomLevel;
                _watchMap.Map.ZoomLevel += 1;
            });

            _watchMap = new WatchMap();
            watchWindow.AddWatchFace(_watchMap);
            watchWindow.TouchManager.BevelDown += TouchManager_BevelDown;
            _watchMap.Map.Center = Map.Center;
            watchWindow.Show();

            Map.ViewChangeOnFrame += Map_ViewChangeOnFrame;
            Map.TargetViewChanged += Map_TargetViewChanged;
            Map.PreviewTouchMove += Map_PreviewTouchMove;

            _poly = new Border
            {
                BorderBrush = Brushes.DarkRed,
                BorderThickness = new Thickness(10),
                Opacity = 0.6,
                Width = 250,
                Height = 300
            };
            Overlay.Children.Add(_poly);
        }

        private void Map_PreviewTouchMove(object sender, TouchEventArgs e)
        {

            //_location = Map.ViewportPointToLocation(e.GetTouchPoint(this).Position);

            //_watchMap.Map.Center = _location;
            //_watchMap.Map.Heading = Map.Heading;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Environment.Exit(0);
        }

        private void Map_TargetViewChanged(object sender, MapEventArgs e)
        {
            _watchMap.Map.Heading = Map.Heading;
            var point = _poly.TransformToAncestor(Overlay).Transform(new Point(0, 0));
            point.X += _poly.Width/2;
            point.Y += _poly.Height/2;
            _location = Map.ViewportPointToLocation(point);
            _watchMap.Map.Center = _location;
            _watchMap.Map.ZoomLevel = Map.ZoomLevel + 2;
        }


        private void TouchManager_BevelDown(object sender, BevelTouchEventArgs e)
        {
            switch (e.BevelSide)
            {
                case BevelSide.Left:
                    Dispatcher.Invoke(() =>
                    {
                        switch (_mode)
                        {
                            case 1:
                                _watchMap.Map.Mode = new AerialMode();
                                break;
                            case 2:
                                _watchMap.Map.Mode = new RoadMode();

                                break;
                        }
                        if (_mode == 2)
                            _mode = 0;
                        _mode++;
                    });
                    break;
                case BevelSide.Right:
                    if (!_down || _location == null) return;
                    Dispatcher.Invoke(() =>
                    {
                        Map.Children.Add(new Pushpin {Location = _location});
                        _watchMap.Map.Children.Add(new Pushpin {Location = _location});
                    });
                    break;
                case BevelSide.Top:
                    Dispatcher.Invoke(() =>
                    {
                        _watchMap.Map.ZoomLevel += 1;
                    });
                    break;
                default:
                    Dispatcher.Invoke(() =>
                    {
                        _watchMap.Map.ZoomLevel -= 1;
                    });
                    break;
            }
        }

        private void Map_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
        }

        private void Map_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            _down = false;
        }

        private void Map_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _down = true;

            _location = Map.ViewportPointToLocation(e.GetTouchPoint(this).Position);

            _watchMap.Map.Center = _location;
            _watchMap.Map.Heading = Map.Heading;

            Canvas.SetLeft(_poly, e.GetTouchPoint(this).Position.X - _poly.Width/2);
            Canvas.SetTop(_poly, e.GetTouchPoint(this).Position.Y - _poly.Height/2);
        }

        public Dictionary<int,double> ZoomValues = new Dictionary<int, double>
        {
            {1,78271.52},
            {2,39135.76},	
            {3,19567.88},
            {4,9783.94},
            {5,4891.97},
            {6,2445.98},
            {7,1222.99},
            {8,611.50},
            {9,305.75},
            {10,152.87},
            {11,76.44},
            {12,38.22},
            {13,19.11},
            {14,9.55},
            {15,4.78},
            {16,2.39},
            {17,1.19},
            {18,0.60},
            {19,0.30}
        };
    }
}
