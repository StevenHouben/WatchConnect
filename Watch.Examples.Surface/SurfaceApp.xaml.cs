using System;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace Watch.Examples.Surface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SurfaceApp
    {
        public SurfaceApp()
        {
            InitializeComponent();


            var tagDefinition = new TagVisualizationDefinition
            {
                Value = 0x1,
                Source = new Uri("Visualizations/FingerVisualization.xaml", UriKind.Relative),
                LostTagTimeout = 2000.0,
                OrientationOffsetFromTag = 5.0,
                PhysicalCenterOffsetFromTag = new Vector(2, 2),
                TagRemovedBehavior = TagRemovedBehavior.Fade,
                UsesTagOrientation = true
            };

            Visualizer.Definitions.Add(tagDefinition);
            Visualizer.VisualizationAdded += Visualizer_VisualizationAdded;
            Visualizer.PreviewVisualizationAdded += Visualizer_PreviewVisualizationAdded;
            Visualizer.PreviewTouchDown += Visualizer_PreviewTouchDown;

        }

        void Visualizer_PreviewTouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Visualizer_PreviewVisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Visualizer_VisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
