
using System.Windows;
using System.Windows.Media;

namespace Watch.Examples.PrivateZone
{
    public partial class Visualizer
    {
        public Visualizer()
        {
            InitializeComponent();
        }

        public ImageSource GrabUi()
        {
            UI.Visibility = Visibility.Hidden;
            Shared.Visibility = Visibility.Visible;
            return UI.Source;
        }
        public void PushUi()
        {
            UI.Visibility = Visibility.Visible;
            Shared.Visibility = Visibility.Hidden;
        }
    }
}
