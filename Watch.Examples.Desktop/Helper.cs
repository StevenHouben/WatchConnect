using System;
using System.Windows.Media;

namespace Watch.Examples.Desktop
{
    public class Helper
    {
        public static Brush RandomBrush()
        {
            var rnd = new Random();

            var properties = typeof(Brushes).GetProperties();

            return (SolidColorBrush)properties[rnd.Next(properties.Length)].GetValue(null, null);
        }
    }
}
