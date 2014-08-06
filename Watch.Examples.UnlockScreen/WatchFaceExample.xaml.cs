
using System.Windows.Media;

namespace Watch.Examples.UnlockScreen
{
    public partial class WatchFaceExample
    {
        public WatchFaceExample()
        {
            InitializeComponent();
        }

        public void SetColor(Brush color)
        {
            Background = color;
        }
    }
}
