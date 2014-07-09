using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Xml;

namespace Watch.Toolkit.Interface
{
    /// <summary>
    /// Interaction logic for WatchVisual.xaml
    /// </summary>
    public class WatchVisual : UserControl,IWatchFace
    {
        public WatchVisual()
        {
            Id = Guid.NewGuid();
        }

        public virtual WatchVisual Clone()
        {
            return (WatchVisual)XamlReader.Load(XmlReader.Create( new StringReader(XamlWriter.Save(this))));
        }

        public virtual Rectangle BuildThumbnail()
        {
            return new Rectangle{Fill = Background};
        }
        public System.Guid Id { get; set; }
    }
}
