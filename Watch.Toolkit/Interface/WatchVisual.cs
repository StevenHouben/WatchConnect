using System.Windows.Controls;

namespace Watch.Toolkit.Interface
{
    /// <summary>
    /// Interaction logic for WatchVisual.xaml
    /// </summary>
    public class WatchVisual : UserControl,IWatchFace
    {
        public WatchVisual()
        {
        }

        public WatchVisual Clone()
        {
            return new WatchVisual
            {
                Background = Background
            };
        }

        public void Suspend()
        {
        }

        public void Resume()
        {
        }

        public object Visual { get; set; }

        public System.Guid Id { get; set; }
    }
}
