using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Interface
{
    public class WatchFaceManager
    {
        readonly Dictionary<Guid, WatchVisual> _faces = new Dictionary<Guid, WatchVisual>();
        public void AddFace(WatchVisual face)
        {
            _faces.Add(face.Id,face);
        }

        public WatchVisual GetNext()
        {
            return _faces.Values.First();
        }
        public bool HasFaces()
        {
            return _faces.Count > 0;
        }

        public void RemoveFace(Guid id)
        {
            _faces.Remove(id);
        }

        public WatchVisual FindFace(Guid id)
        {
            return _faces[id];
        }
    }
}
