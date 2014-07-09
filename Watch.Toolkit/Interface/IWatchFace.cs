using System;
using System.Windows.Shapes;

namespace Watch.Toolkit.Interface
{
    public interface IWatchFace
    {
        WatchVisual Clone();
        Rectangle BuildThumbnail();

        Guid Id { get; set; }
        string Name { get; set; }
    }
}
