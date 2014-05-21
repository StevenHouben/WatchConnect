using System;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using Watch.Interface;

namespace Watch.Examples
{
    /// <summary>
    /// Interaction logic for WatchFace.xaml
    /// </summary>
    public partial class Clock : IWatchFace
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


        public UserControl Visual
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
