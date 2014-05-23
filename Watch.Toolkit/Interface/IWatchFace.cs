using System;

namespace Watch.Toolkit.Interface
{
    public interface IWatchFace
    {
        void Suspend();
        void Resume();
        object Visual { get; set; }
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
