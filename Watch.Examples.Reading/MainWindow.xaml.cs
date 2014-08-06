using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Palettes;
using Watch.Toolkit;
using Watch.Toolkit.Hardware.Arduino;
using Watch.Toolkit.Input.Touch;
using Watch.Toolkit.Processing.MachineLearning;

namespace Watch.Examples.Reading
{
    public partial class MainWindow
    {

        private WatchRuntime _watchWindow;
        private readonly WatchConfiguration _watchConfiguration;
        private WatchFaceExample _menu;
        private string _lastDetection;
        private InkCanvasEditingMode _editMode;
        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;

            KeyDown += MainWindow_KeyDown;

            SurfaceColors.SetDefaultApplicationPalette(new LightSurfacePalette());

            _watchConfiguration = new WatchConfiguration
            {
                Hardware = new Arduino("COM4"),
                ClassifierConfiguration = new ClassifierConfiguration(
                    new List<string> {"Right Hand", "Left Hand", "Left Knuckle", "Hand"},
                    AppDomain.CurrentDomain.BaseDirectory + "recording19.log")
            };

            _watchWindow = new WatchRuntime(_watchConfiguration);

            _menu = new WatchFaceExample();
            _watchWindow.AddWatchFace(_menu);
            _watchWindow.Show();

            _watchWindow.TouchManager.BevelDown += TouchManager_BevelDown;
            _watchWindow.TrackerManager.TrackGestureRecognized += TrackerManager_TrackGestureRecognized;
            _watchWindow.GestureManager.Cover += GestureManager_Cover;

            PreviewTouchDown += MainWindow_PreviewTouchDown;

            ChangeDrawTool(DrawTool.Pen);
            ChangeInteractionMode(InteractionMode.Pan);
        }

        void GestureManager_Cover(object sender, Toolkit.Input.Gestures.GestureDetectedEventArgs e)
        {

            Dispatcher.Invoke(() =>
            {
                Title = "Cover";
                InkCanvas.Strokes.Clear();
            });
        }

        void TrackerManager_TrackGestureRecognized(object sender, Toolkit.Input.Tracker.LabelDetectedEventArgs e)
        {
            _lastDetection = e.Detection;
            Dispatcher.Invoke(() => Title = _lastDetection);

        }

        void MainWindow_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            switch (_lastDetection)
            {
                case "Right Hand":
                    ChangeInteractionMode(InteractionMode.Pan);
                    break;
                case "Left Hand":
                    ChangeInteractionMode(InteractionMode.Draw);
                    break;
                case "Left Knuckle":
                    ChangeInteractionMode(InteractionMode.Select);
                    break;
            } 
        }

        void TouchManager_BevelDown(object sender, Toolkit.Input.Touch.BevelTouchEventArgs e)
        {
            Console.WriteLine(e.BevelSide);
            Dispatcher.Invoke(() =>
            {
                switch (e.BevelSide)
                {
                    case BevelSide.Bottom:
                        ChangeDrawTool(_menu.SelectNextMode());
                        break;
                    case BevelSide.Top:
                        ChangeDrawTool(_menu.SelectPreviousMode());
                        break;
                }

            });
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Environment.Exit(0);
                    break;
                case Key.D:
                    ChangeInteractionMode(InteractionMode.Draw);
                    break;
                case Key.S:
                    ChangeInteractionMode(InteractionMode.Select);
                    break;
                case Key.F:
                    ChangeInteractionMode(InteractionMode.Pan);
                    break;
                case Key.X:
                    ChangeDrawTool(DrawTool.Brush);
                    break;
                case Key.C:
                    ChangeDrawTool(DrawTool.Marker);
                    break;
                case Key.V:
                    ChangeDrawTool(DrawTool.Pen);
                    break;
                case Key.B:
                    ChangeDrawTool(DrawTool.Eraser);
                    break;
            }
        }

        private void ChangeInteractionMode(InteractionMode mode)
        {
            switch (mode)
            {
                case InteractionMode.Draw:
                    InkCanvas.EditingMode = _editMode;
                    //InkCanvas.Visibility = Visibility.Visible;
                    ScrollViewer.PanningMode = PanningMode.None;
                    InkCanvas.BringToFront();
                    break;
                case InteractionMode.Pan:
                    InkCanvas.EditingMode = InkCanvasEditingMode.None;
                    //InkCanvas.Visibility = Visibility.Hidden;
                    ScrollViewer.PanningMode = PanningMode.Both;
                    InkCanvas.BringToFront();
                    break;
                case InteractionMode.Select:
                    InkCanvas.EditingMode = InkCanvasEditingMode.None;
                    //InkCanvas.Visibility = Visibility.Hidden;
                    ScrollViewer.PanningMode = PanningMode.None;
                    Text.BringToFront();
                    break;

            }
        }

        public void ChangeDrawTool(DrawTool type)
        {
            switch (type)
            {
                case DrawTool.Brush:
                    var brushColor = Colors.Blue;
                    brushColor.A = 50;
                    InkCanvas.DefaultDrawingAttributes.Color = brushColor;
                    InkCanvas.DefaultDrawingAttributes.StylusTip = StylusTip.Rectangle;
                    InkCanvas.DefaultDrawingAttributes.Width = InkCanvas.DefaultDrawingAttributes.Height = 40;
                    _editMode = InkCanvasEditingMode.Ink;
                    break;
                case DrawTool.Eraser:
                    //var eraserColor = Colors.White;
                    //InkCanvas.DefaultDrawingAttributes.Color = eraserColor;
                    //InkCanvas.DefaultDrawingAttributes.StylusTip = StylusTip.Rectangle;
                    //InkCanvas.DefaultDrawingAttributes.Width = InkCanvas.DefaultDrawingAttributes.Height = 40;
                    _editMode  = InkCanvasEditingMode.EraseByPoint;
                    break;
                case DrawTool.Marker:
                    var markerColor = Colors.Yellow;
                    markerColor.A = 100;
                    InkCanvas.DefaultDrawingAttributes.Color = markerColor;
                    InkCanvas.DefaultDrawingAttributes.StylusTip = StylusTip.Rectangle;
                    InkCanvas.DefaultDrawingAttributes.Width = InkCanvas.DefaultDrawingAttributes.Height = 20;
                    _editMode = InkCanvasEditingMode.Ink;
                    break;
                case DrawTool.Pen:
                    InkCanvas.DefaultDrawingAttributes.Color = Colors.Black;
                    InkCanvas.DefaultDrawingAttributes.StylusTip = StylusTip.Rectangle;
                    InkCanvas.DefaultDrawingAttributes.Width = InkCanvas.DefaultDrawingAttributes.Height = 5;
                    _editMode = InkCanvasEditingMode.Ink;
                    break;
            }
            InkCanvas.EditingMode = _editMode;

        }
    }
    public static class FrameworkElementExt
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if (element == null) return;

            var parent = element.Parent as Panel;
            if (parent == null) return;

            var maxZ = parent.Children.OfType<UIElement>()
              .Where(x => x != element)
              .Select(Panel.GetZIndex)
              .Max();
            Panel.SetZIndex(element, maxZ + 1);
        }
    }

    public enum DrawTool
    {
        Pen,
        Marker,
        Brush,
        Eraser
    }
    public enum InteractionMode
    {
        Select,
        Pan,
        Draw
    }
}
