using System;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;

namespace Watch.Toolkit.Interface.DefaultFaces
{
    /// <summary>
    /// Interaction logic for Clock.xaml
    /// </summary>
    public partial class Clock : WatchVisual
    {
        readonly Label _lbl;
        public Clock()
        {
            InitializeComponent();

            _lbl = new Label
            {
                Content = DateTime.Now.ToLongTimeString(),
                Foreground = Brushes.White,
                FontSize = 200,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center
            };

            Grid.Children.Add(_lbl);

            var time = new Timer(1000);
            time.Elapsed += time_Elapsed;
            time.Start();
        }

        void time_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _lbl.Content = DateTime.Now.ToLongTimeString();
            });

        }


        public void Suspend()
        {

        }

        public void Resume()
        {

        }


        public object Visual
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Guid Id
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
