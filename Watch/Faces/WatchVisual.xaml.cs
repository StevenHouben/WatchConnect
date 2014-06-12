﻿
using System.Windows.Controls;


namespace Watch.Faces
{
    /// <summary>
    /// Interaction logic for WatchVisual.xaml
    /// </summary>
    public partial class WatchVisual : UserControl
    {
        public WatchVisual()
        {
            InitializeComponent();
        }

        public WatchVisual Clone()
        {
            return new WatchVisual
            {
                Background = Background
            };
        }
    }
}
