namespace Watch.Examples.Map
{
    public partial class WatchMap 
    {

        public Microsoft.Maps.MapControl.WPF.Map Map
        {
            get { return MapControl; }
            set { MapControl = value; }
        }

        public WatchMap()
        {
            InitializeComponent();
        }
    }
}
