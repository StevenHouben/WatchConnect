using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Watch.Examples.Desktop.Design
{
    public partial class WatchApplication
    {
        public WatchApplication(BitmapImage imgSource)
        {
            InitializeComponent();
            Image.Source = imgSource;
        }
    }
}
