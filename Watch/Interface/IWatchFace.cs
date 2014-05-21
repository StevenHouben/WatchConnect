using System;
using System.Windows.Controls;

namespace Watch.Interface
{
    public interface IWatchFace
    {
        void Suspend();
        void Resume();
        UserControl Visual { get; set; }
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
