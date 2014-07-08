using System;
using System.Collections.Generic;

namespace Watch.Toolkit.Interface
{
    public class WatchFaceManager
    {
        readonly Dictionary<Guid,IWatchFace> _faces = new Dictionary<Guid, IWatchFace>(); 

        public void AddFace(IWatchFace face)
        {
            _faces.Add(face.Id,face);
        }

        public void RemoveFace(Guid id)
        {
            _faces.Remove(id);
        }

        public IWatchFace FindFace(Guid id)
        {
            return _faces[id];
        }
    }
}
